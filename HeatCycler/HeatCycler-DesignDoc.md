# Heat Cycler – Design Document (v3)

## 1. High Concept / Fantasy

High fire-rate energy SMG with **infinite ammo** and a **Heat** resource instead of magazines.
The gun wants to stay up: ride the bar, breathe the trigger, and spend heat on purpose.
At its mythic peak (**Infinity Burn**), the bar goes past full and the room melts with you.

**One-liner:** A hose that runs on heat — redline is power, rhythm is uptime, conversion is cash-out — braided through Fire, Shock, and Acid.

**Fantasy anchors (non-negotiable in playtests)**
- Most damage is still **bullets downrange** — elements and R amplify or punctuate; they do not replace the hose.
- The heat bar changes your trigger finger within ~5 seconds of combat.
- Every strong build answers one of: “I run hot” / “I never stop” / “I cash out” — without needing five saturate cards to explain it.


## 2. Role & Fantasy in the Arsenal

- **Slot:** Primary
- **Range:** Close to mid (aggressive pressure; long-range hose is not the fantasy)
- **Role:** Sustained projectile volume, elemental application engine, high uptime pressure
- **Gap filled:** Vanilla Cycler is mag/reload tempo. Heat Cycler is continuous fire with a readable heat skill curve and spend tools on R.
- **Synergies:** Offhand damage can cool you (Radiator); multi-element grids and Cyclotron feed interlacing; movement (sprint/slide) supports Rhythm builds.


## 3. Design Pillars

1. **Heat is the magazine and the skill expression** — meaningful upgrades answer how you gain, hold, spend, or survive heat. Infinite ammo means heat carries **100%** of resource tension; there is nowhere to hide a weak curve.
2. **High uptime is the baseline fantasy** — the gun almost never fully stops; downtime is a stutter-step, not a reload animation.
3. **Redline is a stance, not a free win** — high heat feels strong *and* tense. Base redline should be near break-even or situationally better, not strictly optimal DPS in all ranges. Upgrade carrots (especially Fire) make living hot the dream.
4. **Fast bullets downrange** — low per-hit damage, high RoF, projectile SMG; builds multiply throughput or heat efficiency. Proc/nova power must not outshine the hose.
5. **Three readable paths that intermingle** — pure builds are complete and strong first; hybrids are intentional bonuses, not the only correct endgame.
6. **No hard upgrade count cap** — ship what serves identity; quality over a fixed total.
7. **Elemental interlacing** — primary elements are **Fire, Shock, Acid**. Each teaches a real **heat verb** (not only a DoT color). Multi-element setups create cross-talk. Decay is out by default.
8. **Ship order discipline** — finish pure Fire / Shock / Acid path spines before deepening interlace. Cyclotron is a showcase, not a tax.


## 4. Core Mechanics & Gunfeel

### 4.1 Base gun

- Automatic, projectile
- Low damage, high fire rate, infinite ammo (no magazines / no reload loop)
- Gunfeel (model, sounds, baseline ballistics) borrowed from vanilla Cycler unless noted
- Standard movement; sprint-fire gated behind upgrade (Extralight Frame)

### 4.2 Heat resource (Soft Redline baseline)

| Param | Value | Intent |
|---|---|---|
| MaxHeat | 100 | Readable ceiling for HUD |
| HeatPerShot | 2.4 | ~3.5s Cold→Redline at ~vanilla RoF if pure hold |
| DissipateDelay | 0.15s | Forgives micro gaps; stutter-fire is intentional |
| DissipatePerSecond | 55–65 | Prefer holding a thermal mass over instant full reset; tune so Converter can bank a bar |
| Hard lockout | **None (base)** | No HOT stun on base gun |
| Overcap | Off unless granted (Infinity Burn, etc.) | Classic fantasy is upgrade-owned |

**Derived feel targets**
- Pure hold to redline: ~3.2–3.8s
- Breath (0.25s off trigger): ~14–16 heat dumped — meaningful, not a full reset
- Full idle reset: ~1.8–2.2s including delay (slightly slower than v2’s ~1.7s if dissipate is eased)
- Main dials: HeatPerShot = “mag length”; DissipatePerSecond = “reload speed”

**v3 note on dissipate:** v2’s 65/s full-bar idle (~1.5s) yoyos the bar and starves Converter “hold to cash out.” Prefer the slower end of the band unless Rhythm feels sluggish in playtests.

### 4.3 Heat zones

| Zone | Range | Base behavior |
|---|---|---|
| Cold | 0–39% | Baseline stats only |
| Warm | 40–69% | **Teaching band (v3):** small hook so the climb is a ladder, not dead air — e.g. +3–5% element application **or** DissipateDelay ×0.85 (pick one; keep tiny) |
| Hot | 70–99% | **Carrot band:** +8% damage, +10% element application (if any) |
| Redline | 100% (clamped) | Soft brake + modest carrot; still fully playable |
| Overcap | >100% | Only with Infinity Burn (or similar) |

Zone thresholds are constants; upgrades may shift them (e.g. Hot starts at 60%).

**Warm must not stay pure cosmetic forever.** A dead 30% band teaches nothing on a skill-expression resource.

### 4.4 Soft Redline (at 100%, no overcap)

When a shot would push heat above MaxHeat:

1. Clamp heat to MaxHeat (stay at 100%).
2. **Still fire** (uptime preserved).
3. **Redline Brake (base):** choose a *felt* primary brake (see stance rules below).
4. **Redline Carrot (base):** modest — enough to make redline tempting, not strictly best DPS everywhere.
5. Feel: barrel glow / heat audio / subtle feedback — ladder must be unmistakable without staring at the bar alone.

**Redline is a stance** — wilder, meaner *with opportunity cost* — not a timeout and not a free buff bar you pin forever.

**Base redline is NOT:** self-damage, forced vent, input lock, or heat decay while holding M1.
You stay pinned at 100% until you release fire or spend heat.

