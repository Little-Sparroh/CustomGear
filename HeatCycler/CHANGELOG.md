# Changelog

## 1.1.0 — Soft Redline + Interlace

### Core heat (v2 Soft Redline)
- **No base hard lockout** — at max Heat you keep firing (Soft Redline stance)
- Redline brake (RoF/spread) + carrot (damage/element)
- **Pressure Vent (R)** — spend heat for a small pulse; clears redline
- R router priority: Capacitor Dump → Energy Convergence → Elemental Discharge → Pressure Vent
- **Infinity Burn** — overcap past max; self-DoT + outgoing power scales with overcap depth
- **Capacitor Dump** — spend heat as a narrowing muzzle cone
- Config: MaxHeat, HeatPerShot, Dissipate*, zone/redline mults, vent spend/recovery

### Upgrade semantics (heat verbs)
- Mag/reload ports renamed and retargeted (Thermal Buffer, Hot Load, Quick Sink, Dense Core, Adrenaline Vent, Equipment Radiator, etc.)
- **Decay Energy** cut by default (Fire / Shock / Acid only)
- Toxin Bank clears only when heat hits 0
- Condensed Ejection: plasma projectiles + in-flight lightning arcs

### Crowns & interlace
- **Closed Loop** (Exotic) — sustained Soft Redline auto micro-vents + brief HeatPerShot efficiency
- **Crossflash** — Shock-saturate ignited → heat refund + Fire splash
- **Pyrolysis** — Ignite corroded → lite detonation + Bank stack
- **Tri-Valve** — on hit, `ApplyStatusEffect` Fire/Shock/Acid (no damage re-entry)
- **Acid Spark** — Shock-saturate corroded → spend 1 Bank → Shock arc nearby
- **Braid Protocol** — all 3 primaries within window → brief HeatPerShot efficiency
- **Saturate Catalyst** — saturates stack; next Pressure Vent is stronger

### Cycle Phasing (v2 mode table)
- Coolant / Pyre / Storm / Solvent / Split / Needle / Bleed-Off / Spike
- Locked for entire spray hold from aim yaw octant
- Feedback: log + buff VFX (full HUD label later)

### Catalog
- **43** Heat Cycler upgrades registered (unique ids 92020+)
- Isolated from vanilla Cycler pool


### Technical
- Subsystems: `HeatZone`, `HeatStatLayers`, `HeatVentSystem`, `HeatInterlace`
- GearData gate before CreateUpgrade (NRE fix)
- Soft Redline hard cut (no dual-path lockout config)

## 1.0.0

- Initial release: Heat Cycler custom primary
- First-draft heat + upgrade ports (hard lockout model — superseded by 1.1.0)
