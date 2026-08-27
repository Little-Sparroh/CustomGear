# Voltaic Cell

Shock-element throwable for **Mycopunk**. On impact a **storm cloud** forms above the detonation point and rains Heaven's Fury–style lightning (damage + Shock) for a short duration. Upgrades (later) unlock live-wire body storms, capacitor overshields, and illegal pocket economics.

> Phase 0–1: registration + baseline storm field. Upgrades come later.

Vanilla **Shock Grenade** (`VoltaicGrenade`) is left unmodified — this is a separate gear entry.

## Baseline (no upgrades)

| Stat | Value |
|------|--------|
| Gun damage (per bolt scale) | 100 × **0.4** strike mult |
| Element | `EffectType.Shock` |
| Effect amount (per bolt scale) | 10 × **0.4** |
| Storm radius | 6 (`hitForce`) |
| Storm duration | **3 s** |
| Strike interval | **0.155 s** (Trident Heaven's Fury) |
| Max charges | 3 |
| Recharge | 45s |
| Live Wire / OS / Pocket | **None** (upgrade-gated) |

Detonation **replaces** the vanilla sphere boom. Bolts use `GameManager.SpawnLightningEffect_Rpc` (same VFX path as Trident smites).

Clone base: vanilla **Voltaic Grenade** (shock visuals / NGO spawn).

## Architecture

| Piece | Role |
|-------|------|
| `VoltaicCellBalance` | All baseline numbers including storm knobs |
| `GrenadeRegistration` | Clone VoltaicGrenade → AllGear + GearData |
| `SpawnGearHooks` | NGO remap + stamp identity / baseline |
| `VoltaicCellBehaviour` | Custom data host (storm + future upgrades) |
| `VoltaicCellDetonateHook` | Prefix: skip boom, spawn storm |
| `VoltaicStormCloud` | Lingering HF-style strike field |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/VoltaicCell.dll`

## Install

```
BepInEx/plugins/VoltaicCell.dll
```

## In-game checklist

1. Log shows registered `voltaic_cell`  
2. Gear select lists **Voltaic Cell** (auto-unlocked)  
3. Equip throwable → throw works  
4. On land: **storm cloud** appears, lightning bolts rain for ~3s (Shock + damage)  
5. **No** single sphere boom  
6. No Live Wire on activate, no magnet / launch  
7. Vanilla Shock Grenade still present and unmodified  
8. Save/reload keeps unlock + equipped grenade  

## Design

See `VoltaicCell-DesignDoc.md` for full kit fantasy, gravity wells, and upgrade table.

## Authors

- Sparroh

## License

MIT — see `LICENSE`
