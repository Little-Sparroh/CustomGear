# Siege Cannon — Design Document (v1)

## 0. Locked Decisions (2026-08-07)

| Decision | Lock |
|----------|------|
| Product shape | Parallel new primary — vanilla Gunship Cannon untouched |
| Player-facing name | **Siege Cannon** |
| Paths | **Battery / Halo / Ordnance** |
| Baseline spool / spin-up | **None** — gun does not have spool as baseline identity |
| Plant / hose peer path | **Cut** — do not overlap Chaingun |
| Halo | Full peer path (not a single exotic on missiles) |
| Halo stowed QoL | First-class (SparrohsTurbocharges DNA) — halo works while weapon stowed |
| Siege power | **No nerfs** to siege fantasy / trio (already nerfed in live Gunship) |
| HP + FF + Tinderbox stack | **No deliberate break** in v1 — fix 1D by elevating peers |
| Point Demolition line | **True AP kinetic spike** (no blast; armor/part crack) |
| Heavy Munitions | True heavy: less capacity, more mass, **knockback on hit** |
| Prisms | Special system — **outside** the ~30 pool |
| Rename pass | Full new player-facing names (vanilla names = DNA only) |
| Exotic count | **E6** — Battery 3 + Halo 2 + Ordnance 1 |
| AIM priority | Fire Mission dump > Halo command > ADS |
| Doc scope | Full bible |
| MycoMod (impl) | IsSandbox |
| Working APIName | `siege_cannon` |
| Working GUID | `sparroh.siegecannon` |

---

## 1. High Concept / Fantasy

A SAXON **operator-portable gunship shell system** — the same explosive rotary thump players love on Gunship Cannon, rebuilt so the grid has **three peer careers** instead of one correct sentence.

Baseline is honest **explosive shells**: mid-range thump, visible travel, blast pressure. No spin-up minigun tax, no free siege transform, no free orbit, no free missiles.

Upgrades fork the airframe:

- **Battery** — artillery tempo: fat shells, breech ritual, cook-off, knockback mass, AP spike
- **Halo** — lagrange denial: orbiting shells, stowed minefield, geometry, shred-by-presence
- **Ordnance** — under-wing package: seekers, fire-mission AIM dumps, CAS airborne carrot

**One-liner:** Keep the boom. Make the halo a career. Give the bay a fire mission.

**Product shape:** New parallel primary (**Siege Cannon**). Does **not** replace vanilla Gunship Cannon, Chaingun, Cycler, DMLR, or any other gun.

**SAXON marketing blurb (draft):**
  “SAXON SC-4 Siege Cannon — Ground-authorized gunship shell feeder for personnel who
  failed the flight physical but passed the attitude exam. Battery packages, lagrange
  halo magazines, and under-wing fire missions are field-swappable. Orbiting shells may
  remain armed while the weapon is stowed. (Legal did not approve that sentence.)”

Optional stingers:
- “If the shell is larger than the problem, you brought the correct shell.”
- “The halo is not a toy. It is a mobile no-fly zone for bugs.”
- “Missiles are for when the shells need a second opinion.”
- “Mash the breech like you mean it. The aircraft manual is a suggestion.”

---

## 2. Role & Fantasy in the Arsenal

- **Slot:** Primary
- **Range:** Mid (real shell travel; not SMG panic, not sniper pin)
- **Role:** Explosive shell pressure / optional siege spike / optional halo denial / optional ordnance package
- **Gap filled:**
  - Vanilla Gunship = fun but **1D siege meta** + weak hose path + orphan exotics (Missile Bay, Lagrange Halo)
  - **Chaingun** = kinetic MG, spool, swap-turret, Conference — **owns hose/spool**; Siege Cannon must not clone it
  - Cycler = energy heat hose
  - DMLR = anatomy laser execute
  - Heavies = big cooldowns, not primary shell doctrine

**Not trying to be:** Chaingun 2.0, vanilla Gunship with stickers, pure missile secondary, spin-up minigun, or “only Shipkiller forever.”

### 2.1 Comparison snapshot

```
Weapon                 Niche                         Siege Cannon differentiator
---------------------  ----------------------------  ------------------------------------------
Vanilla Gunship        Explosive rotary (1D meta)    Parallel rework pool; peer Halo + Ordnance
Chaingun               Kinetic MG + spool + turret   Explosive shells; NO spool path; NO turret
Cycler / Heat Cycler   Energy hose / heat            Mag + shell boom; breech/halo/missiles
DMLR                   Anatomy transfer laser        Blast / AP shell / missile / orbit nouns
Swarm Launcher         Hover-dive organic            Hardpoint missiles + halo, not breed swarm
Friend in a Box        Grenade pet                   No pet; ordnance and halo are yours
```

### 2.2 Why no spool path

Vanilla Gunship’s spin-up / plant / volume ladder feels weak because big damage cards are siege-shaped, and a “real” hose career **duplicates Chaingun**. This rework:

- Keeps **explosive shell** identity
- Elevates **Halo** (unique Gunship space) into a full path
- Leaves spool/plant/volume MG fantasy to **Chaingun**
- Does **not** put light spool on baseline either — user lock: weapon has no spool baseline

---

## 3. Design Pillars

1. **Explosive shells are baseline identity** — boom is the default noun.
2. **No baseline spool** — fire rate is a gun stat, not a spin-up skill curve.
3. **Siege is allowed to be strong** — do not fix 1D by nerfing Battery again.
4. **1D is fixed by peer gravity** — Halo and Ordnance get crowns that win without Battery.
5. **On-hit verbs and delivery mode > flat % stickers.**
6. **Armor literacy** — explosion is king of *clear*; kinetic AP and halo shred / missiles are kings of *plated problems*.
7. **Three peer paths; hybrids intended; no anti-synergy matrix.**
8. **R = reload / breech theater; AIM = fire-mission or halo command; no Chaingun swap-deploy.**
9. **~30 ship upgrades**; prisms **outside** that count; exotic shapes larger & equal cell count.
10. **Lagrange / Halo works while stowed** as path-defining behavior.
11. **Full rename pass** for rework identity; vanilla names are DNA only.
12. **Fun readable gunship toys** over pure spreadsheet DPS.
13. **Vanilla Gunship remains** for players who want the old pool.
14. **Do not ship a peer path whose pitch is hold-M1 spool plant volume.**

