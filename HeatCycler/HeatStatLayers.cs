using UnityEngine;

/// <summary>
/// Composes Hot / Redline / Overcap / MassAccel / Lite Energy stat layers.
/// Single place so zone brakes/carrots don't fight upgrade dynamics.
/// </summary>
internal static class HeatStatLayers
{
    /// <summary>
    /// Applies fire-interval layers onto gun data.
    /// Call each tick while the weapon is active.
    /// </summary>
    public static void ApplyFireIntervalLayers(
        Gun gun,
        in CyclerHeatBehaviour.Data data,
        float currentHeat,
        float heatNormalized,
        float baseFireIntervalCaptured,
        out float appliedInterval)
    {
        appliedInterval = -1f;
        if (gun == null)
            return;

        ref GunData gd = ref gun.GunData;
        float interval = baseFireIntervalCaptured > 0f
            ? baseFireIntervalCaptured
            : gd.fireInterval;

        // Mass Acceleration: hotter → faster (efficiency * heat)
        if (data.massAccelEfficiency > 0f && interval > 0f)
        {
            float mult = 1f + currentHeat * data.massAccelEfficiency;
            interval /= Mathf.Max(0.05f, mult);
        }

        // Soft Redline brake (not while overcapped — IB wants you there)
        bool overcapped = data.infinityBurn && currentHeat > data.maxHeat + 0.0001f;
        bool redline = !overcapped && currentHeat >= data.maxHeat - 0.0001f;
        if (redline && data.redlineFireIntervalMult > 1f)
            interval *= data.redlineFireIntervalMult;

        // Infinity Burn: slight RoF reward while overcapped
        if (overcapped && data.infinityBurnOvercapFireIntervalMult > 0f
            && data.infinityBurnOvercapFireIntervalMult < 1f)
            interval *= data.infinityBurnOvercapFireIntervalMult;

        gd.fireInterval = Mathf.Max(0.02f, interval);
        appliedInterval = gd.fireInterval;
        _ = heatNormalized;
    }


    public static void ApplyLiteEnergyBulletSpeed(
        Gun gun,
        in CyclerHeatBehaviour.Data data,
        float baseFireIntervalCaptured,
        float baseBulletSpeedCaptured)
    {
        if (gun == null || data.liteEnergyBulletSpeedBonus <= 0f)
            return;
        if (baseFireIntervalCaptured <= 0.01f || baseBulletSpeedCaptured <= 0f)
            return;

        ref GunData gd = ref gun.GunData;
        float frRatio = baseFireIntervalCaptured / Mathf.Max(0.02f, gd.fireInterval);
        float t = Mathf.Clamp01(frRatio - 1f);
        gd.bulletSpeed = baseBulletSpeedCaptured * (1f + data.liteEnergyBulletSpeedBonus * t);
    }

    /// <summary>Damage / element carrots for Hot, Redline, and Infinity Burn overcap.</summary>
    public static void ApplyZoneToBullet(
        ref BulletData bullet,
        in CyclerHeatBehaviour.Data data,
        HeatZone zone,
        float heatNormalized)
    {
        float dmgMult = 1f;
        float elemMult = 1f;

        switch (zone)
        {
            case HeatZone.Hot:
                dmgMult *= data.hotDamageMult > 0f ? data.hotDamageMult : 1f;
                elemMult *= data.hotElementMult > 0f ? data.hotElementMult : 1f;
                break;
            case HeatZone.Redline:
                dmgMult *= data.redlineDamageMult > 0f ? data.redlineDamageMult : 1f;
                elemMult *= data.redlineElementMult > 0f ? data.redlineElementMult : 1f;
                break;
            case HeatZone.Overcap:
                // Base redline carrot + overcap scale
                dmgMult *= data.redlineDamageMult > 0f ? data.redlineDamageMult : 1f;
                elemMult *= data.redlineElementMult > 0f ? data.redlineElementMult : 1f;
                float oc = Mathf.Max(0f, heatNormalized - 1f); // 0 at cap, 1 at 200%
                dmgMult *= 1f + oc * Mathf.Max(0f, data.infinityBurnOutgoingDamagePerOvercap);
                elemMult *= 1f + oc * Mathf.Max(0f, data.infinityBurnOutgoingElementPerOvercap);
                break;
        }

        if (dmgMult != 1f)
            bullet.damage *= dmgMult;
        if (elemMult != 1f && bullet.damageEffect > EffectType.Normal)
            bullet.damageEffectAmount *= elemMult;
    }

    /// <summary>
    /// Redline spread is applied on bullets (GunData spread field layout varies).
    /// Slight horizontal/vertical aim noise proxy via reduced effective accuracy feel:
    /// callers may also widen via fire constraints; this keeps compile-safe.
    /// </summary>
    public static void ApplyRedlineSpreadToBullet(
        ref BulletData bullet,
        in CyclerHeatBehaviour.Data data,
        HeatZone zone)
    {
        if (zone != HeatZone.Redline)
            return;
        if (data.redlineSpreadMult <= 1f)
            return;

        // No direct spread field on BulletData in all builds — nudge magnetism down so shots feel looser.
        if (bullet.targetMagnetism > 0f)
            bullet.targetMagnetism /= data.redlineSpreadMult;
    }
}

