# Hive Launcher – Design Document (v1)

> Status: **Design only** — no implementation yet.
> Working title in notes: Swarm Launcher Rework. **Ship name: Hive Launcher.**
> Template base: `.new.SwarmLauncherRework` weapon content project.
> Product shape: **separate primary gear** — vanilla Swarm Launcher is left unmodified.

---

## 1. High Concept / Fantasy

**Hive Launcher** is the living pellet swarm that finally has more than one way to keep bees.

Vanilla Swarm Launcher funnels real power into two stacks:

- **Mitosis + Remote Control + Wait Charging** (hover battery)
- **Scatterburst + Guidance Array** (semi volley / tracking hose)

**The Hive Must Grow** is the grid toy that makes building fun — and everything else is niche utility, hold-R toys, or fire-mode accidents (Quick Grouping forcing semi; Scatterburst bugged auto).

Hive Launcher keeps the plant → hover → dive gunfeel and rebuilds the grid around three **peer loops** plus a universal hive exotic:

- **Apiary** — plant a cloud, wait, steer, rain (time is power).
- **Swarmfront** — trigger is damage now (fat volleys, tracking spray).
- **Second Skin** — wear the swarm; movement is the crosshair (orbit / escort / fling).

**The Hive Must Grow** is universal: adjacency is the reason the grid is a puzzle for every build.

**One-liner:** *Grow the hive on the grid — then hold the cloud, dump the volley, or wear the swarm and fly through the pack.*

**Element spine:** Normal at baseline. Shock is a first-class glue loop (Electrified Pellets + Static Reload). Fire / Acid opt-in via payload cards, not required for identity.

---

## 2. Role & Fantasy in the Arsenal

| Trait | Value |
|-------|--------|
| **Slot** | Primary |
| **Range** | Mid (hover plant); Swarmfront extends effective spray; Second Skin is close–mid body radius |
| **Role** | Hover-swarm controller, volley shotgun-primary, body-orbit skirmisher |
| **Gap filled** | Vanilla Swarm is two builds + a junk drawer. Hive Launcher is three peer loops + hive adjacency |
| **Synergies** | Movement / air time (Skin + Crop Duster), shock status, thin co-op heal/breed, offhand ammo refund toys |

**Product shape:** New primary (**Hive Launcher**). Does **not** replace or patch vanilla Swarm Launcher.

**Not trying to be:** hitscan DMR, heat-infinite SMG, grenade siphon battery, melee charge stick, or a mandatory external-mod dependency.

| Gear / mod | Relationship |
|------------|----------------|
| Vanilla Swarm Launcher | Left in game untouched |
| AtmosphericEnergizersRework | **DNA baked in** — reserve regen while hovering (see §6.4); no hard runtime dep |
| DMLR / Spillway / Junk Flinger / Heat Cycler | Sibling design structure only |
| SparrohsTurbocharges | Soft synergy; no hard dep |

---

## 3. Design Pillars

1. **Three peer loops** — Apiary / Swarmfront / Second Skin. Each answers “how do I use this gun?” differently.
2. **The Hive Must Grow is universal** — adjacency mag (and light hive economy) is the build-around for everyone, not a path crown.
3. **No dead neighbor cards** — every upgrade has a real verb; nothing exists only to touch THMG.
4. **Fire-mode honesty** — baseline full-auto plant. **Only Scatterburst** may force semi (core identity). Quick Grouping never touches fire mode. Fix Scatterburst auto bug.
5. **Crop Duster is sacred** — airborne auto-seed stays; it lives on **Second Skin** as spine, not generic mobility glue.
6. **Hold-R utility pile is cut** — Portable Solar Array, Overload Contingency, Munition Siphon are gone. No path-owned hold-R required for v1.
7. **Thin co-op, not a path** — Breeding Season + Cross Pollination are a two-card branch with solo-viable fallbacks.
8. **Shock loop is glue** — Electrified Pellets + Static Reload stay strong and path-agnostic (or light riders on any path).
9. **Distributed power** — many Standard/Rare/Epic cards carry damage, volume, hover potency, or economy. Empty-grid must clear packs.
10. **Soft crowns only** — hybrids allowed and celebrated (poster stacks preserved). No hard exclusion matrix.
11. **~30 upgrades for v1** — exotic shapes larger than others; each exotic same cell count; full rename pass OK.
12. **Failure states stay fun** — over-hold Wait Charging into empty reserves, Scatterburst whiff volley, orbit empty on bad fling, mitosis starve.