---

## 4. Core Mechanics & Gunfeel (Baseline)

### 4.1 Base gun

| Trait        | Draft / intent |
|--------------|----------------|
| Fire mode    | Full-auto **explosive shells** — no spool ramp |
| Damage       | Mid per shell (vanilla Gunship ballpark ~24) |
| Blast        | Modest baseline explosion (identity); not room-delete free |
| RoF          | Fixed gun-stat cadence (~240 RPM class target — tune in playtest) |
| Mag / reserve| Large mag (~60), hungry reserves (~180) — Gunship ballpark |
| Projectile   | Visible shell travel (~70 speed class), gravity, thump impact |
| ADS / AIM    | ADS optional; **AIM claimed by Ordnance charger and/or Halo command** when equipped |
| Handling     | Heavy CAS gun; readable recoil; not a mobility SMG |
| Model/audio  | Borrow Gunship until art; shell thumps, blast punch, breech weight |

Draft firefeel band (VALIDATE IN PLAYTEST):
- RoF: stable full-auto, no spin-up curve
- Mag: ~60
- Reserves: hungry relative to mag
- Per-shell damage + modest blast: pressure weapon, not sniper
- hitForce: modest baseline; Dense Core owns real knockback

### 4.2 No spool (baseline — locked)

- There is **no** spool01, no barrel RPM skill curve, no plant-for-RoF on baseline.
- Fire interval is a normal GunData stat.
- Cards may change RoF as a **stat** (rare generics) but must not rebuild a minigun path.
- Visual barrel spin (if any on the borrowed model) is cosmetic only unless a future backlog card explicitly says otherwise — **not** a power system in v1.

### 4.3 Inputs

| Input        | Baseline                         | Upgraded claims |
|--------------|----------------------------------|-----------------|
| Hold M1      | Explosive full-auto              | Battery caliber, Halo capture, Ordnance sidepod riders |
| AIM / RMB    | ADS tighten (if any)             | **Fire Mission dump** (Ordnance) or **Halo command** (Halo) per priority |
| R            | Reload only                      | Battery breech mash / cook theater when those cards equipped |
| Weapon swap  | Normal                           | **Halo persists while stowed** if Halo path unlocked |
| Sprint       | Standard (no free sprint-fire)   | Halo cards often grant sprint-fire |

### 4.4 AIM priority (LOCKED)

When multiple systems want AIM:

1. **Fire Mission dump** — if Fire Mission Array (or charger ladder) is equipped AND charge ≥ dump threshold AND player requests dump
2. **Halo command** — if a Halo command card is equipped (detonate / pulse / expand)
3. **ADS** — default aim behavior

Tap vs hold split is a playtest knob if both Ordnance and Halo command are equipped (e.g. tap = missile, hold = halo pulse). Not a v1 design blocker; priority table above is the rule of thumb.

### 4.5 Baseline combat loop (zero upgrades)

```
M1 hold → explosive shells pressure mid-range packs
   → track threats, manage blast spacing
   → R when dry
   → AIM is ADS only
   → no missiles, no halo, no mag-1 siege, no mash breech, no cook-off
```

Skill without upgrades: spacing, blast placement, reload timing, not face-tanking at bad ranges.

### 4.6 What baseline does NOT include

- No spool / spin-up / plant-RoF
- No mag-1 siege transform
- No mash-R breech / cook-off
- No tracking missiles / charge meter
- No lagrange orbit
- No kinetic AP mode
- No shred stacks
- No bomblet split
- No infinite ammo
- No sprint-fire
- No stowed weapon effects

Those are path-, exotic-, or unlock-owned.

---

## 5. Shared Framework Vocabulary

Upgrades speak these verbs. Baseline owns BlastShell full-auto only.

### 5.1 DeliveryMode (per shot / per payload)

```
BlastShell    — default explosive sphere on impact
KineticSpike  — no blast; fat direct hit; armor/part crack rules
Missile       — tracking projectile; on-arrive payload rules
OrbitShell    — halo-captured shell; contact / command / timeout detonate
```

Upgrades **tag** which modes they buff. Mode swaps are explicit (e.g. Bodkin Spike sets KineticSpike).

### 5.2 Armor literacy (anti–explosion-only meta)

| Channel | Good at | Rules of thumb |
|---------|---------|----------------|
| **BlastShell** | Multi-part clear, packs, soft targets | Full splash; **reduced vs heavy armor / thick shells** unless Shredded or Battery invests in Crack/breach |
| **KineticSpike** | Plated parts, shells, single hard targets | **No sphere**; high direct; **bonus vs armor/shell/plated**; applies **Crack** |
| **Missile** | Priority targets, airborne, out-of-angle | Seek + **armor-fair direct** coefficient better than uninvested splash; optional modest arrive-blast that still respects armor rules |
| **Halo** | Denial, geometry clear, openers | Pass chip + detonate; applies **Shred** by presence/passes so splash and missiles become honest |

**Crack (Battery / AP):** short debuff on a part — next blast/missile against that brain gains bonus vs that part / inward crumb.  
**Shredded (Halo):** whole-brain soft open for splash and arrive-blasts.  
**Painted (Ordnance):** missile priority + charger bonus.

This is how Bodkin stops being a trap **without nerfing siege blasts**: AP is a *different win condition*, not “delete your identity for +50.”

Explosion stays fun and strong for clear. It is no longer the only correct noun for every problem.

### 5.3 Breech (Battery R-theater)

