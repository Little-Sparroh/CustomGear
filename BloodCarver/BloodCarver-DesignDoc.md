# Blood Carver – Design Document (v1)

## 1. High Concept / Fantasy

A close-range industrial chainsaw that harvests blood from enemy anatomy, spends it on command, and feeds your heavy weapon between swings.

You want to be in the pile. Limbs, shells, and cores mint Blood. RMB cashes it. The heavy slot is the butcher’s second hand.

**One-liner:** Harvest the body. Spend the red. Feed the cannon.

**Product shape:** New primary weapon (**Blood Carver**). Does **not** replace vanilla The Carver.


## 2. Role & Fantasy in the Arsenal

- **Slot:** Primary
- **Range:** Melee / extreme close (short saw reach; **no damage falloff** inside that reach)
- **Role:** High-risk melee clear + boss shred, blood resource engine, heavy-weapon battery
- **Gap filled:** Vanilla Carver is one-dimensional — blood is upgrade-gated sticker math, Rage is a second competing meter, heavy is one epic card, and brutal falloff punishes the exact range the fantasy demands
- **Synergies:** Any heavy weapon; movement abilities (threshold spends); co-op heal/chunk toys; status self-synergies while living in melee

**Not trying to be:** mid-range hose, pure tank with free DR, or a second heavy weapon.


## 3. Design Pillars

1. **Blood is baseline** — the gun always tracks Blood. Upgrades teach how you stack, hold, and spend; they do not unlock the meter.
2. **Stacking and consuming are both skillful** — holding high blood is a stance; RMB dumps are decisions; threshold autos are build choices.
3. **Heavy weapons are the third verb** — Carver harvests; heavy executes (ammo, next-shot empower, swap windows).
4. **Melee risk is paid in blood currency** — DR, chunks, pull, deflect scale with blood state or spends, not only “holding M1.”
5. **Three gravity-well paths that intermingle** — pure builds work; hybrids are intended.
6. **~30 upgrades** for v1 ship; exotic shapes larger than others; each exotic same cell count.
7. **Rage is cut** — one stack language only (Blood). Frenzy fantasy lives on spends / high-stack stances.
8. **Downtime must not erase the bank** — blood falls off **1 stack at a time**, never continuous full-bar melt.
9. **No damage falloff** — full damage across the entire legal saw volume; range is enforced by reach only.
10. **Full rename pass** — new player-facing names (vanilla echoes only in fate tables).


## 4. Core Mechanics & Gunfeel

### 4.1 Base gun

| Trait | Value / intent |
|---|---|
| Fire mode | Continuous saw (box-cast, Carver DNA) |
| Damage | Modest per tick, high tick rate |
| Range | Short max reach (~5–6m starting target) |
| **Falloff** | **None** — damage multiplier is 1.0 for all hits in reach |
| Mag / reserve | Keep a mag loop (reload beat matters); numbers tune in playtest |
| Movement | Standard; sprint-saw and speed toys are upgrade-owned |
| Model / audio | Borrow vanilla Carver chainsaw feel until custom art |

### 4.2 Why no falloff

Vanilla Carver’s 4–5m falloff to 0.4× actively fights the fantasy. Blood Carver’s limitation is **reach** (you must be close), not **soft damage tax for being close-but-not-perfect**. Box cast length / `maxDamageRange` still define the legal volume; `GetDamageMultiplier` is treated as flat 1.0 (or falloff distances set so the curve never bites).

### 4.3 Blood resource (baseline)

| Param | Draft | Intent |
|---|---|---|
| MaxBlood | 20 | Readable HUD stacks; path cards raise cap |
| **BloodOnDamage** | **1 per 10 damage instances** | **Baseline income while sawing** — not kill-gated |
| BloodOnLimbKill | 1 | Anatomy mint — weakest |
| BloodOnShellKill | 2 | Mid |
| BloodOnCoreKill | 3 | Strongest single mint |
| BloodOnFullKill (brain) | +0 or small bonus if not already counted via parts | Avoid double-dip abuse; prefer part events |
| Optional saw drip | 0 on baseline (upgrade-owned) | Keeps kill/part focus |
| Combat grace | 7s since last blood gain | “In combat” window |
| **Decay** | **−1 stack per interval** after grace | Default interval ~0.85s |
| Decay while stowed | Same or slightly slower (~1.1s) | Optional comfort; not required |
| Baseline passive | +1.0% damage per stack (soft) | Meter always means something |
| Baseline DR | None free | DR is upgrade / spend owned so tank isn’t default |

