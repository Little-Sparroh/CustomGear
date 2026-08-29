# Phalanx Impaler

BepInEx **sandbox** mod for **Mycopunk**. Registers **Phalanx Impaler** as a `GearType.Melee` kit: long-reach thrust string, frontal **buckler**, and **R** javelin pin with shaft-out retrieve.

**One-liner:** *Longest poke in the kit. Raise the plate. Throw the shaft. Retrieve and do it again.*

## Soft dependency

| Mod | Role |
|-----|------|
| **MeleeRework** (`sparroh.meleerework`) | Melee loadout slot + kit list (`MeleeKitRegistry.RegisterKit`). Soft/reflection — this mod still loads without it. |

Without MeleeRework: gear injects into `AllGear` and logs a warning; you will not get the melee equip UI.

## Phase 0–3 (this build)

| Feature | Status |
|---------|--------|
| Register kit (`phalanx_impaler`, id 93000) | **In** |
| Clone vanilla `MeleeGear` + NGO remap/stamp | **In** |
| Soft MeleeRework `RegisterKit` | **In** |
| Balance floor (`PhalanxImpalerBalance`) | **In** |
| Long quick-V poke + full-equip 3-hit string / finisher | **In** |
| RMB frontal buckler + Perfect Brace + M1 bash | **In** |
| R javelin + pin + shaft-out retrieve | **In** |
| Upgrades / Lunge / Aegis / Discarius / Creed | Later |

## Empty-grid combat

- **Tap V / quick melee:** long poke (longest kit reach), gun stays out — no throw/guard.
- **Hold V / full equip:** pike + buckler fantasy (vanilla melee model placeholder).
- **M1 string:** thrust → thrust → **finisher** (wider, stronger, longer recovery).
- **Hold RMB:** frontal plate DR (~25%) + mild move slow; rear/flank not covered.
- **M1 while guarding:** Shield Bash (Perfect Brace empowers).
- **R (equipped only):** javelin throw → soft pin; shaft-out weakens M1 until retrieve (hit / melee kill / ~3s timer).
- **No ammo. No reload.** Gun-out R remains reload.

Tune numbers in `PhalanxImpalerBalance.cs`.

## Building

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/PhalanxImpaler.dll`

GUID: `sparroh.phalanximpaler` · flags: **IsSandbox**.

## Design

Full design: [`PhalanxImpaler-DesignDoc.md`](PhalanxImpaler-DesignDoc.md).

## Authors

- Sparroh

## License

MIT — see `LICENSE`
