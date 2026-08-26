# Marksman Laser Rifle (DMLR Rework) — Design Doc v3

## 1. High Concept / Fantasy

A dual-mode marksman rifle that reads enemy anatomy and routes damage through it.

- Setup mode tags, cracks, and dismantles outer structure (limbs, shells, attachments).
- Execute mode spends that preparation — transfer, Expose dump, arcs — into higher-value
  structure (usually the Core), unless Hot Swap flips which mode is setup vs execute.
- Power comes from on-hit effects, part kills, created weakpoints, and mode dialogue —
  not raw % damage stacking alone.

**Weapon loop (one sentence):**
  Outer structure → inner payoff; setup mode builds, execute mode spends.

One-liner: Tag the body. Cut the limb. Dump the pain into the heart.

v3 elevation: the kit is no longer three good loops that share a dictionary. It is
**one loop with three path dialects**. Baseline teaches the grammar; upgrades scale it.

---

## 2. Role in the Arsenal

- Slot: Primary
- Range: Mid–Long
- Identity: Precision / anatomy / setup → execute
- Loop: Charge ↔ discharge, and more importantly **build ↔ spend** across modes
- Clear vs Boss: Both viable — path and upgrades decide the mix

Not trying to be: generic laser DPS, pure ADS sniper, or status grenade launcher.

---

## 3. Core Mechanics & Gunfeel (Baseline)

### Firing
- DMR: fully automatic, high accuracy, no ADS (canAim = false)
- Laser: hold RMB (aim) to beam; release or empty charge → DMR
- Both held: laser wins by default
- Hot Swap: roles and priority flip (see keystone)
- Reload (R): reload only — never mode toggle
- Charge: ~20 DMR hits to full laser (matches default mag) unless upgrades change economy

### Mode roles (default vs Hot Swap)

                    Default                         Hot Swap
Setup mode          DMR (mark, crack, strip)        Laser (paint, charge breach, soften)
Execute mode        Laser (transfer, expose dump)   DMR (heavy slugs, breach spend)
Both buttons        Laser priority                  DMR priority

### What baseline includes (v3)
- Clean auto-DMR + hold-laser charge loop
- **Scan** (teaching-scale Severance only — see §4.0)
- No strong innate Transfer / Mark / Expose power

### What baseline does NOT include
- No keystone-level damage transfer
- No full Mark / Expose / Sever payoffs
- Those scale through the upgrade grid

Baseline must **speak the weapon sentence** at low volume so players learn build→spend
before Neural Feedback or Hard-Light Designator show up. Power remains upgrade-gated;
literacy is not.

---

## 4. Shared Framework: Severance

Severance is the vocabulary the whole weapon speaks. Paths are dialects. Upgrades scale
volume; they should not invent a second language.

### 4.0 Scan (baseline teaching hook) — NEW in v3

Scan is weak, short, and always on. It exists so the gun is never “generic dual-mode”
before keystones.

- DMR hits on **limbs or shells** apply Scan to that part (short duration, refreshable)
- Execute-mode hits **consume** Scan on the part (or brain — tune in playtest) for exactly
  one teaching-scale payoff per consume:
    - tiny Transfer toward the normal destination, **or**
    - tiny laser-charge / breach-ammo crumb (pick one primary in implement; other may be
      path-flavored later)
- Numbers stay below notice as “build power” — players should feel *direction*, not melt
- Scan does **not** replace Mark, Expose, or Neural Feedback
- Upgrades may deepen Scan (longer, stronger consume) but must not delete the baseline hook
- AOE/DoT: do not apply Scan unless an upgrade says otherwise (precision bias)

TUNING INTENT: after one magazine of correct play, a new player should feel
“I set something up on the body, then spent it with the other mode” even with zero path
cards.

### 4.1 Mark
- Applied to the specific enemy part hit (limb / shell / core / attachment)
- Short duration, refreshable
- Hook for transfer bonuses, arcs, echo damage, charge/ammo refunds, etc.
- Stronger and more build-defining than Scan
- Sources: path upgrades + **guaranteed bootstrap in frozen 30** (see §4.8)

### 4.2 Transfer
- Fraction of damage dealt to a part is also applied to another part on the same brain
- Default destination: **drill path** (sticky focus) — weakest outermost living shell →
  inward on that breach → Core when it will accept normal damage
- Never limb → limb
- Default mode roles:
  - Setup mode: low/no power transfer unless a card says otherwise (Scan = teaching only)
  - Execute mode: primary transfer/execute tool when upgraded
- Hot Swap: high transfer rides with execute mode (DMR slugs), not with “laser” by name

TUNING CAUTION:
  Too much unconditional core transfer will trivialize bosses. Prefer:
  - Partial transfer (never 100% baseline on a single card)
  - Higher transfer on execute than on setup
  - Bonuses gated on Marked parts, part-kills/overkill, or Expose windows
  - Re-transfer guard so transferred hits cannot chain-transfer forever
  - Transfer must not fully obsolete shooting the core directly — Expose and direct
    core execute time should still feel optimal during dump windows
  - **Stacking law is ship-blocking** (see §4.9), not a playtest footnote