- Mash R / crank / cook verbs live here when cards say so
- **Not nerfed** — still the siege reload fantasy
- Halo prefers orbit management + command; Ordnance prefers AIM spends
- Baseline R is always normal reload when no breech cards are equipped

### 5.4 Halo (path spine)

- **Capture** — shell joins halo instead of (or in addition to) direct flight
- **Orbit shell** — active body: cap, radius, angular speed, lifetime
- **Bank** — shells held in halo as stored pressure
- **Release / Command** — detonate all / slice / expand (AIM when claimed)
- **Stow link** — halo bound to **player**, survives weapon stow/swap
- **Shred (Halo)** — orbit passes / proximity build armor-open stacks

### 5.5 Bay / Charge (Ordnance)

- **Bay:** tracking missiles (side-pod cadence, on-shot fraction, or replace — card-defined)
- **Charge / Fire Mission:** damage dealt fills meter; AIM dumps tracking missile ∝ charge
- Missiles use armor-fair direct rules (+ optional loft blast)

### 5.6 Heat / Cook (Battery spice)

- Cook-Off line: mash or fire builds cook; Fire infusion + damage; combust risk stays as identity tax
- Not a Cycler full heat rewrite — shell-cook fantasy only

### 5.7 What we deliberately do NOT vocabulary

- Spool / redline belt / plant max-RoF (Chaingun)
- Conference Call origin displacement (Chaingun)
- Swap-deploy turret (Chaingun)
- Friend pet AI (Friend in a Box)
- Anatomy transfer laser (DMLR)

---

## 6. Paths (gravity wells — hybrids intended)

### Path A — BATTERY (siege / caliber / breech)
**“One shell. One argument.”**

- Spine: fat shells, blast size with breach tools, mag-1 / low-mag drama, mash breech, cook-off, heavy mass knockback, AP spike
- ST native; clear via huge spheres
- **Power budget: HIGH and allowed** — peer paths match height; we do not drag Battery down
- Hybrid hooks: AP into cracked parts; charger dumps into cooked targets; fat shells captured into halo as orbiting bombs

### Path B — HALO (orbit / denial / geometry)
**“The magazine doesn’t leave. It orbits.”**

- Spine: capture, orbit cap/radius, stowed persistence, command detonate, shred-on-pass, sprint-CAS with halo up
- Clear native via walking the field through packs; ST via banked detonations + shred openers
- **Why it wins without Battery:** uptime while you use other gear; geometry control; shred opens plate for any delivery
- Hybrid hooks: Battery shells become orbit bombs; Ordnance + halo layered denial; shred feeds missile/blast finishers

### Path C — ORDNANCE (missiles / fire-mission / airframe)
**“Under-wing package. Operator optional.”**

- Spine: hardpoint missiles, fire-mission charge→AIM dump, paint/priority, seeker quality, CAS airborne carrot
- Clear via multi-missile + loft; ST via charged fire-mission execute
- **Why it wins without Battery:** armor-fair direct seek + paint execute; does not need Shipkiller radius
- Hybrid hooks: Battery cook on arrive; halo + bay weather; airborne CAS with either path

### Path × verb matrix

```
                 BATTERY                 HALO                      ORDNANCE
BlastShell       caliber / breach        detonate weather          arrive-blast riders
KineticSpike     AP crown line           —                         optional tip hybrid (backlog)
Shred            spends Crack            core fantasy (passes)     missiles benefit
Breech / Cook    core fantasy            —                         —
Capture/Orbit    hybrid fat orbit bombs  core fantasy              —
Bay / Charge     hybrid dump amp         —                         core fantasy
Stowed effect    —                       core fantasy              —
AIM claim        —                       command (if no FM dump)   Fire Mission dump
R claim          breech / cook           reload; optional reel     reload (default)
Sprint-fire      —                       common on Halo cards      optional CAS crumbs
Spool/Plant      NEVER                   NEVER                     NEVER
```

---

## 7. Crowns & Sacred Cows

### 7.1 Exotics (E6 — equal large shapes)

**A-EX1. Shipkiller Breech — Exotic** (Hull Piercer DNA — **not nerfed**)  
- Magazine collapses toward **1** (or ultra-low).  
- Damage, explosion size, bullet size **greatly** increased.  
- Fire rate / efficiency / ammo taxes remain part of the fantasy (heavy tax, high payoff).  
- Still the “delete the argument” button.  
- **No power cut vs live siege fantasy.**

**A-EX2. Manual Crank — Exotic** (Frantic Fingers DNA — **not nerfed**)  
- Normal reload disabled or bypassed.  
- Mash R to chamber ammo into the mag.  
- Designed to still sing with Shipkiller (siege mash fantasy preserved).  
- Stack with Cook-Off is **allowed** (no anti-synergy pass in v1).

**A-EX3. Cook-Off Protocol — Exotic** (Tinderbox DNA — **not nerfed**)  
- Mash R (implementation may also allow fire-build cook if cleaner) overheats/cooks shells: **Fire infusion + damage**.  
- Reload rules can stay empty-mag gated as vanilla fantasy.  
- Spontaneous combust chance remains the spicy tax.  
- Stack with Manual Crank is **allowed**.

**B-EX1. Skyhook Halo — Exotic** (Lagrange Halo DNA + Turbocharges stowed QoL)  
- Enables **capture + orbit** system (path unlock).  
- Sprint-fire enabled while exotic equipped (or while halo active — tune).  
- **Halo shells persist and function while weapon is stowed.**  
- Orbit radius / shell cap / lifetime tuned so it is denial weather, not invisible infinite minefield.  
- First Halo upgrade that enables the system: this exotic **or** any Halo card that sets HaloUnlocked (see §8 — Skyhook is the mode-defining exotic; cheaper cards may also unlock lite capture if desired — v1: **Skyhook enables full system**; supporting cards require it OR also set unlock on Apply for draft flexibility).  

