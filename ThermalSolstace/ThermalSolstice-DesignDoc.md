# Thermal Solstice — Design Document (v1)

## 0. Locked Decisions (2026-08-14)

| Decision | Lock |
|----------|------|
| Product shape | **Parallel new heavy** — does **not** replace or patch vanilla Laser Cannon |
| Player-facing name | **Thermal Solstice** |
| Working APIName | `thermal_solstice` |
| Working GUID | `sparroh.thermalsolstice` |
| Slot | **Heavy** |
| Relation to vanilla | Cousin fantasy to **Laser Cannon** (dedicated continuous beam heavy); own gear id, grid, strings |
| Identity direction | **A — Thermal Lance** (sustained beam → heat → path dialect) |
| Paths | **Reactor / Conflagration / Optics** |
| Heat model | **Soft meter only** — readable skill/power channel; **no** hard overheat shutdown, forced vent lockout, or mandatory self-damage on baseline |
| Baseline Fire | **Strong native ignite** — Conflagration elevates cook fantasy; does not invent Fire from zero |
| Main-weapon bias | **Pack-consistent hose** — strong continuous main-battery heavy; not Saw-ramp specialist ST |
| Dual-mode | **No** — full-time beam; DMLR keeps dual-mode anatomy |
| Severance / Mark / Brand spine | **No** — stay clear of DMLR Severance and Final Judgement Brand as core vocabulary |
| Tone | **SAXON industrial siege laser** — heat shimmer, hazard stripes, collimator industrial; not magic staff |
| Exotic count | **E6** — equal large hex shapes |
| Ship pool | **~30** upgrades; hybrids OK; no anti-synergy matrix |
| MycoMod (impl) | IsSandbox |
| Doc file | ThermalSolstice-DesignDoc.txt (this file) |
| Catalog wink | May reference Laser Cannon as cousin; must not claim to be a rework patch of vanilla LC |

---

## 1. High Concept / Fantasy

A SAXON **man-portable thermal siege collimator**. Hold the trigger. A thick continuous beam walks the lane, ignites what it touches, and the core warms under your thumb. Heat is not a punishment meter — it is a **soft power channel**: the longer you commit, the more the gun *reads* as alive. Upgrades decide whether that warmth becomes reactor overdrive, wildfire authority, or optical geometry.

Baseline is honest main-battery heavy: point, hold, cook, manage mag. No charge gate, no mode swap, no free Supernova, no free self-immolation loop.

**One-liner:** *Hold the sun. Manage the heat. Empty the lane.*

**Product shape:** New parallel heavy (**Thermal Solstice**). Does not replace Laser Cannon, DMLR, Final Judgement, AMR, Rocket Salvo, Manifold, or any other catalog entry.

**SAXON marketing blurb (draft):**
  “SAXON TS-7 Thermal Solstice — Continuous thermal collimator for
   extended field authorization. Heat telemetry is advisory. Ignition
   of local flora, fauna, and coworkers is within expected operating
   envelope. (Not a Laser Cannon. Different docket. Similar sunset.)”

Optional stingers:
- “If it is still moving, you are under-committed.”
- “Heat is not a warning. Heat is a progress bar.”
- “Magazine: large. Patience: optional. Aim: non-negotiable.”
- “DMLR charges a laser. This *is* the laser.”
- “Solstice: longest day. Longest beam. Same joke every fiscal year.”

---

## 2. Role & Fantasy in the Arsenal

- **Slot:** Heavy
- **Range:** Mid–Long continuous beam (main-battery reach; not sniper charge, not shotgun cone)
- **Role:** Sustained lane delete / ignite hose / optional heat-power, wildfire, or beam-geometry forks
- **Loop:** Acquire lane → hold beam → heat climbs (soft) → Fire cooks → reload beat → repeat
- **Gap filled:**
  - Laser Cannon (vanilla) = dedicated Fire beam heavy (sibling; we are parallel catalog)
  - DMLR = dual-mode marksman + part-time laser / Severance anatomy
  - Final Judgement = 8s charge rocket / Hammer of Dawn designator
  - AMR = kinetic bolt / chamber spike
  - Rocket Salvo = lock barrage + ally heal
  - Manifold Rocket = ray rockets, anti-sphere doctrine
  - Zephyr = sonic overpressure cone
  - Hard-Light Constructor = Shatter jam + paint (primary)
  - Nothing owns **full-time thermal main-battery heavy with soft heat channel + Reactor / Conflagration / Optics grid** as a parallel catalog weapon

**Not trying to be:** DMLR always-on, Last Argument charge delete, Salvo support lock, anatomy transfer rifle, or a silent stat-stick clone of vanilla LC with a new name only.

### 2.1 Comparison snapshot

```
Weapon                 Niche                         Thermal Solstice differentiator
---------------------  ----------------------------  ------------------------------------------
Laser Cannon (vanilla) Dedicated Fire beam heavy     Parallel cousin; heat channel + 3-path grid
DMLR                   Dual-mode anatomy rifle       Full-time beam; no Severance spine
Final Judgement        8s charge rocket / HoD        Continuous hose; no authorization gate
AMR                    Kinetic bolt / chamber        Beam tick DPS; Fire native
Rocket Salvo           Lock barrage + ally heal      Single continuous beam; no heal-lock
Manifold Rocket        Ray rockets, no spheres       Hitscan/beam collimator; heat, not manifold
Zephyr                 Sonic cone primary            Heavy beam lane, not pressure blast
Heaven’s Fury          Proc smites on hit            Sustained beam identity, not proc sky
```

### 2.2 Naming note

**Thermal Solstice** deliberately evokes solar / peak-heat / longest-burn without colliding on Laser Cannon API name, gear id, or display string. Codex may wink at the cousin; it must not ship as a replacement mod for vanilla LC.