### 4.3 Expose (created weakpoint)
- Temporary state on a Core (sometimes a designated part)
- Granted by shell breaks, brand completion, or specific exotics — not free
- While Exposed: bonus damage taken + stronger on-hit payoffs (charge/ammo refund, etc.)
- This IS the weakpoint system: real anatomy + created windows, no fake crit boxes

#### Expose residual (v3 cohesion law)
Execute climax must **not** hard-off outer-structure setup. During Expose:

  **Primary rule (locked):** Transfer rate and Overkill Conduit payoffs are amplified
  while the brain is Exposed. Direct core execute remains optimal; limbs/shells setup
  earlier still pays during the dump instead of becoming dead context.

Alternates (do not stack all; backlog if primary underperforms):
  - Limb kill during Expose extends Expose slightly or refunds charge
  - Marked outer parts echo a fraction into the Exposed core (mini-Resonance)

### 4.4 Sever (part-kill payoffs)
- Limb / shell / attachment kills are first-class events
- Overkill, bursts, mark spread, stun amplify, transfer spikes hang off part-kill callbacks
- Differentiated from generic "on kill" wherever possible

### 4.5 Precision bias
- Prefer OnBeforeDamage / direct hits for anatomy math
- AOE/DoT apply reduced transfer or skip Mark/Scan application unless an upgrade says
  otherwise (DamageFlags.Precision / non-AOE checks)

### 4.6 Anatomy conversion (global pillar language) — NEW in v3

**Limb conversion is Dissection’s dialect, not the only weapon-global form of pillar 3.**

Weapon-global rule:
  Damage to **outer structure** (limb or shell) converts into **inner value**
  (transfer, Expose, resources, tokens, clear seeds).

| Path        | Outer tissue | Inner payoff dialect                          |
|-------------|--------------|-----------------------------------------------|
| Dissection  | Limbs        | Transfer drip/spikes, artery tokens, anti-regen |
| Breach      | Shells       | Brand → Expose, inward splash, execute dump   |
| Conductor   | Any (Mark)   | Arcs, echo, detonations, marked recycling     |

Hybrids are encouraged. They share Scan, Mark, drill destination, and the stacking law
so they **compound under rules** instead of triple-dipping unbounded transfer.

On limb-poor trash: one anatomy action (Scan consume, single Mark, one shell chip) must
seed value. Full dissection is never required for pack clear.

### 4.7 Setup token pattern (canonical micro-combo)

Best cards already use this. v3 makes it law for new design:

  Setup mode **applies** a short-lived token on part/brain.
  Execute mode **consumes** it for a spike.
  Mode switch is the spend button — not a side effect.

Canonical examples:
  - Open Artery (Arterial Shred): limb setup → next execute hit bonus transfer
  - Core Brand: shell stacks → execute hit Exposes
  - Demonstrator's Trick: switch empowers next on-hit Mark behavior

Rules:
  - Prefer consume-on-execute-hit over “while laser exists” passive auras
  - Under Hot Swap, tokens consume on **execute mode**, not hard-coded laser/DMR name
    unless the card is intentionally mode-locked
  - Each path should eventually own at least one Rare switch/token payoff so cycling
    is not Conductor-only (Dissection has Artery; Breach uses Brand finish; Conductor
    has Demo Trick)

### 4.8 Mark bootstrap (frozen 30) — NEW in v3

Conductor and Mark-gated bonuses must boot without a rare primer brick.

**Locked bootstrap:**
  1. Demonstrator's Trick continues to enable Mark-on-DMR and switch empowers.
  2. **Sympathetic Arc** applies a short Mark on its **direct** laser hit part if that
     part is not already Marked (self-bootstrapping clear path). Arc jumps do not full
     Mark-spray unless a card says so.
  3. Overkill Conduit’s “if Marked” bonus remains real, not flavor text.

Backlog still holds Conductive Primer for denser Mark application later.

### 4.9 Transfer stacking law (ship-blocking) — NEW in v3

Mix-and-match stays; unbounded parallel transfer does not.

Per damaging event (one direct hit’s transfer resolution):

  1. At most **one passive % transfer** applies at full value (e.g. Neural Feedback).
  2. At most **one event transfer** applies at full value (Overkill Conduit, Severance
     Cycle pulse, Voltaic Battery detonation transfer, etc.).
  3. Arc-carried and similar ride-along transfer always use a **reduced** scale and never
     count as a second full passive.
  4. Transferred damage:
       - Sets re-transfer guard (no chain transfer)
       - Does **not** apply Mark, Scan, Brand, or Open Artery
       - Does **not** benefit from Expose damage mult unless a card explicitly says
         transferred damage may
  5. Pulverizer inward splash is splash, not Neural passive; it still respects re-transfer
     guard and should stay modest when stacked with Neural

Hybrids should feel clever, not like delete-everything.

