# Changelog

## 0.1.0

### Phase 0 — Registration
- Runtime clone of vanilla **TheCarver** as new primary **Blood Carver** (`blood_carver`, id `93010`)
- GearInfo / TextBlocks / AllGear inject / PlayerData persistence
- SpawnGear remap + live stamp (catalog Prefab/Info + `BloodCarverBehaviour`)
- `BloodCarverBalance` sheet (AMR-style) for GunData + saw volume + blood/spend knobs
- Sandbox MycoMod flag

### Phase 1 — Base mechanics (no path upgrades)
- Baseline **Blood** meter always on (not upgrade-gated)
- Blood on **damage instances** (1 per 10 ticks) + anatomy kill bonuses (limb/shell/core)
- 1-stack-at-a-time decay after 7s grace
- No damage falloff inside reach
- Passive +1% damage per blood stack
- RMB / Aim → **Exsanguinate** (spend blood → pulse + short saw buff)
- ADS disabled so Aim is free for spend
- Vanilla Carver left untouched as a separate weapon

### Design
- Design doc v1.1: blood gained on damage instances, not kill-only
