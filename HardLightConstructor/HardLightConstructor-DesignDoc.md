# Hard-Light Constructor — Design Document (v1)

## 1. High Concept / Fantasy

A mid-range SAXON **hard-light fabricator primary**. Fires slow, chunky plasma slabs on a moderate full-auto cycle.
Plasma paints enemy anatomy with **Shatter** — a real status that seizes mycostructure until full saturation
**Jams** the target (control lock, **no DoT**). Upgrades fork the gun into anatomy seizure, **walkable light
architecture** via a painting system, or **Saxonite Revelry** — shove, bumper, and launch enemies into each other.

Baseline leaves RMB free. Bridger investment unlocks **Paint** (stamp / stroke / gesture panes).
Terrain hits leave tiny non-walkable scorch marks so the hard-light fantasy reads immediately.

One-liner: Paint the joint until it locks. Paint the air until you can stand on it. Bowl the room with bodies.

Product shape: New primary weapon (**Hard-Light Constructor**). Does not replace any vanilla gun or Bruiser ability.

SAXON marketing blurb (draft):
“SAXON HLC-9 Hard-Light Constructor — Field fabricator for hostile architecture and joint seizure.
Walkable projections require Form 12-B authorization. Launching personnel at other personnel is not
covered under standard warranty. (It is covered under Revelry addendum 7.)”

Optional stingers:
- “If the map will not give you high ground, fabricate a policy violation.”
- “Shatter is not damage. Shatter is a scheduling conflict inside the target.”
- “RMB unbound until you earn a brush. That is not a bug. That is licensing.”


## 2. Role & Fantasy in the Arsenal

- Slot: Primary
- Range: Mid (plasma travel time matters; not a sniper, not an SMG hose)
- Role: Gimmick controller / fabricator — jam anatomy, build temporary geometry, bowl packs
- Gap filled:
  - Bruiser Hard-Light Projector = class CD shield (stun/reflect) — not a full upgrade grid gun
  - Plate Launcher = metal plates + magnetic recall + High Ground — scrap physics, not light paint
  - DMLR Hard-Light Bypass / Designator = laser/shell names only — anatomy rifle, not constructor
  - Friend in a Box = discrete deployable ally unit — not walkable architecture
  - Shocklance = close charge poke — no jam + paint + launch trifecta
  - Nothing owns “plasma fabricator → Shatter jam → paint walkable light → yeet enemies into enemies”
- Synergies: Allies dump into jammed targets; co-op bridges; Revelry sets up team focus fire on airborne/pinned

Not trying to be: Bruiser kit replacement, Plate Launcher 2.0, DMLR reskin, Globbler flood, pure DPS hose,
or infinite permanent architecture tool.

### 2.1 Comparison snapshot

```
Weapon / kit              Niche                         Constructor differentiator
------------------------  ----------------------------  ----------------------------------
Bruiser Hard-Light        CD shield stun/reflect        Primary grid; jam + paint + revelry
Plate Launcher            Stick/recall metal plates     Projected light; paint strokes; no recall ID
DMLR                      Anatomy transfer rifle        Jam control + constructs, not transfer DPS
Friend in a Box           Smart deployable unit         Walkable panes / bridges, not AI friend
Shocklance                Charge melee-range poke       Mid plasma + fabricator fantasy
Heaven Piercer            Draw bow pin/bleed/rain       Auto plasma + architecture + bowling
Needle Carbine            Poison needles + extract      Shatter = lock not DoT; paint not consume
Rhythm Stitchers          Dual-trigger sonic tempo      Single projector + geometry toys
```


## 3. Design Pillars

1. On-hit, status, and construct identity > flat % damage stickers.
2. **Shatter is a true EffectType** with full-saturation **Jam** — **no DoT ticks** (control, not cook).
3. Baseline plasma + Shatter apply + terrain scorch juice; **no free walkable paint, no free bowling**.
4. **Paint system** is first-class Bridger fantasy (stamp / stroke / gesture / contextual normals).
5. Three peer paths (Shatter / Bridger / Saxonite Revelry); hybrids intended; no anti-synergy matrix.
6. RMB stays free on baseline; path crowns and Bridger paint claim it via priority table.
7. Reload stays reload — do not overload R on baseline.
8. ~30 upgrades for v1 ship; exotic shapes larger than others; each exotic same cell count.
9. Construct budget is sacred — max panes, max area, charge pool; no map-wide pavement.
10. Boss-safe CC: soft Jam / reduced launch; never permanent stunlock architecture.
11. Fun gimmick first — readable toys, co-op friendly bridges, bowling that feels silly and strong.
12. Industrial SAXON hard-light tone (cyan/white lattice, hazard stripes) — not magic force fields.


## 4. Core Mechanics & Gunfeel

### 4.1 Base gun

| Trait        | Draft / intent                                                    |
|--------------|-------------------------------------------------------------------|
| Fire mode    | Full-auto, moderate RoF, slow chunky plasma projectiles           |
| Damage       | Modest per bolt — value is Shatter apply + upgrade toys           |
| Range        | Mid; projectile travel readable; light falloff OK                 |
| Mag/reserve  | Medium mag; reload beat matters                                   |
| Projectile   | Large hard-light plasma slab/bolt (slow-ish vs SMG darts)         |
| ADS / RMB    | No baseline ADS requirement; **RMB unbound** on baseline          |
| Model/audio  | Projector chassis, lattice emitter, plasma thump, jam crystalline |

Draft firefeel band (VALIDATE IN PLAYTEST):
- RoF mental target: industrial projector, ~3.5–5.5 rps (between deliberate slug and Cycler hose)
- Bullet speed: readable travel (not hitscan; not molasses)
- Mag: ~18–24
- hitForce: modest baseline; Revelry multiplies

### 4.2 Inputs