### 4.10 Severance Cycle constraints — NEW in v3

Severance Cycle is exotic cadence fantasy, not a second weapon.

**Do not ship both at full strength:**
  - Full automatic DMR ↔ Laser cadence, **and**
  - Full laser charge drain suppression (infinite beam windows)

**Preferred shape (locked intent):**
  - Auto-cadence may remain, **but charge still drains** during laser windows
  - Dissection stacks still amplify the **switch pulse** (the skill expression)
  - Infinite-beam DNA only as a reduced/partial drain suppress if playtest demands —
    not “drain disabled” at exotic full power alongside forced flips

**Hot Swap interaction (define in implement, document here):**
  - Input priority follows Hot Swap (both buttons → execute mode per Hot Swap table)
  - High transfer still rides execute mode
  - Breach Ammo still builds from laser-role hits when Hot Swap is equipped; forced
    Cycle laser windows do build Breach Ammo
  - Forced windows must not strand Open Artery / Brand tokens unreadably — pulse and
    token consume still fire on valid execute hits inside the window
  - UI must show cycle phase; silent forced swaps are a feel bug

Player-authored cycling (Demo, Artery, Brand, charge bar) remains the default skill
fantasy. Cycle is a metronome modifier, not a replacement brain.

### 4.11 Feedback priority (UI is design)

If players cannot read state, pillars collapse into “I think something happened.”

**One primary telegraph per moment** (highest wins for attention):
  1. Expose core pulse
  2. Mark outline
  3. Open Artery / Brand-ready flash
  4. Breach Ammo ready / slug armed
  5. Dissection stacks / Cycle phase (secondary)

VFX production order follows this list. Debug transfer text is not a substitute.

### Mode dialogue summary

No forced anti-synergy between paths. Mixing Dissection + Breach + Conductor is allowed
and encouraged when it creates weird builds — under stacking laws, not under hope.

---

## 5. Design Pillars

1. On-hit and part events > flat % damage stacking
2. **Anatomy conversion** — outer structure (limb or shell) → inner payoff; paths are dialects
3. **Mode dialogue** — setup builds, execute spends; cycling is the spend button
4. Transfer, Expose, and Sever are the fantasy payoffs (upgrade-scaled; Scan teaches)
5. Three peer path dialects + thin support + generic staples
6. Mix-and-match allowed — **stacking laws** replace exclusion matrices
7. ~30 upgrades ship first; Exotics larger equal shapes; 3 paths of build identity
8. Exotic shapes should always be larger than others; each exotic same cell count
9. **Readable loop** — baseline Scan + feedback priority so identity is not keystone-or-bust

---

## 6. Strengths / Weaknesses

Strengths
- Excellent at prioritized part removal
- High skill expression (what you shoot and when you switch modes matter)
- Flexible ST bossing or marked-pack clear via path choice
- Strong payoff windows (Expose, limb overkill, arcs, token consumes)
- Hot Swap role inversion multiplies build space without a new verb set
- Shared Severance vocabulary enables hybrids that still read as the same weapon

Weaknesses / failure modes (design against these)
- Weaker "brain-off spray" without Conductor investment
- Transfer/Expose/tokens require setup — poor play is noticeable (intended)
- Reload/charge mismanagement still punishes
- Not the best pure move-speed left-click clear gun
- **Keystone-or-bust** if Scan/Mark bootstrap ever regress — protect teaching hooks
- **Charge-bar false cycle** if execute never consumes setup (laser camping)
- **Checklist rot** if every elite is Brand 5 → dump with no tissue read
- **Expose abandoning setup** if residual law is ignored at implement
- **Severance Cycle vs player cadence** if drain suppress + auto-flip both ship full
- **Hybrid transfer melt** if stacking law is treated as optional
- **Limb-poor trash** if Dissection has no fallback seed (Scan/Mark must work in packs)
- **Conductor Mark starve** without bootstrap
- Drill-path transfer into shells can feel like “wrong target” without VFX clarity

---

## 7. Paths Overview

Path   Name          Conversion dialect                         ST     Clear
A      Dissection    Limb → transfer / artery / anti-regen      *****  ***
B      Breach        Shell → Brand / Expose → execute dump      *****  **
C      Conductor     Mark → arc / echo / detonate / recycle     ***    *****
S      Support       Tiny ally-laser branch                     —      —
G      Generic       Staple gun economy / patterns              flex   flex

Each path answers the same question — *how does outer work become inner payoff?* —
with a different accent. None should require ignoring mode dialogue to function.

---

## 8. Full Upgrade List (~30)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity
Cell rule: Exotic shapes larger than others; all Exotics same cell count.

Player-facing names below. API names assigned at implementation.
Vanilla names are NOT kept — full rename pass for rework identity.

Cards below include v3 cohesion notes where rules changed.

------------------------------------------------------------------------------
PATH A — DISSECTION (limb → transfer)                    [10]
------------------------------------------------------------------------------