---

## 4. Core Mechanics & Gunfeel

### 4.1 Base gun (no upgrades)

| Trait | Draft / intent |
|-------|----------------|
| Fire mode | **Full-auto** plant (vanilla Swarm spirit) |
| Behavior | Launches hovering pellets that dive/track on release (or auto-dive rules when upgraded) |
| Damage | Honest mid pellet; slight raise vs vanilla empty-grid if needed so non-crown is playable |
| Element | Normal |
| Bullets per shot | ~2 spirit |
| Fire rate | High RoF hose spirit (~vanilla 857 RPM ballpark) |
| Magazine / reserve | ~36 / ~288 spirit (paths + Hive mutate hard) |
| Reload | Standard reload beat |
| Hover / dive / orbit / mitosis | Hover+dive baseline; orbit **OFF** until Second Skin cards |
| ADS | Normal unless a card says otherwise |
| Model / audio | Borrow Swarm Launcher until custom art |
| Hold-R specials | **None** on baseline |

### 4.2 Base combat loop

```
Hold M1 → plant hovering pellets in aim volume
Release / dive rules → pellets dive to enemies
   ↘ reload on empty
   ↘ without crowns: honest auto swarm plant (not waiting for Mitosis or Scatterburst)
   ↘ Apiary: hold longer, steer, charge, mitosis rain
   ↘ Swarmfront: fatter shots, tracking spray, semi volley dump
   ↘ Second Skin: feed orbit on body → fly through packs → fling shell
```

### 4.3 Inputs

| Input | Baseline | With crowns |
|-------|----------|-------------|
| **M1 hold** | Plant hover pellets (auto) | Scatterburst: semi volley; Skin: feed orbit; Apiary: fill hover battery |
| **M1 release** | Dive / commit hover cloud (vanilla spirit) | Wait Charging cashes time; Skin may fling orbit shell (see §6.3) |
| **RMB / AIM** | Normal ADS | Unchanged unless a card removes ADS |
| **R tap** | Reload | Reload only |
| **R hold** | Nothing | **Nothing in v1** (utilities cut) |

### 4.4 Fire-mode policy (LOCKED)

1. Baseline = **full-auto plant**.
2. **Scatterburst** sets **semi-auto** + fat pellets-per-shot + mag tax + RoF tax. This is intentional identity.
3. **Implement semi correctly** — fix vanilla bug where Scatterburst still full-autos.
4. **Quick Grouping** (and any other pellet-count card) **must not** set semi or change fire mode — only pellets/shot, mag/ammo, RoF as stated.
5. No other card forces semi unless a future exotic’s entire identity is a distinct fire verb with explicit card text.

### 4.5 Soft crown matrix (not hard mutex)

| Combo | Behavior |
|-------|----------|
| Any single path crown stack | Full fantasy |
| Apiary + Swarmfront | Steered / charged volleys — Remote + Guidance etc. allowed; power watched in playtest |
| Apiary + Second Skin | Hover battery can **dock into orbit** on release or dual-pool (impl pick one; prefer dock-on-release) |
| Swarmfront + Second Skin | Volleys can **feed orbit** or fling as shotgun shell from body — hybrid freak OK with size/damage taxes |
| All three | Allowed; stack soft taxes so it is fun, not delete-everything |
| THMG + anything | Always on-plan — cluster shapes into the hive |
| Crop Duster + Skin | **Poster** — airborne densifies / auto-feeds orbit |
| Crop Duster + non-Skin | Still works (sacred); seeds normal hover pellets or path-appropriate pellets |

---

## 5. The Hive System (LOCKED — universal)

### 5.1 Vanilla success

**The Hive Must Grow:** magazine size increases for each unique upgrade that **touches** this one. This is the fun of the weapon’s grid. It must remain the centerpiece.

### 5.2 Hive Launcher rules

1. **THMG is Exotic, universal** — not locked to a path. Every serious build wants it.
2. **Primary payoff = magazine size** per unique touching upgrade (vanilla spirit; retune numbers in playtest).
3. **Secondary payoff (light):** small **reserve** and/or **hover/orbit capacity** crumb per neighbor so Apiary/Skin economies feel the hive — not a second damage stat that replaces verbs.
4. **Optional spice (playtest only):** tiny swarm potency at high neighbor counts (e.g. +pellets crumb or +hover damage %). Default **off** for v1 freeze if mag alone reads clean.
5. **No cards exist solely to touch THMG.** Neighbor value comes from real path/glue verbs; shapes are designed so clustering is natural.
6. **Exotic shape:** large, equal cell count vs other exotics; placeable as a hub.

