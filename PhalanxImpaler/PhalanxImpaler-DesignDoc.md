# Phalanx Impaler — Design Document (v1)

> Status: **Design only** — implementation follows this doc and MeleeRework hooks.
> Package / Thunderstore name (planned): **PhalanxImpaler** / **SpearMelee**
> GUID (planned): `sparroh.phalanximpaler`
> MycoMod flags: **IsSandbox** (gameplay rules; melee kit)
>
> Product shape: **Melee kit** (`GearType.Melee`) that registers through **MeleeRework**
> extension hooks. Not a primary weapon. Not a Fists reskin. Not Boarding Trident.
>
> Sibling docs (do not absorb; contrast locks below):
>   - MeleeRework — slot platform, tap/hold V, Fists kit, extension API
>   - Stalker's Blade — short dual slash + throw knife + crouch Ambush (contrast)
>   - Blood Carver — continuous saw + blood + heavy feast (primary until convert)
>   - Saxonite Wrench — discrete slam + shockwave + pull (primary)
>   - Boarding Trident — multi-prong *rifle* (name/fantasy collision only — different slot)
>   - Hard-Light Constructor / Bruiser — hard-light architecture & ability shield (Aegis March contrast)
>   - Shocklance — charge poke + Auger lunge + Fisticuffs (DNA borrow, not copy)
>   - DMLR Rework — Mark / Expose vocabulary precedent (thin gun window only)
>
> Wiki / vanilla DNA borrowed (rename on spear grid; no hard character deps):
>   - Deflector / Charge Shield → frontal buckler bias
>   - Auger Subroutine / Lunge Latch → stand/charge lunge thrust (not slide Railblade)
>   - Skewer / Swing Through → multi-target thrust pierce
>   - Fisticuffs → inverse thin gun window after throw/impale (not Payroll)
>   - Handspring stays Wrangler; Fists owns Pile Driver — spear does not steal
>   - Bruiser Hard-Light Projector → contrast only; Aegis March is carried wall, not deploy CD
>
> Vanilla anchors (implement against decompile, do not invent APIs):
>   - MeleeRework: tap V quick-melee, hold V equip, `IMeleeKit` / RegisterKit
>   - Guard / damage resist patterns (Fists Guard, Carver Deflector)
>   - Projectile throw + retrieve (Stalker's Blade family)
>   - Hard-light VFX language (Bruiser / Constructor) for Aegis March presentation only

---

## 1. High Concept / Fantasy

**Phalanx Impaler** is SAXON's issue pike for employees who believe the correct range is
"just outside their teeth, just inside your plate."

You do not saw the pile. You do not crack the floor. You do not crouch-vanish.
You **hold the line, write holes at long melee reach, and throw the shaft when the line
moves.** Baseline is honest hybrid melee — a three-hit thrust string, a frontal buckler
on RMB, a javelin on R, no ammo, no reload. The grid forks the kit into lancer skewers,
hoplite plate (including a mobile hard-light tower), or javelin pin-and-retrieve.

**One-liner:** *Longest poke in the kit. Raise the plate. Throw the shaft. Retrieve and do it again.*

**Element baseline:** Normal (kinetic). Element tips are **opt-in cards only** — no forced chemistry.

---

## 2. Role & Fantasy in the Arsenal

| | |
|--|--|
| **Slot** | **Melee** (`GearType.Melee`) via MeleeRework registration hooks |
| **Range** | Longest melee kit reach + mid **javelin** throw |
| **Role** | Line-holder / gap-closer hybrid — thrust combo clear, frontal brace, mid-range pin |
| **Gap filled** | Fists = punch + omni Guard + economy. Blade = short assassin + knife throw + crouch Ambush. Carver = saw. Wrench = slam/wave/pull. Shocklance = gun poke + Auger. **Nothing owns long-reach thrust combo + offhand buckler + javelin hybrid on a melee kit.** |
| **Synergies** | MeleeRework slot; stowed primary after throw/impale window; Bruiser hard-light soft duo (different verbs); CQC mission modifier; co-op formation crumbs |
| **Not trying to be** | Fists brawler, Blade stalker, Carver blender, Wrench AOE clear, Boarding Trident rifle, Bruiser ability replacement, Payroll ammo engine |

**Product shape:** New melee kit (**Phalanx Impaler**). Default fallback remains Fists.
Does **not** replace vanilla MeleeGear punch in-place; registers as its own catalog entry.

---

## 3. Design Pillars

1. **Longest honest melee reach in the kit roster.** Limitation is volume geometry, not soft falloff tax inside reach.
2. **Hybrid is baseline.** Thrust + buckler + javelin all exist empty-grid; paths crown them, they do not unlock existence.
3. **RMB is shield. R is throw.** Melee has no reload — R is free for javelin. Do not steal gun ADS while gun-out.
4. **Three-hit string is the melee gunfeel.** Thrust → thrust → sweep/finisher. Finisher is the readable payoff.
5. **Buckler is frontal, not Fists Guard.** Projectile-lean plate; weaker from rear/flank; bash on release / M1-from-block.
6. **Discrete thrusts, not a saw or slam.** Faster recovery than Wrench, longer than Blade, zero continuous tick DPS.
7. **No ammo. No reload.** Cadence = combo recovery + throw retrieve + guard commitment.
8. **Three peer paths + glue exotic + Hoplite second exotic.** Lancer / Hoplite / Javelin. Hybrids intended.
9. **Aegis March is a carried wall, not Bruiser Projector.** Mobile with the wielder; no prison/dome/cube kit copy.
10. **Discarius Protocol is the signature Oddity** — spear-off, shield-only identity flip.
11. **Thin economy.** Fists owns Payroll. Impaler may open a short gun window after throw/impale only.
12. **~30 upgrades;** exotic shapes larger than others; each exotic the **same** cell count.
13. **Don't steal sibling niches.** No crouch Ambush, no blood meter, no shockwave primary loop, no Pile Driver, no slide Railblade, no Payroll.
14. **Self-contained v1.** Hard dep: MeleeRework hooks when that platform exists. Soft only: Bruiser VFX literacy, Shocklance DNA, OmniMovement.
15. **On-hit / spatial / state verbs > flat % stickers.**

---

## 4. Locked Decisions

| Decision | Lock |
|----------|------|
| Display name | **Phalanx Impaler** |
| APIName | `phalanx_impaler` |
| Slot | Melee (MeleeRework kit) |
| Baseline damage type | **Normal / kinetic** |
| Innate element | **None** |
| Ammo / magazine / reserve | **None** |
| Reload | **None** — R is **throw**, not reload |
| M1 | 3-hit thrust string (discrete) |
| RMB | **Hold = buckler guard** (full equip only) |
| R | **Javelin throw** (full equip only) |
| Baseline shield | Modest frontal DR + projectile bias |
| Combo shape | 3-hit; finisher on hit 3 |
| Throw while gun out | **No** |
| Shield while gun out | **No** |
| Economy | **Thin** — no Payroll-class engine |
| Heavy feed | **Out of v1** |
| True Bruiser Projector clone | **Out** — Aegis March contrast only |
| Pile Driver / Handspring on spear | **Out** |
| Slide lunge (Railblade) | **Out** — stand/charge lunge only |
| 5th exotic glue | **Shaft & Plate** |
| Hoplite 2nd exotic | **Aegis March** (mobile hard-light tower) |
| Signature Oddity | **Discarius Protocol** (shield-only mode) |
| Product | New melee kit, not primary, not Fists patch |

---

## 5. Melee Roster Differentiation

| | **Fists** | **Stalker's Blade** | **Blood Carver** | **Saxonite Wrench** | **Phalanx Impaler** |
|--|-----------|---------------------|------------------|---------------------|---------------------|
| Slot (v1) | Melee | Melee | Primary (convert later) | Primary (convert later) | **Melee** |
| Cadence | Punch | Dual slash + knife | Hold-M1 saw ticks | Tap / charged slam | **Thrust string + javelin** |
| Stance / RMB | Guard (omni DR) | Throw knife | Blood spend | Gravity pull | **Buckler (frontal)** |
| R | Overload if card | Unused baseline | Reload / Hold-R snare | Unused baseline | **Throw javelin** |
| Resource | None | Mark / opener / optional Poison | Blood stacks | Torque (momentary) | Pin / combo index / optional Aegis heat |
| Crowd tool | Size + multi-hit | Railblade light pierce | Pull + saw volume | Shockwave + pull | **Long poke + finisher sweep + throw pin** |
| Fantasy | Brawler economy / tempo | Assassin approach → execute | Harvest → spend → heavy | Gravity clear / freeze-shatter | **Hoplite hybrid line-hold** |
| Sustain | Guard DR | Soft Vanish only if built | Blood DR / chunks | Minimal / cryo ward | Frontal plate; Aegis March if built |
| Reach | Mid melee | Shortest | Short saw | Short–mid slam | **Longest melee** |

---

## 6. Shared Framework (events upgrades speak)

Emit through MeleeRework-compatible host (`PhalanxImpalerBehaviour` / kit router):

| Event / state | Fired when | Used for |
|---------------|------------|----------|
| **OnMeleeHit** | Quick or full thrust/bash damages enemy | Stacks, pin refresh, thin economy |
| **OnMeleeKill** | Spear/bash/throw kill | Retrieve, tempo, thin windows |
| **OnThrust** | M1 thrust connects (not bash) | Lancer spine |
| **OnComboStep** | Hit index 1 / 2 / 3 in string | String cards, finisher gates |
| **OnComboFinisher** | Hit 3 connects (or Discarius bash finisher) | Impaler's Creed, sweep payoffs |
| **OnLunge** | Lunge thrust commits | Lancer crown supports |
| **OnGuardStart / End** | RMB guard edge | Hoplite spine |
| **WhileGuarding** | Each damage instance under guard | DR, Aegis heat, brace cards |
| **OnPerfectBrace** | Guard absorbs projectile / heavy chip cleanly | Bash empower, throw-from-guard |
| **OnBash** | Shield bash connects | Hoplite / Discarius |
| **OnThrow** | R javelin (or Discarius disc) leaves hand | Javelin spine |
| **OnThrowHit** | Projectile connects | Pin, retrieve, Warhead |
| **OnRetrieve** | Shaft/disc returns | Shaft & Plate, tempo |
| **ShaftOut / DiscOut** | Thrown state active | Profile swap |
| **AegisActive** | Aegis March wall up | Mobile wall rules |
| **DiscariusActive** | Oddity shield-only mode | Remap inputs / dead stats |

Gun windows from Impaler cards buff *stowed/equipped primary* briefly — thin and hybrid-flavored, not Fists Payroll.

---

## 7. Core Mechanics & Gunfeel

### 7.1 Baseline kit

| Trait | Draft / intent |
|-------|----------------|
| Fire mode | Discrete 3-hit thrust string — **not** continuous saw |
| M1 | Thrust 1 → Thrust 2 → Finisher sweep (wider, slower, more damage) |
| RMB hold | Frontal buckler guard |
| RMB release after absorb / M1-from-block | Shield Bash |
| R | Javelin throw |
| Ammo | **None** |
| Element | Normal |
| Reach | **Longest** melee kit (draft noticeably > Fists; playtest) |
| Size | Medium thrust volume; finisher wider; anti-pack without Carver AOE |
| Falloff | **None** inside legal reach |
| Damage | Strong single-line poke; multi-target needs finisher / lunge / cards |
| Movement | Full move while equipped; mild slow while guarding |
| Model / audio | Industrial pike + offhand buckler; metal *shink* on thrust; plate *thunk* on brace; javelin whistle; retrieve snap. Borrow closest melee until custom art |

### 7.2 Why no ammo / reload

Same lesson as Saxonite Wrench / Stalker's Blade: melee fantasy must not soft-lock mid-line because a cell ran dry. Cadence pressure = whiff recovery, throw-retrieve, and guard commitment. **R is throw identity**, not a wasted reload bind.

### 7.3 Input contract (MeleeRework)

| State | Tap V | Hold V (≥ ~0.25s) | M1 | RMB | R |
|-------|-------|-------------------|----|-----|---|
| Gun out | **Quick thrust** (long poke); stay on gun | **Equip Phalanx Impaler** | Gun | Gun ADS/alt | Gun reload |
| Spear equipped | Quick thrust in-place | Refresh / flourish | **3-hit string** | **Hold: Guard** | **Throw javelin** |
| Shaft out (thrown) | Short poke / bash-forward | — | Shortened string or bash bias | Guard (buckler still up) | Throw blocked until retrieve |
| Discarius mode | Quick bash | Equip plate-only | Bash string | Guard | **Throw disc** |

### 7.4 Quick melee (tap V)

- Does **not** long-term change weapon slot (MeleeRework quick profile).
- Uses equipped kit's quick thrust (Phalanx Impaler if selected) — **long poke**, best gun-out finisher reach in the melee roster.
- No throw, no shield on quick-V (gun still out; ADS/reload ownership stays clean).
- Discarius: quick bash instead of thrust.

### 7.5 Full equip (hold V)

- Stow previous gun; pike in main hand, buckler in offhand; full M1/RMB/R map.
- HUD: optional combo pip (1–3), shaft-out pip, Aegis heat if exotic, Discarius glyph if Oddity.
- Exit: normal weapon swap / shoot primary bind per MeleeRework.

### 7.6 Three-hit thrust string (baseline)

| Step | Draft | Intent |
|------|-------|--------|
| Hit 1 | Fast straight thrust; modest damage; establishes string | Opener poke |
| Hit 2 | Slightly stronger / same speed; keeps pressure | Link |
| Hit 3 **Finisher** | Wider sweep or heavy tip-punch; highest damage; longer recovery | Readable payoff |
| Whiff | String can continue or soft-reset on long whiff (prefer: allow 1 whiff buffer, then reset) | Forgiveness without infinite air-swing |
| Interrupt | Taking heavy hit / guard cancel / throw / stow resets string | Clarity |
| Timing | Distinct cadence — not mash-as-saw; small commit windows | Skill toy empty-grid |

**Sacred cow:** 3-hit string exists with zero upgrades. Cards improve tempo, damage, finisher rules, or add lunge into the string — they do not invent combos from nothing.

### 7.7 RMB — Buckler Guard (baseline)

**Only while fully equipped** (or Discarius equipped).

| Piece | Draft |
|-------|--------|
| Input | Hold RMB (Aim) |
| Shape | **Frontal cone / plate** — strong in front, weak behind/flank |
| Baseline DR | ~20–30% frontal vs most damage (playtest; below Fists peak turtle if both stacked conceptually) |
| Projectile bias | Extra nullify/deflect **chance** vs projectiles in frontal cone (modest empty-grid; Deflector DNA lite) |
| Move | Mild slow while guarding (~15–25%) so it's a stance |
| Duration | As long as held; no baseline stamina meter (Aegis March may add heat) |
| Offensive | No free chip; **Shield Bash** is the offensive guard verb |
| Visual | Offhand plate up; clear audio bed; optional hard-light rim only if Aegis equipped |
| Cancel | Release RMB; M1 bash/thrust; throw; stow |

**What Guard is NOT:** Fists omni brace, Carver full deflector saw shield, Bruiser placed projector, infinite safety.

### 7.8 Shield Bash (baseline)

| Piece | Draft |
|-------|--------|
| Triggers | Release RMB shortly after absorbing damage **or** M1 while guarding (breaks guard into bash) |
| Effect | Short-range frontal bash — modest damage, light stagger/knockback |
| Empower | Perfect Brace (ate a projectile / heavy chip cleanly) → stronger bash (Hoplite cards raise) |
| Recovery | Short; can re-guard after |
| Discarius | Bash is the primary M1 identity — string of bashes replaces thrusts |

### 7.9 R — Javelin Throw (baseline)

| Piece | Draft |
|-------|--------|
| Input | Tap R while **fully equipped** |
| Projectile | Mid-range fast throw (longer than Blade knife; not a sniper lob) |
| Damage | Modest–solid vs M1 finisher; meaningful on pin setups |
| Pin | **Soft pin / hard slow** on hit (draft 0.4–0.7s grunts; bosses reduced slow only) |
| Cost | **Shaft out** — melee becomes shortened poke / bash-bias profile until retrieve |
| Retrieve | On throw hit, on melee kill, or miss timer (~2.5–3.5s) — shaft snaps back |
| CD | Soft gated by shaft-out more than a long cooldown; tiny throw recovery ~0.25–0.35s |
| Fail | Throw with shaft already out: blocked (unless Returning Warhead / support says otherwise) |
| Gun-out | **No throw** — preserves gun reload on R |
| Guard | Can throw from guard (lowers plate briefly) — Perfect Brace throw is upgrade-owned peak |

**Intent:** Always teaches mid-range pin + shaft cost. Weak enough that Javelin crowns still matter.

### 7.10 Soft Pin (baseline from throw; path expands)

| Piece | Draft |
|-------|--------|
| Source baseline | Throw hit |
| Duration | Short; refreshable with rules |
| Effect | Root-lite or very hard slow on grunts; bosses soft slow only |
| Path C | Longer pin, finisher gates, Returning Warhead boomerang, thin gun expose |
| Clarity | Tip VFX / stuck shaft silhouette when possible |

### 7.11 Base combat loop (no upgrades)

```
Gun fight → tap V long poke finishers / panic reach
OR hold V → spear up
   → M1 thrust string (poke → poke → finisher sweep)
   → hold RMB brace projectiles / chip from the front
   → release / M1-from-block bash
   → R throw javelin to pin mid-range straggler
   → close, retrieve, re-string
   → stow when gun is better
```

Skill without upgrades: spacing at long reach, when to throw vs keep shaft, not turtling forever on buckler, finisher timing, facing the threat (frontal plate).

### 7.12 What baseline does NOT include

- Free omni Guard (Fists)
- Free Aegis March hard-light wall
- Free lunge thrust
- Free multi-grunt skewer
- Free Handspring / Pile Driver
- Free ammo / grenade Payroll
- Free element tips
- Throw or shield while gun out
- Discarius shield-only mode
- Bruiser stun prison / dome / cubes

---

## 8. Upgrade Paths (gravity wells — hybrids intended)

### Path A — LANCER (Lunge / Combo / Impale)
**"Close the gap. Write the hole."**

- Spine: combo length/speed, thrust damage, reach, multi-part pierce on thrusts, **lunge thrust** (forward commit — Auger / Lunge Latch DNA, **not** slide Railblade)
- **Crown: Impaler's Creed** — finisher or lunge thrust skewers through N grunts / multi-part; brief stick-pin on elites
- Hybrid: lunge into shield brace; throw sets up impale closer; Discarius remaps some cards to bash-lunge

### Path B — HOPLITE (Shield / Phalanx / Bash / Wall)
**"The plate is the argument."**

- Spine: guard DR, frontal projectile nullify/deflect, bash damage, move-while-guard, brace → tempo, optional ally frontal formation crumb
- **Crown 1: Phalanx Protocol** — while guarding: stronger frontal wall + bash shockwave-lite on Perfect Brace; mild formation buff if ally nearby facing same general direction
- **Crown 2: Aegis March** — mobile hard-light tower shield while marching under guard (see §10)
- Hybrid: block then lunge; block then throw; Discarius pure plate builds live here

### Path C — JAVELIN (Throw / Pin / Mid-range hybrid)
**"The shaft leaves. The fight doesn't."**

- Spine: throw damage/range/arc, pin quality, retrieve rules, throw-then-melee amp, optional bounce, thin gun expose after pin/kill
- **Crown: Returning Warhead** — throw becomes strong pin + **boomerang retrieve** (primary fantasy)
- Hybrid: Hoplite throw-from-guard; Lancer throw into lunge finish; Discarius throws the disc instead

### Glue exotic (peer, not a fourth well prison)

**Shaft & Plate** — dual-tool crown. Both spear + buckler in hand = quality; retrieve flourish; throw-from-Perfect-Brace empowered.

### Path × verb matrix

```
                   LANCER                 HOPLITE                JAVELIN
Thrust / combo     core fantasy           bash remap (Discarius) throw→melee amp
Lunge              core fantasy           advance under plate    gap-close to pin
Guard / bash       brace after lunge      core fantasy           throw-from-guard
Throw / pin        setup for impale       plate-cover throw      core fantasy
Finisher           Creed skewer           bash shockwave-lite    pin execute
Aegis March        wall → lunge out       core fantasy           wall → safe throw
Discarius          soft-dead / bash-lunge pure plate fantasy     disc throw path
```

---

## 9. Crowns & Signature Cards

### Impaler's Creed — Exotic (Lancer crown)
- On **Combo Finisher** and/or **Lunge** thrust: pierce through N additional grunts / parts (draft 1–2 grunts; multi-part on same brain preferred when possible)
- Elites/bosses: single hard skewer + brief **stick-pin** (short root-lite); no multi-pierce delete
- Distinct impale VFX/audio; tiny self-hitch (~0.08–0.12s) on big Creed procs so it reads as a decision
- Recovery gated — not hallway infinite

### Phalanx Protocol — Exotic (Hoplite crown 1)
- While guarding: +frontal DR; +projectile nullify/deflect chance in frontal cone
- On **Perfect Brace**: next bash emits a **small kinetic shockwave-lite** (not Wrench Aftershock; short radius, light damage)
- **Formation crumb:** if an ally is nearby and facing roughly the same way, both gain a tiny outgoing damage or DR pip for a short window (mild co-op juice; not Unconditional Protector overshield)
- Moving while guarding less punished (stacks with Mobile Guard cards)

### Aegis March — Exotic (Hoplite crown 2 — mobile hard-light wall)
- While fully equipped **and guarding** (optional short post-guard grace): project a **mobile hard-light tower shield** that moves with the wielder
- Presentation: SAXON hard-light lattice on/around the buckler — taller frontal wall silhouette
- Effects while active:
  - Stronger frontal projectile mitigation (nullify/deflect bias)
  - Mild frontal shove / separation on enemies that body-check the wall
  - Optional tiny ally cover if standing in your frontal lee (very mild; not full Bruiser dome)
- **Heat / stamina:** cannot march forever — heat builds while Aegis is up; overheat drops wall and applies short guard fatigue (or forces plate cooldown). Cards may improve heat.
- **Not a placed ability:** no CD projector throw-down; no stun-on-place baseline; no prison cube; no modular multi-charges
- Works in **Discarius** mode (plate-only hoplite is on-theme)
- Soft duo with Bruiser in lobby: different verbs (carried vs placed); no package dependency

#### Aegis March vs Bruiser Hard-Light Projector (contrast lock)

| | **Bruiser Projector** | **Aegis March** |
|--|----------------------|-----------------|
| Source | Character ability CD | Melee kit exotic |
| Placement | Place / dome / prison / cubes | **Carried** — walks with you |
| Stun / reflect | Baseline stun + reflect | **No full stun prison**; limited frontal deflect/nullify + shove |
| Duration | Ability duration / CD | Guard-tied + heat while marching |
| Fantasy | Deployable architecture | **Hoplite advance** |
| Upgrades | Domed, Prison, Modular, Overshield… | Impaler grid only — do not copy those cards |

### Returning Warhead — Exotic (Javelin crown)
- Throw gains strong pin and damage
- **Boomerang retrieve:** shaft returns along a path, can hit again on the way back (or guaranteed retrieve with bonus damage on return — prefer readable boomerang arc)
- On return hit / retrieve complete: brief melee empower window
- Bosses: reduced pin; boomerang target caps
- Discarius: applies to **disc throw** with same rules (shield boomerang)

### Shaft & Plate — Exotic (Glue crown)
- While **spear + buckler both in hand** (not shaft-out): +thrust quality and/or +guard quality
- On retrieve: both tools snap ready + brief flourish (+attack speed / shorter recovery)
- On throw from **Perfect Brace**: empowered javelin (damage/pin)
- Does not replace Creed / Protocol / Aegis / Warhead; feeds all
- Discarius: "both in hand" becomes **plate ready** (no spear) — still grants guard/bash quality + disc retrieve flourish (honest remap)

### Brace Accounting — Epic (Hoplite support)
- While guarding, damage taken grants a small thin economy drip (tiny ammo/charge crumb) — keep modest vs Fists Brace Accounting cousin

### Gunhand Tip — Epic (thin economy / swap)
- On throw pin or Combo Finisher: brief damage window for **stowed/primary gun** + optional tiny mag crumb
- Fisticuffs inverse / Blade Gunhand cousin — **not** Payroll multi-resource

### Lunge Latch — Epic or Rare (Lancer)
- On charged finisher timing or dedicated lunge input window: short forward lunge thrust (Halo/Auger short-lunge DNA)
- Distance modest; geometry/boss safe clamps
- Prefer: finisher held slightly longer OR post-hit-2 timing window — document one primary rule in impl (**recommend: hold M1 slightly on hit-2→3 window converts finisher into Lunge Finisher** when card owned)

### Boundary Incursion — Oddity
- Grid grow — universal keep

### Discarius Protocol — Oddity (signature transform)
- See §11 full rules

---

## 10. Aegis March — Deep Dive

### 10.1 Fantasy
You do not drop a projector and walk away. You **are** the wall, and the wall advances.

### 10.2 Behaviour sketch

```
While Aegis March equipped AND fully equipped AND (guarding OR short grace after guard):
  1. Spawn/attach mobile hard-light tower VFX to offhand / frontal plane
  2. Expand frontal mitigation volume slightly beyond physical buckler
  3. Each frame: heat += rate * dt (faster if absorbing projectiles)
  4. On body-check: mild shove along plate normal
  5. If heat >= max: drop Aegis, start fatigue timer, keep basic buckler if still holding RMB
  6. Heat decays when not marching / not guarding
```

### 10.3 Risk budget

| Failure | Mitigation |
|---------|------------|
| Perma-wall immortal | Heat cap; fatigue; frontal-only |
| Bruiser obsolete | No stun prison, no dome, no multi-charge place, no team overshield spam |
| Ally grief block | Ally projectiles pass or reduced interact; no team-hostile full reflect by default |
| Aegis + Phalanx Protocol immortal | Stacking DR soft-cap; heat accelerates under fire |
| Discarius + Aegis turtle meta | Bash-forward requirement for DPS; heat still applies |

### 10.4 Mix notes
- Protocol + Aegis = peak Hoplite (encouraged)
- Aegis + Creed = wall → drop guard → lunge skewer
- Aegis + Warhead = safe throw from behind plate
- Aegis + Discarius = pure mobile tower shield brawler

---

## 11. Discarius Protocol — Oddity Deep Dive

### 11.1 Fantasy
HR lost the pike. You kept the plate. **Throw the disc. Bash the line.**

### 11.2 Transform rules

| Piece | Rule |
|-------|------|
| Rarity | **Oddity** |
| Effect | **Removes the spear.** Kit becomes buckler-primary |
| Visual | Pike stowed/absent; larger or dual-read buckler; optional disc silhouette |
| M1 | **Shield bash string** (3-hit bash cadence replaces thrust string) |
| RMB | Guard (same frontal plate; Hoplite cards fully apply) |
| R | **Throw the shield** (discus) instead of javelin; pin/retrieve family retained |
| Quick-V | Short shield bash poke |
| Combo events | `OnComboStep` / `OnComboFinisher` still fire (bash indices) so finisher cards can remap |
| Aegis March | **Valid** — signature fantasy |
| Phalanx Protocol | **Valid** |
| Returning Warhead | **Valid** on disc |
| Shaft & Plate | Remap to plate-ready quality + disc retrieve flourish |
| Impaler's Creed | Remap: bash finisher / bash-lunge gains limited skewer **or** heavy stagger execute (prefer **heavy bash finisher pierce-lite 1 grunt** so card is not fully dead) |
| Lancer thrust-only stats | Soft-dead or convert reach→bash range, thrust dmg→bash dmg where honest |
| Javelin range cards | Apply to disc throw |
| Intent | Build-defining identity flip — readable in gear UI when Oddity socketed |

### 11.3 What Discarius is NOT
- Not a second melee kit catalog entry
- Not true dual-wield gun+shield
- Not Bruiser ability unlock
- Not infinite disc multithrow without retrieve

### 11.4 Grid / rarity note
Discarius is the **signature Oddity**. **Boundary Incursion** remains the universal grid-grow Oddity.
Frozen 30 includes **both** by trimming one low-priority Standard if needed (see §12).

---

## 12. Full Upgrade List (~30 ship + backlog)

Rarity: Standard / Rare / Epic / Exotic / Oddity  
Tags: L Lancer · H Hoplite · J Javelin · G Glue / gunfeel · D Discarius  
Cell rule: Exotics larger; all Exotics same cell count.  
Names are player-facing.  
Numbers are **v0 starting targets** — validate in playtest.

Suggested IDs at impl (verify collisions): gear **93000**; upgrades **93001–93030**.

------------------------------------------------------------------------------
EXOTICS (5) — equal large shapes
------------------------------------------------------------------------------

X1. Impaler's Creed — Exotic (L) — Lancer crown
    Finisher/lunge skewer pierce; elite stick-pin; recovery gated.

X2. Phalanx Protocol — Exotic (H) — Hoplite crown
    Stronger frontal guard; Perfect Brace bash shockwave-lite; formation crumb.

X3. Aegis March — Exotic (H) — Mobile hard-light tower
    Guard-tied carried wall; heat-limited; shove; not Bruiser projector.

X4. Returning Warhead — Exotic (J) — Javelin crown
    Strong throw pin + boomerang retrieve; return empower window.

X5. Shaft & Plate — Exotic (G) — Dual-tool glue
    Both tools quality; retrieve flourish; Perfect Brace throw empower.
    Discarius: plate-ready remap.

------------------------------------------------------------------------------
EPICS (7)
------------------------------------------------------------------------------

E1. Lunge Latch — Epic (L)
    Hit-2→3 hold window (or equivalent) converts finisher into forward lunge thrust.
    Distance modest; clamps on geometry/bosses.

E2. String Doctrine — Epic (L)
    Faster string recovery; finisher damage up; mild multi-hit forgiveness on finisher.

E3. Perfect Brace — Epic (H)
    Clean projectile absorb window empowers next bash/throw; teaches brace skill.
    (Name = card; baseline already has weaker Perfect Brace hook — this deepens it.)

E4. Bulwark Tempo — Epic (H)
    After releasing guard, brief +move and +bash/thrust damage (riposte window).
    Feeds Creed and bash shockwave.

E5. Tip Authority — Epic (J)
    +Throw damage and pin duration; throw hits refresh a short melee amp vs that target.

E6. Gunhand Tip — Epic (G)
    Throw pin or Combo Finisher → brief stowed/primary gun damage window (+ optional tiny mag crumb).

E7. Brace Accounting — Epic (H/G)
    While guarding, damage taken grants small thin economy drip (modest; not Payroll).

------------------------------------------------------------------------------
RARES (10)
------------------------------------------------------------------------------

R1. Longer Pike — Rare (L/G)
    +Reach. Still no falloff. Longest-kit identity card.

R2. Weighted Tip — Rare (L)
    +Thrust and finisher damage.

R3. Skewer Training — Rare (L)
    Thrusts gain mild pierce chance / +1 part pierce (weaker than Creed).

R4. Mobile Guard — Rare (H)
    Reduced move penalty while guarding.

R5. Plate Training — Rare (H)
    +Baseline frontal Guard DR.

R6. Bash Weight — Rare (H)
    +Shield Bash damage and knockback.

R7. Retrieve Line — Rare (J)
    Faster shaft/disc return on miss timer; throw recovery down.

R8. Javelin Cord — Rare (J)
    +Throw range and projectile speed.

R9. Pin Needle — Rare (J)
    +Pin duration / boss soft-slow quality on throw.

R10. Quick Draw Pike — Rare (G)
    Faster quick-V startup / shorter gun-return gap.

------------------------------------------------------------------------------
STANDARDS (5)
------------------------------------------------------------------------------

S1. Honed Tip — Standard (G)
    +Melee and throw damage. Stackable mild.

S2. Snap Recovery — Standard (G)
    Shorter recovery after hit or whiff (string "fire rate").

S3. Guard Drill — Standard (H)
    Minor +Guard DR; tiny bash damage.

S4. Cord Wrap — Standard (J)
    Minor +throw damage; minor +retrieve speed.

S5. Issue Balance — Standard (G)
    Slightly faster equip (hold-V complete) and stow cleanliness.

------------------------------------------------------------------------------
ODDITIES (2)
------------------------------------------------------------------------------

O1. Discarius Protocol — Oddity (D) — Signature transform
    Remove spear. Bash string + disc throw. Hoplite/Javelin hot; Lancer remaps.

O2. Boundary Incursion — Oddity (G)
    +Upgrade grid size.

------------------------------------------------------------------------------
FROZEN v1 SHIP POOL (exactly 30)
------------------------------------------------------------------------------

  EXOTIC (5)
    1  Impaler's Creed
    2  Phalanx Protocol
    3  Aegis March
    4  Returning Warhead
    5  Shaft & Plate

  EPIC (7)
    6  Lunge Latch
    7  String Doctrine
    8  Perfect Brace
    9  Bulwark Tempo
    10 Tip Authority
    11 Gunhand Tip
    12 Brace Accounting

  RARE (10)
    13 Longer Pike
    14 Weighted Tip
    15 Skewer Training
    16 Mobile Guard
    17 Plate Training
    18 Bash Weight
    19 Retrieve Line
    20 Javelin Cord
    21 Pin Needle
    22 Quick Draw Pike

  STANDARD (5)
    23 Honed Tip
    24 Snap Recovery
    25 Guard Drill
    26 Cord Wrap
    27 Issue Balance

  ODDITY (2)
    28 Discarius Protocol
    29 Boundary Incursion

  FLEX (1) — pick one for slot 30:
    RECOMMENDED: **Finishing Bell** — Epic demoted count OR promote a 6th Standard
    **Final pick for slot 30: Finisher's Heel — Rare (L)**
      Bonus damage on Combo Finisher vs low-HP targets.

  RECONCILED FROZEN 30:

    EXOTIC (5)
      1  Impaler's Creed
      2  Phalanx Protocol
      3  Aegis March
      4  Returning Warhead
      5  Shaft & Plate

    EPIC (7)
      6  Lunge Latch
      7  String Doctrine
      8  Perfect Brace
      9  Bulwark Tempo
      10 Tip Authority
      11 Gunhand Tip
      12 Brace Accounting

    RARE (11)
      13 Longer Pike
      14 Weighted Tip
      15 Skewer Training
      16 Mobile Guard
      17 Plate Training
      18 Bash Weight
      19 Retrieve Line
      20 Javelin Cord
      21 Pin Needle
      22 Quick Draw Pike
      23 Finisher's Heel

    STANDARD (5)
      24 Honed Tip
      25 Snap Recovery
      26 Guard Drill
      27 Cord Wrap
      28 Issue Balance

    ODDITY (2)
      29 Discarius Protocol
      30 Boundary Incursion

Count: 5X + 7E + 11R + 5S + 2O = **30**.

BACKLOG (designed, expand later)
  Element Tip (Fire/Shock/Acid opt-in on thrusts)
  Ally Phalanx Aura (stronger formation than Protocol crumb)
  Second Wind Tip (melee kills drip ability charge — thin)
  Core Nick (core/anatomy thrust bias)
  Bounce Javelin (throw ricochet once)
  Twin Toss (second throw without full Warhead — careful)
  Guard Stamina variant (if heat-less turtle appears)
  Soft Hands (damage on equip → brief free frontal DR)
  Reckless Advance cousin (+move while thrusting, take more damage)
  Heavy Handshake cousin (melee kills drip heavy ammo — thin; Carver owns feast)
  True dual-wield gun+shield experiment (rejected for v1 input clarity)
  Discarius dual-disc without retrieve (rejected)
  Aegis reflect full projectiles like Carver Deflector (careful power)
  Deployable mini-plate (too close to Bruiser — reject unless sharp contrast)

------------------------------------------------------------------------------
CUT / DEMOTE
------------------------------------------------------------------------------

| Idea | Fate |
|------|------|
| RMB = throw | **Cut** — R = throw; RMB = shield |
| Throw while gun out | **Cut** — reload/ADS clarity |
| Baseline omni Guard | **Cut** — frontal only |
| Baseline Aegis wall | **Cut** — exotic |
| Baseline free lunge | **Cut** — Lunge Latch / Creed |
| Payroll-class ammo engine | **Cut** — Fists owns; Gunhand Tip thin only |
| Pile Driver / Handspring on spear | **Cut** — Wrangler / Fists |
| Slide Railblade lunge | **Cut** — Stalker's Blade |
| Shockwave primary loop | **Cut** — Wrench |
| Blood meter / heavy feast | **Cut** — Carver |
| Continuous saw ticks | **Cut** — Carver |
| Bruiser Domed / Prison / Modular copy | **Cut** — contrast lock |
| Boarding Trident name / rifle identity | **Cut** — different product |
| Crouch Ambush stealth | **Cut** — Blade |
| Primary-slot spear | **Cut** — melee kit only v1 |

---

## 13. Example Builds

### Pure Lancer
Longer Pike → Weighted Tip → Skewer Training → String Doctrine → Lunge Latch → **Impaler's Creed** → Finisher's Heel → Snap Recovery  
*Play:* Hold V in the lane; string into lunge finishers; skewer lines; throw only to pin runners.

### Pure Hoplite wall
Plate Training → Mobile Guard → Bash Weight → Perfect Brace → **Phalanx Protocol** → **Aegis March** → Bulwark Tempo → Brace Accounting  
*Play:* March under hard-light plate; Perfect Brace → bash shockwave; advance the line.

### Pure Javelin
Javelin Cord → Pin Needle → Retrieve Line → Tip Authority → **Returning Warhead** → Gunhand Tip → Cord Wrap → Quick Draw Pike  
*Play:* Throw pin → boomerang → close or swap to gun window → retrieve flourish.

### Poster hybrid (recommended fantasy complete)
Impaler's Creed + Phalanx Protocol + Aegis March + Returning Warhead + Shaft & Plate + Lunge Latch + Gunhand Tip  
*Play:* Brace under Aegis, throw Warhead, retrieve, lunge Creed finisher, gun cleans elites.

### Discarius plate-only
**Discarius Protocol** → Aegis March → Phalanx Protocol → Bash Weight → Perfect Brace → Returning Warhead → Bulwark Tempo → Mobile Guard  
*Play:* No pike. Bash string, disc boomerang, mobile tower shield. Peak Oddity fantasy.

### Gun-out skirmisher (minimal full equip)
Quick Draw Pike → Longer Pike → Honed Tip → Gunhand Tip → Tip Authority  
*Play:* Stay on primary; tap-V long pokes; hold V only for throw pin setups or emergency plate.

---

## 14. Strengths, Weaknesses & Failure Modes

### Strengths
- Unique arsenal fantasy (hoplite hybrid) with clear sibling contrast
- Longest melee reach + mid javelin without becoming a primary
- Buckler gives full-equip identity distinct from Fists omni Guard
- 3-hit string readable empty-grid skill
- Aegis March delivers hard-light fantasy without deleting Bruiser
- Discarius Oddity is a memorable build-defining flip
- Thin gun-swap economy respects Fists Payroll
- CQC modifier and line-hold co-op both supported

### Weaknesses
- Frontal plate punishes being surrounded (by design)
- Shaft-out state punishes throw spam
- No baseline pack shockwave or saw volume
- Pure static aerial bosses may prefer guns if pins miss
- Discarius abandons longest-reach identity (trade accepted)

### Failure modes to avoid in tuning

| Failure | Mitigation |
|---------|------------|
| Aegis perma-immortal | Heat, fatigue, frontal-only, DR soft-cap with Protocol |
| Phalanx + Aegis + Fists dual-kit nonsense | Different slots; still tune frontal DR bands |
| Creed hallway delete | Pierce cap; bosses no multi-pierce; recovery |
| Warhead pin forever | Short pin; boss resist; retrieve ICD |
| Gunhand Tip = Payroll | Tiny window/crumb only |
| Discarius makes Lancer cards trash | Honest remaps for Creed/Lunge/reach→bash |
| Throw steals reload muscle memory | Full-equip only; gun-out R stays reload |
| Feels like Blade with a stick | No crouch Ambush, longer reach, shield identity, 3-hit thrust not dual slash |
| Feels like Fists with a stick | Frontal plate not omni; throw; string; no Payroll |
| Feels like Bruiser melee | No placeable projector; heat march; kit exotic not ability |
| Feels like Trident | Melee kit, no mag, thrust not prong rifle |
| Turtle meta empty-grid | Modest baseline DR; bash/throw required for pace |

---

## 15. Naming & Presentation

| Slot | Value |
|------|--------|
| Display name | **Phalanx Impaler** |
| Internal / API | `phalanx_impaler` |
| Design nicknames | Issue Pike, Hoplite Spear, Saxonite Pike (notes only) |
| Short description | *Issue pike and buckler. No ammo, no reload — thrust string, raise the plate, throw the shaft. Ambush is not the point; the line is. Bolt on lancer skewers, hoplite hard-light march, javelin warheads — or Oddity out the spear and fight plate-only.* |
| Thunderstore name (later) | `PhalanxImpaler` |
| GUID (later) | `sparroh.phalanximpaler` |
| Folder today | `.new.SpearMelee` |
| Suggested IDs | Gear **93000**; upgrades **93001–93030** (verify at impl) |
| Design doc file | `PhalanxImpaler-DesignDoc.txt` |

### SAXON marketing blurb (draft)

> SAXON Phalanx Impaler — For employees who measure professionalism in meters of reach.  
> One pike. One plate. Zero cells. Zero "please reload while the fungus writes your eulogy."  
> Baseline: thrust, thrust, finish. Brace the front. Throw the shaft. Retrieve. Repeat.  
> Aftermarket: impaler's creed, phalanx protocol, aegis march, returning warheads, shaft-and-plate hospitality.  
> Oddity addendum: lost the spear? Keep the plate. Discarius Protocol is not a refund.  
> If your melee option comes with a magazine UI, a crouch tutorial, or a chainsaw disclaimer, you are holding the wrong product.  
> "If they wanted to stand over there, they should have brought a longer argument."

### Flavor / in-world

Industrial pike issued to corridor-control contractors and boarding teams who failed the "bring a gun" seminar.
Saxonite tip holds optional aftermarket channels (grid not included — wait, grid is included).
Offhand buckler doubles as HR-approved "personal space enforcer." Hard-light tower kit sold as Aegis March
upgrade and is definitely not a Bruiser union grievance.

---

## 16. Synergy Notes (Player-Facing, Soft Only)

| Partner | Why it feels good |
|---------|-------------------|
| **MeleeRework / Fists** | Slot platform; Fists for brawler days, Impaler for line-hold days — swap kits |
| **Stalker's Blade** | Peer melee; assassin vs hoplite — no shared Ambush |
| **Bruiser** | Placed projector + your carried Aegis = layered hard-light without package dep |
| **Shocklance + Fisticuffs** | Soft co-op joke; your melee feeds their lance |
| **DMLR / precision guns** | Gunhand Tip + pin windows → primary dumps |
| **Blood Carver / Wrench** | Dual-melee meme loadouts; different verbs — no shared meter |
| **OmniMovement** | Strafe-lunge and march feel better |
| **Hard-Light Constructor** | Soft VFX literacy only; different slot/fantasy |

**Explicit non-goals v1:** hard deps; heavy ammo feed; blood meter; mag/reload; baseline element; baseline Aegis; replacing Fists as default melee; Bruiser ability rewrite.

---

## 17. Success Criteria / Player Fantasy Checklist

- [ ] Phalanx Impaler appears as a selectable **melee kit** (MeleeRework slot)
- [ ] Tap V long poke useful with **zero** upgrades
- [ ] Hold V equips pike + buckler with clear feedback
- [ ] 3-hit string readable; finisher feels like a payoff empty-grid
- [ ] RMB frontal guard works; rear attacks punish turtle mistakes
- [ ] Shield Bash from brace/release is usable empty-grid
- [ ] R throw pins and puts shaft out; retrieve is reliable
- [ ] Creed build: finisher/lunge skewers a line without map-wipe forever
- [ ] Protocol build: Perfect Brace → bash shockwave reads
- [ ] Aegis March: mobile wall marches, heats, drops — not Bruiser place
- [ ] Warhead build: boomerang pin-retrieve loop feels great
- [ ] Shaft & Plate makes dual tools feel mandatory-cool
- [ ] Discarius: spear gone; bash + disc + Aegis still complete fantasy
- [ ] Gunhand Tip does not replace reloading as a lifestyle
- [ ] No ammo UI / no reload beat on this kit
- [ ] Does not obsolete Fists Guard/Payroll, Blade Ambush, Wrench/Carver, or Bruiser Projector
- [ ] Failure states stay fun (shaft out, surrounded plate, overheated Aegis)
- [ ] Sandbox MP: guard/throw/lunge/Aegis readable enough on clients

---

## 18. Implementation Appendix (For Later — Not This Pass)

Design-only milestone: **this document**. When coding starts:

| Piece | Approach |
|-------|----------|
| Platform | **MeleeRework** `RegisterKit` / `IMeleeKit` quick + full profiles |
| Registration | Melee gear catalog entry; persistence by gear id; fallback Fists if missing |
| Host | `PhalanxImpalerBehaviour` + `Data` struct for combo/guard/throw/Aegis/Discarius flags |
| Base type | Evaluate `MeleeGear` / Throwable DNA vs custom hitcast after Assembly lookup — **do not invent APIs** |
| Combo | State machine hit index 1–3; timers; finisher volume swap |
| Guard | While equipped + Aim held: frontal cone DR + projectile bias; subscribe damage path |
| Bash | Release/M1-from-guard → short melee volume |
| Throw | Mid projectile; shaft-out state replicated; retrieve timers |
| Aegis | Attach frontal volume + VFX while guard+exotic; heat in Update |
| Discarius | On Apply: swap profiles/flags; hide pike mesh; remap events |
| Upgrades | `PlayerData.CreateUpgrade` + `UpgradeProperty` Apply/Remove on FindGear(`phalanx_impaler`) |
| No ammo | Infinite / bypass; R = throw; hide reload |
| Mod flags | `[MycoMod(..., ModFlags.IsSandbox)]` |
| Deps | Soft recommend MeleeRework; document hook version |
| Model | Pike + buckler meshes when AssetBundle exists; placeholder OK |

### Suggested `PhalanxImpalerBehaviour.Data` fields (sketch)

```
// Core mults
float damageMult;
float reachMult;
float sizeMult;
float recoveryMult;
float quickDrawMult;
float equipSpeedMult;

// Combo
int comboIndex;              // runtime
float finisherDamageMult;
float finisherSizeMult;
float stringRecoveryMult;
bool stringDoctrine;
bool finishersHeel;
float finisherLowHpThreshold;
float finisherLowHpMult;

// Lancer
bool lungeLatch;
float lungeDistance;
bool impalersCreed;
int creedPierceGrunts;
float creedElitePinDuration;
float creedRecoveryMult;
float skewerPierceChance;
int skewerBonusPartPierce;

// Guard / Hoplite
float guardDrMult;           // frontal
float guardProjectileNullifyChance;
float guardMovePenaltyMult;
float bashDamageMult;
float bashKnockbackMult;
bool phalanxProtocol;
float protocolBashWaveRadius;
float protocolBashWaveDamage;
float formationRadius;
float formationBuffDuration;
bool perfectBraceCard;
float perfectBraceWindow;
float bulwarkTempoDuration;
float bulwarkTempoDamageMult;
bool braceAccounting;
float braceAccountingCrumb;

// Aegis March
bool aegisMarch;
float aegisHeatMax;
float aegisHeatPerSecond;
float aegisHeatOnAbsorb;
float aegisHeatDecay;
float aegisFatigueDuration;
float aegisShoveForce;
float aegisFrontalBonusMitigation;

// Throw / Javelin
float throwDamageMult;
float throwRangeMult;
float throwSpeedMult;
float pinDuration;
float pinBossMult;
float retrieveTimeMult;
bool shaftOut;               // runtime
bool returningWarhead;
float warheadReturnDamageMult;
bool tipAuthority;
float tipAuthorityMeleeAmp;
float tipAuthorityDuration;
bool retrieveLine;
bool javelinCord;
bool pinNeedle;

// Glue / economy
bool shaftAndPlate;
float bothToolsQualityMult;
float retrieveFlourishDuration;
float perfectBraceThrowMult;
bool gunhandTip;
float gunhandDuration;
float gunhandDamageMult;
float gunhandAmmoCrumb;      // keep tiny or 0

// Discarius
bool discariusProtocol;
// when true: use bash string profile, disc throw, hide pike
```

### Ship cut vs stretch

**v1 must-ship (fantasy complete):**
- Melee kit registration + tap/hold V profiles
- 3-hit thrust string + long quick-V poke + no ammo
- RMB frontal buckler + Shield Bash
- R javelin + pin + shaft-out retrieve
- All **5** exotics (Creed, Protocol, Aegis March, Warhead, Shaft & Plate)
- Discarius Protocol Oddity transform
- Boundary Incursion
- Thin Gunhand Tip economy
- Aegis heat + Bruiser contrast respected
- Frozen 30 registration

**Stretch / post-v1:**
- Element tips, bounce javelin, stronger formation aura
- Custom meshes / Wwise
- Config knobs for reach / guard DR / heat
- Convert-era notes if other kits move slots

### Phased delivery (when coding)

| Phase | Deliverable |
|-------|-------------|
| P0 | Register kit + quick/full thrust + no ammo |
| P1 | 3-hit string + finisher volume |
| P2 | Frontal guard + bash |
| P3 | Javelin throw + pin + retrieve |
| P4 | Lunge Latch + Impaler's Creed |
| P5 | Phalanx Protocol + Perfect Brace deep |
| P6 | Aegis March mobile wall + heat |
| P7 | Returning Warhead boomerang |
| P8 | Shaft & Plate + Gunhand Tip |
| P9 | Discarius Protocol transform |
| P10 | Frozen 30 + balance + README |

---

## 19. Open Tuning Questions (playtest, not design blockers)

1. Baseline reach vs Fists / Blade (how much longer is "longest"?)  
2. Guard frontal DR 20% vs 30%; projectile nullify empty-grid rate  
3. Finisher recovery vs mash feel  
4. Lunge Latch input: hold on hit-2→3 vs separate bind  
5. Throw retrieve 2.5s vs 3.5s  
6. Pin duration vs boss soft-slow  
7. Aegis heat rate under fire vs idle march  
8. Creed pierce 1 vs 2 grunts  
9. Warhead boomerang damage on return vs outbound only  
10. Discarius Creed remap: pierce-lite vs stagger execute  
11. Formation crumb strength (keep tiny)  
12. Exact hex shapes for 30 upgrades — author during implementation  
13. Whether quick-V can ever bash in Discarius only (default **yes** for Discarius, thrust otherwise)

---

## 20. Relationship to Sibling Projects

| Project | Relationship |
|---------|----------------|
| **MeleeRework** | **Hard platform** when shipped — slot, tap/hold V, kit API |
| **Fists** | Peer melee kit; different verbs; do not steal Guard-omni/Payroll/Haymaker/Pile Driver |
| **Stalker's Blade** | Peer melee; no Ambush/stealth/slide Railblade steal; throw family is cousin only |
| **Blood Carver** | Primary saw until convert; no shared blood |
| **Saxonite Wrench** | Primary slam until convert; no shockwave/pull steal; Lunge Latch DNA cousin only |
| **Boarding Trident** | Name/fantasy adjacency only — rifle primary, not this kit |
| **Bruiser / Hard-Light** | Aegis March contrast lock; soft VFX literacy; no ability replace |
| **Shocklance** | Auger/Skewer/Fisticuffs DNA borrow with renames |
| **DMLR** | Thin expose/gun window precedent only |
| **Weapon template in this folder** | Scaffold only; product is melee kit + design doc first |

---

## 21. Universal Truths (Mycopunk alignment)

- Exotic shapes should always be larger than others; each exotic the same cell count.
- v1 targets **~30** upgrades (frozen list above); backlog is real design, not trash.
- Three paths create different builds but **may intermingle** on the grid.
- Prefer verbs: thrust string, finisher, lunge, frontal guard, Perfect Brace, bash, javelin pin, retrieve, Aegis heat, Discarius transform.
- No second blood/torque meter.
- No ammo/reload on this kit.
- Hybrids intended; no anti-synergy matrix.

---

## 22. Design Checklist

- [x] Name: **Phalanx Impaler**  
- [x] Melee kit via MeleeRework (not primary)  
- [x] R = **javelin throw**; RMB = **buckler guard**  
- [x] Baseline shield modest frontal (lock A)  
- [x] 3-hit thrust string (lock A)  
- [x] Longest melee reach identity  
- [x] No ammo / no reload  
- [x] Paths: Lancer / Hoplite / Javelin  
- [x] Exotics: Creed, Protocol, **Aegis March**, Warhead, Shaft & Plate  
- [x] Oddity: **Discarius Protocol** (+ Boundary Incursion)  
- [x] Aegis vs Bruiser contrast documented  
- [x] Thin economy (Gunhand Tip; no Payroll)  
- [x] Frozen 30 table  
- [x] Differentiated from Fists / Blade / Carver / Wrench / Trident / Bruiser  
- [x] Wiki DNA borrowed with renames  
- [x] Failure modes documented  
- [x] Implementation deferred  

---

## 23. Design Changelog

### v1 (this doc) — 2026-08-07

- Product: **Phalanx Impaler** melee kit for MeleeRework hooks
- User locks: name Phalanx Impaler; R = throw; modest frontal shield; 3-hit combo
- User adds: Discarius Protocol Oddity (shield-only); Aegis March Hoplite exotic (mobile hard-light wall)
- Paths: Lancer / Hoplite / Javelin + Shaft & Plate glue
- Baseline: long thrust string, frontal buckler + bash, javelin pin + retrieve, no ammo
- Frozen 30 + backlog + sibling contrast + Bruiser contrast + Discarius remap rules
- Rejected: gun-out throw, omni Guard, Bruiser projector copy, Railblade, Payroll, primary-slot spear

---

## 24. Next Steps After This Doc

1. Confirm MeleeRework hook surface when that project implements RegisterKit  
2. Implement P0–P3 (kit + string + guard + throw) against decompile  
3. Layer lunge / Creed / Protocol / Aegis / Warhead / Shaft & Plate  
4. Implement Discarius transform + remaps  
5. Register frozen 30  
6. Playtest reach, guard DR, Aegis heat, Creed pierce, Warhead pin, Discarius feel  
7. Art/audio pass when ready  

---

*End of Phalanx Impaler Design Doc v1*
