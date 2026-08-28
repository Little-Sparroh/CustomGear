# Changelog

## 0.1.0

### Phase 0 — Registration
- Runtime clone of vanilla **TheCarver** as new primary **Zephyr** (`zephyr`, id `93700`)
- GearInfo / TextBlocks / AllGear inject / PlayerData persistence
- SpawnGear remap + live stamp (catalog Prefab/Info + `ZephyrWeaponBehaviour`)
- `ZephyrBalance` sheet (AMR-style) for GunData + cone blast knobs
- Sandbox MycoMod flag
- Vanilla Carver left untouched as a separate weapon

### Phase 1 — Base mechanics (no path upgrades)
- Instant **semi** pressure blast (replaces Carver continuous saw `FireBullet`)
- Forward **cone** volume sample with distance + angular falloff
- High **hitForce** knockback via `EnemyBrain.AddImpulseForce_Client` (bosses resist; allies skipped)
- Mag **3** / reserve **15** / fire interval **0.65s** / reload **~1.8s**
- ADS disabled (RMB free for later path overrides)
- Vanilla Carver blood/rage/shackle toys zeroed on baseline
