# Changelog

## 0.2.0

### Path-wall baseline (P0–P1)

- **Base clone** switched from Incendiary Grenade → **Photon Disc** (motion donor)
- Stock throw: Disc tumble → long surface wave path
- Each wave step paints a lingering **WaterWallSegment** ribbon (~2s)
- Enemies inside live segments become **Wet** (Water status, ICD)
- Disc kit stripped: no attunement, ammo-toss, bounce-chain, health chunks, gun-as-disc
- Throwable economy kept: **3 charges / 45s**
- Primer numbers: step damage **5**, Water amount **~2.5**, wave length **15**, `hitForce` **2.5**
- Hook retargeted to `PhotonDiscBullet.Detonate` (Disc overrides base grenade Detonate)
- WetSlick no longer baseline (file retained for later)
- Gear id **94200**, version **0.2.0**

## 0.1.0

### Phase 0 — Registration
- Renamed template → **Splash Canister** (`sparroh.splashcanister`, api `splash_canister`)
- Cloned vanilla **Incendiary Grenade** for model / NGO spawn
- `SplashCanisterBalance` single source of truth
- Shared family baseline boom + Water
- Save persistence + equip stamp / grenadeID rebind

### Phase 1 — Base mechanics (superseded by 0.2.0 path-wall)
- Primary boom + weak aftershock **WetSlick**
- No upgrades yet
