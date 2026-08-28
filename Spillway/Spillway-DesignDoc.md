# Spillway – Design Document (v1)

> Status: **Design only** — no implementation yet.
> Working title in notes: Globbler Rework. **Ship name: Spillway.**
> Template base: `.new.GlobblerRework` weapon content project.
> Product shape: **separate primary gear** — vanilla Globbler is left unmodified.

---

## 1. High Concept / Fantasy

**Spillway** is the solvent grenade hose that finally has more than one fantasy.

Vanilla Globbler funnels real power into **Pressure Cooker** (hold-to-eat-mag mega glob). **Globblometer** eats half the basic grid and does almost nothing alone. Flood is cool but lonely. Grenade siphon only fattens explosion size. Everything else is a sidegrade waiting for the cook build.

Spillway keeps the bouncy acid-grenade gunfeel and rebuilds the grid around three peer modes plus a free field exotic:

- **Cook** the magazine into one apocalypse shot (with deliberate overcook self-acid tech).
- **Storm** a hose of mini-nades so clear is a real build.
- **Recipe Loader** turns the gun into a magazine for your **full equipped grenade** (DiscWorld DNA).
- **Flood** paints running elemental waves — equippable with any path.

Every upgrade contributes Globblometer. Globblometer is **damage**. Empty-grid Spillway must not feel like a wet noodle.

**One-liner:** *Cook the vat, storm the room, or load your real grenade into the mag — every upgrade feeds the meter that makes Spillway hit.*

**Element spine:** `EffectType.Acid` at baseline. Recipe Loader replaces payload with the equipped throwable’s full identity.

---

## 2. Role & Fantasy in the Arsenal

| Trait | Value |
|-------|--------|
| **Slot** | Primary |
| **Range** | Mid (lob arc); Storm tightens; Cooker / Recipe extend effective alpha |
| **Role** | AOE grenade primary, field denial, throwable synergy engine |
| **Gap filled** | Vanilla Globbler is one build (Pressure Cooker). Spillway is three peer modes + Flood |
| **Synergies** | Equipped grenade kit (Acid / Incendiary / Shock / Disc / custom), self-acid loops, movement, offhand ammo refund toys |

**Product shape:** New primary (**Spillway**). Does **not** replace or patch vanilla Globbler.

**Not trying to be:** long-range DMR, pure hitscan hose, heavy weapon, or a mandatory grenade-mod dependency.

| Gear | Relationship |
|------|----------------|
| Vanilla Globbler | Left in game untouched |
| Caustic Flask / grenade reworks | Soft synergy via Recipe; no hard dependency |
| DiscWorldRework | **Recipe Loader DNA source** — gun handling, full throwable projectile |
| Photon Disc / other throwables | Valid Recipe payloads when equipped |
| Heat Cycler / Aussie Special | Sibling design structure only |

---

## 3. Design Pillars

1. **Baseline must hit** — raise empty-grid damage vs vanilla Globbler spirit; Cooker is not the damage tax.
2. **Globblometer is damage currency** — every upgrade adds meter; **no cards exist only to give meter**.
3. **Three peer path crowns + Flood free exotic** — Cooker / Storm / Recipe are path spines; Flood mixes with all.
4. **Soft crown conflicts** — Cooker + Storm both equippable; hybrids shrink/speed rather than hard-disable.
5. **Recipe = full grenade** — DiscWorld pattern: gun keeps RoF/mag/reload/spread; shots are real throwable projectiles with grenade stats and upgrades.
6. **Siphon is not Cooker support** — Globulous Siphon rewrite empowers the *next payload*, any mode.
7. **The Glob + Cooker is intentional tech** — mag 1 cannot stack cook ammo → overcook self-acid strategy.
8. **Flood is first-class field, not a lonely exotic** — wave + trail supports in glue; turbo DNA baked in.
9. **Distributed power** — many Standard/Rare/Epic cards carry damage, size, economy, bounce, field verbs.
10. **~30 upgrades for v1** — exotic shapes larger and equal cell count; full rename pass.
11. **Self-contained v1** — no hard deps on grenade reworks or DiscWorldRework at runtime (pattern copied, not required).

---

## 4. Core Mechanics & Gunfeel

### 4.1 Base gun (no upgrades)

| Trait | Draft / intent |
|-------|----------------|
| Fire mode | Automatic / semi-auto acid globs (vanilla Globbler cadence spirit) |
| Projectile | `GlobblerBullet` path with **Ziggs-Q hop** (forward + up arcs, not Reflect) |
| Damage | **Raised vs vanilla** — empty grid clears packs; meter multiplies further |
| Element | Acid |
| Magazine / reserve | ~7 / 63 spirit (Storm / The Glob mutate) |
| Reload | Standard reload beat + door anim fantasy kept if art allows |
| Bounces | **2** surface hops (3 impacts); hop continues **forward** with heavy gravity |
| Detonate | **Every impact** (per-hop damage share); enemy hit = full damage once + stop |
| Explosion size | `hitForce` / `data.force` (vanilla); intermediate pops slightly smaller |
| ADS | Normal unless a crown says otherwise |
| Model / audio | Borrow Globbler until custom art |
| Cooker / Storm / Recipe / Flood | **Off** |


### 4.2 Base combat loop

