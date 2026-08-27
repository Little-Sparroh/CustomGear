# Splash Canister — Design Document

> Status: **Design only** — no implementation yet.  
> Working title in notes: Splash Grenade / Splashnade. **Ship name: Splash Canister.**  
> Template base: `.new.SplashGrenade` grenade content project.  
> Source notes: `splashnade.txt`.

---

## 1. High Concept / Fantasy

**Splash Canister** is the throwable catalyst.

Where **Incendiary** is a fire boom economy, **Shock Grenade** is mobility lightning, **Acid Grenade** is puddles and pull toys, and **Photon Disc** is a tumbling attuned energy disc, Splash Canister is **water as primer**: you soak targets and ground so every other element pays out harder — while a **thin regenerating shell** eats chip damage so you can stay in the wet zone and cook the room.

**One-liner:** *Soak the field. Every element hits harder when it’s wet — and a thin regenerating shell keeps chip damage off you while you cook the room.*

**Element:** `EffectType.Water` (not Fire, Shock, Acid, or Bees).  
This is load-bearing identity: Water is a **catalyst status** (no DoT of its own), with real vanilla hooks into Shock and room to grow multi-element reaction kits. Cryo is a first-class reaction partner (full kit expected next patch).

---

## 2. Role in the Arsenal

| Gear | Fantasy | Splash Canister relationship |
|------|---------|------------------------------|
| **Incendiary Grenade** | Instant fire boom + self-fire economy | Splash trades mono-fire burst for **wet primer + steam reactions**. No wildfire / combust stack clone. |
| **Shock Grenade** | Shock boom + mobility / Live Wire | Splash **wets**; Shock **pays out** on wet (vanilla amp). Complementary, not a second shock nade. |
| **Acid Grenade** | Puddles, vacuum pull, overhealth pulse | Splash lake can *become* acid when infused; baseline is water slick, not corrosion. Polymer-style big overshield dump is **not** the healing identity. |
| **Photon Disc** | Tumble disc + Fire/Shock/Acid **attunement** | Wholly different projectile and fantasy. Disc asks *“what element am I?”*; Splash asks *“what happens when elements hit wet?”* No tumble, wave trail, attunement UI, or DiscWorld. |
| **Honey Jar** | Bees element + minions + pure HoT nectar | Soft synergy only: Fluid Morphology **Bees** lake grants ally pure HoT. No code coupling. |
| **Friend in a Box** | Deployable ally AI | Complementary slot. Splash zones are **fields**, not units. |

**Niche tags:** Elemental Manipulation · Catalyst · Regenerating Shield · Zone Control

**Slot:** Throwable / grenade gear  
**Range:** Mid throw arc, impact AOE + optional lingering lake  
**Role:** Multi-element enabler, soft CC/control, chip-damage aegis

---

## 3. Design Pillars

1. **Water is the primer, not the payload**  
   Baseline applies Water hard and deals modest impact damage. Burst DPS lives on **reactions** and partner elements — not a fat mono-element boom.

2. **Multi-element builds are the fantasy**  
   Upgrades reward Fire, Shock, Acid, Bees, and Cryo interacting with wet targets and wet ground. Splash is the glue for elemental loadouts.

3. **Regenerating shield, not HoT tank**  
   Healing identity is a **small (≈5–10) overshield** that blocks chip, breaks, then **rebuilds quickly after a short undamaged delay**. Not Honey Jar nectar, not Welding Heat burst heal, not Polymer’s large one-shot overshield dump.

4. **Gravity wells, not locked paths**  
   Upgrade “paths” are **thematic attractors** (Catalyst / Hydrodynamics / Aegis). Players mix freely. Path locks only where a rare explicitly requires an exotic (Undertow → Tsunami).

5. **Readable exotics**  
   Three mode-defining exotics: knockback wave · suspension trap · living elemental lake. Same large hex footprint each.

6. **Not Photon Disc**  
   No disc tumble, ground wave trail, bounce-chain identity, attunement vector, ammo-toss economy, or gun-as-disc conversion. Different silhouette every throw.

7. **Self-contained v1**  
   No hard dependencies on Honey Jar, DiscWorldRework, FriendinaBox, or other mods. Wiki synergies are **player-facing notes only**.

8. **Grounded in real APIs**  
   Water/Cryo/Shock interactions, puddle/zone patterns, and negative-heal overshield patterns come from decompile — do not invent parallel status systems when vanilla hooks exist.

---

## 4. Baseline (No Upgrades)

**Feel target:** status-first throwable that teaches “wet is a catalyst” on the first throw. Shared family boom numbers; identity is Water primer + slick + shield upgrades.

**Shared family baseline (locked):** damage **100** · effect amount **10** · max charges **3** · recharge **45** · explosion radius (`hitForce`) **6**.

