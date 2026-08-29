# Needle Carbine — Design Document (v1)

## 1. High Concept / Fantasy

A mid-range medical-industrial needle carbine that sticks toxin darts into targets,
builds toward a Needler-style supercombine detonation, and runs a three-way economy
of application, consumption, and self-dosing.

Poison is a real status (`EffectType.Poison`). Needles are a separate stick-counter
that detonates when enough land in a short window. Upgrades teach you to paint the
room with multiple DoTs, cash enemy suffering for sustain, or become the trial subject.

One-liner: Inject the fever. Supercombine the dose. Cash the suffering — or take it yourself.

Product shape: New primary weapon (**Needle Carbine**). Does not replace any vanilla gun.


## 2. Role & Fantasy in the Arsenal

- Slot: Primary
- Range: Mid (carbine). Light falloff acceptable; identity is apply + detonate, not sniper.
- Role: Debuff portfolio manager — stack Poison/needles, multi-DoT riders, consume, self-toxin
- Gap filled:
  - Globbler = acid AOE plumber
  - Cycler = generic element SMG
  - Acid grenade = puddle utility
  - Nothing owns “stick needles → supercombine → spend or self-dose the chart”
- Synergies: Any DoT gear (ally paints, you Mercy Kill); movement spends from Extract;
  co-op “I saturated them, you execute”

Not trying to be: pure DPS hose, Globbler 2.0, Blood Carver melee, or Cycler with a reskin.


## 3. Design Pillars

1. On-hit and status economy > flat % damage stickers.
2. Poison is a true EffectType with full-saturation DoT (not a fake tracker forever).
3. Needles / Supercombine are baseline fantasy (Needler DNA) — always on, upgrade-shaped.
4. Multi-DoT is first-class — Poison is the spine; Fire/Shock/Acid/Bees are real riders.
5. Three peer paths (Blowdart / Sadist / Own Medicine); hybrids intended; no anti-synergy matrix.
6. Own Medicine is spicy — real player DoT pressure; mitigation is partial and upgrade-owned.
7. ~30 upgrades for v1 ship; exotic shapes larger than others; each exotic same cell count.
8. Medical / industrial tone (SAXON pharma-lab), not cartoon slime.
9. RMB teaches consume early (baseline Extract); Path C may rewrite RMB.
10. Reload stays reload — do not overload R unless a future exotic explicitly needs hold-R.


## 4. Core Mechanics & Gunfeel

### 4.1 Base gun

| Trait        | Draft / intent                                              |
|--------------|-------------------------------------------------------------|
| Fire mode    | Fast full-auto (or very fast semi) needle stream            |
| Damage       | Low per dart — value is apply + supercombine                |
| Range        | Mid carbine; light falloff OK                               |
| Mag/reserve  | Medium mag; reload beat matters                             |
| Projectile   | Fast darts (CartridgeSMG / light AR DNA until custom art)   |
| ADS          | Optional light ADS; not a sniper                            |
| Model/audio  | Clinical pneumatic hiss, needle clack, supercombine crack   |

### 4.2 Inputs

| Input | Role |
|-------|------|
| M1    | Fire needles |
| RMB   | Extract (baseline) — see 4.6; Path C may override |
| R     | Reload only |
| Heavy | Normal heavy equip (no baseline heavy link) |

### 4.3 Poison — true EffectType

New status in the vanilla saturation pipeline (same rules as Fire/Acid/Bees).

| Tuning (draft)           | Value        | Notes |
|--------------------------|--------------|-------|
| EffectType               | Poison = 11  | Append after Cryo = 10 (confirm no collision at impl) |
| DamageMultiplier         | ×1.0         | Pure DoT identity; amps stay Decay/Rot |
| FullSaturationLifetime    | 5.5s         | Longer linger than Fire/Acid (3s) |
| DecayDelay               | 3s           | Match vanilla |
| DecaySpeed               | 0.3 /s       | Match vanilla |
| SaturationAddMultiplier  | 0.1          | Vanilla constant — each apply adds amount×0.1 |
| Full-sat DoT (enemy)     | 10 / 0.2s    | Match Fire/Acid/Bees tick pattern |
| Full-sat DoT (player)    | 0.165 / 0.2s | Spicy self-harm uses real player ticks |
| On full saturation       | Start DoT    | Locked: DoT at full stacks/saturation |
| Verb / UI                | “Poisoned”   | Sickly teal-green clinical |
| Mesh/VFX                 | Clone Acid or Bees materials as placeholder; custom later |

