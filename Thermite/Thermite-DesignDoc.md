# Thermite — Design Document

> Status: **Design only** — no implementation yet.  
> Working title in notes: Incendiary Grenade Rework. **Ship name: Thermite.**  
> Template base: `.new.IncendiaryGrenadeRework` grenade content project.  
> Product shape: **separate gear** — vanilla Incendiary Grenade is left unmodified.

---

## 1. High Concept / Fantasy

**Thermite** is the throwable welding charge.

Where **Honey Jar** is bees + nectar HoT, **Splash Canister** is water primer + thin regenerating chip-shell, **Caustic Flask** is corrosion + timed % DR plating, and **Voltaic Cell** is speed lightning + fat overshield dumps, Thermite is **fire as triage and wildfire**: a bland fire boom at stock that upgrades into instant flesh-mending blasts, a self-combustion heal engine, mobile ember recharge, cluster multiplication, and scorched ground — without HoT identity, blue bars, or “stand still in your crater.”

**One-liner:** *Throw thermite. Upgrades turn it into a welding triage boom, a self-combustion heal engine, or a cluster wildfire — instant HP mends, never a HoT or blue bar.*

**Element:** `EffectType.Fire`  
Load-bearing identity: ignite saturation, instant base-HP heal pulses, self-fire economy, cluster bomblets, and scorched earth. Not Shock, Acid, Bees, Water, overshield, or HoT-as-spine.

---

## 2. Role in the Arsenal

| Gear | Fantasy | Thermite relationship |
|------|---------|------------------------|
| **Vanilla Incendiary Grenade** | Legacy fire boom; IC + Cluster dominate; Hearth rewards camping | **Left in game.** Thermite is the intentional parallel kit; no patch requirement on vanilla. |
| **Honey Jar** | Bees + pure **HoT** nectar | Soft synergy only. Thermite = **instant** HP pulses, not nectar ticks. |
| **Splash Canister** | Water primer + **regen chip shell** | Splash = thin shell that rebuilds on idle. Thermite = event heals on throw/boom/combust. |
| **Caustic Flask** | Acid + puddles / vacuum / **timed % DR** | Flask = plating. Thermite = real HP mend. Different verb. |
| **Voltaic Cell** | Shock + mobility / **fat overshield** | Cell = blue capacitor. Thermite = green/white flesh heal. No OS API on this gear. |
| **Friend in a Box** | Deployable ally AI | Complementary. Thermite is boom/field fire, not a unit. |
| **MS-7 Caduceus** | Beam medic primary | Caduceus owns sustained triage beam. Thermite is throwable burst mend + fire DPS. |
| **Fire weapons / ignite kits** | Fire status payoffs | Soft synergy via Napalm / Wildfire / IC fuel. No code coupling. |

**Niche tags:** Fire · Instant Heal (base HP) · Self-Fire Economy · Cluster · Scorched Earth · Mobile Hearth

**Slot:** Throwable / grenade gear  
**Range:** Mid throw arc, impact AOE + optional clusters / burn field / ember  
**Role:** Primary-element fire nade that *opts into* welding triage, combustion self-heal engine, or pack wildfire

---

## 3. Design Pillars

1. **Bland baseline**  
   Stock throw matches other primary-element grenades: damage + Fire apply. No free heal, cluster, combustion, hearth, or scorched field.

2. **Flat Healing only — this kit’s sustain home**  
   Healing is **flat base-HP pulses** (instant, not HoT) on discrete events (throw, boom, IC proc, optional pyre pickups).  

   **Never** HoT-as-identity. **Never** overshield / negative-heal blue bar. **Never** Caustic % DR. **Never** Splash regen shell.

3. **Internal Combustion crowns the heal tree**  
   IC is not “the DPS exotic that also heals.” It is the **Combustion well exotic**: self-fire fuel → stack → nova that **heals you and ignites the room**. Damage on the proc is real; the *tree identity* is heal-engine.

4. **No stand-still hearth**  
   Vanilla Hearth & Home dies here. **Mobile Hearth** rewards kiting your own heat (ember + move gate).

5. **Three gravity wells + one field exotic**  
   Combustion · Hearth · Wildfire, plus **Scorched Earth** as peer exotic zone denial that still respects mobility (no camp-to-win).

6. **Cluster remains a celebrated DPS peak**  
   Not deleted, not nerfed out of existence — but Combustion and Hearth must be peer-complete, not support cast for Cluster.

7. **No binary death gambles**  
   **Gambler’s Bargain is cut.** No “recharge or die” coin flips. Risk is readable self-fire, not RNG execution.

8. **Shared staples stay shared**  
   Grenade Belt, In For A Penny / Pound line, Boundary Incursion, Multiversal Thievery, Edge Fault — **not** in custom ~30. Optional parity later.

9. **Names are free to change**  
   Fantasy and mechanical role matter more than vanilla strings. Iconic beats (Internal Combustion, Cluster Bomb, Welding Heat) may keep or rename for Thermite branding.

10. **Self-contained v1**  
    No hard deps on Honey, Splash, Caustic, Voltaic, Friend, Caduceus, or other mods. Cross-kit notes are player-facing only.

11. **~30 upgrades**  
    Sibling universal truths apply. **4 exotics**, equal large hex footprints.

---

## 4. Baseline (No Upgrades)

**Feel target:** same honesty as the shared throwable family — a clean elemental boom. Identity is on the grid.

