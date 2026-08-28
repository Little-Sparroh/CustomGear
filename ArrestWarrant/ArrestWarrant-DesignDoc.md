# Arrest Warrant — Design Document (v1)

## 0. Locked Decisions (2026-08-14)

| Decision | Lock |
|----------|------|
| Product shape | **Parallel new heavy** — nothing replaced |
| Player-facing name | **Arrest Warrant** |
| Working APIName | `arrest_warrant` |
| Working GUID | `sparroh.arrestwarrant` |
| Slot | **Heavy** |
| Relation to vanilla | Spiritual successor / cousin to **G6 Street Sweeper** (unfinished acid mag-1 heavy); does **not** replace or patch vanilla G6 |
| Paths | **License / Flush / Brace** |
| Core fantasy | **Swap-fodder catalyst** — use this heavy to empower the rest of the loadout |
| Baseline License | **Always on** — weak Warrant window after reload exists with zero upgrades |
| Empower targets | **Any / all** — primary, melee, grenade, player body, heavy itself; per-upgrade |
| Mag | **1 baseline** (comfortable default); **not sacred** — path cards may raise mag |
| Input claims | **R** = reload (combat verb); **Hold R** and **RMB** free for path crowns / toys |
| Element | **Acid** baseline (G6 DNA); Flush deepens; other elements only as explicit cards |
| Tone | **SAXON gallows humor / badge-and-paperwork** (court cousin to Final Judgement, street-level) |
| Exotic count | **E6** — equal large hex shapes |
| Ship pool | **~30** upgrades; hybrids OK; no anti-synergy matrix |
| MycoMod (impl) | IsSandbox |
| Doc file | ArrestWarrant-DesignDoc.txt (this file) |
| Vanilla G6 | Left untouched in catalog; Arrest Warrant is parallel unlock |

Wiki / vanilla DNA (inspiration only — not a patch list):

| Vanilla G6 | Arrest Warrant |
|------------|----------------|
| License to Kill (Epic, +44–50% other-weapon dmg ~4s after reload) | **Baseline verb + full License path** |
| Acerbic Flush (Epic, acid explosion on reload) | **Flush path keystone DNA** |
| Kinetic Absorbers (Rare, hold SS: slow + DR) | **Brace path DNA** |
| Melodramatic Hero (Exotic, crit HP hero shot + heal) | **Brace exotic** |
| Secret Pocket (Exotic, hold R emergency ammo when dry) | **Brace / economy exotic — Hold R claim** |
| G6 Choke / Corn All Around The Cob | Geometry rares |
| Crowd Control / Double Loading / Driver Update | Glue retuned into pool |
| Mag 1, 16 pellets, acid, falloff 6→20, reserves 6 | Baseline band starting point (retuned upward honesty) |

---

## 1. High Concept / Fantasy

A SAXON **field authorization shotgun**. One short-range acid blast is the *stamp*. Reload is the *notarization*. The rest of your kit — primary, melee, grenades, even your legs — gets meaner while the **Warrant** is live.

You do not main Arrest Warrant as a DPS hose. You **cycle through it**: peel a face, slap the badge, swap out, murder with a licensed loadout, dip back when the window fades.

Baseline is honest and complete without upgrades:
- Mag-1 (default) acid pellet burst at short range — mediocre as a pure gun, **functional** as panic peel
- Reload always grants a **weak Warrant** window to other gear
- No free permanent aura; power is earned by spending heavy tempo

Upgrades fork the badge:

- **License** — the Warrant gets thicker, longer, weirder (dmg, RoF, element riders, grenade crumbs, on-swap dumps)
- **Flush** — the stamp also *cleans the street* (acid geometry, reload booms, linger denial)
- **Brace** — hold the tube, eat the hit, hero-shot, second wind, then leave licensed

**One-liner:** *Blast. Notarize. Swap. The rest of the kit does the murder — with a badge.*

**Product shape:** New parallel heavy (**Arrest Warrant**). Does not replace G6 Street Sweeper, Zephyr, Final Judgement, Manifold, Caduceus, or any vanilla heavy.

**SAXON marketing blurb (draft):**
  “SAXON AW-6 Arrest Warrant — Short-range acid authorization system.
   Reload notarizes concurrent loadout privileges for a limited window.
   Other weapons may become statistically impolite. Street sweeping is
   metaphorical unless Flush packages are installed. (Legal: do not serve
   this document on civilians. Also do not call it the G6. Catalog
   confusion voids nothing; we checked.)”

Optional stingers:
- “You don’t main the badge. You flash it.”
- “If the primary is doing the murder, the paperwork worked.”
- “Hold R is not a suggestion. It is a clause.”
- “Magazine capacity: usually one. Appeals: file during the Warrant window.”
- “Similar to Street Sweeper. Different docket number. Better lawyer.”

---

## 2. Role & Fantasy in the Arsenal

- **Slot:** Heavy
- **Range:** Short (face-check / peel); choke cards ease, never mid-range DMR identity
- **Role:** Loadout battery / swap catalyst / panic peel / optional acid room control / optional survivor brace
- **Loop:** Equip heavy → stamp (shot) → notarize (R) → swap → dump licensed kit → re-dip when window dies or peel needed
- **Gap filled:**
  - G6 Street Sweeper (vanilla) = unfinished acid mag-1 heavy with lonely License to Kill epic
  - Zephyr = primary sonic cone clear — not heavy swap battery
  - Caduceus = team tether support primary — not self-loadout warrant
  - Final Judgement = 8s strategic charge nuke — not short stamp cycle
  - Manifold / Salvo / Siege = rocket volume fantasies
  - HeavyAmmoRegen / LoadoutHeavy QoL = ammo/loadout helpers, not a weapon identity
  - Nothing owns “heavy shotgun stamp → reload notarizes **your whole kit** → swap fodder excellence”

**Not trying to be:** fixed G6 stat-stick, Zephyr 2.0, Globbler acid primary, Caduceus heal beam, Final Judgement charge tube, permanent other-weapon aura with no heavy interaction, or a mag-fed comfort shotgun that never wants to leave your hands.

### 2.1 Comparison snapshot

```
Weapon / kit           Niche                            Arrest Warrant differentiator
---------------------  -------------------------------  ------------------------------------------
G6 Street Sweeper      Vanilla unfinished acid heavy    Parallel full fantasy; baseline Warrant
Zephyr                 Primary sonic cone clear         Heavy catalyst; swap loop, not main clear
Caduceus               Team tether support primary      Self-loadout Warrant, not ally beam
Final Judgement        8s charge strategic heavy        Short stamp cycle; mag comfort optional
Manifold Rocket        Raycast rocket primary           Acid shotgun + kit buff, not rockets
Globbler               Acid puddle primary              Acid is seasoning + Flush path only
AMR                    Bolt primary, Chambered Argument Reload arms *other gear*, not own bolt
DMLR Hot Swap          Laser↔DMR internal mode economy  Cross-slot loadout battery, not mode swap
License to Kill (G6)   One lonely epic                  Entire path + baseline verb
```