PoisonStatusEffect pattern (mirror AcidStatusEffect):
- OnFullSaturationUpdate → DamageTarget DoT with EffectType.Poison + DamageFlags.DamageOverTime
- No innate damage amp (unlike Decay/Rot)
- No innate slow/shatter (unlike Cryo)

Baseline dart Poison application (draft):
- effectAmount per dart ~1.0–1.5 (≈0.10–0.15 saturation per hit)
- ~7–10 focused hits to full-sat a fresh target without supercombine help
- Supercombine dumps a large bonus effectAmount (see 4.4)

### 4.4 Needles & Supercombine (Needler DNA — baseline)

Separate counter from Poison saturation so both systems can breathe.

| Param                    | Draft              | Intent |
|--------------------------|--------------------|--------|
| Needle stacks            | +1 per dart hit from this weapon | “How many darts stuck” |
| Max needle display       | Threshold (no huge bank) | Supercombine consumes them |
| Supercombine threshold   | 7                  | Playtest band 5–9 |
| Needle grace window      | 3.0s since last needle on that target | Must keep landing hits |
| On grace expiry          | Needle stacks → 0  | No infinite bank across downtime |
| On supercombine          | Consume all needles on target → detonation | Readable bang |
| Baseline detonate damage | Modest burst on part/brain (≈ mid mag worth, not delete) | Upgrades scale |
| Baseline detonate radius | Small (single-target biased; light splash) | Clear via upgrades |
| Supercombine + Poison    | Large Poison effectAmount dump on primary target | Helps full-sat / refresh window |
| Telegraph                | Visible needle fill 1..7 then crack/flash | Player must read it |
| Multi-target             | Needles tracked per target (part or brain — prefer brain-level for clarity) | Avoid split-body confusion |

Relationship Poison ↔ Needles:
- Every dart: hit damage + small Poison amount + +1 Needle (if grace valid / refresh grace)
- Supercombine: explosion AND poison dump
- Path cards may add secondary elements on supercombine, harvest instead of damage, etc.

Sacred cow: Supercombine exists with zero upgrades. Upgrades shape threshold, payload, and payoffs.

### 4.5 Multi-DoT rules

1. Poison is always the spine — other DoTs are riders or supercombine payloads, not replacements.
2. Baseline gun applies Poison only (+ needles). Fire/Shock/Acid/Bees come from upgrades.
3. Prefer one free secondary applicator family per small card; Formulary/exotics break this.
4. Sadist pays for diversity — consuming three different DoTs > three× Poison only.
5. Own Medicine secondary self-elements are opt-in and expensive.
6. Respect vanilla parallel statuses (one saturation bar per EffectType). Do not require EnableEffectMixing.

### 4.6 Baseline RMB — Extract

Teaches Sadist early (Blood Carver RMB lesson).

Requirements:
- Aimed target has Poison saturation > 0 and/or needle stacks > 0 and/or another consumable full status you applied
- Short recovery between Extracts (~0.4s)

Baseline effect (draft):
| Piece     | Draft |
|-----------|-------|
| Consume   | Reduce Poison saturation by ~0.35 OR remove up to 2 needle stacks (prefer poison first if both) |
| Heal      | Small (noticeable sip, not a second medkit) |
| Ammo      | Tiny crumb to mag (0–1) optional on baseline — full ammo refund is upgrade-owned |
| Damage    | None on baseline Extract (damage consume is Path B) |
| Fail      | No valid target / empty chart → weak whiff feedback, no reward |

RMB priority when multiple overrides equipped:
1. Path C crown Double Dose / Self-Dose override
2. Path B Mercy Kill / empowered Extract
3. Modified baseline Extract
4. Else baseline Extract

