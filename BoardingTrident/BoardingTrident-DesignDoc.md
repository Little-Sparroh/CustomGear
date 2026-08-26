# Boarding Trident – Design Document (v1)

## 1. High Concept / Fantasy

A pirate boarding rifle that rakes the deck on the move and stakes the mast when you sight true.

Hipfire sweeps **horizontal** prongs across the crew. ADS drops a **vertical** stake through stacked chitin. One doctrine card makes hip a grape battery and ADS a lone cannon. Mythic toys forge every prong into a **cutlass blade**, turn the barrel into a **clockwork screw** that cycles horizontal and vertical power as it spins, and open the **bilge** or call a **wet squall** — chemistries you choose, never ones the gun forces on you.

**One-liner:** Rake the deck, stake the mast — and when the screw turns, the whole clock is yours.

**Product shape:** New parallel primary (**Boarding Trident**). Does **not** replace vanilla Trident S2 / WideGun.


## 2. Role & Fantasy in the Arsenal

- **Slot:** Primary
- **Range:** Mid (close-mid clear on hip; mid precision on ADS)
- **Role:** Dual-stance multi-prong rifle — volume ↔ stake, with optional blade replace, spin clock, and dual chemistry (acid bilge / wet-shock storm)
- **Gap filled:** Vanilla Trident’s upgrades mostly change bullet count. Barrel flip, spin, corrosion, and water are under-used or poorly looped. Boarding Trident makes **switching**, **orientation**, and **chosen chemistry** the skill expression.
- **Synergies:** Shock offhands / grenades (Powderwake, Squall); acid grenades optional but not required for Bilge; movement (slide, Powderwake); co-op wet setup for team shock

**Not trying to be:** pure minigun, pure sniper, forced one-axis specialist, or a fire weapon.


## 3. Design Pillars

1. **Switching is the baseline skill** — hip ↔ ADS are both first-class; builds improve the dialogue, not delete a stance.
2. **One Doctrine exotic owns Broadside + Lone Cannoneer** — dual-mode in a single card, not two path prisons.
3. **Blades replace shots** — damage = sum of pellets that would have fired; one blade per full shot; pierce same-brain only; upgrade-gated.
4. **Three gravity wells elsewhere** — Cutlass / Screw / Tide (not “minigun path vs cannoneer path”).
5. **Two chemistries, clean split:** Bilge = Acid (player-gated Seacock); Storm = Wet → Shock (water lob, HF, Powderwake). Fire is minor/backlog.
6. **Player agency on self-acid** — Hold R opens Seacock; never auto-corrode from M1 alone.
7. **Screw clock dual-dips axis buffs** — as the barrel spins through quadrants, Horizontal and Vertical tagged bonuses both pay every revolution.
8. **~30 ship upgrades**; exotic shapes larger than others; each exotic same cell count; full naval/pirate rename pass.
9. **Cadence cards work on both stances** — Squadron / Gunner’s Precision echoes do not force hip-only or ADS-only fire modes.


## 4. Core Mechanics & Gunfeel

### 4.1 Base gun (no upgrades)

| Trait | Value / intent |
|---|---|
| Fire mode | Fully automatic projectile rifle |
| Bullets per shot | 3 |
| Hipfire rake | **Horizontal** (deck sweep) |
| ADS rake | Barrel rotates; **Vertical** (mast stake) |
| Damage | Modest per pellet; volume from prongs (Trident-like ballpark) |
| Mag / reserve | Keep a mag + reload loop (“reload the battery”) |
| Blades | Off |
| Doctrine dual-mode | Off (both stances stay 3-prong) |
| Screw clock | Off |
| Seacock | Off |
| Elements | None innate |
| Model / audio | Borrow vanilla Trident / WideGun until custom art |
| Gun crab parent | Barrel (vanilla pattern) |

**Teaching without stickers:** muzzle flash / prong VFX stretched on the active combat axis so hip vs ADS is readable in ~2 seconds.

**Optional baseline juice (easy cut):** after hip↔ADS swap, next shot has slightly tighter prong spacing only (no damage mult).

### 4.2 Why hip = horizontal, ADS = vertical

Vanilla WideGun is the opposite (hip vertical, ADS horizontal). Boarding Trident **deliberately flips** this for boarding fantasy:

- Hip = run the deck, sweep wide crews (horizontal grape)
- ADS = pin tall stacks, limb columns, masts (vertical stake)

Document in flavor text so players are not gaslit against muscle memory from vanilla Trident.

### 4.3 Inputs

| Input | Role |
|---|---|
| **M1** | Fire |
| **RMB / Aim** | ADS posture (vertical rake when clock off; zoom / doctrine stake face) |
| **R tap** (< ~0.25s) | Reload |
| **R hold** (≥ ~0.25s) | **Open Seacock** when a Bilge card grants the verb; else no hold-R behavior |
| Sprint / slide | Standard; movement payoffs are upgrade-owned |

### 4.4 Combat axis (shared framework)