```
Hold/tap M1 → lob acid glob → forward arc hop → pop → hop → pop → final land pop (AOE + Acid)

   ↘ reload on empty
   ↘ without crowns: honest AOE primary (not waiting for Cooker)
   ↘ Globblometer from equipped upgrades multiplies damage
   ↘ with Cooker: hold M1 eats mag into charge → release mega payload
   ↘ with Storm: rapid mini payloads
   ↘ with Recipe: each shot is a full equipped grenade
   ↘ with Flood: surface impact starts a running wave (+ trail when upgraded)
   ↘ hold R with Siphon: eat grenade charge → empower next shot
```

### 4.3 Inputs

| Input | Baseline | With crowns |
|-------|----------|-------------|
| **M1** | Fire glob | Cooker: hold charges, release fires; Storm: rapid fire; Recipe: fire full grenade |
| **RMB / AIM** | Normal ADS | Unchanged unless a card removes ADS |
| **R tap** | Reload | Reload |
| **R hold** | No baseline special | **Siphon** owns hold-R when equipped |

### 4.4 Crown priority (SOFT — not hard mutex)

Unlike Aussie Special’s hard Blightflame > Twin-Hopper rule, Spillway uses **soft modifiers** when multiple crowns are equipped:

| Combo | Behavior |
|-------|----------|
| Cooker alone | Full hold-to-eat-mag cook |
| Storm alone | Mini payload, high RoF, reduced size/damage per shot |
| Recipe alone | Full grenade per trigger; gun handling |
| Flood alone | Wave on surface impact |
| **Cooker + Storm** | Charge still works; charge eat slightly faster or payload size reduced; RoF/handling crumbs from Storm — **weaker cooks, livelier tempo** |
| **Cooker + Recipe** | Cook spends **gun** ammo; release fires **one full grenade-payload** scaled by charge stacks |
| **Storm + Recipe** | Mag-fed full grenades at Storm cadence — **power budget with size/RoF/damage taxes** (see §5) |
| **Any + Flood** | Waves on surface impact; wave element follows active payload element |
| **Cooker + Storm + Recipe** | Allowed hybrid freak; stack soft taxes so it is fun, not delete-everything |

**No hard disable** of Cooker when Storm is equipped (and vice versa).

---

## 5. Globblometer System (LOCKED)

### 5.1 Vanilla failure

- Many cards only add `globblometer` (`UpgradeProperty_Globbler_Globblometer` / Globblin).
- Meter alone does nothing; it only scales *other* upgrades (speed, siphon size, switch dmg, funnel drain, self-resist duration, volatile max, rand element chance).
- Players stack dead stats to enable Cooker-adjacent cards.

### 5.2 Spillway rules

1. **Every upgrade contributes Globblometer** on Apply (rarity-banded rider).
2. **No upgrade exists solely to grant meter.** Globblin-as-filler is cut or rewritten into a real verb that *also* grants meter via the universal rider.
3. **Baseline payoff = DAMAGE** (always on, no exotic required):

```
damageMult = 1 + MeterDamageCoeff * GlobblometerNormalized
GlobblometerNormalized = clamp(globblometer, 0, Max) / Max
Max = 50 (vanilla cap unless playtest raises)
```

Starting target: **MeterDamageCoeff ≈ 0.80–1.20** (at full meter, roughly +80–120% damage). Tune so:
- Empty grid (0 meter): raised base damage still playable
- Half grid: clearly stronger
- Full meter + damage staples: competitive without requiring Cooker

4. Path cards may **also** scale with meter (secondary):
   - Cooker: charge efficiency / max size ceiling
   - Storm: slight RoF or mini-size restore
   - Recipe: slight throw force or effect amount
   - Flood: wave length crumb
   Secondary scaling is spice; **damage is the spine**.

5. Implementation: prefer a single **universal meter rider** on upgrade registration (or behaviour that sums rarity × count) rather than re-implementing `IGlobblometerProperty` on every card. Cards that need extra meter beyond the rider can add a small bonus in Apply.

### 5.3 Rarity → meter contribution (draft)

| Rarity | Meter per equipped card |
|--------|-------------------------|
| Standard | +3 |
| Rare | +4 |
| Epic | +5 |
| Exotic | +6 |
| Oddity | +2 |

Playtest: if full grids always cap at 50 too early, lower Standard/Rare; if meter feels scarce, raise. Cap behavior at 50 remains.

### 5.4 What meter is NOT

- Not a requirement gate (“need 20 meter to unlock Cooker”).
- Not the only damage source (base damage + staples + path verbs still matter).
- Not restored as pure Globblin filler cards.

---

## 6. Crowns & Sacred Systems

### 6.1 Pressure Cooker — Exotic (Path A crown)

**Fantasy:** Hold the trigger to feed the vat. Release a glob (or Recipe grenade) whose size and damage scale with ammo cooked. Overcook the whole mag → explode on yourself.

**Vanilla DNA kept:**
- `chargeTimePerAmmo`, `sizePerChargedAmmo`, `damagePerChargedAmmo`
- Hold fire while `chargeTimePerAmmo > 0` eats ammo into `globChargeAmmo`
- `RemainingAmmo < 1` while charging → self explosion (Acid)
- Fire consumes charge; reload after cook shot (vanilla OnFire reload when charging)

**Spillway retune intent:**
- Still the ST / alpha fantasy
- Damage budget shared with meter + staples so Cooker is not mandatory
- Soft with Storm: reduced size-per-charge and/or faster charge tick with less peak multiplier
- Soft with Recipe: cooked stacks multiply **grenade payload** force/damage, not a fake glob

**HUD:** keep charge bar spirit (`GlobblerHUD` charge parent).

### 6.2 The Glob ↔ Cooker (LOCKED interaction)

Vanilla **The Glob**: magazine size → 1, bigger explosion, faster reload.

