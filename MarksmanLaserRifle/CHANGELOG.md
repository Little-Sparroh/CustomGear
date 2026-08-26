# Changelog

## 1.0.0

First stable release of the **Marksman Laser Rifle** — a full dual-mode DMLR rework with the Severance anatomy framework.

### Weapon
- Independent primary gear slot (`dmlr_rework`) — vanilla Scout / DMLR untouched
- Fully automatic DMR, ADS disabled, hold-aim (RMB) laser discharge
- ~20 DMR hits to full laser charge; reload never mode-toggles
- Hot Swap input handling (role/priority flip, empty-mag idle fixes)
- Persistence: unlock, level, and equipped upgrades survive relaunch

### Severance framework
- Shared runtime for **Mark**, **Expose**, and **Transfer**
- Transfer destinations prefer shells and damageable cores (never limbs)
- Drill-path transfer: focus one outermost shell, then inward on that breach
- Skip shell-gated cores until a living shell is cleared
- Re-transfer guard so transferred hits do not chain forever

### Dissection path
- **Neural Feedback** — limb damage transfers % to core (laser > DMR)
- **Overkill Conduit** — limb-kill overkill multiplies and transfers inward
- **Arterial Shred** — +limb / −shell; Open Artery bonus laser transfer
- **Joint Breaker** — every 3rd DMR limb hit applies Decay
- **Phantom Pain** — bonus damage + charge refund vs regrown limbs
- **Bleed Charge** — limb hits generate bonus laser charge

### Breach path
- **Hard-Light Designator** — shell kills Expose the core; laser refund on Exposed
- **Core Brand** — DMR shell Brand stacks; max → next laser Exposes
- **Pulverizer** — +shell / −limb with inward splash
- **Fault Line** — repeated DMR hits escalate damage on the same shell
- **Reactor Tap** — core kills refund charge; Exposed also refunds ammo
- **Breach Charge** — shell hits generate bonus laser charge
- **Collapse Wave** — shell-kill pulse scaled by shell max HP

### Conductor path
- **Sympathetic Arc** — laser on Marked parts arcs Shock to nearby enemies
- **Sympathetic Resonance** — Marked damage echoes on other marks on that brain
- **Voltaic Battery** — reload throws aim-battery blast + inward transfer
- **Demonstrator's Trick** — mode-switch Mark empower (spread / heavy Mark)
- **Triple Feed** — DMR always Shock or Acid (rolled per magazine)
- **Rot Thread** — every 3rd DMR shell hit applies Rot
- **Marked Recycling** — laser refunds DMR ammo on Marked or Exposed parts
- **Elemental Emitter** — Shocking + Corrosive merge into one rolled Shock/Acid laser
- **Incendiary Laser** — periodic fire damage wave along the aim path

### Gunfeel & staples
- **Condensed Munitions** — dump mag in one shot; pierce + charge scale with ammo spent
- **Long Scope** — reverse falloff on DMR and laser
- **Overheated Capacitor** — laser damage ramps with continuous beam time
- **Gravitational Collapse** — with Laser Hover, laser pulls enemies toward aim
- Remaining vanilla DMLR upgrades port as independent copies (Hot Swap, Aux Reserves, Laser Hover, economy cards, etc.)
- Vanilla Scout upgrade pool is never modified

---

## 0.6.0
- Phase 1C Conductor path:
  - Sympathetic Arc (ArcLightning): laser on Marked arcs Shock to nearby enemies
  - Sympathetic Resonance (Elemental Recycling): Marked damage echoes on same brain
  - Collapse Wave (Hot Exhaust): shell-kill pulse × shell max HP
  - Voltaic Battery v2 (Tosser): reload throws aim-battery blast + inward transfer
  - Demonstrator's Trick v2 (TosserTwo): mode-switch Mark empower (not flat dmg buff)
  - Hard-Light Designator moved to Shield Piercing slot

## 0.5.2
- Hot Swap: empty DMR mag no longer auto-starts laser with no buttons held
  (hard idle when Fire/Aim released; poll real input; clear forceEnableFire on empty)

## 0.5.1
- Hot Swap: laser only while LMB held with charge — no sticky laser after DMR
- Phantom Pain: track killed limb ids/names (not brains) so sibling limbs aren't "regrown"
- Triple Feed: stamp GunData.damageEffect so Photon Disc / shared systems see the element
- Elemental Emitter: Shocking + Corrosive Laser merge into one rolled Shock/Acid laser
- Incendiary Laser kept as separate fire-wave upgrade

