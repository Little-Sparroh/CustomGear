# Heaven Piercer — Design Document (v1)

## 1. High Concept / Fantasy

A mid-to-long range SAXON industrial **compound bow** primary. Hold to draw, release anytime.
Charge scales damage, arrow speed, effective range, and **gravity** (longer draw = flatter flight).
A baseline **sweet-spot** band near full draw rewards timing with a critical loose.
Upgrades fork the weapon into precision pierce, stuck bleed/pin control, or Swarm-style rain denial.

One-liner: Draw the cams. Time the sweet spot. Pierce the sky — or nail the floor.

Product shape: New primary weapon (**Heaven Piercer**). Does not replace any vanilla gun.


## 2. Role & Fantasy in the Arsenal

- Slot: Primary
- Range: Mid–Long (charge decides; plucks lob, full draws laser-line)
- Role: Deliberate projectile marksman with optional control and area denial forks
- Gap filled:
  - Shocklance = close charge poke / coil
  - DMLR = bolt + laser anatomy rifle
  - Swarm Launcher = hover-pellet hose (no draw fantasy)
  - Globbler = acid plumber denial
  - Nothing owns “compound draw → gravity/speed curve → sweet crit → pierce / bleed stick / rain dive”
- Synergies: Status allies (you pin/bleed, they execute); movement after loose; co-op rain fields

Not trying to be: hitscan sniper, Globbler flood, full Ballista zipline gun, or Swarm Launcher with a bow skin.


## 3. Design Pillars

1. Draw skill is baseline — charge always matters (damage, speed, range, gravity).
2. Sweet spot is baseline — holding forever is not optimal; timing is.
3. On-hit and projectile identity > flat % damage stickers.
4. Three peer paths (Heaven Piercer / Hamstringer / Rain of Arrows); hybrids intended; no anti-synergy matrix.
5. Bleed is a true EffectType with full-saturation DoT (Needle Poison precedent).
6. Rain reuses Swarm-style hover/offset → dive projectile behaviour — bow still draws; rain is payload mode.
7. Crossbow hold (charge latch) is exotic-owned, not free.
8. ~30 upgrades for v1 ship; exotic shapes larger than others; each exotic same cell count.
9. Compound-bow tone (cams, cables, riser, industrial SAXON) — not knightly longbow.
10. RMB stays free for path overrides; R stays reload unless an exotic explicitly needs hold-R.
11. Reload stays reload — do not overload R on baseline.


## 4. Core Mechanics & Gunfeel

### 4.1 Base gun

| Trait        | Draft / intent                                              |
|--------------|-------------------------------------------------------------|
| Fire mode    | Charge-on-hold, loose-on-release (semi projectile)          |
| Damage       | Low pluck → high full draw; sweet spot adds crit mult       |
| Range        | Charge-scaled; light falloff at long range even on full     |
| Mag/reserve  | Small mag (draft 8); each arrow matters                     |
| Projectile   | Single heavy arrow; gravity + speed from charge             |
| ADS / RMB    | No baseline ADS requirement; RMB reserved for overrides     |
| Model/audio  | Compound cams, cable creak, string thump, sweet-spot tick   |

### 4.2 Inputs

| Input     | Role |
|-----------|------|
| Hold M1   | Draw (charge) |
| Release M1| Loose arrow at current charge |
| R         | Reload only |
| RMB       | Unbound on baseline — path overrides only |
| Heavy     | Normal heavy equip (no baseline heavy link) |

### 4.3 Charge model (Shocklance Half Cocked DNA as baseline)

GunData.chargeData intent:

| Field                 | Baseline        | Intent |
|-----------------------|-----------------|--------|
| duration              | ~0.65s          | Full draw time (playtest 0.55–0.75) |
| fireOnRelease         | true            | Loose on release |
| canFireWhileCharging  | true            | Can loose early |
| fireWhenFullyCharged  | false           | No auto-fire at full (crossbow exotic may change feel, not auto-dump) |
| coolDownSpeed         | snappy          | Partial draws recover cleanly |

Min charge floor: ~0.12–0.15s normalized before a legal loose registers as more than a fumble
(still fires a weak pluck if released after floor; below floor = cancel or weakest pluck — prefer weakest pluck for forgiveness).

Movement while drawn: mild move-speed penalty (upgrade-owned removal / invert). Skill tax for power shots.

### 4.4 Charge scaling on loose

On loose, read NormalizedChargeTime (0→1) and apply curves to BulletData / GunData shot instance:

| Stat            | Charge 0 (pluck)     | Charge 1 (full)           |
|-----------------|----------------------|---------------------------|
| Damage          | Low                  | High                      |
| bulletSpeed     | Slow                 | Fast                      |
| Effective range | Short                | Long                      |
| **bulletGravity** | **High (lob arc)** | **Near-zero (flat)**    |
| Optional size   | Thin shaft           | Slightly thicker          |

