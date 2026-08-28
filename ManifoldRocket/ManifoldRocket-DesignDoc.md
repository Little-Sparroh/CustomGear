# Manifold Rocket — Design Document (v1)

## 0. Locked Decisions (2026-08-07)

| Decision | Lock |
|----------|------|
| Product shape | Parallel new primary — nothing replaced |
| Player-facing name | **Manifold Rocket** |
| Working APIName | `manifold_rocket` |
| Working GUID | `sparroh.manifoldrocket` |
| Slot | **Primary** |
| Paths | **Guidance / MIRV / Phosphorus** |
| Damage model | **Raycast-only** — no damage-sphere HP delete (VFX bloom OK) |
| Baseline delivery | ImpactSpike + **generous ShrapnelRays** |
| Manual rocket control | **Upgrade-only** (baseline = dumbfire) |
| White Phosphorus | **Full peer path** (Option A), not a single orphan exotic |
| Rocket jump | **Light baseline** + **Jump-Jet Coupler** exotic crown |
| 6th exotic | **Jump-Jet Coupler** (RJ build crown; pierce is Epic line) |
| vs Siege Cannon | Different enough by deliberate rockets + ray manifold; no hard non-overlap anxiety |
| vs Rocket Salvo | No innate multi-lock heal barrage |
| vs Last Argument | No 8s charge sniper identity; mag > 1; primary pacing |
| vs Thermite | Fire linger = DPS/denial, **not** instant HP welding triage |
| vs Swarm | Heavy singular rockets, not hover-pellet hose |
| Exotic count | **E6** — equal large hex shapes |
| Ship pool | **~30** upgrades; hybrids OK; no anti-synergy matrix |
| MycoMod (impl) | IsSandbox |
| Doc file | ManifoldRocket-DesignDoc.txt (this file) |

---

## 1. High Concept / Fantasy

A SAXON **man-portable manifold rocket system** — slow, heavy, readable flight. One rocket is a decision.

Baseline is honest **dumbfire**: visible travel, fat **ImpactSpike** on the part you hit, then a **manifold of short ShrapnelRays** that do the pack work spheres usually steal. No free lock-on, no free pilot, no free MIRV, no free WP weather.

Upgrades fork the tube:

- **Guidance** — fly the argument (wire steer, soft seek, terminal track)
- **MIRV** — one bus, many raycast children (airburst, carpet, delayed focus)
- **Phosphorus** — the boom is the match; Fire weather via status probes, not sphere nukes

**One-liner:** *One tube. Many rays. Fly it, split it, or set the room on fire — never a free damage sphere.*

**Product shape:** New parallel primary (**Manifold Rocket**). Does not replace Gunship, Siege Cannon, Rocket Salvo, Last Argument, Swarm, or Thermite.

**SAXON marketing blurb (draft):**
  “SAXON MR-6 Manifold Rocket — Field recoilless for operators who think
   OverlapSphere is a war crime. Impact spikes the part you meant. Shrapnel
   rays finish the argument. Guidance packages, MIRV buses, and white
   phosphorus manifolds are field-swappable. (Legal requested we stop calling
   the jump coupler ‘intentional.’)”

Optional stingers:
- “If the sphere is free, the balance is broken.”
- “You don’t fire rockets. You authorize them.”
- “The manifold is not splash. It is a jury of rays.”
- “Jump-Jet Coupler voids the warranty and improves the operator.”

---

## 2. Role & Fantasy in the Arsenal

- **Slot:** Primary
- **Range:** Mid (readable rocket travel; not SMG panic, not Last Argument map-snipe)
- **Role:** Deliberate rocket pressure / optional pilot skill / optional submunition weather / optional Fire denial / mobility via rocket jump
- **Gap filled:**
  - Siege Cannon = full-auto *shells* + halo + fire-mission bay
  - Rocket Salvo = lock-on multi-rocket *heal+damage* barrage (Glider)
  - Last Argument = ultra-charge single decisive heavy rocket
  - Swarm = hover-dive pellet weather
  - Thermite = throwable fire triage / wildfire
  - Nothing owns “dumbfire rocket → raycast manifold → steer / MIRV bus / WP primary weather + rocket jump”

**Not trying to be:** Siege 2.0, Salvo heal-lock, Last Argument charge sniper, Swarm hose, Thermite heal nade, or sphere-meta launcher.

### 2.1 Comparison snapshot

```
Weapon                 Niche                         Manifold differentiator
---------------------  ----------------------------  ------------------------------------------
Siege Cannon           Auto shells + halo + bay      Deliberate rockets; ray manifold; no halo
Rocket Salvo           Lock barrage + ally heal      No free multi-lock heal; skill pilot opt-in
Last Argument          8s charge sniper rocket       Mag > 1; primary pace; no charge spine
Swarm Launcher         Hover-dive pellets            Heavy singular bus; not pellet hose
Gunship (vanilla)      Explosive rotary              Parallel space via Siege; Manifold ≠ shells
Thermite               Throwable fire heal/wildfire  Primary WP denial; no instant HP spine
Heaven Piercer         Draw bow + rain               Rockets, not arrows
```

---

## 3. Design Pillars

1. **Raycast damage is sacred** — ImpactSpike, ShrapnelRays, PierceLine, Fire status ticks. No OverlapSphere HP delete.
2. **VFX bloom ≠ damage sphere** — smoke, flash, debris free; damage must justify a ray/line/status event.
3. **Baseline manifold is generous** — pack clear without upgrades comes from **many short rays**, not one fat sphere.
4. **Baseline rocket jump is light** — classic launcher mobility; Jump-Jet Coupler makes it a build.
5. **Dumbfire baseline** — steer / lock / MIRV / WP are upgrade-owned.
6. **On-hit verbs and delivery mode > flat % damage stickers.**
7. **Three peer paths; hybrids intended; no anti-synergy matrix.**
8. **R = reload; AIM/RMB free on baseline → claimed by Guidance steer / MIRV airburst command.**
9. **~30 ship upgrades**; exotic shapes larger & equal cell count (6).
10. **No Salvo heal-barrage identity; no Thermite instant-heal identity.**
11. **Fun readable rocket toys** over spreadsheet sphere DPS.
12. **Armor literacy** — ImpactSpike and focused rays crack parts; uninvested ray spray is clear king, not boss delete free.

