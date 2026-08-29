# Hooklash

SAXON issue coil-whip melee kit for **Mycopunk**.

**Phase 0–1:** registration + empty-grid combat (no upgrades yet).

- **Slot:** Melee (`GearType.Melee`) via MeleeRework kit list
- **M1:** 2-hit lash string (full equip) / short crack (quick-V)
- **RMB:** Context tether — enemy yank or surface self-reel
- **No ammo / no reload**

Soft-depends on **MeleeRework** (`sparroh.meleerework` / Knuckles) for the melee loadout slot UI. The kit still registers into `AllGear` without it.

## Requirements

- Mycopunk
- BepInEx 5.4.2403+
- Recommended: [MeleeRework](https://thunderstore.io) (Sparroh)

## Install

```
BepInEx/plugins/Hooklash.dll
```

## In-game checklist

1. Log: `[Hooklash] Registered 'Hooklash' (api=hooklash, id=93100)` and MeleeRework yes/no  
2. Melee slot lists **Hooklash** (with MeleeRework)  
3. Equip → tap V short lash with gun out  
4. Hold V → full equip; M1 opener then heavier finisher  
5. RMB on enemy → yank in; RMB on wall/floor → reel self  
6. After reel, next lash feels slightly stronger  
7. Quit/relaunch → GearData / kit selection persists  

## Balance

All empty-grid numbers live in `HooklashBalance.cs` (AMR-style single sheet).

| Floor | Draft |
|-------|-------|
| Damage | 82 |
| Size | 0.52 |
| Reach | 3.0 m |
| Cooldown | 0.32 s |
| Cast range | 14 m |
| Post-reel amp | 1.12× / 1.25 s |

## Architecture

| File | Role |
|------|------|
| `Plugin.cs` | BepInEx entry, timing, MeleeRework soft-dep |
| `WeaponRegistration.cs` | Clone MeleeGear, AllGear, GearData |
| `MeleeReworkBridge.cs` | Reflection RegisterKit / SaveFromGear |
| `SpawnGearHooks.cs` | NGO remap + identity stamp |
| `HooklashBalance.cs` | Empty-grid constants |
| `HooklashBehaviour.cs` | String / tether / amp state |
| `HooklashCombatHooks.cs` | Setup restamp + 2-hit FireBullet |
| `HooklashTetherHooks.cs` | RMB cast + reel tick |

## IDs

| | |
|--|--|
| GUID | `sparroh.hooklash` |
| Gear ID | `93100` |
| APIName | `hooklash` |

Upgrade ids **93101–93130** reserved for later phases (design doc frozen 30).

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/Hooklash.dll`

## License

MIT — see `LICENSE`
