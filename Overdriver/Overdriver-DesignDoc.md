# Overdriver — Design Document (v1)

> Status: **Design only** — no implementation yet.
> Working title in notes: Accelerator Rework. **Ship name: Overdriver.**
> Template base: `.new.AcceleratorRework` weapon content project.
> Product shape: **separate primary gear** — vanilla Accelerator is left unmodified.

---

## 0. Locked Decisions (2026-08-08)

| Decision | Lock |
|----------|------|
| Product shape | Parallel new primary — vanilla Accelerator untouched |
| Player-facing name | **Overdriver** |
| APIName (working) | `overdriver` |
| GUID (working) | `sparroh.overdriver` |
| Paths | **Cascade / Vector / Payload** |
| Baseline empty-grid DPS | **Already top-tier among base weapons — do NOT buff stock damage** |
| Full-auto | **Baseline** — Full-Auto Trigger card cut |
| Missiles ↔ bees | Same fantasy family — **Payload is one unified ordnance/hive career** |
| Live Wire pairing | **Sacred** — Vector + Payload written for Shock Grenade / Voltaic Cell Wire peaks |
| Hard dep on Shock Grenade / Voltaic Cell | **None** — soft synergy only |
| Empty-grid problem | Not wet noodles — **upgrade gravity is the failure** (missile never-reload mono-build) |
| Hive anti-starvation | **Mix default:** arming scales with burst commitment; reload still pays; Wire/self-damage keeps Payback/Requeening alive under ammo-gen |
| Missile philosophy | Payload owns ordnance career; **light hybrid riders** on Cascade/Vector OK |
| Damage modifier rule | Prefer **% mults** on this pool; avoid flat damage adds |
| Soft crowns | Hybrids allowed; no hard exclusion matrix |
| ~30 upgrades v1 | Exotic shapes larger than others; each exotic same cell count |
| Rename pass | Full new player-facing names (vanilla = DNA only) |
| MycoMod (impl) | IsSandbox |
| Doc scope | Full bible |

---

## 1. High Concept / Fantasy

**Overdriver** is the SAXON sprint-fire shock burster that finally has more than one correct sentence.

Vanilla Accelerator is serviceable at stock — among the highest empty-grid DPS primaries — but the upgrade grid collapses into one chart line:

- **Unstable Recursion + Inertia Accelerant + never-reload**
- Real kill power rides **Oxythane's Promise + Payback** tracking missiles
- Primary bullets become worthless filler
- Full-Auto Trigger is a Rare tax for a fire mode the gun should already have
- Reload / hive / warp / combustive / ruptured-spleen fantasies starve under ammo-gen meta

Overdriver keeps the **already-strong** full-auto shock burst hose and rebuilds the grid around three **peer careers**:

- **Cascade** — the longer you commit, the meaner the burst (bullets grow; dump is a choice)
- **Vector** — speed is the multiplier; body is the capacitor (Live Wire poster home)
- **Payload** — the burst arms the bay; tracking bees, living hive, grenade convert

**One-liner:** *Hold the trigger and the gun gets hungrier — grow the burst, ride the speed, or arm the hive.*

**SAXON marketing blurb (draft):**
  “SAXON OD-2 Overdriver — Personnel-portable burst accelerator for operators who
  refuse to stop moving. Cascade packages grow the volley. Vector packages convert
  ground speed into muzzle authority. Payload bays arm tracking bees mid-spray.
  Compatible with Live Wire body-storm doctrine. (Do not ask Legal about the bees.)”

Optional stingers:
- “If the burst is still small, you let go too early.”
- “Speed is not a buff. Speed is the second magazine.”
- “The bay does not fire missiles. The bay fires bees with opinions.”
- “Full-auto was always the correct trigger. We fixed the paperwork.”

---

## 2. Role & Fantasy in the Arsenal

| Trait | Value |
|-------|--------|
| **Slot** | Primary |
| **Range** | Close–mid shock burster (falloff present; range cards extend) |
| **Role** | Sprint-fire shock volume, burst-growth commitment, speed-scaled skirmish, bee/ordnance bay |
| **Gap filled** | Vanilla Accelerator = strong stock + one upgraded mono-build (missiles + never-reload). Overdriver = three peer upgraded careers on the same strong hose |
| **Synergies** | **Live Wire / self-shock (sacred)**, Higher Voltage, movement employees, Shocklance Subway, Swarm Blitz, speed supports |

**Product shape:** New primary (**Overdriver**). Does **not** replace or patch vanilla Accelerator.

**Not trying to be:** heat-infinite SMG (Cycler), anatomy laser (DMLR), explosive rotary (Siege/Gunship), hover-swarm plant (Hive Launcher), or a mandatory external-mod dependency.

### 2.1 Comparison snapshot

```
Weapon                 Niche                         Overdriver differentiator
---------------------  ----------------------------  ------------------------------------------
Vanilla Accelerator    Shock burst (1D upgrade meta) Parallel pool; peer Cascade/Vector/Payload
Heat Cycler            Energy heat hose              Mag + burst growth + bees; not heat bar
Chaingun               Kinetic MG + spool            Shock bursts; sprint identity; no spool
DMLR                   Anatomy transfer laser        Volume shock / speed / bay — not parts
Hive Launcher          Hover-dive organic swarm      Tracking bees + living hive on a burster
Siege Cannon           Explosive shells / halo       Shock primary; bees not gunship shells
Voltaic Cell / Shock   Live Wire body storm          Soft sacred pairing — no hard dep
```

### 2.2 Why we do not buff empty-grid

Stock Accelerator already leads base-weapon DPS. Raising the floor would:

- Distort arsenal balance against other primaries
- Hide the real problem (upgrade mono-culture)
- Make upgraded boards absurd before path power is tuned

