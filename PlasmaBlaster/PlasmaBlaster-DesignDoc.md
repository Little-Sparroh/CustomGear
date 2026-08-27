# Plasma Blaster — Design Document (v1)

> Status: **Design only** — no implementation yet.
> Project folder: `.new.PlasmaRifle`
> Template base: weapon content template (CartridgeSMG clone path until custom art).
> Display name: **Plasma Blaster**
> APIName (draft): `plasma_blaster`

---

## 1. High Concept / Fantasy

A mid-range SAXON **plasma blaster** — industrial stormtrooper energy rifle energy without the joke accuracy stat line. Hold the trigger: deliberate full-auto bolts, each one a bright plasma slug that **applies Decay** on contact. No free splash. No free shove. The gun teaches aim, mag discipline, and rot literacy first.

Upgrades fork three peer fantasies:

  Bloom     — impact ray-splash and splash geometry (the “bolts bloom” fantasy, earned)
  Fallout   — **Isotope Tip** transforms bolts into sticky radioactive blobs that cook auras
  Ion       — charged white lance / capacitor execute plays

One-liner: Hold the trigger. Paint them with rot. Upgrade the bolt into a star, a tumor, or a white lance.

Product shape: New primary weapon (**Plasma Blaster**). Does not replace Cycler, Hard-Light Constructor,
DMLR, Laser Cannon, Globbler, Plate Launcher, or any vanilla gun.

SAXON marketing blurb (draft):
  “SAXON PB-11 Plasma Blaster — Standard-issue cellular denaturant projector for corridor
  and ramp work. Bolts apply Decay on contact. Splash authorization requires Form 12-B.
  Sticky isotope tips are not standard issue. (They are extremely popular.)”

Optional stingers:
  - “If it glowed and stuck, you equipped the wrong tip — or the right one.”
  - “Ray-splash is a privilege, not a default.”
  - “Low cyclic rate is a feature. Panic is a skill issue.”
  - “Half-life is not a suggestion. It is a schedule.”

---

## 2. Role & Fantasy in the Arsenal

- Slot: Primary
- Range: Mid (readable projectile travel; light falloff OK)
- Role: Decay-literate blaster — focus fire ST, path-owned clear toys
- Loop: Track → bolt stream → Decay saturates → reload beat → path payoffs
- Gap filled:
  - Cycler = fast energy SMG hose + optional Condensed Ejection slow plasma — not low-RPM blaster + Decay spine
  - Hard-Light Constructor = chunky plasma → Shatter Jam → paint architecture — control fabricator, not rot blaster
  - DMLR / Laser Cannon = beam / dual-mode laser anatomy — not bolt auto
  - Globbler = bouncy acid globs / puddles / flood — plumber, not rifle
  - Plate Launcher = stick metal + magnetic recall — scrap physics, not fallout aura
  - Needle Carbine = Poison needles + supercombine + Extract — medical stick economy
  - Gunship Cannon = explosive rotary shells — boom MG
  - Splash Canister = Water catalyst throwable — not a primary
  - Nothing owns “low-RPM plasma blaster → baseline Decay → opt-in ray-splash / sticky isotope blobs / ion lance”

- Synergies: Allies dump into Decay-amped targets; Fallout auras are co-op cook zones; Bloom packs peel for team focus

Not trying to be: Cycler 2.0, HLC fabricator, Globbler rifle, Plate stick/recall, Needle supercombine,
Gunship boom, free-splash room delete, or hitscan laser.

### 2.1 Comparison snapshot

```
Weapon / kit              Niche                         Plasma Blaster differentiator
------------------------  ----------------------------  ------------------------------------------
Cycler                    Energy SMG hose               Low RPM blaster; Decay spine; splash path-owned
Hard-Light Constructor    Plasma → Jam → paint          Decay damage amp, not Shatter lock; no architecture
DMLR / Laser Cannon       Beam / anatomy execute        Bolt auto, not beam
Globbler                  Bounce acid plumber           Rifle bolts; stick only via Isotope Tip transform
Plate Launcher            Stick + magnetic recall       Blobs cook in place; no recall identity
Needle Carbine            Poison needles + extract      Decay not Poison; no supercombine/Extract
Gunship Cannon            Explosive rotary              Kinetic-feel energy bolts; splash not free shells
Accelerator               Shock bursts + mobility       Sustained auto Decay, not sprint burst kit
Zephyr                    Sonic cone knockback          No baseline force; not pressure cone
```

---

## 3. Design Pillars

1. On-hit status and path verbs > flat % damage stickers.
2. **Baseline Decay is sacred** — real vanilla Decay apply on every bolt; no fake tracker forever.
3. **No baseline splash** — ray-splash / bloom is Path A (and cards that explicitly enable bloom).
4. **No baseline hitforce** — knockback/shove is upgrade-owned only if a card says so (default: none).
5. **Full-auto, lower RPM** — blaster cadence, not Cycler hose and not semi-tap rifle.
6. Three peer paths (Bloom / Fallout / Ion); hybrids intended; no anti-synergy matrix.
7. **Isotope Tip is a full weapon transform** — equipped → bolts become sticky fallout blobs (not RMB alt, not every-Nth).
8. RMB stays free on baseline; Ion / Purge may claim it via priority table.
9. R = reload only — do not overload reload on baseline.
10. ~30 upgrades for v1 ship; exotic shapes larger than others; each exotic same cell count.
11. Fallout must not become Plate recall or Needle supercombine.
12. Industrial SAXON plasma tone (cyan/white bolt, sickly green fallout) — not magic space wizard.

