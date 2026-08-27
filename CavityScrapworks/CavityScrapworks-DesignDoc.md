# Cavity Scrapworks — Design Document (v1)

> Status: **Design only** — no implementation yet.
> Working title in notes: Plate Launcher Rework. **Ship name: Cavity Scrapworks.**
> Template base: `.new.PlateLauncherRework` weapon content project.
> Product shape: **separate primary gear** — vanilla FR.15833 Plate Launcher is left unmodified.

---

## 0. Locked Decisions (2026-08-08)

| Decision | Lock |
|----------|------|
| Product shape | Parallel new primary — vanilla Plate Launcher untouched |
| Player-facing name | **Cavity Scrapworks** |
| Paths | **Salvo / Lattice / Interdictor** |
| Universal exotic | **Clearing Plasma** (blades on live plates — any path) |
| Vector bounce | **Remove bounce block only** — Vector plates bounce again; not a separate design fork |
| High Ground | **Lattice spine** — stand-on-plate combat buff is field identity, not a third path |
| Improvised Explosives | **No grenade tax** — plate-native arm/detonate from mag/recall/catch economy |
| Painter's Attachment | Self-sufficient — plates enable acid/corrosion without external setup |
| Nice Catch | Rewrite lethality; glue spice, not a path |
| Catcher-as-path | **Cut** — recall/catch toys are glue or Lattice riders |
| Kitchen-sink hybrid | **Celebrated** (Salvo volume feeding Lattice field) — elevate peers, do not nerf sink |
| Soft crowns | Hybrids allowed; **no hard exclusion matrix** |
| Rename pass | Full new player-facing names OK (vanilla names = DNA only; KEEP called out) |
| Pool | **~30** ship upgrades; exotic shapes larger & equal cell count |
| Baseline | Honest stick → recall → catch; no free Vector / fence / High Ground / salad mag |
| Recall input | Vanilla-feel sacred; paths modify plates, do not steal baseline recall identity |
| MycoMod (impl) | IsSandbox |
| Working APIName | `cavity_scrapworks` |
| Working GUID | `sparroh.cavityscrapworks` |
| Doc scope | Full bible |

---

## 1. High Concept / Fantasy

**Cavity Scrapworks** is the SAXON cavity-mag plateworks that finally has more than one career.

Vanilla Plate Launcher is fun and unique — heavy metal plates that stick and recall magnetically — but real power collapses into one kitchen-sink sentence:

> Fencing Construction + Clearing Plasma + Damage Arc + Compressed Plate Storage + Salad Plates

Vector Interdiction is the more interesting single-plate pilot fantasy, but bounce was code-blocked so it cannot compete. Improvised Explosives taxes grenades for crumbs. Painter's Attachment needs corrosion you cannot force. Nice Catch kills you. High Ground, Impulse, and dual-magnetism never form a peer career.

Cavity Scrapworks keeps stick → recall → catch and rebuilds the grid around three peer loops plus one universal blade exotic:

- **Salvo** — rapid plate gun (mag, RoF, cycle, hose)
- **Lattice** — stuck plate field (fence, elemental arc, High Ground throne)
- **Interdictor** — single-plate pilot (Vector redirect; **bounce restored**)

**Clearing Plasma** is universal: spinning light blades on live plates reward both volume and planted fields.

**One-liner:** *Hose the room, fence the room, or fly one plate through the room. Bounce is not a myth.*

**Element spine:** Normal at baseline. Shock is first-class Lattice glue (Fence + Electrified + Charge Recycling). Fire / Acid / Bees opt-in via payload cards — not required for identity.

**SAXON marketing blurb (draft):**
  “SAXON CS-8 Cavity Scrapworks — Cavity-fed magnetic plateworks for personnel who
  solve architecture with scrap. Multi-plate lattice packages, rapid cavity magazines,
  and single-plate interdiction yokes are field-swappable. Plates may bounce. Legal
  insists we clarify that was always the intent.”

Optional stingers:
- “If the plate is still out, the room is still yours.”
- “One plate, well flown, outranks a salad you cannot aim.”
- “Stand on your work. DR is a professional courtesy.”
- “We removed the bounce inhibitor. You are welcome.”

---

## 2. Role & Fantasy in the Arsenal

| Trait | Value |
|-------|--------|
| **Slot** | Primary |
| **Range** | Mid (plate travel readable; Interdictor extends via bounce lines; Salvo is closer hose) |
| **Role** | Magnetic scrap controller — volume plate gun, planted field denial, or single-plate pilot |
| **Gap filled** | Vanilla Plate is one kitchen sink + a weak Vector trap. Scrapworks is three peer loops + universal blades |
| **Synergies** | Shock status, movement (catch / Impulse), positioning (High Ground), BounceIndicatorPlus soft QoL |

**Product shape:** New primary (**Cavity Scrapworks**). Does **not** replace or patch vanilla Plate Launcher.

**Not trying to be:** Hard-Light Constructor (paint/walkable light), Photon Disc (grenade disc world), Siege Cannon (explosive shells), DMLR (anatomy transfer), or a mandatory external-mod dependency.

### 2.1 Comparison snapshot

```
Weapon / kit              Niche                         Scrapworks differentiator
------------------------  ----------------------------  ------------------------------------------
Vanilla Plate Launcher    Stick/recall (1D sink)        Parallel pool; peer Salvo/Lattice/Vector
Hard-Light Constructor    Paint walkable light + Jam    Metal plates + magnet recall; no RMB brush
Photon Disc / Disc World  Disc grenade / gun override   Plate physics + catch; DiscWorld excludes Plate
Siege Cannon              Explosive shells / halo       Magnetic scrap, not CAS boom
DMLR                      Anatomy transfer laser        Geometry + pilot, not part transfer
Hive Launcher             Hover-dive organic swarm      Hard plates, fence, catch throne
Friend in a Box           Deployable ally               Your plates, not a pet
```

### 2.2 Boundary vs Hard-Light Constructor (LOCKED)