---

## 3. Design Pillars

1. **Full-time continuous beam is sacred** — zero upgrades still feels like a complete main-battery heavy hose.
2. **Soft heat meter is sacred** — builds while firing, decays while not; readable; **never** hard-shutdown baseline.
3. **Strong native Fire is sacred** — baseline ignites; Conflagration is mastery, not “unlock Fire.”
4. **Main-weapon hose bias** — pack-consistent sustained DPS; Saw-ramp ST is not the baseline sentence (optional Optics exotic only if ever, not v1 spine).
5. **Heat is a channel, not a curse** — Reactor may *opt into* self-damage / risky overdrive; baseline heat is power-adjacent juice only.
6. **On-hit / heat / ignite / beam-geometry verbs > flat % damage stickers.**
7. **Three peer paths (Reactor / Conflagration / Optics); hybrids intended; no anti-synergy matrix.**
8. **DMLR contrast is law** — no dual-mode, no Severance Mark/Transfer/Expose as spine.
9. **R = reload only; RMB free on baseline** → claimed by path toys (vent pulse, prism paint, scorch brush) when crowns demand.
10. **~30 ship upgrades; E6 equal large exotic shapes.**
11. **Boss-safe** — strong ST via uptime and Optics focus cards; no permanent floor delete; ignite/scorch budgets capped.
12. **Ally-safe** — beam respects friendly rules; scorch ally damage low/off; no team-cook identity.
13. **SAXON industrial siege-laser tone** — collimator, heat shimmer, hazard stripes, solar catalog humor.

---

## 4. Core Mechanics & Gunfeel (Baseline)

### 4.1 Base gun

| Trait        | Draft / intent |
|--------------|----------------|
| Fire mode    | **Hold M1** continuous beam (tick damage on interval) |
| Damage       | High sustained tick DPS — main-weapon heavy band (cousin to vanilla LC ~780/tick as *feel* anchor, retune in playtest) |
| Element      | **Fire** strong native `damageEffect` + solid effectAmount |
| Fire rate    | Beam tick interval ~0.22–0.28 s (vanilla LC 0.26 anchor) |
| Mag / reserve| **Large mag** (draft 400–500); heavy ammo economy (reserves 0 or modest pool per heavy rules — match project heavy ammo patterns at impl) |
| Reload       | Deliberate ~2.5–3.2 s — the downtime beat after a long hold |
| Spread/recoil| Near-hitscan collimator; light recoil thrash that *reads* more as heat shimmer when hot |
| Range        | Long effective (falloff starts far); main-battery reach |
| ADS / RMB    | Optional light ADS; **RMB unbound** on baseline |
| Movement     | Mild move penalty while firing (plant-friendly, not Sturdy root); path cards may root or surge |
| Model/audio  | Siege collimator chassis, solar-cell mag, rising heat whine, ignition hiss, beam roar scales with heat |

Draft firefeel band (VALIDATE IN PLAYTEST):

| Param | Draft |
|-------|--------|
| Tick damage | High main-battery (LC-cousin) |
| Fire effectAmount | Strong (baseline ignite in a focused burst of ticks, not 30s of grazing) |
| Tick interval | ~0.26 s |
| Mag | ~450 |
| Reload | ~3.0 s |
| Falloff start/end | Long (e.g. 250–400 m band — tune to mission scales) |
| Heat build while firing | ~0.35–0.50 Heat/s toward 1.0 |
| Heat decay while not firing | ~0.55–0.80 Heat/s after short grace (~0.15–0.25 s) |
| Soft heat bonus at full | Small inherent juice only (e.g. +5–10% damage **or** +beam width crumb **or** VFX-only + tiny ammo efficiency) — must not rival Reactor cards |
| Pierce | **None** on baseline (Optics owns pierce/split) |
| Hover / root | **None** on baseline |

### 4.2 Inputs

| Input | Baseline | Upgraded claims |
|-------|----------|-----------------|
| Hold M1 | Continuous thermal beam + Heat build + Fire apply | All path riders |
| Release M1 | Stop beam; Heat decays after grace | Reactor soft bonuses linger briefly; Conflagration scorch may persist |
| RMB | **Unbound** | Emergency Vent pulse / Prism paint / Scorch brush (priority table §13) |
| R | Reload only | Reload only |
| Heavy equip | Normal | No baseline link to other gear |

### 4.3 Heat model (soft — sacred)

```
Heat ∈ [0, 1]

While firing (owner, mag > 0):
  Heat += buildRate * dt
  Clamp 1.0

While not firing:
  After graceDelay:
    Heat -= decayRate * dt
    Clamp 0.0

Baseline at Heat ≥ thresholds (juice only):
  0.00–0.33  Cool   — default beam VFX/audio
  0.33–0.66  Warm   — brighter core, light shimmer; optional tiny +dmg crumb
  0.66–1.00  Hot    — heavy shimmer, audio roar; soft cap juice (see table)
  1.00       Solstice peak — max baseline juice; still NO shutdown, NO forced self-damage
```

**Soft meter laws:**
1. Heat never stops you from firing on baseline.
2. Heat never empties the mag as a punishment on baseline.
3. Heat never deals self-damage on baseline.
4. Reactor cards may *add* opt-in risk (self-damage, ammo tax, vent spend) — labeled and path-owned.
5. UI must show Heat (bar / ring / collimator glow). Unreadable heat is a design bug.

### 4.4 Baseline combat loop (zero upgrades)