---

## 4. Core Mechanics & Gunfeel (Baseline)

### 4.1 Base gun

| Trait        | Draft / intent |
|--------------|----------------|
| Fire mode    | Semi / slow pump — **one rocket per trigger** |
| Damage       | High ImpactSpike on struck part |
| Manifold     | **Generous ShrapnelRays** on impact (pack identity) |
| Sphere       | **None** as damage channel |
| RoF          | Deliberate (~60–90 RPM class target — tune) |
| Mag / reserve| Small mag (**4–6**); hungry enough that whiffs hurt |
| Projectile   | Visible rocket, mid speed (~55–75), light gravity |
| ADS / AIM    | Optional ADS; **AIM unbound on baseline** (path claims) |
| Handling     | Heavy tube; readable recoil punch; not a mobility SMG *except via RJ* |
| Rocket jump  | **Light baseline** self-knockback from own ImpactSpike / near self-rays |
| Model/audio  | Borrow launcher-adjacent until art; tube thump, whoosh, ray crackle, jump kick |

Draft firefeel band (VALIDATE IN PLAYTEST):
- Mag: 5 (band 4–6)
- Reserves: modest relative to mag
- ImpactSpike: high single-part hit
- ShrapnelRays: **10–14** baseline rays, short length, steep damage falloff along ray
- Self RJ: readable hop; mild or zero self-damage at baseline (prefer **low self-damage** so jump is joyful)

### 4.2 Anti-sphere damage model (SACRED)

```
ImpactSpike     — fat direct hit on the part/brain the rocket struck
ShrapnelRays    — N short rays from impact (cone / toward nearby parts / random-in-sphere-direction)
                  each ray = independent raycast damage event (budgeted)
PierceLine      — rocket or detonation continues as a line through hierarchy (upgrade-owned)
BurnTicks       — WP/Fire: real EffectType.Fire saturation + DoT — not sphere nuke
Submunition     — MIRV children each use ImpactSpike and/or small ShrapnelRays (no child spheres)
RocketJump      — self knockback impulse from own detonation rules (not damage-sphere team wipe)
```

**Explicitly banned as damage:**
- OverlapSphere / damage-sphere explosions as HP dealers
- “Explosion size” as stealth multi-target delete stat
- MIRV that multiplies full sphere payloads
- WP “cloud” that deals flat AOE HP without ray/status justification

**Allowed:**
- VFX explosion bloom, smoke, light, debris
- Fire status application via short **probe rays** or contact ticks on entities already touched by rays
- Soft physics knockback volumes if they deal **no** or **token** damage (RJ uses directed self impulse)

### 4.3 Baseline impact sequence

```
Fire M1 → dumbfire rocket flies (speed + light gravity)
  → On impact (enemy part / terrain / timeout):
      1. ImpactSpike on primary hit (if enemy part)
      2. Spawn ShrapnelRays (count C, length L, damage budget B split across rays)
      3. Each ray: Physics.Raycast / game damage ray → OnBeforeDamage/OnDamageTarget
      4. If owner in self-jump range: apply RocketJump impulse (baseline light)
      5. VFX boom (no damage sphere)
  → Done
```

### 4.4 Inputs

| Input        | Baseline                         | Upgraded claims |
|--------------|----------------------------------|-----------------|
| Tap/Hold M1  | Fire one dumbfire rocket         | Guidance mid-flight claims; MIRV riders; WP payload tags |
| AIM / RMB    | Unbound / mild ADS if any        | **Wire steer** (Guidance) or **Airburst command** (MIRV) per priority |
| R            | Reload only                      | Reload only (no breech mash identity) |
| Movement     | Standard + light RJ on self-hit  | Jump-Jet Coupler amp; Guidance cancel→jump toys |

### 4.5 AIM priority (LOCKED draft)

When multiple systems want AIM:

1. **Wire-Guidance steer** — if active rocket in flight and Wire-Guidance equipped
2. **MIRV airburst / split command** — if M.I.R.V. Bus (or command card) equipped and rocket in flight
3. **ADS** — default

Tap vs hold split is a playtest knob if both Guidance and MIRV command are equipped
(e.g. hold = steer, tap = airburst). Priority table above is the rule of thumb.

### 4.6 Baseline combat loop (zero upgrades)

```
M1 → lead the dumbfire rocket
   → ImpactSpike the part you meant
   → ShrapnelRays tidy adjacent limbs / nearby grunts
   → optional light rocket jump for reposition
   → R when dry
   → no steer, no lock, no MIRV, no WP field, no pierce
```

Skill without upgrades: leading shots, aiming at valuable parts, using ray manifold geometry, jump timing, reload discipline.

### 4.7 What baseline does NOT include

- No manual wire steer
- No soft lock / hunter seek
- No MIRV airburst / carpet
- No White Phosphorus field
- No PierceLine
- No ally heal
- No Salvo multi-lock barrage
- No damage spheres
- No halo orbit
- No mash-R breech / cook-off siege trio
- No charge-sniper 8s loop
- No Jump-Jet Coupler full amp (only light baseline RJ)

Those are path-, exotic-, or unlock-owned.

---

## 5. Shared Framework Vocabulary

Upgrades speak these verbs. Baseline owns ImpactSpike + ShrapnelRays + light RocketJump only.

### 5.1 DeliveryMode (per rocket / per child)

```
ImpactSpike    — primary hit damage on struck part
ShrapnelRays   — manifold rays from detonation point
PierceLine     — line continuation / hierarchy walk
Submunition    — child rocket/bomblet with its own mini-rules
BurnProbe      — Fire apply via ray tip or short probe (Phosphorus)
Steer          — player or soft-seek modifies velocity mid-flight
Airburst       — detonate before impact on command or fuse
```

