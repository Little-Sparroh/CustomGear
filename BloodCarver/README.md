# Blood Carver

Custom **primary** chainsaw for **Mycopunk**. Clones vanilla **The Carver**, then owns a baseline **Blood** resource and **Exsanguinate** spend. Does **not** replace vanilla Carver.

**Phase 0:** registration, spawn, persistence, balance sheet  
**Phase 1:** base combat (blood, decay, no falloff, RMB spend) — no path upgrades yet

## Fantasy

Harvest the body. Spend the red. Feed the cannon (heavy path later).

Hold M1 in the pile → damage ticks mint Blood → part kills mint more → tap Aim/RMB to cash stacks for a pulse and a brief saw frenzy.

## Identity

| Field | Value |
|---|---|
| GUID | `sparroh.bloodcarver` |
| API name | `blood_carver` |
| Gear ID | `93010` |
| Base clone | `TheCarver` |
| Flags | `IsSandbox` |

## Baseline loop

1. **M1** — continuous saw (box cast, Carver DNA)
2. **Blood** — +1 stack every 10 damage instances; limb/shell/core kills +1/+2/+3
3. **Decay** — after 7s without gain, −1 stack every ~0.85s (never full-bar melt)
4. **Passive** — +1% outgoing damage per stack
5. **Aim / RMB** — Exsanguinate if blood ≥ 3: spend up to 5 → forward pulse + ~1s saw buff
6. **R** — reload (mag loop kept)
7. **Falloff** — none inside reach (~5.5m)

## Architecture

| File | Role |
|---|---|
| `Plugin.cs` | Boot, Harmony, register gear |
| `BloodCarverBalance.cs` | Tunable constants |
| `WeaponRegistration.cs` | Clone TheCarver, AllGear, stats apply |
| `SpawnGearHooks.cs` | NGO remap + stamp |
| `BloodCarverBehaviour.cs` | Blood runtime + spend |
| `BloodCarverCombatHooks.cs` | Lifecycle + tick + falloff safety |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/BloodCarver.dll`

## Install

```
BepInEx/plugins/BloodCarver.dll
```

## In-game checklist

1. Log shows registered `blood_carver` / TheCarver base index  
2. Gear select lists **Blood Carver**  
3. Equip → saw works (Carver model/audio)  
4. Hitting enemies builds blood before kills  
5. Stacks on HUD; idle decay is 1-at-a-time  
6. Full damage at max reach  
7. Aim spends blood (no ADS)  
8. Quit/relaunch keeps unlock / loadout id  
9. Vanilla Carver still available  

## Later phases

- Path A/B/C upgrades + crowns  
- Iron Snare (Hold R)  
- Heavy Fuel / Abattoir Link  
- Deflector / Magnet / Viscera  

See `BloodCarver-DesignDoc.md`.

## Authors

- Sparroh

## License

MIT — see `LICENSE`
