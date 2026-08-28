# Final Judgement — Design Document (v1)

## 0. Locked Decisions (2026-08-14)

| Decision | Lock |
|----------|------|
| Product shape | **Parallel new heavy** — nothing replaced |
| Player-facing name | **Final Judgement** |
| Working APIName | `final_judgement` |
| Working GUID | `sparroh.finaljudgement` |
| Slot | **Heavy** |
| Relation to vanilla | Similar-but-different to **The Last Argument** (8s charge rocket heavy fantasy); does **not** replace or patch vanilla LA |
| Paths | **Warhead / Hammer / Retribution** |
| Charge spine | **~8s full authorization** is sacred baseline identity |
| Mag | **1** baseline |
| Damage model | **Classic damage spheres allowed** (contrast Manifold Rocket anti-sphere doctrine) |
| Branding | **Brand** system name OK (UI may say designated / painted / authorized) |
| Judgement + Fallout | **Merged** into single peer path **Hammer** (Hammer of Dawn fantasy) |
| Hammer delivery | **(A) Marker conversion** — HoD exotic converts authorization into designator → orbital → fallout (not full rocket × beam double-dip) |
| Fallout element | **Fire** (for now) |
| Path C | **Retribution** (charge economy / execute / after-the-boom payoffs) |
| Tone | **Hallows / SAXON gallows humor** |
| Exotic count | **E6** — equal large hex shapes |
| Ship pool | **~30** upgrades; hybrids OK; no anti-synergy matrix |
| MycoMod (impl) | IsSandbox |
| Doc file | FinalJudgement-DesignDoc.txt (this file) |
| Open / playtest | Prior Q5–Q6 still unsure — treated as non-blocking knobs (§16) |

---

## 1. High Concept / Fantasy

A SAXON **man-portable strategic charge tube**. Hold the argument for ~eight seconds. Authorize one expensive rocket. Baseline is honest: long charge → fat rocket → **classic sphere boom**. No free orbital, no free weather, no free second shot.

Upgrades fork the tube:

- **Warhead** — the boom was always enough (sphere excellence, airburst, armor bias)
- **Hammer** — Judgement + Fallout as one path: **designator → orbital column → Fire fallout** (Gears Hammer of Dawn)
- **Retribution** — the court stays in session (charge refunds, execute windows, brand-chain after the boom)

**One-liner:** *Hold eight seconds. Point at the problem. Either the rocket is the verdict — or the sky is.*

**Product shape:** New parallel heavy (**Final Judgement**). Does not replace The Last Argument, Manifold Rocket, Rocket Salvo, Siege, Gunship, Heaven’s Fury, or Thermite.

**SAXON marketing blurb (draft):**
  “SAXON FJ-1 Final Judgement — Field strategic charge system. Full
   authorization cycle: eight seconds. Orbital liaison packages sold
   separately and are not a metaphor. Residual fallout is a feature.
   (Legal: do not call it a feature in writing. Also do not call it
   The Last Argument. Catalog confusion voids nothing; we checked.)”

Optional stingers:
- “If the sky answers on the first ring, escalate.”
- “Fallout duration exceeds warranty.”
- “You don’t fire the sun. You file the paperwork.”
- “Magazine capacity: one. Appeals: none.”
- “Similar to Last Argument. Different docket number.”

---

## 2. Role & Fantasy in the Arsenal

- **Slot:** Heavy
- **Range:** Long strategic — charge tax is the skill gate; rocket travel readable
- **Role:** Single-authorization delete / optional orbital designator / optional post-boom economy
- **Loop:** Plant or kite while charging → authorize → boom or sky → live with mag-1 downtime
- **Gap filled:**
  - The Last Argument (vanilla) = ultra-charge heavy rocket (sibling fantasy; we are parallel catalog, not a rework patch)
  - Manifold Rocket = primary dumbfire + ray manifold, **no spheres**, mag > 1
  - AMR = primary kinetic bolt, Chambered Argument reload spike — not charge rocket
  - Heaven’s Fury = primary hit-proc smites — not designated heavy orbital
  - Rocket Salvo = lock barrage + ally heal
  - Thermite = throwable Fire heal/wildfire — not heavy designator
  - Nothing owns “8s heavy charge → classic sphere **or** marker-converted Hammer of Dawn (Brand + orbital + Fire fallout) + Retribution economy”

**Not trying to be:** Manifold 2.0, Salvo heal-lock, HF proc smite primary, Thermite heal nade, AMR bolt rifle, or a vanilla Last Argument replacement mod.

### 2.1 Comparison snapshot

```
Weapon                 Niche                         Final Judgement differentiator
---------------------  ----------------------------  ------------------------------------------
The Last Argument      Vanilla 8s charge heavy       Parallel name/fantasy cousin; own grid + HoD path
Manifold Rocket        Primary raycast rockets       Heavy; spheres OK; 8s charge; mag 1
AMR                    Primary kinetic bolt          Charge rocket / orbital; not tube bolt
Heaven’s Fury          Primary smite on hit          Designated orbital heavy path, not proc smites
Rocket Salvo           Lock barrage + ally heal      Single auth; no heal-lock swarm
Siege / Gunship        Explosive volume              One shot is a meeting with command
Thermite               Throwable Fire triage         Heavy Fire fallout denial; no instant heal spine
Heaven Piercer         Draw bow primary              8s strategic charge, not sweet-spot bow
```

### 2.2 Naming note

**Final Judgement** deliberately echoes **The Last Argument** (court / verdict / last word) without colliding on API name, gear id, or display string. Codex may wink at the cousin; it must not claim to be a rework of vanilla LA.

