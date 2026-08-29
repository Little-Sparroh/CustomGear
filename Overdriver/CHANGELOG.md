# Changelog

## 1.0.0

- Phase 0: register Overdriver as parallel primary (`api=overdriver`, id `92400`)
- Clone vanilla `AcceleratorGun` for model / NGO spawn / burst growth / sprint-fire
- Isolated GearInfo + empty upgrade pool (vanilla Accelerator untouched)
- Phase 1: `OverdriverBalance` + `ApplyOverdriverStats` — full-auto baseline, shock element, growth caps; upgrade-driven Accel fields zeroed
- Persistence via `PlayerData.OnAwake` prefix/postfix + `EnsureGearData`
- Spawn remap: catalog → AcceleratorGun prefab → stamp `OverdriverBehaviour`