| Property | v0 target | Notes |
|----------|-----------|--------|
| Throw / arc / bounce | Incendiary-like | Clone throw feel; swap element + behaviour |
| Impact damage | **100** | Shared family boom; delivery + wet apply |
| Element | `EffectType.Water` | Player-facing “wet/soaked”; not a separate enum |
| Water effect amount (`damageEffectAmount`) | **10** | Full-wet class dump from empty (shared family) |
| Explosion radius (`hitForce`) | **6** | Tunable via High Pressure |
| **Aftershock slick** | **Weak wet slick, ~1.5–2.5s** | Re-applies light Water in radius; sells niche immediately |
| Slick tick | Tiny/no damage + small Water application | Presence > power |
| Healing / shield | **None** | **Regenerating Shield** (Liquid Metal / Hydro Barrier) is upgrade fantasy |
| Max charges | **3** | Shared family baseline; Double Canteen stacks on top |
| Recharge duration | **45** | Shared family baseline |
| Self-hit | Water **can** apply to thrower at reduced amount | Insulated Shell reduces further |
| Self damage | Low impact damage to self vs enemies | |


### Baseline detonation sequence

```
Throw → bounce/fuse (vanilla grenade rules)
  → Primary boom (DamageData: modest damage, EffectType.Water, high effect amount, AOE)
  → Spawn weak WetSlick at impact (short duration, light re-wet)
  → Slick expires quietly (no lake / wave / trap unless upgraded)
```

### Why boom + weak slick (not boom-only / always-lake)

- Boom-only reads as “another elemental nade” with a boring element.
- Always-lake steals Fluid Morphology’s exotic beat.
- Weak slick teaches the niche and gives Spring Source / lake upgrades something to grow.

---

## 5. Water, Cryo, Reactions & Shield Rules (System Truths)

### 5.1 Water element (from game)

- `EffectType.Water = 9`
- `WaterStatusEffect`: **no DoT** on full saturation
- Full saturation lifetime **4.5s** (longer than default 3s Fire/Shock/Acid)
- Vanilla interactions already shipped:
  - On Water init: if target has Shock saturation > 0 → add **+10 Shock**
  - On Shock `Add`: if target is **fully Wet** → Shock amount forced to **10** (full dump)
  - On Shock full-sat tick: if fully Wet → Shock DoT **×1.25** and gains **Precision**
- Saturation add: `amount * 0.1` per application → roughly **10 effect amount ≈ full wet** from empty, before decay
- `StatusEffect.EnableEffectMixing = false` — design around **sequential and zone-mediated** combos, not true dual-status fusion on one application tick

**Design read:** Water alone is intentionally “weak DPS.” Its power is **enabling** Shock (vanilla) and Splash reaction cards (mod).

### 5.2 Cryo element (from game + next-patch expectation)

- `EffectType.Cryo = 10`
- Full saturation lifetime **4s**
- While fully saturated: **SlowTargetThisTick(0.6f)** every update
- Shatter meter: non-Cryo damage builds toward **200**; melee fills fast on enemies; shatter deals large precision-style burst and clears Cryo
- **Cold Snap** and Fluid Morphology **Cryo lake** key off real Cryo saturation (full kit expected next patch — do not invent a fake “chill” status)

### 5.3 “Wet” language for upgrade text

| Term | Meaning |
|------|---------|
| **Wet / soaked** | Target has Water status with saturation > 0 |
| **Fully wet** | `IsFullySaturated` on Water |
| **Applying water** | Damage/effect that adds Water saturation (boom, slick, lake, wave front) |
| **Reaction** | A Splash upgrade effect that triggers when another element meets wet state or wet zone |

### 5.4 Regenerating shield (Liquid Metal) — precise rules

```
LiquidMetalShield (personal, while upgrade equipped and upgrades enabled):
  maxHP          = 5–10 base (stackable via Surface Tension)
  currentHP      = 0..maxHP
  regenDelay     = ~1.0s after last damage to shield OR player health
  regenRate      = fast (full refill in ~1.5–2.5s once regen starts)
  on damage      = absorb from shield first; remainder hits health
  on break       = currentHP hits 0; delay restarts; no explosion unless later upgrade
  does NOT       = heal missing base health; grant permanent armor; stack infinite layers
  display        = clear stack/bar while maxHP > 0
```

**Intent:** block chip and stray DoT ticks so the player can stand in reaction zones. Breaks to real hits. Rebuilds if you stop eating damage briefly.

### 5.5 Hydro Barrier stacks (separate from Liquid Metal)

```
HydroBarrierPulse (on boom, if upgrade equipped):
  allies in radius (including thrower) gain +1–2 overshield stacks
  stack policy   = small cap (e.g. max 4–6 from this source); short decay or timed
  type           = overshield HP (negative-heal / overhealth pattern — Acid Polymer analogue, much smaller)
  NOT the same   = Liquid Metal’s personal regen engine
```

### 5.6 Aqua Resonance

While `LiquidMetalShield.currentHP > 0` (shield is **up**, not merely equipped): gain **elemental damage resistance** (Fire/Shock/Acid/Bees/Cryo/Water incoming — tune as a single elemental DR mult).  
When shield is broken (0 HP), resonance is off until shield regenerates above 0.