**Shared family baseline (locked):** damage **100** · effect amount **10** · max charges **3** · recharge **45** · explosion radius (`hitForce`) **6**.

| Property | v0 target | Notes |
|----------|-----------|--------|
| Throw / arc / bounce | Incendiary-like | Clone throw feel; Fire element |
| Impact damage | **100** | Shared family boom; delivery + ignite |
| Element | `EffectType.Fire` | |
| Fire effect amount (`damageEffectAmount`) | **10** | Full-sat class dump from empty (shared family) |
| Explosion radius (`hitForce`) | **6** | Wide Bore scales |
| **Healing** | **None** | **Flat Healing** (Welding Heat / Restoration / IC) is upgrade-only |
| **Cluster** | **None** | Exotic unlock |
| **Combustion stacks** | **None** | Exotic unlock |
| **Hearth ember** | **None** | Exotic unlock |
| **Scorched field** | **None** | Exotic unlock |
| Overshield / HoT / % DR | **None** | Permanently out of identity |
| Max charges | **3** | Shared family baseline; Two's Company stacks on top |
| Recharge duration | **45** | Shared family baseline |
| Self-hit | Fire can apply at reduced amount | Fire Gel reduces further |
| Self damage | Low vs enemy impact | Home Brew / self-fire toys use intentionally |


### Baseline detonation sequence

```
Throw → bounce/fuse (vanilla grenade rules)
  → Primary boom (DamageData: standard damage, EffectType.Fire, high effect amount, AOE)
  → Done
```

### Why bland stock

- Primary-element grenades are **bland at baseline**; fantasy is upgrade-gated.  
- Free heal steals Welding Heat / IC.  
- Free cluster steals Cluster Bomb.  
- Free field steals Scorched Earth.  
- Vanilla’s problem was not “too simple stock” — it was **two-build gravity** (IC vs Cluster) plus a hearth that punishes movement.

---

## 5. System Truths

### 5.1 Fire element (from game)

- `FireStatusEffect`: DoT on full saturation (enemy ≈ 10, player ≈ 0.165 per tick — vanilla constants).  
- Saturation add: `amount * 0.1` per application → roughly **10 effect amount ≈ full ignite** from empty, before decay.  
- Default full-sat lifetime in line with standard elements (~3s class unless a linger system re-applies).  
- No invented parallel “burn” status — use real `EffectType.Fire`.

### 5.2 Language for upgrade text

| Term | Meaning |
|------|---------|
| **Ignited / on fire** | Target has Fire status with saturation > 0 |
| **Fully ignited** | `IsFullySaturated` on Fire |
| **Applying fire** | Damage/effect that adds Fire saturation (boom, cluster child, scorched tick, IC nova, melee cards) |
| **Combustion stacks** | IC resource on the local player from taking fire damage (0–100) |
| **Your ember / scorched field** | Zones spawned by **this player’s** Thermite systems only |
| **Instant heal** | Single flat HP restore to base health on an event — **not** a duration HoT |

### 5.3 Instant heal (upgrade-only) — locked

```
ThermiteHeal (base HP only):
  grant sources (when respective upgrades equipped):
    - Welding Heat: on primary boom (and inheriting child booms if flagged) → players in radius
    - Restoration Protocol: on throw → self
    - Internal Combustion proc: on 100-stack nova → self
    - Funeral Mote (optional rare under Scorched / Combustion): ally walks through scorched edge once → small pulse
  amount       = from upgrade rolls (rares mid; IC proc mid-high; Welding stackable)
  type         = positive heal into missing base health only
  overheal     = NONE — do not grant overhealth; clamp to max health
  stack policy = instant pulses; no lingering regen buff as identity
  display      = standard heal numbers / feedback (not blue overhealth bar)
  NOT included = HoT ticks, HealTarget(..., -amount), % DR plating,
                 Splash regenerating chip engine, Honey nectar, baseline grant
```

**Differentiation (player-facing):**

| Kit | Sustain verb |
|-----|----------------|
| Honey Jar | Pure **HoT** to base health |
| Splash Canister | Thin shell that **rebuilds after you stop taking chip** |
| Caustic Flask | Timed **% damage resistance** (no HP restore) |
| Voltaic Cell | **Fat capacitor dump** (blue overshield) |
| **Thermite** | **Flat Healing** — instant HP pulse when you throw / boom / combust |


### 5.4 Internal Combustion (upgrade-gated exotic) — heal-tree crown

Port vanilla IC fantasy; reframe as Combustion well identity:

```
While equipped:
  Taking fire damage (DoT or hit with Fire) grants Combustion stacks (efficiency from rolls).
  At 100 stacks:
    - Clear stacks
    - Create fiery explosion at player (size from rolls; Fire damage + Fire apply)
    - Instant heal self (healing from rolls) — PURE HP, no OS
  Stacks display mandatory.
```

**Design read:** IC is the **heal engine exotic**. The nova’s damage is real and satisfying; the tree’s gravity is “run hot, get paid in flesh.” Welding Heat + IC is an intended peak (boom mend + combust mend), not accidental double dip to delete — tune amounts under a soft per-window heal budget if needed.

### 5.5 Mobile Hearth (upgrade-gated exotic) — Hearth D locked