### 5.3 Design implication for shapes

- Path supports should include **compact pieces that hug a hub** and a few **awkward spines** that force interesting THMG placement.
- Do not require THMG for a path to function — empty-THMG Apiary/Swarmfront/Skin must still play; THMG is the multiplier fantasy.

### 5.4 What Hive is NOT

- Not a fourth path.
- Not Globblometer-style “meter from every card → only damage” (mag adjacency is the readable spine).
- Not a gate (“need 4 neighbors to unlock Mitosis”).

---

## 6. Crowns & Sacred Systems

### 6.1 Path A — APIARY (hover time-battery)

**Fantasy:** Plant a cloud. The longer it lives under your care, the meaner it gets. Steer it. Feed it ammo. Rain.

**Loop:** plant → hold/steer → charge → dive/rain → reload/regen → repeat.

#### A-Crown spine (vanilla DNA)

**Wait Charging — Exotic or Epic engine**  
Pellet damage increases the longer they hover (duration cap + max damage %). Keep as Apiary’s time-is-power card.

**Remote Control — Epic/Exotic**  
Hovering pellets follow crosshair; travel slower. Apiary aim skill expression.

**Mitosis — Exotic**  
Hovering pellets consume **reserve** ammo to release diving pellet rain (reduced damage). The ammo→rain engine. Needs reserve economy supports.

**Atmospheric Energizers — Epic** (bake AtmosphericEnergizersRework DNA)  
While swarm is hovering (or orbiting — see Skin note), regenerate **stored/reserve** ammo over time — not mag. Configurable spirit: delay after last fire, tick interval, cap at ammo capacity. Makes Mitosis sustainable without being free mag cheat.

#### Apiary supports (design vocabulary)

- Hover duration / hover speed control
- Dive damage on release
- Reserve capacity and Mitosis efficiency (cheaper rain / more rain)
- Quick Release rewrite: early release = speed **and/or** partial charge keep (tempo, not only move speed)
- Tight plant / range cards that help aim-point battery play

**Apiary is NOT:** body orbit (Skin), semi volley identity (Scatterburst).

---

### 6.2 Path B — SWARMFRONT (trigger volume)

**Fantasy:** The swarm hits when you pull the trigger. Living shotgun, tracking hose, fat automatic spray.

**Loop:** shoot → pellets threaten immediately (auto-dive / tracking / fat BPS) → mag management → repeat.

#### B-Crown spine (vanilla DNA)

**Scatterburst — Exotic**  
- **+~30 bullets per shot** (fat volley)
- **Magazine size greatly decreased**
- **Fire rate −50% spirit**
- **Sets semi-automatic** (intentional)
- **BUGFIX:** must actually be semi (no phantom full-auto)

Identity: deliberate semi volley / living shotgun. Pairs with Guidance + THMG (hive mag offsets the cut).

**Guidance Array — Exotic**  
Pellets spray forward with **full target tracking**. Mag + ammo capacity greatly increased. Swarmfront’s hose brain.

**Autoswarm — Epic/Rare**  
More accurate; automatically dive toward targets; speed/spread package (vanilla spirit). Removes some plant-hold skill for instant threat.

**Quick Grouping — Rare/Epic**  
Fire a shot of ~6 pellets spirit; mag + ammo up; RoF down hard.  
**LOCKED:** does **not** set semi or touch fire mode — only BPS + economy + RoF.

#### Swarmfront supports

- Explosive Pellets (AOE volley clear)
- High Caliber Pellets (slower, heavier pellets)
- Sprinkler / spread control
- Mag/ammo staples that aren’t THMG
- Tracking / dive haste crumbs

**Swarmfront is NOT:** wait-for-charge ST (Apiary), body-radius escort (Skin).

---

### 6.3 Path C — SECOND SKIN (body-orbit)  [PEER LOOP — not glue]

**Fantasy:** You don’t leave the bees in the air at your crosshair — you **wear** them. The swarm orbits and escorts you. Movement is aiming. Flying through a pack is the clear button; flinging the shell is the punch.

**Loop:** M1 feeds orbit → move through enemies (aura / contact dive) → release/fling orbit shell → refill.

This is a different answer than Apiary (aim-point battery) and Swarmfront (per-trigger volume).

#### C-Crown spine