---

## 4. Core Mechanics & Gunfeel

### 4.1 Base gun

| Trait        | Draft / intent                                                         |
|--------------|------------------------------------------------------------------------|
| Fire mode    | **Full-auto**, low–mid cyclic rate                                     |
| Damage       | Mid per bolt — ST from focus + Decay amp, not splash volume            |
| Range        | Mid; projectile travel readable; light falloff OK                      |
| Mag/reserve  | Medium mag; reload is a real beat                                      |
| Projectile   | Glowing plasma bolt (faster than HLC slabs; fatter/slower than Cycler) |
| Element      | **Decay** on hit                                                       |
| Splash       | **None**                                                               |
| Hitforce     | **None / negligible**                                                  |
| ADS / RMB    | ADS optional tightener; **RMB unbound** on baseline                    |
| Model/audio  | Blaster rifle chassis; bolt hiss/crack; Decay crackle on apply         |

Draft firefeel band (VALIDATE IN PLAYTEST):

| Param              | Draft                                      |
|--------------------|--------------------------------------------|
| RoF mental target  | ~5–8 rps (well below Cycler ~11+ hose feel)|
| Fire interval      | ~0.12–0.20 s                               |
| Mag                | ~22–28                                     |
| Reserves           | Hungry relative to mag                     |
| Bullet speed       | Readable travel (not hitscan; not molasses)|
| Per-bolt damage    | Mid                                        |
| hitForce           | 0                                          |
| Decay effectAmount | ~0.8–1.5 per bolt (playtest to full-sat feel) |
| Reload             | ~1.4–1.8 s                                 |

### 4.2 Inputs

| Input     | Baseline role                         | Upgraded claims                                      |
|-----------|---------------------------------------|------------------------------------------------------|
| Hold M1   | Full-auto plasma bolts + Decay        | Prism multi-bolt, blob transform, bloom on impact    |
| RMB       | **Unbound**                           | Ion Lance charge / Fallout Purge detonate (priority) |
| R         | Reload only                           | Reload only                                          |
| Heavy     | Normal heavy equip                    | No baseline heavy link                               |

### 4.3 Decay — baseline spine

Use vanilla **Decay** status (do not invent Radiation EffectType for v1).

| Rule | Intent |
|------|--------|
| Every bolt applies Decay effectAmount on direct hit | Teaches rot identity immediately |
| Full saturation uses vanilla Decay rules (damage amp / tech behavior as game defines) | No parallel status system |
| Fallout auras refresh/apply Decay in radius | Path B deepens the same spine |
| Rot | Not baseline. At most one rare owns Rot if needed later — do not double with Decay spam |
| Bloom splash Decay | Only when a bloom source exists; splash may apply reduced Decay amount |

TUNING CAUTION:
  Decay amp can snowball with multi-bolt (Prism) and aura ticks (Isotope). Prefer:
  - Modest per-bolt amount on baseline
  - Reduced amount on splash/aura ticks vs direct
  - Soft caps / ICD on aura re-apply
  - Prism shares Decay budget across pellets (not full amount × N)

### 4.4 Baseline combat loop (zero upgrades)

```
M1 hold → low-RPM plasma bolts → direct hit damage + Decay apply
   → focus fire priority targets / parts
   → R when dry
   → RMB does nothing
   → no splash, no shove, no blob, no charge
```

Skill without upgrades: tracking at blaster RPM, mag husbandry, aiming what matters under Decay,
not brain-off spraying half-mags into empty air.

### 4.5 What baseline does NOT include

- No impact ray-splash / bloom
- No hitforce / knockback
- No sticky blobs
- No ricochet
- No multi-bolt scatter
- No ion charge shot
- No lingering ground plasma
- No heat / overheat brick
- No RMB power
- No Rot (Decay only)

Those are path-, exotic-, or unlock-owned.

---

## 5. Shared Framework Vocabulary

Upgrades speak these verbs. Baseline owns **Bolt + Decay** only.

### 5.1 Bolt
- Primary projectile
- Direct hit damage + Decay apply
- All paths deliver through Bolt unless a card transforms or replaces the projectile

### 5.2 Bloom / Splash (Path A–owned enable)
- On-impact AoE at hit point (enemy or terrain, per card)
- **Not free** — requires a bloom source (exotic/epic/rare that enables splash)
- Scales: radius, damage fraction of bolt, shape, chain, wall redirect
- May apply reduced Decay in radius when enabled

### 5.3 Fallout Blob (Path B–owned)
- Sticky projectile/payload created by **Isotope Tip** transform
- Attaches to hit part/brain; ticks AoE damage + Decay around host
- Duration, pulse curve (Half-Life), max concurrent, purge/detonate
- Death/expiry may leave short slick (upgrade-owned)

### 5.4 Ion (Path C–owned)
- Charged execute bolt (RMB hold-to-charge recommended)
- Pierce, shield/core bias, stun hitch, capacitor mag dump
- Splash on ion only if Bloom sources also equipped (hybrid), unless card says otherwise

### 5.5 Overcharge (soft / optional)
- Mag- or time-built resource for bigger blooms or faster ion charge
- Not a baseline heat brick — zero Overcharge cards = no meter gameplay