### 5.7 Fluid Morphology lake — infusion rules (locked)

```
Lake spawns on detonate when Fluid Morphology equipped (after boom + slick rules).
State: Uninfused (Water slick+) until FIRST qualifying infusion.

Infusion authority: LOCAL PLAYER only
  - First damage/effect application that:
      (a) is caused by the local player (or their gear / their Splash systems), AND
      (b) carries EffectType in { Fire, Shock, Acid, Bees, Cryo }, AND
      (c) hits the lake volume or a target standing in the lake (implementation choice: prefer
          damage events with origin in lake radius OR direct lake collider element apply)
    → locks lake mode for remaining lifetime
  - Ally elements do NOT infuse (co-op cannot steal your lake identity)
  - Water applications never infuse (already the default)
  - Normal / non-element damage does not infuse

Once locked: mode cannot change until lake expires.
Uninfused lake: pure Water re-wet + Spring Source fuel + light pressure.
```

| First element (local player) | Lake becomes |
|------------------------------|--------------|
| **Fire** | Burning lake — Fire DoT / ignite pressure in zone |
| **Shock** | Live electrical field — Shock apply + stun-leaning ticks (rides vanilla wet+shock amp) |
| **Acid** | Acid puddle analogue — Acid apply; optional Decay lean on metallic if easy later |
| **Bees** | Honey lake — **ally pure HoT** (temporary regeneration only; no overshield, no permanent regen) |
| **Cryo** | Snow field — heavy slow via Cryo apply / move slow; sets up Cold Snap / shatter |

**Bees lake HoT (locked pure HoT):**

```
HoneyLakeRegen (allies in zone, including thrower):
  regenPerSecond = R        // weaker than Honey Jar Sugar Coated peak
  while inside only         // exits clear quickly
  type = healing over time to base health
  NOT = blue overhealth, DR, permanent regen delay changes
```

### 5.8 Overshield implementation anchor (from game)

Acid Polymer Coating pattern: `IDamageSource.HealTarget(source, player, -50f, position)` — negative heal grants overhealth.  
Splash uses the same family of API for Hydro Barrier / small pulses; Liquid Metal may be a dedicated player stack that absorbs in `OnBeforeTakeDamage` for cleaner regen-delay semantics. Impl chooses the cleaner hook; fantasy stays identical.

---

## 6. Gravity Wells (Thematic Attractors)

Not exclusive trees. Taking one exotic or epic pulls related cards into value; every upgrade remains equippable with every other (except explicit requirement flags).

### Well A — **Catalyst** (reaction economy)

*Wet is the primer. Other elements pay out.*

Core pieces: Boiling Point, Tesla Coil, Cold Snap, Dilution, Ebb and Flow, Capillary Action, Soaking Wet, reaction window standards.

### Well B — **Hydrodynamics** (control / displacement)

*Move bodies, trap, drag, wave.*

Core pieces: Tsunami, Bubble Trap, Riptide, Undertow, High Tide, Churn, knockback/radius cards.

### Well C — **Aegis** (regenerating shield + sustain)

*Chip shell, pulse barrier, stand-in-water recharge, elemental DR while shielded.*

Core pieces: Liquid Metal, Hydro Barrier, Spring Source, Aqua Resonance, Surface Tension, Capillary Seal, Insulated Shell.

**Fluid Morphology** sits across **A + B** as the living-lake exotic — multi-element centerpiece with zone control.

### Mix examples (expected, not edge cases)

- Fluid Morphology + Boiling Point + Incendiary loadout → lake locks Fire, steam clouds on wet ignites  
- Tsunami + Undertow + Riptide → wave into drag pit, bonus damage window  
- Liquid Metal + Aqua Resonance + Spring Source → stand in slick, chip shell up, grenade rolling  
- Tesla Coil + Shock gun / Photon Disc → wet → electrocute arcs (Splash wets, partner shocks)  
- Cold Snap + Cryo lake + Cryo weapon → freeze payoff detonations  
- Dilution + Acid gun → corrosion spread on wet/corroded packs  
- Bubble Trap + any Catalyst epic → fixed targets for reactions  

---

## 7. Content Budget & Universal Truths

Aligned with Honey Jar / FriendinaBox / vanilla grenade expectations:

| Rule | Value |
|------|--------|
| Total upgrades | **~30** (named kit + standards + fillers) |
| Exotics | **3** — Tsunami, Bubble Trap, Fluid Morphology |
| Exotic hex footprint | **Equal and large** across all three |
| Epics | **~7–8** |
| Rares | **~9–10** |
| Standards | **~8–10** (stackable spine) |
| Path locks | **Undertow** requires **Tsunami** only |
| Oddity / Contraband grid steal | Out of scope unless later parity pass |
| Shared vanilla staples (Grenade Belt, In For A Penny line, Boundary Incursion) | Optional parity later — **not** required in custom ~30 |
| v1 cross-mod | **None** |