With Cooker:
- Mag 1 means you cannot stack multiple cooked ammo into one shot.
- Holding fire still advances the charge timer and can drive the **overcook self-explode** path.
- This is a **deliberate strategy**: self-apply Acid (and pair with Replicator Resistance / self-acid payoffs), not a bug.

Doc / UI note: upgrade descriptions should not promise “cook the whole mag” when mag is 1; flavor can wink at “single-chamber overcook.”

### 6.3 Storm Vat — Exotic (Path B crown) [NEW]

**Fantasy:** The vat never closes. Rapid mini grenades (or mini full-grenades under Recipe).

**Effects (draft):**
- Fire rate greatly increased
- Per-shot explosion size and damage reduced (mini)
- Optional: +bulletsPerShot (2–3 micros) with further per-pellet split
- Mag size increased modestly OR reload sped up
- Globblometer still multiplies damage so Storm scales with grid fill

**Soft with Cooker:** see §4.4 — not disabled.

**Name alternatives if needed:** Microburst, Scatter Vat, Hose Gate, Pellet Spill.

### 6.4 Recipe Loader — Exotic (Path C crown) [NEW — DiscWorld DNA]

**Fantasy:** Spillway stops firing globs. Each shot fires your **equipped throwable as a full projectile** — real damage, element, explosion, and grenade upgrade behavior. The gun is a magazine and trigger; the grenade is the bullet.

**From DiscWorldRework (Photon Disc / Disc World):**

| Controlled by **Spillway (gun)** | Controlled by **equipped grenade** |
|----------------------------------|--------------------------------------|
| Fire rate | Damage |
| Reload speed | Element / effect amount |
| Magazine size | Explosion radius (`hitForce` / grenade rules) |
| Ammo capacity / reserves | Projectile prefab & flight (speed, gravity, bounce, fuse) |
| Spread / recoil | Grenade upgrade flags & detonate extras |
| Cooker ammo-eat / Storm cadence | Puddle, pull, cluster, disc wave, etc. |
| Globblometer **gun-side damage mult** | Attunement / grenade-specific systems |

**Rules (LOCKED):**
1. Does **not** spend grenade cooldown per gun shot — **magazine ammo is the cost** (DiscWorld spirit).
2. Continuous weapons / invalid sources excluded or fallback (mirror DiscWorld continuous + Plate Launcher exclude spirit).
3. No throwable equipped → fallback baseline acid glob + clear feedback (sound/UI).
4. Observer/network: fire full grenade bullets with correct damage source attribution (gun as source or grenade-as-source with gun owner — pick one at impl; prefer readable kill credit).
5. Grenade upgrades on the throwable still apply to fired payloads (as DiscWorld keeps disc upgrades).

**Not:** “glob with grenade element tint only.”  
**Is:** replace projectile with the real grenade shot.

### 6.5 Flood — Exotic (NON-PATH)

**Fantasy:** Impact a surface → running wave of solvent. Field control for any build.

**Vanilla DNA:**
- `GlobblerUpgradeFlags.Wave`
- `GlobblerBullet.OnHit` surface → wave march on fuse
- `waveLength` distance
- Flash Flood lengthens

**Spillway enhancements (bake turbo DNA):**
- Wave travels farther (base exotic already strong; supports push further)
- **Lingering acid trail** along the wave path (puddle ticks / corrode on contact) — from SparrohsTurbocharges Flood note
- With Recipe: wave element follows grenade element when feasible
- With Storm: more frequent smaller waves
- With Cooker: one fat wave from the cooked impact

**Not** a Path C exclusive. No path tax.

### 6.6 Globulous Siphon — Exotic (rewrite, not Cooker-tied)

**Fantasy:** Hold RELOAD to drink grenade charge into the next shot.

**Vanilla:** size only (`grenadeChargePerSecond`, `grenadeSizePerCharge`, `grenadeSizeMax`), meter scales size.

**Spillway:**
- Still hold-R, eats throwable charge, disables recharge while sipping
- Empowers **next payload**: explosion size **and** effect amount / Recipe potency
- Works equally with baseline globs, Storm micros, Cooker release, Recipe grenades
- **Explicitly not** a Pressure Cooker battery or cook-speed card
- Meter may mildly improve siphon efficiency (secondary), not the card’s only identity

### 6.7 Impact Funnel — Exotic (rewrite)

**Fantasy:** Pain in, solvent out. Damage taken builds a multiplier spent into shots.

**Vanilla:** `damagePerTakenDamage`, `maxDamageMult`; fire consumes meter; globblometer slows drain.

**Spillway:**
- Keep taken-damage → next-shot damage bank
- Pair with raised baseline so Funnel is a spike tool, not the only way to deal damage
- Self-damage (Cooker overcook, Siphon mishaps, Replicator loops) can feed Funnel — intentional
- Storm spends bank across more shots (faster drain per time, less per shot) — soft feel, not a special case bug

### 6.8 Boundary Incursion — Oddity

Grid grow. Universal keep. Still grants meter rider (small).

---

## 7. Upgrade Paths (gravity wells — hybrids intended)

### Path A — COOKER
**“Feed the vat. Release the sun.”**

- **Spine:** charge, size-per-ammo, damage-per-ammo, overcook risk/reward, single-shot alpha
- **Crown:** Pressure Cooker
- **Supports:** The Glob (overcook tech), Heavy arc, Instant Det, Gunpowder, switch-in amp, Funnel execute
- **Hybrid hooks:** Cooker+Recipe nuke; Cooker+Flood fat wave; mag-1 self-acid

### Path B — STORM
**“Never stop spilling.”**

