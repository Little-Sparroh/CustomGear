# Changelog

## 1.0.0

- Phase 0: Register Needle Carbine (`needle_carbine`, id 87530) by cloning ScoutLaserRifle
- Phase 0: `NcBalance` sheet + `ApplyNeedleCarbineStats`; ADS/laser disabled (RMB free for Extract)
- Phase 0: Persistence via Global.LoadInstance + PlayerData.OnAwake; SpawnGear remap/stamp
- Phase 1: Needle stacks (threshold 7, grace 3s) + supercombine burst + poison dump
- Phase 1: True `EffectType` Poison = 11 (`PoisonStatusEffect`, Acid-pattern DoT, 5.5s full-sat)
- Phase 1: Baseline Extract on aim press (consume poison/needles → small heal)
- MycoMod `IsSandbox`; no upgrade pool yet