**Decay rules (locked)**
- Never continuous float drain of the whole bar.
- After grace expires with blood > 0: every DecayInterval, blood = max(0, blood − 1).
- Any blood gain refreshes grace and pauses decay.
- HUD shows integer stacks; decay ticks are readable (stack display updates per tick).

**Compare vanilla:** Blood only existed with Blood flag; continuous −1.5/s after 9s idle deleted banks during inevitable downtime. Rage was a second meter that full-dumped after 5s. Both patterns are rejected.

### 4.4 Inputs

| Input | Role |
|---|---|
| **M1** | Saw (hold) |
| **RMB** | **Active blood spend** (baseline Exsanguinate) |
| **R tap** | Reload |
| **R hold** | **Lockdown / Shackle** when that exotic is equipped; otherwise no hold-R behavior on baseline |
| Heavy key | Normal heavy equip; Path C empowers this slot |

### 4.5 Baseline RMB — Exsanguinate

**Requirements**
- CurrentBlood ≥ SpendMin (draft: 3)
- Not in spend recovery

**Effect (baseline)**

| Piece | Draft |
|---|---|
| Cost | min(CurrentBlood, 5) flat (or fixed 5 if blood ≥ 5) |
| Timing | Instant on press |
| Damage | Short forward cone / pulse, modest damage, no element on base |
| Buff | Brief saw window: +RoF or +area for ~1.0s |
| Recovery | ~0.35–0.5s before next RMB spend |
| Move | No root |

**Intent:** Always teaches consume. Weak enough that Path B crowns still matter.

### 4.6 RMB priority (when multiple spend mods stacked)

1. Path B crown full-dump (Bloodletting) — replaces baseline effect
2. Other exotic RMB overrides (if any — avoid shipping many)
3. Modified baseline Exsanguinate (cost/effect cards)
4. Else baseline Exsanguinate

**Threshold auto-spend** (Path B cards): can fire independently when blood crosses threshold; does not require RMB. If both would fire same frame, auto-spend resolves first, then RMB sees remaining blood.

### 4.7 Hold R — Lockdown (exotic-owned only)

When **Iron Snare** (Lockdown rewrite) is equipped:

| Piece | Behavior |
|---|---|
| Input | **Hold R** (press duration ≥ ~0.25s) — distinct from tap-R reload |
| Effect | Shackle / float nearby enemies (vanilla Lockdown DNA) |
| Recharge | Move-fast charge + optional blood/heavy hooks from supports |
| Conflict | Tap R still reloads if hold threshold not met; UI shows snare charge when exotic present |
| RMB | **Unchanged** — blood spend stays on RMB |

If Iron Snare is not equipped, hold R does nothing special (reload only on tap).

### 4.8 Base combat loop (no upgrades)

```
Hold M1 in the pile → each damage tick feeds the instance counter → Blood climbs
   ↘ part kills (limb/shell/core) mint bonus blood on top
   ↘ stacks grant mild damage; 1-at-a-time decay only after grace
   ↘ tap RMB to spend ~5 blood → pulse + short saw buff
   ↘ reload on R when mag empty
   ↘ (with Iron Snare) hold R to shackle, then re-enter
```

Skill without upgrades: stay in reach (no falloff crutch), keep the saw on target to mint blood even on tanks, time RMB between packs, don’t face-tank without DR cards, use anatomy priority (cores when safe).


## 5. Upgrade Paths (gravity wells — hybrids intended)

### Path A — HEMORRHAGE (Blood Stacking)
**“The redder I am, the meaner the saw.”**