**Second Skin — Exotic (NEW keystone)**  
- Hover cloud rules **convert** to **body-orbit escort** while equipped (or: pellets prefer orbit bind over world hover — pick one clear rule in impl).
- Orbiting pellets deal contact/near-radius threat and can auto-dive to enemies that enter radius.
- **M1** feeds pellets into the orbit (spends mag as normal).
- **Release M1** (or alt commit): **Fling** — orbit pellets launch outward as a directed shell along aim/movement vector (clear punch / gap closer finisher).
- Orbit has a **soft cap** (pellet count); overcap either flings oldest or refuses feed with feedback.
- Visual: tight living halo / trail — must read instantly vs world hover cloud.

**Crop Duster — Exotic/Epic (SACRED)**  
- While airborne, automatically spawn swarm pellets (vanilla).
- **Second Skin binding:** airborne spawns **feed the orbit** (densify / refresh) rather than only world-hover in front — you are a mobile hive skin.
- Fire rate of auto-spawn still spins up with air time (vanilla spirit).
- **Still triggers when Hive Launcher is not equipped** (sacred unequipped behavior kept).
- When Skin is not equipped: behaves as vanilla-style forward airborne seed into normal hover pellets (still good on Apiary/Swarmfront hybrids).

**Chitin Veil — Epic (NEW or Migration rewrite)**  
While orbit has ≥N pellets: minor DR and/or move speed. Rewards keeping the skin fed. Not a generic sprint-fire card.

**Hiveburst — Epic (NEW — Skin fling payoff)**  
Fling damage/size scales with pellets consumed from orbit. The Skin “Scatterburst moment” without stealing Scatterburst’s semi identity.

#### Second Skin supports

- Orbit radius / orbit speed
- Contact damage vs fling damage split
- Air time / jump crumbs that serve Crop Duster (not generic “move fast while shooting” as path identity)
- Sprint-fire while orbit active (Migration DNA, Skin-tied)
- Quick Release DNA: fling early for speed burst

**Remote Control interaction (soft):**  
With Skin + Remote Control: either (A) fling direction follows crosshair hard, or (B) orbit center slightly biased toward aim — document one; do not make Skin just “Remote Control on your feet.”

**Mitosis interaction (soft):**  
Mitosis can spend reserve to rain from **orbit** as well as world hover — hybrid Apiary/Skin allowed.

**Wait Charging interaction (soft):**  
Orbit time charges pellet damage the same as hover time — time-is-power still works when worn.

**Second Skin is NOT:**
- A mobility glue path (move speed alone)
- A shock path (shock stays glue)
- A co-op path
- World turret nests (rejected Nest direction)
- Combat split-proliferation snowball as the core loop (rejected Bloom-as-path; tiny hatch crumbs OK on supports later)

---

### 6.4 Atmospheric Energizers (baked rework DNA)

Port spirit from `AtmosphericEnergizersRework` (no hard mod dependency):

- Regen **StoredAmmo / reserve**, not magazine
- Only while hover **or orbit** pellets are active
- Delay after last manual fire before regen starts
- Tick from weapon regen interval; cap at ammo capacity
- Enables Mitosis and long Skin uptime without free infinite mag hose

---

### 6.5 Thin co-op branch (NOT a path)

**Breeding Season — Epic**  
While firing, extra pellets from nearby players who also run Hive Launcher / Swarm-family (vanilla spirit).  
**Solo fallback (LOCKED intent):** if no ally swarm users in radius, spawn a reduced **echo pellet** from self on a cooldown (or tiny orbit gift) so the card is never brick solo. Ally presence upgrades the effect, not enables it.

**Cross Pollination — Rare/Epic**  
Pellets can track to players and heal them (vanilla spirit, modest healing). Thin support — not a build pillar. Optional: orbit pellets can top up allies in radius when Skin equipped.

No third co-op card required for v1.

---

### 6.6 Shock glue (NOT a path)

**Electrified Pellets — Rare**  
Pellets apply shock. Keep strong.

**Static Reload — Rare**  
Electrocuting a target with any weapon increases Hive Launcher reload speed. Keep strong.

These support all three paths (volley shock, charged cloud shock, orbit shock aura). Do not build Path C around them.

---

## 7. Paths Overview

| Path | Name | Fantasy | ST | Clear | Mobility |
|------|------|---------|----|-------|----------|
| A | Apiary | Time-battery hover → rain | **** | *** | ** |
| B | Swarmfront | Trigger volume / semi volley / track hose | *** | ***** | ** |
| C | Second Skin | Body-orbit escort → fling | *** | **** | ***** |
| H | Hive | Universal adjacency mag | flex | flex | — |
| S | Co-op | Thin breed + heal | — | — | — |
| G | Glue | Shock, explosive, economy, staples | flex | flex | flex |

