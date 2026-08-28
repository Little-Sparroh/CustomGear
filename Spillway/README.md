# Spillway

A BepInEx custom **primary weapon** for **Mycopunk**.

**Spillway** is a parallel Globbler-class acid grenade hose. Vanilla Globbler is left unmodified.

> Phase 0/1 (v0.1.0): registration + raised empty-grid baseline.  
> Crowns (Cooker / Storm / Recipe / Flood) and the full upgrade pool ship in later phases.  
> See `Spillway-DesignDoc.md` for the full design.

## What you get now

- Equip **Spillway** from gear select (auto-unlocked)
- Lob **acid** globs in heavy **forward arcs** (Ziggs Q spirit) — hop, hop, land
- **Explodes on every impact** (per-hop damage tax so a full chain ≈ one primary shot)
- Direct enemy contact = full damage once and stop
- Baseline **damage 100**, acid **effect amount 10**

- Mag **7** / reserve **63**
- No Pressure Cooker / Siphon / Flood on the base kit


## Architecture

| Type | Role |
|---|---|
| Clone base | Vanilla **Globbler** (NGO spawn + `GlobblerBullet` / HUD path) |
| Catalog | Runtime clone with unique `GearInfo` (`spillway` / `92800`) |
| `SpillwayBalance` | Single balance sheet for base `GunData` |
| `SpillwayBehaviour` | Custom data host (meter + future crowns) |
| `SpawnGearHooks` | Remap equip to Globbler prefab, stamp identity |

### Registration flow

```
Plugin.Awake
  ├─ Harmony: Global.LoadInstance → TryRegisterGear
  ├─ Harmony: PlayerData.OnAwake prefix/postfix (persistence)
  ├─ Harmony: SpawnGear_Server + GearSelectionWindow
  ├─ Harmony: Gun.ModifyBulletData (meter → damage, Spillway only)
  └─ Harmony: Bounce + GlobblerBullet.OnHit (arc hop + multi-pop, Spillway only)


TryRegisterGear
  ├─ Find Globbler in AllGear
  ├─ Instantiate disabled catalog clone
  ├─ GearInfo + TextBlocks + SpillwayBehaviour
  ├─ ApplySpillwayStats + zero Globbler crown fields
  └─ Inject AllGear (vanilla Globbler stays)
```

## Balance

Tune `SpillwayBalance.cs`:

| Lever | Default intent |
|-------|----------------|
| Damage | **100** absolute (`DamageMultiplier` unused while Damage > 0) |
| Effect amount | **10** |
| Element | Acid |
| Mag / reserve | 7 / 63 |
| Gravity | Globbler baseline × 1.85 (steeper arcs) |
| Bounces | 2 surface hops → 3 impacts |
| Bounce hop | Forward + up (not Reflect); speed decay 0.72 |
| Bounce pop damage | Auto `1/(maxBounces+1)` per surface impact |
| Charge / cooker | Off |


Set `SpillwayBalance.Damage` to a positive absolute to override the multiplier lock.

## Building

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/Spillway.dll`

## Install

```
BepInEx/plugins/Spillway.dll
```

Requires BepInEx Pack for Mycopunk (`5.4.2403`+). Marked `[MycoMod(..., IsSandbox)]`.

## In-game checklist

1. Log: `[Spillway] Registered gear 'Spillway' (api=spillway, id=92800)`
2. Gear select lists **Spillway** and still lists **Globbler**
3. Equip → acid globs hop forward in arcs and pop on each impact

4. Empty grid feels stronger than stock Globbler
5. No cooker charge bar / siphon UI on base kit
6. Quit → relaunch keeps unlock / equipped loadout

## Project layout

| File | Purpose |
|---|---|
| `Plugin.cs` | Entry, persistence, combat hook |
| `WeaponRegistration.cs` | Globbler clone + `ApplySpillwayStats` |
| `SpawnGearHooks.cs` | Equip remap + stamp |
| `SpillwayBalance.cs` | Base GunData sheet |
| `SpillwayBehaviour.cs` | Behaviour host |
| `SpillwayProjectileHooks.cs` | Arc bounce + multi-pop detonate |
| `UpgradeRegistration.cs` | CreateUpgrade helpers (unused until Phase 2) |
| `Spillway-DesignDoc.md` | Full design |


## Authors

- Sparroh

## License

MIT — see `LICENSE`