### 5.6 Afterimage / Ghost (soft)
- Delayed echo bolt or echo bloom
- Hybrid glue; do not become a fourth full path

---

## 6. Upgrade Paths (gravity wells — hybrids intended)

### Path A — BLOOM (ray-splash geometry)
“Every upgraded hit paints a star.”

- Spine: enable splash on impact, radius, damage share, wall redirect, kill-cascades, ricochet-then-bloom, multi-bolt fan
- Clear vs ST: Clear native once bloom is online; ST via focus + Decay still works without splash
- Hybrid hooks: blobs inherit bloom radius; ion impacts bloom when sources equipped

### Path B — FALLOUT (sticky isotope cooker)
“Don’t pull it out. Let it cook the room.”

- Spine: **Isotope Tip transform**, aura ticks, Half-Life age curve, max blobs, purge, death slick, cascade
- Clear vs ST: Clear via multi-blob auras; ST via stacking cook on elites/boss parts
- Hybrid hooks: Bloom radius cards enlarge aura; Ion purge/detonate moments

### Path C — ION (charge / disrupt / execute)
“Hold for the white bolt.”

- Spine: RMB charge lance, pierce, shield/core bias, capacitor breach, stun hitch, charge economy
- Clear vs ST: ST native; clear via charged splash hybrids or pack pierce
- Hybrid hooks: charged bolt through a Fallout-tagged pack; Bloom on lance impact

### Path × verb matrix

```
                 BLOOM                 FALLOUT                ION
Bolt             multi / splash enable transform to blob      charge lance variant
Decay            reduced on splash     aura refresh spine     optional on lance
Splash           core fantasy          aura is sticky splash  hybrid only
Transform        Prism fan             Isotope Tip keystone   Capacitor dump
RMB              optional purge hybrid Purge detonate         Ion charge (primary claim)
```

---

## 7. Crowns & Sacred Cows

### Prism Scatter — Exotic (Bloom crown)
- Each trigger pull fires a **fan of bolts** (draft: 3) in a tight cone/spread pattern.
- **Shared budget:** total direct damage and total Decay application across the fan ≈ 1.0–1.15× a single baseline bolt’s worth (playtest), not 3× full bolts.
- If any Bloom splash source is equipped, splash may proc once per trigger (primary impact) or per bolt at heavily reduced radius — prefer **once per trigger** for readability.
- −Accuracy / +spread feel is intentional; mag empties faster in wall-time if RoF unchanged — optional slight fire-interval tax.
- Visual: three bright bolts; readable stormtrooper volley without Cycler hose.

### Nova Lattice — Exotic (Bloom crown)
- When a **bloom** damages an enemy, it may chain a reduced bloom to a nearby enemy (draft: 1–2 jumps, steep falloff).
- **Requires a bloom source** (this card may also grant a modest baseline bloom enable so the exotic is not dead alone — see deep-dive).
- Chains do not infinite-loop (flag chained blooms as non-chaining).
- Clear amplifier for pack fights; weak alone on single isolated targets.

### Isotope Tip — Exotic (Fallout keystone transform)
- **Full weapon transform** while equipped:
  - Bolts become **slow sticky fallout blobs**
  - On enemy hit: attach to part/brain (prefer brain-level attach for clarity if part attach is noisy)
  - On terrain hit: optional short-lived ground blob / dud rules (see deep-dive)
  - Attached blob: periodic **AoE tick** (damage + Decay amount) around host
- Replaces normal bolt flight/impact behavior (no double-dipping full bolt + full blob unless a card says residual).
- Max concurrent blobs soft cap (draft 4–6 world + attached).
- Does **not** recall to gun. Does **not** build Needle-style supercombine stacks.

### Half-Life — Exotic (Fallout crown)
- Fallout blobs gain an **age curve**:
  - Early life: weaker ticks
  - Mid life: nominal ticks
  - Late life: stronger ticks
  - On natural expiry: **final bloom/pulse** (damage + Decay hitch in radius)
- Encourages leaving blobs to cook rather than instant purge every time.
- Works with Isotope Tip; if Isotope Tip unequipped, no-ops (or grey “Requires Isotope Tip”).

### Ion Lance — Exotic (Ion crown)
- **RMB hold-to-charge** (recommended lock): release fires a **white ion bolt**.
- Pierce draft: 1–2 additional parts/enemies.
- Bonus damage vs shields and/or cores (playtest which reads better).
- Brief stun hitch on full charge hit (boss-reduced).
- Ammo cost: 1 shot or small mag tax per lance (not free).
- M1 remains normal auto (or blob auto if Isotope Tip up) while charging if possible; if engine-awkward, freeze M1 during charge.

### Capacitor Breach — Exotic (Ion crown)
- Activate (RMB tap when not charging, or hold-R alternate — prefer **RMB tap with Lance priority when holding**):
  convert remaining magazine into **one supercell blast** (damage scales with ammo spent).
- Condensed Munitions DNA, plasma-flavored.
- Optional mild self-danger at very high dump (tune lightly; not Infinity Burn clone).

### Sacred cows (do not cut without rewriting identity)

- Baseline full-auto low RPM plasma bolts
- Baseline **Decay** on direct hit
- **No** baseline splash
- **No** baseline hitforce
- Isotope Tip = **full transform** to sticky blobs
- Prism Scatter, Half-Life, Ion Lance exist in ship exotic row
- ~30 upgrades; equal large exotic footprints
- Mix-and-match paths allowed

