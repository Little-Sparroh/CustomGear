# Changelog

## 0.1.0

### Phase 0 — Registration
- New primary **Spillway** (`spillway`, id `92800`) cloned from vanilla **Globbler**
- Injected into `Global.AllGear` with isolated upgrade pool (vanilla Globbler untouched)
- Spawn remap: catalog → vanilla Globbler NGO prefab → stamp identity + `ApplyUpgrades`
- Persistence via `PlayerData.OnAwake` prefix/postfix + `EnsureGearData`
- Auto-unlock in gear select; TextBlocks display name

### Phase 1 — Base mechanics
- `SpillwayBalance` sheet — absolute damage **100**, acid effect amount **10**

- Mag 7 / reserve 63, Acid element, cooker/siphon/flood fields zeroed
- `SpillwayBehaviour` host with Globblometer → damage infrastructure (empty grid = 1×)
- **Ziggs-Q projectile:** forward + up arc hops (not angle Reflect), heavier gravity, `maxBounces = 2`
- **Explode on every impact** with per-hop damage share (`1 / (bounces+1)`); direct enemy hit = full damage once
- `SpillwayProjectileHooks` — Spillway-only Bounce / GlobblerBullet.OnHit (vanilla Globbler untouched)
- No upgrade pool yet (Phase 2+)