```
enum ProngAxis { Horizontal, Vertical }

GetCombatAxis():
  if Screw enabled AND spin01 >= clockThreshold:
      return ClockAxis(fireAngle)     // see §5 Screw
  else:
      return IsAiming ? Vertical : Horizontal
```

- **Aim posture** and **combat axis** are related but not identical once Screw is online (see §5).
- Axis-tagged upgrade riders read `GetCombatAxis()` at shot time.

### 4.5 Base combat loop (no upgrades)

```
Hip M1 → horizontal triple rake across packs
ADS M1 → vertical triple rake on tall targets / stacked parts
Swap freely — both stances are complete
Reload on R when the battery runs dry
```

Skill without upgrades: pick the rake that matches the enemy shape; don’t face-tank; manage mag.


## 5. Shared Vocabulary

### 5.1 Axis
- Default: Hip → Horizontal, ADS → Vertical
- Under Screw (spin ≥ threshold): **clock sector** owns H/V for cut direction + axis-tagged buffs
- Hooks: on-swap effects, “fired both axes this mag,” cross-deck empower
- **Forbidden:** cards that only work if you never ADS or never hip

### 5.2 Prongs
- Pellet count, spacing, per-pellet mults
- Volley construction happens first; blades and ammo accounting use the **finished volley**

### 5.3 Doctrine (single exotic — dual mode)
| Posture | Effect |
|---|---|
| **Hip** | Multi-prong broadside volume (grape / extra pellets + spacing) |
| **ADS** | Collapse to **1** high-damage prong (lone cannon stake) |

Aim-owned. Screw does **not** override Doctrine’s multi vs single — only cut axis / axis tags.

### 5.4 Blades (upgrade-gated)
When blade mode is active on a shot:

```
build volley as normal (doctrine, bps, mults, cadence)
pelletsWouldFire = finished volley
bladeDamage = sum(damage of every pellet in that volley)
ammoCost = same total ammo as the volley replaced
suppress pellet spawns
spawn ONE blade:
  damage = bladeDamage
  orientation = continuous cut angle
      (hip/ADS axis angle when clock off;
       fireAngle when Screw clock on)
  pierce = same brain only (parts / shells / core on one EnemyBrain)
do not pierce additional brains / multi-enemy chains on baseline blade rules
```

- **1 blade per full shot** (including spun shots)
- Supports may add pierce count, blade thickness, on-pierce payoffs — still same-brain unless a future exotic explicitly says otherwise (none in v1)

### 5.5 Bilge (Acid)
- Apply acid to enemies via Tide cards
- **Seacock:** player-controlled self-acid stance (Hold R)
- Exploit cards: +damage / blade length / apply rate while target corroded and/or while Seacock open
- **Never** open Seacock from passive M1 fire alone

### 5.6 Storm (Wet / Shock)
- Water projectiles apply **Wet** only (not acid setup)
- Shock payoffs, smites (Heaven’s Fury rewrite), Powderwake (Tailwind rewrite)
- Wet targets: amplified shock / smite preference / chain eligibility
- **Water does not accelerate acid saturate**

### 5.7 Fire
- Not a Tide pillar
- At most backlog; omit from frozen 30


## 6. Seacock (player-controlled self-acid)

**Granted by:** Black Bilge and/or a smaller Bilge rare that teaches the verb. Without a granting card, Hold R does nothing special.

| Piece | Behavior |
|---|---|
| Input | **Hold R** ≥ ~0.25s (tap R still reloads if threshold not met) |
| Effect | Apply self-acid saturation; start **Bilge stance** window |
| Window | Draft 2.0–3.5s (card-tuned); HUD stack pip |
| During stance | Card-defined: +damage, +acid apply on prongs/blades, etc. |
| Tax | Normal player acid DoT while saturated — readable cost |
| Re-open | After window ends; optional cooldown or bilge charges from corroding enemies (card-owned) |
| Pump (optional on crown) | Tap R during stance vents acid ring / coats nearby and may end or refresh stance |

**Priority with reload:** same pattern as Blood Carver Iron Snare — short press = reload intent; long press = Seacock when available.

**Non-goal:** automatic self-corrode on continuous fire, reload, or spin redline without player arming Seacock.


## 7. Path B deep dive — Orlop Screw & the Clock

### 7.1 Fantasy
Keep the barrel turning until horizontal and vertical investments both pay every revolution. Spin is not “hip minigun only” — it is a **clock that dual-dips axis buffs**.

### 7.2 Clock mapping

Barrel roll `fireAngle` (0–360°), sampled at shot instant:

| Sector | Clock | Combat axis |
|---|---|---|
| 90° → 180° | **3 → 6** | **Horizontal** |
| 180° → 270° | **6 → 9** | **Vertical** |
| 270° → 360° | **9 → 12** | **Horizontal** |
| 0° → 90° | **12 → 3** | **Vertical** |

```
        12 V
         |
  9 H ----+---- 3 H
         |
        6 V
```

Alternating 90° quadrants: H, V, H, V.