### 5.2 Shrapnel manifold (core noun)

| Param (draft)     | Baseline        | Upgrade space |
|-------------------|-----------------|---------------|
| Ray count         | 10–14           | +count cards, MIRV child budgets |
| Ray length        | Short           | +length rares |
| Damage split      | Budget B / N    | bias toward primary cone / same-brain parts |
| Aim bias          | Mild toward nearby enemy parts | Guidance / smart fuze cards |
| Pierce on ray     | Off             | Epic line |
| Terrain stop      | Yes             | optional punch-through backlog |

**Tuning rule:** total manifold damage budget should feel strong on packs and honest on single armor — not “14 full ImpactSpikes.”

### 5.3 Rocket jump

```
On own detonation (impact or airburst):
  if owner within jump radius R_j:
    apply self impulse away from detonation point (or along aim-up bias)
    optional token self-damage (baseline low / zero preferred)
Jump-Jet Coupler:
  +impulse, +control, −self-tax, mid-air chain rules, optional shrapnel-assisted hop
```

RJ must not become free infinite vertical without ammo spend. Mag pressure is the brake.

### 5.4 Guidance (path spine)

- **Wire steer:** hold AIM to bend active rocket toward look
- **Soft lock / Hunter:** terminal track assist within cone/radius
- **Camera follow (backlog or thin epic):** optional missile-cam feel — careful motion sickness
- **Detonate-on-release:** optional with Wire-Guidance

### 5.5 MIRV / bus (path spine)

- **Airburst split:** command or fuse → N Submunitions
- **Carpet:** drop bomblets along flight path
- **Focus split:** more children toward Painted / last-hurt / aim point
- Each child: **ImpactSpike and/or 2–4 micro-rays** — budgeted, never full parent sphere×N

### 5.6 Phosphorus / WP (path spine)

- **White Phosphorus exotic** enables full WP weather system
- Impact / airburst seeds **BurnProbes** and linger emitters that **apply EffectType.Fire**
- Linger deals damage primarily through **Fire full-sat DoT + probe ticks**, not flat AOE HP spheres
- Trail along flight optional (path cards)
- **No instant ally HP welding** (Thermite owns that throwable fantasy)

### 5.7 Armor literacy

| Channel | Good at | Rules of thumb |
|---------|---------|----------------|
| ImpactSpike | Priority part, ST openers | High direct; bonus vs shells when AP cards present |
| ShrapnelRays | Packs, multi-limb tidy | Clear king; reduced per-ray vs heavy plate unless Shred/Crack |
| PierceLine | Hierarchy walk ST | Epic+; retention curve |
| Submunition | Coverage weather | Budget damage; focus cards for ST |
| BurnProbe / Fire | Soften, linger, cook | Status pipeline; full-sat DoT is the payoff |
| RocketJump | Mobility | Not a damage channel |

**Crack (optional Battery-adjacent backlog / thin rare):** part debuff for follow-up spikes.  
**Painted (Guidance/MIRV):** seek and focus-split priority.

### 5.8 What we deliberately do NOT vocabulary

- Damage spheres / explosion radius as DPS
- Salvo ally-heal lock barrage
- Thermite instant HP pulses / Internal Combustion heal engine
- Siege halo orbit / mash breech / Shipkiller mag-1 trio as spine
- Chaingun spool / plant hose
- Last Argument 8s charge identity
- Friend pet AI

---

## 6. Paths (gravity wells — hybrids intended)

### Path A — GUIDANCE (pilot / seek / place)
**“You don’t fire rockets. You fly them.”**

- Spine: Wire-Guidance steer, Hunter-Seeker soft lock, terminal track, paint, detonate-on-command, jump-cancel toys
- ST via putting ImpactSpike on the right part; clear via steered manifold through packs
- **Why it wins without MIRV/WP:** skill placement of spike + rays; seek assists without Salvo barrage
- Hybrid hooks: steer a MIRV bus; drop WP on the exact tile; jump-jet + steer cancel

### Path B — MIRV (bus / split / carpet)
**“One tube. Many arguments.”**

- Spine: M.I.R.V. Bus airburst, Carpet Protocol path drops, child ray budgets, delayed focus, split count
- Clear native; ST via focus split onto Painted / aim point
- **Why it wins without Guidance:** coverage weather from one trigger; still raycast-fair
- Hybrid hooks: guided bus; WP children; jump while carpet seeds below

### Path C — PHOSPHORUS (fire weather / cook / linger)
**“The boom is the match. The room is the fuel.”**

- Spine: White Phosphorus unlock, BurnProbes, linger duration, flight trail, cook payoffs, white-hot tip hybrid
- Clear via ignite weather; ST via full-sat cook + ImpactSpike
- **Why it wins without Guidance/MIRV:** primary-weapon Fire denial that Thermite doesn’t own as a gun
- Hybrid hooks: steered WP placement; MIRV WP bomblets; jump through your own heat (careful self-Fire rules)

### Path × verb matrix

```
                 GUIDANCE                 MIRV                      PHOSPHORUS
ImpactSpike      steered precision        child primaries           white-hot tip
ShrapnelRays     on-place manifold        split micro-rays          spark / probe spread
PierceLine       optional epic            optional                  white-hot epic
Manual steer     core fantasy             hybrid aim-split          hybrid drop-WP
Airburst/Split   command detonate         core fantasy              WP bomblets
Fire linger      —                        hybrid children           core fantasy
RocketJump       cancel/steer toys        hop while carpeting       hop through heat (taxed)
Sphere damage    NEVER                    NEVER                     NEVER
Ally instant heal NEVER                   NEVER                     NEVER
```

---

## 7. Crowns & Sacred Cows

### 7.1 Exotics (E6 — equal large shapes)

**A-EX1. Wire-Guidance — Exotic** (Guidance path unlock crown)  
- Enables **manual steer** of the active in-flight rocket while holding AIM.  
- Optional: release AIM or secondary input **command-detonates** (airburst at current position using manifold rules).  
- Rocket speed may reduce slightly while steering (readable control tax).  
- This is the pilot fantasy keystone.

