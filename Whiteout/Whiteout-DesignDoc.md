# Whiteout — Design Document (v1)

> Status: **Design only** — no implementation yet.  
> Working folder: `.new.Cryothrower`  
> Product shape: **parallel new primary** — does **not** replace or patch Au-Si Jackrabbit, Pesticide, or Rime Charge.  
> Doc file: `Whiteout-DesignDoc.md`

---

## 0. Locked Decisions (2026-08-21)

| Decision | Lock |
|----------|------|
| Product shape | **Parallel new primary** — not a Jackrabbit rework, not a Pesticide promotion |
| Player-facing name | **Whiteout** |
| Working APIName | `whiteout` |
| Working GUID | `sparroh.whiteout` |
| Folder / project today | `.new.Cryothrower` (rename optional at ship) |
| Slot | **Primary** |
| Element | **`EffectType.Cryo` (10)** — strong native apply on hose + lob |
| Primary fire | **Hold M1** continuous cryo cone (raycast fan v1) |
| Alt-fire | **RMB** cryo cell lob (not hold-R; R stays reload) |
| Lob economy | **Shared mag tax** — lob spends magazine ammo |
| Cone primitive (v1) | **Jackrabbit-style raycast fan** (proven); ConeBullet volume is post-v1 option |
| Paths | **Gale / Rime Cell / Shatter** |
| Rime Charge synergy | **Soft only** — no hard mod dep; optional wet/fire spice cards |
| Sacred cows (user) | **Unsure** — defaults below; revisit after playtest |
| Relation to Jackrabbit | Cousin *delivery* DNA (stream + lob); Fire bounce shotgun identity stays on Jackrabbit |
| Relation to Rime Charge | Cousin *element* kit; Rime owns floor/plates/blizzard grenade; Whiteout owns lane hose + lob punctuation |
| Relation to Thermal Solstice | Opposite temperature continuous weapon; Solstice = long Fire beam heavy; Whiteout = short Cryo cone primary |
| Relation to Zephyr | Both cones; Zephyr = instant sonic scarce blast; Whiteout = sustained freeze hose |
| Exotic count | **E6** — equal large hex shapes |
| Ship pool | **~30** upgrades; hybrids OK; no anti-synergy matrix |
| MycoMod (impl) | IsSandbox |
| Tone | SAXON industrial **coolant projector / de-icer weaponized** — blue-white fog, rime crust, hazard stripes; not magic ice staff |

### 0.1 User lock map (Q&A)

| # | Question | Answer |
|---|----------|--------|
| 1 | Ship name | **Whiteout** |
| 2 | Alt-fire binding | **RMB** |
| 3 | Lob economy | **Shared mag tax** |
| 4 | Cone primitive | **Raycast fan for now** |
| 5 | Path names | Approved direction (branded below as Gale / Rime Cell / Shatter — Path A renamed off ship name to avoid “Whiteout path on Whiteout”) |
| 6 | Rime synergy | **Soft** |
| 7 | Extra sacred cows | **Unsure** → use defaults in §3; open questions in §16 |

---

## 1. High Concept / Fantasy

A SAXON man-portable **cryo projector**. Hold the trigger and a constant frost cone hoses the lane: tick damage, heavy Cryo saturation, enemies crawling under full freeze. Tap RMB and the gun lobs a **rime cell** — a cryo grenade paid from the same magazine — to dump saturation on a clump or seed a shatter setup.

Baseline is honest flamethrower intimacy at cold temperature: short cone, high uptime, pack paint, ammo anxiety on long holds and greedy lobs. No free ice rink, no shattering armor plates, no ambient blizzard god — those live on **Rime Charge**. Whiteout’s job is **lane ownership** and **freeze → crack**.

**One-liner:** *Hose them white. Lob the cell. Shatter the statue.*

**Product shape:** New parallel primary (**Whiteout**). Does not replace Jackrabbit, Pesticide, Rime Charge, Thermal Solstice, Zephyr, or any other catalog entry.

**SAXON marketing blurb (draft):**
> SAXON W-9 Whiteout — Continuous cryo projector for extended field refrigeration.  
> Hold trigger to authorize fog cone. Secondary trigger authorizes cell lob from primary magazine.  
> Full freeze of local fauna is within expected operating envelope.  
> (Not a flamethrower. Different docket. Similar HR complaints.)

Optional stingers:
- “If it is still sprinting, you are under-painting.”
- “Magazine feeds the hose *and* the lob. Budget winter.”
- “Jackrabbit borrowed a lighter. This *is* the cold front.”
- “Rime Charge owns the floor. Whiteout owns the lane.”
- “Shatter is not optional. It is the punchline.”

---

## 2. Role & Fantasy in the Arsenal

- **Slot:** Primary  
- **Range:** Short–mid continuous cone (draft 8–12 m effective)  
- **Role:** Pack freeze hose / cryo setup battery / optional lob ordnance / shatter payoff forks  
- **Loop:** Paint cone → full-sat slow → lob or ally crack → reload beat → repeat  
- **Gap filled:**
  - Jackrabbit = Fire bounce shotgun; Pesticide = ADS Fire stream side-mode  
  - Rime Charge = Cryo throwable terrain + plates  
  - Thermal Solstice = Fire continuous beam **heavy**  
  - Zephyr = instant sonic pressure cone, scarce mag  
  - Globbler / acid kits = puddle denial, not freeze  
  - Nothing owns **full-time cryo cone primary with mag-tax lob + shatter grid** as parallel catalog gear  

**Not trying to be:** Jackrabbit with cryo paint, Rime Charge on a trigger, Solstice beam, Zephyr thump, or a silent stat-stick “Pesticide but cold.”

### 2.1 Comparison snapshot

```
Weapon / kit              Niche                            Whiteout differentiator
------------------------  -------------------------------  ------------------------------------------
Jackrabbit + Pesticide    Fire bounce SG + ADS flame       Full-time Cryo hose primary; RMB lob
PesticideRework           Turbo range + missing-HP on ADS  Separate QoL mod; not this gun
Rime Charge               Cryo nade terrain + plates       Lane hose; no plates/rink/blizzard spine
Thermal Solstice          Fire beam heavy + soft heat      Short cone primary; Cryo; mag-tax lob
Zephyr                    Instant sonic scarce cone        Sustained freeze hose, not pressure blast
Splash / wet kits         Water primer                     Soft Flash Freeze partner only
Thermite / fire kits      Ignite / mend                    Soft Flash Thaw spice only
```