| Input     | Baseline role                                      | Upgraded claims                                      |
|-----------|----------------------------------------------------|------------------------------------------------------|
| Hold M1   | Full-auto plasma                                   | Resonate on panes/jammed parts; paint reinforce      |
| RMB       | **Unbound**                                        | Bridger Paint / Revelry Launch / other overrides     |
| R         | Reload only                                        | Reload only (no baseline hold-R)                     |
| Heavy     | Normal heavy equip                                 | No baseline heavy link                               |

### 4.3 Baseline combat loop (zero upgrades)

```
M1 plasma into target → Shatter saturation climbs (no DoT)
   → full sat: JAM window (lock / seize) — readable crystalline freeze-frame
   → terrain/prop hits leave MICRO SCORCH (non-walkable juice only)
   → reposition; R when dry
   → RMB does nothing until a path claims it
```

Skill without upgrades: lead slow plasma, focus fire to Jam priority parts/brains, don’t brain-off spray
seven packs half-saturated, use Jam windows for team/self burst — pure gun + status literacy.

### 4.4 What baseline does NOT include

- No walkable panes / bridges / ramps
- No Paint RMB
- No enemy launch bowling
- No prison cubes / full reflect bastions
- No Shatter DoT
- No multi-pane lattice
- No High Ground combat buff from standing on light
- No ADS requirement

Those are path-, exotic-, or unlock-owned.

### 4.5 Baseline juice — micro scorch (Paint unlock package C)

On plasma impact with **terrain / static world** (not enemies):
- Spawn a tiny hard-light **scorch decal / flicker pane** (non-solid or non-walkable)
- Duration short (~0.4–0.8s)
- No collision for standing, no DR, no Resonant payoffs
- Exists so the gun always “writes light” even before Bridger

Sacred cow: scorch is cosmetic/readability, not a stealth Bridger baseline.


## 5. Shared Framework Vocabulary

Upgrades speak these verbs. Baseline owns Plasma / Shatter apply / Jam on full sat / Scorch juice only.

### 5.1 Plasma
- Full-auto moderate projector bolts
- Primary delivery for damage + Shatter amount + hitForce
- Can Resonate when striking owned constructs or qualifying jammed targets (upgrade-owned payoffs)

### 5.2 Shatter (true EffectType) — NO DoT

New status in the vanilla saturation pipeline (same apply/decay rules family as Fire/Acid/Bees/Cryo).

| Tuning (draft)           | Value / intent                                      | Notes |
|--------------------------|-----------------------------------------------------|-------|
| EffectType               | Shatter = free slot (audit vs Poison/Bleed mods)    | Confirm no collision at impl |
| DamageMultiplier         | ×1.0                                                | No innate amp (Decay/Rot stay amps) |
| FullSaturationLifetime    | ~4.0–5.0s                                           | Jam window length band |
| DecayDelay               | 3s                                                  | Match vanilla |
| DecaySpeed               | 0.3 /s                                              | Match vanilla |
| Full-sat behaviour       | **Jam only — NO DoT ticks**                         | LOCKED |
| On full saturation       | Enter Jam state + VFX/audio                         | Locked |
| Verb / UI                | “Shattered” / “Jammed”                              | Cyan crystalline |
| Mesh/VFX                 | Hard-light crack overlay; limb seize flash          | Custom later OK |

ShatterStatusEffect pattern:
- OnFullSaturation → apply Jam behaviour (attack slow / functional seize / accuracy wreck)
- OnFullSaturationUpdate → **maintain Jam only** (refresh VFX, re-assert slow) — **do not DamageTarget DoT**
- On saturation drop below full → Jam ends
- No innate damage amp; no innate heal; no innate pull

Baseline plasma Shatter application (draft):
- effectAmount per bolt ~0.9–1.3 (≈0.09–0.13 sat per hit)
- ~8–12 focused hits to full-sat a fresh target without upgrade help
- Prefer last-hit **part locus** for VFX; saturation bar may be brain-level for UI clarity (DMLR lesson)

### 5.3 Jam
- The full-sat Shatter state: target (or part fantasy) is seized
- Grunts: meaningful lock (wind-up cancel / move slow / attack interrupt band)
- Elites: shorter / softer
- Bosses: soft only (damage taken mult optional via cards; never hard CC forever)
- Revelry **spends or snapshots** Jam for launch payoffs
- Anti-chain: brief per-target Jam reapply grace after hard launch if needed

### 5.4 Paint / Construct
- Bridger-owned system for spawning temporary hard-light geometry
- Unlock: **any first Bridger upgrade** enables basic **Stamp** RMB (package A)
- Advanced brushes (Stroke, gesture ramp, air bridge) are epic/crown-owned
- Constructs use **Construct Charge** pool + duration + max active caps

### 5.5 Lattice
- Linked panes that share buffs, arcs, fence logic, or structural “connected” bonuses
- Not free baseline; Bridger epics/crowns

### 5.6 Impulse
- Push / launch force on enemies (and optional self via bumper toys)
- Baseline hitForce modest; Revelry and bumper panes own the fantasy

### 5.7 Revelry
- Enemy-as-projectile: launched brains deal impact damage to what they hit
- Combo mult when launched target strikes another enemy brain
- Bumper panes redirect without full player aim launch

### 5.8 Resonate
- M1 (or specific cards) hitting **your** pane or a **Jammed** part/brain triggers path payoffs
- Bridger: reinforce duration/HP, shockwave, prism
- Shatter: bonus seize burst, spread
- Revelry: mid-air detonate nudge, redirect

### 5.9 Construct Charge
- Separate small resource for Paint (draft 3–5 max)
- Regen: slow in combat, faster out of combat; optional refunds on pane expire / Revelry kills
- Prevents infinite architecture; mag still gates plasma


## 6. Paint System (Bridger spine — hybrid-friendly)

### 6.1 Unlock ladder (LOCKED: A+C)

