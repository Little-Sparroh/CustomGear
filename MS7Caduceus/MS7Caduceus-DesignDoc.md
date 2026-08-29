# MS-7 Caduceus — Design Document (v1)

## 1. High Concept / Fantasy

A mid-range SAXON **field-medic tether primary**. Hold fire to lock a **Caduceus beam** onto an ally or an enemy. RMB cycles **polarity**: Mend (heal), Overclock (buff), Judgment (debuff). Sustained tethering builds **Grace** — a shared Über-style meter whose baseline Discharge is deliberately weak; path crowns rewrite Discharge into miracle windows (team damage gospel, true downed revive, condemnation payoffs).

The gun is Mycopunk’s first dedicated **support primary**. It does not replace DMLR Nanites, Swarm Cross Pollination, Bruiser shields, Scrapper pole HoTs, Friend in a Box, or Honey Jar nectar — those remain side toys. Caduceus owns the fantasy of *holding the line with a beam and choosing mercy or judgment*.

One-liner: Lock the tether. Cycle the polarity. Spend Grace when the squad needs a miracle.

Product shape: New primary weapon (**MS-7 Caduceus**). Does not replace any vanilla gun.

SAXON marketing blurb (draft):
“SAXON MS-7 Caduceus — Dual-polarity field tether for trauma triage and hostile designation.
Ally lock authorized. Hostile lock… also authorized. Grace discharge requires Form 12-MED.
Resurrection is not a metaphor. (It is a liability waiver.)”

Optional stingers:
- “Mend the faithful. Overclock the useful. Judge the rest.”
- “There is no self-Mend subroutine. That is not a bug. That is doctrine.”
- “If your secondary cannot keep you alive, the staff was never the problem.”

---

## 2. Role & Fantasy in the Arsenal

- Slot: Primary
- Range: Mid tether (lock band ~20–35u; linger softens break)
- Role: Co-op force multiplier; solo-viable via Judgment, not via healbotting
- Gap filled:
  - DMLR Engineering Nanites / Assist Charge = thin laser heal side-branch
  - Swarm Cross Pollination = pellet track-heal side card
  - Accelerator Honey Sweet / Sweet Heavens = bee heal side cards
  - Scrapper Stolen Nanites / Vending Machine = pole utility HoT
  - Bruiser Hard-Light Prison / With Me / One For All = class shield + death heal
  - Friend in a Box = deployable ally unit
  - Honey Jar = field bees + temporary nectar
  - Helminth Symbiote = selfish/mutual HP bond rifle
  - Nothing owns “hold-beam medic staff → Mend / Overclock / Judgment + Grace Über + true revive”

### 2.1 Comparison snapshot

```
Weapon / kit              Niche                         Caduceus differentiator
------------------------  ----------------------------  ----------------------------------
DMLR Nanites              Laser heal side card          Full support primary + 3 polarities
Swarm Cross Pollination   Pellet ally heal              Lock tether + Grace meter
Bruiser Hard-Light        Shield / prison / immunity    Beam triage, not projector tank
Scrapper Grapple          Mobility node + optional HoT  Combat tether, not traversal
Friend in a Box           Smart deployable unit         Player-aimed beam, not AI friend
Honey Jar                 Territory bees + nectar       Instant beam triage, not jar field
Helminth Receiver         HP/Vitality bond rifle        Team polarities, not Host feed
Hard-Light Constructor    Jam / paint / bowl            Support buffs/heals/debuffs
Heaven Piercer            Draw bow pin/bleed/rain       Hold-beam medic, not projectile skill
```

Not trying to be: DMLR laser DPS, Bruiser shield tank, Friend deployable, Honey Jar field HoT, pure AFK healbot, Mercy GA flyer, or TF2 Medi Gun with no Judgment teeth.

Synergies: You Mend/Overclock the carry; Judgment paints bosses for the squad; revive saves wipes; secondary weapon covers personal DPS gaps by design.

---

## 3. Design Pillars

1. **Tether skill is baseline** — lock, hold, break, re-acquire, polarity choice. Not brain-off aura.
2. **Three polarities are baseline** — Mend / Overclock / Judgment all work weak with zero upgrades; paths deepen, they do not unlock the verbs.
3. **No self-Mend** — doctrine. Solo lives on Judgment + weaker self-Overclock + secondary weapon.
4. **Grace is always real** — meter builds and Discharge exists on stock; stock Discharge is weak on purpose.
5. **On-tether identity > flat % damage stickers** — heal rates, amp windows, marks, revive, multiplex.
6. **Three peer paths** (Triage / Overclock / Judgment); hybrids intended; no anti-synergy matrix.
7. **True downed revive is Triage crown fantasy** — co-op defining, budgeted, not free baseline.
8. **No sidearm on this gear** — player already has a secondary weapon slot.
9. **No Guardian Angel mobility** — characters already own movement kits.
10. **~30 upgrades for v1 ship**; exotic shapes larger than others; each exotic same cell count.
11. **R stays reload/vent** — never polarity toggle. RMB owns polarity cycle.
12. **Tone: SAXON medical-industrial + angelic/religious irony** — clinical hardware, hymn names, hazard stripes, not soft fantasy cleric.

---

## 4. Core Mechanics & Gunfeel

### 4.1 Base gun

