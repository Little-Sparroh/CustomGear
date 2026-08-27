# Plasma Blaster

A Mycopunk primary weapon mod: low-RPM full-auto plasma bolts that apply **Decay** on contact.

**Phase 0–1 (current):** registration + baseline gunfeel. No upgrades yet.

## Fantasy

Hold the trigger. Paint them with rot. Later upgrades fork Bloom (ray-splash), Fallout (sticky isotope blobs), and Ion (charge lance).

## Baseline (zero upgrades)

| Trait | Value |
|--------|--------|
| Fire mode | Full-auto ~6.25 rps |
| Damage | Mid per bolt + Decay (delivered in drill chunks) |
| Mag / reserve | 24 / 96 |
| Reload | 1.6 s |
| Projectile | Traveling plasma **cylinder** (~1.25 m) that drills until budget spent |
| Splash / hitforce | None |
| RMB / ADS | Unbound / off |
| Hold R laser prompt | Suppressed (Scout HUD chrome hidden) |

Tune numbers in `PlasmaBlasterBalance.cs` (including `Cylinder*` knobs).


## Identity

| Slot | Value |
|------|--------|
| GUID | `sparroh.plasmablaster` |
| APIName | `plasma_blaster` |
| Gear ID | `92400` |
| Clone base | `ScoutLaserRifle` |
| Flags | `ModFlags.IsSandbox` |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/PlasmaBlaster.dll`

## In-game checklist

1. Log shows registered `plasma_blaster` / id 92400 from Scout index  
2. Gear select lists **Plasma Blaster** (auto-unlocked)  
3. Equip → full-auto **cylinders** that fly out and drill on contact  
4. Hits tick Decay/damage until bolt budget empty; no knockback / splash  
5. No permanent **Hold R** laser prompt; RMB does not enter Scout laser  
6. Quit/relaunch keeps loadout (`Persistence OK`)


## Layout

| File | Role |
|------|------|
| `Plugin.cs` | Entry, registration timing, identity |
| `PlasmaBlasterBalance.cs` | Base GunData / laser-disable sheet |
| `WeaponRegistration.cs` | Clone, stats, cylinder bullet assign, AllGear |
| `PlasmaCylinderBullet.cs` | Traveling drill cylinder (`IBullet`) |
| `PlasmaBlasterBehaviour.cs` | Data host for future upgrades |
| `PlasmaBlasterLaserHooks.cs` | Block Scout laser + hide Hold R HUD |
| `SpawnGearHooks.cs` | Equip remap + stamp |
| `UpgradeRegistration.cs` | Helper kept for Phase 2+ |


## License

MIT — see `LICENSE`
