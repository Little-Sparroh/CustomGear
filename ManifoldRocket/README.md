# Manifold Rocket

SAXON **MR-6 Manifold Rocket** — a deliberate dumbfire rocket primary for **Mycopunk**.

Detonations deal **ImpactSpike** + **ShrapnelRays**. No damage spheres.

## Phase 0 / 1 (this build)

| Feature | Status |
|---------|--------|
| Gear registration (Globbler clone) | Yes |
| Persistence / equip stamp | Yes |
| Semi dumbfire, mag 5 | Yes |
| ImpactSpike + ShrapnelRays | Yes |
| Light rocket jump | Yes |
| Upgrades / paths | Later |

## Install

```
BepInEx/plugins/ManifoldRocket.dll
```

Requires BepInEx Pack for Mycopunk. Marked **IsSandbox** (all clients need the mod).

## Tuning

Edit `MrBalance.cs` and rebuild — single source of truth for GunData + manifold params.

## Identity

| Field | Value |
|-------|--------|
| GUID | `sparroh.manifoldrocket` |
| APIName | `manifold_rocket` |
| Gear ID | `93400` |

| Base clone | `Globbler` |

## Authors

- Sparroh

## License

MIT — see `LICENSE`