- **Spine:** RoF, mini size, multi-glob, replication / mag economy, bounce volume
- **Crown:** Storm Vat
- **Supports:** Triagon DNA, Forceful Expulsion, Auto/Improved/Pocket Replication, Volatile size chaos, Balloons/puddle hybrid
- **Hybrid hooks:** Storm+Recipe grenade SMG; Storm+Flood carpet waves

### Path C — RECIPE
**“The gun is a magazine for your grenade.”**

- **Spine:** throwable identity, siphon potency, loadout synergy, element flexibility
- **Crown:** Recipe Loader
- **Supports:** Siphon rewrite, element/volatile toys, throw-force handling, anti-self lining when firing nasty nades
- **Hybrid hooks:** any grenade mod’s fantasy becomes primary fire; Flood carries grenade element

### Flood cluster (non-path)
Flood exotic + Flash Flood rewrite + trail/puddle supports + wave damage. Attracts field players without locking a path.

### Path × verb matrix

```
                 COOKER              STORM               RECIPE              FLOOD
Alpha            core                weak per shot       grenade alpha       wave tick
Clear            charged splash      core                mag-fed nades       carpet
Loadout syn      optional            optional            core                element follow
Self-acid tech   overcook / Glob     low                 depends on nade     low
Meter            dmg + cook eff      dmg + micro restore dmg + potency       dmg + length
```

---

## 8. Damage & Economy Rules (LOCKED)

### 8.1 Vanilla failure mode
Effective power ≈ Pressure Cooker + meter stack + siphon size. Baseline damage poor. Non-cook builds feel like traps.

### 8.2 Spillway rules
1. **Raise baseline damage** so empty-grid Spillway is a real primary.
2. **Globblometer → damage** is the universal grid-fill reward.
3. **Cooker peak** is high but not the only ST path (Recipe heavy nades + Funnel also ST).
4. **Storm+Recipe** must pay taxes (size, RoF ceiling, or damage per grenade) so mag-fed full Incendiaries do not obsolete throwables entirely — throwable cooldown still matters for *throw* utility; gun is a different verb.
5. **Distributed verbs:** size, bounce, RoF, reload, acid appl, puddle, wave, self-resist — not only +% damage rares.
6. **No pure meter cards.**

### 8.3 Budget sketch (playtest dials)

| Lever | Starting intent |
|-------|-----------------|
| Baseline damage vs vanilla Globbler | +25–40% empty-grid feel (tune) |
| Full meter damage | +80–120% via meter coeff |
| Cooker damage per cooked ammo | Competitive with vanilla spirit but not mandatory |
| Storm per-shot | ~40–60% of baseline glob after mini tax; RoF makes up clear |
| Storm+Recipe per grenade | Below a manual throw’s full value; volume is the trade |
| Siphon max size | Strong next-shot button; not infinite cook |

---

## 9. Full Upgrade List (~30 ship + backlog)

Rarity: Standard / Rare / Epic / Exotic / Oddity  
Tags: A Cooker · B Storm · C Recipe · F Flood · G Glue · M Meter-rider (all)  
Cell rule: Exotics larger; all Exotics same cell count.  
Names are player-facing (full rename). Every card includes the **universal meter rider**.

------------------------------------------------------------------------------
PATH A — COOKER                                          [~7]
------------------------------------------------------------------------------

A1. Pressure Cooker — Exotic (Keystone)
    Hold fire to spend magazine ammo into a charged payload. Size and damage
    scale with ammo cooked. Cooking the entire magazine detonates on you.
    Soft-combos with Storm (faster/weaker cooks) and Recipe (charged full grenade).

A2. Single Chamber — Rare (The Glob rewrite)
    Magazine size becomes 1. Explosion size up, reload up.
    With Pressure Cooker: cannot multi-stack cook ammo; holding fire enables
    deliberate overcook self-acid. Not a bug — tech.

A3. Heavy Lob — Rare (Heavy Globs rewrite)
    +Damage, +bullet gravity (lobbier arc). Works on globs and Recipe arcs where applicable.

A4. Contact Fuse — Rare (Instant Detonation rewrite)
    Detonate on first surface/enemy contact (0 bounce). Snappier Cooker and Storm hits.

A5. Black Powder — Rare (Gunpowder Packing rewrite)
    +Projectile speed/force, +recoil. Flatter cook lines.

A6. First Spill — Rare (Globulous Excitement rewrite)
    First shot after equip deals bonus damage (meter can mildly scale). Swap-in poke.

A7. Cooker’s Gauge — Epic
    While Pressure Cooker is equipped: +charge efficiency (less time or more size
    per ammo). Mild effect without Cooker (small +explosion size only).

------------------------------------------------------------------------------
PATH B — STORM                                           [~7]
------------------------------------------------------------------------------

B1. Storm Vat — Exotic (Keystone) [NEW]
    Rapid mini payloads. +Fire rate, −per-shot size/damage. Optional multi-pellet.
    Soft with Cooker. With Recipe: mag-fed mini/full grenades under Storm taxes.

B2. Tri-Spill — Epic (Triagon rewrite)
    Fire three payloads in a triangle. −Fire rate, −explosion size per pellet.
    Natural Storm pair; valid without Storm as a spread card.

B3. Force Jet — Rare (Forceful Expulsion rewrite)
    +Fire rate, +projectile speed, −damage, −explosion size. Storm glue.

B4. Pocket Overflow — Epic (Pocket Replication rewrite)
    Ammo refunded into Spillway from dealing damage with your other weapon can
    overflow the magazine.