---

## 8. Exotic Deep-Dives

### 8.1 Prism Scatter

**Fantasy:** Stormtrooper volley — one trigger, a fist of bolts.

**Behaviour sketch:**
- On fire: spawn N bolts (default 3) with angular offsets around aim.
- DamagePerBolt = BaseDamage * prismDamageShare / N (or shared pool rolled once).
- DecayPerBolt = BaseDecay * prismDecayShare / N.
- Recoil: single kick per trigger, slightly above baseline.
- Mag: consume 1 ammo per trigger (preferred) OR 1 per bolt (hungrier — playtest; prefer **1 ammo per trigger** so fan is a pattern card not an ammo tax nightmare).

**Mix notes:**
- + Isotope Tip: fan of sticky blobs (strong — enforce max-blob cap and shared aura ICD).
- + Nova Lattice: only primary bloom chains unless card says each bolt blooms.
- + Ion Lance: unchanged (lance is separate input).

### 8.2 Nova Lattice

**Fantasy:** Splash jumps the gap between bodies.

**Behaviour sketch:**
- Grants **Bloom Enable** at modest radius/damage if no other bloom source (so exotic stands alone).
- On bloom damage event: find nearest valid enemy in chain range not already hit by this chain; spawn reduced bloom.
- Chain damage mult draft 0.45 → 0.25 per jump; max jumps 2.
- ICD per source bloom event.

**Mix notes:**
- Loves Wide Aperture / Starfire Cascades.
- With Fallout auras: aura ticks are NOT blooms unless flagged — do not chain every tick (perf + power).

### 8.3 Isotope Tip (transform)

**Fantasy:** The magazine is full of sticky glowing tumors.

**Projectile:**
- Slower than baseline bolt; larger visual; arcing optional (slight gravity OK, not Globbler bounce spam).
- Max bounces: 0 baseline under transform (stick or splat).

**Attach rules:**
```
OnEnemyHit:
  if attachedCount >= MaxBlobs: refresh oldest or reject new (prefer refresh duration on same brain)
  attach blob to brain (or part if part-cook fantasy desired later)
  start lifetime timer
  begin tick loop

OnTerrainHit:
  v0: spawn short ground blob (duration < enemy attach) OR fizzle with small Decay puff
  prefer short ground blob so misses still teach Fallout
```

**Tick loop (attached):**
```
every tickInterval:
  AoE damage in auraRadius around host (falloff optional)
  apply Decay amount (reduced vs direct bolt)
  respect ICD / DamageFlags so ticks do not proc on-hit bolt cards infinitely
```

**Detach / end:**
- Host death → blob expires ( Contagion Film may leave slick )
- Duration end → Half-Life final pulse if equipped, else quiet expire
- Purge Trigger → early detonate all owned blobs

**Power budget:**
- Direct impact damage of blob on stick ≤ baseline bolt (or slightly less); power moves to aura over time.
- Elite/boss: aura ticks honest but not delete-AFK; max blobs prevent infinite room denial.

### 8.4 Half-Life

**Fantasy:** The longer it lives, the meaner it gets — then it ends loud.

**Age curve (draft):**
| Life phase     | Tick damage mult | Decay mult |
|----------------|------------------|------------|
| 0–30% lifetime | 0.7×             | 0.8×       |
| 30–70%         | 1.0×             | 1.0×       |
| 70–100%        | 1.35×            | 1.15×      |
| Expiry pulse   | one-shot bloom   | hitch      |

- Purge early = weaker than full cook (intentional skill: greed vs safety).
- Requires Isotope Tip.

### 8.5 Ion Lance

**Fantasy:** One white bolt through the line.

**Charge model (draft):**
| Param            | Draft        |
|------------------|--------------|
| Min charge       | ~0.25 s      |
| Full charge      | ~0.7–1.0 s   |
| Damage scale     | 1.5× → 3× bolt |
| Pierce           | 0 → 2        |
| Shield/core mult | up to ~1.5×  |
| Stun hitch       | full charge only; short |
| Ammo             | 1–3 per lance |

**Input priority (RMB):**
1. If Ion Lance equipped and holding charge gesture → Lance
2. Else if Purge Trigger and blobs active → Purge
3. Else if Capacitor Breach tap rules → Breach
4. Else unbound

### 8.6 Capacitor Breach

**Fantasy:** Empty the cell into one screaming star.

**Behaviour sketch:**
- Consume remaining mag (min 3 ammo to activate).
- Fire one supercell projectile or instant beam-bolt hybrid with damage ∝ ammo spent.
- Reload lock brief after dump.
- With Isotope Tip: supercell may be a mega-blob (dangerous, fun) — allow with strict 1-at-a-time cap.

### 8.7 Exotic coexistence

| Pair | Rule |
|------|------|
| Prism + Isotope | Allowed; shared blob cap; fan may cost 1 ammo |
| Half-Life + Isotope | Half-Life requires Isotope |
| Nova + any bloom | Allowed; chain flags prevent loops |
| Ion Lance + Capacitor | Both allowed; RMB priority table |
| All exotics | Grid-limited; power via caps not hard bans |
| Footprints | All six exotics **same cell count**, larger than typical rares/epics |

---

## 9. Content Budget & Universal Truths