| Trait        | Draft / intent                                                    |
|--------------|-------------------------------------------------------------------|
| Fire mode    | Hold-to-tether beam (lock-on assist); not projectile hose         |
| Damage       | Judgment chip modest; Mend/Overclock deal no/enemy-only damage    |
| Range        | Mid tether lock; hard max range with soft falloff near edge       |
| Mag/reserve  | “Emitter cell” mag that drains while beaming OR discrete vent heat — prefer **heat/vent** or slow cell drain so reload is a real beat without feeling like an SMG |
| Projectile   | Continuous tether VFX (ally gold/cyan vs enemy brass/crimson)     |
| ADS / RMB    | No ADS requirement; **RMB = polarity cycle**                      |
| Model/audio  | Staff/projector chassis, caduceus rail, clinical hum, hymn stingers |

Draft resource model (VALIDATE IN PLAYTEST) — pick one at implement, design assumes **Emitter Heat**:

| Model            | Intent |
|------------------|--------|
| **Emitter Heat** (preferred) | Beaming builds heat; overheat forces vent (R); Grace still separate |
| Cell mag         | Mag drains while tethered; reload swaps cell; simpler but more “gun” |

Heat band draft: ~8–12s continuous beam to overheat without upgrades; vent ~1.2–1.6s.

### 4.2 Inputs

| Input        | Baseline role                                      | Upgraded claims                                      |
|--------------|----------------------------------------------------|------------------------------------------------------|
| Hold M1      | Acquire + maintain Caduceus tether                 | Multiplex second tether; path tick payoffs           |
| Tap RMB      | Cycle polarity: Mend → Overclock → Judgment → Mend | Unchanged cycle order (no path reorders without epic)|
| R            | Vent heat / reload cell only                       | Reload only (no baseline hold-R power)               |
| Grace full + use | **Discharge** (weak generic)                    | Path crowns rewrite Discharge effect                 |
| Heavy        | Normal heavy equip                                 | No baseline heavy link                               |
| Secondary    | Player’s other weapon                              | Personal defense/DPS — staff never grows a pistol    |

**Discharge input (draft):** when Grace is full, **tap reload while not overheated** OR dedicated “use Grace” on interact-adjacent binding if reload conflicts — prefer **tap R when Grace full and not holding vent need**, else **hold R** only if heat is empty. Final: implementer picks clearest of:

1. Grace full + **M3 / ability-style** if gear can claim a clean button  
2. Grace full + **double-tap R**  
3. Grace full + **RMB hold** (conflicts with cycle — avoid)  

**Locked preference for doc:** Grace full shows prompt; **tap a dedicated Discharge bind** if available, else **tap R when Grace ≥ 100% and heat below vent-threshold**. Document in HUD. Playtest may swap.

### 4.3 Polarity definitions (baseline)

#### Mend (Heal) — ally only
- Tether must target an **ally player** (not self — **LOCKED**)
- Applies continuous HPS to tethered ally
- Builds Grace at **slow** rate
- No damage
- Invalid on enemies (soft deny + retarget hint); invalid on self (doctrine ping)

Draft Mend HPS: ~12–18 HP/s (VALIDATE — must matter in co-op, not delete all risk)

#### Overclock (Buff) — ally preferred; self allowed weak
- Ally tether: outgoing damage amp (draft +12–18% while tethered + 0.5s linger after break)
- **Self tether allowed** at reduced amp (draft ~50–60% of ally value) — solo drip of agency without Mend
- Builds Grace at **medium** rate
- No meaningful direct damage

#### Judgment (Debuff) — enemy only (solo spine)
- Tether enemy brain/part (prefer brain-level readability)
- Light chip DPS (draft modest — below mid primary hose)
- Applies **Condemned** stacks slowly (soft damage-taken amp and/or ally-hit marker)
- Builds Grace at **medium-fast** rate (reward active judgment)
- Invalid on allies

### 4.4 Tether rules (sacred)

1. **Lock-on beam** — aim near valid target in cone/radius; snap lock; hold M1 maintains  
2. **Single tether** baseline  
3. **Linger** ~0.9–1.3s after LoS break or aim leave (Mercy DNA) — effects tick at reduced strength during linger  
4. **Max range** hard cut after linger expires out of range  
5. **Retarget** — release and re-hold, or flick aim to higher-priority target with short re-lock grace  
6. **Priority draft:** low-HP allies in Mend; player-aim in Overclock; elites/boss parts in Judgment  
7. **Multiplayer authority:** owner gun validates tether target; heal/buff/debuff apply via existing damage/heal hooks  

### 4.5 Grace meter (sacred)

| Param              | Draft        | Intent |
|--------------------|--------------|--------|
| Max Grace          | 100          | Full bar |
| Mend build/s       | 4–6          | Slow — pure heal doesn’t AFK Über forever |
| Overclock build/s  | 7–9          | Medium |
| Judgment build/s   | 8–11         | Active play rewarded |
| Full time (pure)   | ~12–22s      | Depends on polarity mix |
| Baseline Discharge | Weak pulse   | See §4.6 |
| Path Discharge     | Crown rewrite| Replaces weak effect when crown equipped |

Grace does **not** pause beam. Discharge spends bar to 0 (or to residual crumb if epic says otherwise).

### 4.6 Baseline Grace Discharge (weak on purpose)

When Grace hits 100% and player Discharges:

- **Small team pulse** centered on owner OR on current tether target (prefer **tether target if valid, else self**):
  - Allies in small radius: tiny heal crumb (draft 8–15 HP once)
  - Owner: tiny short Overclock crumb (+5–8% damage 2s)
  - Enemies in tiny radius: 1 Condemned stack
- VFX: polite hymn chime — readable but not apocalyptic  
- This exists so the meter always teaches the verb. Crowns make it a **miracle**.

### 4.7 What baseline does NOT include

- No self-Mend  
- No multi-tether  
- No true revive  
- No invuln Über  
- No Kritz-class damage window  
- No GA dash / tether pull mobility  
- No sidearm / pistol mode  
- No deployable zone  
- No permanent aura without tether  