**Unlock rule (LOCKED for v1):**  
- **Skyhook Halo** enables Halo system (capture, orbit, stow link).  
- Other Halo cards either require Skyhook or no-op their halo bits until unlocked — prefer **require Skyhook** for readability.  
- Exception: none in first 30.

**B-EX2. Shepherd’s Crown — Exotic** (Halo mythic)  
- Build-defining halo payoff: **+orbit cap**, smarter contact rules, **Shred-on-pass** at strong rate, optional widen-on-kill or dual-ring lite.  
- Stowed halo damage/presence slightly amplified (reward obj / heavy swap play).  
- Still respects hard cap + timeout — not map-wide permanent mines.  
- The “halo is a career” crown.

**C-EX1. Fire Mission Array — Exotic** (Missile Bay + Ballistic Charger fused)  
- Enables **tracking missile** system.  
- Damage dealt fills **Fire Mission** charge; **AIM** launches a tracking missile with payload ∝ charge.  
- Also enables lighter **pod missile** cadence or on-shot fraction as baseline-with-exotic (ladder cards specialize bay vs pure charger).  
- Missiles use **armor-fair direct hit** rules (+ optional modest arrive-blast from loft cards).  
- This is the Ordnance path unlock exotic.

### 7.2 Sacred cows (do not cut without rewriting identity)

- Parallel catalog weapon; vanilla Gunship untouched  
- Explosive BlastShell baseline  
- **No baseline spool; no spool/plant peer path**  
- Siege trio unnerfed and all Exotic  
- Halo is a full path with **stowed persistence**  
- Bodkin = true AP kinetic (no blast)  
- Dense Core = less ammo, more mass, enemy knockback on hit  
- Fire Mission owns AIM dump priority over Halo command  
- Prisms outside the 30  
- Three peer paths; hybrids OK  
- ~30 upgrades; equal large exotic shapes (6)  
- Not Chaingun; not Friend pet; not DMLR anatomy gun  

---

## 8. Full Upgrade List (~30 ship + backlog)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity  
Cell rule: Exotic shapes larger than others; all Exotics same cell count.  
Prisms: **not in this list** (special / global — do not spend pool slots).  
Player-facing names below. API names assigned at implementation.

Vanilla DNA → rework name (inspiration only):

| Vanilla | Rework |
|---------|--------|
| Hull Piercer | Shipkiller Breech |
| Frantic Fingers | Manual Crank |
| Tinderbox | Cook-Off Protocol |
| Heavy Munitions | Dense Core Shells |
| Lagrange Halo | Skyhook Halo (+ stowed) |
| (new) | Shepherd’s Crown |
| Missile Bay + Ballistic Charger | Fire Mission Array |
| Point Demolition | Bodkin Spike |
| Higher Caliber | Bore-Up |
| Thumper | Cadence Ram (backlog / generic) |
| Firmly Planted / Industrial Feed / Overclocked | **Not path-owned** — backlog or omit |
| Gunship (airborne) | CAS Envelope |
| Elemental Munitions | Cocktail Fuzes (backlog) |
| Safety "Regulations" | Illegal Volley (backlog) |
| Unarmored | Exposed Receiver |
| Hold-Plating | Blast Skirt (backlog) |
| Adaptive Stabilizers | Gyro Settle (backlog thin generic) |
| Cargo Storage / Magnetic Loader | Spare Crates / Speed Loader |

------------------------------------------------------------------------------
PATH A — BATTERY
------------------------------------------------------------------------------

A-EX1. Shipkiller Breech — Exotic (crown)
       Mag toward 1; huge damage / explosion / bullet size; heavy RoF/efficiency/ammo taxes.
       **No power cut vs live siege fantasy.**

A-EX2. Manual Crank — Exotic (crown)
       Reload disabled; mash R chambers ammo. Full siege mash fantasy retained.

A-EX3. Cook-Off Protocol — Exotic (crown)
       Mash-R cook: Fire + damage on shells; empty-mag reload rules; combust chance tax. Retained.

A-EP1. Dense Core Shells — Epic (Heavy Munitions rewrite)
       −Magazine and/or −reserves (heavier belt).
       +Damage, +bullet mass/size.
       **On-hit knockback to enemies** (headline fantasy).
       Optional light self-slide on fire as drawback flavor only (default: **enemy knockback only** — see open questions if revisited).
       Name finally matches effect.

A-EP2. Bore-Up — Epic
       +Explosion size, −fire rate (Higher Caliber DNA). Battery clear ladder.

A-EP3. Cracked Fuze — Epic
       Blast hits apply **Crack** to the primary part hit.
       Next direct / missile hit on that brain gains bonus vs cracked part / small inward crumb.

A-RA1. Drag Brakes — Rare
       +Damage, −range (Drag Caps DNA).

A-RA2. Hard Seat — Rare
       +Recoil control; small +damage when Battery caliber cards present (or unconditional mild +damage / −spread).

A-ST1. Thick Casings — Standard
       Minor +damage.

------------------------------------------------------------------------------
PATH B — HALO
------------------------------------------------------------------------------

B-EX1. Skyhook Halo — Exotic (path unlock crown)
       Capture + orbit + sprint-fire + **stowed persistence**.
       Enables Halo system for supporting cards.

B-EX2. Shepherd’s Crown — Exotic (mythic)
       +Orbit cap, stronger contact rules, strong Shred-on-pass, stowed presence amp.
       Hard cap + timeout still apply.

B-EP1. Grazing Shred — Epic
       Orbit passes / proximity build **Shred** on brains.
       Full Shred: blasts and missile arrive-blasts gain armor-ignore crumb + small concussion pulse.
       Stacks with Shepherd’s Crown (rate/caps tune — diminishing if needed).

B-EP2. Command Detonation — Epic
       AIM pulses the halo when Fire Mission is not dumping:
       - Tap: detonate nearest N orbit shells toward look
       - Or full-bank slice in aim cone (pick one primary feel at impl; prefer **cone slice** for readability)
       Clears banked pressure on purpose — skill expression.