### 2.2 Naming note

**Whiteout** is the ship face. Internal notes may still say Cryothrower. APIName `whiteout` avoids colliding with path fantasy strings and Jackrabbit APIs. Codex may wink at Pesticide/Jackrabbit as “warm cousin delivery”; must not ship as a rework patch.

Path A is branded **Gale** so the gun name and path name are not identical (same pattern as Solstice ≠ Reactor).

---

## 3. Design Pillars

1. **Full-time cryo cone is sacred** — zero upgrades still feels like a complete freeze hose primary.  
2. **Strong native Cryo is sacred** — baseline full-sats focused grunts; Shatter path elevates payoff, does not invent freeze.  
3. **RMB lob is sacred baseline punctuation** — mag-tax cell exists at zero upgrades; Rime Cell path elevates ordnance.  
4. **R stays reload** — no Jackrabbit hold-R charge steal on baseline.  
5. **Shared mag economy is law** — hose and lob drink the same pool; greedy lobs are a skill tax.  
6. **Raycast fan v1** — Jackrabbit Pesticide DNA; readable, implementable; ConeBullet optional later.  
7. **On-hit / freeze / lob / shatter verbs > flat % damage stickers.**  
8. **Three peer paths (Gale / Rime Cell / Shatter); hybrids intended; no anti-synergy matrix.**  
9. **Rime Charge contrast is law** — no Shattering Armor plates, Ice Rink, Frozen Mist god-cloud, or Blizzard ambient as Whiteout spine. Thin residual fog/puddle crumbs OK on cards.  
10. **Jackrabbit contrast is law** — no bounce-shell primary identity; no Fire spine; flamethrower is not ADS-gated side-mode (M1 *is* the stream).  
11. **~30 ship upgrades; E6 equal large exotic shapes.**  
12. **Boss-safe** — full freeze slow OK; hard CC / shatter bursts budgeted; no permanent floor delete.  
13. **Ally-safe** — cone and lob respect friendly rules; self-Cryo mild; no team-freeze identity.  
14. **SAXON industrial coolant-projector tone.**

---

## 4. Core Mechanics & Gunfeel (Baseline)

### 4.1 Base gun

| Trait | Draft / intent |
|-------|----------------|
| Fire mode | **Hold M1** continuous cryo cone (raycast fan ticks) |
| Damage | Medium–high sustained tick DPS — flamethrower band, not sniper, not Zephyr one-shot |
| Element | **Cryo** strong native `damageEffect` + solid effectAmount on hose ticks |
| Tick rate | ~0.10 s (Jackrabbit flamethrower cadence anchor) |
| Mag / reserve | **Large mag** continuous drain (draft 200–320); modest reserve |
| Reload | Deliberate ~2.0–2.6 s — downtime after long paint or lob spam |
| Cone | Short–mid; draft length **9–12 m**; width via fan magnetism / multi-ray spread |
| Lob (RMB) | Arc cryo cell; impact AOE Cryo + modest damage; **mag tax** |
| ADS | Optional light ADS; does **not** gate the hose (unlike Pesticide) |
| Movement | Mild move penalty while hosing (plant-friendly, not full root) |
| Model/audio | Coolant tank + projector nozzle, frost exhaust, rising hiss, lob *thunk* |

Draft firefeel band (VALIDATE IN PLAYTEST):

| Param | Draft |
|-------|--------|
| Hose tick damage | Medium flamethrower (pack melts under focus; not Solstice heavy) |
| Hose Cryo effectAmount / tick window | Strong — grunt full-sat in ~0.5–1.0 s focus |
| Tick interval | 0.10 s |
| Mag | ~250 |
| Reserve | ~250–400 (match primary patterns at impl) |
| Reload | ~2.3 s |
| Cone length | ~10 m |
| Fan rays / magnetism | Jackrabbit-like target magnetism; surface magnetism low |
| Falloff | Soft inside cone; hard stop near max range |
| Lob mag tax | **25–40** ammo per lob (tune so ~6–10 lobs per mag if dry-firing only lobs; fewer if hosing) |
| Lob damage | Modest boom (setup > delete); Cryo effectAmount **high** (~full-sat class dump in radius) |
| Lob radius (`force` / hitForce pattern) | ~4–6 m start |
| Lob speed / gravity | Jackrabbit lob ballpark (speed ~35, gravity ~30) as start |
| Lob ICD | None beyond mag tax + throw animation; optional short 0.35 s input debounce |
| Pierce | **None** on baseline hose |
| Freeze hard-CC (Frozen Solid stun) | **None** on baseline — full-sat slow only |

### 4.2 Inputs

| Input | Baseline | Upgraded claims |
|-------|----------|-----------------|
| Hold M1 | Cryo cone hose + mag drain + Cryo apply | All path riders |
| Release M1 | Stop hose | Gale residual fog breath (card); Shatter latch ticks may linger on targets |
| RMB | **Fire lob cell** (mag tax); fails if mag < tax | Rime Cell mortar / cluster / mine arm |
| R | **Reload only** | Reload only |
| ADS | Optional precision crumb only if a card says so | Gale collimation cards |

### 4.3 Hose model (baseline) — raycast fan

Mirror Jackrabbit Pesticide structure, Cryo-flavored:

```
While M1 held, Active, mag > 0, owner:
  every FlamethrowerDamageInterval (0.10 s):
    spend mag proportional to dt (continuous drain)
    RaycastTargetsAndSurface from muzzle along aim
      range = hoseRange
      targetMagnetism = hoseMagnetism
      surfaceMagnetism low
    for each valid target hit:
      DamageData(
        damage = hoseDps * interval,
        effect = Cryo,
        effectAmount = hoseCryoPerInterval,
        flags = DamageOverTime | AOE
      )
    light camera shake
  VFX/audio loop while hosing
On M1 release / disable / empty:
  stop VFX/audio; stop ticks
```

**Mag drain:** continuous while hosing (not only on hit). Empty air still costs winter.

**Multi-target:** fan may tag multiple colliders per interval (Pesticide-style). Primary skill = sweep discipline + focus elites to full-sat.

### 4.4 Lob model (baseline) — RMB + shared mag