- **Spine:** max blood, blood-on-part, slower decay interval, scale-with-stacks (dmg, RoF, area, DR, jump)
- **Crown:** **Crimson Throne** — at high/max blood, saw gains a stance (area + DR + pull); optional soft overcap
- **Native element lean:** none required; Fire coating pairs well as “hot blood”
- **Hybrid hooks:** high stacks make RMB dumps scarier; high stacks mint more Heavy Fuel per Path C cards

### Path B — EXSANGUINATE (Blood Consuming)
**“I bank red, then cash the room.”**

- **Spine:** RMB upgrades, partial spends, full dumps, threshold autos, spend → burst / lightning / frenzy / move charge / chunks
- **Crown:** **Bloodletting** — RMB spends **all** blood → forward butcher cone ∝ stacks spent
- **Support crown DNA:** **Slippery Arteries** (threshold auto → move ability charge + brief ghost DR)
- **Hybrid hooks:** Stack path builds bigger dumps; Heavy path turns dumps into Butcher Rounds

### Path C — BUTCHER’S FEAST (Heavy Weapons)
**“The saw feeds the cannon.”**

- **Spine:** anatomy kills and blood spends mint **Heavy Fuel** (ammo / next-heavy-shot multiplier / swap window)
- **Crown:** **Abattoir Link** — RMB spend (or threshold) loads a **Butcher Round** into the heavy (guaranteed empowered next heavy shot)
- **v1 scope (locked):** ammo + next-shot empower + faster swap windows — **no** custom heavy projectile type
- **Hybrid hooks:** Hemorrhage keeps Fuel income high; Exsanguinate converts spends into heavy spikes

### Path × verb matrix

```
                 HEMORRHAGE           EXSANGUINATE          BUTCHER’S FEAST
Stack           stance, scale,       bigger RMB numbers    more Fuel per stack
                slow decay
Spend           optional tax for     cone, frenzy,         spend → Butcher Round
                DR/blocks            threshold autos       / heavy ammo
Heavy           bloody swap feels    dump then swap        core fantasy
                natural              delete
```


## 6. Crowns & Sacred Cows

### Crimson Throne (Exotic) — Hemorrhage crown
- While Blood ≥ ~80% MaxBlood: +saw area, +DR, mild grunt pull
- Optional: MaxBlood soft overcap to 25–30 with decaying extra stacks still 1-at-a-time
- Must feel like *ruling the pile*, not a boring +% sticker

### Bloodletting (Exotic) — Exsanguinate crown
- **Replaces** baseline RMB effect
- Spend **all** current blood (min threshold) → forward butcher cone/pulse
- Damage ∝ blood spent; brief self-hitch (~0.12s) so it’s a decision
- Clear bar dump VFX/audio

### Abattoir Link (Exotic) — Butcher’s Feast crown
- On RMB spend (or on threshold spend if configured): grant **Butcher Round** charge (max 1–2)
- Next heavy shot consumes charge: large damage mult and/or flat bonus + mild ammo refund
- Works with any heavy in `Gear[5]` / heavy slot — no custom projectile

### Iron Snare (Exotic) — Lockdown rewrite
- **Hold R** shackles nearby enemies
- Recharge by moving quickly (+ passive trickle)
- Supports may: blood spend refunds snare charge; heavy kills refund snare charge
- **Does not use RMB**

### Sanguine Aegis (Exotic) — Deflector rewrite
- While sawing: deflect/nullify chance and/or DR
- Scales with Blood stacks **or** costs 1 blood when a block succeeds (build-defining choice via turbo/stat — pick one primary rule in impl; recommend **scale with stacks** + Big Aegis card for size)

### Marrow Magnet (Exotic) — Super Magnet rewrite
- While sawing: pull grunts; also pull fallen limbs/parts (turbo DNA baked in)
- Pull strength scales lightly with Blood

### Viscera Sampler (Exotic) — Nutrition rewrite
- Part/core kills chance to drop health chunks
- After consuming a chunk: saw area + reach for ~3s (turbo DNA baked in)

### Boundary Incursion (Oddity)
- Grid grow — universal keep


## 7. Full Upgrade List (~30 ship + backlog)