**Success metric is peer upgraded fantasies**, not a stronger brick with no cards.

---

## 3. Design Pillars

1. **Stock hose stays elite** — no empty-grid damage charity; full-auto baseline; shock burst identity preserved.
2. **Upgrade gravity is the rework** — three peer careers; missiles/bees must not be the only DPS noun.
3. **Bullets remain real after upgrades** — Cascade especially; Payload bees *ride* commitment rather than replace the spray.
4. **Full-auto is free** — Full-Auto Trigger deleted as a card.
5. **Acceleration is the shared verb** — stacks, speed, arming cycles; something should always be climbing while you commit.
6. **Live Wire is sacred architecture** — no card requires it; poster Vector+Payload boards feel written for it.
7. **Hive survives ammo-gen** — potency/arming from burst commitment; reload still pays; self-shock/self-bee keep reactive bees alive.
8. **Bees = ordnance noun** — tracking “missiles” and living bees share Payload fantasy language where honest.
9. **Distributed power** — many Std/Rare/Epic carry % damage, burst size, shock appl, speed, economy. Prefer **% mults**; no flat damage adds.
10. **Soft crowns** — hybrids allowed and celebrated; watch double-dip in playtest, don’t ban.
11. **~30 upgrades for v1** — exotic shapes larger; each exotic same cell count; full rename pass.
12. **Failure states stay fun** — overspread recursion, face-tank for Payback, warp into bad geometry, combustive dry nade, bee self-damage greed.

---

## 4. Core Mechanics & Gunfeel

### 4.1 Base gun (no upgrades)

| Trait | Draft / intent |
|-------|----------------|
| Fire mode | **Full-auto** shock burst SMG (vanilla Accel spirit + free full-auto) |
| Damage | **Match vanilla empty-grid authority** — do not raise; slight retune only if full-auto changes cycle math |
| Element | Shock (~vanilla 1.25 spirit) |
| Burst size | ~4 spirit; **grows with continuous fire / bursts in mag** (vanilla Accel grow fantasy) |
| Burst / fire interval | High RoF spirit (~vanilla 857 RPM / 0.07s / burst 0.133s ballpark) |
| Magazine / reserve | ~38 / ~304 spirit (paths mutate) |
| Reload | Standard reload beat |
| Sprint-fire | **ON** baseline (vanilla sacred) |
| Tracking bees / living hive / warp / combustive | **OFF** until upgrades |
| ADS | Normal unless a card says otherwise |
| Model / audio | Borrow Accelerator until custom art |
| Hold-R specials | **None** on baseline (Combustive owns hold-R when equipped) |

**Full-auto note:** Enabling baseline full-auto may slightly change effective DPS vs vanilla tap-burst. Prefer **preserving feel and authority**, not a stealth buff. If full-auto overshoots, shave burst interval / damage **minimally** back to vanilla empty-grid band — never as an excuse to add upgrade floor damage.

### 4.2 Base combat loop

```
Hold M1 → full-auto shock bursts → burst grows while you commit → reload beat
   ↘ sprint-fire freely
   ↘ without crowns: elite stock hose (already wins empty-grid DPS race)
   ↘ Cascade: growth becomes the build; dump / refund / multi-pellet transform
   ↘ Vector: go fast → hit harder; Wire body-storm is the cultural peak
   ↘ Payload: each burst arms bees; hive state / combustive / reactive bay
```

### 4.3 Inputs

| Input | Baseline | With crowns |
|-------|----------|-------------|
| **M1** | Full-auto shock bursts | Unchanged fire mode; path cards mutate burst/bee/speed |
| **RMB / AIM** | Normal ADS | Unchanged unless a card removes ADS |
| **R tap** | Reload | Reload; hive/warp cash-outs may ride completed reload |
| **R hold** | Nothing | **Combustive** owns hold-R when equipped |

### 4.4 Shared tracking (behaviour host — all paths may read)

Host on `OverdriverBehaviour` (names draft):

| Field | Purpose |
|-------|---------|
| `BurstsThisMag` / `CommitmentStacks` | Cascade growth + Payload arming + hive potency |
| `ContinuousFireTime` | Alternate arming signal if burst count is awkward under full-auto |
| `MoveSpeedSample` | Vector damage / size / appl scaling (read player horizontal speed) |
| `ShockSelfActive` | True while local player has full shock / Live Wire-like self state |
| `BeeSelfSwarm` | True while local player is bee-swarmed (Requeening window) |
| `LastBurstSize` | Slipspace refund / Oxythane scale / Spleen dump |
| `HiveCharge` | 0–N arming resource for Payload (see §6.3) |
| `WarpAnchor` | Mini Warp position sample |

### 4.5 Burst growth (baseline identity — not a Cascade exclusive)

Vanilla Accel already grows burst with trigger commitment. Overdriver keeps a **readable baseline grow**:

- Continuous fire / successive bursts increase effective burst size up to a soft cap
- Spread may tick up slightly with growth (readable cost; Cascade leans into it, glue can fight it)
- **Growth alone is not a damage keystone** — stock DPS already elite; growth is gunfeel + path hook

Cascade cards **amplify and pay off** growth (damage per stack, dump, refund).  
Payload cards **spend or mirror** growth into bee arming.  
Vector mostly ignores growth except hybrid riders.

### 4.6 Damage rules (LOCKED)

1. Upgrade damage modifiers are **percentage multipliers** (`damage *= 1.xx`).
2. **No flat damage adds** on this weapon’s upgrade pool.
3. Volume verbs (pellets/burst, bee count, explosion size) and tempo verbs (RoF, reload, refund, speed) are first-class power.
4. Vector speed scaling is **% damage from normalized speed**, not flat.
5. Bee/missile damage scales from **gun damage % pipeline × arming factor**, so gun % cards still matter on Payload boards.