| Rule | Value |
|------|--------|
| Total ship pool | **~30** |
| Paths | 3 peer wells + generics |
| Exotics | **6** — equal large hex |
| Epics | **~8** |
| Rares | **~10** |
| Standards | **~5–6** |
| Oddity | Boundary Incursion (1) optional parity |
| Path locks | Half-Life / Sticky Yield / Purge strongly tied to Isotope Tip; bloom scalers need bloom enable |
| Mix | No anti-synergy matrix |
| Cross-mod v1 | None |

---

## 10. Full Upgrade List (~30 ship)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity  
Cell rule: Exotic shapes larger than others; all Exotics same cell count.  
Player-facing names below. API names assigned at implementation.  
IDs (draft): gear **92400**, upgrades **92401+** (avoid Splash 923xx, template 910xx).

Stack: **✓** CanStack · **—** unique  
Well: primary gravity well (still mixable)

------------------------------------------------------------------------------
EXOTICS (6)
------------------------------------------------------------------------------

X1. Prism Scatter — Exotic — Bloom — —
    Fan of bolts per trigger; shared damage/Decay budget. See §8.1.

X2. Nova Lattice — Exotic — Bloom — —
    Enables modest bloom; blooms chain to nearby enemies at reduced power. See §8.2.

X3. Isotope Tip — Exotic — Fallout — —
    Full transform: bolts → sticky fallout blobs with Decay auras. See §8.3.

X4. Half-Life — Exotic — Fallout — —
    Requires Isotope Tip. Age curve + expiry pulse. See §8.4.

X5. Ion Lance — Exotic — Ion — —
    RMB hold-to-charge pierce lance. See §8.5.

X6. Capacitor Breach — Exotic — Ion — —
    Dump remaining mag into one supercell blast. See §8.6.

------------------------------------------------------------------------------
EPICS (8)
------------------------------------------------------------------------------

E1. Starfire Cascades — Epic — Bloom — —
    Killing a target with a **bloom** (or direct kill while a bloom source is equipped and kill blow was splash-tagged)
    triggers a secondary bloom at the corpse. ICD. Clear loop card.

E2. Refractive Coating — Epic — Bloom — —
    Bolts that hit **terrain** redirect a bloom toward the nearest enemy in range (or along reflected aim).
    Grants bloom enable at reduced radius if none present.
    Wall-bang splash fantasy without baseline splash.

E3. Contagion Film — Epic — Fallout — —
    When a fallout blob expires or its host dies, leave a short **Decay slick** on the ground
    (duration draft 2–4s, light ticks). Requires Isotope Tip to matter.

E4. Critical Mass — Epic — Fallout — —
    When N of your blobs (draft 3) exist within a radius of each other or on one brain,
    they cascade-detonate for bonus damage. Requires Isotope Tip.
    Distinct from Half-Life (spatial stack vs age curve).

E5. White Bolt — Epic — Ion — —
    Ion Lance (and optionally Capacitor projectile) gains +damage and slightly faster charge.
    If Ion Lance unequipped: small chance on M1 to fire an empowered bolt (weak consolation) OR no-op — prefer **scales Lance primarily**.

E6. Shield Eater — Epic — Ion — —
    Bonus damage vs shields; ion splash hitch applies brief Shock amount on shielded targets.
    Direct bolts gain mild shield mult (path glue without forcing Lance).

E7. Overcharge Cycler — Epic — Bloom / Ion — —
    Sustained M1 builds Overcharge stacks. At threshold, next bloom (if any) is enlarged OR next Ion Lance auto-fills partial charge.
    If no bloom/ion: stacks grant mild +Decay amount briefly then vent.
    Not a hard overheat brick.

E8. Ghost Discharge — Epic — Generic hybrid — —
    Every Nth bolt (draft 4th) leaves a delayed **afterimage** impact at the hit point after short delay
    (echo damage; echo may bloom only if bloom enabled). Readable double-tap fantasy.

------------------------------------------------------------------------------
RARES (10)
------------------------------------------------------------------------------

R1. Wide Aperture — Rare — Bloom — ✓
    +Bloom radius; −direct bolt damage slightly.
    If no bloom source: grants **minimal bloom enable** (tiny radius) so the card is never fully dead — OR no-op with UI note.
    Prefer: grants minimal bloom enable (teaches Path A).

R2. Hot Cell — Rare — Generic — ✓
    +Bolt damage; +recoil. Simple chunk card.

R3. Ricochet Charge — Rare — Bloom — ✓
    Bolts bounce +1 off terrain then continue; on final impact, if bloom enabled, bloom once.
    Cycler Ricochet cousin tied to splash fantasy. With Isotope Tip: blobs may bounce once before stick (fun, optional).

R4. Sticky Yield — Rare — Fallout — ✓
    +Blob duration and/or +aura radius. Requires Isotope Tip; else no-op.

R5. Dirty Bomb — Rare — Fallout — ✓
    Blob aura applies additional Decay amount (and tiny direct tick bump). Requires Isotope Tip.

R6. Purge Trigger — Rare — Fallout — —
    RMB detonates all your blobs early for a burst (scales with remaining Half-Life phase if present).
    RMB priority under Ion Lance charge. Requires Isotope Tip.

R7. Coil Tap — Rare — Ion — ✓
    Ion charge time reduced; slight −max lance damage OR −mag size (pick one tax).

R8. Core Bias — Rare — Ion / Generic — ✓
    +Damage to cores after you have applied Decay to that brain this combat (short memory).
    Rewards Decay spine without splash.

