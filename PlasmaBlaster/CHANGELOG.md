# Changelog

## 0.1.0

### Phase 0 — Registration
- Runtime gear registration into `Global.AllGear` (clone of ScoutLaserRifle)
- Identity: APIName `plasma_blaster`, GearId `92400`, display **Plasma Blaster**
- Persistence across save load (`PlayerData.OnAwake` + catalog id stamp)
- Equip remap via `SpawnGear_Server` + GearSelectionWindow safety

### Phase 1 — Baseline mechanics (no upgrades)
- Full-auto low–mid RPM blaster (`PlasmaBlasterBalance`)
- Vanilla **Decay** on direct hit; **hitForce = 0**; no splash
- Projectile swap to `SimpleProjectileBullet` (readable travel)
- Scout laser dual-mode hard-disabled for this gear only
- ADS off (RMB unbound until Ion path)
- Empty upgrade pool (grid present; no modules yet)
