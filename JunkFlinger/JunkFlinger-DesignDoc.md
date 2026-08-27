# Junk Flinger – Design Document (v2)

> Status: **Design only** — no implementation yet.
> Working title in notes: Lead Flinger Rework. **Ship name: Junk Flinger.**
> Template base: `.new.primary.LeadFlingerRework` weapon content project.
> Product shape: **separate primary gear** — vanilla Lead Flinger is left unmodified.
> Supersedes: Design Doc v1 (Gambler / upgrade-gated Junk / core-kill Blood-Rush).

---

## 0. Locked Decisions (v2)

| Decision | Lock |
|----------|------|
| Ship name | **Junk Flinger** |
| Product shape | **Parallel primary**; vanilla Lead Flinger untouched |
| APIName (working) | `junk_flinger` |
| GUID (working) | `sparroh.junkflinger` |
| MycoMod (impl) | `IsSandbox` |
| Paths | **Chamber / Junk / Rush-Echo** (RNG is a Chamber sub-theme, not a peer path) |
| Baseline identity | **6-chamber cylinder framework** + **Junk resource** always on |
| Chamber depth | **Full per-chamber state** even on empty grid (each chamber can be special) |
| Junk mint | **Casings** (on fire / spent round) + **Scrap** (on kill) |
| Baseline Junk spend | **Scrap Pack** (hold-R) — weak teaching spend; **not** Blood-Rush |
| Blood-Rush | **Exotic only** — consume Junk → load N from reserve → fan burst dump |
| Blood-Rush ammo fiction | **True burst dump (2C):** spend Junk → pull N into mag from reserve → dump |
| Residue vs Blood-Rush hold-R | **OPEN** — see §6.3 / §16; both are spend crowns; priority TBD |
| Damage modifier rule | **% multipliers only** on this upgrade pool; no flat gun damage |
| Double Action | **Heavy Chamber**; **no charge-to-fire** |
| Modded Auto | Optional tempo; not a tax enabler |
| Baseline kill → reload speed | **Removed** |
| Practice Makes Perfect | **Snap Cylinder** rewrite |
| Phantom turbo | **Adjacency copy baked in** |
| Doobie + Lucky Bastard | **Encouraged**; elevate peers; **no LB nerf** |
| Doobie path | **Junk** all-in (mag 1 = degenerate single chamber) |
| ~30 upgrades v1 | Exotic shapes larger; each exotic same cell count; full rename pass |
| Soft crowns | Hybrids allowed; no hard exclusion matrix |
| External deps | Pattern/DNA only; no hard runtime deps |
| Doc scope | Full bible |

---

## 1. High Concept / Fantasy

**Junk Flinger** is the outlaw **six-chamber slug revolver** that scavenges the battlefield and stuffs the refuse back into the wheel.

Vanilla Lead Flinger funnels real power into one stack: **Big Fat Doobie + Lucky Bastard + Lucky Last** (+ Modded Auto + Double Action as a feel tax). High potential — but only one viable chart line. Kill-based reload fights refund cards. Practice Makes Perfect is a reload mini-game on a gun that wants to skip reload. Residue, Feeling Lucky, Phantom Limb, and Blood-Rush are cool verbs stranded next to the Doobie stack. Junk only exists if you drafted Residue. The cylinder is just “mag 6.”

Junk Flinger keeps the **click-speed slug revolver gunfeel** and rebuilds identity around systems that are **always on**:

1. **The Cylinder** — six real chambers with per-chamber state. Every shot advances the wheel. Last chamber, packed chambers, empty wheel — all first-class.
2. **Junk** — spent rounds leave **Casings**; kills drop **Scrap**. Hold reload to **Scrap-Pack** the next chambers. Blood-Rush is the mythic junk binge, not the tutorial.
3. **Rush / Echo** — empty the drum for Phantom’s ghost volley, or (with the exotic) drink Junk and fan a reserve-fed burst.

Three peer upgrade paths deepen those systems; they do not invent them from zero.

**One-liner:** *Six chambers. Casings and scrap. Pack the wheel — or drink the junk and dump the cylinder dry.*

**Element spine:** Normal (kinetic lead) at baseline. Fire / Acid / Shock as opt-in glue — not required for identity.

**SAXON / outlaw blurb (draft):**
  “Illegal wheelgun for operators who reload with whatever’s on the floor.
   Casings tick the counter. Corpses feed the hopper. Hold the gate, pack the
   cylinder, and put the mess downrange. Blood-Rush kit sold separately —
   warranty void if you fan the whole reserve into a friendly.”

Optional stingers:
- “If the chamber’s empty, you weren’t scavenging hard enough.”
- “Six shots. Then the wheel asks what you brought back.”
- “Phantom doesn’t miss what you fired. It only misses what you aimed at.”
- “Doobie isn’t luck. Doobie is one chamber and no excuses.”

---

## 2. Role & Fantasy in the Arsenal

| Trait | Value |
|-------|--------|
| **Slot** | Primary |
| **Range** | Close–mid slug (falloff present; Heavy Chamber / range cards extend) |
| **Role** | High-skill semi slug revolver; per-chamber craft; junk economy; dump-and-echo / rush clear |
| **Gap filled** | Vanilla LF = one upgraded mono-build (Doobie infinite last-shot). Junk Flinger = baseline cylinder+junk identity + three peer upgraded careers |
| **Synergies** | Core-kill economy (Outlaw / Volatile), movement (fan / airborne), self-risk (Home Cooking), multi-path hybrids, Phantom adjacency puzzles |

**Product shape:** New primary (**Junk Flinger**). Does **not** replace or patch vanilla Lead Flinger.

**Not trying to be:** long-range DMR, heat-infinite SMG, shotgun pellet hose, or a mandatory external-mod dependency.

### 2.1 Comparison snapshot

```
Weapon                 Niche                         Junk Flinger differentiator
---------------------  ----------------------------  ------------------------------------------
Vanilla Lead Flinger   Slug revolver (1D upgrade)    Parallel pool; baseline cylinder + Junk
Heat Cycler            Energy heat hose              Mag wheel + junk spend; not heat bar
Overdriver             Shock burst / bees            Semi slug; chambers; scrap economy
DMLR                   Anatomy transfer laser        Volume/chamber revolver — not parts
Aussie / Spillway      Sibling doc structure only    Shared bible patterns, different verbs
```

### 2.2 Relationship table

| Gear | Relationship |
|------|----------------|
| Vanilla Lead Flinger | Left in game untouched |
| Aussie Special / Spillway / Heat Cycler / DMLR / Overdriver | Sibling design structure only |
| SparrohsTurbocharges Phantom note | **Baked in** — touching upgrades copy onto phantom shots |
| Frantic Greed / other global upgrades | Soft synergy only; no hard dep |

---

## 3. Design Pillars

1. **Cylinder is the gun** — baseline tracks six chambers with real per-chamber state. Upgrades write onto chambers; they don’t invent the wheel.
2. **Junk is baseline economy** — casings from shots, scrap from kills; always minting; always a reason to watch the counter.
3. **Baseline spend teaches; exotics mythologize** — Scrap Pack is the weak hold-R verb on every empty grid. Blood-Rush / Residue are mythic spends, not the only way Junk matters.
4. **Three peer paths: Chamber / Junk / Rush-Echo** — RNG (variance, streaks, hot chambers) lives under **Chamber**, not as a fourth religion.
5. **Doobie + Lucky Bastard is allowed and celebrated** — elevate other strategies to peer power; **do not** anti-synergy-nerf the combo.
6. **Doobie is Junk all-in** — mag-1 mega chamber = concentrated refuse/lead, not the gambler crown.
7. **Distributed damage** — many Standard/Rare/Epic cards carry meaningful **% damage** (and volume/tempo verbs). Power is a grid, not two staples.
8. **Damage modifier rule (LOCKED)** — all damage bonuses on this weapon are **percentage multipliers only**. No flat damage adds on upgrades. No mixing flat + % on the same gun’s upgrade pool.
9. **Baseline does not grant reload-on-kill** — kill payoffs are path-owned (except Junk scrap mint, which is resource, not reload accel).
10. **No charge-to-fire tax** — Double Action’s hold-trigger identity is cut; rewrite as click-to-fire Heavy Chamber.
11. **Soft path tension** — Chamber likes last-chamber drama; Rush-Echo likes empty→reload or junk binge; Junk likes bank vs pack. Hybrids allowed; no hard exclusion matrix.
12. **Phantom adjacency copy is sacred** — upgrades touching Phantom Limb copy their on-bullet effects onto phantom shots (turbo DNA baked in).
13. **~30 upgrades for v1** — exotic shapes larger than others; each exotic same cell count; full rename pass.
14. **Failure states stay fun** — cold chambers, junk greed, phantom into empty air, Blood-Rush dry reserve, over-pack into nothing, self-hit risk.