### 4.7 Soft crown matrix (not hard mutex)

| Combo | Behavior |
|-------|----------|
| Any single path crown stack | Full fantasy |
| Cascade + Oxythane rider | Growing burst feeds fatter end-of-burst bees — **poster hybrid** |
| Vector + Payback | Wire self-damage / speed demon reactive bees — **sacred Live Wire poster** |
| Payload hive + Vector | Requeening while zooming; Royal Jelly heals the greed |
| Cascade dump + Combustive | One spicy elemental spleen |
| All three | Allowed; soft taxes / playtest watch on recursion × missile × speed triple-dip |
| Live Wire + anything | Always on-plan — external grenade synergy |

**Crown conflict priority (input only):**
1. Combustive hold-R — owns reload-hold
2. Everything else is passive / fire-path — no M1 steal in v1

---

## 5. Live Wire Architecture (SACRED)

### 5.1 What Live Wire is

Shock Grenade exotic: you become the grenade — periodic self-explosions, move speed bonus, self-shock culture. Voltaic Cell rework keeps Wire as a cultural peak. Accelerator vanilla already orbits this pairing (Inertia + Payback + Royal Jelly + Remote Charging).

### 5.2 Rules

1. **No Overdriver card requires Live Wire** (or Voltaic Cell, or any grenade mod).
2. **Poster boards assume Wire is available** in the player’s grenade kit.
3. Self-shock / electrocute-on-self is the bridge API — works with vanilla Wire, Voltaic Wire, environmental shock, friendly fire shock, etc.
4. Higher Voltage–style “grenade element matches gun” remains a player-facing synergy note, not code coupling.

### 5.3 Per-path Wire dialogue

| Path | Wire dialogue |
|------|----------------|
| **Vector** | Speed → damage (Inertia DNA); self-shock speed stacking; Royal Jelly regen while shocked+moving; Remote Charging mag on electrocute |
| **Payload** | Self-damage / body boom → **Payback bees**; bee self-damage alternate fuel; hive uptime without reload |
| **Cascade** | World shock saturation → Slipspace-style burst refund; commitment while Wire keeps you mobile |
| **Glue** | Feedback Loop, Shocker, Momentum Impact — speed toys that stack with Wire |

### 5.4 Anti-patterns (do not ship)

- Cards that only function with a specific grenade APIName
- Nerfing Payback/Inertia “because Wire is strong” — elevate peers instead
- Making self-damage mandatory without Royal Jelly / heal answers on the same grid

---

## 6. Crowns & Sacred Systems

### 6.1 Path A — CASCADE

**Fantasy:** *The longer I spray, the meaner the burst.*

#### C1. Unstable Cascade — Exotic (keystone)
**Vanilla DNA:** Unstable Recursion — +damage and +spread per burst.

**Overdriver:**
- Each burst in a continuous hold / mag grants a Cascade stack
- Per stack: **+% damage**, slight +spread, optional tiny +shock appl
- Stacks clear on reload **or** after a short fire gap (pick one in impl; prefer **fire gap ~0.4–0.6s** so never-reload still works but stutter-breathing is a skill)
- Spread cost is real — long holds paint rooms, not sniper lanes
- **This is the bullet mythic** — primary projectiles carry the build

Starting targets (playtest): +8–12% damage/stack; soft cap ~8–12 stacks; spread +per stack readable but not instantly unusable.

#### C2. Ruptured Spleen — Exotic (dump crown)
**Vanilla DNA:** Fire one massive burst.

**Overdriver:**
- On activation (draft: **next burst after reload** OR **tap a threshold at max stacks** — prefer **consume all Cascade stacks → one massive burst** so it dialogues with C1)
- Damage/size ∝ stacks consumed (and/or mag fraction)
- Clears stacks (cash-out beat)
- Pair with Three-in-One / multi-pellet for shotgun spleen; pair with Combustive for elemental spleen

**Not:** a dead epic. **Is:** the Cascade execute button.

#### C3. Slipspace Return — Exotic (economy)
**Vanilla DNA:** Slipspace Bullet Transfer — shock stacks before reload → chance to refund last burst; consume stacks on refund.

**Overdriver anti-starvation rewrite:**
- Shock stacks you apply to enemies build **Return charge**
- Chance to refund last burst’s ammo **on burst end** (not only on reload) — so ammo-gen / never-reload still uses it
- Partial stack consume on proc
- Optional: reload with Return charge banked grants a guaranteed mini-refund or RoF blip (reload still feels good)

### 6.2 Path B — VECTOR

**Fantasy:** *Speed is the magazine multiplier.*

#### V1. Inertia Overdrive — Exotic (keystone)
**Vanilla DNA:** Inertia Accelerant — damage scales with move speed; base damage down.

**Overdriver:**
- Keep the trade: modest **base damage % down**, strong **+% damage from normalized speed**
- Sample horizontal speed; cap at a readable max (vanilla Max Speed spirit ~35–41)
- Optional riders at high speed: +bullet size crumb, +shock appl crumb (Size Them Up / Payload Amp DNA folded lightly)
- **Live Wire speed is first-class fuel** — not a bug

Do **not** gut this card to force other paths. Peers must rise to meet it.

#### V2. Chrono Anchor — Exotic (Mini Warp rewrite)
**Vanilla DNA:** Miniaturized Warp Core — on reload, warp to previous position, speed boost, explosion.

**Overdriver:**
- On reload (or optional: on R after N seconds of sprint): warp to anchor, **shock nova**, short speed blitz
- Anchor samples every X m moved or every Y s while weapon drawn
- Nova damage uses gun % pipeline; size modest
- **Combat mobility exotic**, not a DPS dead toy — the speed blitz feeds Inertia Overdrive immediately
- Fun failure: warp into a bad pack / off a ledge