**Art offset:** if rest pose is not “0 = up,” apply a constant offset so **visual prongs** and **logic sectors** agree. Design intent is 3–6 H, 6–9 V, etc.

```csharp
// Reference logic (offset applied in impl)
ProngAxis ClockAxis(float fireAngleDeg) {
    float a = fireAngleDeg % 360f; if (a < 0) a += 360f;
    int q = (int)(a / 90f); // 0..3
    return (q % 2 == 0) ? ProngAxis.Vertical : ProngAxis.Horizontal;
}
```

### 7.3 Spin thresholds (draft)

| Spin 0–1 | Behavior |
|---|---|
| 0.00 – 0.25 | Clock **off**; hip/ADS axis only; mild RoF approach |
| 0.25 – 0.75 | Clock **on**; sector buffs scale with spin01 |
| 0.75 – 1.00 | Full clock; max RoF mult; full sector buffs; redline screw VFX |

- Holding fire raises spin (duration-tuned, Shredify DNA)
- Releasing fire drops spin faster than rise (brief stutter should not hard-reset if a support says so)
- Stow: spin decays (continue or faster — prefer continue mild decay)

### 7.4 Aim posture vs clock cut (split of concerns)

| State | Owns combat axis (cut + H/V tags) | Aim still owns |
|---|---|---|
| Screw off or spin < threshold | Hip→H, ADS→V | FOV, aim spread/range, Doctrine multi vs single, both-stance cadence |
| Screw on, spin ≥ threshold | **Clock sector** | FOV, aim spread/range, Doctrine multi vs single, both-stance cadence |

**Feature, not bug:**
- ADS + Horizontal sector = single fat prong/blade on a **horizontal** line (lone cannon posture, deck-wise cut)
- Hip + Vertical sector = grape on a **vertical** line

Posture ≠ cut angle while the screw sings.

### 7.5 Continuous vs discrete

1. **Rake / blade orientation** follows continuous `fireAngle` (smooth line; vanilla ConstRotation already rotates spread).
2. **Axis-tagged buffs** use discrete sector (H or V).
3. Out-of-sector axis tags: **hard 0** (readable). Untagged / AxisBoth always apply.

### 7.6 Sector edge juice (“bells”)

When fireAngle crosses 12 / 3 / 6 / 9:
- Light audio spark (baseline with Orlop Screw)
- **Eight Bells** (rare/epic): on sector cross, next shot +damage or tiny ammo refund

### 7.7 Doctrine × Screw

Doctrine remains **aim-based**. Screw does not eat it.

| | Hip posture | ADS posture |
|---|---|---|
| H sector | Grape, horizontal cut | Single stake, horizontal cut |
| V sector | Grape, vertical cut | Single stake, vertical cut |

### 7.8 Blades × Screw

One blade per full shot; damage = volley sum; angle = `fireAngle`; sector riders apply; pierce same-brain only.

### 7.9 Cadence × Screw

Burst / semi / damage-RoF trades (Squadron & Gunner’s Precision echoes) apply on **both** stances with no extra Screw rules.


## 8. Upgrade Paths (gravity wells — hybrids intended)

### Path A — CUTLASS (blades & prong geometry)
**“All three teeth become one cut.”**

- **Spine:** blade replace, pierce, slash thickness, swap → cross-cut, spacing
- **Crown:** **Boarding Cutlass**
- **Hybrid hooks:** Doctrine → hip cleaver / ADS stake blade; Screw → rotating guillotine

### Path B — SCREW (spin + clock)
**“Keep the barrel turning until both faces of the clock pay out.”**

- **Spine:** spin meter, RoF, sustain, sector dual-dip, bells
- **Crown:** **Orlop Screw**
- **Hybrid hooks:** multiplies whatever volley Doctrine/Cutlass built; horizontal **and** vertical toys both matter

### Path C — TIDE (Bilge acid + Storm wet/shock)
**“Choose the bilge or the squall — both are the sea.”**

| Verb | Toys |
|---|---|
| **Bilge** | Acid apply, Seacock, rot exploit, acid blades, pump vent |
| **Storm** | Water lob (wet), Heaven’s Fury smites, Powderwake, shock prongs |

- **Crowns:** **Black Bilge** + separate **storm** cards (Bucket and HF stay **two** upgrades)
- Fire demoted
- Hybrid: wet clear + bilge ST on one grid is encouraged

### Glue — Doctrine & staples
- **Privateer’s Doctrine** (Exotic) — path-flexible dual Broadside/Cannoneer
- Economy, range, grid, both-stance cadence supports

### Path × stance matrix (switch-friendly)

```
                 CUTLASS                SCREW                  TIDE
Hip (default H) wide cleaver blade     spun hose + clock      acid/wet deck sweep
ADS (default V) vertical stake blade   spun stake + clock     acid/shock mast pin
Clock sectors   H/V blade riders       CORE FANTASY           H/V elemental riders
On swap         cross-deck empower     optional spin juice    optional seacock tech
Doctrine        cleaver ↔ stake        multi↔single spun      multi↔single elemental
```