Rarity: Standard / Rare / Epic / Exotic / Oddity  
Tags: H Hemorrhage · X Exsanguinate · F Feast (Heavy) · G Glue · B Blood verb · M Melee survival  
Cell rule: Exotics larger; all Exotics same cell count.  
Names are player-facing (full rename).

------------------------------------------------------------------------------
PATH A — HEMORRHAGE                                      [~9]
------------------------------------------------------------------------------

A1. Crimson Throne — Exotic (Keystone)
    At high/max blood: +saw area, +DR, mild pull. Optional soft overcap.
    Stance fantasy for stack path.

A2. Blood Reservoir — Standard (Blood Bucket rewrite)
    +MaxBlood.

A3. Arterial Harvest — Standard (Blood Efficiency rewrite)
    +Blood gained from limb/shell/core events.

A4. Redline Teeth — Rare (Blood Rampage rewrite)
    +Damage per blood stack.

A5. Clot Armor — Rare (Blood Coating rewrite)
    +Damage resistance per blood stack (while Carver active).

A6. Hemoglobin Springs — Epic (Bloody Jumping rewrite)
    +Jump height per blood stack.

A7. Slow Clot — Rare
    Decay interval longer (stacks fall off slower out of combat).

A8. Wide Kerf — Rare (Swinging Technique / area)
    +Saw damage area (box extents).

A9. Long Bar — Standard (Blade Extenders rewrite)
    +Saw reach (maxDamageRange). Still **no falloff**.

A10. Crimson Tempo — Epic
    +Fire rate scaling with BloodNormalized (blood/max).

------------------------------------------------------------------------------
PATH B — EXSANGUINATE                                    [~9]
------------------------------------------------------------------------------

B1. Bloodletting — Exotic (Keystone)
    RMB spends all blood → butcher cone ∝ spent.

B2. Slippery Arteries — Exotic (Slippery Blood rewrite)
    At threshold stacks, auto-consume all blood and partially recharge movement ability.
    Brief ghost DR on trigger.

B3. Phlebotomy — Rare
    RMB costs −1–2 less blood; slightly stronger baseline pulse.

B4. Partial Transfusion — Rare
    RMB can spend half current blood (toggle behavior) for half effect + shorter recovery.
    Enables rhythm dumps without full commit.

B5. Arc Hemorrhage — Epic (Blood Lightning rewrite)
    On blood spend (RMB or threshold): chance per stack spent to fire shocking bolt.

B6. Frenzy Injection — Epic (Rage DNA, spend-gated)
    On RMB spend: gain Frenzy for ~3s (+move speed, +saw damage). No second meter while idle.
    Refreshing spend refreshes duration; does not stack infinitely.

B7. Pressure Valve — Standard
    Baseline RMB spend amount +1–2; +pulse radius.

B8. Sanguine Ignition — Epic (Overheat-on-spend lean)
    After RMB spend, saw applies Fire for a short window; potency scales with blood spent.

B9. Emergency Draw — Rare
    When you drop below 25% HP, next RMB is free (no blood cost) once per grace period.

------------------------------------------------------------------------------
PATH C — BUTCHER’S FEAST                                 [~8]
------------------------------------------------------------------------------

C1. Abattoir Link — Exotic (Keystone)
    Blood spend loads Butcher Round into heavy (empowered next heavy shot).

C2. Core Tithing — Epic (Heavy Pockets rewrite)
    Killing a core grants heavy reserve ammo (or mag ammo if no reserve).

C3. Offal Tax — Rare
    Limb and shell kills grant a smaller amount of heavy ammo.

C4. Fuel Injection — Rare
    RMB spend grants heavy ammo proportional to blood spent.

C5. Bloody Holster — Epic
    While Blood ≥ half Max: swap-to-heavy is faster / ready window; first heavy shot shortly after swap gains bonus damage.

C6. Reciprocal Butcher — Epic
    Heavy weapon kills refund a small amount of Blood (cap per second).

C7. Linked Magazines — Standard
    +Heavy mag size modestly while Blood Carver is in loadout (or while blood > 0).
    Keep modest — Heavy Support grenade exotic already goes huge.

