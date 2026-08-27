using System;
using System.Collections.Generic;
using Pigeon.Math;
using UnityEngine;

/// <summary>
/// Caustic Flask upgrade properties — Phase 2 standard spine.
/// Mutate <see cref="CausticFlaskBehaviour.Data"/> and shared GunData / CooldownData / SelfEffectMultiplier.
/// Remove restores behaviour snapshot + touched prefab fields (ApplyUpgrades is remove-all then apply-all).
/// </summary>

/// <summary>Wide Mouth — explosion (+ future puddle) radius. No CD tax.</summary>
[Serializable]
public class FlaskWideMouthProperty : UpgradeProperty
{
    public global::Range<float> radiusBonus = new global::Range<float>(0.15f, 0.30f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Explosion radius:",
            radiusBonus,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float mult = 1f + radiusBonus.GetValue(ref rand, upgrade, default(BoostParams));
        behaviour.GrenadeData.explosionRadiusMultiplier *= mult;

        if (gear is IWeapon weapon)
            weapon.GunData.hitForce *= mult;
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            behaviour.RestoreFromPrefab();

        if (gear is IWeapon weapon && prefab is IWeapon prefabWeapon)
            weapon.GunData.hitForce = prefabWeapon.GunData.hitForce;
    }
}

/// <summary>Strong Solvent — Acid effect amount on boom (and later puddle slightly).</summary>
[Serializable]
public class FlaskStrongSolventProperty : UpgradeProperty
{
    public global::Range<float> effectBonus = new global::Range<float>(0.20f, 0.40f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Acid amount:",
            effectBonus,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float mult = 1f + effectBonus.GetValue(ref rand, upgrade, default(BoostParams));
        behaviour.GrenadeData.acidEffectAmountMultiplier *= mult;

        if (gear is IWeapon weapon)
            weapon.GunData.damageEffectAmount *= mult;
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            behaviour.RestoreFromPrefab();

        if (gear is IWeapon weapon && prefab is IWeapon prefabWeapon)
            weapon.GunData.damageEffectAmount = prefabWeapon.GunData.damageEffectAmount;
    }
}

/// <summary>Quick Cap — pure recharge CDR. No other tax.</summary>
[Serializable]
public class FlaskQuickCapProperty : UpgradeProperty
{
    public global::Range<float> rechargeReduction = new global::Range<float>(0.12f, 0.20f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Recharge:",
            rechargeReduction,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out _))
            return;

        float reduction = rechargeReduction.GetValue(ref rand, upgrade, default(BoostParams));
        if (gear is Throwable throwable)
            throwable.CooldownData.rechargeDuration *= Mathf.Max(0.25f, 1f - reduction);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (gear is Throwable live && prefab is Throwable prefabThrowable)
            live.CooldownData.rechargeDuration = prefabThrowable.CooldownData.rechargeDuration;
    }
}

/// <summary>Hard Flask — impact / boom damage.</summary>
[Serializable]
public class FlaskHardFlaskProperty : UpgradeProperty
{
    public global::Range<float> damageBonus = new global::Range<float>(0.12f, 0.22f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Damage:",
            damageBonus,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float mult = 1f + damageBonus.GetValue(ref rand, upgrade, default(BoostParams));
        behaviour.GrenadeData.boomDamageMultiplier *= mult;

        if (gear is IWeapon weapon)
            weapon.GunData.damage *= mult;
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            behaviour.RestoreFromPrefab();

        if (gear is IWeapon weapon && prefab is IWeapon prefabWeapon)
            weapon.GunData.damage = prefabWeapon.GunData.damage;
    }
}

/// <summary>Base Lining — less self Acid application from your flask.</summary>
[Serializable]
public class FlaskBaseLiningProperty : UpgradeProperty
{
    public global::Range<float> selfAcidReduction = new global::Range<float>(0.25f, 0.40f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Self acid:",
            selfAcidReduction,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float reduction = selfAcidReduction.GetValue(ref rand, upgrade, default(BoostParams));
        float mult = Mathf.Max(0.05f, 1f - reduction);
        behaviour.GrenadeData.selfAcidMultiplier *= mult;

        if (gear is GrenadeGear grenade)
            grenade.SelfEffectMultiplier *= mult;
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            behaviour.RestoreFromPrefab();

        if (gear is GrenadeGear live && prefab is GrenadeGear prefabGrenade)
            live.SelfEffectMultiplier = prefabGrenade.SelfEffectMultiplier;
    }
}

