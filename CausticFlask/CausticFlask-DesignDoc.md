# Caustic Flask — Design Document

> Status: **Design only** — no implementation yet.  
> Working title in notes: Acid Grenade Rework. **Ship name: Caustic Flask.**  
> Template base: `.new.AcidGrenadeRework` grenade content project.  
> Product shape: **separate gear** — vanilla Acid Grenade is left unmodified.

---

## 1. High Concept / Fantasy

**Caustic Flask** is the throwable solvent lab.

Where **Incendiary** is a fire boom economy, **Shock** is mobility lightning, **Honey Jar** is bees + nectar HoT, and **Splash Canister** is water catalyst + regenerating chip-shell, Caustic Flask is **corrosion with intent**: a bland acid boom at stock that upgrades into melting floors, vacuum clumps, timed armor plating, and the occasional heavy drop — without the vanilla Acid kit’s unthemed circus or cooldown-tax culture.

**One-liner:** *Throw solvent. Upgrades turn it into a melting floor, a gravity well, or a coat of timed armor — without taxing your cooldown for existing.*

**Element:** `EffectType.Acid`  
Load-bearing identity: corrosion saturation, acid puddles, rot-on-metal dual apply, and solvent damage amp. Not Fire, Shock, Bees, or Water.

---

## 2. Role in the Arsenal

| Gear | Fantasy | Caustic Flask relationship |
|------|---------|----------------------------|
| **Vanilla Acid Grenade** | Legacy mishmash (puddles, pull, −50 overshield, teleport, F-ability, heavy, OC) | **Left in game.** Flask is the intentional parallel kit; no patch requirement on vanilla. |
| **Incendiary Grenade** | Fire boom + self-fire economy / Welding Heat | Flask is linger corrosion + timed DR, not heal/wildfire. |
| **Shock Grenade** | Shock boom + mobility / Live Wire | Lightspeed Material Transfer **moves here** (other project). Flask is zone/control. |
| **Future Cryo Grenade** | Cold control (planned) | Connected Systems **moves there**. No employee F-ability circus on Flask. |
| **Honey Jar** | Bees + pure HoT nectar | Soft synergy only. No nectar, no overhealth. |
| **Splash Canister** | Water primer + regenerating shield HP | Splash = small shield pool; Flask = **% damage resistance windows**. Different verb. |
| **Friend in a Box** | Deployable ally AI | Complementary. Flask zones are fields, not units. |
| **Photon Disc** | Tumble disc + attunement | Wholly different projectile fantasy. |

**Niche tags:** Corrosion · Zone Control · Timed Armor (DR) · Pull/Setup · Heavy Drop

**Slot:** Throwable / grenade gear  
**Range:** Mid throw arc, impact AOE + optional puddle / vacuum fuse  
**Role:** Primary-element status nade that *opts into* field denial, clump setup, or proactive DR

---

## 3. Design Pillars

### 3.0 Core triad (product fantasy)

These three are the **load-bearing identity**. Gravity wells, upgrade lists, and exotic politics all serve them. If a card does not touch at least one — and preferably stitches two — it is filler or off-pillar logistics.

| # | Pillar | Player verb | Must not become |
|---|--------|-------------|-----------------|
| **T1** | **Acid build-up is the central system** | Apply, saturate, re-apply, key payoffs off *corroded / fully corroded* | Instant full-sat wallpaper; element tag with no gradient |
| **T2** | **Armor (DR) is the healing type** | Proactive timed damage resistance — throw *before* the spike; extend mid-fight via acid/field loops | Overshield, HoT, blue bar, panic heal clone of Honey/Splash |
| **T3** | **Battlefield control utility** | Own space and enemy position (puddle denial, vacuum clump, collapse setup) | Pure damage stick; CC with no acid/armor sentence |

**Triad test (playtest signal):** players should describe throws as *“pull them into my solvent and walk in plated”* — not only “the armor nade” or only “the pull nade.”

**Cohesion rule (locked):** wells may mix freely, but **bridges are load-bearing, not garnish.** Parallel monobuilds are a failure mode if cross-pillar cards are weak or stretch-only.

### 3.1 Supporting laws

1. **Bland baseline**  
   Stock throw matches the other primary-element grenades: shared family stats (damage **100**, effect amount **10**, charges **3**, recharge **45**, `hitForce` **6**) + Acid apply. No free puddle, pull, armor, heal, or gimmick.  
   Baseline acid apply is **full-sat class (10)** on a clean hit; **holding** full sat and multi-target coverage are payoffs for puddle dwell, collapse, Seal, or multi-throw (see §5.1).

2. **Armor is opt-in**  
   Timed damage resistance lives on upgrades only. Never baseline. Never overhealth. Never HoT.  
   Armor must **care about acid** (see §5.5 Solvent Cure) so plating is precipitated solvent, not a generic DR buff on a random nade.

3. **No cooldown-tax culture**  
   Vanilla Acid feels “long CD” largely because **upgrades stack recharge penalties** (Overcharge-style size-for-CD, Heavy-as-stealth-tax, etc.).  
   Caustic Flask rule: upgrades must **not** casually lengthen recharge. Power budgets use radius, duration, self-risk, explicit multi-charge trade, or opportunity cost.  
   **Heavy Support** is the one loud expensive fantasy — prefer **reduced boom damage** over CD tax so the kit’s culture stays clean.

4. **Three gravity wells + one off-pillar logistics exotic**  
   Solvent Field · Vacuum Lab · Carapace are the triad wells.  
   **Heavy Support** stays as peer *hex* exotic for nostalgia/logistics, but is **off-triad** in design reviews — do not let cargo tuning steal oxygen from T1–T3.

5. **Deteriorate is dual-status**  
   Metallic targets get **Rot in addition to Acid** — never replace Acid with Rot.

6. **Readable exotics**  
   Serious reservoir · vacuum collapse · strong timed carapace · heavy cargo. Equal large hex footprints.

7. **Polymer is dead; plating lives**  
   No `HealTarget(..., -50f)` overshield dump. Former Polymer fantasy → **Polymer Plating** (timed DR).

8. **Self-contained v1**  
   No hard deps on Honey Jar, Splash, Friend, Shock rework, or Cryo. Cross-kit moves (Lightspeed → Shock, Connected → Cryo) are **other projects’** notes only.

9. **~30 upgrades**  
   Honey Jar / Splash / DMLR universal truths apply.

10. **Puddle is the triad hinge**  
    When field is online, the puddle/reservoir is where acid, control, and armor meet (Valves, Harden, Spurt density, enemy pressure). Vacuum’s job is force bodies into the hinge; Carapace’s job is live on it; Field’s job is be it.

