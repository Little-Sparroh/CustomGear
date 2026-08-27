# Thermite

A BepInEx content mod for **Mycopunk** that adds **Thermite** — a separate Fire-element throwable parallel to the vanilla Incendiary Grenade.

- Runtime registration into `Global.AllGear` (no Unity editor required)
- Reuse of the vanilla **Incendiary Grenade** model / throw / bullet path
- Bland baseline boom; fantasy is upgrade-gated
- Instant base-HP heal identity (no HoT, no overshield) — heal cards land in later phases
- Official `PlayerData.CreateUpgrade` pool targeted at `thermite` (ids 92601–92630)

Vanilla Incendiary Grenade is **left unmodified**.

## Identity

| Field | Value |
|---|---|
| Display name | Thermite |
| APIName | `thermite` |
| Gear ID | `92600` |
| Upgrade IDs | `92601–92630` |
| GUID | `sparroh.thermite` |
| Element | `EffectType.Fire` |
| Flags | `ModFlags.IsSandbox` |

## Architecture

Live equip is still a vanilla `IncendiaryGrenade` NetworkBehaviour (spawn remaps to that NGO prefab). Custom state lives on `ThermiteBehaviour`. Compatible systems are pushed into `IncendiaryGrenade.Data` + flags via `SyncToVanillaIncendiary` after `ApplyUpgrades`.

```
Plugin.Awake
  ├─ Harmony: PlayerData.OnAwake / Global.LoadInstance → register gear
  ├─ SpawnGearHooks → remap catalog index → stamp identity
  └─ PlayerData.AddRegisterUpgradesCallback → ThermiteUpgradeRegistrar

TryRegisterGear
  ├─ Find IncendiaryGrenade in AllGear
  ├─ Catalog clone + GearInfo (thermite / 92600)
  ├─ ThermiteBehaviour + bland Fire baseline
  └─ Clear vanilla Incendiary gimmicks
```

## Current content (v0.6.0)

### Baseline
- Damage ~10, Fire effect amount 10, 1 charge, vanilla recharge
- No free heal/cluster/IC/hearth/scorched/Gambler until upgrades

### Standards (9) · Rares (10) · Epics (5) · Exotics (4) — **28 total**
| ID | Name | Rarity |
|---|---|---|
| 92601–92609 | Wide Bore … Quick Tongs | Standard spine |
| 92610 | Welding Heat | Rare — boom instant heal |
| 92611 | Restoration Protocol | Rare — throw pure HP |
| 92612 | Napalm | Rare |
| 92613 | Heat Sink | Rare |
| 92614 | Volatile Explosives | Rare |
| 92615 | Ember Stride | Rare |
| 92616 | Warm Front | Rare |
| 92617 | Cauterize Jacket | Rare — ignited Welding amp |
| 92618 | Slag Splitter | Rare (Cluster path) |
| 92619 | Funeral Mote | Rare — move-through scorched heal |
| 92620 | Maniac Maneuver | Epic — Wildfire |
| 92622 | Ember Relay | Epic — ignited detonate CDR |
| 92623 | Violent Reaction | Epic — corroded → next radius |
| 92624 | Impact Cascade | Epic — melee ignited charge |
| 92626 | Afterburn Fuse | Epic — fuse + boom Fire/radius |
| 92627 | Internal Combustion | Exotic |
| 92628 | Mobile Hearth | Exotic — move-gated ember CDR |
| 92629 | Cluster Bomb | Exotic |
| 92630 | Scorched Earth | Exotic — lingering fire field |

**Cut:** Give and Take (92621), Hot Boxing (92625).

### Planned
Phase 7 polish / ship packaging.

## Building

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/Thermite.dll`

## Debug

`Debug.GrantAllUpgrades` (default **true**) grants one unlocked inventory instance of each registered upgrade on load. Does not auto-equip the hex grid. Disable before shipping if drops should be earned normally.

## Authors

- Sparroh

## License

MIT — see `LICENSE`
