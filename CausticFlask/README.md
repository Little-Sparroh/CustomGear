# Caustic Flask

Separate **Acid-element throwable** for Mycopunk. Vanilla Acid Grenade is left unmodified.

**One-liner:** *Throw solvent. Upgrades turn it into a melting floor, a gravity well, or a coat of timed armor — without taxing your cooldown for existing.*

> Status: **0.6.0** — Full ~30 upgrade board (baseline through Phase 6 leftovers/Heavy).  
> See `CausticFlask-DesignDoc.md` for the full design.

## What ships in 0.6.0






- New gear: **Caustic Flask** (`caustic_flask`, gear id `92400`)
- Clones vanilla **Acid Grenade** at runtime (model / NGO / `AcidGrenadeBullet`)
- Baseline: clean corrosive boom (~10 damage, Acid effect amount **~6** — partial saturation)
- **No** free puddle, pull, armor, overshield, or heavy drop at stock
- Auto-unlocked in gear select
- Sandbox mod flag (`ModFlags.IsSandbox`)
- **9 standard upgrades** (stackable): Wide Mouth, Strong Solvent, Quick Cap, Hard Flask, Base Lining, Deep Vat, Twin Flask, Viscous Mix, Throw Weight
- **Debug grant** (`Debug.GrantAllUpgrades`, default on): one unlocked inventory instance of each upgrade on load (place on hex yourself)
- **Equip persistence**: `grenadeID` saved as catalog id so Flask stays equipped after restart
- **Solvent Field:** Gas Puddle, Catalytic Reservoir, Catalytic Seal, Gas Valves, Universal Solvent, Solvent Siphon (Deep Vat scales field duration)
- **Vacuum Lab:** Vacuum Tube, Event Horizon, Clump Tax (Viscous Mix / Throw Weight support)
- **Carapace:** Polymer Plating, Saxonite Carapace, Plate Polish, Puddle Harden, Defensive Spurt (timed DR, not overshield)
- **Phase 6:** Deteriorate, Overclock, Exothermic, Heavy Support/Payload, Odd Cocktail, Greased Joints







## Planned (later phases)

Three gravity wells + logistics exotic:

| Well | Fantasy |
|------|---------|
| **Solvent Field** | Puddles, reservoir, valves, siphon |
| **Vacuum Lab** | Pull, clump tax, Event Horizon collapse |
| **Carapace** | Timed armor (DR), Solvent Cure, Defensive Spurt |
| **Heavy Support** | Cargo drop (off-triad logistics) |

~30 upgrades, 4 equal large exotics. No cooldown-tax culture. Stretch cards (Odd Cocktail, Greased Joints) stay on the board.

## Architecture

| Piece | Role |
|-------|------|
| Clone `AcidGrenade` | NGO spawn base + Acid bullet path |
| `CausticFlaskBehaviour` | Flask-specific `Data` host (upgrades mutate this) |
| Spawn remap | Catalog index → vanilla Acid prefab, then stamp Flask identity |
| Vanilla Acid | **Never patched** |

Live equipped instances are still the `AcidGrenade` NetworkBehaviour type; Flask identity is `GearInfo` + `CausticFlaskBehaviour`. Gimmick fields on `AcidGrenade.Data` are cleared so stock throws stay bland.

## Building

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/CausticFlask.dll`

## Install

```
BepInEx/plugins/CausticFlask.dll
```

## In-game checklist (0.6.0)

1. Log shows full board (`92401–92430`) + grant
2. **Deteriorate** on metal → Acid + Rot
3. **Overclock** under OC damage → charge refund
4. **Exothermic** on full-Shock → Fire
5. **Heavy Support** → crate + weaker boom; Payload alone no-ops
6. **Odd Cocktail** → occasional non-Acid second boom
7. **Greased Joints** → F-ability charge on players in boom
8. Prior wells still work; stock still bland; vanilla Acid unchanged







## Soft synergies (no mod deps)

Shock weapons (future Exothermic), metal-heavy missions (Deteriorate), OC modifiers (Overclock), heavy fans (Heavy Support). Distinct from Honey Jar HoT and Splash Canister shield HP — Flask armor is **timed damage resistance**.

## Authors

- Sparroh

## License

MIT — see `LICENSE`