R9. Ventilated Barrel — Rare — Generic — ✓
    −Recoil; slight −Overcharge build rate if Overcharge Cycler present; mild handling.

R10. Plasma Accelerator — Rare — Generic — ✓
    +Bolt speed; +range / falloff start. Helps mid-long blaster feel.

------------------------------------------------------------------------------
STANDARDS (5) + ODDITY (1)
------------------------------------------------------------------------------

S1. Expanded Cell — Standard — Generic — ✓
    +Magazine size.

S2. Field Charger — Standard — Generic — ✓
    +Reload speed.

S3. Focusing Lens — Standard — Generic — ✓
    +Range; tighter hip spread.

S4. Spare Cells — Standard — Generic — ✓
    +Ammo reserves.

S5. Kinetic Jacket — Standard — Generic — ✓
    Mild +direct damage (lowest tier staple).

S6. Rot Primer — Standard — Fallout / Generic — ✓
    Mild +Decay effectAmount on direct bolts. Spine stackable.

O1. Boundary Incursion — Oddity — Generic — —
    Increases upgrade grid size. (Parity with vanilla/other kits.)

------------------------------------------------------------------------------
FROZEN SHIP 30
------------------------------------------------------------------------------

  EXOTIC (6)
    1  Prism Scatter
    2  Nova Lattice
    3  Isotope Tip
    4  Half-Life
    5  Ion Lance
    6  Capacitor Breach

  EPIC (8)
    7  Starfire Cascades
    8  Refractive Coating
    9  Contagion Film
    10 Critical Mass
    11 White Bolt
    12 Shield Eater
    13 Overcharge Cycler
    14 Ghost Discharge

  RARE (10)
    15 Wide Aperture
    16 Hot Cell
    17 Ricochet Charge
    18 Sticky Yield
    19 Dirty Bomb
    20 Purge Trigger
    21 Coil Tap
    22 Core Bias
    23 Ventilated Barrel
    24 Plasma Accelerator

  STANDARD (5)
    25 Expanded Cell
    26 Field Charger
    27 Focusing Lens
    28 Spare Cells
    29 Rot Primer
       (Kinetic Jacket → backlog if Oddity takes the 30th slot)

  ODDITY (1)
    30 Boundary Incursion

  If Kinetic Jacket must ship in first 30, drop Spare Cells or merge Field Charger/Expanded Cell later.
  Recommended: ship Rot Primer over Kinetic Jacket for identity; Kinetic Jacket backlog.

------------------------------------------------------------------------------
BACKLOG (designed, not first 30)
------------------------------------------------------------------------------

  Kinetic Jacket, Heavy Jacket (−RoF +damage), Burst Gate (3-round), 
  EMP Bloom (ion splash stun), Ground Zero (blob ground always), 
  Isotope Splitter (blob splits on death), Ally Cook Warning VFX only,
  Bee Plasma joke, Fire Tip / Shock Tip mono cards, 
  Full Radiation EffectType (only if Decay proves wrong fantasy),
  Stormtrooper Irony (spread up, damage up — meme exotic),
  Hover Cell (slow fall while charging ion), 
  Magboot Latch (no move penalty — unnecessary),
  Shared Processing crumbs, Infinity-style self-damage ammo (avoid unless wanted)

---

## 11. Example Builds (mix-and-match encouraged)

**Lane painter (Bloom clear)**
  Nova Lattice + Wide Aperture + Starfire Cascades + Ricochet Charge
  + Refractive Coating + Rot Primer
  Enable splash, bounce walls, chain packs, Decay still amps.

**Volley trooper**
  Prism Scatter + Hot Cell + Plasma Accelerator + Focusing Lens
  + Ghost Discharge + Expanded Cell
  Fan bolts, mid-range tracking, echo hits — splash optional add-on.

**Walking reactor (Fallout)**
  Isotope Tip + Half-Life + Critical Mass + Dirty Bomb
  + Contagion Film + Sticky Yield + Purge Trigger
  Stick tumors, greed the age curve, cascade when stacked, purge when swarmed.

**Ion executioner**
  Ion Lance + Capacitor Breach + White Bolt + Shield Eater
  + Coil Tap + Core Bias + Field Charger
  Charge white bolts through elites; dump mag on expose windows.

**Hybrid freak**
  Isotope Tip + Nova Lattice + Prism Scatter + Ghost Discharge
  Fan blobs (capped), auras cook, lattice chains only on true blooms —
  tune caps so this is fun, not room-delete.

---

## 12. Economy & Tuning Rules of Thumb

- Baseline Decay amount: noticeable in a mag dump, not one-tap full sat on bosses.
- Prism shares budgets — never full damage × pellet count.
- Bloom default damage fraction of bolt: draft 0.35–0.55 in small radius; Wide Aperture trades direct for radius.
- Fallout: power over time; stick impact should feel slightly weaker than a raw bolt so aura is the point.
- Max blobs 4–6; aura tick ICD; boss aura mult if needed.
- Half-Life expiry pulse < Critical Mass cascade peak (different fantasies).
- Ion Lance ammo cost prevents infinite stun poke.
- Capacitor Breach min ammo gate prevents tap-spam micro dumps.
- Watch stacked Decay: Rot Primer + Dirty Bomb + aura + Prism — prefer diminishing on non-direct applies.
- No baseline hitforce — if a future card adds shove, ally mult ≈ 0.