| Noun | HLC | Cavity Scrapworks |
|------|-----|-------------------|
| Material | Hard-light plasma / panes | Heavy metal plates |
| Place verb | RMB Paint (stamp/stroke) | M1 fire → stick |
| Retrieve | Duration / charge expire | **Magnetic recall + catch** |
| Stand buff | Bridger High Ground on light | **Lattice High Ground on your stuck plate** |
| Control | Shatter → Jam (no DoT) | Stick damage, fence, blades, pilot |
| Identity | Fabricator architecture | Scrap magnet yo-yo |

Do **not** add free RMB paint bridges or Shatter. Standing on a plate you fired is allowed and Lattice-owned.

---

## 3. Design Pillars

1. **Three peer loops** — Salvo / Lattice / Interdictor. Each answers “how do I use plates?” differently.
2. **Kitchen sink is celebrated** — Salvo+Lattice hybrid remains a poster stack; fix 1D by elevating peers, not nerfing the sink.
3. **Vector bounce block is removed** — implementation fix, not a balance essay. Interdictor must be able to bounce.
4. **Clearing Plasma is universal** — blades on live plates serve every path; not locked to Salvo or Lattice alone.
5. **High Ground is Lattice spine** — stand-on-plate DR + damage is field identity.
6. **On-hit / geometry / pilot verbs > flat % only** — honest % glue still exists so light grids bite.
7. **No grenade tax on Improvised** — arm/detonate is plate-native (mag, recall, catch, or arm state).
8. **Painter is self-sufficient** — acid/corrosion comes from the card or the plate, not “become corroded somehow.”
9. **Nice Catch is spicy, not suicide** — risk/reward kept; default must not one-tap the player.
10. **Baseline is honest stick/recall/catch** — crowns own multi-mag, fence, Vector, High Ground, blades.
11. **Soft crowns only** — hybrids allowed; Vector’s 1-plate cap soft-tensions multi-plate cards (underperform, don’t brick).
12. **~30 upgrades for v1** — exotic shapes larger than others; each exotic same cell count; rename pass OK.
13. **Failure states stay fun** — whiffed redirect, empty cavity mid-hose, standing off your only plate, bounce into void, over-recall.
14. **Vanilla Plate remains** for players who want the old pool.

---

## 4. Core Mechanics & Gunfeel (Baseline)

### 4.1 Vanilla Plate Launcher reference (wiki spirit — confirm in decompile at impl)

```
Damage 76, Element Normal
RPM 150, fire interval 0.4s
Mag 1, ammo capacity 100, reload 1.5s
Bullet speed 320
Falloff start 250 / end 500 / max range 500 / min mult 0.75
Spread 0; recoil X 3–4, Y 10
Identity: heavy plate stick → magnetic recall → catch
```

Confirm concrete class (`PlateLauncher` and related bullet/plate types) in decompile before clone.

### 4.2 Base gun (no upgrades)

| Trait | Draft / intent |
|-------|----------------|
| Fire mode | Semi / deliberate plate launch (vanilla spirit; mag 1 baseline) |
| Behavior | Plate flies, sticks in enemy or terrain, recalls magnetically, catch returns to gun |
| Damage | Honest mid-heavy impact; empty-grid playable |
| Element | Normal |
| Magazine / reserve | **1** / high reserve spirit (~100) — paths mutate mag hard |
| Reload / cavity | Reload beat when dry; recall/catch is the signature tempo |
| Multi-plate / fence / blades / Vector / High Ground | **OFF** until upgrades |
| Bounce | Baseline bounce only if vanilla empty-grid has it; **Interdictor guarantees bounce via Vector fix + Bouncy supports** |
| ADS | Normal unless a card says otherwise |
| Model / audio | Borrow Plate Launcher until custom art |
| Hold-R / RMB specials | **None** on baseline beyond vanilla aim/recall ownership |

### 4.3 Inputs

| Input | Baseline | With crowns |
|-------|----------|-------------|
| **M1** | Fire plate | Salvo multi-mag hose; Lattice plant; Interdictor single plate |
| **M1 hold / press after fire** | Vanilla | **Vector:** redirect in-flight plate |
| **RMB / AIM** | ADS if any | Unchanged unless a future card claims it (v1 prefers not to steal AIM) |
| **R tap** | Reload | Reload only |
| **R hold** | Nothing baseline | Reserved; v1 frozen 30 prefers not to require hold-R |
| **Catch** | Plate returns to gun (vanilla) | Impulse / Nice Catch / economy riders |
| **Stand on plate** | Physics only | **High Ground** combat buff when Lattice card equipped |

### 4.4 Baseline combat loop

```
M1 → fire plate → stick in target/terrain
    → recall (vanilla trigger) → plate returns, damages on the way
    → catch → cavity ready
    ↘ without crowns: honest yo-yo scrap (not waiting for Salad or Fence)
    ↘ Salvo: multi-plate dump → blades/impact → fast cycle recall
    ↘ Lattice: plant field → fence/arc/stand → cascade payoff → recall when spent
    ↘ Interdictor: one plate → redirect → bounce lines → Power Recall cash-out → catch
```

Skill without upgrades: lead the plate, choose stick vs terrain plant, time recall through packs, don’t face-tank during slow cavity.

### 4.5 What baseline does NOT include

- No mag > 1 / Salad hose
- No Fencing arcs
- No Clearing Plasma blades
- No Damage Arc elemental boom
- No Vector redirect
- No guaranteed bounce package
- No High Ground combat buff
- No Auto-Demolition
- No Improvised arm
- No Painter acid puddles
- No Nice Catch risk package
- No Impulse catch boost

Those are path-, universal-, or glue-owned.

### 4.6 Plate lifecycle vocabulary (shared)

```
Fire     → plate leaves the cavity
Flight   → travel, optional bounce / redirect / pierce
Stick    → embed in enemy part or terrain (field noun for Lattice)
Live     → plate exists in world (blades, fence, High Ground eligible)
Recall   → magnetic return; damage window on the way home
Catch    → plate re-enters gun; tempo / risk / mobility hooks
Arm      → upgrade-gated armed state (Improvised rewrite, etc.)
```

---

## 5. Shared Framework Vocabulary

Upgrades speak these verbs. Baseline owns Fire / Stick / Recall / Catch only.

