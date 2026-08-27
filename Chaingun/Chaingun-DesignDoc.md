# Chaingun — Design Document (v1)

## 1. High Concept / Fantasy

A SAXON industrial **rotary chaingun primary**. Hold the trigger and barrels spool into a
wall of kinetic rounds. Modest damage per bullet; power is **volume, coverage, and position**.

Baseline is a readable ballistic MG with **light always-on spin-up** and a hungry belt —
no free turret, no free Conference Call, no free flak lattice.

Upgrades fork the gun into three peer fantasies:

  Hellstorm   — true machine gun: suppression, heat literacy, belt economy, shred
  Auto Turret — swap off to **plant the gun**; swap back to **pick it up**
  Warthog     — redline RoF and **Conference Call** origin-displaced volleys

One-liner: Spool up. Own the lane. Drop the gun on swap — pick it up when you come back.

Product shape: New primary weapon (**Chaingun**). Does not replace Gunship Cannon, Cycler,
Friend in a Box, or any vanilla gun.

SAXON marketing blurb (draft):
  “SAXON CG-6 Chaingun — Continuous kinetic denial system for corridor, ramp, and
  airspace discipline. Barrel assembly authorized for operator-carried and
  unattended emplacement per Form 12-CG. Leaving the weapon behind is a feature.
  (Retrieving it is also a feature. Usually.)”

Optional stingers:
  - “The correct amount of bullets is more.”
  - “If you can still hear yourself think, the barrels are not finished spooling.”
  - “Conference Call does not miss. It arrives from somewhere inconvenient.”
  - “Weapon swap is a deployment order. Read the manual. Or don’t. The gun will.”


## 2. Role & Fantasy in the Arsenal

- Slot: Primary
- Range: Mid–long (real falloff; not a room-delete SMG, not a sniper)
- Role: Sustained ballistic suppression / lane ownership / optional self-emplacement
- Gap filled:
  - Cycler = energy SMG hose — close cartridge chatter, not heavy MG
  - Gunship Cannon = explosive rotary with plant-for-RoF spin-up — **boom shells**, not lead
  - Lead Flinger = trigger-spam slug personality — not sustained auto
  - DMLR laser = charge beam execute — sustained *beam*, not ballistic MG
  - Accelerator = shock bursts + mobility — not MG
  - Friend in a Box = grenade deployable ally (mine/turret/support) — **throwable pet**, not “my primary becomes a turret”
  - Hard-Light Constructor = plasma fabricator / paint — architecture toys
  - Plate Launcher = stick/recall plates — constructs, not automatic fire
  - Swarm Launcher = hover-then-dive pellets — organic swarm, not kinetic chaingun
  - Nothing owns “ballistic chaingun → spool literacy → swap-deploy self → Conference Call converging storm”

- Synergies: Allies dump into suppressed packs; turret holds a lane while you objective,
  revive, or heavy; Conference geometry shreds flanks and airborne leap attacks

Not trying to be: Gunship 2.0, Cycler with bigger numbers, Friend-in-a-Box primary,
pure ADS LMG sniper, or infinite free fire without economy.

### 2.1 Comparison snapshot

```
Weapon / kit              Niche                         Chaingun differentiator
------------------------  ----------------------------  ------------------------------------------
Cycler                    Energy SMG hose               Heavy ballistic MG; spool; belt weight
Gunship Cannon            Explosive rotary / plant RoF  Kinetic bullets; swap-deploy not plant-RoF
Lead Flinger              Semi/slug trigger skill       Full-auto sustained volume
DMLR (laser)              Anatomy beam execute          Ballistic suppression, not transfer laser
Friend in a Box           Grenade pet deployable        Your primary relocated on weapon swap
Hard-Light Constructor    Jam + paint architecture      Lead hose + emplacement, not fabricator
Plate Launcher            Stick/recall plates           No plate economy; auto fire
Swarm Launcher            Hover swarm dive              Conference uses displacement DNA only
Heaven Piercer            Draw-bow pin/bleed            Auto spool MG, not charged bow
Needle Carbine            Poison needles + extract      Kinetic volume, not DoT extract
```


## 3. Design Pillars

1. On-hit volume, coverage toys, and position > flat % damage stickers.
2. **Spool is baseline identity** — always-on light ramp to max RoF while holding M1.
3. **Ballistic bullets**, not explosive shells (Gunship keeps the boom niche).
4. Baseline is a complete MG fantasy; **turret and Conference are path-owned**.
5. Three peer paths (Hellstorm / Auto Turret / Warthog); hybrids intended; no anti-synergy matrix.
6. **Turret verb = weapon swap** — deploy on swap-off, recall on swap-back. Not RMB.
7. RMB stays free on baseline; paths may claim vent / alt toys — never deploy.
8. R = reload only — do not overload reload on baseline.
9. ~30 upgrades for v1 ship; exotic shapes larger than others; each exotic same cell count.
10. Turret power budget is sacred — snapshot stats, caps, and recall rules prevent AFK win buttons.
11. Conference Call is **origin displacement toward aim** (BL2-accurate), not homing forks.
12. Fun gimmick first — readable brass storm, co-op lane gifts, deploy plays that feel clever.
13. Industrial SAXON rotary tone (hazard stripes, belt feed, barrel glow) — not magic minigun.


## 4. Core Mechanics & Gunfeel

### 4.1 Base gun

| Trait        | Draft / intent                                                         |
|--------------|------------------------------------------------------------------------|
| Fire mode    | Full-auto ballistic; **light always-on spin-up** to max RoF            |
| Damage       | Low–mid per bullet — DPS lives in sustained uptime + upgrade toys      |
| Range        | Mid–long; readable falloff so it is not a panic SMG                    |
| Mag/reserve  | Large mag, hungry reserves — reload is a real beat                     |
| Projectile   | Fast kinetic tracers (near-hitscan feel OK; slight travel preferred)   |
| ADS / RMB    | ADS optional tightener; **RMB unbound** on baseline                    |
| Handling     | Heavy: soft move penalty while at high spool; hip-fire usable          |
| Model/audio  | Multi-barrel rotary, belt/box mag, spool whine, brass storm            |

