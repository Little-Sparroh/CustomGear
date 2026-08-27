# Junk Flinger

A BepInEx **sandbox** primary weapon for **Mycopunk**.

Parallel gear — **vanilla Lead Flinger is left unmodified**.

## Fantasy

Six chambers. Casings and scrap. Pack the wheel — or (later) drink the junk and dump the cylinder dry.

## Phase 0+1 (this build)

| System | Status |
|--------|--------|
| Gear registration + persistence | Done |
| Clone Lead Flinger (`FastReloadShotgun`) | Done |
| Strip baseline kill→reload | Done |
| Per-chamber cylinder state | Done |
| Junk mint (casings + scrap) | Done |
| Scrap Pack hold-R | Done |
| Junk HUD (secondary) | Done |
| %-only damage pipeline host | Done |
| Upgrade pool (~30) | Partial — Phase 2 Chamber (7) |
| Crowns (Doobie, LB, Phantom, Blood-Rush, Residue, …) | LB + DMH + supports in; Junk/Rush later |


### Empty-grid loop

```
Click M1 → slug from current chamber → casings mint Junk
Kills → scrap mint Junk
Hold R (Junk ≥ 3) → arm Scrap Pack
Reload → next 6 chambers are Packed (+% damage / size)
```

### Identity

| Field | Value |
|-------|--------|
| GUID | `sparroh.junkflinger` |
| APIName | `junk_flinger` |
| Gear ID | `93000` (930xx block reserved for this mod) |

| MycoMod | `IsSandbox` |
| Base type | `FastReloadShotgun` |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/JunkFlinger.dll`

## Install

```
BepInEx/plugins/JunkFlinger.dll
```

## Design

See `JunkFlinger-DesignDoc.txt` (v2) for full bible, frozen 30, and later phases.

### Locked spend rules (for later phases)

- **Blood-Rush** steals **RMB** when free / only used for aim (not hold-R)
- **Scrap Pack** remains baseline hold-R; Residue hold-R still open vs empower path
- **Doobie + Blood-Rush**: temporary magazine overflow
- **Phantom adjacency**: any edge touch (same as existing cell-touch upgrades)

## Authors

- Sparroh

## License

MIT — see `LICENSE`
