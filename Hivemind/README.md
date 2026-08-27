# Hive Launcher

Custom **primary weapon** for Mycopunk. A living pellet swarm that plants a hovering cloud and dives on release.

**Product shape:** separate gear — vanilla Swarm Launcher is **not** patched or replaced.

## Phase status

| Phase | Status |
|-------|--------|
| 0 — Registration | Done |
| 1 — Base mechanics (no upgrades) | Done |
| 2+ — Upgrade paths (Apiary / Swarmfront / Second Skin / Hive) | Later |

## What you get (v1.0.0)

- Auto-unlocked primary in gear select: **Hive Launcher**
- Full-auto plant → hover cloud → release dive (vanilla SwarmGun feel)
- Mag **36** / reserve **288**, ~857 RPM, 2 pellets/shot
- Slight empty-grid damage raise vs vanilla Swarm (34 vs 32)
- Save/load persistence for unlock, level, and equipped loadout

## Dependencies

- Mycopunk
- [BepInEx](https://github.com/BepInEx/BepInEx) 5.4.2403+ (Thunderstore pack `BepInEx-BepInExPack_Mycopunk`)

## Building

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/HiveLauncher.dll`

## Installing

```
BepInEx/plugins/HiveLauncher.dll
```

## In-game checklist

1. BepInEx log: `[HiveLauncher] Registered gear 'Hive Launcher' (api=hive_launcher, id=91900)`
2. Gear select lists **Hive Launcher** (auto-unlocked)
3. Equip primary → hold M1 plants hover pellets; release dives
4. Reload fills tube-style from reserve
5. Quit / relaunch → loadout and level kept (`Persistence OK` in log)
6. Vanilla Swarm Launcher still present and unchanged

## Architecture notes

- **Base type:** `SwarmGun` (required — `SwarmBullet` casts parent to `SwarmGun`)
- **Catalog clone** gets unique `GearInfo` + `HiveLauncherBalance` stats
- **NGO spawn** remaps to vanilla SwarmGun prefab, then stamps catalog identity
- **Balance:** edit `HiveLauncherBalance.cs` only
- **Upgrades:** not registered yet; hex pool is empty on purpose

## Identity

| Field | Value |
|-------|--------|
| GUID | `sparroh.hivelauncher` |
| Gear ID | `91900` |
| API name | `hive_launcher` |
| Flags | `IsSandbox` |

## Authors

- Sparroh

## License

MIT — see `LICENSE`