### 5.1 Cavity (magazine)
- How many plates may be live / fired before recall pressure
- Salvo owns cavity volume (Salad, Compressed Storage)
- Lattice wants enough plates for a field (Fence +1 mag DNA) without becoming pure hose
- Interdictor **caps at 1** while Vector is equipped

### 5.2 Live plate
- Any plate currently in the world
- Clearing Plasma blades tick while live
- Fence links live plates to each other
- High Ground requires standing on **your** live stuck plate

### 5.3 Field (Lattice)
- 2+ live stuck plates (or 1 plate + throne play) forming denial geometry
- Fence arcs, Damage Arc targets, Shockwave plants, Becall cascade on mass recall

### 5.4 Redirect (Interdictor)
- Player steers the single live plate (Vector press/hold)
- Bounce is first-class after block removal
- Skill expression: lines, banks, multi-hit geometry

### 5.5 Recall window
- Damage mult while returning (Power Recall DNA)
- Bee explosions (Becall), cascade clears, catch prep
- Auto-Demolition forces early recall for Salvo tempo

### 5.6 Catch beat
- Plate returns to cavity
- Impulse (mid-air boost), Nice Catch rewrite (risk/reward), economy crumbs
- Not a full path — glue and Lattice/Salvo tempo spice

### 5.7 Blade aura (Clearing Plasma — universal)
- Live plates spin light blades; slice nearby enemies
- Works on Salvo clouds, Lattice fields, and the single Interdictor plate
- Size / blade damage tuned so 1-plate Interdictor is a scalpel and N-plate Salvo is a blender

### 5.8 Soft multi-plate rules (with Vector)

| Situation | Behavior |
|-----------|----------|
| Vector equipped | Max **one** live plate (vanilla Vector spirit) |
| Salad / Compressed / Fence mag | Mag stats may still apply for future non-Vector swaps; while Vector active, cavity behaves as 1 |
| Fence with 1 plate | No inter-plate arc (needs 2+); other Fence riders (shock on plate, mag crumb) may still apply lightly or no-op cleanly |
| Damage Arc with 1 plate | Still valid — shoot your one plate with element |
| High Ground with 1 plate | Fully valid throne |
| Plasma with 1 plate | Single blade disc — ST scalpel, not room blender |

No hard ban list; UI/tooltips should say Vector limits live plates to 1.

---

## 6. Paths Overview

| Path | Name | Fantasy | ST | Clear | Mobility |
|------|------|---------|----|-------|----------|
| A | **Salvo** | Rapid plate gun — cavity hose, cycle, volume | ** | ***** | ** |
| B | **Lattice** | Stuck field — fence, arc, High Ground throne | **** | **** | *** |
| C | **Interdictor** | Single-plate pilot — Vector, bounce, line skill | ***** | *** | *** |
| U | **Universal** | Clearing Plasma blades | flex | flex | — |
| G | **Glue** | Elements, catch spice, staples, oddities | flex | flex | flex |

---

## 7. Crowns & Sacred Systems

### 7.1 Path A — SALVO (rapid plate gun)

**Fantasy:** The launcher is a scrap automatic. Plates are ammo. Cycle is king.

**Loop:** dump cavity → live plates threaten (impact + blades) → auto/fast recall → restock → repeat.

#### A-Crown spine (vanilla DNA)

**Salad Plates — Exotic (KEEP identity)**  
Fire more plates at a faster rate; plate size reduced. Mag + RoF engine. Recoil package vanilla spirit.  
Salvo’s volume keystone.

**Compressed Plate Storage — Exotic (KEEP identity)**  
Fire additional plates before recalling (+mag spirit, vanilla +3). Cavity depth engine.  
Pairs with Salad for kitchen-sink volume; also feeds Lattice hybrids.

**Aerodynamic Plates — Rare/Epic (KEEP)**  
Plates travel faster and can be fired more often. Cycle speed support.

**Auto-Demolition — Epic (KEEP; Salvo-tied)**  
Plates automatically recall on impact. Hose tempo — don’t babysit sticks.  
Lattice hybrids may dislike instant recall; soft tension is intentional and fun (player chooses).

#### Salvo supports

- Rail-Tuning / Pole Alignment — flight + recall speed
- Slap-Job — damage up, spread up (hose tax)
- Double A / mag-adjacent staples
- Clearing Plasma (universal) — blender on many live plates
- Small plate size cards that lean into Salad fantasy

**Salvo is NOT:** babysitting a permanent fence (Lattice), single-plate pilot (Interdictor).

**Salvo wants plates moving and cycling.**

---

### 7.2 Path B — LATTICE (stuck plate field)

**Fantasy:** Plant scrap. The room becomes a circuit. Stand on your work. Arc through it. Detonate it.

**Loop:** stick 2–N plates → fence / blades / elemental arc / High Ground → cascade payoffs → recall when the field is spent.

#### B-Crown spine (vanilla DNA)

**Fencing Construction — Exotic (KEEP identity; KEYSTONE)**  
Fired plates arc lightning to each other; shock damage between them; magazine size increased.  
Lattice’s defining multi-plate noun.

**Damage Arc — Exotic (KEEP identity)**  
Shoot a live plate with an element to create an elemental explosion.  
Field execute button — works with 1+ plates; shines with a planted set.

**High Ground — Epic (KEEP identity; LATTICE SPINE)**  
While standing on one of **your** plates: damage resistance + all outgoing damage increased.  
Not an orphan. Not a third path. The throne is Lattice.

#### Lattice supports

- Shockwave — impact push on stick
- Storage Surplus — size per empty adjacent cell (grid toy + mass)
- Tungsten Cube — slow, heavy, huge plate (anchor / throne mass)
- Electrified Plates / Decay Plates — status on sticks
- Becall — bee explosion on recall (field collapse payoff)
- Charge Recycling — movement ability crumb on electrocute (Fence glue)
- Weight Rebalanced — slower heavier hits for planted ST

**Clearing Plasma (universal)** — denial blades on planted plates (room control, not only hose).

**Lattice is NOT:** mag-dump SMG (Salvo), redirect pilot (Interdictor).