```
On detonate (after boom rules):
  Spawn Hearth Ember at impact (short–medium duration; radius from rolls).
  Optional: brief ember wake along final approach (stretch).

Recharge bonus while local player is:
  (a) inside YOUR ember radius, AND
  (b) moving (velocity above gate OR distance-traveled-in-zone ticks)

Stationary inside ember: weak or zero bonus (locked — no camp meta).
Cluster children / IC nova / Scorched Earth may refresh or multi-point embers
so “home” follows the fight (see exotic coexistence).

Charge bonus ballpark: vanilla Hearth was +100–165% — keep strong but gated by movement.
```

### 5.6 Cluster Bomb (upgrade-gated exotic)

```
On primary detonate:
  Spawn N child thermite bomblets (count/spread/arc from rolls).
  Children: smaller damage/radius/effect; still EffectType.Fire.
  Welding Heat inheritance: YES at reduced heal amount (tune) so Cluster+Welding is triage rain, not full N× medic god.
  Scorched Earth: children do NOT each spawn full exotic fields (one field layer rules — see §9).
```

### 5.7 Scorched Earth (upgrade-gated exotic) — fourth exotic

```
On detonate (after boom):
  Spawn Scorched Field at impact — lingering fire ground.
  Duration: medium (e.g. 6–12s v0), improved by Deep Charge / field duration standards.
  Ticks: Fire apply + light damage to enemies (presence/pressure > burst).
  Players: NO damage-free camp heal from standing in field.
  Optional rare: Funeral Mote — allies who MOVE through field edge get one small instant heal pulse per field (movement-gated).
  Mobile Hearth: standing/moving in scorched field may count as ember-equivalent for recharge IF Mobile Hearth equipped (single “heat home” pipeline — preferred merge).
  Visual: white-hot slag / thermite glitter on ground (placeholder OK).
```

**Why this fourth exotic:**  
Gives Wildfire a **zone** peer to Cluster’s **multiply**, gives Hearth more mobile heat real estate, and does not steal IC’s heal-crown or reintroduce stand-still Hearth.

### 5.8 Self-fire economy

| Card family | Fantasy |
|-------------|---------|
| **Heat Sink** | Gain grenade charge while ignited |
| **Give and Take** | While ignited: take more fire DoT, deal more damage |
| **Maniac Maneuver / Wildfire** | Self-grenade damage → Wildfire buff (normal damage → Fire) |
| **Fire Gel** | Less self Fire application from your thermite |
| **Home Brew** | More boom damage; more self damage from your boom |
| **Cheap Material** | Faster recharge; take more fire damage |
| **Hot Boxing** | Melee applies fire |
| **Impact Cascade** | Melee hit on ignited target → grenade charge |

Risk is **readable fire math**, not Gambler binary death.

### 5.9 Cooldown philosophy

| Allowed | Avoid |
|---------|--------|
| Heated Charge pure CDR | Hidden recharge bricks on radius/damage/heal cards |
| Two’s Company: +1 charge with **explicit mild +CD** on that card only | Stacking CD taxes until thermite feels unusable |
| Heat Sink / Mobile Hearth / Ember Relay economy | Camp-crater as the only recharge story |
| Scorched + Hearth movement recharge | Quiet global CD penalties on Welding / IC |

### 5.10 Explicit cuts / non-goals

| Beat | Fate |
|------|------|
| Gambler’s Bargain | **Deleted** — binary recharge-or-die |
| Grenade Belt | **Out of custom ~30** (shared staple later) |
| In For A Penny / Pound / Penny-Pound | **Out of custom ~30** (shared staple later) |
| Boundary / Edge / Multiversal grid steal | Out of custom ~30 |
| HoT identity | **Banned** |
| Overshield / negative heal | **Banned** on this gear |
| Vanilla stand-still Hearth | **Replaced** by Mobile Hearth (D) |
| Vanilla Incendiary gear | **Unmodified** |

### 5.11 Gambler replacement — Ember Relay (Epic)

```
On detonate: if local player is ignited, refund a partial grenade charge (fraction from rolls).
ICD optional if multi-charge + scorched ticks get weird.
Skill expression: stay hot on purpose — not coin-flip death.
```

---

## 6. Gravity Wells (Thematic Attractors)

Not exclusive trees. Taking one exotic or epic pulls related cards into value; every upgrade remains equippable with every other (except explicit requirement flags).

### Well A — **Combustion** (heal engine / self-fire)

*Run hot. Stack. Detonate. Instant heal. Ignite the room.*

Core pieces: Internal Combustion, Welding Heat, Restoration Protocol, Heat Sink, Give and Take, Maniac Maneuver, Fire Gel, Home Brew, Cheap Material, Hot Boxing, Impact Cascade, Ember Relay, Cauterize Jacket.

### Well B — **Hearth** (mobile recharge / kite the heat)

*Your fire is home — only if you keep moving.*

Core pieces: Mobile Hearth, Heated Charge, Two’s Company, Ember Stride, Warm Front, Deep Charge (ember/field duration), Quick Tongs (self-safety while kiting).

### Well C — **Wildfire** (pack fire / multiply / field)

*More fire. Bigger fire. Fire that stays and multiplies.*

Core pieces: Cluster Bomb, Scorched Earth, Volatile Explosives, Large Ignition, Violent Reaction, Napalm, Wide Bore, Hard Charge, Slag Mouth, Afterburn Fuse.

### Mix examples (expected, not edge cases)

