# Siege Cannon

SAXON **SC-4 Siege Cannon** — a parallel primary for **Mycopunk** built on Gunship Cannon DNA.

Full-auto **explosive shells**. Vanilla Gunship Cannon stays in the game unchanged.

## Status

| Phase | Content |
|-------|---------|
| **0** | Registration, MiniCannon clone, persistence, equip stamp |
| **1** | Baseline gunfeel (`ScBalance`) — no spool, no upgrades |
| Later | Battery / Halo / Ordnance paths (~30 upgrades) |

See `SiegeCannon-DesignDoc.md` for the full design bible.

## Identity

| Field | Value |
|-------|--------|
| GUID | `sparroh.siegecannon` |
| APIName | `siege_cannon` |
| Gear ID | `94000` |
| Clone | `MiniCannon` (Gunship Cannon) |
| Flags | `IsSandbox` |

## Baseline (Phase 1)

- Damage ~24, full-auto ~240 RPM
- Mag 60 / reserves 180
- Visible shell travel (speed 70, gravity)
- No spin-up / spool
- No ADS (AIM reserved for Fire Mission / Halo command later)
- No path systems until upgrades ship

## Architecture

| File | Role |
|------|------|
| `Plugin.cs` | BepInEx entry, boot timing, persistence |
| `ScBalance.cs` | Single balance sheet (AMR-style) |
| `SiegeCannonBehaviour.cs` | Custom data host on catalog + live instances |
| `WeaponRegistration.cs` | Clone MiniCannon, GearInfo, AllGear, stats |
| `SpawnGearHooks.cs` | NGO equip remap + stamp |
| `UpgradeRegistration.cs` | CreateUpgrade helpers (unused until upgrade phase) |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/SiegeCannon.dll`

## Install

```
BepInEx/plugins/SiegeCannon.dll
```

## In-game checklist

1. Log shows registered `siege_cannon` id `94000`, base `MiniCannon`
2. Gear select lists **Siege Cannon** (auto-unlocked)
3. Equip → Gunship model / explosive shells
4. Full-auto thump, **no spin-up ramp**
5. Vanilla Gunship Cannon still present
6. Quit / relaunch with weapon equipped → Persistence OK

## Authors

- Sparroh

## License

MIT — see `LICENSE`
