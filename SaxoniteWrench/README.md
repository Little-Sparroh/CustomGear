# Saxonite Wrench

SAXON gravitic impact wrench for **Mycopunk** — a **melee kit** (`GearType.Melee`), not a primary gun.

**One-liner:** *Wind the core. Crack the floor. Pull them into the head.*

## Requires

- BepInEx Pack for Mycopunk
- **[MeleeRework](https://thunderstore.io)** (`sparroh.meleerework`) — soft dependency for the melee loadout slot UI and kit list. Without it the gear still registers into `AllGear`, but equipping via the melee slot needs MeleeRework.

## Phase 0 / 1 (this build)

| Feature | Status |
|---------|--------|
| Register as Melee kit (clone MeleeGear) | Yes |
| Appear in MeleeRework melee slot | Yes (soft) |
| Tap smash / charged slam | Yes |
| Perfect Torque sweet spot | Yes |
| Shockwave on impact | Yes |
| RMB gravity pull | Yes (full equip) |
| No ammo / no reload | Yes |
| Upgrade grid (~30 cards) | Later |
| Exotics (Aftershock, Event Horizon, Absolute Zero, Sledge Jump) | Later |

## Controls (with MeleeRework)

| Input | Effect |
|-------|--------|
| **Tap V** | Quick smash (gun stays out) |
| **Hold V** | Equip wrench |
| **M1** (equipped) | Tap smash, or hold to charge slam |
| **RMB** (equipped) | Gravity pull pulse |
| **R** | Unused on baseline |

## Identity

| Field | Value |
|-------|--------|
| GUID | `sparroh.saxonitewrench` |
| APIName | `saxonite_wrench` |
| Gear ID | `92800` |
| Slot | Melee (`player.Gear[4]`) |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/SaxoniteWrench.dll`

## Design

See `SaxoniteWrench-DesignDoc.md` for fantasy, wells, and the full upgrade table (implementation phased).
