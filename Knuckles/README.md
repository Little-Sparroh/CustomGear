# MeleeRework

BepInEx **sandbox** mod for **Mycopunk** that elevates melee into a real loadout slot and ships **Fists** as the default kit.

**One-liner:** *Tap V to jab. Hold V to put 'em up. The slot is the product; Fists are the proof.*

## Current build (0.1.0)

| Feature | Status |
|---------|--------|
| Fists identity (name, unlock, XP, grid shell) | **In** |
| Baseline damage / size / reach buffs | **In** (config) |
| Melee equip slot in gear select | **In** |
| Melee kit persistence | **In** |
| `MeleeKitRegistry` hooks for future kits | **In** (API stub) |
| Tap V vs hold V full equip | Next |
| Full-equip Guard (RMB DR) | Next |
| Frozen 30 Fists upgrades | Next |

## Config (`BepInEx/config/sparroh.meleerework.cfg`)

| Key | Default | Notes |
|-----|---------|--------|
| Enable Mod | true | Master toggle |
| Enable Melee Gear Slot | true | Gear select melee slot |
| Damage Multiplier | 1.45 | vs vanilla 70 |
| Size Multiplier | 1.20 | hit forgiveness |
| Reach Multiplier | 1.15 | still extreme close |
| Cooldown Multiplier | 0.90 | lower = snappier |

Live reload supported (file watcher + debounce).

## Design

Full design: [`MeleeRework-DesignDoc.md`](MeleeRework-DesignDoc.md).

## Future kits

Blood Carver / Saxonite Wrench stay separate projects. When converted to melee:

```csharp
MeleeKitRegistry.RegisterKit(yourMeleeGear);
```

## Building

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/MeleeRework.dll`

GUID: `sparroh.meleerework` · flags: **IsSandbox**.

## Authors

- Sparroh

## License

MIT — see `LICENSE`