#### Soft Redline stance rules (v3 — critical)

**Goal:** pinning 100% is a real choice, not the always-correct hold-M1 default.

| Rule | Guidance |
|---|---|
| Base DPS at redline | Near **break-even** vs Hot band at intended mid-close range after brake+carrot. Situationally better in melee chaos; worse at long hose range. |
| Primary brake | Prefer **one clear felt cost**. Default lean: **mild RoF brake + modest spread** (hose stays aggressive close-mid). Avoid stacking strong carrot + strong Mass Accel + element mults until redline is free power. |
| Spread | Keep modest on base (e.g. ≤ ×1.20). Heavy spread fights PPSH fantasy; range falloff of accuracy is the long-range tax, not a shotgun bloom. |
| Carrot ownership | **Base carrot stays modest** (e.g. damage ~×1.08–1.12, element ~×1.12–1.18). Big “melt while hot” power is **upgrade-owned** (Ember Feed, Scorched Chamber, Mass Accel, IB). |
| No passive heat bleed while M1 | Still locked. If redline feels free in playtests, tighten brake/carrot first — do not add M1 decay as a first fix. |
| Mass Accel interaction | Mass Accel may make cold feel weak by design, but **base redline + Mass Accel must not trivially dominate** without IB/Fire package investment. Tune efficiency so Hot→Redline is the climb reward, not an instant win at 40% heat. |

**Failure state to avoid:** players learn “hold M1 forever, vent only in melee panic” and heat becomes cosmetic.

### 4.5 Cooling rules

| Condition | Behavior |
|---|---|
| Firing | +HeatPerShot per bullet; no dissipate |
| Not firing, within DissipateDelay | No dissipate |
| Not firing, after delay | −DissipatePerSecond |
| Redline + holding fire | Stay at 100%; brake+carrot active |
| Redline + release | Delay then dissipate; exit redline when heat < 100% |
| Stow / swap | **Continue dissipating** (preferred) |

### 4.6 Base R — Pressure Vent

R is always a heat verb on this gun.

**Hard rule (v3):** R **spends or transforms** heat / heat-linked value. R must not primarily *add* heat as its player-facing read (see Energy Convergence).

**Input**
- Tap R (press < 0.25s): Pressure Vent
- Hold R: same as tap on **base** kit (hold-to-channel is upgrade-owned and must not fight Dump)

**Requirements**
- CurrentHeat ≥ 15, else empty feedback
- Not in Vent recovery

**Effect (base)**

| Piece | Value |
|---|---|
| Heat spent | min(CurrentHeat, 35) flat |
| Timing | Instant (no charge) |
| Damage | Small radial pulse, r ≈ 3.5m, damage ≈ 18–22, **no element** on base |
| Utility | Drops heat (clears Redline if at 100%); bypass DissipateDelay for 0.35s after vent |
| Recovery | 0.45s before R can vent again (fire still allowed) |
| Move | No root; optional tiny recoil bump |

**Intent:** always useful teaching tool; weak enough that Dump / Discharge / Convergence still matter.
Saturate Catalyst and other vent scalers must not let base Vent outshine Capacitor Dump.

**R ownership priority (design intent when stacked)**
1. Capacitor Dump (exotic) — replaces vent with full heat→cone
2. Energy Convergence — **amplifies next spend / converts stored value**; must not steal Dump’s tap to dump-add-heat only (see §7 / §8.8)
3. Elemental Discharge — vent becomes elemental nova
4. Else Pressure Vent baseline

### 4.7 Base combat loop (no upgrades)

```
Hold M1 → bar climbs → Warm (small hook) → Hot (feels better) → Redline (stance: strong but costly)
   ↘ tap R to dump ~35 heat + peewee pulse
   ↘ or release ~0.2–0.4s to breathe off heat
   ↘ return to Hot band and re-hose
```

Skill without upgrades: ride Hot/Redline, breath-tap, panic-vent in melee, don’t face-tank redline accuracy at long range.


## 5. Elemental Interlacing (Pillar 7)

Elements are not three parallel DoT stickers. Each teaches a **heat verb**; multi-element setups create **cross-talk**.

| Element | Heat verb | Play feel | Natural path lean | Completeness bar |
|---|---|---|---|---|
| **Fire** | Pressure / Redline fuel | Sustained apply; rewards staying hot; ignites can *add* heat; detonations & overcap | Redline | Must generate or multiply pressure — not only ride zone carrots |
| **Shock** | Tempo / refund | Saturate → cool or move; keeps the hose alive | Rhythm | Refunds with a **ceiling** so the hose still climbs |
| **Acid** | Bank / convert | DoTs and stacks become stored value you **spend on R** or ride | Converter | Bank is currency, not only +flat damage until heat empties |

**Rules**
- Single-element builds are complete and strong **without** Cyclotron or interlace epics.
- Two-element builds unlock interlace cards.
- Three-element / Cyclotron builds are the “messy genius” fantasy — celebrated, **not required**.
- **Decay is not a primary pillar** (no default applicator).
- **Bullet primacy:** saturate procs, novas, and braid windows punctuate; they do not become the build’s main DPS identity.

**Cross-talk examples**
- Shock saturate while target is ignited → bigger heat refund and/or fire splash (Crossflash) — tune so hybrid ≠ strict pure dominate
- Acid Bank consumed on Vent/R/Dump → bonus spend damage (Solvent Capacitance)
- Igniting a corroded target → lite detonation + Bank (Pyrolysis)
- Cyclotron braids all three verbs in one spray

**Paths are heat strategies; elements are the tools that feed them.**

### 5.1 Path × Element matrix (target fantasy)

```
                 REDLINE              RHYTHM               CONVERTER
Fire          fuel, detonate,       ignite keeps         dump carries
              overcap power         pressure during      burn payload
                                    breath windows

Shock         refunds extend        saturate = tempo     spends can
              time-on-redline       engine (capped)      chain-shock
              (support, not delete)

Acid          stacks scale with     corrosion uptime     bank → R spend
              heat% / overcap       via DoT value        multiplier (core)
```

