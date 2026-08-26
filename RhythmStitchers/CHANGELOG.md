# Changelog

## 0.1.0

- Phase 0: register Rhythm Stitchers (`rhythm_stitchers`, id 93000) by cloning AcceleratorGun
- Persistence across save load (AllGear inject + GearData rebind)
- Spawn remap + identity stamp (NGO-safe)
- Phase 1 baseline:
  - Dual independent semi fire (LMB left / RMB right, no ADS)
  - Independent L|R magazines + shared reserve
  - Dual channel-aware reload
  - Primary HUD `L|R`
  - Shared Tempo (120 BPM) + light on-beat damage crumb (+8%)
  - Accelerator burst / upgrade identity neutered on the clone
- Custom crosshair + pendulum metronome HUD:
  - Center dot replaces vanilla Accelerator brackets while equipped
  - Bottom arc: one needle sweeps left ↔ right (alternate-trigger timing)
  - Sweet zones at the L and R ends — LMB bonus on left tip, RMB on right tip
  - Per-channel success/miss flash on fire
  - On-beat window ±110 ms; on-point damage crumb +25%
  - On-beat damage hooks AcceleratorGun.ModifyBulletData (override never called base)



- No upgrades yet (empty pool for Phase 2+)
