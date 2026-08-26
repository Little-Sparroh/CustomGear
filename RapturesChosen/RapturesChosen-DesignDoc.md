# Rapture's Chosen – Design Document (v1.1)

> Status: **Design only** — no implementation yet.
> Working / ship name: **Rapture's Chosen** (API-friendly: `RapturesChosen` / `raptures_chosen`).
> Source fantasy: vanilla **Shocklance** rework + absorbed **A16 Beam Ripper** (`ChargeSniper`) upgrade DNA.
> Template base: `.new.ShocklanceRework` weapon content project.
> Product shape: **separate primary gear** — vanilla Shocklance and vanilla Beam Ripper are left unmodified.
> **No separate Beam Ripper rework** — that product is deleted; its card fantasies live only here.

---

## 1. High Concept / Fantasy

**Rapture's Chosen** is the dual-mode shock lance that finally has more than one fantasy.

Vanilla Shocklance funnels real power into one stack: **Compressor + Half Cocked + Splintered Lance + Micro Detonators**. Railshot cannot compete. Auger Subroutine is a full exotic tax on R, does not share the gun's element, and only a thin support tree (Gearshift / Centripetal) cares about it. Shield pierce is free and unearned. Storm's Eye died when ammo-refund meta made empty magazines unrealistic. The gun was once mega-OP, then hard-nerfed into a single viable chart line.

Rapture's Chosen keeps the charge-coil gunfeel and rebuilds around a **baseline dual mode**:

- **M1** — charge and fire a piercing shock **coil**
- **RMB** — charge and release **Auger** (joust / drill) — free, not an exotic

Three peer doctrines all speak both modes:

- **Scattercoil** — volume, size, short range, single-burst detonations
- **Railstake** — one thin lightning stake; pierce pays; shield break is earned
- **Dynamo** — the joust charges the lance; the lance pays the joust

**One-liner:** *Charge the coil, scatter or skewer the line — the drill is always in your off-hand (unless you lay a rail and ride the lightning).*

**Element spine:** Shock at baseline on **both** M1 and Auger. Fire / Acid / Decay as opt-in glue — not required for identity. **Auger always inherits the gun's active element.**

**Absorbed heavy DNA:** Vanilla **A16 Beam Ripper** (`ChargeSniper`) is a long-range charge piercer with its own exotic package (javelin delayed boom, path-trail dets, knockback miss-refund, airborne/sprint charge toys). Those fantasies are **ported into Rapture's Chosen** as path cards — not as a second ship product. Vanilla Beam Ripper stays in the heavy slot untouched.

---

## 2. Role & Fantasy in the Arsenal

| Trait | Value |
|-------|--------|
| **Slot** | Primary |
| **Range** | Close–mid baseline; Railstake extends mid–long |
| **Role** | Dual-mode charge coil + joust drill; pack clear, pierce ST, or mode-tempo hybrid |
| **Gap filled** | Vanilla Shocklance is one shotgun stack + optional stranded Auger exotic. Rapture's Chosen is three peer dual-mode doctrines |
| **Synergies** | Movement (joust engage/escape), shock offhands/grenades, co-op linger zones / tether, melee contact (Fisticuffs DNA) |