A1. Neural Feedback — Exotic (Keystone)
    Damage dealt to limbs transfers a percentage to the Core (or best available
    inner part via drill path) on the same brain.
    Execute-mode transfer rate is significantly higher than setup-mode.
    With Hot Swap, the high transfer rate moves to the execute mode (DMR slugs).
    Starting targets (VALIDATE IN PLAYTEST — core transfer can get strong fast):
      Setup/DMR transfer 8–12% · Execute/Laser transfer 20–30%
    Prefer these as "of damage dealt to the limb", not multiplicative with every
    other damage card without the stacking law (§4.9).
    Passive % slot = this card for stacking law purposes.

A2. Overkill Conduit — Epic
    When you kill a limb, overkill damage is multiplied and transferred inward
    (Core/Shell). Extra multiplier if that limb was Marked.
    This is the "big transfer moment" — safer than high passive transfer because
    it requires a part kill.
    Event transfer slot for stacking law. Amplified during Expose (§4.3 residual).

A3. Severance Cycle — Exotic (mode cadence)
    Automatically cycles setup ↔ execute on an interval.
    Limb hits grant Dissection stacks; each mode switch consumes stacks to fire a
    transfer pulse into the last brain you limb-damaged (event transfer).
    **v3 constraints (§4.10):** charge still drains during laser windows; do not
    also fully disable drain. Stacks + pulse are the fantasy, not infinite beam.
    Mixable with Hot Swap under the documented interaction table — chaos allowed,
    silence and stranded tokens are not.

A4. Arterial Shred — Rare (Shredder rewrite) — **canonical setup token**
    +Limb damage, −Shell damage.
    Limb hits apply Open Artery briefly: your next **execute-mode** hit on that brain
    gains bonus transfer % (consumes token). Hot Swap–aware: execute mode, not
    hard-coded "laser" name only.

A5. Joint Breaker — Rare
    Every 3rd DMR/setup hit on a limb applies Decay (detach tech / anti-regrow setup).
    DECAY lives here — not on Rot Thread.

A6. Phantom Pain — Epic
    Hitting a regrown limb (part that died earlier this combat on that brain)
    deals bonus damage and refunds a small amount of laser charge.

A7. Bilateral Trauma — Epic
    Limb hits also deal a fraction of that damage to a second random living limb
    on the same brain.

A8. Marrow Flare — Rare
    On limb kill: elemental burst at the limb (Fire / Shock / Acid rolled on
    upgrade apply). Small radius, on-hit flavored clear.

A9. Field Amputation — Standard
    Minor +limb damage.

A10. Bleed Charge — Standard
    Limb hits generate bonus laser charge.
    Path-flavored economy that funds execute from correct setup tissue.

------------------------------------------------------------------------------
PATH B — BREACH (shell → expose → execute)               [10]
------------------------------------------------------------------------------

B1. Hard-Light Designator — Exotic (Keystone)
    Shell kills Expose the Core for several seconds.
    Damage vs Exposed cores is increased.
    Execute hits on Exposed cores refund laser charge (execute loop).
    Expose residual (§4.3): transfer/overkill amp active during window so prior
    outer setup still matters while beaming the core.

B2. Core Brand — Epic — **canonical setup token**
    Setup/DMR hits on shells apply Brand stacks. At max stacks, your next
    **execute-mode** hit on that brain Exposes the core briefly even without a
    shell kill. Hot Swap–aware consume.

B3. Hot Swap — Exotic (role reverse keystone)
    - Laser becomes default fire; DMR becomes the alt (RMB)
    - Both held → DMR wins
    - Laser damage builds Breach Ammo (special reserve, not normal mag)
    - DMR shots become heavier, slower breach slugs that spend Breach Ammo for
      huge shell/core damage and bonus transfer-on-part-break
    - Laser is setup/paint; DMR is execute
    Still mixable with other exotics — no hard ban.
    Breach Ammo generation and slug spend are the build↔spend loop under role flip.
    See §4.10 for Severance Cycle interaction.

B4. Pulverizer — Rare (rewrite)
    +Shell damage, −Limb damage.
    Shell hits splash a % of damage inward to the next inner part (toward core).
    Splash inward should stay modest; respects re-transfer guard; not a second
    Neural passive for stacking law.

B5. Spalling — Rare
    Shell hits fling a shrapnel tick to a nearby limb on the same brain.

B6. Collapse Wave — Epic
    On shell kill, emit a concussive elemental pulse scaled by the shell's max
    health (Concussive Wave DNA, anatomy-gated).

B7. Fault Line — Rare
    Repeated DMR/setup hits on the same shell escalate damage on that shell
    (per-part ramp; resets on part death or timeout).

B8. Reactor Tap — Rare (Power Core rewrite)
    Core kills refund laser charge.
    If the core was Exposed, also refund DMR ammo.

B9. Hard-Light Bypass — Rare
    Laser pierces shields + minor +damage to shells.

B10. Breach Charge — Standard
    Shell hits generate bonus laser charge.