---

## 8. Full Upgrade Table

IDs are design placeholders (implementation range suggestion: **gear 92300**, **upgrades 92301+** — avoid template `920xx`, Friend `921xx`, Honey Jar `922xx`).

Rarity key: **S** Standard · **R** Rare · **E** Epic · **X** Exotic  

Stack: **✓** CanStack · **—** unique  

Well: primary gravity well (still mixable)

### 8.1 Standards (~9) — stat spine

| # | Name | Well | Stack | Intent | Rough numbers (v0) |
|---|------|------|-------|--------|---------------------|
| 1 | **High Pressure** | Hydro | ✓ | Explosion + slick/lake radius | +15–30% radius |
| 2 | **Soaking Wet** | Catalyst | ✓ | Water effect amount on boom (and slick slightly) | +20–40% effect amount |
| 3 | **Quick Valve** | Aegis | ✓ | Faster recharge | −12–20% recharge |
| 4 | **Deep Reservoir** | Hydro | ✓ | Slick / lake / steam cloud duration | +20–35% linger durations |
| 5 | **Insulated Shell** | Aegis | ✓ | Less self Water application from your canister | −25–40% self effect amount |
| 6 | **Hard Case** | Catalyst | ✓ | Impact damage | +12–22% boom damage |
| 7 | **Double Canteen** | Aegis | ✓ | + max charges; longer recharge | +1 charge; +15–25% CD |
| 8 | **Surface Tension** | Aegis | ✓ | Liquid Metal max shield HP | +2–4 shield HP per stack |
| 9 | **Churn** | Hydro | ✓ | Tsunami / Riptide force and control radius | +15–25% control force/radius |

### 8.2 Rares (~10)

| # | Name | Well | Stack | Intent | Notes |
|---|------|------|-------|--------|-------|
| 10 | **Liquid Metal** | Aegis | — | Gain a **personal regenerating shield** that rebuilds quickly after you stop taking damage | Primary healing/defense identity |
| 11 | **Spring Source** | Aegis | ✓ | Standing in **your** water slick / lake recharges the canister faster | Gas Valves analogue; your zones only |
| 12 | **Ebb and Flow** | Catalyst | ✓ | Outgoing damage increased (mild). **Stronger** when the hit is random/converted elemental (Elemental Munitions / Volatile Element / Harmonizer-style) | Lower than Napalm-tier mono cards on plain damage |
| 13 | **Aqua Resonance** | Aegis | ✓ | While regenerating shield **has HP > 0**, take reduced **elemental** damage | Requires Liquid Metal to matter fully |
| 14 | **Undertow** | Hydro | ✓ | While **Tsunami** equipped: enemies caught in the wave are left defenseless / take bonus damage for X duration | **Requires Tsunami** |
| 15 | **Capillary Seal** | Aegis | ✓ | Liquid Metal regen delay reduced; slightly faster regen rate | Shield feel card |
| 16 | **Rinse Cycle** | Catalyst | ✓ | Re-applying Water to a fully wet target deals a small bonus damage instance (catalyst drip) | Rewards sustained wet |
| 17 | **Floodgate** | Hydro | ✓ | Weak slick lasts longer and re-wets slightly harder | Pre-Morphology field card |
| 18 | **Spray and Pray** | Catalyst | — | Boom applies a short “reactive film”: next elemental hit on affected enemies from you deals bonus damage once | One-shot amp mark |
| 19 | **Drift Current** | Hydro | ✓ | Brief movespeed burst when your canister detonates | Mobility glue |

### 8.3 Epics (~8)

| # | Name | Well | Stack | Intent | Notes |
|---|------|------|-------|--------|-------|
| 20 | **Boiling Point** | Catalyst | — | Targets who are **wet** and become **ignited** create a **steam cloud** that damages targets in the area | Fire reaction |
| 21 | **Tesla Coil** | Catalyst | — | Targets who are **wet** and become **electrocuted** briefly **arc lightning** to nearby targets | Shock reaction; loves vanilla wet+shock |
| 22 | **Cold Snap** | Catalyst | — | Targets who are **wet** and reach **full Cryo** instantly **detonate** for bonus damage | Cryo reaction (next-patch full Cryo support) |
| 23 | **Dilution** | Catalyst | — | If the boom hits a **corroded** target, **spread corrosion** to all in the explosion radius | Acid reaction / pack clear |
| 24 | **Riptide** | Hydro | — | Creates a force **inward** that slowly drags enemies to the epicenter | Vacuum Tube cousin; softer/longer |
| 25 | **Hydro Barrier** | Aegis | — | Boom pulse grants a **small stacking (1–2) overshield** to players in range | Team aegis beat |
| 26 | **Capillary Action** | Catalyst | ✓ | Your **guns** apply slightly more element amount to targets that are **already wet** | Loadout glue |
| 27 | **High Tide** | Hydro | ✓ | Larger boom radius and longer baseline slick; mild knockback on boom | Control spine epic |