**Product shape:** New primary (**Rapture's Chosen**). Does **not** replace or patch vanilla Shocklance.

**Not trying to be:** pure ADS sniper, heat-infinite SMG, mandatory external-mod dependency, or a return of pre-nerf Shocklance numbers.

| Gear | Relationship |
|------|----------------|
| Vanilla Shocklance | Left in game untouched |
| Vanilla A16 Beam Ripper (`ChargeSniper`) | Left in game untouched; **upgrade fantasies absorbed here**; **no Beam Ripper rework product** |
| DMLR / Junk Flinger / Aussie / Trident / Heat Cycler docs | Sibling design structure only |
| ArcLightningRework / HeavensFury / PesticideRework | Optional DNA (chains, smites, input-owns-mode) — no hard runtime deps |
| Heaven Piercer | Charge-curve language cousin; Half Cocked stays path-owned here |

---

## 3. Design Pillars

1. **Dual mode is baseline vocabulary** — M1 coil + RMB Auger on the empty grid. Every path improves **both** modes; no dedicated “Auger-only path.”
2. **Three peer doctrines** — Scattercoil / Railstake / Dynamo. No single mandatory exotic stack.
3. **Scattercoil stays celebrated** — multi-coil + size + detonate still slaps; peers match via their own verbs, not by nerfing Scatter into the ground.
4. **Railstake is elevated to peer power** — Railshot is no longer a weak Rare sidegrade; pierce/line identity gets exotic-tier budget.
5. **Dynamo owns mode dialogue** — stacks, windows, and economy between coil and joust; replaces multi-Auger exotic stacking.
6. **Auger shares gun element** — Shock baseline; element upgrades apply to coils **and** drill ticks.
7. **Shield pierce is upgrade-gated** — removed from baseline.
8. **Micro Detonators do not chain** — one explosion on kill; no explosion-from-explosion recursion.
9. **Storm's Eye must fire in refund-meta play** — never gated on emptying the magazine.
10. **Half Cocked is path-owned only** — not baseline charge scaling.
11. **ADS is off** — RMB is Auger at baseline (Pesticide / Twin-Hopper input lesson).
12. **RMB can be crown-replaced** — **Leyline** (and only explicit RMB-swap crowns) may replace Auger with a rideable rail along the shot line; Auger-path cards idle unless they define a rail rider.
13. **On-hit / pierce / drill / detonate / mode windows > flat % only** — honest % glue still exists so light grids bite.
14. **~30 upgrades for v1** — exotic shapes larger than others; each exotic same cell count; renames allowed. Beam Ripper absorbs land in backlog / promotion candidates first so frozen 30 stays honest.
15. **Failure states stay fun** — whiffed joust, overspread scatter, rail miss, det into empty air, Dynamo spent on the wrong mode, mislaid Leyline into a wall.
16. **Overflow exotics demote rarity** — do not delete fantasy; Epic/Rare is fine.
17. **Beam Ripper rework is cut** — one dual-mode primary owns both Shocklance and Beam Ripper card DNA.

---

## 4. Core Mechanics & Gunfeel

### 4.1 Base gun (no upgrades)

| Trait | Draft / intent |
|-------|----------------|
| Fire mode (M1) | Charge-to-fire single shock coil; pierce bodies/parts |
| Fire mode (RMB) | Charge-to-release Auger joust/drill |
| Damage | Honest close poke; empty-grid playable, not pre-nerf OP |
| Element | **Shock on M1 and Auger** |
| Shield pierce | **OFF** |
| Half Cocked curve | **OFF** (fixed charge → fire; no mid-charge loose scaling) |
| Magazine / reserve | ~8 / ~32 spirit (vanilla Shocklance ballpark) |
| Charge time (M1) | ~0.3–0.45s spirit |
| Auger | Single-stack baseline numbers (below old multi-Auger stack); modest duration/damage/size |
| Reload | Standard reload on R |
| ADS | **Disabled** (`canAim = false`) |
| Multi-pellet / rail needle / detonators / linger / Dynamo stacks | **OFF** until upgrades |
| Model / audio | Borrow Shocklance until custom art |

### 4.2 Vanilla Shocklance reference (wiki spirit — confirm in decompile at impl)

```
Damage 40, Element Shock 5.5, RPM 90, interval ~0.67s
Mag 8, reserve 32, reload 1.9s, bullet speed 210
Charge time ~0.3s
Falloff start 16 / end 22 / max range 26 / min mult 0.5
Spread 0; recoil X 6, Y 3
Identity: close charge poke, pierce coil, shock
```

Confirm concrete class name in decompile before clone (dump may lag; search Gun subclasses / gear APIName at impl).

### 4.3 Inputs

| Input | Baseline | With path crowns |
|-------|----------|------------------|
| **Hold M1** | Charge coil | Half Cocked (A): may loose early with scale; Battery Recursion: full-auto no charge after first; etc. |
| **Release M1** | Fire coil at full charge (baseline requires full unless card says otherwise) | Half Cocked: fire any time while charging |
| **Hold RMB** | Charge Auger | Path riders on charge speed / move while charging; **Leyline:** charge rail lay |
| **Release RMB** | Launch Auger drill | Path A/B/C modify drill; **Leyline:** lay rideable rail along aim/shot line (Auger off) |
| **R tap** | Reload | Storm's Eye rewrite may proc on reload (Dynamo); Residue-style hold-R is **not** baseline |
| **R hold** | No baseline special | Reserved if a future crown needs it; v1 frozen 30 prefers not to steal R |
| **ADS / Aim** | **None** | None — RMB is Auger **or** Leyline rail when that crown is equipped |


### 4.4 Baseline combat loop

```
M1 charge → fire piercing shock coil → reposition
RMB charge → release Auger through targets → drill ticks (Shock) → exit
R when dry
    ↘ without crowns: honest dual-mode poke + emergency joust
    ↘ Scattercoil: multi fat coils + kill boom; Auger dives packs
    ↘ Railstake: needle stake + pierce ramp; Auger closes and primes
    ↘ Dynamo: joust builds stacks / Gearshift window → coil cash-out (or reverse)
    ↘ Leyline equipped: RMB lays rideable rail on the shot line → mount → reposition / engage (Auger idle)
```

Skill without upgrades: spacing, pierce lines, when to spend Auger as engage vs escape, don't face-tank during charge.


### 4.5 What baseline does NOT include

- No shield pierce
- No Half Cocked (early loose + charge scaling)
- No multi-pellet / Compressor size package
- No Railshot needle package
- No Micro Detonators
- No Dynamo stacks / Gearshift windows
- No linger zones (Killing Field / Subway)
- No Laying Cable tether
- No Battery Recursion full-auto
- No Storm's Eye bonus coil
- No multi-Auger exotic stacking
- No ADS
- No Leyline rideable rail (RMB stays Auger until that crown)
- No Beam Ripper package (javelin delayed boom, path-trail dets, Third Law knockback, etc.)

Those are path- or upgrade-owned.


### 4.6 Auger baseline rules (LOCKED)

| Rule | Intent |
|------|--------|
| Input | **RMB** charge → release (not R) |
| Element | **Same as gun** (Shock default; element cards apply) |
| Stacking | One baseline instance; **no** “equip Auger 3×” — duration/damage/size from Path C (and light A/B riders) |
| Role | Always-available joust/drill; paths specialize it, none uniquely “enable” it |
| Numbers | Modest vs old multi-stack Auger; Deep Bore / Dynamo crowns restore mythic drill |

### 4.7 Coil baseline rules (LOCKED)

| Rule | Intent |
|------|--------|
| Charge | Must complete charge to fire unless Half Cocked / Battery Recursion / similar |
| Pierce | Bodies/parts yes; **shields no** until Shield Breaker |
| Element | Shock; shared with Auger |
| Count | 1 coil per shot until Splinter / similar |

### 4.8 Crown / mode priority (SOFT — not hard mutex)

| Combo | Behavior |
|-------|----------|
| Any single path crown | Full fantasy; both modes still work |
| Scatter + Rail shape cards | Soft tension (spread vs needle); both function; player chooses |
| Dynamo + anything | **Encouraged** — mode windows feed specialized M1 |
| Half Cocked + Rail | Valid; charge stake skill expression |
| Half Cocked + Splinter | Poster Scatter hybrid |
| Micro Det + Rail | Valid but weaker clear than Scatter; no ban |
| Battery Recursion + Half Cocked | Recursion may idle Half Cocked curve after first shot — acceptable soft tension |
| Leyline + Auger-path cards | **Hard swap:** Auger off; Deep Bore / Centripetal / Pack Dive / Lined Joust / Exit Storm (S1) / Gearshift-from-Auger **idle** unless card defines a rail rider |
| Leyline + Dynamo Core | Valid — M1 still builds/spends stacks; joust-half of Dynamo idles unless rail-end is wired as a mode event (optional later) |
| Leyline + Godrail / Skewer | **Encouraged** — line identity; rail follows the stake line |
| All three paths | Allowed; watch stacked clear/ST in playtest — no ban list |

**RMB ownership (v1.1):** **Auger by default.** Crowns **modify** Auger unless an explicit **RMB-swap crown** (Leyline) is equipped — then RMB is rail only. No other v1 crown steals RMB.


---

## 5. Shared Framework Vocabulary

### 5.1 Coil (M1)
- Charged projectile lance: damage, size, spread/count, range/falloff, pierce rules, element appl
- Path A mutates volume/size; Path B mutates line/pierce; Path C mutates economy/windows around the shot

### 5.2 Auger / Joust (RMB)
- Charge → launch through enemies; continuous drill damage while overlapping
- Inherits **gun element** and relevant gun damage % pipeline where sensible
- Exit moment = first-class hook (Gearshift, exit spark, Storm rewrite option, Dynamo convert)
- **Disabled entirely** while Leyline (RMB-swap) is equipped

### 5.2b Leyline / Rideable Rail (upgrade-gated — Path B) [NEW v1.1]
- Replaces Auger on **RMB** while equipped (hard ownership swap)
- Lays a temporary **rideable rail** along the line the bullet / aim ray would travel
- Player can mount and ride for reposition, engage, or escape
- Duration, length, speed, and whether allies can ride are card-tuned
- Failure: rail into wall / void, dismount mid-pack, spent RMB with no useful line
- Optional later hooks: rail end ≈ Auger end for Exit Storm / Spark Gap (not required for v1 draft)

### 5.3 Dynamo stacks (upgrade-gated — Path C)

- Built by using one mode; spent by the other (or on explicit discharge cards)
- Examples: Auger ticks → stacks → next M1 empowered; M1 hits → stacks → next Auger longer/meaner
- Cap and decay are card-tuned; empty-grid has **zero** stacks

### 5.4 Detonate (once)
- Kill → chance for **one** elemental explosion
- **No chain** (explosion kills do not roll another Micro Det explosion)
- Element follows gun / card roll
- **Straggler's Revenge** (Beam Ripper DNA) is a separate verb: delayed **path-trail** explosions along the projectile path — not Micro Det chain, not kill-gated the same way


### 5.5 Pierce / Skewer
- Pierce count and per-pierce damage ramps
- Swing Through: multi-pierce builds next-shot bonus (crosshair gauge DNA)
- Distinct from shield pierce

### 5.6 Shield Break
- Explicit flag: coil and/or Auger ignore shields
- Baseline false

### 5.7 Mode window (Gearshift DNA)
- After Auger (and optionally during): charge speed, DR, maybe +M1 damage
- Dynamo path spine; light copies may appear as smaller rares

### 5.8 Element rule (LOCKED)
```
gunElement = Shock unless an element card sets Fire / Acid / Decay / rolled
M1 coil applies gunElement
Auger drill ticks apply gunElement
Detonate explosions prefer gunElement (or card-specified)
```

---

## 6. Damage & Economy Rules

### 6.1 Vanilla failure modes

| Failure | Fix |
|---------|-----|
| Only shotgun stack viable | Three dual-mode peer paths; elevate Rail; Dynamo rewards switching |
| Auger exotic tax + wrong element | Baseline RMB + shared element |
| Railshot too weak | Exotic-tier Rail package + pierce spine |
| Storm's Eye dead (never empty mag) | Rewrite trigger off last-round |
| Free shield pierce | Gated upgrade |
| Micro Det chain snowball | Single explosion only |
| Multi-Auger stack as DPS | Path C duration/damage cards |

### 6.2 Rapture's Chosen rules

1. Prefer **% damage** and **verbs** (pellets, pierce ramp, drill duration, det size, Dynamo spend) over huge flat packages.
2. Empty-grid must clear packs without feeling broken-on-purpose or wet-noodle.
3. Scatter poster grids remain strong; Rail/Dynamo must reach competitive clear/ST on their verbs.
4. Ammo refund meta is assumed common — **do not** design payoffs that require dumping the mag to zero.
5. Auger damage scales with gun damage upgrades so glue cards matter on both modes.
6. Watch stacked transfer-like effects: det + linger + Dynamo spend + pierce ramp — fun hybrids, not delete-everything.

### 6.3 Budget sketch (playtest dials)

| Lever | Starting intent |
|-------|-----------------|
| Baseline vs vanilla empty Shocklance | Slightly more honest dual-mode feel; not old OP |
| Typical Standard | Modest % damage or economy |
| Typical Rare | Path verb + modest % |
| Typical Epic | Strong verb or window |
| Micro Det (no chain) | Slightly higher single-boom EV than a single link of old chain if needed |
| Rail exotic | Must feel like a real sniper-coil transformation |
| Auger baseline | Useful mobility + modest DPS; crowns make it mythic |
| Dynamo stack spend | Readable cash-out every few mode swaps |

---

## 7. Upgrade Paths (gravity wells — hybrids intended)

### Path A — SCATTERCOIL
**“The room is a circuit waiting to short.”**

| Mode | Expression |
|------|------------|
| **M1** | Multi-coil, +size, −range, Half Cocked curve, kill → single detonation |
| **Auger** | Wider/shorter drill; dive into clumps; joust-kill sparks; exit pop |

- **Spine:** volume, size, short range, charge skill (Half Cocked), detonate-once
- **Crowns:** Fractured Rapture (Splinter DNA); Micro Detonators (no chain)
- **Supports:** Compressor, Notched Breakpoint, Half Cocked, Crescendo, Storm-adjacent clear toys if not Dynamo-owned
- **Hybrid hooks:** Dynamo windows after joust → fat multi-coil dump; Rail usually soft-tensions spread

### Path B — RAILSTAKE
**“One coil through everything that matters.”**

| Mode | Expression |
|------|------------|
| **M1** | Tight line, +range/+damage, Skewer ramp, Swing Through, Shield Breaker; Beam Ripper DNA (javelin delayed boom, path-trail dets, reverse falloff) |
| **Auger** | Longer/thinner drill; applies pierce/Skewer stacks or “lined up” mark spent by next Rail shot; gap-close → stake |
| **RMB alt** | **Leyline** — replace Auger with rideable rail along the shot line |

- **Spine:** needle projectile, pierce payoffs, range rewrite, earned shield break, line mobility (Leyline)
- **Crowns:** Godrail (Railshot elevated); Skewer (Epic in frozen 30); **Leyline** (RMB rail swap — backlog / promote); Beam Ripper exotics as promotion candidates
- **Supports:** Coil Extension / Extra Capacitors, Black Market Battery, Auxiliary Chargers, Energy Inversion (reverse falloff), Third Law, airborne/sprint charge toys from Beam Ripper
- **Hybrid hooks:** Gearshift charge speed → deliberate Rail stakes; Dynamo stacks on pierce spend; Leyline + Godrail = ride the stake line


### Path C — DYNAMO
**“The joust charges the lance; the lance pays the joust.”**

| Mode | Expression |
|------|------------|
| **M1** | Charge economy, Battery Recursion, ramps, linger fields, **Storm's Eye rewrite**, spend Dynamo stacks |
| **Auger** | Gearshift windows, turn rate, Forward Charge, build Dynamo on drill, Deep Bore duration/size |

- **Spine:** mode dialogue, shared charge economy, post-joust windows, linger utility, non-mag-empty storm
- **Crowns:** Gearshift; Dynamo Core (mode↔mode stacks; replaces multi-Auger)
- **Supports:** Centripetal Cornering, Forward Charge, Fisticuffs rewrite, Killing Field, Subway, Ready and Waiting overlap OK, Laying Cable co-op
- **Hybrid hooks:** Best generalist glue — makes Scatter and Rail feel better after a joust without being “the Auger path”

### Path × mode matrix

```
                    SCATTERCOIL              RAILSTAKE                 DYNAMO
M1 Coil          multi + size + det       needle + pierce + BR DNA  economy + windows + storm
Auger            wide dive + pack kill    thin prime + close gap    stacks + Gearshift + Deep Bore
RMB alt          —                        Leyline rideable rail     —
Mag desire       flexible / Half Cocked   deliberate stakes         refund-friendly; never need empty
Reload           normal                   normal                    Storm rewrite / tempo OK
Clear            det + pellets            pierce columns + path det linger + mode cash-out
ST               fat charged multi        skewer boss lines         windowed empowered stakes/drills
Poster hybrid    Fractured + Micro Det    Godrail + Skewer          Gearshift + Dynamo Core
                     + Half Cocked            + Shield Breaker          + either shape path
                                          (+ Leyline optional)
```


---

## 8. Crowns & Sacred Systems

### 8.1 Fractured Rapture — Exotic (Path A) [Splintered Lance DNA]

**Fantasy:** The lance shatters into multiple coils.

**Effects (draft):**
- +Bullets per shot (vanilla +2 spirit → 3 total)
- Large spread (cone/fan); Notched Breakpoint tightens vertical into a fan
- Auger rider (light): slightly wider drill hitbox while equipped

**Not:** the only viable card. **Is:** Scatter volume crown.

### 8.2 Micro Detonators — Exotic (Path A) [REWRITE — no chain]

**Fantasy:** Kills pop a shock (or gun-element) micro-bomb. Once.

**Effects (draft):**
- On kill: chance for **one** elemental explosion (size/damage tuned)
- Element follows gun element
- **REMOVED:** “targets killed by the explosion can explode again”
- Optional Auger rider: joust-kills use same single-boom roll

**Compensation:** If single-boom feels weak vs old chain EV, raise chance/size/damage slightly — do **not** re-add chain on this exotic.

### 8.3 Godrail — Exotic (Path B) [Railshot elevated]

**Fantasy:** One thin lightning stake. Range and authority restored.

**Effects (draft):**
- Bullet size → needle (~0.15 spirit)
- Large +range, +damage, +element amount (at or above old Railshot; package must compete with Scatter)
- Near-zero spread interaction (fights Splinter softly)
- Optional: slight +charge time trade OR slight pierce +1 — tune so it is a real transformation
- Auger rider: longer, thinner drill; slightly +drill damage

**Rarity lock:** **Exotic** (was Rare — that was the power problem).

### 8.4 Skewer — Exotic or Epic (Path B)

**Fantasy:** Each body the stake passes through feeds the next.

**Effects (draft):**
- −Base damage modest; +damage per target/part pierced
- Pairs with Godrail; also valid on baseline single coil
- Auger rider: drill ticks on a brain count as pierce stacks for next M1 (cap)

**Budget note:** If only 6 exotic slots, **demote Skewer to Epic** and fold a smaller pierce bonus into Godrail.

### 8.5 Gearshift — Exotic (Path C)

**Fantasy:** After the joust, the lance is redlined and you are armored.

**Effects (draft):**
- While Auger active and for a short time after: +charge speed, +damage resistance
- Optional light +M1 damage during window (keep modest — window identity, not another Doobie)
- Duration stackable with multiple copies if CanStack, or single strong exotic — prefer readable one-card window

### 8.6 Dynamo Core — Exotic (Path C) [NEW — replaces multi-Auger stack]

**Fantasy:** Motion and lightning share one capacitor.

**Effects (draft):**
- Using Auger builds Dynamo stacks (per tick or per enemy touched)
- Using M1 builds Dynamo stacks (per hit or per pierce)
- Spend options (card defines primary; keep one clear rule in UI):
  - **Prefer:** Next shot or next Auger **consumes stacks** for +damage / +size / +drill duration
  - Alternate: manual discharge on reload (ties Storm rewrite)
- Caps stacks; decay out of combat
- This is the mythic “I live in both modes” crown

### 8.7 Half Cocked — Exotic or Epic (Path A) [path-owned only]

**Fantasy:** Loose early or hold for a fatter, longer, meaner coil.

**Effects:** Charge duration up; can fire while charging; damage/size/range scale with charge time (vanilla DNA).

**Baseline:** **OFF.**  
**Rarity:** Doesn't matter for design lock — **prefer Epic** in frozen 30 to keep 6 exotics clean; elevate to Exotic if playtests say the curve is the Scatter mythic beat.

### 8.8 Boundary Incursion — Oddity

Grid grow. Universal keep.

### 8.9 Leyline — Epic (Path B) [NEW v1.1 — RMB swap]

**Fantasy:** Where the stake would fly, a rideable rail of lightning remains. You mount it.

**Effects (draft):**
- **Replaces Auger on RMB** while equipped (hard ownership swap — not a soft rider)
- Charge/release RMB lays a temporary **rideable rail** along the aim ray / bullet path
- Mount and ride for reposition, engage, or escape; length/duration/speed card-tuned
- Optional: light shock tick to enemies the rail passes through (keep modest — mobility first)
- Auger-path cards **idle** unless they define an explicit rail rider (see §4.8)

**Rarity:** **Epic** default (exotic budget full at 6). Elevate to Exotic keystone if playtest says the rail *is* the Railstake mythic beat — then demote another exotic.

**Not:** baseline. **Is:** the “delete Auger, ride the line” crown the user asked for.

### 8.10 Inverted Spear of Heaven — Exotic or Epic (Path B) [Beam Ripper — Inverted Spear of Hell DNA]

**Fantasy:** The coil becomes a javelin — heavier impact, delayed shock detonation after a short hang time.

**Effects (draft, wiki spirit):**
- Projectile reads as javelin: +impact damage, high speed, low/no gravity
- After short delay at impact (or along path end): shock (gun-element) explosion
- Soft-tensions Fractured multi-coil; pairs with Godrail / Skewer

**Budget:** Backlog / promotion candidate — do not silently expand frozen 30 exotic count.

### 8.11 Straggler's Wake — Exotic or Epic (Path B or A) [Beam Ripper — Straggler's Revenge DNA]

**Fantasy:** After a short delay, a trail of explosions walks the projectile's path.

**Effects (draft):**
- On M1 fire (or on hit): delayed path-trail detonations along the coil's flight line
- **Not** Micro Det chain — separate verb; does not recurse from its own explosions
- Soft-tensions / stacks with Micro Det (kill boom + path trail) — watch clear EV in playtest

**Budget:** Backlog / promotion candidate.

### 8.12 Third Law — Epic (Path B/G) [Beam Ripper DNA]

**Fantasy:** Every shot kicks you back; misses return to the battery.

**Effects (draft):**
- Firing applies rearward knockback (`ChargeSniper.fireKnockback` DNA)
- Ammo from shots that hit **no** targets refunds to reserves
- Mobility + economy toy; pairs with Leyline reposition and airborne cards

### 8.13 Beam Ripper support package (absorbed — not all frozen)

| Vanilla Beam Ripper | RC placement | Notes |
|---------------------|--------------|-------|
| Safety Override | **Merge into Battery Recursion** (C) | Full-auto + mag fantasy; Safety Override name backlog only |
| Energy Inversion | Path B Epic/Rare backlog | Reverse falloff; long-stake authority |
| Arise | Path B/C Rare backlog | Levitate while charging |
| Inertial Overload | Path B Rare backlog | +damage while airborne |
| Running Hot | Path B/G Rare backlog | +direct-hit damage while sprinting |
| Quick Charge | Glue / merge Flurrying | +charge speed |
| Battery Extension | Glue / merge Gladiator's | +reserves |

---

## 9. Storm's Eye Rewrite (LOCKED direction)


### 9.1 Problem
Vanilla: after firing **last shot in mag**, bonus overhead coil.  
Modern ammo refund → players rarely empty mag → card is dead.

### 9.2 Rewrite options (pick primary for v1; others backlog)

| ID | Trigger | Fantasy |
|----|---------|---------|
| **S1 (prefer)** | **On Auger end** (successful joust ≥1 enemy hit or always on exit) | Exit storm coil upward/forward |
| **S2** | **On reload complete** | Reload punchline storm |
| **S3** | **Spend N Dynamo stacks** | Capacitor storm discharge |
| **S4 (optional)** | **On Leyline rail end / dismount** | Only if Leyline equipped and S1 would otherwise be dead |
| **CUT** | Last round in magazine | Dead in refund meta |

**v1 lock: S1 primary** — reinforces dual-mode without requiring Dynamo Core.  
If Dynamo Core equipped, S1 coil can scale lightly with stacks consumed or current stacks (hybrid juice).  
**With Leyline:** S1 idles unless S4 is implemented; prefer documenting idle over silent no-proc.


**Card placement:** Epic Path C (or glue epic). Name rename OK (e.g. **Rapture Discharge**, **Skycoil**, **Exit Storm**).

---

## 10. Full Upgrade List (~30 ship + backlog)

Rarity: Standard / Rare / Epic / Exotic / Oddity  
Tags: A Scattercoil · B Railstake · C Dynamo · G Glue  
Cell rule: Exotics larger; all Exotics same cell count.  
Names are player-facing (renames allowed). Vanilla names in fate table.

------------------------------------------------------------------------------
PATH A — SCATTERCOIL                                      [~9]
------------------------------------------------------------------------------

A1. Fractured Rapture — Exotic (Keystone) [Splintered Lance]
    Fire multiple coils with little accuracy. Light +Auger width.
    Volume crown.

A2. Micro Detonators — Exotic (Keystone) [REWRITE]
    Killing a target has a chance to create **one** elemental explosion.
    **No chain.** Explosion uses gun element. Joust-kills eligible.

A3. Half Cocked — Epic (or Exotic if elevated)
    +Charge duration; can fire while charging; damage/size/range scale with charge.
    Path-owned only — not baseline.

A4. Compressor — Rare
    +Bullet size; −range. Scatter fattening.

A5. Notched Breakpoint — Rare
    Greatly improves vertical grouping when Fractured Rapture equipped (fan, not blob).

A6. Crescendo — Epic
    Damage increases with each shot fired before reloading (stacking with DR on multi copies).
    Refund-meta friendly (doesn't need empty mag).

A7. Pack Dive — Rare [NEW — Auger Scatter rider]
    Auger deals +damage vs enemies within a short radius of another enemy (clump brawler).
    On joust-kill: small chance at Micro-style single spark if Micro not equipped (tiny); if Micro equipped, +det size once.

A8. Overload Shell — Rare [NEW]
    Modest +% damage. Coils slightly larger. Honest Scatter glue.

A9. Triple Feed Capacitor — Standard [Triple A DNA]
    +Magazine size.

------------------------------------------------------------------------------
PATH B — RAILSTAKE                                        [~9]
------------------------------------------------------------------------------

B1. Godrail — Exotic (Keystone) [Railshot elevated]
    Fire in a tight needle line. Greatly +range, +damage, +element amount.
    Auger becomes longer/thinner and slightly harder-hitting.

B2. Skewer — Epic (demoted from exotic budget if needed; listed Epic in frozen 30)
    −Base damage; +damage per pierced target/part.
    Auger ticks can add pierce stacks for next M1 (cap).

B3. Swing Through — Rare
    Piercing multiple targets builds extra damage for your next shot (crosshair gauge DNA).

B4. Shield Breaker — Rare [NEW — was baseline free]
    Coils pierce shields. Optional rider: Auger also pierces shields (prefer **both** for dual-mode honesty).

B5. Coil Extension — Standard [keep identity]
    +Range.

B6. Extra Capacitors — Epic
    Greatly +range; +recoil. Rail reach tax.

B7. Black Market Battery — Epic
    Greatly +damage and +charge-up time. Deliberate stake.

B8. Auxiliary Chargers — Standard
    +Damage; +charge-up time. Smaller Black Market.

B9. Lined Joust — Rare [NEW — Auger Rail rider]
    After Auger hits at least one enemy, your next M1 coil gains bonus pierce damage
    (small free taste of Skewer; stacks additively-capped with Skewer).

B10. Leyline — Epic [NEW v1.1 — RMB swap]
    Replace Auger on RMB with a rideable rail laid along the aim/shot line.
    Mount and ride. Auger-path cards idle unless they define a rail rider.
    **Promotion candidate** for frozen 30 (see backlog note).

B11. Inverted Spear of Heaven — Exotic/Epic [Beam Ripper — Inverted Spear of Hell]
    Javelin coil: +impact damage; delayed shock (gun-element) explosion.
    Backlog / promotion candidate.

B12. Straggler's Wake — Exotic/Epic [Beam Ripper — Straggler's Revenge]
    After a short delay, path-trail explosions follow the projectile line.
    Not Micro Det chain. Backlog / promotion candidate.

B13. Third Law — Epic [Beam Ripper]
    Firing knocks you backward. Shots that hit nothing refund ammo to reserves.
    Backlog / promotion candidate.

B14. Energy Inversion — Epic/Rare [Beam Ripper]
    Damage falloff reversed; max damage increased. Long-stake authority.
    Backlog.

B15. Arise — Rare [Beam Ripper]
    Levitate upward while charging. Backlog.

B16. Inertial Overload — Rare [Beam Ripper]
    +Damage while airborne. Backlog.

B17. Running Hot — Rare [Beam Ripper]
    Direct hits +damage while sprinting. Backlog.

------------------------------------------------------------------------------
PATH C — DYNAMO                                           [~10]
------------------------------------------------------------------------------


C1. Gearshift — Exotic (Keystone)
    While Auger is active and for a short time after: +charge speed, +DR
    (optional modest +M1 damage during window).

C2. Dynamo Core — Exotic (Keystone) [NEW]
    Build Dynamo stacks from M1 hits and Auger ticks. Next coil or next Auger
    consumes stacks for empowered damage/duration/size. Cap + out-of-combat decay.
    Replaces multi-Auger exotic stacking fantasy.

C3. Exit Storm — Epic [Storm's Eye REWRITE]
    When Auger ends, fire an additional coil (upward/forward) with +size/+damage.
    **Not** last-mag gated. Scales lightly with Dynamo stacks if present.

C4. Deep Bore — Epic [multi-Auger replacement]
    +Auger duration, +Auger damage, +Auger size. The “more drill” card without duplicate exotics.

C5. Centripetal Cornering — Rare
    +Turning speed while Auger active.

C6. Forward Charge — Rare
    +Move speed while charging M1 or Auger.

C7. Fisticuffs — Rare [rewrite lean Dynamo]
    Dealing melee **or Auger contact** damage briefly +Shocklance/Rapture damage and refunds some reserve ammo.

C8. Killing Field — Epic
    Coils linger and +outgoing damage for you while inside.

C9. Subway — Epic
    Coils linger and +move speed for any player inside.

C10. Battery Recursion — Epic [also absorbs Beam Ripper Safety Override fantasy]
    After the first charged shot in a mag, consecutive shots fire full-auto without charging
    until reload or swap. Tempo card; soft-tensions Half Cocked.
    **Safety Override** (Beam Ripper full-auto + mag) merges here — no separate Safety Override card in frozen pool.


C11. Ready and Waiting — Rare
    Weapon passively charges over time (M1 charge); slowed when unequipped.
    Does not charge Auger by default (keep RMB intentional) unless playtest wants both.

C12. Laying Cable — Epic (co-op) [exotic demoted]
    Press ability/interact binding (vanilla E spirit) to tether to another employee:
    you +DR/+damage; ally movement ability recharges faster.
    Dual-mode: while tethered, Auger and M1 both benefit from your +damage.

------------------------------------------------------------------------------
GLUE / ELEMENT / GUNFEEL                                  [~8]
------------------------------------------------------------------------------

G1. Coated Coils — Standard
    Coils **and Auger** apply more element.

G2. Fwoosh — Rare
    Gun element → Fire (M1 + Auger).

G3. Fester — Epic
    Gun element → Decay (M1 + Auger).

G4. Squire Supplier — Standard
    +Reload speed.

G5. Gladiator's Pockets — Standard
    +Ammo reserves.

G6. Flurrying Configuration — Rare
    +Charge speed (M1); −magazine size.

G7. Boundary Incursion — Oddity
    +Upgrade grid size.

G8. Ion Sheath — Standard [NEW — distributed %]
    Modest +% damage (both modes).

G9. Aftercoil — Rare [NEW]
    Shortly after firing M1, +Auger charge speed (tiny Dynamo taste without exotic).

G10. Spark Gap — Rare [NEW]
    Shortly after Auger ends, +M1 damage briefly (Gearshift-lite; does not replace Gearshift).

------------------------------------------------------------------------------
FROZEN v1 SHIP POOL (exactly 30)
------------------------------------------------------------------------------

EXOTIC (6)
  1  Fractured Rapture     (A)  — multi-coil
  2  Micro Detonators      (A)  — single kill explosion; NO chain
  3  Godrail               (B)  — elevated Railshot package
  4  Gearshift             (C)  — post-joust window
  5  Dynamo Core           (C)  — mode↔mode stacks
  6  Half Cocked           (A)  — charge curve (path-owned)
      ※ If Half Cocked feels too strong as exotic, swap #6 with Deep Bore or
        Laying Cable and demote Half Cocked to Epic (replace an Epic below).
      ※ Skewer is Epic #8 — demoted by design to fit budget.

EPIC (8)
  7  Crescendo             (A)  — per-shot ramp before reload
  8  Skewer                (B)  — per-pierce damage
  9  Extra Capacitors      (B)  — range + recoil
  10 Black Market Battery  (B)  — damage + charge time
  11 Exit Storm            (C)  — Storm's Eye rewrite (Auger end)
  12 Deep Bore             (C)  — Auger duration/damage/size
  13 Killing Field         (C)  — linger +damage
  14 Battery Recursion     (C)  — full-auto follow-up shots

RARE (10)
  15 Compressor            (A)  — size up, range down
  16 Notched Breakpoint    (A)  — fan grouping with Fractured
  17 Pack Dive             (A)  — Auger clump brawler
  18 Swing Through         (B)  — multi-pierce → next shot
  19 Shield Breaker        (B)  — shield pierce (M1 + Auger)
  20 Lined Joust           (B)  — Auger → next coil pierce amp
  21 Centripetal Cornering (C)  — Auger turn rate
  22 Forward Charge        (C)  — move while charging
  23 Fisticuffs            (C)  — melee/Auger contact → dmg + ammo
  24 Spark Gap             (G)  — post-Auger M1 amp (lite)

STANDARD (5)
  25 Coil Extension        (B)  — +range
  26 Auxiliary Chargers    (B)  — +dmg +charge time
  27 Triple Feed Capacitor (A)  — +mag
  28 Coated Coils          (G)  — +element appl both modes
  29 Ion Sheath            (G)  — modest +% damage both modes

ODDITY (1)
  30 Boundary Incursion    (G)  — +grid size

BACKLOG (designed, expand later)
  Subway, Laying Cable, Ready and Waiting, Flurrying Configuration,
  Squire Supplier, Gladiator's Pockets, Fwoosh, Fester,
  Overload Shell, Aftercoil, Pack Dive upgrades,
  Secondary Charge (optional rare CHAIN det — explicitly separate from Micro),
  Multiversal Thievery / Edge Fault (contraband parity only if desired),
  Godrail pierce+1 fold variants, Dynamo manual reload discharge (S3),
  Exit Storm reload variant (S2) / Leyline-end variant (S4),
  Skewer exotic elevation if budget expands past 6.

BEAM RIPPER ABSORB + LEYLINE (v1.1 — backlog / promotion candidates; frozen 30 unchanged)
  ★ Leyline (B Epic) — RMB rideable rail; top promote if Rail mobility fantasy is missing
  ★ Inverted Spear of Heaven (B Exotic/Epic) — javelin delayed boom
  ★ Straggler's Wake (B/A Exotic/Epic) — path-trail dets (not Micro chain)
  ★ Third Law (B/G Epic) — fire knockback + miss refund
  Energy Inversion, Arise, Inertial Overload, Running Hot
  Quick Charge → merge Flurrying; Battery Extension → merge Gladiator's
  Safety Override → merged into Battery Recursion (no separate card)

NOTE: Subway and Laying Cable are real fantasies — backlog first to keep 30 tight;
promote over weaker glue if co-op/field feel is missing in playtest.
NOTE (v1.1): Beam Ripper cards + Leyline stay **out of frozen 30** until a cut pass
picks what they displace (e.g. promote Leyline over Lined Joust / Extra Capacitors /
Black Market if line-mobility is the missing Rail beat).


------------------------------------------------------------------------------
CUT / DEMOTE FROM VANILLA IDENTITY
------------------------------------------------------------------------------

| Vanilla | Fate |
|---------|------|
| Auger Subroutine as exotic enable | **Baseline RMB**; removed from pool as enabler |
| Multi-Auger stack | **Deep Bore + Dynamo Core** |
| Auger element ≠ gun | **FIXED** — shared element always |
| Inherent shield pierce | **CUT from baseline** → Shield Breaker |
| Micro Detonators chain | **CUT** — single explosion only |
| Storm's Eye last-mag | **CUT** → Exit Storm on Auger end |
| Railshot as weak Rare | **Godrail Exotic** |
| ADS | **Removed** on this product |
| Half Cocked baseline | **No** — path-owned card only |
| Pre-nerf OP numbers | **Not returning** |
| Edge Fault / Multiversal | Optional contraband; not frozen 30 |
| Separate Beam Ripper rework product | **CUT** — fantasies absorbed into RC only |
| Beam Ripper Safety Override as own card | **Merged** into Battery Recursion |


---

## 11. Example Builds

### Pure Scattercoil
Compressor → Fractured Rapture → Notched Breakpoint → Half Cocked → Micro Detonators → Overload Shell / Ion Sheath → Pack Dive  
*Play:* Fat multi-coils, charge skill, single kill booms; Auger dives clumps for sparks.

### Poster shotgun (celebrated)
**Fractured Rapture + Half Cocked + Micro Detonators + Compressor + Notched Breakpoint**  
*Play:* Same fantasy as vanilla winner stack, dual-mode Auger free underneath. Not nerfed on purpose.

### Pure Railstake
Godrail → Skewer → Swing Through → Shield Breaker → Extra Capacitors → Lined Joust → Coil Extension  
*Play:* Needle stakes, pierce ramps, shields don't save them; Auger closes and primes Lined Joust.

### Railstake + Leyline (backlog promote)
Godrail → Skewer → Leyline → Shield Breaker → Third Law → Energy Inversion → Coil Extension  
*Play:* Lay rail on the stake line, ride in, dump needle coils; Auger idle by design.

### Beam Ripper DNA poster (backlog)
Godrail → Inverted Spear of Heaven → Straggler's Wake → Third Law → Half Cocked or Black Market  
*Play:* Javelin stakes with delayed boom + path trail; knockback kite. Not frozen 30 yet.


### Pure Dynamo
Dynamo Core → Gearshift → Deep Bore → Exit Storm → Killing Field → Forward Charge → Spark Gap → Battery Recursion  
*Play:* Joust → window → empowered coils / storm exit; stacks cash out; never needs empty mag.

### Scatter + Dynamo hybrid
Fractured Rapture + Micro Det + Gearshift + Dynamo Core + Half Cocked  
*Play:* Joust in, Gearshift window, dump multi-coils, Exit Storm if equipped, detonate leftovers.

### Rail + Dynamo hybrid
Godrail + Skewer + Gearshift + Dynamo Core + Shield Breaker + Lined Joust  
*Play:* Gap-close joust, windowed Godrail stakes into cores, pierce payoffs.

### Hybrid freak (trailer)
Fractured Rapture + Godrail + Dynamo Core + Micro Detonators  
*Play:* Soft tension on projectile shape; Dynamo still pays; chaos grid allowed — tune if broken.

### Co-op field (backlog promote)
Killing Field + Subway + Laying Cable + Gearshift + Coated Coils  
*Play:* Linger lanes, tether buddy, joust logistics.

---

## 12. Strengths, Weaknesses & Risks

### Strengths
- Dual-mode readable in seconds (M1 coil / RMB drill)
- Three doctrines all use both modes
- Scatter fantasy preserved and celebrated
- Rail finally has exotic authority
- Dynamo makes switching a build, not a footgun
- Auger element fixed; shield pierce earned
- Storm's Eye lives again under refund meta
- Micro Det less snowbally without chain
- Beam Ripper long-stake / path-det / knockback DNA has a home without a second product
- Leyline offers a true RMB identity fork (ride the line vs joust)

### Weaknesses / fun failure states
- Whiffed Auger into empty space
- Half Cocked undercharge plinks
- Godrail miss at long range
- Dynamo stacks decay / spent on wrong mode
- Fractured without Notched = vertical mess
- Battery Recursion + bad positioning = ammo hose into dirt
- No ADS — mid-long Rail is projectile skill, not zoom crutch
- Leyline into wall / void; dismount mid-pack
- Leyline build “forgets” Auger-path cards are idle

### Design risks
- **Scatter still outscales peers** — fix by elevating Godrail/Skewer/Dynamo EV, not gutting Micro/Fractured
- **Godrail + Skewer + Swing Through** boss melt — tune pierce ramps conservatively
- **Dynamo Core complexity** — UI stacks must be obvious
- **Exit Storm proc spam** if Auger too short cooldown — gate on hit or recovery
- **Element cards** must truly touch Auger (impl test)
- **Shield Breaker** too cheap → mandatory; keep Rare with a small trade or modest opportunity cost on grid
- **Leyline hard-swaps RMB** — must be loud in UI; Auger-path dead cards feel bad if not labeled
- **Straggler's + Micro Det** clear EV stack — separate verbs, shared budget watch
- **Primary carrying heavy DNA** — keep numbers primary-scale, not Beam Ripper 600-dmg sniper
- Network: Auger authority, det explosions, linger zones, tether, **rideable rail** mount/sync


---

## 13. Success Criteria / Player Fantasy Checklist

- [ ] Empty-grid Rapture's Chosen clears packs with coil + emergency Auger
- [ ] Pure Scattercoil competes for clear without feeling mandatory for all content
- [ ] Pure Railstake competes for ST/boss without shotgun stack
- [ ] Pure Dynamo feels complete living on mode swaps (not “missing a shape exotic”)
- [ ] Vanilla winner stack fantasy still slaps on Scatter cards
- [ ] Auger is available with zero Auger upgrades
- [ ] Auger deals Shock (or modified gun element) — never untyped disconnect
- [ ] Baseline does **not** pierce shields; Shield Breaker does (both modes)
- [ ] Micro Detonators never chain
- [ ] Exit Storm triggers without emptying the magazine
- [ ] Half Cocked is absent until equipped
- [ ] ADS is off; RMB is Auger **unless Leyline** replaces it
- [ ] Multi-Auger exotic stacking is gone; Deep Bore / Dynamo cover drill investment
- [ ] Vanilla Shocklance still exists untouched
- [ ] Vanilla Beam Ripper still exists untouched; **no separate Beam Ripper rework**
- [ ] Leyline (when equipped) lays a rideable rail on the shot line and disables Auger
- [ ] Beam Ripper card fantasies are design-owned here (backlog/promote), not a second mod
- [ ] Co-op: linger/tether/dets respect friendly rules; fun for allies
- [ ] Failure states stay fun


---

## 14. Universal Truths (Mycopunk alignment)

- Exotic shapes should always be larger than others; each exotic should use the same number of cells.
- v1 targets **~30** upgrades (frozen list above); backlog is real design, not trash.
- Three paths create different build options but **may intermingle** on the grid.
- Renames allowed; fate table keeps vanilla → new mapping.
- Prefer coil / auger / pierce / detonate / dynamo verbs over generic stickers only — Ion Sheath exists so light grids bite.
- Parallel product: **Rapture's Chosen**; vanilla **Shocklance** and **Beam Ripper** unmodified.
- Dual-mode is baseline; paths specialize both modes; no Auger-only path.
- **No separate Beam Ripper rework** — absorb upgrade DNA; delete that product line.
- RMB is Auger unless an explicit swap crown (Leyline) says otherwise.


---

## 15. Vanilla Shocklance → Rapture's Chosen Fate Table

| Vanilla name | Rapture's Chosen name | Path | Notes |
|--------------|----------------------|------|-------|
| (baseline shield pierce) | **Removed** | — | Shield Breaker rare |
| (baseline stats/charge) | Retune empty-grid honesty | — | Not pre-nerf OP |
| Auger Subroutine | **Baseline RMB** | — | Not an upgrade enabler; element synced |
| Multi-Auger | Deep Bore + Dynamo Core | C | No duplicate exotic stack |
| Gearshift | Gearshift | C | Exotic keystone |
| Centripetal Cornering | Centripetal Cornering | C | Keep |
| Forward Charge | Forward Charge | C | Charges both modes |
| Fisticuffs | Fisticuffs | C | +Auger contact |
| Splintered Lance | Fractured Rapture | A | Exotic |
| Compressor | Compressor | A | Keep |
| Half Cocked | Half Cocked | A | Path-owned; not baseline |
| Notched Breakpoint | Notched Breakpoint | A | Keep with Fractured |
| Micro Detonators | Micro Detonators | A | **No chain** |
| Railshot | Godrail | B | **Exotic elevated** |
| Skewer | Skewer | B | Epic in frozen 30 |
| Swing Through | Swing Through | B | Keep |
| Coil Extension | Coil Extension | B/G | Keep |
| Extra Capacitors | Extra Capacitors | B | Keep |
| Auxiliary Chargers | Auxiliary Chargers | B | Keep |
| Black Market Battery | Black Market Battery | B | Keep |
| Battery Recursion | Battery Recursion | C | Epic tempo |
| Ready and Waiting | Ready and Waiting | backlog | M1 passive charge |
| Crescendo | Crescendo | A | Keep |
| Storm's Eye | Exit Storm | C | **Auger-end rewrite** |
| Killing Field | Killing Field | C | Keep |
| Subway | Subway | backlog | Promote if field feel thin |
| Laying Cable | Laying Cable | backlog | Co-op; demoted from exotic pressure |
| Coated Coils | Coated Coils | G | Both modes |
| Fwoosh | Fwoosh | backlog | Element both modes |
| Fester | Fester | backlog | Element both modes |
| Flurrying Configuration | Flurrying Configuration | backlog | |
| Squire Supplier | Squire Supplier | backlog | |
| Gladiator's Pockets | Gladiator's Pockets | backlog | |
| Triple A | Triple Feed Capacitor | A | +mag |
| Boundary Incursion | Boundary Incursion | G | Keep |
| Edge Fault / Multiversal | optional | — | Not frozen 30 |
| (none) | Dynamo Core | C | NEW mode stacks |
| (none) | Shield Breaker | B | NEW earned shield pierce |
| (none) | Pack Dive | A | NEW Auger scatter rider |
| (none) | Lined Joust | B | NEW Auger rail rider |
| (none) | Deep Bore | C | NEW drill investment |
| (none) | Spark Gap | G | NEW lite post-joust M1 |
| (none) | Ion Sheath | G | NEW % glue |
| (none) | Aftercoil | backlog | post-M1 Auger charge |
| (none) | Leyline | B | NEW v1.1 — RMB rideable rail (replaces Auger) |

### 15b. Vanilla A16 Beam Ripper (`ChargeSniper`) → Rapture's Chosen Fate

| Vanilla Beam Ripper | Rapture's Chosen name | Path | Notes |
|---------------------|----------------------|------|-------|
| (weapon / heavy product) | **No rework product** | — | Vanilla heavy left untouched; fantasies absorbed here |
| Inverted Spear of Hell | Inverted Spear of Heaven | B | backlog / promote; javelin + delayed boom |
| Straggler's Revenge | Straggler's Wake | B/A | backlog / promote; path-trail dets; **not** Micro chain |
| Third Law | Third Law | B/G | backlog / promote; knockback + miss refund |
| Safety Override | → Battery Recursion | C | **Merged**; no separate card |
| Energy Inversion | Energy Inversion | B | backlog; reverse falloff |
| Arise | Arise | B/C | backlog; levitate while charging |
| Inertial Overload | Inertial Overload | B | backlog; +dmg airborne |
| Running Hot | Running Hot | B/G | backlog; sprint direct-hit amp |
| Quick Charge | → Flurrying / glue | G | merge charge-speed fantasy |
| Battery Extension | → Gladiator's Pockets | G | merge reserves |
| (none on BR) | Leyline | B | NEW — rideable rail on shot line; RMB swap |

---

## 16. Implementation Notes (for later coding passes)


### 16.1 Product / registration

- New primary via weapon template: clone **vanilla Shocklance** gun type (not CartridgeSMG)
- Unique gear id + APIName e.g. `raptures_chosen` / id in high custom range
- Display name **Rapture's Chosen**
- `PlayerData.CreateUpgrade` pool; SpawnGear remap + stamp identity + ApplyUpgrades
- `[MycoMod(..., ModFlags.IsSandbox)]`
- Do **not** remove vanilla Shocklance from AllGear
- Do **not** remove vanilla Beam Ripper (`ChargeSniper`) from AllGear — absorb design only


### 16.2 Host behaviour

`RapturesChosenBehaviour` (or subclass when prefab exists) holding:

```
// identity / modes
bool augerBaselineEnabled;       // true unless leylined
bool leylined;                   // RMB swap: rideable rail; disables Auger
bool canAim;                     // false


// element
EffectType gunElement;           // Shock default; Fwoosh/Fester/etc. set

// scatter
int bonusCoils;
float coilSizeMult;
float spreadAdd;
bool halfCocked;
float halfCockedChargeBonus;

// rail / railstake
bool godrail;
float railRangeMult;
float railDamageMult;
bool skewer;
float skewerPerPierce;
bool shieldBreaker;
float swingThroughCharge;

// beam ripper DNA (upgrade-gated)
bool invertedSpear;
bool stragglersWake;
bool thirdLaw;
float fireKnockback;
bool energyInversion;
bool ariseWhileCharging;
bool inertialOverload;
bool runningHot;


// detonate
bool microDet;
float microDetChance;
float microDetSize;
float microDetDamage;
// NO chain flag

// dynamo
bool dynamoCore;
int dynamoStacks;
int dynamoStackCap;
float gearshiftWindow;
float gearshiftChargeMult;
float gearshiftDR;
bool exitStorm;
bool deepBore;
float augerDurationMult;
float augerDamageMult;
float augerSizeMult;

// glue
float damageMultiplier;          // aggregate % both modes where applicable
```

### 16.3 Hooks (draft)

| Area | Approach |
|------|----------|
| Baseline Auger | Port Auger Subroutine behaviour to RMB; strip R binding; always on unless Leyline |
| Auger element | Force drill damage effect = gunElement every tick |
| Shield pierce | Clear baseline flag; set only with Shield Breaker on coil + Auger traces |
| M1 fire | Charge gun path; Half Cocked mutates ChargeData; Godrail/Fractured/Spear/Straggler mutate bullet |
| OnKill | Micro Det single explosion; no recursive OnKill from det |
| Auger end | Exit Storm; Gearshift window start; Dynamo convert optional |
| Leyline RMB | Aim ray → spawn rideable rail; mount player; disable Auger input path |
| Leyline end | Optional S4 Exit Storm; clear rail GO |
| Third Law | On fire: AddForce back; on zero-hit shot: refund reserve |
| On pierce | Skewer / Swing Through / Lined Joust |
| Apply upgrades | Sum mults; disable canAim; sync element; if leylined set auger off |
| Network | Auger motion owner-auth; det RPC; linger zones; tether; **rail mount sync** |


### 16.4 Input map

```
M1     → coil charge/fire (Gun charge path)
RMB    → Auger charge/release  OR  Leyline rail lay (if leylined)
R      → reload only
Aim    → disabled
```

Match PesticideRework / Aussie Blightflame lesson: when an ability owns RMB, ADS is gone.
Leyline is the only v1.1 crown that **replaces** Auger on RMB (hard swap).


### 16.5 Micro Detonators impl note

```
OnKill:
  if microDet && roll(chance):
      SpawnExplosion(once)
      // DO NOT register kills from this explosion as Micro Det sources
      mark explosion with NoMicroDetChain flag / separate damage channel
```

### 16.6 Related mods / DNA (not required at runtime)

| Source | DNA |
|--------|-----|
| Vanilla Shocklance | Coil charge, Auger, upgrade verbs |
| Vanilla A16 Beam Ripper (`ChargeSniper`) | Charge piercer; ammo regen HUD; fireKnockback; explosion-on-saturated; FastAirborneRecharge; upgrade package absorbed |
| PesticideRework | Input owns mode; ADS off |
| ArcLightningRework | Optional pierce/drill chain shock later |
| HeavensFury | Optional smite-on-condition backlog |
| Ballista zipline / movement attach (decompile) | Leyline rideable-rail mount reference |
| DMLR / Junk / Aussie / Trident / Cycler docs | Structure, frozen 30, fate, soft crowns |
| Weapon template | Registration, SpawnGear stamp, CreateUpgrade |

### 16.7 Decompile note

**Before impl:** locate Shocklance gun class, Auger upgrade property, shield pierce flag, charge data, and bullet type under `.Resources/Current-Assembly-CSharp`.  
**Beam Ripper confirmed:** `ChargeSniper.cs`, `ChargeSniperUpgradeFlags.cs`, `ChargeSniperHUD.cs` — fields `ammoRegenMultiplier`, `ammoOnDamageMultiplier`, `fireKnockback`, `explosionRadius` / damage / effect, `FastAirborneRecharge`.  
Do not invent member names. Leyline may need movement/zipline types (e.g. Ballista zipline DNA) — search at impl.


---

## 17. Open Tuning Questions (playtest, not design blockers)

1. Half Cocked as Exotic #6 vs Epic swap with Deep Bore / Laying Cable.
2. Godrail damage/range package vs Fractured+Compressor+Half Cocked EV.
3. Skewer per-pierce curve so bosses don't melt with Godrail+Swing Through.
4. Micro Det single-boom chance/size without chain.
5. Dynamo stack build rate, cap, spend formula, decay.
6. Exit Storm: always on Auger end vs requires ≥1 enemy hit; recovery gate.
7. Gearshift window duration and whether +M1 damage is included.
8. Shield Breaker: any trade (slight −damage vs shields-only content?) or pure upside Rare.
9. Battery Recursion interaction with Half Cocked (document in UI).
10. Baseline Auger damage so empty-grid joust is useful but not old multi-Auger.
11. Whether Ready and Waiting should also trickle Auger charge (default no).
12. Promote Subway / Laying Cable into frozen 30 if co-op fantasy feels thin.
13. **Leyline:** Epic vs Exotic; rail length/duration/speed; ally ride?; S4 Exit Storm on rail end?
14. Which frozen-30 cards Leyline / Spear / Straggler's displace if promoted?
15. Straggler's Wake + Micro Det stacked clear EV.
16. Primary-scale numbers for Beam Ripper DNA (not heavy 600-dmg sniper).

---

## 18. Locked Decisions Log

| Decision | Lock |
|----------|------|
| Ship name | **Rapture's Chosen** (`RapturesChosen`) |
| Product shape | **Parallel primary**; vanilla Shocklance **and** Beam Ripper untouched |
| Beam Ripper rework product | **CUT** — upgrade DNA absorbed here only |
| Path A | **Scattercoil** — Fractured Rapture + Micro Detonators (+ Half Cocked) |
| Path B | **Railstake** — Godrail + Skewer spine (+ Leyline / BR DNA backlog) |
| Path C | **Dynamo** — Gearshift + Dynamo Core; dual-mode tempo |
| Auger | **Baseline RMB**; not a path; all paths incorporate both modes |
| Auger element | **Shares gun element always** |
| RMB ownership | **Auger default**; **Leyline** may hard-swap RMB to rideable rail |
| Leyline | Path B Epic (default); backlog / promotion candidate; not frozen 30 yet |
| Shield pierce | **Upgrade-only** (Shield Breaker) |
| Micro Detonators | **No chain** |
| Straggler's Wake | Separate path-trail verb; **not** Micro chain |
| Storm's Eye | **Exit Storm** on Auger end (not last mag); S4 optional if Leyline |
| Half Cocked | **Path-owned only** (not baseline); rarity flexible (frozen as Exotic #6) |
| ADS | **Off** |
| Multi-Auger stack | **Removed**; Deep Bore + Dynamo Core |
| Safety Override (BR) | **Merged** into Battery Recursion |
| Exotic overflow | **Demote rarity**, don't delete fantasy |
| Renames | **Allowed** |
| Doc scope | **Full** frozen 30 + fate + impl + BR absorb |
| External deps | Pattern/DNA only; no hard runtime deps |
| Pre-nerf OP | **Not returning** |

### Design changelog

#### v1.1
- Absorb **A16 Beam Ripper** (`ChargeSniper`) upgrade DNA; **no separate BR rework**
- Vanilla Beam Ripper left in heavy slot untouched
- **Leyline** — Epic Path B crown: RMB rideable rail along shot line; hard-swaps Auger
- Crowns/drafts: Inverted Spear of Heaven, Straggler's Wake, Third Law + BR support package
- Safety Override → Battery Recursion merge
- Pillars/inputs/priority/vocabulary/matrix updated for RMB swap + BR verbs
- Fate table §15b; backlog promotion candidates; frozen 30 unchanged pending cut pass
- Impl notes: `ChargeSniper` fields, Leyline hooks, rail network

#### v1
- Rapture's Chosen identity; parallel product
- Baseline dual-mode: M1 coil + RMB Auger
- Paths: Scattercoil / Railstake / Dynamo — all dual-mode
- Auger element sync; shield pierce gated; Micro Det no chain
- Storm's Eye → Exit Storm (Auger end)
- Railshot → Godrail Exotic; Dynamo Core replaces multi-Auger
- Half Cocked path-owned; ADS off
- Frozen 30 + backlog + fate table + impl notes

---

## 19. Next Steps After This Doc

1. Confirm Half Cocked stays Exotic #6 vs Epic swap after a gut-check pass.
2. Confirm decompile type names for Shocklance / Auger / shield pierce (+ Leyline mount/zipline DNA).
3. Decide Leyline / Spear / Straggler's promote vs stay backlog (and what they cut from frozen 30).
4. Implement Rapture's Chosen clone registration from Shocklance.
5. Implement behaviour host: no ADS, RMB Auger, shared element, Leyline flag.
6. Strip baseline shield pierce; implement Shield Breaker.
7. Implement Scatter package (Fractured, Compressor, Half Cocked, Micro Det no chain).
8. Implement Rail package (Godrail, Skewer, Swing Through, Lined Joust).
9. Implement Dynamo package (stacks, Gearshift, Deep Bore, Exit Storm).
10. Optional: Leyline rail + Beam Ripper DNA cards when promoted.
11. Register frozen pool; icons/strings.
12. Balance pass: empty grid, pure A/B/C, poster Scatter, Rail+Dynamo, hybrid freak, Leyline builds.
13. Optional: promote Subway / Laying Cable / element cards from backlog.

---

*End Rapture's Chosen Design Doc v1.1*