Gravity is a first-class draw axis. Short draws teach lobbing over cover; full draws read as laser-line power shots.
Curve shape: prefer smoothstep or ease-out so the last 20% of draw still feels meaningful without being binary.

Draft anchors (VALIDATE IN PLAYTEST):

| Param              | Draft        |
|--------------------|--------------|
| minDamageMult      | 0.35× listed |
| maxDamageMult      | 1.00× listed |
| minBulletSpeed     | ~35–45       |
| maxBulletSpeed     | ~110–140     |
| maxBulletGravity   | ~18–25       |
| minBulletGravity   | ~0–2         |
| falloff start/end  | widen with charge |

### 4.5 Sweet spot (baseline skill toy)

| Param            | Draft              | Intent |
|------------------|--------------------|--------|
| Band             | 0.82–0.95 norm     | Near full, not only 1.0 |
| Crit mult        | ×1.28–1.38         | Readable reward |
| Overdraw (1.0)   | Full damage, no crit; optional tiny sway | Holding forever is suboptimal |
| Feedback         | UI bracket tick + cam click + brief reticle flash | Must be learnable without wiki |
| Widen/narrow     | Upgrade-owned      | Path A / generic |

Sacred cow: sweet spot exists with zero upgrades.

### 4.6 What baseline does NOT include

- No pierce
- No embed / stick
- No Bleed application
- No rain / hover-dive
- No charge latch (crossbow)
- No RMB ability
- No auto-fire at full charge

Those are path- or exotic-owned.

### 4.7 Base combat loop (no upgrades)

```
Hold M1 → draw bar climbs → gravity flattens in the mind’s eye → sweet-spot tick → release
   → weak lob pluck / solid shot / sweet crit
   → reposition, redraw; R when dry
```

Skill without upgrades: time the band, lead targets on partial draws, use lob plucks for close scramble, don’t face-tank while glued to full draw.


## 5. Shared Framework Vocabulary

Upgrades speak these verbs. Baseline only owns Draw / Loose / Sweet Spot / Overdraw (soft).

### 5.1 Draw / Loose
- Charge hold → release fires one (or upgraded many) projectile(s)
- All scaling reads NormalizedChargeTime at loose moment
- Latch exotic freezes charge value while held

### 5.2 Sweet Spot
- Critical band near full draw
- Payoffs: damage crit now; later cards may refund ammo, add pierce, seed rain, etc. on sweet only

### 5.3 Overdraw
- Charge clamped at 1.0 without sweet crit
- Path A deepens overdraw power (damage, knockback, shield break) as an alternate mastery to sweet timing

### 5.4 Bleed (true EffectType)
- New status in vanilla saturation pipeline (same rules as Fire/Acid/Bees/Cryo)
- Full saturation → DoT ticks
- Applied by Hamstringer path (not baseline arrows)
- See §11 for tuning table

### 5.5 Embed / Stick
- Arrows remain “in” the target for a duration (counter + VFX)
- Separate from Bleed saturation so both can breathe (Needle stacks precedent)
- Sources: Path B cards; some Rain ground stakes may count as field embeds

### 5.6 Pin
- Short root / hard slow window (Lockdown-lite / Cryo-adjacent)
- Exotic-gated or high rare; respect bosses (reduced duration or soft slow only)

### 5.7 Pierce
- Arrow continues through parts/enemies with retention curve
- Prefer hierarchy-aware pierce when possible (DMLR lesson)
- Path A owned

### 5.8 Rain / Hover-Dive (Swarm DNA)
- Qualifying looses spawn arrows that **hover/offset in air**, then **dive** toward enemies or aim point
- Not a second Swarm Launcher: bow still draws; charge still scales payload
- Path C owned; implementation forks bullet behaviour toward Swarm-style hover projectile

### 5.9 Crossbow Latch
- Hold charge at a fixed notch indefinitely (default full or sweet)
- Apogee Latch exotic only


## 6. Upgrade Paths (gravity wells — hybrids intended)

### Path A — HEAVEN PIERCER (precision / pierce / overdraw)
“One arrow through the cathedral.”

- Spine: pierce count, core/shell bias, overdraw power, reverse falloff, draw-speed vs damage trade, sweet-spot payoffs, scope/range toys
- Clear vs ST: ST native; clear via pierce chains and multi-part walks
- Hybrid hooks: pinned/bleeding targets take bonus pierce damage; rain markers become snipe beacons

### Path B — HAMSTRINGER (embed / bleed / control)
“Leave the shaft in. Let them limp.”

- Spine: embed on hit, Bleed apply, slow (Cryo amount and/or custom move mult), pin windows, detonate/yank stuck shafts, leech from bleed
- Clear vs ST: multi-embed packs + bleed cook; ST via pin + focused bleed stacks
- Hybrid hooks: pierce multi-embeds; rain seeds many sticks