## 9. Crowns & Sacred Systems

### Privateer’s Doctrine — Exotic (Glue keystone)
- **Hip:** extra prongs / broadside spacing; per-pellet damage adjusted so grape is volume-forward
- **ADS:** single prong; large damage mult (lone cannon)
- One card replaces the need for separate Broadside + Lone Cannoneer path crowns
- Composes with blades (sum includes doctrine volley) and Screw (posture independent of clock axis)

### Boarding Cutlass — Exotic (Cutlass crown)
- Shots become energy blades (replace pellets)
- Damage = sum of volley; 1 blade/shot; pierce same-brain
- Cut angle from GetCombatAxis continuous representation / fireAngle under Screw
- Swap: next blade empowered **cross-deck** once (bonus damage or brief dual-axis thickness)

### Orlop Screw — Exotic (Screw crown)
- Continuous fire spins up barrel (Shredify DNA)
- Clock on above threshold; H/V sector buff dual-dip
- RoF scales with spin; spread pattern rotates with fireAngle
- Soft ammo hunger or mild spread at redline optional (tune so uptime stays fun)

### Black Bilge — Exotic (Tide / Bilge crown)
- Grants Seacock (Hold R)
- While Seacock open: strong +damage and prongs/blades apply heavy acid
- Optional Pump: tap R during stance → acid vent ring

### Heave the Bucket — Exotic or Epic (Storm — water only)
- On reload (empty-mag preferential, Bailing Water DNA): lob water ball
- Applies **Wet** in radius; **not** acid
- Reload speed rider

### Skywrath Broadside — Exotic or Epic (Heaven’s Fury rewrite — own card)
- Dealing damage has a chance to smite area with shocking lightning
- Smites **prefer wet** targets; soak/wet application chance retained in shock family
- Remains separate from Heave the Bucket

### Powderwake — Epic or Exotic (Tailwind rewrite)
- Electrocuting a target (rules: any source vs other-weapon — prefer **any shock saturate you cause or ally causes**, tune abuse) grants Powderwake stacks
- Consume stacks while firing Boarding Trident to move faster
- Fed naturally by Skywrath and shock loadouts

### Boundary Incursion — Oddity
- Grid grow — keep


## 10. Full Upgrade List (~30 ship + backlog)

Rarity: Standard / Rare / Epic / Exotic / Oddity  
Tags: C Cutlass · S Screw · T Tide · D Doctrine/Glue · B Bilge · W Storm  
Cell rule: Exotics larger; all Exotics same cell count.  
Names are player-facing (full rename). Vanilla echoes only in fate table.

------------------------------------------------------------------------------
PATH A — CUTLASS                                           [~8]
------------------------------------------------------------------------------

A1. Boarding Cutlass — Exotic (Keystone)
    Shots become axis blades. Damage = sum of replaced volley.
    Pierce along slash, same brain only. 1 blade per shot.
    After hip↔ADS swap, next blade is cross-deck empowered.

A2. Piercing Keel — Rare
    +Blade pierce count (additional parts on same brain).
    Minor +blade damage.

A3. Cross-Deck Cleaver — Epic
    On stance swap: gain a short window where the next shot
    (blade or prongs) deals bonus damage and applies slight
    stagger. Stacks cleanly with Cutlass cross-deck.

A4. Grape Lattice — Rare
    +Horizontal spacing / width when combat axis is Horizontal.
    (Pays under hip default and Screw H sectors.)

A5. Mast Nails — Rare
    +Vertical spacing / height when combat axis is Vertical.
    (Pays under ADS default and Screw V sectors.)

A6. Torpedo Prongs — Epic
    +Base bullets per shot (both stances) before Doctrine collapse.
    Blade sum grows automatically.

A7. Powder Finale — Rare (Gunpowder Finale rewrite)
    Last shot in mag deals more damage (prongs or blade).

A8. Keelhaul Edge — Standard
    Minor universal damage. Filler cutlass steel.

------------------------------------------------------------------------------
PATH B — SCREW                                             [~8]
------------------------------------------------------------------------------

B1. Orlop Screw — Exotic (Keystone)
    Spin-up on continuous fire. Clock sectors own H/V tags
    above threshold. RoF scales with spin. Barrel angle
    rotates rake/blade continuously.

B2. Eight Bells — Epic
    Crossing a clock bell (12/3/6/9) empowers the next shot
    (+damage and tiny ammo refund). Audio “bell” feedback.

B3. Screw Momentum — Rare
    Spin decays slower on brief trigger release; faster spin-up
    after kills.

B4. Magazine Screw — Rare
    +Ammo capacity / mag; spin redline slightly less hungry.

B5. Ungovernable Screw — Rare (Ungovernable rewrite)
    +Fire rate, +recoil, +spread — wild battery. Stronger while
    spin ≥ 0.75.

B6. Bearing Grease — Standard
    Slightly faster spin-up; slightly tighter spread while spinning.