Draft firefeel band (VALIDATE IN PLAYTEST):
- Idle / first-shot RoF: deliberately sluggish (~6–8 rps mental start)
- Max spool RoF: true MG hose (~12–18 rps mental target — above Cycler weight, below cartoon)
- Spool time 0→max: ~0.6–1.0s hold (light; not Gunship plant tax)
- Spool decay when trigger released: ~0.25–0.45s to idle
- Mag: ~80–120
- Reserves: hungry relative to mag (forces belt awareness)
- Per-bullet damage: modest; Heavy Tips / caliber cards are the chunky fork
- hitForce: low baseline (suppression is upgrade-owned, not ragdoll hose)

### 4.2 Spool (baseline — always on)

While holding M1:
  spool01 = move toward 1.0 at spoolUpRate
  fireInterval effective = lerp(idleInterval, maxInterval, spool01 curve)

On release M1:
  spool01 = move toward 0.0 at spoolDownRate

Notes:
- First bullets are slower / slightly wider; full storm rewards commitment
- Visual/audio must sell spool (barrel RPM, whine pitch, tracer density)
- **Plant-for-higher-max-RoF** is upgrade-owned (Hellstorm / hybrid) — do not clone
  Gunship Firmly Planted as free baseline kit
- Soft move mult at high spool is OK; hard root is upgrade-owned

### 4.3 Inputs

| Input        | Baseline role                         | Upgraded claims                                      |
|--------------|---------------------------------------|------------------------------------------------------|
| Hold M1      | Full-auto; spool climbs               | Redline, Conference displacement, suppression stacks |
| RMB          | **Unbound**                           | Hellstorm Vent / optional Warthog dump (not deploy)  |
| R            | Reload only                           | Reload only (no baseline hold-R overclock)           |
| Weapon swap  | Normal swap                           | **Auto Turret path:** swap-off = deploy; swap-back = recall |
| Heavy        | Normal heavy equip                    | Swap-off to heavy also deploys chaingun if path on   |

### 4.4 Baseline combat loop (zero upgrades)

```
M1 hold → spool climbs → kinetic tracers fill the lane
   → track packs / prioritize threats
   → release to reposition (spool decays)
   → R when dry
   → weapon swap is normal (no turret)
   → RMB does nothing
```

Skill without upgrades: burst-spool discipline, tracking, not dumping the belt into empty air,
managing reload windows, owning mid-range lanes.

### 4.5 What baseline does NOT include

- No deployable turret
- No Conference Call displacement
- No heat / overheat brick
- No flak lattice / anti-air curtains
- No multi-pellet shotgun chaingun
- No infinite ammo
- No hard root while firing
- No explosive shells
- No Friend-style ally AI pet
- No RMB power

Those are path-, exotic-, or unlock-owned.


## 5. Shared Framework Vocabulary

Upgrades speak these verbs. Baseline owns Spool + kinetic full-auto only.

### 5.1 Spool
- Fire-rate ramp 0→max while holding M1; decays on release
- Baseline always-on light curve
- Cards may: faster spool, higher max RoF, slower decay, plant-bonus max, redline past 1.0

### 5.2 Heat (Hellstorm-owned resource)
- Optional path meter built by sustained fire at high spool
- Not a baseline brick — zero Hellstorm cards = no heat gameplay
- Payoffs: vent burst, suppression amp, redline risk, DR-while-hot crumbs
- Overheat failure should stutter or force vent — not brick the whole build silently

### 5.3 Deploy (Auto Turret-owned)
- Chaingun becomes a world turret when you **swap off** it
- Recalled when you **swap back** to it
- First Auto Turret upgrade enables the system (Paint-unlock analogue)
- Turret uses a **snapshot** of gun stats + eligible upgrades at deploy time
- Distinct from FriendinaBox grenade pet and from Gunship stand-still spin tax

### 5.4 Conference (Warthog-owned)
- BL2 Conference Call DNA via Swarm-like **fire-origin displacement**
- Each pellet/bullet spawns from a random offset origin (left/right/above/around)
  then flies **toward the aim point** (converging storm)
- Not homing forks to extra enemies (unless a later hybrid card explicitly adds seek)
- Close range = murderous collapsing cone; long range = still converges on reticle
- Damage split rules apply when multi-pellet Conference is active

### 5.5 Lattice / Flak (Warthog crown / hybrid spice)
- Tracers leave short-lived segments or burst clouds that punish crossings / airborne
- Anti-air fantasy without being a separate AA weapon
- Not a third equal pillar in v1 naming — lives under Warthog crowns + backlog

### 5.6 Suppression
- Volume-based soft CC / accuracy break / stagger crumbs on sustained hits
- Hellstorm spine; never permanent stunlock on bosses

### 5.7 Belt
- Mag + reserve economy fantasy
- Cards: bigger belts, partial reload-while-spooling, reserve siphon, empty-mag drama


## 6. Auto Turret System (path spine — hybrid-friendly)

### 6.1 Unlock ladder (LOCKED)

| Tier | Source                            | What you get                                      |
|------|-----------------------------------|---------------------------------------------------|
| 0    | Baseline                          | Normal weapon swap; no deploy                     |
| 1    | **Any first Auto Turret upgrade** | Swap-off deploys; swap-back recalls               |
| 2    | Turret rares / epics              | AI, arc, ammo rules, durability, leash            |
| 3    | Crowns / key epics                | Dual presence, Conference inheritance, smart prio |

### 6.2 Deploy / recall loop (LOCKED)

```
[Chaingun equipped] + [DeployUnlocked]
    → Player swaps OFF chaingun (other primary / heavy / valid gear swap)
    → Chaingun DEPLOYS at plant point as live turret
    → Player now uses the gear they swapped to
    → Turret auto-fires using snapshot stats + eligible upgrades
    → Player swaps BACK to chaingun
    → Turret RECALLS into hands (pickup / fold-in)
```