### Path C — RAIN OF ARROWS (hover-dive / denial)
“Own the air, then the floor.”

- Spine: convert loose into Swarm-like hover/offset cloud → dive; multi-arrow volleys; hangtime; ground shaft fields; wait-charged rain; sprinkler spread
- Clear vs ST: clear native; ST via focused dive + wait charge
- Hybrid hooks: Heaven charge quality scales rain power; Hamstringer makes fields apply Bleed

### Path × verb matrix

```
                 HEAVEN PIERCER        HAMSTRINGER            RAIN OF ARROWS
Draw/Loose       core fantasy          delivers sticks        delivers hover payload
Sweet Spot       crit + pierce procs   embed bonus / bleed    denser rain / wait bonus
Pierce           core fantasy          multi-target embed     optional through-cloud
Bleed/Embed      optional rider        core fantasy           field applies bleed
Hover-Dive       snipe beacon toys     barbed rain            core fantasy
Latch/Crossbow   exotic crown          held pin setup         held monsoon charge
```


## 7. Crowns & Sacred Cows

### Skyhook Spire (Exotic) — Heaven Piercer crown
- Sweet-spot and/or full-draw shots pierce N additional parts/enemies.
- High damage retention per pierce step (draft 70–85% retained).
- Prefer walking meaningful targets (brains/parts in line) over pure random bounce.
- Multiple stacks (if CanStack later) increase pierce count modestly — v1 may be unique.

### Apogee Latch (Exotic) — Heaven Piercer crown / crossbow keystone
- While drawn, charge can be **held indefinitely** at a chosen notch:
  - Default notch: sweet-spot center OR full (config on apply / first latch — pick sweet center as default fantasy)
- Movement penalty while latched may be slightly higher than normal draw (crossbow brace).
- Release still looses normally; re-draw required after shot.
- Does not auto-fire at full.
- This is the bow → crossbow identity flip.

### Barbed Covenant (Exotic) — Hamstringer crown
- On hit (Path B embed rules): arrows embed and apply Bleed amount.
- While embedded: periodic small Bleed apply pulses OR direct DoT bridge into Bleed saturation.
- At max embeds on a target OR on target death: barb burst (small radius) that spreads embed fragments / Bleed amount to nearby enemies.
- Readable shaft count on target.

### Harpoon Law (Exotic) — Hamstringer crown
- Heavy embed: briefly **pins** (root or very hard slow) non-boss enemies; bosses get reduced slow only.
- Optional RMB override while this crown is equipped: **Yank**
  - Aimed embedded grunt: pull them toward you OR pull yourself toward heavy target (choose one primary fantasy in impl — recommend: yank grunt to you; self-pull is Auger-adjacent backlog)
- Pin duration short (draft 0.6–1.0s grunts). Cooldown per target to prevent stunlock forever.

### Monsoon Protocol (Exotic) — Rain of Arrows crown
- On loose at charge ≥ threshold (draft ≥0.5): instead of (or in addition to) a single direct arrow, spawn a **hovering arrow cloud** (Swarm DNA) that offsets in air then dives.
- Charge scales: pellet/arrow count, hang time, dive damage.
- Sweet-spot loose: bonus count or tighter dive accuracy.
- Bow remains charge-based — this is payload conversion, not full-auto swarm hose.

### Quiver Storm (Exotic) — Rain of Arrows crown
- Hovering or ground-planted arrows periodically pulse or re-dive (Mitosis / Wait Charging cousin).
- May consume a small ammo crumb or storm charge to avoid infinite clear (tune).
- Encourages seeding the air/floor then fighting inside your weather.

Sacred cows (do not cut without rewriting identity):
- Baseline charge loose with gravity/speed/damage/range scaling
- Baseline sweet spot
- Compound bow fantasy (not crossbow until Apogee Latch)
- Bleed as real EffectType when Hamstringer engages
- Rain uses Swarm-style hover → dive, not Globbler puddle flood
- RMB free on baseline
- Three peer paths, hybrids OK


## 8. Full Upgrade List (~30 ship + backlog)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity
Cell rule: Exotic shapes larger than others; all Exotics same cell count.

Player-facing names below. API names assigned at implementation.

------------------------------------------------------------------------------
PATH A — HEAVEN PIERCER
------------------------------------------------------------------------------

A-EX1. Skyhook Spire — Exotic (crown)
       Sweet/full shots pierce additional targets with high retention.

A-EX2. Apogee Latch — Exotic (crown)
       Hold draw indefinitely at a fixed notch (crossbow).

A-EP1. Spine Splitter — Epic
       +Pierce count on all looses (weaker retention than Spire). Works without Spire.