| Tier | Source                         | What you get                                      |
|------|--------------------------------|---------------------------------------------------|
| 0    | Baseline                       | Micro scorch only (non-walkable)                  |
| 1    | **Any first Bridger upgrade**  | RMB **Stamp** walkable pane (basic brush)         |
| 2    | Bridger epics / rares          | Larger stamps, duration, charge, High Ground, etc.|
| 3    | Crowns / key epics             | Stroke, gesture ramp, air bridge, bastion cover   |

### 6.2 Construct rules

| Param                 | Draft                         | Intent |
|-----------------------|-------------------------------|--------|
| Max active panes      | 4–6                           | Anti-pave |
| Max total surface m²  | Soft cap / refuse oldest        | Budget |
| Default pane duration | 6–10s                         | Temporary architecture |
| Stamp cost            | 1 Construct Charge            | |
| Stroke cost           | 1 + length crumbs             | Longer bridges cost more |
| Pane HP (if bastion)  | Low unless Bastion cards      | Default = walkable only |
| Ally use              | Allies can stand on your light| Co-op friendly |
| Enemy use             | Enemies can walk too unless card says otherwise | Fair silly default |
| Owner                 | Painter’s gun authority       | MP |

### 6.3 Brush modes

| Mode            | Input (once unlocked)              | Result |
|-----------------|------------------------------------|--------|
| **Stamp**       | RMB tap at aim                     | Single pane on surface or floating disc at aim point |
| **Stroke**      | RMB hold + drag aim                | Extrude bridge/ramp along stroke; costs charge by length |
| **Surface bias**| Stamp on floor / wall / ceiling    | Floor plate / wall cover / canopy orientation |
| **Air bridge**  | Stroke through empty space         | Suspended walkway (needs charge + distance budget) |
| **Gesture ramp**| Stroke with strong upward delta    | Walkable ramp (mobility signature) |
| **Gesture bumper** | Short flick near enemies (Revelry hybrid cards) | Angled bumper that redirects bodies |
| **Resonate**    | M1 hit on your pane                | Reinforce / path payoff |

### 6.4 Gesture classifier (implementation-facing)

While RMB held (Paint mode active):
1. Sample aim point + surface normal each tick
2. Accumulate aim delta in player-local space
3. On release:
   - **Tap** (short hold, low delta) → Stamp
   - **Horizontal dominant delta** → bridge / fence stroke
   - **Vertical dominant delta** → ramp / pillar stroke
   - Optional: flick speed above threshold near enemy → bumper stamp (if Revelry bumper cards)
4. Soft snap orientations to “useful” angles (flat floor, 30–45° ramp notches) so it feels toy-like, not CAD
5. Reject strokes that exceed max length; spawn partial valid segment + whiff feedback

### 6.5 Standing on light (High Ground DNA — upgrade-owned combat buffs)

- Collision: solid walkable collider on Stamp+ panes
- Baseline Bridger stamp: walkable only (no free DR/damage)
- **Scaffold Privilege** / High Ground analogue cards: DR and/or outgoing damage while feet on **your** pane
- Distinct from Plate Launcher High Ground name — use original player-facing names (e.g. Scaffold Privilege)

### 6.6 What Paint is NOT

- Not Plate Launcher magnetic recall
- Not Bruiser Hard-Light Prison full immune cube by default
- Not permanent map editing
- Not baseline RMB


## 7. Upgrade Paths (gravity wells — hybrids intended)

### Path A — SHATTER (anatomy jam / seize)
“Lock the joint. Then break the light.”

- Spine: +Shatter per hit, multi-part seize VFX, jam payoffs, spread on part interaction, Resonate jammed parts,
  shorter time-to-Jam, boss-safe amp windows while Jammed, crystalline bursts **without DoT identity**
- Clear vs ST: ST native via focus Jam; clear via spread / detonate-on-jam cards
- Hybrid hooks: Jammed targets are better launch fuel (Revelry); panes that apply Shatter on touch (Bridger)

### Path B — BRIDGER (walkable hard-light / Paint)
“If the map won’t give you high ground, fabricate it.”

- Spine: Stamp unlock on first card, pane size/duration/charges, Stroke/gestures, Lattice, Scaffold Privilege,
  Resonate reinforce, bastion cover-lite, ally-friendly floors, mobility ramps
- Clear vs ST: mobility + angles + pane shockwaves; ST via elevated safe lanes and Resonant setup
- Hybrid hooks: bumper panes for Revelry; panes apply minor Shatter amount on enemy touch

### Path C — SAXONITE REVELRY (position control + bowling)
“The enemy is ammunition.”

- Spine: hitForce, bumper constructs, launch jammed targets along aim, impact damage on collision with
  enemies/terrain, pin-lite windows, anti-grav toys, Ballroom redirect, airborne Resonate
- Clear vs ST: clear native (bowling packs); ST via pin + team dump into seized elite
- Hybrid hooks: wants Shatter Jam as launch prerequisite quality; wants Bridger bumpers/ramps as rails

### Path × verb matrix

```
                 SHATTER                 BRIDGER                    REVELRY
Plasma           delivers Shatter        Resonates panes            delivers Impulse
Jam              core fantasy            optional pane-touch        launch prerequisite
Paint/Construct  jam-field toys          core fantasy               bumpers / rails
Impulse          shatter knock burst     step boost / lift          core fantasy
Resonate         shoot jammed part       shoot pane                 shoot airborne / bumper
RMB claim        rare seize tools        Paint brushes              Launch (priority when valid)
```


## 8. Crowns & Sacred Cows

### Seizure Lattice — Exotic (Shatter crown)
- While an enemy is Jammed, periodically pulse a soft seize hitch to nearby parts on the same brain
  and/or nearby enemies (draft pulse 1.0–1.25s, modest radius).
- Does not deal DoT — control spread only (tiny damage optional chip — prefer **no damage**, pure CC hitch).
- Readable lattice VFX between seized joints.