B7. Full Revolution — Epic
    After a complete 360° while firing, next shot chains a free
    echo tick at half damage along the same cut (prong or blade).

B8. Orlop Reserves — Standard
    +Magazine size.

------------------------------------------------------------------------------
PATH C — TIDE                                              [~10]
------------------------------------------------------------------------------

-- Bilge --

C1. Black Bilge — Exotic (Keystone)
    Hold R opens Seacock (self-acid stance). While open:
    +damage and prongs/blades apply heavy acid. Optional Pump vent.

C2. Barnacle Rounds — Rare (Acid Shot rewrite)
    Prongs/blades apply acid. Multi-pellet volleys apply acid
    more reliably (per-volley bias, not per-pellet spam abuse).

C3. Copper Poison — Rare (Rot Advantage rewrite)
    +Damage vs corroded targets. Blades gain slight thickness
    vs corroded parts on the same brain.

C4. Open Seacock — Rare
    Grants Seacock verb if Black Bilge not present (weaker window).
    With Black Bilge: +Seacock duration and shorter re-open.

C5. Bilge Rat — Epic
    While Seacock open: move slightly faster; acid apply rate up.
    Taking acid damage from self refreshes a tiny portion of
    Seacock (still player-opened only).

-- Storm --

C6. Heave the Bucket — Exotic (Bailing Water rewrite)
    Reload lobs a water ball (wet only). +Reload speed.
    Separate from smite card.

C7. Skywrath Broadside — Exotic (Heaven’s Fury rewrite)
    Chance on dealing damage to smite with shock lightning.
    Prefers wet targets; may apply wet/soak on smite.

C8. Powderwake — Epic (Tailwind rewrite)
    Shock saturates grant stacks; consume while firing for
    move speed.

C9. Saltpetre Primer — Rare
    Prongs have a chance to apply shock. Bonus chance vs wet.

C10. Riding the Squall — Standard
    Minor move speed while a Powderwake stack is available
    or shortly after a smite you caused.

------------------------------------------------------------------------------
DOCTRINE / GLUE / GUNFEEL                                  [~10]
------------------------------------------------------------------------------

G1. Privateer’s Doctrine — Exotic (Keystone)
    Hip: broadside multi-prong volume.
    ADS: single high-damage stake.
    Both-stance identity in one card.

G2. Ridging Burst — Rare (Squadron rewrite)
    Fire a short burst. +Mag slightly.
    Works on **both** hip and ADS (no stance lock).

G3. Hangfire Discipline — Rare (Gunner’s Precision rewrite)
    +Damage, −fire rate, firing no longer automatic (semi).
    Works on **both** stances.

G4. Spyglass Deck — Rare (Spyglass rewrite)
    +Aim zoom. While aiming: −fire rate, −spread, +range.
    Does not disable hip fantasy.

G5. Line the Rails — Standard (Line er’ Up rewrite)
    +Damage while aiming.

G6. Cargo Hold — Standard
    +Ammo reserves.

G7. Compressed Grape — Standard (Compressed Ammunition)
    +Magazine size.

G8. Compatible Shot — Rare (Compatible Ammunition)
    Kills refund some ammo.

G9. Plunder the Core — Epic (Plunder rewrite)
    Core kills have a chance to refund a large amount of ammo.

G10. Cross-Deck Slide — Epic (Cross-Deck rewrite)
    Slide speed briefly increased after killing a target.

G11. Thrill of Boarding — Epic (Thrill of the Fight rewrite)
    Core kills grant charge to the ability closest to full.

G12. Bottom Heavy Battery — Rare (Bottom Heavy rewrite)
    +Ammo reserves; +recoil.

G13. Top Deck Storage — Rare
    +Mag and reserves.

G14. Discount Marksman — Standard
    +Aim zoom.

G15. Energy Capstan — Rare (Energy Condenser rewrite)
    +Fire rate but must charge briefly before shooting.
    Both stances.

G16. Boundary Incursion — Oddity
    +Upgrade grid size.

G17. Tread the Line — Epic
    +Damage per rare cell surrounding this upgrade;
    +recoil per other adjacent cell. (Grid-skill toy; keep DNA.)

------------------------------------------------------------------------------
FROZEN v1 SHIP POOL (exactly 30)
------------------------------------------------------------------------------

  EXOTIC (7)
    1  Privateer’s Doctrine      (D)
    2  Boarding Cutlass          (C)
    3  Orlop Screw               (S)
    4  Black Bilge               (T/B)
    5  Heave the Bucket          (T/W)
    6  Skywrath Broadside        (T/W)
    7  Powderwake                (T/W)  — if exotic budget tight, demote to Epic #11 and promote Eight Bells; prefer Powderwake as Epic below