A-EP2. Aphelion Cam — Epic
       Overdraw (full charge hold ≥0.2s past sweet end) grants bonus damage and hit force;
       sweet crit mult slightly reduced while equipped (mastery fork — still mixable).

A-EP3. Silent Notch — Epic
       Sweet-spot band widened; sweet looses refund 1 ammo (cooldown per N seconds).

A-EP4. Sunder Point — Epic
       Bonus damage vs shells/cores; partial draws deal less to limbs (precision tax).

A-RA1. Bodkin Points — Rare
       +Armor/shell bias; minor +pierce chance (not full pierce card).

A-RA2. Longspine Riser — Rare
       +Range and max bullet speed; +draw duration slightly.

A-RA3. Reverse Falloff String — Rare
       Damage increases with distance (marksman signature); strongest on high-charge shots.

A-RA4. Overdraw Limbs — Rare
       +Max damage, +draw duration.

A-RA5. Cord Efficiency — Rare
       −Draw duration (faster charge), −max damage slightly.

A-ST1. Peak Bracket — Standard
       Minor sweet-spot crit mult increase.

------------------------------------------------------------------------------
PATH B — HAMSTRINGER
------------------------------------------------------------------------------

B-EX1. Barbed Covenant — Exotic (crown)
       Embed + Bleed engine; death/max-stack barb burst.

B-EX2. Harpoon Law — Exotic (crown)
       Pin on heavy embed; RMB Yank override.

B-EP1. Serrated Broadheads — Epic
       All hits embed for duration; +Bleed amount on embed refresh.

B-EP2. Tendon Charter — Epic
       Embedded targets are slowed (move mult); full Bleed sat adds harder slow pulse.

B-EP3. Shaft Harvest — Epic
       RMB (if free) or alt: detonate all your embeds in aim cone for burst ∝ embed count.
       If Harpoon Law equipped, Yank priority wins; Harvest becomes hold-RMB or automatic on reload — resolve via RMB priority list.

B-EP4. Leech Barbs — Epic
       While any enemy you embedded is at full Bleed saturation, drip mild HP and/or ammo crumbs (soft cap).

B-RA1. Cryo Broadhead — Rare
       Arrows apply Cryo amount (slow/shatter pipeline rider).

B-RA2. Deep Embed — Rare
       +Embed duration, +max embeds per target.

B-RA3. Arterial Notch — Rare
       Sweet-spot hits apply bonus Bleed amount.

B-RA4. Crippling Fletch — Rare
       First embed on a target briefly soft-pins (short slow root); weaker than Harpoon Law.

B-ST1. Barbed Twine — Standard
       Minor +Bleed effectAmount when Bleed is available on the shot.

------------------------------------------------------------------------------
PATH C — RAIN OF ARROWS
------------------------------------------------------------------------------

C-EX1. Monsoon Protocol — Exotic (crown)
       Charge-threshold loose → Swarm-style hover cloud → dive; scales with charge.

C-EX2. Quiver Storm — Exotic (crown)
       Hover/ground arrows pulse or re-dive over time.

C-EP1. Split Quiver — Epic
       Loose fires 3 arrows (spread). Per-arrow damage reduced. Charge still scales each.

C-EP2. Hangtime Cams — Epic
       Hover duration up; Wait-Charging DNA — dive damage increases the longer arrows hang.

C-EP3. Ground Stakes — Epic
       Diving arrows that hit terrain plant a short-lived shaft field (damage + mild slow zone).
       Distinct from Globbler acid flood — small stakes, not waves.

C-EP4. Sprinkler Brackets — Epic
       +Horizontal spread of hover cloud; +dive target acquisition radius.

C-RA1. Fletching Storm — Rare
       +Arrows per Monsoon cloud (or +1 extra arrow on any multi-loose).

C-RA2. Dive Accelerator — Rare
       Hover arrows dive faster / sooner; −hang time slightly.

C-RA3. Skyhook Rain — Rare
       Sweet-spot looses add +hover arrows or tighter tracking.

C-RA4. Remote Nock — Rare
       Hover cloud slowly follows crosshair (Remote Control DNA), slower travel.

C-ST1. Loose Quiver — Standard
       Minor +bulletsPerShot when rain mode active (or +1% dive damage — prefer simple +ammo on rain kills in impl if bulletsPerShot fights charge gun).

------------------------------------------------------------------------------
GENERIC / GUNFEEL
------------------------------------------------------------------------------

G-RA1. Draw Speed Oil — Rare
       −Charge duration (faster draw).

G-RA2. Heavy Limbs — Rare
       +Damage, +charge duration, +hit force.

G-ST5. Light Riser — Standard (frozen 30)
       Reduced move penalty while drawn; slight −max damage.

G-RA3. Deep Quiver — Rare
       +Magazine size, −reload speed slightly.

G-ST1. Field Fletching — Standard

       +Reload speed.