### 4.7 Base combat loop (no upgrades)

```
M1 needles into target → Needle stacks climb + Poison saturates
   → at 7 needles within grace: SUPERCOMBINE bang + poison dump → DoT cooks
   → RMB Extract on a poisoned target for a sip of sustain
   → re-stick needles during DoT windows; reload on R
```

Skill without upgrades: focus fire to supercombine, don’t spray seven packs half-stuck,
time Extract between packs, don’t stand in your own future Path C dose.


## 5. Upgrade Paths (gravity wells — hybrids intended)

### Path A — BLOWDART (Apply / multi-DoT / spread)
“The ward fills with fever.”

- Spine: +Poison per hit, +needle gain, multi-needle shots, chain inject, longer grace,
  lower supercombine threshold, splash ampules, secondary DoT riders
- Clear vs ST: spread and clouds for clear; stack density + formulary for ST
- Hybrid hooks: more statuses on targets feed Sadist; denser Poison feeds Own Medicine copy

### Path B — SADIST (Consume debuffs for benefit)
“Chart the symptoms. Bill the patient.”

- Spine: Extract upgrades; consume Poison / DoT full-sats / Decay-Rot → ammo, reload,
  heal, overclock, move charge, next-shot empower, execute bursts
- Supercombine harvest variants: detonate for personal buff instead of (or plus) damage
- Hybrid hooks: Blowdart fills the chart; Own Medicine can self-Extract emergency (limited)

### Path C — OWN MEDICINE (Self-status engine — spicy)
“Trial subject: operator.”

- Spine: fire tax and/or RMB Self-Dose applies real Poison (and opt-in elements) to you
- Spicy locks:
  - Full self-Poison = real player DoT ticks
  - No free full DR; Triage-style cards are partial only
  - Panic-firing while saturated can kill you — valid fantasy
- Payoff: while self-saturated, darts gain power / copy statuses out / supercombine partial heal
- Hybrid hooks: Sympathetic copy wants Blowdart density; self-Extract/Crash Cart touches Sadist

### Path × verb matrix

```
                 BLOWDART              SADIST                 OWN MEDICINE
Apply            core fantasy          fills the menu         self is the canvas
Supercombine     payload / spread      harvest option         self-heal fraction
Consume/Extract  optional tax          core fantasy           emergency cleanse toys
Self-status      light (opt-in)        avoid as default       core fantasy
```


## 6. Crowns & Sacred Cows

### Pandemic Protocol (Exotic) — Blowdart crown
- While an enemy is at full Poison saturation, periodically seed small Poison amount
  to nearby enemies (radius draft ~6–8m, pulse ~1.0–1.25s).
- Seed amount weak alone; exists to keep wards infected and feed multi-target supercombines.
- Does not auto-supercombine spreads.

### Compound Formulary (Exotic) — Blowdart crown
- Needles gain a second DoT line (Fire / Shock / Acid / Bees — chosen or rolled on upgrade apply).
- Supercombine detonates primary Poison payload AND triggers a bonus burst scaled by
  how many distinct DoT statuses were fully saturated on the target.
- The multi-DoT identity card.

### Mercy Kill (Exotic) — Sadist crown
- Replaces Extract on press when target has ≥1 consumable status (or always replaces if equipped).
- Consume ALL tracked consumable statuses you can legally clear on that target
  (full-sat DoTs you may remove or hard-reduce; Poison sat → 0; needles → 0).
- Burst damage ∝ weighted status count (Decay/Rot worth more than a single DoT).
- Brief self-hitch (~0.1s) so it is a decision, not a spam click.
- Clear “chart stamped paid” VFX/audio.

### Leech Ledger (Exotic) — Sadist crown
- While any enemy you applied Poison to is taking Poison DoT (full sat), gain a resource drip:
  ammo crumbs and/or mild HP regen (Energy Convergence DNA, poison-flavored).
- Drip scales lightly with number of poisoned enemies (soft cap).
- Encourages maintain-the-ward play without requiring constant RMB.

