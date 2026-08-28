# Zephyr — Design Document (v1)

## 1. High Concept / Fantasy

A short–mid range SAXON industrial **sonic overpressure cannon**. Each trigger pull dumps a compressed pressure front in a cone: damage, stagger, and hard knockback. Baseline is Thundergun-true — slow cycle, fat cone, scarce shots, pack-delete fantasy.

Upgrades fork the weapon into pure lane-clear overpressure, gravity-well stack-and-collapse control, or harmonic resonance that cracks limbs and shells.

One-liner: Point at the swarm. Pull the trigger. The air does the rest.

Product shape: New primary weapon (**Zephyr** / SAXON catalog tone). Does not replace any vanilla gun.

Working APIName: `zephyr`
Working display: **Zephyr**
Catalog blurb tone: industrial acoustic weapon, not COD prop copy.


## 2. Role & Fantasy in the Arsenal

- Slot: **Primary**
- Range: Short–Mid cone (centerline strong; edges shove and wound)
- Role: Panic clear / space creation with optional control and anatomy forks
- Loop: Aim cone → blast → reposition while the chamber recycles
- Gap filled:
  - Globbler = acid puddle denial (not knockback)
  - Swarm Launcher = hover-dive hose (not pressure cone)
  - Plate Launcher = stick / magnetic recall (not sonic front)
  - Shocklance = close pierce coil + optional Auger dash
  - DMLR = anatomy rifle (precision, not blast CC)
  - Heaven Piercer = draw skill projectile
  - Nothing owns **forward sonic overpressure** as a primary identity
- Synergies: Allies dump into wells or cracked parts; you peel packs off objectives; co-op “stack then collapse” moments

Not trying to be: Globbler flood, Plate stick/recall, Shocklance poke, generic pellet shotgun, full second DMLR.


## 3. Design Pillars

1. Baseline blast identity is sacred — zero upgrades still feels like a complete Thundergun-class gun.
2. Instant semi blast (no charge gate on baseline). Charge superblast is exotic-owned.
3. On-hit / spatial verbs > flat % damage stickers.
4. Three peer paths (Overpressure / Singularity / Resonance); hybrids intended; no anti-synergy matrix.
5. Gravity wells and limb sever are **opt-in** — not free on baseline.
6. Reuse Severance-style part typing as a **library** for Resonance; do not fork a second full DMLR transfer fantasy.
7. ~30 upgrades for v1 ship; exotic shapes larger than others; each exotic same cell count.
8. RMB stays free on baseline for path overrides; R stays reload only.
9. Ammo anxiety is part of the fantasy — every shot is a decision.
10. Industrial SAXON + fungal-resonant aesthetic — pressure rings, not cartoon lightning COD clone.
11. Ally force mult << enemy force (no team-yeet off map).
12. Bosses resist full launch; still take solid body damage and path payoffs.


## 4. Core Mechanics & Gunfeel

### 4.1 Base gun

| Trait        | Draft / intent                                                    |
|--------------|-------------------------------------------------------------------|
| Fire mode    | Instant semi pressure blast (classic Thundergun)                  |
| Damage       | High centerline; falloff by distance **and** angle from aim axis  |
| Force        | High `hitForce` along blast normal; edges launch more than melt   |
| Range        | Short–mid cone / frustum (draft 12–18 m effective)                |
| Mag/reserve  | **Mag 3** (playtest 2–4); modest reserve (draft 12–18)            |
| Cycle        | Slow fire interval (draft 0.55–0.75 s) + deliberate reload        |
| Projectile   | Prefer **wave volume** (cone overlap / fast invisible front), not pellet buckshot |
| ADS / RMB    | No baseline ADS requirement; RMB reserved for path overrides      |
| Model/audio  | Industrial resonator horn / coil stack; bass thump; dust ring VFX |

### 4.2 Inputs

| Input     | Role |
|-----------|------|
| M1        | Fire pressure blast (instant) |
| R         | Reload only |
| RMB       | Unbound on baseline — path overrides only |
| Heavy     | Normal heavy equip (no baseline heavy link) |

### 4.3 Blast model (baseline)

On fire (owner):

1. Build a **forward cone / frustum** from muzzle (or camera-forward with muzzle origin).
2. Overlap query enemies/parts in volume (decompile pass: prefer existing damage volume / spherecast chains over inventing physics).
3. For each valid target:
   - **Damage** scales by:
     - Distance falloff (near > far)
     - Angular falloff (aim axis > edge)
   - **Hit force** along blast direction (slight upward bias optional for readable launches)