C8. Render Down — Rare
    While sawing at MaxBlood, passively drip a tiny amount of heavy ammo over time (slow).

------------------------------------------------------------------------------
GLUE / MELEE SURVIVAL / GUNFEEL                          [~10]
------------------------------------------------------------------------------

G1. Sanguine Aegis — Exotic (Deflector rewrite)
    While sawing: deflect/nullify and DR; scales with blood stacks.

G2. Bulwark Plates — Epic (Big Deflector rewrite)
    Aegis size + DR up; move slower while sawing.

G3. Marrow Magnet — Exotic (Super Magnet rewrite)
    Pull grunts + fallen limbs while sawing; scales lightly with blood.

G4. Iron Snare — Exotic (Lockdown rewrite)
    **Hold R** to shackle nearby enemies. Recharge by moving quickly.
    RMB remains blood spend.

G5. Viscera Sampler — Exotic (Nutrition Sampler rewrite)
    Kill/part chance to drop health chunks; chunk → saw area/reach window.

G6. Family Style — Epic (Sharing Lunch rewrite)
    Viscera Sampler chunks also spawn for allies (or ally-visible copies).

G7. Repair Plume — Epic (Repairing Exhaust rewrite)
    While sawing, periodically heal nearby allies.

G8. Reckless Advance — Rare (Rush rewrite)
    While sawing: +move speed, take more damage. Blood stacks reduce the taken-damage penalty slightly.

G9. Warning Label — Rare (Safety Warning rewrite)
    +Damage; take more damage while sawing. Spend blood to briefly suppress the penalty.

G10. Adrenal Kerf — Epic (Shocking Adrenaline rewrite)
    After you are electrocuted: +saw speed briefly.

G11. Caustic Teeth — Rare (Corrosive Coating rewrite)
    After you are corroded: +reach briefly (still no falloff).

G12. Flashpoint Teeth — Rare (Ignition Recharge rewrite)
    When you are ignited: refund saw mag ammo.

G13. Greedy Reservoir — Rare (Greedy Fuck rewrite)
    +Mag and reserve; less ammo regen from offhand damage.

G14. Spare Teeth — Standard (Additional RAM Stick)
    +Magazine size.

G15. Deep Pockets — Standard (RAM Storage)
    +Ammo reserves.

G16. Needle Point — Rare (Aim For The Liver rewrite)
    Smaller saw area; +damage. Precision butcher.

G17. Boundary Incursion — Oddity
    +Upgrade grid size.

------------------------------------------------------------------------------
FROZEN v1 SHIP POOL (exactly 30)
------------------------------------------------------------------------------

EXOTIC (7)
  1  Crimson Throne          (A)
  2  Bloodletting            (B)
  3  Slippery Arteries       (B)
  4  Abattoir Link           (C)
  5  Sanguine Aegis          (G)
  6  Marrow Magnet           (G)
  7  Viscera Sampler         (G)
  8  Iron Snare              (G)  — if 7 exotics preferred, drop Family Style from epic and keep 8th exotic; see note

NOTE: Eight exotics is acceptable if cell budgets match; if hard-capped to 7, cut Iron Snare to backlog only if Hold-R control is deferred — **prefer keeping Iron Snare in v1** and cut one glue epic.

Recommended frozen 30:

  EXOTIC (8)
    1  Crimson Throne
    2  Bloodletting
    3  Slippery Arteries
    4  Abattoir Link
    5  Sanguine Aegis
    6  Marrow Magnet
    7  Viscera Sampler
    8  Iron Snare

  EPIC (7)
    9  Hemoglobin Springs
    10 Crimson Tempo
    11 Arc Hemorrhage
    12 Frenzy Injection
    13 Core Tithing
    14 Bloody Holster
    15 Repair Plume

  RARE (9)
    16 Redline Teeth
    17 Clot Armor
    18 Slow Clot
    19 Phlebotomy
    20 Fuel Injection
    21 Offal Tax
    22 Reckless Advance
    23 Needle Point
    24 Warning Label

  STANDARD (5)
    25 Blood Reservoir
    26 Arterial Harvest
    27 Long Bar
    28 Spare Teeth
    29 Deep Pockets

  ODDITY (1)
    30 Boundary Incursion