```
On RMB (owner, not reloading, mag >= lobTax, debounce OK):
  mag -= lobTax
  spawn GrenadeBullet-like cell:
    damage modest
    damageEffect = Cryo
    damageEffectAmount high (~10 class full-sat dump in AOE)
    speed/gravity arc
    force = lobRadius
    maxBounces = 0 baseline
  fire SFX + light recoil punch
```

**No charge gate on baseline lob** — instant RMB (Jackrabbit charge-lob is upgrade-optional via Cryo Mortar exotic).  
**No separate cell meter** — magazine *is* the economy.

### 4.5 Cryo combat literacy (baseline)

From game / Rime Charge system truths (do not invent parallel chill):

| Term | Meaning |
|------|---------|
| **Chilled / freezing** | Cryo saturation > 0 |
| **Fully frozen** | `IsFullySaturated` on Cryo — heavy slow (`SlowTargetThisTick` class) |
| **Frozen solid** | Hard CC stun-style — **upgrade-gated** (Flash Freeze etc.) |
| **Shatter** | Vanilla shatter meter / kit cards that pay out when freeze breaks or on bonus vs frozen |
| **Your fog / cell** | Hose and lobs from **this** Whiteout only |

Saturation add uses real status rules (`amount * 0.1` class → ~10 effect ≈ full freeze from empty before decay).

### 4.6 Baseline combat loop (zero upgrades)

```
Find pack seam → Hold M1 cryo hose
   → grunts slow under full-sat; tick damage deletes trash
   → RMB lob into dense clump for sat dump (mag tax)
   → focus elite to full freeze; allies (or your shatter cards later) crack
   → R on dry; never brain-off hose empty air forever
```

Skill without upgrades: cone discipline, mag budget between hose and lob, freeze focus vs spray, using lob as punctuation not panic dump.

### 4.7 What baseline does NOT include

- No Ice Rink / ally skate / enemy slide  
- No Shattering Armor plates  
- No Frozen Mist AI-blind god cloud  
- No Blizzard ambient zone  
- No Cyclone orbiters / Oven heat vents (Rime retained posters stay on Rime)  
- No hard Frozen Solid CC  
- No freeze-break explosions  
- No cluster lobs / mines / mortar charge  
- No cone pierce / long glacial beam mode  
- No Fire element spine  
- No ADS-only stream (Pesticide pattern rejected for primary identity)  
- No hold-R lob charge (RMB locked)  
- No separate lob ammo pool  

Those are path-, exotic-, or explicitly out-of-identity.

---

## 5. Shared Framework Vocabulary

Upgrades speak these verbs. Baseline owns **Hose**, **Lob**, **Cryo apply**.

### 5.1 Hose
- Continuous raycast-fan cryo damage + apply while M1 held and mag allows  
- Primary delivery for Gale geometry and Shatter on-contact riders  
- Width / range / tick density / residual fog are Gale-owned unless a card says otherwise  

### 5.2 Lob / Cell
- RMB mag-tax grenade projectile  
- Rime Cell spends and multiplies cells; other paths may read “on lob detonate” as hybrid bait  

### 5.3 Cryo / Freeze
- Real `EffectType.Cryo` via hose ticks + lob boom  
- Baseline strong; Gale multiplies paint; Shatter multiplies payoff  

### 5.4 Frostbite (optional light stack)
- Soft stack while target remains in hose (Gale-owned teaching tool)  
- At threshold: bonus sat or tiny damage — **not** a second full status system  
- Cap hard; clears shortly after leaving cone  

### 5.5 Shatter payoff
- Bonus damage vs fully frozen, shatter meter fill, on-break iceburst, convert procs  
- Shatter path core; hybrids OK  

### 5.6 Fog crumb
- Short residual cryo mist along hose path or on lob (thin)  
- **Not** Rime Frozen Mist exotic; no full untarget cloud on Whiteout spine  
- Gale exotic **Whiteout Curtain** is the widest legal fog read  

### 5.7 Brittle window
- Short window after full-sat or on purpose card where non-Cryo / all damage cracks harder  
- Shatter-owned  

### 5.8 Mag budget
- Shared resource; economy cards may refund hose ticks or discount lob tax  
- Infinite hose + free lob is a tuning bug  

---

## 6. Upgrade Paths (gravity wells — hybrids intended)

### Path A — GALE (hose mastery / pack freeze)
“Own the cone. Paint the room white.”

- Spine: cone range/width, tick density, Cryo apply rate, Frostbite stacks, ammo efficiency while hosing, mild move-while-hose  
- Clear vs ST: Clear native via sweep paint; ST via focus sat + hybrid Shatter  
- Hybrid hooks: hose feeds lob setups; Frostbite targets take bonus shatter crack  

### Path B — RIME CELL (lob / ordnance)
“The grenade is a second weapon, not a party trick.”

- Spine: lob tax discount, radius, multi-lob, bounce/mine arm, residual cryo puddle (thin), charge mortar  
- Clear vs ST: Clear via cluster/mortar; ST via fat cell into elite feet then hose  
- Hybrid hooks: lob full-sats for Shatter; Gale wide hose cleans mortar leftovers  

### Path C — SHATTER (freeze payoff / burst)
“Freeze is the setup. The crack is the kill.”

- Spine: damage vs fully frozen, shatter fill, on-break bursts, Brittle windows, cryo match amp, optional kinetic spike on break  
- Clear vs ST: ST native on locked targets; clear via break cascades  
- Hybrid hooks: Gale paints faster into payoff; Cell dumps sat for instant crack windows  

### Path × verb matrix

```
                 GALE                    RIME CELL                 SHATTER
Hose             core delivery           paints after lob          builds freeze for crack
Lob              seeds packs to hose     core delivery             burst sat / break setup
Cryo sat         core paint              lob dump                  condition for payoff
Frostbite        core (light)            —                         hybrid crack bait
Shatter payoff   hybrid bait             lob breaks statues        core
Fog crumb        core exotic             lob seeds thin puddle     —
RMB claim        fog brush (rare)        lob / mortar / cluster    shatter pulse (rare)
```

---

## 7. Crowns & Sacred Cows