/// <summary>Deep Vat — puddle / reservoir duration mult (data-ready; Field systems use it later).</summary>
[Serializable]
public class FlaskDeepVatProperty : UpgradeProperty
{
    public global::Range<float> durationBonus = new global::Range<float>(0.20f, 0.35f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Field duration:",
            durationBonus,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float mult = 1f + durationBonus.GetValue(ref rand, upgrade, default(BoostParams));
        behaviour.GrenadeData.fieldDurationMultiplier *= mult;

        // If a field is already unlocked, scale live durations too.
        if (behaviour.GrenadeData.puddleDuration > 0f)
            behaviour.GrenadeData.puddleDuration *= mult;
        if (behaviour.GrenadeData.reservoirDuration > 0f)
            behaviour.GrenadeData.reservoirDuration *= mult;

        behaviour.SyncToVanillaAcidGrenade(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }
    }
}

// =============================================================================
// Phase 5 — Carapace (timed armor)
// =============================================================================

/// <summary>Polymer Plating (Rare) — boom grants timed Armor (DR) to players in radius.</summary>
[Serializable]
public class FlaskPolymerPlatingProperty : UpgradeProperty
{
    public global::Range<float> dr = new global::Range<float>(0.12f, 0.18f);
    public global::Range<float> duration = new global::Range<float>(2.5f, 3.5f);
    public global::Range<float> radiusMult = new global::Range<float>(1.0f, 1.15f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create("Armor DR:", dr, upgrade, ref rand, OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
        yield return StatData.Create("Armor duration:", duration, upgrade, ref rand, OverrideType.Add,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
        yield return StatData.Create("Armor radius:", radiusMult, upgrade, ref rand, OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float d = dr.GetValue(ref rand, upgrade, default(BoostParams));
        float t = duration.GetValue(ref rand, upgrade, default(BoostParams));
        float r = radiusMult.GetValue(ref rand, upgrade, default(BoostParams));

        behaviour.GrenadeData.armorDr += d;
        behaviour.GrenadeData.armorDuration = Mathf.Max(behaviour.GrenadeData.armorDuration, t);
        behaviour.GrenadeData.armorRadiusMult *= r;

        // Solvent Cure baseline when any armor source is present.
        if (behaviour.GrenadeData.solventCureDurationPerCorroded <= 0f)
            behaviour.GrenadeData.solventCureDurationPerCorroded = 0.15f;
        if (behaviour.GrenadeData.solventCureDrPerFullyCorroded <= 0f)
            behaviour.GrenadeData.solventCureDrPerFullyCorroded = 0.01f;
        if (behaviour.GrenadeData.corrosionPulseDuration <= 0f)
            behaviour.GrenadeData.corrosionPulseDuration = 0.4f;

        FlaskArmorPlayerHooks.EnsureBound(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            behaviour.RestoreFromPrefab();
    }
}

/// <summary>Saxonite Carapace (Exotic) — strong timed Armor + Solvent Cure scaling.</summary>
[Serializable]
public class FlaskSaxoniteCarapaceProperty : UpgradeProperty
{
    public global::Range<float> dr = new global::Range<float>(0.22f, 0.30f);
    public global::Range<float> duration = new global::Range<float>(3.5f, 5.0f);
    public global::Range<float> radiusMult = new global::Range<float>(1.1f, 1.3f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create("Carapace DR:", dr, upgrade, ref rand, OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
        yield return StatData.Create("Carapace duration:", duration, upgrade, ref rand, OverrideType.Add,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
        yield return StatData.Create("Armor radius:", radiusMult, upgrade, ref rand, OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float d = dr.GetValue(ref rand, upgrade, default(BoostParams));
        float t = duration.GetValue(ref rand, upgrade, default(BoostParams));
        float r = radiusMult.GetValue(ref rand, upgrade, default(BoostParams));

        behaviour.GrenadeData.saxoniteCarapace = true;
        behaviour.GrenadeData.armorDr = Mathf.Max(behaviour.GrenadeData.armorDr, d);
        behaviour.GrenadeData.armorDuration = Mathf.Max(behaviour.GrenadeData.armorDuration, t);
        behaviour.GrenadeData.armorRadiusMult *= r;
        behaviour.GrenadeData.armorDrCap = ArmorPlatingBuff.HardDrCap;

        // Stronger Solvent Cure / Corrosion Pulse with Carapace.
        behaviour.GrenadeData.solventCureDurationPerCorroded =
            Mathf.Max(behaviour.GrenadeData.solventCureDurationPerCorroded, 0.25f);
        behaviour.GrenadeData.solventCureDrPerFullyCorroded =
            Mathf.Max(behaviour.GrenadeData.solventCureDrPerFullyCorroded, 0.015f);
        behaviour.GrenadeData.corrosionPulseDuration =
            Mathf.Max(behaviour.GrenadeData.corrosionPulseDuration, 0.6f);
        behaviour.GrenadeData.corrosionPulseIcd =
            behaviour.GrenadeData.corrosionPulseIcd > 0f ? behaviour.GrenadeData.corrosionPulseIcd : 1.5f;
        behaviour.GrenadeData.corrosionPulseRadius =
            behaviour.GrenadeData.corrosionPulseRadius > 0f ? behaviour.GrenadeData.corrosionPulseRadius : 12f;

        FlaskArmorPlayerHooks.EnsureBound(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            behaviour.RestoreFromPrefab();
    }
}

/// <summary>Plate Polish (Rare) — +Armor duration and/or mild +DR within hard cap.</summary>
[Serializable]
public class FlaskPlatePolishProperty : UpgradeProperty
{
    public global::Range<float> durationMult = new global::Range<float>(0.15f, 0.30f);
    public global::Range<float> drAdd = new global::Range<float>(0.03f, 0.06f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create("Armor duration:", durationMult, upgrade, ref rand, OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
        yield return StatData.Create("Armor DR:", drAdd, upgrade, ref rand, OverrideType.Add,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float dm = durationMult.GetValue(ref rand, upgrade, default(BoostParams));
        float da = drAdd.GetValue(ref rand, upgrade, default(BoostParams));
        behaviour.GrenadeData.platePolishDurationMult *= (1f + dm);
        behaviour.GrenadeData.platePolishDrAdd += da;
        FlaskArmorPlayerHooks.EnsureBound(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            behaviour.RestoreFromPrefab();
    }
}

/// <summary>
/// Puddle Harden (Rare) — while Armor is active, standing in your puddle mildly refreshes duration.
/// Bridge Field+Carapace. Does not create armor from zero.
/// </summary>
[Serializable]
public class FlaskPuddleHardenProperty : UpgradeProperty
{
    public global::Range<float> refresh = new global::Range<float>(0.35f, 0.6f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create("Puddle armor refresh:", refresh, upgrade, ref rand, OverrideType.Add,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        behaviour.GrenadeData.puddleHardenRefresh +=
            refresh.GetValue(ref rand, upgrade, default(BoostParams));
        FlaskArmorPlayerHooks.EnsureBound(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            behaviour.RestoreFromPrefab();
    }
}

/// <summary>
/// Defensive Spurt (Epic) — while Armor is active, chance on taking damage to emit a small acid explosion.
/// </summary>
[Serializable]
public class FlaskDefensiveSpurtProperty : UpgradeProperty
{
    public global::Range<float> chance = new global::Range<float>(0.25f, 0.40f);
    public global::Range<float> size = new global::Range<float>(2.2f, 3.2f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create("Spurt chance:", chance, upgrade, ref rand, OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
        yield return StatData.Create("Spurt size:", size, upgrade, ref rand, OverrideType.Add,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        behaviour.GrenadeData.damageExplodeChance =
            Mathf.Max(behaviour.GrenadeData.damageExplodeChance,
                chance.GetValue(ref rand, upgrade, default(BoostParams)));
        behaviour.GrenadeData.damageExplodeSize =
            Mathf.Max(behaviour.GrenadeData.damageExplodeSize,
                size.GetValue(ref rand, upgrade, default(BoostParams)));
        FlaskArmorPlayerHooks.EnsureBound(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            behaviour.RestoreFromPrefab();
    }
}



/// <summary>
/// Twin Flask — +1 max charge with explicit mild +CD on this card only.
/// </summary>
[Serializable]
public class FlaskTwinFlaskProperty : UpgradeProperty
{
    public global::Range<int> extraCharges = new global::Range<int>(1, 1);
    public global::Range<float> rechargePenalty = new global::Range<float>(0.15f, 0.25f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Max charges:",
            extraCharges,
            upgrade,
            ref rand,
            OverrideType.Add,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
        yield return StatData.Create(
            "Recharge:",
            rechargePenalty,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out _))
            return;

        if (gear is not Throwable throwable)
            return;

        int extra = extraCharges.GetValue(ref rand, upgrade, default(BoostParams));
        float penalty = rechargePenalty.GetValue(ref rand, upgrade, default(BoostParams));

        throwable.CooldownData.maxCharges = Mathf.Max(1, throwable.CooldownData.maxCharges + extra);
        throwable.CooldownData.rechargeDuration *= (1f + penalty);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (gear is Throwable live && prefab is Throwable prefabThrowable)
        {
            live.CooldownData.maxCharges = prefabThrowable.CooldownData.maxCharges;
            live.CooldownData.rechargeDuration = prefabThrowable.CooldownData.rechargeDuration;
        }
    }
}

/// <summary>
/// Viscous Mix — longer fuse / more pull arm time when vacuum is active.
/// Stores pullFuseBonus; Vacuum phase reads it. Also lengthens GunData.reloadDuration (fuse).
/// </summary>
[Serializable]
public class FlaskViscousMixProperty : UpgradeProperty
{
    public global::Range<float> fuseBonus = new global::Range<float>(0.20f, 0.35f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Fuse / pull arm:",
            fuseBonus,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float bonus = fuseBonus.GetValue(ref rand, upgrade, default(BoostParams));
        behaviour.GrenadeData.pullFuseBonus += bonus;

        // Grenade fuse after last bounce is GunData.reloadDuration.
        if (gear is IWeapon weapon)
            weapon.GunData.reloadDuration *= (1f + bonus);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            behaviour.RestoreFromPrefab();

        if (gear is IWeapon weapon && prefab is IWeapon prefabWeapon)
            weapon.GunData.reloadDuration = prefabWeapon.GunData.reloadDuration;
    }
}

/// <summary>Throw Weight — throw force up; less gravity (Saxonite-ish throw feel).</summary>
[Serializable]
public class FlaskThrowWeightProperty : UpgradeProperty
{
    public global::Range<float> speedBonus = new global::Range<float>(0.15f, 0.30f);
    public global::Range<float> gravityReduction = new global::Range<float>(0.25f, 0.45f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Throw speed:",
            speedBonus,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
        yield return StatData.Create(
            "Throw gravity:",
            gravityReduction,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out _))
            return;

        if (gear is not IWeapon weapon)
            return;

        float speed = 1f + speedBonus.GetValue(ref rand, upgrade, default(BoostParams));
        float grav = Mathf.Max(0.05f, 1f - gravityReduction.GetValue(ref rand, upgrade, default(BoostParams)));

        weapon.GunData.bulletSpeed *= speed;
        weapon.GunData.bulletGravity *= grav;
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (gear is IWeapon weapon && prefab is IWeapon prefabWeapon)
        {
            weapon.GunData.bulletSpeed = prefabWeapon.GunData.bulletSpeed;
            weapon.GunData.bulletGravity = prefabWeapon.GunData.bulletGravity;
        }
    }
}

// =============================================================================
// Phase 3 — Solvent Field
// =============================================================================

/// <summary>Gas Puddle (Epic) — boom leaves a lingering acid puddle.</summary>
[Serializable]
public class FlaskGasPuddleProperty : UpgradeProperty
{
    public global::Range<float> duration = new global::Range<float>(3.5f, 5.5f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Puddle duration:",
            duration,
            upgrade,
            ref rand,
            OverrideType.Add,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float d = duration.GetValue(ref rand, upgrade, default(BoostParams));
        // Unique epic: take max so stacking sources don't double-count oddly.
        behaviour.GrenadeData.puddleDuration = Mathf.Max(behaviour.GrenadeData.puddleDuration, d);
        behaviour.SyncToVanillaAcidGrenade(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }
    }
}

/// <summary>
/// Catalytic Reservoir (Exotic) — serious long field. Supersets Gas Puddle layer.
/// </summary>
[Serializable]
public class FlaskCatalyticReservoirProperty : UpgradeProperty
{
    public global::Range<float> duration = new global::Range<float>(8f, 12f);
    public global::Range<float> radiusBonus = new global::Range<float>(0.10f, 0.25f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Reservoir duration:",
            duration,
            upgrade,
            ref rand,
            OverrideType.Add,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
        yield return StatData.Create(
            "Reservoir size:",
            radiusBonus,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float d = duration.GetValue(ref rand, upgrade, default(BoostParams));
        float r = 1f + radiusBonus.GetValue(ref rand, upgrade, default(BoostParams));

        behaviour.GrenadeData.catalyticReservoir = true;
        behaviour.GrenadeData.reservoirDuration = Mathf.Max(behaviour.GrenadeData.reservoirDuration, d);
        behaviour.GrenadeData.reservoirRadiusMult *= r;

        // Reservoir owns the floor layer — also widen explosion/puddle radius slightly.
        if (gear is IWeapon weapon)
            weapon.GunData.hitForce *= r;

        behaviour.SyncToVanillaAcidGrenade(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }

        if (gear is IWeapon weapon && prefab is IWeapon prefabWeapon)
            weapon.GunData.hitForce = prefabWeapon.GunData.hitForce;
    }
}

/// <summary>Catalytic Seal (Rare) — puddle/reservoir applies more Acid per tick (data for later tick amp).</summary>
[Serializable]
public class FlaskCatalyticSealProperty : UpgradeProperty
{
    public global::Range<float> tickBonus = new global::Range<float>(0.25f, 0.45f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Puddle acid:",
            tickBonus,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float bonus = tickBonus.GetValue(ref rand, upgrade, default(BoostParams));
        float mult = 1f + bonus;
        behaviour.GrenadeData.puddleTickAcidMult *= mult;
        // Vanilla AcidPuddle tick amount is fixed; also push boom acid slightly so Seal reads without custom puddle MB.
        if (gear is IWeapon weapon)
            weapon.GunData.damageEffectAmount *= (1f + bonus * 0.35f);

    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            behaviour.RestoreFromPrefab();

        if (gear is IWeapon weapon && prefab is IWeapon prefabWeapon)
            weapon.GunData.damageEffectAmount = prefabWeapon.GunData.damageEffectAmount;
    }
}

/// <summary>Gas Valves (Rare) — recharge faster while standing in acid puddle (vanilla AcidPuddle flag window).</summary>
[Serializable]
public class FlaskGasValvesProperty : UpgradeProperty
{
    /// <summary>Absolute recharge multiplier while in puddle (vanilla-style ~2.9–4.2).</summary>
    public global::Range<float> rechargeMult = new global::Range<float>(2.9f, 4.2f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Puddle recharge:",
            rechargeMult,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float mult = rechargeMult.GetValue(ref rand, upgrade, default(BoostParams));
        // Stack: take max-ish by adding fractional excess after first.
        if (behaviour.GrenadeData.rechargeMultiplierInAcidPuddle <= 0f)
            behaviour.GrenadeData.rechargeMultiplierInAcidPuddle = mult;
        else
            behaviour.GrenadeData.rechargeMultiplierInAcidPuddle += (mult - 1f) * 0.5f;

        behaviour.GrenadeData.puddleChargesApplied++;
        behaviour.SyncToVanillaAcidGrenade(gear);
        FlaskPlayerHooks.EnsureBound(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }
    }
}

/// <summary>Universal Solvent (Rare) — outgoing Acid damage increased.</summary>
[Serializable]
public class FlaskUniversalSolventProperty : UpgradeProperty
{
    public global::Range<float> acidDamageBonus = new global::Range<float>(0.15f, 0.30f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Acid damage:",
            acidDamageBonus,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float mult = 1f + acidDamageBonus.GetValue(ref rand, upgrade, default(BoostParams));
        behaviour.GrenadeData.acidOutgoingDamageMult *= mult;

        if (gear is GrenadeGear grenade)
        {
            // GrenadeGear.ModifyOutgoingDamage multiplies when effect matches gun damageEffect.
            if (grenade.GenericGrenadeData.outgoingDamageMultiplier <= 0f)
                grenade.GenericGrenadeData.outgoingDamageMultiplier = mult;
            else
                grenade.GenericGrenadeData.outgoingDamageMultiplier *= mult;
        }

        behaviour.SyncToVanillaAcidGrenade(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }

        if (gear is GrenadeGear live && prefab is GrenadeGear prefabGrenade)
            live.GenericGrenadeData.outgoingDamageMultiplier = prefabGrenade.GenericGrenadeData.outgoingDamageMultiplier;
    }
}

/// <summary>Solvent Siphon (Rare) — kills on corroded targets refund small grenade charge.</summary>
[Serializable]
public class FlaskSolventSiphonProperty : UpgradeProperty
{
    public global::Range<float> chargeRefund = new global::Range<float>(0.12f, 0.22f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Corroded kill charge:",
            chargeRefund,
            upgrade,
            ref rand,
            OverrideType.Add,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float refund = chargeRefund.GetValue(ref rand, upgrade, default(BoostParams));
        behaviour.GrenadeData.solventSiphonCharge += refund;
        FlaskPlayerHooks.EnsureBound(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            behaviour.RestoreFromPrefab();
    }
}

// =============================================================================
// Phase 4 — Vacuum Lab
// =============================================================================

/// <summary>Vacuum Tube (Epic) — pull targets in during fuse, then explode.</summary>
[Serializable]
public class FlaskVacuumTubeProperty : UpgradeProperty
{
    public global::Range<float> force = new global::Range<float>(18f, 28f);
    public global::Range<float> radiusMult = new global::Range<float>(1.0f, 1.25f);
    /// <summary>
    /// Fuse after last bounce. REQUIRED for pull — OnFuseActive only runs while fuseDuration > 0.
    /// GrenadeGear sets bullet FuseDuration from GunData.reloadDuration on fire.
    /// </summary>
    public global::Range<float> fuseDuration = new global::Range<float>(1.1f, 1.6f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Pull force:",
            force,
            upgrade,
            ref rand,
            OverrideType.Add,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
        yield return StatData.Create(
            "Pull radius:",
            radiusMult,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
        yield return StatData.Create(
            "Fuse:",
            fuseDuration,
            upgrade,
            ref rand,
            OverrideType.Add,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float f = force.GetValue(ref rand, upgrade, default(BoostParams));
        float r = radiusMult.GetValue(ref rand, upgrade, default(BoostParams));
        float fuse = fuseDuration.GetValue(ref rand, upgrade, default(BoostParams));

        // One pull layer: take max force; radius stacks mildly.
        behaviour.GrenadeData.pullInForce = Mathf.Max(behaviour.GrenadeData.pullInForce, f);
        if (behaviour.GrenadeData.pullInRadius <= 1f)
            behaviour.GrenadeData.pullInRadius = r;
        else
            behaviour.GrenadeData.pullInRadius += (r - 1f) * 0.5f;

        if (gear is IWeapon weapon)
        {
            // Must have fuse > 0 or grenade detonates on impact and never pulls.
            float targetFuse = fuse;
            if (behaviour.GrenadeData.pullFuseBonus > 0f)
                targetFuse *= (1f + behaviour.GrenadeData.pullFuseBonus);
            weapon.GunData.reloadDuration = Mathf.Max(weapon.GunData.reloadDuration, targetFuse);
        }

        behaviour.SyncToVanillaAcidGrenade(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }

        if (gear is IWeapon weapon && prefab is IWeapon prefabWeapon)
            weapon.GunData.reloadDuration = prefabWeapon.GunData.reloadDuration;
    }
}


/// <summary>
/// Event Horizon (Exotic) — extended arming vacuum; collapse prefers corroded targets.
/// Supersets Vacuum Tube pull presentation (one pull layer).
/// </summary>
[Serializable]
public class FlaskEventHorizonProperty : UpgradeProperty
{
    public global::Range<float> force = new global::Range<float>(28f, 40f);
    public global::Range<float> radiusMult = new global::Range<float>(1.15f, 1.45f);
    public global::Range<float> fuseBonus = new global::Range<float>(0.35f, 0.55f);
    public global::Range<float> collapseDamageBonus = new global::Range<float>(0.25f, 0.45f);
    public global::Range<float> collapseAcid = new global::Range<float>(3f, 6f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Pull force:",
            force,
            upgrade,
            ref rand,
            OverrideType.Add,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
        yield return StatData.Create(
            "Pull radius:",
            radiusMult,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
        yield return StatData.Create(
            "Arm time:",
            fuseBonus,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
        yield return StatData.Create(
            "Collapse damage:",
            collapseDamageBonus,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
        yield return StatData.Create(
            "Collapse acid:",
            collapseAcid,
            upgrade,
            ref rand,
            OverrideType.Add,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float f = force.GetValue(ref rand, upgrade, default(BoostParams));
        float r = radiusMult.GetValue(ref rand, upgrade, default(BoostParams));
        float fuse = fuseBonus.GetValue(ref rand, upgrade, default(BoostParams));
        float cDmg = collapseDamageBonus.GetValue(ref rand, upgrade, default(BoostParams));
        float cAcid = collapseAcid.GetValue(ref rand, upgrade, default(BoostParams));

        behaviour.GrenadeData.eventHorizon = true;
        behaviour.GrenadeData.pullInForce = Mathf.Max(behaviour.GrenadeData.pullInForce, f);
        behaviour.GrenadeData.pullInRadius = Mathf.Max(behaviour.GrenadeData.pullInRadius, r);
        behaviour.GrenadeData.collapseDamageMult = Mathf.Max(behaviour.GrenadeData.collapseDamageMult, 1f + cDmg);
        behaviour.GrenadeData.collapseAcidBonus = Mathf.Max(behaviour.GrenadeData.collapseAcidBonus, cAcid);

        if (gear is IWeapon weapon)
        {
            // Ensure a real arm window even if Tube isn't equipped; then extend.
            float baseFuse = Mathf.Max(weapon.GunData.reloadDuration, 1.25f);
            weapon.GunData.reloadDuration = baseFuse * (1f + fuse);
        }

        behaviour.SyncToVanillaAcidGrenade(gear);
    }


    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }

        if (gear is IWeapon weapon && prefab is IWeapon prefabWeapon)
            weapon.GunData.reloadDuration = prefabWeapon.GunData.reloadDuration;
    }
}

/// <summary>
/// Clump Tax (Rare) — enemies in pull radius take bonus detonation/collapse damage;
/// full bonus if corroded / fully corroded. Dead without Vacuum Tube or Event Horizon.
/// </summary>
[Serializable]
public class FlaskClumpTaxProperty : UpgradeProperty
{
    public global::Range<float> bonusDamage = new global::Range<float>(0.20f, 0.40f);
    public global::Range<float> cleanMult = new global::Range<float>(0.30f, 0.40f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create(
            "Clump bonus (corroded):",
            bonusDamage,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
        yield return StatData.Create(
            "Clean target mult:",
            cleanMult,
            upgrade,
            ref rand,
            OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon,
            default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float bonus = bonusDamage.GetValue(ref rand, upgrade, default(BoostParams));
        float clean = cleanMult.GetValue(ref rand, upgrade, default(BoostParams));

        behaviour.GrenadeData.clumpTaxMult += bonus;
        // Keep clean mult as the lowest (harshest) partial if multiple stacks.
        if (behaviour.GrenadeData.clumpTaxCleanMult <= 0f)
            behaviour.GrenadeData.clumpTaxCleanMult = clean;
        else
            behaviour.GrenadeData.clumpTaxCleanMult = Mathf.Min(behaviour.GrenadeData.clumpTaxCleanMult, clean);

        behaviour.SyncToVanillaAcidGrenade(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }
    }
}

// =============================================================================
// Phase 6 — Remaining kit
// =============================================================================

/// <summary>Deteriorate (Epic) — metal targets get Acid AND Rot (never replace Acid).</summary>
[Serializable]
public class FlaskDeteriorateProperty : UpgradeProperty
{
    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return new StatData(
            "Metal dual-status",
            "Acid + Rot",
            OverrideType.Override,
            StatData.LabelType.BeforeWithColon);
    }


    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;
        behaviour.GrenadeData.deteriorateDualStatus = true;
        behaviour.SyncToVanillaAcidGrenade(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }
    }
}

/// <summary>Overclock (Epic) — OC hits recharge flask; lethal OC clamp. No CD tax.</summary>
[Serializable]
public class FlaskOverclockProperty : UpgradeProperty
{
    public global::Range<float> charge = new global::Range<float>(0.35f, 0.55f);
    public global::Range<float> cooldown = new global::Range<float>(8f, 12f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create("OC charge:", charge, upgrade, ref rand, OverrideType.Add,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
        yield return StatData.Create("OC ICD:", cooldown, upgrade, ref rand, OverrideType.Add,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        behaviour.GrenadeData.overclockCharge += charge.GetValue(ref rand, upgrade, default(BoostParams));
        behaviour.GrenadeData.overclockChargeCooldown =
            cooldown.GetValue(ref rand, upgrade, default(BoostParams));
        behaviour.GrenadeData.overclockChargesApplied++;
        behaviour.SyncToVanillaAcidGrenade(gear);

        // Re-run enable so AcidGrenade subscribes OnBeforeTakeDamage for OC.
        try
        {
            if (gear is AcidGrenade acid)
                acid.OnUpgradesEnabled();
        }
        catch
        {
        }
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }
    }
}

/// <summary>Exothermic Reaction (Epic) — fully Shock-sat targets also get Fire.</summary>
[Serializable]
public class FlaskExothermicProperty : UpgradeProperty
{
    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return new StatData(
            "Shock → Fire",
            "On full Shock sat",
            OverrideType.Override,
            StatData.LabelType.BeforeWithColon);
    }


    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;
        behaviour.GrenadeData.electroIgnite = true;
        behaviour.SyncToVanillaAcidGrenade(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }
    }
}

/// <summary>
/// Heavy Support (Exotic) — cargo heavy drop. Cost: reduced boom damage (not CD tax).
/// </summary>
[Serializable]
public class FlaskHeavySupportProperty : UpgradeProperty
{
    public global::Range<float> boomDamagePenalty = new global::Range<float>(0.35f, 0.50f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return new StatData("Heavy drop", "On detonate", OverrideType.Override, StatData.LabelType.BeforeWithColon);

        yield return StatData.Create("Boom damage:", boomDamagePenalty, upgrade, ref rand, OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        float pen = boomDamagePenalty.GetValue(ref rand, upgrade, default(BoostParams));
        behaviour.GrenadeData.heavySupport = true;
        behaviour.GrenadeData.heavyBoomDamageMult *= Mathf.Max(0.2f, 1f - pen);

        if (gear is IWeapon weapon)
            weapon.GunData.damage *= Mathf.Max(0.2f, 1f - pen);

        behaviour.SyncToVanillaAcidGrenade(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }
        if (gear is IWeapon weapon && prefab is IWeapon prefabWeapon)
            weapon.GunData.damage = prefabWeapon.GunData.damage;
    }
}

/// <summary>Heavy Payload (Rare) — polish drop feel; no-op without Heavy Support.</summary>
[Serializable]
public class FlaskHeavyPayloadProperty : UpgradeProperty
{
    public global::Range<float> speedBonus = new global::Range<float>(0.20f, 0.40f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create("Cargo speed:", speedBonus, upgrade, ref rand, OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;

        // Soften boom tax slightly when payload is present with Heavy.
        float bonus = speedBonus.GetValue(ref rand, upgrade, default(BoostParams));
        behaviour.GrenadeData.heavyPayloadSpeedMult *= (1f + bonus);
        if (behaviour.GrenadeData.heavySupport)
            behaviour.GrenadeData.heavyBoomDamageMult = Mathf.Min(1f,
                behaviour.GrenadeData.heavyBoomDamageMult + bonus * 0.15f);

        behaviour.SyncToVanillaAcidGrenade(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }
    }
}

/// <summary>Odd Cocktail (Epic) — chance of a second non-Acid element explosion.</summary>
[Serializable]
public class FlaskOddCocktailProperty : UpgradeProperty
{
    public global::Range<float> chance = new global::Range<float>(0.20f, 0.35f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create("Second boom chance:", chance, upgrade, ref rand, OverrideType.Multiply,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;
        behaviour.GrenadeData.randExplosionChance =
            Mathf.Max(behaviour.GrenadeData.randExplosionChance,
                chance.GetValue(ref rand, upgrade, default(BoostParams)));
        behaviour.SyncToVanillaAcidGrenade(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }
    }
}

/// <summary>Greased Joints (Rare) — players in boom gain movement ability charge.</summary>
[Serializable]
public class FlaskGreasedJointsProperty : UpgradeProperty
{
    public global::Range<float> recharge = new global::Range<float>(0.15f, 0.30f);

    public override IEnumerator<StatData> GetStatData(
        Pigeon.Math.Random rand, IUpgradable gear, UpgradeInstance upgrade)
    {
        yield return StatData.Create("Move ability charge:", recharge, upgrade, ref rand, OverrideType.Add,
            StatData.LabelType.BeforeWithColon, default(BoostParams));
    }

    public override void Apply(IGear gear, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (!CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
            return;
        behaviour.GrenadeData.moveAbilityRecharge +=
            recharge.GetValue(ref rand, upgrade, default(BoostParams));
        behaviour.SyncToVanillaAcidGrenade(gear);
    }

    public override void Remove(IGear gear, IGear prefab, UpgradeInstance upgrade, ref Pigeon.Math.Random rand)
    {
        if (CausticFlaskBehaviour.TryGet(gear, out CausticFlaskBehaviour behaviour))
        {
            behaviour.RestoreFromPrefab();
            behaviour.SyncToVanillaAcidGrenade(gear);
        }
    }
}