**A-EX2. Hunter-Seeker — Exotic** (Guidance mythic)  
- Soft lock / terminal tracking within cone + radius.  
- Prefer elites / Painted / last-hurt when multiple targets.  
- Weaker and narrower than Rocket Salvo multi-lock barrage — **one rocket, smart finish**, not a heal-lock swarm.  
- Stacks with Wire-Guidance (steer + terminal assist = intended peak).

**B-EX1. M.I.R.V. Bus — Exotic** (MIRV path unlock crown)  
- On airburst command or timed fuse: split into **N Submunitions** (draft 4–7).  
- Each child: ImpactSpike and/or 2–4 micro ShrapnelRays — **budgeted**.  
- Parent ImpactSpike may be reduced or deferred to children (tune so bus ≠ parent full power × N).  
- AIM priority: airburst command when no active Wire steer claim, or tap-airburst / hold-steer split.

**B-EX2. Carpet Protocol — Exotic** (MIRV mythic)  
- While rocket is in flight, periodically **drop bomblets** along path (or on altitude gate).  
- Bomblets use mini ImpactSpike + tiny ray count.  
- Encourages lofted shots over lanes; pairs with steer for shaped carpets.  
- Hard cap on live bomblets; timeout — not infinite floor delete.

**C-EX1. White Phosphorus — Exotic** (Phosphorus path unlock crown)  
- Enables WP weather system on detonation (and optionally on airburst/children if hybrid).  
- Seeds linger emitters: **BurnProbes** apply `EffectType.Fire` on cadence to enemies in probe range.  
- Damage identity = Fire saturation → full-sat DoT + probe tick damage — **not** sphere HP aura.  
- Self-Fire: reduced apply to owner; Jump-Jet / mobility still viable with Fire Gel-like rares.  
- **No ally instant heal.**

**G-EX1. Jump-Jet Coupler — Exotic** (mobility crown — path-agnostic seat, celebrates baseline RJ)  
- Greatly amplifies rocket jump: +impulse, better directional control, reduced/removed self-tax.  
- Optional: shrapnel rays that pass near owner contribute hop crumbs (anti-sphere still).  
- Optional: mid-air reload crumb or brief glide after jump (tune — don’t outshine movement mods).  
- Mag is still the brake.  
- Intended with all three paths (steer-jump, carpet-jump, WP-jump).

### 7.2 Sacred cows (do not cut without rewriting identity)

- Parallel catalog primary; name **Manifold Rocket**  
- **No damage-sphere HP delete**  
- Baseline **generous ShrapnelRays** + ImpactSpike  
- Baseline **light rocket jump**  
- Manual steer **upgrade-only** (Wire-Guidance)  
- Phosphorus is a **full peer path** unlocked by White Phosphorus exotic  
- Jump-Jet Coupler is the **6th exotic**  
- No Salvo heal-barrage spine  
- No Thermite instant-heal spine  
- Three peer paths; hybrids OK  
- ~30 upgrades; 6 equal large exotics  
- R = reload only  
- AIM: Wire steer > MIRV airburst > ADS  

---

## 8. Full Upgrade List (~30 ship + backlog)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity  
Cell rule: Exotic shapes larger than others; all Exotics same cell count.  
Player-facing names below. API names assigned at implementation.

Wiki / sibling DNA → Manifold name (inspiration only):

| DNA | Manifold |
|-----|----------|
| Swarm Remote Control | Wire-Guidance |
| Salvo lock / Missile Bay seek | Hunter-Seeker (single-rocket, no heal barrage) |
| Salvo M.I.R.V. / Cluster Bomb | M.I.R.V. Bus |
| Mitosis / carpet fantasy | Carpet Protocol |
| Thermite scorched / Napalm (DPS only) | White Phosphorus + linger ladder |
| Classic RL jump | Jump-Jet Coupler |
| Point Detonators spirit | Needle Fuze / ray focus (no sphere) |
| Guidance Array spirit | Smart manifold bias cards |
| Decaying Warhead | optional backlog element tip |

------------------------------------------------------------------------------
PATH A — GUIDANCE
------------------------------------------------------------------------------

A-EX1. Wire-Guidance — Exotic (path unlock crown)
       Hold AIM to steer active rocket; optional command detonate.
       Enables Guidance system for supporting cards.

A-EX2. Hunter-Seeker — Exotic (mythic)
       Soft lock + terminal track; elite/Paint bias; not Salvo multi-lock heal.

A-EP1. Terminal Authority — Epic
       +Turn rate / track strength during final approach (with Hunter or Wire).
       Brief speed-up on lock confirm.

A-EP2. Paint Laser — Epic
       AIM (when not steering a live rocket) or hip rays apply **Painted**.
       Guided rockets and MIRV focus prefer Painted parts.

A-EP3. Dead-Man’s Switch — Epic
       If rocket would expire or leave world bounds, auto airburst manifold
       at last valid point (anti-whiff QoL with teeth).

A-RA1. Control Surfaces — Rare
       +Steer authority; −rocket speed slightly while steering.

A-RA2. Lock Tone — Rare
       +Soft lock acquire radius/cone; audio lock cue.

A-RA3. Part Preference — Rare
       Seeker and ray bias prefer shells/cores when available (anatomy literacy).

A-ST1. Gyro Ring — Standard
       Minor +accuracy / −spread on dumbfire (baseline lead assist crumb).

------------------------------------------------------------------------------
PATH B — MIRV
------------------------------------------------------------------------------

B-EX1. M.I.R.V. Bus — Exotic (path unlock crown)
       Airburst/fuse split into N budgeted raycast submunitions.

B-EX2. Carpet Protocol — Exotic (mythic)
       Path bomblets along flight; hard cap + timeout.

B-EP1. Focus Split — Epic
       Children prefer Painted / aim-point / last-hurt brain.
       ST claw inside MIRV clear.