4. Apply screen shove / bass punch / pressure-ring VFX once per shot.

Draft anchors (VALIDATE IN PLAYTEST):

| Param                 | Draft              |
|-----------------------|--------------------|
| Mag size              | 3                  |
| Reserve               | 15                 |
| Fire interval         | ~0.65 s            |
| Reload duration       | ~1.6–2.0 s         |
| Cone length           | ~14 m              |
| Cone half-angle       | ~18–22°            |
| Center damage         | High (pack-delete grunts on centerline near) |
| Edge damage           | ~35–50% of center  |
| Far damage            | ~40–60% of near    |
| Center hitForce       | Very high          |
| Edge hitForce         | High (launch > kill) |
| Boss launch mult      | ~0.25–0.4× force   |
| Ally force mult       | ~0.0–0.15× (prefer 0 damage to allies; tiny shove max) |

Sacred cow: cone + force + scarce ammo exist with zero upgrades.

### 4.4 What baseline does NOT include

- No gravity wells
- No Resonance / Tuned stacks
- No limb sever payoffs
- No multi-pulse / double front
- No charged Zeus superblast
- No lingering shockwave fields
- No self-propel / sonic jump
- No RMB ability
- No pierce-walk beyond single cone sample (Lane Sweeper is exotic)

Those are path- or exotic-owned.

### 4.5 Base combat loop (no upgrades)

```
Aim at densest pack → M1 pressure front → centerline deletes / edges launch
   → sidestep while chamber recycles → second shot peels remainder
   → R when dry; never spray-brain-off
```

Skill without upgrades: centerline discipline, ammo husbandry, using launches to create space, not panic-dumping all three rounds into one elite toe.


## 5. Shared Framework Vocabulary

Upgrades speak these verbs. Baseline only owns **Blast** (damage + force + cone).

### 5.1 Blast
- Instant cone pressure hit
- Damage + hitForce with distance/angle falloff
- All paths deliver through Blast unless a card says otherwise

### 5.2 Overpressure
- Multi-pulse fronts, wider/longer cone, residual wave rings, wall-bounce pressure
- Path A owned; some generics amplify blast stats only

### 5.3 Well (Singularity)
- Pull volume placed in world or spawned on blast
- Ticks pull enemies toward center
- **Collapse** = delayed sonic boom (damage + outward force)
- Path B owned; RMB often places/detonates when crowns demand it

### 5.4 Resonance / Tuned
- Stacks applied to **parts** (limb / shell / core / other) on blast hit
- At threshold: crack window, bonus part damage, or Sever setup
- Not a full DMLR Mark/Transfer kit — lighter harmonic state
- Path C owned

### 5.5 Sever
- Part-kill is a first-class event (limb / shell / attachment)
- Payoffs: cascades, ammo crumbs, shockwave shards, well seeds
- Prefer DMLR-style part typing helpers (EnemyLimbPart / Shell / Core)

### 5.6 Backblast
- Optional self-force or rear pressure ring
- Must not become free infinite mobility without cost
- Ally-safe rules still apply

### 5.7 Harmonic (soft)
- Pitch/intensity ramp toys (Crescendo cousin)
- Do not steal Cycler identity; keep sonic-flavored and blast-tied


## 6. Upgrade Paths (gravity wells — hybrids intended)

### Path A — OVERPRESSURE (pure Thundergun / Zeus)
“Bigger front. Harder shove. Empty the lane.”

- Spine: cone size, multi-pulse, force, multi-kill ammo economy, wall bounce, residual rings
- Clear vs ST: Clear native; ST via centerline discipline + force interrupt
- Hybrid hooks: blasts feed wells; overkill shards apply Resonance

### Path B — SINGULARITY (gravity wells)
“Stack the room. Collapse the stack.”

- Spine: place/pull wells, collapse damage, pull strength, multi-tag economy, blast-into-well bonuses
- Clear vs ST: Clear via stack-collapse; ST via pinning elites in well then collapse
- Hybrid hooks: collapse inherits Overpressure rings; collapse severs Tuned parts

### Path C — RESONANCE (limb sever / harmonic anatomy)
“Find the frequency. Crack the joint.”