### Whiteout Curtain — Exotic (Gale crown)
- Hose becomes a **wide fog wall**: increased fan width / magnetism, bonus Cryo apply, short residual fog crumbs along the swept volume.  
- Soft obscurement **crumb only** (VFX + mild enemy accuracy penalty inside *your* fog) — **not** Rime Frozen Mist full AI untarget.  
- Mag drain slightly increased OR range slightly decreased (honest width tax — pick one in playtest; default **+drain**).  
- The namesake “paint the lane white” keystone.

### Glacial Bore — Exotic (Gale crown)
- Hold-M1 can run a **collimated long cryo lance** profile: longer range, narrower fan, higher per-target tick, reduced multi-tag.  
- Still continuous hose (not Solstice heavy beam, not charge gate).  
- Optional: ADS tightens further into Bore.  
- Cutting tool for elites/lanes without abandoning cone family.

### Cryo Mortar — Exotic (Rime Cell crown)
- RMB becomes **chargeable** (hold RMB): release fires super-lob with radius/damage/Cryo scaled by charge.  
- Tap RMB still quick-lob at baseline tax; full charge costs **increased mag tax**.  
- Airburst option at full charge (detonate above aim point / at range gate).  
- Teaches lob as ordnance without deleting instant punctuation.

### Scatter Cells — Exotic (Rime Cell crown)
- Lob splits into **N submunitions** on impact or mid-arc (cluster).  
- Each sub does reduced damage/Cryo; total sat ≥ single lob when all connect.  
- Mag tax +10–20% vs baseline lob.  
- Pack clear ordnance crown.

### Thermal Shock — Exotic (Shatter crown)
- When a fully frozen target **dies** or **shatter-procs** from your Whiteout damage: iceburst AOE (damage + Cryo crumb to neighbors).  
- Cap bursts/sec and radius; bosses reduced mult.  
- The “statue explodes into the pack” clip factory.

### Permafrost Latch — Exotic (Shatter crown)
- Fully frozen state from your apply **lasts longer** and/or **re-applies light Cryo** to nearby enemies when a latched target dies.  
- Freeze authority crown — pairs with Thermal Shock and Gale paint.  
- Duration caps; not permanent mission freeze.

**Sacred cows (do not cut without rewriting identity):**
- Parallel weapon (not Jackrabbit/Pesticide replace)  
- Hold-M1 cryo hose baseline  
- RMB mag-tax lob baseline  
- R = reload only  
- Strong native Cryo  
- Shared mag economy  
- Three peer paths; hybrids OK  
- E6 crowns above  
- No Rime plates/rink/blizzard spine  
- No Fire bounce-shell spine  

---

## 8. Full Upgrade List (~30 ship + backlog)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity  
Cell rule: Exotic shapes larger than others; all Exotics same cell count.  
Player-facing names below. API names assigned at implementation.  
Jackrabbit / Rime names are DNA only — **full rename** for parallel identity where needed.

------------------------------------------------------------------------------
PATH A — GALE
------------------------------------------------------------------------------

A-EX1. Whiteout Curtain — Exotic (crown)  
       Wide fog hose + residual crumbs + apply bonus; soft obscure crumb only.

A-EX2. Glacial Bore — Exotic (crown)  
       Long narrow continuous cryo lance profile while hosing.

A-EP1. Pressure Jets — Epic  
       +Hose range. Slightly −fan width OR neutral width — prefer slight width trim so range is the gift.

A-EP2. Supercooled Feed — Epic  
       +Cryo effectAmount on hose ticks. Faster time-to-freeze.

A-EP3. Frostbite Thread — Epic  
       Targets continuously in hose gain Frostbite stacks; at cap, bonus damage and/or bonus sat.  
       Stacks decay fast outside cone.

A-EP4. Drift Veil — Epic  
       When you stop hosing, emit a short forward residual cryo breath (fog crumb + light damage/apply).  
       Scales lightly with how long you were hosing this trigger pull (hybrid bait with mag discipline).

A-RA1. Rim Fans — Rare  
       +Fan width / target magnetism. Mild −range.

A-RA2. Coolant Recirc — Rare  
       While hosing, small chance per tick to refund 1 mag ammo.  
       Uptime economy; tune vs Deep Tank + tax discounts.

A-RA3. Planted Nozzle — Rare  
       While hosing: −move speed, +hose damage crumb, +DR crumb (Sturdy-lite).

A-RA4. Rime Kiss — Rare  
       First tick on a brain that is not chilled applies bonus Cryo amount (opener).

A-ST1. Nozzle Polish — Standard  
       Minor +hose damage.

------------------------------------------------------------------------------
PATH B — RIME CELL
------------------------------------------------------------------------------

B-EX1. Cryo Mortar — Exotic (crown)  
       Hold-RMB charge super-lob; tap still quick-lob; full charge airburst + higher tax.

B-EX2. Scatter Cells — Exotic (crown)  
       Lob cluster submunitions; total sat honest; tax up slightly.

B-EP1. Thin Shell — Epic  
       −Lob mag tax %. Pure economy for Cell path.

B-EP2. Hard Pack Cells — Epic  
       +Lob impact damage and/or radius. Boom denser.

B-EP3. Cryo Minelet — Epic  
       Lobs arm on settle; detonate when enemy enters trigger radius OR failsafe timeout.  
       Cap armed mines; not full Rime Frost Mines fantasy clone — lighter, gun-tied.

B-EP4. Secondary Burst — Epic  
       On lob detonate, hose gains a short window of +Cryo apply and +tick damage (cell→hose hybrid).

B-RA1. Arc Weight — Rare  
       +Lob throw force / less gravity feel; easier long lobs.

B-RA2. Cell Splitter — Rare  
       Lob bounces once then detonates (or micro-split on bounce). Without Scatter, single bounce only.

B-RA3. Magazine Primer — Rare  
       After a lob, next 1.5–2.5 s of hose drains mag slower (recoup window).

B-RA4. Deep Freeze Cap — Rare  
       +Lob Cryo effectAmount. Fatter sat dump.

B-ST1. Cell Lathe — Standard  
       Minor −lob mag tax crumb OR +lob radius crumb (pick one primary at impl — prefer tax crumb).

------------------------------------------------------------------------------
PATH C — SHATTER
------------------------------------------------------------------------------

C-EX1. Thermal Shock — Exotic (crown)  
       On kill/shatter-proc of fully frozen: iceburst AOE. Caps enforced.

C-EX2. Permafrost Latch — Exotic (crown)  
       Longer full-sat authority; death spreads light Cryo to neighbors.