B5. Wide Reservoir — Rare (Improved Replication rewrite)
    Magazine size increased (vanilla used globblometer; Spillway: flat/mag curve
    + universal meter already buffs damage — do not double-dip meter as mag only).

B6. Turbine Feed — Rare (Auto-Replicator rewrite)
    +Fire rate economy / handling. (Vanilla was +meter −RoF — inverted identity:
    Storm wants RoF; meter comes from universal rider.)

B7. Chaotic Gauge — Rare (Volatile Acid rewrite)
    Payloads fire with randomized size. Max size scales slightly with meter
    (secondary). Fun Storm chaos.

------------------------------------------------------------------------------
PATH C — RECIPE                                          [~6]
------------------------------------------------------------------------------

C1. Recipe Loader — Exotic (Keystone) [NEW — DiscWorld]
    Shots fire your equipped grenade as a full projectile. Gun keeps handling
    (RoF, reload, mag, spread, recoil). Does not spend grenade cooldown per shot.
    No grenade equipped → fallback acid glob.

C2. Globulous Siphon — Exotic (rewrite — not Cooker-tied)
    Hold RELOAD to consume grenade charge, empowering your next payload’s
    explosion size and effect/potency. Works with all fire modes.

C3. Soft Lining — Rare
    Reduce self-damage and self-element application from your own Spillway
    payloads (important for Recipe nasty nades and Cooker overcook).

C4. Throw Weight — Rare
    +Projectile speed / −gravity for payloads (helps fat Recipe arcs and globs).

C5. Cross-Load — Epic
    While Recipe Loader is equipped, a small chance on kill to refund gun ammo.
    Negligible without Recipe (or tiny universal ammo crumb — prefer Recipe-gated).

C6. Primer Coat — Standard
    +Element application amount on payloads (acid baseline; follows Recipe element).

------------------------------------------------------------------------------
FLOOD CLUSTER (non-path)                                 [~4]
------------------------------------------------------------------------------

F1. Flood — Exotic (NON-PATH)
    Payloads that hit a surface create a running wave. Baked: strong base length.
    Trail puddles unlocked or partially included — prefer base wave + trail on
    exotic or split trail to F2 if power high.

F2. Flash Flood — Rare (rewrite)
    Waves travel farther. (If trail is on F1, this is pure length/damage.)

F3. Slick Trail — Epic
    Waves leave lingering acid (or payload-element) trails that apply corrosion
    on contact. (Turbo Flood DNA.) If merged into F1, demote this to backlog.

F4. Wavefront — Rare
    Wave ticks deal increased damage / effect amount.

------------------------------------------------------------------------------
GLUE / GUNFEEL / SURVIVAL / FUNNEL                       [~8]
------------------------------------------------------------------------------

G1. Impact Funnel — Exotic
    Damage taken builds a bank that multiplies payload damage. Firing spends the
    bank. Self-damage can feed the bank. Meter mildly improves efficiency.

G2. Replicator Carapace — Epic (Replicator Resistance rewrite)
    Damaging yourself with a Spillway payload grants damage resistance for a
    short time. Duration scales lightly with meter (secondary).

G3. Balloon Reservoir — Epic (Glob Balloons rewrite)
    Payloads deal less burst but leave acid puddles (duration/size). Field glue;
    pairs with Flood trails and Recipe acid flasks.

G4. Volatile Cocktail — Epic (Volatile Element rewrite)
    Chance for payload to infuse Fire or Shock at full apply. With Recipe, can
    roll a non-grenade bonus element occasionally (keep readable).

G5. Slipstream — Epic (Slipper Shoes rewrite)
    While Spillway is active, move faster. (Vanilla scaled only with meter;
    Spillway: flat movespeed + small meter crumb — meter already = damage.)

G6. Mag Extender — Standard
    +Magazine size.

G7. Quick Breach — Standard
    +Reload speed.

G8. Solvent Press — Standard [NEW — distributed damage]
    Modest +damage. Honest staple so grid fill is not only meter.

G9. Wide Bore — Standard
    +Explosion size.

G10. Stable Grip — Standard
    −Recoil.

G11. Boundary Incursion — Oddity
    +Upgrade grid size.

------------------------------------------------------------------------------
FROZEN v1 SHIP POOL (exactly 30)
------------------------------------------------------------------------------

EXOTIC (6)
  1  Pressure Cooker          (A)
  2  Storm Vat                (B)
  3  Recipe Loader            (C)
  4  Flood                    (F, non-path)
  5  Globulous Siphon         (C / free)
  6  Impact Funnel            (G)

EPIC (8)
  7  Cooker’s Gauge           (A)
  8  Tri-Spill                (B)
  9  Pocket Overflow          (B)
  10 Cross-Load               (C)
  11 Slick Trail              (F)   — if trail baked into Flood exotic, swap for Balloon Reservoir
  12 Balloon Reservoir        (G)
  13 Replicator Carapace      (G)
  14 Volatile Cocktail        (G)

RARE (10)
  15 Single Chamber           (A)   — The Glob / overcook tech
  16 Heavy Lob                (A)
  17 Contact Fuse             (A)
  18 Black Powder             (A)
  19 Force Jet                (B)
  20 Wide Reservoir           (B)
  21 Turbine Feed             (B)
  22 Chaotic Gauge            (B)
  23 Soft Lining              (C)
  24 Flash Flood              (F)

STANDARD (5)
  25 Primer Coat              (C)
  26 Mag Extender             (G)
  27 Quick Breach             (G)
  28 Solvent Press            (G)
  29 Wide Bore                (G)

