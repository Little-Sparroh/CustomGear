# Stalker's Blade — Design Document (v1)

> Status: **Design only** — implementation follows this doc and MeleeRework hooks.
> Package / Thunderstore name (planned): **StalkersBlade** / **KnifeMelee**
> GUID (planned): `sparroh.stalkersblade`
> MycoMod flags: **IsSandbox** (gameplay rules; melee kit)
>
> Product shape: **Melee kit** (`GearType.Melee`) that registers through **MeleeRework**
> extension hooks. Not a primary weapon. Not a Fists reskin.
>
> Sibling docs (do not absorb; contrast locks below):
>   - MeleeRework — slot platform, tap/hold V, Fists kit, extension API
>   - Blood Carver — continuous saw + blood + heavy feast (primary until convert)
>   - Saxonite Wrench — discrete slam + shockwave + pull + optional cryo (primary)
>   - Needle Carbine — real `EffectType.Poison` + needles/supercombine (soft poison duo)
>   - DMLR Rework — Mark / Expose vocabulary precedent (simpler Mark on blade)
>   - Aussie Special — Slideshot damage-budget lesson (movement amp ≠ DPS crutch)
>
> Wiki / vanilla DNA borrowed (rename on blade grid; no hard character deps):
>   - Slideshot, Smart Slide, Tumbleweed, Landing Roll, Dust Kick, Knee Slide
>   - Handspring stays Wrangler; Fists owns Pile Driver — blade does not steal
>   - Fisticuffs (Shocklance) → inverse **Gunhand Cut** (Ambush buffs stowed gun)
>   - Impact Cascade / Dead or Alive → thin economy + Mark language only
>
> Vanilla anchors (implement against decompile, do not invent APIs):
>   - MeleeRework: tap V quick-melee, hold V equip, `IMeleeKit` / RegisterKit
>   - Crouch / slide state on player movement
>   - `EffectType.Poison` per Needle Carbine (confirm id; soft share)
>   - No native AI stealth — combat stealth is invented vocabulary (below)

---

## 1. High Concept / Fantasy

**Stalker's Blade** is SAXON's quiet argument: a matched pair of issue knives for employees who prefer not to appear on the incident report.

You do not hold the room. You do not saw the pile. You do not crack the floor.
You **stay low, cut once, and leave.**

Baseline is honest dual-blade melee — fast discrete slashes, a thrown knife on RMB,
no ammo, no reload. Power comes from **how you arrive**: crouch Low Profile, slide
approach, flank Ambush, and full-HP openers. The grid forks the kit into ghost
stalking, railblade lunges, or venom warrant executes.

**One-liner:** *Stay low. Cut once. Don't be there when they look.*

**Element baseline:** Normal (kinetic). Poison is **Execute path only** — opt-in,
Needle Carbine soft duo, no hard dependency.

---

## 2. Role & Fantasy in the Arsenal

| | |
|--|--|
| **Slot** | **Melee** (`GearType.Melee`) via MeleeRework registration hooks |
| **Range** | Extreme close (shortest melee kit) + short–mid **throw** |
| **Role** | Assassin finisher / elite opener / movement-linked burst; optional full-equip stalker mode |
| **Gap filled** | Mycopunk has **no stealth/assassin fantasy**. Slide toys buff guns (Wrangler, Slideshot). Melee is punch, saw, or slam. Nothing owns **low-profile approach → single-target execute**. |
| **Synergies** | Crouch/slide movement; stowed primary after Ambush (Gunhand Cut); Needle Carbine Poison paint → blade execute; CQC mission modifier; OmniMovement / AimAndCrouchToggles soft UX |
| **Not trying to be** | Fists brawler, Carver blender, Wrench AOE clear, primary gun, MGS stealth sim, Payroll ammo engine |