---

## 4. Core Mechanics & Gunfeel

### 4.1 Base gun (no upgrades)

| Trait | Draft / intent |
|-------|----------------|
| Fire mode | Semi-auto slug; fire as fast as you click |
| Damage | Honest mid slug; **slight raise vs vanilla empty-grid spirit** if needed so non-Doobie is playable |
| Element | Normal |
| Magazine / cylinder | **6 chambers** / ~126 reserve spirit (paths mutate) |
| Reload | Standard reload beat; refills chamber slots |
| Kill → faster reload | **OFF** on baseline |
| Cylinder framework | **ON** — full per-chamber state (§4.2) |
| Junk resource | **ON** — mint + Scrap Pack spend (§4.3–4.4) |
| Blood-Rush / Residue nova / Phantom / variance deck | **OFF** until upgrades |
| ADS | Normal unless a crown says otherwise |
| Model / audio | Borrow Lead Flinger / FastReloadShotgun until custom art |
| Charge-to-fire | **None** on baseline |

**Vanilla wiki spirit (confirm at impl):**
```
Damage 50, RPM 840, interval 0.071, mag 6, reserve 126, reload 1.4s
Falloff 35–60, bullet speed 100
Identity: semi slug, click-fast; vanilla also had kill→faster reload (STRIP on clone)
```

### 4.2 Cylinder framework (BASELINE — always on)

The magazine is not an opaque ammo int. It is a **wheel of chamber slots**.

| Piece | Baseline rule |
|-------|----------------|
| `ChamberCount` | 6 (Doobie forces 1; mag-size cards grow the wheel) |
| `ChamberIndex` | Advances on each shot; wraps / locks empty at end of wheel |
| `Chambers[]` | Array of per-chamber state (see below) |
| Last chamber | Index == last filled slot — always readable (audio/VFX tick) |
| Empty wheel | Natural reload beat; Rush-Echo path pays this harder |
| Reload | Refills slots from reserve; may apply Pack state if Scrap Pack was charged |

#### Per-chamber state (baseline schema)

Each chamber carries flags/data upgrades and baseline spends can write:

| Field | Baseline use |
|-------|----------------|
| `Occupied` | Has a round |
| `Packed` | Scrap-Pack empowered (baseline spend) |
| `PackTier` | 0 = normal; 1+ = packed potency (stacking rules TBD) |
| `DamageMult` | Chamber-local % (starts 1.0; Packed applies small bonus) |
| `SizeMult` | Chamber-local bullet size (Packed slight +) |
| `Tags` | Upgrade-owned: `Hot`, `Whiff`, `Explosive`, `Elemental`, `PhantomMarked`, etc. |
| `RolledVariance` | Optional; set when a variance card arms the chamber (not baseline RNG spam) |

**Baseline chamber behavior (empty grid):**
- All chambers spawn **Occupied, PackTier 0** on reload unless Scrap Pack charged.
- Firing consumes current chamber → advances index.
- **Last chamber** gets a tiny readable carrot so the wheel teaches itself: e.g. **+6–10% damage** and a hammer VFX/audio (playtest dial; can be VFX-only if damage carrot feels too strong).
- Chambers are **individually addressable** — upgrades may pre-roll, tag, or replace the next N chambers without turning the gun into Feeling Lucky by default.

**What baseline cylinder is NOT:**
- Not a full 1–200% variance deck on every shot (that’s Dead Man’s Hand / Loaded Dice).
- Not free explosive last-shot (Lucky Last).
- Not free ammo refund (Lucky Bastard).

**Impl note:** Even when `magazineSize` changes, rebuild `Chambers[]` length to match. Doobie = length 1; every shot is last chamber.

### 4.3 Junk resource (BASELINE — always on)

Junk is a first-class resource on the behaviour host (Heat-Cycler pattern: always visible, always minting, weak baseline spend).

#### Mint — two channels (LOCKED)

| Channel | When | Draft amount | Fiction |
|---------|------|--------------|---------|
| **Casings** | On firing a round (shot spent) | +1 Junk per shot (tune; multi-pellet = per shot not per pellet unless Shrapnel says otherwise) | Ejected brass / spent hulls scooped mid-fight |
| **Scrap** | On killing a target | +2–3 Junk per kill (cores maybe +bonus via upgrades only) | Torn plates, teeth, loose screws |

**Rules:**
- Both channels work with **zero upgrades**.
- Casings ensure Junk income even when you’re whiffing kills (chamber practice still feeds the hopper).
- Scrap rewards clean-up and makes pack/rush tempo spike after a good cylinder.
- Soft cap e.g. **25–30** Junk — greed has a ceiling; overflow can tick a tiny waste VFX or convert at awful rate (prefer hard soft-cap with “full hopper” audio).
- HUD: Junk counter / pips on weapon or player chrome — **must be readable empty-grid**.

**Not baseline:**
- Residue’s full-stack explosive nova
- Blood-Rush burst dump
- Home Cooking self-hit mint (upgrade)
- Hit-not-kill mint (Refuse Rounds upgrade)

### 4.4 Baseline Junk spend — Scrap Pack (hold-R)

Blood-Rush is **Exotic**. Empty-grid still needs a junk **payoff**, not only a counter.

**Scrap Pack — baseline hold RELOAD**

| Piece | Draft |
|-------|--------|
| Input | **Hold R** (≥ ~0.25–0.35s). **Tap R** = normal reload |
| Requirement | Junk ≥ **PackCost** (e.g. 3); not already fully packed pending; not mid conflicting exotic spend |
| Consume | **PackCost** Junk (fixed), *or* spend up to remaining empty/next-cylinder slots × cost-per-chamber — prefer **flat cost → full next wheel pack** for readability |
| Effect | Mark the **next cylinder** (all chambers refilled on upcoming/current reload, or immediately pack remaining live chambers + next reload — pick one impl; prefer **arm Pack on next reload completion** for clean UX) with `Packed` |
| Packed chamber bonus | **+% damage** (e.g. +12–18% per packed shot) and slight **+bullet size**; clear VFX (dirty slug / wrapped tape / scrap jacket) |
| Duration | Until those packed chambers are fired; no permanent gun buff |
| Failure | Hold with 0 Junk → empty hopper click; no reload cancel if player wanted tap-reload (input disambiguation critical) |
| Recovery | Short arm time; can pack again after packed wheel is spent |

**Intent:** Always useful teaching tool — scavenge → pack → shoot meaner six. Weak enough that **Blood-Rush** and **Residue** still feel like mythic upgrades.

**Combat read:**
```
Click M1 → casings mint Junk → kills mint Scrap
Hold R (Junk ≥ cost) → arm Scrap Pack
Reload (or finish wheel) → next 6 chambers are Packed mean slugs
Empty packed wheel → repeat
```

### 4.5 Base combat loop (no upgrades)

```
Click M1 → slug from current chamber → wheel advances
   ↘ casings tick Junk every shot
   ↘ kills add Scrap Junk
   ↘ last chamber ticks harder (tiny % / VFX)
   ↘ empty wheel → reload beat
   ↘ hold R when Junk banked → Scrap-Pack the next wheel
   ↘ without crowns: honest click-speed revolver with a scavenger loop
   ↘ Chamber path: last-shot drama, variance tags, multi-chamber craft
   ↘ Junk path: mint rate, pack efficiency, Doobie single refuse chamber, Residue nova
   ↘ Rush-Echo: Phantom ghost volley; Blood-Rush junk binge burst dump
```

### 4.6 Inputs

| Input | Baseline | With crowns |
|-------|----------|-------------|
| **M1** | Fire current chamber (semi) | Modded Auto: full-auto; Blood-Rush armed: fan dump; Doobie: single mega chamber |
| **RMB / AIM** | Normal ADS | Unchanged unless a card removes ADS |
| **R tap** | Reload (refill chambers from reserve) | Reload; Phantom triggers on completed reload when equipped; Snap Cylinder may snap-finish |
| **R hold** | **Scrap Pack** (baseline Junk spend) | See §4.7 ownership priority |

### 4.7 Hold-R ownership priority (SOFT — Residue interaction OPEN)