**Lattice wants plates to stay long enough to matter.**

**vs HLC:** throne is a **fired metal plate**, not a painted hard-light pane.

---

### 7.3 Path C — INTERDICTOR (Vector pilot)

**Fantasy:** One plate. You fly it. Bounce is back.

**Loop:** fire one → redirect → bounce / pierce / rail lines → Power Recall cash-out on return → catch → repeat.

#### C-Crown spine

**Vector Interdiction — Exotic (KEEP identity; KEYSTONE)**  
Press or hold after firing to redirect the plate. **Maximum one live plate.**  
**IMPL LOCK:** remove the code that blocks bounce on Vector plates. No other design essay required.

**Bouncy Plates — Rare (KEEP; INTERDICTOR SPINE)**  
Plates bounce; damage increases per bounce.  
With Vector fix, this is real power again — not a trap.

**Power Recall — Rare/Epic (KEEP)**  
Greatly increased damage while returning and after bouncing.  
Interdictor cash-out window.

#### Interdictor supports

- Shish Kebab — pierce through targets
- Simple Inertia — damage increases with travel distance
- Rail-Tuning — higher fire/flight speed
- Weight Rebalanced — slower, harder hits (optional ST lean)
- Pole Alignment — faster recall for catch tempo
- Light catch glue (Impulse) as hybrid mobility

**Optional new spice (only if Vector+bounce still feels thin in playtest — backlog first):**
- Snap Bank — brief aim-assist bend on bounce
- Yo-Yo Latch — catch briefly empowers next plate
Do **not** ship new exotics until bounce restore is validated.

**Interdictor is NOT:** multi-plate fence career, salad hose.

**Interdictor wants one plate and a clean line.**

---

### 7.4 Universal — Clearing Plasma (LOCKED placement = universal exotic)

**Clearing Plasma — Exotic (KEEP identity; UNIVERSAL)**  
Plates spin, slicing things around them with light blades. Size up; blade damage tax vanilla spirit.  

| Path | Read |
|------|------|
| Salvo | Many small blades = blender clear |
| Lattice | Planted blade mines / corridor denial |
| Interdictor | One flying buzzsaw you steer |

No path ownership. Every serious build may want it. Equal exotic cell count vs other exotics; hub-friendly shape.

---

### 7.5 Dead-card rewrite policy (LOCKED direction)

#### Improvised Explosives — Epic (REWRITE — no grenade tax)

**Vanilla problem:** Consumes grenade charge for little benefit.

**Cavity Scrapworks:**
- **No grenade charge spend**
- Arm plates from **plate-native** economy (pick one primary at impl, document in tooltip):
  - **Prefer A:** Firing or sticking **arms** the plate; on recall or manual detonate (second press / catch-prox), plate explodes for serious AOE
  - **B:** Spend **1 cavity plate / ammo** equivalent to arm a live plate for a large boom (still not grenade)
- Payoff must feel like a real Epic bomb — pack delete or boss chunk — because the fantasy is explosives, not a crumb
- If a future patch ever reintroduces a grenade tax, it must be **mythically** strong per user lock; **v1 ships with zero grenade tax**

#### Painter's Attachment — Epic (REWRITE — self-sufficient)

**Vanilla problem:** Requires player corrosion; nearly unforced.

**Cavity Scrapworks:**
- Plates **apply Acid** on stick and/or create acid puddle on impact **without** requiring the player to be corroded
- Optional rider: if *you* are corroded/acid-status’d, puddle size/duration bonus (synergy crumb, not a gate)
- No external mod dependency

#### Nice Catch — Rare (REWRITE — spicy, not suicide)

**Vanilla problem:** Self-damage on catch is too lethal; “less damage at larger mag” doesn’t save mag-1.

**Cavity Scrapworks:**
- Keep: +damage and +move speed while holding the gun; catch has a cost/risk fantasy
- Change cost to one of (impl pick, playtest):
  - **Prefer:** Self-damage **capped** as a small % of current HP or flat chip that cannot kill from healthy
  - Or: self-damage converted partly to **shock/knockback pulse** at catch point (hurt nearby enemies, chip self lightly)
  - Or: brief vulnerable window instead of HP tax, with DR crumb if Catcher glue invested
- Large cavity (Salvo) still reduces cost (vanilla spirit)
- Mag-1 Interdictor must be able to run a tuned Nice Catch without constant downstate

---

## 8. Full Upgrade List (~30)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity  
Cell rule: Exotic shapes larger than others; all Exotics same cell count.  
Player-facing names below are **working titles** — full rename pass OK at implement.  
Vanilla names kept where identity is already perfect (**KEEP**).

------------------------------------------------------------------------------
UNIVERSAL                                                        [2]
------------------------------------------------------------------------------

U1. Clearing Plasma — Exotic (UNIVERSAL) [KEEP]
    Live plates spin light blades; slice nearby enemies.
    Size up; blade damage −% (vanilla spirit). Serves all paths.

U2. Boundary Incursion — Oddity [KEEP]
    Increases upgrade grid size.

------------------------------------------------------------------------------
PATH A — SALVO                                                   [8]
------------------------------------------------------------------------------

A1. Salad Plates — Exotic [KEEP]
    +Fire rate, +magazine size, −plate size, recoil package (vanilla spirit).
    Salvo volume keystone.

A2. Compressed Plate Storage — Exotic [KEEP]
    +Magazine size (vanilla +3 spirit). Fire more before recalling.
    Cavity depth; kitchen-sink and Lattice hybrid feeder.

A3. Aerodynamic Plates — Rare [KEEP]
    Plates travel faster; fire more often (cycle).

A4. Auto-Demolition — Epic [KEEP]
    Plates automatically recall on impact. Salvo tempo engine.
    Soft-tensions Lattice “stay planted” play.

A5. Cavity Feeder — Rare (NEW — Salvo economy)
    +Reload speed and/or +reserve feed into cavity after full recall/catch.
    Teaches dump → restock cadence.

A6. Scatter Cavity — Rare (Slap-Job rewrite / KEEP spirit)
    +Damage, +spread. Hose tax card.