### 5.2 Pure-path minimum complete kits (ship before more interlace depth)

**Pure Fire Redline — minimum complete**
- Applicator: Heated Battery
- Pressure verbs: Ember Feed **and/or** Scorched Chamber (at least one heat-scaling apply card)
- Optional pressure gen: Thermal Runaway (ignite → +heat)
- Spine: Mass Acceleration, Thermal Buffer, Charge Shield, Cycling Repairs
- Crown: Infinity Burn
- Finisher: Scorching Detonation
- *Hybrid seam (optional):* Shock Recursion to extend overcap — support, not required

**Pure Shock Rhythm — minimum complete**
- Applicator: Charged Cartridges
- Engine: Shock Recursion (**with refund cap** — see §10)
- Tempo: Adrenaline Vent / kill refunds, Quick Sink, movement cool (Perpetual Motion / Extralight / Heat Slicks as available)
- Crown: Closed Loop
- Support: Momentum Overload
- *Hybrid seam (optional):* Crossflash + Heated Battery

**Pure Acid Converter — minimum complete**
- Applicator: Corrosive Plasma
- Bank: Toxin Bank (currency rules in §5.3)
- Spend mult: **Solvent Capacitance** (Bank → next R-spend)
- Crown: Capacitor Dump (short, mean cone)
- Support: Capacitor Plates and/or Energy Convergence (as spend amplifier — §8.8)
- Optional uptime: Corrode Radiator
- *Hybrid seam (optional):* Pyrolysis + Heated Battery for bank gen

**Interlace depth (after pures feel done)**
- Crossflash, Pyrolysis, Acid Spark, Braid Protocol, Violent Reaction, Tri-Valve, Saturate Catalyst, Cyclotron showcase

### 5.3 Toxin Bank rules (v3 — LOCKED direction)

**Problem with v2:** clear only when `heat == 0` is path-asymmetric (Redline never clears; Dump/refunds brick bank by accident).

**v3 rules**
- Bank stacks grant stored value (bonus damage per stack and/or spend power).
- **Clear / spend triggers (intentional):**
  - Full Capacitor Dump consumes bank into dump power (via Solvent Capacitance or built-in Dump×Bank).
  - Acid Spark spends 1 stack (unchanged).
  - Optional: explicit flush on Elemental Discharge.
- **Do not clear solely because heat hit 0** from breath, Closed Loop, or refunds.
- Optional safety: slow stack decay over long idle (seconds), not combat yoyo.
- Redline may still *scale* bank effect with heat% via upgrades — that is feature, not the only clear rule.

### 5.4 Shock refund guard rails (v3)

High RoF + uncapped saturate refunds = resource deletes itself in dense packs.

| Guard | Intent |
|---|---|
| Per-source refund values stay juicy | Shock Recursion still feels like the Rhythm engine |
| **Global heat refund cap / sec** (hidden or stated) | Prevents Recursion + Crossflash + kills + phase Coolant from erasing HeatPerShot |
| Efficiency mults don’t full-stack multiply | Closed Loop × Braid × phase Coolant → take **best** or diminished combine |
| Closed Loop is Rhythm’s answer to stuck-at-100% | Must not become universal infinite-fire when paired with IB |

Tune caps in playtests so a pure Shock room-clear still climbs into Hot/Redline under continuous fire without breath — but cannot sit at 0 heat while hosing forever.


## 6. Upgrade Paths (gravity wells — hybrids intended)

### Path A — REDLINE (priority fantasy)
**“The hotter I am, the more I melt the room.”**

- **Spine:** stats scale with heat%; Fire native; overcap is the dream
- **Crown:** **Infinity Burn** — overcap allowed; self-DoT; **outgoing power while overcapped**
- **Supports:** Mass Acceleration, Thermal Buffer, Charge Shield, Cycling Repairs, Scorched Chamber, Ember Feed, Scorching Detonation, Thermal Runaway
- **Hybrid hooks:** Shock refunds extend time-on-redline (capped); Acid banks turn overcap into dump fuel

### Path B — RHYTHM (uptime / tempo)
**“I almost never stop shooting.”**

- **Spine:** heat refunds (capped), fast sinks, movement cool, kill vents; Shock native
- **Crown:** **Closed Loop** (and/or Shock Recursion as epic engine)
- **Supports:** Adrenaline Vent, Quick Sink, Perpetual Motion, Extralight, Momentum Overload, Equipment Radiator, Arc Coolant
- **Hybrid hooks:** refunds feed Redline uptime; cool windows set up Converter spends

### Path C — CONVERTER (heat as currency)
**“I build heat, then cash it.”**

- **Spine:** R-spends, stored stacks, short mean dumps/novas; Acid native banker
- **Crown:** **Capacitor Dump** — spend current heat → directed power ∝ heat spent (+ Bank if present)
- **Supports:** Solvent Capacitance, Energy Convergence (spend amp), Elemental Discharge, Rocket Vent, Toxin Bank, Capacitor Plates
- **Hybrid hooks:** Redline builds bigger dumps; Rhythm lets you dump more often

**Path balance intent:** Rhythm and Redline can de-emphasize R; Converter *is* R. Converter’s supporting kit is first-class, not stretch leftovers.


## 7. Crowns & Sacred Cows

### Infinity Burn (Exotic) — Redline crown / classic fantasy
- Heat may exceed MaxHeat up to ~2× (e.g. hard cap 200)
- Per-shot heat continues to add in overcap
- While overcapped: self-DoT (+ optional self-element) scales with overcap depth
- **Outgoing damage and/or Fire application and/or RoF also scale with overcap depth**
- Soft redline brake may relax or invert while overcapped (gun wants you there)
- No hard lockout; R still spends heat as safety valve / Dump fuel
- Must feel *good* to live in overcap **as a build** (Shield/Repairs/heal package), not only masochistic and not free power without investment
- **Closed Loop + IB:** micro-vent should be a safety valve the player can lean on, not an automatic stall of mythic form every 1.5s if the build wants to stay overcapped — tune sustain timer / vent size or suppress auto-vent while overcapped if playtests demand

