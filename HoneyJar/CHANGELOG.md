# Changelog

## 0.1.0

### Phase 0 — Registration
- Renamed template → **Honey Jar** (`sparroh.honeyjar`, api `honey_jar`, id `92200`)
- Clones vanilla **Incendiary Grenade** for model / NGO spawn
- `HoneyJarBalance` single source of truth (AMR-style const layout)
- Shared family baseline: damage **100**, `EffectType.Bees`, effect amount **10**, charges **3**, recharge **45**, `hitForce` **6**
- Save persistence (`PlayerData.OnAwake` prefix/postfix) + equip stamp / grenadeID rebind
- Incendiary gimmick Data wiped on catalog + live stamp

### Phase 1 — Base mechanics
- Primary boom uses stamped Bees GunData (vanilla `GrenadeBullet.Detonate`)
- Weak aftershock **BeeCloud** (~2s) with light tick damage + bee apply
- Reduced self bee application (`SelfEffectMultiplier = 0.35`)
- No upgrades yet (kit in later phases)