```
Find lane / pack seam → Hold M1 thermal beam
   → ticks delete grunts; Fire saturation climbs on focus targets
   → Heat rises (soft); beam looks/sounds hungrier
   → sweep or pin elites while mag allows
   → R on dry; Heat decays on the reload beat
   → no vent, no prism split, no supernova pulses, no scorch fields, no self-damage
```

Skill without upgrades: lane discipline, ignite focus vs spray, mag husbandry on long holds, using reload as heat reset rhythm, not panic-beaming empty air.

### 4.5 What baseline does NOT include

- No hard overheat / shutdown / forced vent
- No self-damage
- No Supernova periodic explosions
- No beam pierce / prism split / return beam
- No lingering world scorch fields (tiny impact ignite VFX OK)
- No hover / root turret stance
- No secondary-ability charge bank (Shared Processing DNA is card-owned, thin)
- No dual-mode DMR
- No Severance Mark / Transfer / Expose
- No Brand designator (Final Judgement lane)
- No mag-1 charge rocket fantasy

Those are path-, exotic-, or unlock-owned.

---

## 5. Shared Framework Vocabulary

Upgrades speak these verbs. Baseline owns **Beam**, **Heat (soft)**, **Fire apply**.

### 5.1 Beam
- Continuous collimator damage ticks while M1 held and mag allows
- Primary delivery for damage, Fire amount, Heat build, and path on-hit riders
- Width / pierce / split / pulse are Optics-owned unless a card says otherwise

### 5.2 Heat
- Soft [0,1] channel from sustained fire
- Reactor spends / multiplies Heat as power
- Other paths may read Heat as a conditional bonus (hybrid bait) without owning the meter

### 5.3 Ignite / Fire
- Real `EffectType.Fire` saturation via gun damageEffect + effectAmount
- Baseline strong; Conflagration multiplies apply, burn zones, ignite-on-kill, stack-on-ignite
- “Ignited” = target at or near full Fire saturation / burning state as game defines

### 5.4 Scorch
- World-space burn patch left by beam or kills (Conflagration-owned)
- Duration + radius budgeted; ally-safe; not mission-wide pavement
- Distinct from Hard-Light Constructor walkable panes — **damage/status floor, not architecture**

### 5.5 Vent
- Spend or dump Heat for a burst payoff (Reactor-owned)
- Soft meter means Vent is a *spend button*, not an emergency eject from shutdown
- Often RMB when crowned

### 5.6 Solstice Peak
- Flavor name for Heat ≈ 1.0 state
- Baseline: max soft juice + VFX
- Cards: “while at Solstice Peak…” conditionals

### 5.7 Pulse / Supernova
- Periodic beam-anchored explosions while firing (Optics-owned Supernova DNA)
- Ammo tax and interval are real costs
- Element may roll or stay Fire-biased

### 5.8 Prism / Split
- Additional beam lines, forks to nearby targets, or return traces (Optics-owned)
- Clear amplifier; must still reward aim (primary beam remains strongest)

### 5.9 Focus
- Optional conditional: bonus while continuously connected to the same brain/part
- **Not** baseline Saw-ramp identity; only light Optics cards if used — hose bias remains law
- Prefer width/pierce/pulse over hard ramp-ST as the Optics fantasy center

### 5.10 Capacitor (thin)
- Shared Processing Array DNA: sustained fire banks a small resource for secondary ability charge or a one-shot dump
- **Not a full path** — one Epic/Rare + optional generic; loadout-dependent by nature

---

## 6. Upgrade Paths (gravity wells — hybrids intended)

### Path A — REACTOR (heat = power)
“The core wants to run hot. Let it.”

- Spine: Heat build/decay manipulation, damage/ammo efficiency while Warm/Hot/Peak, Vent spend bursts, opt-in self-damage overdrive (Scorched Earth elevated), tick-rate while hot
- Clear vs ST: Both — uptime power scales whatever you point at; Vent for pack spike
- Hybrid hooks: Hot beam applies bonus Fire (Conflagration); Hot beam widens slightly (Optics)

### Path B — CONFLAGRATION (wildfire apostle)
“If it burns, it belongs to you.”

- Spine: +Fire apply, damage vs ignited, Solar Flare-style stacks on ignite, scorch fields, ignite spread on kill, Formicidae-style ignited melt (with apply trade if needed)
- Clear vs ST: Clear native via spread/scorch; ST via focus ignite + melt cards
- Hybrid hooks: Scorch + Pulse double cook; Heat-at-Peak ignite rate (Reactor)

### Path C — OPTICS (industrial cutting geometry)
“Shape the beam. Own the line.”

- Spine: beam width, pierce, prism split, Supernova cadence, range/collimation, light focus conditionals without abandoning hose
- Clear vs ST: Clear via width/split/pulse; ST via pierce into cores + collimated damage
- Hybrid hooks: Prism lines apply Fire; Pulses spend Heat crumb (Reactor hybrid)

### Path × verb matrix

```
                 REACTOR              CONFLAGRATION           OPTICS
Beam             delivers Heat power  delivers Fire           delivers geometry
Heat             core fantasy         conditional cook bonus  optional pulse fuel
Ignite/Fire      rider while hot      core fantasy            prism/scorch hybrid
Scorch           vent leaves patch    core fantasy            pulse seeds scorch
Vent             core fantasy         ignite vent hybrid      —
Pulse/Prism      heat-tax pulses      fire-element pulses     core fantasy
RMB claim        Vent                 Scorch brush (rare)     Prism paint / pulse arm
```

---

## 7. Crowns & Sacred Cows

### Criticality — Exotic (Reactor crown)
- While Heat ≥ high threshold (e.g. 0.75+), beam gains large damage and ammo efficiency.
- **Opt-in risk rider:** modest self-damage per second **or** per tick while in Criticality (playtest: prefer small DoT-to-self while Peak, not chunk random hits).
- Soft meter preserved: you can leave Criticality by releasing M1; no hard lockout.
- Scorched Earth DNA elevated — the “run hot on purpose” card.