### Capacitor Dump (Exotic) — Converter crown
- **Replaces** Pressure Vent
- Spend **current heat** → forward **short, mean** narrowing cone (violent exhale, not turret mode)
- Damage ∝ heat spent; **bonus from Bank / Solvent Capacitance**
- Brief ~0.15s self-hitch (decision, not free extra DPS every frame)
- Duration biased **short** so hose identity returns immediately
- Usable anytime heat > threshold — **not** tied to lockout or broken charge-fire
- Clear bar dump VFX/audio
- **Bank interaction:** Dump is a primary intentional Bank cash-out (see §5.3)

### Cyclotron (Oddity/Epic) — sacred fun / interlacing enabler
- Every N shots cycle bullet element **Fire → Shock → Acid → …**
- Fixed application amount per elemental shot
- Showcase of pillar 7; treat as identity piece, not a joke stub
- **Not required** for strong pure builds
- Optional: small heat verb tick on each cycle step
- Note: stacking Cyclotron + a single full applicator can overwrite bullet element — acceptable chaos; document in tooltip if confusing

### Cycle Phasing (Oddity) — keep the toy, fix readability
- On spray start, lock one mode for the entire hold
- **HUD must show locked mode**; if not clean in mod constraints, **demote or cut modes** before shipping confusion into the identity kit
- Efficiency / Coolant modes respect §5.4 stacking rules
- Draft modes (heat/element first):

| # | Mode | Effect during this spray |
|---|---|---|
| 0 | Coolant | On hit: −0.5–1 heat (cap refund/sec — shares global refund budget) |
| 1 | Pyre | +Fire appl |
| 2 | Storm | +Shock appl; saturate → small refund (capped) |
| 3 | Solvent | +Acid appl; +Bank chance |
| 4 | Split | +1 pellet; HeatPerShot ×1.25 |
| 5 | Needle | +pierce or +bounce |
| 6 | Bleed-Off | Every 0.5s auto-spend 5 heat for tiny aim-point splash |
| 7 | Spike | Soft overcap lite: heat may go to 120% this spray with mild self-tick |

### Closed Loop (Exotic) — Rhythm crown
- If you would sit at Redline ≥ ~1.5s continuous, auto micro-vent ~25 heat and gain ~1s HeatPerShot ×0.7
- Turns “stuck at 100%” into tempo, not a brick
- Efficiency window respects diminished stacking with Braid / phase Coolant
- See IB interaction note above

### Support exotics
- **Scorching Detonation** — kill fully ignited target → explosion (Fire finisher)
- **Violent Reaction** — hit target with 2+ of {Fire, Shock, Acid} → spread those elements nearby
- **Corrode Radiator** — corroded enemies periodically dissipate your heat while acid DoT ticks
- **Condensed Ejection** — slow magnetic projectiles, +damage, in-flight arcs (Converter projectile identity; still secondary to bullet volume fantasy)
- **Boundary Incursion** — grid grow (universal oddity)

**Crown clarity:** one mythic crown per path (IB / Closed Loop / Dump). Support exotics must not outshine crowns.


## 8. Full Upgrade Brainstorm

**Tags:** R Redline · Y Rhythm · C Converter · G Glue · F Fire · S Shock · A Acid · X Interlace · H Heat verb

Rarities are flexible. Names may differ from vanilla. This is a brainstorm ceiling, not a mandatory ship list.

**Priority tags (v3)**
- **P0** — pure-path minimum complete (ship / finish before more X depth)
- **P1** — strong supports / crowns
- **P2** — interlace & stretch
- **P3** — backlog

### 8.1 Glue / gun stats

| Name | Rarity | Tags | Pri | One-liner |
|---|---|---|---|---|
| Focused Lenses | Std | G | P1 | +falloff start/end range |
| Precision Fire | Std | G | P1 | Tighter spread, slower fire rate |
| Stability Module | Std | G H | P1 | −ADS recoil; while ADS, DissipateDelay ×0.5 |
| Overclocked Firing | Std | G H | P1 | Faster fire; +spread; slight +HeatPerShot (pair with spend/refund tools) |
| Extralight Frame | Epic | G Y | P1 | Fire while sprinting |
| Ricochet | Rare | G | P2 | +1 bounce |
| Double Bore (Double Shot) | Epic | G H | P1 | +1 bullet/shot; HeatPerShot ×1.35; +spread — needs economy support |
| Smart Slide | Rare | G Y | P2 | Mild tracking while sliding |
| Full Spectrum | Epic | G | P2 | +damage per distinct rarity among other equipped upgrades |
| Boundary Incursion | Oddity | G | P2 | +row or column on upgrade grid |

### 8.2 Heat economy

