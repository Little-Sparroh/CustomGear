using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

/// <summary>
/// Live-reload config for MeleeRework baseline + future input/guard knobs.
/// </summary>
public static class ConfigManager
{
    private const float DebounceSeconds = 0.25f;

    private static ConfigFile config;
    private static ManualLogSource logger;
    private static FileSystemWatcher configWatcher;
    private static volatile bool reloadPending;
    private static float lastReloadTime;

    public static ConfigEntry<bool> EnableMod { get; private set; }
    public static ConfigEntry<float> DamageMultiplier { get; private set; }
    public static ConfigEntry<float> SizeMultiplier { get; private set; }
    public static ConfigEntry<float> ReachMultiplier { get; private set; }
    public static ConfigEntry<float> CooldownMultiplier { get; private set; }
    public static ConfigEntry<bool> EnableMeleeGearSlot { get; private set; }

    public static void Initialize(ConfigFile configFile, ManualLogSource log)
    {
        config = configFile;
        logger = log;

        EnableMod = config.Bind(
            "General",
            "Enable Mod",
            true,
            "Master toggle. When false, vanilla melee behaviour is left alone.");

        EnableMeleeGearSlot = config.Bind(
            "General",
            "Enable Melee Gear Slot",
            true,
            "Show a Melee equip slot in gear selection (GearType.Melee).");

        DamageMultiplier = config.Bind(
            "Fists Baseline",
            "Damage Multiplier",
            1.45f,
            "Multiplies vanilla MeleeGear damage (70 → ~101 at 1.45). Target: trash in 2–3 hits.");

        SizeMultiplier = config.Bind(
            "Fists Baseline",
            "Size Multiplier",
            1.2f,
            "Multiplies bulletMagnetismTarget (hit forgiveness / multi-target slap).");

        ReachMultiplier = config.Bind(
            "Fists Baseline",
            "Reach Multiplier",
            1.15f,
            "Multiplies maxDamageRange / falloff distances. Still extreme close.");

        CooldownMultiplier = config.Bind(
            "Fists Baseline",
            "Cooldown Multiplier",
            0.9f,
            "Multiplies melee rechargeDuration. Lower = snappier cadence. 1.0 = vanilla.");

        try
        {
            SetupFileWatcher();
        }
        catch (Exception ex)
        {
            logger.LogError($"Error setting up config file watcher: {ex.Message}");
        }
    }

    public static void Tick()
    {
        if (!reloadPending)
            return;

        if (Time.unscaledTime - lastReloadTime < DebounceSeconds)
            return;

        reloadPending = false;
        lastReloadTime = Time.unscaledTime;

        try
        {
            config.Reload();
            logger.LogInfo("Config reloaded from disk.");
            FistsBaseline.MarkDirty();
        }
        catch (Exception ex)
        {
            logger.LogError($"Error reloading config: {ex.Message}");
        }
    }

    public static void Dispose()
    {
        if (configWatcher != null)
        {
            configWatcher.EnableRaisingEvents = false;
            configWatcher.Changed -= OnConfigFileChanged;
            configWatcher.Created -= OnConfigFileChanged;
            configWatcher.Renamed -= OnConfigFileChanged;
            configWatcher.Dispose();
            configWatcher = null;
        }
    }

    private static void SetupFileWatcher()
    {
        configWatcher = new FileSystemWatcher(Paths.ConfigPath, $"{MeleeReworkPlugin.PluginGUID}.cfg");
        configWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName;
        configWatcher.Changed += OnConfigFileChanged;
        configWatcher.Created += OnConfigFileChanged;
        configWatcher.Renamed += OnConfigFileChanged;
        configWatcher.EnableRaisingEvents = true;
    }

    private static void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        reloadPending = true;
    }
}