### 6.3 Plant point (draft)

- Default: player feet + forward bias (short raycast to ground/surface)
- If no valid ground: deploy fails with whiff feedback; swap still occurs (no softlock)
- Optional epic: wall/ceiling mount, short air tripod
- Facing: toward look direction at deploy instant

### 6.4 What you hold while deployed

- The gear you swapped to — chaingun is **on the ground**, not a dual-wield ghost
- Dual-presence (you keep a backup barrel / remote fire) is **crown/epic-owned**, not default
- Default is the honest fantasy: leave the gun, do something else, come back

### 6.5 Turret combat rules (draft)

| Param              | Draft                              | Intent |
|--------------------|------------------------------------|--------|
| Fire mode          | Auto-acquire + fire in arc         | Holds lane |
| Targeting          | Nearest threat in forward cone     | Readable |
| Arc                | ~90–120° yaw, limited pitch        | Not omniscient sphere |
| Range              | ≤ player effective range           | No sniper turret free |
| RoF                | Snapshot spool rules (starts cold or warm — tune) | |
| Damage             | Snapshot × turretDamageMult (≤1.0 default) | Anti-AFK budget |
| Ammo               | Shared belt with player mag/reserves OR local belt copy — **pick one at impl; prefer shared** | |
| Duration           | Until recall / death / mission transit / optional max lifetime | |
| HP                 | Optional low HP; destroyed → forced free recall CD | |
| Owner              | Deploying player authority         | MP |

Sacred budget rules:
- Default turretDamageMult < 1.0 so planted gun ≠ free second perfect player
- No baseline dual-wield (player + full turret both at 100%)
- Bosses: turret helps; does not hard-tank or perma-stunlock
- Forced recall on owner downed / scene transition / unequip edge cases

### 6.6 Swap edge cases

| Case | Behavior |
|------|----------|
| Swap off without DeployUnlocked | Normal swap |
| Swap off with deploy already active | Should not happen (gun is deployed); ignore / recall-first |
| Swap to chaingun | Recall |
| Swap chaingun → heavy → other | Stay deployed until chaingun re-selected |
| Player death | Forced recall + clear world instance |
| Mag empty while deployed | Turret dry-clicks / auto-reload rules per cards |
| Two chainguns (future) | Out of scope v1 |

### 6.7 What Deploy is NOT

- Not RMB place
- Not Friend in a Box ally unit / multiplayer-fake pet
- Not Gunship Firmly Planted stand-still RoF tax
- Not permanent map turret
- Not baseline kit


## 7. Conference Call System (Warthog spine)

### 7.1 BL2-accurate behavior (LOCKED)

Each shot (or each pellet in a multi-bullet shot):
1. Choose a random **origin offset** in a ring/box around the true muzzle
   (left, right, above, slight behind-lateral — readable, not behind camera junk)
2. Set projectile start position = muzzle + offset (world space, clamped)
3. Set projectile direction = (aimPoint - startPosition).normalized
   (with optional tiny spread after convergence vector)
4. Fire

Result: bullets **arrive from the wings** and **converge on the aim point**.

```
Normal:       muzzle ──────────────────────────► aim point

Conference:   (offset L) ───────────────────────► aim point
              (offset R) ───────────────────────► aim point
              (offset Up) ──────────────────────► aim point
```

### 7.2 Mycopunk analogue

- Implementation DNA: **Swarm Launcher-style fire-origin displacement** before flight
- Identity divergence: Swarm hovers then dives; Conference is **immediate kinetic**
  displaced shots with no hover phase
- Do **not** clone Swarm hover/dive/ally-breeding fantasy

### 7.3 Gunfeel targets

- Close range: collapsing murder cone — flanks and faces both eat lead
- Mid range: signature “shots from nowhere” readability with tracer paths
- Long range: still usable; convergence keeps it honest on the reticle
- Audio: slight stereo whoosh variance per offset; not identical muzzle slap every round

### 7.4 Damage / economy rules (draft)

- When Conference adds extra pellets: total damage budget splits (e.g. 3 pellets @ ~40–50% each)
  so it is coverage, not free ×3 DPS
- Single-bullet displaced origin (no extra pellets) can be an earlier card — pure geometry
- Deployed turret may inherit Conference only if a hybrid/crown says so

### 7.5 What Conference is NOT

- Not baseline
- Not “bullets home to 2 extra enemies” (that is a different card if ever added)
- Not Lagrange Halo orbit-around-player (Gunship exotic — avoid clone)
- Not Swarm hover cloud


## 8. Upgrade Paths (gravity wells — hybrids intended)

### Path A — HELLSTORM (base machine gun)
“The correct amount of bullets is more.”

- Spine: spool mastery, heat/vent, suppression, belt economy, shred vs packs,
  DR-while-firing crumbs, accuracy settle over sustained fire
- Clear vs ST: clear native; ST via focused suppression windows and caliber cards
- Hybrid hooks: heat redline feeds Warthog; planted turret inherits suppression aura

### Path B — AUTO TURRET (swap-deploy / swap-back pickup)
“Leave the gun. Take the objective.”

- Spine: Deploy unlock on first card, AI, arc, ammo rules, durability, leash,
  smart priority, dual-presence crowns, Conference inheritance hybrids
- Clear vs ST: lane clear while you do tasks; ST via focused fire + player heavy/other gear
- Hybrid hooks: wants Hellstorm belt so turret does not dry; wants Warthog Conference geometry

### Path C — WARTHOG (extreme RoF / Conference geometry)
“Safety wire cut. Feed open.”

- Spine: higher max spool, faster spool, multi-pellet, Conference displacement,
  lattice/flak crowns, dump-mag payoffs, redline past heat
- Clear vs ST: clear native; ST weaker unless caliber hybrids
- Hybrid hooks: turret that Conferences; Hellstorm heat as redline fuel

### Path × verb matrix