C-EP1. Ice Pick — Epic  
       Outgoing damage matching this gun’s element (**Cryo**) increased (hose + lob + bursts you own).

C-EP2. Brittle Lacquer — Epic  
       Vs fully frozen: +damage and/or +shatter meter fill on your hits.

C-EP3. Flash Fracture — Epic  
       When you apply full-sat (enter fully frozen) to a target, deal a one-shot bonus damage spike (ignite-proc analogue for freeze).

C-EP4. Kinetic Thaw Spike — Epic  
       RMB or a short RMB-hold while aiming a fully frozen target: spend small mag tax to fire a **kinetic shatter pulse** along aim (non-Cryo or hybrid damage) that prefers shatter fill.  
       If too busy with Mortar charge, demote to “on lob impact vs fully frozen → bonus kinetic chunk” instead.  
       **Default v1:** lob impact vs fully frozen deals bonus kinetic chunk (simpler; no RMB conflict with Mortar).

C-RA1. Flash Freeze — Rare  
       Damaging a **wet** target with Whiteout systems applies **Frozen Solid** short hard CC.  
       Soft Splash synergy; ICD per target.

C-RA2. Flash Thaw — Rare  
       Damaging an **ignited** target: clear Ignite + Frozen/Cryo, apply **Wet**.  
       Soft Thermite/fire synergy; intentional cross-spice.

C-RA3. Sub Zero Feed — Rare  
       Applying full-sat refunds a small mag crumb (freeze economy). Stackable mild.

C-RA4. Cryo Ward Nozzle — Rare  
       While local player is chilled/frozen: −incoming damage crumb (self-risk mitigator if cards apply self-Cryo). Mild alone.

C-ST1. Crystalline Tips — Standard  
       Minor +damage vs fully frozen.

------------------------------------------------------------------------------
GENERIC / GUNFEEL
------------------------------------------------------------------------------

G-RA1. Deep Tank — Rare  
       +Magazine size. Slightly +reload duration.

G-RA2. Quick Vent — Rare  
       +Reload speed. Slightly −magazine size.

G-RA3. Dual Regulator — Rare  
       +Hose DPS slightly and −lob tax slightly, each half-strength of a dedicated card. Hybrid glue.

G-ST1. Reserve Coolant — Standard  
       +Ammo reserves crumb.

G-ST2. Gyro Gimbals — Standard  
       −Recoil / aim wander while hosing.

G-ST3. Thermal Sleeve — Standard  
       −Self Cryo application from your own hose/lob.

G-OD1. Boundary Incursion — Oddity  
       Increases upgrade grid size.

------------------------------------------------------------------------------
FROZEN 30 FOR V1 SHIP
------------------------------------------------------------------------------

EXOTIC (6)
  1  Whiteout Curtain
  2  Glacial Bore
  3  Cryo Mortar
  4  Scatter Cells
  5  Thermal Shock
  6  Permafrost Latch

EPIC (8)
  7  Pressure Jets
  8  Supercooled Feed
  9  Frostbite Thread
 10  Thin Shell
 11  Hard Pack Cells
 12  Ice Pick
 13  Brittle Lacquer
 14  Flash Fracture

RARE (10)
 15  Rim Fans
 16  Coolant Recirc
 17  Rime Kiss
 18  Arc Weight
 19  Magazine Primer
 20  Deep Freeze Cap
 21  Flash Freeze
 22  Flash Thaw
 23  Sub Zero Feed
 24  Deep Tank

STANDARD (5)
 25  Nozzle Polish
 26  Cell Lathe
 27  Crystalline Tips
 28  Reserve Coolant
 29  Gyro Gimbals

ODDITY (1)
 30  Boundary Incursion

------------------------------------------------------------------------------
BACKLOG (designed, not in first 30)
------------------------------------------------------------------------------

Gale
- Drift Veil  
- Planted Nozzle  
- Thermal Sleeve (promote if self-freeze grief shows up)  
- True ConeBullet volume migration  
- Ally-safe warm/cold aura thin support — keep off spine  

Rime Cell
- Cryo Minelet  
- Secondary Burst  
- Cell Splitter  
- Multi-lob double tap exotic  
- Lob leaves thin ice puddle (not rink)  

Shatter
- Kinetic Thaw Spike as real RMB pulse (if Mortar charge UX allows priority table)  
- Cryo Ward Nozzle  
- Shatter cascade on limb break  
- Dual-element “cryo then kinetic alternating ticks”  

Generic
- Quick Vent, Dual Regulator  
- Move-speed-while-hosing  
- Shared Processing thin secondary bus — rejected as path  

Explicit non-goals on this kit (belong elsewhere)
- Shattering Armor plates → **Rime Charge**  
- Ice Rink / Blizzard / Frozen Mist full fantasies → **Rime Charge**  
- Fire bounce shells / immolation → **Jackrabbit**  
- Soft heat meter siege beam → **Thermal Solstice**  
- Sonic scarce thump → **Zephyr**  

---

## 9. Example Builds

Gale whiteout painter  
  Whiteout Curtain + Glacial Bore + Supercooled Feed + Frostbite Thread  
  + Rim Fans + Rime Kiss + Coolant Recirc + Nozzle Polish + Deep Tank  
  Sweep packs white; Bore elites; refund ticks keep the fog up.

Cell ordnance  
  Cryo Mortar + Scatter Cells + Thin Shell + Hard Pack Cells  
  + Arc Weight + Deep Freeze Cap + Magazine Primer + Cell Lathe  
  Lob-first clear; hose cleans stragglers in Primer window.

Shatter statue cracker  
  Thermal Shock + Permafrost Latch + Ice Pick + Brittle Lacquer + Flash Fracture  
  + Sub Zero Feed + Crystalline Tips + Supercooled Feed (hybrid)  
  Paint or lob to full-sat → crack → iceburst chains.

Hybrid winter freak (showcase)  
  Whiteout Curtain + Cryo Mortar + Thermal Shock  
  + Permafrost Latch + Frostbite Thread + Thin Shell + Brittle Lacquer  
  Fog paint, fat cells, exploding statues — allowed under hybrid law.

Budget controller (mag anxiety skill)  
  Thin Shell + Magazine Primer + Coolant Recirc + Deep Tank  
  + Pressure Jets + Rime Kiss  
  Teaches shared mag without requiring all crowns.