### 2.2 Naming note

**Arrest Warrant** deliberately echoes badge / paperwork / street authority without colliding on G6 API name, gear id, or display string. Codex may wink at the Street Sweeper cousin; it must not claim to be a rework patch of vanilla G6.

---

## 3. Design Pillars

1. **Swap-fodder is the fantasy** — success is “my primary just deleted that pack because I flashed the badge,” not “I held heavy all mission.”
2. **Baseline Warrant is sacred** — every reload grants a weak other-gear window with zero upgrades. Paths thicken it; they do not invent the verb.
3. **SS/AW body damage stays honest** — power budget lives in Warrant potency, Flush geometry, Brace survival — not turning the heavy into a secret best primary.
4. **Short range is identity** — choke eases; never erases face-check fantasy.
5. **Mag 1 is default, not sacred** — Double Loading / path cards may raise mag; identity survives mag 2–3 if Warrant still gates power.
6. **On-action verbs > flat % stickers** — shot, reload, hold-R, RMB, swap-away, while-holding, on-Warrant-expire.
7. **Empower targets are per-card** — primary, melee, grenade, player body, heavy self; no global rule that everything always buffs everything.
8. **Three peer paths; hybrids intended; no anti-synergy matrix.**
9. **R = reload notarization** (always). **Hold R** and **RMB** are path/exotic claim slots (Secret Pocket DNA, special warrants, flush commands, brace channel).
10. **~30 ship upgrades; E6 equal large exotic shapes.**
11. **Acid stays baseline element** — Flush owns acid excellence; don’t default to Fire/Shock identity theft.
12. **Boss-safe / ally-safe** — Warrant is self-loadout; Flush acid respects friendly rules; Brace DR is personal.
13. **Zero upgrades = complete gun + complete loop** — stamp → notarize → swap → dump.
14. **Hallows / SAXON badge humor** in blurbs; industrial shotgun gunfeel.

---

## 4. Core Mechanics & Gunfeel (Baseline)

### 4.1 Base gun

| Trait         | Draft / intent |
|---------------|----------------|
| Fire mode     | Semi pump / single trigger — **one authorization blast per shot** |
| Delivery      | Multi-pellet acid energy burst (G6 DNA: ~12–16 pellets) |
| Damage        | Honest peel — deletes/wounds close grunts on center mass; loses to real primaries at range and marathon DPS |
| Element       | Acid (modest apply per pellet) |
| Range         | Short — falloff starts close (draft start ~7–9m, end ~18–22m); max falloff mult not as cruel as vanilla 0.1 (draft ~0.25–0.35 so edge pellets aren’t pure insult) |
| Mag / reserve | **Mag 1** default; reserves draft **6–8** (heavy ammo economy is global brake) |
| Reload        | Snappy intentional (~0.9–1.2s) — **this is the notarization beat** |
| Spread        | Wide horizontal bias (street broom); vertical present but not pure sky RNG |
| Recoil        | Fat vertical punch per shot — readable stamp |
| ADS / RMB     | Optional weak ADS; **RMB unbound** on baseline |
| Hold R        | Unbound on baseline (Secret Pocket / path claims later) |
| Movement      | Slight heavy equip weight; Brace path adds plant slow |
| Model/audio   | Industrial badge-shotgun: hazard stripes, warrant stencil, acid green vents, pump clack, wet acid hiss, notarize stamp SFX on reload complete |

Draft firefeel band (VALIDATE IN PLAYTEST):

| Param | Draft |
|-------|--------|
| Damage per pellet | Medium-low; total close burst = “peel pack / crack soft elite face” not “delete tank” |
| Pellets | 14 (band 12–16) |
| Fire interval | ~0.45–0.55s (mag 1 makes this mostly academic between reloads) |
| Mag | 1 |
| Reserves | 7 (band 6–8) |
| Reload | 1.0 s |
| Falloff start / end | 8 m / 20 m |
| Max falloff mult | 0.3 |
| Spread H / V | Wide / medium-wide |
| Acid amount | Modest per pellet; full burst should matter on unarmored |
| Baseline Warrant duration | **3.5–4.5 s** after reload completes |
| Baseline Warrant other-weapon damage | **+18–25%** (weaker than vanilla LtK’s ~44–50% — room for path to grow) |
| Baseline Warrant targets | **Primary + Melee + Grenade damage** (simple blanket); path cards specialize |

### 4.2 Inputs

| Input | Baseline | Upgraded claims |
|-------|----------|-----------------|
| M1 | Fire authorization blast | Flush pellet riders; Brace hero shot; License on-shot stack crumbs |
| R (tap) | Reload + **notarize** (grant Warrant) | Always reload; path riders on notarize (Flush boom, License potency, Brace DR pulse) |
| Hold R | Unbound | Secret Pocket emergency load; Brace channel absorb; License “extend warrant” / special serve |
| RMB | Unbound | License Serve (mark/detonate warrant); Flush directional spray command; Brace plant toggle |
| Swap away | Warrant stays on other gear for remaining duration | On-swap dump cards (License); drop Brace while-holding effects |
| Heavy equip | Normal | No baseline link to other heavies |

### 4.3 Warrant system (sacred baseline verb)

**Warrant** = timed buff state on the player after a successful **notarization** (reload complete while Arrest Warrant is the active heavy, or as cards specify).

```
Notarize (baseline):
  On reload complete while AW equipped and reload was “real” (spent empty or partial per gun rules):
    1. Grant Warrant buff for T seconds
    2. Play stamp SFX + brief badge VFX on player
    3. HUD pip: Warrant timer

While Warrant active:
  - Other weapons (primary, melee) deal +X% damage
  - Grenades deal +X% damage (and/or +status amount — pick one in impl; prefer damage mult for clarity)
  - Arrest Warrant’s own shots do NOT consume or heavily benefit from Warrant
    (avoid “buff myself forever on the heavy” loop); minor self benefit only if a card says so

Warrant ends:
  - Timer expiry
  - Optional: death
  - Cards may refresh, stack, or convert on expiry
```

**Stacking model (draft):**
- Baseline = **duration refresh** (re-notarize refreshes T; no infinite stack)
- License path may add **Warrant Charges** (stacks 1–3) that amplify potency or add riders
- Prefer readable: timer bar + optional stack pips — not ten invisible mults

**What can be empowered (menu of verbs — per upgrade):**

