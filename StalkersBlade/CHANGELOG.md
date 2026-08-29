# Changelog

## 0.1.0

### Phase 0 — Registration
- Product retarget from weapon template → **Stalker's Blade** melee kit
- GUID `sparroh.stalkersblade`, gear id **92900**, api `stalkers_blade`
- Runtime clone of vanilla `MeleeGear` (Fists DNA) into `Global.AllGear`
- Soft MeleeRework bridge: reflection `MeleeKitRegistry.RegisterKit`
- SpawnGear remap/stamp so NGO spawns base MeleeGear then applies blade identity
- Persistence via GearData + MeleeRework save flag when present

### Phase 1 — Empty-grid combat (K0–K2)
- `StalkersBladeBalance` single balance sheet (AMR-style)
- Dual-slash floor: damage / size / reach / cooldown
- Ambush: crouch, slide window, flank cone, clean first strike
- Opener: ≥95% HP mild mult
- RMB throw while blades equipped: Mark, blade-out profile, retrieve on hit/kill/timer
- Setup postfix resists MeleeRework FistsBaseline stomp on our kit