### Emergency Vent — Exotic (Reactor crown)
- RMB (or release-M1 + RMB): **spend** current Heat (all or majority) to emit a radial thermal shockwave.
- Damage/radius scale with Heat spent; applies Fire amount in radius.
- Brief personal heat shimmer DR crumb optional (not a real shield).
- Cooldown short–mid; cannot soft-lock bosses forever.
- Teaches Heat as a spendable resource without baseline shutdown.

### Solar Authority — Exotic (Conflagration crown)
- Igniting a target (Fire full-sat enter) grants stacking beam damage (Solar Flare elevated).
- Stacks have duration and cap; refreshing on new ignites.
- At high stacks, beam gains minor Fire apply bonus (snowball cook, not infinite).
- The “wildfire apostle” keystone.

### Wildfire Charter — Exotic (Conflagration crown)
- Beam leaves short-lived **Scorch** patches on terrain and on kills.
- Enemies standing in Scorch take Fire ticks / Fire apply.
- Killing an ignited enemy spreads a small Fire burst to nearby enemies (pack cook).
- Hard caps on active scorch patches and total area — no mission pavement.
- Ally damage off or trivial.

### Supernova Cadence — Exotic (Optics crown)
- While firing, every N seconds the beam emits an elemental explosion at the aim impact / along beam.
- Explosion damage + size tunable; **additional ammo cost** per pulse (Supernova DNA).
- Default element Fire; optional rolled Shock/Acid on apply for build variety (one roll per upgrade instance).
- Interval readable (audio wind-up tick). Miss-aiming wastes ammo — skill tax.

### Prism Array — Exotic (Optics crown)
- Primary beam gains **split traces**: 1–2 weaker side beams toward nearby enemies within a cone of the aim line (or last hit brain’s neighbors).
- Side beams deal reduced damage and reduced Fire apply; primary remains king.
- Optional: after a short continuous hold, a single **return glint** traces back along the beam line for a bonus tick (readable optics toy).
- Clear identity without deleting aim.

Sacred cows (do not cut without rewriting identity):
- Parallel weapon (not vanilla LC replace)
- Continuous full-time beam baseline
- Soft Heat meter (no hard overheat baseline)
- Strong native Fire
- Hose / main-battery bias
- Three peer paths; hybrids OK
- E6 crowns above as path-defining exotics
- No DMLR Severance spine
- No 8s charge rocket spine

---

## 8. Full Upgrade List (~30 ship + backlog)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity  
Cell rule: Exotic shapes larger than others; all Exotics same cell count.  
Player-facing names below. API names assigned at implementation.  
Vanilla LC names are DNA only — **full rename** for parallel identity.

------------------------------------------------------------------------------
PATH A — REACTOR
------------------------------------------------------------------------------

A-EX1. Criticality — Exotic (crown)
       High-Heat threshold: +damage, +ammo efficiency, opt-in self-damage while hot.
       Scorched Earth elevated.

A-EX2. Emergency Vent — Exotic (crown)
       RMB spend Heat → radial thermal shockwave + Fire apply; scales with Heat spent.

A-EP1. Thermal Runaway — Epic
       +Heat build rate while firing. Soft Peak reached faster; decay unchanged.
       Makes Reactor conditionals available sooner without hard overheat.

A-EP2. Heat Pump Loop — Epic
       While Heat ≥ 0.5: +beam damage. While Heat < 0.5: +reload speed crumb.
       Rewards living in the warm band; still soft.

A-EP3. Bleedoff Turbine — Epic
       Heat decay is slower for a short time after you stop firing (linger power window).
       Opposite of instant chill — hybrid-friendly with Vent timing.

A-EP4. Secondary Bus — Epic (Capacitor / Shared Processing DNA)
       Sustained fire builds Capacitor stacks. On stop-firing, convert stacks into
       secondary ability charge (or a small personal overcharge buff if no secondary).
       Thin loadout bridge — not a third fantasy.

A-RA1. Redline Governors — Rare
       +Damage while at Solstice Peak (Heat ≥ 0.9). Tiny −damage while Cool (Heat < 0.25).
       Focus tax toward hot play.

A-RA2. Phase Coolant — Rare
       −Self-damage taken from Criticality (and any Reactor self-damage riders) by a %.
       Enables hotter Criticality play without deleting the risk fantasy.

A-RA3. Mag Circulation — Rare
       While firing at Heat ≥ 0.5, small chance per tick to refund 1 mag ammo.
       Uptime economy; not infinite with Criticality ammo efficiency — tune together.

A-RA4. Plant Stance — Rare
       While firing: −move speed, +damage resistance crumb, +beam damage crumb.
       Sturdy-lite; not full root unless stacked with a future card.

A-ST1. Core Warmers — Standard
       Minor +Heat build rate.

------------------------------------------------------------------------------
PATH B — CONFLAGRATION
------------------------------------------------------------------------------

B-EX1. Solar Authority — Exotic (crown)
       On ignite: stacking beam damage buff (cap + duration). Wildfire apostle keystone.

B-EX2. Wildfire Charter — Exotic (crown)
       Beam/kills leave Scorch; ignited kills spread Fire. Area caps enforced.

B-EP1. Formicid Lens — Epic
       +Damage vs ignited targets. −Fire effectAmount on the gun (apply slower).
       Formicidae Magnifier DNA — melt what already burns; less spray-ignite.

B-EP2. Kindling Hymn — Epic
       +Fire effectAmount on beam ticks. Faster time-to-ignite.