---


## 4. Baseline (No Upgrades)

**Feel target:** same honesty as the shared throwable family — a clean elemental boom. Identity is on the grid.

**Shared family baseline (locked):** damage **100** · effect amount **10** · max charges **3** · recharge **45** · explosion radius (`hitForce`) **6**.

| Property | v0 target | Notes |
|----------|-----------|--------|
| Throw / arc / bounce | Incendiary-like | Clone throw feel; Acid element |
| Impact damage | **100** | Shared family boom; delivery + corrosion |
| Element | `EffectType.Acid` | |
| Acid effect amount (`damageEffectAmount`) | **10** | Full-sat class dump from empty (see §5.1); build-up still real via decay, puddles, multi-throw |
| Explosion radius (`hitForce`) | **6** | Wide Mouth scales |
| **Puddle** | **None** | Gas Puddle / Reservoir unlock field |
| **Pull** | **None** | Vacuum Tube / Event Horizon unlock |
| **Armor** | **None** | Polymer Plating / Carapace unlock — **Damage Resistance** is upgrade-only |
| Healing / overshield | **None** | Permanently out of identity (sustain = timed DR, not HP/OS) |
| Max charges | **3** | Shared family baseline; Twin Flask stacks on top |
| Recharge duration | **45** | Shared family baseline; no inherited upgrade CD debuffs |
| Self-hit | Acid can apply at reduced amount | Base Lining reduces further |
| Self damage | Low vs enemy impact | |


### Baseline detonation sequence

```
Throw → bounce/fuse (vanilla grenade rules)
  → Primary boom (DamageData: standard damage, EffectType.Acid, high effect amount, AOE)
  → Done
```

### Why bland stock (not free puddle / free armor)

- Primary-element grenades are meant to be **bland at baseline**; fantasy is upgrade-gated.  
- Free puddle steals Gas Puddle / Catalytic Reservoir.  
- Free armor steals Carapace and collides with “proactive upgrade identity.”  
- Vanilla’s problem was not “too simple stock” — it was **unthemed upgrades + CD taxes**.

---

## 5. System Truths

### 5.1 Acid element (from game) + build-up curve (locked intent)

- `AcidStatusEffect`: DoT on full saturation (enemy ≈ 10, player ≈ 0.165 per tick — vanilla constants).  
- Saturation add: `amount * 0.1` per application → roughly **10 effect amount ≈ full corrosion** from empty, before decay.  
- Default full-sat lifetime in line with other standard elements (~3s class unless puddle re-applies).

**Build-up is a system, not flavor text (T1):**

Baseline boom uses the **shared family effect amount 10** (full-sat class from empty on a clean direct hit). T1 still has a gradient — it is **not** “wallpaper forever full green”:

| Source | Saturation intent |
|--------|-------------------|
| Baseline boom | **10** effect amount — full-sat class dump from empty on direct hit (shared family lock) |
| Strong Solvent / amp cards | Over-apply / re-sat faster after decay; stronger puddle feed |
| Puddle / Reservoir ticks | Re-apply and **hold** sat on dwell after decay |
| Event Horizon collapse | Bonus acid spike on enemies still in well (prefer already corroded) |
| Catalytic Seal | Faster tick finish / re-sat |
| Multi-throw / Twin Flask / 3-charge cadence | Second hit re-saturates what decayed; pack coverage |

**Fully corroded** remains the payoff breakpoint for collapse bonus tier, Siphon clarity, and readable “melted” moments. Failure mode is not “stock amount too high” — it is **infinite free full-sat with zero field/re-apply investment** (no puddle, no multi-throw, no collapse). Decay + zone re-apply keep the gradient honest.

### 5.2 “Corroded” language for upgrade text

| Term | Meaning |
|------|---------|
| **Corroded** | Target has Acid status with saturation > 0 |
| **Fully corroded** | `IsFullySaturated` on Acid |
| **Applying acid** | Damage/effect that adds Acid saturation (boom, puddle tick, reservoir, spurt) |
| **Your puddle / lake** | Zones spawned by **this player’s** Caustic Flask systems only |

### 5.3 Acid puddle (upgrade-gated)

```
AcidPuddle (when Gas Puddle / Reservoir active):
  pattern     = GameManager.SpawnAcidPuddle_Rpc / AcidPuddle BurstEffect
  tick        = ~0.2s; applies Acid effect amount (DamageFlags.AOE | AcidPuddle)
  detect      = NonPlayer for damage/apply; players may be present for Valves sensing
  duration    = from upgrade rolls; Deep Vat scales
  radius      = tied to explosion force/size; Wide Mouth scales
```

**Gas Valves:** recharge multiplier while recently affected by **your** puddle’s AcidPuddle flag window (vanilla `lastAcidPuddleDamageTime` pattern — even if player takes 0 damage, presence-in-puddle sensing may need a clean “standing in own puddle” check at impl; fantasy = stand in your solvent to cool the flask).

### 5.4 Deteriorate — dual status (locked rewrite)

Vanilla Acid path on metal:

```csharp
// VANILLA (bad): replaces acid with rot
if (ApplyRot && target.IsMetal())
    damage.effect = EffectType.Rot;
```

**Caustic Flask rule:**

```
On damaging a metallic target with flask systems:
  1. Apply Acid as normal (unchanged effect on primary hit)
  2. ALSO apply Rot (second application / extra effect packet)
Never: swap Acid → Rot only
```

Rot remains the metal-decay fantasy; Acid remains the kit’s spine. Both must read on the target when possible.

### 5.5 Timed Armor (upgrade-only) — precise rules

```
ArmorPlating buff (player targets):
  damageTakenMult = 1 - DR           // e.g. 12–25% per card; hard CAP from this gear ~40–45%
  duration        = D                // e.g. 2.5–5.0s typical; refresh extends remaining time, cap Dmax
  stack policy    = single instance per target (refresh duration; optional mild DR soft-stack within hard cap)
  decay           = time only        // NOT hit-charge plates (locked)
  on damage       = full remaining DR applies to incoming damage.damage via OnBeforeTakeDamage
  display         = stack icon + remaining time (mandatory)
  type            = temporary damage resistance
  NOT included    = blue overhealth, HoT, permanent resist, baseline grant, Polymer -50 heal
```

**Sources (v1):**