### Autotoxin Loop (Exotic) — Own Medicine crown
- While you are fully saturated with any element (Poison preferred, others count),
  carbine darts deal that element’s rider + full Poison apply.
- Supercombine refunds a controlled self-heal (fraction of detonate damage, capped)
  so the loop can stabilize without deleting spicy risk.
- If not self-saturated, gun feels closer to baseline (crown wants you on the dose).

### Sympathetic Sickness (Exotic) — Own Medicine crown
- Your active full saturations are written onto enemies you needle at reduced effectAmount.
- RMB becomes Double Dose: apply strong self-Poison (and opt-in second element if owned)
  AND apply a chunk to the aimed target.
- Co-op rule: default no team chem aura; only enemies receive copy.

Sacred cows (do not cut without rewriting identity):
- Baseline needles + supercombine
- True Poison EffectType with full-sat DoT
- Baseline Extract
- Spicy self-DoT when Path C engaged
- Multi-DoT riders exist in the ship pool (not backlog-only)


## 7. Full Upgrade List (~30 ship + backlog)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity
Cell rule: Exotic shapes larger than others; all Exotics same cell count.

Player-facing names below. API names assigned at implementation.

------------------------------------------------------------------------------
PATH A — BLOWDART                                            ship subset below
------------------------------------------------------------------------------

A-EX1. Pandemic Protocol — Exotic (crown)
       Full Poison on an enemy periodically seeds Poison to nearby enemies.

A-EX2. Compound Formulary — Exotic (crown)
       Second DoT line on needles; supercombine bonus for multi-status targets.

A-EP1. Supercombine Catalyst — Epic
       +Supercombine damage and radius; supercombine threshold −1 (min floor 4).

A-EP2. Secondary Septicemia — Epic
       Supercombine applies a rolled secondary full-element hitch (Fire or Acid on apply).
       Distinct from Formulary’s ongoing rider — this is detonate-only payload.

A-EP3. Needle Storm — Epic
       Fire a 3-needle burst per trigger pull. −Accuracy / +spread. Mag economy feels heavier.

A-EP4. Ward Discharge — Epic
       On supercombine, emit a short Poison pulse around the target (small radius).

A-RA1. Hollow Points — Rare
       Every 3rd dart on the same target grants +1 bonus needle stack.

A-RA2. Anticoagulant Coating — Rare
       +Poison effectAmount per dart.

A-RA3. Incendiary Ampules — Rare
       Darts apply Fire amount (multi-DoT rider).

A-RA4. Live Wire Serum — Rare
       Darts apply Shock amount.

A-RA5. Caustic Base — Rare
       Darts apply Acid amount.

A-RA6. Apiary Trace — Rare
       Darts apply Bees amount (slightly lower apply rate than other riders).

A-ST1. Soft Tissue Bias — Standard
       Minor +damage vs non-shell / flesh-leaning parts.

------------------------------------------------------------------------------
PATH B — SADIST
------------------------------------------------------------------------------

B-EX1. Mercy Kill — Exotic (crown)
       Consume all consumable statuses on target → burst ∝ weighted count.

B-EX2. Leech Ledger — Exotic (crown)
       Poison DoT on your victims drips ammo/HP to you (soft capped).

B-EP1. Extractive Billing — Epic
       Extract heals more proportional to statuses / poison amount removed.

B-EP2. Polypharmacy — Epic
       Hitting a target that currently has ≥2 distinct DoT statuses (full or partial)
       grants a brief carbine RoF buff.

B-RA1. Fever Tax — Rare
       Extract refunds ammo to magazine (scales lightly with amount consumed).

B-RA2. (ship) see Generic for mag toys that support consume reload loops

------------------------------------------------------------------------------
PATH C — OWN MEDICINE
------------------------------------------------------------------------------

C-EX1. Autotoxin Loop — Exotic (crown)
       While self fully saturated, darts carry your element + Poison; supercombine self-heals (capped).

C-EX2. Sympathetic Sickness — Exotic (crown)
       Copy your full sats outward on needle hit; RMB = Double Dose.