Those are path-, exotic-, or never-owned.

### 4.8 Baseline combat loop (zero upgrades)

```
RMB × N → choose polarity
Hold M1 → lock ally (Mend/Overclock) or enemy (Judgment)
   → Mend keeps a friend up; Overclock juices them (or weak self); Judgment chips + stacks
   → Grace climbs; heat climbs
   → break / linger / retarget; R vents when hot
   → Grace full → Discharge (polite pulse)
   → swap to secondary when you need real personal delete
```

Skill without upgrades: polarity discipline, who to lock, when to Judgment for Grace, heat management, positioning so linger saves a break, not face-tanking because “I’m the medic.”

---

## 5. Shared Framework Vocabulary

Upgrades speak these verbs. Baseline owns Tether / Polarity / Grace / weak Discharge / Heat.

### 5.1 Tether
- Active Caduceus lock on one target (baseline)
- Multiplex exotic adds a second Mend-class link

### 5.2 Polarity
- Mend / Overclock / Judgment cycle via RMB
- Some cards buff one polarity harder; none delete the cycle without an exotic that explicitly “locks preferred polarity” (backlog only)

### 5.3 Grace
- 0–100 meter from tether time (polarity-weighted)
- Discharge spends Grace

### 5.4 Discharge
- Baseline weak pulse
- Crowns **replace** the baseline effect (not always stack on top — prefer replace + optional additive crumbs from epics)

### 5.5 Linger
- Soft maintain after break; upgrade-owned duration/strength

### 5.6 Triage
- Mend payoffs: HPS, burst heal, cleanse-lite, assist refunds, revive

### 5.7 Overclock (verb)
- Ally (and weak self) offensive buffs: damage, RoF crumb, reload help, move match, overshield crumb

### 5.8 Judgment / Condemned
- Enemy debuff stacks: damage taken, slow shred, ally-hit marker, execute windows
- Counter separate from EffectType unless a card injects a real status

### 5.9 Last Rites
- Downed / broken-player interaction space; true revive is crown-owned

### 5.10 Mend Bank (upgrade-owned)
- Stored heal resource filled by Siphon / assists; dump into allies via Mend or Discharge riders

### 5.11 Heat / Vent
- Emitter endurance; R clears; cards may extend beam time or vent on Discharge

---

## 6. Upgrade Paths (gravity wells — hybrids intended)

### Path A — TRIAGE (Heal Staff)
“Keep them standing.”

- Spine: +Mend HPS, Grace-on-heal assists, burst Mend on Discharge (pre-crown), cleanse-lite, heat relief while Mending, Last Rites prep, true revive crown, multiplex second heal tether
- Clear vs ST: N/A combat role — enabler; personal clear stays Judgment/secondary
- Hybrid hooks: Siphon Gospel fills Mend Bank; Multiplex + Kritz is choir mode

### Path B — OVERCLOCK (Buff Staff)
“Make them gods for three seconds.”

- Spine: damage amp strength/duration, RoF/reload crumbs, Quick-Fix-style move match while tethered, overshield crumbs, Guardian DR share, Kritz Protocol Discharge rewrite
- Clear vs ST: you don’t clear — your carry does
- Hybrid hooks: Condemnation paint + Overclock carry = boss delete windows

### Path C — JUDGMENT (Debuff Staff)
“Mercy for them is a bullet with their name on it.”

- Spine: Condemned apply rate/cap, damage-taken amp, chip DPS, slow/shred, mark for allies, Siphon → Mend Bank, execute hymns, solo agency
- Clear vs ST: Judgment chip + mark clear assist; ST via boss Condemned windows
- Hybrid hooks: Siphon feeds Triage; Condemned amps Overclocked allies’ damage

### Path × verb matrix

```
                  TRIAGE                 OVERCLOCK                JUDGMENT
Tether            Mend focus             Buff focus               Enemy lock spine
Grace build       Slow honest            Medium                   Medium-fast
Discharge         Revive / heal miracle  Kritz damage gospel      Condemnation detonate / siphon dump
Self target       Forbidden (Mend)       Weak OK                  N/A
Co-op fantasy     Peak                   Peak                     High (mark for team)
Solo fantasy      Weak                   Medium (self OC)         Strongest path
```

---

## 7. Crowns & Sacred Cows

### Last Rites — Exotic (Triage crown)
- **Grace Discharge rewrite** when a **downed / broken ally** is in range or is the tether target:
  - Performs a **true revive / instant recover** of that ally (Mycopunk downed-player rules — implement to the real “broken player / pickup” API)
  - If no downed ally: falls back to a **large Mend bomb** on tethered ally (or nearest injured ally) so the crown is never dead in solo
- Cooldown / Grace cost: full Grace bar; additional **per-ally revive internal CD** (draft 45–75s) to prevent infinite wipe undo
- Boss missions / solo: Mend bomb path is the solo value
- Readable angelic brass + clinical defibrillator snap

### Caduceus Multiplex — Exotic (Triage crown)
- While Mending, a **second Mend tether** auto-locks a nearby injured ally at reduced HPS (draft 40–55% of primary)
- Second tether does **not** double full Grace rate (draft +30–40% Grace vs single Mend, not 2×)
- Overclock/Judgment remain single-target unless future backlog says otherwise
- Valkyrie-lite without flight