When multiple spend systems exist:

| Priority (draft) | Owner | Behavior |
|------------------|--------|----------|
| 1 (highest) | **Blood-Rush** exotic when player is arming / confirming Rush | Replaces baseline Scrap Pack with Junk → reserve burst load (§6.2) |
| 2 | **Residue** exotic | **OPEN** vs Blood-Rush — see §6.3 |
| 3 | **Scrap Pack** baseline | Default weak pack-next-cylinder |

**Until Residue vs Rush is locked:**
- Document both as Junk-spend crowns.
- Do **not** ship hard mutex code assumptions in the bible beyond: *one hold-R primary fantasy must win when both equipped; hybrids may merge effects in playtest.*
- Preferred directions to try later (§16): Residue empowers packed/Rush shots with explosion; **or** Residue replaces pack with full-stack nova; **or** Residue is kill-mint + on-pack detonation without stealing Rush.

### 4.8 Crown priority (SOFT — not hard mutex)

| Combo | Behavior |
|-------|----------|
| Any single crown | Full fantasy |
| Doobie + Lucky Bastard | **Fully supported poster hybrid** — mag 1 + last-shot refund loop |
| Doobie + Scrap Pack / Residue | Single chamber becomes the junk altar — pack/nova feeds the one slug |
| Doobie + Lucky Last | Last (=only) chamber explodes — valid |
| Phantom + anything | Phantom copies **adjacent** upgrade effects onto echo shots |
| Blood-Rush + Phantom | Rush dump → later reload still echos what you fired — hybrid freak OK |
| Blood-Rush + Chamber luck | Burst chambers can inherit variance/last rules per impl notes |
| Chamber + Rush-Echo | Soft tension (last chamber vs dump); both work; player chooses tempo |
| All three paths | Allowed; power from verbs stacking; watched in playtest — no ban list |

---

## 5. Damage & Economy Rules (LOCKED)

### 5.1 Vanilla failure mode

Effective power ≈ Doobie (huge flat-ish package) + LB refund + Lucky Last AOE + DA charge damage + Modded Auto to make DA usable. Multi-shot grids and non-Doobie exotics feel like traps. Practice Makes Perfect / kill-reload fight refund builds. Junk/Phantom/Blood-Rush/Feeling Lucky stranded.

### 5.2 Junk Flinger rules

1. **All upgrade damage modifiers are % only** (e.g. `damage *= 1.15`). **No flat damage** on weapon upgrades.
2. Do not mix “some cards flat, some %.” One language: **multiplicative percent**.
3. Vanilla ports that used flat damage (Doobie package, etc.) **retune to % of gun damage** (plus bullet size / recoil / RoF as non-damage levers).
4. **Distributed % damage** across many rarities so light grids and multi-chamber builds scale.
5. **Volume verbs** (shrapnel pellets, phantom echo, blood-rush burst count, junk explosion size, packed chamber count) are first-class power.
6. **Tempo verbs** (RoF, reload, mag/chamber count, refund chance) are first-class.
7. **Chamber-local mults** compose with gun-global mults:  
   `shot = base * gunDamageMult * chamber.DamageMult * conditionalMults`.
8. **Doobie stays strong** with LB; peer paths must reach competitive clear/ST without requiring mag 1.
9. **Empty-grid** must clear packs via honest slug + Scrap Pack loop — not wet noodle, not broken.
10. **Junk amounts are not damage** — stacks feed spends; spends deal % gun damage.

### 5.3 Budget sketch (playtest dials — all damage as %)

| Lever | Starting intent |
|-------|-----------------|
| Baseline damage vs vanilla LF | +10–20% empty-grid feel if needed (catalog %, not upgrade flats) |
| Last-chamber baseline carrot | +6–10% on final occupied chamber (or VFX-only) |
| Scrap Pack packed chamber | +12–18% damage; slight size |
| Typical Standard damage card | +8–12% damage |
| Typical Rare damage card | +12–18% damage |
| Typical Epic damage card | +15–25% damage **or** strong non-damage verb + smaller % |
| Doobie | Mag 1; large **%** damage; +bullet size; −RoF; −reserves; recoil up |
| Dead Man’s Hand variance | Chambers/shots roll **% of shot damage** in a band — not flat rolls |
| Residue spend | Explosion = **% of gun damage × f(stacks)** |
| Blood-Rush burst | Per-pellet uses gun **%** pipeline; burst count is the volume verb |
| Heavy Chamber | −RoF, large **%** damage, optional +range % — **click to fire** |
| Lucky Bastard | High last-chamber refund chance; **no mag-1 special nerf** |

### 5.4 What damage is NOT

- Not flat adds that die in late scaling.
- Not “only Doobie / only DA have real damage.”
- Not charge-gated damage.
- Not “Junk stacks deal damage by existing.”

---

## 6. Crowns & Sacred Systems

### 6.1 Lucky Bastard — Exotic (Path A — Chamber engine)

**Fantasy:** The last round in the cylinder always has another chance.

**Vanilla DNA:** Firing last shot has a high chance to refund ammo (~81–89%, +1 ammo).

**Junk Flinger:**
- Keep as **Exotic** last-chamber engine
- Reads **chamber state**: refund re-occupies the last slot (or pushes a fresh chamber) so the wheel keeps spinning
- **No anti-synergy with Doobie** — mag 1 means every shot is “last”; infinite hammer loop is intentional
- Multi-chamber grids also want LB (Lucky Last, packed last, fan tempo)
- Refund is ammo economy, not a damage card
- Packed last chamber that refunds stays interesting (keep pack tier or re-pack rules — prefer: refunded chamber keeps PackTier)

### 6.2 Blood-Rush — Exotic (Path C — Rush keystone) [REWRITTEN]

**Fantasy:** Drink the hopper. The cylinder becomes a belt. Fan it all.

**Vanilla DNA:** Core kill loads burst ammo; press to fire large burst (~20–28).  
**v1 doc DNA:** Kept core→burst.  
**v2 LOCK:** **No core gate.** Junk is the fuel. **Exotic only** (not baseline).

**Activation (2C — true burst dump):**
```
Hold RELOAD while Blood-Rush equipped and Junk ≥ RushCost
  → consume RushCost Junk (e.g. 6–10)  [or tiered: spend more → larger N]
  → instantly load N bullets into the magazine FROM RESERVE
  → chambers rebuild as a Rush wheel (optional Rush tags on each)
  → fire mode becomes dump/fan (semi spam or brief auto-assist — prefer click-fan unless Modded Auto)
  → each Rush bullet deals bonus % damage
  → ends when Rush chambers spent, reserve can’t feed, or cancel rule
```

| Piece | Draft |
|-------|--------|
| Cost | e.g. **8 Junk** baseline exotic cost (supports reduce) |
| Burst size N | e.g. **18–28** spirit (volume verb; tune vs per-hit %) |
| Ammo | **Reserve → mag** instantly; if reserve < N, load what you can (partial rush) |
| Damage | Per hit: gun % pipeline × RushDamageMult (e.g. 0.55–0.75 of normal shot if N is huge, or nearer 1.0 if N is modest — **tune so core-free Rush isn’t boss delete and isn’t wet**) |
| Relation to Scrap Pack | **Replaces** baseline Scrap Pack while exotic equipped (or: hold longer / second threshold arms Rush vs Pack — prefer **replace** for one readable mythic spend) |
| Cores | **Optional upgrade riders** may refund Junk or extend N on core kill — not required to activate |
| Phantom | Shots fired during Rush record for echo like normal |
| Doobie | Odd hybrid: Rush may temporarily override mag-1 for the dump then restore — **playtest carefully**; valid freak build |

**Failure states:** dry reserve → sad click partial rush; spending Junk into empty air; Rush without scrap income discipline.

### 6.3 Residue — Exotic (Path B — Junk crown) [HOLD-R vs RUSH OPEN]

**Fantasy:** Kills leave junk. Stuff the barrel with everything you’ve scavenged and fire a fat explosive slug — *or* make every pack/rush shot dirty.

**Vanilla DNA:**
- Kill → Junk stacks (+3–4)  *(v2: scrap mint is baseline; Residue amplifies)*
- Hold R → consume all Junk → large explosive bullet
- Size ∝ Junk consumed

**Junk Flinger:**
- Residue is a **Junk path crown**, not the invent-Junk card
- Always-on mint means Residue focuses on **spend quality / mint multipliers / explosive fantasy**
- Damage/size: **% of gun damage × f(stacks consumed)**