C-EP1. Controlled Overdose — Epic
       RMB Self-Dose: large self-Poison apply + next magazine empowered (damage/apply).
       Spicy — can self-full-sat quickly. Conflicts resolved via RMB priority (this modifies
       Extract into dose unless Mercy Kill crown takes over on enemy-aimed cast —
       if aiming enemy: Double Dose rules when Sympathetic equipped; else Self-Dose + weak Extract).

C-EP2. Crash Cart — Epic
       Once per combat when you drop below a low HP threshold while self-Poisoned:
       consume your Poison status, brief speed + small heal. Safety valve that costs the dose.

C-RA1. Triage Protocols — Rare
       Self-Poison DoT damage taken reduced by a partial percent (draft 25–40%).
       Does NOT stop saturation or delete spicy identity.

------------------------------------------------------------------------------
GENERIC / GUNFEEL
------------------------------------------------------------------------------

G-EP1. (none extra beyond path epics in frozen 30 — room reserved in backlog)

G-RA1. Rapid Infusion — Rare
       +Fire rate, −per-dart damage.

G-RA2. Deep Magazine — Rare
       +Magazine size, −reload speed.

G-ST1. Extended Cannula — Standard
       +Range.

G-ST2. Spare Cartridges — Standard
       +Ammo reserves.

G-ST3. Clinic Reload — Standard
       +Reload speed.

G-ST4. Stick and Move — Standard
       Brief movespeed bonus on supercombine.

G-OD1. Boundary Incursion — Oddity
       Increases upgrade grid size.

------------------------------------------------------------------------------
FROZEN 30 FOR V1 SHIP
------------------------------------------------------------------------------

EXOTIC (6)
  1  Pandemic Protocol
  2  Compound Formulary
  3  Mercy Kill
  4  Leech Ledger
  5  Autotoxin Loop
  6  Sympathetic Sickness

EPIC (8)
  7  Supercombine Catalyst
  8  Secondary Septicemia
  9  Needle Storm
 10  Ward Discharge
 11  Extractive Billing
 12  Polypharmacy
 13  Controlled Overdose
 14  Crash Cart

RARE (10)
 15  Hollow Points
 16  Anticoagulant Coating
 17  Incendiary Ampules
 18  Live Wire Serum
 19  Caustic Base
 20  Apiary Trace
 21  Fever Tax
 22  Triage Protocols
 23  Rapid Infusion
 24  Deep Magazine

STANDARD (5)
 25  Extended Cannula
 26  Spare Cartridges
 27  Clinic Reload
 28  Soft Tissue Bias
 29  Stick and Move

ODDITY (1)
 30  Boundary Incursion

------------------------------------------------------------------------------
BACKLOG (designed, not in first 30)
------------------------------------------------------------------------------

- Decay Applicator / Rot Applicator (amp family — powerful; balance carefully)
- Needle Inheritance (on kill, nearest enemy gains half needles)
- Bounce Cannula (darts ricochet once toward another enemy)
- Ampule Puddle (supercombine leaves brief Poison puddle — keep distinct from Globbler flood)
- Harvest Detonation (supercombine heals instead of damage — Sadist lean)
- Overclock Serum (consume statuses → overclock charge)
- Move-Charge Extract (Extract grants movement ability charge)
- Ally Transfer (Extract from ally cleanses them and throws Poison to nearest enemy)
- Clinic Turret (root + DR + RoF while holding M1 — execute stance)
- Infinite Clinic (Infinity Burn DNA: infinite ammo, sustained fire self-Poisons harder)
- Corrosive Reaction mirror (damage up after you are poisoned)
- Immolator mirror (self-Poison mag refill — very strong with Autotoxin; backlog on purpose)
- Heavy Formulary (supercombine empowers next heavy shot)
- Threshold auto-Extract at N poisoned enemies
- Bees nest on supercombine only
- Water coupler (Water apply to boost Shock rider builds)


## 8. Example Builds

Needler purist (supercombine carry)
  Supercombine Catalyst + Needle Storm + Hollow Points + Ward Discharge
  + Stick and Move + Anticoagulant Coating
  Focus fire, bang, reposition.