B-EP3. Afterglow — Epic
       When you stop firing, emit a short forward cone of residual Fire apply / light damage
       (heat-exhaust breath). Scales lightly with current Heat (Reactor hybrid bait).

B-EP4. Cinder Stack — Epic
       Damaging an already-ignited target grants short stacking move speed or reload crumb
       (pick one primary in impl — prefer move speed for hose kiting).
       Aggressive cook mobility, not Photon Surge laser-only clone name.

B-RA1. Pilot Light — Rare
       First tick on a brain that is not ignited applies bonus Fire amount (opener).

B-RA2. Napalm Thread — Rare
       Beam leaves micro-Scorch even without Wildfire Charter, but tiny and brief
       (teaching scorch). Charter enlarges and empowers.
       If too much baseline creep, demote to “requires any Conflagration card.”

B-RA3. Flashpoint — Rare
       Igniting a target deals a one-shot burst of bonus damage (ignite proc spike).

B-RA4. Smog Sight — Rare
       +Damage vs targets standing in your Scorch. Slight −damage vs targets in clean air
       (scorch commitment tax).

B-ST1. Incendiary Collimator — Standard
       Minor +Fire effectAmount.

------------------------------------------------------------------------------
PATH C — OPTICS
------------------------------------------------------------------------------

C-EX1. Supernova Cadence — Exotic (crown)
       Periodic beam-anchored elemental explosions while firing; ammo tax.

C-EX2. Prism Array — Exotic (crown)
       Side split beams to nearby targets; primary remains strongest.

C-EP1. Aperture Saw — Epic
       +Beam width (thicker hose). Slightly −tick damage per target to keep power honest
       OR neutral damage with pure width — prefer slight per-target trim so width is the gift.

C-EP2. Hard-Light Bore — Epic
       Beam pierces +1 additional enemy part/body (hierarchy-aware if available).
       Minor +damage to shells/cores.
       Name avoids DMLR “Hard-Light Bypass” collision — use **Collimated Bore** if needed.

C-EP3. Refraction Latch — Epic
       While continuously hitting the same brain for >T seconds, +damage to that brain
       (light Focus — hose-safe ramp, resets on break). Cap the ramp hard so Saw-ST
       cannot outshine path identity. Optional backlog demote if hose bias slips.

C-EP4. Glint Protocol — Epic
       Every Nth tick, fire a high-damage single collimated spike along aim (readable
       “lens flash”) with bonus pierce crumb. Cadence toy inside the hose.

C-RA1. Long Collimator — Rare
       +Range / reduced falloff. Damage holds farther.

C-RA2. Gyro Gimbals — Rare
       −Recoil / beam wander while firing. Cleaner hose tracking.

C-RA3. Diffraction Mesh — Rare
       Prism side-beams (if any) gain +Fire apply crumb. Without Prism, small splash
       Fire at impact point (micro).

C-RA4. Pulse Armature — Rare
       Supernova interval −% (faster pulses) but each pulse costs additional ammo.
       Requires Supernova Cadence to show full value; minor solo tick if unequipped.

C-ST1. Lens Polish — Standard
       Minor +beam damage.

------------------------------------------------------------------------------
GENERIC / GUNFEEL
------------------------------------------------------------------------------

G-RA1. Heavy Feed — Rare
       +Fire rate (lower tick interval), −per-tick damage slightly.
       Hose denser; DPS roughly conserved or slight gain.

G-RA2. Deep Cell — Rare
       +Magazine size, −reload speed slightly.

G-RA3. Field Calibrator — Rare
       +Reload speed, −magazine size slightly.

G-ST1. Reserve Bus — Standard
       +Ammo reserves / heavy ammo economy crumb (match heavy ammo system at impl).

G-ST2. Motion Compensators — Standard
       −Recoil (vanilla DNA rename OK as generic).

G-ST3. Heat Pump — Standard
       Minor +beam damage (vanilla DNA name reuse allowed on generic staple only).

G-OD1. Boundary Incursion — Oddity
       Increases upgrade grid size.

------------------------------------------------------------------------------
FROZEN 30 FOR V1 SHIP
------------------------------------------------------------------------------

EXOTIC (6)
  1  Criticality
  2  Emergency Vent
  3  Solar Authority
  4  Wildfire Charter
  5  Supernova Cadence
  6  Prism Array

EPIC (8)
  7  Thermal Runaway
  8  Heat Pump Loop
  9  Formicid Lens
 10  Kindling Hymn
 11  Afterglow
 12  Aperture Saw
 13  Collimated Bore          (Hard-Light Bore rename — avoid DMLR collision)
 14  Secondary Bus

RARE (10)
 15  Redline Governors
 16  Phase Coolant
 17  Mag Circulation
 18  Pilot Light
 19  Flashpoint
 20  Smog Sight
 21  Long Collimator
 22  Gyro Gimbals
 23  Heavy Feed
 24  Deep Cell

STANDARD (5)
 25  Core Warmers
 26  Incendiary Collimator
 27  Lens Polish
 28  Reserve Bus
 29  Motion Compensators

ODDITY (1)
 30  Boundary Incursion

------------------------------------------------------------------------------
BACKLOG (designed, not in first 30)
------------------------------------------------------------------------------

Reactor
- Bleedoff Turbine
- Plant Stance
- Capacitor overcharge personal buff variant if secondary charge feels dead solo
- Full Scorched self-damage tuning knobs as separate cards
- “Heat converts to DR while not firing” chill armor

Conflagration
- Cinder Stack
- Napalm Thread (or promote if scorch unread without Charter)
- Blue Flame (damage vs ignited +++ / ignite duration −)
- Ally-safe warm aura (thin support — keep thin)
- Fungal charcoal: scorch slows enemies lightly

