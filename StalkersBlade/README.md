# Stalker's Blade

BepInEx **sandbox** mod for **Mycopunk**. Registers **Stalker's Blade** as a `GearType.Melee` kit: dual-slash floor stats, crouch/slide/flank **Ambush**, mild full-HP **opener**, and RMB **throw** with Mark + blade-out retrieve.

**One-liner:** *Stay low. Cut once. Don't be there when they look.*

## Soft dependency

| Mod | Role |
|-----|------|
| **MeleeRework** (`sparroh.meleerework`) | Melee loadout slot + kit list (`MeleeKitRegistry.RegisterKit`). Soft/reflection — this mod still loads without it. |

Without MeleeRework: gear injects into `AllGear` and logs a warning; you will not get the melee equip UI.

## Phase 0 / 1 (this build)

| Feature | Status |
|---------|--------|
| Register kit (`stalkers_blade`, id 92900) | **In** |
| Clone vanilla `MeleeGear` + NGO remap/stamp | **In** |
| Soft MeleeRework `RegisterKit` | **In** |
| Balance floor (`StalkersBladeBalance`) | **In** |
| Ambush (crouch / slide window / flank / first strike) | **In** |
| Opener (≥95% HP) | **In** |
| RMB throw + Mark + blade-out retrieve | **In** |
| Upgrades / Ghost / Railblade / Warrant / Twin Sheath | Later |

## Empty-grid combat

- **M1 / V melee:** discrete slash (MeleeGear hitcast), high ST damage, tight size, short reach, no ammo.
- **Crouch:** Low Profile — hits always qualify for Ambush (~1.3×).
- **Slide / post-slide buffer:** Ambush window.
- **Flank / clean first strike:** additional Ambush qualifiers.
- **Full-HP opener:** mild mult, stacks with Ambush.
- **RMB (blades equipped only):** throw knife → Mark target, retrieve on hit; blade-out weakens M1 until retrieve (hit / Ambush kill / miss timer).

Tune numbers in `StalkersBladeBalance.cs`.

## Building

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/StalkersBlade.dll`

GUID: `sparroh.stalkersblade` · flags: **IsSandbox**.

## Design

Full design: [`StalkersBlade-DesignDoc.md`](StalkersBlade-DesignDoc.md).

## Authors

- Sparroh

## License

MIT — see `LICENSE`
