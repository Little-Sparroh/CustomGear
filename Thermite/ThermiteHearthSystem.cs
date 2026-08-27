using System.Collections.Generic;
using Pigeon.Movement;
using UnityEngine;

/// <summary>
/// Mobile Hearth: plant ember zones on detonate / IC nova; recharge only while
/// the local player is inside AND moving. Stationary bonus is near-zero (design locked).
/// </summary>
public static class ThermiteHearthSystem
{
    private static readonly List<Ember> Embers = new List<Ember>(4);
    private static GameObject _runnerGo;
    private static ThermiteHearthRunner _runner;

    private struct Ember
    {
        public Vector3 Position;
        public float Radius;
        public float ExpireTime;
        public GameObject Visual;
    }

    /// <summary>Plant or refresh an ember at world position.</summary>
    public static void PlantEmber(IGear gear, Vector3 position, ThermiteBehaviour behaviour)
    {
        if (gear == null || behaviour == null)
            return;

        ref ThermiteBehaviour.Data data = ref behaviour.GrenadeData;
        if (!data.mobileHearth)
            return;

        if (gear is not Throwable throwable || throwable.Player == null)
            return;
        if (!throwable.Player.IsLocalPlayer)
            return;

        float duration = data.hearthDuration;
        if (duration <= 0f)
            duration = 8f;
        if (data.fieldDurationMultiplier > 0f)
            duration *= data.fieldDurationMultiplier;

        // Ember radius ≈ boom radius × hearth mult × Warm Front.
        // hitForce is the grenade explosion radius source (same as vanilla Hearth).
        float hearthMult = data.hearthRadius > 0f ? data.hearthRadius : 1.5f;
        hearthMult *= Mathf.Max(0.25f, data.warmFrontRadiusMult);

        float boomRadius = 6f; // safe floor if weapon data missing
        if (gear is IWeapon weapon)
            boomRadius = Mathf.Max(3f, weapon.GunData.hitForce * Mathf.Max(0.25f, data.explosionRadiusMultiplier));

        // Slightly larger than the boom so kiting the ring is comfortable.
        float radius = boomRadius * hearthMult * 1.35f;
        radius = Mathf.Clamp(radius, 8f, 40f);


        // Keep a small cap of concurrent embers (Two's Company / multi-throw).
        const int maxEmbers = 2;
        while (Embers.Count >= maxEmbers)
            RemoveEmberAt(0);

        GameObject visual = CreateVisual(position, radius);
        Embers.Add(new Ember
        {
            Position = position,
            Radius = radius,
            ExpireTime = Time.time + duration,
            Visual = visual
        });

        EnsureRunner(gear);
        ThermitePlugin.Logger?.LogDebug(
            $"[Thermite] Hearth ember planted r={radius:0.#} t={duration:0.#}s at {position}");
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
            _runnerGo = new GameObject("[Thermite] HearthRunner");
            UnityEngine.Object.DontDestroyOnLoad(_runnerGo);

        }