| Name | Rarity | Tags | Pri | One-liner |
|---|---|---|---|---|
| Thermal Buffer (Bigazine) | Std | H R | P0 | MaxHeat ×1.9–2.15 |
| Quick Sink (Compact) | Std | H Y | P0 | MaxHeat −8–12; DissipatePerSecond ×1.3–1.45 |
| Dense Core (Doublestack) | Std | H R | P1 | MaxHeat ×1.2–1.35; fire interval ×1.08–1.14 |
| Hot Load (Bulging) | Std | H R | P1 | +flat damage; HeatPerShot +0.15–0.25 |
| Heat Slicks | Std | H Y | P1 | While moving, HeatPerShot ×0.85–0.9 |
| Aftercool | Std | H Y | P2 | After vent or breath ≥0.2s, Dissipate ×1.5 for 0.6s |
| Redline Governor | Rare | H R | P1 | Redline RoF brake halved; carrot unchanged |
| Redline Throttle | Rare | H R | P1 | Stronger redline carrot; slightly harsher brake |
| Adrenaline Vent | Rare | H Y | P0 | On kill: instantly −12–20 heat (counts toward refund budget) |
| Equipment Radiator | Rare | H Y C | P1 | Other-gear damage dissipates heat (per ~60 dmg) |
| Mass Acceleration | Epic | H R | P0 | Worse base interval; RoF scales up with heat — tune per §4.4 |
| Lite Exhaust | Epic | H G | P2 | Base RoF down; bullet speed scales with effective RoF |
| Cycling Repairs | Epic | H R | P0 | Heal every N shots; heal ×1.5 when Heat ≥70% |
| Charge Shield | Rare | H R | P0 | While Hot/Redline/Overcap, damage taken ×0.75–0.85 |
| Perpetual Motion | Epic | H Y | P1 | Sprint/slide dissipate heat even during DissipateDelay |
| Closed Loop | Exotic | H Y | P0 | Sustained Redline → auto micro-vent + short efficiency |
| Capacitor Plates | Rare | H C | P0 | Lower vent min cost; more effect per heat spent |

### 8.3 Element applicators (primary three)

| Name | Rarity | Tags | Pri | One-liner |
|---|---|---|---|---|
| Heated Battery | Rare | F | P0 | Bullets apply Fire buildup |
| Charged Cartridges | Rare | S | P0 | Bullets apply Shock buildup |
| Corrosive Plasma | Rare | A | P0 | Bullets apply Acid buildup |

### 8.4 Fire package (pressure verb — finish P0)

| Name | Rarity | Tags | Pri | One-liner |
|---|---|---|---|---|
| Ember Feed | Rare | F R H | **P0** | Heat ≥70%: +Fire application per shot |
| Scorched Chamber | Epic | F R | **P0** | Redline/Overcap: Fire application ×1.4–1.6 |
| Superheat Reaction | Epic | F R | P1 | Rapid ignites stack +Fire/shot; fall off without ignites |
| Scorching Detonation | Exotic | F R | P0 | Kill fully ignited target → explosion |
| Thermal Runaway | Epic | F R H | **P0** | Igniting a target adds +2–4 heat (Fire generates pressure) |
| Immolation Rhythm | Rare | F Y | P2 | ≥1 ignited target nearby: reduced DissipateDelay |

### 8.5 Shock package (tempo verb — guard rails)

| Name | Rarity | Tags | Pri | One-liner |
|---|---|---|---|---|
| Shock Recursion | Epic | S Y H | P0 | Fully shocking a target refunds heat (**subject to refund cap/sec**) |
| Momentum Overload | Rare | S Y | P1 | Shock saturate → movespeed ~4s |
| Arc Coolant | Rare | S Y H | P1 | Shock application has chance to −1–2 heat (shares refund budget) |
| Overload Siphon | Epic | S Y | P2 | Shock saturate shortens Vent recovery 50% for 2s |
| Static Grip | Std | S G | P2 | +Shock application; slight +spread |

### 8.6 Acid package (bank/convert verb — finish P0)

| Name | Rarity | Tags | Pri | One-liner |
|---|---|---|---|---|
| Toxin Bank | Epic | A C H | P0 | Corrode → Bank stacks; bonus dmg/stack; **clear/spend per §5.3** (not heat==0 only) |
| Solvent Capacitance | Epic | A C H | **P0** | Each Bank stack increases next R-spend / Dump effect |
| Corrode Radiator | Exotic | A Y H | P1 | Your corroded targets periodically dissipate your heat while acid DoT ticks |
| Viscous Rounds | Rare | A C | P2 | +Acid appl; slight −bullet speed; +damage |
| Caustic Meter | Rare | A H | P2 | ≥1 corroded target alive: slight −HeatPerShot |

### 8.7 Interlace package (after pures — P2 default for new depth)

| Name | Rarity | Tags | Pri | One-liner |
|---|---|---|---|---|
| Violent Reaction | Exotic | X F S A | P1 | Hit target with 2+ primaries active → spread those elements nearby |
| Crossflash | Epic | X F S | P1 | Shock-saturate an ignited target → bonus heat refund + fire splash (refund budget) |
| Acid Spark | Epic | X A S | P1 | Shock-saturate a corroded target → spend 1 Bank for shock arc to nearby foe |
| Pyrolysis | Epic | X F A | P1 | Ignite a corroded target → lite detonation + Bank stacks |
| Braid Protocol | Epic | X H | P2 | Applied all 3 primaries within 3s → short HeatPerShot efficiency (diminished stack) |
| Elemental Discharge | Rare | X C H | P1 | R-spend → elemental nova; costs ~30–50 heat |
| Tri-Valve | Rare | X G | P1 | Small appl to all three primaries (enables interlace thresholds) |
| Saturate Catalyst | Rare | X H | P2 | Any saturate grants brief stack that boosts next vent (must not eclipse Dump) |
| Cyclotron | Epic | X | P1 | Cycle Fire→Shock→Acid; showcase, not tax |

**Interlace balance intent:** hybrids should feel intentional and strong, not strictly dominate equal-investment pures. Crossflash especially must not pay both Fire and Shock so hard that pure either path is obsolete.

### 8.8 Converter / R-spend package

| Name | Rarity | Tags | Pri | One-liner |
|---|---|---|---|---|
| Capacitor Dump | Exotic | C H | P0 | Replace vent: spend heat → **short mean** cone ∝ heat (+ Bank) |
| Energy Convergence | Rare | C A H | P0 | Elemental DoTs/debuffs build Power stacks. **R spends/transforms stacks into dump/vent fuel or next-spend amp — not “tap R to add heat” as the primary read** |
| Rocket Vent | Epic | C Y H | P2 | While sliding, spend 8–12 heat for tracking micro-missile |
| Condensed Ejection | Exotic | C G | P2 | Slow magnetic projectiles; +damage; in-flight arcs |
| Dump Resonator | Rare | C H | P1 | After R-spend, 1s of shots at −HeatPerShot |
| Safety Release | Std | C H | P2 | Pressure Vent costs −10 heat; +pulse radius |
| Flash Vent | Rare | C Y | P2 | Vent deals no damage but spends ×1.3 heat and grants 0.5s RoF buff |
| Solvent Capacitance | Epic | A C H | P0 | See Acid package — listed here as Converter spine |

