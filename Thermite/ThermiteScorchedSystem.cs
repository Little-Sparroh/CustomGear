using System;
using System.Collections.Generic;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Scorched Earth: lingering fire ground on primary detonate.
/// Enemy ticks: light damage + Fire apply. No ally HoT.
/// Funeral Mote: allies who move into/through field get one instant heal per field.
/// Mobile Hearth merge: fields count as heat-home for move-gated recharge.
/// </summary>
public static class ThermiteScorchedSystem
{
    private static readonly List<Field> Fields = new List<Field>(4);
    private static GameObject _runnerGo;
    private static ThermiteScorchedRunner _runner;

    private sealed class Field
    {
        public Vector3 Position;
        public float Radius;
        public float ExpireTime;
        public float NextTickTime;
        public float TickInterval;
        public float TickDamage;
        public float TickFire;
        public float FuneralHeal;
        public GameObject Visual;
        public readonly HashSet<int> FuneralGranted = new HashSet<int>();
        public readonly HashSet<int> WasInside = new HashSet<int>();
    }

    /// <summary>
    /// Always-on baseline fire pool from <see cref="ThermiteBalance"/>.
    /// Replaces impact damage during the sandbox pass; no exotic flag required.
    /// </summary>
    public static void PlantBaselinePool(IGear gear, Vector3 position)
    {
        if (gear == null)
            return;

        if (gear is not Throwable throwable || throwable.Player == null)
            return;
        if (!throwable.Player.IsLocalPlayer)
            return;

        float boomRadius = ThermiteBalance.HitForce;
        if (gear is IWeapon weapon)
            boomRadius = Mathf.Max(1f, weapon.GunData.hitForce);

        float radius = Mathf.Max(1f, boomRadius * ThermiteBalance.FirePoolRadiusMult);
        float duration = ThermiteBalance.FirePoolDuration;
        float tickInterval = ThermiteBalance.FirePoolTickInterval;
        float tickDamage = ThermiteBalance.FirePoolTickDamage;
        float tickFire = ThermiteBalance.FirePoolTickFire;
        int maxFields = ThermiteBalance.FirePoolMaxConcurrent > 0
            ? ThermiteBalance.FirePoolMaxConcurrent
            : 2;

        PlantInternal(
            gear,
            position,
            radius,
            duration,
            tickInterval,
            tickDamage,
            tickFire,
            funeralHeal: 0f,
            maxFields,
            logTag: "Baseline pool");
    }

    /// <summary>
    /// Scorched Earth exotic plant — upgrade-gated, scales from ThermiteBehaviour.Data.
    /// </summary>
    public static void PlantField(IGear gear, Vector3 position, ThermiteBehaviour behaviour)
    {
        if (gear == null || behaviour == null)
            return;

        ref ThermiteBehaviour.Data data = ref behaviour.GrenadeData;
        if (!data.scorchedEarth)
            return;

        if (gear is not Throwable throwable || throwable.Player == null)
            return;
        if (!throwable.Player.IsLocalPlayer)
            return;

        float duration = data.scorchedDuration > 0f ? data.scorchedDuration : 8f;
        if (data.fieldDurationMultiplier > 0f)
            duration *= data.fieldDurationMultiplier;

        float scorchedMult = data.scorchedRadius > 0f ? data.scorchedRadius : 1.35f;
        scorchedMult *= Mathf.Max(0.25f, data.warmFrontRadiusMult);

        float boomRadius = 6f;
        if (gear is IWeapon weapon)
            boomRadius = Mathf.Max(3f, weapon.GunData.hitForce * Mathf.Max(0.25f, data.explosionRadiusMultiplier));

        float radius = boomRadius * scorchedMult * 1.25f;
        radius = Mathf.Clamp(radius, 8f, 40f);

        float tickInterval = data.scorchedTickInterval > 0.05f ? data.scorchedTickInterval : 0.5f;
        float tickDamage = data.scorchedTickDamage > 0f ? data.scorchedTickDamage : 4f;
        float tickFire = data.scorchedTickFire > 0f ? data.scorchedTickFire : 2.5f;
        float funeral = Mathf.Max(0f, data.funeralMoteHealAmount);

        int maxFields = data.maxScorchedFields > 0 ? data.maxScorchedFields : 2;

        PlantInternal(
            gear,
            position,
            radius,
            duration,
            tickInterval,
            tickDamage,
            tickFire,
            funeral,
            maxFields,
            logTag: "Scorched field");
    }

