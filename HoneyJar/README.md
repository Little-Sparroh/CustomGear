# Honey Jar

Bee-element throwable for **Mycopunk**. Throw a jar of bees — primary boom applies swarm status, then a weak lingering cloud keeps working the field.

> Phase 0–1: registration + baseline boom/cloud. Upgrades (hives, cloaks, nectar) come later.

## Baseline (no upgrades)

| Stat | Value |
|------|--------|
| Damage | 100 |
| Element | `EffectType.Bees` |
| Effect amount | 10 |
| Explosion radius (`hitForce`) | 6 |
| Max charges | 3 |
| Recharge | 45s |
| Aftershock cloud | ~2s weak bee ticks |

Clone base: vanilla **Incendiary Grenade** (visuals until custom art).

## Architecture

| Piece | Role |
|-------|------|
| `HoneyJarBalance` | All baseline numbers (tune here) |
| `GrenadeRegistration` | Clone Incendiary → AllGear + GearData |
| `SpawnGearHooks` | NGO remap + stamp identity / baseline |
| `HoneyJarBehaviour` | Custom data host (upgrades later) |
| `HoneyJarDetonateHook` | Postfix boom → spawn `BeeCloud` |
| `BeeCloud` | Local field linger ticks |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/HoneyJar.dll`

## Install

```
BepInEx/plugins/HoneyJar.dll
```

## In-game checklist

1. Log shows registered `honey_jar` id `92200` with Bees baseline  
2. Gear select lists **Honey Jar** (auto-unlocked)  
3. Equip throwable → throw works  
4. Boom applies **Bees** (not Fire)  
5. Amber placeholder cloud lingers ~2s with light ticks  
6. Save/reload keeps unlock + equipped grenade  

## Design

See `HoneyJar-DesignDoc.md` for full kit fantasy, gravity wells, and upgrade table.

## Authors

- Sparroh

## License

MIT — see `LICENSE`
