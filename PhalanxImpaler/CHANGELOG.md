# Changelog

## 0.1.0

- Initial Phalanx Impaler empty-grid release (design P0–P3)
- Registers `phalanx_impaler` (id 93000) as `GearType.Melee` by cloning vanilla MeleeGear
- Soft MeleeRework bridge (`MeleeKitRegistry.RegisterKit` + melee kit persistence)
- NGO equip remap/stamp (catalog clone → base MeleeGear prefab)
- Balance floor in `PhalanxImpalerBalance` (longest-kit reach, medium volume)
- 3-hit thrust string with finisher damage/size/recovery
- RMB frontal buckler guard + Perfect Brace window + M1-from-guard bash
- R javelin throw + soft pin + shaft-out retrieve (hit / kill / miss timer)
- No ammo / no reload on kit; gun-out R stays reload