- Spine: Resonance stacks on parts, limb/shell bias, Sever payoffs, cascade cracks
- Clear vs ST: ST native on anatomy; clear via cascades and shard bursts
- Hybrid hooks: wells pull Tuned harder; Overpressure multi-hit builds stacks fast

### Path × verb matrix

```
                 OVERPRESSURE          SINGULARITY           RESONANCE
Blast cone       core fantasy          delivers into wells   delivers tune stacks
Knockback        core fantasy          pull then invert      knocks detached parts
Gravity well     optional rider        core fantasy          wells crack tuned parts
Sever/Resonate   overkill shatter      collapse severs       core fantasy
Ammo economy     multi-kill refunds    well multi-tag refund part-kill refunds
```


## 7. Crowns & Sacred Cows

### Zeus Chamber (Exotic) — Overpressure crown
- After reload, or on RMB (if free) / next shot: **superblast** — larger cone, higher damage, higher force (PaP / Zeus fantasy).
- Draft: 1 superblast charge stored after full reload; or hold-RMB short windup only while this exotic is equipped (still not baseline charge gun).
- Prefer: **first shot after reload is Zeus-charged** (readable, no new hold-M1 baseline).
- Multiple fantasy OK later; v1 = empowered post-reload front.

### Lane Sweeper (Exotic) — Overpressure crown
- Blast **walks** forward in 2–3 pressure steps (or pierces through pack with high force/damage retention).
- Reads as a moving shock front down a corridor.
- Retention draft 65–80% per step; cap steps so one mag cannot map-wipe unaided.

### Event Horizon (Exotic) — Singularity crown
- Blasts spawn a **Well** at the densest cluster hit (or aim point if no hit).
- Well pulls for duration, then auto-collapses.
- Tuned / marked-adjacent targets receive stronger pull if Resonance cards present.
- RMB override (optional): manual detonate nearest well early.

### Implosion Hymn (Exotic) — Singularity crown
- On Well **collapse**: fire an **outward** Thundergun ring (damage + force away from center).
- This is the “stack then delete” payoff.
- Scales modestly with number of enemies inside well at collapse (soft cap).

### Sympathetic Fracture (Exotic) — Resonance crown
- Blast hits apply **Resonance** to parts.
- At max Resonance on a part: next blast (or immediate) guarantees a heavy crack — huge part damage and/or forced limb break window on non-bosses.
- Bosses: bonus part damage only (no hard forced delete without health respect).

### Bone Choir (Exotic) — Resonance crown
- On **Sever** (limb/shell kill): harmonic cascade deals damage to 1–2 other living parts on the same brain and may fling a small pressure shard to a nearby enemy.
- Overkill on limbs amplifies cascade.
- Readable “chord” VFX/audio on cascade.

Sacred cows (do not cut without rewriting identity):
- Baseline instant semi cone blast with force
- Mag scarcity (~3)
- No free wells / resonance on baseline
- Three peer paths, hybrids OK
- RMB free on baseline
- Ally-safe force rules
- Zeus fantasy is exotic (post-reload superblast), not baseline charge gun


## 8. Full Upgrade List (~30 ship + backlog)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity
Cell rule: Exotic shapes larger than others; all Exotics same cell count.

Player-facing names below. API names assigned at implementation.

------------------------------------------------------------------------------
PATH A — OVERPRESSURE
------------------------------------------------------------------------------

A-EX1. Zeus Chamber — Exotic (crown)
       First shot after reload is a superblast (bigger cone, damage, force).

A-EX2. Lane Sweeper — Exotic (crown)
       Blast advances as a multi-step shock front with retention.

A-EP1. Double Front — Epic
       Each shot fires a second weaker pressure pulse after a short delay.

A-EP2. Bore Horn — Epic
       +Cone length and center damage; −edge width slightly (tighter lance of air).

A-EP3. Breach Bellows — Epic
       Multi-kills (2+ enemies slain by one blast) refund 1 ammo (cooldown per N seconds).

A-EP4. Reverberant Hull — Epic
       Blasts that hit world geometry spawn a secondary reduced ring along the surface normal.

A-RA1. Wide Mouth — Rare
       +Cone half-angle; slight −center damage.

A-RA2. Hard Shove — Rare
       +Hit force; minor +stagger window if stagger API exists.

A-RA3. Throat Bore — Rare
       +Cone length / effective range.