#### V3. Live Capacitor — Exotic (Wire marriage crown)
**New / elevated from Remote Charging + Royal Jelly spirits.**

**Overdriver:**
- While you are shocked (self): **+% damage**, **+move speed crumb**, and **magazine drip or burst ammo crumb** on electrocute pulses
- While shocked **and** moving above a threshold: **regen** scales with speed (Royal Jelly DNA)
- Explicit poster card for Live Wire without naming the grenade in code

If pool pressure is tight, fold Remote Charging into this exotic and keep Royal Jelly as a supporting Epic (see frozen list).

### 6.3 Path C — PAYLOAD

**Fantasy:** *The burst is the arming cycle; the bay does the killing — with bees.*

#### Unified bee noun
Player-facing language prefers **bees** for tracking projectiles and living swarms where honest. Internal can still be missile prefabs. Combustive non-elemental → bees stays.

#### Hive anti-starvation (LOCKED mix)

1. **Primary — Arming:** each burst fired (or commitment stack) adds **HiveCharge**. End-of-burst tracking bees and hive potency scale with HiveCharge / burst size.
2. **Secondary — Reload still pays:** Awaken / Sweet Heavens / Warp-style cash-outs on reload gain **bonus** from HiveCharge (reload is a style, not a corpse).
3. **Wire bridge:** self-damage and self-bee damage feed **Payback** and maintain **Requeening** state under never-reload.
4. **No second hold-R** for hive in v1 — Combustive owns hold-R.

#### P1. Oxythane Apiculture — Exotic (end-of-burst bees)
**Vanilla DNA:** Oxythane's Promise — tracking missile end of each burst; damage/size scale with burst size.

**Overdriver:**
- End of each burst: fire **tracking bee(s)**
- Damage / explosion size / count scale with **burst size + HiveCharge + gun % damage**
- Must not out-DPS a committed Cascade hose alone without investment — arming and % cards required for mythic bay
- Hybrid: Cascade stacks feed fatter bees (rider, not free win)

#### P2. Payback Swarm — Exotic (reactive bees)
**Vanilla DNA:** Payback — on taking damage, chance to fire tracking missile.

**Overdriver:**
- On damage taken: chance to launch tracking bee(s)
- Chance / damage / count improve while **shocked** or **bee-swarmed** (Wire + hive dialogue)
- Explosion size readable; tracking radius vanilla spirit
- **Sacred Live Wire card** — body-storm is free proc fuel
- Bee self-damage (Awaken / Sweet Heavens) is valid alternate fuel

#### P3. Combustive Rounds — Exotic (grenade convert)
**Vanilla DNA:** Hold R consume grenade → rest of mag becomes explosive bullets of grenade element; non-elemental → bees.

**Overdriver:**
- Keep hold-R identity exactly in spirit
- Explosive bullet damage uses gun % pipeline; explosion size retuned so it is a **real Payload crown**, not a fun weakling
- Element from grenade matters (Shock Wire boards stay coherent; Fire/Acid become opt-in via nade choice)
- Non-elemental → bees keeps Payload noun unity
- Dry grenade = fun failure (empty click / no convert)

#### P4. Requeening — Epic or Exotic-lite (hive state engine)
**Vanilla DNA:** While swarmed by bees: +damage, +move speed, −gravity.

**Overdriver:**
- Keep state engine
- Ensure bee-swarm uptime has sources that **do not require empty-mag reload** (burst-threshold bee apply-to-self crumb, Payback near-miss, Wasp overapply, etc.)
- Speed feeds Vector hybrids; damage feeds all paths

**Pool note:** If exotic count is tight, Requeening stays **Epic engine** under Payload with P1–P3 as the three exotics; Awaken/Sweet Heavens support the state.

### 6.4 Boundary Incursion — Oddity

Grid grow. Universal keep. Not a damage staple.

---

## 7. Upgrade Paths (gravity wells — hybrids intended)

### Path A — CASCADE
**“The longer I spray, the meaner the burst.”**

- **Spine:** commitment stacks, bullet damage, multi-pellet, dump, shock-refund
- **Crowns:** Unstable Cascade; Ruptured Spleen; Slipspace Return
- **Supports:** Three-in-One, Number 5, spread control, shock appl for refund, kill tempo
- **Hybrid hooks:** stacks → Oxythane bee size; Spleen + Combustive; refund keeps Cascade rolling under Wire mobility

### Path B — VECTOR
**“Speed is the magazine multiplier.”**

- **Spine:** move speed → damage, sprint verbs, self-shock capacitor, warp reposition
- **Crowns:** Inertia Overdrive; Chrono Anchor; Live Capacitor
- **Supports:** Feedback Loop, Shocker, Momentum Impact, Size Them Up, Royal Jelly (if not folded), sprint ammo
- **Hybrid hooks:** Wire + Payback; speed + Requeening; warp blitz → Inertia spike

### Path C — PAYLOAD
**“Arm the bay. Loose the bees.”**

- **Spine:** end-of-burst bees, reactive bees, living hive, grenade convert
- **Crowns:** Oxythane Apiculture; Payback Swarm; Combustive Rounds
- **Supports:** Wasp Gun, Requeening, Awaken The Hive, Sweet Heavens, Honey Sweet, Queen's Cannons, Shake The Nest
- **Hybrid hooks:** Wire body-storm; Cascade arming; Vector speed while Requeening

### Path × verb matrix

```
                  CASCADE               VECTOR                PAYLOAD
Bullets           mythic growth/dump    speed-scaled hose     arming cycle
Bees (tracking)   hybrid rider          hybrid rider          primary verb
Living hive       optional              speed while swarmed   career
Reload            refund / spleen setup warp nova cash-out    hive cash-out bonus
Self-shock        mobility to keep hold sacred fuel           Payback fuel
Never-reload      first-class           first-class           first-class (arming ≠ reload)
```

