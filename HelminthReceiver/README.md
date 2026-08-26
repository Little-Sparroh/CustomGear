# Helminth Receiver

SAXON Type-L bio-ordnance primary for **Mycopunk**.

Mid-rate organic pulse rifle that fires on **Vitality** instead of ammo. Light leech on hit. Hold reload to **Feed** Host health into the weapon.

> Feed it. Direct it. Survive the relationship.

## Status

**v0.2.0 — Phase 2 Standards + glue** (sacred loop + first upgrade wave).

| Feature | Status |
|---------|--------|
| Gear registration + persistence | Done |
| Vitality fire economy | Done |
| Hold-reload Feed + safety floor | Done |
| Passive drip | Done |
| Innate leech + Bond | Done |
| Vitality HUD | Done |
| Standards + glue upgrades (8) | Done |
| Full ~30 upgrade pool | Phase 3+ |


## Controls

| Input | Effect |
|-------|--------|
| Fire | Spend Vitality per pulse; soft-lock when empty |
| Hold Reload | Feed channel — convert Host HP → Vitality (stops at 18% HP floor) |
| Release Reload | Cancel Feed |

## Install

1. BepInEx 5.4.2403+ for Mycopunk  
2. [SparrohUILib](https://thunderstore.io) (soft dep — HUD)  
3. Drop `HelminthReceiver.dll` into `BepInEx/plugins/`

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/HelminthReceiver.dll`

## Design

See `HelminthReceiver-DesignDoc.txt` for the full fantasy, economy rules, and ~30 upgrade catalog.

## Authors

- Sparroh

## License

MIT — see `LICENSE`