B-EP3. Umbilical Loop — Epic
       +Orbit cap and +radius.
       While weapon stowed, halo shells gain mild +damage or +Shred rate (obj-play reward).

B-EP4. Catch Net — Epic
       Chance to recapture near-miss shells / ricochets into orbit.
       Pairs with Battery ricochet backlog; useful standalone for bank building.

B-RA1. Orbit Governor — Rare
       +Angular speed and/or tighter default radius control (snappier halo).

B-RA2. Soft Fuze — Rare
       Orbit shells prefer **delay fuze** after first enemy graze (longer presence, less instant pop).
       Alternate roll on apply: contact-eager vs delay — or fixed delay identity.

B-RA3. Standing Wave — Rare
       Orbit shells deal small **pass chip** without detonating (denial rain).
       Detonate still available via contact threshold / command.

B-RA4. Wide Lagrange — Rare
       +Orbit radius; slight −pass chip density (coverage fork).

B-ST1. Orbit Bracket — Standard
       Minor +orbit cap crumb (requires Skyhook).

------------------------------------------------------------------------------
PATH C — ORDNANCE
------------------------------------------------------------------------------

C-EX1. Fire Mission Array — Exotic (path unlock crown)
       Tracking missiles + damage→charge→AIM fire-mission dump ∝ charge.
       Armor-fair direct. Light pod cadence included.

C-EP1. Priority Paint — Epic
       Fire-mission and pod missiles prefer elites / last-hurt / Painted parts.
       Near-miss or pass applies **Painted**.

C-EP2. Sidepod Rhythm — Epic
       Every Nth shell also spawns a light tracking rocket (damage budgeted).
       Bay fantasy without deleting gun shells.
       Strengthens the “Array includes bay” read.

C-EP3. CAS Envelope — Epic (Gunship airborne rewrite)
       +Damage while airborne.
       **Carrot-forward:** little or no grounded grief (avoid vanilla-style brutal grounded penalty).
       Rewards jumps / slides / glider / airtime play.

C-RA1. Seeker Heads — Rare
       +Missile turn rate / track radius.

C-RA2. Loft Fuzes — Rare
       Missiles gain small arrive-blast (armor-literate); slight −direct if needed for budget.

C-RA3. Paint Primer — Rare
       Direct shell hits apply brief Painted (feeds missiles without AIM).

C-ST1. Hardpoint Bracket — Standard
       Minor +missile damage and/or +charge gain crumb.

------------------------------------------------------------------------------
AP / KINETIC LINE (Battery-adjacent, peer-enabling)
------------------------------------------------------------------------------

K-EP1. Bodkin Spike — Epic (Point Demolition rewrite — true AP)
       DeliveryMode → **KineticSpike** (explosion size 0).
       Large **direct damage**.
       **Bonus vs shells / armored / plated parts**.
       Applies **Crack** on hit.
       Real boss/armor path — not a trap card.

K-RA1. Sabot Sleeves — Rare
       Empowers KineticSpike: +pierce along same-brain parts (shell→limb→core preference), still no blast.

K-RA2. Anvil Tips — Rare
       KineticSpike +knockback (pairs with Dense Core fantasy on AP mode).

------------------------------------------------------------------------------
GENERIC / GUNFEEL
------------------------------------------------------------------------------

G-RA1. Exposed Receiver — Rare (Unarmored DNA)
       +Damage; +damage taken while this gun is the active weapon.

G-RA2. Range Gate — Rare
       +Falloff start / effective range.

G-ST1. Speed Loader — Standard
       +Reload speed.

G-ST2. Spare Crates — Standard
       +Ammo reserves (Cargo Storage DNA).

G-OD1. Boundary Incursion — Oddity
       Increases upgrade grid size.

------------------------------------------------------------------------------
FROZEN 30 FOR V1 SHIP
------------------------------------------------------------------------------

EXOTIC (6)
  1  Shipkiller Breech
  2  Manual Crank
  3  Cook-Off Protocol
  4  Skyhook Halo
  5  Shepherd’s Crown
  6  Fire Mission Array

EPIC (8)
  7  Dense Core Shells
  8  Bore-Up
  9  Bodkin Spike
 10  Grazing Shred
 11  Command Detonation
 12  Umbilical Loop
 13  Priority Paint
 14  CAS Envelope

RARE (10)
 15  Drag Brakes
 16  Orbit Governor
 17  Soft Fuze
 18  Standing Wave
 19  Wide Lagrange
 20  Seeker Heads
 21  Loft Fuzes
 22  Sabot Sleeves
 23  Anvil Tips
 24  Exposed Receiver

STANDARD (5)
 25  Thick Casings
 26  Orbit Bracket
 27  Hardpoint Bracket
 28  Speed Loader
 29  Spare Crates

ODDITY (1)
 30  Boundary Incursion

------------------------------------------------------------------------------
BACKLOG (designed, not in first 30)
------------------------------------------------------------------------------

Battery
- Cracked Fuze
- Hard Seat
- Cadence Ram (Thumper DNA — burst shells with stagger/Crack riders, NOT a hose path)
- Bomblet Split (impact split; coverage weather)
- Illegal Volley (multi-bullet damage split)
- Cocktail Fuzes (element chance)
- Self-slide rider on Dense Core (if ever wanted)

Halo
- Catch Net
- Dead Man’s Orbit (downed/stow edge controlled burst then clear)
- Ally-Safe Lattice (co-op friendly halo rules card)
- Reel-In (hold R tightens radius)
- Dual Ring (second orbit band — expensive, maybe never)

Ordnance
- Sidepod Rhythm
- Paint Primer
- Split Fire Mission vs Hardpoint Bay into two exotics (only if E7 ever happens)
- Loft cluster warhead
- Lock-On tone / hold-AIM paint channel