---

## 8. Full Upgrade List

Rarity guide: Standard / Rare / Epic / Exotic / Oddity  
Cell rule: Exotic shapes larger than others; all Exotics same cell count.  
Player-facing names below. API names at implementation.  
Vanilla names are DNA only — full rename pass.

------------------------------------------------------------------------------
PATH A — CASCADE                                              [target ~10]
------------------------------------------------------------------------------

A1. Unstable Cascade — Exotic (keystone)
    Commitment stacks: +% damage and +spread per burst in a hold.
    Stacks decay on fire gap (preferred) or reload. Bullet mythic.

A2. Ruptured Spleen — Exotic (dump)
    Consume Cascade stacks (or dump mag fraction) into one massive burst.
    Damage/size ∝ stacks consumed.

A3. Slipspace Return — Exotic (economy)
    Shock applied to enemies builds Return charge.
    Chance to refund last burst ammo on burst end; partial consume.
    Reload with banked charge feels extra good (bonus, not gate).

A4. Triple Bore — Epic (Three-in-One rewrite)
    +Pellets per shot; −accuracy; retune % damage so multi-pellet is a real
    clear branch (vanilla’s heavy damage cut made it a trap — fix the trap,
    don’t buff empty-grid).

A5. Number Five — Rare
    Every burst fires 5 bullets (vanilla Number 5, To Go spirit).
    Pair with Cascade stacks and Oxythane bee scaling.

A6. Recursion Jacket — Epic
    Softens spread gain per Cascade stack; slight −max stacks or −damage/stack
    as trade. Lets Cascade snipe mid-range without deleting the wild identity.

A7. Saturate Latch — Rare
    Fully shocking a target grants bonus Cascade stacks or pauses stack decay
    briefly. Wire-world / shock-focus glue inside Cascade.

A8. Overburst Capacitor — Rare
    At max Cascade stacks: +RoF crumb and +shock appl; +spread crumb.
    Rewards riding the redline without being a second keystone.

A9. Kinetic Optimization — Standard (port)
    +% damage, −shock application. Classic trade staple.

A10. Shaved Projectiles — Standard (port)
     +Magazine size.

A11. Flash Charge — Standard (port)
     +Reload speed.

A12. Hair Trigger — Rare (port / retarget)
     On kill: stacking +fire rate and +reload speed. Tempo glue.

------------------------------------------------------------------------------
PATH B — VECTOR                                               [target ~10]
------------------------------------------------------------------------------

B1. Inertia Overdrive — Exotic (keystone)
    −% base damage; +% damage from normalized move speed (cap).
    High-speed crumbs: bullet size / shock appl optional.

B2. Chrono Anchor — Exotic (Mini Warp rewrite)
    Sample anchor while fighting. On reload: warp to anchor, shock nova,
    speed blitz that feeds Inertia.

B3. Live Capacitor — Exotic (Wire marriage)
    While shocked: +% damage, +speed crumb, ammo drip on electrocute pulses.
    While shocked and moving fast: regen scales with speed (Royal Jelly DNA).

B4. Feedback Loop — Rare (port)
    After firing a burst, gain speed for each bullet fired.

B5. Shocker — Rare (port)
    On kill: stacking move speed bonus.

B6. Momentum Impact — Rare (port)
    Direct hits deal +% damage while sprinting.

B7. Size Them Up — Rare (port)
    Bullet size increased while sprinting.

B8. Royal Jelly — Epic (if not fully folded into Live Capacitor)
    While holding Overdriver: regen scales with move speed; further increased
    when Shock is fully applied to you.
    If Live Capacitor already owns this, demote Royal Jelly to backlog or
    thin support (+regen only, no damage).

B9. Remote Charging — Epic (if not folded into Live Capacitor)
    Magazine refilled / dripped when you are electrocuted.
    Prefer fold into Live Capacitor for frozen 30; keep as separate if Wire
    boards need a cheaper epic slot.

B10. Sprint Nest — Rare (Shake The Nest DNA, Vector-leaning)
     Generate ammo over time while sprinting.
     If bees on bullets / HiveCharge > 0: faster regen while airborne.

B11. Quick Reset — Rare (port)
     Gain a burst of speed when reloading with an empty magazine.
     Still valid even if rare under ammo-gen — empty reload is a style choice.

B12. Energy Cohesion — Standard (port)
     +Range / falloff.

------------------------------------------------------------------------------
PATH C — PAYLOAD                                              [target ~10]
------------------------------------------------------------------------------

C1. Oxythane Apiculture — Exotic
    End of each burst: tracking bee(s). Damage/size/count scale with burst
    size, HiveCharge, and gun % damage.

C2. Payback Swarm — Exotic
    On damage taken: chance to fire tracking bee(s).
    Improved while shocked or bee-swarmed.

C3. Combustive Rounds — Exotic
    Hold R: consume grenade, convert rest of mag to explosive bullets of that
    element. Non-elemental → bees. Retuned to peer crown power.

C4. Requeening — Epic (engine)
    While bee-swarmed: +% damage, +move speed, −gravity.
    Uptime sources must include non-reload paths (see §6.3).

C5. Awaken The Hive — Rare/Epic
    On reload: bee explosion that damages you and nearby enemies.
    Size/bee count scale with bursts fired / HiveCharge this mag.
    Self-damage feeds Payback; swarm feeds Requeening.

C6. Sweet Heavens — Epic
    On reload: healing bee swarm; after short duration bees damage you.
    Heal-then-Payback greed loop; Wire-compatible.

C7. Wasp Gun — Epic
    Bullets apply bees (living hive applicator).