---

## 8. Full Upgrade List (~30)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity  
Cell rule: Exotic shapes larger than others; all Exotics same cell count.  
Player-facing names below are **working titles** — full rename pass OK at implement.  
Vanilla names kept where identity is already perfect (call out KEEP).

------------------------------------------------------------------------------
UNIVERSAL / HIVE                                                 [2]
------------------------------------------------------------------------------

H1. The Hive Must Grow — Exotic (UNIVERSAL KEYSTONE) [KEEP name]
    Magazine size increased per unique upgrade touching this one.
    Light secondary: +reserve crumb per neighbor (playtest).
    Hub shape; equal exotic cell count.

H2. Boundary Incursion — Oddity [KEEP]
    Increases upgrade grid size.

------------------------------------------------------------------------------
PATH A — APIARY                                                  [8]
------------------------------------------------------------------------------

A1. Wait Charging — Exotic [KEEP]
    Pellet damage increases the longer they hover (and orbit, if Skin).
    Duration + max damage % (vanilla spirit).

A2. Mitosis — Exotic [KEEP]
    Hovering/orbiting pellets consume reserve ammo to rain diving pellets
    at reduced damage. CanTurbocharge spirit OK.

A3. Remote Control — Epic [KEEP]
    Hover pellets follow crosshair; slower travel.
    With Skin: fling/orbit bias toward aim (see §6.3).

A4. Atmospheric Energizers — Epic [KEEP name; REWORK DNA]
    Reserve ammo regenerates over time while pellets are hovering or orbiting.
    Fire delay before regen; does not top off mag directly.

A5. Pollen Clock — Rare (NEW — Wait Charging support)
    +Max hover charge duration and/or faster charge ramp.
    Small −dive speed (time to cook).

A6. Queen’s Share — Rare (NEW — Mitosis economy)
    Mitosis ammo cost reduced; mitosis rain damage penalty slightly lessened.

A7. Early Swarm — Rare (Quick Release rewrite)
    Releasing / flinging before mag empty grants move speed burst.
    Apiary: keep a fraction of Wait Charging progress on early dive (crumb).

A8. Deep Reserves — Standard
    +Ammo capacity (reserve). Feeds Mitosis / Energizers loop.

------------------------------------------------------------------------------
PATH B — SWARMFRONT                                              [8]
------------------------------------------------------------------------------

B1. Scatterburst — Exotic [KEEP identity]
    +~30 bullets per shot; mag size down hard; fire rate −50% spirit;
    **semi-auto ON** (bugfixed). Living shotgun volley.

B2. Guidance Array — Exotic [KEEP]
    Full target tracking spray; mag + ammo greatly increased.

B3. Autoswarm — Epic [KEEP]
    Accuracy up; auto-dive; bullet speed / spread package (vanilla spirit).

B4. Quick Grouping — Rare [KEEP verb; FIX]
    ~6 pellets per shot spirit; +mag/+ammo; −RoF.
    **Does not change fire mode.**

B5. Explosive Pellets — Epic [KEEP]
    Pellets explode; per-pellet damage down; explosion size up.

B6. High Caliber Pellets — Rare [KEEP]
    Fire slower, higher-damage pellets.

B7. Needle Swarm — Rare (Over Extended / Tight Planting merge spirit)
    +Range/accuracy; slight −mag or −spread width (pick one tax).

B8. Rapid Pollination — Standard [KEEP spirit]
    +Fire rate; −accuracy. Swarmfront hose filler.

------------------------------------------------------------------------------
PATH C — SECOND SKIN                                             [8]
------------------------------------------------------------------------------

C1. Second Skin — Exotic (NEW KEYSTONE)
    Pellets bind as body-orbit escort. M1 feeds orbit; release flings shell.
    Soft orbit cap. Contact threat in radius. See §6.3.

C2. Crop Duster — Exotic [KEEP; SACRED]
    Airborne auto-spawn pellets; spin-up with air time; works unequipped.
    With Second Skin: spawns feed orbit. Without: forward hover seed.

C3. Hiveburst — Epic (NEW)
    Fling damage and/or size scales with orbit pellets consumed.

