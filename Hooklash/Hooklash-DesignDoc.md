# Hooklash — Design Document (v1)

> Status: **Design only** — implementation follows this doc and MeleeRework hooks.
> Package / Thunderstore name (planned): **Hooklash** / **WhipMelee**
> GUID (planned): `sparroh.hooklash`
> MycoMod flags: **IsSandbox** (gameplay rules; melee kit)
>
> Product shape: **Melee kit** (`GearType.Melee`) that registers through **MeleeRework**
> extension hooks. Not a primary weapon. Not a Fists reskin. Not a Scrapper Grapple
> or Wrangler Rocket Lasso replacement.
>
> Sibling docs (do not absorb; contrast locks below):
>   - MeleeRework — slot platform, tap/hold V, Fists kit, extension API
>   - Stalker's Blade — short dual slash + throw knife + crouch Ambush (contrast)
>   - Phalanx Impaler — long thrust string + buckler + javelin pin (contrast)
>   - Blood Carver — continuous saw + blood + heavy feast (primary until convert)
>   - Saxonite Wrench — discrete slam + shockwave + AOE gravity pull (primary)
>   - Boarding Trident — multi-prong rifle (name/fantasy collision only — different slot)
>   - DMLR Rework — Mark / Expose vocabulary precedent (thin gun window only)
>
> Wiki / vanilla DNA borrowed (rename on whip grid; no hard character deps):
>   - Rocket Lasso / Yank em! / Loose Rope / Extra Rope / Quick Knot → Reel force/range/CD
>   - Rocket Jump → surface-hook reverse launch DNA
>   - Wrecking Ball → Pendulum swing-into-enemies damage
>   - Spare Cable / Wireless Extenders / Tether Battery / Solar Panel → cast range / sustain
>   - Impulse Vault → detach speed burst
>   - The Ole One-Two → post-reel lash amp
>   - Dynamite on a String → backlog boom-cord only
>   - Double Lasso → **Double Coil** exotic
>   - Super Magnet / Electromagnet → tip seek + multi-pull lite (not saw aura)
>   - Ammo Continuity → thin gun crumb on cast/reel
>   - Handspring stays Wrangler; Fists owns Pile Driver — whip does not steal
>   - Laying Cable (Shocklance) → soft ally-tether backlog only
>
> Vanilla anchors (implement against decompile, do not invent APIs):
>   - MeleeRework: tap V quick-melee, hold V equip, `IMeleeKit` / RegisterKit
>   - Player knockback / force / pull patterns (Wrench, Carver magnet, lasso)
>   - Surface hit / melee jump DNA (`meleeJumpForce` family — Pendulum is whip-owned swing, not Pile Driver)
>   - `EffectType` Shock for opt-in tip cards
>   - No native AI "lasso state" — reel is invented kit vocabulary (below)

---

## 1. High Concept / Fantasy

**Hooklash** is SAXON's issue coil-whip for employees who treat architecture as a handhold
and enemy mass as a scheduling conflict.

You do not saw the pile. You do not crack the floor. You do not crouch-vanish.
You do not raise a plate and throw a shaft.

You **crack an arc**, and you **cast a tether** that is context-smart:
- **Terrain hook** → reel *you* to the world (gap-close, vertical, swing)
- **Enemy hook** → reel *them* to you (herd, isolate, execute)

Baseline is honest hybrid melee — a two-hit lash string, a bidirectional tether on RMB,
no ammo, no reload. The grid forks the kit into skyhook acrobatics, cattle-call herding,
or multi-tip crack volleys — with optional shock tips for employees who like their cable live.

**One-liner:** *Crack the room. Hook the wall. Yank whatever isn't bolted down — including you.*

**Element baseline:** Normal (kinetic). Shock is **opt-in Crack-path cards only** — no forced chemistry.

---

## 2. Role & Fantasy in the Arsenal

| | |
|--|--|
| **Slot** | **Melee** (`GearType.Melee`) via MeleeRework registration hooks |
| **Range** | Mid melee lash arc + mid–long **tether cast** |
| **Role** | Movement tool + herder hybrid — gap-close via surface reel, pack control via enemy yank, clear via crack arcs |
| **Gap filled** | Fists = punch + omni Guard + economy. Blade = short assassin + knife throw + crouch Ambush. Impaler = long thrust + buckler + javelin pin. Carver = saw. Wrench = slam + AOE gravity well. Scrapper/Wrangler = character ability energy. **Nothing owns bidirectional aimed cable tether + sweeping lash combat on a melee kit.** |
| **Synergies** | MeleeRework slot; stowed primary after reel window; OmniMovement strafe-reel; Shock grenades / wet-shock kits (soft); CQC mission modifier; co-op herd into ally AOE |
| **Not trying to be** | Fists brawler, Blade stalker, Impaler hoplite, Carver blender, Wrench gravity hammer, Scrapper Grapple Pole, Wrangler Rocket Lasso, Payroll ammo engine |

**Product shape:** New melee kit (**Hooklash**). Default fallback remains Fists.
Does **not** replace vanilla MeleeGear punch in-place; registers as its own catalog entry.

---

## 3. Design Pillars

1. **Bidirectional tether is the identity.** Surface → you. Enemy → them. Same cast, context resolve.
2. **Aimed cable, not AOE well.** Wrench owns pulse/vortex gravity. Hooklash owns a line with an attach point.
3. **Hybrid is baseline.** 2-hit lash + tether both exist empty-grid; paths crown them, they do not unlock existence.
4. **RMB is tether. R is free.** Melee has no reload — R unused baseline (Double Coil does not steal R).
5. **Two-hit string is the melee gunfeel.** Opener crack → heavier finisher. Readable payoff without Impaler's three-step doctrine.
6. **Discrete lashes, not a saw or slam.** Faster recovery than Wrench, wider than Blade, zero continuous tick DPS.
7. **No ammo. No reload.** Cadence = string recovery + tether CD/break + swing commitment.
8. **Three peer paths + glue + charge exotic.** Skyhook / Reel / Crack + Live Coil + Double Coil. Hybrids intended.
9. **Pendulum is exotic, not baseline.** Empty-grid teaches cast-and-reel; swing mastery is Skyhook crown.
10. **Shock is opt-in.** Real shock status when enabled; kinetic without those cards.
11. **Thin economy.** Fists owns Payroll. Hooklash may open a short gun window after reel only.
12. **~30 upgrades;** exotic shapes larger than others; each exotic the **same** cell count.
13. **Don't steal sibling niches.** No Guard, no Ambush, no javelin pin, no floor quake, no blood, no Pile Driver, no character grapple energy rewrite.
14. **Self-contained v1.** Hard dep: MeleeRework hooks when that platform exists. Soft only: OmniMovement, Shock kits, Wrangler/Scrapper DNA literacy.
15. **On-hit / spatial / state verbs > flat % stickers.**

---

## 4. Locked Decisions

