# Zephyr

SAXON industrial **sonic overpressure cannon** for Mycopunk.

Point at the swarm. Pull the trigger. The air does the rest.

## Phase 0 / 1 (this build)

- New primary weapon **Zephyr** (`zephyr`, gear id `93700`)
- Clones vanilla **TheCarver** for model / NGO spawn validity
- Instant semi **cone pressure blast** (damage + knockback, scarce mag)
- No path upgrades yet (Overpressure / Singularity / Resonance come later)

## Install

```
BepInEx/plugins/Zephyr.dll
```

Requires BepInEx Pack for Mycopunk. **Sandbox** mod — all clients need the same plugin in multiplayer.

## In-game checklist

1. BepInEx log shows `Zephyr v0.1.0 loaded` and gear registration
2. Gear select lists **Zephyr** (auto-unlocked)
3. Equip primary → M1 fires one blast per trigger (~0.65s cycle)
4. Mag 3; centerline hits hard; edges shove survivors
5. Quit / relaunch keeps equip + level

## Tuning

All baseline numbers live in `ZephyrBalance.cs` (AMR-style sheet).

## Authors

- Sparroh

## License

MIT — see `LICENSE`