A-RA4. Overkill Ring — Rare
       When a blast kills an enemy, leftover “overkill budget” becomes a small residual ring.

A-ST1. Chamber Brace — Standard
       Minor +blast damage.

------------------------------------------------------------------------------
PATH B — SINGULARITY
------------------------------------------------------------------------------

B-EX1. Event Horizon — Exotic (crown)
       Blasts seed Wells; pull then auto-collapse. Optional RMB early detonate.

B-EX2. Implosion Hymn — Exotic (crown)
       Well collapse emits outward sonic ring (damage + force).

B-EP1. Anchor Spike — Epic
       Wells last longer and pull harder; collapse damage +minor.

B-EP2. Stack Tax — Epic
       Enemies inside a Well take bonus damage from your Blasts.

B-EP3. Wellspring Reload — Epic
       Collapsing a Well with 3+ enemies inside refunds 1 ammo (cooldown).

B-EP4. Twin Singularities — Epic
       Can maintain 2 Wells; new well steals oldest if at cap (or weaker second well).

B-RA1. Soft Horizon — Rare
       Blasts apply a brief light pull toward aim axis (not full Well).

B-RA2. Collapse Weight — Rare
       +Collapse damage and outward force (needs a collapse source).

B-RA3. Drag Coefficient — Rare
       +Pull strength; −Well radius slightly.

B-RA4. Seed Fragment — Rare
       Enemy deaths inside Wells leave a tiny short-lived micro-well (low pull, quick collapse).

B-ST1. Mass Hint — Standard
       Minor +pull strength when Wells are available.

------------------------------------------------------------------------------
PATH C — RESONANCE
------------------------------------------------------------------------------

C-EX1. Sympathetic Fracture — Exotic (crown)
       Resonance stacks on parts; max stacks open heavy crack / limb break windows.

C-EX2. Bone Choir — Exotic (crown)
       On Sever: cascade to other parts on brain + optional shard to nearby enemy.

C-EP1. Joint Hymn — Epic
       +Limb damage; blasts apply bonus Resonance to limbs.

C-EP2. Shell Harmonic — Epic
       +Shell damage; Resonance on shells builds faster; shell breaks pulse minor blast.

C-EP3. Tuned Payload — Epic
       Enemies/parts at high Resonance take bonus blast damage and force.

C-EP4. Severance Dividend — Epic
       On part-kill: refund small ammo crumb OR shorten reload briefly (pick one in impl; prefer ammo crumb with soft cap).

C-RA1. Frequency Primer — Rare
       All blasts apply minor Resonance amount.

C-RA2. Brittle Mode — Rare
       +Damage to parts below 50% HP (crack finishers).

C-RA3. Dissonant Kick — Rare
       Severing a limb applies a short slow to that brain (soft CC; bosses reduced).

C-RA4. Core Echo — Rare
       When you damage a Tuned limb/shell, a fraction echos to Core (low % — not DMLR transfer fantasy).
       Draft 8–12% of damage dealt; re-echo guard required.

C-ST1. Tuning Fork — Standard
       Minor +Resonance apply amount.

------------------------------------------------------------------------------
GENERIC / GUNFEEL
------------------------------------------------------------------------------

G-RA1. Heavy Diaphragm — Rare
       +Damage, +force, +fire interval (slower).

G-RA2. Rapid Vent — Rare
       −Fire interval (faster blasts), −damage slightly.

G-RA3. Deep Magazine — Rare
       +Magazine size (e.g. 3 → 4/5), −reload speed slightly.

G-ST1. Field Couplers — Standard
       +Reload speed.

G-ST2. Spare Cells — Standard
       +Ammo reserves.

G-ST3. Muzzle Seal — Standard
       Minor −spread / tighter angular falloff (more center bias).

G-ST4. Bass Kick — Standard
       Minor +hit force.

G-OD1. Boundary Incursion — Oddity
       Increases upgrade grid size.

------------------------------------------------------------------------------
FROZEN 30 FOR V1 SHIP
------------------------------------------------------------------------------

EXOTIC (6)
  1  Zeus Chamber
  2  Lane Sweeper
  3  Event Horizon
  4  Implosion Hymn
  5  Sympathetic Fracture
  6  Bone Choir