A7. Rapid Poles — Rare (Pole Alignment KEEP spirit; Salvo-lean)
    Plates recall faster. Cycle the yo-yo.

A8. Scrap Jacket — Standard
    Modest +% plate damage. Distributed power staple.

------------------------------------------------------------------------------
PATH B — LATTICE                                                 [9]
------------------------------------------------------------------------------

B1. Fencing Construction — Exotic [KEEP; KEYSTONE]
    Live plates arc lightning between each other; shock damage to targets
    between them; +magazine size. Field noun.

B2. Damage Arc — Exotic [KEEP]
    Shoot a live plate while it carries / you apply an element to create an
    elemental explosion (vanilla spirit; cooldown crumb).
    Field execute.

B3. High Ground — Epic [KEEP; LATTICE SPINE]
    While standing on one of your plates: +DR and +all outgoing damage.
    Throne fantasy.

B4. Shockwave — Epic [KEEP]
    Plates create a shockwave on impact that pushes enemies (size + force).

B5. Becall — Epic [KEEP]
    Plates create a bee explosion when recalled. Field collapse payoff.

B6. Tungsten Cube — Rare [KEEP]
    Plates slower, much higher gravity, greatly increased size. Anchor / throne mass.

B7. Storage Surplus — Epic [KEEP]
    Plate size increased per empty cell surrounding this upgrade.
    Grid-spatial Lattice toy.

B8. Electrified Plates — Rare [KEEP]
    Plates apply shock. Fence glue.

B9. Planted Decay — Rare (Decay Plates KEEP)
    Plates apply decay. Lattice anti-regrow / stick pressure.

------------------------------------------------------------------------------
PATH C — INTERDICTOR                                             [7]
------------------------------------------------------------------------------

C1. Vector Interdiction — Exotic [KEEP; KEYSTONE]
    Press/hold after firing to redirect. Max one live plate.
    **Remove bounce block in code.**

C2. Bouncy Plates — Rare [KEEP; SPINE]
    +Bounces; +damage per bounce. Real again with Vector fix.

C3. Power Recall — Rare [KEEP]
    Greatly increased damage while returning and after bouncing.

C4. Shish Kebab — Rare [KEEP]
    Plates pierce; max hits up; speed-on-hit / gravity package vanilla spirit.

C5. Simple Inertia — Rare [KEEP]
    Damage increases the further plates travel (reverse-falloff spirit).

C6. Rail-Tuning — Rare [KEEP]
    Plates fired at higher speeds. Pilot flight card (also mild Salvo hybrid).

C7. Weight Rebalanced — Rare [KEEP]
    Slower plates, more damage. Optional Interdictor ST lean / Lattice hybrid mass.

------------------------------------------------------------------------------
GLUE / CATCH / PAYLOAD / STAPLES                                 [10]
------------------------------------------------------------------------------

G1. Improvised Explosives — Epic [REWRITE — no grenade tax]
    Arm plates natively; detonate on recall or trigger for real AOE.
    See §7.5. Mythic-feeling Epic bomb without grenade charge.

G2. Painter's Attachment — Epic [REWRITE — self-sufficient]
    Plates apply Acid and/or leave acid puddle on impact.
    Bonus if you are already corroded (optional); never gated on it.

G3. Nice Catch — Rare [REWRITE — non-lethal default]
    +Damage and +move speed while holding weapon; catch applies tuned self-cost
    that cannot casually kill you. Cost reduced at larger cavity sizes.

G4. Impulse Redirection — Epic [KEEP]
    Catching a plate while mid-air boosts you upward (force band vanilla spirit).

G5. Return to Receiver — Oddity [KEEP spirit; retune]
    Magnetism goes both ways. Vanilla range tax was harsh (−50%); retune so the
    toy is playable (milder range tax or compensatory flight/catch benefit).

G6. Charge Recycling — Epic [KEEP]
    Partial movement-ability recharge when you electrocute a target with this weapon.

G7. Double A — Standard [KEEP]
    Battery / ammo capacity increased.

G8. Sharper Edges — Standard [KEEP]
    Damage increased (modest % ).

G9. Edge Fault — Contraband/Oddity [optional KEEP]
    Second grid-size toy; ship if pool wants two grid cards, else backlog.

G10. Multiversal Thievery — Contraband [optional KEEP / backlog]
    Steal columns from other grids. Flavor contraband; not required for v1 freeze.

------------------------------------------------------------------------------
POOL TARGET (~30)
------------------------------------------------------------------------------

Path / bucket design vocabulary above is wider than 30.
Recommended **frozen 30 for v1 implement:**

  EXOTIC (6)
    1  Clearing Plasma              (universal)
    2  Salad Plates                 (Salvo)
    3  Compressed Plate Storage     (Salvo)
    4  Fencing Construction         (Lattice)
    5  Damage Arc                   (Lattice)
    6  Vector Interdiction          (Interdictor)

  EPIC (8)
    7  Auto-Demolition              (Salvo)
    8  High Ground                  (Lattice spine)
    9  Shockwave                    (Lattice)
    10 Becall                       (Lattice)
    11 Storage Surplus              (Lattice)
    12 Improvised Explosives        (glue rewrite — no grenade tax)
    13 Painter's Attachment         (glue rewrite — self-sufficient acid)
    14 Impulse Redirection          (glue)

  RARE (10)
    15 Aerodynamic Plates
    16 Cavity Feeder
    17 Electrified Plates
    18 Tungsten Cube
    19 Bouncy Plates
    20 Power Recall
    21 Shish Kebab
    22 Simple Inertia
    23 Rail-Tuning
    24 Nice Catch                   (non-lethal rewrite)

  STANDARD (5)
    25 Scrap Jacket
    26 Double A
    27 Sharper Edges
    28 Pole Alignment
    29 Decay Plates

  ODDITY (1)
    30 Boundary Incursion

  BACKLOG (designed, add when expanding past 30):
    Scatter Cavity / Slap-Job, Weight Rebalanced, Charge Recycling,
    Return to Receiver (retuned), Edge Fault, Multiversal Thievery,
    Snap Bank, Yo-Yo Latch, Salvo-specific spread hose cards,
    Lattice-only field duration / delayed recall holds,
    Interdictor bounce-count exotic if Vector still thin after bounce fix

  Notes on cuts:
    - Weight Rebalanced → backlog (Inertia + Tungsten cover mass/ST lean)
    - Charge Recycling → backlog (Fence + Electrified still define shock loop)
    - Return to Receiver → backlog until range tax retune is fun
    - Slap-Job → backlog (Salad already owns volume personality)


