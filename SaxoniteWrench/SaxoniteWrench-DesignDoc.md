# Saxonite Wrench — Design Document (v1.1)

> Status: **Phase 0–1 implementation** (registration + baseline combat). Upgrades later.
> Working titles in notes: Grav Hammer / Impact Wrench / Grav-Maul.
> **Ship name: Saxonite Wrench.**
> Folder: `.new.SaxoniteWrench`.
> Product shape: **GearType.Melee kit** — clones vanilla MeleeGear / Fists; equips via MeleeRework melee slot (`player.Gear[4]`). Not a primary weapon. Does not patch a disabled vanilla wrench prefab in-place.
> Soft dependency: **MeleeRework** (`sparroh.meleerework`) for slot UI + kit registry.
> Fantasy anchors: Halo Gravity Hammer (shockwave, pull, hammer-jump) + SAXON industrial tone + optional Cryo/shatter path (experimental Cryo + Rime Charge research).
> Sibling docs: MeleeRework (platform + Fists), Blood Carver (saw contrast), Rime Charge (cryo systems), Heaven Piercer (charge-release DNA reference).


---

## 1. High Concept / Fantasy

**Saxonite Wrench** is SAXON’s favorite argument-ender: a two-handed gravitic impact wrench with an impeller in the head.

You do not hose. You do not saw. You **wind torque, crack the floor, and yank the room into the strike.** Baseline is honest kinetic melee — tap smash or charged slam, weak gravity tug on RMB, **no ammo, no reload.** The upgrade grid forks the tool into shockwave clear, gravity well control, or a cold head that freezes the pile so the next swing shatters it.

**One-liner:** *Wind the core. Crack the floor. Pull them into the head — and if you bolted on the cold kit, shatter whatever freezes.*

**Element baseline:** Normal (kinetic). Cryo is **Rimehead path only** — opt-in on the grid.

---

## 2. Role & Fantasy in the Arsenal

| | |
|--|--|
| **Slot** | **Melee** (`GearType.Melee`) — MeleeRework loadout slot |
| **Range** | Melee reach + shockwave ring (the wave *is* the range) |
| **Role** | Discrete gravity-slam melee kit — pack clear via waves, setup via pull, execute via charged drops / optional freeze-shatter |
| **Gap filled** | Fists = jab/guard/economy. Blood Carver = continuous saw primary. **Nothing owns slam → shockwave → pull → optional freeze-shatter** on the melee slot with zero ammo anxiety. |
| **Synergies** | MeleeRework platform (required soft); free primary slot for a gun; Rime Charge / Cryo (soft); Sledge Jump; co-op pull into ally AOE |
| **Not trying to be** | Fists 2, Blood Carver 2, a primary gun, ammo weapon, pure cryo gun, heavy-weapon battery |

**Product shape:** New **melee kit** (**Saxonite Wrench**). Primary slot stays free. Does **not** replace Fists default or Carver.


---

## 3. Design Pillars

1. **No ammo. No reload.** The weapon just functions. Cadence is swing recovery, charge time, and RMB well cooldown — never magazines or reserve.
2. **Discrete impacts, not a continuous saw.** Tap and charged slam are the verbs. Carver owns hold-M1 tick DPS.
3. **Gravity is baseline identity; cryo is opt-in.** Weak RMB pull always teaches the Halo fantasy. Rimehead cards own freeze/shatter.
4. **Charge skill is real.** Hold-to-charge slam scales power; a near-full **Perfect Torque** sweet band rewards timing (Heaven Piercer / Half Cocked DNA). Holding forever is slightly worse than the band.
5. **Shockwave is the signature.** Every legal impact can emit a kinetic ring. Impetus path crowns the wave.
6. **Three peer gravity wells + mix-and-match.** Impetus / Wellspring / Rimehead. Hybrids intended; no anti-synergy matrix.
7. **Sledge Jump is the mobility exotic** — Halo hammer-jump DNA, not free baseline rocket boots.
8. **No heavy-weapon link in v1.** Carver owns butcher’s feast. Do not steal that niche.
9. **~30 upgrades;** exotic shapes larger than others; each exotic the **same** cell count.
10. **SAXON industrial + gravity + cryo spice** — catalog comedy, impeller tech, cold-kit aftermarket. Not Covenant cosplay names on the box.
11. **On-hit and spatial verbs > flat % stickers.** Pull, wave, shatter windows, aerial slams.
12. **Melee kit on MeleeRework platform.** Soft dep on MeleeRework; soft tips to Rime Charge; no hard content deps.

---

## 4. Locked Decisions

| Decision | Lock |
|----------|------|
| Display name | **Saxonite Wrench** |
| APIName | `saxonite_wrench` |
| Gear ID | **92800** |
| GUID | `sparroh.saxonitewrench` |
| **Slot** | **`GearType.Melee`** (not Primary) |
| Clone base | Vanilla **MeleeGear / Fists** |
| Platform | **MeleeRework** soft dep (slot UI + `MeleeKitRegistry`) |
| Baseline damage type | **Normal / kinetic** |
| Innate element | **None** |
| Cryo | **Rimehead path only** |
| Ammo / magazine / reserve | **None** |
| Reload | **None** — R does nothing on baseline |
| Tap V (MeleeRework) | Quick tap smash (gun out) |
| Hold V | Equip wrench full profile |
| M1 (equipped) | Tap smash + optional hold-charge slam |
| RMB (equipped) | Baseline weak gravity **pull** (not Fists Guard) |
| Sledge Jump | **Exotic** (4th exotic, glue/mobility) |
| Heavy weapon feed | **Out of v1** |
| Tone | SAXON, gravity, industrial, cryo path |
| Fourth exotic | **Sledge Jump** (not Deflection Mass as crown) |
| Product | New melee kit, not in-place vanilla patch / not primary |