```
                 HELLSTORM              AUTO TURRET              WARTHOG
Spool            mastery / settle       snapshot at deploy       max RoF / redline
Heat             core fantasy           optional shared vent     redline fuel
Deploy           suppression aura gift  core fantasy             Conference inherit
Conference       —                      hybrid inherit           core fantasy
Lattice/Flak     —                      air-denial hybrid        crown spice
Suppression      core fantasy           turret aura              volume byproduct
Belt             core fantasy           shared ammo rules        dump drama
RMB claim        Vent                   unbound (swap is verb)   optional dump
Swap claim       —                      Deploy / Recall          —
```


## 9. Crowns & Sacred Cows

### Walking Barrage — Exotic (Hellstorm crown)
- Sustained fire at high spool builds **Suppression** on targets (soft accuracy break /
  attack hitch / move crumb — boss-safe).
- At full Suppression stacks on a target, next volley window deals bonus shred damage.
- Readable: tracer weight + enemy flinch grammar.
- The “own the lane with volume” payoff — not a DoT identity.

### Redline Belt — Exotic (Hellstorm crown)
- Past a heat threshold, max RoF climbs further (redline) and tracers intensify.
- Overheat: forced vent stutter OR self-damage crumb OR accuracy collapse (pick one primary
  failure at impl; prefer forced vent with dramatic audio).
- RMB **Vent** (if this crown or Vent card equipped): dump heat in a forward cone burst
  (modest damage + clear heat).
- Infinity-Burn-adjacent fantasy without free infinite ammo unless a separate card says so.

### Emplacement Doctrine — Exotic (Auto Turret crown)
- Mode-defining deploy exotic: greatly improves deployed turret (damage mult toward parity,
  wider arc, smarter target priority: elites/cores bias).
- On deploy, short **spool gift** so turret does not wake ice-cold.
- On recall, refund a crumb of mag OR grant brief player spool headstart.
- Still respects budget caps — not a full second perfect you.

### Fireteam Split — Exotic (Auto Turret crown)
- **Dual presence lite:** while deployed, you may fire a reduced-output “umbilical” pattern
  from a shoulder/hip ghost OR your swapped gear gains a small chaingun assist (pick one
  readable fantasy at impl — prefer: deployed gun full auto + you keep normal swapped gear
  with a passive assist aura, not full dual miniguns).
- Alternative read (preferred if simpler): while deployed, **kills by either you or turret**
  refresh turret duration / feed shared belt efficiency.
- Must not equal “two full chainguns forever.”

### Conference Protocol — Exotic (Warthog crown)
- Enables full Conference Call: multi-pellet shots with **random origin displacement**
  converging on aim point (BL2-accurate).
- Damage split across pellets; coverage king.
- Tracers must sell side-origins (stereo + visible paths).
- Deployed turret inherits Conference **only if** this exotic is equipped (hybrid bait).

### Sky Lattice — Exotic (Warthog crown)
- Tracers leave short-lived **lattice segments** along flight path (0.4–0.8s).
- Enemies crossing segments take chip damage; **airborne / leap** targets take bonus.
- Anti-air bullet lattice fantasy without a separate AA weapon.
- Cap active segments; oldest despawn — no map-wide wire farm.

Sacred cows (do not cut without rewriting identity):
- Baseline full-auto ballistic + light always-on spool
- No baseline turret / Conference / heat brick
- Turret = swap-off deploy / swap-back recall (not RMB)
- First Auto Turret upgrade unlocks Deploy
- Conference = origin displacement toward aim (not homing multi-target default)
- Ballistic, not explosive shells
- Three peer paths; hybrids OK
- ~30 upgrades; equal large exotic shapes
- Not Friend pet; not Gunship boom; not Cycler reskin


## 10. Full Upgrade List (~30 ship + backlog)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity  
Cell rule: Exotic shapes larger than others; all Exotics same cell count.  
Player-facing names below. API names assigned at implementation.

------------------------------------------------------------------------------
PATH A — HELLSTORM
------------------------------------------------------------------------------

A-EX1. Walking Barrage — Exotic (crown)
       Sustained high-spool fire applies Suppression; full stacks open shred window.

A-EX2. Redline Belt — Exotic (crown)
       Heat redline RoF; overheat vent drama; optional RMB Vent cone.

A-EP1. Suppressive Doctrine — Epic
       +Suppression apply rate; slight −damage to non-suppressed (focus tax optional — tune).

A-EP2. Barrel Settle — Epic
       Spread decreases over time while firing at high spool (Adaptive Stabilizers DNA,
       kinetic — not Gunship exclusive fantasy theft if numbers differ).

A-EP3. Hold-Fast Plating — Epic
       Gain DR while firing at high spool; soft move penalty increases slightly.

A-EP4. Linked Belts — Epic
       +Magazine size and +reserves; reload slightly slower.

A-RA1. Spool Governor — Rare
       Faster spool-up; slightly lower max RoF (or neutral max — tune).

A-RA2. Thermal Mass — Rare
       Heat builds slower; vent window longer if heat path active. Harmless if no heat cards.

A-RA3. Tracer Weight — Rare
       +Damage, −bullet speed slightly / +falloff honesty (chunky tracers).

A-RA4. Shredder Tips — Rare
       +Limb damage; slight −shell damage (pack shred bias).

A-ST1. Belt Extension — Standard
       Minor +magazine size.

------------------------------------------------------------------------------
PATH B — AUTO TURRET
------------------------------------------------------------------------------

B-EX1. Emplacement Doctrine — Exotic (crown)
       Stronger turret, spool gift on deploy, recall refund crumb.

B-EX2. Fireteam Split — Exotic (crown)
       Dual-presence lite / shared kill feed rules while deployed (see §9).

B-EP1. Sentry Arc — Epic
       +Turret yaw/pitch arc and acquire range.

B-EP2. Priority Latch — Epic
       Turret prefers elites / marked / last player target (last-hurt bias).

B-EP3. Shared Feed — Epic
       Efficient shared ammo: turret shots cost less reserve OR partial regen while deployed.