### 8.4 Exotics (3)

| # | Name | Well | Stack | Intent |
|---|------|------|-------|--------|
| 28 | **Tsunami** | Hydro | — | On impact, creates a **wave** that **knocks enemies back** and applies light Water along the front |
| 29 | **Bubble Trap** | Hydro | — | Enemies hit by the grenade are **suspended in the air** for a duration, unable to move |
| 30 | **Fluid Morphology** | Catalyst / Hydro | — | Creates a **lake of water**. The **first element the local player applies** to the lake changes its effect (Fire / Shock / Acid / Bees / Cryo) |

*Count = 30 on the nose. Stretch candidates live in §12 if standards merge in balance.*

---

## 9. Exotic Deep-Dives

### 9.1 Tsunami (Hydrodynamics)

**Fantasy:** A wall of water shoves the room.

**Behaviour sketch:**

- On detonate (after primary boom): emit a **radial wave** from impact (expanding ring or short-lived front).
- Enemies hit by the wave: **knockback** away from epicenter + light Water application.
- Duration of wave travel: short (readable one-beat control), improved mildly by Churn / High Pressure.
- Does **not** replace the boom; it is the exotic body of the throw.
- Visual: water ring / foam crest (placeholder OK).

**Undertow (Rare):**  
If Tsunami is equipped, enemies hit by the wave gain a short **defenseless / bonus damage taken** debuff (X duration, stackable mild on duration or mult). If Tsunami unequipped, property no-ops (or grey text “Requires Tsunami”).

**Mix notes:**  
Pairs with Riptide (wave out → drag in is spicy; tune so they don’t cancel — prefer wave first, then drag on remaining fuse fantasy, or Riptide on boom and wave as separate pulse). Bubble Trap can catch wave victims mid-air. Catalyst reactions love clumped post-wave packs if knockback is tuned moderate.

**Knockback budget:**  
Should feel great on trash; bosses/CC resist use game norms. Never map-eject players unintentionally (friendly knockback off or heavily reduced).

### 9.2 Bubble Trap (Hydrodynamics)

**Fantasy:** Enemies hang in glistening spheres — free aim, free reactions.

**Behaviour sketch:**

- Enemies damaged by the **primary boom** (and optionally Tsunami wave if both equipped) are **suspended**: no movement, still damageable, brief duration (e.g. 1.25–2.5s v0).
- ICD / max targets per throw to prevent endless CC chains with Double Canteen.
- Bosses: shortened duration or partial slow instead of full suspend if needed for fairness.
- Visual: bubble / sphere VFX on affected targets.

**Mix notes:**  
Best friends with Boiling Point, Tesla Coil, Cold Snap (fixed targets). Fluid Morphology lake under a bubble clump is peak co-op theater. Not a hard stun-lock build if durations stay honest.

**Risk budget:**  
CC exotic must not delete enemy agency for long pack fights. Prefer strong but short.

### 9.3 Fluid Morphology (Catalyst / signature)

**Fantasy:** You spill a lake. The first element you feed it becomes the weather.

**Behaviour sketch:**

- On detonate: spawn **Lake** at impact (upgrades baseline slick into a serious zone, or replaces weak slick — prefer **consumes/upgrades** slick layer to avoid double zones).
- Duration: medium (e.g. 8–14s v0), improved by Deep Reservoir.
- **Uninfused:** Water re-wet ticks, Spring Source eligible, light/no damage.
- **Infusion:** see §5.7 — **local player only**, first of Fire / Shock / Acid / Bees / Cryo locks mode.
- Mode behaviours:

| Mode | Enemies | Allies |
|------|---------|--------|
| Fire | Fire apply + DoT pressure | None special |
| Shock | Shock apply + stun-leaning ticks | None special (self-shock reduced) |
| Acid | Acid apply (Decay optional later) | None special |
| Bees | Light bee pressure optional / or none | **Pure HoT** while inside |
| Cryo | Cryo apply + heavy slow | None special |

- Visual: water plane that recolors / VFX-swaps on infusion (critical readability).

**Mix notes:**  
Works under Tsunami/Bubble (lake at center). Boiling Point steam can proc off wet+ignite inside Fire lake. Tesla Coil loves Shock lake + wet. Cold Snap loves Cryo lake. Bees lake is support ceiling without stealing Honey Jar’s hive identity.

### 9.4 Exotic coexistence

| Pair | Rule |
|------|------|
| Tsunami + Bubble Trap | Allowed. Wave may apply trap if both equipped (shared CC budget via shorter durations). |
| Fluid Morphology + either | Allowed. Lake at impact; wave/trap still fire. |
| All three | Allowed if grid fits; power budget via duration/ICD/radius, not hard ban. |
| Footprints | All three exotics **same cell count**, larger than typical rares/epics. |

---

## 10. Named Kit — Detailed Specs

### Boiling Point (Epic)