B-EP2. Delayed Bus — Epic
       Children hang briefly then dive/ray (Swarm DNA cousin — still rockets).
       Hang increases micro-ray accuracy or count slightly (Wait-Charging crumb).

B-EP3. Bus Armor — Epic
       +Child ImpactSpike vs shells/plated; −child ray count slightly (ST fork).

B-RA1. Extra Seats — Rare
       +Submunition count; −per-child damage budget slightly.

B-RA2. Short Fuse — Rare
       Faster default airburst fuse; or +command airburst window.

B-RA3. Scatter Pattern — Rare
       Wider child spread (coverage fork).

B-ST1. Rack Bracket — Standard
       Minor +child damage crumb (requires M.I.R.V. Bus).

------------------------------------------------------------------------------
PATH C — PHOSPHORUS
------------------------------------------------------------------------------

C-EX1. White Phosphorus — Exotic (path unlock crown)
       WP weather via BurnProbes + EffectType.Fire; no sphere HP aura; no ally heal.

C-EP1. Linger Lattice — Epic
       +WP field duration and probe cadence.
       Full-sat enemies in field take bonus probe tick damage.

C-EP2. Flight Trail — Epic
       Rocket leaves a short Fire-probe trail while flying (denial line).
       Trail does not deal sphere damage — probe ticks only.

C-EP3. White-Hot Tip — Epic
       +ImpactSpike damage; ImpactSpike applies bonus Fire amount.
       Mild −ShrapnelRay count (ST cook fork).

C-RA1. Napalm Seats — Rare
       +Fire effect amount on spike and probes.

C-RA2. Dense Smoke — Rare
       +Probe radius / count; slight −direct spike (weather fork).
       Still probe/ray based — not damage sphere.

C-RA3. Fire Gel Sleeve — Rare
       −Self Fire apply from own WP and RJ interactions.

C-ST1. Match Head — Standard
       Minor +Fire amount on ImpactSpike when WP unlocked.

------------------------------------------------------------------------------
MOBILITY / JUMP-JET LINE
------------------------------------------------------------------------------

J-EX1. Jump-Jet Coupler — Exotic (crown)
       Big RJ amp: impulse, control, −self-tax; optional near-owner ray hop crumbs.
       Works with all paths.

J-EP1. Blast Scoop — Epic
       RJ reads detonation below/behind more generously; small horizontal dash component.

J-RA1. Light Tube — Rare
       +Move speed while this gun active; slight −rocket damage.

J-RA2. Cushioned Coupler — Rare
       −Self damage from own rockets further (stacks toward zero with Coupler).

------------------------------------------------------------------------------
RAY MANIFOLD / AP LINE (cross-path)
------------------------------------------------------------------------------

R-EP1. Needle Fuze — Epic
       On detonation: add **PierceLine** through primary hit hierarchy
       (shell→limb→core preference when possible) with retention curve.
       Still no sphere.

R-EP2. Manifold Overdrive — Epic
       +ShrapnelRay count and +length; −ImpactSpike slightly (clear fork).

R-RA1. Focused Petals — Rare
       Rays bias harder into a tighter cone along impact normal / aim.

R-RA2. Long Petals — Rare
       +Ray length; −ray count slightly.

R-RA3. Part-Seeking Shrapnel — Rare
       Rays prefer nearest enemy parts over empty air (smart manifold).

R-ST1. Extra Petals — Standard
       Minor +ray count.

------------------------------------------------------------------------------
GENERIC / GUNFEEL
------------------------------------------------------------------------------

G-RA1. Heavy Warhead — Rare
       +ImpactSpike damage; −rocket speed; +gravity slightly.

G-RA2. Fast Tube — Rare
       +Rocket speed; −ImpactSpike slightly.

G-ST1. Speed Loader — Standard
       +Reload speed.

G-ST2. Spare Tubes — Standard
       +Ammo reserves / +mag crumb.

G-OD1. Boundary Incursion — Oddity
       Increases upgrade grid size.

------------------------------------------------------------------------------
FROZEN 30 FOR V1 SHIP
------------------------------------------------------------------------------

EXOTIC (6)
  1  Wire-Guidance
  2  Hunter-Seeker
  3  M.I.R.V. Bus
  4  Carpet Protocol
  5  White Phosphorus
  6  Jump-Jet Coupler

EPIC (8)
  7  Terminal Authority
  8  Paint Laser
  9  Focus Split
 10  Delayed Bus
 11  Linger Lattice
 12  Flight Trail
 13  Needle Fuze
 14  Manifold Overdrive

RARE (10)
 15  Control Surfaces
 16  Lock Tone
 17  Extra Seats
 18  Short Fuse
 19  Napalm Seats
 20  Fire Gel Sleeve
 21  Focused Petals
 22  Part-Seeking Shrapnel
 23  Heavy Warhead
 24  Cushioned Coupler

STANDARD (5)
 25  Gyro Ring
 26  Rack Bracket
 27  Match Head
 28  Extra Petals
 29  Speed Loader

ODDITY (1)
 30  Boundary Incursion

------------------------------------------------------------------------------
BACKLOG (designed, not in first 30)
------------------------------------------------------------------------------

Guidance
- Dead-Man’s Switch
- Part Preference
- Missile-cam follow (careful)
- Detonate-on-release as separate card (if not baked into Wire)

MIRV
- Bus Armor
- Scatter Pattern
- Loft cluster loft angle toys
- Child WP seats (explicit hybrid card)

Phosphorus
- White-Hot Tip
- Dense Smoke
- Cook payoff on full-sat execute spike
- Thermite-style scorched ground visual with probe-only damage (already implied)

Jump / mobility
- Blast Scoop
- Light Tube
- Mid-air steer cancel → hop (Wire + Jump-Jet interaction card)

Ray / AP
- Long Petals
- Crack-on-spike rare
- Sabot pierce retention ladder

Generic
- Fast Tube, Spare Tubes, Range Gate, Recoil Brace
- Element tip backlog (Decay/Shock) — not identity