---

## 13. Strengths, Weaknesses & Failure Modes

### Strengths
- Clear blaster fantasy at a readable RPM
- Decay spine works with zero upgrades
- Splash fantasy preserved but earned (Bloom)
- Sticky radioactive blob fantasy as full transform crown
- Ion gives ST execute without stealing DMLR anatomy kit
- High hybrid ceiling

### Weaknesses
- Loses pure hose DPS to Cycler
- Loses architecture/control lock to HLC
- Loses puddle denial to Globbler
- Without Bloom/Fallout investment, clear is “just shoot them with Decay”
- Blob path weaker if player purges too early or never lets ticks work
- Low RPM punishes panic spray (intentional)

### Failure modes to avoid

| Failure | Mitigation |
|---------|------------|
| Baseline splash creeps back in | Sacred cow; code review any OnHit AoE without bloom flag |
| Isotope Tip too strong free clear | Weaker stick impact; aura ICD; max blobs |
| Prism × Isotope room delete | Shared budget + hard blob cap |
| Nova chains aura ticks | Aura ≠ bloom unless flagged |
| Half-Life dead without Isotope | Require flag; grey text |
| Ion Lance infinite stun | Boss reduce; ammo cost; short hitch |
| Decay snowball with all paths | Reduced non-direct Decay amounts |
| Reads as HLC | No Jam, paint, panes, revelry |
| Reads as Plate | No recall |
| Reads as Needle | No supercombine / Extract / Poison |
| 30 cards but 3 builds only | Keep standards universal; minimize hard locks |

---

## 14. Visual, Audio & Thematic Design

### Appearance
- Industrial blaster rifle: heat shroud, cell magazine, emitter cowling
- Bolt: cyan/white plasma slug with short trail (cosmetic only on baseline)
- Decay apply: sickly green crackle on impact
- Bloom (upgraded): radial ray splash / star burst at contact
- Fallout blob: larger green-black sticky glob; aura ring on host
- Ion lance: bright white elongated bolt; charge whine

### Sound design goals
- Low cyclic mechanical + energy crack (not SMG chatter)
- Decay: wet static tick on apply
- Bloom: crystalline splash
- Blob stick: heavy wet thump; aura: low geiger-like pulse
- Ion: capacitor whine → sharp discharge
- Capacitor Breach: rising dump scream

### Flavor / codex
> “PB-11 bolts denature mycostructure on contact. Field technicians are reminded that
> ‘it looked like it should explode’ is not an approved modification rationale.”
> — SAXON Armory Bulletin 11-B (partially burned)

---

## 15. Success Criteria / Player Fantasy Checklist

- [ ] Zero-upgrade gun feels like a complete low-RPM plasma blaster with Decay
- [ ] First Bloom card makes “bolts splash” feel like a discovery, not a stat tweak
- [ ] Isotope Tip visibly transforms the weapon fantasy in one equip
- [ ] Half-Life makes greeding duration exciting
- [ ] Prism Scatter reads as volley, not accidental shotgun hose
- [ ] Ion Lance RMB charge is a deliberate execute button
- [ ] Hybrids work without a ban list
- [ ] Co-op: allies understand glowing stuck blobs as “dump here”
- [ ] Nobody confuses it for HLC paint or Plate recall after one mag

---

## 16. Implementation Appendix (For Later — Not This Pass)

Design-only milestone: **this document**. When coding starts, prefer:

| Piece | Approach |
|-------|----------|
| Registration | Weapon template clone path; unique GearInfo id **92400**, APIName `plasma_blaster` |
| Display name | **Plasma Blaster** via TextBlocks |
| Data host | `PlasmaBlasterBehaviour` with `Data` struct for flags/scalars |
| Fire hook | Harmony `Gun.OnFiredBullet` postfix (template) until real prefab subclass |
| Decay | `GunData.damageEffect = Decay` (or apply via damage path); tune effectAmount |
| Bloom | On bullet hit / damage target: if bloomEnabled, GameManager-style AoE DamageData |
| Blob entity | Lightweight networked or local visual + tick component attached to enemy / world |
| Transform | Isotope Tip sets `projectileMode = FalloutBlob`; fire hook spawns blob prefab/logic |
| Ion charge | Input read on owner; charge01 while RMB; release spawns lance bolt |
| Purge / Breach | RMB priority table in behaviour |
| Upgrades | `PlayerData.CreateUpgrade` + properties Apply/Remove → behaviour snapshot restore |
| Mod flags | `[MycoMod(..., ModFlags.IsSandbox)]` |
| Base gun pick | CartridgeSMG or energy-like gun with projectile; retune interval/damage/effect |
| Cross-mod | None v1 |

### Suggested `PlasmaBlasterBehaviour.Data` fields (sketch)