Recommended exotic six + Powderwake epic:

  EXOTIC (6)
    1  Privateer’s Doctrine
    2  Boarding Cutlass
    3  Orlop Screw
    4  Black Bilge
    5  Heave the Bucket
    6  Skywrath Broadside

  EPIC (8)
    7  Powderwake
    8  Cross-Deck Cleaver
    9  Eight Bells
    10 Full Revolution
    11 Torpedo Prongs
    12 Bilge Rat
    13 Plunder the Core
    14 Cross-Deck Slide

  RARE (10)
    15 Piercing Keel
    16 Grape Lattice
    17 Mast Nails
    18 Screw Momentum
    19 Magazine Screw
    20 Barnacle Rounds
    21 Copper Poison
    22 Open Seacock
    23 Ridging Burst
    24 Hangfire Discipline

  STANDARD (5)
    25 Keelhaul Edge
    26 Bearing Grease
    27 Cargo Hold
    28 Compressed Grape
    29 Line the Rails

  ODDITY (1)
    30 Boundary Incursion

BACKLOG (designed, expand later)
  Ungovernable Screw, Orlop Reserves, Powder Finale, Saltpetre Primer,
  Riding the Squall, Spyglass Deck, Compatible Shot, Thrill of Boarding,
  Bottom Heavy Battery, Top Deck Storage, Discount Marksman, Energy Capstan,
  Tread the Line, Eight Bells→ if cut from epic, Multiversal/Edge Fault parity,
  Fire-in-the-Hold echo (fire — only if needed), Hornet Hunting joke card

------------------------------------------------------------------------------
CUT / DEMOTE FROM VANILLA TRIDENT IDENTITY
------------------------------------------------------------------------------

| Vanilla | Fate |
|---|---|
| Broadside + Lone Cannoneer as separate cards | **Fused** into Privateer’s Doctrine |
| Hip vertical / ADS horizontal | **Flipped** on Boarding Trident |
| Shredify | Orlop Screw + clock dual-dip (much deeper) |
| Acid Shot / Rot Advantage / Corrosive Reaction | Barnacle / Copper / Seacock (player-gated) |
| Bailing Water → vague utility | Heave the Bucket = **wet only** (storm) |
| Heaven’s Fury | Skywrath Broadside — **own card**, wet-preferring |
| Tailwind | Powderwake |
| Fire Shot / Fire in the Hold | **Backlog / omit** frozen 30 |
| Gunner’s Precision / Squadron | Hangfire / Ridging Burst — **both stances** |
| Axis lock-in path design | **Rejected** |
| Auto self-corrode | **Rejected** |
| Blades as bonus pellets | **Rejected** — replace shot, sum damage |


## 11. Example Builds

### Doctrine privateer (baseline poster)
Privateer’s Doctrine → Ridging Burst or Hangfire Discipline → Cargo Hold → Compressed Grape → Line the Rails → Compatible Shot (backlog) / Plunder  
*Hip grape, ADS stake — constantly flipping posture.*

### Cutlass corsair
Boarding Cutlass → Privateer’s Doctrine → Piercing Keel → Cross-Deck Cleaver → Grape Lattice → Mast Nails  
*Cleaver ↔ stake blades; both axis spacing cards pay; swap for cross-deck.*

### Orlop clock (the Screw point)
Orlop Screw → Grape Lattice → Mast Nails → Eight Bells → Magazine Screw → Privateer’s Doctrine  
*Spin once per fight phase; horizontal and vertical toys both proc every revolution. ADS stake in an H sector is a feature.*

### Clock cutlass
Orlop Screw → Boarding Cutlass → Eight Bells → Full Revolution → Piercing Keel  
*Rotating guillotine; bells mark sector pays.*

### Black bilge
Black Bilge → Barnacle Rounds → Copper Poison → Open Seacock → Boarding Cutlass → Bilge Rat  
*Hold R, open the seacock, acid blades on a timer you chose.*

### Deck storm
Heave the Bucket → Skywrath Broadside → Powderwake → Saltpetre (backlog) / Orlop Screw → Ridging Burst  
*Wet the room, smite the wet, surf Powderwake, optional spin.*

### Full pirate freak (hybrid)
Doctrine + Cutlass + Screw + Black Bilge or Bucket+Skywrath  
*Encouraged — no anti-synergy matrix.*


## 12. Strengths, Weaknesses & Risks

### Strengths
- Readable dual stance without path lock-in
- Doctrine is one card, one lesson
- Screw makes investing in **both** axis toys correct
- Blades scale honestly with volley sum (Doctrine and Torpedo Prongs matter)
- Corrosion loop is opt-in and controlled
- Storm loop uses wet→shock truthfully
- Hybrids are intentional

### Weaknesses / fun failure states
- Wrong rake shape for the pack (horizontal into a single tall boss column) — fix by swap or wait for Screw sector
- Dry Seacock timing (opened too early / died with bilge closed)
- Spin dropped before clock threshold in a panic release
- Blade-only clear without pierce investment feels shorter range than bullets
- Storm without wet setup = weaker smites
- Hangfire semi on both stances is a real DPS tax — intentional discipline