### Kritz Protocol — Exotic (Overclock crown)
- **Grace Discharge rewrite:** tethered ally (and short radius allies) gain a **large outgoing damage amp window** (draft +35–50% for 4–6s)
- Self included at partial rate if self-Overclocking or if no ally tether
- During window, optional minor RoF crumb on beneficiaries
- Not full invuln; pure TF2 Kritzkrieg DNA, SAXON-named
- Replaces baseline weak Discharge when equipped

### Guardian Link — Exotic (Overclock crown)
- While Overclock-tethered to an ally:
  - Ally gains DR crumb (draft 10–18%)
  - Owner gains smaller DR crumb (draft 6–12%)
  - Optional: small fraction of damage ally takes is redirected to owner as **soft** damage (cap per second) — **default OFF for v1**; enable only if playtest wants TF2 “body block” fantasy. Doc default: **shared DR only, no redirect**
- Does not outshine Bruiser true shields

### Condemnation — Exotic (Judgment crown)
- Judgment tether applies **Condemned** stacks faster; raises cap
- At max Condemned: ally hits against that target gain bonus damage and/or small cleave crumb
- **Grace Discharge rewrite (Judgment-flavored):** detonate Condemned on tethered (or aimed) target for burst ∝ stacks + brief mega damage-taken window
- If Kritz also equipped: Discharge priority table decides (see §12.7) — hybrid allowed via priority, not ban

### Siphon Gospel — Exotic (Judgment crown)
- While Judgment-tethering, a portion of chip damage and/or uptime fills **Mend Bank**
- Mend Bank spends automatically as bonus HPS when you Mend allies, or dumps a chunk on Discharge if no heal crown claimed Discharge
- Creates the drain-tank → squad-feed hybrid
- Solo: Bank can drip weak overshield/self-heal crumb **only as tiny Mend Bank overflow** (not self-Mend tether) — draft optional; prefer Bank does nothing selfish except tiny owner heal crumb on Discharge if no allies present

Sacred cows (do not cut without rewriting identity):
- Baseline three polarities
- No self-Mend
- RMB polarity cycle
- Grace + weak baseline Discharge
- True revive on Last Rites
- No sidearm, no GA mobility
- Single tether until Multiplex
- Co-op first, Judgment solo spine
- SAXON medical-industrial + hymn irony tone

---

## 8. Full Upgrade List (~30 ship + backlog)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity  
Cell rule: Exotic shapes larger than others; all Exotics same cell count.  
Player-facing names below. API names assigned at implementation.

------------------------------------------------------------------------------
PATH A — TRIAGE
------------------------------------------------------------------------------

A-EX1. Last Rites — Exotic (crown)
       Grace Discharge revives downed ally in range/tether; else large Mend bomb.
       Per-ally revive ICD.

A-EX2. Caduceus Multiplex — Exotic (crown)
       Second reduced Mend tether to nearby injured ally; Grace rate partial.

A-EP1. Trauma Latch — Epic
       +Mend HPS. Mend linger stronger (heal continues harder during linger).

A-EP2. Nanite Communion — Epic
       Healing an ally with Mend restores a small amount of Grace (Assist Charge DNA, original name).
       Diminishing so it cannot fully replace Judgment Grace farming.

A-EP3. Soft Landing Protocol — Epic
       Mend on an ally below 30% HP gains bonus HPS and a one-shot triage burst every N seconds per target.

A-EP4. Absolution Overflow — Epic
       Baseline Discharge (and heal-flavored Discharge) heals more and applies a short regen HoT (nectar-adjacent, pure HoT, no overhealth unless card says).

A-RA1. Field Sutures — Rare
       +Mend HPS (modest).

A-RA2. Cleanse Pulse — Rare
       Every N seconds while Mending, remove one mild debuff/status amount from ally (cleanse-lite; respect what APIs allow).

A-RA3. Triage Priority — Rare
       Auto-prefer lowest HP ally in lock cone when acquiring Mend tether.

A-RA4. Emitter Mercy — Rare
       While Mending, heat gain reduced.

A-ST1. First Aid Latch — Standard
       Minor +Mend HPS.

------------------------------------------------------------------------------
PATH B — OVERCLOCK
------------------------------------------------------------------------------

B-EX1. Kritz Protocol — Exotic (crown)
       Grace Discharge → large team/ally damage amp window.

B-EX2. Guardian Link — Exotic (crown)
       While Overclock-tethered: shared DR crumbs (ally > owner).

B-EP1. Damage Gospel — Epic
       +Overclock damage amp strength; +buff linger after break.

B-EP2. Adrenal Tether — Epic
       Overclock also grants modest fire-rate crumb to ally (and weaker to self).

B-EP3. Quick-Fix Gait — Epic
       While Overclock-tethered, ally move speed matches yours if you are faster (clamp); small shared speed crumb otherwise (Quick-Fix DNA).

B-EP4. Reload Benediction — Epic
       Overclock-tethered ally reloads faster; killing blows by that ally refund tiny Grace.

B-RA1. Sermon Amplifier — Rare
       +Overclock amp percent.

B-RA2. Choir Discipline — Rare
       +Overclock buff duration / linger.

B-RA3. Scaffold Blessing — Rare
       Overclock grants a small overshield crumb on tether acquire (ICD per ally).

B-RA4. Self-Flagellation Tax — Rare
       Self-Overclock strength increased (still below full ally amp). Solo quality card.

B-ST1. Minor Benediction — Standard
       Minor +Overclock amp.

------------------------------------------------------------------------------
PATH C — JUDGMENT
------------------------------------------------------------------------------

C-EX1. Condemnation — Exotic (crown)
       Faster/higher Condemned; max-stack ally payoff; Discharge detonates mark window.

C-EX2. Siphon Gospel — Exotic (crown)
       Judgment fills Mend Bank; Mend spends Bank as bonus heal.