---

## 3. Design Pillars

1. **~8s full authorization is sacred** — baseline identity is the long charge. Partial fire is not free skill expression (unlike Heaven Piercer).
2. **Mag 1 is sacred** — every shot is a decision; economy lives in reserves, refunds (Retribution), and not whiffing.
3. **Classic damage spheres are allowed** — baseline rocket boom and Fallout zones may use sphere/capsule HP. This is intentional contrast to Manifold.
4. **Zero upgrades = complete gun** — charge → rocket → sphere must feel finished without HoD.
5. **Hammer is opt-in spectacle** — Brand / orbital / fallout are path-owned, unlocked by **Hammer of Dawn** exotic.
6. **Marker conversion (A)** — HoD converts the authorization into designator→beam→fallout; does not full-power rocket × full beam.
7. **Judgement + Fallout are one path** — sky strike without linger is HF-lite; linger without sky is a nade puddle. Merged on purpose.
8. **Fallout = Fire** (v1) — real `EffectType.Fire` saturation + zone ticks; not Thermite ally heal.
9. **Three peer paths; hybrids intended; no anti-synergy matrix.**
10. **On-hit / spatial / charge verbs > flat % damage stickers.**
11. **R = reload only; RMB free on baseline** → claimed by designator paint / path toys.
12. **~30 ship upgrades; E6 equal large exotic shapes.**
13. **Boss-safe** — full beam OK; fallout tick rate capped; no permanent floor delete.
14. **Ally-safe** — fallout ally damage low/off; beam respects friendly rules; no team-yeet identity.
15. **Hallows humor** in blurbs; industrial SAXON tone in gunfeel.

---

## 4. Core Mechanics & Gunfeel (Baseline)

### 4.1 Base gun

| Trait        | Draft / intent |
|--------------|----------------|
| Fire mode    | **Charge-to-fire** — hold M1 to authorize |
| Charge time  | **~8.0 s** full (`ChargeData.duration`) — playtest 7.5–8.5 |
| Partial fire | **Disabled or near-useless** on baseline — full charge required (`fireWhenFullyCharged` and/or min charge gate ≈ 1.0) |
| Release      | Fire on full charge auto and/or release-at-full — pick one in impl; prefer **fire when fully charged** for “authorization complete” readability |
| Damage       | Very high rocket direct + **classic impact sphere** |
| Projectile   | Fat visible rocket; mid-slow; light gravity OK |
| Mag / reserve| **Mag 1**; hungry reserves (draft 4–8 total heavy rockets in pool — tune with heavy ammo economy) |
| Reload       | Deliberate single-tube reload after shot |
| ADS / RMB    | Optional ADS; **RMB unbound** on baseline |
| Movement     | Heavy move penalty while charging (plant fantasy); upgrades can ease |
| Model/audio  | Strategic tube, hazard stripes, rising charge whine → authority clack → fat whoosh → sphere thump |

Draft firefeel band (VALIDATE IN PLAYTEST):

| Param | Draft |
|-------|--------|
| Charge duration | 8.0 s |
| Mag | 1 |
| Reserves | 6 (band 4–8) |
| Rocket speed | Readable (~45–70) |
| Impact sphere radius | Large readable pack delete near epicenter |
| Sphere damage | High; falloff by distance from center |
| Direct hit bonus | Optional spike on primary part inside sphere |
| Move speed while charging | Noticeable slow (e.g. 0.4–0.6×) |
| Cancel charge | Release early = cancel, no ammo spend (preferred) OR fumble weak shot — **prefer cancel no spend** so 8s whiffs aren’t ammo grief |

### 4.2 Inputs

| Input | Baseline | Upgraded claims |
|-------|----------|-----------------|
| Hold M1 | Charge authorization | Warhead riders; Hammer Brand seed on full auth; Retribution bank |
| Full charge / release | Fire one rocket | Hammer marker conversion (no/weak rocket → Brand + call-in) |
| RMB | Unbound | Laser paint Brand; drag Brand (Hammer epics); optional Retribution mark |
| R | Reload only | Reload only |
| Heavy equip | Normal | No baseline link to other gear |

### 4.3 Baseline combat loop (zero upgrades)

```
Find angle / plant → Hold M1 ~8s (charge whine) → authorization complete
   → fat rocket flies → impact sphere deletes pack / cracks elite
   → mag empty → R or manage heavy ammo
   → no Brand, no orbital, no fallout field, no charge refund
```

Skill without upgrades: positioning during the long charge, not eating interrupt, leading the rocket, owning the sphere epicenter, ammo husbandry.

### 4.4 What baseline does NOT include

- No Brand / designator paint
- No orbital column / Hammer of Dawn
- No Fire fallout zone
- No airburst command
- No multi-stage warhead
- No charge refund / execute economy
- No mag > 1
- No free partial-charge combat shot
- No ally heal
- No Salvo multi-lock
- No Manifold ray-only doctrine (spheres are the point)

Those are path-, exotic-, or unlock-owned.

### 4.5 Charge model (sacred)

```
ChargeData intent:
  duration              ≈ 8.0s
  fireWhenFullyCharged  true  (preferred baseline)
  fireOnRelease         false or only-at-full
  canFireWhileCharging  false for combat shot
  coolDownSpeed         snappy cancel recovery
```

**Authorization metaphor:** the bar is not a damage mult curve like a bow — it is a **permission gate**. Full = legal shot. Empty = still filing.

Optional later exotic: **Walk-Away Fuse** — finish charge, plant authorization, move before release. Not baseline.

---

## 5. Shared Framework Vocabulary