---

## 9. Example Builds (mix-and-match encouraged)

### Kitchen sink (celebrated hybrid)
Salad Plates + Compressed Plate Storage + Fencing Construction + Clearing Plasma + Damage Arc  
(+ High Ground if throne moments; + Electrified)  
**Read:** Dump a cavity, blades + fence clear the room, arc-detonate priority plates, restock.

### Salvo hose
Salad + Compressed Storage + Aero + Auto-Demolition + Clearing Plasma + Cavity Feeder + Pole Alignment  
**Read:** Impact → auto-recall blender. Minimal babysitting.

### Throne lattice
Fence + High Ground + Clearing Plasma + Shockwave + Tungsten Cube + Electrified  
(+ Damage Arc for execute)  
**Read:** Plant anchors, stand on scrap, DR up, outgoing up, blades deny, shockwave peel.

### Bounce pilot (Interdictor)
Vector Interdiction + Bouncy Plates + Power Recall + Shish Kebab + Simple Inertia + Rail-Tuning  
(+ Clearing Plasma single buzzsaw; + Nice Catch if tuned)  
**Read:** One plate, bounce lines, redirect, cash out on return.

### Acid works
Painter rewrite + Damage Arc + Lattice sticks + Improvised (arm)  
**Read:** Acid field + elemental plate pops + native bombs — no corrosion scavenger hunt.

### Catch acrobat (glue hybrid)
Impulse + Nice Catch rewrite + Pole Alignment + Power Recall + Vector or Salvo  
**Read:** Yo-yo movement skill expression; not a full path, still a build accent.

### Grenade-free demolition
Improvised rewrite + Auto-Demo or Becall + Shockwave + Salad  
**Read:** Armed plates cycle or collapse into real explosions without touching grenade charges.

---

## 10. Soft Hybrid Matrix (not mutex)

| Combo | Behavior |
|-------|----------|
| Salvo + Lattice | **Poster kitchen sink** — volume feeds field; watch stacked clear in playtest |
| Salvo + Interdictor | Vector 1-plate cap fights Salad; gun still works; multi-mag wasted while Vector on |
| Lattice + Interdictor | One-plate throne + fence soft-fail; Damage Arc + High Ground + Vector = skill throne pilot |
| All three | Allowed; power from verbs; no ban list |
| Plasma + anything | Always on-plan |
| Auto-Demo + Lattice | Soft tension (recall vs stay); player chooses tempo |
| High Ground + Vector | Valid one-plate throne |
| Improvised + any path | Armed payoff on that path’s lifecycle |

---

## 11. Economy & Tuning Rules of Thumb

1. **Empty-grid** must clear packs with stick/recall/catch literacy — not wet noodle, not free blender.
2. **Salvo** power is volume × cycle × blades — not only Salad % damage.
3. **Lattice** power is geometry uptime × fence/arc/throne — reward planting, not only mag size.
4. **Interdictor** power is bounce count × redirect accuracy × Power Recall window — bounce fix is mandatory for peer status.
5. **Clearing Plasma** blade damage tax exists so N-plate Salvo doesn’t instantly delete bosses without aim; Interdictor single blade should still feel like a real exotic.
6. **High Ground** outgoing damage applies broadly (vanilla spirit) — watch stacking with team DPS; DR band should not make the player immortal.
7. **Damage Arc** cooldown prevents infinite plate-pop machine-gunning.
8. **Improvised** boom scales with gun % cards where possible (prefer % of plate damage × size), not a disconnected flat grenade.
9. **Nice Catch** hard rule: catch cost must not routinely down a full-HP player on mag 1.
10. **Kitchen-sink stack** should remain excellent; Interdictor and pure Lattice must reach “I don’t need Salad” viability, not exceed sink by deleting the fantasy.

### 11.1 Budget sketch (playtest dials)

| Lever | Starting intent |
|-------|-----------------|
| Baseline damage vs vanilla Plate | Match spirit; slight empty-grid raise only if needed |
| Standard damage card | +8–12% |
| Rare damage / verb card | +12–18% or strong verb + smaller % |
| Epic verb | Strong lifecycle verb; % secondary |
| Exotic | Build-defining verb (mag transform, fence, vector, blades, arc) |
| Plasma blade damage | Vanilla −60% spirit starting point; retune per path feel |
| High Ground | +30–50% outgoing / +60–78% DR spirit — validate immortality |
| Power Recall | +80–125% on return/bounce window spirit |
| Vector | 1 plate hard cap; bounce enabled |

### 11.2 Damage modifier preference

Prefer **percentage multipliers** and **verbs** (bounce count, blade ticks, fence intervals, mag, RoF). Avoid late-game-dead flat adds on the weapon pool where siblings already locked % only (Junk Flinger lesson). Vanilla ports that used flat packages should retune to % of gun/plate damage where practical.

---

## 12. Strengths, Weaknesses & Co-op

**Strengths**
- Unique magnetic scrap gunfeel preserved and split into three careers
- Kitchen-sink hybrid still slaps
- Vector pilot finally has bounce teeth
- High Ground throne is a real Lattice identity
- Universal blades readable on any live plate
- Soft hybrid freak builds (throne pilot, acid works, catch acrobat)

**Weaknesses**
- Baseline mag 1 is deliberate and slower than SMG hose without Salvo
- Lattice misplay = standing nowhere / recalling your throne
- Interdictor whiffs are obvious (skill-expressive failure)
- Auto-Demo fights pure plant play
- Parallel weapon = players must find/unlock Scrapworks (by design)
- Not a paint-architecture gun (HLC owns that)