G-ST2. Spare Shafts — Standard
       +Ammo reserves.

G-ST3. Brace Stance — Standard
       Minor +accuracy / −spread while charge > 50%.

G-ST4. Cam Whisper — Standard
       Mild +bullet speed at all charge levels.

G-OD1. Boundary Incursion — Oddity
       Increases upgrade grid size.

------------------------------------------------------------------------------
FROZEN 30 FOR V1 SHIP
------------------------------------------------------------------------------

EXOTIC (6)
  1  Skyhook Spire
  2  Apogee Latch
  3  Barbed Covenant
  4  Harpoon Law
  5  Monsoon Protocol
  6  Quiver Storm

EPIC (8)
  7  Spine Splitter
  8  Silent Notch
  9  Serrated Broadheads
 10  Tendon Charter
 11  Split Quiver
 12  Hangtime Cams
 13  Ground Stakes
 14  Leech Barbs

RARE (10)
 15  Bodkin Points
 16  Longspine Riser
 17  Reverse Falloff String
 18  Cryo Broadhead
 19  Deep Embed
 20  Arterial Notch
 21  Fletching Storm
 22  Dive Accelerator
 23  Draw Speed Oil
 24  Heavy Limbs

STANDARD (5)
 25  Peak Bracket
 26  Barbed Twine
 27  Field Fletching
 28  Spare Shafts
 29  Light Riser

ODDITY (1)
 30  Boundary Incursion

Rarity note: Light Riser is Standard in the frozen 30 (path catalog draft listed it Rare).
Frozen list rarity is ship truth for v1.


------------------------------------------------------------------------------
BACKLOG (designed, not in first 30)
------------------------------------------------------------------------------

- Aphelion Cam (overdraw mastery fork)
- Sunder Point
- Cord Efficiency
- Shaft Harvest (RMB detonate embeds)
- Crippling Fletch
- Sprinkler Brackets
- Skyhook Rain
- Remote Nock
- Deep Quiver
- Brace Stance
- Cam Whisper
- Overdraw Limbs
- Quiver Storm ammo crumb economy variants
- Self-pull harpoon (Auger-adjacent)
- Ally heal rain (Cross Pollination cousin — careful)
- Infinite latch + auto-release exotic (probably never — fights sweet spot)
- Decay/Rot applicators on embed
- Ballista-style surface pin cable (too heavy systems)
- True ADS scope exotic
- Hold-R monsoon channel


## 9. Example Builds

Heaven sniper (ST)
  Skyhook Spire + Spine Splitter + Reverse Falloff String + Longspine Riser
  + Silent Notch + Peak Bracket
  Time sweet spots, delete lines of parts.

Crossbow brace
  Apogee Latch + Heavy Limbs + Bodkin Points + Skyhook Spire
  + Draw Speed Oil (optional) + Spare Shafts
  Latch sweet, walk shots like a siege engine.

Bleed warden (Hamstringer)
  Barbed Covenant + Serrated Broadheads + Tendon Charter + Deep Embed
  + Cryo Broadhead + Leech Barbs + Arterial Notch
  Stick packs, cook Bleed, leech the ward.

Harpoon controller
  Harpoon Law + Barbed Covenant + Crippling Fletch (backlog) / Deep Embed
  + Tendon Charter + Light Riser
  Pin priority targets, yank grunts, team focuses pinned elite.

Monsoon clear (Rain)
  Monsoon Protocol + Quiver Storm + Split Quiver + Hangtime Cams
  + Ground Stakes + Fletching Storm + Dive Accelerator
  Draw once, weather the lane, fight in your stakes.

Hybrid freak
  Skyhook Spire + Barbed Covenant + Monsoon Protocol
  + Arterial Notch + Hangtime Cams
  Pierce seeds, bleed cooks, rain re-applies — no artificial brakes.


## 10. Economy & Tuning Rules of Thumb

- Power budget lives in charge quality, sweet timing, embeds, and rain payload — not raw RoF.
- Mag ~8: missing sweet spots and whiffed draws hurt; Deep Quiver backlog if too punishing.
- Gravity curve must be readable at mid charge (not only endpoints).
- Sweet band too wide → no skill; too narrow → frustration. Start 0.82–0.95.
- Overdraw must not beat sweet DPS by enough to delete the band (Aphelion Cam is backlog for a reason).
- Bleed full-sat should matter in Hamstringer builds and be nearly absent otherwise.
- Pin is short and anti-chain-CC’d; bosses resist hard pin.
- Monsoon must not out-clear a dedicated Swarm build without draw skill — charge threshold and mag cost enforce this.
- Quiver Storm needs a spend (ammo crumb / storm charge) if playtests show infinite floor delete.
- Watch stacked slow: Cryo + Tendon + Ground Stakes + Pin — prefer diminishing or non-stacking move mult floors.
- Pierce + Monsoon: cap effective multi-hit storms so one loose doesn’t map-wipe.