Upgrades speak these verbs. Baseline owns **Authorize / Rocket / ImpactSphere** only.

### 5.1 Authorize (Charge)
- Hold to fill 8s permission
- Full auth spends mag-1 rocket (or converts under Hammer crown)
- Charge time reducers are rare and small — identity tax stays

### 5.2 Rocket
- Visible projectile carrying the authorization
- Under **marker conversion**, rocket may become a **marker round** (low/no sphere) that only places Brand

### 5.3 ImpactSphere
- Classic explosion HP volume on rocket impact (baseline)
- Warhead path fattens, shapes, delays, airbursts this sphere
- Not banned (Manifold’s sacred cow is the opposite)

### 5.4 Brand (designator mark)
- World or part mark: “authorized coordinates”
- Duration, single-target preference, visible paint
- Hammer-owned; internal name **Brand** (player copy: designated / painted / docket)

### 5.5 Judgement (orbital column)
- After call-in delay: sky-origin strike on Brand (WideGun smite DNA)
- Capsule/beam damage from high point → ground
- Readable VFX (beam / lightning / orbital pencil)
- Hammer-owned; enabled by **Hammer of Dawn** exotic

### 5.6 Fallout (Fire zone)
- Post-Judgement (and optional Warhead hybrid) linger zone
- Classic sphere/cylinder ticks + **EffectType.Fire** apply
- Damage identity = zone pulses + Fire sat → DoT — not Thermite ally heal
- Hammer-owned spine; duration/radius cards deepen

### 5.7 Retribution (economy / aftercourt)
- On-kill / on-elite-break / on-fallout-cook charge crumbs, ammo crumbs, shortened next auth, execute mults
- Path C owned
- Must not erase mag-1 identity (refunds are gated, not free full auto charge)

### 5.8 Airburst / Fuse (Warhead)
- Detonate rocket before impact on command or timer
- Sphere at air point; still classic sphere rules

### 5.9 What we deliberately do NOT vocabulary

- Manifold ShrapnelRay-only damage doctrine as identity
- Salvo multi-lock ally heal barrage
- Thermite instant HP welding / IC heal engine
- Heaven’s Fury free proc smites on every bullet (ours is designated heavy)
- AMR tube bolt / Chambered Argument reload mini-game as spine
- Baseline free HoD
- Mag-fed comfort heavy

---

## 6. Paths (gravity wells — hybrids intended)

### Path A — WARHEAD (classic rocket excellence)
**“The boom was always enough.”**

- Spine: sphere radius/damage, airburst, delayed fuse, multi-stage pops, armor-biased epicenter, shockwave rings
- ST via epicenter placement + AP cards; clear via fat sphere
- **Why it wins without Hammer/Retribution:** pure Last-Argument-cousin nuke fantasy, no sky choreography
- Hybrid hooks: airburst seeds Brand; fat sphere + short fallout rare; refund on multi-kill sphere

### Path B — HAMMER (Judgement + Fallout merged) ★
**“You don’t fire the sun. You file the paperwork.”**

- Spine: **Hammer of Dawn** unlocks Brand + call-in orbital + base Fire fallout (**marker conversion**)
- Judgement = authorization + sky column on Brand
- Fallout = aftermath Fire weather zone (classic linger sphere/cylinder)
- Clear via beam + fallout cook; ST via Brand on elite part + column focus
- **Why it wins without Warhead:** strategic designator fantasy the arsenal under-serves on a heavy
- Hybrid hooks: Warhead airburst places Brand mid-air; Retribution refunds while fallout cooks; drag-Brand chase

### Path C — RETRIBUTION (aftercourt / economy)
**“The argument continues after the boom.”**

- Spine: elite/pack kill refunds, shortened next charge, execute windows on Branded/fallout-cooked, contempt stacks
- Keeps the gun from being only boom-size vs sky-beam
- **Why it wins without Warhead/Hammer:** cadence — more authorizations per fight without raising mag above 1
- Hybrid hooks: fallout kills feed refunds; sphere multi-kills bank charge; Brand execute finisher

### Path × verb matrix

```
                 WARHEAD                  HAMMER                      RETRIBUTION
Authorize        long charge spine        converts to marker mode     refunds / shortens next
Rocket           full payload             marker round (conversion)   same + execute riders
ImpactSphere     core fantasy             weak/none under conversion  multi-kill refund hooks
Brand            optional hybrid seed     core fantasy                execute / chain priority
Orbital column   —                        core fantasy (Judgement)    optional refund on beam kills
Fire fallout     hybrid short ash         core fantasy (Fallout)      cook kills → economy
Airburst/fuse    core fantasy             Brand-at-air hybrid         —
Ally heal        NEVER                    NEVER                       NEVER
Mag > 1 baseline NEVER                    NEVER                       NEVER (refund ≠ +mag)
```

---

## 7. Crowns & Sacred Cows

### 7.1 Exotics (E6 — equal large shapes)

**A-EX1. City-Killer — Exotic** (Warhead path unlock crown)  
- Greatly increases ImpactSphere radius and epicenter damage.  
- Readable “this was the meeting” boom.  
- Slightly longer post-fire recovery or −rocket speed (tax so it’s not free Manifold-clear).  
- Pure Warhead keystone — no Brand required.

**A-EX2. Airburst Authority — Exotic** (Warhead mythic)  
- Enables **airburst command** (RMB or release-mod while rocket in flight) and/or smart fuse.  
- Sphere detonates at commanded point; height advantage for packs in pits.  
- Optional second delayed ring (multi-stage) at reduced power.  
- AIM/RMB priority: airburst command when rocket live and no Hammer paint claim.

