# Changelog

## 1.0.0

- Phase 0: Register **Boarding Trident** as a new primary (clone of WideGun / Trident S2)
- Unique gear id `91200`, API name `boarding_trident`, sandbox mod flag
- Persistence across save load (AllGear inject + GearData rebind)
- Equip spawn remap: NGO spawns vanilla WideGun, then stamps catalog identity
- Phase 1: Base combat without upgrades
  - Full-auto **5-prong** rifle, mag 75 / reserve 450 (Trident-like ballpark)
  - **Flipped axes:** hip = horizontal rake; **RMB rotates** barrel + crosshair to vertical stake
  - **No ADS zoom** — AimFOV 0; rotation-only stance (same hip damage/spread/range)
  - Custom **5-dot rake crosshair** (hides vanilla); rotates with barrel on RMB
  - No ADS FOV zoom (AimFOV 0 + OnStartAim block on live WideGun stamp)

  - Muzzle flash BarScale matches the active combat axis
  - `BoardingTridentBalance` single balance sheet (AMR-style)
- Vanilla Trident S2 left unchanged
- Upgrade pool empty (Phase 2+)