- IC + Heat Sink + Fire Gel + Welding Heat → combustion medic peak  
- Welding Heat + Restoration + Mobile Hearth → throw-to-triage while kiting embers  
- Cluster + Volatile + Napalm + Large Ignition → pack delete  
- Scorched Earth + Mobile Hearth + Ember Stride → living heat map, grenade always coming back while moving  
- IC + Mobile Hearth + Scorched Earth → combust, plant field, kite your own forge  
- Cluster + Welding Heat (reduced child heal) → triage rain  
- Maniac Maneuver + Napalm + Give and Take → full fire convert diver  
- Ember Relay + Heat Sink + Cheap Material → hot recharge engine without Gambler  

---

## 7. Content Budget & Universal Truths

Aligned with Honey Jar / Splash Canister / Caustic Flask / Voltaic Cell / FriendinaBox:

| Rule | Value |
|------|--------|
| Total upgrades | **~30** (named kit + standards + fillers) |
| Exotics | **4** — Internal Combustion, Mobile Hearth, Cluster Bomb, Scorched Earth |
| Exotic hex footprint | **Equal and large** across all four |
| Epics | **~7** |
| Rares | **~10** |
| Standards | **~9** (stackable spine) |
| Path locks | **Slag Splitter** requires **Cluster Bomb** only; **Funeral Mote** stronger with Scorched Earth (soft) |
| Oddity / Contraband grid steal | Out of scope unless later parity pass |
| Shared vanilla staples (Belt, Penny line, Boundary, etc.) | **Not** in custom ~30 |
| v1 cross-mod | **None** |
| Vanilla Incendiary Grenade | **Unmodified** |

---

## 8. Full Upgrade Table

IDs are design placeholders (implementation range: **gear 92600**, **upgrades 92601–92630** — avoid template `920xx`, Friend `921xx`, Honey `922xx`, Splash `923xx`, Caustic `924xx`, Voltaic `925xx`).

Rarity key: **S** Standard · **R** Rare · **E** Epic · **X** Exotic  

Stack: **✓** CanStack · **—** unique  

Well: primary gravity well (still mixable)

### 8.1 Standards (~9) — stat spine

| # | Name | Well | Stack | Intent | Rough numbers (v0) |
|---|------|------|-------|--------|---------------------|
| 1 | **Wide Bore** | Wildfire | ✓ | Explosion (+ cluster / field / heal) radius | +15–30% radius · **no CD tax** |
| 2 | **White Phosphor** | Wildfire | ✓ | Fire effect amount on boom (field/cluster slightly) | +20–40% effect amount |
| 3 | **Heated Charge** | Hearth | ✓ | Faster recharge | −12–20% recharge · pure CDR |
| 4 | **Hard Charge** | Wildfire | ✓ | Impact / nova / child damage | +12–22% boom damage |
| 5 | **Fire Gel** | Combustion | ✓ | Less self Fire application from your thermite | −25–40% self effect amount (vanilla gel fantasy) |
| 6 | **Deep Charge** | Hearth | ✓ | Ember / scorched / linger durations | +20–35% heat-zone durations |
| 7 | **Two’s Company** | Hearth | ✓ | +1 max charge | +1 charge; **explicit mild +15–25% CD** on this card only |
| 8 | **Throw Weight** | Hearth | ✓ | Throw force up; less gravity feel | Mobility of the *throw* |
| 9 | **Quick Tongs** | Combustion | ✓ | Less self *damage* from your own boom (not element) | −20–35% self boom damage |

### 8.2 Rares (~10)

| # | Name | Well | Stack | Intent | Notes |
|---|------|------|-------|--------|-------|
| 10 | **Welding Heat** | Combustion | ✓ | Boom grants **instant heal** to players in radius | **Primary heal identity** — pure HP |
| 11 | **Restoration Protocol** | Combustion | ✓ | **Instant heal self** when you throw | Pure HP on throw |
| 12 | **Napalm** | Wildfire | ✓ | Outgoing damage matching this grenade’s element (**Fire**) increased | Vanilla fantasy |
| 13 | **Heat Sink** | Combustion | ✓ | Gain grenade charge while **you are ignited** | Vanilla fantasy |
| 14 | **Volatile Explosives** | Wildfire | ✓ | Bounce-explode behavior / extra bounce detonations | Vanilla fantasy kept |
| 15 | **Ember Stride** | Hearth | ✓ | Brief movespeed burst when your thermite detonates | Kite glue for Mobile Hearth |
| 16 | **Warm Front** | Hearth | ✓ | +ember/scorched radius; mild +recharge mult while moving in your heat | Hearth gravity |
| 17 | **Cauterize Jacket** | Combustion | ✓ | While ignited, next boom’s **Welding Heat** heal amount increased | IC + Welding glue; no HoT |
| 18 | **Slag Splitter** | Wildfire | ✓ | While **Cluster Bomb** equipped: +child count or spread | **Requires Cluster Bomb** |
| 19 | **Funeral Mote** | Wildfire | ✓ | Allies who **move through** your scorched field gain one small **instant heal** pulse per field | Movement-gated; weak without Scorched Earth |

### 8.3 Epics (~7)