B-EP4. Hardpoint Anchor — Epic
       Turret gains HP / damage resist; destroyed turret imposes shorter recall CD.

B-RA1. Quick Emplace — Rare
       Faster deploy fold-out; shorter deploy whiff recovery.

B-RA2. Recall Impulse — Rare
       On recall: brief player spool headstart + small speed crumb.

B-RA3. Tripod Grip — Rare
       Better plant validity (steeper ground / short wall mount).

B-RA4. Watchdog Optics — Rare
       Turret acquires faster; slight +turret accuracy.

B-ST1. Field Latch — Standard
       First Turret card tier: enables Deploy if not yet enabled; minor +turret duration crumb.
       (Any Auto Turret card enables Deploy — Field Latch is the cheap key standard.)

Note: **Field Latch** and all other Auto Turret cards share the rule: on Apply, set DeployUnlocked.

------------------------------------------------------------------------------
PATH C — WARTHOG
------------------------------------------------------------------------------

C-EX1. Conference Protocol — Exotic (crown)
       Full Conference Call multi-pellet origin-displacement converging on aim.

C-EX2. Sky Lattice — Exotic (crown)
       Tracer lattice segments; bonus vs airborne / leap; crossing chip.

C-EP1. Safety Wire Cut — Epic
       +Max spool RoF; +spread slightly; heat builds faster if heat present.

C-EP2. Lateral Feed — Epic
       Enables single-bullet origin displacement (Conference lite) without full multi-pellet.
       Stacks into Conference Protocol (wider offset ring / more pellets).

C-EP3. Dump Valve — Epic
       On empty mag: final burst of N displaced rounds (mini Conference dump).

C-EP4. Collapsing Pattern — Epic
       Conference offsets start wide and tighten toward mag end (or reverse — playtest).
       Readable “storm closes in” fantasy.

C-RA1. Hot Spool — Rare
       Faster spool-up and slightly higher max RoF; +heat gain if heat path active.

C-RA2. Twin Link — Rare
       +Bullets per shot (2) with damage split; pre-Conference volume card.

C-RA3. Brass Hurricane — Rare
       +Fire rate, −per-bullet damage slightly.

C-RA4. Wing Tracer — Rare
       Displaced origins bias harder left/right (wider stage); slight −long-range ease.

C-ST1. Loose Bolts — Standard
       Minor +max spool RoF crumb.

------------------------------------------------------------------------------
GENERIC / GUNFEEL
------------------------------------------------------------------------------

G-RA1. Heavy Tips — Rare
       +Damage, +tracer size, −fire rate slightly.

G-RA2. Deep Box — Rare
       +Magazine size, −reload speed slightly.

G-RA3. Range Gate — Rare
       +Effective range / falloff start.

G-ST1. Speed Loader — Standard
       +Reload speed.

G-ST2. Spare Crates — Standard
       +Ammo reserves.

G-ST3. Muzzle Brake — Standard
       −Recoil / tighter base spread.

G-ST4. Collimator Rail — Standard
       Slight +ADS tightness / hip-fire group (if ADS exists).

G-OD1. Boundary Incursion — Oddity
       Increases upgrade grid size.

------------------------------------------------------------------------------
FROZEN 30 FOR V1 SHIP
------------------------------------------------------------------------------

EXOTIC (6)
  1  Walking Barrage
  2  Redline Belt
  3  Emplacement Doctrine
  4  Fireteam Split
  5  Conference Protocol
  6  Sky Lattice

EPIC (8)
  7  Suppressive Doctrine
  8  Barrel Settle
  9  Sentry Arc
 10  Priority Latch
 11  Shared Feed
 12  Safety Wire Cut
 13  Lateral Feed
 14  Hold-Fast Plating

RARE (10)
 15  Spool Governor
 16  Tracer Weight
 17  Shredder Tips
 18  Quick Emplace
 19  Recall Impulse
 20  Watchdog Optics
 21  Hot Spool
 22  Twin Link
 23  Heavy Tips
 24  Deep Box

STANDARD (5)
 25  Belt Extension
 26  Field Latch
 27  Loose Bolts
 28  Speed Loader
 29  Spare Crates

ODDITY (1)
 30  Boundary Incursion

------------------------------------------------------------------------------
BACKLOG (designed, not in first 30)
------------------------------------------------------------------------------

Hellstorm
- Linked Belts (if cut from epic freeze — re-add)
- Thermal Mass
- Infinite Storm (Infinity Burn DNA: infinite ammo, self-heat/kinetic self-risk)
- Planted Roots (stand-still max RoF gift — careful vs Gunship Firmly Planted)
- Core Rattle (bonus vs cores while suppressed)
- Ally Suppressive Gift (near allies gain accuracy when you suppress)

Auto Turret
- Hardpoint Anchor
- Tripod Grip
- Remote Trigger (RMB pings turret focus target — only if RMB still free)
- Ammo Umbilical (player reload also tops turret belt)
- Decoy Frame (enemies prefer shooting turret)
- Dual Hardpoint (two deploys — expensive, probably never for v1)
- Swap to grenade also keeps deploy (QoL)

Warthog
- Dump Valve
- Collapsing Pattern
- Brass Hurricane
- Wing Tracer
- Full homing Conference hybrid (explicitly different card)
- Mag Dump Alt (hold RMB dump accelerated belt — only if not conflicting Vent)
- Ricochet Conference (displaced shots bounce toward aim)

Generic
- Range Gate, Muzzle Brake, Collimator Rail
- Element tips (Fire/Shock/Acid) — keep off identity unless needed
- True ballistic drop card


## 11. Example Builds

Lane owner (Hellstorm)
  Walking Barrage + Redline Belt + Suppressive Doctrine + Barrel Settle
  + Hold-Fast Plating + Tracer Weight + Belt Extension
  Spool up, suppress the pack, redline the priority target, vent when screaming.

Objective buddy (Auto Turret)
  Emplacement Doctrine + Fireteam Split + Sentry Arc + Priority Latch
  + Shared Feed + Field Latch + Quick Emplace + Recall Impulse
  Plant on swap, do the console / revive / heavy work, swap back clean.

