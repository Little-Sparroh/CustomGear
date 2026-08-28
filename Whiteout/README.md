# Whiteout

SAXON W-9 **Whiteout** — continuous cryo projector primary for Mycopunk.

Hold fire to hose a frost cone. Secondary fires a mag-fed rime cell from the same magazine. Reload stays reload.

## Phase status

| Phase | Status |
|-------|--------|
| 0 — Registration (BounceShotgun clone, balance, equip/persist) | **Done** |
| 1 — Baseline hose + lob (zero upgrades) | **Done** |
| 2+ — Gale / Rime Cell / Shatter upgrade pool | Not yet |

## Controls (baseline)

| Input | Action |
|-------|--------|
| Hold M1 | Cryo cone hose (raycast fan) + continuous mag drain |
| RMB | Lob cryo cell (mag tax) |
| R | Reload only |

## Identity

- GUID: `sparroh.whiteout`
- APIName: `whiteout`
- Gear id: `87700` (primary band; not 928–931xx melee/junk blocks)
- Base clone: Jackrabbit (`BounceShotgun`)
- MycoMod: `IsSandbox`

Does **not** replace or patch vanilla Jackrabbit / Pesticide.

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/Whiteout.dll`

## Balance

Tune `WhiteoutBalance.cs` — single source of truth for GunData + hose/lob constants.