| Upgrade | Who | When |
|---------|-----|------|
| Polymer Plating | Players in boom radius | On detonate |
| Saxonite Carapace | Players in boom radius | On detonate (strong) |
| Plate Polish | — | Improves duration / DR of Armor you apply |
| Puddle Harden | Self | Mild Armor refresh while in **your** puddle **if** Armor already active |
| Solvent Cure (rule) | Players receiving Armor from flask boom | On detonate — scales with acid just applied (below) |
| Corrosion Pulse (rule) | Self (optional tiny) | On fully corroding or killing a corroded target near you — small duration pulse |

#### Solvent Cure (locked armor↔acid bridge)

Armor is **precipitated solvent**, not a generic DR stamp.

```
On flask detonate, when applying ArmorPlating to a player:
  base DR + duration from Polymer / Carapace / Polish
  PLUS mild bonus duration (and/or tiny DR within hard cap) scaled by
    how much acid this detonate applied in radius
    (e.g. count of enemies that became corroded or fully corroded, or total acid effect amount dealt)
  Floor: armor still applies with zero enemies (empty throw is weak plate, not zero plate)
  Ceiling: hard DR cap unchanged; duration soft-cap still applies
```

Fantasy read: more solvent in the air → thicker film. Empty hallway throws remain valid pre-plate but peak Carapace wants corrosion in the room.

#### Mid-fight armor loop (T2 must not be pre-cast only)

Proactive DR loses the “heal role” war if it never pays during the spike. Required loops:

| Loop | Card / rule | Role |
|------|-------------|------|
| Field hinge | **Puddle Harden** (v1 must-ship) | Refresh while wading armored |
| Armor → acid | **Defensive Spurt** | Vent acid when hit while plated |
| Acid → armor | **Corrosion Pulse** (baseline rule when any Armor source equipped) | Tiny duration on nearby full-corrode / corroded kill — ICD’d |
| Economy | Valves / Siphon | Stay in the mess; throw again before plate expires |

**Impl anchor:** Incendiary-style `player.OnBeforeTakeDamage` → `damage.damage *= mult`.  
**Do not use:** `IDamageSource.HealTarget(source, player, -50f, pos)` (vanilla Polymer overshield).

### 5.6 Cooldown philosophy (locked)

| Allowed | Banned as default pattern |
|---------|---------------------------|
| Quick Cap pure CDR | “Bigger boom but longer CD” (Overcharge culture) |
| Twin Flask: +1 charge with **explicit mild +CD** on that card only | Hidden recharge penalties on radius/damage/utility cards |
| Solvent Siphon / Valves / Overclock charge refunds | Stacking multiple CD taxes until flask feels unusable |
| Heavy Support: **reduced boom damage** (preferred cost) | Heavy Support as silent global CD brick |

**Design read of vanilla pain:** baseline recharge was acceptable; **upgrades made the flask worse to throw**. Caustic Flask inverts that.

### 5.7 Vacuum pull (upgrade-gated)

Port `AcidGrenadeBullet` fuse-active pattern:

- On fuse start after max bounces: vortex VFX + pull pulses (~0.2s interval).  
- Server applies impulse toward epicenter (`IPullable` / enemy brain dedupe).  
- Players pulled at heavily reduced force if at all (tune; prefer enemy-focused).  
- Event Horizon extends arm time and adds **collapse** payoff on detonate.

**Control spends/banks acid (T3↔T1):**

- **Clump Tax:** bonus detonation/collapse damage **prefers corroded** targets (full bonus if Acid sat > 0; reduced or zero on clean targets). Vacuum without corrosion investment is weaker CC, not a second damage kit.  
- **Collapse (Event Horizon):** primary fantasy = saturation spike and/or bonus damage tier on **corroded / fully corroded** enemies still in the well — not only generic boom amp.  

### 5.8 Heavy Support (stays)

Port vanilla Acid cargo path:

- On detonate flag → `CallHeavyWeapon_ServerRpc` → `CargoPodCore.CallCargoDrop_Server` → heavy weapon crate for owner.  
- **Cost (locked preference):** reduced primary explosion damage while equipped — cargo *is* the power.  
- Optional alt if playtest demands: explicit +recharge **only on this card** (document in UI). Prefer damage cost first.  
- **Heavy Payload** rare: requires Heavy Support; faster drop / better feel.

### 5.9 Overclock (stays)

Port vanilla Acid Overclock fantasy:

- On before take damage: if damage has `DamageFlags.Overclocked`, gain grenade charge (falloff by distance to source).  
- If OC damage would kill and ICD ready: clamp to leave player barely alive (vanilla ~1 HP / `health - 0.25` style).  
- Stack display + cooldown on death protection.  
- **No CD tax** on the card itself.

### 5.10 Explicit moves off this kit

| Vanilla Acid card | Fate |
|-------------------|------|
| Lightspeed Material Transfer | **Shock Grenade** (other project) |
| Connected Systems | **Future Cryo grenade** |
| Polymer Coating overshield | **Deleted** → Polymer Plating (Armor) |
| In For A Penny line / Grenade Belt / Multiversal Thievery / Edge Fault / Boundary Incursion | Not in v1 custom ~30 (optional parity later) |

---

## 6. Gravity Wells (Thematic Attractors)

Not exclusive trees. Taking one exotic or epic pulls related cards into value; every upgrade remains equippable with every other (except explicit requirement flags).

**Wells serve the triad** (§3.0). A well that never needs another pillar is a monobuild risk — bridges below are load-bearing.

### Well A — **Solvent Field** (T1 hinge · corrosion / puddles)

*Seed acid. Stand in your mess. Everything melts.*

Core pieces: Gas Puddle, Catalytic Reservoir, Gas Valves, Universal Solvent, Deteriorate, Catalytic Seal, Solvent Siphon, Strong Solvent, Deep Vat, Wide Mouth.

Cross-element spice (keep lean): **Exothermic Reaction** v1; **Odd Cocktail** stretch if board feels off-acid.

**Hinge role:** when online, puddle/reservoir is where T1/T2/T3 meet (Valves, Harden, Spurt density, enemy pressure, vacuum dump zone).

### Well B — **Vacuum Lab** (T3 · control / setup)

*Pull, clump, collapse — finish the acid sentence.*

Core pieces: Vacuum Tube, Event Horizon, Clump Tax, Viscous Mix, Throw Weight.

**Greased Joints:** low-priority filler unless tied to plated/corroded state (“slick plating” move refresh) — do not treat as pillar support.

**T1 stitch:** Clump Tax + collapse prefer **corroded** targets (§5.7).

### Well C — **Carapace** (T2 · timed Armor / proactive defense)

*Spray plating. Walk into the bad idea.*

