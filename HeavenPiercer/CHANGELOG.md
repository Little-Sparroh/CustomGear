# Changelog

## 0.1.0

- Phase 0: Register Heaven Piercer as a new primary (clone Shocklance, GearInfo `heaven_piercer` / id 87600)
- Phase 1: Base compound-bow gunfeel
  - Hold M1 charge, release loose (`fireOnRelease` + `canFireWhileCharging`)
  - Charge scales damage, bullet speed, gravity, and falloff range
  - Baseline sweet-spot crit band **0.80–0.90** (fixed, not random)
  - Draw HUD: APMP-style progress bar with fixed sweet band + LOOSE/NICE feedback
  - Magazine **1** (single nocked arrow; reload = re-nock); reserves 40
  - Projectile arrows via SimpleProjectileBullet swap (bypasses Shocklance hitscan spiral)
  - Arrow spawn offset off camera/muzzle so projectiles no longer clip in first-person view
  - Mild move-speed penalty while drawing
- `HpBalance` single balance sheet (AMR-style)
- Persistence + SpawnGear remap/stamp
- No upgrades yet (empty pool; grid present)