Optics
- Refraction Latch (promote only if ST feel weak)
- Glint Protocol
- Diffraction Mesh
- Pulse Armature
- True multi-pierce ladder
- Beam ricochet off world once (careful, jank)

Generic
- Field Calibrator, Heat Pump staple
- Hover-while-firing (Kinetic Impossibility cousin) — not identity
- Photon stride move-speed-while-firing
- Element-rolled beam (Shock/Acid) — Conflagration is Fire-first; keep off v1 spine
- Shared Processing larger investment tree — rejected as full path

---

## 9. Example Builds

Reactor redline (hot hose)
  Criticality + Emergency Vent + Thermal Runaway + Heat Pump Loop
  + Redline Governors + Phase Coolant + Mag Circulation + Heavy Feed
  Live in Peak; Vent when packs clump; Phase Coolant keeps self-damage honest.

Wildfire apostle (clear cook)
  Solar Authority + Wildfire Charter + Kindling Hymn + Formicid Lens
  + Pilot Light + Flashpoint + Smog Sight + Incendiary Collimator
  Ignite → stacks → scorch floor → melt ignited; Formicid rewards focus on burners.

Optics lane control
  Supernova Cadence + Prism Array + Aperture Saw + Collimated Bore
  + Long Collimator + Gyro Gimbals + Lens Polish + Deep Cell
  Wide piercing hose with pulse punctuation and split clear.

Hybrid solstice freak (showcase)
  Criticality + Solar Authority + Supernova Cadence
  + Wildfire Charter + Heat Pump Loop + Kindling Hymn + Prism Array
  Hot beam cooks, pulses detonate, scorch claims the floor — allowed under soft heat laws.

Vent bomber (Reactor + Conflagration)
  Emergency Vent + Afterglow + Thermal Runaway + Flashpoint
  + Pilot Light + Core Warmers
  Build Heat, Vent for radial ignite, Afterglow on release, re-light with Pilot.

---

## 10. Economy & Tuning Rules of Thumb

- Power budget lives in **uptime × aim × Heat conditionals × ignite state** — not only raw tick damage stickers.
- Soft Heat baseline juice must stay **below** a single Reactor Rare so cards feel like volume knobs.
- Criticality self-damage: annoying if ignored, survivable if kited; never couple with forced shutdown.
- Vent should feel better than “just keep beaming” for clumped packs, worse than beam for single burning elite (unless stacked ignite cards).
- Solar Authority stacks: strong but duration-gated; no permanent +100% from one ignite.
- Wildfire scorch: max patches 4–8; duration 3–6s; if players pave missions, cut duration first.
- Supernova ammo tax must be visible in the mag; free pulses are a feel bug.
- Prism side-beams: ≤50–60% primary damage; Fire apply reduced; primary aim still matters.
- Formicid Lens apply penalty should hurt spray-ignite but not make ignite impossible with Kindling.
- Mag 450-class: Criticality ammo efficiency + Mag Circulation can infinity-beam — watch stacked economy; nerf refunds before nerfing fantasy spikes.
- Secondary Bus: modest charge only; must not outpace dedicated support kits.
- Bosses: full beam OK; scorch tick rate capped; Vent radial soft on bosses if needed.

### Playtest acceptance tests (pass/fail feel)

1. **Teaching:** Zero upgrades — hold beam, see Heat climb, ignite a target, delete a lane. If it feels like a generic damage hose with a useless bar, Heat juice/VFX is too weak.
2. **Soft law:** Player can hold Peak forever without shutdown. If they feel punished for existing at Peak without Criticality, baseline Peak tax crept in — remove it.
3. **Reactor opt-in:** Criticality without Phase Coolant is spicy but playable; with it, comfortable hot hose.
4. **Cook literacy:** Kindling + Solar Authority makes ignite-first play obviously stronger than brain-off spray within one mag.
5. **Hose bias:** Prism + Aperture clear packs without requiring Refraction Latch Saw play.
6. **Anti-pave:** Wildfire Charter does not carpet whole missions.
7. **Anti-infinite:** Criticality + Mag Circulation + Deep Cell does not delete reload as a verb.
8. **DMLR contrast:** A DMLR player picking Solstice should not look for mode swap or limb transfer — loop is hold/heat/ignite/geometry only.
9. **Cousin, not clone:** Side-by-side with vanilla LC, Solstice reads Heat + path toys; not “same gun different icon.”

---

## 11. Status & Counter Split (explicit)

| Status / counter | Role on this gun                    | Baseline? | Notes |
|------------------|-------------------------------------|-----------|-------|
| Fire EffectType  | Native beam element / ignite spine  | Yes       | Strong |
| Heat (custom)    | Soft power channel 0–1              | Yes       | Not a StatusEffect; behaviour meter |
| Scorch (world)   | Burn patches                        | Charter / cards | Not walkable architecture |
| Capacitor stacks | Secondary bus resource              | Secondary Bus | Thin |
| Shock/Acid       | Optional Supernova roll             | Exotic roll | Not spine |
| Cryo/Poison/etc. | Not identity                        | No        | |
| Decay/Rot        | Not identity                        | No        | |
| Shatter/Jam      | Not identity (HLC owns)             | No        | |
| Mark/Transfer    | Not identity (DMLR owns)            | No        | |
| Brand            | Not identity (FJ owns)              | No        | |

---

## 12. Strengths, Weaknesses & Co-op

Strengths
- Clearest full-time beam heavy fantasy in the parallel catalog
- Soft Heat gives readable skill/power without dual-mode complexity
- Strong baseline Fire + hose DPS — complete gun at zero upgrades
- Three distinct dialects: power / wildfire / geometry
- Hybrid space is natural (hot cook pulses)
- Co-op: you ignite and scorch; allies dump into burning targets

