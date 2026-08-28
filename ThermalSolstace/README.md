# Thermal Solstice

SAXON **TS-7 Thermal Solstice** — a parallel heavy weapon for **Mycopunk**. Continuous thermal siege beam with a soft Heat channel and strong native Fire. Does **not** replace vanilla Laser Cannon.

## Phase 0 / 1 (current)

| Feature | Status |
|---------|--------|
| Register as Heavy (`thermal_solstice`, id `93600`) | Yes |
| Clone vanilla `HeavyLaser` for model / NGO / beam | Yes |
| Baseline GunData from `TsBalance` | Yes |
| Soft Heat build/decay (no shutdown) | Yes |
| Soft Peak damage crumb | Yes |
| Mild move penalty while firing | Yes |
| RMB / ADS unbound | Yes |
| Upgrade pool (Reactor / Conflagration / Optics) | Later |

## Identity

| Field | Value |
|-------|--------|
| GUID | `sparroh.thermalsolstice` |
| API name | `thermal_solstice` |
| Gear id | `93600` |
| Display | Thermal Solstice |
| Base clone | `HeavyLaser` |
| MycoMod | IsSandbox |

## Architecture

| File | Role |
|------|------|
| `Plugin.cs` | Boot, persistence hooks, identity |
| `TsBalance.cs` | Single balance sheet (AmrBalance style) |
| `WeaponRegistration.cs` | Clone HeavyLaser, apply stats, sanitize `LaserData` |
| `ThermalSolsticeBehaviour.cs` | Soft Heat + fire move penalty |
| `ThermalSolsticeCombatHooks.cs` | Re-assert stats; Peak juice on continuous damage |
| `SpawnGearHooks.cs` | Equip remap + stamp |
| `UpgradeRegistration.cs` | Helpers reserved for later phases |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/ThermalSolstice.dll`

## In-game checklist

1. Log shows registered `thermal_solstice` from `HeavyLaser`
2. Gear select lists **Thermal Solstice** (heavy, auto-unlocked)
3. Equip → hold M1 continuous Fire beam
4. Heat climbs while firing; decays after short grace; no shutdown at Peak
5. Vanilla Laser Cannon still equippable
6. Quit / relaunch keeps unlock and loadout

## Design

See `ThermalSolstice-DesignDoc.md` for full fantasy, paths, and frozen upgrade list.

## Authors

- Sparroh

## License

MIT — see `LICENSE`