    private static void PlantInternal(
        IGear gear,
        Vector3 position,
        float radius,
        float duration,
        float tickInterval,
        float tickDamage,
        float tickFire,
        float funeralHeal,
        int maxFields,
        string logTag)
    {
        while (Fields.Count >= maxFields)
            RemoveFieldAt(0);

        GameObject visual = CreateVisual(position, radius);
        float now = Time.time;
        Fields.Add(new Field
        {
            Position = position,
            Radius = radius,
            ExpireTime = now + duration,
            NextTickTime = now + tickInterval,
            TickInterval = tickInterval,
            TickDamage = tickDamage,
            TickFire = tickFire,
            FuneralHeal = funeralHeal,
            Visual = visual
        });

        EnsureRunner(gear);
        ThermitePlugin.Logger?.LogDebug(
            $"[Thermite] {logTag} planted r={radius:0.#} t={duration:0.#}s dmg={tickDamage:0.#} fire={tickFire:0.#}");
    }


    public static void EnsureRunner(IGear gear)
    {
        if (_runner != null)
        {
            _runner.Bind(gear);
            return;
        }

        if (_runnerGo == null)
        {
            _runnerGo = new GameObject("[Thermite] ScorchedRunner");
            UnityEngine.Object.DontDestroyOnLoad(_runnerGo);

        }

        _runner = _runnerGo.GetComponent<ThermiteScorchedRunner>();
        if (_runner == null)
            _runner = _runnerGo.AddComponent<ThermiteScorchedRunner>();
        _runner.Bind(gear);
    }

    /// <summary>True if local player is inside any active scorched field.</summary>
    public static bool IsLocalPlayerInAnyField(Player player, out float bestRadius)
    {
        bestRadius = 0f;
        if (player == null || Fields.Count == 0)
            return false;

        Vector3 pos = player.InterpolatedPosition;
        bool inside = false;
        for (int i = 0; i < Fields.Count; i++)
        {
            Field f = Fields[i];
            float r = f.Radius;
            if ((pos - f.Position).sqrMagnitude <= r * r)
            {
                inside = true;
                if (r > bestRadius)
                    bestRadius = r;
            }
        }

        return inside;
    }

    public static void Tick(IGear gear, ThermiteBehaviour behaviour, Player localPlayer)
    {
        if (gear == null || behaviour == null)
            return;

        float now = Time.time;
        for (int i = Fields.Count - 1; i >= 0; i--)
        {
            if (now >= Fields[i].ExpireTime)
                RemoveFieldAt(i);
        }

        if (Fields.Count == 0)
            return;

        ref ThermiteBehaviour.Data data = ref behaviour.GrenadeData;

        // Enemy damage ticks (owner only). Baseline pools tick without scorchedEarth.
        for (int i = 0; i < Fields.Count; i++)
        {
            Field f = Fields[i];
            if (now < f.NextTickTime)
                continue;

            f.NextTickTime = now + f.TickInterval;
            TickEnemies(gear, f);
        }

        // Funeral Mote — movement-gated ally heal (exotic path only).
        if (data.scorchedEarth &&
            data.funeralMoteHealAmount > 0f &&
            GameManager.players != null)
            TickFuneralMote(localPlayer, data.funeralMoteHealAmount, gear);

    }

    private static void TickEnemies(IGear gear, Field field)
    {
        if (GameManager.Instance == null || gear is not IDamageSource source)
            return;

        var enumerator = default(IDamageSource.TargetEnumerator);
        try
        {
            if (!enumerator.GetTargetsInSphere(field.Position, field.Radius, 345216, TargetType.NonPlayer))
                return;

            var damage = new DamageData(
                field.TickDamage,
                EffectType.Fire,
                field.TickFire,
                DamageFlags.AOE | DamageFlags.DamageOverTime);

            while (enumerator.MoveNext())
            {
                ITarget t = enumerator.Current;
                if (t == null || !t.Exists() || t.IsPlayer())
                    continue;

                try
                {
                    IDamageSource.DamageTarget(source, t, damage, t.GetHealthbarPosition(), null);
                }
                catch
                {
                }
            }
        }
        finally
        {
            try { ((IDisposable)enumerator).Dispose(); } catch { /* ignore */ }
        }
    }

