# Changelog

## 0.1.0

### Phase 0 — Registration
- Renamed template → **Voltaic Cell** (`sparroh.voltaiccell`, api `voltaic_cell`, id `94400`)
- Clones vanilla **Voltaic Grenade** (Shock) for model / NGO spawn
- `VoltaicCellBalance` single source of truth (AMR-style const layout)
- Shared family baseline stats: damage **100**, `EffectType.Shock`, effect amount **10**, charges **3**, recharge **45**, `hitForce` **6**
- Save persistence (`PlayerData.OnAwake` prefix/postfix) + equip stamp / grenadeID rebind
- Vanilla Voltaic gimmick Data + upgrade flags wiped on catalog + live stamp
- Vanilla Shock Grenade left unmodified

### Phase 1 — Storm field baseline
- **Replaces** standard sphere boom with a Heaven's Fury–style storm cloud
- On detonate: spawn `VoltaicStormCloud` above impact (placeholder cloud VFX)
- Bolts every **0.155s** for **3s** in boom radius — damage + Shock per strike
- VFX via `GameManager.SpawnLightningEffect_Rpc` (Trident smite path)
- Strike damage/shock scaled from gun data (`StormStrikeDamageMult` / `StormStrikeShockMult`)
- Reduced self shock on bolts (`SelfEffectMultiplier`)
- No upgrades yet (kit in later phases)