Chem warfare (Blowdart multi-DoT)
  Compound Formulary + Incendiary Ampules + Caustic Base + Live Wire Serum
  + Pandemic Protocol + Polypharmacy
  Rainbow chart, detonate for Formulary bonus, seed the room.

Vampire admin (Sadist)
  Mercy Kill + Leech Ledger + Extractive Billing + Fever Tax
  + Anticoagulant Coating + Deep Magazine
  Keep Poison DoTs rolling, bill targets for burst heal/ammo.

Trial subject (Own Medicine spicy)
  Autotoxin Loop + Sympathetic Sickness + Controlled Overdose
  + Triage Protocols + Crash Cart + Live Wire Serum
  Stay saturated on purpose; copy out; don’t skip Crash Cart until you know the damage.

Hybrid freak
  Compound Formulary + Mercy Kill + Autotoxin Loop
  + Polypharmacy + Supercombine Catalyst
  Apply rainbow, cash charts, maintain self-aura — no artificial brakes.


## 9. Economy & Tuning Rules of Thumb

- Per-dart damage stays low; power budget lives in Poison uptime, supercombine, consume, self-loops.
- Supercombine threshold 7 / grace 3.0s — if players never detonate, lower threshold or raise grace;
  if every spray auto-bangs, raise threshold or tighten grace.
- Supercombine baseline damage must not obsolete focusing a weak point without status setup.
- Extract baseline heal: noticeable between packs, not infinite sustain in boss DPS checks.
- Mercy Kill burst: reward setup; place below “delete elite with one button, zero setup.”
- Leech Ledger soft cap: 3–4 poisoned enemies for full drip.
- Self-Poison: with Triage + Crash Cart, skilled players survive; without them, Path C is hard mode.
- Watch stacked apply: Anticoagulant + Formulary + Pandemic + Ward Discharge should infect,
  not melt bosses through DoT alone without supercombine/consume skill.
- Multi-DoT riders use modest effectAmount so Cycler-level element SMG does not appear for free.
- Decay/Rot applicators stay backlog until Poison/DoT identity is proven.


## 10. Status Split (explicit)

| Status   | Role on this gun                              | Baseline? |
|----------|-----------------------------------------------|-----------|
| Poison   | Spine DoT (new EffectType)                    | Yes       |
| Needles  | Supercombine counter (not an EffectType)      | Yes       |
| Fire     | Rider / Formulary / Septicemia                | Upgrade   |
| Shock    | Rider (Live Wire)                             | Upgrade   |
| Acid     | Rider (Caustic)                               | Upgrade   |
| Bees     | Rider (Apiary Trace)                          | Upgrade   |
| Decay    | Amp; consume-valuable; apply backlog          | Backlog   |
| Rot      | Amp; consume-valuable; apply backlog          | Backlog   |
| Water    | Shock coupler backlog                         | Backlog   |
| Cryo     | Not in identity                               | No        |

Sadist consumable weights (draft for Mercy Kill math):
- Poison full sat: 1.0
- Each other DoT full sat: 1.0
- Decay full sat: 1.5
- Rot full sat: 1.75
- Needle stacks at detonate time if harvested: 0.15 each (if card allows needle consume)


## 11. Implementation Notes

### 11.1 Gear registration
- Follow weapon template: clone base gun (CartridgeSMG or similar), GearInfo id high-range,
  APIName `needle_carbine`, behaviour component, SpawnGear stamp, CreateUpgrade pool.
- Plugin: rename GUID `sparroh.needlecarbine`, MycoMod **IsSandbox** (gameplay rules).
- Persistence: stable gear id; register before PlayerData.OnAwake AddGear.

### 11.2 Poison EffectType (phased)

Phase 0 — Vertical slice without enum
  - Needle stacks + supercombine via OnDamageTarget
  - Poison simulated on behaviour tracker if needed for prototype feel