Explicitly rejected as identity
- Damage-sphere explosion DPS
- Salvo multi-lock ally heal barrage
- Thermite instant HP welding / IC heal engine
- Siege halo / mash breech / mag-1 Shipkiller spine
- Last Argument 8s charge spine
- Baseline free Wire-Guidance

---

## 9. Example Builds

**Pilot Surgeon (Guidance ST)**  
Wire-Guidance + Hunter-Seeker + Terminal Authority + Paint Laser  
+ Part-Seeking Shrapnel + Needle Fuze + Gyro Ring  
→ Steer onto the part, terminal finish, pierce line, manifold tidies.

**Bus Clear (MIRV)**  
M.I.R.V. Bus + Carpet Protocol + Focus Split + Extra Seats  
+ Delayed Bus + Manifold Overdrive + Short Fuse  
→ One trigger weathers the lane in raycast children.

**WP Denial (Phosphorus)**  
White Phosphorus + Linger Lattice + Flight Trail + Napalm Seats  
+ Fire Gel Sleeve + Match Head + Heavy Warhead  
→ Paint the room in Fire probes; cook packs; spike elites.

**Jump-Jet Acrobat**  
Jump-Jet Coupler + Cushioned Coupler + Wire-Guidance + Light Tube (backlog) / Fast Tube  
+ Blast Scoop (backlog) + Speed Loader  
→ Mobility-first tube; steer cancel and hop as language.

**Guided WP Strike (hybrid)**  
Wire-Guidance + White Phosphorus + Flight Trail + Paint Laser  
+ Linger Lattice + Control Surfaces  
→ Put the match exactly where the objective needs it.

**Seeking Bus (hybrid)**  
Hunter-Seeker + M.I.R.V. Bus + Focus Split + Lock Tone  
+ Extra Seats + Terminal Authority  
→ Soft lock parent, focus-split children onto Painted elite.

**Jump Carpet (hybrid)**  
Jump-Jet Coupler + Carpet Protocol + M.I.R.V. Bus + Manifold Overdrive  
→ Loft, hop, seed the floor with ray bomblets.

---

## 10. Economy & Tuning Rules of Thumb

- **Power budget lives in spike placement, ray count/geometry, bus child budgets, Fire cook, and jump ammo spend** — not explosion radius.
- Total ShrapnelRay budget must not equal N × full ImpactSpike.
- MIRV parent+children combined DPS should beat uninvested single rocket on packs and lose on pure ST to Guidance+Needle without Focus Split investment.
- WP full-sat time: grunts fast, elites medium, bosses slow/capped probe rate.
- WP must never be a hidden damage sphere — if a designer reaches for OverlapSphere damage, stop and use probes/rays.
- Hunter-Seeker must not recreate Salvo multi-target heal lock.
- Wire steer tax (speed/turn) should feel skillful, not molasses.
- Jump-Jet Coupler is allowed to feel great; mag size and reload remain brakes.
- Baseline RJ joy > baseline RJ punishment (low self-damage).
- Hybrids should be strong; watch guided MIRV WP carpet jump delete — fun first, then scalar caps (live child caps, probe rates).
- No upgrade reintroduces sphere damage as a “temporary” shortcut.

---

## 11. Status / Counter Split

| System / counter     | Role                              | Baseline? | Owner |
|----------------------|-----------------------------------|-----------|-------|
| ImpactSpike          | Primary hit damage                | Yes       | Baseline |
| ShrapnelRays         | Pack manifold                     | Yes       | Baseline |
| Light RocketJump     | Mobility crumb                    | Yes       | Baseline |
| Wire steer           | Manual pilot                      | No        | Guidance exotic |
| Hunter soft lock     | Terminal assist                   | No        | Guidance exotic |
| Painted              | Seek / focus priority             | No        | Guidance epic+ |
| M.I.R.V. split       | Submunition bus                   | No        | MIRV exotic |
| Carpet bomblets      | Path weather                      | No        | MIRV exotic |
| White Phosphorus     | Fire weather unlock               | No        | Phosphorus exotic |
| BurnProbe / Fire     | Linger cook                       | No        | Phosphorus |
| PierceLine           | Hierarchy ST                      | No        | Needle Fuze epic |
| Jump-Jet amp         | RJ build                          | No        | Jump-Jet Coupler |
| Damage sphere HP     | —                                 | **Never** | **Banned** |
| Ally instant heal    | —                                 | **No**    | Thermite’s lane |
| Salvo multi-lock heal| —                                 | **No**    | Salvo’s lane |
| Halo / breech siege  | —                                 | **No**    | Siege’s lane |

### 11.1 Fire (Phosphorus) — use real EffectType.Fire

| Tuning (draft)        | Value / note |
|-----------------------|--------------|
| EffectType            | Fire (vanilla) |
| Apply via             | ImpactSpike riders, BurnProbes, trail probes, child tips |
| Full-sat DoT          | Vanilla Fire enemy tick pattern as start |
| Self apply            | Reduced; Fire Gel Sleeve / Coupler interactions |
| Ally heal on boom     | **None** |
| Sphere HP aura        | **None** |

---

## 12. Strengths, Weaknesses & Co-op

**Strengths**
- Unique anti-sphere launcher fantasy with readable skill
- Baseline pack viability via generous rays without breaking ST forever
- Three very different endgames from one tube
- Rocket jump as joyful gunfeel + build crown
- WP primary denial without stealing Thermite heal identity
- Pilot and bus fantasies the arsenal under-serves on a primary

**Weaknesses**
- Low brain-off DPS vs hose guns
- Mag pressure; whiffed rockets hurt
- Close scramble weaker than SMG without RJ practice
- Steer skill ceiling may frustrate some players (Hunter is the on-ramp)
- WP self-Fire mismanagement without Gel
- Parallel weapon = must find/unlock (by design)

**Co-op**
- No team-heal rocket identity (avoids Salvo overlap and grief ambiguity)
- WP fields should not grief allies with heavy self-Fire (ally Fire apply low/off)
- Knockback RJ uses vanilla friendly policies — don’t feature ally pit-yeets
- Guided rockets need clear ally-readable trails
- Carpet/MIRV FX budget so the screen stays legible