EPIC (8)
  7  Double Front
  8  Breach Bellows
  9  Anchor Spike
 10  Stack Tax
 11  Implosion-adjacent: Wellspring Reload
 12  Joint Hymn
 13  Tuned Payload
 14  Twin Singularities

RARE (10)
 15  Wide Mouth
 16  Hard Shove
 17  Throat Bore
 18  Soft Horizon
 19  Collapse Weight
 20  Frequency Primer
 21  Brittle Mode
 22  Dissonant Kick
 23  Heavy Diaphragm
 24  Rapid Vent

STANDARD (5)
 25  Chamber Brace
 26  Mass Hint
 27  Tuning Fork
 28  Field Couplers
 29  Spare Cells

ODDITY (1)
 30  Boundary Incursion

Frozen list rarity is ship truth for v1.

------------------------------------------------------------------------------
BACKLOG (designed, not in first 30)
------------------------------------------------------------------------------

- Bore Horn (tighter long lance)
- Reverberant Hull (wall bounce rings)
- Overkill Ring
- Seed Fragment
- Drag Coefficient
- Shell Harmonic
- Severance Dividend
- Core Echo (light transfer — tune carefully)
- Deep Magazine
- Muzzle Seal
- Bass Kick
- Backblast Vent (self shove on fire — mobility tax/risk)
- Sonic Jump exotic (blast at feet launches player — Auger-adjacent, easy to grief)
- Continuous roar beam mode (rejected for baseline; maybe never)
- Full DMLR-style Transfer suite (out of scope — Core Echo is the ceiling)
- Ally healing harmonic field (support creep)
- True EffectType “Deafened” unless playtest demands new status
- Contraband grid thieves (use global patterns only if needed later)


## 9. Example Builds

Lane delete (Overpressure)
  Zeus Chamber + Lane Sweeper + Double Front + Breach Bellows
  + Wide Mouth + Hard Shove + Throat Bore
  Reload, Zeus the chokepoint, sweep the remainder.

Well shepherd (Singularity)
  Event Horizon + Implosion Hymn + Anchor Spike + Stack Tax
  + Wellspring Reload + Soft Horizon + Collapse Weight
  Seed wells, blast the pile, hymn the collapse, ammo returns.

Bone conductor (Resonance)
  Sympathetic Fracture + Bone Choir + Joint Hymn + Tuned Payload
  + Frequency Primer + Brittle Mode + Dissonant Kick
  Tune limbs, crack, cascade the choir through the brain.

Hybrid freak
  Zeus Chamber + Event Horizon + Sympathetic Fracture
  + Stack Tax + Tuned Payload + Breach Bellows
  Superblast seeds a well full of Tuned parts — no artificial brakes.

Slow howitzer
  Heavy Diaphragm + Zeus Chamber + Bore Horn (backlog) / Throat Bore
  + Hard Shove + Spare Cells
  Fewer, meaner fronts.


## 10. Economy & Tuning Rules of Thumb

- Power budget lives in **cone quality, force, wells, and sever events** — not RoF.
- Mag 3: whiffing both panic shots should hurt; Deep Magazine is backlog if too punishing.
- Multi-kill refunds (Breach Bellows, Wellspring) need cooldowns so infinite clear loops don’t appear.
- Wells must not permanently stunlock bosses — pull strength soft-capped; duration short (draft 1.2–2.0 s pull + collapse).
- Collapse + Zeus + Lane Sweeper: cap stacked AOE so one button doesn’t finish objectives alone at low investment.
- Resonance should matter in Path C and be nearly absent otherwise (Frequency Primer is the light generic on-ramp).
- Core Echo stays low % with re-echo guard; never build full Neural Feedback here.
- Ally force: default **no ally damage**, minimal or zero ally knockback.
- Environmental grief (map pits): prefer upward-biased launch on grunts; reduce force near lethal ledges if API allows later — not v1 blocker.
- Watch slow stacking: Dissonant Kick + well drag + external Cryo — prefer diminishing move mult floors.


## 11. Status & Counter Split (explicit)

| Status / counter | Role on this gun                            | Baseline? |
|------------------|---------------------------------------------|-----------|
| Resonance/Tuned  | Part stacks toward crack (custom counter)   | Path C    |
| Sever event      | Part-kill callback                          | Path C    |
| Well / Collapse  | World spatial objects                       | Path B    |
| Slow             | Dissonant Kick rider only (light)           | Upgrade   |
| Shock/Fire/Acid  | Optional backlog elemental rings            | Backlog   |
| Bleed            | Not identity (Heaven Piercer owns bow bleed)| No        |
| Decay/Rot        | Amp backlog only                            | Backlog   |
| New EffectType   | Not required for v1                         | No        |