Weaknesses
- Brain-off beam into empty air wastes mag (intended)
- Reload beat after long holds is real downtime
- Reactor self-damage builds are self-selecting risk
- Weaker “one big authorization boom” than Final Judgement
- Not an anatomy surgeon; limbs are just meat in the lane
- Secondary Bus is loadout-dependent and intentionally thin

Co-op
- Ignite and Scorch are gifts — default ally-safe floors
- Vent radial should not grief allies
- Avoid team-hostile beam rules
- You are main battery, not Salvo heal support

---

## 13. Visual, Audio & Thematic Design

Appearance
- SAXON siege collimator: long shrouded emitter, solar-cell heat sinks, hazard stripes,
  fungal-etched “LONGEST DAY” stencil humor, glowing heat gauges along the receiver
- Beam: thick coherent thermal column; color shifts Cool white-cyan → Warm gold → Hot deep orange-white at Peak
- Ignite: standard Fire plus Solstice-tinted ember motes on beam contact
- Scorch: glassy black-orange floor scars with heat ripple
- Vent: radial heat ring + dust loft
- Supernova: periodic bright node along beam / at impact
- Prism: secondary thinner traces forking from primary

Sound
- Fire start: collimator spool + ignition hiss
- Loop: low roar; pitch/intensity tracks Heat
- Peak: high shimmer overtone (not alarm — soft meter)
- Ignite: crisp catch
- Vent: pressure dump whump
- Supernova: charging tick → concussive glass thump
- Reload: cell clack + coolant sigh (Heat visually drops)

Flavor / codex line (in-game style)
  Thermal Solstice
  Continuous thermal siege beam. Builds soft Heat while firing. Strong native Fire.
  Reactor upgrades spend Heat as power. Conflagration owns wildfire. Optics shapes the beam.

---

## 14. Implementation Notes (for later)

### 14.1 Gear registration
- Follow weapon template in this repo: clone suitable beam-capable base if available,
  else clone a Gun and Harmony-hook continuous fire to match LC feel; GearInfo high-range id,
  APIName `thermal_solstice`, behaviour component, SpawnGear stamp, CreateUpgrade pool.
- Prefer basing visuals/fire on **Laser Cannon** prefab when present in AllGear; fallback any Primary/Heavy Gun.
- Plugin: GUID `sparroh.thermalsolstice`, MycoMod **IsSandbox**.
- Persistence: stable gear id; register before PlayerData.OnAwake AddGear.
- Do **not** remove or patch vanilla Laser Cannon out of AllGear.

### 14.2 Behaviour host
ThermalSolsticeBehaviour (or true Gun subclass when prefab exists):
- WeaponData: heat rates, soft peak mults, fire amounts, path flags, vent stats, pulse stats, prism stats, scorch caps
- Runtime: current Heat, grace timer, capacitor stacks, scorch list, pulse timer, prism targets cache
- Runtime: Criticality self-damage accumulator
- Prefab snapshot restore on upgrade Remove

### 14.3 Heat
- Update on owner while equipped / firing flags from gun state
- Do not drive Heat on non-owners beyond cosmetic if MP requires
- HUD: Heat bar bound to behaviour

### 14.4 Fire
- GunData.damageEffect = Fire; strong effectAmount baseline
- Conflagration cards mutate effectAmount and on-ignite callbacks
- Hook OnSaturateTarget / ignite enter for Solar Authority stacks

### 14.5 Scorch
- Pool decals + trigger volumes or periodic sphere overlap
- Owner-authoritative spawn; duration despawn; max concurrent cap
- Apply Fire amount / light damage on interval to enemies in volume

### 14.6 Vent / RMB priority

1. Emergency Vent — if equipped and Heat ≥ minimum spend threshold
2. Optional Optics prism-paint / pulse-arm (backlog)
3. Optional Conflagration scorch brush (backlog)
4. Else unbound

### 14.7 Hooks

| Hook | Use |
|------|-----|
| Update / gun firing state | Heat build/decay; Criticality self-damage; pulse timer |
| OnFiredBullet / beam tick | Damage riders; Fire confirm; Mag Circulation; Glint |
| OnBeforeDamage | Formicid / Redline / Peak mults; Smog Sight |
| OnDamageTarget | Pilot Light; capacitor stacks; ignite tracking |
| OnSaturateTarget | Solar Authority stacks; Flashpoint |
| OnKillTarget | Wildfire spread; economy crumbs |
| Terrain impact | Micro scorch / Charter scorch seed |
| RMB | Vent priority |

### 14.8 Supernova / Prism
- Pulse: timer while firing; spawn explosion at aim raycast hit or last enemy hit point; spend ammo via gun API
- Prism: on tick, query nearby enemies in forward cone; apply reduced DamageData traces (linecast)

### 14.9 Multiplayer
- Sandbox mod; all clients need the same plugin
- Damage/Fire follow IDamageSource authority
- Heat can be owner-simulated with cosmetic replicate
- Scorch: owner-spawned networked or validated local ghosts — document at impl

### 14.10 VFX / audio priority
1. Beam core + Heat color shift
2. Heat meter UI
3. Ignite catch
4. Peak shimmer (soft, not alarm)
5. Vent ring
6. Supernova pulse
7. Prism side traces
8. Scorch floor

### 14.11 Vanilla LC coexistence
- Different gear id, APIName, display name, upgrade ids
- May share bullet/beam VFX assets initially
- Balance as peer main-battery heavy, not strict stat clone — Heat + paths are the differentiator

---

## 15. Deliberate Non-Goals