**Product shape:** New melee kit (**Stalker's Blade**). Default fallback remains Fists.
Does **not** replace vanilla MeleeGear punch in-place; registers as its own catalog entry.

---

## 3. Design Pillars

1. **Stealth is crouch + approach, not a dedicated stance key.** Low Profile lives on crouch so it works on quick-V and full equip without stealing RMB.
2. **RMB is throw.** Mid-range Mark tool and dual-blade cost/retrieve fantasy — not Guard, not pull, not blood spend.
3. **Dual blades are mechanical, not cosmetic.** L-R slash string, throw costs one blade, Twin Sheath crowns the pair.
4. **Ambush is the readable stealth verb.** Position + movement state + first-strike — never "wait for aggro drop."
5. **Opener matters.** Mild full-HP bonus on baseline; Path C owns the sentence-opening fantasy.
6. **Discrete cuts, not a saw or slam.** Faster than Wrench, tighter than Fists, zero continuous tick DPS.
7. **No ammo. No reload.** Cadence = recovery + throw retrieve + approach setup.
8. **Three peer paths + dual-blade glue exotic.** Low Profile / Slide Cut / Execute. Hybrids intended.
9. **Poison is opt-in.** Real `EffectType.Poison` when enabled; kinetic bleed-lite only if needed as non-Poison finisher spice — prefer Poison cards over a second fake DoT.
10. **Vanish is soft.** Speed + mild DR + next-Ambush amp. No real invis/opacity in v1.
11. **Thin economy.** Fists owns Payroll. Blade may buff the *gun you swap to*, not refill the whole kit.
12. **~30 upgrades;** exotic shapes larger than others; each exotic the **same** cell count.
13. **Don't steal sibling niches.** No Guard, no blood meter, no shockwave/Event Horizon, no heavy feast, no Pile Driver.
14. **Self-contained v1.** Hard dep: MeleeRework hooks when that platform exists. Soft only: Needle Carbine, OmniMovement, crouch toggles, Wrangler slide DNA.
15. **On-hit / spatial / state verbs > flat % stickers.**

---

## 4. Locked Decisions

| Decision | Lock |
|----------|------|
| Display name | **Stalker's Blade** |
| APIName | `stalkers_blade` |
| Slot | Melee (MeleeRework kit) |
| Baseline damage type | **Normal / kinetic** |
| Innate element | **None** |
| Poison | **Execute path only** (real EffectType) |
| Ammo / magazine / reserve | **None** |
| Reload | **None** — R unused on baseline |
| Stealth stance | **Crouch = Low Profile** |
| RMB | **Thrown knife** (blades-up only) |
| M1 | Dual slash string (discrete) |
| Dual blades | **Yes** — mechanical |
| Opener (full HP) | **Yes** — mild baseline + path spine |
| Vanish | **Soft** only in v1 |
| 4th exotic | **Twin Sheath** |
| Economy | **Thin** — no Payroll-class engine |
| Guard / DR baseline | **None free** on crouch |
| Heavy feed | **Out of v1** |
| True invis | **Out of v1** |
| Throw while gun out | **No** — blades equipped only |
| Product | New melee kit, not primary, not Fists patch |

---

## 5. Melee Roster Differentiation

| | **Fists** | **Blood Carver** | **Saxonite Wrench** | **Stalker's Blade** |
|--|-----------|------------------|---------------------|---------------------|
| Slot (v1) | Melee | Primary (convert later) | Primary (convert later) | **Melee** |
| Cadence | Punch | Hold-M1 saw ticks | Tap / charged slam | **Dual slash + throw** |
| Stance / RMB | Guard (RMB) | Blood spend (RMB) | Gravity pull (RMB) | **Throw (RMB)**; **crouch = stealth** |
| Resource | None | Blood stacks | Torque (momentary) | Mark / opener / optional Poison |
| Crowd tool | Size + multi-hit | Pull + saw volume | Shockwave + pull | **None baseline** (Railblade light pierce only) |
| Fantasy | Brawler economy / tempo | Harvest → spend → heavy | Gravity clear / freeze-shatter | **Assassin approach → execute** |
| Sustain | Guard DR | Blood DR / chunks | Minimal / cryo ward | Soft Vanish only if built |

---

## 6. What "Stealth" Means in Mycopunk

There is **no AI stealth / aggro-drop system** to hook. Stalker's Blade invents a
**readable combat stealth language** (same structural trick as DMLR Mark/Expose —
vocabulary upgrades speak; not fake stealth meters that lie).

| Term | Meaning |
|------|---------|
| **Low Profile** | While **crouching**: Ambush qualification, quieter presentation, upgrade hooks for speed/DR/Vanish. Works gun-out and blades-up. |
| **Approach** | Entered engagement via crouch-walk, slide, or throw setup |
| **Ambush** | Knife/throw hit that qualifies via crouch, slide window, flank cone, and/or clean first-strike rules |
| **Mark** | Soft "marked for death" state from throw (baseline) and path cards |
| **Opener** | Bonus vs targets at full HP (or ≥95% HP) — assassin first cut |
| **Vanish** | Short post-Ambush-kill window: move speed + mild DR + next-Ambush amp (upgrade-owned peak on Ghost Protocol) |
| **Blade Out** | One knife thrown; melee runs single-blade profile until retrieve |
| **Poison** | Real status when Execute cards enable it — not a second blood meter |

**Design pillar:** Stealth is **positioning + movement state + first-strike**, never
"stand still until the room forgets you."

---

## 7. Shared Framework (events upgrades speak)

Emit through MeleeRework-compatible host (`StalkersBladeBehaviour` / kit router):

| Event / state | Fired when | Used for |
|---------------|------------|----------|
| **OnMeleeHit** | Quick or full slash damages enemy | Stacks, Mark refresh, Poison apply |
| **OnMeleeKill** | Blade kill | Vanish gates, retrieve, thin economy |
| **OnAmbush** | Hit qualifies as Ambush | Mult, Ghost, Warrant, juice |
| **OnAmbushKill** | Kill on an Ambush hit | Ghost Protocol, Twin Sheath snap-back |
| **OnOpener** | Hit vs full-HP (threshold) target | First Blood / Black Warrant |
| **OnThrow** | RMB knife leaves hand | Mark, Poison tip, Twin Sheath follow-up |
| **OnThrowHit** / **OnThrowAmbush** | Projectile connects | Retrieve rules, Mark apply |
| **OnRetrieve** | Blade returns | Twin Sheath, tempo cards |
| **WhileCrouching / LowProfile** | Crouch active | Path A spine |
| **OnSlideStart / WhileSliding / SlideWindow** | Slide + short post-slide buffer | Path B spine |
| **OnMarkApplied** | Mark lands | Execute supports |
| **VanishActive** | Soft Vanish window | DR/speed/Ambush hyper |

Gun windows from blade cards buff *stowed/equipped primary* briefly — economy stays
thin and assassin-flavored (swap-up), not Fists Payroll.

---

## 8. Core Mechanics & Gunfeel

### 8.1 Dual-blade baseline

| Trait | Draft / intent |
|-------|----------------|
| Fire mode | Discrete dual-slash string (L then R cadence) — **not** continuous saw |
| M1 | Fast slash; Ambush often reads as cross-cut juice (both blades) |
| RMB | Throw **one** blade (see 8.5) |
| R | **Unused** baseline |
| Ammo | **None** |
| Element | Normal |
| Reach | Shortest melee kit (draft shorter than Fists) |
| Size | Tight precision volume (anti-pack without cards) |
| Falloff | **None** inside legal reach |
| Damage | High single-target; weak multi-target baseline |
| Opener | Mild bonus vs full HP / ≥95% HP on baseline |
| Ambush mult | ~1.25–1.4× when qualified (path cards raise) |
| Movement | Full crouch + slide while blades up — this *is* the gunfeel |
| Model / audio | Matched industrial stilettos / issue blades; quiet cloth + sharp *shink* on Ambush; throw whistle; retrieve snap. Borrow closest melee until custom art |

### 8.2 Why no ammo / reload

Same lesson as Saxonite Wrench: melee fantasy must not soft-lock mid-approach because
a cell ran dry. Cadence pressure = whiff recovery, throw-retrieve, and setup quality.

### 8.3 Input contract (MeleeRework)

| State | Tap V | Hold V (≥ ~0.25s) | M1 | RMB | Crouch | R |
|-------|-------|-------------------|----|-----|--------|---|
| Gun out | **Quick slash** (kit quick profile); stay on gun | **Equip Stalker's Blade** | Gun | Gun ADS/alt | **Low Profile** still applies to quick-V | Gun |
| Blades equipped | Quick slash in-place | Refresh / flourish | **Dual slash** | **Throw** | **Low Profile** | Unused baseline* |
| One blade out (thrown) | Slash (single-blade profile) | — | Single-blade M1 | Throw blocked or second throw if Twin Sheath | Low Profile | — |

\* R reserved for future exotic only — **none in frozen 30**.

### 8.4 Quick melee (tap V)

- Does **not** long-term change weapon slot (MeleeRework quick profile).
- Uses equipped melee kit's quick slash (Stalker's Blade if selected).
- **Crouch + quick-V** is a first-class assassin finisher without hold-V commit.
- Slide window + quick-V also valid.
- No throw on quick-V (gun still out; ADS ownership stays clean).

