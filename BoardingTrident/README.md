# Boarding Trident

A BepInEx content mod for **Mycopunk** that adds a new primary weapon:

**Boarding Trident** — pirate boarding multi-prong rifle. Hipfire sweeps a **horizontal** five-prong rake across the deck; hold **RMB** to rotate the barrel and drop a **vertical** stake. No ADS zoom — rotation only. Parallel to vanilla Trident S2 / WideGun (does not replace it).

Phase 0/1 ships registration + base gunfeel. Upgrades (Doctrine, Cutlass, Screw, Tide) come later.


## Identity

| | |
|---|---|
| GUID | `sparroh.boardingtrident` |
| Gear API | `boarding_trident` |
| Gear ID | `91200` |

| Base clone | `WideGun` (Trident S2 model / fire setup) |
| Flags | `IsSandbox` |

## Base combat (no upgrades)

| Trait | Value |
|---|---|
| Fire mode | Full-auto projectile |
| Bullets per shot | 5 |
| Hip rake | **Horizontal** |
| RMB hold | Barrel + crosshair rotate to **Vertical** (no FOV zoom) |
| Damage | 15 per pellet (Trident ballpark) |
| Mag / reserve | 75 / 450 |
| Reload | 1.6s |
| Elements | None innate |

**Note vs vanilla Trident:** vanilla is hip vertical / ADS horizontal with zoom. Boarding Trident flips the axes and uses RMB for rotation only (no ADS presentation).


## Architecture

| File | Role |
|---|---|
| `Plugin.cs` | BepInEx entry, registration timing, persistence |
| `BoardingTridentBalance.cs` | Single balance sheet (AMR-style) |
| `BoardingTridentBehaviour.cs` | Custom data host + `GetCombatAxis()` |
| `BoardingTridentCombatHooks.cs` | Harmony: flipped spread, offsets, barrel, muzzle |
| `WeaponRegistration.cs` | Clone WideGun, GearInfo, AllGear, ApplyStats |
| `SpawnGearHooks.cs` | Equip remap + stamp + GearSelectionWindow safety |
| `UpgradeRegistration.cs` | CreateUpgrade helpers (unused until upgrade phase) |

### Registration flow

```
Plugin.Awake
  ├─ Harmony: Global.LoadInstance → TryRegisterGear
  ├─ Harmony: PlayerData.OnAwake prefix/postfix
  ├─ Harmony: SpawnGear_Server + GearSelectionWindow
  └─ BoardingTridentCombatHooks (WideGun axis flip)

TryRegisterGear
  ├─ Find WideGun in AllGear
  ├─ Catalog clone + GearInfo (empty upgrade pool)
  ├─ BoardingTridentBehaviour + ApplyBoardingTridentStats
  └─ Inject AllGear / EnsureGearData

Equip
  ├─ Remap catalog index → WideGun NGO prefab
  ├─ Stamp Prefab/Info + behaviour
  └─ ApplyUpgrades + re-apply stats if needed
```

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/BoardingTrident.dll`

## Install

```
<Mycopunk>/BepInEx/plugins/BoardingTrident.dll
```

Or via r2modman / Thunderstore when published.

## In-game checklist

1. Log shows registered `boarding_trident` id 91200  

2. Gear select lists **Boarding Trident** (auto-unlocked)  
3. Equip primary → WideGun model, 5-prong auto fire, mag reload  
4. Hip rake is **horizontal**; hold RMB rotates barrel + crosshair to **vertical** (no zoom)  

5. Vanilla Trident still present and unchanged  
6. Quit / relaunch keeps unlock and equip (`Persistence OK` in log)  
7. No upgrade cards yet (grid may stay empty until first CreateUpgrade)

## Design

See `BoardingTrident-DesignDoc.md` for full fantasy, upgrade paths, and later phases.

## Authors

- Sparroh

## License

MIT — see `LICENSE`