C8. Queen's Cannons — Rare
    Bullets have a small chance to explode on impact (bee-flavored pop).

C9. Honey Sweet — Rare
    Shoot allies with bee bullets to heal them. Thin co-op; solo-safe skip.

C10. Payload Amplifier — Standard (port)
     Bullets apply more elemental effect (shock/bee appl spine).

C11. Bay Loader — Rare (new)
     +HiveCharge gain per burst; slight −bullet damage % (arming tax).
     Makes pure Payload commit to the bay without gutting the hose entirely.

C12. Swarm Fuse — Rare (new)
     At HiveCharge thresholds mid-mag (not only reload): pulse a small bee
     nova around you or the aim point. Anti-starvation living-hive verb.

------------------------------------------------------------------------------
GENERIC / GLUE                                                [target ~6–8]
------------------------------------------------------------------------------

G1. Boundary Incursion — Oddity
    Increases upgrade grid size.

G2. Edge Fault — Contraband (optional parity)
    Grid size. Include only if other reworks ship contraband parity.

G3. Multiversal Thievery — Contraband (optional parity)
    Steal columns. Optional.

G4. Focused Lenses — Standard
    +Falloff start/end (rename of range staple if Energy Cohesion is path-tagged).

G5. Stabilized Rails — Rare
    −Spread; slight −RoF. Fights Cascade wildness and Triple Bore.

G6. Overclocked Bolt — Rare
    +RoF; +spread; slight −damage %. Tempo staple.

G7. Reserve Bins — Standard
    +Ammo capacity.

G8. Hardened Jacket — Rare
    Modest universal +% damage staple (distributed power).

G9. Shock Primer — Standard
    Small +shock application. Feeds Slipspace / saturate world.

G10. Full-Auto Trigger — CUT
     Baseline fire mode. Do not register.

------------------------------------------------------------------------------
CUT / DO NOT PORT AS-IS
------------------------------------------------------------------------------

| Vanilla | Fate |
|---------|------|
| Full-Auto Trigger | **Cut** — baseline |
| Unstable Recursion | → Unstable Cascade |
| Inertia Accelerant | → Inertia Overdrive |
| Oxythane's Promise | → Oxythane Apiculture |
| Payback | → Payback Swarm |
| Miniaturized Warp Core | → Chrono Anchor |
| Slipspace Bullet Transfer | → Slipspace Return |
| Ruptured Spleen | Kept name OK or rename; mechanics elevated |
| Combustive Rounds | Kept; power elevated |
| Requeening / hive suite | Kept spirits; anti-starvation hooks |
| Remote Charging / Royal Jelly | Fold toward Live Capacitor where possible |

---

## 9. Frozen ~30 for v1 implement

Exactly 30 design slots (adjust names freely; keep roles):

### EXOTIC (8) — equal large shapes (LOCKED freeze)
1. Unstable Cascade  
2. Ruptured Spleen  
3. Inertia Overdrive  
4. Chrono Anchor  
5. Live Capacitor  
6. Oxythane Apiculture  
7. Payback Swarm  
8. Combustive Rounds  

Slipspace Return ships as **Epic** in the frozen 30 (still Cascade spine).

**If drop rates demand fewer exotics (trim guide):**
- **To 6:** keep Unstable Cascade, Ruptured Spleen, Inertia Overdrive, Live Capacitor, Oxythane Apiculture, Payback Swarm; demote Chrono Anchor + Combustive to Epic.
- Do not gut Inertia / Payback to “balance Wire”; elevate Cascade/Payload peers instead.


### EPIC (8)
9. Slipspace Return  
10. Triple Bore  
11. Requeening  
12. Sweet Heavens  
13. Wasp Gun  
14. Recursion Jacket  
15. Royal Jelly *(thin if Live Capacitor overlaps — prefer unique: +regen only while moving, no damage)*  
16. Hardened Jacket *or* Overburst Capacitor (pick in impl; prefer Overburst Capacitor)

### RARE (9)
17. Number Five  
18. Saturate Latch  
19. Feedback Loop  
20. Shocker  
21. Momentum Impact  
22. Size Them Up  
23. Awaken The Hive  
24. Queen's Cannons  
25. Bay Loader *or* Swarm Fuse (prefer **Swarm Fuse** for anti-starvation; Bay Loader backlog)

### STANDARD (4)
26. Kinetic Optimization  
27. Shaved Projectiles  
28. Payload Amplifier  
29. Shock Primer  

### ODDITY (1)
30. Boundary Incursion  

### BACKLOG (designed, not in first 30)
- Hair Trigger, Flash Charge, Energy Cohesion, Quick Reset, Sprint Nest  
- Honey Sweet (co-op)  
- Bay Loader (if Swarm Fuse took the rare slot)  
- Remote Charging as separate epic (if un-folded)  
- Stabilized Rails, Overclocked Bolt, Reserve Bins  
- Edge Fault / Multiversal Thievery  
- Recursion Jacket if cut for space (keep if Cascade mid-range feels bad)

---

## 10. Example Builds (mix-and-match encouraged)

### Live Wire Vector poster (sacred)
Inertia Overdrive + Live Capacitor + Feedback Loop + Shocker + Momentum Impact  
+ Payback Swarm + Chrono Anchor  
*Grenade: Live Wire (vanilla or Voltaic). Body-storm → speed → damage; Payback bees on every pulse; warp out when greedy.*

### Cascade bullet storm
Unstable Cascade + Recursion Jacket + Triple Bore + Number Five + Saturate Latch  
+ Slipspace Return + Overburst Capacitor  
*Bullets are the build. Refund keeps the hose up. No bees required.*

### Cascade → bay hybrid
Unstable Cascade + Oxythane Apiculture + Number Five + Payload Amplifier  
+ Slipspace Return  
*Stacks and burst size feed fat end-of-burst bees without abandoning the hose.*