### 11.1 Resonance counter (not EffectType)

| Param              | Draft     | Intent |
|--------------------|-----------|--------|
| Apply on blast     | Path C    | Per part hit in cone |
| Stacks to crack    | 3–5       | Sympathetic Fracture owns payoff |
| Duration           | 4–6 s     | Refresh on re-hit |
| Boss rule          | Damage amp / no forced limb delete | Fairness |
| VFX                | Ring ticks on part; crack flash    | Readable |

### 11.2 Well object (draft)

| Param              | Draft        | Intent |
|--------------------|--------------|--------|
| Pull duration      | 1.2–1.8 s    | Stack window |
| Radius             | 4–6 m        | Readable |
| Collapse delay     | end of pull  | Hymn payoff |
| Max active wells   | 1 (2 with Twin Singularities) | Clarity |
| Pull vs bosses     | reduced      | No perma-drag |


## 12. Implementation Notes

### 12.1 Gear registration
- Follow weapon template in this repo: clone base gun, GearInfo high-range id, APIName `zephyr`, behaviour component, SpawnGear stamp, CreateUpgrade pool.
- Prefer a base Gun with workable projectile or hitscan-adjacent fire; blast logic lives in behaviour + fire hook (cone overlap), not pellet authenticity.
- Candidate bases: cartridge / shotgun-like for slow punchy cycle; verify AllGear at impl. BounceShotgun or similar may match cycle better than SMG — choose for fire interval + feel, then override projectile path.
- Plugin: GUID `sparroh.zephyr`, MycoMod **IsSandbox**.
- Persistence: stable gear id; register before PlayerData.OnAwake AddGear.

### 12.2 Blast hook
- OnFiredBullet / fire postfix (owner):
  - Suppress or ignore meaningless pellet if needed; perform cone overlap damage
  - Apply damage via proper IDamageSource paths (authority-safe)
  - Apply hitForce along blast direction
  - Angular + distance falloff curves on behaviour data
- Decompile pass required: `GunData.hitForce`, damage helpers, any existing explosion/cone utilities, enemy knockback acceptance.

### 12.3 Wells
- Behaviour-spawned runtime objects (no prefab required v1): center point, radius, end time, owner ref.
- Tick pull on main thread (enemy root / brain position).
- Collapse: damage + outward force + VFX; Implosion Hymn rings.
- RMB: early detonate if Event Horizon claims priority.

### 12.4 Resonance / Sever
- Copy **patterns** from DMLR SeveranceSystem (part kind, brain, limb/shell/core) — prefer shared-style helpers local to this mod unless a common lib exists.
- Do **not** hard-depend on DMLRRework DLL for v1 (optional soft detect later).
- OnDamageTarget: apply Resonance when flags say so.
- OnKillTarget: if part kind limb/shell → Sever payoffs (Bone Choir, dividends).

### 12.5 Zeus Chamber
- Flag: next blast empowered after reload completes (or magazine hits full).
- Empowered shot consumes flag; larger cone multipliers from WeaponData.

### 12.6 Lane Sweeper
- On blast: 2–3 stepped overlap queries along aim axis over ~0.15–0.25 s, or single thick capsule; retention curve per step.

### 12.7 Hooks

| Hook              | Use |
|-------------------|-----|
| OnFiredBullet     | Cone blast; well seed; Zeus flag; Lane steps |
| Reload complete   | Zeus Chamber arm |
| Update (gun)      | Well ticks; collapse |
| OnBeforeDamage    | Tuned bonus; Stack Tax; falloff already applied pre-damage preferred |
| OnDamageTarget    | Resonance apply; Soft Horizon pull tag |
| OnKillTarget      | Multi-kill ammo; Sever cascade; well death seeds |
| RMB press         | Early well detonate (Event Horizon) |

### 12.8 RMB priority

1. Event Horizon early detonate (if equipped and well active)
2. Future path overrides
3. Else unbound

Note: Zeus Chamber v1 prefers **post-reload empower**, not RMB, to keep RMB clean for wells.