**Hold-R interaction — UNLOCKED (user unsure):**

| Option | Behavior | Pros | Cons |
|--------|----------|------|------|
| **A — Replace** | Residue replaces Scrap Pack (and fights Blood-Rush for hold-R): full-stack → one explosive slug | Classic vanilla read | Button conflict with Rush exotic |
| **B — Empower** | Residue does not steal hold-R; Packed and/or Rush chambers **explode / leave residue pools**; +mint | Best hybrid with Rush; one button | Less “fire the bomb” fantasy |
| **C — Dual threshold** | Short hold = Pack/Rush; long hold = Residue nova | Both verbs | Fussy UX |

**Doc default for writing supports until locked:** treat Residue as **mint amplifier + explosive payload crown**, and write Packing Grease / Scrap Hopper assuming spend exists; finalize hold-R in §16 playtest. Prefer exploring **B** if Blood-Rush already owns mythic hold-R dump.

**With Doobie:** single chamber receives nova/pack altar treatment — scavenge → stuff the one hole → boom.

### 6.4 Big Fat Doobie / Single Chamber Junk — Exotic (Path B)

**Fantasy:** One chamber. All the junk. All the lead.

**Vanilla DNA:** Mag 1; much larger bullet; large damage package; heavy recoil; slower fire; less range/reserves.

**Junk Flinger rewrite:**
- `ChamberCount` → **1**
- Large **% damage** (not flat +147 style)
- +Bullet size; +Recoil %; −fire rate %; −ammo capacity % / efficiency as trades
- **Lives on Junk path** — concentrated refuse/lead
- Every shot is last chamber → LB/Lucky Last poster hybrid
- Scrap Pack / Residue feed the single altar chamber
- Explicitly synergizes with Lucky Bastard, Lucky Last, Home Cooking risk

**Not:** the only viable ST card. **Is:** the Junk all-in crown and a valid hybrid piece.

### 6.5 Phantom Limb — Exotic (Path C — Echo crown)

**Fantasy:** On reload, a ghost Junk Flinger materializes and fires a copy of every bullet you fired before the reload.

**Vanilla DNA:** Replay spent mag as phantom shots on reload.

**Junk Flinger + turbo DNA (LOCKED):**
- On reload completion: phantom fires copies of bullets spent since last full reload (match vanilla spirit)
- **Adjacency copy:** any upgrade whose shape **touches** Phantom Limb on the grid has its **on-bullet / on-hit effects copied onto phantom shots**
- “Touching” = edge/corner adjacent per game hex rules (confirm at impl)
- Phantom shots respect owner damage % and **should replay chamber tags where honest** (Packed echo, elemental, Lucky Last-on-final-echo if defined)
- Whiffing a full phantom volley into empty air is a fun failure state

### 6.6 Dead Man’s Hand — Exotic (Path A — Chamber luck keystone)

**Fantasy:** The cylinder is a deck. Chambers come up hot or cold.

**v2 role:** Chamber **sub-theme** crown — not a peer path name. Still Exotic if frozen 30 keeps six exotics.

**Effects (draft):**
- Modest +chamber count
- On reload (or on arm), each chamber **pre-rolls** a damage % band (e.g. 25–175% or 50–150% — tune) stored in chamber state
- Natural max → `Hot` tag + VFX; natural min → `Whiff` beat (fun failure)
- Supports (Loaded Dice, Snake Eyes, High Roller) deepen band / consolations / Junk mint on min/max

**Feeling Lucky** vanilla → spine of this exotic + Loaded Dice support (same as v1 split).

### 6.7 Boundary Incursion — Oddity

Grid grow. Universal keep. No damage staple.

---

## 7. Upgrade Paths (gravity wells — hybrids intended)

### Path A — CHAMBER
**“The wheel is the weapon.”**

- **Spine:** per-chamber state, last-shot payoffs, empty-wheel timing, optional luck tags, mag as more chambers
- **Crowns:** Lucky Bastard; Dead Man’s Hand (luck sub-theme)
- **Supports:** Lucky Last, Hot Streak, Loaded Dice, Snake Eyes / High Roller, Extra Chambers, Heavy Chamber, Fan Fire
- **Hybrid hooks:** LB + Doobie (Junk); min/max rolls mint Junk; last-shot + Phantom adjacency; Packed last chamber drama

### Path B — JUNK
**“Casings tick. Scrap drops. Spend the hopper.”**

- **Spine:** mint rate (casing/scrap), cap, pack efficiency, risk-to-mint, single-chamber all-in, explosive refuse
- **Crowns:** Residue; Big Fat Doobie
- **Supports:** Scrap Hopper, Home Cooking, Refuse Rounds, Packing Grease, Volatile Munitions, Bandolier (keeps Rush/reserve fed)
- **Hybrid hooks:** Doobie + LB + LL poster; Residue + Blood-Rush (when hold-R locked); risk cards + Chamber luck

### Path C — RUSH / ECHO
**“Empty the gun — or binge the junk and don’t stop.”**

- **Spine:** dump tempo, reload power window, phantom volley, Blood-Rush reserve binge, core/reserve economy
- **Crowns:** Phantom Limb; Blood-Rush
- **Supports:** Juiced Up, Snap Cylinder, Outlaw, Modded Auto, Fresh Cylinder, Delirium
- **Hybrid hooks:** Phantom adjacency copies Chamber/Junk cards; Rush dump recorded into Phantom; Outlaw keeps reserve alive for Rush

### Path × verb matrix

```
                  CHAMBER              JUNK                  RUSH / ECHO
Damage form       chamber % / last     pack % / spend %      volume (echo/burst) + %
Mag desire        last chamber         flexible / mag 1      empty → reload OR rush N
Reload            avoid if LB looping  pack between wheels   power window / echo
Junk relation     optional luck mint   mint + pack/nova      Blood-Rush binge fuel
Clear             Lucky Last / hot     pack wheel / nova     phantom + rush fan
ST                LB loop / hot chamber Doobie altar         rush into core / echo
Poster hybrid     LB + Doobie + LL     Residue + Doobie      Phantom + Blood-Rush
```

**RNG note:** Variance is a **Chamber toolkit**, not Path “Gambler.” Marketing sentence leads with wheel + scrap, not dice.

---

## 8. Double Action & Modded Auto Fate (LOCKED direction)

### 8.1 Problem

Double Action = longer trigger pull (charge) + huge damage. On a semi click-revolver this feels bad, so players also take Modded Auto — double tax — and the pair mostly exists to feed Doobie.

### 8.2 Double Action → Heavy Chamber (rewrite)

**Heavy Chamber — Rare or Epic (Chamber / glue)**  
- **No charge gate** — still click-to-fire  
- Fire rate reduced (%)  
- Damage increased (**% only**)  
- Optional: +range % / +falloff start %  
- Writes as heavier chamber archetype; valid on multi-chamber and Doobie  

**CUT:** chargeTime / “needs longer trigger pull” identity entirely.

### 8.3 Modded Auto — tempo card (keep, retune role)

**Modded Auto — Epic (Rush-Echo / glue)**  
- Full-auto at reduced fire rate (%)  
- **Not** a damage keystone  
- **Not** required to make another card usable  
- Honest hose for Blood-Rush fans, Juiced dumps, and players who hate semi spam  

---

## 9. Full Upgrade List (~30 ship + backlog)

Rarity: Standard / Rare / Epic / Exotic / Oddity  
Tags: A Chamber · B Junk · C Rush-Echo · G Glue · J Junk-resource · % = percent damage only  
Cell rule: Exotics larger; all Exotics same cell count.  
Names are player-facing (full rename where noted).  
**All damage numbers in implementation must be % multipliers.**

------------------------------------------------------------------------------
PATH A — CHAMBER                                         [~8]
------------------------------------------------------------------------------

A1. Dead Man’s Hand — Exotic (Luck keystone)
    +Chamber count. On reload, each chamber pre-rolls damage as a random
    percentage of normal shot damage within a wide band (e.g. 25%–175% — tune).
    Hot max / whiff min tags. Variance identity under Chamber — not a peer path.
    Damage language: pure % of shot damage. No flat roll adds.

A2. Lucky Bastard — Exotic (Engine)
    Firing your last chamber has a high chance to refund ammo into that slot.
    No special-case nerf with mag 1. Poster pair with Doobie; also enables
    multi-chamber last-shot loops with Lucky Last / Packed last.

A3. Lucky Last — Rare (keep identity)
    The last occupied chamber explodes (AOE). Clear tool among several.
    Explosion damage follows gun % / triggering shot %.

