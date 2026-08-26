# Aussie Special

Break-action hopping shotgun primary for **Mycopunk**. Twin triggers, shared mag of two, shells that wake up after a bounce.

**Does not replace** vanilla Au-Si Jackrabbit / BounceShotgun.

## Phase 0 / 1 (current)

| Feature | Status |
|---|---|
| Register as new primary | Yes |
| Clone BounceShotgun visuals / bullets | Yes |
| Independent chambers 1\|1 + reserve 72 | Yes |
| HUD primary ammo as `L\|R` | Yes |
| Semi, 6 pellets, 1 bounce | Yes |
| No ADS | Yes |
| Dual-trigger LMB + RMB (independent cadence) | Yes |
| Pre-bounce ×0.90 (ours only) | Yes |

| Upgrades / Seeking / Blightflame / slug | Later |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/AussieSpecial.dll`

## Install

```
BepInEx/plugins/AussieSpecial.dll
```

## Identity

| Field | Value |
|---|---|
| GUID | `sparroh.aussiespecial` |
| APIName | `aussie_special` |
| Gear ID | `87530` |
| Flags | `ModFlags.IsSandbox` |

## Balance

Edit `AussieSpecialBalance.cs` — single sheet for base GunData / aim / pre-bounce mult.

## Authors

- Sparroh

## License

MIT — see `LICENSE`