### Catastrophic Unbind — Exotic (Shatter crown)
- On reaching Jam (full Shatter sat), emit a crystalline burst:
  - Bonus burst damage once (not a DoT)
  - Spreads Shatter amount to nearby enemies
  - Brief bonus vulnerability window on primary target
- The “break the light” payoff card — still not a damage DoT status.

### Cantilever Doctrine — Exotic (Bridger crown)
- Unlocks / empowers **Stroke** painting: hold RMB and drag to extrude bridges and ramps.
- +Construct Charge capacity; strokes cost length crumbs instead of full stamps per segment.
- Soft snap ramps; max stroke length enforced.
- Mode-defining fabricator exotic.

### Bastion Pane — Exotic (Bridger crown)
- Your panes gain HP and block a portion of enemy projectiles (cover-lite).
- Not full Hard-Light Prison immunity cube; large melee can still threaten through edges.
- Allies behind/on bastion gain minor DR crumb (optional — tune so Bruiser shield stays king of true shield).
- Resonate M1 can repair pane HP.

### Kinetic Pageant — Exotic (Revelry crown)
- RMB (when valid): **Launch** a Jammed non-boss enemy along aim (or camera forward).
- Impact damage ∝ launch speed × remaining Jam quality / Shatter sat snapshot.
- On hitting another enemy: Revelry collision damage + small Shatter splash.
- Bosses: heavy shove / stagger only, no full yeet (or heavily reduced).
- Cooldown per target to prevent infinite pinball stunlock.
- RMB priority: valid Launch target beats Paint Stamp (see §12.7).

### Ballroom Protocol — Exotic (Revelry crown)
- Your constructs (and/or short-lived bumper stamps) act as **redirect pads**:
  enemies that touch them inherit a velocity kick along pane normal.
- Optional: Jammed enemies receive stronger redirect.
- Encourages building bowling alleys and ramps into pits/walls/packs.
- Works with Bridger panes; if no panes, grants a limited bumper-stamp RMB fallback (weaker than full Paint).

Sacred cows (do not cut without rewriting identity):
- Baseline full-auto moderate plasma + Shatter apply
- Shatter EffectType with **Jam and no DoT**
- Baseline RMB unbound
- Baseline micro scorch (non-walkable)
- First Bridger upgrade unlocks Stamp Paint
- Construct charge budget / max panes
- Three peer paths; hybrids OK
- Revelry bowling is upgrade-owned
- Not Plate recall; not Bruiser kit replace


## 9. Full Upgrade List (~30 ship + backlog)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity  
Cell rule: Exotic shapes larger than others; all Exotics same cell count.  
Player-facing names below. API names assigned at implementation.

------------------------------------------------------------------------------
PATH A — SHATTER
------------------------------------------------------------------------------

A-EX1. Seizure Lattice — Exotic (crown)
       Jammed targets pulse soft seize hitches along a lattice (no DoT).

A-EX2. Catastrophic Unbind — Exotic (crown)
       On Jam enter: crystalline burst damage + Shatter spread + brief vuln window.

A-EP1. Joint Compiler — Epic
       +Shatter effectAmount per plasma hit. Faster time-to-Jam.

A-EP2. Fault Injection — Epic
       Every Nth hit on the same part applies bonus Shatter and a micro hitch (pre-Jam tease).

A-EP3. Sympathetic Seize — Epic
       Damaging a Jammed target applies small Shatter amount to one nearby enemy.

A-EP4. Core Lock Bias — Epic
       Bonus Shatter amount and modest damage vs cores/shells while target is Jammed.

A-RA1. Refractive Tips — Rare
       Plasma applies bonus Shatter when hitting limbs (part-bias).

A-RA2. Brittle Frame — Rare
       Jammed targets take increased direct damage (not DoT). Duration of Jam slightly reduced.

A-RA3. Echo Fracture — Rare
       On Jam expire, small one-shot shatter chip in tiny radius (not a status DoT).

A-RA4. Priority Thread — Rare
       +Damage to Jammed targets. Slight −damage to non-Jammed (focus tax).

A-ST1. Primer Coat — Standard
       Minor +Shatter effectAmount.

------------------------------------------------------------------------------
PATH B — BRIDGER
------------------------------------------------------------------------------

B-EX1. Cantilever Doctrine — Exotic (crown)
       Stroke paint bridges/ramps; construct charge capacity; fabricator mode.

B-EX2. Bastion Pane — Exotic (crown)
       Panes gain HP / partial projectile block; Resonate repairs.

B-EP1. Scaffold Privilege — Epic
       While standing on your pane: +outgoing damage and +DR (High Ground analogue, original name).

B-EP2. Wide Stamp — Epic
       +Pane size; Stamp costs +0 or slight charge tax (tune).

B-EP3. Lattice Bond — Epic
       Panes near each other link: shared duration refresh crumb; optional light arc between links.

B-EP4. Resonant Keystone — Epic
       M1 hits on your panes reinforce duration and emit a short shockwave (push, modest damage).

B-RA1. Quickset Slurry — Rare
       +Pane duration.

B-RA2. Spare Projectors — Rare
       +Construct Charge capacity and/or regen.

B-RA3. Friction Lattice — Rare
       Enemies walking on your panes are slowed (soft).

B-RA4. Climbing Warrant — Rare
       Ramps/stamps slightly steeper snap + minor move speed while on your light.

B-ST1. Survey Mark — Standard
       First Bridger card tier: enables Stamp if not yet enabled; minor +pane duration crumb.
       (Any Bridger card enables Stamp — Survey Mark is the “cheap key” standard.)

Note: **Survey Mark** and all other Bridger cards share the rule: on Apply, set PaintUnlocked Stamp.

------------------------------------------------------------------------------
PATH C — SAXONITE REVELRY
------------------------------------------------------------------------------