Core pieces: Saxonite Carapace, Polymer Plating, Plate Polish, Defensive Spurt, Puddle Harden, Base Lining, Quick Cap, Twin Flask.

**Overclock:** retained vanilla DNA (survival/economy); weak acid story — do not pretend it reinforces T1.

**T1 stitch:** Solvent Cure + Corrosion Pulse (§5.5). **Field stitch:** Puddle Harden + Spurt on the hinge.

### Logistics (off-triad peer exotic)

**Heavy Support** + **Heavy Payload** — cargo drop fantasy retained from vanilla Acid. Equal *hex* weight to other exotics; **off-triad** in design reviews. Not healing, not mobility, not acid build-up. Do not let Payload/boom-cost tuning steal oxygen from T1–T3.

### Load-bearing bridges (not garnish)

| Bridge | Pillars | Card / rule | v1? |
|--------|---------|-------------|-----|
| Puddle Harden | Field + Carapace | Armor refresh in own puddle | **Must-ship** |
| Defensive Spurt | Carapace → Field | Acid vent while plated | **Must-ship** |
| Clump Tax (corroded) | Vacuum + Field | Bonus prefers Acid sat > 0 | **Must-ship** |
| Solvent Cure | Field + Carapace | Armor scales with acid applied | **Rule** (with any Armor source) |
| Corrosion Pulse | Field + Carapace | Tiny armor duration on nearby full-corrode / corroded kill | **Rule** (with any Armor source) |
| Collapse payoff | Vacuum + Field | Horizon acid/damage tier on corroded | **Must-ship** (on exotic) |
| Pressure Weld | Vacuum + Carapace | Optional rare: short ally armor tick ∝ enemies pulled | Stretch / replace filler |
| Gas Valves / Siphon | Field economy | Stay on hinge; throw cadence | Must-ship |

### Mix examples (expected, not edge cases)

- Reservoir + Valves + Harden + Carapace → armored wading, flask always coming back  
- Event Horizon + Clump Tax + Strong Solvent → vacuum pack, melt the *corroded* ball  
- Carapace + Polymer + Spurt + Solvent Cure → plated dive; thicker film when the room is wet with acid  
- Horizon + Reservoir → pull into vat (peak triad theater)  
- Deteriorate + Universal Solvent + Gas Puddle → metal strip + field re-apply  
- Overclock + Carapace → survive OC blasts, convert into charge, stay plated  
- Heavy Support + Hard Flask + Siphon → weaker boom, crate payoff (off-triad logistics lane)

---


## 7. Content Budget & Universal Truths

Aligned with Honey Jar / Splash Canister / FriendinaBox / vanilla grenade expectations:

| Rule | Value |
|------|--------|
| Total upgrades | **~30** (named kit + standards + fillers) |
| Exotics | **4** — Catalytic Reservoir, Event Horizon, Saxonite Carapace, Heavy Support |
| Exotic hex footprint | **Equal and large** across all four |
| Epics | **~7** |
| Rares | **~10** |
| Standards | **~9** (stackable spine) |
| Path locks | **Heavy Payload** requires **Heavy Support** only |
| Oddity / Contraband grid steal | Out of scope unless later parity pass |
| Shared vanilla staples (Grenade Belt, Penny line, Boundary Incursion) | Optional parity later — **not** required in custom ~30 |
| v1 cross-mod | **None** |
| Vanilla Acid Grenade | **Unmodified** |

---

## 8. Full Upgrade Table

IDs are design placeholders (implementation range: **gear 92400**, **upgrades 92401–92430** — avoid template `920xx`, Friend `921xx`, Honey `922xx`, Splash `923xx`).

Rarity key: **S** Standard · **R** Rare · **E** Epic · **X** Exotic  

Stack: **✓** CanStack · **—** unique  

Well: primary gravity well (still mixable)

### 8.1 Standards (~9) — stat spine

| # | Name | Well | Stack | Intent | Rough numbers (v0) |
|---|------|------|-------|--------|---------------------|
| 1 | **Wide Mouth** | Field | ✓ | Explosion (+ puddle/lake) radius | +15–30% radius · **no CD tax** |
| 2 | **Strong Solvent** | Field | ✓ | Acid effect amount on boom (puddle slightly) | +20–40% effect amount |
| 3 | **Quick Cap** | Carapace | ✓ | Faster recharge | −12–20% recharge · pure CDR |
| 4 | **Hard Flask** | Field | ✓ | Impact damage | +12–22% boom damage |
| 5 | **Base Lining** | Carapace | ✓ | Less self Acid application from your flask | −25–40% self effect amount |
| 6 | **Deep Vat** | Field | ✓ | Puddle / reservoir duration | +20–35% linger durations |
| 7 | **Twin Flask** | Carapace | ✓ | +1 max charge | +1 charge; **explicit mild +15–25% CD** on this card only |
| 8 | **Viscous Mix** | Vacuum | ✓ | Longer fuse / more pull pulses when vacuum upgrades active | Arming feel, not global slow-nade tax |
| 9 | **Throw Weight** | Vacuum | ✓ | Throw force up; less gravity | Saxonite-ish mobility of the *throw*, not player teleport |

### 8.2 Rares (~10)

| # | Name | Well | Stack | Intent | Notes |
|---|------|------|-------|--------|-------|
| 10 | **Polymer Plating** | Carapace | ✓ | Boom grants **timed Armor (DR)** to players in radius | **Polymer overshield replacement** |
| 11 | **Gas Valves** | Field | ✓ | Recharge faster while standing in **your** acid puddle/reservoir | Vanilla fantasy kept |
| 12 | **Universal Solvent** | Field | ✓ | Outgoing damage matching this grenade’s element (**Acid**) increased | Napalm analogue |
| 13 | **Greased Joints** | Vacuum | ✓ | Players hit by boom gain movement ability charge | Filler unless tied to Armor/corroded; optional tiny CDR **bonus only** |
| 14 | **Puddle Harden** | Carapace | ✓ | While **Armor** is active, standing in your puddle mildly refreshes Armor duration | **Bridge** Field+Carapace · **v1 must-ship** |
| 15 | **Clump Tax** | Vacuum | ✓ | Pull-window enemies take bonus detonation/collapse damage; **full bonus if corroded** | **Bridge** Vacuum+Field · dead without pull |
| 16 | **Plate Polish** | Carapace | ✓ | +Armor duration and/or mild +DR | Stacks with Polymer / Carapace within hard cap |
| 17 | **Solvent Siphon** | Field | ✓ | Kills on corroded targets refund small grenade charge | Economy without CD debuff |
| 18 | **Heavy Payload** | Field | ✓ | While **Heavy Support** equipped: faster cargo / improved drop feel | **Requires Heavy Support** |
| 19 | **Catalytic Seal** | Field | ✓ | Puddle/reservoir applies more Acid per tick | |