| Target | Example effects |
|--------|-----------------|
| Primary | Damage, RoF, reload, spread, element amount, projectile riders |
| Melee | Damage, reach, attack speed, lifesteal crumb |
| Grenade | Damage, radius, charge regen crumb, throw force |
| Player body | Move speed, DR, HP crumb, magnet boots feel (careful) |
| Heavy (AW) | Self shot quality during Brace/hero; ammo economy |
| On-swap | One-shot dump when leaving heavy |
| On-expire | Parting shot / acid burst / haste crumb |

### 4.4 Baseline combat loop (zero upgrades)

```
Face-check or peel → M1 acid burst (honest short-range stamp)
   → R notarize (~1s) → Warrant window opens on primary/melee/grenade
   → SWAP to primary (or melee) → dump licensed kit for ~4s
   → when Warrant fades or peel needed → heavy again → stamp → notarize → repeat
   → Hold R / RMB do nothing yet
```

Skill without upgrades:
- Not wasting notarize when you won’t swap
- Not sitting on heavy during a live Warrant
- Using the blast as peel/interrupt, not as main clear
- Ammo husbandry (mag 1 + limited reserves)
- Timing Warrant with elite windows / grenade throws

### 4.5 What baseline does NOT include

- No acid explosion on reload (Flush-owned)
- No while-holding DR plant (Brace-owned)
- No hero crit-HP shot (Brace exotic)
- No Secret Pocket (Brace/economy exotic)
- No Warrant stacks / on-swap dumps / grenade charge refunds (License-owned)
- No choke sniper conversion
- No full-auto hose
- No permanent other-weapon aura without notarize
- No team-wide Warrant (self loadout only unless a future co-op card is explicit)
- No mag > 1 (until cards)

Those are path-, exotic-, or unlock-owned.

### 4.6 Mag model (not sacred)

```
Baseline mag = 1
Double Loading / path comfort may raise to 2–3
Identity test: if mag rises, notarize should still matter
  - Prefer: Warrant grants on reload complete still
  - Optional card: also grant crumb Warrant on each shot (weaker) so mag>1 isn’t “ignore reload”
  - Never: infinite Warrant with no AW interaction
```

---

## 5. Shared Framework Vocabulary

Upgrades speak these verbs. Baseline owns **Blast / Notarize / Warrant** only.

### 5.1 Blast (authorization shot)
- Multi-pellet short-range acid burst
- Peel / stamp readability
- All paths can ride Blast; none replace it as the M1 verb without an exotic rebuild

### 5.2 Notarize (reload complete)
- Combat verb, not pure downtime
- Always grants baseline Warrant
- Flush booms, License potency pulses, Brace DR pulses hook here

### 5.3 Warrant (timed loadout buff)
- Core fantasy state
- Duration + potency + optional stacks + optional target filters
- License path owns excellence; baseline owns existence

### 5.4 Serve (License active verb)
- RMB or Hold-R claim: mark an enemy as **Served** (damage-taken amp while Warrant live), or detonate remaining Warrant into a burst, or extend timer
- Upgrade-owned

### 5.5 Flush (acid geometry)
- Reload explosions, pellet acid excellence, linger puddles/splashes, cone reshapes
- Path B owned
- Classic spheres/splashes OK for acid (not Manifold-constrained)

### 5.6 Brace (while-holding / survivor)
- Move slow + DR while AW hard-equipped
- Hero shot at low HP
- Secret Pocket / second wind
- Path C owned

### 5.7 Swap Dump
- On weapon-swap away from AW while Warrant active: one-shot bonus (damage pulse, speed crumb, grenade charge, acid nova)
- License-leaning; hybrid OK

### 5.8 Pocket (Hold R economy)
- Emergency ammo / emergency notarize / channel absorb
- Hold-R claim space

### 5.9 What we deliberately do NOT vocabulary

- Zephyr cone force-launch as identity
- Caduceus ally tether / Grace Über
- Final Judgement 8s charge authorization
- Globbler primary puddle hose as default
- Permanent always-on other-weapon aura with no AW action
- Team-wide damage aura baseline
- Full-auto mag-dump shotgun as default fantasy

---

## 6. Paths (gravity wells — hybrids intended)

### Path A — LICENSE (swap battery) ★ identity path
**“I touched the badge and now my primary is illegal.”**

- Spine: Warrant duration, potency, stacks, target specialization, on-swap dumps, Serve mark, grenade/melee riders
- ST via licensed primary burst windows; clear via swapping to the right tool while badged
- **Why it wins without Flush/Brace:** pure swap-fodder fantasy the arsenal under-serves
- Hybrid hooks: notarize Flush boom while Warrant thickens; Brace plant then swap-dump

### Path B — FLUSH (acid street control)
**“The stamp also cleans the street.”**

- Spine: Acerbic reload boom, acid amount, linger splashes, choke/fan geometry, pellet count, short-range delete quality
- Clear via acid geometry + reload cadence; still wants swap but can hold heavier if built
- **Why it wins without License/Brace:** G6 shotgun excellence — the gun itself becomes worth firing more
- Hybrid hooks: Flush kills refresh Warrant; acid-cooked targets take bonus licensed damage

### Path C — BRACE (survivor / hero / pocket)
**“Plant the tube. Eat the hit. Stamp. Leave.”**

- Spine: while-holding DR + slow, Melodramatic hero shot, Secret Pocket, absorb channel Hold R, low-HP Warrant potency
- Fantasy: panic tool and clutch second wind; still feeds swap by keeping you alive to notarize
- **Why it wins without License/Flush:** clutch identity — the heavy that saves the run then hands the kill to your primary
- Hybrid hooks: hero shot notarizes free; Pocket reload still grants Warrant; DR plant during notarize animation

### Path × verb matrix

```
                 LICENSE                     FLUSH                        BRACE
Blast            optional on-shot crumb      acid excellence / geometry   hero shot riders
Notarize (R)     thick Warrant ★             Acerbic boom / splash ★      DR pulse / pocket arm
Hold R           extend / special serve      optional second flush        Secret Pocket / absorb ★
RMB              Serve mark / dump ★         directional flush command    plant toggle / taunt crumb
Warrant          CORE FANTASY ★              hybrid refresh on acid kill  low-HP potency hybrid
While holding    prep / weak passive         optional acid drip           CORE DR + slow ★
Swap away        dump / keep Warrant ★       linger OK                    drop brace keep Warrant
Other weapons    CORE ★                      hybrid                       hybrid sustain
AW body DPS      stays honest                path may raise               hero moments only
```

---

## 7. Crowns & Sacred Cows

### 7.1 Exotics (E6 — equal large shapes)