------------------------------------------------------------------------------
PATH C — CONDUCTOR (mark / arc / clear)                  [10]
------------------------------------------------------------------------------

C1. Sympathetic Arc — Exotic (Arc Lightning rewrite) — **Mark bootstrap**
    Laser/execute hits on Marked parts arc lightning to nearby enemies.
    **v3:** Direct laser/execute hit applies a short Mark to the hit part if it is
    not already Marked (self-bootstrap). Jumps do not auto-Mark entire packs.
    Arc targeting priority: other Marked parts → limbs → nearest enemy part.
    Arcs can carry a reduced Transfer percentage if transfer upgrades are also
    equipped (reduced rate — stacking law §4.9; never full passive Neural on jumps).

C2. Sympathetic Resonance — Epic
    When you damage a Marked part, other Marked parts on that brain take a small
    echo hit (clear amplifier for multi-mark play).

C3. Voltaic Battery — Exotic (Tosser rewrite)
    On reload, throw a battery that sticks to the first enemy part hit.
    - Detonates after a short fuse or when shot
    - Explosion damage applies to stuck part and transfers a portion into its
      parent/core (event transfer; stacking law)
    - Power increases when fewer rounds remain in the mag when thrown
    - Shooting the battery triggers the larger second blast (classic tosser fantasy)

C4. Demonstrator's Trick — Epic — **canonical switch token**
    Mode switch empowers your next on-hit:
    - Setup → Execute: next execute hit spreads Mark to nearby parts on that brain
    - Execute → Setup: next setup hit applies a heavy Mark + bonus damage to that part
    Also enables broader Mark-on-setup-hit support as implemented.
    Template for “cycling is the spend button.”

C5. Triple Feed — Rare (Triple rewrite)
    DMR shots always apply Shock or Acid (rolled per magazine).
    No Fire on this card — leaves room for Elemental Emitter.

C6. Rot Thread — Rare
    Every 3rd DMR/setup hit on a shell applies Rot (shells only — mirrors Joint Breaker
    which is limbs-only Decay).
    ROT lives here — Joint Breaker owns Decay. Do not double up.

C7. Incendiary Lattice — Rare
    While in execute/laser beam, periodically emit a damage wave along the aim path
    that deals full damage only to Marked targets (reduced to unmarked).

C8. Parting Shot — Epic (Tainted Exhaust rewrite)
    When you kill a part with the DMR/setup mode, create a small elemental explosion
    that applies Mark to the nearest limb of nearby enemies.

C9. Marked Recycling — Epic (Two-Way Charging rewrite)
    Execute/laser damage refunds DMR ammo only when hitting Marked or Exposed parts.
    Rewards spending on prepared tissue — not generic laser uptime.

C10. Conductive Primer — Standard
    Small chance on DMR hit to apply Shock + apply Mark.
    Backlog-friendly denser Mark; frozen 30 relies on Arc bootstrap + Demo first.

------------------------------------------------------------------------------
SUPPORT (thin)                                           [2]
------------------------------------------------------------------------------

S1. Engineering Nanites — Epic
    Laser heals allies (keep fantasy). Healing contribution is modest; not a main
    build pillar.

S2. Assist Charge — Rare
    Healing an ally with the laser restores a small amount of laser charge.

------------------------------------------------------------------------------
GENERIC STAPLES & GUNFEEL                                [10+]
------------------------------------------------------------------------------

G1. Boundary Incursion — Oddity
    Increases upgrade grid size. (Keep.)

G2. Kinetic Amplifier — Rare
    Modest universal damage increase. Staple filler.

G3. Beam Amplifier — Standard
    Modest laser damage increase.

G4. Heavy Hitting — Rare
    DMR damage up, fire rate down.

G5. Trifecta — Rare
    DMR fires in 3-round bursts.

G6. Condensed Munitions — Epic
    Dump remaining mag into one shot. Damage scales with ammo spent.
    Gains pierce through additional parts per 10 ammo (hierarchy-aware pierce
    preferred over random bounce). Builds laser charge based on ammo spent.
    Setup-finisher / execute-adjacent spike; still benefits from anatomy reads
    when pierce walks meaningful parts.

G7. Long Scope — Standard
    +Range. Includes reverse falloff: damage increases with distance (marksman
    signature). Applies to DMR and laser.

G8. Aux Reserves — Standard
    +Ammo capacity.

G9. Reorganized Reserves — Standard
    +Laser charge capacity, −DMR ammo capacity.

G10. Quickswap Cartridge — Standard
    +Reload speed, −Magazine size.

G11. Rapid Charge — Rare
    +Laser charge on hit, −Laser capacity.

G12. Feedback Amplifier — Standard
    +Laser charge gained from dealing damage.

G13. Overcharge Cycling — Epic
    Chance on kill to refund DMR ammo.

G14. Coronal Ejection — Rare
    Laser damage greatly increased, drains faster, requires full charge to start
    firing. Strong execute button for Expose windows.
    Partial/non-Coronal laser must remain valid for Scan consume, Brand finish,
    Breach paint, and transfer drip — Coronal is a dump option, not the only laser.