| Decision | Lock |
|----------|------|
| Display name | **Hooklash** |
| APIName | `hooklash` |
| Slot | Melee (MeleeRework kit) |
| Baseline damage type | **Normal / kinetic** |
| Innate element | **None** |
| Shock | **Opt-in Crack path cards only** |
| Ammo / magazine / reserve | **None** |
| Reload | **None** — R unused on baseline |
| M1 | **2-hit lash string** (discrete) |
| RMB | **Cast tether** (full equip only) |
| Tether resolve | Context: enemy → pull them; surface → pull you |
| Pendulum swing | **Exotic only** (Pendulum Protocol) |
| Path C crown | **Cat-o'-Nine** (multi-tip volley) |
| Exotic count | **5** — Pendulum Protocol, Cattle Call, Cat-o'-Nine, Live Coil, **Double Coil** |
| Economy | **Thin** — no Payroll-class engine |
| Guard / DR baseline | **None free** |
| Heavy feed | **Out of v1** |
| Throw / tether while gun out | **No** |
| Character ability replace | **Out** — does not patch Grapple Pole or Rocket Lasso |
| Product | New melee kit, not primary, not Fists patch |

---

## 5. Melee Roster Differentiation

| | **Fists** | **Stalker's Blade** | **Phalanx Impaler** | **Hooklash** | **Blood Carver** | **Saxonite Wrench** |
|--|-----------|---------------------|---------------------|--------------|------------------|---------------------|
| Slot (v1) | Melee | Melee | Melee | **Melee** | Primary (convert later) | Primary (convert later) |
| Cadence | Punch | Dual slash + knife | Thrust string + javelin | **2-hit lash + tether** | Hold-M1 saw ticks | Tap / charged slam |
| Stance / RMB | Guard (omni DR) | Throw knife | Buckler (frontal) | **Tether cast** | Blood spend | Gravity pull (AOE well) |
| R | Overload if card | Unused baseline | Throw javelin | Unused baseline* | Reload / Hold-R snare | Unused baseline |
| Resource | None | Mark / opener / optional Poison | Pin / combo / optional Aegis heat | Attach state / optional 2nd coil / optional Shock | Blood stacks | Torque (momentary) |
| Crowd tool | Size + multi-hit | Railblade light pierce | Long poke + finisher + throw pin | **Yank pack + crack arc + Cat-o'-Nine** | Pull + saw volume | Shockwave + pull |
| Fantasy | Brawler economy / tempo | Assassin approach → execute | Hoplite hybrid line-hold | **Tether acrobat / herder** | Harvest → spend → heavy | Gravity clear / freeze-shatter |
| Sustain | Guard DR | Soft Vanish only if built | Frontal plate; Aegis if built | Spacing + detach burst; no free DR | Blood DR / chunks | Minimal / cryo ward |
| Reach | Mid melee | Shortest | Longest melee | **Mid lash + long cast** | Short saw | Short–mid slam |
| Mobility | Pile Driver card | Slide Railblade | Lunge | **Surface reel + Pendulum** | Blood jump card | Sledge Jump |

\* Double Coil is a **charge exotic**, not an R-bind exotic.

---

## 6. Shared Framework (events upgrades speak)

Emit through MeleeRework-compatible host (`HooklashBehaviour` / kit router):

| Event / state | Fired when | Used for |
|---------------|------------|----------|
| **OnMeleeHit** | Quick or full lash damages enemy | Stacks, shock apply, thin economy |
| **OnMeleeKill** | Lash/tether-kill | CD refund, tempo, thin windows |
| **OnLash** | M1 crack connects (not tether tip alone) | Crack spine |
| **OnComboStep** | Hit index 1 / 2 in string | String cards, finisher gates |
| **OnComboFinisher** | Hit 2 connects | Cat-o'-Nine gates, finisher payoffs |
| **OnTetherCast** | RMB cast leaves hand | Ammo Continuity cousins, Double Coil spend |
| **OnTetherHit** | Tip connects enemy or surface | Resolve branch |
| **OnEnemyAttach** | Tip latched to enemy | Reel path |
| **OnSurfaceAttach** | Tip latched to world | Skyhook path |
| **OnReelStart / End** | Reel motion begins / completes or breaks | Amp windows, shock dump |
| **OnEnemyReeled** | Enemy pull completes / arrives | Pin, execute, Cattle Call |
| **OnSelfReeled** | Player arrives at surface / mid-reel detach | Aerial amp, detach burst |
| **OnPendulumSwing** | While swinging on surface hook | Wrecking Ball damage, swing cards |
| **OnPendulumRelease** | Player releases swing launch | Launch force cards |
| **OnTetherBreak** | Miss timer, range snap, cancel | Recovery rules |
| **CoilCharges** | Double Coil charge count | Cast gating |
| **ShockCordActive** | Opt-in shock cards armed | Element apply rules |

Gun windows from Hooklash cards buff *stowed/equipped primary* briefly — thin and hybrid-flavored, not Fists Payroll.

---

## 7. Core Mechanics & Gunfeel

### 7.1 Baseline kit

| Trait | Draft / intent |
|-------|----------------|
| Fire mode | Discrete 2-hit lash string — **not** continuous saw |
| M1 | Hit 1 fast arc → Hit 2 heavier/wider finisher |
| RMB | Cast tether (context resolve) |
| R | **Unused** baseline |
| Ammo | **None** |
| Element | Normal |
| Lash reach | Mid melee (draft between Blade and Impaler; playtest) |
| Cast range | Mid–long tether (noticeably beyond lash reach) |
| Size | Arc volume on lashes; finisher wider; anti-pack without Carver AOE |
| Falloff | **None** inside legal lash volume; tether is binary hit/miss |
| Damage | Solid arc clear; single-target execute needs reel setup or Cat-o'-Nine |
| Movement | Full move while equipped; reel overrides briefly during self-pull |
| Model / audio | Industrial coil-whip / cable lash; *crack* on finisher; spool whine on cast; *thunk* attach; reel winch; swing whoosh. Borrow closest melee until custom art |

### 7.2 Why no ammo / reload

Same lesson as Saxonite Wrench / Stalker's Blade / Phalanx Impaler: melee fantasy must not
soft-lock mid-line because a cell ran dry. Cadence pressure = whiff recovery, tether CD/break,
and swing commitment. **R stays free** (no fake reload bind).

### 7.3 Input contract (MeleeRework)

| State | Tap V | Hold V (≥ ~0.25s) | M1 | RMB | R |
|-------|-------|-------------------|----|-----|---|
| Gun out | **Quick lash** (short crack); stay on gun | **Equip Hooklash** | Gun | Gun ADS/alt | Gun reload |
| Whip equipped | Quick lash in-place | Refresh / flourish | **2-hit string** | **Cast tether** | Unused baseline |
| Tether in flight | — | — | Can buffer lash | Cast blocked until resolve/break | — |
| Enemy attached / reeling | — | — | Lash during reel if allowed (prefer: yes, reduced) | Recast blocked until break | — |
| Surface attached / reeling or swinging | — | — | Aerial / swing lash if cards/baseline allow | Recast blocked | — |
| Pendulum active (exotic) | — | — | Swing-lash / Wrecking hits | Release may launch (see §10) | — |

### 7.4 Quick melee (tap V)

- Does **not** long-term change weapon slot (MeleeRework quick profile).
- Uses equipped kit's quick lash (Hooklash if selected) — short combat crack.
- **No tether on quick-V** (gun still out; ADS/reload ownership stays clean).
- Useful gun-out finisher / panic arc without hold-V commit.

### 7.5 Full equip (hold V)

- Stow previous gun; coil in hand; full M1/RMB map.
- HUD: optional combo pip (1–2), tether state, coil charges if Double Coil, shock glyph if armed.
- Exit: normal weapon swap / shoot primary bind per MeleeRework.