**A-EX1. License to Kill — Exotic** (License path unlock crown) ★  
- Greatly increases Warrant **potency and duration** on other weapons after notarize.  
- Baseline Warrant becomes “real illegal” — the fantasy keystone.  
- Slightly **reduces AW direct damage** or pellet count (tax: you are not meant to main the tube).  
- Gates or strongly amplifies supporting License cards (prefer soft amplify if hard gate feels bad).  
- DNA: vanilla License to Kill, elevated from lonely Epic to crown.

**A-EX2. Hot Pursuit — Exotic** (License mythic)  
- **On swap away** from AW while Warrant is active: consume a portion of remaining Warrant to grant a **Pursuit** window (draft 2–3s): massive move speed + reload speed on the weapon you swapped to, and/or a one-shot damage amp on first hit.  
- Optional: killing during Pursuit refunds a **crumb** of heavy ammo (ICD).  
- RMB optional: **Serve** — mark look-target; marked takes +damage from all your licensed weapons for Warrant remainder.  
- Fantasy: flash badge, chase, delete.

**B-EX1. Acerbic Mandate — Exotic** (Flush path unlock crown)  
- Notarize (reload complete) triggers a **large acid explosion** at player (or muzzle).  
- +Acid amount on AW pellets permanently while equipped.  
- DNA: vanilla Acerbic Flush elevated.  
- Makes reload a room-clear verb; pairs with mag comfort cards carefully (boom per reload, not per shot).

**B-EX2. Street Ordinance — Exotic** (Flush mythic)  
- Blast becomes a **denser short cone** (choke + pellet coherence) **or** a **horizontal fan wall** (pick one mode via RMB toggle, or two cards — prefer **RMB toggle Fan ↔ Choke** while this exotic equipped).  
- +Effective range modestly in Choke; +width in Fan.  
- Residual **acid slick** on terrain where majority of pellets hit (short deny, real Acid apply).  
- Fantasy: the broom finally sweeps clean.

**C-EX1. Melodramatic Hero — Exotic** (Brace path unlock crown)  
- After reaching **critical HP**, next AW shot: tight spread, +damage, +range, **heals** you.  
- Cannot retrigger until you return to **full HP** (vanilla DNA).  
- While waiting for hero shot, minor DR.  
- Fantasy: clutch stamp, then notarize and hand the fight back to primary.

**C-EX2. Secret Pocket — Exotic** (Brace / economy mythic)  
- First time mag **and** reserves are empty each mission/combat encounter: **Hold R** loads Pocket ammo directly into mag (vanilla DNA; draft full mag 1–3 depending on mag cards).  
- Pocket load **still notarizes** (grants Warrant) — clutch badge flash from empty.  
- Optional turbo later: Pocket can trigger once per X minutes instead of once ever.  
- **Hold R** is the claim; do not fight normal reload input when ammo remains.

### 7.2 Sacred cows (do not cut without rewriting identity)

- Parallel heavy named **Arrest Warrant** (not a vanilla G6 patch)  
- **Baseline Warrant on notarize** always exists  
- Swap-fodder fantasy > AW main-DPS fantasy  
- Short-range shotgun stamp identity  
- Acid baseline element  
- R = reload notarization  
- Hold R / RMB free until paths claim  
- Three peer paths; hybrids OK  
- ~30 upgrades; 6 equal large exotics  
- No permanent kit aura without AW action  
- Vanilla G6 left in catalog untouched  

---

## 8. Full Upgrade List (~30 ship + backlog)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity  
Cell rule: Exotic shapes larger than others; all Exotics same cell count.  
Player-facing names below. API names assigned at implementation.

Wiki / sibling DNA → Arrest Warrant (inspiration only):

| DNA | Arrest Warrant |
|-----|----------------|
| G6 License to Kill | License to Kill exotic + baseline Warrant |
| G6 Acerbic Flush | Acerbic Mandate |
| G6 Kinetic Absorbers | Kinetic Absorbers rare + Brace spine |
| G6 Melodramatic Hero | Melodramatic Hero exotic |
| G6 Secret Pocket | Secret Pocket exotic (Hold R) |
| G6 G6 Choke / Corn Cob | Choke / Fan geometry line → Street Ordinance |
| G6 Crowd Control / Double Loading / Driver Update | Glue retuned |
| AMR Chambered Argument | Cousin “reload arms power” — but arms **other gear** |
| Caduceus Grace | Different — self kit timer, not team Über |
| Final Judgement Brand | Serve mark is lighter cousin, not orbital |
| DMLR Hot Swap | Name nod only in Hot Pursuit; different system |

------------------------------------------------------------------------------
PATH A — LICENSE
------------------------------------------------------------------------------

A-EX1. License to Kill — Exotic (path unlock crown) ★
       +Warrant potency and duration; slight −AW body damage.
       The swap-fodder keystone.

A-EX2. Hot Pursuit — Exotic (mythic)
       On swap-away: Pursuit window (speed + first-hit amp / reload haste).
       Optional RMB Serve mark while Warrant live.

A-EP1. Stacked Charges — Epic
       Notarize grants a Warrant Charge (max 3). Each charge adds potency.
       Refresh duration on notarize; charges decay only on full expiry or optional spend.

A-EP2. Cross-Clearance — Epic
       Warrant also grants +RoF and −reload on primary (and/or +melee attack speed).
       Specializes “the kit feels faster,” not only harder.

A-EP3. Grenade Annex — Epic
       While Warrant active: grenade damage/radius up AND kills/throws may crumb grenade charge (ICD).
       Makes heavy ↔ nade loop filthy.

A-EP4. Probable Cause — Epic
       First hit from each licensed weapon during a Warrant applies bonus Acid or bonus flat damage once.
       Encourages swapping through multiple tools in one window.

A-RA1. Extended Docket — Rare
       +Warrant duration; −Warrant potency slightly.

A-RA2. Harsh Sentence — Rare
       +Warrant potency; −Warrant duration slightly.

A-RA3. Quick Serve — Rare
       −Reload duration (faster notarize); −AW pellet damage slightly.

A-RA4. Badge Bounce — Rare
       On kill with a licensed weapon: +small Warrant duration crumb (cap per window).

A-ST1. Paperwork — Standard
       Minor +Warrant duration.

A-ST2. Fine Print — Standard
       Minor +Warrant potency.

------------------------------------------------------------------------------
PATH B — FLUSH
------------------------------------------------------------------------------

B-EX1. Acerbic Mandate — Exotic (path unlock crown)
       Acid explosion on notarize; +Acid on pellets.

B-EX2. Street Ordinance — Exotic (mythic)
       RMB toggle Fan ↔ Choke geometry; acid slick on dense impact.