- When a **wet** target becomes **ignited** (Fire full sat / ignite event — match game verb), spawn a **steam cloud** at their position.
- Steam: short duration AOE damage (Neutral or Fire-typed with 0 extra sat to avoid loops), damages enemies in radius.
- ICD per target to prevent ignite-tick spam.
- Optional: consume a portion of Water sat on proc (readable “flash to steam”) — v0 can skip consume if feel suffers.
- Stack: unique.

### Tesla Coil (Epic)

- When a **wet** target becomes **electrocuted** (Shock full sat), briefly arc to nearby enemies (damage + optional light Shock apply).
- Arc count and range modest; ICD per source target.
- Explicitly benefits from vanilla wet+shock amp without reimplementing it.
- Stack: unique.

### Cold Snap (Epic)

- When a target is **wet** and reaches **full Cryo** saturation, trigger a **bonus detonation** damage instance on that target (and mild AOE optional).
- Does **not** replace Cryo shatter; it is an additional wet payoff. Shatter still works via normal Cryo rules.
- ICD per target per Cryo life cycle.
- Stack: unique.
- **Locked trigger:** real **Cryo** full saturation only (no fake chill layer).

### Dilution (Epic)

- On primary boom DamageTarget path: if target is **corroded** (Acid full sat or Acid sat > 0 — prefer **fully corroded** for clarity), apply Acid effect amount to **other enemies** in explosion radius.
- Pack-spread fantasy; weaker than primary acid weapons on purpose.
- Stack: unique.

### Riptide (Epic)

- On detonate (or during fuse if vacuum-style pre-pull desired): apply inward pull toward epicenter for T seconds at intervals (Acid Vacuum Tube cousin).
- Softer continuous drag rather than only pre-explode yank — differentiate feel from Acid.
- Churn scales force/radius.
- Friendly players: no pull or heavily reduced.
- Stack: unique.

### Hydro Barrier (Epic)

- On boom: players in radius gain **+1–2 overshield HP** (stacking, capped).
- Small numbers — team chip buffer, not Polymer −50 dump.
- Stack: unique (or mild stack on pulse size if needed).

### Capillary Action (Epic)

- While equipped: when local player deals gun/projectile damage to a **wet** enemy, multiply **element amount** applied by that hit (e.g. +15–30% effect amount), not necessarily raw damage.
- Reinforces multi-element gun + Splash loop.
- Stack: mild.

### High Tide (Epic)

- Increases explosion radius and baseline slick duration/radius; adds mild knockback on boom even without Tsunami.
- Stack: mild.

### Liquid Metal (Rare)

- Enables personal regenerating shield per §5.4.
- Stack: unique (size via Surface Tension).

### Spring Source (Rare)

- While local player stands in **their** WetSlick or Lake: multiply grenade recharge rate.
- Clear feedback (audio/UI optional).
- Stack: mild.

### Ebb and Flow (Rare)

- Base: small outgoing damage mult (lower than Incendiary Napalm / Shock Higher Voltage mono cards).
- Bonus tier: additional mult when the damage instance has a **non-Normal** effect that was **converted/random** (flags or heuristics: random element rolls, Harmonizer convert, Volatile Element, Odd Cocktail-style). If detection is too hard in v1, approximate with “non-Water elemental damage you deal” at a middle bonus — prefer true convert detection when hooks allow.
- Stack: mild.

### Aqua Resonance (Rare)

- While Liquid Metal shield currentHP > 0: elemental incoming damage multiplier < 1.
- Stack: mild.

### Undertow (Rare)

- Gate: Tsunami equipped.
- Wave-hit enemies: bonus damage taken debuff for X seconds.
- Stack: mild on duration or mult.

---

## 11. Competitive Differentiation — Photon Disc

| Axis | Photon Disc | Splash Canister |
|------|-------------|-----------------|
| Projectile | Fast disc, surface tumble | Classic grenade arc |
| Identity mechanic | **Attunement** (Fire/Shock/Acid loadout vector) | **Wet catalyst** + **lake infusion** |
| Element spine | Shock base + attuned swaps | Water base + reaction payoffs |
| Signature exotic | DiscWorld / Linked List / Bullet Time | Fluid Morphology lake |
| Motion fantasy | Bounce, wave trail, chain | Wave knockback, bubbles, riptide drag |
| Defense | Chunks, self-damage loops | Regenerating chip shield |
| Build question | “What am I attuned to?” | “What am I combining with wet?” |

Splash should **never** feel like “Photon Disc but water.” If a card starts reading as attunement or tumble-disc, cut or rewrite it.

---

## 12. Soft Synergy Notes (Player-Facing, Soft Only)

No mod dependencies. Loadout tips for README / codex blurb.