Generic
- Range Gate, Blast Skirt, Gyro Settle, Muzzle Brake, Soft Mounts
- Firmly Planted single card (NOT a path — only if gunship flavor demanded; default omit)
- Edge Fault / Multiversal (contraband globals — not required in pool)

Explicitly rejected as path identity
- Spool / spin-up trees
- Plant-for-max-RoF careers
- Redline belt hose ladders
- Chaingun Conference / swap-turret DNA
- Nerf passes on Shipkiller / Manual Crank / Cook-Off in v1

---

## 9. Example Builds

**Classic Siege (allowed to slap)**  
Shipkiller Breech + Manual Crank + Cook-Off Protocol + Dense Core Shells + Bore-Up + Thick Casings  
→ Mag-1 cooked heavy shells with mash chamber. **Still the meme. Still valid.**

**AP Surgeon**  
Bodkin Spike + Sabot Sleeves + Anvil Tips + Dense Core Shells + Drag Brakes + Manual Crank  
→ Kinetic crack boss parts; knockback control; no splash required.

**Stowed Halo Controller**  
Skyhook Halo + Shepherd’s Crown + Grazing Shred + Umbilical Loop + Command Detonation + Orbit Bracket  
→ Build the ring, swap to heavy/obj, walk denial through the room, command-slice priority packs.

**Fire Mission CAS**  
Fire Mission Array + Priority Paint + CAS Envelope + Seeker Heads + Loft Fuzes + Hardpoint Bracket  
→ Charge on damage, AIM dump elites, airborne carrot, armor-fair seeks.

**Orbiting Shipkillers (hybrid)**  
Shipkiller Breech + Skyhook Halo + Shepherd’s Crown + Bore-Up  
→ Absurd shells that bank into orbit — high clip potential; tune caps so it is fun not mission wipe.

**Shred → Missile Finish**  
Skyhook Halo + Grazing Shred + Fire Mission Array + Priority Paint + Standing Wave  
→ Halo opens armor; fire-mission executes Painted elites.

**Knockback Bully**  
Dense Core Shells + Anvil Tips + Bodkin Spike + Exposed Receiver + Drag Brakes  
→ AP mass with yeet; glass-ish receiver tax.

---

## 10. Economy & Tuning Rules of Thumb

- **Do not balance v1 by nerfing Shipkiller / Manual Crank / Cook-Off.** If the gun is 1D, buff Halo/Ordnance crowns and armor literacy first.
- Bodkin Spike must feel **better than uninvested splash on armored AI**, worse on dense packs than Bore-Up / Shipkiller.
- Shred full-stack time: grunts fast, elites medium, bosses slow / capped.
- Halo: **hard cap** active orbit shells; **timeout**; stowed persistence is QoL + path identity, not infinite map mine.
- Shepherd’s Crown raises cap and rate but still respects global safety caps.
- Fire Mission full dump should feel like a **heavy ability**, not every second.
- Sidepod / multi-missile cards: **budget damage** — coverage and priority, not free ×N DPS.
- Dense Core: capacity down must be felt; enemy knockback is the joy.
- CAS Envelope: airborne carrot; **avoid grounded grief**.
- Hybrids should be strong; watch Shipkiller + Halo bank + Fire Mission delete — fun first, then tune scalars.
- Prisms: if the game offers them to this gear, they do not consume design-30 slots.
- No upgrade should introduce a spool skill curve as a build spine.

---

## 11. Status / Counter Split

| System / counter     | Role                              | Baseline? | Owner |
|----------------------|-----------------------------------|-----------|-------|
| Explosive BlastShell | Primary damage delivery           | Yes       | Baseline |
| Spool / plant RoF    | —                                 | **No**    | **Never (v1)** |
| Shipkiller mag-1     | Siege transform                   | No        | Battery exotic |
| Manual Crank mash-R  | Breech reload theater             | No        | Battery exotic |
| Cook-Off             | Fire cook + combust tax           | No        | Battery exotic |
| Dense Core knockback | Heavy mass shells                 | No        | Battery epic |
| Crack                | Part debuff for follow-up         | No        | Battery / Bodkin |
| KineticSpike AP      | Armor/part crack ST               | No        | Bodkin line |
| Halo capture/orbit   | Denial magazine                   | No        | Halo (Skyhook) |
| Stowed halo          | Obj/heavy swap uptime             | No        | Halo |
| Shred                | Armor open via presence           | No        | Halo |
| Command Detonation   | AIM halo spend                    | No        | Halo epic |
| Fire Mission         | Charge → AIM missile              | No        | Ordnance exotic |
| Paint                | Missile priority                  | No        | Ordnance |
| Airborne CAS         | Airtime damage carrot             | No        | Ordnance |
| Chaingun turret/Conf | Not used                          | No        | Other weapon |
| Prisms               | Special                           | Special   | Outside 30 |

---

## 12. Strengths, Weaknesses & Co-op

**Strengths**
- Keeps beloved siege fantasy at full volume
- Halo is a unique career (stowed denial, geometry) nothing else owns
- Ordnance fire-missions are readable executes
- AP spike fixes armor-without-sphere
- Clear hybrid clip moments (orbiting shipkillers, shred → missile)
- Does not step on Chaingun’s spool/turret identity

**Weaknesses**
- Baseline hungrier / heavier than SMGs
- Reload mismanagement still punishes (unless Crank)
- AP weak on pure swarm without splash/halo cards
- Halo misplay can clutter FX or self-detonate poorly placed banks
- Parallel weapon = players must find/unlock it (by design)
- No brain-off spool hose — volume clear wants Halo or Battery investment

**Co-op**
- Halo denial zones help the team (readable ring)
- Shred sets up ally splash
- Knockback should use vanilla friendly knockback policy (don’t grief allies into pits as a feature)
- Fire missions need clear VFX so allies understand elite pops
- Stowed halo should not obscure ally cameras excessively (cap + FX budget)