Phase 1 — EffectType + class
  - Add EffectType.Poison = 11 (or free slot)
  - PoisonStatusEffect : StatusEffect (Acid DoT pattern, lifetime 5.5s)
  - Patch StatusEffectManager.CreateEffect switch
  - Verify effectPool length: static ctor sizes by positive enum values — appending works
    if enum is extended in a publicized/injected way; if enum cannot be extended cleanly,
    use unused/obsolete slot only after audit, or maintain parallel manager (document failure)

Phase 2 — StatusEffectData
  - Inject/clone StatusEffectData into Global effect table (verb, colors, materials, audio)
  - Global.GetEffect(Poison) must be non-null for mesh stages and saturate text

Phase 3 — Gun wiring
  - GunData.damageEffect = Poison, baseline effectAmount on shots
  - Upgrades mutate amounts / add parallel applies on hit
  - Sadist reads target.GetStatusEffects() and reduces/removes via status APIs

Phase 4 — Own Medicine
  - Apply Poison to local player through damage/status pipeline
  - Listen OnFullySaturated / OnSaturatedEffectRemoved
  - Spicy DoT is “free” once full sat exists — do not double-tax unless card says so

Fallback if EffectType injection is blocked mid-project:
  - First-class custom Poison tracker with DoT ticks that still aims to look like a status
  - Design doc identity unchanged; impl debt tracked in changelog

### 11.3 Hooks
| Hook              | Use |
|-------------------|-----|
| OnFiredBullet     | Optional muzzle VFX; prefer damage path for apply |
| OnBeforeDamage    | Mults, self-tax, precision flags |
| OnDamageTarget    | Needle +1, poison apply confirm, Polypharmacy checks |
| OnSaturateTarget  | Full-sat payoffs, Pandemic registration, Leech Ledger tracking |
| OnKillTarget      | Backlog inheritance; optional Extract refunds |
| Player damage     | Crash Cart threshold; Triage mult on Poison DoT flags |

### 11.4 State host
NeedleCarbineBehaviour (or true Gun subclass when prefab exists):
- WeaponData: apply mults, thresholds, extract powers, self-dose flags, rider elements
- Runtime: per-target needle map (instance id → stacks, lastHitTime)
- Runtime: set of brains you poisoned for Leech Ledger
- Prefab snapshot restore on upgrade Remove (template pattern)

### 11.5 HUD
- Crosshair target: needle pips 0..threshold + Poison sat fragment if readable
- Self: toxin icon when self sat > 0; full-sat warning pulse (Path C)
- Prefer SparrohUILib if dependency acceptable; else minimal world/highlighter text

### 11.6 Multiplayer
- Sandbox mod; all clients need the same plugin
- Status application must follow host/owner rules of IDamageSource.DamageTarget
- Needle map: owner-authoritative for local gun; validate supercombine on damage authority
- Do not assume client-only status writes stick

### 11.7 VFX / audio priority
1. Needle stick + count telegraph
2. Supercombine crack / burst
3. Poison full-sat verb + loop (StatusEffectData)
4. Extract “billing” confirm
5. Self-dose warning stinger (Path C)


## 12. Deliberate Non-Goals

- Not replacing vanilla weapons or Acid grenade identity
- Not a Globbler flood primary
- Not baseline free multi-element shotgunning (riders are upgrade-owned)
- Not full DR / immortal self-Poison tank as default Path C
- Not loading R with baseline hold abilities
- Not shipping Decay/Rot apply in first 30 without playtest of Poison spine
- Not requiring custom Unity prefab for v1 (runtime clone OK); prefab is later polish
- Not effect-mixing exploits against EnableEffectMixing = false


## 13. Open Tuning Questions (playtest, not design blockers)

1. Supercombine threshold 7 vs 5–6 for game fire rates and mag sizes.
2. Needle grace 3.0s — too strict for mid-range tracking shots?
3. Baseline Extract heal amount vs boss fight sustain.
4. Poison full-sat lifetime 5.5s vs Leech Ledger uptime.
5. How hard Secondary Septicemia / Formulary secondary amounts should be vs Cycler element cards.
6. Autotoxin self-heal cap on supercombine — prevent immortal loops with Leech Ledger.
7. Mercy Kill weight table vs elite/boss HP.
8. Whether needles track per EnemyBrain vs per part (brain recommended).
9. EffectType injection approach after first assembly audit post-game update.