B-EP1. Caustic Broom — Epic
       +Pellets and +Acid amount; −bullet speed slightly (wet heavy spray).

B-EP2. Secondary Spill — Epic
       On Blast hit: small acid splash to nearby parts/enemies (clear fork).

B-EP3. Hold-R Gutter — Epic
       Hold R while AW equipped and not empty: channel a short **acid hose**
       (drains mag/reserves faster or heat) — panic clear claim on Hold R
       if Secret Pocket not competing; priority table in §14.
       Alternative if Hold R crowded: make this RMB. Prefer Hold R only if Pocket absent.

B-EP4. Clean Sweep — Epic
       Enemies killed by AW Blast or Flush boom refresh a short Warrant crumb
       (hybrid glue into License).

B-RA1. G6 Choke — Rare
       Tighter spread; +falloff start/end modestly. (Vanilla name OK as nod.)

B-RA2. Corn All Around The Cob — Rare
       Negate vertical spread; +horizontal width. (Vanilla name OK.)

B-RA3. Corrosive Primer — Rare
       +Acid effect amount on pellets and Flush booms.

B-RA4. Wet Work — Rare
       +Blast hitForce / stagger; −damage slightly (peel fork).

B-ST1. Sour Shells — Standard
       Minor +Acid amount.

B-ST2. Extra Bristles — Standard
       Minor +pellets (e.g. +2).

------------------------------------------------------------------------------
PATH C — BRACE
------------------------------------------------------------------------------

C-EX1. Melodramatic Hero — Exotic (path unlock crown)
       Crit HP → next shot hero (spread/dmg/range/heal); rearm at full HP.

C-EX2. Secret Pocket — Exotic (mythic)
       Hold R emergency mag when dry (first time rules); still notarizes.

C-EP1. Kinetic Absorbers — Epic
       While holding AW: −move speed, −damage taken (vanilla DNA elevated).
       Core Brace plant.

C-EP2. Last Call — Epic
       Below 40% HP: Warrant potency increased; notarize also heals a crumb.
       Clutch hybrid into License.

C-EP3. Hold the Line — Epic
       Hold R while AW equipped and mag not empty: channel **Absorb**
       (extra DR, root or heavy slow, build a bank). Release or swap:
       bank converts to short super-Warrant or heal. Competes with Pocket/Gutter — see priority.

C-EP4. Contempt — Epic
       After taking damage while Brace-holding AW, next Blast +damage and
       notarize gains +Warrant duration.

C-RA1. Riot Shield Stance — Rare
       +DR while holding AW; +reload duration slightly (slower notarize tax).

C-RA2. Soft Knees — Rare
       −Move penalty from Brace cards / Kinetic Absorbers; −DR slightly.

C-RA3. Second Wind — Rare
       On Melodramatic Hero heal proc: also refresh Warrant to full duration.

C-RA4. Badge Heavy — Rare
       +AW body damage while below 50% HP (Brace gun quality without erasing swap fantasy).

C-ST1. Thick Jacket — Standard
       Minor +DR while holding AW.

C-ST2. Deep Breath — Standard
       Minor +reload speed (faster notarize under pressure).

------------------------------------------------------------------------------
GENERIC / GUNFEEL
------------------------------------------------------------------------------

G-EP1. Double Loading — Epic (or Rare if footprint tight)
       +Magazine size (+1 or +2). Mag not sacred — this is the comfort fork.
       Each reload still notarizes once (not per shell).

G-RA1. Driver Update — Rare
       +Fire rate and +reload speed (vanilla DNA).

G-RA2. Crowd Control — Rare
       +Reserves and +fire rate; −damage (vanilla DNA retuned).

G-RA3. Spare Badges — Rare
       +Ammo reserves / ammo collect.

G-ST1. Speed Loader — Standard
       +Reload speed.

G-ST2. Hardened Rounds — Standard
       Minor +AW damage (glue; must not outscale Warrant fantasy).

G-OD1. Boundary Incursion — Oddity
       Increases upgrade grid size.

------------------------------------------------------------------------------
FROZEN 30 FOR V1 SHIP
------------------------------------------------------------------------------

EXOTIC (6)
  1  License to Kill
  2  Hot Pursuit
  3  Acerbic Mandate
  4  Street Ordinance
  5  Melodramatic Hero
  6  Secret Pocket

EPIC (8)
  7  Stacked Charges
  8  Cross-Clearance
  9  Grenade Annex
 10  Acerbic-adjacent: Caustic Broom
 11  Secondary Spill
 12  Kinetic Absorbers
 13  Last Call
 14  Double Loading

RARE (10)
 15  Extended Docket
 16  Harsh Sentence
 17  Quick Serve
 18  Badge Bounce
 19  G6 Choke
 20  Corn All Around The Cob
 21  Corrosive Primer
 22  Riot Shield Stance
 23  Soft Knees
 24  Driver Update

STANDARD (5)
 25  Paperwork
 26  Fine Print
 27  Sour Shells
 28  Thick Jacket
 29  Speed Loader

ODDITY (1)
 30  Boundary Incursion

------------------------------------------------------------------------------
BACKLOG (designed, not in first 30)
------------------------------------------------------------------------------

License
- Probable Cause
- Serve-only epic if Hot Pursuit RMB feels crowded
- Ally-share Warrant pulse (co-op card — careful)
- Melee-only specialization exotic (brass knuckles warrant)

Flush
- Hold-R Gutter (if cut for Hold R priority)
- Clean Sweep
- Wet Work
- Extra Bristles
- Longer slick / wall-stick acid

Brace
- Hold the Line channel
- Contempt
- Second Wind
- Badge Heavy
- Deep Breath

Generic
- Crowd Control, Spare Badges, Hardened Rounds
- Range Gate, Recoil Brace

Explicitly rejected as identity (for now)
- Vanilla G6 replace/overwrite
- Permanent kit aura with no notarize/shot/hold interaction
- Baseline team-wide damage share
- Turning AW into best-in-slot main DPS heavy with no swap need
- Full-auto SMG conversion as default
- 8s charge Final Judgement clone
- Caduceus ally heal beam
- Zephyr force-launch cone as default delivery
- Removing baseline Warrant

---

## 9. Example Builds

**Badge Only (License pure)**  
License to Kill + Hot Pursuit + Stacked Charges + Cross-Clearance  
+ Extended Docket + Badge Bounce + Paperwork + Fine Print  
→ Stamp, notarize, swap, Pursuit chase, primary does the murder. AW body stays humble.

**Street Cleaner (Flush pure)**  
Acerbic Mandate + Street Ordinance + Caustic Broom + Secondary Spill  
+ G6 Choke or Corn Cob + Corrosive Primer + Sour Shells + Double Loading  
→ Reload booms, geometry toggle, acid delete; Warrant still weak-baseline helps swap leftovers.