---

## 13. Visual, Audio & Thematic Design

**Appearance**
- SAXON industrial recoilless tube: hazard stripes, manifold vent ring at muzzle,
  “NO SPHERE CHARGE” stencil joke optional, fungal-etched range tables
- Guidance: glowing control fins, wire spool LEDs, lock diamond
- MIRV: bus collar rings, child pods that visibly peel on split
- Phosphorus: white-hot tip, chalky smoke, lingering ember probes (not orange sphere blob alone)
- Jump-Jet: coupler nozzles near stock; jump leaves brief thruster soot

**Sound**
- Baseline: tube thump + rocket whoosh + **ray crackle manifold** (distinct from vanilla boom)
- Steer: soft servo whine while AIM held
- Lock: crisp tone (not Salvo barrage choir)
- MIRV split: mechanical peel + multiple mini whooshes
- WP: harsh chemical hiss + quiet probe ticks
- RJ: body kick + thruster cough (Coupler = louder ordered burst)

**Flavor / codex line (in-game style)**
  Manifold Rocket  
  Semi-automatic rocket primary.  
  Detonations deal ImpactSpike and ShrapnelRay damage (no damage spheres).  
  Guidance upgrades enable steering and seeking.  
  MIRV upgrades split into raycast submunitions.  
  Phosphorus upgrades enable white phosphorus Fire weather.  
  Jump-Jet Coupler amplifies rocket jump.

---

## 14. Implementation Notes (for later)

### 14.1 Gear registration
- Follow weapon template in this repo: clone base gun, GearInfo high-range id,
  APIName `manifold_rocket`, behaviour component, SpawnGear stamp, CreateUpgrade pool.
- **Clone candidate:** prefer a projectile gun with explosion-adjacent VFX if available
  (Gunship / launcher-like). Else any projectile Gun and replace detonation damage path.
  Do **not** rely on vanilla explosion damage spheres remaining active.
- Plugin: GUID `sparroh.manifoldrocket`, MycoMod **IsSandbox**.
- Persistence: stable gear id; register before `PlayerData.OnAwake` AddGear.
- Working gear id band: pick free high-range id at impl (e.g. 95xxx) — confirm unused.

### 14.2 Behaviour host
ManifoldRocketBehaviour (or true Gun subclass when prefab exists):
- WeaponData: ray count/length/budget, RJ params, steer flags, seek params,
  MIRV child rules, WP probe params, pierce flags, AIM claim flags
- Runtime: active rocket ref, steer state, lock target, live children list,
  WP emitter list, jump cooldown crumb
- Prefab snapshot restore on upgrade Remove

### 14.3 Detonation pipeline (critical)

```
OnRocketImpact / OnAirburst:
  1. Resolve primary hit part (if any) → ImpactSpike DamageData (Precision-ish flags)
  2. Build ray directions (cone + part-seek bias)
  3. For each ray: raycast → damage events (budgeted)
  4. If PierceLine: walk hierarchy with retention
  5. If WP: spawn probe emitter (Fire apply ticks) — NO sphere damage loop
  6. If MIRV split: spawn children with child budgets; do not recurse infinitely
  7. If owner in RJ range: ApplyRocketJump impulse
  8. Play VFX boom (visual only)
```

Harmony / hooks likely:
- Gun.OnFiredBullet — tag rocket, attach steer/MIRV/WP payload component
- Bullet impact / damage callbacks — replace or zero sphere branch; run manifold
- Search decompile for explosion / DamageSphere / hitForce radius paths on chosen base gun
  and **disable damage portion** while keeping FX if needed

### 14.4 Guidance
- While Wire-Guidance and rocket alive and AIM held: each tick bend velocity toward look
- Hunter: acquire target in cone; terminal velocity lerp
- Paint Laser: separate AIM mode when no live rocket

### 14.5 MIRV
- Airburst input or fuse timer → spawn N child projectiles or instant child detonations
- Carpet: interval drops while parent alive
- Caps: max live children, max carpet seeds

### 14.6 WP emitters
- Lightweight tickers: every dt, short probe rays or filtered neighbor queries that
  **only** apply Fire amount / tiny probe damage — document why any overlap query
  is not a damage sphere (status apply only, strict caps)
- Prefer ray probes for consistency with sacred cow

### 14.7 Rocket jump
- On detonation: vector = owner.position - detonation.position (or up-bias blend)
- Baseline impulse low; Coupler multiplies
- Optional ignore or reduce self ImpactSpike when jump-tagged

### 14.8 Hooks

| Hook | Use |
|------|-----|
| OnFiredBullet | Attach payload; start fuse; trail WP |
| Bullet impact / explode | Manifold pipeline; zero sphere damage |
| AIM input | Steer > airburst > paint > ADS |
| OnBeforeDamage | AP/pierce/Fire riders; ray budget flags |
| OnDamageTarget | Paint; charge crumbs if any |
| OnSaturateTarget | WP full-sat payoffs |
| Owner damage from self | RJ tax / Gel |

### 14.9 HUD
- Live rocket indicator when Guidance/MIRV command relevant
- Lock tone / diamond when Hunter equipped
- WP field subtle edge when standing in own probes (optional)
- Prefer SparrohUILib if dependency acceptable

### 14.10 VFX / audio priority
1. Ray manifold crackle readable on every detonation
2. ImpactSpike punch distinct from rays
3. Steer servo + lock tone
4. MIRV peel + child whoosh
5. WP hiss + probe ticks
6. RJ thruster kick
7. Visual boom without lying about sphere damage

### 14.11 Multiplayer
- IsSandbox; identical mod on all clients
- Rocket ownership = firing player; steer input owner-authoritative
- Children/WP emitters owned by firer; damage follows IDamageSource patterns
- Cap FX replication so carpet+WP doesn’t melt netcode

---

## 15. Deliberate Non-Goals