| # | Name | Well | Stack | Intent | Notes |
|---|------|------|-------|--------|-------|
| 20 | **Maniac Maneuver** | Combustion | — | Damaging yourself with thermite grants **Wildfire**: normal damage you deal becomes Fire | Vanilla fantasy |
| 21 | **Give and Take** | Combustion | — | While ignited: take more fire DoT, deal more damage | Vanilla fantasy |
| 22 | **Ember Relay** | Combustion | — | Detonating while ignited refunds **partial charge** | **Gambler replacement** |
| 23 | **Violent Reaction** | Wildfire | — | Hitting a corroded target increases next explosion size | Vanilla fantasy |
| 24 | **Impact Cascade** | Combustion | — | Melee hit on ignited target generates grenade charge | Vanilla fantasy |
| 25 | **Hot Boxing** | Combustion | — | Your melee applies fire | Vanilla fantasy |
| 26 | **Afterburn Fuse** | Wildfire | — | Longer fuse after last bounce; boom gains bonus Fire amount / radius | Arming payoff; not global slow-nade tax alone |

### 8.4 Exotics (4)

| # | Name | Well | Stack | Intent |
|---|------|------|-------|--------|
| 27 | **Internal Combustion** | Combustion | — | Fire damage taken → stacks → at 100: fiery nova, ignite nearby, **instant self heal**. Heal-tree crown |
| 28 | **Mobile Hearth** | Hearth | — | Last detonation plants an ember; **recharge faster while moving inside your heat** — no camp |
| 29 | **Cluster Bomb** | Wildfire | — | Primary boom splits into child thermite bomblets |
| 30 | **Scorched Earth** | Wildfire | — | Detonation leaves a **lingering thermite field** that keeps burning and feeding mobile hearth logic |

*Count = 30 on the nose.*

---

## 9. Exotic Deep-Dives

### 9.1 Internal Combustion (Combustion — heal crown)

**Fantasy:** You are a pressure vessel. Take fire, cook off, mend, repeat.

**Behaviour sketch:**

- While equipped: fire damage taken adds Combustion stacks (efficiency rolls; wiki ballpark +250–320% efficiency language → tune to ~feel of vanilla 100-stack cadence).  
- At 100: nova at player — Fire AOE damage + Fire apply (size rolls); **instant heal self** (heal rolls); clear stacks.  
- Stack UI mandatory.  
- Does not grant overshield. Does not start a HoT.  
- Fire Gel / Quick Tongs make the lifestyle livable; Give and Take / Heat Sink make it greedy.

**Mix notes:**  
Welding Heat + IC = dual instant mend (boom + proc).  
Heat Sink + IC = charge while cooking.  
Mobile Hearth + IC = nova can refresh ember at your feet so you kite outward.  
Cluster + IC = both peaks legal; board space is the cost.

**Risk budget:**  
Proc heal strong but event-gated (need fire intake). Nova must not delete the user; self-damage from nova tuned carefully. Soft heal-per-10s budget if Welding+IC+Restoration becomes wipe insurance.

### 9.2 Mobile Hearth (Hearth)

**Fantasy:** Home is where the thermite was — and you only get mail if you’re still moving.

**Behaviour sketch:**

- On detonate: spawn **Hearth Ember** zone at impact.  
- Duration/radius from rolls; Deep Charge / Warm Front scale.  
- Recharge multiplier while player inside **and moving** (velocity gate).  
- Stationary: little/no bonus (locked).  
- Ember Stride helps you leave and re-enter on purpose.  
- With Scorched Earth: prefer **one heat-home pipeline** — scorched field counts as ember for recharge rules when both owned (no double-dip two full multipliers unless explicitly tuned lower).

**Mix notes:**  
Restoration + Hearth = throw heal, land, kite ring for CDR.  
Cluster children: optional micro-embers or only primary plants ember (prefer **primary + IC nova + scorched** as plant sources; children do not spam full hearths).

**Risk budget:**  
Must never beat “throw and AFK in crater.” Movement gate is load-bearing. Bonus can be strong (vanilla was huge) because skill is required.

### 9.3 Cluster Bomb (Wildfire)

**Fantasy:** One throw, many problems.

**Behaviour sketch:**

- On primary detonate: spawn child grenades (count/spread/speed from rolls).  
- Children detonate for reduced damage/radius/Fire amount.  
- Slag Splitter rare: +children / better spread while exotic equipped.  
- Welding Heat on children: **reduced** heal per child (e.g. 25–40% of primary pulse) with optional per-throw heal cap.  
- Scorched Earth: **one** field from primary only (children never each spawn full fields).

**Mix notes:**  
Celebrated DPS peak. Peer posters must still win without it (IC medic, Hearth kite, Scorched control).

**Risk budget:**  
Child heal cap. Child scorched ban. Performance cap on concurrent children.

### 9.4 Scorched Earth (Wildfire — fourth exotic)

**Fantasy:** The ground keeps working. You don’t stand in it like an idiot — you use it as a moving forge line.

**Behaviour sketch:**

- On detonate: spawn **Scorched Field** (duration/radius/tick from rolls).  
- Enemy ticks: Fire apply + light damage.  
- No ally HoT from standing in field.  
- Funeral Mote rare: movement-through pulse heal (instant, once per ally per field).  
- Mobile Hearth merge: field satisfies “your heat” for move-gated recharge when Hearth equipped.  
- Visual: lingering thermite / white slag pool.

**Mix notes:**  
Scorched + Hearth = map-wide mobile CDR without vanilla camp.  
Scorched + Cluster = primary field + child damage (no multi-fields).  
Scorched + IC = combust inside your own forge for stacks + ticks.  
Scorched + Welding = boom mend, field pressure — field does not replace Welding.

**Risk budget:**  
Duration caps; max concurrent fields (e.g. 2 with Two’s Company). No stand-still heal. Tick damage must not grief allies.