### 8.5 Full equip (hold V)

- Stow previous gun; blades in hands; full M1/RMB map.
- HUD: optional Mark / Vanish / blade-out pips (keep thin).
- Exit: normal weapon swap / shoot primary bind per MeleeRework.

### 8.6 Low Profile (crouch — baseline)

| Piece | Draft |
|-------|--------|
| Trigger | Player crouching (vanilla crouch state) |
| Ambush | Crouch hits **always qualify** for Ambush (subject to general Ambush rules) |
| Move | Vanilla crouch speed; **no free speed refund** on baseline |
| DR | **None free** — turtle is upgrade-owned and still mild |
| Audio/VFX | Quieter footsteps / dampened blade flourish while crouched |
| Gun-out | Yes — crouch quick-V benefits |
| Slide | Separate system; exiting crouch into slide keeps a short Low Profile grace only if a card says so |
| Stand | Loses Low Profile immediately (optional 0.1–0.15s card grace) |

**What Low Profile is NOT:** Fists Guard, invisibility, or a second crouch key.

### 8.7 Ambush (baseline, always on, modest)

A blade hit or throw hit is an **Ambush** if **any** of:

1. Attacker is crouching (Low Profile), **or**
2. Attacker is sliding **or** within post-slide buffer (draft ~0.4–0.6s), **or**
3. Hit is from behind / outside target forward cone (dot-product flank), **or**
4. Clean first strike: first blade/throw hit on that brain within ~6–8s **and** that brain has not damaged you recently (draft ~3–4s)

**Baseline Ambush:** damage mult ~1.25–1.4×; distinct *shink* + brief white flash.
Bosses / elites may receive reduced Ambush mult (playtest).

Ambush is **not** only backstab — crouch and slide must work in chaotic PvE where
perfect rear cones are rare.

### 8.8 Opener (baseline, always on, mild)

| Piece | Draft |
|-------|--------|
| Condition | Target current HP ≥ 95% max (or exact full HP — prefer ≥95% for chip forgiveness) |
| Effect | Mild damage bonus on blade/throw (draft ~1.1–1.15×), **stacks multiplicatively with Ambush** at reduced compound if both (tune: opener+Ambush should feel great, not delete bosses empty-grid) |
| Intent | Empty-grid teaches "open on fresh targets"; Path C owns the fantasy peak |

### 8.9 RMB — Thrown Knife (baseline)

| Piece | Draft |
|-------|--------|
| Input | Tap RMB while **blades fully equipped** |
| Projectile | Short–mid arc / fast throw (not a sniper lob) |
| Damage | Modest vs M1 Ambush; still meaningful on opener/Ambush throw |
| Mark | **Applies Mark** on hit (baseline soft Mark duration draft ~4–6s) |
| Ambush | Throw can Ambush via crouch / slide window / flank / clean first strike |
| Cost | **One blade out** — melee becomes single-blade profile (slightly slower or weaker string) until retrieve |
| Retrieve | On throw hit, on Ambush melee kill, or miss timer (~2–3s) — blade snaps back |
| CD | Soft gated by blade-out state more than a long cooldown; tiny throw recovery ~0.2–0.3s |
| Fail | Throw with blade already out: blocked (unless Twin Sheath / Twin Fang rules) |
| Gun-out | **No throw** — preserves gun RMB/ADS |

**Intent:** Always teaches mid-range Mark + dual-blade cost. Weak enough that path crowns matter.

### 8.10 Mark (baseline from throw; path expands)