---

## 5. Core Mechanics & Gunfeel

### 5.1 Base weapon (no upgrades)

| Trait | Draft / intent |
|-------|----------------|
| Fire mode | Discrete melee swings (not continuous box-saw) |
| M1 tap | Quick smash — modest damage, **small** shockwave (~2–3m), light knockback |
| M1 hold → release | Charged slam — damage, shockwave radius, and knockback scale with charge |
| Sweet spot (baseline) | Near-full charge band (“Perfect Torque”) grants crit-class mult; full hold at 1.0 = full damage **without** crit (or tiny sway) so forever-hold is suboptimal |
| RMB | Short gravity **pull** toward aim point / wielder — weak strength, short cooldown, limited target count |
| R | **Unused** on baseline (no reload). No hold-R exotic in v1 |
| Ammo | **None** — infinite function |
| Element | Normal |
| Reach | Short–mid melee legal volume (draft ~4–6m head reach; playtest) |
| Shockwave | On every successful impact (tap or charged); radius/damage scale with charge |
| Falloff | **None** inside legal impact + wave volume (Carver lesson: limitation is reach, not soft tax) |
| Movement | Mild move-speed penalty while at high charge; Sledge Jump is exotic-owned |
| Model / audio | Industrial wrench / maul; impeller spool-up on charge; floor crack on slam; gravitic *whump* on wave. Borrow closest melee feel until custom art |

### 5.2 Why no ammo / reload

Mycopunk primaries usually run mag + damage-regen ammo. Saxonite Wrench rejects that loop on purpose:

- Melee fantasy should not stop because a cell ran dry mid-pile.
- Economy cards would waste grid space better spent on wave / well / cryo.
- Cadence pressure comes from **whiff cost** (recovery + charge time), not reload animations.

**Implementation note (later):** force magazine/reserve infinite or bypass fire-gate ammo checks; hide reload UI; ensure `Reload` input is no-op while this gear is active.

### 5.3 Inputs (MeleeRework contract)

| Input | Role |
|-------|------|
| **Tap V** | Quick tap smash while gun stays out |
| **Hold V (~0.25s)** | Equip wrench full profile (MeleeRework owns detect/swap) |
| **M1 tap** (equipped) | Quick smash |
| **M1 hold → release** (equipped) | Charged slam at current torque |
| **RMB press** (equipped) | Gravity pull (Well). Baseline = pulse tug. **Not** Fists Guard |
| **R** | Nothing (baseline). Reserved only if a future exotic needs it — **none in v1** |
| **Jump** (with Sledge Jump exotic) | Jump window during/after slam enables hammer-jump propulsion |
| Heavy key | Normal heavy equip — **no** wrench→heavy feed cards in v1 |

MeleeRework owns tap/hold V and slot swap. This kit owns attacks while equipped / quick profile on tap V.

### 5.4 Charge / Torque model

Heaven Piercer + Shocklance Half Cocked DNA as structural precedent (melee-flavored):

| Field | Baseline intent |
|-------|-----------------|
| Full charge time | ~0.55–0.75s (playtest) |
| Release to swing | true — slam fires on release once past min floor |
| Can release early | true — partial torque = weaker slam |
| Auto-fire at full | **false** — player chooses the drop |
| Min charge floor | ~0.12–0.15 normalized — below = tap-tier smash or weakest legal slam (prefer weakest slam for forgiveness) |
| Move penalty | Scales up near full charge |

**On slam, read NormalizedTorque (0→1):**

| Stat | Torque 0 (tap / pluck) | Torque 1 (full) |
|------|------------------------|-----------------|
| Impact damage | Low–mid | High |
| Shockwave radius | ~2–3m | ~4.5–6m (Halo ~4.5m full-class) |
| Shockwave damage | Low | High |
| Knockback | Light | Heavy |
| Optional head VFX scale | Small | Large impeller flare |

Curve: smoothstep / ease-out so the last 20% of charge still matters without being binary.

### 5.5 Perfect Torque sweet spot (baseline skill toy)

| Param | Draft | Intent |
|-------|-------|--------|
| Band | ~0.82–0.95 normalized | Near full, not only 1.0 |
| Crit / bonus mult | ×1.25–1.35 | Readable reward |
| At 1.0 overhold | Full damage, **no** sweet crit; optional tiny aim sway | Holding forever is suboptimal |
| Perfect Torque rare | Widens band and/or raises mult | Path-agnostic skill card |

Audio: distinct “latch” tick when entering the band (cams/cable cousin to Heaven Piercer).

### 5.6 Impact & Shockwave (baseline)

```
On successful melee Impact (head volume hits ≥1 valid target OR ground slam rules — pick at impl):
  1. Apply Impact damage to primary target(s) in head cast (Normal)
  2. Emit Shockwave sphere/cylinder at impact point:
       - radius from torque + Wide Impeller
       - damage from torque + Hard Face
       - knockback from torque + Dense Head
       - no innate element
  3. Start swing recovery (Snap Recovery shortens)
  4. Clear / partial-clear charge
```

**Ground slam rules (draft):** if charge ≥ threshold and aim/pitch is downward-ish OR no target in head cast, still emit shockwave at feet/forward ground point so whiffed air swings are not the only failure — full whiff (no ground, no target) still costs recovery. Tune so floor-crack fantasy works in open space.