ODDITY (1)
  30 Boundary Incursion       (G)

BACKLOG (designed, expand later)
  First Spill, Throw Weight, Wavefront, Slipstream, Stable Grip,
  Cooker+Storm named hybrid epic, Recipe ammo economy variants,
  Multiversal Thievery / Edge Fault (contraband parity only if desired),
  Full Spectrum spatial rarity toy, Pack Eater / Shrunken Container as
  intentional meter-dense downside cards (only if they gain a real verb —
  not pure meter), ally-acid aura, deep Caustic Flask keyword matrix beyond
  full-projectile Recipe.

------------------------------------------------------------------------------
CUT / DEMOTE FROM VANILLA IDENTITY
------------------------------------------------------------------------------

| Vanilla | Fate |
|---------|------|
| Globblin / pure +globblometer | **CUT** — universal meter rider on all cards |
| Auto-Replicator (+meter −RoF) | **Turbine Feed** — RoF/economy; meter via rider |
| Improved Replication (mag from meter) | **Wide Reservoir** — real mag card + rider |
| Shrunken Container (−mag +meter) | Cut or backlog as real downside verb only |
| Pack Eater | Cut / backlog |
| Pressure Cooker as only build | **Peer crown** among three + Flood |
| Globulous Siphon as size-for-cook | **Next-payload empower**, all modes |
| Flood lonely exotic | **Non-path** + trail supports |
| (none) Recipe | **Recipe Loader** DiscWorld full grenade |
| (none) Storm crown | **Storm Vat** |
| Baseline weak damage | **Raised baseline + meter→damage** |
| The Glob vs Cooker “broken” | **Documented overcook tech** |
| Full Spectrum | Backlog spatial toy |
| Glorb (−reload +meter) | Cut; reload lives on Quick Breach |

---

## 10. Example Builds

### Pure Cooker (ST)
Solvent Press → Wide Bore → Heavy Lob → Contact Fuse → Cooker’s Gauge → **Pressure Cooker** → Impact Funnel  
*Play:* Hold, dump mag into one shot, Funnel spikes execute. Meter from full grid is the damage floor.

### Mag-1 Overcook Chemist
**Single Chamber** + **Pressure Cooker** + Soft Lining + Replicator Carapace + Primer Coat  
*Play:* Mag 1; hold to self-detonate / self-acid on purpose; resist window; Funnel optional. Documented tech.

### Pure Storm (clear)
**Storm Vat** → Force Jet → Tri-Spill → Wide Reservoir → Pocket Overflow → Turbine Feed → Solvent Press  
*Play:* Hose micros, bounce rooms, never touch Cooker.

### Seeking… wait, Flood carpet
**Flood** → Slick Trail → Flash Flood → Balloon Reservoir → Wide Bore → Storm Vat  
*Play:* Rapid impacts paint waves and trails; field denial clear.

### Recipe Incendiary Primary
**Recipe Loader** + equipped Incendiary Grenade + Globulous Siphon + Soft Lining + Mag Extender + Cross-Load  
*Play:* Mag-fed full fire nades; siphon fattens next boom; throw still available on grenade key for utility.

### Recipe + Cooker nuke
**Recipe Loader** + **Pressure Cooker** + Siphon + Heavy Lob + Funnel  
*Play:* Cook gun ammo, release one enormous full grenade payload.

### Storm + Recipe (budget carefully)
**Storm Vat** + **Recipe Loader** + Force Jet + Soft Lining + Turbine Feed  
*Play:* Grenade SMG — watch taxes in playtest; should feel strong, not free infinite throws.

### Hybrid poster
Pressure Cooker + Flood + Recipe Loader + Solvent Press + Slick Trail  
*Play:* Cook a Recipe grenade into a wave-painting apocalypse. Soft Storm optional.

---

## 11. Strengths, Weaknesses & Risks

### Strengths
- Three real builds + Flood mixes
- Meter always means something (damage)
- Recipe makes grenade loadout identity matter on a primary
- Overcook self-acid is a readable high-skill / meme tech
- Empty grid is playable

### Weaknesses / fun failure states
- No Recipe + weak throwable slot = Path C sad (fallback globs still OK)
- Cooker whiff (full cook into empty air)
- Storm without meter/staples feels piddly (by design until grid fills)
- Self-cook without Carapace / Soft Lining hurts
- Flood on open vertical geometry may wave awkwardly (vanilla risk)

### Design risks
- **Storm + Recipe power** — mag-fed full grenades; needs taxes and playtest
- **Meter stacking ceiling** with Solvent Press + path damage + Cooker + Funnel
- **Network/Recipe** — firing full grenade bullets from gun fire path (DiscWorld is the template)
- **Element VFX** on Flood waves when Recipe element ≠ Acid
- **Siphon + Recipe** charge economy vs grenade throw uptime
- Soft Cooker+Storm readability in HUD (charge bar + fast RoF)

---

## 12. Success Criteria / Player Fantasy Checklist

- [ ] Empty-grid Spillway clears packs without Pressure Cooker
- [ ] Baseline damage no longer feels “horrible”
- [ ] Filling the grid raises damage via Globblometer even with “non-damage” verbs
- [ ] No upgrade exists only to add Globblometer
- [ ] Pure Storm builds compete for clear without Cooker
- [ ] Pure Cooker still delivers mythic alpha
- [ ] Recipe Loader fires **full** grenades (DiscWorld feel), not tinted globs
- [ ] Recipe does not spend grenade CD per gun shot
- [ ] Flood is exciting with Cooker, Storm, and Recipe
- [ ] Flood trail puddles read clearly
- [ ] Siphon empowers next shot in all modes; not marketed as Cooker support
- [ ] Single Chamber + Cooker overcook self-acid is documented and fun with Carapace
- [ ] Soft Cooker+Storm hybrids work without disabling either crown
- [ ] Vanilla Globbler still exists untouched
- [ ] Co-op: waves/puddles help the team; Recipe respects friendly-fire rules of the grenade