**Co-op**
- Fence and blades are team-readable denial
- High Ground is selfish throne (OK) — don’t require allies to stand on your plates
- Shockwave knockback should respect friendly knockback policy
- Becall / Improvised booms need clear VFX so allies don’t face-tank your recall
- Acid puddles are shared pressure (Painter rewrite)

---

## 13. Visual, Audio & Thematic Design

**Appearance**
- SAXON industrial scrapworks: cavity magazine, magnetic yoke, hazard stripes, worn plate feed
- Salvo: smaller plates, faster eject, cavity chatter
- Lattice: thicker anchors, visible arc tethers between plates, throne plate glow when stood on
- Interdictor: single plate with redirect trail / bounce sparks; yoke HUD pip
- Plasma: light-blade discs on live plates (cyan/white industrial, not magic)

**Sound**
- Baseline: heavy plate launch clang + stick thud + magnetic whoop on recall + catch clack
- Fence: electric snap between plates
- Plasma: spinning blade hum while live
- Vector redirect: servo yaw / magnet tug
- Bounce: ricochet clang (satisfying — this is the Interdictor juiceline)
- High Ground: low shield hum while standing on plate
- Improvised arm: fuze tick; detonate metal boom
- Painter: acid hiss puddle

**Flavor / codex line (in-game style)**
  Cavity Scrapworks  
  Launches heavy metal plates that stick and recall magnetically.  
  Salvo upgrades expand cavity fire rate and volume.  
  Lattice upgrades turn stuck plates into a field (fence, throne, arc).  
  Interdictor upgrades pilot a single plate (redirect; bounce enabled).  
  Clearing Plasma blades any live plate.

---

## 14. Implementation Notes (for later)

### 14.1 Gear registration
- Follow weapon template in this repo: clone base gun, GearInfo high-range id,
  APIName `cavity_scrapworks`, behaviour component, SpawnGear stamp, CreateUpgrade pool.
- **Clone candidate:** vanilla **PlateLauncher** from `Global.AllGear` (correct plate bullet, recall, catch DNA).
  Do **not** clone CartridgeSMG unless PlateLauncher is unavailable.
- Plugin: GUID `sparroh.cavityscrapworks`, MycoMod **IsSandbox**.
- Persistence: stable gear id; register before `PlayerData.OnAwake` AddGear.
- Working gear id band: pick free high-range id at impl (e.g. 95xxx) — confirm unused.
- Do **not** overwrite vanilla Plate Launcher upgrade pool.

### 14.2 Behaviour host
CavityScrapworksBehaviour (or true Gun subclass when prefab exists):
- WeaponData flags: salvo/lattice/interdictor params, plasma blade params, arm state,
  high ground active, nice catch params, painter acid params, vector redirect ownership
- Runtime: live plate list/owner links, fence link graph, blade tick timers, arm fuzes,
  stand-on-plate check, redirect input state
- Prefab snapshot restore on upgrade Remove

### 14.3 Vector bounce fix (CRITICAL PATH)
- Locate vanilla Vector Interdiction / PlateLauncher code path that **disables bounce**
- On Scrapworks (and only Scrapworks pool — do not silently patch vanilla unless a separate QoL mod is intended): ensure Vector plates **can bounce**
- Confirm interaction with Bouncy Plates maxBounces and Power Recall “after bouncing” mult
- Soft synergy: BounceIndicatorPlus shows lines when bounces ≥ 1 — no hard dep

### 14.4 Hooks (draft)

| Hook | Use |
|------|-----|
| Fire / OnFiredBullet | Mag rules, Vector single-plate enforce, arm-on-fire, plasma attach |
| Plate stick / OnHit | Shockwave, Painter puddle, Auto-Demo recall, Lattice plant register |
| Recall start/update | Power Recall mult, Becall, Improvised detonate option, fence teardown |
| Catch | Impulse, Nice Catch cost, Cavity Feeder crumbs |
| Player stand / ground check | High Ground buff while on owned plate collider |
| Damage / status apply | Electrified, Decay, Charge Recycling on electrocute |
| Redirect input | Vector press/hold steer |
| Element shot vs live plate | Damage Arc explosion |

Exact method names: confirm in `.Resources/Assembly` at impl (`PlateLauncher`, plate bullet type, upgrade flag enums).

### 14.5 Fence / multi-plate
- Track owner’s live plates; interval tick arcs between pairs in range (vanilla Fence spirit)
- With 0–1 plates: no arc (clean no-op)
- Shock damage uses gun element pipeline where sensible

### 14.6 High Ground
- Detect player grounded on collider/owned plate volume
- Apply DR + outgoing damage buff while true; clear on leave/recall/destroy
- Allies: default **no** shared throne unless a backlog co-op card says so

### 14.7 Clearing Plasma
- Per live plate: blade radius damage ticks
- Scale feel: 1 plate = scalpel; N plates = blender with blade damage tax

### 14.8 Improvised (no grenade)
- Do **not** call grenade charge spend APIs
- Arm flag on plate; explode on recall and/or secondary trigger
- Damage: % of plate/gun damage × size factor

### 14.9 Painter
- On stick/impact: apply Acid effect amount and/or spawn acid puddle
- Optional bonus if local player has corrosion/acid status — never required

### 14.10 Nice Catch
- On catch: apply capped self chip or pulse rewrite
- Enforce non-lethal floor (e.g. leave player at ≥1 HP from this cost, or % max HP cap)

### 14.11 HUD
- Live plate count / cavity
- Vector redirect affordance
- Armed plate pip if Improvised
- High Ground active indicator when standing on plate
- Prefer SparrohUILib if dependency acceptable; else minimal

### 14.12 VFX / audio priority
1. Stick / recall / catch baseline juice
2. Bounce clang (Interdictor juiceline)
3. Fence arcs
4. Plasma blade discs
5. High Ground throne read
6. Damage Arc elemental pop
7. Improvised arm/detonate
8. Painter acid puddle

### 14.13 Multiplayer
- IsSandbox; identical mod on all clients
- Plate ownership = firing player; fence/plasma/high ground owner-authoritative
- Match vanilla plate replication patterns where possible

### 14.14 Related mods (DNA only — no hard deps)

