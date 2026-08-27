# Chaingun

SAXON industrial **rotary chaingun** primary for **Mycopunk**.

Hold the trigger and barrels spool into a wall of kinetic rounds. Modest damage per bullet; power is **volume, coverage, and position**.

## Phase 0 / 1 (this build)

- Runtime registration into `Global.AllGear` (clone of vanilla **MiniCannon** / Gunship Cannon)
- Unique gear identity: `chaingun` / id `93100`
- Kinetic projectile swap (no explosive shells)
- Always-on light **spool** (RoF ramp while holding M1, decay on release)
- Soft move penalty at high spool
- **No ADS** on baseline
- **No upgrades** yet (paths ship later)
- Persistence across save load

## Identity

| | |
|---|---|
| Slot | Primary |
| GUID | `sparroh.chaingun` |
| API name | `chaingun` |
| Gear id | `93100` |
| Clone base | `MiniCannon` |
| MycoMod | `IsSandbox` |

## Baseline loop

```
M1 hold → spool climbs → kinetic tracers fill the lane
release → spool decays
R reload
weapon swap = normal (no turret yet)
RMB = unbound
```

Tune numbers in `ChaingunBalance.cs`.

## Architecture

| File | Role |
|---|---|
| `Plugin.cs` | BepInEx entry, boot, persistence |
| `ChaingunBalance.cs` | Single balance sheet |
| `WeaponRegistration.cs` | Clone MiniCannon, GearInfo, stats, projectile swap |
| `ChaingunBehaviour.cs` | Spool runtime + soft move |
| `ChaingunCombatHooks.cs` | Update tick + FireInterval postfix |
| `SpawnGearHooks.cs` | NGO equip remap + stamp |
| `Chaingun-DesignDoc.md` | Full design bible (paths / upgrades later) |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/Chaingun.dll`

## Install

```
BepInEx/plugins/Chaingun.dll
```

## In-game checklist

1. Log shows registered `chaingun` id `93100` from MiniCannon base  
2. Gear select lists **Chaingun**  
3. Equip → full-auto **kinetic** (not boom shells)  
4. First shots slower; hold M1 → RoF climbs ~0.75s  
5. Release → RoF decays ~0.35s  
6. Quit/relaunch with equipped → `Persistence OK`  
7. Vanilla Gunship Cannon untouched  

## Later phases (not in 1.0.0)

- Hellstorm / Auto Turret / Warthog upgrade paths  
- Swap-deploy turret, Conference Call, heat/vent, Sky Lattice  
- HUD spool meter  

See `Chaingun-DesignDoc.md`.

## Authors

- Sparroh

## License

MIT — see `LICENSE`