### 9.5 Exotic coexistence

| Pair | Rule |
|------|------|
| IC + Mobile Hearth | **Encouraged.** Nova + kite. |
| IC + Welding Heat | **Encouraged.** Heal-tree peak. |
| Cluster + Scorched Earth | Allowed. One field layer from primary. |
| Cluster + Welding Heat | Allowed. Reduced child heal + per-throw cap. |
| Mobile Hearth + Scorched Earth | **Encouraged.** Single heat-home pipeline. |
| All four | Allowed if grid fits; power via caps, move gates, child rules. |
| Footprints | All four exotics **same cell count**, larger than typical rares/epics. |
| Slag Splitter without Cluster | No-op / requires flag. |
| Funeral Mote without Scorched | Near-dead card (acceptable rare gravity). |

---

## 10. Named Kit — Detailed Specs

### Internal Combustion (Exotic)

- Stack gain on fire damage taken; 100 → nova + instant self heal + Fire AOE.  
- Unique. Heal-tree crown.  
- See §9.1.

### Mobile Hearth (Exotic)

- Ember plant on detonate; move-gated recharge in your heat.  
- Unique.  
- See §9.2.

### Cluster Bomb (Exotic)

- Child bomblets on primary detonate.  
- Unique.  
- See §9.3.

### Scorched Earth (Exotic)

- Lingering fire field on detonate.  
- Unique.  
- See §9.4.

### Welding Heat (Rare)

- On detonate: for each alive player in explosion radius, **instant Heal** amount R.  
- Stackable: amount and/or radius within sanity caps.  
- **Never** overhealth. **Never** HoT.  
- Children: reduced amount if Cluster equipped.

### Restoration Protocol (Rare)

- On throw (consume charge / fire throwable): instant heal self amount R.  
- Stackable amount.  
- Works even if boom misses — throw is the event.

### Napalm (Rare)

- Outgoing `EffectType.Fire` damage increased (boom, children, field ticks you own, IC nova, Wildfire-converted hits if easy).

### Heat Sink (Rare)

- While local player ignited: multiply recharge / add charge over time (vanilla feel).  
- Stack display while active.

### Volatile Explosives (Rare)

- Bounce detonation behavior per vanilla Volatile fantasy.  
- Tune so it doesn’t infinitely chain with Cluster (ICD / bounce budget).

### Ember Stride (Rare)

- On detonate: short movespeed buff to thrower.  
- Supports Mobile Hearth kiting.

### Warm Front (Rare)

- +heat zone radius; +move-gated recharge mult while in your heat.  
- Scales Hearth / Scorched.

### Cauterize Jacket (Rare)

- While ignited, multiply next Welding Heat pulse (consume flag on boom or short window).  
- Instant heal amp only — no regen buff.

### Slag Splitter (Rare)

- Gate: Cluster Bomb equipped.  
- +child count or spread; mild stack.

### Funeral Mote (Rare)

- On ally entering scorched field with movement: one instant heal pulse; mark ally until field dies.  
- No pulse while standing still inside from the start (must cross / move through).

### Maniac Maneuver (Epic)

- On self-damage from your thermite: grant Wildfire buff duration D.  
- While Wildfire: outgoing Normal damage converted to Fire.  
- Unique.

### Give and Take (Epic)

- While ignited: +outgoing damage; +incoming fire DoT mult (vanilla ranges as starting point).  
- Unique.

### Ember Relay (Epic)

- On detonate if ignited: `AddCharge` fraction.  
- Unique. **Replaces Gambler’s Bargain.**  
- No kill-you chance. Ever.

### Violent Reaction (Epic)

- If boom hits corroded target: buff next explosion radius.  
- Unique.

### Impact Cascade (Epic)

- On melee damage to ignited enemy: grenade charge.  
- Unique or mild stack — prefer unique with strong number.

### Hot Boxing (Epic)

- Melee applies Fire effect amount.  
- Unique.

### Afterburn Fuse (Epic)

- +fuse time after max bounces; +Fire amount and/or radius on that boom.  
- Unique.  
- Optional mild pull-in of “arming” fantasy without Acid vacuum clone.

### Standards

- Stat spines only; no silent heal/IC/cluster enables.

---

## 11. Synergy Notes (Player-Facing, Soft Only)

No mod dependencies. Loadout tips for README / codex blurb.

| Partner | Why it feels good |
|---------|-------------------|
| Fire weapons / ignite kits | Napalm, Heat Sink fuel, Violent Reaction packs |
| Caustic Flask | Corrode → Violent Reaction; DR + instant heal is strong but throw-gated |
| Voltaic Cell | Different sustain verbs; Excited Plasma-style cross-spice lives on Cell |
| Splash Canister | Boiling Point steam on their side; you supply ignite |
| Honey Jar | No shared heal identity; pure damage co-op |
| Melee / Hot Boxing employees | Impact Cascade + Hot Boxing loop |
| Caduceus | Beam sustained triage + your burst mend / ignite paint |
| Movement-heavy play | Mobile Hearth + Ember Stride + Scorched lines |

**Explicit non-goals v1:** patching vanilla Incendiary; Gambler; Belt/Penny in custom 30; HoT or overshield identity; stand-still hearth; cross-mod heal APIs.

---

## 12. Strengths, Weaknesses & Failure Modes

### Strengths