**Energy Convergence v3 contract**
- Stacks are built from ignite/electrocute/corrode (or elemental DoT ticks) as designed.
- On R: either (a) consume stacks to **multiply the current spend** (Vent/Dump/Discharge), or (b) hold-R channel distinct from tap-Dump that converts stacks into **dump fuel meter** without fighting Dump priority.
- **Anti-pattern (v2 impl):** tap R → only `AddHeat` while Dump wants a full bar on the same button. That is opposite verbs on one key — fix in design and code.

### 8.9 Stretch / optional

| Name | Rarity | Tags | Pri | One-liner |
|---|---|---|---|---|
| Heat Pump | Std | H R | P3 | +damage lightly scaling with HeatNormalized |
| Stand-In Coil | Rare | S G | P3 | First shot after vent applies bonus Shock |
| Magazine Ghost | Oddity | H | P3 | UX notches every 10 heat; tiny RoF on notch edges |
| Multiversal Thievery / Edge Fault | Contraband | G | P3 | Vanilla grid parity only if desired |
| Decay Spool | — | — | — | **Default cut** unless a unique heat verb is invented |

### 8.10 First-draft → v3 fate

| First draft / v2 | v3 fate |
|---|---|
| Mag → MaxHeat clones | Only as Thermal Buffer / Quick Sink / Dense Core with clear trades |
| Hard overheat lockout as core | Removed from base; Closed Loop reframes top-end |
| Soft redline free-win risk | Stance rules §4.4; modest base carrot; upgrade-owned melt |
| Decay Energy | Cut by default |
| Dump Charge old impl | Capacitor Dump short mean cone + base Pressure Vent |
| Infinity Burn | Elevated crown; build-to-live-in-overcap |
| Cyclotron | Elevated sacred oddity; not mandatory |
| Cycle Phasing | Kept; HUD or cut modes |
| Equipment Siphon +max heat | Equipment Radiator (cools) |
| Corrode Siphon ammo | Corrode Radiator (heat) |
| Toxin Bank clear heat==0 | **Replaced** by intentional spend/clear §5.3 |
| Energy Convergence → AddHeat on R | **Replaced** by spend amp / dump fuel contract §8.8 |
| Interlace before pure Fire/Acid verbs | **Reversed** — P0 pure spines first |
| Uncapped Shock refunds | Global refund budget §5.4 |

### 8.11 Suggested ship bands (no hard cap)

- **P0 complete (~core identity):** glue essentials, 3 applicators, heat economy spine, Fire pressure cards (Ember/Scorched/Runaway), Shock Recursion + cap, Toxin Bank + Solvent Capacitance, crowns (IB, Dump, Closed Loop), Detonation, key supports (Mass Accel, Shield, Repairs, Adrenaline)
- **P1:** shipped interlace that already exists (Violent, Crossflash, Pyrolysis, Acid Spark, Tri-Valve, Braid, Catalyst, Cyclotron, Discharge), remaining Rhythm movement cool, Capacitor Plates, Dump Resonator
- **P2 stretch:** Flash Vent variants, Condensed Ejection, Rocket Vent, remaining element niche cards, Phasing if HUD OK
- **P3 backlog:** table 8.9

**Policy:** keep what serves identity; finish P0 gaps before adding new X cards.


## 9. Example Builds (readable pure + hybrids)

### Pure Redline Fire (P0 target)
Heated Battery → Ember Feed / Scorched Chamber → Thermal Runaway → Mass Acceleration → Thermal Buffer → Charge Shield → Cycling Repairs → **Infinity Burn** → Scorching Detonation  
*Hybrid seam:* capped Shock Recursion to stay overcapped longer.

### Pure Shock Rhythm (P0 target)
Charged Cartridges → Shock Recursion → Adrenaline Vent → Quick Sink → Perpetual Motion → Extralight → Momentum Overload → **Closed Loop**  
*Hybrid seam:* Crossflash + Heated Battery (should feel spicy, not mandatory).

### Pure Acid Converter (P0 target)
Corrosive Plasma → Toxin Bank → **Solvent Capacitance** → Energy Convergence (spend amp) → Capacitor Plates → **Capacitor Dump** → Corrode Radiator  
*Hybrid seam:* Pyrolysis + Heated Battery for bank gen.

### Cyclotron Braid (interlace showcase — not required endgame)
**Cyclotron** → Tri-Valve / Braid Protocol → Violent Reaction → Crossflash or Pyrolysis → Mass Accel or Dump → Infinity Burn *or* Capacitor Dump

### Fire + Shock hybrid (uptime redline)
Heated Battery + Charged Cartridges → Crossflash → Shock Recursion → Ember Feed → Mass Accel → Infinity Burn  
*Check:* pure Fire without Shock should still clear content; hybrid is preference/power spike, not gate.


## 10. Strengths, Weaknesses, Risks & Guard Rails

### Strengths
- Infinity Burn north star preserved and elevated
- Soft redline solves high-uptime vs heat-fuse better than hard lockout
- Element verbs (pressure / tempo / bank) give Mycopunk-native depth
- Paths readable but not siloed
- R always means heat interaction (spend/transform)
- Hose + heat is a continuous skill resource mag weapons cannot copy

### Weaknesses / failure states (should stay fun)
- Redline accuracy cost at long range
- Overcap self-damage if greedy without Shield/Repairs
- Bad dump timing (spent too early / too late)
- Lost rhythm (no saturates/kills; refund budget dry)
- Converter without stacks/heat feels “empty R”
- Breath/refund accidentally wasting a planned dump window

### Design risks & mitigations