Conference butcher (Warthog)
  Conference Protocol + Sky Lattice + Safety Wire Cut + Lateral Feed
  + Twin Link + Hot Spool + Loose Bolts + Heavy Tips
  Displaced storm collapses on the reticle; lattice punishes leaps.

Hybrid showcase (recommended trailer build)
  Emplacement Doctrine + Conference Protocol + Walking Barrage
  + Shared Feed + Lateral Feed + Priority Latch + Barrel Settle
  Plant a Conference turret, run the flank, suppress what moves.

Redline emplacement
  Redline Belt + Emplacement Doctrine + Safety Wire Cut + Shared Feed
  + Hot Spool + Hold-Fast Plating
  Heat drama on body; turret holds while you vent and reposition.


## 12. Economy & Tuning Rules of Thumb

- Power budget lives in uptime, coverage, and deploy plays — not raw per-bullet DPS.
- Per-bullet damage stays modest; Heavy Tips / Tracer Weight are the chunky forks.
- Spool 0.6–1.0s to max: if it feels like a tax, shorten; if instant hose, lengthen.
- **Turret default damage mult < 1.0**; Emplacement Doctrine approaches parity, does not exceed dual-wield fantasy.
- Shared ammo preferred: empty belt is a real joint failure state for player + turret.
- Conference multi-pellet must split damage — coverage, not free ×N DPS.
- Lateral Feed alone should feel like a cool geometry toy before full Conference Protocol.
- Suppression is soft CC; bosses get reduced stacks / no hard lock.
- Sky Lattice segment cap prevents wire-farming the map.
- Watch hybrid delete: Conference turret + Walking Barrage + Sky Lattice — fun, not mission wipe.
- Do not let swap-deploy become “I never hold the chaingun again” without Fireteam/Emplacement investment — baseline path cards should make holding it still good.
- Heat is opt-in; zero Hellstorm heat cards = zero overheat punishment.


## 13. Status & Counter Split (explicit)

| System / counter     | Role on this gun                    | Baseline? | Notes |
|----------------------|-------------------------------------|-----------|-------|
| Spool                | RoF ramp identity                   | Yes       | Always-on light |
| Kinetic bullets      | Primary damage delivery             | Yes       | Not explosive |
| Heat                 | Redline / vent resource             | Hellstorm | Opt-in |
| Suppression          | Volume soft CC / shred windows      | Hellstorm | Boss-safe |
| Deploy turret        | Swap-off emplacement                | Auto Turret | Unlock on first card |
| Conference           | Origin-displaced converging fire    | Warthog   | Upgrade-owned |
| Lattice / Flak       | Crossing / airborne denial          | Warthog crown | Segment-capped |
| Fire/Shock/Acid      | Optional tip backlog                | Backlog   | Not identity |
| Friend ally rules    | Not used                            | No        | Distinct deployable |
| Shatter/Jam/etc.     | Not identity                        | No        | Other weapons own |


## 14. Implementation Notes

### 14.1 Gear registration
- Follow weapon template in this repo: clone base gun, GearInfo high-range id,
  APIName `chaingun`, behaviour component, SpawnGear stamp, CreateUpgrade pool.
- Prefer a projectile Gun in AllGear. **Clone candidate:** CartridgeSMG for simple tracers
  until art exists; retune GunData heavily toward MG weight.
  Alternate: study Gunship Cannon only for spool-adjacent upgrade DNA — **do not** inherit
  explosive shells as baseline.
- Plugin: GUID `sparroh.chaingun`, MycoMod **IsSandbox**.
- Persistence: stable gear id; register before PlayerData.OnAwake AddGear.
- Working gear id band: pick free high-range id at impl (e.g. 93xxx) — confirm unused.

### 14.2 Behaviour host
ChaingunBehaviour (or true Gun subclass when prefab exists):
- WeaponData: spool rates, heat flags, deploy flags, conference params, suppression params,
  lattice flags, turret mults
- Runtime: spool01, heat01, deploy state, turret instance ref, conference offset params
- Runtime: suppression trackers (per-target soft state)
- Runtime: lattice segment pool
- Prefab snapshot restore on upgrade Remove

### 14.3 Spool
- Tick while firing: spool01 = MoveTowards(1, upRate * dt)
- Tick while not: spool01 = MoveTowards(0, downRate * dt)
- fireInterval = Lerp(idle, max, curve(spool01))
- Drive barrel spin VFX/audio from spool01
- Harmony or Gun fire path: apply interval before shot gate

### 14.4 Deploy / swap hooks
Critical: hook **gear swap / equip / unequip** paths on local player.

Draft flow:
1. On chaingun unequip (swap off) + DeployUnlocked + owner local/server auth:
   - Spawn turret networked object at plant point
   - Snapshot GunData + relevant WeaponData flags into turret controller
   - Hide/disable carried chaingun visuals as needed
2. Turret controller: acquire targets in arc, call fire with snapshot stats
3. On chaingun equip (swap back):
   - Despawn/fold turret; restore carried weapon; apply recall bonuses

Reference DNA:
- FriendinaBox deployable lifecycle (spawn, duration, owner, combat hooks) — **pattern only**
- BallistaTurret / TurretInteractable — world turret aim/fire patterns (map props, not gear)
- AutoTurretFoldout — fold anim reference if visual fold-out desired
- Do **not** require map Ballista interaction; this is gear-owned deployable

MP:
- Sandbox mod; all clients need the plugin
- Owner-authoritative deploy; replicate turret transform + fire FX
- Snapshot must be deterministic enough that clients agree on visual RoF

### 14.5 Conference displacement
On fire (owner):
- For each pellet i in bulletsPerShot (Conference-aware count):
  - offset = random point in annulus/box in muzzle local space (x/y dominant)
  - start = muzzle.TransformPoint(offset)
  - dir = (aimPoint - start).normalized
  - spawn bullet at start with dir