C-EX1. Kinetic Pageant — Exotic (crown)
       RMB Launch Jammed grunts/elites along aim; collision payoffs; boss soft shove.

C-EX2. Ballroom Protocol — Exotic (crown)
       Panes/bumpers redirect enemies; bowling alley fantasy.

C-EP1. Closing Ceremonies — Epic
       Launched enemies that impact another brain apply Shatter amount and bonus collision damage.

C-EP2. Wallflower Tax — Epic
       Enemies slammed into terrain take bonus impact damage; brief daze.

C-EP3. Dance Card — Epic
       While a target is Jammed, your plasma gains +hitForce and slight pull-to-crosshair (soft).

C-EP4. Encore Pin — Epic
       Launch impact briefly soft-pins non-bosses (short). Weaker than full Heaven Piercer pin fantasy.

C-RA1. Mass Driver Rails — Rare
       +hitForce on all plasma.

C-RA2. Trampoline Edict — Rare
       Standing on your pane and jumping gains vertical boost; enemies landing on pane bounce lightly.

C-RA3. Crowd Surf — Rare
       Shockwave-on-pane-touch for enemies (small push). Requires any pane (Bridger hybrid bait).

C-RA4. Pageant Primer — Rare
       +Shatter amount on the first plasma hit against a target you recently launched (re-arm loop).

C-ST1. Etiquette Breach — Standard
       Minor +hitForce.

------------------------------------------------------------------------------
GENERIC / GUNFEEL
------------------------------------------------------------------------------

G-RA1. Projector Feed — Rare
       +Fire rate, −per-bolt damage slightly.

G-RA2. Heavy Slabs — Rare
       +Damage, +projectile size, −fire rate slightly, +hitForce crumb.

G-RA3. Deep Cell — Rare
       +Magazine size, −reload speed slightly.

G-ST1. Field Calibrator — Standard
       +Reload speed.

G-ST2. Reserve Lattice — Standard
       +Ammo reserves.

G-ST3. Collimator — Standard
       −Spread / tighter plasma grouping.

G-ST4. Mid-Range Lens — Standard
       +Effective range / bullet speed crumb.

G-OD1. Boundary Incursion — Oddity
       Increases upgrade grid size.

------------------------------------------------------------------------------
FROZEN 30 FOR V1 SHIP
------------------------------------------------------------------------------

EXOTIC (6)
  1  Seizure Lattice
  2  Catastrophic Unbind
  3  Cantilever Doctrine
  4  Bastion Pane
  5  Kinetic Pageant
  6  Ballroom Protocol

EPIC (8)
  7  Joint Compiler
  8  Fault Injection
  9  Scaffold Privilege
 10  Resonant Keystone
 11  Closing Ceremonies
 12  Wallflower Tax
 13  Dance Card
 14  Wide Stamp

RARE (10)
 15  Refractive Tips
 16  Brittle Frame
 17  Quickset Slurry
 18  Spare Projectors
 19  Friction Lattice
 20  Mass Driver Rails
 21  Trampoline Edict
 22  Projector Feed
 23  Heavy Slabs
 24  Deep Cell

STANDARD (5)
 25  Primer Coat
 26  Survey Mark
 27  Etiquette Breach
 28  Field Calibrator
 29  Reserve Lattice

ODDITY (1)
 30  Boundary Incursion

------------------------------------------------------------------------------
BACKLOG (designed, not in first 30)
------------------------------------------------------------------------------

Shatter path
- Sympathetic Seize
- Core Lock Bias
- Echo Fracture
- Priority Thread
- Part-local Jam overlays (true per-part saturation if brain-level proves too blunt)
- Shatter amount on pane touch (Bridger hybrid card)
- Anti-regrow seize (Decay-adjacent — careful)

Bridger path
- Lattice Bond
- Climbing Warrant
- Air Bridge License (explicit long air strokes)
- Multi-Stamp burst (spend 2 charges → 3 small panes)
- Pane mine (expire explosion — keep off Globbler flood)
- Ally-only floors (enemies fall through — spicy, MP edge cases)
- Mobile pane follow (slow hover disc under feet — antigrav cousin)
- Full Prison-lite cube exotic (explicit non-goal for v1; backlog only if distinct from Bruiser)

Revelry path
- Encore Pin
- Crowd Surf
- Pageant Primer
- Anti-Grav Ballroom (AntigravityField DNA bubble)
- Self-launch off bumper (Backboost cousin — friendly fire rules)
- Chain Revelry (collision can re-launch once at reduced force)
- Boss yeet toggle (probably never)

Generic
- Collimator, Mid-Range Lens
- Infinite Projector (Infinity Burn DNA: ammo infinite, self-Shatter risk — funny, backlog)
- True ADS scope exotic
- Element coating on panes (Hard-Light Coating cousin — Fire/Shock/Acid on touch)


## 10. Example Builds

Jam surgeon (Shatter ST)
  Seizure Lattice + Catastrophic Unbind + Joint Compiler + Fault Injection
  + Brittle Frame + Refractive Tips + Primer Coat
  Focus fire, Jam, burst the unbind, lattice the pack’s joints.

Scaffold sniper-lite (Bridger)
  Cantilever Doctrine + Bastion Pane + Scaffold Privilege + Wide Stamp
  + Resonant Keystone + Quickset + Spare Projectors + Heavy Slabs
  Paint a ramp, stand on privilege, resonate panes, mid-range delete.

Bowling commissioner (Revelry)
  Kinetic Pageant + Ballroom Protocol + Closing Ceremonies + Wallflower Tax
  + Dance Card + Mass Driver Rails + Etiquette Breach
  Jam → Launch → chain collisions; bumpers keep the dance floor honest.

Hybrid freak (recommended showcase)
  Catastrophic Unbind + Cantilever Doctrine + Kinetic Pageant
  + Scaffold Privilege + Closing Ceremonies + Joint Compiler
  Build the alley, jam the pin, launch the ball — no artificial brakes.