BACKLOG (designed, expand later)
  Wide Kerf, Partial Transfusion, Pressure Valve, Sanguine Ignition, Emergency Draw,
  Reciprocal Butcher, Linked Magazines, Render Down, Bulwark Plates, Family Style,
  Adrenal Kerf, Caustic Teeth, Flashpoint Teeth, Greedy Reservoir,
  Multiversal Thievery / Edge Fault (contraband parity only if desired)

------------------------------------------------------------------------------
CUT / DEMOTE FROM VANILLA IDENTITY
------------------------------------------------------------------------------

| Vanilla | Fate |
|---|---|
| Rage (separate meter) | **Cut.** Frenzy Injection = spend-gated buff only |
| Blood as upgrade-gated flag | **Baseline always on** |
| Continuous blood drain | **Cut.** 1-stack-at-a-time decay |
| Damage falloff | **Cut.** Flat damage in reach |
| Lockdown on Aim/RMB | **Rebound to Hold R** (Iron Snare) |
| Heavy Pockets as only heavy fantasy | **Demoted** to Core Tithing building block |
| Overheat System as hold-M1 only | Prefer Sanguine Ignition (spend window); hold-M1 overheat backlog |
| Aim For The Liver | Kept as Needle Point |
| Deflector / Magnet / Nutrition | Renamed exotics; turbo DNA baked in where noted |


## 8. Example Builds

### Pure Hemorrhage (stack tank)
Blood Reservoir → Arterial Harvest → Redline Teeth → Clot Armor → Slow Clot → Crimson Tempo → **Crimson Throne** → Sanguine Aegis  
*Play:* Live at high stacks; RMB only for emergencies; decay won’t wipe you between rooms.

### Pure Exsanguinate (dump assassin)
Arterial Harvest → Phlebotomy → Arc Hemorrhage → Frenzy Injection → **Bloodletting** → Slippery Arteries → Reckless Advance  
*Play:* Bank to 12–20, RMB delete pack, slide on threshold, re-enter.

### Pure Butcher’s Feast (heavy battery)
Arterial Harvest → Core Tithing → Offal Tax → Fuel Injection → Bloody Holster → **Abattoir Link** → Long Bar  
*Play:* Carve cores → mint ammo → RMB load Butcher Round → heavy delete elite → repeat.

### Hybrid butcher (recommended “poster” build)
Crimson Throne + Bloodletting + Abattoir Link + Marrow Magnet + Viscera Sampler  
Stack in the pull, dump cone, heavy finishes, chunk heals, 1-stack decay keeps leftover blood for next room.

### Control butcher
Iron Snare + Marrow Magnet + Clot Armor + Fuel Injection + Abattoir Link  
Hold R snare → pull → saw → spend into heavy.


## 9. Strengths, Weaknesses & Design Pillars (checklist form)

### Strengths
- Readable blood verb (stack / RMB / threshold / heavy)
- Melee fantasy without falloff betrayal
- Downtime-safe bank (1-at-a-time decay)
- Heavy slot synergy without forcing one heavy type
- Three paths + intentional hybrids

### Weaknesses / fun failure states
- Out of reach = zero damage (by design)
- Mag empty mid-pile without Flashpoint/economy
- RMB’d too early (small dump) or too late (died with 20 blood)
- Heavy path without a heavy equipped feels incomplete (UI hint?)
- Stack tank without Aegis/Magnet still dies to ranged chip

### Design risks
- Butcher Round + strong heavies overkill elites — tune mult carefully
- Eight exotics crowding grids — keep shapes consistent
- Hold R vs reload buffering — need clear hold threshold + UI
- Anatomy blood double-counting on part+brain kills — define once in impl
- Baseline passive +%dmg per stack too strong with Redline Teeth — keep baseline tiny


## 10. Success Criteria / Player Fantasy Checklist