**B-EX1. Hammer of Dawn — Exotic** (Hammer path unlock crown) ★  
- Enables full **designator loop** with **marker conversion (A)**:  
  1. Full authorization no longer fires a full-power sphere rocket.  
  2. Fires **marker round** (or instant Brand at aim point if marker hits terrain/enemy).  
  3. **Call-in delay** (draft 0.6–1.2s), telegraphed.  
  4. **Orbital column** strikes Brand (sky → ground capsule/beam).  
  5. Seeds **Fire fallout** zone at impact for duration T.  
- This is the path keystone and the Gears fantasy.  
- Without this exotic, Brand/orbital/fallout cards do nothing or only minor previews (prefer hard gate).

**B-EX2. Permanent Court — Exotic** (Hammer mythic)  
- +Fallout duration and pulse cadence; optional **second Judgement pulse** at 50% power after delay.  
- **Keep It Painted:** while holding RMB after Brand is live, Brand slowly crawls toward look (true HoD hold-the-beam).  
- Hard caps: max Brand move speed, max live fallout zones (1–2), boss tick cap.  
- Mag-1 and charge time remain brakes.

**C-EX1. Contempt of Court — Exotic** (Retribution path unlock crown)  
- Elite or multi-kill (2+) with your authorization refunds a **large charge crumb** (draft 30–50% next charge) or **1 heavy ammo** on cooldown.  
- Enables Retribution economy language for supporting cards.  
- Cannot loop into true full-auto 8s skip — hard ICD (e.g. 10–15s) and diminishing returns.

**C-EX2. Second Reading — Exotic** (Retribution mythic / flex seat)  
- After a successful authorization (rocket or Judgement), gain **Second Reading** window (draft 4–6s):  
  next full charge is **much faster** (draft 40–60% duration) OR free marker repaint once.  
- If unused, expires.  
- Fantasy: the docket allows one immediate appeal.  
- Still mag-1 per shot; economy is time, not clip size.

### 7.2 Sacred cows (do not cut without rewriting identity)

- Parallel heavy named **Final Judgement** (not a vanilla LA patch)  
- **~8s** full authorization baseline  
- **Mag 1** baseline  
- Baseline **classic ImpactSphere** rocket (complete without upgrades)  
- **Hammer of Dawn** = marker conversion designator keystone  
- Judgement + Fallout **merged** on Hammer path  
- Fallout = **Fire** (v1)  
- No Thermite ally instant heal  
- No Salvo heal-lock barrage  
- Three peer paths; hybrids OK  
- ~30 upgrades; 6 equal large exotics  
- R = reload only  
- RMB free until paths claim it  
- Ally-safe beam/fallout  

---

## 8. Full Upgrade List (~30 ship + backlog)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity  
Cell rule: Exotic shapes larger than others; all Exotics same cell count.  
Player-facing names below. API names assigned at implementation.

Wiki / sibling DNA → Final Judgement (inspiration only):

| DNA | Final Judgement |
|-----|-----------------|
| The Last Argument (vanilla) | Cousin fantasy; parallel catalog name |
| Gears Hammer of Dawn | Hammer of Dawn + Permanent Court |
| Heaven’s Fury smite column | Judgement orbital implementation DNA |
| Thermite / Napalm Fire | Fallout Fire zone (no ally heal) |
| Manifold anti-sphere | **Inverted** — spheres celebrated |
| AMR Chambered Argument | Different — charge gate, not reload spike |
| ChargeSniper explosion riders | Sphere mult language only |

------------------------------------------------------------------------------
PATH A — WARHEAD
------------------------------------------------------------------------------

A-EX1. City-Killer — Exotic (path unlock crown)
       +ImpactSphere radius and epicenter damage; slight mobility/recovery tax.

A-EX2. Airburst Authority — Exotic (mythic)
       In-flight airburst command and/or smart fuse; optional delayed second ring.

A-EP1. Crater Protocol — Epic
       +Sphere damage; enemies at epicenter take bonus part damage (ST claw).

A-EP2. Delayed Fuse — Epic
       Rocket can stick/delay briefly then sphere (or longer fuse airburst).
       Reads as “place the verdict.”

A-EP3. Shockwave Collar — Epic
       On sphere: additional outer ring at reduced damage (clear fork).
       Still classic spheres — not ray manifold.

A-EP4. Sabot Cap — Epic
       +Direct rocket hit vs shells/plated; −sphere radius slightly (ST fork).

A-RA1. Wide Ordinance — Rare
       +Sphere radius; −epicenter damage slightly.

A-RA2. Dense Core — Rare
       +Epicenter damage; −sphere radius slightly.

A-RA3. Fast Tube — Rare
       +Rocket speed; −sphere radius slightly.

A-RA4. Soft Launch — Rare
       −Move penalty while charging; −sphere damage slightly.

A-ST1. Primer Charge — Standard
       Minor +sphere damage.

------------------------------------------------------------------------------
PATH B — HAMMER (Judgement + Fallout)
------------------------------------------------------------------------------

B-EX1. Hammer of Dawn — Exotic (path unlock crown) ★
       Marker conversion: Brand → call-in → orbital column → Fire fallout.
       Gates Hammer system for supporting cards.

B-EX2. Permanent Court — Exotic (mythic)
       +Fallout power/duration; optional second pulse; RMB drag Brand (hold-the-beam).

B-EP1. Target Package — Epic
       Brand can bind to enemy parts/brains (not only terrain); elite preference.
       Column focuses branded part when possible.

B-EP2. Priority Fire — Epic
       −Call-in delay; slight −fallout duration (speed fork).