C-EP1. Open Vein Mark — Epic
       +Condemned apply rate; Judgment chip damage up.

C-EP2. Armor Confession — Epic
       Condemned targets take bonus damage to shells/armor-like parts (anatomy-aware if cheap; else generic DR shred).

C-EP3. Withering Gaze — Epic
       Judgment applies soft slow (move mult); bosses reduced.

C-EP4. Execute Hymn — Epic
       Bonus Judgment damage and bonus Grace gain vs targets below 25% HP.

C-RA1. Brass Verdict — Rare
       +Judgment chip DPS.

C-RA2. Stigma — Rare
       +Max Condemned stacks; +damage-taken per stack slightly.

C-RA3. Heretic’s Beacon — Rare
       Condemned targets are more visible to allies (mark VFX / mild highlight); ally hits generate tiny Grace crumbs (cap).

C-RA4. Heat Inquisition — Rare
       Judgment builds less heat (active play comfort).

C-ST1. Minor Writ — Standard
       Minor +Condemned apply.

------------------------------------------------------------------------------
GENERIC / GUNFEEL
------------------------------------------------------------------------------

G-RA1. Extended Emitter — Rare
       +Tether max range.

G-RA2. Grace Capacitor — Rare
       +Grace gain from all polarities (modest global).

G-RA3. Beam Focus — Rare
       +Lock aim assist / tighter reacquire; −spread of lock cone junk.

G-RA4. Thermal Choir — Rare
       +Heat capacity; +vent speed slightly.

G-ST1. Lingering Hymn — Standard
       +Linger duration.

G-ST2. Stable Latch — Standard
       Slightly harder to break lock from minor aim jitter.

G-ST3. Field Vent — Standard
       +Vent/reload speed.

G-ST4. Processional Pace — Standard
       Minor +move speed while beaming (not GA; small).

G-OD1. Boundary Incursion — Oddity
       Increases upgrade grid size.

------------------------------------------------------------------------------
FROZEN 30 FOR V1 SHIP
------------------------------------------------------------------------------

EXOTIC (6)
  1  Last Rites
  2  Caduceus Multiplex
  3  Kritz Protocol
  4  Guardian Link
  5  Condemnation
  6  Siphon Gospel

EPIC (8)
  7  Trauma Latch
  8  Nanite Communion
  9  Soft Landing Protocol
 10  Damage Gospel
 11  Adrenal Tether
 12  Quick-Fix Gait
 13  Open Vein Mark
 14  Execute Hymn

RARE (10)
 15  Field Sutures
 16  Cleanse Pulse
 17  Emitter Mercy
 18  Sermon Amplifier
 19  Choir Discipline
 20  Self-Flagellation Tax
 21  Brass Verdict
 22  Stigma
 23  Extended Emitter
 24  Grace Capacitor

STANDARD (5)
 25  First Aid Latch
 26  Minor Benediction
 27  Minor Writ
 28  Lingering Hymn
 29  Field Vent

ODDITY (1)
 30  Boundary Incursion

------------------------------------------------------------------------------
BACKLOG (designed, not in first 30)
------------------------------------------------------------------------------

Triage
- Absolution Overflow
- Triage Priority
- Mass Triage Discharge (multi-ally heal bomb without revive)
- Status cleanse expansion
- Mend Bank capacity cards (pairs Siphon)

Overclock
- Reload Benediction
- Scaffold Blessing
- Damage redirect Guardian variant (explicit opt-in)
- RoF gospel stacking rules
- Full team aura Discharge without tether (probably never — fights tether identity)

Judgment
- Armor Confession
- Withering Gaze
- Heretic’s Beacon
- Heat Inquisition
- Contagion Thread (Condemned spreads on kill)
- Boss-only writ cards

Generic
- Beam Focus, Thermal Choir, Stable Latch, Processional Pace
- Polarity lock exotic (hold preferred mode) — backlog
- Discharge on overheat failsafe
- FriendinaBox / Breeding Season soft synergy notes only

Never (locked non-goals)
- Self-Mend tether
- Sidearm mode
- Guardian Angel dash
- Baseline multi-beam
- Baseline true revive

---

## 9. Example Builds

Choir medic (co-op Triage)
  Last Rites + Caduceus Multiplex + Trauma Latch + Nanite Communion
  + Soft Landing Protocol + Field Sutures + Emitter Mercy + Lingering Hymn
  Double Mend, assist Grace, revive when it matters.

Kritz deacon (Overclock)
  Kritz Protocol + Guardian Link + Damage Gospel + Adrenal Tether
  + Quick-Fix Gait + Sermon Amplifier + Self-Flagellation Tax
  Glue to the carry; Discharge for delete windows; self-OC for solo scraps.

Inquisitor (Judgment solo/co-op)
  Condemnation + Siphon Gospel + Open Vein Mark + Execute Hymn
  + Brass Verdict + Stigma + Grace Capacitor + Extended Emitter
  Paint bosses, bank heals for allies, detonate Condemned on Discharge.

Hybrid freak (showcase)
  Last Rites + Kritz Protocol + Condemnation
  + Nanite Communion + Damage Gospel + Open Vein Mark
  Revive, gospel damage, mark detonate — no artificial brakes; Discharge priority table matters.

Siphon nurse
  Siphon Gospel + Caduceus Multiplex + Trauma Latch + Soft Landing
  + Open Vein Mark + Field Sutures
  Judgment farms Bank; Mend dumps it into two allies.

---

## 10. Economy & Tuning Rules of Thumb