### Payload hive lord
Oxythane Apiculture + Payback Swarm + Wasp Gun + Requeening + Awaken The Hive  
+ Sweet Heavens + Swarm Fuse  
*Arm on bursts; cash out on reload when you choose; Wire or self-bee greed for Payback.*

### Combustive elemental dump
Combustive Rounds + Ruptured Spleen + Triple Bore + Kinetic Optimization  
+ matching elemental grenade (Shock for Wire boards; Fire/Acid for spice)  
*Hold-R convert → spleen the spicy mag.*

### Speed hive freak (all three)
Inertia Overdrive + Requeening + Oxythane Apiculture + Feedback Loop  
+ Live Capacitor + Unstable Cascade (light)  
*Zoom while swarmed; bees and bullets both scale; playtest for triple-dip.*

---

## 11. Economy & Anti-Double-Dip Rules

1. **Empty-grid DPS:** do not raise. Full-auto parity pass only.
2. **Oxythane bees** scale with burst/HiveCharge/gun % — without Payload investment they are a rider, not a delete button.
3. **Payback** is strong with Wire by design; peer paths must clear rooms without it.
4. **Cascade stack damage** and **Inertia speed damage** both % — stacked hybrids are allowed; if broken, add soft diminishing on *combined* bonus above a threshold rather than bans.
5. **Never-reload** is a valid global style; every path must function without forcing empty reload. Reload cash-outs are bonuses.
6. **HiveCharge** gains from shooting, not from standing in reload animation.
7. **Combustive** should not grant free grenade refunds that infinite-loop convert without cost.
8. **Chrono Anchor** nova is utility + Wire fodder, not a third primary DPS crown — keep nova modest.
9. Prefer one Live Capacitor exotic over three fragmented Wire epics that bloat the pool.

---

## 12. Strengths, Weaknesses & Failure States

### Strengths
- Stock hose already elite — rework spends budget on fantasy, not floor
- Live Wire pairing remains a cultural peak
- Three readable upgraded careers
- Bees/missiles unified under Payload without deleting Cascade bullets
- Sprint-fire shock identity preserved
- Hybrids (Cascade bay, Wire Payback, combustive spleen) are intentional

### Weaknesses / fun failure states
- Cascade overspread at long range
- Vector without speed (standing still) feels punished — correct
- Payback greed (face-tank) without Royal Jelly / Capacitor heal
- Chrono Anchor into bad geometry
- Combustive with no grenade charge
- Hive self-damage without heal answers
- Pure Payload under-invested arming (bees tickle; hose still carries — OK)

### Design risks
- Inertia + Wire + Payback remains so strong peers feel fake — **elevate peers, don’t gut Wire poster**
- Full-auto baseline stealth-buffs empty-grid — monitor cycle math
- Too many exotics dilute drops — freeze list trims Slipspace to Epic
- Bee prefab / tracking missile sameness — VFX/audio must sell “bees”
- Ammo-gen meta still might undervalue reload cash-outs — Swarm Fuse + arming carry Payload

---

## 13. Success Criteria / Player Fantasy Checklist

- [ ] Empty-grid Overdriver feels like top-tier stock hose (not buffed into absurdity)
- [ ] Full-auto is baseline; no Full-Auto Trigger card exists
- [ ] A Cascade board kills primarily with bullets and feels mythic at high stacks
- [ ] Ruptured Spleen is a deliberate cash-out people build around
- [ ] A Vector + Live Wire board feels like the weapon’s cultural peak pairing
- [ ] Payback bees proc satisfyingly on Wire pulses without being the only build
- [ ] A Payload hive board works under never-reload / ammo-gen
- [ ] Reload hive/warp cash-outs still feel good when chosen
- [ ] Combustive is strong enough to be a real exotic crown
- [ ] Chrono Anchor is used for fights, not ignored
- [ ] Hybrid Cascade + Oxythane feels intentional
- [ ] No card hard-requires a grenade mod GUID
- [ ] Failure states stay funny (warp fail, self-bee greed, dry combustive)
- [ ] Co-op: Honey Sweet backlog OK; Payback/Oxythane bees help the room

---

## 14. Vanilla Fate Table (complete)

| Vanilla upgrade | Overdriver fate |
|-----------------|-----------------|
| Full-Auto Trigger | **Cut** (baseline) |
| Unstable Recursion | **Unstable Cascade** (Exotic) |
| Inertia Accelerant | **Inertia Overdrive** (Exotic) |
| Oxythane's Promise | **Oxythane Apiculture** (Exotic) |
| Payback | **Payback Swarm** (Exotic) |
| Combustive Rounds | **Combustive Rounds** (Exotic, power up) |
| Miniaturized Warp Core | **Chrono Anchor** (Exotic) |
| Ruptured Spleen | **Ruptured Spleen** (Exotic, stack consume) |
| Slipspace Bullet Transfer | **Slipspace Return** (Epic, burst-end refund) |
| Three-in-One | **Triple Bore** (Epic, retune trap math) |
| Number 5, To Go | **Number Five** (Rare) |
| Requeening | **Requeening** (Epic engine + uptime hooks) |
| Awaken The Hive | **Awaken The Hive** (Rare/Epic, HiveCharge scale) |
| Sweet Heavens | **Sweet Heavens** (Epic) |
| Wasp Gun | **Wasp Gun** (Epic) |
| Queen's Cannons | **Queen's Cannons** (Rare) |
| Honey Sweet | Backlog Rare (co-op) |
| Feedback Loop | **Feedback Loop** (Rare) |
| Shocker | **Shocker** (Rare) |
| Momentum Impact | **Momentum Impact** (Rare) |
| Size Them Up | **Size Them Up** (Rare) |
| Hair Trigger | Backlog Rare |
| Quick Reset | Backlog Rare |
| Shake The Nest | **Sprint Nest** backlog / Vector rare |
| The Royal Jelly of Hazaran | Fold → **Live Capacitor** + thin Royal Jelly epic |
| Remote Charging | Fold → **Live Capacitor** |
| Kinetic Optimization | Standard port |
| Shaved Projectiles | Standard port |
| Flash Charge | Backlog Standard |
| Energy Cohesion | Backlog / glue |
| Payload Amplifier | Standard port |
| Boundary Incursion | Oddity keep |
| Edge Fault | Optional contraband |
| Multiversal Thievery | Optional contraband |