B-EP3. Scorched Docket — Epic
       +Fallout radius and duration; +pulse cadence (Fire zone weather).

B-EP4. Laser Paint — Epic
       While charging (or RMB): visible paint beam pre-places Brand ghost.
       Full auth confirms Brand without needing marker travel (QoL + skill aim).
       If Hammer exotic missing: ghost only, no orbital (or hard-require exotic).

B-RA1. Hot Zone — Rare
       +Fire effect amount from fallout pulses and column riders.

B-RA2. Collateral Briefing — Rare
       +Orbital capsule radius; −column core damage slightly (clear fork).

B-RA3. Clean Room — Rare
       −Ally/self Fire apply and fallout damage to non-hostiles.

B-RA4. Rubber Stamp — Rare
       Minor −charge duration while Hammer of Dawn equipped (small; identity-safe).

B-ST1. Ash Sample — Standard
       Minor +fallout tick damage when zone active (requires HoD).

B-ST2. Docket Clip — Standard
       Minor +Brand duration.

------------------------------------------------------------------------------
PATH C — RETRIBUTION
------------------------------------------------------------------------------

C-EX1. Contempt of Court — Exotic (path unlock crown)
       Elite or multi-kill refunds charge crumb or 1 ammo (ICD / DR).

C-EX2. Second Reading — Exotic (mythic)
       Post-auth window: next charge much faster or one free repaint.

C-EP1. Summary Execution — Epic
       Bonus damage to targets below 30% HP (or Branded targets) from rocket/column.

C-EP2. Continuance — Epic
       On kill during fallout or sphere: +small charge bank toward next auth (cap).

C-EP3. Double Jeopardy — Epic
       First enemy slain by an authorization marks nearby enemies for brief bonus
       damage from your next authorization (chain reading).

C-EP4. Bail Denied — Epic
       Branded or fallout-cooked elites take longer to lose Brand / take +Fire amount.

C-RA1. Clerk’s Fee — Rare
       Minor ammo crumb on authorization kill (soft cap per encounter).

C-RA2. Fast Appeal — Rare
       +Second Reading window duration (requires Second Reading) OR minor −reload.

C-RA3. Prior Conviction — Rare
       Targets hit by your previous authorization take +damage from the next
       (short memory debuff; 1 stack).

C-ST1. Filing Fee — Standard
       Minor +damage (generic glue).

------------------------------------------------------------------------------
GENERIC / GUNFEEL
------------------------------------------------------------------------------

G-RA1. Heavy Mount — Rare
       +Damage; +charge duration slightly (slower auth, harder boom).

G-RA2. Expedited Review — Rare
       −Charge duration small; −damage slightly (cadence fork — watch identity).

G-ST1. Speed Loader — Standard
       +Reload speed.

G-ST2. Spare Dockets — Standard
       +Ammo reserves.

G-OD1. Boundary Incursion — Oddity
       Increases upgrade grid size.

------------------------------------------------------------------------------
FROZEN 30 FOR V1 SHIP
------------------------------------------------------------------------------

EXOTIC (6)
  1  City-Killer
  2  Airburst Authority
  3  Hammer of Dawn
  4  Permanent Court
  5  Contempt of Court
  6  Second Reading

EPIC (8)
  7  Crater Protocol
  8  Delayed Fuse
  9  Shockwave Collar
 10  Target Package
 11  Priority Fire
 12  Scorched Docket
 13  Summary Execution
 14  Continuance

RARE (10)
 15  Wide Ordinance
 16  Dense Core
 17  Soft Launch
 18  Hot Zone
 19  Collateral Briefing
 20  Clean Room
 21  Rubber Stamp
 22  Clerk’s Fee
 23  Prior Conviction
 24  Expedited Review

STANDARD (5)
 25  Primer Charge
 26  Ash Sample
 27  Docket Clip
 28  Filing Fee
 29  Speed Loader

ODDITY (1)
 30  Boundary Incursion

------------------------------------------------------------------------------
BACKLOG (designed, not in first 30)
------------------------------------------------------------------------------

Warhead
- Sabot Cap
- Fast Tube
- Multi-stage triple pop
- Cluster light children (careful vs Manifold MIRV — keep sphere-flavored, few children)

Hammer
- Laser Paint (if cut from epic freeze for space — high value QoL)
- Double Jeopardy moved from C if hybrid seat needed
- Walk-Away Fuse (plant full charge, move, detonate) — strong candidate for later E7 or replaces flex
- Fallout Shock variant (element swap card)
- Multi-Brand cap 2 explicit card

Retribution
- Bail Denied
- Fast Appeal
- Double Jeopardy (if not in freeze)
- Ammo-on-column-kill specific card

Generic
- Spare Dockets, Heavy Mount, Range Gate, Recoil Brace
- Charge hold audio skin packs

Explicitly rejected as identity
- Mag > 1 baseline
- Free partial-charge combat DPS curve (bow-like)
- Thermite ally instant heal
- Salvo multi-lock heal barrage
- Manifold “no damage spheres” doctrine
- Vanilla Last Argument replacement / overwrite
- Baseline free Hammer of Dawn
- Heaven’s Fury free proc smite on every tick without Brand

---

## 9. Example Builds

**City Court (Warhead pure)**  
City-Killer + Airburst Authority + Crater Protocol + Shockwave Collar  
+ Dense Core + Soft Launch + Primer Charge  
→ Plant, 8s, airburst the pit, classic sphere deletes. No sky required.

**Hammer of Dawn (designator)**  
Hammer of Dawn + Permanent Court + Target Package + Scorched Docket  
+ Priority Fire + Hot Zone + Clean Room + Ash Sample  
→ Marker conversion, drag Brand, orbital, Fire weather. The fantasy build.