Aim point:
- Ray from camera/look through crosshair to range end or first surface/target

VFX:
- Do not draw fake barrels everywhere; tracers from offset starts sell the fantasy
- Optional brief side-flash at offset origin

Swarm Launcher lessons:
- Reuse **displacement** idea only
- Skip hover, dive, breeding, remote-control swarm identity

### 14.6 Heat / Vent
- heat01 += buildRate * spool01 * dt while firing (if heat enabled by cards)
- heat01 -= coolRate * dt while not firing / after vent
- Redline Belt: if heat01 > threshold, max RoF mult climbs; at 1.0 trigger overheat state
- RMB Vent when claimed: cone burst, heat01 → 0, short CD

### 14.7 Suppression
- OnDamageTarget at high spool: add Suppression stacks on brain/part
- Full stacks: apply soft debuff + Walking Barrage shred window flag
- Decay stacks on time; bosses reduced cap

### 14.8 Sky Lattice
- On bullet travel or on hit miss segment: spawn short line trigger/VFX
- On enemy overlap: chip damage; if airborne flag, bonus
- Pool + max active segments; expire oldest

### 14.9 Hooks

| Hook                 | Use |
|----------------------|-----|
| Fire gate / interval | Spool-scaled RoF |
| OnFiredBullet        | Conference origin rewrite; lattice crumbs; tracer intensity |
| OnBeforeDamage       | Suppression shred window; turret mult; Conference split already in pellet damage |
| OnDamageTarget       | Suppression apply; Priority Latch last-target memory |
| OnKillTarget         | Fireteam feed; Shared Feed refunds; dump valve arming |
| Gear equip/unequip   | Deploy / recall |
| RMB                  | Vent if claimed |
| Player death / scene | Forced recall |

### 14.10 RMB priority

1. Redline / Hellstorm **Vent** — if equipped and heat > crumb
2. Future Warthog dump alt — if equipped and no Vent conflict (backlog)
3. Else unbound (baseline)

Deploy is **never** on RMB.

### 14.11 HUD
- Spool meter (thin barrel RPM bar) — always useful
- Heat bar when heat path present
- Deploy glyph when DeployUnlocked (ready / active / recall hint)
- Conference icon when Protocol / Lateral Feed active
- Prefer SparrohUILib if dependency acceptable; else minimal world-space / existing gun HUD hooks

### 14.12 VFX / audio priority
1. Spool whine climb + barrel RPM
2. Full-auto ballistic slap at max spool (weight, not SMG bee)
3. Brass / belt tick
4. Deploy fold-out plant thud + turret spin-up
5. Recall fold-in clack
6. Conference side-origin whoosh + converging tracers
7. Lattice segment hum / pop on cross
8. Vent roar + heat hiss
9. Suppression flinch stinger on full stacks

### 14.13 Multiplayer
- IsSandbox; identical mod on all clients
- Turret NGO prefab or runtime network spawn strategy documented at impl
- Conference offsets: owner-rolled with seed or owner-only visual + server damage validation
- Prefer simple: owner simulates fire, server validates damage, clients play FX


## 15. Deliberate Non-Goals

- Not replacing Gunship Cannon explosive rotary fantasy
- Not Cycler energy SMG reskin
- Not Friend in a Box grenade pet / fake multiplayer ally
- Not baseline RMB deploy
- Not baseline Conference or heat brick
- Not explosive shells as identity
- Not Lagrange Halo orbit-bullet clone
- Not Swarm hover-dive clone
- Not hard root Firmly Planted as free baseline
- Not dual full-DPS player + turret without crown investment
- Not permanent map turrets
- Not requiring custom Unity prefab for v1 (runtime clone OK)
- Not shipping full elemental tip suite in first 30
- Not homing multi-target “smart forks” as default Conference


## 16. Open Tuning Questions (playtest, not design blockers)

1. Spool time 0.6 vs 1.0s vs max RoF band.
2. Mag 80 vs 120 vs reserve hunger.
3. Bullet speed hitscan-feel vs visible tracers for Conference readability.
4. Turret damage mult 0.55 vs 0.75 vs Emplacement ceiling.
5. Shared ammo vs local turret belt copy.
6. Turret wake cold vs spool gift baseline on any deploy.
7. Conference pellet count 2 vs 3 vs 4 and split percents.
8. Lateral Feed offset radius before Protocol.
9. Heat build rate and Vent cone power.
10. Suppression stack time-to-full on grunts vs elites.
11. Sky Lattice segment duration and airborne detect reliability.
12. Move penalty at full spool — soft enough for Mycopunk mobility?
13. Plant point feet vs forward raycast frustration on ramps.
14. Whether heavy-swap deploy feels amazing or griefy in co-op.


## 17. Success Criteria / Player Fantasy Checklist

- [ ] Light spool is obvious in audio/VFX with zero upgrades
- [ ] Baseline chaingun feels like a real MG lane owner, not a loud Cycler
- [ ] RMB does nothing on baseline
- [ ] Weapon swap does nothing special on baseline
- [ ] First Auto Turret card (e.g. Field Latch) enables swap-deploy / swap-back recall
- [ ] Deployed turret holds a lane while you use other gear
- [ ] Swap back always recalls cleanly (no orphan turrets)
- [ ] Emplacement Doctrine makes the turret feel like a real build payoff
- [ ] Fireteam Split does not equal two full chainguns for free
- [ ] Lateral Feed alone sells side-origin shots
- [ ] Conference Protocol is a converging storm from displaced origins (BL2-accurate)
- [ ] Conference is coverage, not free triple DPS
- [ ] Sky Lattice punishes airborne / crossings without map wire cheese
- [ ] Walking Barrage suppression is readable and boss-safe
- [ ] Redline Belt heat drama is exciting, not a silent brick
- [ ] Hybrid Emplacement + Conference + Barrage feels intentional
- [ ] No explosive shell identity creep vs Gunship
- [ ] No Friend pet identity creep
- [ ] SAXON industrial rotary tone reads in model/audio direction