### 7.6 Two-hit lash string (baseline)

| Step | Draft | Intent |
|------|-------|--------|
| Hit 1 | Fast horizontal/diagonal crack; modest damage; establishes string | Opener |
| Hit 2 **Finisher** | Heavier crack; wider arc; higher damage; longer recovery | Readable payoff |
| Whiff | Soft buffer one whiff then reset (prefer) | Forgiveness without air-swing forever |
| Interrupt | Heavy hit / tether cast / stow resets string | Clarity |
| Timing | Distinct cadence — not mash-as-saw | Skill toy empty-grid |

**Sacred cow:** 2-hit string exists with zero upgrades. Cards improve tempo, damage, arc width,
or gate Cat-o'-Nine on finisher — they do not invent combos from nothing.

### 7.7 RMB — Context Tether (baseline)

**Only while fully equipped.**

| Piece | Draft |
|-------|--------|
| Input | Tap RMB (Aim) while fully equipped |
| Projectile | Fast tip cast (hitscan-feel or very fast projectile — prefer readable travel) |
| Range | Mid–long (draft noticeably > lash reach; playtest) |
| Resolve order | First valid **enemy** in tip volume, else first valid **surface**, else miss |
| Enemy attach | Begin **enemy reel** toward wielder |
| Surface attach | Begin **self reel** toward attach point |
| Damage on tip | Modest tip tick on enemy attach (optional tiny; prefer mild so reel is the verb) |
| Miss | Soft recovery + miss timer (~0.4–0.6s) before recast |
| Break | Max reel time, range snap, manual cancel (stow / jump policy — playtest), target death |
| CD | Soft gated by cast recovery + reel duration more than a long cooldown; baseline recast ~0.35–0.55s after break |
| Gun-out | **No cast** — preserves gun ADS |
| Charges | 1 baseline; **Double Coil** adds a second charge |

**Intent:** Always teaches bidirectional cable. Weak enough that path crowns still matter.

### 7.8 Enemy reel (baseline)

| Piece | Draft |
|-------|--------|
| Targets | Grunts: strong pull. Elites: reduced. Bosses: soft tug and/or brief slow only — **no** boss yoink into void |
| Duration | Short reel to arrive or timeout |
| On arrive | Brief **soft pin / stagger** (draft 0.25–0.45s grunts); optional mild post-reel lash amp (Ole One-Two DNA, tiny empty-grid) |
| Multi | **Single target** baseline — Cattle Call owns pack yank |
| Safety | Ally pull **off** or tiny; pit/hazard clamps on knock vectors |
| During reel | Wielder can move; lash during reel allowed at reduced effectiveness (playtest) |

### 7.9 Surface reel (baseline)

| Piece | Draft |
|-------|--------|
| Effect | Pull **player** toward attach point along cable |
| Control | Mild air steer while reeling (cards improve) |
| Arrive | Short momentum carry; no free infinite swing on baseline |
| Detach | On arrive, jump cancel, or max time — **Impulse Vault**-class burst is upgrade-owned peak; tiny baseline carry OK |
| Pendulum | **Not baseline** — Pendulum Protocol exotic enables sustain swing |
| Combat | Aerial lash after self-reel is legal; amp is card-owned peak |

### 7.10 Soft post-reel amp (baseline, mild)

| Piece | Draft |
|-------|--------|
| Trigger | Successful enemy reel complete **or** self-reel arrive |
| Effect | Next lash (or short window ~1.0–1.5s) gains mild damage/size (draft ~1.1–1.15×) |
| Intent | Empty-grid teaches reel → crack sequencing (Ole One-Two literacy) |
| Path cards | Raise window quality; Live Coil crowns it |

### 7.11 Base combat loop (no upgrades)

```
Gun fight → tap V short crack finishers / panic arc
OR hold V → whip up
   → M1 lash string (crack → finisher)
   → RMB tether enemy straggler → yank in → finisher
   → RMB tether ledge/wall → reel self in → aerial crack
   → stow when gun is better
```

Skill without upgrades: aim the tip, choose enemy vs surface, finisher timing, not face-tanking
with a cable and no Guard, spending reel amp on the right target.

### 7.12 What baseline does NOT include

- Free Guard / Ambush / buckler
- Free Pendulum sustain swing
- Free multi-enemy Cattle Call
- Free Cat-o'-Nine multi-tip
- Free Double Coil second charge
- Free Handspring / Pile Driver
- Free ammo / grenade Payroll
- Free Shock tips
- Tether while gun out
- AOE gravity well (Wrench)
- Character Grapple energy / Lasso charges rewrite

---

## 8. Upgrade Paths (gravity wells — hybrids intended)

### Path A — SKYHOOK (Player mobility / surface cable)
**"The architecture is a handhold."**

- Spine: cast range to surfaces, self-reel speed, air control, detach burst, aerial lash amp, swing supports
- **Crown: Pendulum Protocol** — sustain swing on surface hook; swing-through damage (Wrecking Ball); release launch
- Hybrid: swing into Cattle Call pack; aerial Cat-o'-Nine; Double Coil second skyhook mid-air

### Path B — REEL (Enemy displacement / herd)
**"If it has mass, it has a meeting with you."**

- Spine: enemy pull force, tip seek, post-pull pin quality, chain/multi yank supports, pull-then-lash execute, thin expose
- **Crown: Cattle Call** — cone/chain reel multiple grunts; elite single hard yank + short gun expose window
- Hybrid: reel into finisher Cat-o'-Nine; skyhook dive then yank; Live Coil quality on both hooks

### Path C — CRACK (Combat sweep / tip authority)
**"The tip is the argument."**

- Spine: lash damage, arc width, string recovery, finisher mult, tip precision vs cores, **opt-in Shock**
- **Crown: Cat-o'-Nine** — finisher (or empowered cast) becomes multi-tip volley crack
- Hybrid: shock dump on reel (Cord Surge); multi-tip after Cattle Call clump; aerial multi-tip off Pendulum

### Glue exotic (peer)

**Live Coil** — dual-tool crown. Surface *and* enemy hooks stronger; successful reel empowers next lash hard; lash kill shortens tether recovery.

### Charge exotic (peer)

**Double Coil** — second tether charge (Double Lasso DNA). Enables double-yank, skyhook then enemy hook, or panic recast without waiting full recovery.

### Path × verb matrix

```
                    SKYHOOK                REEL                   CRACK
Surface tether      core fantasy           setup dive             aerial crack amp
Enemy tether        gap to pack            core fantasy           reel → finisher
Lash string         swing-lash             pull-then-crack        core fantasy
Pendulum            core fantasy           swing into herd        aerial Cat-o'-Nine
Cattle Call         yank then launch out   core fantasy           clump → multi-tip
Cat-o'-Nine         aerial volley          volley the pile        core fantasy
Double Coil         second skyhook         double yank            cast → crack tempo
Shock (opt-in)      —                      Cord Surge on reel     Live Tip spine
```

---

## 9. Crowns & Signature Cards

### Pendulum Protocol — Exotic (Skyhook crown)
- After **surface attach**, instead of auto-arrive-only: may **sustain swing** on the cable (hold move/aim policy — document one primary rule in impl; **recommend: hold RMB after surface attach converts reel into pendulum**, release RMB or jump = launch)
- While swinging: passing through enemies deals **Wrecking Ball**-class swing damage (gated ICD per target)
- Release: launch impulse along swing tangent / aim blend (Rocket Jump cousin, swing-timed)
- Clamps: max swing time; energy/heat optional; no infinite skybox; geometry/pit safety
- Clear cable VFX + whoosh audio bed