A4. Loaded Dice — Epic (Feeling Lucky port / support)
    Deepens Dead Man’s Hand band and/or +chambers and slightly improves
    minimum roll %. If DMH cut from exotic count, elevate this instead
    (only one primary variance exotic in frozen 30).

A5. Hot Streak — Rare
    Consecutive hits without missing raise a streak; each stack adds small
    % damage (caps). Missing or reloading clears or halves stacks.
    Rewards click accuracy; pure % ramp.

A6. Snake Eyes — Rare
    On natural minimum chamber roll (or low roll chance if no DMH):
    refund 1 ammo OR mint bonus Junk (hybrid hook into baseline economy).
    Failure state → consolation prize.

A7. High Roller — Rare
    On natural maximum chamber roll: brief move speed and small % damage
    window. Celebrates the hot chamber.

A8. Extra Chambers — Standard
    +Magazine / chamber count. More wheel, more packs, more rolls.

------------------------------------------------------------------------------
PATH B — JUNK                                            [~9]
------------------------------------------------------------------------------

B1. Residue — Exotic (Keystone)
    Amplifies Junk fantasy: improved scrap mint and/or explosive spend.
    Hold-R exact behavior OPEN vs Blood-Rush (§6.3). Damage always
    % gun × stack factor when nova exists. Not required to invent Junk.

B2. Big Fat Doobie — Exotic (Keystone) (Junk all-in)
    Chamber count becomes 1. The chamber is much larger and deals greatly
    increased damage (%). +Bullet size, +recoil %, −fire rate %, −reserves %.
    Junk-path fantasy: one chamber of concentrated lead/refuse.
    Fully intended with Lucky Bastard / Lucky Last / Pack / Residue.

B3. Home Cooking — Rare (risk)
    Each bullet has a chance to deal bonus % damage and a smaller chance to
    damage you. Self-hit mints bonus Junk (baseline economy hook).
    Self-damage = flat HP or % max HP (HP cost ≠ gun damage rule).

B4. Scrap Hopper — Epic
    +Junk from scrap kills and/or +casings per shot. Mild −move speed or
    +recoil % while Junk above a threshold (greed weight).

B5. Packing Grease — Rare
    Scrap Pack (baseline hold-R) is faster / lower commit; +packed potency
    or +packed size. QoL for baseline spend — still useful if Residue/Rush
    replace pack (then retarget to “whatever spend you have”).

B6. Volatile Munitions — Epic (lean Junk)
    Killing a core causes an elemental explosion (Fire / Shock / Acid rolled
    on apply). Explosion damage = % of gun damage. Clear finisher; also glue.

B7. Refuse Rounds — Rare
    Modest +% damage. Shots have a small chance to mint bonus Junk on hit
    (extra casings / embedded scrap) — not only on kill.

B8. Lead Poisoning — Standard (rename OK: Toxic Slug)
    Modest +% damage. Honest staple.

B9. Bandolier — Standard
    +Ammo capacity (reserves). Feeds Blood-Rush dumps and long missions.

------------------------------------------------------------------------------
PATH C — RUSH / ECHO                                     [~9]
------------------------------------------------------------------------------

C1. Phantom Limb — Exotic (Keystone)
    On reload, a phantom Junk Flinger fires a copy of every bullet you fired
    prior to reloading.
    **Adjacency copy (LOCKED):** upgrades touching this one on the grid copy
    their relevant on-shot/on-hit effects onto phantom bullets.
    Prefer replaying chamber tags (Packed, Hot, etc.) where honest.

C2. Blood-Rush — Exotic (Keystone) [REWRITTEN]
    Hold RELOAD: consume Junk → instantly load N rounds from reserve into
    the cylinder → fan dump with bonus % damage per shot.
    No core-kill gate. Replaces baseline Scrap Pack while equipped (draft).
    Burst size is the volume verb; per-hit stays on % pipeline.

C3. Juiced Up — Epic
    Unloading your entire wheel within a time window supercharges your next
    wheel (+% damage on those chambers). Empty-mag engine.

C4. Outlaw — Epic
    Killing a core refunds ammo to the magazine / reserve (+N). Keeps Rush
    fed; reduces forcedowns mid-echo loops.

C5. Snap Cylinder — Rare (Practice Makes Perfect rewrite)
    Kill during reload → snap-complete reload (readable, combat-tied).
    Serves players who actually reload (Phantom / non-LB loops).

C6. Modded Auto — Epic
    Fire full-auto at reduced fire rate (%). Tempo only; no damage keystone.
    Comfort for Rush fans and Juiced dumps.

C7. Heavy Chamber — Rare (Double Action rewrite)
    −Fire rate %, +% damage, optional +range %. **Click to fire — no charge.**
    Deliberate heavy slug; valid with or without Doobie.

C8. Fan Fire — Rare
    +% damage when hip-firing. Aggressive revolver stance.

C9. Fresh Cylinder — Standard (Fresh Bullet Frenzy lean)
    After reload, briefly +% damage. Pays the reload tax with a carrot.

------------------------------------------------------------------------------
GLUE / GUNFEEL / MOBILITY / ELEMENT                      [shared pool]
------------------------------------------------------------------------------

G1. Shrapnel Loading — Rare
    Each shot fires 3 pellets with reduced damage per pellet (split EV;
    % language). Volume clear. Casings mint: once per trigger pull default.

G2. High Caliber — Rare
    +% damage, −max fire rate %. Honest trade staple.

G3. Just Add More Gunpowder — Rare
    +% damage, +recoil %. Distributed damage.

G4. Controlled Substance — Rare
    −Recoil %, −% damage. Handling downshift.

G5. Breathe Deep — Standard
    +Range / falloff. No damage flat.

G6. Adrenaline Rush — Rare
    Firing briefly increases move speed.

G7. Ride the High — Epic
    Killing a target while airborne causes a brief hover.

G8. Delirium — Epic
    Killing a target briefly increases your damage (%).

G9. Burning Tips — Rare
    Bullets apply Fire buildup. Element opt-in.

G10. Ralph’s Conundrum — Epic
    Firing toggles element between Fire and Acid.

G11. Chemical Symbiosis — Rare
    While fully affected by an element, channel it to bullets.

G12. Unnatural Focus — Rare
    +Aim zoom; +range while aiming.

G13. Hallucinated Space — Standard
    +Chamber count (alternate Extra Chambers naming).

G14. Self Storage — Epic
    Chamber count + per occupied adjacent cell, − per empty adjacent cell.
    Spatial mag toy — backlog if too swingy for v1.

G15. Boundary Incursion — Oddity
    +Upgrade grid size.

G16. Lead Press — Standard
    Modest +% damage. Grid-fill bite without Doobie.

G17. Cylinder Grease — Standard
    +Reload speed %.

G18. Stable Grip — Standard
    −Recoil %.

G19. Brass Catcher — Rare [NEW — casing economy]
    +Junk from casings (e.g. +1 extra per shot) or chance for double casings.
    Chamber/Junk glue; makes empty-grid pack uptime rise without kill gating.

G20. Wrecker Instinct — Rare [NEW — scrap economy]
    +Junk from scrap kills; slight +% damage for a moment after scrap mint.
    Junk path support without being Residue.

------------------------------------------------------------------------------
FROZEN v1 SHIP POOL (exactly 30) — v2 RETUNE
------------------------------------------------------------------------------

EXOTIC (6)
  1  Dead Man’s Hand          (A)  — chamber luck crown (sub-theme, not peer path)
  2  Lucky Bastard            (A)  — last-chamber refund engine
  3  Residue                  (B)  — Junk spend/mint crown (hold-R vs Rush OPEN)
  4  Big Fat Doobie           (B)  — Junk all-in chamber 1; % damage
  5  Phantom Limb             (C)  — reload echo + adjacency copy
  6  Blood-Rush               (C)  — Junk → reserve burst dump (NO core gate)

EPIC (8)
  7  Loaded Dice              (A)  — variance / chamber support for DMH
  8  Scrap Hopper             (B)  — +Junk mint; greed weight
  9  Volatile Munitions       (B)  — core elemental explosion (% dmg)
  10 Juiced Up                (C)  — empty wheel → next wheel % damage
  11 Outlaw                   (C)  — core → magazine/reserve ammo
  12 Modded Auto              (C)  — full-auto tempo; not a damage keystone
  13 Ride the High            (G)  — airborne kill hover
  14 Delirium                 (G)  — on-kill brief % damage