**Clutch Marshal (Brace pure)**  
Melodramatic Hero + Secret Pocket + Kinetic Absorbers + Last Call  
+ Riot Shield Stance + Soft Knees + Thick Jacket + Speed Loader  
→ Plant, eat hit, hero shot, Pocket from empty, notarize, hand off.

**Acid Badge (hybrid License + Flush)**  
License to Kill + Acerbic Mandate + Stacked Charges + Secondary Spill  
+ Corrosive Primer + Harsh Sentence + Badge Bounce  
→ Notarize is boom + thick Warrant; acid sets up licensed finishers.

**Panic Paperwork (hybrid License + Brace)**  
License to Kill + Melodramatic Hero + Hot Pursuit + Kinetic Absorbers  
+ Last Call + Quick Serve + Soft Knees  
→ Survive the face-check, hero stamp, notarize, Pursuit out on primary.

**Broom Tank (hybrid Flush + Brace)**  
Acerbic Mandate + Kinetic Absorbers + Street Ordinance + Double Loading  
+ Riot Shield Stance + Corrosive Primer  
→ Hold longer on AW, boom reloads, still can swap with baseline Warrant.

**Comfort Clerk (mag > 1 hybrid)**  
Double Loading + Driver Update + License to Kill + Cross-Clearance  
+ Speed Loader + Paperwork  
→ Mag 2–3 comfort; notarize still the power button; more stamps per reserve tank.

---

## 10. Economy & Tuning Rules of Thumb

- **Power budget lives in:** Warrant potency×duration, notarize cadence, Flush boom size, Brace DR windows, heavy ammo scarcity — not infinite stacks.
- **Baseline Warrant must feel real** but leave headroom for License to Kill crown (~baseline +20% / 4s → crown +45–60% / 6–8s band — VALIDATE).
- **AW body DPS** unbuilt should lose to a real primary in marathon; win only as peel + setup.
- **Flush pure** may approach “worth holding” for clear — OK if License pure still wins burst swap DPS fantasy.
- **Brace pure** wins clutch / survival, not DPS charts.
- **Double Loading** must not erase notarize (one Warrant per reload complete, not per pellet).
- **Hold R conflicts:** Secret Pocket (empty only) < Hold the Line / Gutter when ammo exists — see §14 priority.
- **RMB conflicts:** Street Ordinance toggle vs Hot Pursuit Serve — priority table; both exotics rare together but hybrid possible.
- **Grenade Annex** ICD so nade regen doesn’t go infinite with spam throws.
- **Stacked Charges** hard cap 3; decay rules simple.
- **Melodramatic Hero** rearm at full HP is the brake — do not remove.
- **Secret Pocket** once-per-empty-cycle is the brake — do not make it every reload.
- **Ally-safe:** Flush explosions default low/no ally damage; Warrant is self-only.
- **Heavy ammo economy** global brake; respect sparroh HeavyAmmoRegen in playtest.
- **Hot Pursuit** must not make permanent speed-tank with swap macro abuse — duration short, ICD on ammo crumb.

---

## 11. Status / Counter Split

| System / counter        | Role                         | Baseline? | Owner |
|-------------------------|------------------------------|-----------|-------|
| Blast (acid pellets)    | Short stamp / peel           | Yes       | Baseline |
| Notarize (reload)       | Grant Warrant                | Yes       | Baseline |
| Warrant timer           | Other-gear amp               | Yes       | Baseline |
| Warrant Charges         | Stack potency                | No        | License epic |
| Serve mark              | Target damage-taken amp      | No        | License exotic/RMB |
| Pursuit on-swap         | Speed/first-hit window       | No        | License exotic |
| Acerbic reload boom     | Acid sphere on R             | No        | Flush exotic |
| Fan/Choke geometry      | Spread modes + slick         | No        | Flush exotic |
| Acid slick / splash     | Linger deny                  | No        | Flush |
| Kinetic Absorbers DR    | While-holding plant          | No        | Brace |
| Melodramatic Hero shot  | Crit-HP clutch blast         | No        | Brace exotic |
| Secret Pocket           | Hold R emergency ammo        | No        | Brace exotic |
| Mag > 1                 | Comfort                      | No        | Double Loading |
| Team Warrant share      | Co-op                        | **No**    | Backlog only |
| Permanent kit aura      | —                            | **No**    | Rejected |

### 11.1 Acid — use real EffectType.Acid (or game acid equivalent)

| Tuning (draft)     | Value / note |
|--------------------|--------------|
| Baseline pellets   | Modest acid amount each |
| Flush boom         | Burst acid + HP damage |
| Slick              | Tick apply while stood in |
| Licensed Probable Cause | Optional once-per-weapon acid hitch |
| Ally apply         | Reduced / off |
| Boss               | Apply OK; no permanent floor delete |

### 11.2 Warrant (gameplay state — buff, not enemy EffectType)

| Tuning (draft)     | Value / note |
|--------------------|--------------|
| Grant on           | Reload complete (notarize) |
| Duration baseline  | ~4 s |
| Potency baseline   | ~+20% dmg other weapons + grenades |
| UI                 | Timer bar + optional charge pips |
| Refresh            | Re-notarize refreshes duration |
| Self AW benefit    | None or tiny unless card |
| Clear on           | Expiry, death |

### 11.3 Serve (optional mark)

| Tuning (draft)     | Value / note |
|--------------------|--------------|
| Apply              | RMB while Warrant live (Hot Pursuit) |
| Effect             | +damage taken from owner’s licensed weapons |
| Duration           | Min(Warrant remaining, cap) |
| Max marks          | 1 |
| UI                 | Badge glyph on target |

---

## 12. Strengths, Weaknesses & Co-op

**Strengths**
- Unique heavy fantasy: loadout battery / swap fodder
- Baseline complete loop without upgrades
- Three readable paths (illegal kit / acid broom / clutch marshal)
- Synergizes with *any* strong primary/melee/nade the player already loves
- Panic peel + notarize is always available
- Co-op readable “they just badged” moments if VFX clear
- Mag comfort optional without mandatory mag-1 pain

**Weaknesses**
- AW alone is not a full clear plan if uninvested in Flush
- Requires discipline to swap (brain-off heavy-hold is wrong except Brace/Flush leans)
- Heavy ammo scarce — whiffed stamps hurt
- Short range on the heavy itself
- Warrant timer management is a skill tax
- Parallel unlock cost (doesn’t fix vanilla G6 for unmodded catalog)

**Co-op**
- Warrant is **self loadout** by default — no steal ally DPS identity from Caduceus
- Flush acid must not grief allies
- Serve mark readable to allies optional (nice-to-have outline)
- Don’t feature team-yeet
- One player’s Acerbic Mandate boom should be telegraphed (color-coded acid)