---

## 13. Universal Truths (Mycopunk alignment)

- Exotic shapes should always be larger than others; each exotic should use the same number of cells.
- v1 targets **~30** upgrades (frozen list above); backlog is real design, not trash.
- Three paths create different build options but **may intermingle**; Flood is a free exotic attractor.
- Full rename for rework identity (vanilla names only in fate table).
- Prefer cook / storm / recipe / flood verbs over generic +% only — but Solvent Press exists so damage is honest.
- Primary element identity: **Acid** at baseline; Recipe replaces payload identity.
- Parallel product: **Spillway**; vanilla **Globbler** unmodified.

---

## 14. Vanilla Globbler → Spillway Fate Table

| Vanilla name | Spillway name | Path | Notes |
|--------------|---------------|------|-------|
| (baseline weak dmg) | Raised baseline | — | Empty grid playable |
| (globblometer hollow) | Meter → damage + universal rider | — | No pure meter cards |
| Pressure Cooker | Pressure Cooker | A | Peer crown; soft with Storm |
| The Glob | Single Chamber | A | Overcook tech with Cooker |
| Heavy Globs | Heavy Lob | A | |
| Instant Detonation | Contact Fuse | A | |
| Gunpowder Packing | Black Powder | A | |
| Globulous Excitement | First Spill | backlog | |
| (none) | Storm Vat | B | New crown |
| Triagon | Tri-Spill | B | |
| Forceful Expulsion | Force Jet | B | |
| Pocket Replication | Pocket Overflow | B | |
| Improved Replication | Wide Reservoir | B | Mag card; not meter-only |
| Auto-Replicator | Turbine Feed | B | RoF identity flip |
| Volatile Acid | Chaotic Gauge | B | |
| (none) | Recipe Loader | C | DiscWorld full grenade |
| Globulous Siphon | Globulous Siphon | C | Next-payload; not Cooker battery |
| (none) | Soft Lining | C | Self-damage reduction |
| Flood | Flood | F | Non-path; +trail DNA |
| Flash Flood | Flash Flood | F | |
| (turbo trail) | Slick Trail | F | |
| Impact Funnel | Impact Funnel | G | |
| Replicator Resistance | Replicator Carapace | G | |
| Glob Balloons | Balloon Reservoir | G | |
| Volatile Element | Volatile Cocktail | G | |
| Slipper Shoes | Slipstream | backlog | |
| Globblin | **CUT** | — | Universal rider |
| Glorb | **CUT** | — | |
| Shrunken Container | **CUT** / backlog | — | |
| Pack Eater | **CUT** / backlog | — | |
| Full Spectrum | backlog | — | |
| Boundary Incursion | Boundary Incursion | G | Keep name |
| Multiversal Thievery / Edge Fault | optional parity | — | Not in frozen 30 |
| (none) | Solvent Press | G | Distributed damage |
| (none) | Mag Extender / Quick Breach / Wide Bore | G | Staples |

---

## 15. Implementation Notes (for later coding passes)

### Product / registration
- New primary via weapon template: clone **Globbler** (not CartridgeSMG)
- Unique gear id + APIName e.g. `spillway`
- Display name **Spillway**
- `PlayerData.CreateUpgrade` pool; SpawnGear remap + stamp identity + ApplyUpgrades
- `[MycoMod(..., ModFlags.IsSandbox)]`
- Do **not** remove vanilla Globbler from AllGear

### Host behaviour
- `SpillwayBehaviour` (or subclass when prefab exists) holding:
  - globblometer (computed from equipped upgrades / rarity riders)
  - meterDamageCoeff
  - cooker / storm / recipe / flood / siphon / funnel flags and stats
  - soft-combo modifiers
  - default explosion size cache (vanilla `DefaultExplosionSize`)
- Prefer extending live `Globbler` / `GlobblerBullet` via Harmony + behaviour when cloning; ideal long-term: `Spillway : Globbler` prefab

### Globblometer
- On upgrades applied: sum universal rider from each equipped upgrade rarity
- Apply damage mult in `ModifyBulletData` and/or `OnBeforeDamage`
- Remove dependency on pure `IGlobblometerProperty` filler cards
- Keep `MaxGlobblometer = 50` unless raising cap

### Hooks

| Area | Approach |
|------|----------|
| Baseline damage | Raise catalog `GunData.damage` on Spillway prefab/clone |
| Meter damage | `ModifyBulletData` / damage callback × (1 + coeff * normalized) |
| Pressure Cooker | Port vanilla charge loop (`OnActiveUpdate` ammo eat, `ModifyBulletData` size/dmg, self-explode) |
| Storm Vat | fireInterval, force/damage mult, optional bulletsPerShot; soft cooker mults |
| Recipe Loader | **DiscWorld pattern**: on fire, suppress glob bullet; `FireBulletCustom` from equipped `ThrowableGear` / grenade with grenade `GunData` and upgrade flags; gun spends mag only |
| Flood | Set `GlobblerUpgradeFlags.Wave` + waveLength; trail = spawn puddles along wave march in `GlobblerBullet.OnFuseActive` |
| Siphon | Port `OverrideHoldReload` + charge eat from throwable `IActivatedAbility`; apply size+potency on next `ModifyBulletData` |
| Funnel | Port taken-damage counter + spend on fire |
| Single Chamber | magazineSize = 1; document overcook with cooker |
| Network | Recipe shots need observer RPCs appropriate to grenade bullet type; Flood/Cooker already have Globbler RPC patterns |