RARE (10)
  15 Lucky Last               (A)  — last chamber explodes
  16 Hot Streak               (A)  — hit streak % ramp
  17 Home Cooking             (B)  — % dmg chance + self-hit → Junk
  18 Packing Grease           (B)  — Scrap Pack / spend QoL + potency
  19 Refuse Rounds            (B)  — modest % dmg; hit chance → Junk
  20 Snap Cylinder            (C)  — kill during reload → snap finish
  21 Heavy Chamber            (C/A)— DA rewrite: % dmg, −RoF, no charge
  22 Fan Fire                 (C)  — hip-fire % damage
  23 Shrapnel Loading         (G)  — 3 pellets; split EV; no flats
  24 High Caliber             (G)  — % dmg, −RoF
  —  Brass Catcher / Wrecker Instinct: promote into frozen 30 if a Rare
     slot opens (e.g. swap vs Fan Fire or High Caliber in playtest)

STANDARD (5)
  25 Extra Chambers           (A/G) — +chamber count
  26 Lead Poisoning           (G)   — modest % damage staple
  27 Lead Press               (G)   — modest % damage staple
  28 Bandolier                (G)   — +ammo reserves (Rush fuel)
  29 Cylinder Grease          (G)   — +reload speed %

ODDITY (1)
  30 Boundary Incursion       (G)   — +upgrade grid size

BACKLOG (designed, expand later)
  Snake Eyes, High Roller, Fresh Cylinder, Just Add More Gunpowder,
  Controlled Substance, Breathe Deep, Adrenaline Rush, Burning Tips,
  Ralph’s Conundrum, Chemical Symbiosis, Unnatural Focus, Hallucinated Space,
  Self Storage, Stable Grip, Brass Catcher, Wrecker Instinct,
  Multiversal Thievery / Edge Fault (contraband parity only if desired),
  Phantom VFX pass, Junk HUD pips, Residue↔Rush hold-R final mode,
  core-kill riders that extend Blood-Rush (optional, not activation gates).

NOTE: Delirium ships as Epic #14. Self Storage stays backlog.
Baseline systems (Cylinder state, Casings, Scrap, Scrap Pack) are NOT upgrades.


------------------------------------------------------------------------------
CUT / DEMOTE / REWRITE FROM VANILLA + v1 DOC
------------------------------------------------------------------------------

| Source | Fate |
|--------|------|
| Kill → faster reload (baseline) | **CUT from baseline**; path-owned kill verbs only |
| Junk only via Residue | **CUT** — Junk baseline (casings + scrap) |
| Blood-Rush core → burst | **REWRITE** — Exotic; Junk cost → reserve dump; no core gate |
| Blood-Rush as baseline spend | **CUT** — baseline spend is **Scrap Pack** |
| Gambler as peer path name | **DEMOTE** — luck is Chamber sub-theme |
| RNG as primary fantasy | **DEMOTE** — still ship DMH; marketing leads wheel+junk |
| Practice Makes Perfect | **Snap Cylinder** |
| Double Action (charge fire) | **Heavy Chamber** — % dmg, −RoF, **no charge** |
| Big Fat Doobie as only build | **Junk exotic**; peers elevated; still strong with LB |
| Lucky Bastard nerf vs Doobie | **Never** — combo encouraged |
| Feeling Lucky | **Dead Man’s Hand** + **Loaded Dice** |
| Phantom Limb | Keep + **adjacency effect copy** |
| Modded Auto | Tempo only; not DA enabler |
| Flat damage packages | **All retuned to %** |
| Edge Fault / Multiversal | Optional contraband parity; not in frozen 30 |

---

## 10. Example Builds

### Pure Chamber (multi-chamber, light luck)
Lead Press → Extra Chambers → Dead Man’s Hand → Loaded Dice → Hot Streak → Lucky Last → High Caliber  
*Play:* Fat wheel, pre-rolled chambers, explode the last round, streak rewards accuracy. Scrap Pack between wheels. No Doobie required.

### Poster hybrid (Doobie + LB + LL) — still valid
**Big Fat Doobie** + **Lucky Bastard** + **Lucky Last** + Lead Press / High Caliber + Home Cooking  
*Play:* Infinite (or near-infinite) mega exploding chamber. Celebrated, not nerfed. Pack the single altar when Junk allows.

### Pure Junk scavenger (no Doobie, no Rush)
Scrap Hopper → Refuse Rounds → Packing Grease → Home Cooking → Brass Catcher (or Lead Poisoning) → Volatile Munitions → Bandolier  
*Play:* Casings+scrap flood the hopper; Scrap Pack every wheel; cores pop elemental. Mag stays multi-chamber.

### Junk all-in chamber
**Big Fat Doobie** + Residue + Packing Grease + Lead Press + Scrap Hopper  
*Play:* Mint Junk, stuff the single chamber, optional LB if offered.

### Pure Phantom echo
**Phantom Limb** (touch: High Caliber, Lucky Last, Packed-supporting cards) → Juiced Up → Snap Cylinder → Cylinder Grease → Fan Fire → Outlaw  
*Play:* Dump wheel → reload → ghost volley with copied adjacent effects. Baseline Pack still buffs what gets recorded.

### Blood-Rush binge
**Blood-Rush** + Bandolier + Outlaw + Modded Auto + Scrap Hopper + Delirium  
*Play:* Fill hopper with casings/scrap → hold R → reserve dumps into the wheel → fan. No cores required to arm; cores keep ammo flowing via Outlaw.

### Heavy click slug (no Doobie)
Heavy Chamber + High Caliber + Lead Press + Fan Fire + Delirium + Extra Chambers  
*Play:* Slow hard clicks; % stacks; marksman revolver; pack when convenient.

### Hybrid freak (trailer)
Phantom Limb + Dead Man’s Hand + Blood-Rush + Lucky Bastard  
*Play:* Pre-roll chambers, refund last shots, binge Junk into a dump, echo the chaos on reload.

---

## 11. Strengths, Weaknesses & Risks

### Strengths
- Baseline fantasy without drafting a crown (wheel + junk + pack)
- Three real upgraded careers + celebrated Doobie hybrid
- Damage is % everywhere — scales into late game
- Distributed % cards make light grids matter
- Phantom adjacency creates buildcraft spatial puzzles
- Junk has mint channels + baseline spend + crown spends
- No charge-to-fire feel tax
- Baseline doesn’t fight refund builds with kill-reload
- Blood-Rush no longer sad without cores

### Weaknesses / fun failure states
- Dead Man’s Hand cold wheel (low pre-rolls)
- Scrap Pack greed (holding Junk forever / packing into downtime)
- Blood-Rush into dry reserve or empty air
- Phantom volley into wrong direction
- Home Cooking self-chip without heal support
- Heavy Chamber + Doobie = very slow click (player choice)
- Hopper full soft-cap while learning spend cadence

### Design risks
- **Doobie+LB+LL still outscales peers** — fix by elevating Phantom/Rush/Pack/Residue EV, not by nerfing LB
- **Residue vs Blood-Rush hold-R** unresolved — must lock before impl of spend input
- **Phantom adjacency copy** complexity (what copies; chamber tags?)
- **% stacking ceiling** — many small % + Pack + Hot Streak + Delirium + Juiced + Doobie
- **Blood-Rush N vs per-hit %** — boss delete vs wet noodle
- **Casing mint rate** too fast (always capped) or too slow (Pack never up)
- **Full chamber framework** impl cost vs mag-int simplicity — commit early to `Chambers[]`
- **Scrap Pack vs tap-reload** input disambiguation
- **Doobie + Blood-Rush** mag override rules
- Network: phantom volley + rush load + pack state authority

---

## 12. Success Criteria / Player Fantasy Checklist