- Power budget lives in **tether uptime quality**, polarity choice, and Grace windows — not staff DPS.
- Baseline Mend must save allies in co-op without erasing all danger; start conservative HPS.
- Grace full time ~12–22s mixed play; Nanite Communion must not make pure Mend the fastest Grace farm by a huge margin (Judgment should stay competitive).
- Baseline Discharge stays weak enough that crowns feel like identity flips.
- Last Rites revive ICD is sacred — fun miracle, not wipe undo on cooldown.
- Kritz window must not exceed “entire mission is amp” — duration short, Grace gated.
- Guardian Link DR < Bruiser shield fantasy.
- Condemned damage-taken stacks need diminishing or caps so hybrid delete doesn’t map-wipe.
- Heat exists so infinite face-tank beam has a vent beat; Emitter Mercy / Heat Inquisition are comfort, not delete heat.
- Self-Overclock always weaker than ally Overclock.
- No self-Mend ever — if solo sustain is miserable, tune Judgment chip, self-OC, and Discharge crumbs — not Mend-self.
- Secondary weapon is part of the loadout fantasy; do not buff staff personal DPS until secondary feels pointless.

---

## 11. Status & Counter Split (explicit)

| Status / counter | Role on this gun                         | Baseline? |
|------------------|------------------------------------------|-----------|
| Mend HPS         | Ally heal ticks                          | Yes       |
| Overclock buff   | Damage (and card) amp while tethered     | Yes       |
| Condemned stacks | Enemy debuff counter (not EffectType)    | Yes       |
| Grace            | Meter 0–100                              | Yes       |
| Mend Bank        | Stored heal (Siphon)                     | Exotic    |
| Heat             | Emitter endurance                        | Yes       |
| Fire/Shock/Acid  | Not identity                             | No        |
| Bees/Poison/Bleed| Not identity                             | No        |
| Cryo             | Optional slow via Withering Gaze backlog | Backlog   |
| Shatter/etc.     | Not identity                             | No        |

### 11.1 Condemned counter (draft)

| Param                 | Draft     | Intent |
|-----------------------|-----------|--------|
| Apply while Judgment  | +1 / 0.35–0.5s | Baseline slow cook |
| Max stacks            | 5         | Stigma / Condemnation raise |
| Per stack             | +3–5% damage taken | Soft |
| Duration              | 3–5s refresh | |
| Max stack payoff      | Crown     | Ally bonus / Discharge detonate |
| Bosses                | Full stacks OK; slow cards reduced | |

Prefer custom counter on behaviour over new EffectType unless systemic interactions demand it.

### 11.2 Heal application

Use the same heal pathways as vanilla ally heal upgrades (Engineering Nanites, Cross Pollination, Stolen Nanites patterns) — confirm `Player` / damageable heal API in decompile at implement. Do not invent godmode regen.

---

## 12. Implementation Notes

### 12.1 Gear registration
- Follow weapon template in this repo: clone base gun, GearInfo high-range id, APIName `ms7_caduceus` (or `caduceus`), behaviour component, SpawnGear stamp, CreateUpgrade pool.
- Prefer a beam-capable or continuous-fire gun in AllGear if one exists; else any Gun + custom tether tick (DMLR laser hold DNA is a research anchor, but Caduceus is not a DMLR clone).
- Plugin: GUID `sparroh.mercystaff` or `sparroh.ms7caduceus`, MycoMod **IsSandbox**.
- Persistence: stable gear id; register before PlayerData.OnAwake AddGear.
- Display name: **MS-7 Caduceus**

### 12.2 Behaviour host
MercyStaffBehaviour / CaduceusBehaviour:
- WeaponData: polarity, mendHps, overclockAmp, selfOverclockMult, judgmentDps, condemned rules, grace rates, linger, range, heat, multiplex, discharge mode flags, revive rules, mend bank
- Runtime: current target id, linger timer, grace, heat, condemned map, mend bank, revive ICDs, second tether target
- Prefab snapshot restore on upgrade Remove

### 12.3 Tether loop
Each tick while M1 held (owner):
1. If no lock: sphere/cone query for valid targets by polarity
2. Validate ally vs enemy vs self rules (reject self on Mend)
3. Maintain beam VFX start→target
4. Apply polarity effect
5. Add Grace, add Heat
6. On M1 release or invalid: enter Linger

Reference DNA:
- DMLR laser hold / aim laser
- Mercy beam linger (design)
- Cross Pollination ally targeting
- Friend mark target tracking (FriendinaBox)

### 12.4 Heal / buff / debuff hooks

| Effect     | Approach |
|------------|----------|
| Mend       | Heal API on ally Player (match Nanites) |
| Overclock  | Temporary damage mult on ally gear/player — prefer existing boost/stat buff patterns; else Harmony on damage dealt by tethered player |
| Judgment   | DamageTarget chip + Condemned component on brain |
| Revive     | Hook broken/downed player recover path (audit Assembly at impl — Energy Injection “pick up broken player” is a lead) |
| DR share   | Temporary DR flags on players |

### 12.5 Grace Discharge
- On Discharge input: if grace < 100 return
- grace = 0
- Switch on equipped crown flags (priority §12.7)
- Else baseline weak pulse

### 12.6 Hooks

| Hook / system     | Use |
|-------------------|-----|
| Update / weapon tick | Tether maintain, heat, grace, linger |
| RMB press         | Polarity cycle |
| R press           | Vent; maybe Discharge if policy says |
| OnDamageTarget    | Judgment chip confirm; Heretic Grace crumbs |
| Ally damage dealt | Overclock mult application point |
| Downed/broken API | Last Rites |
| HUD               | Polarity glyph, Grace bar, Heat, tether target pip, revive prompt |