### 8.3 Epics (~7)

| # | Name | Well | Stack | Intent | Notes |
|---|------|------|-------|--------|-------|
| 20 | **Gas Puddle** | Field | — | Boom leaves a **lingering acid puddle** | Primary field unlock |
| 21 | **Vacuum Tube** | Vacuum | — | Pull targets in during fuse, then explode | Vanilla fantasy kept |
| 22 | **Deteriorate** | Field | — | Metallic targets: apply **Acid and Rot** (additive) | **Rewrite locked** |
| 23 | **Defensive Spurt** | Carapace | — | While **Armor is active**, chance on taking damage to emit a small acid explosion | Weak/no-op without Armor |
| 24 | **Overclock** | Carapace | — | OC enemy explosions recharge flask (range falloff); lethal OC hit can leave you at 1 HP (ICD) | Vanilla fantasy kept |
| 25 | **Odd Cocktail** | Field | — | Chance of a second explosion of a random **non-Acid** element | Off-acid spice — **stretch if** board dilutes T1 |
| 26 | **Exothermic Reaction** | Field | — | Hitting an electrocuted target with the flask ignites it | Prefer this single cross-element epic in v1 |


### 8.4 Exotics (4)

| # | Name | Well | Stack | Intent |
|---|------|------|-------|--------|
| 27 | **Catalytic Reservoir** | Field | — | Detonation leaves a **serious acid reservoir** (long field denial). Consumes/upgrades Gas Puddle layer when both owned |
| 28 | **Event Horizon** | Vacuum | — | Extended arming vacuum; **collapse** prefers corroded / full-sat acid+damage payoff |
| 29 | **Saxonite Carapace** | Carapace | — | Strong timed Armor + **Solvent Cure** scaling — defines the Armor well |
| 30 | **Heavy Support** | Logistics | — | Heavy weapon drop. Cost: reduced boom damage. **Off-triad** logistics exotic |

*Count = 30 on the nose. Stretch: merge Viscous Mix into Vacuum Tube stats if board feels tight.*

---

## 9. Exotic Deep-Dives

### 9.1 Catalytic Reservoir (Solvent Field)

**Fantasy:** You plant a vat. The floor keeps working.

**Behaviour sketch:**

- On detonate (after boom): spawn **Reservoir** zone at impact.  
- Duration: medium-long (e.g. 8–14s v0), improved by Deep Vat.  
- Ticks: Acid apply (+ light damage optional; presence/pressure > burst).  
- Gas Valves eligible. Catalytic Seal strengthens ticks.  
- **With Gas Puddle:** single zone layer — Reservoir **absorbs/upgrades** puddle (no double floor).  
- Visual: larger, longer acid pool (placeholder OK).

**Mix notes:**  
Carapace + Puddle Harden = armored wading. Clump Tax + Event Horizon can feed bodies into the vat. Heavy Support crate can land in/near reservoir for chaotic logistics theater.

### 9.2 Event Horizon (Vacuum Lab)

**Fantasy:** A long drink of gravity, then the flask eats the room.

**Behaviour sketch:**

- On fuse start (after bounce rules): stronger/longer vacuum than Vacuum Tube alone.  
- Pull pulses while fuse active; Viscous Mix adds pulses/arm time.  
- On detonate: **collapse** — bonus **Acid saturation spike** and/or bonus damage to enemies still inside pull radius; **prefer already corroded / fully corroded** for full payoff tier (§5.7).  
- Clump Tax multiplies collapse on corroded targets.  
- **No player teleport** (Lightspeed is Shock’s card now).  
- Visual: vortex during fuse + implosion cue on boom.

**Mix notes:**  
Vacuum Tube + Event Horizon: Event Horizon is the exotic superset; Tube stats feed Horizon if both owned (impl: exotic enables pull; Tube adds force/radius — or exotic replaces Tube layer). Prefer **no double vortex**.  
Collapse without prior acid is a weaker finish — T3 completes T1’s sentence.

**With Vacuum Tube only:** standard pull-then-boom.  
**With Event Horizon:** longer arm + corroded-preferring collapse payoff.

### 9.3 Saxonite Carapace (Carapace)

**Fantasy:** Solvent that hardens on friendlies — walk into the bad idea.

**Behaviour sketch:**

- On detonate: apply **strong timed Armor** to alive players in radius (including thrower).  
- DR and duration higher than Polymer Plating alone; Plate Polish still stacks within **hard DR cap**.  
- **Solvent Cure** applies: thicker/longer film when this detonate applied meaningful acid (§5.5).  
- Defensive Spurt keys off Armor active; **Corrosion Pulse** can extend mid-fight.  
- Time-based only — no plate HP, no overshield.  
- Visual: brief green-grey plating VFX / stack icon.

**Mix notes:**  
Polymer Plating + Carapace = duration/DR stack within cap (not double independent gods). Harden + Valves = live on the hinge. Overclock keeps you alive through OC so plating windows matter. Base Lining reduces self-acid while you stand in your own mess.

**Risk budget:**  
Hard cap DR from this gear (~40–45%). Short-to-medium D. Refresh requires throwing / hinge loops. Must not combine with future kits into permanent unkillable without throw cadence.

### 9.4 Heavy Support (Logistics)

**Fantasy:** The flask is also a flare gun for ordinance.

**Behaviour sketch:**

- On detonate: server cargo pod → heavy weapon crate for owner (vanilla Acid path).  
- **Cost (preferred):** while equipped, primary explosion damage reduced (e.g. noticeable but not zero — crate is the headline).  
- **Not preferred:** global recharge brick. If playtest needs time cost, explicit +CD **on this card only** and called out in description.  
- Heavy Payload rare: faster arrival / polish; no-ops without Heavy Support.  
- Multi-charge / Twin Flask: define whether each detonate calls cargo (recommend **ICD per drop** so Double charge ≠ double heavy spam).

**Mix notes:**  
Not a healing card. Not a mobility card. Not a T1–T3 card. Pure logistics exotic that stayed because it is memorable and on-theme for “expensive solvent ops.” Equal hex footprint; **off-triad** when prioritizing design oxygen.

### 9.5 Exotic coexistence