- [ ] Empty-grid Junk Flinger clears packs without Doobie via slug + Scrap Pack
- [ ] Casings mint Junk on shots; scrap mints Junk on kills — both readable
- [ ] Junk HUD exists and matters with zero upgrades
- [ ] Per-chamber state is real (pack/last/upgrade tags), not flavor text
- [ ] Pure Chamber multi-chamber builds compete for clear/ST
- [ ] Pure Junk scavenger (no Doobie, no Rush) feels complete
- [ ] Pure Phantom and pure Blood-Rush each feel complete
- [ ] Blood-Rush activates from Junk, **not** from core kill
- [ ] Blood-Rush is Exotic; baseline spend is Scrap Pack
- [ ] Doobie + Lucky Bastard + Lucky Last still slaps and is not discouraged
- [ ] No upgrade uses flat gun damage; all gun damage mods are %
- [ ] Double Action charge identity is gone; Heavy Chamber is click-to-fire
- [ ] Modded Auto is optional comfort/tempo, not a tax
- [ ] Baseline has no kill→reload-speed passive
- [ ] Practice Makes Perfect is gone or replaced by Snap Cylinder
- [ ] Phantom adjacency copy is readable and exciting in grid craft
- [ ] Doobie reads as Junk all-in, not “the gambler card”
- [ ] Path names read Chamber / Junk / Rush-Echo (no Gambler peer path)
- [ ] Vanilla Lead Flinger still exists untouched
- [ ] Co-op: explosions/echoes/rush respect friendly-fire rules; fun for allies
- [ ] Residue vs Blood-Rush hold-R locked before spend input ships

---

## 13. Universal Truths (Mycopunk alignment)

- Exotic shapes should always be larger than others; each exotic should use the same number of cells.
- v1 targets **~30** upgrades (frozen list above); backlog is real design, not trash.
- Three paths create different build options but **may intermingle** on the grid.
- Full rename for rework identity (iconic names kept where sacred: Doobie, Lucky Bastard, Residue, Phantom Limb, Blood-Rush).
- Prefer chamber / junk / rush-echo verbs over generic +% only — but Lead Press / Lead Poisoning / High Caliber exist so damage is honest and distributed.
- **Damage modifiers: % only. No flat gun damage on this upgrade pool.**
- Parallel product: **Junk Flinger**; vanilla **Lead Flinger** unmodified.
- Baseline systems > upgrade-gated identity (Cycler heat lesson).

---

## 14. Vanilla Lead Flinger → Junk Flinger Fate Table

| Vanilla name | Junk Flinger name | Path | Notes |
|--------------|-------------------|------|-------|
| (baseline kill reload speed) | **Removed** | — | Path-owned kills only |
| (baseline stats) | Slight empty-grid raise if needed | — | Catalog retune; still slug revolver |
| (mag as int only) | **Cylinder framework** | baseline | Per-chamber state always on |
| (no junk without Residue) | **Casings + Scrap + Scrap Pack** | baseline | Resource always on |
| Big Fat Doobie | Big Fat Doobie | B | Junk exotic; **% damage** retune |
| Lucky Bastard | Lucky Bastard | A | No Doobie nerf; last **chamber** |
| Lucky Last | Lucky Last | A | One clear tool among many |
| Feeling Lucky | Dead Man’s Hand + Loaded Dice | A | Chamber luck sub-theme |
| Residue | Residue | B | Crown; mint amplify + spend OPEN vs Rush |
| Phantom Limb | Phantom Limb | C | + adjacency copy; chamber tag replay |
| Blood-Rush | Blood-Rush | C | **Exotic**; Junk → reserve dump; **no core gate** |
| Double Action | Heavy Chamber | A/C/G | **No charge**; % dmg; −RoF |
| Modded Auto | Modded Auto | C/G | Tempo only |
| Juiced Up | Juiced Up | C | Empty wheel → next wheel % |
| Outlaw | Outlaw | C | Core → ammo (Rush fuel) |
| Practice Makes Perfect | Snap Cylinder | C | Kill-during-reload snap |
| Home Cooking | Home Cooking | B | Risk; self-hit → Junk |
| Volatile Munitions | Volatile Munitions | B/G | Core elemental; % dmg |
| Shrapnel Loading | Shrapnel Loading | G | Volume clear; casing once/shot |
| High Caliber | High Caliber | G | % dmg −RoF |
| Just Add More Gunpowder | backlog / Gunpowder | G | % dmg +recoil |
| Lead Poisoning | Lead Poisoning | G | % dmg staple |
| Fan Fire | Fan Fire | C/G | Hip-fire % |
| Fresh Bullet Frenzy | Fresh Cylinder | backlog | Post-reload % |
| Delirium | Delirium | G | On-kill % window |
| Adrenaline Rush | Adrenaline Rush | backlog | Move on fire |
| Ride the High | Ride the High | G | Airborne kill hover |
| Controlled Substance | backlog | G | −recoil −% dmg |
| Breathe Deep | backlog | G | Range |
| Bandolier | Bandolier | G | Reserves / Rush fuel |
| Hallucinated Space | Extra Chambers / backlog | A/G | Chambers |
| Unnatural Focus | backlog | G | ADS range |
| Burning Tips | backlog | G | Fire appl |
| Chemical Symbiosis | backlog | G | Channel element |
| Ralph’s Conundrum | backlog | G | Fire/Acid toggle |
| Self Storage | backlog | G | Spatial chambers |
| Boundary Incursion | Boundary Incursion | G | Keep |
| (none) | Scrap Pack | baseline | Hold-R weak Junk spend |
| (none) | Lead Press | G | Distributed % dmg |
| (none) | Scrap Hopper | B | Junk mint up |
| (none) | Packing Grease | B | Pack/spend QoL |
| (none) | Refuse Rounds | B | Hit → Junk chance |
| (none) | Hot Streak | A | Accuracy % ramp |
| (none) | Dead Man’s Hand | A | Variance exotic |
| (none) | Cylinder Grease | G | Reload % |
| (none) | Brass Catcher | backlog/B | Casing mint up |
| (none) | Wrecker Instinct | backlog/B | Scrap mint up |
| Edge Fault / Multiversal | optional | — | Not frozen 30 |

---

## 15. Implementation Notes (for later coding passes)

### 15.1 Product / registration

- New primary via weapon template: clone **Lead Flinger / FastReloadShotgun** (not CartridgeSMG)
- Unique gear id + APIName e.g. `junk_flinger`
- Display name **Junk Flinger**
- `PlayerData.CreateUpgrade` pool; SpawnGear remap + stamp identity + ApplyUpgrades
- `[MycoMod(..., ModFlags.IsSandbox)]`
- Do **not** remove vanilla Lead Flinger from AllGear

### 15.2 Host behaviour

`JunkFlingerBehaviour` (or subclass when prefab exists) holding:

```
// cylinder
int chamberCount;                    // default 6
int chamberIndex;
struct ChamberState {
    bool occupied;
    int packTier;
    float damageMult;                // default 1f
    float sizeMult;                  // default 1f
    byte tags;                       // Hot, Whiff, Explosive, Rush, etc.
    float varianceMult;              // 1f if unused
}
ChamberState[] chambers;

// junk economy
int junkStacks;
int junkSoftCap;                     // e.g. 30
int casingsPerShot;                  // baseline 1
int scrapPerKill;                    // baseline 2–3
bool scrapPackArmed;
float scrapPackHoldProgress;
float scrapPackDamageMult;           // e.g. 1.15f
int scrapPackCost;                   // e.g. 3

// crowns / path state
float damageMultiplier;              // aggregate gun % (start 1f)
bool residueEnabled;
bool doobieEnabled;
bool phantomEnabled;
bool bloodRushEnabled;
bool deadMansHandEnabled;
float varianceMin, varianceMax;
float luckyBastardChance;
bool luckyLastEnabled;
bool moddedAutoEnabled;
bool heavyChamberEnabled;
float heavyChamberDamageMult;
float heavyChamberRoFMult;
bool juicedReady;
float juicedDamageMult;
int bloodRushBurstRemaining;
int bloodRushCost;
float bloodRushDamageMult;
bool bloodRushActive;

// phantom
List<PhantomShotRecord> shotsThisMag;  // include chamber snapshot tags
PhantomCopyFlags phantomCopy;          // adjacency resolved on ApplyUpgrades
```

- Prefer Harmony + behaviour on cloned gun; ideal long-term: `JunkFlinger : Gun` prefab
- **Strip vanilla kill→reload speed** on the clone’s baseline behaviour

### 15.3 Damage pipeline (LOCKED)

```
// Pseudocode — all upgrade damage is multiplicative %
base = GunData.damage
shot = base * behaviour.damageMultiplier
shot *= chambers[chamberIndex].damageMult          // pack / chamber-local
if (chambers[i].varianceMult != 1f) shot *= variance
if (hotStreak)    shot *= streakMult
if (juicedMag)    shot *= juicedDamageMult
if (heavyChamber) shot *= heavyChamberDamageMult
if (bloodRushActive) shot *= bloodRushDamageMult
// Doobie applies its % into damageMultiplier or doobieMult
// NEVER: shot += flatUpgradeDamage
```