- Clear **instant-heal grenade** niche vs Honey HoT / Splash shell / Caustic DR / Voltaic OS  
- IC is a proper **heal-tree crown**, not only a DPS poster  
- Mobile Hearth fixes vanilla’s suicide camp  
- Cluster remains a celebrated peak with peer-complete alternatives  
- Scorched Earth adds zone identity without fourth-well confusion  
- Self-fire economy stays spicy and readable  
- No Gambler binary toxicity  

### Weaknesses

- Stock throw is intentionally bland  
- Heal / hearth / cluster / field are dead stats until unlock cards  
- Instant heal is proactive (throw/boom/combust first) — panic HoT is Honey’s job  
- Self-fire builds punish sloppy Fire Gel / positioning  
- Cluster+Welding needs caps or becomes co-op medic storm  

### Failure modes to avoid in tuning

| Failure | Mitigation |
|---------|------------|
| Secret HoT creep | Every heal is event pulse; review descriptions |
| Blue bar creep | Ban negative-heal API on this gear |
| Camp hearth returns | Move gate mandatory; stationary mult ≈ 0 |
| IC only build wins | Peer posters: Hearth kite, Cluster DPS, Scorched control, Welding triage without IC |
| Cluster only build wins | Same |
| Welding+Cluster infinite team heal | Reduced child heal + per-throw cap |
| Scorched multi-field lag | Max concurrent fields; children don’t spawn fields |
| Gambler-like RNG kill | Ember Relay has no death chance |
| CD tax culture | Review every property for accidental rechargeDuration writes |
| 30 upgrades / 1 forced build | Standards universal; minimal path locks |
| Funeral Mote stand-AFK heal | Movement-through requirement |
| IC nova suicide | Self-damage tune; Quick Tongs / Fire Gel matter |

---

## 13. Implementation Appendix (For Later — Not This Pass)

Design-only milestone: **this document**. When coding starts, prefer:

| Piece | Approach |
|-------|----------|
| Registration | Existing grenade template `GrenadeRegistration` clone path; Fire element on `GunData` |
| Name / IDs | Display **Thermite**; `APIName` `thermite`; gear id **92600**; upgrades **92601–92630** |
| Data host | `ThermiteBehaviour` (rename from example) with `Data` struct for flags/scalars |
| Detonate | Harmony on `GrenadeBullet.Detonate` (template / FriendinaBox style) |
| Instant heal | Positive heal APIs into base health only; clamp max HP; **no** `HealTarget(..., -amount)` |
| Internal Combustion | Port vanilla stack-on-fire-damage + nova; heal pulse on proc |
| Mobile Hearth | Zone at impact + velocity-gated recharge mult; track last ember(s) |
| Cluster | Spawn child grenade bullets / scaled detonations |
| Scorched Earth | Linger zone + Fire ticks; merge heat-home with Hearth when both owned |
| Wildfire buff | Port Maniac Maneuver normal→fire convert |
| Upgrades | `PlayerData.CreateUpgrade` + `UpgradeProperty` Apply/Remove restoring prefab snapshot |
| Mod flags | `[MycoMod(..., ModFlags.IsSandbox)]` |
| Vanilla Incendiary | **Do not patch** |
| Cross-mod | None in v1 |

### Suggested `ThermiteBehaviour.Data` fields (sketch)

```
// Baseline / scales
float explosionRadiusMultiplier;
float fireEffectAmountMultiplier;
float boomDamageMultiplier;
float selfFireMultiplier;
float selfBoomDamageMultiplier;

// Instant heal
float weldingHealAmount;
float weldingHealRadiusMult;
float weldingChildHealMult;      // < 1
float weldingHealThrowCap;       // per throw budget
float restorationHealAmount;
float cauterizeJacketHealMult;   // while ignited window
float funeralMoteHealAmount;

// Internal Combustion
bool internalCombustion;
float combustionEfficiency;
float combustionNovaSize;
float combustionNovaDamageMult;
float combustionHealAmount;
int combustionStacks;            // runtime, not upgrade static

// Mobile Hearth
bool mobileHearth;
float hearthDuration;
float hearthRadius;
float hearthRechargeMultMoving;
float hearthRechargeMultStationary; // ~0–0.15
float hearthMoveSpeedGate;
float emberStrideSpeed;
float emberStrideDuration;
float warmFrontRadiusMult;
float warmFrontRechargeMult;

// Cluster
bool clusterBomb;
int clusterChildCount;
float clusterSpread;
float clusterDamageMult;
float clusterRadiusMult;
float clusterFireMult;
float slagSplitterChildBonus;

// Scorched Earth
bool scorchedEarth;
float scorchedDuration;
float scorchedRadius;
float scorchedTickInterval;
float scorchedTickDamage;
float scorchedTickFire;
int maxScorchedFields;

// Self-fire economy
float heatSinkCharge;
float giveAndTakeOutgoing;
float giveAndTakeIncomingFire;
bool maniacManeuver;
float wildfireDuration;
float emberRelayChargeFraction;
float impactCascadeCharge;
float hotBoxingFireAmount;
bool volatileExplosives;
float violentReactionNextRadiusMult;
float afterburnFuseBonus;
float afterburnBoomFireMult;

// Napalm
float fireOutgoingDamageMult;
```

### Ship cut vs stretch

**v1 must-ship (fantasy complete):**