Mobile bastion (co-op)
  Bastion Pane + Lattice Bond (backlog) / Wide Stamp + Scaffold Privilege
  + Friction Lattice + Survey Mark + Spare Projectors
  Ally bridge + cover-lite; you paint, they shoot.


## 11. Economy & Tuning Rules of Thumb

- Power budget lives in Jam uptime, construct utility, and Revelry collisions — not raw RoF DPS.
- Plasma per-bolt damage stays modest; Heavy Slabs is the chunky fork, not the baseline.
- Time-to-Jam ~8–12 baseline hits; Joint Compiler should feel like a real investment, not instant CC.
- **No Shatter DoT** — if Jam feels weak, tune lock quality / Brittle Frame / Unbind burst, not ticks.
- Construct Charge 3–5 and max panes 4–6: if players pave missions, cut duration or raise cost.
- Stamp must feel good with only Survey Mark; Stroke remains crown candy.
- Scaffold Privilege must not exceed “stand on Plate High Ground” comfort without Bastion investment.
- Kinetic Pageant launch CD per target; bosses soft-only.
- Watch stacked CC: Jam + Friction + Encore Pin + Dance Card pull — prefer diminishing move mult floors.
- Watch hybrid delete: Unbind burst + Launch collision + Resonant shockwave — fun, not map wipe.
- Scorch must never become walkable via bug; treat as separate non-solid VFX pool.


## 12. Status & Counter Split (explicit)

| Status / counter   | Role on this gun                         | Baseline? | DoT? |
|--------------------|------------------------------------------|-----------|------|
| Shatter EffectType | Spine seize → Jam at full sat            | Yes       | **No** |
| Jam                | Full-sat state (control lock)            | Yes       | No   |
| Construct panes    | Walkable geometry counter/list           | Bridger   | N/A  |
| Construct Charge   | Paint resource                           | Bridger   | N/A  |
| Micro scorch       | Non-walkable juice                       | Yes       | N/A  |
| Fire/Shock/Acid    | Optional pane coating backlog            | Backlog   | —    |
| Cryo               | Not identity (slow via Jam/Friction)     | No        | —    |
| Poison/Bleed/Bees  | Not identity                             | No        | —    |
| Decay/Rot          | Amp backlog only                         | Backlog   | —    |

### 12.1 Shatter EffectType (draft — mirror Needle/Bleed injection, different payoff)

| Tuning (draft)           | Value         | Notes |
|--------------------------|---------------|-------|
| EffectType               | Shatter = N   | Free slot after audit (Poison/Bleed mods may claim 11+) |
| DamageMultiplier         | ×1.0          | |
| FullSaturationLifetime    | 4.5s          | Jam window |
| DecayDelay               | 3s            | |
| DecaySpeed               | 0.3 /s        | |
| Full-sat DoT             | **None**      | LOCKED |
| Full-sat Jam (grunt)     | Strong soft CC| Attack rate / move / interrupt band |
| Full-sat Jam (elite)     | Reduced       | |
| Full-sat Jam (boss)      | Soft only     | Optional +damage taken card-owned |
| Verb                     | Shattered     | Cyan / white lattice crack |
| Player self-Shatter      | Not baseline  | No Own-Medicine path in v1 |

Jam implementation sketch (not DoT):
- OnFullSaturation: set Jam flag, play seize VFX, apply move mult + attack slow via existing slow/stun hooks
- OnFullSaturationUpdate: refresh Jam modifiers only
- On leave full sat: clear Jam


## 13. Implementation Notes

### 13.1 Gear registration
- Follow weapon template in this repo: clone base gun, GearInfo high-range id, APIName `hard_light_constructor`,
  behaviour component, SpawnGear stamp, CreateUpgrade pool.
- Prefer projectile Gun in AllGear (CartridgeSMG acceptable visually until custom art; retune GunData heavily).
- Plugin: GUID `sparroh.hardlightweapon` (or `sparroh.hardlightconstructor`), MycoMod **IsSandbox**.
- Persistence: stable gear id; register before PlayerData.OnAwake AddGear.

### 13.2 Behaviour host
HardLightConstructorBehaviour (or true Gun subclass when prefab exists):
- WeaponData: shatter amounts, jam rules, paint flags, construct caps, impulse mults, resonate flags
- Runtime: construct list (pane ids, colliders, HP, owner, links)
- Runtime: Construct Charge current/max/regen
- Runtime: Paint state machine (idle / stamp / stroke sampling)
- Runtime: launch cooldowns per target instance id
- Prefab snapshot restore on upgrade Remove

### 13.3 Shatter EffectType (phased — copy Needle approach, skip DoT)

Phase 0 — Vertical slice without enum
  - Behaviour-side Shatter sat tracker + Jam flag for feel prototype

Phase 1 — EffectType + class
  - Add EffectType.Shatter = free slot
  - ShatterStatusEffect : StatusEffect
  - OnFullSaturation / Update → Jam only (**no DamageTarget DoT**)
  - Patch StatusEffectManager.CreateEffect switch
  - Verify effectPool sizing from enum

Phase 2 — StatusEffectData
  - Inject/clone into Global effect table (verb, colors, materials, audio)

Phase 3 — Gun wiring
  - GunData.damageEffect = Shatter (or Normal damage + separate Shatter apply on hit)
  - Prefer: Normal (or light) hit damage + explicit Shatter apply amount so Brittle/Unbind stay readable
  - Upgrades mutate apply amounts and Jam quality

Phase 4 — Path payoffs
  - Seizure Lattice pulses, Unbind burst, Launch snapshot sat, pane-touch apply (backlog)

Fallback if EffectType injection blocked:
  - First-class custom Shatter tracker with Jam; design identity unchanged; debt in changelog