**Precision bias:** Impact prefers direct hits; shockwave is AOE (may apply reduced anatomy transfer if ever relevant — usually N/A). Cryo apply from path cards: Impact full amount, Wave reduced unless card says otherwise.

### 5.7 RMB — Gravity Well (baseline)

| Piece | Draft |
|-------|-------|
| Type | Pulse pull (not sustained vortex — Event Horizon owns sustain) |
| Shape | Forward cone or small sphere along aim |
| Strength | Weak — repositions grunts; bosses resist / reduced |
| Duration | Instant impulse + optional brief drag |
| Cooldown | ~1.2–1.8s baseline |
| Damage | **None** on baseline (Grinder Coils adds damage) |
| Element | None |
| Max targets | Small cap (e.g. 4–6) baseline |

**Intent:** Always teaches “tug into the blast.” Weak enough that Wellspring crowns still matter.

### 5.8 Base combat loop (no upgrades)

```
Close gap → M1 tap pack trash OR hold-charge slam elites
RMB tug stragglers into the next blast radius
Manage recovery and charge timing (sweet spot)
No reload. No ammo check. Just swing again.
```

Skill without upgrades: spacing, torque timing, pull-then-slam sequencing, not face-tanking without DR cards (none free on baseline).

---

## 6. Shared Vocabulary

| Term | Meaning |
|------|---------|
| **Impact** | Melee hit from the wrench head volume |
| **Shockwave / Wave** | Radial kinetic pulse on impact |
| **Well / Tug** | Gravity pull from RMB (baseline pulse; Event Horizon = sustained) |
| **Torque / Charge** | Hold-M1 normalized 0→1 slam power |
| **Perfect Torque** | Sweet-spot band near full charge |
| **Chilled / freezing** | Target has Cryo saturation > 0 |
| **Fully frozen** | Cryo `IsFullySaturated` (slow active) |
| **Frozen solid** | Harder CC only from specific cards (e.g. Flash Frost on small targets) — not baseline |
| **Shatter** | Vanilla shatter meter proc (~200 fill; melee fills fast) or kit cards that pay when freeze breaks |
| **Your wave / well** | AOE and pull spawned by **this player’s** Saxonite Wrench only |

### 6.1 Cryo system truths (reuse game / Rime research — do not invent parallel chill)

- `EffectType.Cryo = 10`
- Full saturation lifetime ~4s class; while fully saturated: slow (`SlowTargetThisTick` ~0.6f class)
- Shatter meter: non-Cryo damage builds toward ~200; **melee fills fast**; shatter = large precision-style burst and clears Cryo
- Saturation add: `amount * 0.1` per application → roughly **10 effect amount ≈ full freeze** from empty, before decay
- Experimental anchors:
  - `ShatterMelee` / `UpgradeProperty_CryoGrenade_ShatterMelee`: damage rider ~100, window ~2–2.22s
  - `FrozenEleDmg` / `FrozenElementMult`: ~1.10–1.13× vs frozen
- **Rimehead owns melee shatter execute on a primary.** Rime Charge keeps grenade control (rinks, plates, blizzard). Soft duo; no hard dependency.

### 6.2 No-ammo economy language

| Allowed cadence knobs | Forbidden |
|-----------------------|-----------|
| Swing recovery | Magazine size |
| Charge duration | Ammo capacity / reserve |
| RMB well cooldown | Reload duration |
| Whiff punishment | “Damage recharges ammo” cards |
| Inertia Battery ramp | Cell/heat mag that empties to soft-lock the gun |

---

## 7. Design Pillars vs Blood Carver (differentiation lock)

| | **Blood Carver** | **Saxonite Wrench** |
|--|------------------|---------------------|
| Cadence | Hold-M1 continuous tick saw | Discrete tap / charged slam |
| Resource | Blood stacks | None (torque is momentary, not a bank) |
| Fantasy | Harvest anatomy → spend red → feed heavy | Gravity control → shockwave → optional freeze-shatter |
| Element lean | Neutral / fire-on-spend | Kinetic baseline + **Cryo path** |
| Crowd tool | Pull while sawing (exotic) | Shockwave + pull **are the gun** |
| Ammo | Mag loop | **None** |
| Heavy | Path C identity | **Out of v1** |
| Sustain | Blood DR / chunks / deflect | Minimal baseline; cryo ward / spacing / Sledge Jump |

---

## 8. Gravity Wells (Thematic Attractors)

Not exclusive trees. Taking one exotic or epic pulls related cards into value; every upgrade remains equippable with every other (except explicit requirement flags — **none required in v1** beyond natural dead-stat mildness).

### Well A — **IMPETUS** (Shockwave / Clear)

*Own the blast radius. Multi-kill floor cracks.*

Core pieces: Aftershock, Wide Impeller, Hard Face, Dense Head, Ground Fracture, Follow-Through, Quake Step, Crust Breaker.

### Well B — **WELLSPRING** (Gravity / Setup)

*Pull them in. You are the center of mass.*

Core pieces: Event Horizon, Tug Coil, Lunge Latch, Anchor Bolt, Grinder Coils, Mass Driver, Wellspring Cap.

### Well C — **RIMEHEAD** (Cryo / Shatter)

*Freeze the pile. Crack it with the head.*

Core pieces: Absolute Zero, Cold Bolts, Brittle Seam, Rime Face, Wave Cryo, Shatter Timing, Flash Frost, Cryo Ward Head.

### Peer exotic (not a fourth well prison)