## 18. Strengths, Weaknesses & Co-op

Strengths
- Unique ballistic MG fantasy missing from the arsenal
- Swap-deploy is a high-skill positioning toy and co-op gift
- Conference Call geometry is instantly readable and clip-friendly
- Deep hybrid space (plant Conference turret, suppress, redline)
- Strong clear with path investment; flexible objective play

Weaknesses
- Low brain-off burst without spool commitment
- Reload / belt hunger punishes spray
- Turret bad placement is a real mistake (still fun)
- Weaker pure single-target sniper delete than DMLR / charge weapons
- Mobility tax at full spool if over-tuned

Co-op
- Your turret is a lane gift — default friendly fire rules match vanilla bullets
- Suppression sets up ally focus fire
- Avoid grief deploy in doorways without HP/destroy rules
- Conference side-origins should not opaque ally cameras excessively


## 19. Visual, Audio & Thematic Design

Appearance
- SAXON industrial rotary chaingun: multi-barrel cluster, exposed belt/box mag,
  hazard stripes, heat-discolored muzzle ring, unauthorized field-weld brackets,
  fungal-etched “DO NOT EMPLACE INDOORS” sticker (ignored)
- Tracers: dense kinetic streaks; Conference shows clear side-entry paths
- Deployed: bipod/tripod fold-out, barrels stay live, status light (friendly cyan / heat amber)
- Lattice: thin hard-line tracer afterimages, not magic runes
- Suppression: air distortion / enemy aim-flinch chevrons

Sound
- Spool: electric-mechanical whine climbing with barrel RPM
- Fire: heavy cyclic thud-thud-thud (weight > bee SMG)
- Belt: metallic feed ticks at high RoF
- Deploy: servo plant + bipod crack
- Recall: reverse servo + mag seat clack
- Conference: asymmetric whooshes L/R
- Lattice: thin wire hum + snap on cross
- Vent: pressure roar + cooling hiss
- Redline: alarm harmonic under fire loop

Flavor / codex line (in-game style)
  Chaingun
  Full-auto rotary kinetic weapon. Barrels spool up while firing.
  Auto Turret upgrades deploy the gun on weapon swap and recall it on swap back.
  Warthog upgrades enable Conference Call displaced volleys.


## 20. Locked Review Decisions (2026-08-06)

| Decision              | Lock |
|-----------------------|------|
| Form factor           | Ballistic rotary chaingun primary |
| Player-facing name    | Chaingun |
| Slot                  | Primary |
| Paths                 | Hellstorm / Auto Turret / Warthog |
| Fire mode             | Full-auto kinetic; light always-on spool |
| Baseline spool        | A — always-on light spin-up |
| Baseline RMB          | Unbound |
| Turret depth          | Path-gated; first Auto Turret card unlocks |
| Turret input          | **Swap off = deploy; swap back = recall** (not RMB) |
| Signature draws       | 1) Auto Turret  2) Conference Call |
| Conference model      | BL2-accurate origin displacement → aim point |
| Conference analogue   | Swarm-like displacement DNA only (no hover/dive) |
| Heat                  | Hellstorm opt-in (not baseline brick) |
| Lattice / AA          | Warthog crown spice (Sky Lattice), not third pillar name |
| Explosive shells      | No (Gunship keeps boom) |
| Friend pet rules      | No |
| Ship pool             | Frozen 30 listed above |
| Crowns                | Walking Barrage, Redline Belt, Emplacement Doctrine, Fireteam Split, Conference Protocol, Sky Lattice |
| Doc depth             | Full bible |
| MycoMod flag          | IsSandbox at implementation |
| Working APIName       | chaingun |
| Working GUID          | sparroh.chaingun |
| Doc file              | Chaingun-DesignDoc.txt (this file) |
| Tone                  | SAXON industrial rotary |
| Gimmick priority      | Fun / readable toys over pure meta DPS |


## 21. Changelog

v1 (2026-08-06)
- Initial full design from locked user decisions
- Paths: Hellstorm (base MG), Auto Turret (swap-deploy/recall), Warthog (RoF + Conference)
- Research anchors:
  - Wiki: Cycler, Gunship Cannon (Firmly Planted, Industrial Feed, Adaptive Stabilizers,
    Hold-Plating, Overclocked, Lagrange Halo contrast), Lead Flinger, DMLR, Accelerator,
    Swarm Launcher (displacement analogue only), FriendinaBox contrast, Plate Launcher contrast
  - Decompile touchpoints: AutoTurretFoldout, BallistaTurret / TurretInteractable patterns,
    Gun fire / GunData fields from weapon template
  - Sibling docs: Hard-Light Constructor (bible structure, path unlock ladder, RMB-free baseline),
    DMLR Rework (path rigor), FriendinaBox (deployable lifecycle contrast)
- User locks: name Chaingun; spool A; turret path-gated; **swap deploy/recall not RMB**;
  Conference BL2 origin displacement; full bible; primary slot; turret + Conference as draws


## 22. Implementation checklist (post-design)

- [ ] Rename plugin/csproj/thunderstore from template → Chaingun
- [ ] ChaingunBehaviour.Data fields from §14.2
- [ ] Retune cloned GunData (RoF band, mag, reserves, spread, falloff)
- [ ] Spool runtime + fire interval gating
- [ ] Swap equip/unequip deploy + recall
- [ ] Turret controller (arc, acquire, snapshot fire, ammo share)
- [ ] Field Latch / any Turret card sets DeployUnlocked
- [ ] Conference origin displacement on fire
- [ ] Lateral Feed → Conference Protocol progression
- [ ] Heat + Redline Belt + Vent RMB priority
- [ ] Suppression + Walking Barrage
- [ ] Sky Lattice segment pool
- [ ] UpgradeRegistration frozen 30
- [ ] HUD: spool, heat, deploy state
- [ ] Persistence + SpawnGear stamp
- [ ] Playtest pass on §16 knobs
- [ ] Confirm no orphan turrets on death/scene/menu