    private static void TickFuneralMote(Player localPlayer, float healFallback, IGear gear)
    {
        if (GameManager.players == null)
            return;

        float speedGate = 1.2f;

        for (int p = 0; p < GameManager.players.Count; p++)
        {
            Player ally = GameManager.players[p];
            if (ally == null || !ally.IsAlive)
                continue;

            int id = ally.GetInstanceID();
            Vector3 pos = ally.InterpolatedPosition;

            float speed = 0f;
            try
            {
                if (ally.TryGetComponent<Rigidbody>(out var rb) && rb != null)
                    speed = rb.velocity.magnitude;
            }
            catch
            {
            }

            bool moving = speed >= speedGate;
            // Local player speed may come from hearth runner estimate if rb is quiet.
            if (!moving && ally.IsLocalPlayer && _runner != null)
                moving = _runner.EstimatedSpeed >= speedGate;

            for (int i = 0; i < Fields.Count; i++)
            {
                Field f = Fields[i];
                float heal = f.FuneralHeal > 0f ? f.FuneralHeal : healFallback;
                if (heal <= 0f)
                    continue;

                float r = f.Radius;
                bool inside = (pos - f.Position).sqrMagnitude <= r * r;
                bool wasInside = f.WasInside.Contains(id);

                if (inside && !wasInside)
                {
                    f.WasInside.Add(id);
                    // Must be moving when entering (or crossing edge).
                    if (moving && !f.FuneralGranted.Contains(id))
                    {
                        f.FuneralGranted.Add(id);
                        try
                        {
                            ally.Heal(heal, gear as IDamageSource);
                            if (ally.IsLocalPlayer)
                            {
                                try { Global.PlayHealInstantSound(); } catch { /* ignore */ }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                else if (!inside && wasInside)
                {
                    f.WasInside.Remove(id);
                }
                else if (inside)
                {
                    f.WasInside.Add(id);
                }
            }
        }
    }

    public static void ClearAll()
    {
        for (int i = Fields.Count - 1; i >= 0; i--)
            RemoveFieldAt(i);
    }

    private static void RemoveFieldAt(int index)
    {
        if (index < 0 || index >= Fields.Count)
            return;

        Field f = Fields[index];
        if (f.Visual != null)
            UnityEngine.Object.Destroy(f.Visual);

        Fields.RemoveAt(index);
    }

    private static GameObject CreateVisual(Vector3 position, float radius)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "[Thermite] ScorchedField";
        UnityEngine.Object.Destroy(go.GetComponent<Collider>());

        go.transform.position = position + Vector3.up * 0.04f;
        float diameter = Mathf.Max(0.5f, radius * 2f);
        go.transform.localScale = new Vector3(diameter, 0.04f, diameter);

        try
        {
            var rend = go.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                rend.material = new Material(Shader.Find("Standard") ?? rend.sharedMaterial.shader);
                if (rend.material.HasProperty("_Color"))
                    rend.material.color = new Color(0.95f, 0.85f, 0.55f, 0.4f); // white-hot slag
            }
        }
        catch
        {
        }

        UnityEngine.Object.DontDestroyOnLoad(go);

        return go;
    }
}

/// <summary>Ticks scorched fields and feeds speed estimate for Funeral Mote.</summary>
public sealed class ThermiteScorchedRunner : MonoBehaviour
{
    private IGear _gear;
    private Vector3 _lastPos;
    private bool _hasLastPos;

    public float EstimatedSpeed { get; private set; }

    public void Bind(IGear gear)
    {
        _gear = gear;
        _hasLastPos = false;
    }

    private void Update()
    {
        if (_gear == null || !ThermiteBehaviour.TryGet(_gear, out ThermiteBehaviour behaviour))
            return;

        // Baseline pools keep the runner alive even without Scorched Earth exotic.
        Player player = null;

        if (_gear is Throwable t)
            player = t.Player;
        if (player == null)
            player = Player.LocalPlayer;
        if (player == null || !player.IsLocalPlayer)
            return;

        Vector3 pos = player.InterpolatedPosition;
        if (_hasLastPos)
        {
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            EstimatedSpeed = (pos - _lastPos).magnitude / dt;
        }
        else
        {
            EstimatedSpeed = 0f;
            _hasLastPos = true;
        }

        _lastPos = pos;
        ThermiteScorchedSystem.Tick(_gear, behaviour, player);
    }

    private void OnDestroy()
    {
        ThermiteScorchedSystem.ClearAll();
    }
}