| Pair | Rule |
|------|------|
| Reservoir + Event Horizon | Allowed. Pull into vat is peak. |
| Carapace + either | Allowed. Armor is player-bound. |
| Heavy + any | Allowed. Boom damage cost is the budget. |
| All four | Allowed if grid fits; power via ICD/caps/durations. |
| Footprints | All four exotics **same cell count**, larger than typical rares/epics. |
| Vacuum Tube + Event Horizon | One pull system; exotic supersets/upgrades. |
| Gas Puddle + Reservoir | One floor layer; exotic supersets/upgrades. |

---

## 10. Named Kit — Detailed Specs

### Gas Puddle (Epic)

- Sets `puddleDuration > 0`; detonate spawns acid puddle.  
- Deep Vat / Wide Mouth / Catalytic Seal scale.  
- Unique.  
- If Catalytic Reservoir also equipped: reservoir consumes puddle layer (see §9.1).

### Vacuum Tube (Epic)

- Sets pull force + pull radius mult + enables fuse-active pull.  
- Viscous Mix / Throw Weight support.  
- Unique.  
- If Event Horizon equipped: horizon owns pull presentation; Tube contributes numbers.

### Deteriorate (Epic) — rewrite

- On flask damage to `target.IsMetal()`: keep Acid on primary hit; **additional** Rot application.  
- Does **not** set `damage.effect = Rot` as a replacement.  
- Unique.

### Defensive Spurt (Epic)

- Requires local player Armor remaining time > 0 (or DR buff active).  
- On after/before take damage: accumulate or roll chance → small acid explosion at player (vanilla Defensive Spurt DNA).  
- Without Armor: no procs (or grey “Requires Armor” if UI allows).  
- Unique.

### Overclock (Epic)

- Subscribe `OnBeforeTakeDamage` when equipped.  
- Overclocked incoming damage → `AddCharge` scaled by distance.  
- Lethal OC clamp + ICD + stack display.  
- Unique. **No recharge duration penalty.**

### Odd Cocktail (Epic)

- On detonate: chance for second explosion at impact with random `EffectType` ≠ Acid (reroll Acid → Normal like vanilla).  
- Unique.

### Exothermic Reaction (Epic)

- When damaging a target fully saturated with Shock: also apply Fire full-dump style (vanilla ElectroIgnite DNA).  
- Unique.

### Polymer Plating (Rare)

- On detonate: for each alive player in `radius = explosion * mult`, apply ArmorPlating (DR, duration from rolls).  
- **Solvent Cure** scales bonus duration/DR with acid applied this detonate (§5.5).  
- Stackable: improves DR and/or duration and/or radius within hard caps.  
- **Never** negative heal / overhealth.

### Gas Valves (Rare)

- While standing in your puddle/reservoir (impl: puddle touch window or overlap test): multiply recharge gain.  

- Stack display while active.  
- Scales honestly with long baseline CD builds without being the only way to play.

### Universal Solvent (Rare)

- Outgoing damage with `EffectType.Acid` increased (mirror grenade `outgoingDamageMultiplier` filtered to Acid — boom, puddle, spurt, reservoir ticks you own).

### Greased Joints (Rare)

- Players in boom radius: `RechargeMovementAbility` fraction.  
- Optional tiny CDR bonus only — **not** a CD penalty card.

### Clump Tax (Rare)

- Enemies inside pull radius at detonate moment (or tagged during pull) take bonus damage from primary boom / collapse.  
- **Full bonus if target is corroded** (Acid sat > 0); reduced or zero on clean targets (§5.7).  
- Dead card without Vacuum Tube or Event Horizon — acceptable rare gravity. **Bridge** Vacuum+Field.

### Plate Polish (Rare)

- Multiplies Armor duration and/or adds flat DR from your Armor-applying upgrades.  
- Hard cap still enforced at apply time.

### Puddle Harden (Rare)

- Each tick or periodic check: if Armor active and player in own puddle → refresh small duration slice.  
- Does not create Armor from zero.  
- **v1 must-ship bridge** (Field + Carapace) — not stretch.

### Solvent Siphon (Rare)

- On kill callback where victim had Acid sat > 0 (or kill by your acid systems): `AddCharge` small.  

- ICD optional to prevent chain insanity.

### Heavy Payload (Rare)

- Gate: Heavy Support equipped.  
- Faster cargo / reduced drop delay / polish.  
- No-op without exotic.

### Saxonite Carapace (Exotic)

- See §9.3. Strong Armor apply on boom.

### Catalytic Reservoir (Exotic)

- See §9.1.

### Event Horizon (Exotic)

- See §9.2.

### Heavy Support (Exotic)

- See §9.4.

---

## 11. Synergy Notes (Player-Facing, Soft Only)

No mod dependencies. Loadout tips for README / codex blurb.

| Partner | Why it feels good |
|---------|-------------------|
| Shock weapons / Shock Grenade | Exothermic Reaction; wet/shock partners still fine; Lightspeed lives on Shock later |
| Cryo (future) | Connected Systems lives there — Flask stays corrosion/armor |
| Splash Canister | Splash wets; Flask corrodes — different floors. DR% vs shield HP both defensive but stack carefully in co-op theory |
| Honey Jar | No shared heal identity; pure damage/status co-op |
| Incendiary | Exothermic / Odd Cocktail chaos; no Welding Heat clone |
| Metal-heavy missions | Deteriorate dual-status payoff |
| OC-heavy enemy modifiers | Overclock + Carapace survival fantasy |
| Heavy weapons fans | Heavy Support crate loop |

**Explicit non-goals v1:** patching vanilla Acid; implementing Lightspeed or Connected Systems on this gear; overshield parity with Polymer; cross-mod armor sharing APIs.

---

## 12. Pillar Evaluation, Strengths, Weaknesses & Failure Modes

### 12.1 Triad grades (design review)

| Pillar | Grade | Note |
|--------|-------|------|
| T1 Acid build-up as center | **B+ → target A-** | Best-supported well; build-up curve + corroded breakpoints lock the grade up |
| T2 Armor as healing type | **B → target A-** | Distinct and capped; Solvent Cure + mid-fight loops fix “DR next to acid” |
| T3 Battlefield control | **A-** | Vacuum + field denial coherent; corroded-preferring payoffs keep CC on-spine |
| **Pillars as a triad** | **C+ → target B+/A-** | Was parallel wells; bridges in §6 are now load-bearing |

**Pass signal:** *“pull them into my solvent and walk in plated.”*  
**Fail signals:** “only the armor nade” / “only the pull nade” / Heavy crate is why you equip it / every hit full green / armor only pre-cast never mid-fight.

### 12.2 How pillars reinforce each other

