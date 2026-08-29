# Hard-Light Constructor

SAXON HLC-9 hard-light plasma projector primary for **Mycopunk**.

**Phase 0/1 (v0.1.0):** registration + baseline combat.

- Clones vanilla **PlateLauncher** chassis (NGO spawn)
- Full-auto mid-range plasma slabs (`HlcBalance`)
- True **Shatter** EffectType (slot 12) → full saturation **Jam** (no DoT)
- Terrain hits leave brief **micro scorch** (non-walkable)
- RMB unbound (no ADS / no plate recall)
- No upgrade pool yet

## Identity

| | |
|---|---|
| GUID | `sparroh.hardlightconstructor` |
| Gear ID | `93200` |
| API name | `hard_light_constructor` |
| MycoMod | `IsSandbox` |
| Base clone | `PlateLauncher` |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/HardLightConstructor.dll`

## In-game checklist

1. Log shows gear registration + Shatter inject  
2. Gear select lists **Hard-Light Constructor**  
3. Equip primary → full-auto fire (not plate stick/recall)  
4. Hits build Shatter → Jam lock (no DoT ticks)  
5. Terrain hits flicker micro scorch  
6. RMB does nothing  
7. Quit/relaunch keeps unlock/level  

## Architecture

| File | Role |
|---|---|
| `Plugin.cs` | Boot, identity, persistence hooks |
| `HlcBalance.cs` | Base GunData sheet |
| `HlcShatter.cs` | EffectType 12 + Jam status |
| `HlcPlateSuppress.cs` | Kill plate recall/ammo/cast paths |
| `HardLightConstructorBehaviour.cs` | Runtime host + scorch |
| `HardLightConstructorCombatHooks.cs` | Tick / upgrades / scorch OnHit |
| `WeaponRegistration.cs` | Clone + stats + projectile swap |
| `SpawnGearHooks.cs` | Equip remap + stamp |

## License

MIT — see `LICENSE`