C4. Chitin Veil — Epic (NEW / Migration DNA)
    While orbit ≥N pellets: +move speed and small DR.
    Sprint-fire allowed while orbit active (Migration spirit folded here
    or split to C6 if too dense).

C5. Swarm Girdle — Rare (NEW)
    +Orbit capacity (max pellets worn) and/or +orbit radius.

C6. Migration — Rare [KEEP name; RETIE]
    Fire while sprinting/sliding (if not fully folded into Chitin Veil).
    Prefer Skin-relevant: also slight orbit feed efficiency while sprinting.

C7. Blitz Planting — Rare [KEEP spirit; RETIE]
    Firing / feeding orbit briefly increases move speed.
    Skin clear tempo card.

C8. Empty Comb — Standard (NEW)
    After a full fling (orbit emptied), briefly +reload speed or +feed rate.
    Teaches dump → refill cadence.

------------------------------------------------------------------------------
THIN CO-OP                                                       [2]
------------------------------------------------------------------------------

S1. Breeding Season — Epic [KEEP + solo fallback]
    Extra pellets from nearby swarm-family allies while firing.
    Solo: reduced self-echo pellet on cooldown so card is never brick.

S2. Cross Pollination — Rare [KEEP]
    Pellets track to players and heal (modest). Thin only.

------------------------------------------------------------------------------
GLUE / SHOCK / STAPLES                                           [8]
------------------------------------------------------------------------------

G1. Electrified Pellets — Rare [KEEP]
    Pellets apply shock.

G2. Static Reload — Rare [KEEP]
    Electrocuting a target with any weapon increases this weapon’s reload speed.

G3. Large Energy Storage — Standard [KEEP spirit]
    +Magazine size (non-THMG staple).

G4. Swarm Reserves — Standard [KEEP spirit]
    +Ammo capacity.

G5. Tool for the Job — Rare [KEEP spirit; optional retune]
    More ammo regen from damage with other weapon; +ammo capacity.
    Supports Mitosis without hold-R.

G6. Sprinkler — Rare [KEEP or merge with Needle Swarm if pool tight]
    +Range and +spread.

G7. Hardened Carapace — Standard (NEW staple)
    Modest +% pellet damage. Distributed power rule.

G8. Edge Fault — Contraband/Oddity [optional KEEP]
    Grid size toy if you want a second grid oddity; else backlog.

------------------------------------------------------------------------------
POOL TARGET (~30)
------------------------------------------------------------------------------

Recommended frozen 30 for v1:

  EXOTIC (7)
    1  The Hive Must Grow
    2  Wait Charging
    3  Mitosis
    4  Scatterburst
    5  Guidance Array
    6  Second Skin
    7  Crop Duster

  EPIC (7)
    8  Remote Control
    9  Atmospheric Energizers
    10 Autoswarm
    11 Explosive Pellets
    12 Hiveburst
    13 Chitin Veil
    14 Breeding Season

  RARE (10)
    15 Quick Grouping          (no semi)
    16 High Caliber Pellets
    17 Pollen Clock
    18 Queen’s Share
    19 Early Swarm
    20 Swarm Girdle
    21 Migration
    22 Blitz Planting
    23 Electrified Pellets
    24 Static Reload

  STANDARD (5)
    25 Deep Reserves
    26 Rapid Pollination
    27 Large Energy Storage
    28 Hardened Carapace
    29 Empty Comb

  ODDITY (1)
    30 Boundary Incursion

  BACKLOG (designed, expand later)
    Needle Swarm, Sprinkler, Tool for the Job, Swarm Reserves,
    Cross Pollination (if cut from 30 for space — prefer ship it by
    swapping Empty Comb or Hardened Carapace if co-op matters more),
    Edge Fault, Multiversal Thievery, Precise Targeting (cut unless ADS
    becomes real), Power Redistribution (cut)

  Prefer shipping **Cross Pollination** in the first 30 by replacing
  Empty Comb or Hardened Carapace if thin co-op must be complete on day one:

  ALT swap: 29 Cross Pollination (Rare), Empty Comb → backlog.

------------------------------------------------------------------------------
HARD CUT (do not port)
------------------------------------------------------------------------------

- Portable Solar Array (hold-R heal)
- Overload Contingency (hold-R melee)
- Munition Siphon (hold-R grenade charge)
- Power Redistribution (reload-move generic — default cut)
- Any new hold-R utility pile

---

## 9. Example Builds (mix-and-match encouraged)