| Risk | Mitigation |
|---|---|
| Soft redline too soft = no resource | §4.4 stance rules; modest base carrot; playtest pin-M1 default |
| Soft redline too hard = old frustration | Keep fire-at-100%; no lockout; Closed Loop for Rhythm |
| Refund runaway on hose | §5.4 global refund cap/sec; diminished efficiency stacking |
| Interlace / Cyclotron mandatory | §5.2 pure minimum kits; pillar 8 ship order; hybrid ≠ strict dominate |
| Base Vent steals Dump thunder | Keep Vent weak; Catalyst/scalers capped |
| Cycle Phasing unreadable | HUD mode name or cut/demote |
| R conflict / opposite verbs | Dump > Convergence(amp) > Discharge > Vent; Convergence never “only AddHeat” |
| Toxin Bank path-asymmetric clear | §5.3 intentional spend clear |
| Mass Accel makes cold unusable + redline free | Tune efficiency; redline break-even without full Fire/IB package |
| Double Shot / Overclock HPS spike without tools | Pair with economy supports; avoid fake difficulty |
| Dump as long turret mode | Short mean cone; return to hose immediately |
| Proc engine replaces bullets | Bullet primacy checklist §11 |
| IB self-DoT vs outgoing curve | Build-to-live package; not free and not suicide-only |
| MP desync on refunds/novas/self-DoT | Sandbox flag; same mod version; authority-safe hooks |

### Edge cases — explicit rules needed in impl

- Closed Loop micro-vent while overcapped under IB (safety vs stall mythic form)
- Bank stacks when Dump drains heat mid-cone (consume at start of Dump preferred)
- Braid × Closed Loop × phase Coolant efficiency (diminished / best-of)
- Cyclotron + single applicator element overwrite (tooltip OK)
- Vent input during dump hitch (ignore or queue — pick one)
- Stow/swap dissipate in multiplayer (confirm continue dissipate)


## 11. Success Criteria / Player Fantasy Checklist

### Core loop
- [ ] Soft redline: holding M1 at 100% still feels like playing the game
- [ ] Soft redline: pinning 100% is **not** strictly best in all situations without investment
- [ ] Infinity Burn run feels like the weapon’s mythic form and a *build*, not a death sentence or freebie
- [ ] Stutter-breathing the trigger is a real technique with visible bar feedback
- [ ] Warm → Hot → Redline reads as a ladder (audio/VFX), not a dead band then a switch

### Paths
- [ ] A Redline build is visibly scarier at 80–120% heat than at 10%
- [ ] A Rhythm build can chain a room with almost no full stops **without** erasing the heat bar entirely
- [ ] A Converter dump is a crisp cash-out every few seconds; Bank visibly powers it
- [ ] Pure Fire / pure Shock / pure Acid each feel complete without Cyclotron

### Elements & hose
- [ ] Fire / Shock / Acid each change heat *economy*, not only DoT color
- [ ] Fire can generate or multiply pressure (not only ride zone carrots)
- [ ] Shock refunds have a ceiling; hose still climbs under sustained fire
- [ ] Acid Bank is spent on purpose (R/Dump/Spark), not bricked by heat==0
- [ ] Cyclotron run feels chaotic-good and enables interlace payoffs
- [ ] Hybrid grids feel intentional; pures are not obsolete
- [ ] **Most damage is still pellets** — procs punctuate

### R & failure
- [ ] R always means something heat-related (spend or transform — not “add heat” as the main read)
- [ ] Failure states stay fun — not AFK lockout
- [ ] Co-op: detonations / novas / spreads help allies; refunds can stay personal

### Playtest anchors (every build)
- [ ] Do I want to hold M1 most of the fight?
- [ ] Is most damage still pellets, not waiting for a proc?
- [ ] Does the heat bar change my trigger finger within 5 seconds of combat?
- [ ] Can I explain my build as run hot / never stop / cash out without listing five saturate cards?


## 12. Universal Truths (Mycopunk alignment)

- Exotic shapes should be larger than others; each exotic should use the same number of cells as other exotics.
- No hard total upgrade cap for this rework; prefer a rich core pool over padding.
- Three paths create different build options but **may intermingle** on the grid.
- Primary elements for this weapon’s identity: **Fire, Shock, Acid**.
- Names may be renamed freely for heat clarity; vanilla echoes are optional recognition aids.
- Upgrades should prefer heat verbs: ±Max Heat, ±HeatPerShot, dissipate, refund, scale-with-heat%, spend heat, redline/overcap rules, element×heat cross-talk.
- **Bullet hose fantasy outranks status-proc fantasy** when they conflict.
- **Pures before interlace depth** when scheduling design/impl work.


## 13. Open Items (next design/impl passes)

1. Exact R conflict table when Dump + Convergence + Discharge stacked — **priority locked:** Dump > Convergence > Discharge > Pressure Vent; Convergence contract §8.8
2. Infinity Burn numeric curve (self-DPS vs outgoing mult vs hard cap) — **initial:** +35% dmg / +25% elem per full overcap depth; OC fire interval ×0.92; validate live-in-overcap with Shield/Repairs
3. Cycle Phasing HUD feasibility — **if unclean, demote modes or card**
4. Trim brainstorm → ship pool by P0/P1/P2 — **finish P0 gaps first**
5. Toxin Bank clear rule — **v3 LOCKED direction §5.3** (intentional spend; not heat==0 only) — **impl still on v2 heat==0; migrate**
6. Warm zone small hook — **v3: enable tiny teaching hook** (§4.3)
7. Vent pulse: keep small damage vs pure utility cool — **impl: small pulse damage**
8. Stow/swap: confirm continue dissipate in multiplayer
9. Soft Redline stance tuning pass — break-even DPS vs Hot; Mass Accel interaction
10. Dump VFX/feel — **LOCKED shape: narrowing cone;** bias **shorter/meaner** duration
11. Soft Redline — **LOCKED: hard cut (no config flag dual-path)**
12. Code structure — **LOCKED: extract subsystems** (`HeatZone`, `HeatStatLayers`, `HeatVentSystem`)
13. **NEW:** Global heat refund budget / sec + efficiency diminished stacking
14. **NEW:** Implement Ember Feed, Scorched Chamber, Thermal Runaway (Fire P0)
15. **NEW:** Implement Solvent Capacitance; retie Bank to Dump/R
16. **NEW:** Rewrite Energy Convergence off AddHeat-primary R
17. Closed Loop behavior while overcapped (IB)