### 13.4 Paint / constructs
- Pool simple pane prefabs: quad mesh + box collider + optional bastion HP component
- Stamp: raycast aim → place on surface with normal alignment, or floating if air-stamp allowed
- Stroke: while hold, sample points, spawn segmented panes or stretch a single bridge mesh
- Gesture classifier on RMB release (§6.4)
- Walking: standard Unity collider; ensure player movement accepts temporary colliders
- AntigravityField / IGravityModifier only for backlog anti-grav ballroom cards
- Expire: fade VFX → disable collider → pool release
- Max panes: despawn oldest on overflow

### 13.5 Launch / impulse (Revelry)
- On Kinetic Pageant RMB: validate Jammed non-boss target in aim cone/ray
- Apply velocity impulse along aim (enemy movement API / hitForce patterns from decompile)
- Track “revelry projectile” state briefly; on collision with enemy/terrain, deal impact DamageData
- Closing Ceremonies adds Shatter apply on enemy-enemy impact
- Boss path: reduced impulse only
- Reference DNA: GunData.hitForce, BarrelLaunchPad impulse feel, Bruiser Force Projection push

### 13.6 Hooks

| Hook              | Use |
|-------------------|-----|
| OnFiredBullet     | Plasma VFX size; optional resonate prep |
| OnBeforeDamage    | Brittle Frame mult, jammed damage bias, precision flags |
| OnDamageTarget    | Shatter apply confirm; Fault Injection counters; Pageant primer |
| OnSaturateTarget  | Jam enter → Unbind, Lattice registration, launch eligibility |
| OnKillTarget      | Charge refunds; construct refunds; backlog |
| Terrain impact    | Micro scorch spawn |
| RMB press/hold    | Paint vs Launch priority |
| Player ground check | Scaffold Privilege detection (feet on owned pane) |

### 13.7 RMB priority

1. Kinetic Pageant Launch — if equipped and valid Jammed launch target under aim
2. Ballroom bumper-stamp fallback — if equipped and no Bridger Paint and flick context (optional)
3. Bridger Paint (Stroke if Cantilever + hold; else Stamp) — if PaintUnlocked
4. Future path overrides
5. Else unbound (baseline)

### 13.8 HUD
- Shatter sat fragment on aimed target + Jam icon when full
- Construct Charge pips
- Paint mode glyph when Bridger unlocked (stamp vs stroke)
- Launch reticle tick when Pageant has valid target
- Prefer SparrohUILib if dependency acceptable; else minimal

### 13.9 Multiplayer
- Sandbox mod; all clients need the same plugin
- Status application follows IDamageSource authority
- Constructs: owner-authoritative spawn; colliders replicated or locally spawned with state sync strategy
  (document chosen approach at impl — prefer simple owner-spawned networked object if NGO allows;
   else client ghosts with authority validation on stand/buffs)
- Launch: authority validates Jam + applies impulse server-side when required

### 13.10 VFX / audio priority
1. Plasma slab flight + impact thump
2. Shatter sat build shimmer → Jam crystalline lock stinger
3. Micro scorch flicker on terrain
4. Pane spawn hum + footstep-on-light tick
5. Stroke extrude beam
6. Bastion hit sparks
7. Launch whoosh + bowling impact brass sting
8. Unbind burst crack (one-shot, not DoT loop)


## 14. Deliberate Non-Goals

- Not replacing Bruiser Hard-Light Projector fantasy as a class identity
- Not Plate Launcher magnetic recall / catch economy
- Not DMLR anatomy transfer DPS rifle
- Not Shatter DoT (LOCKED)
- Not baseline walkable Paint or baseline Launch
- Not infinite permanent architecture
- Not full Hard-Light Prison immune cubes in v1 ship pool
- Not baseline RMB power
- Not Own-Medicine self-Shatter path in v1
- Not requiring custom Unity prefab for v1 (runtime clone OK)
- Not team-hostile floors by default
- Not shipping Decay/Rot apply in first 30


## 15. Open Tuning Questions (playtest, not design blockers)

1. Plasma RoF 3.5 vs 5.5 rps vs mag size feel.
2. Bullet speed — how much lead skill is fun mid-fight.
3. Time-to-Jam 8–12 hits vs mission density.
4. Jam strength on grunts vs elite frustration.
5. Construct Charge 3 vs 5; pane duration 6 vs 10s.
6. Max panes 4 vs 6.
7. Scaffold Privilege numbers vs Plate High Ground comfort.
8. Bastion block % — must stay below true Bruiser shield.
9. Launch force and collision damage vs pack wipe risk.
10. Stroke length economy under Cantilever.
11. EffectType slot after other Sparroh status mods (Poison/Bleed).
12. Whether damage stays Normal + separate Shatter apply vs damageEffect = Shatter.
13. Enemy walk-on-pane default: keep fair (yes) or ally-only backlog sooner.
14. Gesture classifier false positives (ramp vs bridge) — snap notches.


## 16. Success Criteria / Player Fantasy Checklist

- [ ] Moderate full-auto plasma feels chunky and readable with zero upgrades
- [ ] Shatter builds to Jam with **no DoT ticks**; Jam is obvious (VFX + enemy behaviour)
- [ ] Terrain hits scorch lightly; scorch is not walkable
- [ ] RMB does nothing on baseline
- [ ] First Bridger card (e.g. Survey Mark) enables Stamp walkable panes
- [ ] Cantilever Stroke paints a bridge/ramp that you can run across
- [ ] Scaffold Privilege makes “my floor” combat-relevant
- [ ] Bastion Pane blocks some junk fire without deleting Bruiser
- [ ] Seizure Lattice spreads control without becoming a damage aura DoT
- [ ] Catastrophic Unbind is a one-shot burst payoff on Jam enter
- [ ] Kinetic Pageant launches a jammed grunt into a pack for silly damage
- [ ] Ballroom bumpers redirect bodies along painted rails
- [ ] Hybrid Unbind + Cantilever + Pageant feels intentional
- [ ] Construct caps prevent mission-wide pavement
- [ ] Bosses never hard-stunlock via Jam + Launch
- [ ] SAXON hard-light audio/VFX read industrial cyan lattice, not magic purple wizard