---

## 13. Visual, Audio & Thematic Design

**Appearance**
- SAXON CAS industrial: hazard stripes, aircraft feed tray, unauthorized ground braces,
  “REMOVE BEFORE FLIGHT” tag half-peeled and ignored
- Battery: absurd shell scale, breech clank, cook hiss, white-hot fuze
- Halo: orbiting shell ring with lagrange shimmer; faint tether hum when stowed; detonate flash on command
- Ordnance: wing-pod chirp, missile whoosh, charge meter glow on weapon/HUD
- AP: sharp crack, plate sparks, no bloom sphere

**Sound**
- Baseline: heavy shell thump + blast punch (weight > bee SMG)
- Breech mash: mechanical crank / feed clack
- Cook-Off: rising hiss + combust sting on self-proc
- Halo: orbital hum (loop); stowed = quieter distant hum; command = ordered crack chain
- Missiles: lock tone crumb + launch whoosh + arrive thump
- Dense Core: deeper impact + body knockback slap

**Flavor / codex line (in-game style)**
  Siege Cannon  
  Full-auto explosive shell weapon.  
  Battery upgrades enable siege breech fantasy.  
  Halo upgrades orbit shells (including while stowed).  
  Ordnance upgrades enable tracking fire missions.

---

## 14. Implementation Notes (for later)

### 14.1 Gear registration
- Follow weapon template in this repo: clone base gun, GearInfo high-range id,
  APIName `siege_cannon`, behaviour component, SpawnGear stamp, CreateUpgrade pool.
- **Clone candidate:** vanilla **Gunship Cannon** from `Global.AllGear` (explosive shells, correct VFX/audio DNA).
  Do **not** clone CartridgeSMG unless Gunship is unavailable.
- Plugin: GUID `sparroh.siegecannon`, MycoMod **IsSandbox**.
- Persistence: stable gear id; register before `PlayerData.OnAwake` AddGear.
- Working gear id band: pick free high-range id at impl (e.g. 94xxx) — confirm unused.
- Do **not** overwrite vanilla Gunship upgrade pool.

### 14.2 Behaviour host
SiegeCannonBehaviour (or true Gun subclass when prefab exists):
- WeaponData: delivery mode flags, breech/cook flags, halo params, fire-mission params,
  knockback params, crack/shred/paint params, AIM claim flags
- Runtime: cook01, fireMissionCharge, halo shell list (owner-linked), shred trackers,
  crack timers, painted timers, crank input state
- Prefab snapshot restore on upgrade Remove
- **No spool01 field**

### 14.3 Halo / stow
Critical: halo is bound to **player/owner**, not to “gun currently equipped.”

Draft flow:
1. On capture: spawn/track orbit shell in player-local halo space
2. On weapon stow/swap: **do not despawn** halo; keep simulating contact/shred/timeout
3. On weapon re-equip: continue same halo
4. On owner death / scene transition: clear halo
5. Command Detonation reads aim from owner even if another weapon is active **only if** design allows stowed command — **v1 draft:** command requires Siege Cannon active OR Shepherd/Umbilical explicitly allows stowed command. Prefer: **stowed halo auto-behaves; command AIM requires Siege Cannon equipped** unless a card says otherwise.

Turbocharges DNA: promote stowed persistence to first-class; re-check that mod for edge cases when implementing.

### 14.4 Fire Mission
- OnDamageTarget (owner): add charge ∝ damage * efficiency
- AIM dump: if charge ≥ threshold, spawn tracking missile, spend charge (full or partial — prefer full dump for readability)
- Pod cadence: separate timer/counter from charge dump

### 14.5 Breech / Cook
- Manual Crank: block normal reload; on R tap add ammo chunks to mag from reserves
- Cook-Off: R mash builds cook; at cook threshold shells gain Fire + damage; combust roll while cooked
- Both equipped: define single R channel that serves crank ammo + cook build (vanilla Gunship already combines these fantasies — mirror that feel, unnerfed)

### 14.6 Bodkin / AP
- On apply: set DeliveryMode KineticSpike; zero explosion size on gun data / bullet flags
- OnBeforeDamage: armor/shell/plated mults; apply Crack
- Sabot: pierce same-brain part walk

### 14.7 Dense Core knockback
- On hit: apply enemy knockback using hitForce / custom impulse
- Default: **no** self-slide unless a backlog rider says so

### 14.8 Hooks

| Hook | Use |
|------|-----|
| Fire / OnFiredBullet | Capture chance; sidepod spawn; cook payload tags; delivery mode |
| OnBeforeDamage | AP mults; shred armor crumb; crack spend |
| OnDamageTarget | Fire Mission charge; paint; crack apply; shred apply from direct if needed |
| AIM input | Fire Mission dump > Halo command > ADS |
| R input | Crank / cook / reload |
| Equip/unequip | Halo persists; do not clear on stow |
| Player death / scene | Clear halo, charge, cook |

### 14.9 HUD
- Fire Mission charge bar when Ordnance exotic present
- Halo shell count / cap when Halo unlocked
- Cook meter when Cook-Off present
- Prefer SparrohUILib if dependency acceptable; else minimal existing gun HUD hooks

### 14.10 VFX / audio priority
1. Baseline shell thump + blast
2. Halo orbit readability (even when stowed — subtle)
3. Command detonation chain
4. Fire Mission launch + track
5. Breech mash + cook hiss
6. AP crack sparks (no fake blast)
7. Dense Core impact weight

### 14.11 Multiplayer
- IsSandbox; identical mod on all clients
- Halo ownership = deploying player; replicate orbit shells or owner-sim + FX
- Missiles: owner aim/charge authority with server-safe damage validation pattern matching vanilla missiles where possible

---

## 15. Deliberate Non-Goals