**Appeals Office (Retribution cadence)**  
Contempt of Court + Second Reading + Continuance + Summary Execution  
+ Clerk’s Fee + Expedited Review + Speed Loader  
→ Mag still 1, but authorizations come back through kills and Second Reading windows.

**Painted Crater (hybrid Warhead + Hammer)**  
City-Killer + Hammer of Dawn + Airburst Authority + Target Package  
→ Airburst marker / Brand at height, then column + residual fat philosophy — tune so conversion rules stay honest (airburst may place Brand then Judgement; sphere power deferred).

**Cook the Books (hybrid Hammer + Retribution)**  
Hammer of Dawn + Contempt of Court + Scorched Docket + Continuance  
+ Hot Zone + Second Reading  
→ Fallout cooks packs → refunds → faster next docket.

**Express Nuke (hybrid Warhead + Retribution)**  
City-Killer + Contempt of Court + Crater Protocol + Summary Execution  
+ Prior Conviction + Soft Launch  
→ Big spheres, execute low-HP, refund crumbs without learning designator.

---

## 10. Economy & Tuning Rules of Thumb

- **Power budget lives in:** charge time tax, mag-1 scarcity, sphere radius/damage, orbital budget, fallout DPS×duration, refund ICD — not infinite Brands.
- **Marker conversion must not be rocket full power + full beam + full fallout.** Defer rocket sphere when HoD converts.
- Warhead pure should beat uninvested Hammer on “I just want boom now” simplicity; Hammer should win on sustained lane denial and spectacle skill ceiling.
- Retribution must not collapse effective charge to SMG cadence — ICD + diminishing returns + still one rocket per auth.
- Fallout boss tick rate capped; grunts cook fast, elites medium, bosses slow.
- Call-in delay is part of fantasy — do not zero it without a heavy tax.
- Brand drag (Permanent Court) needs max speed so it is skill tracking, not mouse-vacuum delete.
- Expedited Review / Rubber Stamp charge cuts are **small**; stacking caps required.
- Hybrids intended; watch airburst + HoD + Contempt loops — cap live zones and refund ICD first.
- Heavy ammo economy is a global brake; don’t ignore sparroh HeavyAmmoRegen interactions in playtest.

---

## 11. Status / Counter Split

| System / counter     | Role                              | Baseline? | Owner |
|----------------------|-----------------------------------|-----------|-------|
| Authorize (8s)       | Permission gate                   | Yes       | Baseline |
| Rocket               | Delivery                          | Yes       | Baseline |
| ImpactSphere         | Classic explosion HP              | Yes       | Baseline |
| Airburst             | Mid-air sphere                    | No        | Warhead exotic |
| Brand                | Designator mark                   | No        | Hammer |
| Judgement orbital    | Sky column on Brand               | No        | Hammer exotic |
| Fire fallout zone    | Linger cook                       | No        | Hammer |
| Charge/ammo refunds  | Cadence                           | No        | Retribution |
| Second Reading       | Post-auth fast window             | No        | Retribution exotic |
| Ally instant heal    | —                                 | **No**    | Thermite’s lane |
| Salvo multi-lock heal| —                                 | **No**    | Salvo’s lane |
| Free proc smites     | —                                 | **No**    | HF’s lane |
| Ray-only no-sphere   | —                                 | **No**    | Manifold’s lane |

### 11.1 Fire (Fallout) — use real EffectType.Fire

| Tuning (draft)        | Value / note |
|-----------------------|--------------|
| EffectType            | Fire (vanilla) |
| Apply via             | Fallout pulses, optional column riders, Hot Zone |
| Full-sat DoT          | Vanilla Fire enemy tick pattern as start |
| Zone HP pulses        | Allowed (classic sphere/cylinder) — unlike Manifold probes-only |
| Self apply            | Reduced; Clean Room helps |
| Ally heal on boom     | **None** |
| Boss pulse cap        | Yes |

### 11.2 Brand (gameplay state — not necessarily full EffectType)

| Tuning (draft)        | Value / note |
|-----------------------|--------------|
| Place on              | Terrain and/or parts (Target Package) |
| Duration              | ~5–8s if Judgement not yet resolved |
| Live Brands           | Cap 1 baseline HoD; 1–2 with mythic/cards |
| UI                    | Visible marker + optional sky laser telegraph |
| Clear on              | Judgement resolve, timeout, death of bound part |

---

## 12. Strengths, Weaknesses & Co-op

**Strengths**
- Unmistakable 8s strategic heavy fantasy
- Baseline complete without upgrades
- Hammer of Dawn is a rare, legible spectacle path
- Warhead satisfies “just the nuke” players
- Retribution gives cadence without mag creep
- Spheres allowed = honest pack delete vs Manifold’s different skill toy
- Strong co-op “get off the Brand” readability

**Weaknesses**
- Long charge is interruptible / stressful in melee
- Mag 1 whiffs hurt
- HoD call-in delay can feel bad if Brand dies early — Target Package + drag mitigate
- Not a brain-off hose
- Parallel unlock/find cost
- Charge reducers must stay disciplined or identity dies

**Co-op**
- Brand and call-in telegraph should be ally-readable
- Fallout must not grief allies (Clean Room; default low ally Fire)
- Orbital friendly-fire policy = vanilla-safe
- Don’t feature ally pit-yeets
- One player’s Permanent Court floor should remain legible (FX budget)

---

## 13. Visual, Audio & Thematic Design