## 17. Strengths, Weaknesses & Co-op

Strengths
- Unique fabricator + jam + bowling fantasy
- Real Shatter status for systemic interactions (without DoT overlap wars)
- Deep hybrid space (build alley → jam pin → launch ball)
- Co-op bridges and shared high ground
- High toy density / clip potential

Weaknesses
- Low brain-off pure DPS
- Projectile travel + moderate RoF lose to panic SMG close range without practice
- Paint skill ceiling (bad stamps waste charge)
- Setup time vs hyper-aggressive rush waves
- CC limited on bosses by design

Co-op
- You Jam and Launch; allies dump damage into seized/airborne targets
- Bridges are gifts — default enemy-can-walk keeps it fair and funny
- Avoid team-hostile pane rules unless a future exotic opts in
- Bastion is cover assist, not a second Bruiser ultimate


## 18. Visual, Audio & Thematic Design

Appearance
- SAXON industrial projector rifle: lattice emitter muzzle, cyan-white hard-light rails,
  hazard stripes, battery magazine as “phase cell,” fungal-etched unauthorized-modification stickers
- Plasma: slow rectangular/hex slab bolts with light-trail
- Jam: target gains cracked glass hard-light shell / seized joint chevrons
- Panes: translucent cyan walkways with hex grid; bastion thickens edge brightness
- Launch: body trails hard-light motion lines; impact flashes brass + cyan

Sound
- Fire: heavy projector thump (not SMG chatter)
- Flight: low whoose ∝ slab size
- Shatter build: rising crystalline ticks
- Jam: lock clack + short harmonic hold (no damage sizzle loop)
- Scorch: tiny glass sand tick
- Pane place: construction hum + magnet set
- Stroke: continuous fabricator beam
- Launch: brass fanfare sting (Revelry comedy allowed)
- Unbind: single glass-break burst

Flavor / codex line (in-game style)
  Hard-Light Constructor
  Full-auto hard-light plasma projector. Applies Shatter; full saturation Jams (no DoT).
  Bridger upgrades unlock walkable Paint. Revelry launches jammed targets. RMB free until claimed.


## 19. Locked Review Decisions (2026-08-06)

| Decision              | Lock |
|-----------------------|------|
| Form factor           | Hard-light plasma projector primary |
| Player-facing name    | Hard-Light Constructor |
| Slot                  | Primary |
| Paths                 | Shatter / Bridger / Saxonite Revelry |
| Fire mode             | Full-auto, moderate RoF, slow plasma projectiles |
| Baseline RMB          | Unbound |
| Paint unlock          | A+C: micro scorch baseline; first Bridger card enables Stamp |
| Shatter model         | True EffectType.Shatter |
| Shatter DoT           | **None** (Jam control only) |
| Paint system          | Stamp / stroke / gesture / contextual normals |
| Construct budget      | Charge pool + max panes + duration |
| Plate Launcher clone  | No (no recall identity) |
| Bruiser replace       | No |
| Ship pool             | Frozen 30 listed above |
| Crowns                | Seizure Lattice, Catastrophic Unbind, Cantilever Doctrine, Bastion Pane, Kinetic Pageant, Ballroom Protocol |
| MycoMod flag          | IsSandbox at implementation |
| Working APIName       | hard_light_constructor |
| Doc file              | HardLightWeapon-DesignDoc.txt (this file) |
| Tone                  | SAXON industrial hard-light |
| Gimmick priority      | Fun / out-of-the-box over serious meta DPS |


## 20. Changelog

v1 (2026-08-06)
- Initial full design from locked user decisions
- Paths: Shatter (jam parts / anatomy seize), Bridger (walkable paint), Saxonite Revelry (position + launch)
- Research anchors:
  - Bruiser Hard-Light Projector / Prison / Domed / Modular Construction / Force Projection / Coating / Backboost (wiki)
  - Plate Launcher: High Ground, Shockwave, Vector Interdiction, Fencing Construction (wiki) — contrast only
  - Hard-Light Bypass / Designator naming collision awareness (DMLR)
  - Decompile: BruiserShield, AntigravityField, BarrelLaunchPad, hitForce patterns
  - Sibling docs: Heaven Piercer (pin/launch RMB priority), Needle (EffectType phasing, no-DoT divergence),
    DMLR (part literacy), FriendinaBox (deployables), Honey Jar (field control toys)
- User locks: name Constructor; auto moderate plasma; RMB unbound; EffectType Shatter; full bible;
  painting system; unlock A+C; **no DoT on Shatter**


## 21. Implementation checklist (post-design)

- [ ] Rename plugin/csproj/thunderstore from template → HardLightConstructor / HardLightWeapon
- [ ] HardLightConstructorBehaviour.Data fields from §13.2
- [ ] Retune cloned GunData (RoF, projectile speed, mag, hitForce)
- [ ] Shatter EffectType injection phases (Jam, no DoT)
- [ ] Micro scorch on terrain impact
- [ ] Paint unlock on first Bridger Apply; Stamp spawn pool
- [ ] Cantilever Stroke + gesture classifier
- [ ] Bastion HP + projectile block
- [ ] Scaffold Privilege feet-on-pane check
- [ ] Kinetic Pageant Launch + collision damage
- [ ] Ballroom redirect triggers
- [ ] Seizure Lattice + Catastrophic Unbind
- [ ] RMB priority table
- [ ] UpgradeRegistration frozen 30
- [ ] HUD: Shatter/Jam, Construct Charge, Paint mode
- [ ] Persistence + SpawnGear stamp
- [ ] Playtest pass on §15 knobs