**Sledge Jump** — mobility / aerial slam. Feeds all three wells (gap close into Impetus, reposition Well, aerial freeze drops).

### Mix examples (expected, not edge cases)

- Event Horizon + Aftershock → vortex pack → double quake delete  
- Rime Face + Wave Cryo + Brittle Seam + Absolute Zero → freeze wave execute  
- Sledge Jump + Crust Breaker + Perfect Torque → aerial sweet-spot boss chunks  
- Lunge Latch + Follow-Through + Snap Recovery → aggressive clear tempo  
- Grinder Coils + Anchor Bolt + Hard Face → tug damage into slam  
- Absolute Zero + Rime Charge (soft) → grenade winter, wrench shatter  
- Mass Driver + Dense Head → bowling-pin displacement clear  
- Cryo Ward Head + Sledge Jump → cold-head skirmish sustain  

---

## 9. Content Budget & Universal Truths

| Rule | Value |
|------|--------|
| Total upgrades | **~30** |
| Exotics | **4** — Aftershock, Event Horizon, Absolute Zero, Sledge Jump |
| Exotic hex footprint | **Equal and large** across all four |
| Epics | **~9** |
| Rares | **~9** |
| Standards | **~8** |
| Path locks | **None** in v1 (Rime Face is enabler by power, not hard gate flag) |
| Ammo / reload cards | **Zero** |
| Heavy feed cards | **Zero** |
| Oddity / Contraband grid steal | Out of scope unless later parity pass |
| Shared vanilla staples (Boundary, etc.) | **Not** counted inside custom ~30 unless explicitly added later |
| v1 cross-mod | **MeleeRework soft**; Rime Charge soft tips only; no hard content deps |
| Oven / fire identity | **Out** — not a Thermite wrench |
| Blood meter | **Out** — Carver’s language |

*Count note:* 8S + 9R + 9E + 4X = **30**.

---

## 10. Full Upgrade List (~30)

Rarity guide: Standard / Rare / Epic / Exotic  
Well: **I** Impetus · **W** Wellspring · **C** Rimehead · **G** Glue  
Stack: prefer CanStack on standards; uniques on exotics; epics mostly unique  
Cell rule: Exotics larger; all Exotics same cell count.  
Player-facing names below. API names / numeric IDs at implementation (suggest gear **92800**, upgrades **92801–92830** — confirm against other Sparroh id ranges at ship).

Numbers are **v0 starting targets** — validate in playtest.

------------------------------------------------------------------------------
STANDARDS (~8) — spine
------------------------------------------------------------------------------

S1. Long Haft — Standard (G)
    +Melee head reach / legal impact length.
    Does not add falloff; still full damage in volume.

S2. Wide Impeller — Standard (I)
    +Shockwave radius (tap and charged).
    Stackable mild.

S3. Quick Latch — Standard (G)
    Faster charge time (lower full-torque duration).
    Stackable mild.

S4. Snap Recovery — Standard (G)
    Shorter swing recovery after impact or whiff.
    This is the “fire rate” card — no ammo analogue.

S5. Hard Face — Standard (I)
    +Impact and shockwave damage.
    Stackable mild.

S6. Tug Coil — Standard (W)
    +RMB pull strength and/or pull range.
    Stackable mild.

S7. Cold Bolts — Standard (C)
    +Cryo effect amount on wrench Cryo apply sources.
    Mild alone; shines with Rime Face / Wave Cryo / Absolute Zero.

S8. Dense Head — Standard (I)
    +Knockback on impact and shockwave.
    Displacement clear; pairs with Mass Driver.

------------------------------------------------------------------------------
RARES (~9)
------------------------------------------------------------------------------

R1. Perfect Torque — Rare (G)
    Widens sweet-spot band and/or increases sweet mult.
    Baseline band still exists without this card; this makes skill timing more generous/rewarding.

R2. Ground Fracture — Rare (I)
    Shockwave deals bonus damage to grounded targets and/or shells.
    Floor-crack fantasy; less bonus on airborne unless Sledge Jump aerial rules say otherwise.

R3. Lunge Latch — Rare (W)
    On charged release: short lunge toward aim (Halo short-lunge DNA).
    Distance modest; bosses/geometry safe clamps.

R4. Anchor Bolt — Rare (W)
    Enemies affected by your pull are briefly slowed or “stuck” after the tug.
    Soft setup CC — not full chain stun forever.

R5. Brittle Seam — Rare (C)
    +Damage and/or +shatter meter fill vs fully frozen targets.
    Carries FrozenEleDmg + melee shatter-fill fantasy.

R6. Rime Face — Rare (C)
    **Impacts apply Cryo** (primary Rimehead enabler).
    Effect amount from baseline apply + Cold Bolts.
    Without this or Absolute Zero / Wave Cryo, Cold Bolts is near-dead — acceptable rare gravity.

R7. Wave Cryo — Rare (C)
    Shockwave applies reduced Cryo to targets hit by the wave.
    Pack freeze primer; amount < Impact apply.

R8. Counterweight — Rare (G)
    Reduced move-speed penalty while charging; slight charge stability (less accidental release feel).
    Comfort / skirmish card.

R9. Follow-Through — Rare (I)
    If a single shockwave hits ≥ N enemies (draft N=3), refund a slice of torque and/or shorten recovery once.
    Rewards pack spacing; ICD so it cannot perma-stutter-step.

------------------------------------------------------------------------------
EPICS (~9)
------------------------------------------------------------------------------

E1. Quake Step — Epic (I)
    Shockwave leaves a short residual damage ring at the impact point (duration short, ticks few).
    Aftershock is the delayed **second quake**; Quake Step is lingering crust — different verb.