```
// Baseline scales
float damageMultiplier;
float decayEffectAmountMultiplier;
float fireIntervalMultiplier;
float boltSpeedMultiplier;
float rangeMultiplier;
float recoilMultiplier;
float magazineSizeBonus; // or mutate GunData directly in Apply

// Bloom
bool bloomEnabled;
float bloomRadius;
float bloomDamageScale;      // fraction of bolt damage
float bloomDecayScale;       // fraction of bolt decay amount
bool bloomOnTerrain;
bool novaLattice;
float novaChainRange;
int novaMaxJumps;
float novaChainDamageMult;
bool starfireCascades;
bool refractiveCoating;
bool wideAperture;           // or just baked into radius

// Prism
bool prismScatter;
int prismBoltCount;
float prismDamageShare;
float prismDecayShare;
float prismSpreadAngle;

// Fallout / Isotope
bool isotopeTip;             // transform
float blobSpeed;
float blobLifetime;
float blobAuraRadius;
float blobTickInterval;
float blobTickDamageScale;
float blobTickDecayScale;
int maxBlobs;
bool halfLife;
float halfLifeExpiryDamageScale;
bool contagionFilm;
float contagionDuration;
bool criticalMass;
int criticalMassCount;
float criticalMassRadius;
bool purgeTrigger;
float stickyYieldDurationMult;
float dirtyBombDecayMult;

// Ion
bool ionLance;
float ionMinChargeTime;
float ionFullChargeTime;
float ionDamageMultAtFull;
int ionPierceAtFull;
float ionShieldMult;
float ionStunDuration;
int ionAmmoCost;
bool capacitorBreach;
float capacitorMinAmmo;
float whiteBoltMult;
float shieldEaterMult;
float coilTapChargeMult;

// Soft systems
bool overchargeCycler;
float overchargePerShot;
float overchargeThreshold;
bool ghostDischarge;
int ghostEveryN;
float ghostDelay;
float ghostDamageScale;

// Explicit non-goals on data
// no baseline hitForce fields unless a future card needs them
```

### Hit / tick safety flags
- Bloom and aura damage should use flags that prevent re-entry into “on bolt hit” transform logic.
- Transferred/chained blooms marked non-chaining.
- Blob ticks: DamageFlags.DamageOverTime or custom so Prism/Ghost do not proc per tick.

### v1 must-ship (fantasy complete)
- Baseline auto + Decay + no splash + no hitforce
- All 6 exotics with rules above
- Bloom enable path (Nova and/or Wide Aperture)
- Isotope transform + aura ticks + Half-Life curve
- Ion Lance RMB charge
- Full frozen 30 registration stubs
- Caps: max blobs, chain ICD, prism budget

### Stretch / post-v1
- Custom mesh / bolt VFX AssetBundle
- Wwise events
- Critical Mass juiciness polish
- Optional Radiation EffectType if Decay fantasy fails playtest
- Kinetic Jacket and backlog rares

---

## 17. Naming & Presentation

| Slot | Value |
|------|--------|
| Display name | **Plasma Blaster** |
| Internal / API | `plasma_blaster` |
| Design nicknames | Plasma Rifle, PB-11, E-11-ish (notes only) |
| Short description | *Low-RPM plasma automatic. Bolts apply Decay. Upgrades add ray-splash blooms, sticky isotope blobs, and ion lances.* |
| Thunderstore name (later) | `PlasmaBlaster` |
| GUID (later) | `sparroh.plasmablaster` |
| Project folder (current) | `.new.PlasmaRifle` |
| Gear id (draft) | 92400 |
| Upgrade ids (draft) | 92401–92430 |

---

## 18. Open Questions (Balance / Feel — Not Blocking Doc)

1. Prism ammo: 1 per trigger (preferred) vs 1 per pellet?
2. Nova Lattice solo bloom size vs requiring a second bloom card?
3. Isotope terrain miss: short ground blob vs fizzle?
4. Attach to part vs brain for blobs?
5. Ion Lance: allow M1 while charging?
6. Capacitor + Isotope = mega-blob yes/no?
7. Exact RPM/mag/Decay amounts — playtest only.
8. Should Wide Aperture grant minimal bloom enable or no-op?
9. Kinetic Jacket in first 30 vs Rot Primer (doc prefers Rot Primer + Oddity)?
10. Cosmetic bolt trail only — default yes, zero gameplay.

---

## 19. Design Checklist

- [x] Niche: low-RPM plasma blaster with Decay spine
- [x] No baseline splash (user lock)
- [x] No baseline hitforce (user lock)
- [x] Full-auto lower RPM (user lock)
- [x] Baseline Decay (user lock)
- [x] Display name: Plasma Blaster (user lock)
- [x] Isotope Tip: full transform (user lock)
- [x] Crowns: Prism Scatter, Isotope Tip, Half-Life, Ion Lance liked
- [x] Peer exotic pair: Nova Lattice, Capacitor Breach
- [x] ~30 upgrades
- [x] Three gravity wells, mix/match
- [x] Differentiated from Cycler / HLC / Globbler / Plate / Needle
- [x] Implementation deferred

---

## 20. Changelog (Design Doc)

| Date | Change |
|------|--------|
| 2026-08-07 | Initial design doc from user brief (Star Wars blaster + ray splash + sticky radioactive blob), wiki research (Cycler plasma/ricochet, Globbler, Plate stick, Gunship splash, Clearing/Excited/Corrosive Plasma naming), and peer docs (DMLR, Needle, Chaingun, Zephyr, HLC, Splash Canister). User locks: no baseline splash; auto low RPM; no baseline hitforce; baseline Decay; name Plasma Blaster; Isotope Tip full transform; keep Prism Scatter, Isotope Tip, Half-Life, Ion Lance; Ion path fine; extra baseline juice deferred (lean baseline). |

---

*End of design document. Next step when ready: rename template identifiers and implement baseline Plasma Blaster (auto + Decay only), then layer Bloom enable, Isotope transform, and Ion Lance.*