### Design risks
- Clock + Doctrine cognitive load — mitigate with barrel VFX + optional HUD pip
- Volley-sum blades + Torpedo Prongs + Doctrine hip grape = huge cleaver numbers — tune blade coefficient if needed (e.g. sum × 0.9) in playtest, not design-down the fantasy
- Same-brain pierce must not become full-scene delete via weird brain linking
- Hold R vs reload buffering on controllers
- Powderwake stack source rules (self-only vs any shock) — start **self-caused shock saturates** including Skywrath
- Six exotics crowding grids — keep shapes identical cell counts


## 13. Success Criteria / Player Fantasy Checklist

- [ ] Hip horizontal vs ADS vertical is obvious within one magazine
- [ ] Switching stances always feels legal and rewarded, never punished by path
- [ ] Privateer’s Doctrine alone makes hip and ADS play differently
- [ ] Boarding Cutlass blades replace pellets and hit like the full volley
- [ ] Blade pierce stays on one brain unless future design says otherwise
- [ ] Orlop Screw clock makes Grape Lattice AND Mast Nails both pay while spinning
- [ ] ADS + horizontal sector single stake feels intentional and strong
- [ ] Seacock never opens without Hold R (or equivalent player control)
- [ ] Heave the Bucket wets; it does not corrode
- [ ] Skywrath and Bucket are separate upgrades and both feel complete
- [ ] Powderwake move-speed surf is readable with stacks HUD
- [ ] Fire is not required for a complete Tide fantasy
- [ ] ~30 upgrades; hybrids work; failure states stay fun
- [ ] Vanilla Trident still exists unchanged beside this weapon


## 14. Universal Truths (Mycopunk alignment)

- Exotic shapes should always be larger than others; each exotic should use the same number of cells.
- v1 targets **~30** upgrades (frozen list above); backlog is real design, not trash.
- Three paths create different build options but **may intermingle** on the grid.
- Full rename for rework identity (naval/pirate voice).
- Prefer verbs: axis, volley sum, blade replace, spin/clock, Seacock, wet, smite, doctrine posture.
- New parallel primary — do not overwrite vanilla WideGun gear id / upgrade pool.


## 15. Vanilla Trident → Boarding Trident Fate Table

| Vanilla name | Boarding Trident name | Path | Notes |
|---|---|---|---|
| (baseline axis) | Flipped H hip / V ADS | — | New gun only |
| Broadside | ⊂ Privateer’s Doctrine (hip) | D | Fused |
| Lone Cannoneer | ⊂ Privateer’s Doctrine (ADS) | D | Fused |
| Shredify | Orlop Screw | S | + clock |
| Gunner’s Precision | Hangfire Discipline | D | Both stances |
| Squadron | Ridging Burst | D | Both stances |
| Acid Shot | Barnacle Rounds | T/B | |
| Rot Advantage | Copper Poison | T/B | |
| Corrosive Reaction | Black Bilge / Open Seacock | T/B | Player-gated |
| Bailing Water | Heave the Bucket | T/W | Wet only |
| Heaven’s Fury | Skywrath Broadside | T/W | Own card |
| Tailwind | Powderwake | T/W | |
| Gunpowder Finale | Powder Finale | C backlog | |
| Spyglass | Spyglass Deck | backlog | |
| Line er’ Up | Line the Rails | G | |
| Cross-Deck | Cross-Deck Slide | G | Move; Cleaver is swap damage |
| Plunder | Plunder the Core | G | |
| Compatible Ammunition | Compatible Shot | backlog | |
| Cargo Hold | Cargo Hold | G | Keep spirit |
| Compressed Ammunition | Compressed Grape | G | |
| Bottom Heavy | Bottom Heavy Battery | backlog | |
| Top Deck Storage | Top Deck Storage | backlog | |
| Ungovernable | Ungovernable Screw | backlog | |
| Energy Condenser | Energy Capstan | backlog | |
| Thrill of the Fight | Thrill of Boarding | backlog | |
| Fire Shot / Fire in the Hold | — | — | Omitted frozen 30 |
| Hornet Hunting | — | — | Joke backlog only |
| Boundary Incursion | Boundary Incursion | G | Keep name |
| Tread the Line | Tread the Line | backlog | |
| Discount Marksman | Discount Marksman | backlog | |
| Increased Load / etc. | folded into mag staples | G | |


## 16. Implementation Notes (for later coding passes)

### Host
- New gear via weapon template pattern (clone WideGun or suitable primary)
- `BoardingTridentBehaviour` (or subclass when prefab exists) holding:
  - doctrine flags / bps overrides
  - blade mode + pierce count
  - spin01, fireAngle, fireAngleSpeed, clock threshold
  - seacock window timers / charges
  - powderwake stacks
  - water lob prefab ref (reuse grenade water pattern from WideGun)
  - smite queue (WideGun smite pattern)

### Axis API
```csharp
enum ProngAxis { Horizontal, Vertical }

ProngAxis GetCombatAxis() {
    if (screwEnabled && spin01 >= clockThreshold)
        return ClockAxis(fireAngle + artOffset);
    return IsAiming ? ProngAxis.Vertical : ProngAxis.Horizontal;
}
```