### Vanilla Globbler.Data fields (reference)

```
globblometer, extraMagSizeFromDamage, switchDamage, globblometerSpeed,
randElementChance, chargeTimePerAmmo, sizePerChargedAmmo, damagePerChargedAmmo,
acidPuddleSize, acidPuddleDuration, selfDamageResist, selfDamageResistDuration,
sizeWeightMax, waveLength, grenadeChargePerSecond, grenadeSizePerCharge,
grenadeSizeMax, damagePerTakenDamage, maxDamageMult
GlobblometerNormalized = globblometer / 50
```

### Vanilla flags
```
GlobblerUpgradeFlags.Wave = 1
Temp1/2/3 unused — free for Storm / Recipe / Trail if needed
```

### Recipe Loader detail (DiscWorld)
- Read `player.ThrowableGear` (grenade / disc / etc.)
- Exclude continuous / invalid (Plate Launcher spirit if ever relevant)
- For each gun shot index: fire grenade bullet with grenade stats
- Strip any “from gun nerf” flags if grenade type has them (DiscWorld clears `FromGun` on Photon Disc)
- Damage source: prefer gun as `IDamageSource` parent for XP/kill rules consistency — validate in playtest
- Globblometer damage mult applies on top of grenade damage (gun-side), or as a controlled percentage — **prefer applying meter as a mult on final payload damage** so grid fill always matters

### Soft combo implementation sketch
```
if (cooker && storm) {
  sizePerChargedAmmo *= 0.7;
  damagePerChargedAmmo *= 0.75;
  // storm RoF partially retained
}
if (storm && recipe) {
  // per-grenade damage *= 0.55–0.7; keep RoF high but capped
}
```

### HUD
- Reuse / clone GlobblerHUD: charge bar, siphon text, funnel mult text
- Recipe: optional icon tint = grenade element
- Storm: optional rapid-fire audio RTPC

### Related mods / DNA (not required at runtime)

| Source | DNA |
|--------|-----|
| DiscWorldRework | Full throwable projectile from gun fire; handling stays on gun |
| SparrohsTurbocharges | Flood trail + length; Siphon cost reduction ideas |
| Caustic Flask design | Acid puddle / armor language for future synergy; no hard dep |
| DMLR / Aussie / Cycler docs | Structure, frozen 30, fate tables, soft vs hard crowns |
| Vanilla Globbler | Charge, wave, siphon, funnel, puddle, HUD |

---

## 16. Open Tuning Questions (playtest, not design blockers)

1. MeterDamageCoeff exact value (0.8 vs 1.0 vs 1.2 at full 50).
2. Baseline damage raise percent vs vanilla Globbler.
3. Storm mini damage/size fractions.
4. Storm+Recipe per-grenade tax (0.55–0.75?).
5. Whether Flood exotic includes Slick Trail baseline or split to epic.
6. Siphon max potency vs manual grenade throw value.
7. Funnel max mult and drain per shot under Storm.
8. Soft Cooker+Storm numeric mults.
9. Recipe meter mult: full mult on grenade damage or partial (e.g. 50% of meter coeff) to avoid double-dipping fat nades.
10. Single Chamber overcook: should hold-R siphon still work at mag 1? (Yes — separate system.)

---

## 17. Locked Decisions Log

| Decision | Lock |
|----------|------|
| Ship name | **Spillway** |
| Product shape | **Parallel primary**; vanilla Globbler untouched |
| Flood | **Non-path exotic** |
| Cooker ↔ Storm | **Soft** hybrids |
| Recipe Loader | **Full equipped grenade** (DiscWorld), not element tint |
| Globulous Siphon | **Not** Cooker support; next-payload empower |
| The Glob + Cooker | **Intentional overcook / self-acid tech** |
| Globblometer | **Every upgrade contributes**; **no pure meter cards** |
| Meter spine | **Damage** |
| Doc scope | Full frozen 30 + fate + impl |
| External deps | Pattern copy only; no hard runtime deps |
| Rename | Full player-facing rename |

### Design changelog

#### v1 (this doc)
- Spillway identity; parallel product
- Three paths (Cooker / Storm / Recipe) + Flood free exotic
- Universal meter rider; meter → damage; raised baseline
- Recipe Loader DiscWorld full grenade rules
- Soft crown matrix; Single Chamber overcook tech
- Siphon decoupled from Cooker
- Frozen 30 + backlog + fate table + impl notes

---

## 18. Next Steps After This Doc

1. Review frozen 30 vs backlog (especially Slick Trail vs Flood merge, First Spill cut).
2. Confirm Recipe damage attribution + meter mult partial vs full.
3. Implement Spillway clone registration from Globbler.
4. Implement universal meter → damage.
5. Port Cooker / Siphon / Funnel / Flood with Spillway behaviour flags.
6. Implement Storm Vat soft combo modifiers.
7. Implement Recipe Loader fire override (DiscWorld-style).
8. Register frozen pool; icons/strings.
9. Balance pass: empty grid, pure Storm, pure Cooker, Recipe SMG, overcook chemist, Flood carpet.
10. Optional: turbocharge parity notes if SparrohsTurbocharges should ignore Spillway or gain hooks later.

---

*End Spillway Design Doc v1*