**Classic Apiary (poster)**  
Hive Must Grow + Wait Charging + Mitosis + Remote Control  
+ Atmospheric Energizers + Queen’s Share + Deep Reserves  
Plant, steer, cook, rain forever.

**Scatter Guidance (poster)**  
Hive Must Grow + Scatterburst + Guidance Array  
+ Explosive Pellets + Quick Grouping + Large Energy Storage  
Semi volleys that track; hive mag offsets Scatterburst tax.

**Second Skin racer (poster)**  
Hive Must Grow + Second Skin + Crop Duster  
+ Hiveburst + Chitin Veil + Blitz Planting + Migration  
Feed orbit, stay airborne, fling shells through packs.

**Shock hive (glue hybrid)**  
Electrified Pellets + Static Reload + Guidance Array or Second Skin  
+ THMG + Autoswarm  
Status factory on any loop.

**Worn battery (A+C hybrid)**  
Second Skin + Wait Charging + Mitosis + Energizers + Remote Control  
Orbit charges over time; mitosis rains from the body; fling as execute.

**Co-op gardener (thin)**  
Breeding Season + Cross Pollination + Crop Duster + THMG  
+ any path crown  
Supportive colony without being a fourth path.

---

## 10. Economy Rules of Thumb

- **THMG** is the mag story; don’t also put huge flat mag on every exotic.
- **Scatterburst** mag tax should hurt without THMG and feel clever with it.
- **Mitosis** must cost real reserve; Energizers + Deep Reserves + Tool for the Job are the answers — not free rain.
- **Wait Charging** ceiling: long cook is strong, not infinite AFK delete; duration cap stays.
- **Orbit cap** prevents Skin from storing a second Scatterburst for free; Hiveburst spends the store.
- **Crop Duster** unequipped power should remain noticeable but not out-DPS equipped path crowns while on a different primary.
- **Guidance** ammo fat + THMG can balloon reserves — watch Mitosis infinite loops with Energizers (regen delay is the brake).
- **Damage language:** prefer **% multipliers** on staples (Hardened Carapace) consistent with Junk Flinger lesson; volume verbs (BPS, orbit, mitosis rain) are first-class power.
- Watch **A+B+C** stacked tracking + orbit contact + mitosis rain — soft taxes on hybrids, not ban lists.

---

## 11. Strengths / Weaknesses

**Strengths**
- Three readable fantasies (time / trigger / body)
- Grid adjacency puzzle (THMG) remains the joy of building
- High skill expression (Remote cook, fling timing, air Skin)
- Clear poster builds players already love, elevated
- Shock and thin co-op without drowning identity

**Weaknesses**
- Apiary punishes panic-dump and empty reserves
- Scatterburst punishes whiffed semi volleys and low mag without hive
- Second Skin punishes standing still at long range without fling aim
- Not the best pure turret long-range brain-off gun without Guidance
- Orbit VFX/perf must stay readable in multiplayer chaos

---

## 12. Success Criteria / Player Fantasy Checklist

- [ ] “I grew a ridiculous hive on THMG and my mag is a novel.”
- [ ] “I cooked a cloud for five seconds and the boss melted.” (Apiary)
- [ ] “I semi-volley’d a doorway and everything tracking-died.” (Swarmfront)
- [ ] “I flew the lane wearing bees and flung the halo through a pack.” (Skin)
- [ ] “Crop Duster kept my skin full while I stayed airborne.”
- [ ] “Mitosis ate reserves but Energizers paid me back while I hovered.”
- [ ] “Quick Grouping felt like a chunky auto hose — not accidental semi.”
- [ ] “Scatterburst is actually semi and feels intentional.”
- [ ] “Electrified + Static Reload makes reload a shock reward on any path.”
- [ ] “Breeding Season still did something when I solo queued.”
- [ ] “I never equipped Solar / melee charge / grenade siphon because they don’t exist.”

---

## 13. Visual, Audio & Thematic Design

- **Appearance:** SAXON industrial launcher + living comb / pollen grit; orbit mode adds a visible halo of pellets around the player.
- **Apiary:** denser hover cloud, charge shimmer with Wait Charging, mitosis spore-rain.
- **Swarmfront:** louder thicker muzzle volleys; Guidance streaks; Scatterburst chunky single cough.
- **Second Skin:** tight orbit whoosh, fling snap, Crop Duster aerial seed trail into the halo.
- **THMG:** subtle comb growth VFX when neighbors attach (optional, low priority).
- **Flavor:** SAXON brochure energy — “Why aim one bullet when you can employ a workforce?”

---

