# Changelog

## 0.1.3

- Fix: clicking melee slot no longer IndexOutOfRange — slot is installed at `gearEquipSlots[4]` (matches `player.Gear[4]` and `SelectedEquipSlot`)
- Harden `SortGearList` against null AllGear entries / out-of-range SelectedEquipSlot

## 0.1.2


- Fix: melee equip slot no longer covers hub "OK then" — placed on second equip row beside secondary
- Fix: injected slot is standalone (not appended to gearEquipSlots) so vanilla OnOpen cannot mis-bind it
- Fix: GearSlot.Setup NRE — fallback Icon when MeleeGear has null icon; Setup try/catch; showUpgrades false

## 0.1.1


- Fix: MeleeGear is not in vanilla `Global.AllGear` — locate from `Player.Gear[4]` / `_weapons` and inject into AllGear
- Fix: late-register Fists when player gear spawns and when gear select opens
- Fix: null-safe melee slot bind (no `GearSlot.Setup` NRE when GearData missing)
- Reduce log spam when Fists is not ready yet at OnAwake

## 0.1.0


- Initial MeleeRework implementation (P0 slice)
- **Fists identity**: vanilla `MeleeGear` rebranded (name/type/description, unlock, XP, upgrade grid shell)
- **Baseline combat**: configurable damage / size / reach / cooldown multipliers on Fists
- **Melee gear slot**: injects `GearType.Melee` equip slot into gear selection
- **Persistence**: saves selected melee kit id via PlayerData flag
- **Hooks stub**: `MeleeKitRegistry.RegisterKit` for future melee kits
- Sandbox mod GUID `sparroh.meleerework`