E2. Crust Breaker — Epic (I)
    Charged slams deal bonus shell damage and/or ignore a chunk of shell mitigation.
    Boss shell path without becoming DMLR anatomy rifle.

E3. Grinder Coils — Epic (W)
    RMB well deals mild damage while pulling (Grinder variant DNA lite).
    Still not Event Horizon sustain; pulse (or short drag) with chip damage.

E4. Mass Driver — Epic (W)
    Charged slam knock profile becomes a heavy **push** along aim / away from impact normal.
    Big displacement clear; may reduce pull synergy on that slam — hybrid tension is OK and fun.

E5. Shatter Timing — Epic (C)
    When you shatter a target (or hit during/just after a shatter break you caused): gain a ~2s damage window
    (experimental ShatterMelee DNA — large rider / mult).
    Clarifies melee shatter execute identity.

E6. Flash Frost — Epic (C)
    Perfect Torque (sweet-spot) hits apply bonus Cryo and may briefly **Frozen Solid** small/non-boss targets.
    Bosses: bonus Cryo only or reduced CC. ICD per target.

E7. Inertia Battery — Epic (G)
    Consecutive successful impacts without whiff ramp a small damage bonus; resets on whiff or timeout.
    Rewards clean pile work; not a second blood meter — ramp is shallow and short.

E8. Wellspring Cap — Epic (W)
    RMB cooldown reduced; pull max targets increased.
    Pure well economy crown-support.

E9. Cryo Ward Head — Epic (C)
    While you recently applied Cryo with this weapon OR near a fully frozen enemy: mild damage resistance
    (FrozeResist DNA, self-contained — not Rime Charge plates).
    Skirmish sustain for Rimehead divers; short grace after last apply.

------------------------------------------------------------------------------
EXOTICS (4) — equal large shapes
------------------------------------------------------------------------------

X1. Aftershock — Exotic (I) — Impetus crown
    On impact: primary shockwave, then a **delayed second quake** at the same point (or chain to a nearby dense cluster).
    Second wave damage/radius slightly reduced vs primary unless stacked supports say otherwise.
    Clear fantasy peak. Caps: cannot infinite-proc off its own residual forever.

X2. Event Horizon — Exotic (W) — Wellspring crown
    **Hold RMB** = sustained gravity vortex (pull toward well focus).
    **Release RMB** may auto-commit a slam toward/into the well (torque from current charge or a granted partial charge — tune one).
    Baseline pulse tug is replaced/upgraded while equipped.
    Risk budget: bosses resist; max hold duration or heat so perma-vortex is impossible; ally-safe (does not yeet teammates into pits without clamps).

X3. Absolute Zero — Exotic (C) — Rimehead crown
    Charged slam dumps **heavy Cryo** on impact + shockwave.
    Fully frozen targets hit by that slam’s wave take a **shatter-class payoff** (burst and/or forced shatter meter dump rules — prefer paying the real shatter system when possible).
    Does not require Rime Face, but stacks with it. Soft partner to Rime Charge blizzards.

X4. Sledge Jump — Exotic (G) — Mobility crown
    Timed jump during slam recovery / impact window propels the wielder (Halo hammer-jump DNA).
    Aerial slams gain bonus shockwave radius and/or damage.
    Clamps: max launch speed; no infinite skybox combo without ground reset; optional stamina-free but recovery-gated.
    Feeds gap-close into all wells.

---

## 11. Exotic Deep-Dives

### 11.1 Aftershock (Impetus)

**Fantasy:** The floor does not crack once — it answers twice.

**Behaviour sketch:**

- On any qualifying impact that emits a shockwave: schedule second quake after delay D (draft 0.35–0.55s).
- Second quake: radius ~0.85×, damage ~0.7–0.9× primary (playtest).
- Optional: if primary wave killed ≥1 target, second wave gains mild bonus (pack delete juice).
- Quake Step residual can coexist but must not triple-dip into unreadable ground god — prefer Aftershock = discrete second pulse; Quake Step = short DoT ring.

**Mix notes:**  
Event Horizon → pack stacked → double quake.  
Absolute Zero → freeze → shatter on first wave, Aftershock cleans.  
Follow-Through → multi-hit refund into faster second swing.

**Risk budget:**  
Boss multi-quake melt → second wave reduced vs bosses or ICD per brain.  
No recursive Aftershock on Aftershock.

### 11.2 Event Horizon (Wellspring)

**Fantasy:** You do not chase the room — the room falls into you.

**Behaviour sketch:**

- Hold RMB: sustained vortex at aim point or body-centered well (pick one at impl; prefer **aim-point well** for skill expression).
- Continuous pull impulses; Grinder Coils can add tick damage while held.
- Drain: max hold time and/or well “heat” that forces release; Wellspring Cap improves sustain slightly without removing the cap.
- Release: if torque ≥ min or always — auto-slam toward well focus (Lunge Latch stacks naturally).
- Replaces weak baseline pulse while exotic equipped (or upgrades pulse into “tap RMB = pulse, hold = horizon” — prefer **hold = horizon, tap still pulse** if input allows).

**Mix notes:**  
Absolute Zero release-slam into frozen pack.  
Anchor Bolt keeps victims in the second quake.  
Mass Driver on release-slam bowling-pins the far side.

**Risk budget:**  
Bosses reduced pull.  
Ally pull disabled or heavily reduced.  
Geometry / pit safety clamps.  
Cannot softlock AI forever — release or max duration.

### 11.3 Absolute Zero (Rimehead)