Co-op with Rime Charge partner (soft)  
  You: Gale paint + Flash Freeze if wet  
  Them: rink/plates/blizzard  
  No card requires the mod; README tips only.

---

## 10. Economy & Tuning Rules of Thumb

- Power budget lives in **uptime × aim × freeze state × lob punctuation** — not only raw tick stickers.  
- Lob tax must make “only RMB forever” weaker than mixed play for general packs.  
- Hose empty-air drain must hurt enough to punish brain-off spray.  
- Full-sat time on grunts: sub-second focus with Supercooled; baseline still readable.  
- Elites/bosses: slow OK; Flash Freeze hard-CC ICD; Thermal Shock radius/boss mult capped.  
- Coolant Recirc + Deep Tank + Thin Shell can infinity-winter — nerf refunds before fantasy spikes.  
- Whiteout Curtain obscure crumb must not equal Rime Mist untarget.  
- Glacial Bore must not out-Solstice Solstice at long range — keep primary cone family range band.  
- Scatter total sat ≥ single lob when most subs connect; missing cluster is the skill tax.  
- Mortar full charge tax > tap tax; tap remains useful.  
- Flash Freeze/Thaw are spice — builds must function without wet/fire partners.  
- Ally damage off or trivial on hose/lob/bursts.  
- Self-Cryo low; Thermal Sleeve backlog if grief appears.

### Playtest acceptance tests (pass/fail feel)

1. **Teaching:** Zero upgrades — hose freezes a grunt, lob dumps a clump, reload matters.  
2. **Identity:** Does not feel like ADS Jackrabbit or Rime on a trigger.  
3. **Mag law:** Lob-only and hose-only both viable but mixed feels best.  
4. **Gale literacy:** Curtain/Bore change how the cone *reads*, not just +% damage.  
5. **Cell literacy:** Mortar/Scatter make RMB a weapon system.  
6. **Shatter literacy:** Fully frozen targets die faster in an obvious way with path cards.  
7. **Anti-Rime-clone:** No plates/rink/blizzard required for a complete Whiteout fantasy.  
8. **Anti-infinite:** Recirc + Deep Tank + Thin Shell still reloads sometimes.  
9. **Boss-safe:** Freeze slow + bursts don’t softlock bosses.  
10. **Co-op soft:** Wet partner Flash Freeze feels great; not mandatory.

---

## 11. Status & Counter Split (explicit)

| Status / counter | Role on this gun | Baseline? | Notes |
|------------------|------------------|-----------|-------|
| Cryo EffectType | Native hose + lob spine | Yes | Strong |
| Fully frozen slow | Primary CC read | Yes | Full-sat |
| Frozen Solid hard CC | Flash Freeze rare | Card | Wet synergy |
| Frostbite stacks | Gale light stack | Cards | Not full status effect type |
| Shatter meter / break | Shatter payoffs | Cards | Uses real shatter where possible |
| Fog crumb | Residual paint | Curtain / cards | Not Rime Mist |
| Fire | Flash Thaw spice only | Card | Not spine |
| Water / wet | Flash Freeze condition | External / card | Soft |
| Heat (Solstice) | Not identity | No | |
| Plates / rink / blizzard | Not identity | No | Rime owns |
| Mark/Transfer/Brand | Not identity | No | |

---

## 12. Strengths, Weaknesses & Co-op

### Strengths
- Clearest full-time **cryo hose primary** in the parallel catalog  
- Baseline complete: cone + lob + freeze literacy  
- Three dialects: paint / ordnance / crack  
- Shared mag creates meaningful decisions  
- Soft elemental cross-play (wet/fire) without deps  
- Co-op: you freeze; allies dump into statues; Rime partner owns floor  

### Weaknesses
- Short range vs beam/sniper kits  
- Mag anxiety if lob-greedy  
- Weaker “one authorization boom” than charge heavies  
- Not a terrain god (by design)  
- Shatter path weaker if you never secure full-sat  
- Bosses resist fancy CC; must still tick  

### Co-op
- Freeze is a gift — default ally-safe  
- Lob radial ally-safe  
- Thermal Shock must not grief allies  
- You are setup + hose battery, not Salvo heal support  

### Failure modes to avoid

| Failure | Mitigation |
|---------|------------|
| Pesticide reskin | M1 full-time hose; Cryo; RMB lob; no bounce primary |
| Rime on a gun | No plates/rink/blizzard spine |
| Infinite mag | Cap refunds; tax floors |
| Mortar deletes tap lob | Tap always available; charge optional |
| Curtain = full mist untarget | Crumb only |
| Bore = free Solstice | Range band + primary slot power |
| Flash Freeze perma-stun | Short D + ICD |
| Thermal Shock chain wipe bosses | Boss mult + rate cap |
| 30 upgrades / 1 forced build | Standards universal; minimal path locks |
| RMB conflict Mortar vs Shatter pulse | Shatter pulse demoted to lob-vs-frozen chunk in v1 |

---

## 13. Visual, Audio & Thematic Design

### Appearance
- SAXON coolant projector: insulated tank backpack-or-underbarrel cell drum, nozzle with rime vents, hazard stripes, fungal frost etch (“AUTHORIZE WINTER”)  
- Hose: blue-white fog cone; denser at muzzle; rime particles; enemies gain frost crust as sat climbs  
- Full-sat: heavier crust + slow body language (game freeze read)  
- Lob: translucent cryo cell / canister arc; impact rime burst  
- Curtain: wider milky sheet  
- Bore: tighter bright-cyan core stream  
- Mortar: charging frost ring on nozzle; airburst pop  
- Scatter: cell cracks into shards mid-flight or on impact  
- Thermal Shock: crystalline burst / glass-ice detonation  
- Latch: lingering frost tether motes between nearby enemies on death spread  

### Sound
- Hose start: valve open + cold hiss  
- Loop: pressurized fog roar; intensity stable (not Solstice heat whine)  
- Full-sat proc on target: crisp freeze catch (use game feedback where possible)  
- Lob: *thunk-chunk* + arc whistle  
- Mortar charge: rising crystal RTPC  
- Scatter: multi-pop  
- Shockburst: glass shatter thump  
- Reload: tank clack + coolant sigh  

