# CyclerRework (Heat Cycler)

A BepInEx mod for **Mycopunk** that adds **Heat Cycler** — a Soft Redline rework of the vanilla Cycler SMG fantasy.

## Features

- **New primary weapon** (does not replace vanilla Cycler)
- **Infinite ammo** — no magazines or reloads
- **Soft Redline heat system**
  - Builds Heat while firing; dissipates after a short delay when you stop
  - At max Heat you **keep firing** (stance: slight RoF/spread brake, damage/element carrot)
  - No base hard lockout
- **Pressure Vent (R)** — spend a chunk of heat for a small pulse; clears redline
- **R upgrades** can replace/augment vent: Capacitor Dump (cone), Energy Convergence, Elemental Discharge
- **Infinity Burn** — overcap past max; self-DoT + outgoing power scales with overcap depth
- **Elemental interlacing** (Fire / Shock / Acid) — Crossflash, Pyrolysis, Acid Spark, Braid Protocol, Tri-Valve, Violent Reaction, etc.
- **43 custom upgrades** isolated from vanilla Cycler
- Configurable heat / zone / vent numbers via BepInEx config

## Defaults (Soft Redline)

| Setting | Default | Intent |
|---|---|---|
| MaxHeat | 100 | Redline ceiling |
| HeatPerShot | 2.4 | ~3.5s continuous fire to redline |
| DissipatePerSecond | 65 | Full cool ~1.5s idle |
| DissipateDelay | 0.15 | Stutter-fire forgiveness |
| HotThreshold | 0.70 | Hot band carrots |
| RedlineFireIntervalMult | 1.18 | ~15% slower RoF at redline |
| RedlineSpreadMult | 1.25 | Wider spray at redline |
| RedlineDamageMult | 1.12 | Meaner at redline |
| VentSpend | 35 | Pressure Vent heat cost |
| VentMinHeat | 15 | Min heat to vent |
| VentRecovery | 0.45 | Vent cooldown |

## Install

**Thunderstore / r2modman** (when published), or manually:

1. Install BepInEx for Mycopunk
2. Place `CyclerRework.dll` in `BepInEx/plugins/`

## In-game

1. Open gear select
2. Equip **Heat Cycler** (auto-unlocked)
3. Hold fire — heat climbs; at 100% HUD shows redline and you keep shooting
4. Tap **R** to Pressure Vent (~35 heat + small pulse)
5. With **Infinity Burn**, heat can climb past 100% (overcap)
6. Build Fire/Shock/Acid grids for interlace payoffs (Tri-Valve, Cyclotron, Crossflash, …)

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/CyclerRework.dll` (or project target framework output path)

## Design

See `Cycler-DesignDoc.txt` for pillars, paths, Soft Redline model, and full upgrade brainstorm.  
See `CHANGELOG.md` for shipped vs first-draft history.

## Notes

- Marked `IsSandbox` (gameplay-affecting content)
- Multiplayer: all clients should run the same mod version
- Primary elements: **Fire, Shock, Acid** (Decay applicator cut by default)

## Authors

- Sparroh

## License

MIT — see `LICENSE`