---

## 13. Visual, Audio & Thematic Design

**Appearance**
- SAXON street shotgun: shorter tube than strategic heavies, badge/star stencil, “AW-6” plate, warrant clipboard charm optional humor
- Acid green vents / drip when loaded
- License lean: gold badge glow while Warrant active on player wrists/weapon
- Flush lean: wet acid hiss, slick decals, broom-wide muzzle flash
- Brace lean: riot shields plates fold out while holding; cracked glass hero optic when Melodramatic armed

**Sound**
- Fire: wet chunky blast + acid sizzle
- Reload notarize: mechanical pump + **rubber stamp thunk** + short UI “AUTHORIZED”
- Warrant active: low badge hum under other weapons (subtle)
- Warrant expire: paper tear / deny beep
- Acerbic boom: acid pop distinct from Final Judgement sphere
- Melodramatic: heroic brass sting (half-joke)
- Secret Pocket: fabric rip + shell click
- Pursuit swap: heel-spark whoosh

**Flavor / codex line (in-game style)**
  Arrest Warrant  
  Heavy short-range acid authorization shotgun.  
  Reload notarizes a temporary damage warrant on your other weapons.  
  Built to flash the badge, swap out, and let the rest of the kit finish the arrest.  
  License upgrades thicken the warrant. Flush upgrades clean the street.  
  Brace upgrades keep you alive long enough to file the paperwork.

---

## 14. Implementation Notes (for later)

### 14.1 Gear registration
- Follow weapon template in this repo: clone base gun, GearInfo high-range id,
  APIName `arrest_warrant`, behaviour component, SpawnGear stamp, CreateUpgrade pool.
- **Clone candidate:** multi-pellet shotgun-adjacent gun (BounceShotgun / G6 if present in AllGear / pellet heavy). Prefer something that already speaks pellets + element.
- If vanilla G6 type exists in AllGear, cloning it is ideal for DNA; still assign **new** GearInfo (do not overwrite G6 id/APIName).
- Plugin: GUID `sparroh.arrestwarrant`, MycoMod **IsSandbox**.
- Persistence: stable gear id; register before `PlayerData.OnAwake` AddGear.
- Working gear id band: pick free high-range id at impl (confirm unused vs AMR 87421 / Final Judgement / other Sparroh ids).
- Display name **Arrest Warrant**; TextBlocks for UI.

### 14.2 Behaviour host
ArrestWarrantBehaviour (or true Gun subclass when prefab exists):
- WeaponData: warrant duration/potency/charges, flush flags, brace DR/move, hero state, pocket state, pursuit params, serve mark params, geometry mode, double-load mag, input claim flags
- Runtime: warrant end time, charge count, hero armed, pocket used, absorb bank, slick list, serve target ref
- Prefab snapshot restore on upgrade Remove

### 14.3 Warrant pipeline

```
OnReloadComplete (owner, gear is AW):
  GrantOrRefreshWarrant(baseline + card modifiers)
  if AcerbicMandate: SpawnAcidExplosion(player/muzzle)
  if Brace pulse cards: ApplyShortDR
  PlayNotarizeVFX()

OnWeaponSwap(from AW → other):
  if HotPursuit && Warrant active: StartPursuitWindow(other)
  Keep Warrant timer running (unless card spends it)
  Clear while-holding Brace effects

OnDamageDealt (source is primary/melee/grenade):
  if Warrant active: apply potency mult / Probable Cause once flags

OnUpdate:
  Tick Warrant expiry
  Tick Pursuit
  Tick Brace while-holding if current gear is AW
  Tick Hold-R channels
```

Harmony / hooks likely:
- Gun reload complete / ammo refill end
- Player gear swap / equip
- Damage pipeline OnDamageTarget (Warrant mult)
- Hold-R detect (reload button held) — careful vs normal reload
- RMB / aim button for Serve / Ordinance toggle
- Low HP threshold for Melodramatic
- Optional grenade throw/damage hooks for Grenade Annex

### 14.4 Hold R / RMB priority (draft)

**Hold R (while AW equipped):**
1. **Secret Pocket** if dry (mag+reserve empty) and pocket available  
2. **Hold the Line** absorb channel if equipped and mag not empty  
3. **Hold-R Gutter** acid hose if equipped  
4. Else: nothing (don’t block normal reload start on tap)

**RMB (while AW equipped):**
1. **Street Ordinance** Fan/Choke toggle if exotic equipped  
2. **Serve** mark if Hot Pursuit and Warrant active  
3. Else: ADS if enabled / nothing  

**Tap R:** always reload / notarize path; never steal tap for specials.

### 14.5 HUD
- Warrant timer bar (critical readability)
- Warrant charge pips (Stacked Charges)
- Pursuit short buff pip
- Melodramatic “HERO ARMED” tell
- Pocket available icon when dry
- Fan/Choke mode icon
- Prefer SparrohUILib if dependency acceptable

### 14.6 VFX / audio priority
1. Notarize stamp readability  
2. Warrant active tell on player (doesn’t clog other gun VFX)  
3. Baseline blast acid punch  
4. Acerbic reload boom  
5. Pursuit swap whoosh  
6. Hero shot sting  
7. Pocket load  

### 14.7 Multiplayer
- IsSandbox; identical mod on all clients  
- Warrant owner-authoritative  
- Serve mark / acid boom owned by firer  
- Cap slick FX  
- Do not require other players to own AW for your Warrant to buff your primary  

### 14.8 Interaction with other Sparroh mods
- **HeavyAmmoRegen** — playtest stamp cadence; don’t ignore free heavy ammo
- **LoadoutHeavy / DedicatedWeaponKeys / EquipAllWeapons** — swap loop benefits; ensure equip hooks still notarize correctly
- **SalvoMacro / GrenadeMacros** — Grenade Annex fun; watch ICD
- **DisplayGunStats / ShowBoostValues** — expose Warrant mult if possible
- **Sparroh’s Turbocharges** — future turbo hooks on exotics optional
- **Caduceus / Final Judgement / Zephyr** — no system collision expected; different verbs

---

## 15. Deliberate Non-Goals

- Not a vanilla G6 Street Sweeper replace/rework patch  
- Not “just buff G6 stats”  
- Not Zephyr primary cone  
- Not Caduceus team medic  
- Not Final Judgement charge nuke  
- Not Globbler acid primary  
- Not permanent always-on kit aura  
- Not requiring custom Unity prefab for v1 (runtime clone OK)  
- Not baseline team Warrant share  
- Not full-auto identity  

---

## 16. Open Tuning Questions (playtest, not design blockers)

