# DMLR Rework — Marksman Laser Rifle

Custom primary for **Mycopunk**. A dual-mode marksman rifle that reads enemy anatomy and routes damage through it.

**Tag the body. Cut the limb. Dump the pain into the heart.**

Registered as its own gear slot — the vanilla **Scout / DMLR** is unchanged.

## Core gunfeel

| Feature | Behavior |
|---|---|
| Identity | `dmlr_rework` / **Marksman Laser Rifle** |
| Base | Runtime clone of ScoutLaserRifle (model, laser, HUD, sounds) |
| DMR | Fully automatic, high accuracy |
| ADS | Disabled |
| Laser | Hold **aim (RMB)** to beam; release or empty charge → DMR |
| Priority | Both held → laser (default). With **Hot Swap** → DMR |
| Charge | ~20 DMR hits to full laser (mag-sized loop) |
| Reload | **R** reloads only — never mode-toggles |

Baseline is a clean auto-DMR + hold-laser charge loop. Anatomy fantasy is opt-in through the upgrade grid.

## Severance

Upgrades speak a shared vocabulary. The gun does not enable it for free.

| Concept | What it does |
|---|---|
| **Mark** | Short, refreshable tag on the part you hit — hooks arcs, echo, charge, and transfer bonuses |
| **Transfer** | Fraction of damage dealt to a part is also applied inward (core / shell). Laser is the default execute tool |
| **Expose** | Temporary weakpoint on a core after shell breaks or brand completion — bonus damage and stronger on-hit payoffs |
| **Sever** | Limb / shell / core kills are first-class events for overkill, pulses, and refunds |

Transfer prefers living shells and damageable cores — not limbs — and drills one outermost shell at a time before going deeper.

## Build paths

Mix freely. Paths are identity, not hard locks.

### Dissection — limb → core
Strip extremities and dump pain inward.

- **Neural Feedback** — limb damage transfers % to core (laser ≫ DMR)
- **Overkill Conduit** — limb-kill overkill multiplies and transfers inward
- **Arterial Shred** — +limb / −shell; Open Artery bonus laser transfer
- **Joint Breaker** — every 3rd DMR limb hit applies Decay
- **Phantom Pain** — bonus damage + charge refund vs regrown limbs
- **Bleed Charge** — limb hits generate bonus laser charge

### Breach — shell → Expose → execute
Crack armor, open the core, dump the beam.

- **Hard-Light Designator** — shell kills Expose the core; laser on Exposed refunds charge
- **Core Brand** — DMR shell hits stack Brand; max stacks → next laser Exposes
- **Pulverizer** — +shell / −limb; shell hits splash inward
- **Fault Line** — repeated DMR hits on the same shell escalate damage
- **Reactor Tap** — core kills refund charge; Exposed cores also refund ammo
- **Breach Charge** — shell hits generate bonus laser charge
- **Collapse Wave** — shell kill emits a pulse scaled by shell max HP

### Conductor — marks, arcs, clear
Paint packs, laser one body, clean the rest.

- **Sympathetic Arc** — laser on Marked parts arcs to nearby enemies
- **Sympathetic Resonance** — Marked damage echoes to other marks on that brain
- **Voltaic Battery** — reload throws an aim-battery blast with inward transfer
- **Demonstrator's Trick** — mode-switch empowers the next hit (spread Mark / heavy Mark)
- **Triple Feed** — DMR always Shock or Acid (rolled per mag)
- **Rot Thread** — every 3rd DMR shell hit applies Rot
- **Marked Recycling** — laser refunds DMR ammo on Marked or Exposed parts
- **Elemental Emitter** — laser applies rolled Shock or Acid
- **Incendiary Laser** — periodic fire wave along the aim path

### Gunfeel staples
- **Condensed Munitions** — dump the mag in one shot; pierce scales with ammo spent
- **Long Scope** — reverse falloff (damage rises with distance) on DMR and laser
- **Overheated Capacitor** — laser damage ramps the longer the beam is held
- **Gravitational Collapse** — with Laser Hover, laser pulls enemies toward aim
- Plus ported staples: Hot Swap, Aux Reserves, Laser Hover, grid expanders, and other vanilla DMLR economy cards as independent copies

## Install

```
<Mycopunk>/BepInEx/plugins/DMLRRework.dll
```

**Dependency:** [BepInExPack_Mycopunk](https://thunderstore.io/c/mycopunk/p/BepInEx/BepInExPack_Mycopunk/)

Build from source:

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/DMLRRework.dll`

## Notes

- Client-side mod; vanilla Scout / DMLR pool is not modified
- Upgrades are deep-copied onto Marksman via `CreateUpgrade` (mod GUID + id offset)
- Unlock and level persist across relaunch
- Designed for mid–long range precision / anatomy play — not pure spray clear without Conductor investment

## Authors

- Sparroh

## License

MIT — see `LICENSE`
