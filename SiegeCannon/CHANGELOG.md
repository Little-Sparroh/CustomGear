# Changelog

## 1.0.0

- Phase 0: Register **Siege Cannon** as a parallel primary (`siege_cannon`, id `94000`)
- Clone vanilla **MiniCannon** (Gunship Cannon) for model / NGO / explosive shell DNA
- Isolated upgrade pool (vanilla Gunship pool untouched)
- Persistence across save load (`PlayerData.OnAwake` prefix + `EnsureGearData`)
- Equip remap: spawn MiniCannon network prefab, stamp catalog identity + `SiegeCannonBehaviour`
- Phase 1: `ScBalance` baseline (dmg 24, ~240 RPM, mag 60 / reserve 180, shell travel)
- No baseline spool (`enableSpinUp = false`); AIM off (reserved for later paths)
- No upgrade cards yet (Battery / Halo / Ordnance come in later phases)