- Baseline bland fire boom  
- All **4** exotics  
- Welding Heat, Restoration Protocol, Heat Sink, Napalm  
- Maniac Maneuver, Give and Take, Ember Relay, Impact Cascade, Hot Boxing  
- Full standard spine  
- Instant-heal model + move-gated Hearth + no Gambler  
- Cluster child heal caps + single scorched layer  

**Stretch / post-v1:**

- Funeral Mote juiciness  
- Ember wake trail VFX  
- Shared staple parity (Belt, Penny line)  
- Custom mesh / Wwise  
- Config toggles for heal caps / hearth gate  

---

## 14. Naming & Presentation

| Slot | Value |
|------|--------|
| Display name | **Thermite** |
| Internal / API | `thermite` |
| Design nickname | Incendiary Grenade Rework (notes / folder only) |
| Short description | *Fire-element grenade. Stock throw is a clean thermite boom. Upgrades unlock instant welding heals, self-combustion triage, mobile ember recharge, cluster bomblets, and scorched earth — pure HP pulses, no HoT, no blue bar.* |
| Thunderstore name (later) | `Thermite` |
| GUID (later) | `sparroh.thermite` |
| Folder today | `.new.IncendiaryGrenadeRework` (rename optional at ship) |

### Name map (vanilla → Thermite)

| Vanilla-ish beat | Thermite name | Notes |
|------------------|---------------|--------|
| Incendiary Grenade (gear) | **Thermite** | Ship name |
| Internal Combustion | **Internal Combustion** | Keep — perfect heal-crown name (or **Autogenous Combustion**) |
| Hearth & Home | **Mobile Hearth** | Rewrite required |
| Cluster Bomb | **Cluster Bomb** | Keep or **Slag Cluster** |
| *(new)* | **Scorched Earth** | Fourth exotic |
| Welding Heat | **Welding Heat** | Keep — heal spine |
| Restoration Protocol | **Restoration Protocol** | Keep or **Field Dressing** |
| Gambler’s Bargain | **Ember Relay** | Replacement, not rename |
| Heat Sink | **Heat Sink** | Keep |
| Give and Take | **Give and Take** | Keep |
| Maniac Maneuver | **Maniac Maneuver** | Keep or **Wildfire Protocol** |
| Fire Gel | **Fire Gel** | Keep |
| Napalm | **Napalm** | Keep |
| Large Ignition / Engorge-like | **Wide Bore** | Spine rename |
| Heated Charge | **Heated Charge** | Keep |
| Two’s Company | **Two’s Company** | Keep |
| Others | As table | Free to rename in polish |

### SAXON marketing blurb (draft)

> SAXON Thermite — Industrial welding charges for employees who treat trauma with exotherms.  
> Baseline: fire. Aftermarket: instant flesh welds, cook-off combustion triage, hearths that only answer runners, cluster slag, and ground that stays angry.  
> Not a juice box. Not a blue capacitor. Not a camping stove.  
> “If you’re still standing still in the ember, the thermite is not the problem.”

---

## 15. Open Questions (Balance / Feel — Not Blocking Doc)

1. IC stack cadence: strict vanilla parity vs slightly slower with stronger heal pulse.  
2. Welding child heal fraction and per-throw cap numbers.  
3. Mobile Hearth + Scorched: single merged mult vs two diminished mults.  
4. Cluster child count default.  
5. Funeral Mote: keep in v1 or stretch? (Default: **keep** as Scorched gravity rare.)  
6. Exact hex shapes for 30 upgrades — author during implementation.  
7. Max concurrent scorched fields with Two’s Company.  
8. Whether IC nova plants Mobile Hearth ember (default: **yes**).  
9. Ship strings: keep “Internal Combustion” / “Cluster Bomb” vs full SAXON pass.

---

## 16. Design Checklist

- [x] Separate gear (not in-place vanilla patch)  
- [x] Name: **Thermite**  
- [x] Bland baseline (no heal, cluster, IC, hearth, field)  
- [x] Heal = **instant base HP only** (no HoT identity)  
- [x] No overshield / blue bar  
- [x] Internal Combustion = **heal-tree crown**  
- [x] Hearth rewrite **D** (ember + move gate)  
- [x] Gambler’s Bargain **cut** → Ember Relay  
- [x] Belt / Penny line **out** of custom ~30  
- [x] Exotics (4): IC, Mobile Hearth, Cluster Bomb, Scorched Earth  
- [x] Cluster remains celebrated DPS peak; peers complete  
- [x] Names free to change; key fantasies retained  
- [x] ~30 upgrades, 4 equal large exotics  
- [x] Gravity wells mix/match  
- [x] Self-contained v1  
- [x] Implementation deferred  

---

## 17. Changelog (Design Doc)

| Date | Change |
|------|--------|
| 2026-06-08 | Initial design doc from vanilla Incendiary wiki upgrades, decompile anchors (`GrenadeGear`, `GrenadeBullet`, `FireStatusEffect`), sibling docs (Caustic Flask, Voltaic Cell, Honey Jar, Splash Canister), and user locks: ship name Thermite; separate gear; no baseline heal; instant HP only (no HoT/OS); IC as heal-tree crown; Hearth D (ember + move gate); Gambler cut; Belt/Penny out of custom 30; free names; **4th exotic Scorched Earth**; full ~30 table. |

---

*End of design document. Next step when ready: rename template identifiers to Thermite and implement baseline fire boom only, then layer Welding Heat → Internal Combustion → Mobile Hearth → Cluster / Scorched Earth.*