- Residue explosion: `explodeDmg = gunDamageStat * residueDamagePercent * f(stacksConsumed)`
- Volatile core explosion: same % language
- Lucky Last AOE: % of triggering shot or gun % — pick one, stay %

### 15.4 Hooks

| Area | Approach |
|------|----------|
| Baseline kill reload | Do not enable; zero clone-inherited kill reload accel |
| OnFiredBullet | Advance chamber; mint **casings** Junk; record phantom shot w/ chamber snapshot; Hot Streak; Juiced; apply chamber mults to bullet |
| Last chamber | Baseline carrot; Lucky Bastard refund; Lucky Last explode |
| OnKill | Mint **scrap** Junk; Delirium; Volatile core; Outlaw ammo; Snap Cylinder if reloading |
| Hold R | Baseline Scrap Pack **or** Blood-Rush load **or** Residue (when locked) |
| Scrap Pack resolve | On reload complete or immediate remaining-chamber pack — stamp `packTier` |
| Blood-Rush arm | Spend Junk; pull N from reserve into mag; rebuild chambers as Rush tags; set burst remaining |
| Reload complete | Phantom Limb volley; Fresh Cylinder buff; clear shotsThisMag after echo; apply armed Pack |
| Apply upgrades | Sum % mults; resolve Phantom neighbors; force chamber count 1 if Doobie; auto fire interval if Modded Auto |
| Double Action | **Do not port charge**; Heavy Chamber = fireInterval + % damage only |
| Network | Phantom volley owner-auth; Rush load + Junk stacks synced; Pack state synced |

### 15.5 Phantom adjacency copy

1. On grid apply, find upgrades sharing edge (and decide corners) with Phantom Limb.
2. Build `PhantomCopyProfile`: element appliers, explode-on-last, Home Cooking chance, Shrapnel split, etc.
3. **Do copy:** on-hit effects, element, explode flags, per-chamber variance if DMH touches, Packed potency if recorded in shot history, damage mult already global.
4. **Do not copy:** chamber count, reload duration, hold-R spends, Blood-Rush arm triggers, grid size, move speed (unless explicitly desired).
5. Turbo note: generous on-bullet copy, conservative on gun-stats.

### 15.6 Doobie + spends

- Mag/chamber count 1: Scrap Pack empowers the single chamber.
- Residue (if nova): stuffs/explodes the one chamber (prefer altar read).
- Blood-Rush: define override — temporary multi-chamber rush wheel then restore Doobie-1, **or** Rush spends Junk to supercharge the single chamber N times from reserve without growing mag. **Playtest; document chosen rule in changelog.**

### 15.7 Related mods / DNA (not required at runtime)

| Source | DNA |
|--------|-----|
| SparrohsTurbocharges | Phantom adjacency copy |
| Heat Cycler | Baseline resource + weak R spend + exotic mythic spends |
| Aussie / Spillway / DMLR / Overdriver docs | Structure, frozen 30, soft crowns, fate tables |
| Vanilla Lead Flinger | Gunfeel, Residue DNA, Phantom reload, LB/LL/Doobie |
| Weapon template | Registration, SpawnGear stamp, CreateUpgrade |

---

## 16. Open Tuning Questions (playtest, not all design blockers)

1. Dead Man’s Hand band: 1–200% vs 25–175% vs 50–150%; pre-roll on reload vs on fire.
2. Doobie % damage target so it remains mythic but Phantom/Rush ST can compete.
3. Lucky Bastard chance near vanilla ~85% or slight global tune (still no Doobie-only nerf).
4. **Casings per shot** and **scrap per kill** vs soft cap 25–30 — Pack uptime target ~every 1–2 wheels.
5. Scrap Pack cost and packed % bonus.
6. Blood-Rush: cost, N, per-hit % — boss safety vs pack clear.
7. **Residue vs Blood-Rush hold-R (USER OPEN):** Replace / Empower / Dual threshold — **must lock before spend input impl**.
8. Phantom: corner-adjacent or edge-only? Replay Packed tiers?
9. Hot Streak cap and clear rules (reload clears?).
10. Home Cooking self-damage: flat HP vs % max HP.
11. Whether Delirium stays Epic #14 or yields to Brass Catcher / Self Storage.
12. Shrapnel total EV vs single slug; casing mint once per shot.
13. Snap Cylinder: kill-during-reload only vs timing hybrid.
14. % stacking soft cap for conditional buffs (Pack + Juiced + Streak + Delirium).
15. Baseline last-chamber carrot: +% vs VFX-only.
16. Doobie + Blood-Rush mag override rule.
17. Partial Rush when reserve < N: allow vs block activation.

---

## 17. Locked Decisions Log

| Decision | Lock |
|----------|------|
| Ship name | **Junk Flinger** |
| Product shape | **Parallel primary**; vanilla Lead Flinger untouched |
| Paths | **Chamber / Junk / Rush-Echo** |
| RNG | Chamber **sub-theme** (DMH), not peer path “Gambler” |
| Baseline cylinder | **Full per-chamber state** (depth C) |
| Baseline Junk mint | **Casings (on fire) + Scrap (on kill)** |
| Baseline Junk spend | **Scrap Pack** hold-R |
| Blood-Rush | **Exotic only**; Junk → **reserve burst dump (2C)**; **no core gate** |
| Residue vs Rush hold-R | **OPEN** |
| Doobie + LB | **Encouraged**; elevate peers; **no LB nerf** |
| Doobie path | **Junk**, not luck crown |
| Double Action | **Heavy Chamber**; **no charge-to-fire** |
| Modded Auto | Optional tempo; not a tax enabler |
| Damage mods | **% only; no flat gun damage; no mix** |
| Baseline kill reload | **Removed** |
| Practice Makes Perfect | **Snap Cylinder** |
| Phantom turbo | **Adjacency copy baked in** |
| External deps | DNA/pattern only |

### Design changelog

#### v2 (this doc)
- Ground-up identity rethink from user principles
- **Cylinder framework baseline** with full per-chamber state
- **Junk baseline:** casings on shot + scrap on kill; HUD resource
- **Scrap Pack** baseline hold-R spend (Blood-Rush is not the tutorial)
- **Blood-Rush rewrite:** Exotic; consume Junk → load N from reserve → fan dump; core gate removed
- Paths renamed **Chamber / Junk / Rush-Echo**; Gambler demoted to Chamber luck sub-theme
- Residue hold-R vs Blood-Rush left **explicitly open**
- Kept v1 locks: % damage only, no charge DA, Phantom adjacency, Doobie+LB sacred, parallel product, frozen ~30
- Added Brass Catcher / Wrecker Instinct as economy supports (backlog/promote)
- Impl notes updated for Chambers[] + dual mint + Scrap Pack + Rush load

#### v1
- Junk Flinger identity; parallel product
- Three paths: Gambler / Junk / Cylinder-Echo
- Doobie on Junk; LB unnerfed; Double Action → Heavy Chamber
- Damage % only; Phantom adjacency; baseline kill-reload removed
- Frozen 30 + fate + impl notes
- *(Superseded where it conflicts with v2 baseline systems and Blood-Rush rewrite.)*

---

## 18. Next Steps After This Doc

1. **Lock Residue vs Blood-Rush hold-R** (Replace / Empower / Dual) — blocking for spend input.
2. Confirm Scrap Pack resolve timing (arm on hold vs apply on reload complete).
3. Confirm Doobie + Blood-Rush override rule.
4. Review frozen 30 vs Brass Catcher / Wrecker Instinct promotion.
5. Implement Junk Flinger clone registration from Lead Flinger.
6. Implement behaviour host: **Chambers[]**, casing/scrap mint, Scrap Pack, %-only damage pipeline.
7. Implement Doobie as Junk exotic (% retune, chamber count 1).
8. Implement Dead Man’s Hand chamber pre-roll + Lucky Bastard/Last.
9. Implement Phantom replay + adjacency copy + chamber tag snapshots.
10. Implement Blood-Rush Junk → reserve dump.
11. Implement Residue once hold-R rule locked.
12. Port Heavy Chamber / Modded Auto / Juiced / Outlaw / Snap Cylinder / economy supports.
13. Register frozen pool; icons/strings; Junk HUD.
14. Balance pass: empty grid + pack loop, pure chamber, pure junk, pure phantom, pure blood-rush, poster Doobie+LB+LL, hybrid freak.
15. Optional: turbocharge parity — mark vanilla Phantom turbo obsolete when Junk Flinger ships adjacency natively.

---

*End Junk Flinger Design Doc v2*