### 12.7 Discharge priority (when multiple crowns)

1. **Last Rites revive** — if valid downed ally in range/tether  
2. **Last Rites Mend bomb** — if Last Rites equipped and no downed target  
3. **Kritz Protocol** — if equipped and no revive consumed this press  
4. **Condemnation detonate** — if equipped and valid Condemned target  
5. **Siphon Bank dump** — if Bank > 0 and heal dump appropriate  
6. **Baseline weak pulse**

If Last Rites + Kritz both equipped: revive consumes the Discharge when a downed ally exists; otherwise Kritz wins over Mend bomb if both would claim — **prefer: downed → Last Rites; else if Kritz → Kritz; else Last Rites bomb; else Condemnation; else baseline**.

### 12.8 RMB / polarity
Simple enum cycle; HUD icon color:
- Mend: clinical cyan/gold
- Overclock: bright gold/white
- Judgment: brass/crimson

### 12.9 HUD
- Polarity icon + name crumb  
- Grace meter (charge bar DNA / SparrohUILib)  
- Heat meter or integrated emitter bar  
- Tether target name/HP pip  
- Last Rites ready glyph when downed ally exists and Grace full  
- Prefer SparrohUILib if dependency acceptable; else minimal vanilla bars  

### 12.10 Multiplayer
- Sandbox mod; all clients need the same plugin  
- Tether owner-authoritative  
- Heal/buff/revive must respect game authority (server/host rules)  
- Buffs on remote allies: replicate state or apply via RPCs matching vanilla assist heals  
- Revive: must use official recover path to avoid desync  

### 12.11 VFX / audio priority
1. Tether beam (polarity recolors)  
2. Lock acquire tick (soft hymn)  
3. Mend tick shimmer / Overclock harmonic / Judgment low brass grind  
4. Linger fade  
5. Grace full angelic ding (clinical, not cartoon)  
6. Baseline Discharge polite chime  
7. Kritz gospel swell  
8. Last Rites defibrillator snap + choir hit  
9. Condemnation detonate writ crack  
10. Heat warning hiss / vent exhale  

### 12.12 Research anchors (wiki + mods)
- Mercy Caduceus staff: lock beam, heal/boost toggle, linger  
- TF2: Medi Gun / Kritzkrieg / Quick-Fix / Vaccinator fantasies → Kritz, Gait, DR share  
- Wiki: Engineering Nanites, Assist Charge, Cross Pollination, Stolen Nanites, Repairing Exhaust, Honey Sweet, Charge Rerouting, With Me / With You, One For All, Energy Injection, Hard-Light Prison heal  
- Sibling docs: DMLR (thin support non-goal for them; full path for us), FriendinaBox (mark), Honey Jar (HoT rules), Helminth (bond language contrast), Heaven Piercer / Hard-Light (bible structure, RMB priority)  

---

## 13. Deliberate Non-Goals

- Not self-Mend (LOCKED)  
- Not sidearm / Mercy pistol on this gear (LOCKED)  
- Not Guardian Angel mobility (LOCKED)  
- Not replacing Bruiser as shield tank  
- Not FriendinaBox deployable AI  
- Not DMLR anatomy laser identity  
- Not AFK aura heal without tether  
- Not baseline true revive  
- Not baseline multi-tether  
- Not invuln stock Über (Kritz is damage; Last Rites is revive; no stock invuln crown required in v1)  
- Not requiring custom Unity prefab for v1 (runtime clone OK)  
- Not team-hostile heals  
- Not shipping full Vaccinator multi-resist bubble kit in first 30 (Guardian Link is enough DR fantasy)

---

## 14. Open Tuning Questions (playtest, not design blockers)

1. Mend HPS 12 vs 18 vs mission damage rates.  
2. Tether range 20 vs 35.  
3. Linger 0.9 vs 1.3s.  
4. Grace full time and polarity weights.  
5. Heat 8s vs 12s continuous.  
6. Judgment chip DPS vs “staff feels useless solo.”  
7. Self-Overclock mult 50% vs 65%.  
8. Kritz amp % and duration.  
9. Last Rites revive ICD 45 vs 75s.  
10. Condemned max stacks and per-stack taken amp.  
11. Discharge input binding that never fights Vent.  
12. Exact downed/broken player API after assembly audit.  
13. Whether heal should work on FriendinaBox deployables (nice-to-have).  
14. Multiplex second-tether range and HP prioritization.  
15. Heat vs cell-mag final resource model.

---

## 15. Success Criteria / Player Fantasy Checklist

- [ ] Hold M1 locks a tether with zero upgrades; beam is readable  
- [ ] RMB cycles Mend → Overclock → Judgment with clear HUD/VFX  
- [ ] Mend heals allies only; self-Mend denied with readable feedback  
- [ ] Overclock buffs allies; self-Overclock works weaker  
- [ ] Judgment chips enemies and applies Condemned  
- [ ] Linger keeps soft effect after brief LoS break  
- [ ] Grace builds; baseline Discharge is weak but real  
- [ ] Last Rites revives a downed ally on Discharge when valid  
- [ ] Last Rites still does something useful with no downed ally  
- [ ] Caduceus Multiplex heals two allies at once  
- [ ] Kritz Protocol Discharge makes a carry delete a pack/boss window  
- [ ] Guardian Link DR is noticeable but below Bruiser  
- [ ] Condemnation max stacks matter for allies  
- [ ] Siphon Gospel Judgment → Mend Bank → ally heal loop works  
- [ ] Hybrid crowns respect Discharge priority without soft-lock  
- [ ] No sidearm mode; secondary weapon remains the panic button  
- [ ] No dash-to-ally mobility on the gun  
- [ ] SAXON clinical + hymn irony reads in names/VFX/audio  
- [ ] Co-op feels like the intended peak fantasy  