| Partner | Why it feels good |
|---------|-------------------|
| **Fire guns / Incendiary** | Boiling Point steam; Fire lake infusion |
| **Shock guns / Shock Grenade** | Tesla Coil; vanilla wet+shock amp; Shock lake |
| **Photon Disc** | Disc shocks what Splash soaked — complementary roles in one loadout |
| **Acid guns / Acid Grenade / Globbler** | Dilution; Acid lake; corrosion packs |
| **Cryo weapons / sources (next patch)** | Cold Snap; Cryo lake; shatter follow-ups |
| **Honey Jar** | Bees lake pure HoT; thematic water+honey support without code link |
| **Elemental Munitions / Volatile Element / Harmonizer / Syzygy** | Ebb and Flow payout; Capillary Action gun glue |
| **Bruiser Shield Projector** | Different layer (deployed hard-light vs personal chip shell) |

**Explicit non-goals v1:** DiscWorld hooks, Honey Jar hive API, Friend deployable integration, shared trackers across mods, custom fake elements.

---

## 13. Strengths, Weaknesses & Failure Modes

### Strengths

- Makes multi-element loadouts feel intentional  
- Unique Water throwable identity in a Fire/Shock/Acid-heavy grenade row  
- Control tools (wave, bubble, riptide) without being a pure CC stick  
- Chip shield enables aggressive standing in your own zones  
- High co-op readability (lake infusion, steam, arcs)  
- Mix-and-match wells create many boards  

### Weaknesses

- Lower raw burst than Incendiary without reactions  
- Water alone does not DoT — dead turn if you never apply a second element  
- Shield is weak to burst / sustained boss damage  
- Lake infusion requires local player follow-up (skill expression; also a footgun)  
- Cryo payoff waits on Cryo availability in the sandbox  

### Failure modes to avoid in tuning

| Failure | Mitigation |
|---------|------------|
| Permanent sustain god | Small max shield; delay on any damage; no base HoT on Liquid Metal |
| Hydro Barrier = Polymer −50 | Tiny stack values (1–2), hard cap |
| Fluid Morphology always best mono nade | Uninfused lake is weak; power is in infusion + reactions |
| Ally steals lake mode | **Local player infusion only** (locked) |
| Boiling Point / Tesla infinite loop | ICD; steam/arc apply 0 extra sat or non-recursive flags |
| Bubble Trap infinite CC | Short duration, max targets, boss resist |
| Tsunami yeets allies / objectives | No friendly knockback; clamp force |
| Ebb and Flow > mono Napalm always | Mild base; bonus only on convert/random elemental |
| Reads as Photon Disc | No attunement, tumble, or disc economy cards |
| Bees lake steals Honey Jar | Pure HoT only, zone-bound, no hives/minions |
| 30 upgrades but 3 builds only | Keep standards universal; minimize path locks |

---

## 14. Implementation Appendix (For Later — Not This Pass)

Design-only milestone: **this document**. When coding starts, prefer:

| Piece | Approach |
|-------|----------|
| Registration | Existing SplashGrenade `GrenadeRegistration` clone path; set `GunData.damageEffect = Water`, tune damage/effect amount |
| Name / IDs | Display **Splash Canister**; `APIName` e.g. `splash_canister`; gear id **92300**; upgrades **92301–92330** |
| Data host | `SplashCanisterBehaviour` (rename from example) with `Data` struct for all flags/scalars |
| Detonate | Harmony on `GrenadeBullet.Detonate` (template / FriendinaBox-style) |
| Field entities | `WetSlick`, `WaterLake`, `SteamCloud`, wave pulse helper, bubble suspend buff |
| Lake infusion | Listen for local player damage/element applies in lake radius; lock mode enum |
| Pull / wave | Reuse patterns from Acid vacuum / explosion force; custom ring cast |
| Suspend | Movement lock / float buff with duration + break on death |
| Liquid Metal | Player damage prefix absorb + regen timer (main thread) |
| Hydro Barrier | Negative heal overhealth pulse or small overshield stacks |
| Cryo hooks | `EffectType.Cryo` full sat events / status queries |
| Upgrades | `PlayerData.CreateUpgrade` + `UpgradeProperty` Apply/Remove restoring prefab snapshot |
| Mod flags | `[MycoMod(..., ModFlags.IsSandbox)]` |
| Cross-mod | None in v1 |

### Suggested `SplashCanisterBehaviour.Data` fields (sketch)