## 14. Implementation Notes (current mod context)

- Mod adds **Heat Cycler** as a new primary (does not replace vanilla Cycler)
- Existing code: CyclerHeatBehaviour, heat hooks, upgrade property ports
- First draft mapped mag/reload → heat too directly; v2 Soft Redline rebuild landed; **this v3 doc is the cohesion / verb-completion target**
- Config knobs should expose at least: MaxHeat, HeatPerShot, DissipatePerSecond, DissipateDelay, zone thresholds, redline brake/carrot mults, vent spend/recovery, **refund budget/sec**
- Multiplayer: all clients same mod version; heat spends and crowns must be network-safe in impl pass


## 15. Changelog (design)

### v3 (cohesion pass)
- Soft Redline **stance rules**: near break-even base; modest carrot; upgrade-owned melt; Mass Accel guidance
- Warm band gains a tiny teaching hook (no longer forever cosmetic)
- Dissipate guidance eased slightly so Converter can hold a bar
- Element completeness bars + **pure-path minimum kits** before interlace depth
- Fire P0 pressure cards elevated (Ember Feed, Scorched Chamber, Thermal Runaway)
- Shock **refund cap / efficiency stacking** guard rails
- Toxin Bank clear **unlocked from heat==0**; intentional spend/cash-out
- Solvent Capacitance elevated to Converter P0 spine
- Energy Convergence contract: spend amp / dump fuel — **not** tap-R AddHeat
- Capacitor Dump: short mean cone; Bank cash-out
- R hard rule: spend or transform only
- Bullet primacy and playtest anchors codified
- Ship priority P0/P1/P2; path×element matrix called out as uneven → fix order
- Crowns vs support exotics clarity; Closed Loop × IB edge case noted
- Risks table expanded with mitigations; open items for impl migration

### v2
- Soft Redline baseline (no base hard lockout)
- Pressure Vent on R as global verb
- Pillar 7 elemental interlacing (Fire / Shock / Acid)
- Paths as intermingling gravity wells
- No hard upgrade count cap
- Infinity Burn, Cyclotron, Capacitor Dump elevated
- Full tagged upgrade brainstorm + example builds
- Decay demoted/cut by default

### v1 (superseded)
- Hard overheat lockout (~1s) + full heat reset
- Mag/reload dictionary ports of vanilla Cycler upgrades
- Empty/partial path outline in early stub doc


## 16. Implementation status (mod code)

### Shipped core
- Soft Redline heat (no base lockout), zones, Pressure Vent, R router
- Infinity Burn overcap + outgoing scale
- Capacitor Dump narrowing cone (duration/feel still to bias shorter per v3)
- Condensed Ejection plasma + projectile arcs
- Semantic renames / heat verbs on economy cards
- Decay Energy not registered
- GearData gate for CreateUpgrade
- Subsystems: HeatZone, HeatStatLayers, HeatVentSystem, HeatInterlace

### Shipped interlace / crowns
| Card | Status |
|---|---|
| Closed Loop | Shipped |
| Crossflash | Shipped |
| Pyrolysis | Shipped |
| Tri-Valve | Shipped (`ITarget.ApplyStatusEffect` on hit) |
| Acid Spark | Shipped |
| Braid Protocol | Shipped |
| Saturate Catalyst | Shipped (boosts next Pressure Vent) |
| Violent Reaction | Shipped |
| Cyclotron / Elemental Discharge / Dump / IB | Shipped |
| Cycle Phasing | Shipped (v2 mode table: Coolant→Spike; HUD still weak) |

### Catalog count
- **43** registered upgrades (ids 92020–92063 range; Decay id reserved unused)

### v3 gaps vs shipped (priority work)

| Gap | Pri | Notes |
|---|---|---|
| Ember Feed / Scorched Chamber / Thermal Runaway | P0 | Fire pressure verb incomplete |
| Solvent Capacitance | P0 | Bank → R mult missing |
| Toxin Bank clear rule migrate off heat==0 | P0 | §5.3 |
| Energy Convergence rewrite (no AddHeat-primary R) | P0 | §8.8 |
| Refund budget + efficiency diminished stack | P0 | §5.4 |
| Soft Redline stance retune (break-even) | P0 | §4.4 |
| Warm zone tiny hook | P1 | §4.3 |
| Dump shorter/meaner + Bank on dump | P0 | Crown feel |
| Closed Loop vs IB overcap policy | P1 | Edge case |
| Cycle Phasing HUD or demote | P1 | Readability |
| Corrode Radiator, Capacitor Plates, Dump Resonator | P1 | Converter/Rhythm supports |
| Arc Coolant, Overload Siphon, Static Grip | P2 | Shock niche |
| Viscous Rounds, Caustic Meter, Immolation Rhythm | P2 | Element niche |
| Heat Slicks, Aftercool, Governor/Throttle, Perpetual Motion | P1–P2 | Economy |
| Dedicated icons for new interlace cards | P2 | Currently reuse art |

### Impl notes
- Tri-Valve must never call DamageTarget from OnDamageTarget (re-entry crash)
- Multi-element buildup uses status API, not bullet.damageEffect (single element only)
- Toxin Bank clear: **code still heat == 0** — design v3 supersedes; migrate
- Energy Convergence: **code still AddHeat on R** — design v3 supersedes; migrate
- Capacitor Dump does not yet consume/amplify Bank — add with Solvent Capacitance