## 0.5.0
- Phase 1B: Mark plumbing + anatomy upgrades
  - Joint Breaker: every 3rd DMR limb hit applies Decay
  - Rot Thread: every 3rd DMR shell hit applies Rot
  - Fault Line, Reactor Tap, Core Brand, Phantom Pain
  - Bleed Charge, Breach Charge, Marked Recycling, Triple Feed rename

## 0.4.4
- Transfer drill path: focus one outermost shell (weakest), sticky until dead, then inward
  on that breach only — does not clear every shell on a layer before going deeper

## 0.4.3
- Transfer playtest debug (SeveranceSystem.DebugTransfer=true): BepInEx log lines + cyan
  floating "XFER" text on destination + health outline pulse on shell/core

## 0.4.2
- Transfer destination: never pick limbs (parent walk was foot→leg). Prefer shell, then
  damageable core, then shell-under-core; skip limb segments entirely

## 0.4.1
- Transfer destination: skip shell-gated cores (vanilla ignores core hits until a shell dies);
  dump inward into living shell/parent instead so Neural Feedback / Overkill Conduit / Pulverizer
  splash work before the core is exposed

## 0.4.0
- Severance v2 foundation: Mark/Expose/Transfer runtime (`SeveranceSystem`)
- Neural Feedback (Autocycler slot): limb damage transfers % to core (laser > DMR)
- Arterial Shred (Shredder slot): +limb/−shell; Open Artery bonus laser transfer
- Pulverizer rewrite: +shell/−limb + inward splash toward core
- Overkill Conduit (Energy Regeneration slot): limb-kill overkill transfers inward
- Hard-Light Designator (ArcLightning slot): shell kill Exposes core; +dmg + laser charge refund
- Unmapped vanilla upgrades still port as fillers

## 0.3.3
- Condensed Munitions: laser charge scales with ammo spent (20-dump → 20× laserChargeOnHit on hit)

## 0.3.2
- Condensed Munitions: stop double-Kill NRE (disable PierceTargets when budget spent; never Kill from hooks)
- Long Scope: reverse falloff also applies to laser (set range before laser early-return)

## 0.3.1
- Condensed Munitions: real target pierce (PierceTargets + multi-pierce budget), not bounce
- Long Scope: explicit reverse falloff curve (5→55m, damage rises with distance)
- Gravitational Collapse: IPullable.AddImpulseForce_Client pull (no transform teleport / no kill)

## 0.3.0
- Remaining design-doc rewrites:
  - Shredder: laser periodically applies Decay
  - Demonstrator's Trick (TosserTwo): mode-switch temporary damage buff
  - Gravitational Collapse (Sturdy): with Laser Hover, laser pulls enemies to aim
  - Condensed Munitions (DmrDmg): dump mag in one shot; pierce per 10 ammo
  - Incendiary Laser: periodic fire damage wave along aim path

## 0.2.3
- Overheated Capacitor: apply ramp via ModifyContinuousBulletDamage (laser freezes BulletData at beam start)

## 0.2.2
- Wave 2 simpler rewrites:
  - Tainted Exhaust (Hot Exhaust): DMR kill explosion
  - Pulverizer: every 3rd DMR shot applies Rot
  - Triple: DMR always Shock or Acid (rolled)
  - Long Scope: reverse falloff (damage ↑ with distance)
  - Overheated Capacitor (LasDmg): laser dmg ramps with beam airtime

## 0.2.1
- Voltaic Battery (Tosser rewrite): every 3rd DMR shot + ~50% of laser damage
- Upgrade port defers until PlayerData/GearData ready (fixes CreateUpgrade NRE)

## 0.2.0
- Phase 2 wave 1: port vanilla DMLR upgrades onto Marksman Laser Rifle as independent copies
- Deep-copied properties + hex patterns via CreateUpgrade (vanilla Scout pool unchanged)
- Upgrade ids = vanilla NumberID + 50000 under mod GUID sparroh.dmlrrework

## 0.1.0
- Phase 1: Marksman Laser Rifle custom primary (clone of ScoutLaserRifle / DMLR)
- Automatic DMR fire
- ADS removed; hold aim (RMB) discharges laser
- Both buttons held: laser wins (default); with Hotswap, DMR wins
- Laser charge fills in 20 DMR hits (matches default mag)
- Hold-R no longer toggles mode (reload stays reload)
- Independent gear slot — vanilla Scout/DMLR untouched
