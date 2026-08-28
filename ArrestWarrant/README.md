# Arrest Warrant

SAXON **AW-6 Arrest Warrant** — a short-range acid authorization heavy for Mycopunk.

Reload **notarizes** a temporary **Warrant** that makes your primary, melee, and grenades meaner. Flash the badge, swap out, and let the rest of the kit finish the arrest.

Parallel catalog entry — does **not** replace or patch vanilla G6 Street Sweeper.

## Phase 0 / 1 (this build)

| Feature | Status |
|---------|--------|
| Registration + HeavyShotgun clone | Yes |
| Mag 1 acid multi-pellet baseline | Yes |
| Warrant on reload (+20% / 4s other gear) | Yes |
| Upgrade paths (License / Flush / Brace) | Later |
| Hold R / RMB path claims | Later |

### Baseline loop
1. Equip Arrest Warrant (heavy slot)
2. M1 — short-range acid stamp
3. R — reload notarizes Warrant
4. Swap to primary / melee / grenade — dump while badged
5. Re-dip when the window fades

## Identity

| Field | Value |
|-------|--------|
| GUID | `sparroh.arrestwarrant` |
| APIName | `arrest_warrant` |
| Gear ID | `93000` |
| Slot | Heavy |
| Clone base | `HeavyShotgun` (G6 Street Sweeper visuals) |

## Balance

Tune `AwBalance.cs` — single source of truth for GunData + Warrant constants (same style as Anti-Material Rifle).

## Building

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/ArrestWarrant.dll`

## Install

```
BepInEx/plugins/ArrestWarrant.dll
```

Requires BepInEx Pack for Mycopunk. Marked **IsSandbox** (all clients need the mod in multiplayer).

**Optional:** [HeavySelectionExpansion](https://thunderstore.io) — client UI fixes (ScrollArea null-guard, heavy crate list cleanup). Soft dependency; Arrest Warrant still remaps crate/world spawn on its own.

## Authors

- Sparroh

## License

MIT — see `LICENSE`