        _runner = _runnerGo.GetComponent<ThermiteHearthRunner>();
        if (_runner == null)
            _runner = _runnerGo.AddComponent<ThermiteHearthRunner>();
        _runner.Bind(gear);
    }

    public static void Tick(IGear gear, ThermiteBehaviour behaviour, Player player)
    {
        if (gear == null || behaviour == null || player == null)
            return;

        // Expire old embers.
        float now = Time.time;
        for (int i = Embers.Count - 1; i >= 0; i--)
        {
            if (now >= Embers[i].ExpireTime)
                RemoveEmberAt(i);
        }

        ref ThermiteBehaviour.Data data = ref behaviour.GrenadeData;
        if (!data.mobileHearth)
            return;

        if (gear is not Throwable throwable)
            return;

        Vector3 pos = player.InterpolatedPosition;
        float speed = 0f;
        try
        {
            // Prefer rigidbody / movement velocity if available.
            if (player.TryGetComponent<Rigidbody>(out var rb) && rb != null)
                speed = rb.velocity.magnitude;
        }
        catch
        {
        }

        // Fallback: estimate from position delta stored on runner.
        if (speed <= 0.01f && _runner != null)
            speed = _runner.EstimatedSpeed;

        float gate = data.hearthMoveSpeedGate > 0f ? data.hearthMoveSpeedGate : 1.5f;
        bool moving = speed >= gate;

        float bestMult = 0f;
        bool insideAny = false;

        for (int i = 0; i < Embers.Count; i++)
        {
            Ember e = Embers[i];
            float r = e.Radius;
            if ((pos - e.Position).sqrMagnitude > r * r)
                continue;

            insideAny = true;
            float mult = moving
                ? Mathf.Max(0f, data.hearthRechargeMultMoving)
                : Mathf.Max(0f, data.hearthRechargeMultStationary);

            // Warm Front adds move-recharge while in heat.
            if (moving && data.warmFrontRechargeMult > 0f)
                mult += data.warmFrontRechargeMult;

            if (mult > bestMult)
                bestMult = mult;
        }

        // Scorched Earth merge: scorched fields count as heat-home (single pipeline, no double-dip).
        if (data.scorchedEarth &&
            ThermiteScorchedSystem.IsLocalPlayerInAnyField(player, out _))
        {
            insideAny = true;
            float mult = moving
                ? Mathf.Max(0f, data.hearthRechargeMultMoving)
                : Mathf.Max(0f, data.hearthRechargeMultStationary);
            if (moving && data.warmFrontRechargeMult > 0f)
                mult += data.warmFrontRechargeMult;
            if (mult > bestMult)
                bestMult = mult;
        }

        if (!insideAny || bestMult <= 0f)
            return;


        // bestMult is a recharge rate multiplier bonus (e.g. 1.0 = +100% → 2× charge rate).
        // AddCharge expects fraction of a full charge.
        float rechargeDuration = Mathf.Max(0.1f, throwable.CooldownData.rechargeDuration);
        float charge = bestMult * Time.deltaTime / rechargeDuration;
        if (charge > 0f)
            throwable.AddCharge(charge);
    }

    public static void ClearAll()
    {
        for (int i = Embers.Count - 1; i >= 0; i--)
            RemoveEmberAt(i);
    }

    private static void RemoveEmberAt(int index)
    {
        if (index < 0 || index >= Embers.Count)
            return;

        Ember e = Embers[index];
        if (e.Visual != null)
            UnityEngine.Object.Destroy(e.Visual);

        Embers.RemoveAt(index);
    }

    private static GameObject CreateVisual(Vector3 position, float radius)
    {
        // Lightweight placeholder disc — no asset dependency.
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "[Thermite] HearthEmber";
        UnityEngine.Object.Destroy(go.GetComponent<Collider>());

        go.transform.position = position + Vector3.up * 0.05f;
        float diameter = Mathf.Max(0.5f, radius * 2f);
        go.transform.localScale = new Vector3(diameter, 0.05f, diameter);

        try
        {
            var rend = go.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                // Unlit-ish warm tint; shared material instance so we don't leak forever.
                rend.material = new Material(Shader.Find("Standard") ?? rend.sharedMaterial.shader);
                if (rend.material.HasProperty("_Color"))
                    rend.material.color = new Color(1f, 0.45f, 0.1f, 0.35f);
                if (rend.material.HasProperty("_Mode"))
                {
                    // Best-effort transparent; ignore if shader rejects.
                }
            }
        }
        catch
        {
        }

        UnityEngine.Object.DontDestroyOnLoad(go);

        return go;
    }
}

/// <summary>DontDestroyOnLoad runner that ticks Mobile Hearth recharge.</summary>
public sealed class ThermiteHearthRunner : MonoBehaviour
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

        // Keep runner alive for speed estimate even if only Scorched is equipped later;
        // recharge still requires mobileHearth inside Tick.
        if (!behaviour.GrenadeData.mobileHearth && !behaviour.GrenadeData.scorchedEarth)
            return;

        Player player = null;
        if (_gear is Throwable t)
            player = t.Player;
        if (player == null)
            player = Player.LocalPlayer;
        if (player == null || !player.IsLocalPlayer || !player.IsAlive)
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
        ThermiteHearthSystem.Tick(_gear, behaviour, player);
    }

    private void OnDestroy()
    {
        ThermiteHearthSystem.ClearAll();
    }
}
