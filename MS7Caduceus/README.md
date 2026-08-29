# MS-7 Caduceus

SAXON field-medic **tether primary** for Mycopunk.

Hold fire to lock a Caduceus beam. RMB cycles **Mend** (heal ally), **Overclock** (damage amp), and **Judgment** (chip + Condemned). Sustained tethering builds **Grace** for a weak baseline Discharge. Emitter **Heat** forces vent beats. Cannot Mend yourself — doctrine, not defect.

## Phase status

| Phase | Status |
|-------|--------|
| 0 Registration (Shocklance clone, persistence, equip) | Done |
| 1 Baseline tether / polarities / Grace / heat | Done |
| 2+ Upgrades / crowns / HUD | Not yet |

## Controls

| Input | Action |
|-------|--------|
| Hold M1 | Acquire / maintain tether |
| Tap RMB | Cycle polarity Mend → Overclock → Judgment |
| Tap R | **Discharge** if Grace full and heat below threshold; else **Vent** heat |
| Overheat | Beam locks until vented |

## Identity

| Field | Value |
|-------|--------|
| GUID | `sparroh.ms7caduceus` |
| APIName | `ms7_caduceus` |
| Gear ID | `93500` |
| Clone | Shocklance |
| Flag | `IsSandbox` |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/MS7Caduceus.dll`

## Tune

Edit `CaduceusBalance.cs` — single source of truth for GunData + tether/Grace/heat knobs.

## License

MIT — see `LICENSE`