**Appearance**
- SAXON strategic tube: longer than Manifold, court-seal stencils, “AUTH 8.0s” plate, fungal-etched range tables
- Charge: rising lumen rings along barrel; full auth = hard green/white lock
- Warhead: fat warhead glow, crater decals
- Hammer: designator laser, sky pencil beam, ash fallout disk (Fire-orange + legal grey ash)
- Retribution: docket stamps / red contempt ticks on HUD kills

**Sound**
- Charge: low bureaucratic hum → rising whine → **gavel clack** at full auth
- Rocket: heavy whoosh
- Sphere: deep thump + debris
- Call-in: radio chirp / “package inbound” SAXON sting (humor-capable)
- Orbital: sky tear + impact crack (distinct from HF primary smite if possible)
- Fallout: Fire hiss + slow pulse thumps
- Refund: clerk stamp / short UI tick

**Flavor / codex line (in-game style)**
  Final Judgement  
  Heavy strategic charge weapon.  
  Full authorization ~8 seconds. Magazine 1.  
  Baseline: rocket with classic explosive sphere.  
  Hammer upgrades enable designator orbital strikes and Fire fallout.  
  Warhead upgrades enlarge and shape the explosion.  
  Retribution upgrades refund charge and punish marked targets.

---

## 14. Implementation Notes (for later)

### 14.1 Gear registration
- Follow weapon template in this repo: clone base gun, GearInfo high-range id,
  APIName `final_judgement`, behaviour component, SpawnGear stamp, CreateUpgrade pool.
- **Clone candidate:** projectile gun with explosion support (launcher / Gunship-adjacent / charge-capable Gun). Prefer something that already speaks sphere damage.
- Plugin: GUID `sparroh.finaljudgement`, MycoMod **IsSandbox**.
- Persistence: stable gear id; register before `PlayerData.OnAwake` AddGear.
- Working gear id band: pick free high-range id at impl (e.g. 96xxx) — confirm unused vs AMR 87421 / other Sparroh ids.
- Display name **Final Judgement**; do not reuse vanilla Last Argument id/APIName.

### 14.2 Behaviour host
FinalJudgementBehaviour (or true Gun subclass when prefab exists):
- WeaponData: charge gates, sphere mults/radius, airburst flags, HoD flags,
  Brand params, call-in delay, orbital damage/radius, fallout params (Fire amount, pulse DPS, duration),
  retribution refund fractions/ICDs, Second Reading state
- Runtime: charge UI hook, live rocket ref, Brand world/part ref, call-in timer,
  fallout emitter list, refund ICD timers, Second Reading deadline
- Prefab snapshot restore on upgrade Remove

### 14.3 Charge pipeline

```
On charge start: apply move penalty; play charge loop
On charge cancel (release early): clear charge; no ammo (preferred)
On full authorization:
  if HammerOfDawn:
    MarkerConversionFire()  // marker or instant Brand
  else:
    FireRocket()            // full sphere rocket
```

Use `GunData.chargeData` with duration ≈ 8, fireWhenFullyCharged true.

### 14.4 Rocket / sphere pipeline

```
OnRocketImpact / OnAirburst:
  1. Optional direct hit spike on part
  2. Classic ImpactSphere damage (OverlapSphere / game explosion API — allowed)
  3. VFX boom
  4. If hybrid cards seed Brand: place Brand at point (only if HoD rules allow without double-dip exploit)
  5. Retribution kill listeners via OnDamageTarget / kill callbacks
```

### 14.5 Hammer / marker conversion pipeline (critical)

```
MarkerConversionFire():
  1. Spend mag ammo as normal authorization
  2. Spawn marker projectile OR Brand at aim raycast hit
  3. On Brand confirm:
       start call-in timer (telegraph VFX on Brand)
  4. On call-in complete:
       JudgementStrike(Brand.position):
         origin = pos + Vector3.up * 30f (WideGun smite DNA)
         raycast down; GetTargetsInCapsule(origin, end, radius)
         DamageTarget column budget
         SpawnLightning / custom beam RPC
       StartFalloutZone(pos):
         timed pulses + Fire apply (sphere/cylinder OK)
  5. Do NOT also deal full baseline rocket sphere
```

Harmony / hooks likely:
- Gun charge update / fire path — gate full auth; branch HoD
- OnFiredBullet — tag rocket vs marker
- Bullet impact — sphere vs Brand place
- Player Update — call-in timers, Brand drag, fallout ticks
- OnDamageTarget / kill — Retribution crumbs
- WideGun smite as reference only — do not patch WideGun for this weapon

### 14.6 AIM / RMB priority (draft)

1. **Brand drag** (Permanent Court) while Brand live and RMB held  
2. **Airburst command** while Warhead rocket live  
3. **Laser Paint** ghost while charging (Hammer)  
4. **ADS** default  

### 14.7 HUD
- Charge authorization bar (must feel like legal permission, not bow sweet spot)
- Brand marker + call-in countdown
- Fallout zone edge optional
- Second Reading / Contempt ICD pips
- Prefer SparrohUILib if dependency acceptable

### 14.8 VFX / audio priority
1. 8s charge gavel fantasy readable  
2. Sphere boom baseline punch  
3. Designator laser + sky column distinct silhouette  
4. Fallout ash/Fire disk  
5. Airburst cue  
6. Refund stamp  

### 14.9 Multiplayer
- IsSandbox; identical mod on all clients  
- Authorization owner-authoritative  
- Brand/fallout/orbital owned by firer; damage via IDamageSource patterns  
- Cap FX so Permanent Court + fallout doesn’t melt netcode  

---

## 15. Deliberate Non-Goals