| Pairing | Strength | Mechanism |
|---------|----------|-----------|
| Acid ↔ Control | Strong | Clump Tax + collapse prefer corroded; vacuum feeds Reservoir |
| Acid ↔ Armor | Locked bridge | Solvent Cure, Corrosion Pulse, Spurt, Harden |
| Control ↔ Armor | Weak→moderate | Shared boom timing; Pressure Weld stretch; hinge co-location |

### 12.3 Strengths

- Clear upgrade fantasy vs vanilla Acid’s grab-bag  
- Timed Armor is proactive and distinct from HoT / overshield / regen shell  
- Field as triad hinge (not only damage floor)  
- Dual-status Deteriorate rewards metal literacy  
- CD culture respects the player — upgrades should feel good to equip  
- Build-up curve makes *fully corroded* a real breakpoint  
- Heavy retained without pretending it is a triad pillar  

### 12.4 Weaknesses (accepted or mitigated)

- Stock throw is intentionally bland (by design)  
- Armor / puddle / pull are dead stats until unlock cards (mitigate with readable epic/exotic fantasy)  
- Lower raw burst than invested Incendiary  
- DR loses panic-heal war vs Honey/Splash — must throw *before* the spike; mid-fight loops are mandatory  
- Heavy Support softens boom — off-triad logistics, not DPS  
- Conditional cards (Clump Tax, Harden, Spurt, Payload) can feel like traps if tooltips are weak  

### 12.5 Failure modes to avoid in tuning

| Failure | Mitigation |
|---------|------------|
| Permanent DR god | Hard DR cap, short D, refresh needs throws / hinge |
| Armor + anything = unkillable co-op | Cap, no overshield hybrid on this gear |
| Armor is generic DR stamp | **Solvent Cure** — plating scales with acid applied |
| Armor only pre-cast | Harden + Spurt + Corrosion Pulse mid-fight loops |
| Instant full-sat wallpaper with zero investment | Baseline amount is family **10** (full-sat class); gradient via decay + puddle/multi-throw re-sat; collapse/Siphon still prefer corroded state |

| Control ignores acid | Clump Tax / collapse prefer corroded |
| Three monobuilds, no triad | Load-bearing bridges (§6); Harden not stretch |
| CD tax creeps back in | Review every property for rechargeDuration writes |
| Deteriorate deletes Acid identity | Additive Rot only |
| Double puddle + reservoir floors | Single zone layer rule |
| Double vacuum systems | Single pull layer rule |
| Heavy double-crate spam | Drop ICD |
| Heavy steals design oxygen | Off-triad reviews; equal hex only |
| Defensive Spurt without Armor | Gate on Armor active |
| Polymer muscle memory (blue bar) | UI copy says **damage resistance**, not overshield |
| Off-acid epic bloat | Prefer one cross-element epic (Exothermic); Odd Cocktail stretch |
| 30 upgrades but 3 forced builds | Standards universally useful; bridges reward mix without hard locks |

---


## 13. Implementation Appendix (For Later — Not This Pass)

Design-only milestone: **this document**. When coding starts, prefer:

| Piece | Approach |
|-------|----------|
| Registration | Existing grenade template `GrenadeRegistration` clone path; set `GunData.damageEffect = Acid` |
| Name / IDs | Display **Caustic Flask**; `APIName` `caustic_flask`; gear id **92400**; upgrades **92401–92430** |
| Data host | `CausticFlaskBehaviour` (rename from example) with `Data` struct for flags/scalars |
| Detonate | Harmony on `GrenadeBullet.Detonate` (FriendinaBox / template style) |
| Puddle | `GameManager.Instance.SpawnAcidPuddle_Rpc` |
| Pull | Fuse-active impulses like `AcidGrenadeBullet.OnFuseActive` + vortex VFX |
| Armor | Player buff component/stack + `OnBeforeTakeDamage` multiply |
| Deteriorate | Second Rot packet on metal; do not replace Acid effect |
| Heavy | Port `CallHeavyWeapon_ServerRpc` / cargo land crate |
| Overclock | Port before-damage OC charge + lethal clamp + stack display |
| Defensive Spurt | Chance explosion on damage while Armor active |
| Upgrades | `PlayerData.CreateUpgrade` + `UpgradeProperty` Apply/Remove restoring prefab snapshot |
| Mod flags | `[MycoMod(..., ModFlags.IsSandbox)]` |
| Vanilla Acid | **Do not patch** |
| Cross-mod | None in v1 |

### Suggested `CausticFlaskBehaviour.Data` fields (sketch)

```
// Baseline / scales
float explosionRadiusMultiplier;
float acidEffectAmountMultiplier;
float boomDamageMultiplier;
float selfAcidMultiplier;

// Gas Puddle / Reservoir
float puddleDuration;
float puddleTickAcidMult;
float reservoirDuration;          // exotic
float reservoirRadiusMult;
bool catalyticReservoir;

// Gas Valves
float rechargeMultiplierInAcidPuddle;
int puddleChargesApplied;

// Vacuum
float pullInForce;
float pullInRadius;
float pullFuseBonus;              // Viscous Mix
bool eventHorizon;
float collapseDamageMult;
float collapseAcidBonus;          // prefer corroded / full-sat tier
float clumpTaxMult;               // full bonus if corroded
float clumpTaxCleanMult;          // < 1 or 0 on non-corroded


// Armor (timed DR)
float armorDr;                    // 0.12 = 12% DR
float armorDuration;
float armorRadiusMult;
float armorDrCap;                 // enforce
bool saxoniteCarapace;
float platePolishDurationMult;
float platePolishDrAdd;
float puddleHardenRefresh;
float solventCureDurationPerCorroded; // Solvent Cure scale
float solventCureDrPerFullyCorroded;  // mild, within cap
float corrosionPulseDuration;         // mid-fight acid→armor
float corrosionPulseIcd;
float corrosionPulseRadius;


// Defensive Spurt
float damageExplodeChance;
float damageExplodeSize;

// Overclock
float overclockCharge;
float overclockChargeCooldown;
int overclockChargesApplied;

// Odd Cocktail / Exothermic / Deteriorate
float randExplosionChance;
bool deteriorateDualStatus;
bool electroIgnite;

// Heavy Support
bool heavySupport;
float heavyBoomDamageMult;        // < 1 cost
float heavyPayloadSpeedMult;      // rare

// Greased Joints / Solvent / Universal Solvent
float moveAbilityRecharge;
float acidOutgoingDamageMult;
float solventSiphonCharge;

// Twin Flask handled via cooldownData.maxCharges + recharge on property
```

### Ship cut vs stretch

**v1 must-ship (triad fantasy complete):**