```
// Baseline / scales
float explosionRadiusMultiplier;
float waterEffectAmountMultiplier;
float boomDamageMultiplier;
float selfWaterMultiplier;
float slickDuration;
float slickRadiusMultiplier;
float slickTickInterval;
float slickTickWaterAmount;

// Tsunami
bool tsunami;
float tsunamiForce;
float tsunamiRadius;
float tsunamiWaterAmount;
float undertowBonusDamageMult;   // Undertow
float undertowDuration;

// Bubble Trap
bool bubbleTrap;
float bubbleDuration;
int bubbleMaxTargets;

// Fluid Morphology
bool fluidMorphology;
float lakeDuration;
float lakeRadius;
// runtime lake state not in upgrade data

// Riptide
bool riptide;
float riptideForce;
float riptideDuration;
float riptideRadius;

// Reactions
bool boilingPoint;
float steamDamage;
float steamRadius;
float steamDuration;
float steamIcd;

bool teslaCoil;
float arcDamage;
float arcRange;
int arcCount;
float arcIcd;

bool coldSnap;
float coldSnapDamage;
float coldSnapIcd;

bool dilution;
float dilutionAcidAmount;

// Aegis
bool liquidMetal;
float shieldMaxHP;
float shieldRegenDelay;
float shieldRegenPerSecond;

bool hydroBarrier;
float hydroBarrierShieldPerPulse;
float hydroBarrierRadius;

float springSourceRechargeMult;
float aquaResonanceElementalDr;  // e.g. 0.85 = 15% less
float capillaryActionElementMult;
float ebbAndFlowBaseMult;
float ebbAndFlowConvertedBonusMult;

// High Tide / Floodgate / etc.
float highTideRadiusMult;
float highTideSlickDurationMult;
float boomKnockbackForce;
```

### Ship cut vs stretch

**v1 must-ship (fantasy complete):**

- Baseline boom + weak slick + Water element  
- All 3 exotics (Tsunami, Bubble Trap, Fluid Morphology with all 5 infusion modes)  
- Boiling Point, Tesla Coil, Cold Snap, Dilution, Riptide, Hydro Barrier  
- Liquid Metal, Spring Source, Ebb and Flow, Aqua Resonance, Undertow  
- Full standard spine  
- Regenerating shield model + pure HoT bees lake  

**Stretch / post-v1:**

- Capillary Action gun hooks polish  
- High Tide / Floodgate juiciness  
- Spray and Pray mark VFX  
- Decay-on-Acid-lake metallic bonus  
- Audio/Wwise / custom mesh AssetBundle  
- Optional vanilla staple parity (Grenade Belt, etc.)  

---

## 15. Naming & Presentation

| Slot | Value |
|------|--------|
| Display name | **Splash Canister** |
| Internal / API | `splash_canister` |
| Design nicknames | Splash Grenade, Splashnade (notes only) |
| Short description | *Water-element grenade. Soaks targets and ground so other elements pay out. Upgrades add waves, bubble traps, living lakes, and a thin regenerating chip shield.* |
| Thunderstore name (later) | `SplashCanister` |
| GUID (later) | `sparroh.splashcanister` |
| Project folder (current) | `.new.SplashGrenade` |

### SAXON-ish flavor (optional codex)

> “Don’t drink it. Don’t store it next to the battery shelf. Do throw it at anything that hates being interesting.”  
> — Internal memo, Wetworks Division (redacted)

---

## 16. Open Questions (Balance / Feel — Not Blocking Doc)

1. Riptide + Tsunami: same throw ordering — wave then drag, or mutually tuned weaker when combined?  
2. Bubble Trap: boom-only apply vs boom+wave apply when Tsunami equipped?  
3. Steam cloud damage type: Normal vs Fire-with-zero-sat?  
4. Exact hex shapes for 30 upgrades — author during implementation pass.  
5. Default max concurrent lakes/slicks if Double Canteen + long duration? (Cap 2 field zones.)  
6. Ebb and Flow convert detection fidelity in v1 vs simpler “non-Water elemental” bonus.  

---

## 17. Design Checklist

- [x] Niche: Elemental Manipulation  
- [x] Healing: Regenerating shield (Liquid Metal) + small Hydro Barrier pulses  
- [x] Baseline: boom + weak wet slick + Water element  
- [x] Ship name: **Splash Canister**  
- [x] ~30 upgrades  
- [x] Wells interactive (mix/match), not exclusive paths  
- [x] Self-contained (no cross-mod hooks v1)  
- [x] Three exotics from splashnade.txt  
- [x] Epics/rares from splashnade.txt preserved and expanded  
- [x] Wholly distinct from Photon Disc (no attunement / tumble / disc economy)  
- [x] Fluid Morphology Bees mode: **pure HoT**  
- [x] Cold Snap: **full Cryo** saturation  
- [x] Lake infusion: **local player only**  
- [x] Water/Cryo/Shock grounded in decompile  
- [x] Implementation deferred  

---

## 18. Changelog (Design Doc)

| Date | Change |
|------|--------|
| 2026-08-15 | **Shared throwable baseline lock:** damage **100**, `damageEffectAmount` **10**, max charges **3**, recharge **45**, explosion radius (`hitForce`) **6**. Element confirmed **Water**. Sustain column confirmed **Regenerating Shield**. |
| 2026-05-08 | Initial design doc from `splashnade.txt`, wiki grenade/Photon Disc/element research, decompile anchors (Water/Cryo/Shock/Acid puddle/overhealth), Honey Jar structural lessons. User locks: ship name Splash Canister; Bees lake pure HoT; Cold Snap = Cryo; lake infusion local player only; design-only; ~30 upgrades; gravity-well mix paths; regenerating chip shield. |


---

*End of design document. Next step when ready: rename template identifiers and implement baseline Water boom + slick only, then layer upgrades by well.*
