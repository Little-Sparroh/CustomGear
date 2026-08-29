# Overdriver

Custom **primary weapon** for Mycopunk. SAXON OD-2 full-auto shock burster — hold the trigger and the burst grows.

**Product shape:** separate gear — vanilla Accelerator is **not** patched or replaced.

## Phase status

| Phase | Status |
|-------|--------|
| 0 — Registration | Done |
| 1 — Base mechanics (no upgrades) | Done |
| 2+ — Upgrade paths (Cascade / Vector / Payload) | Later |

## What you get (v1.0.0)

- Auto-unlocked primary in gear select: **Overdriver**
- Full-auto shock bursts with vanilla Accel burst growth
- Sprint-fire baseline
- Empty upgrade pool (hex cards come in Phase 2)
- Save/load persistence for unlock, level, and equipped loadout

## Dependencies

- Mycopunk
- [BepInEx](https://github.com/BepInEx/BepInEx) 5.4.2403+ (Thunderstore pack `BepInEx-BepInExPack_Mycopunk`)

## Building

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/Overdriver.dll`

## Installing

```
BepInEx/plugins/Overdriver.dll
```

## In-game checklist

1. BepInEx log: `[Overdriver] Registered gear 'Overdriver' (api=overdriver, id=92300)`
2. Gear select lists **Overdriver** (auto-unlocked)
3. Equip primary → hold M1 full-auto shock bursts; burst size grows while holding
4. Sprint-fire works; no bees/missiles/warp on empty grid
5. Quit / relaunch → loadout and level kept (`Persistence OK` in log)
6. Vanilla Accelerator still present and unchanged

## Architecture notes

- **Base type:** `AcceleratorGun` (burst growth, sprint-fire, shock hose)
- **Catalog clone** gets unique `GearInfo` + `OverdriverBalance` stats
- **NGO spawn** remaps to vanilla AcceleratorGun prefab, then stamps catalog identity
- **Balance:** edit `OverdriverBalance.cs` only (`PreferCloneBaseline` keeps empty-grid damage from Accel)
- **Upgrades:** not registered yet; hex pool is empty on purpose

## Identity

| Field | Value                |
|-------|----------------------|
| GUID | `sparroh.overdriver` |
| Gear ID | `92300`              |
| API name | `overdriver`         |
| Flags | `IsSandbox`          |

## Authors

- Sparroh

## License

MIT — see `LICENSE`