| Mod | Relationship |
|-----|----------------|
| BounceIndicatorPlus | Soft QoL for Interdictor bounce lines |
| ArcLightningRework | Optional fence chain DNA if arcs need teeth |
| Hard-Light Constructor | Boundary sibling — light paint vs metal magnet |
| DiscWorldRework | Continues to exclude Plate family; leave alone |
| Hive / Siege / Junk / Rapture / DMLR docs | Structure only |

---

## 15. Deliberate Non-Goals

- Not replacing or patching vanilla Plate Launcher pool  
- Not Catcher-as-third-path (recall toys stay glue / Lattice riders)  
- Not Hard-Light paint brushes / Shatter Jam  
- Not Photon Disc / Disc World conversion  
- Not grenade-charge taxes on v1 Improvised  
- Not Painter gated on player corrosion  
- Not Nice Catch as a downstate machine  
- Not hard anti-synergy matrix (Vector 1-plate soft tension only)  
- Not nerfing kitchen-sink hybrid into the ground to “fix” 1D  
- Not requiring BounceIndicatorPlus, ArcLightningRework, or UI lib  
- Not requiring custom Unity prefab for v1 (runtime clone OK)  
- Not shipping new Interdictor exotics before bounce fix is validated  

---

## 16. Open Tuning Questions (playtest, not design blockers)

1. Salad mag / RoF / size numbers vs Compressed Storage stacking — cavity ceiling?  
2. Auto-Demo + Fence — is instant recall too hostile to Lattice, or healthy tension?  
3. Plasma blade DPS at 1 vs 6 live plates — boss melt check  
4. High Ground DR/damage — immortality vs glass throne  
5. Damage Arc cooldown and elemental source rules (gun element vs last applied)  
6. Vector redirect responsiveness and bounce count default with/without Bouncy Plates  
7. Power Recall window vs bounce mult stacking  
8. Improvised arm trigger: recall-only vs second input vs catch-prox  
9. Nice Catch self-cost formula that feels spicy on Salvo and safe on Vector  
10. Painter puddle size/duration vs Acid apply amount  
11. Return to Receiver retune (backlog) — what range tax is fair?  
12. Unlock method: auto-unlock like template vs progression?  
13. Rename pass final names (Cavity Scrapworks path card names)  

---

## 17. Success Criteria / Player Fantasy Checklist

- [ ] Vanilla Plate Launcher still exists unchanged  
- [ ] Cavity Scrapworks baseline is stick → recall → catch with mag 1 honesty  
- [ ] Salvo alone (Salad + Storage + Aero/Auto-Demo + Plasma) feels like a real rapid plate gun  
- [ ] Lattice alone (Fence + High Ground + Plasma/Arc) feels like a planted field career without Salad  
- [ ] Vector alone with bounce restored + Power Recall/Bouncy feels peer-viable and fun  
- [ ] Kitchen-sink hybrid still slaps and is celebrated  
- [ ] Clearing Plasma reads on 1 plate and on many plates  
- [ ] High Ground throne is obvious when standing on your plate  
- [ ] Improvised never spends grenade charges  
- [ ] Painter never requires “become corroded first”  
- [ ] Nice Catch does not routinely kill the player  
- [ ] Bounce clang + redirect is the Interdictor juiceline  
- [ ] No hard ban list; Vector 1-plate soft-tensions multi-plate cards cleanly  
- [ ] Empty-grid clears packs without crowns  
- [ ] ~30 upgrades ship; exotic shapes larger & equal  

---

## 18. Review Decisions Locked (2026-08-08)

| Topic | Decision |
|-------|----------|
| Ship name | **Cavity Scrapworks** |
| Paths | **Salvo / Lattice / Interdictor** (not Catcher path) |
| Path split insight | Rapid plate gun vs stuck field vs Vector pilot |
| Clearing Plasma | **Universal exotic** |
| Improvised | **No grenade tax** (plate-native arm) |
| High Ground | **Lattice spine** |
| Vector bounce | **Remove code block** — design complete |
| Recall input | Vanilla-feel; no strong path steal in v1 |
| Sacred | Stick/recall/catch; Vector redirect; fence geometry; High Ground throne; bounce for Interdictor |
| Kitchen sink | Celebrated hybrid, not the only chart line |
| HLC boundary | Metal magnet scrap ≠ hard-light paint |
| First ship pool | Frozen 30 listed in §8 |
| Remainder | Designed backlog |

---

## 19. Changelog vs Vanilla Plate / early Catcher draft

| Old direction | v1 |
|---------------|-----|
| One kitchen-sink chart line | Three peer paths + universal Plasma |
| Vector bounce disabled | Bounce block removed on Scrapworks Vector |
| Catcher as Path C | Cut; catch toys are glue |
| Improvised grenade tax | Removed; plate-native explosives |
| Painter needs corrosion | Self-sufficient acid/puddle |
| Nice Catch suicide | Non-lethal rewrite |
| High Ground orphan Epic | Lattice spine throne |
| Plasma path-locked | Universal exotic |
| Replace vanilla Plate | Parallel new primary |
| Keep all vanilla names | Rename pass OK; KEEP on strong identities |

---

## 20. One-Page Summary (pin this)

**Cavity Scrapworks** — parallel Plate Launcher rework primary.

**Baseline:** fire plate → stick → recall → catch. Mag 1. No free toys.

**Salvo:** Salad + Compressed Storage + cycle cards + Auto-Demo. Plates are ammo.

**Lattice:** Fence + Damage Arc + High Ground + plant supports. Plates are architecture.

**Interdictor:** Vector + bounce restored + Bouncy/Power Recall/pierce/inertia. One plate is the crosshair.

**Universal:** Clearing Plasma blades on any live plate.

**Glue rewrites:** Improvised (no grenade), Painter (self acid), Nice Catch (non-lethal).

**Hybrids:** kitchen sink celebrated; no ban list; Vector caps live plates at 1.

**Ship ~30.** Exotics large equal shapes. IsSandbox. APIName `cavity_scrapworks`.

*Hose the room, fence the room, or fly one plate through the room. Bounce is not a myth.*