## 14. Success Criteria / Player Fantasy Checklist

- [ ] Dart-dart-dart → 7 stacks → visible supercombine bang without any upgrades
- [ ] Poison full-sat DoT visibly cooks after detonate dump
- [ ] Extract sips sustain off a poisoned target between packs
- [ ] Build a rainbow DoT target and Formulary-detonate for a fat multi-status payoff
- [ ] Mercy Kill a fully charted elite for a huge execute
- [ ] Leech Ledger ammo drip while three poisoned grunts tick
- [ ] Autotoxin: stay self-Poisoned on purpose and feel powerful but mortal
- [ ] Sympathetic Double Dose writes your fever onto the boss
- [ ] Crash Cart saves a greedy overdose once per fight
- [ ] Hybrid Formulary + Mercy Kill + Autotoxin feels intentional, not patched


## 15. Strengths, Weaknesses & Co-op

Strengths
- Unique supercombine telegraph and audio fantasy
- Real Poison status for systemic interactions
- Deep multi-DoT + consume + self-dose build space
- Skill expression in focus fire, chart management, self-harm timing

Weaknesses
- Low brain-off DPS
- Supercombine whiffs if target-swapping too fast
- Own Medicine can down the operator
- Setup time vs hyper-aggressive rush waves

Co-op
- You poison and stick; allies benefit from softened / DoT’d packs
- Avoid team-hostile chem auras unless a future exotic explicitly adds them
- Mercy Kill is owner-gated to your chart where possible (you consume what you applied;
  if technical limits force “any status on target,” document and tune damage down)


## 16. Visual, Audio & Thematic Design

Appearance
- SAXON field injector carbine: white/grey clinical plating, warning stripes, ampule magazine,
  pneumatic tubing, fungal-etched hazard labels
- Darts: thin translucent needles with teal toxin reservoir
- Supercombine: needles vibrate → crystalline crack → teal-white burst

Sound
- Fire: suppressed pneumatic spit
- Stick: light metallic tick (count can subtly rise in pitch with stacks)
- Supercombine: glass-break + wet pressure release
- Extract: UI “stamp” + short suction
- Self-dose: heartbeat filter + alarm tick when full sat

Flavor / SAXON blurb (draft)
“SAXON N-series Needle Carbine — For rapid field inoculation of hostile mycoforms.
Not for operator self-trial. (Ignore previous sentence if Directive 7-B applies.)”


## 17. Locked Review Decisions (2026-08-06)

| Decision              | Lock |
|-----------------------|------|
| Form factor           | Needle Carbine |
| Slot                  | Primary |
| Poison model          | True new EffectType + PoisonStatusEffect |
| Baseline Poison       | DoT at full saturation |
| Self-harm             | Spicy (real player DoT) |
| Tone                  | Medical / industrial |
| Multi-DoT             | First-class upgrade riders (Fire/Shock/Acid/Bees in ship pool) |
| Needler DNA           | Baseline needle stacks + supercombine detonate |
| Supercombine threshold| 7 (playtest 5–9) |
| Baseline RMB          | Extract (teach consume early) |
| Doc file              | NeedleCarbine-DesignDoc.txt (this file) |
| Ship pool             | Frozen 30 listed above |
| Path names            | Blowdart / Sadist / Own Medicine |
| Crowns                | Pandemic, Formulary, Mercy Kill, Leech Ledger, Autotoxin, Sympathetic |
| MycoMod flag          | IsSandbox at implementation |
| Working APIName       | needle_carbine |
| Optional name flair   | Player-facing “Needle Carbine”; SAXON N-series in flavor text |


## 18. Changelog

v1 (2026-08-06)
- Initial full design from locked user decisions + Needler supercombine addition
- Research anchors: vanilla StatusEffect saturation, EffectType table, DMLR/Blood Carver doc patterns,
  wiki Cycler/Globbler/Immolator/Energy Convergence / multi-element upgrade DNA