**Fantasy:** The impeller vents winter. What freezes, breaks.

**Behaviour sketch:**

- Charged slam (torque ≥ threshold, draft ≥0.5): add large Cryo effect amount on Impact and Wave.
- Against fully frozen targets in that slam’s wave: shatter-class payoff (prefer triggering/accelerating real shatter; fallback large precision burst + clear cryo).
- Tap smashes: little or no Absolute Zero bonus (keeps charge meaningful).
- Cold Bolts / Rime Face / Wave Cryo / Brittle Seam / Shatter Timing / Flash Frost all stack as supports.

**Mix notes:**  
Rime Charge blizzard soft setup → wrench execute.  
Shatter Timing window after first break → second slam delete.  
Sledge Jump aerial Absolute Zero for drama and radius.

**Risk budget:**  
With full Rime Charge co-op, freeze-delete can spike — payoff ICD per target; bosses resist forced shatter frequency.  
Do not apply full Absolute Zero cryo on every Aftershock second wave unless explicitly tuned (default: **primary slam only**).

### 11.4 Sledge Jump (Glue / Mobility)

**Fantasy:** The same force that cracks the floor throws *you*.

**Behaviour sketch:**

- On impact (or during short post-impact window): if jump is pressed, apply upward/forward launch impulse.
- Direction: blend camera forward + up (Halo-style); optional aim influence.
- Aerial state tag: next slam before landing = **Aerial Slam** (+wave radius/damage).
- Snap Recovery / Quick Latch affect how often you can chain jumps — still recovery-gated.
- Counterweight makes charging in air less miserable.

**Mix notes:**  
Lunge Latch + Sledge Jump = horizontal + vertical commit.  
Ground Fracture may **not** fully apply mid-air (readable: aerial = radius, grounded = fracture) — optional spice.  
Cryo Ward Head keeps divers alive between jumps.

**Risk budget:**  
Max launch clamps; no map-escape infinite without landing.  
PvE geometry: reduce launch near lethal drops if needed.  
Not a second movement ability replacement — it is swing-timed.

### 11.5 Exotic coexistence

| Pair | Rule |
|------|------|
| Aftershock + Event Horizon | **Encouraged.** Pull → double quake. |
| Aftershock + Absolute Zero | Allowed. Absolute Zero on **primary** slam; second quake kinetic unless a future card says otherwise. |
| Event Horizon + Absolute Zero | **Encouraged.** Vortex freeze setup → release slam shatter. |
| Sledge Jump + any | Allowed. Aerial rules apply to all crowns. |
| All four | Allowed if grid fits; power via ICDs, boss resists, hold caps, launch clamps. |
| Footprints | All four exotics **same cell count**, larger than typical rares/epics. |

---

## 12. Named Kit — Detailed Specs (quick reference)

### Aftershock (Exotic)
- Primary wave + delayed second quake. Unique. Impetus crown.

### Event Horizon (Exotic)
- Hold RMB sustained vortex; release slam commit. Unique. Wellspring crown.

### Absolute Zero (Exotic)
- Charged slam heavy Cryo + shatter payoff on fully frozen in wave. Unique. Rimehead crown.

### Sledge Jump (Exotic)
- Slam-timed hammer-jump + aerial slam bonus. Unique. Mobility crown.

### Quake Step (Epic)
- Short residual damage ring. Not a second delayed full quake.

### Crust Breaker (Epic)
- Charged slam shell shred.

### Grinder Coils (Epic)
- Pull deals mild damage.

### Mass Driver (Epic)
- Charged slam heavy push profile.

### Shatter Timing (Epic)
- ~2s post-shatter damage window (ShatterMelee DNA).

### Flash Frost (Epic)
- Sweet-spot bonus Cryo / brief Frozen Solid on small targets.

### Inertia Battery (Epic)
- Shallow consecutive-hit ramp; resets on whiff.

### Wellspring Cap (Epic)
- RMB CD down; more pull targets.

### Cryo Ward Head (Epic)
- Mild DR while cryo-active skirmishing.

### Perfect Torque (Rare)
- Sweet band widen / mult up.

### Ground Fracture (Rare)
- Wave bonus vs grounded / shells.

### Lunge Latch (Rare)
- Charged release short lunge.

### Anchor Bolt (Rare)
- Post-pull slow/stick.

### Brittle Seam (Rare)
- Amp / shatter fill vs fully frozen.

### Rime Face (Rare)
- Impacts apply Cryo.

### Wave Cryo (Rare)
- Wave applies reduced Cryo.

### Counterweight (Rare)
- Charge move penalty down.

### Follow-Through (Rare)
- Multi-hit wave → recovery/torque refund (ICD).

### Standards
- Long Haft, Wide Impeller, Quick Latch, Snap Recovery, Hard Face, Tug Coil, Cold Bolts, Dense Head — stat spines only; no silent exotic enables; **no ammo/reload**.

---

## 13. Synergy Notes (Player-Facing, Soft Only)

No mod dependencies. Loadout tips for README / codex blurb.

| Partner | Why it feels good |
|---------|-------------------|
| **Rime Charge** | Grenade owns winter ground control; wrench owns melee shatter execute |
| Splash / wet kits | Flash Frost / freeze setups if wet→freeze exists elsewhere; soft only |
| Blood Carver | Dual-melee meme loadouts; different verbs (saw vs slam) — not a shared meter |
| Shocklance + Fisticuffs | Ally/self melee damage buffs Shocklance — soft co-op joke |
| Movement employees | Sledge Jump + dashes = chieftain dive fantasy |
| Pull-friendly ally AOE | Tug into glob/acid/fire fields |