### Flavor / codex line
> **Whiteout**  
> Continuous cryo projector. Hold fire to hose a frost cone. Secondary fires a mag-fed rime cell.  
> Gale upgrades own the cone. Rime Cell upgrades own the lob. Shatter upgrades crack the freeze.

---

## 14. Implementation Notes (for later — not this pass)

### 14.1 Gear registration
- Follow weapon template in this repo: clone suitable Gun base (prefer something with continuous fire comfort; **not required** to subclass BounceShotgun).  
- GearInfo high-range id, APIName `whiteout`, behaviour component, SpawnGear stamp, CreateUpgrade pool.  
- Plugin: GUID `sparroh.whiteout`, MycoMod **IsSandbox**.  
- Persistence: stable gear id; register before/during PlayerData gear bind.  
- Do **not** remove or patch Jackrabbit / Pesticide / Rime Charge.

### 14.2 Suggested IDs (placeholders — verify free at impl)
| Piece | Draft |
|-------|--------|
| Gear id | **87700** (after Heaven Piercer 87600 / Needle 87530; avoid 928–931xx) |
| Upgrades | **87701–87730** |
| APIName | `whiteout` |
| Display | Whiteout |

### 14.3 Behaviour host
`WhiteoutBehaviour` (rename from example) with `Data` struct:

```
// Hose
float hoseDamagePerSecond;
float hoseCryoPerSecond;
float hoseRange;
float hoseTargetMagnetism;
float hoseTickInterval;          // 0.10
float hoseMagDrainPerSecond;
float hoseWidthMult;             // Curtain / Rim Fans
float hoseRangeMult;             // Pressure Jets / Bore
bool glacialBore;
float boreRangeMult;
float boreWidthMult;
float boreDamageMult;

// Lob
float lobMagTax;
float lobDamage;
float lobCryoAmount;
float lobRadius;
float lobSpeed;
float lobGravity;
bool cryoMortar;
float mortarChargeDuration;
float mortarMaxTaxMult;
float mortarRadiusMult;
bool scatterCells;
int scatterCount;
float scatterDamageMult;
bool cryoMinelet;
float mineTriggerRadius;
float mineFailsafe;

// Gale extras
bool whiteoutCurtain;
float curtainResidualDuration;
float curtainObscureCrumb;       // mild only
bool frostbiteThread;
int frostbiteMaxStacks;
float frostbiteBonus;
bool driftVeil;

// Shatter
bool thermalShock;
float shockRadius;
float shockDamageMult;
float shockRateCap;
bool permafrostLatch;
float latchDurationMult;
float latchSpreadCryo;
float icePickMult;
float brittleMult;
float brittleShatterFill;
bool flashFracture;
float flashFractureDamage;
bool flashFreeze;
float flashFreezeDuration;
bool flashThaw;
float subZeroRefund;

// Economy / generic
float magSizeMult;
float reloadMult;
float selfCryoMult;
float recircChance;
```

Prefab snapshot restore on upgrade Remove.

### 14.4 Hose hooks
- Owner `Update`/`OnActiveUpdate` while M1: tick timer, mag drain, `IBullet.RaycastTargetsAndSurface` fan (copy BounceShotgun flamethrower block structure).  
- DamageData effect = Cryo; flags DoT|AOE.  
- VFX/audio start/stop on hose enable (Pesticide Start/Stop flamethrower RPC pattern as reference — implement cleanly on behaviour, not by patching BounceShotgun).  

### 14.5 Lob hooks
- RMB input → if mag >= tax, spend, spawn pooled grenade/cell bullet (GrenadeBullet pattern from BounceShotgun `FireLob_Rpc`, Cryo instead of Fire).  
- Mortar: hold RMB charge meter; release fires; tap path if charge < threshold.  
- Priority: if Mortar equipped, hold-RMB charges; click-release under threshold = tap lob.

### 14.6 RMB priority table

1. Cryo Mortar charge handling (if equipped)  
2. Else baseline / upgraded lob  
3. No Shatter RMB pulse in v1 (avoid conflict)  

### 14.7 Shatter / status hooks

| Hook | Use |
|------|-----|
| OnDamageTarget / OnBeforeDamage | Brittle, Ice Pick, Frostbite, Bore mults |
| OnSaturateTarget (Cryo) | Flash Fracture; Sub Zero; Latch bookkeeping |
| OnKillTarget | Thermal Shock; Latch spread; economy crumbs |
| Damage vs wet/ignited | Flash Freeze / Flash Thaw |
| Update | Hose ticks; residual fog; mine triggers; mortar charge |

### 14.8 Multiplayer
- Sandbox mod; all clients need the same plugin  
- Damage/Cryo follow IDamageSource authority  
- Hose VFX replicate via simple start/stop RPCs  
- Lob spawn networked like other grenade bullets  

### 14.9 VFX / audio priority
1. Hose cone loop + freeze crust read  
2. Lob arc + impact  
3. Full-sat feedback  
4. Curtain width read  
5. Bore core stream  
6. Mortar charge  
7. Scatter pops  
8. Thermal Shock burst  

### 14.10 Vanilla coexistence
- Different gear id, APIName, display name, upgrade ids  
- May reference Jackrabbit flamethrower VFX as placeholder  
- Balance as peer primary hose, not strict Pesticide stat clone  

---

## 15. Deliberate Non-Goals

- Not replacing or unhooking Jackrabbit / Pesticide  
- Not replacing Rime Charge or stealing plates/rink/blizzard/Cyclone/Oven spine  
- Not Thermal Solstice heat-beam heavy  
- Not Zephyr scarce sonic thump  
- Not ADS-gated stream as primary identity  
- Not hold-R lob (RMB locked)  
- Not separate lob ammo pool (shared mag locked)  
- Not ConeBullet requirement for v1 (raycast fan locked)  
- Not hard mod dependency on Splash/Thermite/Rime  
- Not requiring custom Unity prefab for v1 (runtime clone OK)  

---

## 16. Open Tuning Questions (playtest, not design blockers)

