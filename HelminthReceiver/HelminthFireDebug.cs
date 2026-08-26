using System;
using UnityEngine;

/// <summary>
/// Temporary fire-path diagnostics for Hardened Stock / no-bullet investigation.
/// Filter BepInEx log for: [Helminth][FireDbg]
/// Set Enabled = false (or remove file) once fixed.
/// </summary>
internal static class HelminthFireDebug
{
    /// <summary>Master switch — leave false in shipping builds; flip true to diagnose fire path.</summary>
    public static bool Enabled = false;


    private const float FireLogCooldown = 0.35f;
    private static float _nextFireLog;
    private static int _fireBulletLogsLeft = 8;
    private static int _fireLogsLeft = 12;

    public static void Log(string msg)
    {
        if (!Enabled)
            return;
        SparrohPlugin.Logger?.LogWarning($"[Helminth][FireDbg] {msg}");
    }

    public static void LogGunSnapshot(string tag, Gun gun, HelminthBehaviour b)
    {
        if (!Enabled || gun == null)
            return;

        try
        {
            ref GunData gd = ref gun.GunData;
            string v = b != null
                ? $"V={b.vitality:0.##}/{b.WeaponData.maxVitality:0.##} Vcost={b.WeaponData.vitalityPerShot:0.##} " +
                  $"rMult={b.WeaponData.recoilMult:0.###} sMult={b.WeaponData.spreadMult:0.###}"
                : "b=null";

            Log(
                $"{tag} | {v} | " +
                $"ammo={gun.RemainingAmmo:0.##} useAmmo={gd.useAmmoOnFire} mag={gd.magazineSize} " +
                $"interval={gd.fireInterval:0.####} auto={gd.automatic} bps={gd.bulletsPerShot} burst={gd.burstSize} " +
                $"spread=({gd.spreadData.spreadSize.x:0.####},{gd.spreadData.spreadSize.y:0.####}) type={gd.spreadData.spreadType} " +
                $"recoilX=({gd.recoilData.recoilX.x:0.####},{gd.recoilData.recoilX.y:0.####}) " +
                $"recoilY=({gd.recoilData.recoilY.x:0.####},{gd.recoilData.recoilY.y:0.####}) " +
                $"recoilZ=({gd.recoilData.recoilZ.x:0.####},{gd.recoilData.recoilZ.y:0.####}) " +
                $"maxRZ={gd.recoilData.maxRecoilZ:0.####} tZ=({gd.recoilData.translateZ.x:0.####},{gd.recoilData.translateZ.y:0.####}) " +
                $"maxTZ={gd.recoilData.maxTranslateZ:0.####} " +
                $"dmg={gd.damage:0.##} limitedAmmo={gd.hasLimitedAmmo} lastFire={gun.LastFireTime:0.###} t={Time.time:0.###}");
        }
        catch (Exception ex)
        {
            Log($"{tag} SNAPSHOT FAIL: {ex.GetType().Name}: {ex.Message}");
        }
    }

    public static bool ShouldLogFire()
    {
        if (!Enabled || _fireLogsLeft <= 0)
            return false;
        if (Time.unscaledTime < _nextFireLog)
            return false;
        _nextFireLog = Time.unscaledTime + FireLogCooldown;
        _fireLogsLeft--;
        return true;
    }

    public static bool ShouldLogFireBullet()
    {
        if (!Enabled || _fireBulletLogsLeft <= 0)
            return false;
        _fireBulletLogsLeft--;
        return true;
    }

    public static void ResetShotBudget()
    {
        // Call if you want more logs mid-session (optional).
        _fireLogsLeft = 12;
        _fireBulletLogsLeft = 8;
        _nextFireLog = 0f;
    }
}