- Not nerfing siege trio in v1  
- Not replacing vanilla Gunship  
- Not Chaingun kinetic / spool / turret / Conference  
- Not baseline spool of any kind  
- Not a plant-minigun peer path  
- Not counting prisms in the 30  
- Not making Point Demolition a splash-delete trap  
- Not Heavy Munitions as “+mag exotic”  
- Not forced anti-synergy between Crank and Cook-Off  
- Not Friend pet AI  
- Not DMLR anatomy transfer as identity  
- Not requiring custom Unity prefab for v1 (runtime clone OK)  

---

## 16. Open Tuning Questions (playtest, not design blockers)

1. Shipkiller mag exactly 1 vs 1–3?  
2. Halo shell cap default vs Shepherd’s Crown cap?  
3. Halo timeout duration and stowed damage mult?  
4. Command Detonation: cone slice vs full bank vs nearest-N?  
5. Stowed halo: auto-only vs allow command while other weapon active?  
6. Fire Mission charge rate vs dump power?  
7. Pod cadence default on Fire Mission Array alone?  
8. Bodkin armor bonus vs pack clear weakness — feel band?  
9. Dense Core capacity penalty size vs knockback strength?  
10. CAS Envelope airborne threshold (jump peak vs any not-grounded)?  
11. Cook-Off combust chance band (keep spicy, not grief)?  
12. Capture mode: all shells vs chance vs alternate shot?  
13. Unlock method: auto-unlock like template vs progression?  

---

## 17. Success Criteria / Player Fantasy Checklist

- [ ] Vanilla Gunship still exists unchanged  
- [ ] Siege Cannon baseline is explosive full-auto **with no spool system**  
- [ ] Shipkiller + Crank + Cook-Off still feels like the beloved siege monster  
- [ ] Skyhook Halo alone makes orbit + sprint-fire + **stowed persistence** obvious  
- [ ] Shepherd’s Crown makes halo feel like a full build, not a gimmick  
- [ ] Grazing Shred opens armor so small detonations matter  
- [ ] Command Detonation is a readable AIM skill moment  
- [ ] Fire Mission AIM dump is a readable execute  
- [ ] Bodkin Spike cracks armor better than uninvested splash  
- [ ] Dense Core = less ammo, more mass, knockback joy  
- [ ] No upgrade path plays like Chaingun spool/plant hose  
- [ ] Hybrids (orbiting shipkillers, shred → missile) feel intentional  
- [ ] Prisms not consuming the 30  
- [ ] Frozen 30 ships clean  
- [ ] AIM priority respects Fire Mission > Halo command > ADS  

---

## 18. Review Decisions Locked (2026-08-07)

| Decision | Lock |
|----------|------|
| Form factor | Explosive shell primary (Gunship DNA) |
| Player-facing name | **Siege Cannon** |
| Slot | Primary |
| Paths | Battery / Halo / Ordnance |
| Baseline spool | **None** |
| Rotary/hose peer path | **Cut** (Chaingun owns that space) |
| Halo depth | Full path; 2 exotics (Skyhook + Shepherd’s Crown) |
| Halo stowed | Yes — first-class |
| Siege trio | All Exotic; **unnerfed** |
| Ordnance exotic | Fire Mission Array (Bay + Charger fused) |
| AIM priority | FM dump > Halo command > ADS |
| Point Demolition | Bodkin Spike true AP kinetic |
| Heavy Munitions | Dense Core Shells (capacity down, knockback on hit) |
| Prisms | Outside ~30 |
| Product shape | Parallel weapon; vanilla Gunship kept |
| Ship pool | Frozen 30 listed above |
| Doc file | SiegeCannon-DesignDoc.txt (this file) |
| Tone | SAXON CAS industrial gunship |
| Gimmick priority | Fun / readable toys; peer paths over siege nerfs |

---

## 19. Changelog

### v1 (2026-08-07)
- Initial full design from locked user decisions
- Paths: Battery (siege), Halo (lagrange career), Ordnance (fire missions)
- Explicitly cut spool/plant/hose peer path to avoid Chaingun overlap
- Baseline: no spool
- Siege trio retained as unnerfed exotics
- Halo expanded from single exotic into full tree with stowed persistence
- Ordnance fused Missile Bay + Ballistic Charger into Fire Mission Array
- Armor literacy: Blast / KineticSpike / Missile / Halo Shred
- Research anchors:
  - Wiki: Gunship Cannon full upgrade list (Hull Piercer, Frantic Fingers, Tinderbox,
    Heavy Munitions, Lagrange Halo, Missile Bay, Ballistic Charger, Point Demolition,
    Firmly Planted, Thumper, etc.)
  - Sibling docs: DMLR Rework, Chaingun, Cycler, Boarding Trident structure
  - User notes: explosion-king meta, siege stack 1D, weak hose, orphan exotics,
    Heavy Munitions misread, Turbocharges stowed halo QoL
- User locks: parallel weapon; no siege nerf; Bodkin = AP; Halo path not Rotary;
  no baseline spool; name Siege Cannon; E6 exotics; AIM priority

---

## 20. Implementation checklist (post-design)

- [ ] Rename plugin/csproj/thunderstore from template → SiegeCannon
- [ ] SiegeCannonBehaviour.Data fields from §14.2
- [ ] Clone Gunship Cannon; retune GunData; **no spool runtime**
- [ ] Battery: Shipkiller / Crank / Cook-Off / Dense Core / Bodkin
- [ ] Halo: capture, orbit sim, cap/timeout, **stow persistence**, command, shred
- [ ] Ordnance: charge meter, AIM dump, pod cadence, paint, seekers
- [ ] AIM priority router
- [ ] Armor literacy hooks (Crack / Shred / AP mults)
- [ ] UpgradeRegistration frozen 30
- [ ] HUD: charge, halo count, cook
- [ ] Persistence + SpawnGear stamp
- [ ] Playtest pass on §16 knobs
- [ ] Confirm halo clears on death/scene; no orphan orbit shells
- [ ] Confirm vanilla Gunship pool untouched
