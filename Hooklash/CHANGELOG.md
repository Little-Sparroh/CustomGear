# Changelog

## 0.1.0

### Phase 0 — Registration
- Product retarget from weapon template → **Hooklash** melee kit
- GUID `sparroh.hooklash`, gear id **93100**, api `hooklash`
- Runtime clone of vanilla `MeleeGear` (Fists DNA) into `Global.AllGear`
- Soft MeleeRework bridge: reflection `MeleeKitRegistry.RegisterKit`
- SpawnGear remap/stamp so NGO spawns base MeleeGear then applies Hooklash identity
- Persistence via GearData + MeleeRework save flag when present

### Phase 1 — Empty-grid combat (H0–H3)
- `HooklashBalance` single balance sheet (AMR-style)
- Lash floor: damage / size / reach / cooldown, no ammo
- 2-hit string: opener crack → heavier/wider finisher (full equip)
- Quick-V: single short crack only; no tether
- RMB context tether (full equip): enemy reel or surface self-reel
- Mild post-reel lash amp (Ole One-Two baseline)
- Setup postfix resists MeleeRework FistsBaseline stomp on our kit