1. Baseline Warrant +18–25% / 4s vs +30% / 3s — which teaches swap better?  
2. Mag 1 default reserves 6 vs 8?  
3. Falloff max mult 0.3 vs 0.4 (how insulting is edge range)?  
4. Does grenade damage mult feel good or should Warrant give charge regen only?  
5. Hot Pursuit: speed vs first-hit amp vs both?  
6. Street Ordinance: RMB toggle vs two separate rares + one exotic?  
7. Double Loading: +1 or +2 mag?  
8. Should baseline Warrant include move speed crumb (probably no)?  
9. Melodramatic heal amount vs vanilla 12–14 band?  
10. Secret Pocket once per mission vs once per empty with long ICD?  
11. Clone base gun after AllGear audit (is G6 present as type/name)?  
12. Auto-unlock vs progression unlock?  
13. Does notarize require empty mag reload only, or any reload complete? (Prefer **any reload complete** so partial/Double Loading still works.)  
14. Hold-R Gutter in backlog vs ship — Hold R crowded?  
15. Ally-visible Warrant VFX on or off by default?  

---

## 17. Success Criteria / Player Fantasy Checklist

- [ ] Baseline blast → reload → Warrant → swap → licensed primary feels complete with zero upgrades  
- [ ] Sitting on AW all mission feels wrong unless Flush/Brace lean  
- [ ] License to Kill alone makes swap fantasy obvious  
- [ ] Hot Pursuit makes swap-away a highlight moment  
- [ ] Acerbic Mandate makes reload a combat verb beyond notarize  
- [ ] Street Ordinance Fan/Choke both readable and useful  
- [ ] Melodramatic Hero clutch is memorable and not farmable every 5s  
- [ ] Secret Pocket Hold R saves a wipe once and still notarizes  
- [ ] Kinetic Absorbers plant is optional Brace fantasy, not mandatory tax  
- [ ] Double Loading comfort doesn’t erase notarize  
- [ ] Hybrids (Acid Badge, Panic Paperwork) feel intentional  
- [ ] Frozen 30 ships clean  
- [ ] Name **Arrest Warrant** nowhere overwrites vanilla G6  
- [ ] Distinct from Zephyr / Caduceus / Final Judgement / Globbler in play  
- [ ] Humor present in blurb; gunfeel stays industrial shotgun  
- [ ] Warrant HUD is readable in chaos  

---

## 18. Review Decisions Locked (2026-08-14)

| Decision | Lock |
|----------|------|
| Form factor | Short-range acid authorization shotgun (heavy) |
| Player-facing name | **Arrest Warrant** |
| APIName / GUID | `arrest_warrant` / `sparroh.arrestwarrant` |
| Slot | Heavy |
| vs vanilla G6 | Parallel cousin — not a replace/rework patch |
| Paths | License / Flush / Brace |
| Baseline Warrant | Always on after notarize (reload) |
| Empower targets | Per-upgrade: any of primary / melee / grenade / body / heavy / swap / expire |
| Mag | 1 default, not sacred |
| Inputs | R notarize; Hold R + RMB path claims |
| Element | Acid baseline |
| Tone | SAXON badge / gallows humor |
| Crowns | License to Kill, Hot Pursuit, Acerbic Mandate, Street Ordinance, Melodramatic Hero, Secret Pocket |
| Ship pool | Frozen 30 listed above |
| Product shape | Parallel weapon |
| Doc file | ArrestWarrant-DesignDoc.txt |

---

## 19. Changelog

### v1 (2026-08-14)
- Initial full design from locked user decisions
- Name: **Arrest Warrant** (parallel to G6 Street Sweeper fantasy, distinct product)
- Paths: License (swap battery), Flush (acid street), Brace (survivor/hero/pocket)
- Sacred baseline Warrant on reload notarize
- Empower targets flexible per card; R / Hold R / RMB as effect triggers
- Mag 1 default not sacred
- Research anchors:
  - Wiki: G6 Street Sweeper stats + 10 upgrades (License to Kill, Acerbic Flush, Kinetic Absorbers, Melodramatic Hero, Secret Pocket, G6 Choke, Corn Cob, Crowd Control, Double Loading, Driver Update)
  - Sibling docs: Final Judgement (parallel heavy bible structure), AMR (reload-arms-power cousin), Caduceus (support contrast), Manifold/Zephyr/HLC (path vocabulary patterns)
  - User locks: parallel; baseline Warrant OK; any empower target; R/HoldR/RMB triggers; mag not sacred; name Arrest Warrant; no hard no’s; full bible
- Frozen 30 + backlog + impl pipeline notes

---

## 20. Implementation checklist (post-design)

- [ ] Rename plugin/csproj/thunderstore from template → ArrestWarrant
- [ ] ArrestWarrantBehaviour.Data fields from §14.2
- [ ] Clone pellet/acid-capable base; new GearInfo (do not overwrite G6)
- [ ] Baseline Blast + mag 1 + notarize Warrant on reload complete
- [ ] Warrant damage mult hook on primary/melee/grenade
- [ ] HUD: Warrant timer (+ charge pips later)
- [ ] License: License to Kill, Hot Pursuit, Stacked Charges, Cross-Clearance, …
- [ ] Flush: Acerbic Mandate, Street Ordinance, Caustic Broom, geometry rares
- [ ] Brace: Melodramatic Hero, Secret Pocket Hold R, Kinetic Absorbers
- [ ] Double Loading mag comfort
- [ ] UpgradeRegistration frozen 30
- [ ] Hold R / RMB priority table
- [ ] Persistence + SpawnGear stamp
- [ ] Playtest pass on §16 knobs
- [ ] Verify no vanilla G6 overwrite
- [ ] Verify baseline loop teaches swap without path investment
- [ ] Verify AW body DPS stays honest vs licensed primary dump

---

## 21. One-Page Summary

**Arrest Warrant** is Mycopunk’s missing **swap-fodder heavy**.

Short-range acid shotgun. Mag usually one. Every reload **notarizes** a **Warrant** — a timed buff that makes your primary, melee, and grenades meaner. You flash the badge, swap out, and let the kit you already love finish the arrest.

**License** thickens the paperwork (potency, stacks, on-swap Pursuit, Serve marks).  
**Flush** makes the broom real (Acerbic reload booms, Fan/Choke geometry, acid slicks).  
**Brace** keeps you alive to file it (DR plant, Melodramatic hero shot, Secret Pocket Hold R).

Vanilla G6 Street Sweeper stays in the game unfinished; this is a parallel SAXON catalog entry with a complete ~30 upgrade grid and a fantasy the lonely “License to Kill” epic always wanted to be.

**Blast. Notarize. Swap. Authorized.**