Axis-tagged properties read axis **at fire time**, not only on Apply().

### Fire pipeline
1. Determine posture (aim) → Doctrine mutates bps / damage profile  
2. Build pellet damage list for the volley  
3. If blade: sum damages, spawn blade, skip pellets  
4. Else: fire pellets with spread along combat axis / fireAngle  
5. Ammo = volley cost either way  

### Hooks
- `OnActiveUpdate` — spin, barrel visual, seacock timers  
- `Fire` / `GetCustomSpread` / `ModifyBulletData` — axis + doctrine + blade  
- `OnBeforeDamage` / `OnDamageTarget` / `OnKillTarget` — copper, plunder, powderwake, smite  
- `OnEffectSaturated` (player) — only for display while Seacock; do not auto-open  
- Hold R — PlayerInput reload hold threshold  
- HUD — Seacock pip, Powderwake stacks, optional clock ring (backlog)

### Vanilla reference
- `WideGun.cs` — barrel, aim bps, ConstRotation spin, Broadside/SingleAim flags, water lob RPC, smite, lightspeed, acid sat damage  
- `WideGunUpgradeFlags` — DoubleHorizontalBullets, SingleAimBullet, ConstRotation  
- Do **not** patch vanilla Trident upgrades for this mod’s pool; register new gear + CreateUpgrade

### Network
- Spin / seacock / doctrine state owner-authoritative with existing gun sync patterns  
- Mark mod `IsSandbox`  
- Unique gear id + upgrade id range

### Registration
- Same flow as weapon template / DMLR / Heat Cycler: AllGear inject, CreateUpgrade, SpawnGear remap, persistence by gear id


## 17. Open Tuning Questions (playtest, not design blockers)

1. Clock threshold 0.25 vs lower (clock from first spin degrees)
2. Blade damage = 1.0 × sum vs 0.85–0.95 × sum for safety
3. Doctrine hip pellet count (+3 vs ×2) and ADS damage mult band
4. Seacock window 2.0–3.5s and Hold R 0.2–0.3s
5. Spin-up duration to 1.0 (Shredify-like 5–7s feel vs snappier boarding pace)
6. Powderwake: self-caused shock only vs any shock on target
7. Skywrath proc rate with high BPS grape (divide by bulletsPerShot like vanilla smite)
8. Whether optional clock HUD ships in v1
9. Exotic count 6 vs 7 if Powderwake must be exotic for fantasy
10. Mag size ballpark vs vanilla 75/450


## 18. Locked Decisions Log

| Decision | Lock |
|---|---|
| Product | New primary **Boarding Trident** (not vanilla replace) |
| Tone | Naval / pirate; names not sacred |
| Hip axis | **Horizontal** |
| ADS axis | **Vertical** |
| Path lock-in to one axis | **Rejected** |
| Broadside + Lone Cannoneer | **One Exotic:** Privateer’s Doctrine |
| Blades | Gated; replace shot; **sum damage**; **1 blade/shot**; pierce **same brain** |
| Seacock input | **Hold R** |
| Self-acid | Player-controlled only |
| Water | Wet / storm only — **not** acid setup |
| HF + Bucket | **Two separate upgrades** |
| Tailwind / HF | Stay in pool; expand wet/shock |
| Fire | De-emphasized; out of frozen 30 |
| Cadence (Squadron / Precision echoes) | **Both stances** |
| Screw clock | 3–6 H, 6–9 V, 9–12 H, 12–3 V (art offset OK) |
| Screw vs aim | Clock owns cut + axis tags; aim owns posture / Doctrine / cadence |
| Sector out-of-tag | Hard 0 |
| Vetoes | Still open for future passes |

### Design changelog

#### v1 (this doc)
- Boarding Trident named and scoped as parallel primary
- Flipped axes; switch-first pillars
- Doctrine fusion; Cutlass / Screw / Tide wells
- Orlop Screw clock-face dual-dip specified
- Blade replace + sum + same-brain pierce
- Seacock Hold R; bilge/storm split; HF and Bucket separate
- Frozen 30 + backlog + fate table + impl notes

#### Prior plan iterations (chat)
- v1 plan: axis-commit paths (rejected)
- v2 plan: switch-first, doctrine fuse, blade rules, locks 1–7
- v3 plan: Screw clock detail, Boarding Trident name, final locks → this doc


## 19. Next Steps After This Doc

1. Review frozen 30 vs backlog cuts; confirm Powderwake epic vs exotic
2. Scaffold gear registration from weapon template (rename Boarding Trident ids)
3. Behaviour host: axis, doctrine, spin/clock, seacock hold-R
4. Boarding Cutlass fire replace + pierce
5. Tide storm (Bucket lob, Skywrath smite, Powderwake stacks)
6. Register frozen 30 CreateUpgrade + patterns
7. Pass flavor strings + icons
8. Playtest: clock readability, blade sum tuning, Seacock feel, doctrine grape/stake balance
