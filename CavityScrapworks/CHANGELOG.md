# Changelog

## 0.1.0

### Phase 0 — Registration
- Parallel primary **Cavity Scrapworks** (`cavity_scrapworks`, gear id `91400`)
- Runtime clone of vanilla **PlateLauncher** (not SMG)
- Isolated empty upgrade pool (vanilla Plate Launcher untouched)
- Spawn remap: NGO spawns PlateLauncher base, stamps catalog identity
- Persistence via `PlayerData.OnAwake` prefix/postfix + `EnsureGearData`
- `[MycoMod(IsSandbox)]`, GUID `sparroh.cavityscrapworks`

### Phase 1 — Baseline mechanics
- `CsBalance` single balance sheet (AMR-style)
- Empty-grid stick → recall → catch via vanilla plate DNA
- Mag 1, Normal element, design-spirit damage / RoF / range / recoil
- Thin `CavityScrapworksBehaviour` host for future path data
- No upgrades shipped yet