### 12.9 State host
ZephyrWeaponBehaviour (or true Gun subclass when prefab exists):
- WeaponData: cone angle/length, falloff curves, force mults, well params, resonance params, Zeus mults, lane steps, flags
- Runtime: well list; per-part resonance map; multi-kill shot accumulator; Zeus armed bool
- Prefab snapshot restore on upgrade Remove

### 12.10 HUD
- Optional well timer pips
- Zeus armed icon after reload
- Resonance readout on aimed part optional (nice-to-have)

### 12.11 Multiplayer
- Sandbox mod; all clients need the same plugin
- Damage/force follow authority rules
- Wells: owner-authoritative spawn/tick; validate collapse damage on authority
- Resonance map: owner gun authority

### 12.12 VFX / audio priority
1. Bass thump + mechanical vent on fire
2. Visible pressure cone / dust ring
3. Enemy launch readability
4. Well: dark spiral / fungal gravity cue + rising whine
5. Collapse: inward suck → outward boom
6. Resonance ticks on parts; crack snap; Bone Choir chord
7. Zeus Chamber: higher pitch spool on armed + louder superblast


## 13. Deliberate Non-Goals

- Not baseline charge-to-fire gun (instant semi locked)
- Not Globbler acid flood
- Not Plate stick/recall clone
- Not full DMLR transfer / Expose suite
- Not baseline free wells or sever
- Not continuous siren beam primary
- Not team-hostile yeet physics
- Not requiring custom Unity prefab for v1 (runtime clone OK)
- Not shipping new EffectType in first 30
- Not COD Thundergun mesh/name plagiarism — Zephyr is SAXON-original fiction


## 14. Open Tuning Questions (playtest, not design blockers)

1. Mag 3 vs 2 vs 4.
2. Cone half-angle 18–22° vs feel of “shotgun of air.”
3. Fire interval 0.65 s vs mission pace.
4. Boss force mult 0.25–0.4.
5. Well pull duration vs collapse damage budget.
6. Resonance stacks to crack (3 vs 5).
7. Zeus mults — must feel PaP-special without deleting rare investment.
8. Lane Sweeper step count vs clear power.
9. Whether cone is pure overlap or needs a visible projectile tracer for netcode/feel.
10. Exact knockback API after decompile pass (`hitForce` sufficiency vs manual impulse).
11. Core Echo inclusion later — keep backlog until Resonance feels good alone.
12. Base gun clone choice in AllGear for animation/cycle.


## 15. Success Criteria / Player Fantasy Checklist

- [ ] Instant M1 blast works with zero upgrades; cone + force readable
- [ ] Mag scarcity makes each shot a decision
- [ ] Centerline deletes packs; edges launch survivors
- [ ] Bosses don’t get yeeted to orbit; still hurt
- [ ] Allies don’t get grief-launched
- [ ] Zeus Chamber post-reload superblast feels like PaP moment
- [ ] Lane Sweeper walks a corridor of pressure
- [ ] Event Horizon seeds wells; pull is visible
- [ ] Implosion Hymn collapse is the stack-delete payoff
- [ ] Sympathetic Fracture tunes and cracks parts
- [ ] Bone Choir cascades on sever
- [ ] Hybrid Zeus + Well + Fracture feels intentional
- [ ] Audio/VFX read industrial sonic weapon, not generic explosion spam


## 16. Strengths, Weaknesses & Co-op

Strengths
- Best-in-class space creation and panic peel
- Three very different endgames from one gun
- Photo-mode knockback moments
- Co-op peel / stack / crack support without being a heal bot
- Scarce-ammo skill expression

Weaknesses
- Low brain-off DPS (must aim the cone)
- Mag pressure; dry gun is helpless briefly
- Close scramble if both shots whiff
- Weaker long-range than marksman primaries
- Boss launch resistance means ST needs path investment (Resonance / Stack Tax)

Co-op
- You peel packs off downed allies and objectives
- Wells set up team focus fire
- Resonance cracks open anatomy for DMLR/ally execute
- Avoid VFX that blinds teammates; keep rings readable not white-out


## 17. Visual, Audio & Thematic Design

Appearance
- SAXON **Zephyr** acoustic cannon: flared resonator muzzle, stacked Helmholtz coils, pressure gauges, fungal-etched “HEARING PROTECTION MANDATORY” decals, backpack or underbarrel capacitor drum as mag metaphor
- Not a COD Thundergun mesh clone — Mycopunk industrial + myco corruption
- Blast: translucent pressure cone, dust and spore ring, heat-shimmer air
- Wells: dark spiral mote gravity with teal/violet fungal light
- Resonance: vibrating outline on parts; crack = white-blue snap rings
- Zeus armed: coils glow brighter; gauge pegs