- Not replacing or unhooking vanilla Laser Cannon
- Not DMLR dual-mode or Severance anatomy spine
- Not Final Judgement Brand / orbital / 8s charge
- Not hard overheat shutdown baseline
- Not baseline self-damage
- Not Saw-ramp specialist as default identity
- Not walkable hard-light architecture (HLC owns)
- Not Salvo ally heal lock
- Not full Secondary ability path (Capacitor stays thin)
- Not shipping Shock/Acid as equal peer elements in v1 spine (Fire-first)
- Not requiring custom Unity prefab for v1 (runtime clone OK)

---

## 16. Open Tuning Questions (playtest, not design blockers)

1. Heat build 0.35 vs 0.50 /s — time-to-Peak vs mag length.
2. Soft Peak baseline juice: +dmg vs +width vs VFX-only.
3. Criticality self-damage per second band.
4. Vent min Heat threshold and cooldown.
5. Solar Authority stack cap and duration.
6. Scorch max patches and duration.
7. Supernova interval 0.8–1.2s and ammo tax.
8. Prism side-beam damage ratio 40–60%.
9. Whether heavy ammo is mag-only (0 reserve) like some heavies or pooled.
10. Collimated Bore pierce: parts vs full bodies.
11. Secondary Bus conversion rate vs feeling useless solo.
12. Formicid apply penalty depth vs Kindling compensation.
13. Move penalty while firing baseline strength.
14. Name collisions in UI with vanilla “Laser” category filters — ensure API/display distinct.

---

## 17. Success Criteria / Player Fantasy Checklist

- [ ] Hold-M1 continuous beam feels like a complete main-battery heavy with zero upgrades
- [ ] Heat meter is visible and climbs/decays readably (soft — no shutdown)
- [ ] Baseline Fire ignites focused targets without Conflagration
- [ ] Peak state changes beam look/sound; optional tiny juice only
- [ ] Criticality makes hot play greedily powerful and slightly self-harming
- [ ] Emergency Vent spends Heat for a satisfying radial cook
- [ ] Solar Authority rewards ignite chains with obvious stack power
- [ ] Wildfire Charter scorch + spread makes pack cook clips
- [ ] Supernova pulses punctuate long holds with ammo tax
- [ ] Prism splits clear side targets without deleting primary aim
- [ ] Hybrid Criticality + Solar + Supernova feels intentional
- [ ] Vanilla Laser Cannon still exists and is equippable alongside
- [ ] No mode swap, no limb-transfer literacy required
- [ ] SAXON siege-laser tone reads industrial solar, not wizard staff
- [ ] ~30 upgrades, E6 equal large exotics, three peer paths

---

## 18. Locked Review Decisions (2026-08-14)

| Decision | Lock |
|----------|------|
| Form factor | Continuous thermal siege beam heavy |
| Player-facing name | Thermal Solstice |
| Product shape | Parallel new heavy (not LC replace) |
| Slot | Heavy |
| Paths | Reactor / Conflagration / Optics |
| Heat | Soft meter only |
| Baseline Fire | Strong native |
| Main-weapon bias | Pack-consistent hose |
| Dual-mode | No |
| Severance/Brand spine | No |
| Crowns | Criticality, Emergency Vent, Solar Authority, Wildfire Charter, Supernova Cadence, Prism Array |
| Ship pool | Frozen 30 listed above |
| Working APIName | thermal_solstice |
| Working GUID | sparroh.thermalsolstice |
| MycoMod | IsSandbox at implementation |
| Doc file | ThermalSolstice-DesignDoc.txt |
| Tone | SAXON industrial siege laser / solar catalog humor |
| User locks | Direction A; soft heat; strong Fire; hose bias; name Thermal Solstice; no extra sacred cows |

---

## 19. Changelog

v1 (2026-08-14)
- Initial full design from locked user decisions (Act mode handoff)
- Identity: Thermal Lance (A) as parallel heavy **Thermal Solstice**
- Paths: Reactor (heat power + opt-in Criticality risk + Vent), Conflagration (ignite/scorch/Solar), Optics (width/pierce/prism/Supernova)
- Research anchors:
  - Wiki: Laser Cannon stats + Core Dump, Formicidae Magnifier, Heat Pump, Motion Compensators, Prismatic Sharpeners, Scorched Earth, Shared Processing Array, Solar Flare, Supernova, Unruly Crowd
  - Wiki: DMLR dual-mode laser contrast; Hot Swap / Sturdy / Supernova-adjacent laser toys
  - Sibling docs: DMLRRework (Severance contrast), HardLightConstructor (path bible structure), FinalJudgement (parallel heavy + charge contrast), Zephyr (baseline sacred cows)
- User locks: parallel weapon; A; soft meter; strong Fire; hose; name Thermal Solstice; no extra constraints
- Frozen 30 + backlog; E6 crowns; implementation sketch; acceptance tests

---

## 20. Implementation checklist (post-design)

- [ ] Rename plugin/csproj/thunderstore from template → ThermalSolstice
- [ ] ThermalSolsticeBehaviour.Data fields from §14.2
- [ ] Retune cloned GunData (beam tick, Fire, mag, range)
- [ ] Soft Heat build/decay + HUD
- [ ] Baseline Fire strong apply
- [ ] Criticality + Phase Coolant self-damage path
- [ ] Emergency Vent RMB + Heat spend
- [ ] Solar Authority ignite stacks
- [ ] Wildfire Charter scorch pool + spread
- [ ] Supernova Cadence pulse + ammo tax
- [ ] Prism Array side traces
- [ ] UpgradeRegistration frozen 30
- [ ] Persistence + SpawnGear stamp (do not touch vanilla LC)
- [ ] Playtest pass on §10 / §16 knobs

---

End of Design Doc v1
