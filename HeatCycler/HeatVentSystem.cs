using System;
using UnityEngine;

/// <summary>
/// R-ability router + Pressure Vent + Capacitor Dump (narrowing cone from muzzle).
/// Priority: Dump > Energy Convergence > Elemental Discharge > Pressure Vent.
/// </summary>
internal sealed class HeatVentSystem
{
    private readonly CyclerHeatBehaviour host;

    private float ventRecoveryTimer;
    private float dissipateDelayBypassUntil = -999f;

    // Capacitor Dump — narrowing cone
    private bool dumpActive;
    private float dumpTimer;
    private float dumpDuration;
    private float dumpDamageBudget;
    private float dumpStartAngle;
    private float dumpEndAngle;
    private float dumpMaxRange;
    private LineRenderer dumpLine;
    private GameObject dumpLineGo;

    private float fireHitchTimer;

    public bool IsDumpActive => dumpActive;
    public bool IsVentRecovering => ventRecoveryTimer > 0f;
    public bool HasDissipateDelayBypass => Time.time < dissipateDelayBypassUntil;
    public bool IsFireHitched => fireHitchTimer > 0f;

    public HeatVentSystem(CyclerHeatBehaviour host)
    {
        this.host = host;
    }

    public void Reset()
    {
        ventRecoveryTimer = 0f;
        dissipateDelayBypassUntil = -999f;
        EndDump();
        fireHitchTimer = 0f;
    }

    public void Tick(float dt, Gun gun)
    {
        if (dt <= 0f)
            return;

        if (ventRecoveryTimer > 0f)
            ventRecoveryTimer = Mathf.Max(0f, ventRecoveryTimer - dt);

        if (fireHitchTimer > 0f)
        {
            fireHitchTimer = Mathf.Max(0f, fireHitchTimer - dt);
            if (fireHitchTimer <= 0f && gun != null)
                host.SyncFireLock(gun);
        }

        TickDumpCone(dt, gun);
    }

    public bool TryHeatAbility(Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return false;

        ref var data = ref host.WeaponData;

        if (data.dumpBeamDps > 0f && !dumpActive)
        {
            if (TryCapacitorDump(gun))
                return true;
        }

        if (data.energyConvHeatPerStack > 0f && host.EnergyConvStoredStacks > 0)
        {
            if (host.TryConsumeEnergyConvergence(gun))
                return true;
        }

        if (data.dischargeRadius > 0f && data.dischargeDamage > 0f)
        {
            if (TryElementalDischarge(gun))
                return true;
        }

        return TryPressureVent(gun);
    }

    public bool TryPressureVent(Gun gun)
    {
        ref var data = ref host.WeaponData;
        if (!data.ventEnabled)
            return false;
        if (ventRecoveryTimer > 0f || dumpActive)
            return false;

        float minHeat = data.ventMinHeat > 0f ? data.ventMinHeat : 15f;
        if (host.CurrentHeat < minHeat)
            return false;

        float spend = data.ventSpend > 0f ? data.ventSpend : 35f;
        float spent = host.SpendHeatUpTo(spend);
        if (spent <= 0f)
            return false;

        ventRecoveryTimer = data.ventRecovery > 0f ? data.ventRecovery : 0.45f;
        float bypass = data.ventBypassDissipateDuration > 0f ? data.ventBypassDissipateDuration : 0.35f;
        dissipateDelayBypassUntil = Time.time + bypass;

        float radius = data.ventRadius > 0f ? data.ventRadius : 3.5f;
        float damage = data.ventDamage > 0f ? data.ventDamage : 20f;

        // Saturate Catalyst: consume stacks into a stronger vent pulse
        host.ConsumeCatalystForVent(out float catDmg, out float catRad);
        damage += catDmg;
        radius += catRad;

        SpawnPulse(gun, radius, damage);

        try { gun.TriggerEffectReload(); } catch { /* optional */ }
        SparrohPlugin.Logger?.LogDebug(
            $"[CyclerRework] Pressure Vent spent={spent:0.#}" +
            (catDmg > 0f ? $" catalyst=+{catDmg:0.#}dmg/+{catRad:0.##}r" : ""));
        return true;
    }