## 11. Status Split (explicit)

| Status / counter | Role on this gun                         | Baseline? |
|------------------|------------------------------------------|-----------|
| Bleed            | Spine DoT for Hamstringer (new EffectType)| Path B   |
| Embed stacks     | Stick counter (not EffectType)           | Path B    |
| Cryo             | Slow/shatter rider                       | Upgrade   |
| Fire/Shock/Acid  | Optional backlog riders                  | Backlog   |
| Bees             | Not identity                             | No        |
| Decay/Rot        | Amp backlog                              | Backlog   |
| Water            | Not identity                             | No        |

### 11.1 Bleed EffectType (draft — mirror Needle Poison / Acid)

| Tuning (draft)           | Value        | Notes |
|--------------------------|--------------|-------|
| EffectType               | Bleed = 11   | Append after Cryo = 10 (confirm no collision at impl) |
| DamageMultiplier         | ×1.0         | Pure DoT identity; amps stay Decay/Rot |
| FullSaturationLifetime    | ~5.0–6.0s    | Linger for cook windows |
| DecayDelay               | 3s           | Match vanilla |
| DecaySpeed               | 0.3 /s       | Match vanilla |
| Full-sat DoT (enemy)     | ~10 / 0.2s   | Match Fire/Acid tick pattern as starting point |
| Full-sat DoT (player)    | low if ever  | No baseline self-bleed fantasy |
| On full saturation       | Start DoT    | Locked |
| Verb / UI                | “Bleeding”   | Deep red / arterial |
| Mesh/VFX                 | Clone Acid or custom blood drip later |

BleedStatusEffect pattern:
- OnFullSaturationUpdate → DamageTarget DoT with EffectType.Bleed + DamageFlags.DamageOverTime
- Optional: mild SlowTargetThisTick at full sat (weaker than Cryo 0.6) — only if Tendon Charter not already covering slow; prefer **no innate slow on Bleed** so Cryo/Tendon stay meaningful

Baseline Path B apply amounts: modest per hit; Barbed Covenant and Serrated own the real saturation rate.

### 11.2 Embed counter (not status)

| Param              | Draft     | Intent |
|--------------------|-----------|--------|
| Embed on hit       | Path B    | Shaft stays |
| Duration           | 4–6s      | Refresh on re-hit |
| Max per target     | 3–5       | Burst at max with Covenant |
| Death cascade      | Covenant  | Spread barbs |


## 12. Implementation Notes

### 12.1 Gear registration
- Follow weapon template in this repo: clone base gun, GearInfo high-range id, APIName `heaven_piercer`, behaviour component, SpawnGear stamp, CreateUpgrade pool.
- Prefer base with projectile + workable charge if present in AllGear; else any projectile Gun and force `gunData.chargeData` (Shocklance / ChargeSniper DNA).
- Note: ChargeSniper often **not** in AllGear — verify at impl; Shocklance or generic Gun + ChargeData injection is fine.
- Plugin: GUID `sparroh.heavenpiercer`, MycoMod **IsSandbox**.
- Persistence: stable gear id; register before PlayerData.OnAwake AddGear.

### 12.2 Charge + gravity loose hook
- On fire / OnFiredBullet (owner):
  - Read `gunData.chargeData.NormalizedChargeTime` (or captured loose charge)
  - Lerp damage, speed, gravity, range into BulletData
  - Apply sweet-spot crit if in band
- Ensure charge value is captured at loose edge (release) so cooldown doesn’t zero the shot.

### 12.3 Crossbow latch (Apogee Latch)
- While flag set and fire held: stop charge time progression at notch; keep `isCurrentlyCharging` visuals
- May need Harmony on Gun charge update path — read Gun charge tick in decompile at impl
- Release looses at latched normalized value

### 12.4 Bleed EffectType (phased — copy Needle approach)

Phase 0 — Vertical slice without enum
  - Embed stacks + fake bleed DoT on behaviour if needed for feel

Phase 1 — EffectType + class
  - Add EffectType.Bleed = 11 (or free slot)
  - BleedStatusEffect : StatusEffect (Acid/Poison DoT pattern)
  - Patch StatusEffectManager.CreateEffect switch
  - Verify effectPool sizing from enum

Phase 2 — StatusEffectData
  - Inject/clone into Global effect table (verb, colors, materials, audio)

Phase 3 — Gun wiring
  - Path B upgrades set damageEffect / extra apply on hit
  - Embed map on BowWeaponBehaviour

Phase 4 — Pin / Yank
  - SlowTargetThisTick / existing shackle-like patterns
  - RMB override priority table

Fallback if EffectType injection blocked:
  - First-class custom Bleed tracker with DoT ticks; design identity unchanged; debt in changelog