**Explicit non-goals v1:** heavy ammo feed; blood meter; mag/reload; innate baseline cryo; replacing Carver; hard dep on Rime Charge; fire/Thermite identity; Deflection Mass as mandatory exotic (can backlog).

---

## 14. Strengths, Weaknesses & Failure Modes

### Strengths

- **Always on** — no ammo anxiety, no reload beat stealing melee flow  
- Readable Halo-class fantasy: slam, wave, pull, jump  
- Three clean wells: clear / control / freeze-execute  
- High skill expression on torque timing and pull-then-slam  
- Soft cryo duo with Rime Charge without forcing grenade choice  
- Distinct from Blood Carver in every primary verb  

### Weaknesses

- No ranged option — gap close or die  
- Charged whiffs are expensive (recovery)  
- Baseline pull is weak without Wellspring  
- Rimehead cards are near-dead stats until Cryo enablers  
- No heavy battery, no blood sustain — less “stand in the blender forever” than Carver tank builds  
- Pure single-target static bosses may prefer anatomy saw or guns  

### Failure modes to avoid in tuning

| Failure | Mitigation |
|---------|------------|
| Event Horizon perma-vortex | Max hold, heat, boss resist, ally-safe |
| Absolute Zero + Rime Charge delete | Payoff ICD; boss shatter frequency caps; AZ on primary slam not every tick |
| Aftershock boss melt | Second wave boss mult; no recursive quake |
| Sledge Jump skybox / pit yeet | Launch clamps; optional lethal-drop dampen |
| Sweet spot too wide | Keep band narrow until Perfect Torque |
| Inertia Battery = free blood meter | Shallow ramp, short timeout, whiff reset |
| Hidden ammo/heat soft-lock | **No** empty-mag gate; heat only on Event Horizon hold if used |
| Pull grief allies | Ally pull off or tiny |
| Mass Driver + pits = unfun | Knockback clamps near hazards |
| Cryo Ward + other DR immortal | Mild DR only; short grace |
| 30 cards / 1 forced build | Standards universal; no hard path locks |
| Feels like Carver with a skin | Enforce discrete swings; no blood; no heavy feed |

---

## 15. Naming & Presentation

| Slot | Value |
|------|--------|
| Display name | **Saxonite Wrench** |
| Internal / API | `saxonite_wrench` |
| Design nicknames | Grav Hammer, Grav-Maul, Impact Wrench (notes only) |
| Short description | *Gravitic impact wrench. No ammo, no reload — just swing. Tap smash or charge a floor-cracking slam, tug enemies in with a gravity well, and bolt on cold-kit upgrades to freeze and shatter the pile.* |
| Thunderstore name | `SaxoniteWrench` |
| GUID | `sparroh.saxonitewrench` |
| Folder | `.new.SaxoniteWrench` |
| Gear / upgrade IDs | Gear **92800**; upgrades **92801–92830** (verify collisions at ship) |
| Type line | **Melee** |
| Soft dep | MeleeRework `sparroh.meleerework` |

### SAXON marketing blurb (draft)

> SAXON Saxonite Wrench — For employees who believe torque is a love language.  
> No cells. No belts. No “please reload while the fungus eats your legs.”  
> Baseline: kinetic impact, gravitic tug, floor-optional.  
> Aftermarket: double quakes, event-horizon hospitality, absolute-zero heads, and sledge-jump commuting.  
> If it still has a magazine UI, you are holding the wrong product.  
> “If they wanted to stay over there, they should have brought their own impeller.”

### Flavor / in-world

Industrial gravitic impeller reverse-engineered into a service wrench large enough to offend HR. Saxonite alloy head survives repeated shock-field discharge. Optional cryo bolt-on kit sold separately (grid not included — wait, grid is included).

---

## 16. Implementation Appendix

### Phase 0–1 (shipped scaffold)

| Piece | Approach |
|-------|----------|
| Registration | Clone vanilla **MeleeGear**; inject AllGear; `GearType.Melee` |
| Spawn | Remap catalog → base MeleeGear NGO prefab; stamp identity |
| MeleeRework | Soft `MeleeKitRegistry.RegisterKit` + kit id flag |
| Balance | `SwBalance` constants (AMR-style) |
| Data host | `SaxoniteWrenchBehaviour` |
| Combat | Behaviour torque + `OnFiredBullet` shockwave + RMB pull tick |
| Ammo | Unlimited / no spend |
| Mod flags | `[MycoMod(..., ModFlags.IsSandbox)]` |
| Soft dep | `sparroh.meleerework` |

### Later phases

| Piece | Approach |
|-------|----------|
| Upgrades | `PlayerData.CreateUpgrade` + `UpgradeProperty` Apply/Remove |
| Cryo | Real `EffectType.Cryo` on DamageData; shatter via real meter |
| Sledge Jump | Jump input window + launch impulse + aerial slam tag |
| Model | Placeholder melee mesh until AssetBundle |

### Suggested `SaxoniteWrenchBehaviour.Data` fields (sketch)