G15. Overheated Capacitor — Rare
    Laser damage ramps the longer the beam is held continuously.

G16. Photon Surge — Epic
    Move faster while firing laser.

G17. Kinetic Impossibility — Rare
    Hover while firing laser.

G18. Gravitational Collapse — Epic
    Requires Kinetic Impossibility. While hovering laser is active, pull enemies
    toward aim point. Marked targets receive stronger pull.

G19. Sturdy — Rare
    While firing laser: root yourself, gain DR, +laser damage.
    Turret-execute stance for Expose windows.

G20. Elemental Emitter — Rare
    Laser deals a rolled element (Fire / Shock / Acid chosen when the upgrade is
    applied — one upgrade, not three separate cards).
    Minor rider based on rolled element:
      Fire  — part-kills leave a brief burn zone
      Shock — status prefers jumping to limbs on the same brain
      Acid  — slightly more effective into shells

G21. Hazard Recycling — Epic
    Gain laser charge when the rolled element is fully applied to you.

------------------------------------------------------------------------------
POOL TOTAL
------------------------------------------------------------------------------

Dissection .......... 10
Breach .............. 10
Conductor ........... 10
Support .............  2
Generic / gunfeel ... 21  (G1–G21; includes staples, mobility, one element card)
------------------------
Listed total ........ 53 design entries if every G* is separate

PRACTICAL DROP POOL TARGET: ~30 upgrades

The lists above are the full design vocabulary. For the live ~30 pool, ship this
priority set first (exactly 30):

  Recommended frozen 30 for v1 implement (v3 cohesion-stable):

    EXOTIC (6)
      1  Neural Feedback
      2  Severance Cycle          (rules per §4.10 — not infinite beam + full auto)
      3  Hard-Light Designator
      4  Hot Swap
      5  Sympathetic Arc          (includes Mark bootstrap §4.8)
      6  Voltaic Battery

    EPIC (8)
      7  Overkill Conduit
      8  Phantom Pain
      9  Core Brand
      10 Collapse Wave
      11 Sympathetic Resonance
      12 Demonstrator's Trick
      13 Condensed Munitions
      14 Marked Recycling

    RARE (10)
      15 Arterial Shred           (execute-mode token consume)
      16 Joint Breaker            (Decay)
      17 Pulverizer
      18 Fault Line
      19 Reactor Tap
      20 Triple Feed
      21 Rot Thread               (Rot)
      22 Elemental Emitter        (rolled Fire/Shock/Acid)
      23 Coronal Ejection
      24 Kinetic Impossibility

    STANDARD (5)
      25 Bleed Charge
      26 Breach Charge
      27 Long Scope
      28 Feedback Amplifier
      29 Aux Reserves

    ODDITY (1)
      30 Boundary Incursion

  BACKLOG (designed, add when expanding pool past 30):
    Bilateral Trauma, Marrow Flare, Field Amputation, Spalling,
    Hard-Light Bypass, Incendiary Lattice, Parting Shot, Conductive Primer,
    Engineering Nanites, Assist Charge, Kinetic Amplifier, Beam Amplifier,
    Heavy Hitting, Trifecta, Reorganized Reserves, Quickswap Cartridge,
    Rapid Charge, Overcharge Cycling, Overheated Capacitor, Photon Surge,
    Gravitational Collapse, Sturdy, Hazard Recycling

  Support (Nanites / Assist Charge) stays designed but backlog — thin branch,
  add when support feel is wanted without bloating the first 30.

  Mark density: if Arc bootstrap + Demo prove insufficient in playtest, promote
  Conductive Primer into Standards by swapping a pure economy staple.

---

## 9. Example Builds (mix-and-match encouraged)

Boss surgeon (ST)
  Neural Feedback + Overkill Conduit + Arterial Shred + Joint Breaker
  + Coronal Ejection + Reactor Tap
  Strip limbs, token into execute transfer, full-charge dump; Expose residual still
  amps transfer if a Breach card is mixed in.

Breach executioner
  Hard-Light Designator + Core Brand + Pulverizer + Fault Line
  + Overheated Capacitor (backlog) or Coronal Ejection + Sturdy (backlog)
  Crack armor, Expose, beam the core — prior shell setup still feeds residual transfer.

Hot Swap slugger
  Hot Swap + Core Brand + Condensed Munitions + Long Scope
  + Fault Line / Pulverizer
  Laser paints breach ammo; DMR dumps anatomy-breaking slugs at range.

Mark conductor (clear)
  Sympathetic Arc + Resonance + Triple Feed + Marked Recycling
  + Rot Thread + Voltaic Battery
  Arc self-Marks if needed; laser one body; arcs/echoes clean the rest.

Hybrid freak (multi-path)
  Neural Feedback + Hard-Light Designator + Sympathetic Arc + Voltaic Battery
  Transfer + Expose + arcs + sticky battery — allowed, bounded by stacking law (§4.9).