### 12.5 Rain / Swarm hover-dive
- When Monsoon (or rain flags) active and charge ≥ threshold:
  - Fork bullet prefab/behaviour toward Swarm Launcher hover projectile pattern
  - Offset bullets in air, hang, then dive/track
- At impl: locate Swarm gun + bullet types in Assembly (name may not include “Swarm”; search hover dive pellet / WideGun-adjacent / launcher types). Wiki identity is source of fantasy; code type names confirmed in decompile pass.
- Charge scales count, hang, dive damage
- Ground Stakes: on terrain impact, spawn short zone (reuse puddle/zone patterns carefully without acid flood identity)

### 12.6 Hooks

| Hook              | Use |
|-------------------|-----|
| Charge update     | Latch freeze; draw move penalty; HUD bar |
| OnFiredBullet     | Apply charge scalers; rain fork; muzzle VFX |
| OnBeforeDamage    | Sweet crit mult, pierce prep, precision flags |
| OnDamageTarget    | Embed +1, Bleed apply, Arterial Notch, Leech tracking |
| OnSaturateTarget  | Full Bleed payoffs |
| OnKillTarget      | Barb burst, ammo crumbs, storm seeds |
| RMB press         | Yank / Harvest priority |

### 12.7 RMB priority

1. Harpoon Law Yank (if equipped and valid embed target)
2. Shaft Harvest detonate (if equipped and no Yank claim)
3. Future path overrides
4. Else unbound

### 12.8 State host
BowWeaponBehaviour (or true Gun subclass when prefab exists):
- WeaponData: sweet band, crit mult, pierce, embed rules, rain flags, latch notch, gravity min/max, speed min/max
- Runtime: per-target embed map; set of bleeding brains for Leech; hover arrow list if needed
- Prefab snapshot restore on upgrade Remove

### 12.9 HUD
- Draw bar with sweet-spot bracket (ChargeProgressBarHUD DNA / SparrohUILib)
- Optional embed pips on aimed target
- Latch icon when Apogee equipped and holding

### 12.10 Multiplayer
- Sandbox mod; all clients need the same plugin
- Damage/status follow IDamageSource authority
- Hover arrows: owner-authoritative spawn; validate dive damage on authority
- Embed map: owner gun authority

### 12.11 VFX / audio priority
1. Draw creak (pitch rises with charge) + cam tension
2. Sweet-spot tick (unique, crisp)
3. Loose thump; flight whoosh scales with speed
4. Gravity readability: arcing trail vs flat streak
5. Embed stick + Bleed full-sat verb
6. Pin crack / Yank reel
7. Rain hover hum → dive shriek volley


## 13. Deliberate Non-Goals

- Not hitscan laser sniper
- Not Globbler acid flood primary
- Not full Ballista zipline weapon
- Not baseline free pierce + bleed + rain
- Not auto-fire at full charge by default
- Not baseline RMB power
- Not knightly fantasy art direction
- Not requiring custom Unity prefab for v1 (runtime clone OK)
- Not shipping Decay/Rot apply in first 30
- Not team-hostile bleed auras


## 14. Open Tuning Questions (playtest, not design blockers)

1. Draw duration 0.65s vs weapon fantasy and mission pace.
2. Sweet band 0.82–0.95 width and crit mult.
3. Mag 8 vs 6/10.
4. Min gravity at full draw — 0 feels sniper; 2 keeps slight drop authenticity.
5. Monsoon charge threshold 0.5 vs 0.65.
6. Hover arrow count vs Swarm clear power budget.
7. Bleed full-sat lifetime vs Leech Barbs uptime.
8. Pin duration and boss rules.
9. Whether Spine Splitter without Spire feels bad (retention numbers).
10. EffectType injection approach after assembly audit post-game update.
11. Exact Swarm bullet class name / clone path in decompile.
12. Yank: pull enemy vs pull self — lock pull-enemy for v1 unless playtest demands mobility.


## 15. Success Criteria / Player Fantasy Checklist

- [ ] Hold-draw-release works with zero upgrades; early loose is weaker lob
- [ ] Full draw is visibly flatter (gravity down) and harder-hitting
- [ ] Sweet-spot tick is audible/visible; crits feel intentional
- [ ] Overholding past sweet is strong but not best
- [ ] Skyhook Spire lines pierce through a pack/parts
- [ ] Apogee Latch holds a notch like a crossbow without auto-firing
- [ ] Barbed Covenant embeds and Bleeds to full sat DoT
- [ ] Harpoon Law pins a grunt; Yank is a decision
- [ ] Monsoon turns a charged loose into hover → dive rain
- [ ] Quiver Storm makes seeded air/floor keep working
- [ ] Hybrid Spire + Covenant + Monsoon feels intentional
- [ ] Compound bow audio/VFX read industrial SAXON, not fantasy longbow