- Baseline bland acid boom with shared family stats (**100** dmg · **10** effect · **3** charges · **45** CD · `hitForce` **6**)  

- All 4 exotics (Heavy = off-triad logistics, still ships)  
- Gas Puddle, Vacuum Tube, Deteriorate (dual), Defensive Spurt, Overclock, **Exothermic**  
- Polymer Plating, Gas Valves, Universal Solvent, **Clump Tax (corroded)**, Plate Polish, **Puddle Harden**, Heavy Payload  
- Full standard spine  
- Timed Armor model + **Solvent Cure** + **Corrosion Pulse** + no CD-tax culture  
- Event Horizon collapse prefers corroded  
- Heavy cost = reduced boom damage  

**Stretch / post-v1:**

- Odd Cocktail (if T1 feels diluted, cut or delay)  
- Greased Joints polish / slick-plating tie-in  
- Pressure Weld (Vacuum↔Carapace rare)  
- Pull-path acid drip juice  
- Shared staple parity (Belt, Penny line)  
- Custom mesh / Wwise  
- Config toggles for Heavy cost mode  


---

## 14. Naming & Presentation

| Slot | Value |
|------|--------|
| Display name | **Caustic Flask** |
| Internal / API | `caustic_flask` |
| Design nickname | Acid Grenade Rework (notes / folder only) |
| Short description | *Acid-element grenade. Stock throw is a clean corrosive boom. Upgrades unlock melting puddles, vacuum collapse, timed armor plating, and heavy cargo — without cooldown taxes.* |
| Thunderstore name (later) | `CausticFlask` |
| GUID (later) | `sparroh.causticflask` |
| Folder today | `.new.AcidGrenadeRework` (rename optional at ship) |

### SAXON marketing blurb (draft)

> SAXON Caustic Flask — Industrial solvent delivery for soft targets and softer warranties.  
> Baseline: corrosion. Aftermarket: reservoirs, event-horizon funnels, temporary carapace plating, and one (1) authorized heavy drop per sufficiently enthusiastic impact.  
> Overshield not included. Overshield was a liability.  
> “If it still has a cooldown after you upgrade it, file a ticket — not a funeral.”

---

## 15. Open Questions (Balance / Feel — Not Blocking Doc)

1. Heavy Support: confirm **boom damage cost** vs explicit +CD after first playtest.  
2. Gas Valves: pure overlap-with-own-puddle vs vanilla “took AcidPuddle flag” sensing (players take 0 puddle damage today).  
3. Event Horizon + Vacuum Tube: exact stat merge formula.  
4. Armor ally radius: full team coat always, or Carapace = team / Polymer = self-biased? (Default: **both team-capable**.)  
5. Twin Flask mild +CD magnitude.  
6. Exact hex shapes for 30 upgrades — author during implementation.  
7. Max concurrent reservoirs if Twin Flask + long Deep Vat (cap 2).  
8. Defensive Spurt ICD and whether it consumes Armor time (default: **no consume**, time-only).  
9. Solvent Cure curve: scale off enemy count corroded vs total acid effect amount vs fully-corroded count.  
10. Corrosion Pulse: on full-sat apply only, or also corroded kill? Radius and ICD.  
11. Clump Tax clean-target mult: 0 vs ~0.35 partial.  
12. Baseline boom effect amount locked to family **10**; re-check sat-feel vs decay/puddle after first playtest (not a return to 5–7).  



---

## 16. Design Checklist

- [x] Separate gear (not in-place vanilla patch)  
- [x] Name: Caustic Flask  
- [x] Core triad locked (acid build-up · armor-as-heal · battlefield control)  
- [x] Bland baseline (no puddle, no armor)  
- [x] Shared family baseline: dmg **100** · effect **10** · charges **3** · recharge **45** · `hitForce` **6**  
- [x] Baseline acid effect amount **10** (full-sat class); T1 gradient via decay / field / multi-throw  
- [x] Sustain identity: **Damage Resistance** (timed DR), upgrade-only  

- [x] Armor = upgrade-only timed DR  
- [x] Solvent Cure + Corrosion Pulse (armor↔acid bridges)  
- [x] Puddle = triad hinge; Harden v1 must-ship  
- [x] Control payoffs prefer corroded (Clump Tax / collapse)  
- [x] No overshield / HoT identity  
- [x] Cooldown-tax culture rejected; baseline CD OK  
- [x] Deteriorate = Acid + Rot additive  
- [x] Lightspeed → Shock (out)  
- [x] Connected Systems → Cryo (out)  
- [x] Heavy Support stays (exotic, **off-triad**)  
- [x] Overclock stays (epic)  
- [x] ~30 upgrades, 4 equal large exotics  
- [x] Gravity wells mix/match with load-bearing bridges  
- [x] Names free to change; key fantasies retained  
- [x] Self-contained v1  
- [x] Implementation deferred  


---

## 17. Changelog (Design Doc)

| Date | Change |
|------|--------|
| 2026-08-15 | **Shared throwable baseline lock:** damage **100**, `damageEffectAmount` **10**, max charges **3**, recharge **45**, explosion radius (`hitForce`) **6**. Sustain column confirmed **Damage Resistance** (timed DR). Baseline acid amount raised from ~5–7 partial-sat curve to family **10** (full-sat class); T1 build-up reframed around decay / puddle re-sat / multi-throw rather than nerfed stock amount. |
| 2026-08-12 | Pillar evaluation pass folded in: core triad §3.0 (acid build-up · armor-as-heal · control); cohesion rule + puddle-as-hinge; baseline sat curve ~50–70%; Solvent Cure + Corrosion Pulse; Clump Tax/collapse prefer corroded; Heavy marked off-triad; Harden promoted to v1 must-ship; Odd Cocktail stretch-leaning; load-bearing bridges table; §12 rewritten with grades/fail signals; ship cut + Data fields + open questions updated. |

| 2026-06-08 | Initial design doc from vanilla Acid wiki + decompile (`AcidGrenade`, `AcidGrenadeBullet`, puddle/OC/Polymer anchors), sibling docs (Honey Jar, Splash Canister, DMLR), and user locks: separate gear, Caustic Flask name, no baseline puddle/armor, CD-tax diagnosis, timed DR only, Deteriorate dual-status, Lightspeed→Shock, Connected→Cryo, Heavy+Overclock stay, full rename OK, ~30 with 4 exotics. |

---

*End of design document. Next step when ready: rename template identifiers to Caustic Flask and implement baseline acid boom only (family stats 100/10/3/45/6), then layer Gas Puddle → Vacuum → Armor (Solvent Cure) → exotics.*