1. Mag size 200 vs 320 vs hose drain rate.  
2. Lob tax 25 vs 40 — how many lobs per mag feels right.  
3. Time-to-full-sat on grunt/elite with baseline vs Supercooled.  
4. Curtain obscure crumb strength (VFX-only vs mild accuracy).  
5. Bore max range before it steals Solstice fantasy.  
6. Mortar charge time 0.6–1.2 s and max tax mult.  
7. Scatter count 3–6 and damage split.  
8. Thermal Shock radius and boss multiplier.  
9. Latch duration mult and spread amount.  
10. Whether Frostbite is needed in v1 or backlog if sat alone reads.  
11. Flash Freeze duration + ICD.  
12. Exact hex shapes for 30 upgrades — author during implementation.  
13. Gear id locked to **87700** (93000 = Junk Flinger; 92800 = Wrench; 92900 = Blade).  


14. Placeholder mesh: clone which gun until art exists.  
15. User “unsure” sacred cows — any post-playtest locks.

---

## 17. Success Criteria / Player Fantasy Checklist

- [ ] Hold-M1 cryo cone feels like a complete primary hose with zero upgrades  
- [ ] RMB lob is satisfying punctuation and costs meaningful mag  
- [ ] R reloads; no hold-R lob confusion  
- [ ] Baseline Cryo full-sats focused grunts without Shatter path  
- [ ] Fully frozen slow is readable on enemies  
- [ ] Whiteout Curtain makes the lane *look* and play wider  
- [ ] Glacial Bore makes elite focus feel like a cutting tool  
- [ ] Cryo Mortar charge lob feels like ordnance without deleting tap lob  
- [ ] Scatter Cells create cluster clear clips  
- [ ] Thermal Shock freeze-break bursts create pack chain moments  
- [ ] Permafrost Latch makes freeze stick and spread on kills  
- [ ] Hybrid Curtain + Mortar + Shock feels intentional  
- [ ] Does not play as Jackrabbit ADS flame or Rime Charge  
- [ ] Soft wet/fire cards feel optional and fun  
- [ ] ~30 upgrades, E6 equal large exotics, three peer paths  
- [ ] SAXON coolant-projector tone reads industrial cold, not wizard staff  

---

## 18. Research Anchors (v1 doc)

| Source | Use |
|--------|-----|
| Wiki Pesticide | Hold AIM flamethrower; damage/fire/range/radius rolls — **contrast** (ours is M1 full-time Cryo) |
| `BounceShotgun` decompile | Flamethrower tick 0.1s, raycast fan, Start/Stop VFX RPC, charge lob GrenadeBullet Fire pattern, mag interactions |
| `ConeBullet` decompile | Continuous cone volume alternative (post-v1) |
| `EffectType.Cryo = 10` | Real element |
| Rime Charge design doc | Cryo language, shatter/wet/fire spice, what **not** to steal (plates/rink/mist/blizzard) |
| Thermal Solstice design doc | Continuous weapon doc structure; heat contrast |
| Zephyr design doc | Cone primary structure; scarce blast contrast |
| PesticideRework | Existing QoL on vanilla flame — coexistence note only |

---

## 19. Naming & Presentation

| Slot | Value |
|------|--------|
| Display name | **Whiteout** |
| Internal / API | `whiteout` |
| Design nickname | Cryothrower (folder / notes) |
| Short description | *Cryo-element projector. Hold fire for a continuous frost cone. Secondary lobs a mag-fed rime cell. Upgrades fork into gale hose mastery, cell ordnance, or freeze-shatter payoffs.* |
| Thunderstore name (later) | `Whiteout` |
| GUID (later) | `sparroh.whiteout` |
| Folder today | `.new.Cryothrower` |

### SAXON marketing blurb (final draft)

> SAXON W-9 Whiteout — Portable winter for employees who solve lane control by lowering the thermostat until the problem stops sprinting.  
> Baseline: cryo hose + mag-fed cell lob. Aftermarket: fog curtains, glacial bores, mortar cells, cluster munitions, and shatter payoffs that turn statues into shrapnel.  
> Not a bounce shotgun. Not a floor grenade. Not a siege laser.  
> “If you’re still hosing empty air, the magazine is not the problem.”

---

## 20. Locked Review Decisions (2026-08-21)

| Decision | Lock |
|----------|------|
| Form factor | Continuous cryo cone primary + RMB lob |
| Player-facing name | Whiteout |
| Product shape | Parallel new primary |
| Slot | Primary |
| Paths | Gale / Rime Cell / Shatter |
| Alt-fire | RMB |
| Lob economy | Shared mag tax |
| Cone v1 | Raycast fan |
| Rime synergy | Soft only |
| Crowns | Whiteout Curtain, Glacial Bore, Cryo Mortar, Scatter Cells, Thermal Shock, Permafrost Latch |
| Ship pool | Frozen 30 listed above |
| Working APIName | whiteout |
| Working GUID | sparroh.whiteout |
| MycoMod | IsSandbox at implementation |
| Doc file | Whiteout-DesignDoc.md |
| Tone | SAXON industrial coolant projector |
| User locks | Name Whiteout; RMB; shared mag; raycast fan; paths OK; soft Rime; sacred cows TBD |

---

## 21. Changelog (Design Doc)

| Date | Change |
|------|--------|
| 2026-08-21 | Initial full design from plan-mode research + user locks. Identity: cryo cone primary **Whiteout** with RMB mag-tax lob. Paths Gale / Rime Cell / Shatter. E6 crowns. Frozen 30. Anchors: BounceShotgun Pesticide/lob DNA, ConeBullet note, EffectType.Cryo, Rime/Solstice/Zephyr contrast, wiki Pesticide. Implementation deferred. |

---

## 22. Implementation checklist (post-design)

- [ ] Rename plugin/csproj/thunderstore from template → Whiteout (when coding starts)  
- [ ] WhiteoutBehaviour.Data fields from §14.3  
- [ ] Retune cloned GunData (continuous feel, Cryo, mag, range)  
- [ ] Hose raycast fan + mag drain + VFX loop  
- [ ] RMB lob + shared mag tax  
- [ ] Whiteout Curtain + Glacial Bore  
- [ ] Cryo Mortar + Scatter Cells  
- [ ] Thermal Shock + Permafrost Latch  
- [ ] UpgradeRegistration frozen 30  
- [ ] Persistence + SpawnGear stamp (do not touch Jackrabbit/Rime)  
- [ ] Playtest pass on §10 / §16 knobs  

---

*End of design document. Next step when ready: rename template identifiers to Whiteout and implement baseline cryo hose + RMB lob only, then layer Gale crowns → Cell crowns → Shatter crowns → remaining grid.*