---

## 10. Economy Rules of Thumb

- Path-flavored charge (Bleed Charge, Breach Charge, Marked Recycling) rewards
  meaningful hits; generic Feedback/Rapid Charge remain valid staples.
- Expose windows should feel like "dump now" — Coronal / Overheat / Sturdy /
  Condensed snap to that — without making partial execute worthless.
- Transfer is partial; Expose still rewards direct core execute time.
- Limb regen exists in vanilla — Phantom Pain and Decay (Joint Breaker) are the
  answers, not infinite limb farm without cost.
- Stacked transfer sources obey §4.9 by law, not by hope.
- Scan consume crumbs must never rival Bleed/Breach Charge card value.

### Playtest acceptance tests (v3) — pass/fail feel targets

1. **Teaching:** With zero path keystones, one magazine of limb/shell DMR then execute
   still feels like build→spend (Scan). If it feels like a plain charge bar, Scan is
   too weak or unread.
2. **Dialogue band:** A correct setup (Artery / full Brand / Expose / Marks) makes the
   next 1–2s of execute **obviously** stronger than raw setup-mode spray.
3. **Recoverability:** Wrong tissue choice is noticeable but recoverable within about
   one magazine — not run-ending.
4. **Trash:** One anatomy action (Scan, single Mark, one shell chip) seeds clear value;
   full multi-limb dissection is not required for packs.
5. **Anti-camp:** If laser/execute uptime is near-permanent without setup cards, economy
   is too generous — tighten refunds before nerfing fantasy spikes.
6. **Anti-starvation:** If execute never appears without three economy cards, baseline
   charge or Scan crumbs are too stingy.
7. **Hybrid:** Neural + Designator + Arc must feel strong and clever, not instant boss
   delete via parallel transfer. If delete, enforce stacking law before cutting fantasy.
8. **Cycle:** Severance Cycle must not strand tokens or erase charge as a reason to
   return to setup. If it does, cut drain suppress first.

---

## 11. Status Split (explicit)

- Joint Breaker → Decay on limbs only (every Nth limb hit)
- Rot Thread → Rot on shells only (every Nth shell hit)

- Triple Feed → Shock or Acid per mag (no Fire)
- Elemental Emitter → one rolled laser element (Fire / Shock / Acid) with minor rider
- Marrow Flare / Parting Shot / Collapse Wave → rolled on-apply burst elements as needed

---

## 12. Implementation Notes (for later)

### State on DmlrReworkBehaviour
- marks (part id → expiry), expose timers per brain, transfer percents
- brand stacks, breach ammo, dissection stacks, open-artery timers
- phantom-pain combat memory per brain
- **Scan** map (part id → expiry) or reuse a weak mark channel with a Scan flag
- drill focus fields (already present)
- cycle phase / breach slug armed
- stacking-law bookkeeping for current hit (passive used / event used)

### Hooks
- OnBeforeDamage  — mults + transfer redirect + Expose residual amps
- OnDamageTarget  — Scan/Mark/charge/brand/stacks; Scan consume on execute
- OnKillTarget    — part-type payoffs (limb vs shell vs core)

### Part typing
- EnemyLimbPart, EnemyShell, EnemyCore, attachments via EnemyComponentType

### Transfer cast
- Extra DamageData to destination with re-transfer guard
  (DamageFlags.Custom or behaviour flag — transferred hits do not transfer again)
- Apply §4.9: no Mark/Scan/Brand/Artery from transferred damage; Expose mult policy

### Hot Swap
- DMLRUpgradeFlags.SwapModes + existing input patch; extend with breach ammo
- Token consumes key off execute mode boolean, not hard-coded laser where required
- Wire Breach Ammo property to Scout slot when chosen (code may exist unmapped)

### Severance Cycle
- Implement §4.10 (drain remains unless partial suppress explicitly tuned)
- Interaction table with Hot Swap in code comments + this doc
- Wire to Scout slot when chosen (code may exist unmapped)

### Scan
- Baseline on weapon behaviour spawn / gear init — not an upgrade card
- Teaching-scale numbers only; precision bias

### Names
- All new player-facing strings

### VFX priority (§4.11)
1. Expose core pulse
2. Mark outline
3. Open Artery / Brand-ready
4. Breach Ammo / slug armed
5. Cycle phase / dissection stacks
Also: transfer tick, battery stick, Scan consume (subtle)

---

## 13. Deliberate Non-Goals

- No forced keystone anti-synergy / exclusion matrix (stacking laws instead)
- No baseline **power** transfer (Scan is teaching-scale only)
- Support is not a third full path
- Not rewriting enemy prefabs or adding real new hitboxes
- Not keeping vanilla names for flavor identity
- Not shipping Severance Cycle as infinite beam + full forced cadence simultaneously
- Not treating limb-only conversion as the sole global pillar (anatomy conversion is)

---

## 14. Changelog vs prior docs / shipped drafts