- [ ] Blood is visible and meaningful with zero blood upgrades equipped
- [ ] Blood climbs while carving a tanky target (damage instances), not only when pieces pop
- [ ] Leaving combat loses blood slowly (1 per tick), not “bar deleted”
- [ ] Saw deals full damage anywhere in reach (no falloff surprise)
- [ ] RMB always means blood spend on baseline
- [ ] Hold R snare only appears/works with Iron Snare; never steals RMB
- [ ] A Hemorrhage build feels scarier at 18 blood than at 2
- [ ] A Bloodletting dump is a crisp cash-out every few seconds
- [ ] An Abattoir Link swap deletes something important
- [ ] Part priority (limb/shell/core) is felt in blood income
- [ ] Hybrid grids feel intentional, not anti-synergistic
- [ ] Failure states stay fun — not AFK lockout, not empty meter from walking
- [ ] Co-op: Repair Plume / Family Style / chunks help allies; Fuel can stay personal


## 11. Universal Truths (Mycopunk alignment)

- Exotic shapes should always be larger than others; each exotic should use the same number of cells.
- v1 targets **~30** upgrades (frozen list above); backlog is real design, not trash.
- Three paths create different build options but **may intermingle** on the grid.
- Full rename for rework identity.
- Prefer blood verbs: ±MaxBlood, ±gain, decay interval, scale-with-stacks, RMB spend, threshold spend, heavy fuel, anatomy mint.
- No second Rage meter.


## 12. Vanilla Carver → Blood Carver Fate Table

| Vanilla name | Blood Carver name | Path | Notes |
|---|---|---|---|
| (baseline blood off) | Baseline Blood | — | Always on |
| Blood Bucket | Blood Reservoir | A | |
| Blood Efficiency | Arterial Harvest | A | Anatomy-weighted |
| Blood Rampage | Redline Teeth | A | |
| Blood Coating | Clot Armor | A | |
| Bloody Jumping | Hemoglobin Springs | A | |
| Slippery Blood | Slippery Arteries | B | Threshold auto |
| Blood Lightning | Arc Hemorrhage | B | On spend |
| Rage | Frenzy Injection | B | Spend buff only; no meter |
| Heavy Pockets | Core Tithing | C | |
| Deflector | Sanguine Aegis | G | Stack-scaled |
| Big Deflector | Bulwark Plates | backlog | |
| Super Magnet | Marrow Magnet | G | +limbs |
| Lockdown | Iron Snare | G | **Hold R** |
| Nutrition Sampler | Viscera Sampler | G | +chunk area window |
| Sharing Lunch | Family Style | backlog | |
| Overheat System | Sanguine Ignition | backlog | Spend-window fire |
| Repairing Exhaust | Repair Plume | G | |
| Rush | Reckless Advance | G | |
| Safety Warning | Warning Label | G | |
| Blade Extenders | Long Bar | A/G | Reach only, no falloff |
| Swinging Technique | Wide Kerf | backlog | |
| Aim For The Liver | Needle Point | G | |
| Additional RAM Stick | Spare Teeth | G | |
| RAM Storage | Deep Pockets | G | |
| Greedy Fuck | Greedy Reservoir | backlog | |
| Ignition Recharge | Flashpoint Teeth | backlog | |
| Corrosive Coating | Caustic Teeth | backlog | |
| Shocking Adrenaline | Adrenal Kerf | backlog | |
| Boundary Incursion | Boundary Incursion | G | Keep name |
| Edge Fault / Multiversal | — | — | Optional contraband parity |


## 13. Turbocharges DNA (bake into design / optional external turbo later)

| Source idea | Native design home |
|---|---|
| Rage 1-stack decay | **Baseline blood decay model** |
| Nutrition chunk → area/reach 3s | Viscera Sampler |
| Super Magnet limb pull | Marrow Magnet |
| Lockdown + Rot | Iron Snare support card or turbo later |
| Deflector nullify chance | Sanguine Aegis numbers |

External SparrohsTurbocharges can still amplify these later; v1 design should not *require* that mod.


## 14. Implementation Notes (for later coding passes)