### Cattle Call — Exotic (Reel crown)
- Enemy tether cast becomes **cone or chain reel**: primary target + N nearby grunts (draft +2–4)
- Elites/bosses: still single-target hard yank / soft boss rules — no multi-boss yoink
- On multi-reel complete: brief clump pin + optional mild shockwave-lite at arrival point (tiny; not Wrench Aftershock)
- Elites: short **gun expose** window for stowed/primary (thin; DMLR literacy, not Payroll)
- Recovery gated — not hallway infinite

### Cat-o'-Nine — Exotic (Crack crown)
- On **Combo Finisher** (hit 2), or alternate: empowered finisher hold window — **prefer finisher automatically becomes multi-tip when exotic equipped**
- Spawns additional tip cracks (draft 3–5 tips) in a fan/arc along aim
- Each tip: reduced individual damage vs single finisher, high total vs packs; light pierce 0–1 grunt per tip (tune)
- Bosses: tips collapse toward single hard multi-hit rather than full fan delete
- Distinct multi-crack audio; tiny self-hitch (~0.08–0.12s) on big volleys so it reads as a decision
- Shock cards: each tip may apply reduced Shock if Live Tip present

### Live Coil — Exotic (Glue crown)
- While whip equipped: +enemy reel force quality and +surface reel speed/control
- On any successful reel complete: strong next-lash amp window (upgrades baseline Ole One-Two)
- On lash kill: refund tether recovery / shave CD (and optionally +1 coil charge fragment toward Double Coil)
- Does not replace Pendulum / Cattle Call / Cat-o'-Nine / Double Coil; feeds all