Old direction / v2                          v3
-----------------------------------------  ------------------------------------------
Three loops sharing a dictionary           One weapon loop, three path dialects
Anatomy fantasy upgrade-only               Scan teaches; power still upgrade-gated
Limb conversion as global pillar risk      Anatomy conversion (limb OR shell → inner)
Setup/execute implied by cards             Token pattern + mode spend is law
Expose can ignore outer setup              Expose residual amps transfer/overkill
Mark easy to starve                        Arc direct-hit Mark bootstrap + Demo
Transfer stack = tuning note               Transfer stacking law ship-blocking
Severance Cycle infinite beam DNA          Cadence + pulse; charge still drains
Hot Swap × Cycle undefined                 Interaction intents documented
UI optional                                Feedback priority is design law
Acceptance = vibes                         Playtest acceptance tests §10
Scattershot rewrites                       Unified Severance vocabulary (kept from v2)
Flat damage heavy                          On-hit / part-kill / expose primary (kept)
No deliberate anti-synergy                 Kept — laws replace bans

---

## 15. Open Tuning Questions (playtest, not design blockers)

1. Neural Feedback execute transfer 20–30% — lower if bosses melt too fast when
   combined with Overkill Conduit under stacking law.
2. How many Brand stacks to Expose, and Expose duration.
3. Breach Ammo generation rate vs slug cost on Hot Swap.
4. Whether Condensed pierce should walk Parent chain only or any parts in beam.
5. Mark duration defaults (~3–5s starting guess); Scan shorter (~1.5–3s).
6. Scan consume: tiny transfer vs tiny charge crumb as the single primary baseline
   payoff (other may become path-tinted later).
7. Expose residual magnitude — transfer/overkill amp % that feels relevant without
   double-paying Designator.
8. Severance Cycle: partial drain suppress allowed at all, or never?
9. Mark bootstrap sufficiency — promote Conductive Primer if Arc+Demo starve packs.

---

## 16. Review Decisions Locked

### From 2026-04-08 (v2 — still in force unless superseded)
- Transfer power is upgrade-gated (Scan is the v3 teaching exception only)
- Execute = transfer/dump by default; Hot Swap reverses roles
- Support stays tiny
- Weapon supports both crowd clear and single-target via upgrade choice
- Full rename (no vanilla player-facing names required)
- No deliberate anti-synergy; mix-and-match allowed
- Condensed Munitions remains Epic
- Element lasers collapsed to one rolled Elemental Emitter
- One Rot card (Rot Thread), one Decay card (Joint Breaker)
- Core transfer strength: start conservative; playtest will decide
- First ship pool: frozen 30 listed above; remainder is designed backlog

### From 2026-08-11 (v3 cohesion review — NEW)
- **One weapon loop:** outer → inner; setup builds, execute spends
- **Anatomy conversion** is the global pillar language; limb-only is Dissection’s dialect
- **Scan** baseline teaching hook ships with the weapon
- **Setup token pattern** is canonical (Artery / Brand / Demo); Hot Swap–aware consumes
- **Expose residual:** transfer + overkill amp during Expose (primary rule)
- **Mark bootstrap:** Sympathetic Arc marks direct hit part if unmarked; Demo retained
- **Transfer stacking law** is ship-blocking (§4.9)
- **Severance Cycle:** no full auto-cadence + full drain suppress together; charge drains;
  stacks/pulse are the fantasy
- **Hot Swap × Cycle** interaction intents documented; implement must follow
- **Feedback priority** list is design law, not polish backlog
- **Playtest acceptance tests** in §10 gate “feel done,” not only DPS sheets
- Frozen 30 card roster unchanged in membership; rules text elevated in place
- Hybrids encouraged under laws; exclusion matrix still rejected

---

## 17. Cohesion Laws (v3 constitution)

Quick reference for design and implement. If a new card fights these, change the card.

1. **Sentence:** Outer structure → inner payoff; setup builds, execute spends.
2. **Dialects:** Dissection limbs, Breach shells, Conductor marks — same sentence.
3. **Scan:** Baseline teach; tiny; precision; never replaces keystones.
4. **Tokens:** Apply on setup, consume on execute; switch is the button.
5. **Expose residual:** Dump windows still pay outer setup (transfer/overkill amp).
6. **Mark boots:** Arc direct hit + Demo; no dead Conductor starts.
7. **Stacking law:** One passive % + one event transfer full; rides reduced;
   transferred hits don’t Mark/Brand/Artery/chain; Expose mult gated.
8. **Cycle:** Metronome modifier, not infinite execute and not a silent second gun.
9. **Economy band:** Correct setup makes execute obviously better for 1–2s;
   wrong tissue recoverable; trash needs one seed action.
10. **Readability:** Expose > Mark > Artery/Brand > Breach Ammo > stacks.
11. **Mix-and-match:** Yes. Unbounded triple-dip transfer: No.
12. **Keystone-or-bust:** Forbidden. Baseline speaks the loop; upgrades raise the volume.

---

End of Design Doc v3