Sound
- Fire: deep brass thump + air tear
- Zeus: longer sub-bass + shattering glass harmonic
- Well: low suction hum rising to collapse boom
- Resonance: thin crystalline ticks → chord on Bone Choir
- Reload: vent hiss + capacitor clamp

Flavor / SAXON blurb (draft)
“SAXON ZPR-1 Zephyr — Directed overpressure system for swarm displacement and structural agitation.
Point the horn downrange. The atmosphere will file the rest of the paperwork.
(Not a toy. Not a speaker. Definitely not ‘just a big fan.’)”


## 18. Locked Review Decisions (2026-08-07)

| Decision              | Lock |
|-----------------------|------|
| Form factor           | Industrial sonic / overpressure cannon |
| Player-facing name    | **Zephyr** |
| Slot                  | Primary |
| Mag scarcity          | Wonder-primary, mag **3** (band 2–4) |
| Fire feel             | **Instant semi blast** (no baseline charge) |
| Paths                 | Overpressure / Singularity / Resonance (equal peers) |
| Wells                 | Path B opt-in |
| Limb sever            | Path C opt-in (Resonance → Sever) |
| RMB                   | Free for path overrides |
| Zeus fantasy          | Exotic post-reload superblast (not baseline charge) |
| Tone                  | SAXON industrial + fungal resonance |
| Ship pool             | Frozen 30 listed above |
| Crowns                | Zeus Chamber, Lane Sweeper, Event Horizon, Implosion Hymn, Sympathetic Fracture, Bone Choir |
| MycoMod flag          | IsSandbox at implementation |
| Working APIName       | zephyr |
| Doc file              | Zephyr-DesignDoc.md (this file) |
| DMLR relationship     | Pattern reuse only; no hard dependency; no full transfer suite |
| Ally safety           | No ally damage; minimal/zero ally knockback |


## 19. Changelog

v1 (2026-08-07)
- Initial full design from locked user decisions:
  - A1 Primary mag 2–4 wonder-primary
  - B1 Three equal paths
  - C1 Instant semi blast
  - D Name: Zephyr
  - E Full Heaven Piercer–depth doc
- Research anchors:
  - Thundergun / Zeus Cannon fantasy (cone, knockback, scarce ammo, PaP power)
  - Mycopunk arsenal gap vs Globbler, Swarm, Plate, Shocklance, DMLR
  - Wiki: Shocklance charge/Auger (contrast — not baseline), Crescendo naming, Plate Impulse/Shockwave names
  - Sibling docs: DMLR Severance vocabulary, Heaven Piercer path/crown/frozen-30 structure, weapon template registration
  - DMLR Gravitational Collapse as well/pull precedent (path-owned here)
- Baseline free of wells/sever; hybrids encouraged


## 20. Implementation checklist (post-design)

- [ ] Rename plugin/csproj/thunderstore from template → Zephyr
- [ ] ZephyrWeaponBehaviour.Data fields from §12.9
- [ ] Cone blast overlap + falloff + hitForce
- [ ] Verify base gun clone in AllGear
- [ ] Well tick + collapse system
- [ ] Resonance map + Sever callbacks
- [ ] Zeus Chamber arm on reload
- [ ] Lane Sweeper multi-step front
- [ ] UpgradeRegistration frozen 30
- [ ] RMB well detonate priority
- [ ] Ally-safe force rules
- [ ] Persistence + SpawnGear stamp
- [ ] VFX/audio placeholders
- [ ] Playtest pass on §14 knobs


## 21. Path fantasy summary (quick reference)

| Path | Name | Fantasy | Crowns |
|------|------|---------|--------|
| A | Overpressure | Thundergun lane delete / Zeus | Zeus Chamber, Lane Sweeper |
| B | Singularity | Stack in wells → collapse boom | Event Horizon, Implosion Hymn |
| C | Resonance | Harmonic crack → limb sever choir | Sympathetic Fracture, Bone Choir |

Baseline: instant cone blast, mag 3, force, no toys.
Endgame: empty the lane, collapse the pile, or sing the swarm apart.