- Not damage-sphere launcher meta  
- Not Rocket Salvo heal-lock barrage  
- Not Last Argument charge sniper  
- Not Siege halo / breech siege trio  
- Not Thermite instant-heal / IC heal engine  
- Not Swarm hover-pellet primary  
- Not baseline free Wire-Guidance  
- Not ally HP rockets  
- Not requiring custom Unity prefab for v1 (runtime clone OK)  
- Not Chaingun spool/plant  
- Not Friend pet AI  

---

## 16. Open Tuning Questions (playtest, not design blockers)

1. Mag 4 vs 5 vs 6?
2. Baseline ray count 10 vs 14 and damage split curve?
3. Baseline RJ impulse and self-damage (zero vs token)?
4. Wire steer speed tax amount?
5. Hunter lock cone/radius vs Salvo feel creep?
6. MIRV child count 4–7 and parent power deferral?
7. Carpet drop interval and live seed cap?
8. WP probe cadence and boss cap?
9. AIM: hold-steer / tap-airburst split when both equipped?
10. Needle Fuze retention per pierce step?
11. Jump-Jet Coupler horizontal dash strength vs movement mod creep?
12. Clone base gun choice after AllGear audit?
13. Unlock method: auto-unlock like template vs progression?

---

## 17. Success Criteria / Player Fantasy Checklist

- [ ] Baseline dumbfire rocket works with zero upgrades
- [ ] Detonation deals ImpactSpike + visible/audible **ray manifold** — packs die without spheres
- [ ] No OverlapSphere-style multi-target HP delete on stock or upgraded paths
- [ ] Light rocket jump works at baseline and feels joyful
- [ ] Jump-Jet Coupler makes RJ a real build without infinite flight
- [ ] Wire-Guidance alone makes pilot fantasy obvious
- [ ] Hunter-Seeker assists without becoming Salvo multi-lock heal
- [ ] M.I.R.V. Bus splits into budgeted raycast children
- [ ] Carpet Protocol seeds a lane without floor-delete infinity
- [ ] White Phosphorus cooks via Fire probes/DoT, not sphere aura
- [ ] No ally instant heal rockets
- [ ] Needle Fuze pierce line reads as ST tool
- [ ] Manifold Overdrive reads as clear fork
- [ ] Hybrids (guided WP, seeking bus, jump carpet) feel intentional
- [ ] Frozen 30 ships clean
- [ ] AIM priority respects Wire steer > MIRV airburst > ADS
- [ ] VFX boom does not mis-teach sphere damage

---

## 18. Review Decisions Locked (2026-08-07)

| Decision | Lock |
|----------|------|
| Form factor | Man-portable dumbfire rocket primary |
| Player-facing name | **Manifold Rocket** |
| Slot | Primary |
| Paths | Guidance / MIRV / Phosphorus |
| Damage model | Raycast manifold only (no damage spheres) |
| Baseline rays | Generous ShrapnelRays + ImpactSpike |
| Manual control | Upgrade-only (Wire-Guidance) |
| WP depth | Full peer path (Option A) |
| Rocket jump | Light baseline + Jump-Jet Coupler exotic |
| 6th exotic | Jump-Jet Coupler |
| Pierce | Needle Fuze Epic (not 6th exotic) |
| Heal rockets | No |
| Product shape | Parallel weapon |
| Ship pool | Frozen 30 listed above |
| Crowns | Wire-Guidance, Hunter-Seeker, M.I.R.V. Bus, Carpet Protocol, White Phosphorus, Jump-Jet Coupler |
| Doc file | ManifoldRocket-DesignDoc.txt |
| Tone | SAXON industrial recoilless / anti-sphere doctrine |
| MycoMod | IsSandbox at implementation |
| Working APIName | manifold_rocket |
| Working GUID | sparroh.manifoldrocket |

---

## 19. Changelog

### v1 (2026-08-07)
- Initial full design from locked user decisions
- Paths: Guidance (pilot/seek), MIRV (bus/carpet), Phosphorus (WP Fire weather)
- Sacred anti-sphere damage model with ImpactSpike + generous ShrapnelRays baseline
- Rocket jump: light baseline + Jump-Jet Coupler exotic
- Manual steer upgrade-only
- WP full peer path unlocked by White Phosphorus exotic
- Research anchors:
  - Wiki: Rocket Salvo, M.I.R.V., Point Detonators, The Last Argument, Swarm
    (Remote Control, Guidance Array, Mitosis, Cluster), Incendiary/Napalm/Cluster,
    Gunship Missile Bay
  - Sibling docs: Siege Cannon (shells/halo/ordnance/AP), Heaven Piercer (3-path bible),
    Thermite (Fire heal — do not copy heal), DMLR (on-hit > % stickers)
  - User locks: name Manifold Rocket; primary; paths OK; steer upgrade-only;
    generous baseline rays; WP = full path A; RJ baseline + Jump-Jet Coupler exotic
- Frozen 30 + backlog + impl pipeline notes

---

## 20. Implementation checklist (post-design)

- [ ] Rename plugin/csproj/thunderstore from template → ManifoldRocket
- [ ] ManifoldRocketBehaviour.Data fields from §14.2
- [ ] Clone projectile base; **strip sphere damage**; implement manifold pipeline
- [ ] Baseline ImpactSpike + ShrapnelRays + light RJ
- [ ] Guidance: Wire steer, Hunter lock, Paint, AIM router
- [ ] MIRV: Bus split, Carpet seeds, Focus Split, child caps
- [ ] Phosphorus: WP emitters, BurnProbes, Fire apply, Flight Trail
- [ ] Jump-Jet Coupler amp + Cushioned Coupler
- [ ] Needle Fuze PierceLine + Manifold Overdrive
- [ ] UpgradeRegistration frozen 30
- [ ] HUD: lock / live rocket / optional WP edge
- [ ] Persistence + SpawnGear stamp
- [ ] Playtest pass on §16 knobs
- [ ] Verify no code path deals OverlapSphere HP on this gear
- [ ] Verify no ally instant heal; no Salvo heal identity creep