---

## 16. Strengths, Weaknesses & Co-op

Strengths
- Unique dedicated support primary niche  
- Three polarities create real in-fight decisions  
- Grace crowns deliver highlight moments (rez, kritz, detonate)  
- Hybrids (siphon nurse, hybrid freak) are expressive  
- Co-op identity is crystal clear  

Weaknesses
- Solo heal fantasy intentionally absent  
- Personal DPS low without Judgment investment + secondary  
- Tether breaks under chaos if player aims poorly  
- Heat/vent punishes panic face-tank beaming  
- Revive ICD means you cannot fix every wipe  

Co-op
- You are the reason the loud primary stays loud  
- Call polarity: Mend the dying, Overclock the carry, Judgment the boss  
- Avoid grief: no team-damage heals, no forced slow on allies  
- Multiplex is a gift; don’t AFK behind walls forever (heat + Grace design)

---

## 17. Visual, Audio & Thematic Design

Appearance
- SAXON industrial **medic projector staff**: chrome/white clinical plates, hazard stripes, caduceus twin-helix rail along the barrel, battery “grace cell” magazine, fungal-etched Form 12-MED stickers, subtle halo ring at emitter (ironic angel, still robot hardware)
- Beams:
  - Mend: cyan-gold soft helix  
  - Overclock: bright gold-white hard helix  
  - Judgment: brass-crimson inverted helix  
- Grace full: emitter halo brightens  
- Last Rites: defibrillator paddles of light on ally  
- Kritz: gold scripture rings around beneficiaries  
- Condemned: writ marks / red seals on enemy 

Sound
- Idle: low clinical hum  
- Lock: soft chime  
- Mend: gentle pulse ticks  
- Overclock: rising harmonic  
- Judgment: grinding brass interval  
- Polarity cycle: mechanical detent + choir sample crumb  
- Grace full: clear angelic ding (short, not meme)  
- Vent: steam/heat exhale  
- Last Rites: snap + choir hit  
- Kritz: gospel swell  
- Condemnation: seal crack  

Flavor / codex line (in-game style)
  MS-7 Caduceus
  Hold-fire field tether. RMB cycles Mend, Overclock, and Judgment.
  Builds Grace for Discharge. Cannot Mend yourself. Doctrine, not defect.

---

## 18. Locked Review Decisions (2026-08-06)

| Decision              | Lock |
|-----------------------|------|
| Form factor           | SAXON field-medic tether staff / projector |
| Player-facing name    | **MS-7 Caduceus** |
| Working APIName       | `ms7_caduceus` |
| Slot                  | Primary |
| Paths                 | Triage / Overclock / Judgment |
| Polarities baseline   | All 3 weak |
| Polarity input        | RMB cycle Mend → Overclock → Judgment |
| Self-Mend             | **None** |
| Self-Overclock        | Yes, weaker than ally |
| Grace                 | Baseline meter + weak Discharge; crowns rewrite |
| Resurrect             | **True revive** on Last Rites crown |
| Sidearm               | **Never** (secondary weapon slot) |
| GA mobility           | **Never** (class mobility) |
| Solo priority         | Co-op first; Judgment solo spine |
| Tone                  | SAXON medical-industrial + angelic/religious irony |
| Ship pool             | Frozen 30 listed above |
| Crowns                | Last Rites, Caduceus Multiplex, Kritz Protocol, Guardian Link, Condemnation, Siphon Gospel |
| Doc depth             | Full bible |
| MycoMod flag          | IsSandbox at implementation |
| Doc file              | MercyStaff-DesignDoc.txt (this file) |
| Guardian damage share | Off by default (DR only) |
| Resource model draft  | Emitter Heat + Vent (cell mag fallback) |

---

## 19. Changelog

v1 (2026-08-06)
- Initial full design from locked user decisions
- Name locked: MS-7 Caduceus
- Paths: Triage (heal) / Overclock (buff) / Judgment (debuff)
- Research anchors:
  - Mercy staff + TF2 Medi Gun family fantasies
  - Wiki support scraps: Nanites, Assist Charge, Cross Pollination, Stolen Nanites, Repairing Exhaust, Honey Sweet, With Me/You, One For All, Energy Injection, Bruiser Prison heal
  - Sibling docs: DMLR, Heaven Piercer, Hard-Light Constructor, FriendinaBox, Honey Jar, Helminth Receiver
- User locks: industrial name; no self-Mend; true revive; 3 polarities baseline; RMB cycle; baseline Grace Discharge; co-op first; no sidearm; no GA; tone A+B; full bible

---

## 20. Implementation checklist (post-design)

- [ ] Rename plugin/csproj/thunderstore from template → MS-7 Caduceus / MercyStaff
- [ ] CaduceusBehaviour.Data fields from §12.2
- [ ] Tether acquire/maintain/linger/range
- [ ] Polarity cycle + HUD
- [ ] Mend heal API wiring
- [ ] Overclock damage mult on ally (+ weak self)
- [ ] Judgment chip + Condemned map
- [ ] Grace meter + baseline Discharge
- [ ] Heat + vent
- [ ] Last Rites revive API audit + implement
- [ ] Multiplex second Mend tether
- [ ] Kritz / Guardian / Condemnation / Siphon crowns
- [ ] Discharge priority table
- [ ] UpgradeRegistration frozen 30
- [ ] Persistence + SpawnGear stamp
- [ ] Playtest pass on §14 knobs
