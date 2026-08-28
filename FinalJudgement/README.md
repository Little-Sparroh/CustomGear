# Final Judgement

SAXON **FJ-1 Final Judgement** — a man-portable strategic charge heavy for Mycopunk.

Hold M1 for ~eight seconds. Authorize one expensive rocket. Classic sphere boom. Magazine 1.

Parallel catalog entry — does **not** replace vanilla The Last Argument.

## Phase status

| Phase | Status |
|-------|--------|
| 0 Registration (HeavyNuke clone, persistence, equip) | Done |
| 1 Baseline gunfeel (8s charge, mag 1, ImpactSphere) | Done |
| Warhead / Hammer / Retribution upgrades | Later |

## Identity

| Field | Value |
|-------|--------|
| GUID | `sparroh.finaljudgement` |
| APIName | `final_judgement` |
| Gear ID | `93100` |
| Slot | Heavy |
| Clone base | `HeavyNuke` |
| MycoMod | IsSandbox |

## Baseline loop

```
Plant / kite → Hold M1 ~8s → full authorization auto-fires
  → fat rocket → classic impact sphere
  → mag empty → reload / manage heavy ammo
```

Early release cancels charge with no ammo spend. RMB unbound on baseline.

## Balance

All base GunData lives in `FjBalance.cs` (AMR-style sheet). Tune there.

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/FinalJudgement.dll`

## Install

```
BepInEx/plugins/FinalJudgement.dll
```

## Authors

- Sparroh

## License

MIT — see `LICENSE`