    public bool TryCapacitorDump(Gun gun)
    {
        ref var data = ref host.WeaponData;
        if (data.dumpBeamDps <= 0f || dumpActive)
            return false;
        if (host.CurrentHeat <= 1f)
            return false;
        if (ventRecoveryTimer > 0f)
            return false;

        float heatAtStart = host.CurrentHeat;
        float full = Mathf.Max(1f, data.maxHeat);
        float duration = data.dumpSecondsPerFullHeat > 0f
            ? data.dumpSecondsPerFullHeat * Mathf.Clamp(heatAtStart / full, 0.15f, 2f)
            : 1.2f * Mathf.Clamp(heatAtStart / full, 0.15f, 2f);

        float dps = data.dumpBeamDps;
        dumpDamageBudget = dps * duration * Mathf.Clamp(heatAtStart / full, 0.25f, 2f);
        dumpDuration = Mathf.Max(0.35f, duration);
        dumpTimer = dumpDuration;
        dumpStartAngle = data.dumpConeStartAngle > 0f ? data.dumpConeStartAngle : 38f;
        dumpEndAngle = data.dumpConeEndAngle > 0f ? data.dumpConeEndAngle : 7f;
        dumpMaxRange = data.dumpConeRange > 0f ? data.dumpConeRange : 28f;
        dumpActive = true;

        host.SpendHeatUpTo(heatAtStart * 0.35f);

        fireHitchTimer = data.dumpFireHitch > 0f ? data.dumpFireHitch : 0.15f;
        if (gun.IsOwner)
            gun.RemainingAmmo = 0f;

        try { gun.TriggerEffectBuff(); } catch { /* optional */ }
        SparrohPlugin.Logger?.LogInfo(
            $"[CyclerRework] Capacitor Dump start heat={heatAtStart:0.#} dur={dumpDuration:0.##}s");
        return true;
    }

    public bool TryElementalDischarge(Gun gun)
    {
        ref var data = ref host.WeaponData;
        if (data.dischargeRadius <= 0f)
            return false;
        if (ventRecoveryTimer > 0f || dumpActive)
            return false;

        float cost = data.dischargeHeatCost > 0f ? data.dischargeHeatCost : 40f;
        if (host.CurrentHeat < Mathf.Min(cost, data.ventMinHeat > 0f ? data.ventMinHeat : 15f))
            return false;

        float spent = host.SpendHeatUpTo(cost);
        if (spent <= 0f)
            return false;

        ventRecoveryTimer = Mathf.Max(0.35f, data.dischargeChargeTime);

        float dmgVal = data.dischargeDamage > 0f ? data.dischargeDamage : 40f;
        float radius = data.dischargeRadius > 0f ? data.dischargeRadius : 5f;
        EffectType elem = gun.GunData.damageEffect > EffectType.Normal
            ? gun.GunData.damageEffect
            : EffectType.Fire;

        try
        {
            VanillaCyclerAssets.TryGetMuzzle(gun, out Vector3 origin, out Vector3 fwd);
            origin += fwd * 1.2f;

            var dmg = new DamageData(
                elem > EffectType.Normal ? 0f : dmgVal,
                elem,
                dmgVal,
                DamageFlags.AOE);

            GameManager.Instance.SpawnExplosionObserverSeeThrough(
                gun, origin, radius, TargetType.NonPlayer, dmg, gun.OwnerClientId, 0f);
            try { gun.TriggerEffectBuff(); } catch { /* optional */ }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[CyclerRework] Discharge nova failed: {ex}");
        }

        return true;
    }

