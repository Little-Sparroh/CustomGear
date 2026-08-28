using System;
using System.Reflection;
using Pigeon.Movement;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Runtime host for Whiteout baseline mechanics.
/// Hold M1: cryo raycast-fan hose. RMB: mag-tax cryo cell lob. R: reload only.
/// </summary>
public sealed class WhiteoutBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        // Hose
        public float hoseDamagePerSecond;
        public float hoseCryoPerSecond;
        public float hoseRange;
        public float hoseTargetMagnetism;
        public float hoseSurfaceMagnetism;
        public float hoseTickInterval;
        public float hoseMagDrainPerSecond;

        // Lob
        public float lobMagTax;
        public float lobDamage;
        public float lobCryoAmount;
        public float lobRadius;
        public float lobSpeed;
        public float lobGravity;
        public float lobInputDebounce;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = "Whiteout";

    private Gun boundGun;
    private float hoseTickTimer;
    private bool hoseActive;
    private bool aimWasHeld;
    private float lobReadyAt = -999f;

    private VisualEffect hoseVfxInstance;
    private bool hoseVfxPlaying;
    private Action<IBullet> releaseLob;

    public ref Data WeaponData => ref data;
    public string Description => description;
    public bool IsHosing => hoseActive;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            hoseDamagePerSecond = WhiteoutBalance.HoseDamagePerSecond,
            hoseCryoPerSecond = WhiteoutBalance.HoseCryoPerSecond,
            hoseRange = WhiteoutBalance.HoseRange,
            hoseTargetMagnetism = WhiteoutBalance.HoseTargetMagnetism,
            hoseSurfaceMagnetism = WhiteoutBalance.HoseSurfaceMagnetism,
            hoseTickInterval = WhiteoutBalance.HoseTickInterval,
            hoseMagDrainPerSecond = WhiteoutBalance.HoseMagDrainPerSecond,
            lobMagTax = WhiteoutBalance.LobMagTax,
            lobDamage = WhiteoutBalance.LobDamage,
            lobCryoAmount = WhiteoutBalance.LobCryoAmount,
            lobRadius = WhiteoutBalance.LobRadius,
            lobSpeed = WhiteoutBalance.LobSpeed,
            lobGravity = WhiteoutBalance.LobGravity,
            lobInputDebounce = WhiteoutBalance.LobInputDebounce
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntime();
    }

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
    }

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopySnapshotFrom(WhiteoutBehaviour template)
    {
        if (template == null)
            return;
        description = template.description;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        ResetRuntime();
    }

    public void ResetRuntime()
    {
        hoseTickTimer = 0f;
        hoseActive = false;
        aimWasHeld = false;
        lobReadyAt = -999f;
        StopHoseVfx();
    }

    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        WeaponRegistration.NeutralizeJackrabbitAltModes(gun);
        WeaponRegistration.CacheLobPrefabFromBase(SparrohPlugin.Logger);
    }

    public void OnUpgradesCleared(Gun gun)
    {
        StopHose();
        data = prefabSnapshot;
        ResetRuntime();
        boundGun = null;
    }

    public void Tick(float dt, Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;

        boundGun = gun;

        // Keep Jackrabbit alt modes dead every frame (defensive).
        WeaponRegistration.NeutralizeJackrabbitAltModes(gun);
        gun.IsAimEnabled = WhiteoutBalance.IsAimEnabled;

        bool active = false;
        try { active = gun.Active; } catch { active = true; }

        if (!active)
        {
            if (hoseActive)
                StopHose();
            aimWasHeld = false;
            return;
        }

        TickHose(dt, gun);
        TickLobInput(gun);
    }

    private void TickHose(float dt, Gun gun)
    {
        bool fireHeld = false;
        try
        {
            if (PlayerInput.Controls != null)
                fireHeld = PlayerInput.Controls.Player.Fire.IsPressed();
        }
        catch
        {
            fireHeld = false;
        }

        bool reloading = false;
        try { reloading = gun.Reloading; } catch { /* */ }

        bool canHose = fireHeld && !reloading && GetRemainingAmmo(gun) > 0.01f;

        if (canHose)
        {
            if (!hoseActive)
                StartHose(gun);

            hoseTickTimer += dt;
            float interval = Mathf.Max(0.05f, data.hoseTickInterval);
            if (hoseTickTimer >= interval)
            {
                float spent = (float)Mathf.FloorToInt(hoseTickTimer / interval) * interval;
                hoseTickTimer -= spent;
                ApplyHoseTick(gun, spent);
            }
        }
        else if (hoseActive)
        {
            StopHose();
        }
    }

    private void StartHose(Gun gun)
    {
        hoseActive = true;
        hoseTickTimer = 0f;
        TryStartHoseVfx(gun);
    }

    private void StopHose()
    {
        hoseActive = false;
        hoseTickTimer = 0f;
        StopHoseVfx();
    }

    private void ApplyHoseTick(Gun gun, float interval)
    {
        // Continuous mag drain (empty air still costs winter).
        float drain = data.hoseMagDrainPerSecond * interval;
        SpendAmmo(gun, drain);

        if (!TryGetMuzzle(gun, out Vector3 origin, out Vector3 direction))
            return;

        float damage = data.hoseDamagePerSecond * interval;
        float cryo = data.hoseCryoPerSecond * interval;
        float range = Mathf.Max(1f, data.hoseRange);

        try
        {
            RaycastHit[] hits = ArrayPool<RaycastHit>.Get();
            IBullet.RaycastTargetsAndSurface(
                origin,
                origin,
                direction,
                range,
                gun.GunData.surfaceCollisionMask,
                gun.GunData.targetCollisionMask,
                data.hoseSurfaceMagnetism,
                data.hoseTargetMagnetism,
                out _,
                hits,
                out _,
                out int targetHits);

            for (int i = 0; i < targetHits; i++)
            {
                if (hits[i].distance < 0f)
                    continue;

                ITarget target = IDamageSource.GetTarget(hits[i].collider);
                if (target == null)
                    continue;

                IDamageSource.DamageTarget(
                    gun,
                    target,
                    new DamageData(
                        damage,
                        EffectType.Cryo,
                        cryo,
                        DamageFlags.DamageOverTime | DamageFlags.AOE),
                    hits[i].point,
                    hits[i].collider);
            }

            ArrayPool<RaycastHit>.Release(hits, targetHits);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Whiteout] Hose tick: {ex.Message}");
        }

        // Light camera shake
        try
        {
            if (gun.playerLook != null)
            {
                gun.playerLook.ShakeTranslateMin(WhiteoutBalance.HoseShakeTranslate);
                gun.playerLook.ShakeRotationMin(WhiteoutBalance.HoseShakeRotation);
            }
        }
        catch { /* */ }
    }

    private void TickLobInput(Gun gun)
    {
        bool aimHeld = false;
        try
        {
            if (PlayerInput.Controls != null)
                aimHeld = PlayerInput.Controls.Player.Aim.IsPressed();
        }
        catch
        {
            aimHeld = false;
        }

        bool pressed = aimHeld && !aimWasHeld;
        aimWasHeld = aimHeld;

        if (!pressed)
            return;

        if (Time.time < lobReadyAt)
            return;

        bool reloading = false;
        try { reloading = gun.Reloading; } catch { /* */ }
        if (reloading)
            return;

        float tax = Mathf.Max(1f, data.lobMagTax);
        if (GetRemainingAmmo(gun) < tax)
            return;

        lobReadyAt = Time.time + Mathf.Max(0.05f, data.lobInputDebounce);
        FireLob(gun, tax);
    }

    private void FireLob(Gun gun, float tax)
    {
        SpendAmmo(gun, tax);

        if (!TryGetMuzzle(gun, out Vector3 position, out Vector3 forward))
            return;

        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

        GrenadeBullet prefab = WeaponRegistration.CachedLobPrefab;
        if (prefab == null)
        {
            WeaponRegistration.CacheLobPrefabFromBase(SparrohPlugin.Logger);
            prefab = WeaponRegistration.CachedLobPrefab;
        }

        if (prefab == null)
        {
            // Fallback: radial cryo boom at aim point / short range
            FireLobFallbackExplosion(gun, position, forward);
            return;
        }

        try
        {
            GrenadeBullet cell = SimplePool.Get(prefab);
            BulletData bulletData = gun.GunData.GetBulletData(ref position, ref rotation);
            bulletData.damage = data.lobDamage;
            bulletData.damageEffect = EffectType.Cryo;
            bulletData.damageEffectAmount = data.lobCryoAmount;
            bulletData.speed = data.lobSpeed;
            bulletData.gravity = data.lobGravity;
            bulletData.damageFlags |= DamageFlags.AOE;
            bulletData.maxBounces = 0;
            bulletData.force = data.lobRadius;

            if (releaseLob == null)
                releaseLob = ReleaseLob;

            cell.Initialize(
                bulletData,
                gun,
                releaseLob,
                gun.IsOwner ? BulletFlags.OwnerGunBullet : BulletFlags.None);

            try
            {
                if (gun.playerLook != null)
                {
                    gun.playerLook.ShakeTranslateMin(0.6f);
                    gun.playerLook.ShakeRotationMin(0.8f);
                }
            }
            catch { /* */ }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Whiteout] FireLob: {ex.Message}");
            FireLobFallbackExplosion(gun, position, forward);
        }
    }

    private void ReleaseLob(IBullet bullet)
    {
        GrenadeBullet prefab = WeaponRegistration.CachedLobPrefab;
        if (prefab != null && bullet is GrenadeBullet gb)
        {
            try { SimplePool.Release(prefab, gb); }
            catch { /* ignore */ }
        }
    }

    private void FireLobFallbackExplosion(Gun gun, Vector3 origin, Vector3 forward)
    {
        try
        {
            Vector3 pos = origin + forward * 2f;
            if (Physics.Raycast(origin, forward, out RaycastHit hit, 40f, ~0, QueryTriggerInteraction.Ignore))
                pos = hit.point;

            var dmg = new DamageData(
                data.lobDamage,
                EffectType.Cryo,
                data.lobCryoAmount,
                DamageFlags.AOE);

            if (GameManager.Instance != null)
            {
                try
                {
                    GameManager.Instance.SpawnExplosionFirstPerson(
                        gun, pos, data.lobRadius, TargetType.NonPlayer, dmg, 2f);
                }
                catch
                {
                    GameManager.Instance.SpawnExplosionObserverSeeThrough(
                        gun, pos, data.lobRadius, TargetType.NonPlayer, dmg, gun.OwnerClientId);
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Whiteout] Lob fallback: {ex.Message}");
        }
    }

    private static bool TryGetMuzzle(Gun gun, out Vector3 origin, out Vector3 direction)
    {
        origin = gun.transform.position;
        direction = gun.transform.forward;

        try
        {
            if (gun.GunData.firePoint != null)
            {
                origin = gun.GunData.firePoint.position;
                direction = gun.GunData.firePoint.forward;
            }
        }
        catch { /* */ }

        try
        {
            if (gun.playerLook != null)
            {
                // Prefer look direction for aim; keep firePoint position when available.
                direction = gun.playerLook.transform.forward;
                if (gun.GunData.firePoint == null)
                    origin = gun.playerLook.transform.position;
            }
        }
        catch { /* */ }

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector3.forward;
        else
            direction.Normalize();

        return true;
    }

    private static float GetRemainingAmmo(Gun gun)
    {
        try { return gun.RemainingAmmo; }
        catch
        {
            try { return gun.RemainingAmmoCount; }
            catch { return 0f; }
        }
    }

    private static void SpendAmmo(Gun gun, float amount)
    {
        if (amount <= 0f || gun == null)
            return;

        try
        {
            float remaining = gun.RemainingAmmo;
            gun.RemainingAmmo = Mathf.Max(0f, remaining - amount);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Whiteout] SpendAmmo: {ex.Message}");
        }
    }

    private void TryStartHoseVfx(Gun gun)
    {
        if (hoseVfxPlaying)
            return;

        try
        {
            VisualEffect template = ResolveFlamethrowerVfx(gun);
            if (template == null)
                return;

            Transform parent = null;
            try { parent = gun.gunModel != null ? gun.gunModel : gun.transform; }
            catch { parent = gun.transform; }

            if (hoseVfxInstance == null || !hoseVfxInstance.gameObject.scene.IsValid())
            {
                hoseVfxInstance = UnityEngine.Object.Instantiate(template, parent);
                if (!gun.IsOwner)
                {
                    try
                    {
                        var r = hoseVfxInstance.GetComponent<Renderer>();
                        if (r != null)
                            r.renderingLayerMask = 1u;
                    }
                    catch { /* */ }
                }
            }

            hoseVfxInstance.Play();
            hoseVfxPlaying = true;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Whiteout] Hose VFX start: {ex.Message}");
        }
    }

    private void StopHoseVfx()
    {
        if (!hoseVfxPlaying && hoseVfxInstance == null)
            return;

        try
        {
            if (hoseVfxInstance != null && hoseVfxInstance.gameObject.scene.IsValid())
                hoseVfxInstance.Stop();
        }
        catch { /* */ }

        hoseVfxPlaying = false;
    }

    private static VisualEffect ResolveFlamethrowerVfx(Gun gun)
    {
        try
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            FieldInfo field = typeof(BounceShotgun).GetField("flamethrowerEffect", flags);

            // Prefer live instance field if present.
            if (gun is BounceShotgun bounce && field?.GetValue(bounce) is VisualEffect live && live != null)
                return live;

            // Catalog / base prefab.
            Gun baseGun = WeaponRegistration.BaseGunPrefab;
            if (baseGun != null && field?.GetValue(baseGun) is VisualEffect fromBase && fromBase != null)
                return fromBase;
        }
        catch { /* */ }

        return null;
    }

    public static bool TryGet(IGear gear, out WhiteoutBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<WhiteoutBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = SparrohPlugin.IsOurGear(gear);
        WhiteoutBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<WhiteoutBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;
        behaviour = gear.gameObject.AddComponent<WhiteoutBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.data = prefabBehaviour.prefabSnapshot;
        behaviour.CapturePrefabSnapshot();
        return true;
    }
}