### Double Coil — Exotic (Charge crown)
- Gain a **second tether charge** (max 2)
- Charges regenerate on break/complete with staggered CD (not both instant forever)
- Enables: surface hook → enemy yank combos; double Cattle Call setups; panic recast
- UI: two coil pips
- With Pendulum: may cast second hook only when not conflicting with swing state (prefer: second charge usable after release or as re-hook — document in impl; **recommend: charges spent on cast, swing doesn't consume extra**)

### Cord Surge — Epic (Shock support)
- On successful reel complete: dump Shock at attach point and/or along cable targets
- Primary Shock payoff without forcing Cat-o'-Nine
- Soft duo with wet/shock loadouts

### Live Tip — Rare or Epic (Shock enabler)
- Lashes apply Shock (amount draft meaningful but modest)
- Without this or Cord Surge, Hooklash stays kinetic
- Cat-o'-Nine tips apply reduced per-tip Shock when both present

### Gunhand Coil — Epic (thin economy / swap)
- On reel complete (enemy or self): brief damage window for **stowed/primary gun** + optional tiny mag crumb
- Fisticuffs / Gunhand cousin — **not** Payroll multi-resource

### Impulse Snap — Epic (Skyhook support)
- On self-reel detach / arrive: burst of speed (Impulse Vault DNA on whip grid)
- Feeds gap-close into Crack/Reel

### Ole One-Two Coil — Epic (glue support)
- Deepens baseline post-reel lash amp (duration + mult)
- Lives under Live Coil crown naturally

### Boundary Incursion — Oddity
- Grid grow — universal keep

---

## 10. Pendulum Protocol — Deep Dive

### 10.1 Fantasy
You do not only zip to a point. You **become a wrecking ball on a SAXON-issue cable.**

### 10.2 Behaviour sketch

```
On surface attach AND Pendulum Protocol equipped:
  1. If player holds RMB (or auto-enter swing — prefer hold-to-swing):
       enter Pendulum state; do not hard-arrive immediately
  2. While Pendulum:
       - Player arcs around attach point (physics-lite or scripted pendulum)
       - OnEnemyOverlap during swing → swing damage (ICD per brain)
       - Heat/time builds toward max swing duration
  3. On release RMB / jump / max time:
       - Apply launch impulse
       - Break tether
       - Grant brief aerial lash amp window
  4. If player never holds for swing: baseline self-reel arrive still works
```

### 10.3 Risk budget

| Failure | Mitigation |
|---------|------------|
| Infinite swing immortal | Max duration; no free DR; ICD swing damage |
| Skybox escape | Launch clamps; optional ground-reset |
| Pit yeet | Hazard dampen near lethal drops |
| Pendulum + Double Coil chaos | Second cast rules while swinging limited |
| Feels like Scrapper Grapple | No pole place, no energy share tree, kit exotic not ability |

### 10.4 Contrast locks

| | **Scrapper Grapple Pole** | **Wrangler Rocket Lasso** | **Pendulum Protocol** |
|--|---------------------------|---------------------------|------------------------|
| Source | Character ability | Character ability | Melee kit exotic |
| Place node | Yes | No (throw lasso) | No — attach point only |
| Swing damage | Wrecking Ball tree | No | Yes on this exotic |
| Enemy pull | No (mostly) | Yes (lasso) | Separate Reel path |
| Energy | Pole energy | Lasso charges | Tether CD / swing max time |

---

## 11. Cattle Call — Deep Dive

### 11.1 Fantasy
One cast. The whole fireteam of grunts has a meeting. At your feet.

### 11.2 Behaviour sketch

```
On enemy tether cast with Cattle Call:
  1. Resolve primary aim target (enemy)
  2. Find up to N additional grunts in cone/radius around primary or along cast
  3. Attach soft tethers (VFX) and reel all toward wielder gather point
  4. Elites: only primary if elite; or primary elite + grunt adds — prefer grunt-only adds
  5. On gather: short pin; optional tiny arrival pulse
  6. If primary was elite: grant thin gun expose window
```

### 11.3 Risk budget

| Failure | Mitigation |
|---------|------------|
| Full-screen delete | Grunt caps; boss excluded from multi |
| Ally grief | No ally yank |
| Physics explosion | Gather point in front of player, not under mesh |
| Cattle + Cat-o'-Nine map wipe | Finisher recovery; tip damage split |

---

## 12. Cat-o'-Nine — Deep Dive

### 12.1 Fantasy
HR issued one coil. You filed a request for nine. Accounting said "close enough."

### 12.2 Behaviour sketch

```
On Combo Finisher with Cat-o'-Nine:
  1. Suppress single finisher volume (or layer)
  2. Spawn K tip cracks in fan along aim (horizontal bias)
  3. Each tip: own small arc cast / hitscan segment
  4. Damage = split formula so pack > single finisher total slightly, ST slightly less than pure finisher card stack
  5. Apply Live Tip shock at reduced per-tip amount if present
```

### 12.3 Risk budget

| Failure | Mitigation |
|---------|------------|
| Boss multi-tip melt | Collapse tips / boss mult |
| Visual noise | Clear fan VFX; cap K |
| Replaces all skill | Still requires hit-2 timing; whiff punishes |

---

## 13. Double Coil — Deep Dive

### 13.1 Fantasy
Two hooks. One argument. No waiting for the winch like a junior employee.

### 13.2 Rules

| Piece | Rule |
|-------|------|
| Max charges | **2** |
| Spend | 1 per RMB cast |
| Regen | Per-charge CD after spend/break (staggered); lash kills may shave via Live Coil |
| Baseline without exotic | Max 1 |
| Pendulum | Swing does not spend extra charge; re-hook spends |
| Cattle Call | Each cast spends 1 even if multi-target |
| UI | Two pips; readable empty/full |

### 13.3 What Double Coil is NOT
- Not infinite cast spam (regen gates)
- Not a third movement ability charge pool shared with dash
- Not Dynamite Cord (backlog)

---

## 14. Shock Policy (opt-in)

| Rule | Detail |
|------|--------|
| Baseline | No Shock |
| Enablers | **Live Tip** (lash apply), **Cord Surge** (reel dump), optional Cat-o'-Nine reduced per-tip |
| System | Real game Shock / electrocute EffectType (confirm id at impl) |
| Hard dependency | **None** |
| Soft duo | Shock grenades, wet setups, Powderwake-style move cards on other kits |
| Not trying to be | Full Storm Tide (Boarding Trident), Live Wire aura carry, baseline shock whip |

---

## 15. Full Upgrade List (~30 ship + backlog)

Rarity: Standard / Rare / Epic / Exotic / Oddity  
Tags: S Skyhook · R Reel · C Crack · G Glue / gunfeel · K Coil charges · Z Shock  
Cell rule: Exotics larger; all Exotics same cell count.  
Names are player-facing.  
Numbers are **v0 starting targets** — validate in playtest.

Suggested IDs at impl (verify collisions): gear **93100**; upgrades **93101–93130**.

------------------------------------------------------------------------------
EXOTICS (5) — equal large shapes
------------------------------------------------------------------------------

X1. Pendulum Protocol — Exotic (S) — Skyhook crown
    Sustain swing on surface hook; Wrecking Ball swing damage; release launch.

X2. Cattle Call — Exotic (R) — Reel crown
    Multi-grunt cone/chain reel; elite hard yank + thin gun expose.

X3. Cat-o'-Nine — Exotic (C) — Crack crown
    Finisher becomes multi-tip crack volley; pack clear peak.

X4. Live Coil — Exotic (G) — Dual-tool glue
    Both hook types quality; reel → strong lash amp; kill shaves tether CD.

X5. Double Coil — Exotic (K) — Charge crown
    Second tether charge; staggered regen; combo cast fantasy.

------------------------------------------------------------------------------
EPICS (7)
------------------------------------------------------------------------------

E1. Impulse Snap — Epic (S)
    Self-reel detach/arrive grants speed burst (Impulse Vault DNA).

E2. Aerial Authority — Epic (S)
    After self-reel or during Pendulum: +lash damage/size briefly.

E3. Chain Yank — Epic (R)
    Enemy reel can grab +1 extra grunt (weaker than Cattle Call; stacks rules: Cattle Call owns multi fantasy — Chain Yank either feeds Call cap or is partial multi without exotic).
    **Impl prefer:** without Cattle Call, +1 grunt; with Cattle Call, +1 to cap.

E4. Tip Seek — Epic (R)
    Tether tip magnetizes toward nearby grunts (Electromagnet DNA lite).

E5. Ole One-Two Coil — Epic (G)
    Stronger/longer baseline post-reel lash amp window.

E6. Gunhand Coil — Epic (G)
    Reel complete → brief stowed/primary gun damage window (+ optional tiny mag crumb).

E7. Cord Surge — Epic (Z)
    Reel complete dumps Shock at attach / along reeled targets.

------------------------------------------------------------------------------
RARES (10)
------------------------------------------------------------------------------

R1. Wireless Coil — Rare (S/G)
    +Tether cast range.

R2. Winch Speed — Rare (S/R)
    +Reel speed (self and enemy).

R3. Spare Cord — Rare (S)
    +Max reel/swing duration; milder break forgiveness.

R4. Yank Weight — Rare (R)
    +Enemy pull force; better elite tug (bosses still soft).

R5. Anchor Hitch — Rare (R)
    Post-enemy-reel pin/slow duration up (Anchor Bolt cousin).

R6. Wide Crack — Rare (C)
    +Lash arc width / size.

R7. Weighted Tip — Rare (C)
    +Lash and finisher damage.

R8. Snap Recovery — Rare (C/G)
    Shorter string recovery after hit or whiff.

R9. Live Tip — Rare (Z)
    Lashes apply Shock (primary Shock enabler).

R10. Quick Draw Coil — Rare (G)
    Faster quick-V startup / shorter gun-return gap.

------------------------------------------------------------------------------
STANDARDS (6)
------------------------------------------------------------------------------

S1. Honed Cord — Standard (G)
    +Lash and tether tip damage. Stackable mild.

S2. Longer Lash — Standard (C/G)
    +Melee lash reach. Still no falloff.

S3. Tight Spool — Standard (G)
    Slightly faster tether cast recovery.

S4. Soft Hitch — Standard (R)
    Mild +enemy pin quality on reel arrive.

S5. Coil Balance — Standard (G)
    Slightly faster equip (hold-V complete) and stow cleanliness.

S6. Crack Tempo — Standard (C)
    Mild +string speed (finisher comes online faster).

------------------------------------------------------------------------------
ODDITY (1)
------------------------------------------------------------------------------

O1. Boundary Incursion — Oddity (G)
    +Upgrade grid size.

------------------------------------------------------------------------------
FROZEN v1 SHIP POOL (exactly 30)
------------------------------------------------------------------------------

  EXOTIC (5)
    1  Pendulum Protocol
    2  Cattle Call
    3  Cat-o'-Nine
    4  Live Coil
    5  Double Coil

  EPIC (7)
    6  Impulse Snap
    7  Aerial Authority
    8  Chain Yank
    9  Tip Seek
    10 Ole One-Two Coil
    11 Gunhand Coil
    12 Cord Surge

  RARE (10)
    13 Wireless Coil
    14 Winch Speed
    15 Spare Cord
    16 Yank Weight
    17 Anchor Hitch
    18 Wide Crack
    19 Weighted Tip
    20 Snap Recovery
    21 Live Tip
    22 Quick Draw Coil

  STANDARD (6)
    23 Honed Cord
    24 Longer Lash
    25 Tight Spool
    26 Soft Hitch
    27 Coil Balance
    28 Crack Tempo

  ODDITY (1)
    29 Boundary Incursion

  FLEX (1) — slot 30:
    RECOMMENDED: **Finisher's Heel — Rare (C)**
      Bonus damage on Combo Finisher vs low-HP targets.

  RECONCILED FROZEN 30:

    EXOTIC (5)
      1  Pendulum Protocol
      2  Cattle Call
      3  Cat-o'-Nine
      4  Live Coil
      5  Double Coil

    EPIC (7)
      6  Impulse Snap
      7  Aerial Authority
      8  Chain Yank
      9  Tip Seek
      10 Ole One-Two Coil
      11 Gunhand Coil
      12 Cord Surge

    RARE (11)
      13 Wireless Coil
      14 Winch Speed
      15 Spare Cord
      16 Yank Weight
      17 Anchor Hitch
      18 Wide Crack
      19 Weighted Tip
      20 Snap Recovery
      21 Live Tip
      22 Quick Draw Coil
      23 Finisher's Heel

    STANDARD (6)
      24 Honed Cord
      25 Longer Lash
      26 Tight Spool
      27 Soft Hitch
      28 Coil Balance
      29 Crack Tempo

    ODDITY (1)
      30 Boundary Incursion

Count: 5X + 7E + 11R + 6S + 1O = **30**.

BACKLOG (designed, expand later)
  Dynamite Cord (Exotic) — tether/tip explodes (Dynamite on a String DNA)
  Chunk Hook — pull health chunks / Hometown Hero cousin
  Core Nick — core/anatomy lash bias
  Surface Priority / Enemy Priority toggles (cast bias cards)
  Rocket Jump Latch — surface hook directly ahead launches away (true Rocket Jump mirror)
  Beehive Cord — first melee while pendulum throws beehive (Scrapper joke; careful)
  Ally Hook — On The Go cousin: briefly tether ally as swing node (co-op; grief rules)
  Laying Cable cousin — damage share while enemy attached (Shocklance turbo literacy)
  Shock Fan — Cat-o'-Nine tips always shock without Live Tip (too strong? keep backlog)
  Soft Hands — damage on equip → brief free move DR
  Heavy Handshake cousin — melee kills drip heavy ammo (thin; Carver owns feast)
  Second Wind Coil — reel kills drip ability charge (thin)
  True three-hit lash string experiment (rejected for v1 — 2-hit lock)
  Gun-out tether (rejected for ADS clarity)
  Baseline Pendulum (rejected — exotic skill toy)
  AOE gravity well RMB (rejected — Wrench)

------------------------------------------------------------------------------
CUT / DEMOTE
------------------------------------------------------------------------------

| Idea | Fate |
|------|------|
| RMB = Guard | **Cut** — Fists/Impaler; RMB = tether |
| RMB = AOE gravity well | **Cut** — Wrench |
| Baseline Pendulum | **Cut** — Pendulum Protocol exotic |
| Baseline multi-yank | **Cut** — Cattle Call / Chain Yank |
| Baseline Shock | **Cut** — Live Tip / Cord Surge |
| Payroll-class ammo engine | **Cut** — Fists owns; Gunhand Coil thin only |
| Pile Driver / Handspring on whip | **Cut** — Wrangler / Fists |
| Slide Railblade | **Cut** — Blade |
| Javelin throw pin | **Cut** — Impaler |
| Crouch Ambush stealth | **Cut** — Blade |
| Blood meter / heavy feast | **Cut** — Carver |
| Continuous saw ticks | **Cut** — Carver |
| Replace Grapple Pole / Rocket Lasso | **Cut** — character abilities stay |
| Tether while gun out | **Cut** — ADS/reload clarity |
| Primary-slot whip | **Cut** — melee kit only v1 |

---

## 16. Example Builds

### Pure Skyhook acrobat
Wireless Coil → Winch Speed → Spare Cord → Impulse Snap → Aerial Authority → **Pendulum Protocol** → **Double Coil** → Quick Draw Coil  
*Play:* Hook walls, swing Wrecking Ball arcs, release launch, second charge re-hook, aerial cracks.

### Pure Reel herder
Yank Weight → Anchor Hitch → Tip Seek → Chain Yank → **Cattle Call** → Ole One-Two Coil → Gunhand Coil → Soft Hitch  
*Play:* Yank packs to feet, pin, finisher, gun window on elites.

### Pure Crack storm
Wide Crack → Weighted Tip → Snap Recovery → Crack Tempo → Live Tip → Cord Surge → **Cat-o'-Nine** → Finisher's Heel  
*Play:* String into multi-tip volleys; optional shock fan; reel only as amp setup.

### Poster hybrid (recommended fantasy complete)
Pendulum Protocol + Cattle Call + Cat-o'-Nine + Live Coil + Double Coil + Cord Surge + Gunhand Coil  
*Play:* Double-charge skyhook into pack, Cattle Call clump, Cat-o'-Nine delete, gun cleans elites, swing the leftovers.

### Shock cowboy
Live Tip → Cord Surge → Cat-o'-Nine → Cattle Call → Tip Seek → Weighted Tip → Ole One-Two Coil  
*Play:* Kinetic+shock hybrid; reel dumps shock; multi-tip spreads status; soft duo with shock grenades.

### Gun-out skirmisher (minimal full equip)
Quick Draw Coil → Longer Lash → Honed Cord → Gunhand Coil → Wireless Coil  
*Play:* Stay on primary; tap-V cracks; hold V only for tether setups and emergency yanks.

---

## 17. Strengths, Weaknesses & Failure Modes

### Strengths
- Unique arsenal fantasy (bidirectional tether acrobat/herder) with clear sibling contrast
- Movement tool that is also a weapon without becoming a character ability
- 2-hit string readable empty-grid skill
- Enemy yank and surface hook both teach from the same RMB
- Cat-o'-Nine delivers pack clear without saw/quake identity
- Double Coil enables expressive cast combos
- Opt-in Shock without forcing chemistry
- Thin gun-swap economy respects Fists Payroll
- CQC modifier and co-op herd-into-ally-AOE both supported

### Weaknesses
- No baseline Guard — wrong tool for face-tank piles
- Tether miss / break punishes panic spam
- Bosses resist full yoink (by design)
- Pure static aerial bosses may prefer guns if hooks miss
- Pendulum skill ceiling may frustrate if swing controls are muddy (tune)

### Failure modes to avoid in tuning

| Failure | Mitigation |
|---------|------------|
| Pendulum immortal / skybox | Max swing time; launch clamps; no free DR |
| Cattle Call full-room delete | Grunt caps; boss excluded from multi |
| Cat-o'-Nine boss melt | Tip collapse / boss mult; recovery |
| Double Coil perma-cast | Staggered charge regen |
| Gunhand Coil = Payroll | Tiny window/crumb only |
| Shock steals grenade identity | Modest apply amounts; opt-in only |
| Feels like Wrench | Aimed line tether, not AOE well; no floor quake primary loop |
| Feels like Impaler | No buckler, no javelin pin, 2-hit crack not 3-hit thrust |
| Feels like Blade | No crouch Ambush, no stealth Vanish, wider arc |
| Feels like Fists | No Guard, no Payroll, tether identity |
| Feels like Scrapper/Wrangler | Kit exotic/CD, not ability energy trees; no pole place |
| Ally/pit grief | Ally pull off; hazard clamps |
| Turtle meta empty-grid | No free DR; must crack and hook to pace |

---

## 18. Naming & Presentation

| Slot | Value |
|------|--------|
| Display name | **Hooklash** |
| Internal / API | `hooklash` |
| Design nicknames | Coil Lash, Issue Coil, Saxonite Lash (notes only) |
| Short description | *Issue coil-whip. No ammo, no reload — crack a two-hit lash, cast a tether that yanks enemies in or reels you to terrain. Bolt on pendulum swings, cattle-call herds, cat-o'-nine volleys, live-coil tempo, double-coil charges — and optional shock tips.* |
| Thunderstore name (later) | `Hooklash` |
| GUID (later) | `sparroh.hooklash` |
| Folder today | `.new.WhipMelee` |
| Suggested IDs | Gear **93100**; upgrades **93101–93130** (verify at impl) |
| Design doc file | `Hooklash-DesignDoc.txt` |

### SAXON marketing blurb (draft)

> SAXON Hooklash — For employees who believe the building should meet them halfway.  
> One coil. Zero cells. Zero "please reload while the fungus writes your eulogy."  
> Baseline: crack, crack harder. Hook the wall or hook the problem. Reel. Repeat.  
> Aftermarket: pendulum protocol, cattle call, cat-o'-nine hospitality, live coil, double coil.  
> Optional: live tips (shock). HR did not approve the nine-tip requisition. We shipped it anyway.  
> If your melee option comes with a magazine UI, a crouch tutorial, or a gravity-well disclaimer, you are holding the wrong product.  
> "If they wanted to stay over there, they should have brought their own anchor point."

### Flavor / in-world

Industrial cable-whip issued to boarding contractors, tower techs, and anyone who failed the
"bring a grappling license" seminar. Saxonite-braided cord survives repeated tip discharge.
Optional aftermarket capacitor tips sold as Live Tip upgrades and are definitely not a union
grievance with Shocklance tether teams.

---

## 19. Synergy Notes (Player-Facing, Soft Only)

| Partner | Why it feels good |
|---------|-------------------|
| **MeleeRework / Fists** | Slot platform; Fists for brawler days, Hooklash for mobility-herd days — swap kits |
| **Stalker's Blade** | Peer melee; assassin vs acrobat — no shared Ambush |
| **Phalanx Impaler** | Peer melee; line-hold vs tether — no shared javelin/buckler |
| **Saxonite Wrench** | Dual-melee meme; AOE well vs aimed cable — different pull verbs |
| **Blood Carver** | Dual-melee meme; saw vs lash — no shared blood |
| **Shock grenades / wet kits** | Cord Surge / Live Tip soft duo |
| **OmniMovement** | Strafe-reel and pendulum feel better |
| **Wrangler / Scrapper** | DNA literacy only; abilities still work beside kit — no package dep |
| **DMLR / precision guns** | Gunhand Coil + Cattle Call expose → primary dumps |
| **Ally AOE** | Cattle Call clumps into friend fire/acid/shock fields |

**Explicit non-goals v1:** hard deps; heavy ammo feed; blood meter; mag/reload; baseline Shock; baseline Pendulum; replacing Fists as default melee; patching Grapple/Lasso abilities.

---

## 20. Wiki / DNA Fate Table (borrow → rename)

| Source | Hooklash home | Notes |
|--------|---------------|-------|
| Rocket Lasso (ability) | Baseline tether literacy | Kit version; not ability replace |
| Yank em! | Yank Weight | |
| Loose Rope / Extra Rope | Wireless Coil / Winch Speed | |
| Quick Knot | Tight Spool | |
| Rocket Jump | Pendulum release / backlog Rocket Jump Latch | |
| Wrecking Ball | Pendulum Protocol swing damage | |
| Spare Cable / Tether Battery | Spare Cord | |
| Wireless Extenders | Wireless Coil | |
| Impulse Vault | Impulse Snap | |
| The Ole One-Two | Ole One-Two Coil + baseline mild amp | |
| Double Lasso | **Double Coil** | |
| Dynamite on a String | Dynamite Cord backlog | |
| Super Magnet | Cattle Call / Tip Seek lite | Not saw aura |
| Electromagnet | Tip Seek | |
| Ammo Continuity | Gunhand Coil thin crumb | |
| Handspring | **Not on whip** | Wrangler / Fists Pile Driver |
| Beehive! | Backlog joke only | |
| On The Go | Ally Hook backlog | |
| Laying Cable (turbo) | Backlog ally share | |
| Charge Thief / Hometown Hero | Chunk Hook backlog | |
| Boundary Incursion | Boundary Incursion | Keep name |

---

## 21. Success Criteria / Player Fantasy Checklist

- [ ] Hooklash appears as a selectable **melee kit** (MeleeRework slot)
- [ ] Tap V short lash useful with **zero** upgrades
- [ ] Hold V equips coil with clear feedback
- [ ] 2-hit string readable; finisher feels like a payoff empty-grid
- [ ] RMB tether yanks **enemies** toward player
- [ ] RMB tether on **terrain** reels **player** toward attach
- [ ] Miss/break recovery is fair, not soft-lock
- [ ] Mild post-reel lash amp teaches sequencing empty-grid
- [ ] Pendulum build: swing damage + release launch reads
- [ ] Cattle Call build: pack clumps without map-wipe forever
- [ ] Cat-o'-Nine build: finisher multi-tip deletes a line, not the whole mission
- [ ] Live Coil makes dual hooks feel mandatory-cool
- [ ] Double Coil second charge enables expressive combos without perma-spam
- [ ] Live Tip / Cord Surge Shock works without hard-requiring other packages
- [ ] Gunhand Coil does not replace reloading as a lifestyle
- [ ] No ammo UI / no reload beat on this kit
- [ ] Does not obsolete Fists Guard/Payroll, Blade Ambush, Impaler plate/javelin, Wrench/Carver, or character Grapple/Lasso
- [ ] Failure states stay fun (missed hook, boss resist, swing timeout)
- [ ] Sandbox MP: tether/reel/swing/lash readable enough on clients

---

## 22. Implementation Appendix (For Later — Not This Pass)

Design-only milestone: **this document**. When coding starts:

| Piece | Approach |
|-------|----------|
| Platform | **MeleeRework** `RegisterKit` / `IMeleeKit` quick + full profiles |
| Registration | Melee gear catalog entry; persistence by gear id; fallback Fists if missing |
| Host | `HooklashBehaviour` + `Data` struct for string/tether/pendulum/charges/shock flags |
| Base type | Evaluate `MeleeGear` / Throwable DNA vs custom hitcast + projectile tip after Assembly lookup — **do not invent APIs** |
| Combo | State machine hit index 1–2; timers; finisher volume swap |
| Tether | Fast projectile or hitscan tip; attach state replicated; reel forces via game knockback/pull patterns |
| Pendulum | Surface attach + hold policy; swing motion; overlap damage ICD; launch on release |
| Cattle Call | Multi-target select + parallel reel; grunt caps |
| Cat-o'-Nine | Finisher spawns multi tip volumes |
| Double Coil | Charge int 0–2; regen timers |
| Shock | Real EffectType on DamageData when cards say so |
| Upgrades | `PlayerData.CreateUpgrade` + `UpgradeProperty` Apply/Remove on FindGear(`hooklash`) |
| No ammo | Infinite / bypass; hide reload; R no-op |
| Mod flags | `[MycoMod(..., ModFlags.IsSandbox)]` |
| Deps | Soft recommend MeleeRework; document hook version |
| Model | Coil-whip mesh when AssetBundle exists; placeholder OK |

### Suggested `HooklashBehaviour.Data` fields (sketch)

```
// Core mults
float damageMult;
float reachMult;
float sizeMult;
float recoveryMult;
float quickDrawMult;
float equipSpeedMult;

// Combo (2-hit)
int comboIndex;              // runtime
float finisherDamageMult;
float finisherSizeMult;
float stringRecoveryMult;
bool finishersHeel;
float finisherLowHpThreshold;
float finisherLowHpMult;

// Tether baseline
float castRangeMult;
float castRecoveryMult;
float reelSpeedMult;
float enemyPullForceMult;
float selfReelSpeedMult;
float tipDamageMult;
float postReelAmpMult;       // baseline Ole One-Two
float postReelAmpDuration;
bool tetherInFlight;         // runtime
bool enemyAttached;          // runtime
bool surfaceAttached;        // runtime

// Skyhook
bool pendulumProtocol;
float pendulumMaxDuration;
float pendulumSwingDamage;
float pendulumSwingIcd;
float pendulumLaunchMult;
bool impulseSnap;
float impulseSnapSpeedMult;
bool aerialAuthority;
float aerialLashMult;
float aerialLashDuration;
bool spareCord;
float maxReelDurationMult;

// Reel
bool cattleCall;
int cattleCallExtraGrunts;
float cattleCallConeAngle;
float cattleCallExposeDuration;
float cattleCallExposeMult;
bool chainYank;
int chainYankExtra;
bool tipSeek;
float tipSeekRadius;
float tipSeekForce;
bool anchorHitch;
float pinDurationMult;
bool yankWeight;

// Crack
bool catONine;
int catONineTipCount;
float catONineTipDamageMult;
float wideCrackMult;
bool weightedTip;

// Glue / charges
bool liveCoil;
float liveCoilReelAmpMult;
float liveCoilKillCdRefund;
bool doubleCoil;
int maxCoilCharges;          // 1 or 2
int coilCharges;             // runtime
float coilRegenDuration;
bool oleOneTwoCoil;
bool gunhandCoil;
float gunhandDuration;
float gunhandDamageMult;
float gunhandAmmoCrumb;      // keep tiny or 0

// Shock opt-in
bool liveTip;
float liveTipShockAmount;
bool cordSurge;
float cordSurgeShockAmount;
```

### Ship cut vs stretch

**v1 must-ship (fantasy complete):**
- Melee kit registration + tap/hold V profiles
- 2-hit lash + quick-V lash + no ammo
- Context tether (enemy pull / surface self-reel) + mild post-reel amp
- All **5** exotics (Pendulum, Cattle Call, Cat-o'-Nine, Live Coil, Double Coil)
- Shock spine (Live Tip + Cord Surge)
- Thin Gunhand Coil economy
- Frozen 30 registration

**Stretch / post-v1:**
- Dynamite Cord, Chunk Hook, Ally Hook, Rocket Jump Latch
- Custom meshes / Wwise spool
- Config knobs for cast range / pull force / swing duration
- Convert-era notes if other kits move slots

### Phased delivery (when coding)

| Phase | Deliverable |
|-------|-------------|
| H0 | Register kit + quick/full lash + no ammo |
| H1 | 2-hit string + finisher volume |
| H2 | Tether cast + enemy reel + surface self-reel |
| H3 | Baseline post-reel amp + juice |
| H4 | Double Coil charges |
| H5 | Cattle Call + Chain Yank + Tip Seek |
| H6 | Pendulum Protocol swing + launch |
| H7 | Cat-o'-Nine multi-tip |
| H8 | Live Coil + Gunhand + Ole One-Two deep |
| H9 | Live Tip + Cord Surge Shock |
| H10 | Frozen 30 + balance + README |

---

## 23. Open Tuning Questions (playtest, not design blockers)

1. Lash reach vs Blade / Fists / Impaler band  
2. Cast range sweet spot (too sniper vs too stubby)  
3. Enemy reel force on elites; boss = slow-only vs micro-tug  
4. Self-reel air control strength  
5. Pendulum enter: hold-RMB vs auto-swing on surface  
6. Pendulum max duration 1.5s vs 3s  
7. Cattle Call extra grunts 2 vs 4  
8. Cat-o'-Nine tip count 3 vs 5; damage split formula  
9. Double Coil regen time per charge  
10. Live Tip shock amount vs grenade pace  
11. Whether lash during enemy reel is full or reduced  
12. Exact hex shapes for 30 upgrades — author during implementation  
13. Post-reel baseline amp 1.1× vs 1.15×  

---

## 24. Relationship to Sibling Projects

| Project | Relationship |
|---------|----------------|
| **MeleeRework** | **Hard platform** when shipped — slot, tap/hold V, kit API |
| **Fists** | Peer melee kit; different verbs; do not steal Guard/Payroll/Haymaker/Pile Driver |
| **Stalker's Blade** | Peer melee; no Ambush/stealth/slide Railblade steal |
| **Phalanx Impaler** | Peer melee; no buckler/javelin/Aegis steal; throw family is cousin only (tether ≠ pin javelin) |
| **Blood Carver** | Primary saw until convert; no shared blood; magnet contrast only |
| **Saxonite Wrench** | Primary slam until convert; **pull contrast lock** — AOE well vs aimed cable |
| **Boarding Trident** | Different slot; Storm shock literacy soft only |
| **OmniMovement** | Soft UX synergy |
| **SparrohsTurbocharges** | Optional later; Double Lasso / Super Magnet DNA already baked as renames |
| **Weapon template in this folder** | Scaffold only; product is melee kit + design doc first |

---

## 25. Universal Truths (Mycopunk alignment)

- Exotic shapes should always be larger than others; each exotic the same cell count.
- v1 targets **~30** upgrades (frozen list above); backlog is real design, not trash.
- Three paths create different builds but **may intermingle** on the grid.
- Prefer verbs: lash string, finisher, tether cast, enemy reel, surface reel, pendulum swing, cattle call, cat-o'-nine, double coil, optional shock.
- No second blood/torque meter.
- No ammo/reload on this kit.
- Hybrids intended; no anti-synergy matrix.

---

## 26. Design Checklist

- [x] Name: **Hooklash**  
- [x] Melee kit via MeleeRework (not primary)  
- [x] RMB = **context tether** (enemy→them, surface→you)  
- [x] M1 = **2-hit lash string**  
- [x] Path C crown: **Cat-o'-Nine**  
- [x] Pendulum = **exotic only**  
- [x] Exotic count **5** including **Double Coil**  
- [x] Shock **opt-in** (Live Tip / Cord Surge)  
- [x] Thin economy (Gunhand Coil; no Payroll)  
- [x] No ammo / no reload  
- [x] Paths: Skyhook / Reel / Crack + Live Coil + Double Coil  
- [x] Frozen 30 table  
- [x] Differentiated from Fists / Blade / Impaler / Carver / Wrench / Grapple / Lasso  
- [x] Wiki DNA borrowed with renames  
- [x] Failure modes documented  
- [x] Implementation deferred  

---

## 27. Design Changelog

### v1 (this doc) — 2026-08-07

- Product: **Hooklash** melee kit for MeleeRework hooks
- User locks: name Hooklash; context tether RMB; 2-hit combo; Cat-o'-Nine crown; Pendulum exotic; opt-in shock; **5 exotics with Double Coil**
- Paths: Skyhook / Reel / Crack + Live Coil glue + Double Coil charge
- Baseline: 2-hit lash, bidirectional tether, mild post-reel amp, no ammo
- Frozen 30 + backlog + sibling contrast + wiki fate table
- Rejected: gun-out tether, baseline pendulum, AOE well RMB, Payroll, Guard, ability replace

---

## 28. Next Steps After This Doc

1. Confirm MeleeRework hook surface when that project implements RegisterKit  
2. Implement H0–H3 (kit + string + tether + amp) against decompile  
3. Layer Double Coil / Cattle Call / Pendulum / Cat-o'-Nine / Live Coil  
4. Register frozen 30  
5. Playtest cast range, pull force, pendulum controls, tip count, charge regen, shock amounts  
6. Art/audio pass when ready  

---

*End of Hooklash Design Doc v1*