| Piece | Draft |
|-------|--------|
| Source baseline | Throw hit |
| Duration | ~4–6s, refreshable |
| Effect baseline | Mild damage taken amp vs **your** blade/throw only (draft ~1.08–1.12×) **or** pure hook with no free amp (prefer tiny amp so throw isn't dead) |
| Path C | Stronger amp, finisher gates, Poison synergy, Black Warrant execute |
| Clarity | Small mark VFX on enemy; no full DMLR anatomy UI required |

### 8.11 Soft Vanish (upgrade-owned peak; tiny baseline optional)

**v1 lock:** No meaningful baseline Vanish. Ghost Protocol / Afterimage own it.

| Piece | Draft (when active) |
|-------|---------------------|
| Move speed | Noticeable burst |
| DR | Mild (not Guard, not Aegis) |
| Next Ambush | Hyper mult or guaranteed quality |
| Duration | ~2–3s Ghost; shorter Afterimage |
| Opacity | **No** real invis in v1 |
| Blade | Ghost Ambush kill can instant-retrieve (Twin Sheath synergy) |

### 8.12 Base combat loop (no upgrades)

```
Gun fight → crouch or slide → tap V Ambush finisher / opener on fresh targets
OR hold V → blades up → crouch approach or slide in → M1 dual slash Ambush
         → RMB throw to Mark straggler → close and execute
         → retrieve → stow when gun is better
```

Skill without upgrades: approach angle, crouch timing, throw-then-cut sequencing,
not face-tanking with butter knives, spending opener on the right target.

### 8.13 What baseline does NOT include

- Free Guard / crouch DR
- Free Vanish
- Free Poison
- Free Handspring / Pile Driver
- Free ammo / grenade Payroll
- Pack shockwave or sustained pull
- Throw while gun out
- True stealth / aggro drop

---

## 9. Upgrade Paths (gravity wells — hybrids intended)

### Path A — LOW PROFILE (Crouch / stalker)
**"Stay small. Strike once. Be gone."**

- Spine: crouch move speed, crouch Ambush quality, quiet approach, panic crouch, opener-from-crouch, soft Vanish supports
- **Crown: Ghost Protocol** — Ambush kill while Low Profile (or shortly after) → soft Vanish window + next Ambush hyper; instant blade retrieve
- Hybrid: Slide enters crouch sticky; Execute makes Vanish kills delete elites; Twin Sheath snap-back mid-Vanish

### Path B — SLIDE CUT (Slide / momentum)
**"The floor is the sheath."**

- Spine: slide → next blade hit amp (Slideshot DNA, modest), slide extend on blade damage (Tumbleweed DNA), dust-kick / landing-roll cousins on blade grid, slide Ambush buffer
- **Crown: Railblade** — during slide, M1 becomes **lunge cut** (distance + light pierce 1–2 grunts + guaranteed Ambush). Recovery gated — not Carver clear
- Hybrid: Low Profile after rail; Execute Marks everything the lunge touches; Twin Sheath dual-thrust lunge juice

### Path C — EXECUTE (Opener / Mark / Poison)
**"First cut writes the ending."**

- Spine: full-HP opener mult, Mark on hit (not only throw), finisher vs low HP / Marked, **Poison** apply (Needle Carbine EffectType), throw tips
- **Crown: Black Warrant** — Ambush vs Marked **or** strong opener threshold = execute mult; grunts can partial true-kill; elites take massive chunk + short **gun expose** window for stowed primary
- Hybrid: Slide applies Mark on pass; crouch Ambush feeds Warrant; Poison from throw + melee

### Glue exotic (peer, not a fourth well prison)

**Twin Sheath** — dual-blade crown. Both blades in hand = quality; throw rules upgrade; Ambush kill snaps both back + dual-flourish tempo.

### Path × verb matrix

```
                  LOW PROFILE           SLIDE CUT              EXECUTE
Crouch            core fantasy          exit into slide        opener-from-crouch
Slide             grace / sticky        core fantasy           Mark on pass
Ambush            Vanish gate           guaranteed on rail     Warrant gate
Throw             retrieve on Ghost     momentum throw         Mark / Poison tip
Full HP opener    mild support          gap-close into open    core fantasy
Poison            —                    —                      opt-in spine
Dual blades       retrieve / flourish   dual lunge juice       Twin Sheath + Warrant
```

---

## 10. Crowns & Signature Cards

### Ghost Protocol — Exotic (Low Profile crown)
- On **Ambush kill** while crouching **or** within short post-crouch grace: gain **soft Vanish** (~2–3s)
- Vanish: +move speed, mild DR, next Ambush hyper
- Instant retrieve if blade out
- ICD / duration caps so it is a rhythm, not perma-DR
- Clear desat/afterimage VFX without true invis

### Railblade — Exotic (Slide Cut crown)
- While sliding (and optional tiny post-slide window): M1 = **lunge cut**
- Lunge: forward distance, light pierce through 1–2 non-boss grunts, **guaranteed Ambush**
- Recovery after lunge prevents infinite hallway delete
- Dual-blade presentation: crossed thrust
- Bosses: no multi-pierce; single hard Ambush hit

### Black Warrant — Exotic (Execute crown)
- Ambush against **Marked** target **or** opener-threshold target: execute mult
- Grunts: high chance to finish / partial true-kill rules (tune — fun, not mission-break)
- Elites/bosses: large Ambush chunk + short **Expose-style window** for *your* guns (DMLR soft literacy, simpler)
- Optional: if Venom Edge present, Warrant Ambush dumps bonus Poison
- Self-hitch tiny (~0.08–0.12s) on big Warrant procs so it reads as a decision

### Twin Sheath — Exotic (Dual-blade glue crown)
- While **both blades in hand**: +Ambush quality and/or +opener
- On throw: delayed **second blade follow-up** throw (short gap) **or** empowered off-hand M1 buffer after throw (pick one primary rule in impl; recommend **follow-up throw** for readable dual fantasy)
- On Ambush kill: both blades snap back immediately + brief dual-flourish (+attack speed / shorter recovery)
- Does not replace Ghost/Rail/Warrant; feeds all three

### Afterimage — Epic (mini-Ghost)
- Perfect Ambush (e.g. crouch+flank or opener+Ambush) grants short mini-Vanish without full Ghost Protocol
- Lets Path A feel before exotic; stacks duration rules with Ghost (refresh, don't infinite)

### Venom Edge — Epic (Poison enabler)
- Ambush melee applies `EffectType.Poison` (amount draft tuned to meaningful sat without deleting Needle Carbine identity)
- Soft duo: carbine can pre-paint; blade executes
- Without this or Poison Tipped, blade stays kinetic

### First Blood — Epic (Opener spine)
- Strongly increases full-HP opener bonus; opener hits refresh a short Ambush quality buffer
- The "assassin opens the fight" card

### Gunhand Cut — Epic (thin economy / swap)
- On Ambush (or Ambush kill): brief damage window for **stowed/primary gun** + tiny mag crumb optional
- Fisticuffs inverse — knuckles/blade feed the gun you swap to
- **Not** Payroll multi-resource; keep numbers modest

### Boundary Incursion — Oddity
- Grid grow — universal keep

---

## 11. Full Upgrade List (~30 ship + backlog)

Rarity: Standard / Rare / Epic / Exotic / Oddity  
Tags: A Low Profile · B Slide Cut · C Execute · G Glue / dual / gunfeel  
Cell rule: Exotics larger; all Exotics same cell count.  
Names are player-facing.  
Numbers are **v0 starting targets** — validate in playtest.

Suggested IDs at impl (verify collisions): gear **92900**; upgrades **92901–92930**.

------------------------------------------------------------------------------
EXOTICS (4) — equal large shapes
------------------------------------------------------------------------------

X1. Ghost Protocol — Exotic (A) — Low Profile crown
    Ambush kill while/after Low Profile → soft Vanish + next Ambush hyper + retrieve.

X2. Railblade — Exotic (B) — Slide Cut crown
    Slide M1 = lunge cut; light grunt pierce; guaranteed Ambush; recovery gated.

X3. Black Warrant — Exotic (C) — Execute crown
    Ambush vs Marked or opener threshold → execute; elites gun-expose window.

X4. Twin Sheath — Exotic (G) — Dual-blade crown
    Both blades quality; throw follow-up; Ambush kill full retrieve + flourish.

------------------------------------------------------------------------------
EPICS (7)
------------------------------------------------------------------------------

E1. Afterimage — Epic (A)
    Perfect Ambush → short mini-Vanish (no full Ghost required).

E2. Venom Edge — Epic (C)
    Ambush melee applies Poison (EffectType.Poison).

E3. Mark of Cain — Epic (C)
    Melee hits apply/refresh Mark (not only throw); +damage vs Marked.

E4. Finisher's Eye — Epic (C)
    Bonus blade damage vs low-HP targets (execute floor).

E5. First Blood — Epic (C)
    Strong full-HP opener bonus; opener feeds short Ambush quality buffer.

E6. Slide Sheath — Epic (B)
    Slide speed/distance while blades relevant; slide Ambush buffer longer;
    modest next-blade-after-slide amp (identity, not Aussie-style DPS crutch).

E7. Gunhand Cut — Epic (G)
    Ambush → brief stowed/primary gun damage window (+ optional tiny mag crumb).

------------------------------------------------------------------------------
RARES (11)
------------------------------------------------------------------------------

R1. Slideshot Shiv — Rare (B)
    After sliding, next blade hit deals increased damage (modest band).
    Melee-native sticky of wiki/vanilla Slideshot.

R2. Low Heel — Rare (A)
    +Move speed while crouching (stay-low chase without standing).

R3. Flank Calculus — Rare (G)
    Wider Ambush flank cone / more forgiving behind-dot.

R4. Stalk Training — Rare (A)
    +Ambush mult while crouching; tiny Quiet Step overlap OK.

R5. Tumble Edge — Rare (B)
    Dealing blade damage extends slide and/or shortens slide recovery (Tumbleweed DNA).

R6. Dust Cut — Rare (B)
    Jump out of slide → Ambush buffer + brief upward/forward assist (Dust Kick cousin).
    Not a second movement ability.

R7. Soft Sole — Rare (A/B)
    Landing into slide/crouch grants short Ambush window (Landing Roll DNA, blade-owned).

R8. Poison Tipped — Rare (C)
    Throw applies Poison (lighter than Venom Edge melee unless stacked).
    Useful without Venom; shines with Warrant.

R9. Keen — Rare (G)
    +Ambush damage mult (all qualifiers).

R10. Retrieve Line — Rare (G)
    Faster blade return on miss timer; throw recovery down.

R11. Cross-Cut — Rare (G)
    Dual-slash: chance for second tick / off-hand follow-through on Ambush.
    Sells dual blades without Twin Sheath.

------------------------------------------------------------------------------
STANDARDS (7)
------------------------------------------------------------------------------

S1. Honed Edge — Standard (G)
    +Blade and throw damage. Stackable mild.

S2. Needle Point — Standard (G)
    −Melee size, +damage. Precision butcher (Carver Needle Point cousin).

S3. Longer Shank — Standard (G)
    +Reach. Still short vs other melee kits; no falloff.

S4. Quick Draw — Standard (G)
    Faster quick-V startup / shorter gun-return gap.

S5. Slippery Grip — Standard (G)
    Shorter swing recovery after hit or whiff.

S6. Opener's Grip — Standard (C)
    +Full-HP opener bonus (baseline opener still exists without it).

S7. Quiet Step — Standard (A)
    Stronger crouch audio/VFX damp + tiny Ambush quality while crouched.

------------------------------------------------------------------------------
ODDITY (1)
------------------------------------------------------------------------------

O1. Boundary Incursion — Oddity (G)
    +Upgrade grid size.

------------------------------------------------------------------------------
FROZEN v1 SHIP POOL (exactly 30)
------------------------------------------------------------------------------

  EXOTIC (4)
    1  Ghost Protocol
    2  Railblade
    3  Black Warrant
    4  Twin Sheath

  EPIC (7)
    5  Afterimage
    6  Venom Edge
    7  Mark of Cain
    8  Finisher's Eye
    9  First Blood
    10 Slide Sheath
    11 Gunhand Cut

  RARE (11)
    12 Slideshot Shiv
    13 Low Heel
    14 Flank Calculus
    15 Stalk Training
    16 Tumble Edge
    17 Dust Cut
    18 Soft Sole
    19 Poison Tipped
    20 Keen
    21 Retrieve Line
    22 Cross-Cut

  STANDARD (7)
    23 Honed Edge
    24 Needle Point
    25 Longer Shank
    26 Quick Draw
    27 Slippery Grip
    28 Opener's Grip
    29 Quiet Step

  ODDITY (1)
    30 Boundary Incursion

Count: 4X + 7E + 11R + 7S + 1O = **30**.

BACKLOG (designed, expand later)
  Silence Tax (Ambush kill → ability/slide/grenade crumb — thin economy exotic/epic)
  Off-hand Parry (tiny chip DR / nullify while blades up — not Fists Guard)
  Twin Fang (double throw without full Twin Sheath)
  Core Nick (core/anatomy Ambush bias)
  Bleed Edge (kinetic DoT if Poison unavailable / alternate finisher)
  Ally Mark share
  Real opacity Vanish experiment
  Panic Crouch (damage → brief free Low Profile DR)
  Crouch Plating (mild DR while crouched — carefully tuned)
  Hands-Up Bonus inverse (gun-out quick-V Ambush economy)
  Smart Edge (Railblade mild auto-aim assist — Smart Slide cousin)
  Knee Sheath (slide-out-of-air DNA if Glider synergy desired)
  Flay (Ambush shreds limb/shell chunk — anatomy beat)
  Throw while quick-melee special (rejected for v1 input clarity)

------------------------------------------------------------------------------
CUT / DEMOTE
------------------------------------------------------------------------------

| Idea | Fate |
|------|------|
| RMB = Stalk hold stance | **Cut** — crouch owns Low Profile; RMB = throw |
| Baseline crouch DR | **Cut** — upgrade-owned only if ever |
| True invis Vanish | **Cut** from v1 — soft only |
| Payroll-class ammo engine | **Cut** — Fists owns; Gunhand Cut thin only |
| Pile Driver / Handspring on blade | **Cut** — Wrangler / Fists |
| Shockwave / gravity well | **Cut** — Wrench |
| Blood meter / heavy feast | **Cut** — Carver |
| Continuous saw ticks | **Cut** — Carver |
| Poison baseline on all hits | **Cut** — opt-in Execute |
| Throw with gun out | **Cut** — ADS clarity |
| Hard dep Needle Carbine | **Cut** — soft duo only |

---

## 12. Poison Relationship (Needle Carbine)

| Rule | Detail |
|------|--------|
| Baseline blade | No Poison |
| Enablers | Venom Edge (melee Ambush), Poison Tipped (throw), optional Black Warrant rider |
| System | Real `EffectType.Poison` per Needle Carbine design (confirm enum id at impl; Cryo=10, Poison draft=11) |
| Hard dependency | **None** |
| If carbine present | Same EffectType; sat bars stack cleanly; carbine paints, blade executes / vice versa |
| If carbine absent | Blade still applies Poison **if** the EffectType exists in the shared game/mod status pipeline; if Poison is only added by carbine mod, impl options: (a) blade registers minimal Poison status when equipped cards need it, or (b) Poison cards no-op with log once — **prefer (a)** small self-contained Poison apply for card honesty |
| Not trying to be | Supercombine needles, Extract/Mercy Kill consume loop, Own Medicine self-dose |

**Soft duo tips (README / codex only):**
- Needle Carbine saturates → Stalker's Blade Ambush / Black Warrant finishes
- Blade Mark + Poison Tipped → carbine Sadist consume menu stays full
- No package dependency either direction

---

## 13. Example Builds

### Ghost Stalker (pure Low Profile)
Quiet Step → Low Heel → Stalk Training → Afterimage → **Ghost Protocol** → First Blood → Keen  
*Play:* Crouch-walk the edge, Ambush, Vanish, restalk. Throw only to Mark before close.

### Railblade Courier (pure Slide Cut)
Slideshot Shiv → Tumble Edge → Dust Cut → Soft Sole → **Railblade** → Slide Sheath → Quick Draw → Twin Sheath  
*Play:* Slide-lunge delete, quick-V between rails, dual retrieve on kills.

### Venom Warrant (pure Execute)
Opener's Grip → First Blood → Mark of Cain → Venom Edge → Poison Tipped → Finisher's Eye → **Black Warrant** → Gunhand Cut  
*Play:* Throw Mark/Poison → Ambush Warrant → swap to primary into expose window.

### Poster hybrid (recommended fantasy complete)
Ghost Protocol + Railblade + Black Warrant + Twin Sheath + First Blood + Slideshot Shiv + Venom Edge  
*Play:* Slide in, Marked/Poisoned Ambush execute, Vanish out, blades snap back, gun cleans elites.

### Gun-out assassin (minimal full equip)
Quick Draw → Keen → Opener's Grip → Stalk Training → Gunhand Cut → Afterimage  
*Play:* Stay on primary; crouch/slide tap-V openers and finishers; hold V only for throw Mark setups.

---

## 14. Strengths, Weaknesses & Failure Modes

### Strengths
- Unique arsenal fantasy (stealth/assassin) with zero overlap on sibling melee verbs
- Crouch stealth works on quick-V without extra keys
- Throw gives mid-range without becoming a primary
- Dual blades readable cost/retrieve loop
- Opener + Ambush skill expression
- Soft Needle Carbine duo without hard deps
- Thin gun-swap economy respects Fists Payroll

### Weaknesses
- Pack clear without Slide/Mark investment (by design)
- No baseline Guard — wrong tool for face-tank piles
- Throw blade-out state punishes spam
- Pure static bosses may prefer anatomy saw or guns if opener windows are missed
- Crouch in a horde shooter feels bad if Low Profile rewards are too weak (tune speed/Ambush)

### Failure modes to avoid in tuning

| Failure | Mitigation |
|---------|------------|
| Crouch turtle meta | No free DR; Vanish mild and gated on Ambush kill |
| Railblade hallway delete | Pierce cap 1–2 grunts; recovery; bosses no pierce |
| Black Warrant boss melt | Reduced execute vs bosses; expose window not 100% transfer |
| Ghost perma-DR | Short duration; ICD; mild DR only |
| Twin Sheath double-throw chaos | Follow-up delay; retrieve rules; boss target caps |
| Opener + Ambush empty-grid delete | Mild baseline opener; compound soft-cap |
| Poison steals Needle Carbine | Blade apply amounts modest; no supercombine |
| Gunhand Cut = Payroll | Tiny window/crumb only |
| Ambush cone useless in chaos | Crouch + slide always qualify — flank is bonus, not required |
| Throw grief / stolen ADS | Blades-up only |
| Feels like Fists with knives | No Guard, no Payroll, tight size, throw identity, opener/Ambush language |
| Feels like Carver | Discrete slashes only; no blood; no hold-M1 tick |

---

## 15. Naming & Presentation

| Slot | Value |
|------|--------|
| Display name | **Stalker's Blade** |
| Internal / API | `stalkers_blade` |
| Design nicknames | Knife, Issue Blade, Twin Shivs (notes only) |
| Short description | *Matched SAXON issue knives. No ammo, no reload — crouch to stalk, slide to cut, throw to mark. Ambush from low profile and open on full-health targets; bolt on ghost vanish, railblade lunges, or venom warrants.* |
| Thunderstore name (later) | `StalkersBlade` |
| GUID (later) | `sparroh.stalkersblade` |
| Folder today | `.new.KnifeMelee` |
| Suggested IDs | Gear **92900**; upgrades **92901–92930** (verify at impl) |
| Design doc file | `StalkersBlade-DesignDoc.txt` |

### SAXON marketing blurb (draft)

> SAXON Stalker's Blade — For employees who believe HR doesn't need every detail.  
> Two blades. Zero cells. Zero "please reload while the fungus writes your eulogy."  
> Baseline: crouch low, cut clean, throw a mark, walk away.  
> Aftermarket: ghost protocol, railblade commuting, black warrants, twin-sheath hospitality.  
> If your melee option comes with a magazine UI or a chainsaw disclaimer, you are holding the wrong product.  
> "If they saw you coming, file it under professional development."

### Flavor / in-world

Paired industrial stilettos issued to quiet contractors and lost more often than admitted.
Matte Saxonite edges hold a toxin channel for aftermarket tips (grid not included — wait,
grid is included). Official training manual redacts the chapter on crouching.

---

## 16. Synergy Notes (Player-Facing, Soft Only)

| Partner | Why it feels good |
|---------|-------------------|
| **MeleeRework / Fists** | Slot platform; Fists for brawler days, Blade for assassin days — swap kits |
| **Needle Carbine** | Poison paint ↔ Ambush execute; Mark keeps charts full |
| **DMLR / precision guns** | Gunhand Cut + Warrant expose → primary dumps |
| **Aussie / slide guns** | Slide culture transfers; blade owns melee slide amp |
| **Wrangler tree** | Landing Roll / Dust Kick / Tumbleweed stack softly with blade cards |
| **OmniMovement** | Slide strafe into Railblade feels better |
| **AimAndCrouchToggles** | Toggle crouch Low Profile comfort |
| **Blood Carver / Wrench** | Dual-melee meme loadouts; different verbs — no shared meter |
| **Shocklance Fisticuffs** | Soft co-op joke if someone else melees |

**Explicit non-goals v1:** hard deps; heavy ammo feed; blood meter; mag/reload; baseline Poison; baseline Vanish DR tank; replacing Fists as default melee.

---

## 17. Success Criteria / Player Fantasy Checklist

- [ ] Stalker's Blade appears as a selectable **melee kit** (MeleeRework slot)
- [ ] Tap V quick slash useful with **zero** upgrades
- [ ] Hold V equips dual blades with clear feedback
- [ ] **Crouch** enables Low Profile Ambush without a second stance key
- [ ] Crouch + quick-V finisher works gun-out
- [ ] RMB throw Marks and puts one blade out; retrieve is reliable
- [ ] Full-HP opener is noticeable empty-grid
- [ ] Ambush *shink* reads without a tutorial dump
- [ ] Ghost build: Ambush kill → soft Vanish → restalk
- [ ] Railblade build: slide lunge deletes a line of grunts, not the whole map forever
- [ ] Warrant build: Mark → Ambush execute → gun window on elites
- [ ] Twin Sheath makes dual blades feel mandatory-cool, not cosmetic
- [ ] Poison cards work without hard-requiring Needle Carbine package
- [ ] No ammo UI / no reload beat
- [ ] Does not obsolete Fists Guard/Payroll or Wrench/Carver fantasies
- [ ] Failure states stay fun (blade out, missed opener, stood up too early)
- [ ] Sandbox MP: crouch/slide/throw/Ambush readable enough on clients

---

## 18. Implementation Appendix (For Later — Not This Pass)

Design-only milestone: **this document**. When coding starts:

| Piece | Approach |
|-------|----------|
| Platform | **MeleeRework** `RegisterKit` / `IMeleeKit` quick + full profiles |
| Registration | Melee gear catalog entry; persistence by gear id; fallback Fists if missing |
| Host | `StalkersBladeBehaviour` + `Data` struct for Ambush/opener/throw/Vanish/Poison flags |
| Base type | Evaluate `MeleeGear` / Throwable DNA vs custom hitcast after Assembly lookup — **do not invent APIs** |
| Crouch / slide | Read real player movement flags each hit; buffer timers for slide window |
| Ambush | Server/owner-authoritative mult on damage path; distinct VFX/audio |
| Throw | Short projectile or hitscan-arc using game damage APIs; blade-out state replicated |
| Poison | `EffectType.Poison` on DamageData when cards say so; share Needle Carbine rules |
| Upgrades | `PlayerData.CreateUpgrade` + `UpgradeProperty` Apply/Remove on FindGear(`stalkers_blade`) |
| No ammo | Infinite / bypass; hide reload; R no-op |
| Mod flags | `[MycoMod(..., ModFlags.IsSandbox)]` |
| Deps | Soft recommend MeleeRework; document hook version |
| Model | Dual blade meshes when AssetBundle exists; placeholder OK |

### Suggested `StalkersBladeBehaviour.Data` fields (sketch)

```
// Core mults
float damageMult;
float reachMult;
float sizeMult;
float recoveryMult;
float quickDrawMult;

// Ambush / opener
float ambushDamageMult;
float flankConeDot;          // wider with Flank Calculus
float openerHpThreshold;     // 0.95 default
float openerDamageMult;
bool keen;

// Low Profile
float crouchMoveSpeedMult;
float crouchAmbushMult;
bool quietStep;
float afterimageDuration;
float ghostVanishDuration;
float ghostMoveMult;
float ghostDamageTakenMult;
float ghostNextAmbushMult;
bool ghostProtocol;

// Slide
float slideWindowDuration;
float slideshotMult;
float slideDurationBonusOnHit;
bool railblade;
float railbladeLungeDistance;
int railbladePierceGrunts;
float railbladeRecoveryMult;
bool slideSheath;
bool dustCut;
bool softSole;
bool tumbleEdge;

// Throw / dual
float throwDamageMult;
float throwRangeMult;
float retrieveTimeMult;
bool bladeOut;
float markDuration;
float markDamageTakenMult;   // vs your blade
bool twinSheath;
float twinFollowUpDelay;
bool crossCut;
float crossCutChance;
bool retrieveLine;

// Execute / Poison
bool markOnMeleeHit;         // Mark of Cain
float markedDamageMult;
float finisherLowHpThreshold;
float finisherDamageMult;
bool firstBlood;
bool venomEdge;
float venomPoisonAmount;
bool poisonTipped;
float throwPoisonAmount;
bool blackWarrant;
float warrantExecuteMult;
float warrantGunExposeDuration;
float warrantGunExposeMult;

// Thin economy
bool gunhandCut;
float gunhandDuration;
float gunhandDamageMult;
float gunhandAmmoCrumb;      // keep tiny or 0
```

### Ship cut vs stretch

**v1 must-ship (fantasy complete):**
- Melee kit registration + tap/hold V profiles
- Dual slash + crouch Low Profile Ambush + mild opener
- RMB throw + Mark + blade-out retrieve
- All **4** exotics (Ghost, Railblade, Warrant, Twin Sheath)
- Venom Edge / Poison Tipped Poison spine
- Slide rares + First Blood + Gunhand Cut
- Soft Vanish only; thin economy; no ammo
- Self-contained Poison apply policy documented

**Stretch / post-v1:**
- Silence Tax, Off-hand Parry, Flay, real opacity
- Custom meshes / Wwise
- Config knobs for Ambush mult / opener threshold
- Convert-era notes if other kits move slots

### Phased delivery (when coding)

| Phase | Deliverable |
|-------|-------------|
| K0 | Register kit + quick/full slash + no ammo |
| K1 | Crouch Ambush + opener + juice |
| K2 | Throw + Mark + blade-out retrieve |
| K3 | Slide window + Slideshot Shiv + Railblade |
| K4 | Ghost Protocol + Afterimage soft Vanish |
| K5 | Execute spine + Black Warrant + Poison cards |
| K6 | Twin Sheath + Cross-Cut dual fantasy |
| K7 | Frozen 30 registration + balance + README |

---

## 19. Open Tuning Questions (playtest, not design blockers)

1. Baseline Ambush mult 1.25 vs 1.35 vs 1.4  
2. Opener threshold full-only vs ≥95%  
3. Opener × Ambush compound soft-cap  
4. Throw retrieve 2s vs 3s; hit-retrieve instant or short delay  
5. Twin Sheath follow-up throw vs empowered off-hand M1  
6. Railblade pierce 1 vs 2 grunts; lunge distance  
7. Ghost DR % and duration  
8. Black Warrant grunt true-kill rules vs big damage only  
9. Venom Edge poison amount vs Needle Carbine dart pace  
10. Whether quick-V can throw-Mark ever (default **no**)  
11. Boss Ambush resist curve  
12. Exact hex shapes for 30 upgrades — author during implementation  

---

## 20. Relationship to Sibling Projects

| Project | Relationship |
|---------|----------------|
| **MeleeRework** | **Hard platform** when shipped — slot, tap/hold V, kit API |
| **Fists** | Peer melee kit; different verbs; do not steal Guard/Payroll/Haymaker/Pile Driver |
| **Blood Carver** | Primary saw until convert; no shared blood; contrast lock |
| **Saxonite Wrench** | Primary slam until convert; no shockwave/pull steal |
| **Needle Carbine** | Soft Poison duo; no package dep |
| **DMLR** | Mark/Expose structural precedent only |
| **Aussie Special** | Slideshot budget lesson — movement amp modest |
| **OmniMovement / crouch toggles** | Soft UX synergy |
| **Weapon template in this folder** | Scaffold only; product is melee kit + design doc first |

---

## 21. Universal Truths (Mycopunk alignment)

- Exotic shapes should always be larger than others; each exotic the same cell count.
- v1 targets **~30** upgrades (frozen list above); backlog is real design, not trash.
- Three paths create different builds but **may intermingle** on the grid.
- Prefer verbs: Ambush, Low Profile, slide window, opener, Mark, throw/retrieve, soft Vanish, optional Poison.
- No second blood/torque meter.
- No ammo/reload on this kit.

---

## 22. Design Checklist

- [x] Name: **Stalker's Blade**  
- [x] Melee kit via MeleeRework (not primary)  
- [x] Stealth on **crouch** (Low Profile)  
- [x] RMB = **thrown knife**  
- [x] Dual blades mechanical  
- [x] Full-HP **opener** baseline + path  
- [x] Ambush vocabulary (crouch / slide / flank / first strike)  
- [x] Soft Vanish only  
- [x] Poison opt-in (Needle Carbine EffectType)  
- [x] Thin economy (Gunhand Cut; no Payroll)  
- [x] 4th exotic: **Twin Sheath**  
- [x] Three wells: Low Profile / Slide Cut / Execute  
- [x] Frozen 30 table  
- [x] Differentiated from Fists / Carver / Wrench  
- [x] Wiki slide DNA borrowed with renames  
- [x] Failure modes documented  
- [x] Implementation deferred  

---

## 23. Design Changelog

### v1 (this doc) — 2026-08-07

- Product: **Stalker's Blade** melee kit for MeleeRework hooks
- User locks: name; crouch stealth (not RMB stalk); RMB throw; dual knives; full-HP opener; soft Vanish; poison from Needle Carbine research (opt-in); thin economy default; Twin Sheath 4th exotic
- Paths: Low Profile / Slide Cut / Execute + Twin Sheath glue
- Baseline: dual slash, crouch Ambush, mild opener, throw Mark + blade-out retrieve, no ammo
- Frozen 30 + backlog + sibling contrast + Poison policy
- Rejected: RMB hold-stalk, true invis, Payroll engine, baseline Poison, gun-out throw

---

## 24. Next Steps After This Doc

1. Confirm MeleeRework hook surface when that project implements RegisterKit  
2. Implement K0–K2 (kit + crouch Ambush + throw) against decompile  
3. Layer slide / Ghost / Warrant / Twin Sheath  
4. Register frozen 30  
5. Playtest Ambush mult, opener compound, Railblade pierce, Poison amounts  
6. Art/audio pass when ready  

---

*End of Stalker's Blade Design Doc v1*