## 14. Implementation Notes (for later)

- Host state on `HiveLauncherBehaviour` (or SwarmGun subclass if prefab path):
  - hover list (vanilla)
  - orbit list + cap + fling request
  - wait-charge timers per pellet or cloud average
  - energizers regen clock (reserve only)
  - crop duster spin-up (equipped + unequipped hooks)
  - breeding solo-echo cooldown
- Fire mode: honor Scatterburst semi flag; strip fire-mode writes from Quick Grouping port
- Harmony/hooks as needed for unequipped Crop Duster and orbit contact damage
- Registration: new gear id + upgrade id range; vanilla Swarm untouched
- Names: ship **Hive Launcher**; upgrades rename-flexible except sacred KEEP names players already love
- VFX priority: orbit halo, fling, mitosis rain, wait-charge glow

---

## 15. Deliberate Non-Goals

- No patching vanilla Swarm Launcher in place
- No hold-R Solar / melee / grenade siphon
- No fourth “Colony glue” path (mobility + shock + co-op amalgam)
- No Nest/turret-as-path (rejected)
- No Bloom exponential split-as-path (rejected; tiny hatch crumbs backlog only)
- No hard keystone ban list
- No forcing THMG for basic function
- No Quick Grouping semi
- No leaving Scatterburst full-auto bug as a “feature”

---

## 16. Open Tuning Questions (playtest, not design blockers)

1. Second Skin: convert all hover to orbit always, or dual-pool with dock-on-release?
2. Fling input: M1 release vs separate commit — which reads better with Apiary dive release?
3. Orbit contact DPS vs fling alpha split (50/50 start?).
4. Crop Duster unequipped strength vs equipped Skin feed rate.
5. THMG mag per neighbor numbers vs Scatterburst mag tax.
6. Mitosis + Energizers sustain ceiling (regen delay / per-tick).
7. Whether Wait Charging + orbit time is too easy vs world-hover skill.
8. Breeding Season solo-echo strength (must feel like 30–50% of full ally value, not 5%).
9. Exotic count 7 vs drop one Apiary exotic to Epic if exotic slot pressure is high.
10. Ship Cross Pollination in first 30 vs backlog.

---

## 17. Review Decisions Locked

| Topic | Decision |
|-------|----------|
| Product shape | Separate primary **Hive Launcher**; vanilla untouched |
| Rename weapon | Yes |
| THMG | Universal exotic build-around |
| Path A | Apiary — hover time-battery |
| Path B | Swarmfront — trigger volume / semi volley |
| Path C | **Second Skin** — body-orbit (not Colony glue) |
| Scatterburst | +~30 BPS, mag down, RoF down, **semi ON**, bugfix auto |
| Quick Grouping | No fire-mode change |
| Co-op | Thin branch + solo fallback on Breeding Season |
| Hold-R utilities | All cut (Solar, Overload, Munition Siphon) |
| Crop Duster | Sacred; Skin spine when equipped; unequipped kept |
| Electrified + Static Reload | Keep; shock glue not a path |
| Crowns | Soft hybrids only |
| Pool | ~30; exotics larger equal shapes |
| Energizers | Bake reserve-hover regen DNA into the card |

---

## 18. Changelog vs vanilla Swarm Launcher (design intent)

| Vanilla | Hive Launcher |
|---------|----------------|
| Two real builds + niche drawer | Three peer loops + hive universal |
| THMG great but lonely system | THMG is the grid framework for all paths |
| Scatterburst bugged auto | Semi volley identity enforced |
| Quick Grouping forces semi | Pellets/economy only |
| Hold-R Solar / melee / grenade siphon | Cut |
| Crop Duster cool but unsupported | Second Skin spine + sacred unequipped |
| Co-op cards brick or lonely | Thin branch + solo fallback |
| Colony-as-path (mobility+shock+coop) | Rejected as glue; Skin is real loop |
| Atmospheric Energizers mag-ish regen | Reserve regen while hover/orbit (rework DNA) |

---

## 19. Next steps (implementation later)

1. Freeze any remaining name bikeshed (weapon is Hive Launcher unless marketing renames).
2. Resolve open questions §16.1–2 (orbit bind rule + fling input) before coding Skin.
3. Port template → HiveLauncher gear id, behaviour, fire hooks.
4. Implement frozen 30; verify Scatterburst semi + Quick Grouping auto.
5. Playtest poster builds + empty grid + hybrid freak.
6. Thunderstore package as sandbox content mod.