## 16. Strengths, Weaknesses & Co-op

Strengths
- Unique draw skill expression (gravity + sweet spot)
- Three very different endgames from one gun
- Real Bleed status for systemic interactions
- Rain denial without copying Globbler
- Crossbow exotic is a satisfying identity flip

Weaknesses
- Low brain-off DPS (must draw)
- Mag pressure; whiffed draws hurt
- Close scramble weaker than SMG/hose without Light Riser / pluck practice
- Setup time vs hyper-aggressive rush waves
- Pin/CC limited on bosses

Co-op
- You pin/bleed/rain; allies dump damage into controlled targets
- Avoid team-hostile fields
- Rain should not grief ally visibility excessively (VFX budget)


## 17. Visual, Audio & Thematic Design

Appearance
- SAXON compound bow: CNC riser, dual cams, cable guards, battery-assist limb sensors, fungal-etched warning decals, quiver mag as cassette of shafts
- Not medieval wood/recurve silhouette
- Arrows: carbon-industrial bolts with glowing nocks; charge brightens nock
- Rain mode: arrows bloom into hover brackets then dive like one-way Swarm pellets
- Embed: shafts visibly stuck in mycoflesh

Sound
- Draw: mechanical cam roll + rising cable tension
- Sweet spot: single clean crystalline click
- Loose: heavy string thump + limb knock
- Flight: whoosh pitch ∝ speed
- Embed: wet chunk
- Bleed full-sat: low arterial hiss
- Pin/Yank: harpoon reel
- Rain: hive-like hover bed → staccato dive impacts

Flavor / SAXON blurb (draft)
“SAXON HP-7 Heaven Piercer — Compound field bow for vertical denial and priority perforation.
Draw to flatten. Release to commit. (Sweet-spot calibration is not a toy. It is policy.)”


## 18. Locked Review Decisions (2026-08-06)

| Decision              | Lock |
|-----------------------|------|
| Form factor           | Industrial compound bow |
| Player-facing name    | Heaven Piercer |
| Slot                  | Primary |
| Paths                 | Heaven Piercer / Hamstringer / Rain of Arrows |
| Sweet spot            | Baseline |
| Crossbow hold         | Apogee Latch exotic only |
| Bleed model           | True EffectType.Bleed + BleedStatusEffect |
| RMB                   | Free for path overrides |
| Tone                  | Compound / SAXON industrial |
| Charge scales         | Damage, speed, range, **gravity** (less drop when charged) |
| Rain tech             | Swarm-style hover/offset → dive projectile conversion |
| Draw input            | Hold M1 charge, release loose |
| Auto-fire at full     | Off by default |
| Ship pool             | Frozen 30 listed above |
| Crowns                | Skyhook Spire, Apogee Latch, Barbed Covenant, Harpoon Law, Monsoon Protocol, Quiver Storm |
| MycoMod flag          | IsSandbox at implementation |
| Working APIName       | heaven_piercer |
| Doc file              | HeavenPiercer-DesignDoc.txt (this file) |
| Yank v1               | Pull embedded grunt toward player (self-pull backlog) |
| Bleed innate slow     | No — slow from Cryo/Tendon/Pin cards |


## 19. Changelog

v1 (2026-08-06)
- Initial full design from locked user decisions
- Research anchors:
  - ChargeData / GunData.bulletGravity / ChargeSniper patterns (decompile)
  - Shocklance Half Cocked (wiki) as early-release charge DNA
  - Swarm Launcher hover → dive, Wait Charging, Mitosis, Remote Control, Sprinkler (wiki)
  - Globbler Flood/puddles as denial contrast (do not copy)
  - Cryo SlowTargetThisTick; no vanilla Bleed EffectType
  - Ballista stick/recall as harpoon inspiration only
  - Sibling docs: Needle Carbine (EffectType + stacks), DMLR (paths/crowns), Heat Cycler, Blood Carver
- Gravity-on-draw and Swarm rain conversion locked per user
- Bleed real EffectType locked per user
- Compound bow tone locked per user


## 20. Implementation checklist (post-design)

- [ ] Rename plugin/csproj/thunderstore from template → HeavenPiercer
- [ ] BowWeaponBehaviour.Data fields from §12.8
- [ ] Charge loose scaler (dmg/speed/grav/range + sweet spot)
- [ ] Verify charge base gun in AllGear
- [ ] Bleed EffectType injection phases
- [ ] Embed map + Barbed Covenant
- [ ] Harpoon pin + RMB Yank
- [ ] Monsoon Swarm-bullet fork
- [ ] Quiver Storm pulses
- [ ] Apogee Latch charge freeze
- [ ] UpgradeRegistration frozen 30
- [ ] HUD draw bracket
- [ ] Persistence + SpawnGear stamp
- [ ] Playtest pass on §14 knobs