    private void TickDumpCone(float dt, Gun gun)
    {
        if (!dumpActive)
        {
            DestroyDumpVisual();
            return;
        }

        if (gun == null)
        {
            EndDump();
            return;
        }

        ref var data = ref host.WeaponData;
        dumpTimer -= dt;
        float t = dumpDuration > 0.01f ? 1f - Mathf.Clamp01(dumpTimer / dumpDuration) : 1f;

        float drainPerSec = data.dumpHeatDrainPerSecond > 0f
            ? data.dumpHeatDrainPerSecond
            : (host.WeaponData.maxHeat / Mathf.Max(0.35f, dumpDuration));
        host.SpendHeatUpTo(drainPerSec * dt);

        float angle = Mathf.Lerp(dumpStartAngle, dumpEndAngle, t);
        float range = dumpMaxRange;

        float tickBudget = dumpDuration > 0.01f
            ? dumpDamageBudget * (dt / dumpDuration)
            : dumpDamageBudget;

        try
        {
            VanillaCyclerAssets.TryGetMuzzle(gun, out Vector3 muzzle, out Vector3 dir);
            // Start slightly ahead of muzzle so nothing sits in the camera
            Vector3 origin = muzzle + dir * 0.35f;

            int mask = gun.GunData.targetCollisionMask | gun.GunData.surfaceCollisionMask;
            int spokes = 5;
            float tickDmg = tickBudget / spokes;

            Vector3 centerEnd = origin + dir * range;
            Transform lookUp = gun.playerLook != null ? gun.playerLook.transform : gun.transform;

            for (int i = 0; i < spokes; i++)
            {
                float yaw = 0f;
                if (spokes > 1)
                {
                    float u = (i / (float)(spokes - 1)) * 2f - 1f;
                    yaw = u * angle * 0.5f;
                }

                Quaternion rot = Quaternion.AngleAxis(yaw, lookUp.up) *
                                 Quaternion.LookRotation(dir, lookUp.up);
                Vector3 shotDir = rot * Vector3.forward;
                Vector3 end = origin + shotDir * range;
                if (Physics.Raycast(origin, shotDir, out RaycastHit hit, range, mask))
                    end = hit.point;

                // Keep AOE off the player — only damage at ray end, modest radius
                float aoeR = Mathf.Lerp(1.4f, 0.55f, t);
                var dmg = new DamageData(
                    tickDmg,
                    gun.GunData.damageEffect,
                    tickDmg * 0.25f,
                    DamageFlags.AOE);
                GameManager.Instance.SpawnExplosionObserverSeeThrough(
                    gun, end, aoeR, TargetType.NonPlayer, dmg, gun.OwnerClientId, 0f);

                if (i == spokes / 2)
                    centerEnd = end;
            }

            // Thin line visual from muzzle → aim point (not a solid cylinder)
            float width = Mathf.Lerp(0.06f, 0.02f, t);
            UpdateDumpLine(origin, centerEnd, width);
        }
        catch
        {
            // ignore tick failures
        }

        if (dumpTimer <= 0f || host.CurrentHeat <= 0.25f)
            EndDump();
    }

    private void SpawnPulse(Gun gun, float radius, float damage)
    {
        if (gun == null)
            return;
        try
        {
            VanillaCyclerAssets.TryGetMuzzle(gun, out Vector3 origin, out _);
            // Pulse around player feet/body is fine for vent utility
            try
            {
                if (gun.Player != null)
                    origin = gun.Player.InterpolatedPosition;
            }
            catch { /* keep muzzle */ }

            var dmg = new DamageData(damage, DamageFlags.AOE);
            GameManager.Instance.SpawnExplosionObserverSeeThrough(
                gun, origin, radius, TargetType.NonPlayer, dmg, gun.OwnerClientId, 0f);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[CyclerRework] Vent pulse failed: {ex.Message}");
        }
    }

    private void UpdateDumpLine(Vector3 start, Vector3 end, float width)
    {
        if (dumpLine == null)
        {
            dumpLineGo = new GameObject("HeatCycler_DumpLine");
            dumpLineGo.hideFlags = HideFlags.HideAndDontSave;
            dumpLine = dumpLineGo.AddComponent<LineRenderer>();
            dumpLine.positionCount = 2;
            dumpLine.useWorldSpace = true;
            dumpLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dumpLine.receiveShadows = false;
            dumpLine.numCapVertices = 2;
            dumpLine.textureMode = LineTextureMode.Stretch;

            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Unlit/Color")
                         ?? Shader.Find("Sprites/Default")
                         ?? Shader.Find("Standard");
            var mat = new Material(shader);
            // Semi-transparent orange — thin line, not opaque volume
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", new Color(1f, 0.45f, 0.1f, 0.55f));
            mat.color = new Color(1f, 0.45f, 0.1f, 0.55f);
            dumpLine.sharedMaterial = mat;
            dumpLine.startColor = new Color(1f, 0.55f, 0.15f, 0.7f);
            dumpLine.endColor = new Color(1f, 0.25f, 0.05f, 0.35f);
        }

        dumpLine.enabled = true;
        dumpLine.startWidth = width;
        dumpLine.endWidth = width * 0.45f;
        dumpLine.SetPosition(0, start);
        dumpLine.SetPosition(1, end);
    }

    private void EndDump()
    {
        dumpActive = false;
        dumpTimer = 0f;
        dumpDuration = 0f;
        dumpDamageBudget = 0f;
        DestroyDumpVisual();
    }

    private void DestroyDumpVisual()
    {
        if (dumpLineGo != null)
        {
            UnityEngine.Object.Destroy(dumpLineGo);
            dumpLineGo = null;
            dumpLine = null;
        }
    }

    public void Destroy()
    {
        EndDump();
    }
}