**New cards (no direct vanilla):** Live Capacitor, Saturate Latch, Recursion Jacket, Overburst Capacitor, Bay Loader, Swarm Fuse, Hardened Jacket, Shock Primer, Stabilized Rails, Overclocked Bolt, etc.

---

## 15. Visual, Audio & Thematic Design

### Appearance
- SAXON industrial burster + fungal shock corruption
- Cascade: growing muzzle flash / widening arc trails as stacks rise
- Vector: speed-line residual, shock halo when self-shocked
- Payload: hive-bay vents, bee trail on tracking shots, comb glow at high HiveCharge
- Chrono Anchor: ghost afterimage at anchor point

### Sound
- Full-auto shock staccato (tighter than vanilla tap-burst)
- Stack tick cues (subtle) at Cascade thresholds
- Bee launch chirp distinct from bullet crack
- Warp crack + nova thump
- Combustive convert spool on hold-R

### Flavor
- Bees with opinions (tracking + living)
- Acceleration as corporate virtue / workplace hazard
- Live Wire compatibility stamped in the manual (SAXON is not liable)

---

## 16. Implementation Notes (for later)

- Host state on `OverdriverBehaviour` / `OverdriverUpgradeFlags`
- Clone base: `AcceleratorGun` (template `baseTypeName`) — sprint-fire + burst grow already close
- Hooks:
  - Fire / OnFiredBullet / burst end — Cascade stacks, HiveCharge, Oxythane bee spawn
  - OnDamageTaken (local) — Payback
  - OnDamageTarget — shock stacks for Slipspace; Wasp bee appl
  - Reload complete — Awaken, Sweet Heavens, Chrono Anchor
  - Hold R — Combustive
  - Tick — speed sample, Live Capacitor regen, Sprint Nest
- Bee projectiles: reuse Accelerator missile prefab if available; retarget VFX later
- Multiplayer: same mod all clients; sandbox flag; bee spawns owner-authoritative per vanilla patterns
- No hard BepInDependency on shock grenade mods
- Config knobs: stack caps, speed curve, HiveCharge rates, Payback chance, full-auto interval parity
- Persistence: unique gear id range; CreateUpgrade mod GUID `sparroh.overdriver`

### Registration sketch
```
Plugin.Awake
  Harmony Global.LoadInstance → register Overdriver clone of AcceleratorGun
  PlayerData.OnAwake prefix/postfix persistence
  AddRegisterUpgradesCallback → frozen 30
  Fire/damage/reload hooks
```

---

## 17. Open Tuning Questions (playtest, not design blockers)

1. Full-auto baseline vs vanilla empty-grid DPS band — shave interval or damage if overshooting.
2. Cascade stack decay: fire-gap vs reload-only — **lean fire-gap**.
3. Spleen consume rule: all stacks vs fixed threshold — **lean all stacks**.
4. Exotic count 8 vs 6 for drop feel.
5. Live Capacitor vs separate Remote Charging + Royal Jelly — **lean merged exotic + thin jelly**.
6. Oxythane bee damage curve so Cascade hybrid ≠ mandatory and pure Payload ≠ weak hose.
7. Swarm Fuse threshold numbers for never-reload hive uptime.
8. Whether tracking bees use bee status apply on explosion (Requeening feed) — **prefer yes, small amount**.
9. Chrono Anchor on reload only vs sprint-R optional — **v1 reload only**.
10. Triple Bore % retune targets so it is not vanilla’s trap.

---

## 18. Deliberate Non-Goals

- Do not replace or patch vanilla Accelerator
- Do not buff empty-grid into a new arms race
- Do not hard-depend on Shock Grenade / Voltaic Cell mods
- Do not ban Inertia + Wire + Payback
- Do not keep Full-Auto Trigger as a card
- Do not make reload mandatory for Payload identity
- Do not split missiles and bees into separate paths
- Do not add heat-bar infinite ammo (Cycler owns that)
- Support/co-op stays thin (Honey Sweet backlog)

---

## 19. Changelog (design)

### v1 (2026-08-08)
- Initial full bible
- Ship name: **Overdriver**
- Paths: Cascade / Vector / Payload
- Locks: no empty-grid buff, full-auto baseline, Live Wire sacred, bees=ordnance noun
- Hive anti-starvation mix default
- Frozen 30 with 8 exotics (Slipspace as Epic)
- Vanilla fate table + example builds + impl notes

---

## 20. Review Decisions Locked

- Parallel gear, not a vanilla patch
- Empty-grid DPS: preserve, don’t raise
- Full-auto baseline; Full-Auto Trigger cut
- Cascade / Vector / Payload peer paths
- Missiles spoken as bees; one Payload career
- Live Wire pairing sacred; soft synergy only
- HiveCharge arming from bursts; reload pays bonus; Wire feeds Payback/Requeening
- Payload owns ordnance; light hybrid riders OK
- % damage only on upgrade pool
- Soft crowns; no exclusion matrix
- Combustive remains hold-R exotic
- Mini Warp → Chrono Anchor combat exotic
- ~30 v1; backlog listed
- Doc is full bible for implement reference