### Host
- Prefer `BloodCarverBehaviour` on runtime clone of `TheCarver` **or** subclass when prefab exists
- Custom data struct parallel to `TheCarver.CarverData` plus:
  - blood, maxBlood, grace timestamp, decay interval
  - spend cost/min/recovery
  - heavy fuel / butcher round charges
  - frenzy timers
  - anatomy blood weights

### Hooks
- `FireBullet` / box cast — force flat range damage (ignore falloff mult)
- `OnKillTarget` / part-kill callbacks — anatomy blood mint; distinguish limb/shell/core
- `Update` — 1-stack decay after grace; snare recharge
- RMB — `PlayerInput` Aim was used for Lockdown vanilla; **rebind blood spend to Aim/RMB**, move snare to Hold Reload
- Heavy — `player.Gear[5] is IWeapon` pattern from vanilla `killTargetHeavyAmmo`
- HUD — `UpdateStackDisplay` for Blood (and Frenzy as timed buff, not permanent meter)

### Input detail (Hold R)
- Track reload button hold time
- If released < 0.25s → reload (if needed) / vanilla reload behavior
- If held ≥ 0.25s and Iron Snare equipped and charged → shackle, cancel reload intent
- Show snare recharge UI only when exotic present (mirror vanilla shackleCooldown attachment)

### Falloff kill-switch
- On apply upgrades / spawn: set `rangeData` falloffStart/End equal or beyond maxDamageRange, maxFalloffDamageMultiplier = 1f
- And/or postfix damage mult to 1f for this gear only

### Network
- Blood stacks owner-authoritative like vanilla blood
- Butcher Round charge should sync or be owner-only with heavy shot local authority patterns
- Mark mod `IsSandbox`

### Registration
- Same flow as weapon template / Heat Cycler: AllGear inject, CreateUpgrade pool, SpawnGear remap, persistence by gear id


## 15. Open Tuning Questions (playtest, not design blockers)

1. MaxBlood 20 vs 15 vs 25 readability
2. Decay interval 0.7–1.2s sweet spot
3. Baseline RMB cost 5 and min 3
4. Anatomy weights 1/2/3 vs 1/1/2
5. Butcher Round heavy damage mult (start ~1.75–2.25× once)
6. Crimson Throne threshold 80% vs Max-only
7. Whether baseline passive +%dmg per stack is 0.5% or 1.0%
8. Mag size — keep ~100 or lower to force reload rhythm more
9. Hold R threshold 0.2–0.3s vs controller feel
10. Exact frozen epic cuts if exotic count must drop to 7


## 16. Locked Decisions Log

| Decision | Lock |
|---|---|
| Product | New primary **Blood Carver** (not vanilla replace) |
| Rage | Cut as meter; Frenzy = spend buff |
| Blood spend input | **RMB** active consume |
| Threshold autos | Upgrade-owned (Slippery Arteries etc.) |
| Heavy path depth | Ammo + next-shot empower + swap windows (no custom heavy projectile) |
| Naming | Full rename |
| Blood sources | **Damage instances (baseline)** + anatomy limb / shell / core kills |
| Blood decay | **1 stack at a time** after grace |
| Damage falloff | **None** |
| Lockdown | **L2 — Hold R** (Iron Snare); RMB stays blood |

### Design changelog

#### v1.1
- **Blood gained on damage instances** (counter → +1 stack), not kill-only
- Anatomy part-kills remain bonus mints on top of instance income
- Baseline combat loop / locked decisions updated accordingly

#### v1 (this doc)
- Blood Carver identity and three paths
- Baseline blood + RMB Exsanguinate
- 1-at-a-time decay; no falloff
- Iron Snare on Hold R
- Frozen 30 + backlog
- Rage cut; heavy feast path; full rename


## 17. Next Steps After This Doc

1. Review frozen 30 vs backlog cuts with playtest priorities
2. Implement behaviour host + blood HUD + no-falloff + decay
3. Implement RMB spend + Hold R snare exotic
4. Register path crowns + glue staples
5. Heavy Fuel / Butcher Round
6. Pass rename strings + icons
7. Balance pass on Butcher Round and stack scaling