- Not a vanilla The Last Argument rework/replace  
- Not Manifold anti-sphere launcher  
- Not Rocket Salvo heal-lock  
- Not Heaven’s Fury primary proc smite  
- Not Thermite heal engine  
- Not AMR bolt rifle  
- Not baseline mag > 1  
- Not baseline free HoD  
- Not bow-like partial charge DPS curve  
- Not requiring custom Unity prefab for v1 (runtime clone OK)  

---

## 16. Open Tuning Questions (playtest, not design blockers)

1. Charge 7.5 vs 8.0 vs 8.5?  
2. Cancel-on-early-release (no ammo) vs fumble weak shot?  
3. fireWhenFullyCharged auto-fire vs release-at-full only?  
4. Reserves 4 vs 6 vs 8?  
5. Call-in delay 0.6 vs 1.2?  
6. Orbital vs fallout damage split under marker conversion?  
7. Brand-on-part default or Target Package gated only?  
8. Permanent Court drag speed cap?  
9. Contempt ICD and refund fraction?  
10. Second Reading = faster charge vs free repaint — which feels better?  
11. Move penalty while charging strength?  
12. Clone base gun after AllGear audit?  
13. Auto-unlock vs progression unlock?  
14. Prior Q5–Q6 from brainstorm (still unsure) — revisit if remembered  

---

## 17. Success Criteria / Player Fantasy Checklist

- [ ] Baseline 8s charge → rocket → classic sphere works with zero upgrades  
- [ ] Mag 1 feels intentional, not broken  
- [ ] Early release does not accidentally dump full power  
- [ ] City-Killer alone makes Warhead fantasy obvious  
- [ ] Airburst Authority makes height/pit plays  
- [ ] Hammer of Dawn alone makes designator fantasy obvious (marker → sky → fallout)  
- [ ] Marker conversion does **not** full rocket × full beam double-dip  
- [ ] Fire fallout cooks via zone + Fire sat, no ally heal  
- [ ] Permanent Court drag/second pulse feels like HoD hold-the-beam  
- [ ] Contempt + Second Reading improve cadence without erasing mag-1  
- [ ] Hybrids (painted crater, cook the books) feel intentional  
- [ ] Frozen 30 ships clean  
- [ ] Name **Final Judgement** nowhere overwrites vanilla Last Argument  
- [ ] Distinct from Manifold, AMR, HF, Salvo in play  
- [ ] Humor present in blurb; gunfeel stays industrial-serious  

---

## 18. Review Decisions Locked (2026-08-14)

| Decision | Lock |
|----------|------|
| Form factor | Man-portable strategic charge heavy rocket |
| Player-facing name | **Final Judgement** |
| APIName / GUID | `final_judgement` / `sparroh.finaljudgement` |
| Slot | Heavy |
| vs vanilla LA | Parallel cousin — not a replace/rework patch |
| Paths | Warhead / Hammer / Retribution |
| Charge | ~8s full authorization sacred |
| Mag | 1 |
| Spheres | Classic damage spheres allowed |
| Hammer merge | Judgement + Fallout single path |
| HoD delivery | Marker conversion (A) |
| Fallout element | Fire |
| Branding noun | Brand (OK) |
| Tone | Hallows / SAXON gallows humor |
| Crowns | City-Killer, Airburst Authority, Hammer of Dawn, Permanent Court, Contempt of Court, Second Reading |
| Ship pool | Frozen 30 listed above |
| Product shape | Parallel weapon |
| Doc file | FinalJudgement-DesignDoc.txt |

---

## 19. Changelog

### v1 (2026-08-14)
- Initial full design from locked user decisions + Hammer of Dawn pivot
- Name: **Final Judgement** (parallel to Last Argument fantasy, distinct product)
- Paths: Warhead (sphere excellence), Hammer (Judgement+Fallout HoD), Retribution (economy/aftercourt)
- Sacred 8s charge, mag 1, classic spheres
- Hammer of Dawn marker conversion (A); Fire fallout
- Research anchors:
  - Sibling docs: Manifold Rocket (anti-sphere contrast; LA called out as 8s cousin),
    AMR (bolt contrast), Heaven Piercer (charge-as-skill but different),
    Zephyr/HLC path bible structure, Heaven’s Fury smite column DNA
  - User locks: parallel; 8s; Brand OK; spheres OK; humor; merge Judgement+Fallout;
    name Final Judgement; Path C Retribution OK; delivery A; Fire; mag 1
- Frozen 30 + backlog + impl pipeline notes

---

## 20. Implementation checklist (post-design)

- [ ] Rename plugin/csproj/thunderstore from template → FinalJudgement
- [ ] FinalJudgementBehaviour.Data fields from §14.2
- [ ] Clone projectile/charge-capable base; enable classic sphere boom
- [ ] Baseline 8s Authorize + mag 1 + ImpactSphere rocket
- [ ] Warhead: City-Killer, Airburst Authority, sphere cards
- [ ] Hammer: HoD marker conversion, Brand, call-in, orbital (WideGun DNA), Fire fallout
- [ ] Permanent Court drag + second pulse
- [ ] Retribution: Contempt refunds, Second Reading window, execute cards
- [ ] UpgradeRegistration frozen 30
- [ ] HUD: auth bar, Brand, call-in, fallout, ICD pips
- [ ] Persistence + SpawnGear stamp
- [ ] Playtest pass on §16 knobs
- [ ] Verify no vanilla Last Argument overwrite
- [ ] Verify no ally instant heal; marker conversion no full double-dip
- [ ] Verify Manifold contrast still holds (this gun may sphere; Manifold must not copy HoD wholesale)