```
// Torque / swing
float chargeDurationMult;
float recoveryMult;
float sweetSpotMin;          // normalized
float sweetSpotMax;
float sweetSpotDamageMult;
float movePenaltyWhileCharging;

// Impact / wave
float impactDamageMult;
float waveDamageMult;
float waveRadiusMult;
float knockbackMult;
float reachMult;
bool aftershock;
float aftershockDelay;
float aftershockDamageMult;
float aftershockRadiusMult;
bool quakeStep;
float quakeStepDuration;
float quakeStepDpsMult;
bool crustBreaker;
float shellDamageMult;
bool groundFracture;
float groundedWaveBonus;
bool followThrough;
int followThroughMinTargets;
float followThroughRecoveryRefund;
bool inertiaBattery;
float inertiaPerStack;
int inertiaMaxStacks;
float inertiaTimeout;

// Well
float pullStrengthMult;
float pullRangeMult;
float pullCooldownMult;
int pullMaxTargets;
bool eventHorizon;
float eventHorizonMaxHold;
float eventHorizonPullPerTick;
bool eventHorizonReleaseSlam;
bool grinderCoils;
float grinderDamageMult;
bool anchorBolt;
float anchorSlowDuration;
bool lungeLatch;
float lungeDistance;
bool massDriver;             // push profile on charged slam
bool wellspringCap;

// Rimehead
bool rimeFace;
bool waveCryo;
float cryoEffectAmountMult;
bool absoluteZero;
float absoluteZeroMinTorque;
float absoluteZeroCryoAmount;
float absoluteZeroShatterPayoffMult;
bool brittleSeam;
float brittleDamageMult;
float brittleShatterFill;
bool shatterTiming;
float shatterTimingDuration;
float shatterTimingDamageMult;
bool flashFrost;
float flashFrostBonusCryo;
float flashFrostSolidDuration; // small targets
bool cryoWardHead;
float cryoWardDamageTakenMult;
float cryoWardGrace;

// Sledge Jump
bool sledgeJump;
float sledgeLaunchMult;
float aerialSlamDamageMult;
float aerialSlamRadiusMult;
```

### Ship cut vs stretch

**v1 must-ship (fantasy complete):**

- No ammo / no reload baseline kinetic wrench  
- Tap + charge slam + sweet spot  
- Baseline shockwave + weak RMB pull  
- All **4** exotics (Aftershock, Event Horizon, Absolute Zero, Sledge Jump)  
- Rime Face / Wave Cryo / Brittle Seam / Shatter Timing cryo spine  
- Full standard spine (reach, radius, charge, recovery, damage, tug, cold bolts, knockback)  
- Self-contained; soft Rime Charge tips only  

**Stretch / post-v1:**

- Deflection Mass (wave knocks projectiles / perfect-torque DR)  
- Hold-R exotic if a second utility is ever needed  
- Thin heavy-feed rare (explicitly rejected for v1 — only if design revisit)  
- Custom mesh / Wwise impeller spool  
- Shared staple parity (Boundary Incursion as oddity drop-in)  
- Config toggles for pull ally safety / launch clamps  

---

## 17. Open Questions (Balance / Feel — Not Blocking Doc)

1. Ground slam without a target: always emit foot wave above torque threshold? (Default: **yes**.)  
2. Event Horizon well center: aim-point vs body-centered? (Default: **aim-point**.)  
3. Absolute Zero on Aftershock second wave? (Default: **no** — primary slam only.)  
4. Sledge Jump: require jump *during* recovery window only, or also jump-cancel into slam? (Default: **impact/recovery window + aerial slam tag**.)  
5. Sweet spot baseline width before Perfect Torque. (Default: **narrow ~0.82–0.95**.)  
6. Exact hex shapes for 30 upgrades — author during implementation.  
7. Boss pull/shatter multipliers — set in first playtest pass.  
8. Whether tap smash shares full upgrade damage mults or a tap coefficient. (Default: **shared mults, lower base tap damage**.)  
9. UI: suppress ammo readout entirely vs show infinite glyph. (Default: **suppress / infinite**.)  
10. Re-check Carver / damage / cryo APIs in `.Resources/Assembly` after game updates before implementation.

---

## 18. Design Checklist

- [x] Name: **Saxonite Wrench**  
- [x] New **melee kit** (not primary; not vanilla wrench patch)  
- [x] MeleeRework soft platform  

- [x] **No ammo / no reload**  
- [x] Baseline **Normal** damage  
- [x] Cryo **opt-in** via Rimehead  
- [x] M1 tap + optional charge  
- [x] RMB baseline pull  
- [x] Sledge Jump as exotic  
- [x] No heavy link v1  
- [x] SAXON / gravity / industrial / cryo tone  
- [x] Three wells: Impetus / Wellspring / Rimehead  
- [x] Four equal large exotics  
- [x] ~30 upgrades table  
- [x] Differentiated from Blood Carver  
- [x] Cryo truths aligned with Rime / experimental  
- [x] Failure modes documented  
- [x] Phase 0–1 scaffold implemented  

---

## 19. Changelog (Design Doc)

| Date | Change |
|------|--------|
| 2026-08-19 | **v1.1 — Melee kit lock.** Slot = `GearType.Melee` (not Primary); clone MeleeGear/Fists; MeleeRework soft dep; tap V / hold V / M1 / RMB input contract; Phase 0–1 implementation notes. |
| 2026-08-07 | Initial design doc from Grav Hammer pitch + user locks: name **Saxonite Wrench**; normal baseline damage; no ammo/reload; RMB pull; tap+charge; Sledge Jump exotic; no heavy link; SAXON/gravity/industrial/cryo tone; wells Impetus/Wellspring/Rimehead; 30-upgrade table; cryo research from Rime Charge + experimental ShatterMelee/FrozenEleDmg; contrast Blood Carver / Shocklance / Heaven Piercer. |

---

*End of design document. Phase 0–1: registration + baseline tap/charge/wave/pull. Next: standards → rares → epics → exotics.*

