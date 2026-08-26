# Anti-Material Rifle — Design Document
# Mycopunk custom primary (mod: sparroh.antimaterialrifle)
# Status: Living design bible — implementation exists (v1.2.x); design revised post-pillar review
# Last rewritten: 2026-08-11
#
# Design revision (2026-08-11):
#   - Chambered Argument adopted as sacred base first-shot state (reload arms the spike)
#   - Clipped + Auto Trigger CUT — they erased tube identity / bolt pace
#   - Support biased toward windows (info/CC/marks/anti-shell), not raw watts
#   - Suppressive lens reframed as lane geometry without breaking the bolt
#   - Code may still register cut modules until a follow-up implementation pass

================================================================================
1. HIGH CONCEPT
================================================================================

SAXON-issue anti-materiel bolt rifle for cracking fungal armor at range.
Slow. Expensive. Final.

Codex / gear select line (in-game style):

  Anti-Material Rifle
  Heavy kinetic bolt-action. High single-shot damage, low capacity, and a
  deliberate single-round reload. The first round after a reload hits hardest.
  Built to open shells before the swarm closes.

One-sentence fantasy:
  Cold professionalism — load the Argument, deliver one perfect kinetic shot
  that ends a threat before it reaches the team, then live with the reload.

================================================================================
2. ROLE IN THE ARSENAL
================================================================================

Slot:     Primary
API name: ballistic_sniper
Gear ID:  87421
Base clone (runtime): CartridgeSMG → rewritten GunData + projectile bullet

Job:
  - Long-range priority elimination
  - Shell / plating rupture
  - Optional team mark, debuff, and setup support
  - Reload-armed first-strike cadence (Chambered Argument)

What it is NOT:
  - Not a laser / rail / hitscan identity (ChargeSniper / DMLR laser lane)
  - Not a charge-to-fire rocket (The Last Argument)
  - Not a close-mid slug spam loop (Lead Flinger)
  - Not a gadget-primary by default (Plate Launcher recall fantasy)
  - Not a run-and-gun SMG (Cycler / Accelerator)
  - Not a full-auto DMR or mag-fed comfort primary (Clipped / Auto Trigger retired)

Open niche it fills:
  The deliberate kinetic bolt-action primary — scarce ammo, tube reload,
  projectile travel + drop, shell-cracking authority, first shot after reload
  as the fight's punctuation mark.

--------------------------------------------------------------------------------
2.1 Comparison snapshot (wiki / vanilla context)
--------------------------------------------------------------------------------

  Weapon              Niche                         AMR differentiator
  ------------------  ----------------------------  ------------------------------
  DMLR                Accurate bolts + charge laser Energy dual-mode, not pure kinetic
  The Last Argument   Ultra-charge long rocket      Heavy/charge, not primary bolt rifle
  Lead Flinger        Fast semi slugs, kill-reload  Close-mid ballistic volume
  Plate Launcher      Sticky plates + recall        Exotic gadget weapon
  ChargeSniper (code) Energy charge sniper          Charge + elemental hooks
  Cycler / Accelerator Fast chaotic primaries       Opposite pacing pole
  Gunship Cannon      Explosive volume primary      Sustained fire, not precision bolt

================================================================================
3. CORE MECHANICS (SACRED)
================================================================================

These define the gun. Upgrades may bend them; they must not erase them.
Identity-breakers that remove tube reload or convert the gun to full-auto are
out of the active pool (see §8.7 CUT).

  1. Bolt-action / slow semi. One meaningful trigger pull at a time.
     No full-auto fire-mode conversion in the active catalog.

  2. Projectile bullets — travel time and bullet drop. Leading targets matters.
     Never default to rail/hitscan identity.

  3. Single-round (tube) reload. Each shell loads individually; full empty mag
     still costs about the full reload duration. Fire during reload cancels
     remaining shells if at least one round is chambered (soft-cancel).
     No upgrade may replace tube with full-magazine reload.

  4. Chambered Argument (first-shot arming) — SACRED BASE STATE.
     The first round fired after a reload sequence ends is empowered.
     Follow-up rounds in that magazine return to baseline.
     This is a magazine/reload state, not "full-HP only" and not upgrade-gated.
     See §3.1–3.2.

  5. ADS is a commitment. Slow aim enter, strong zoom, heavy move penalty while
     scoped. Hip-fire is intentionally near-unusable.

  6. No base alt-fire. Scope is the interaction. Gadgets (C4, etc.) come only
     from upgrades.

  7. Every shot must feel expensive — audio, recoil, ammo, and time.

Reload note (implementation truth):
  Per-shell duration = total reload time / magazine size (empty). Partial reloads
  scale the same way. Cancel-to-fire is a core skill expression, not a bug.
  Bolt-close (~0.5s) applies when the tube sequence ends or is fire-canceled —
  not after every intermediate shell.

--------------------------------------------------------------------------------
3.1 Chambered Argument (base first-shot state)
--------------------------------------------------------------------------------

Name:     Chambered Argument (design); ship copy may shorten to "Argument round"
Intent:   Reload arms the spike. The gun's authority lives in the opening strike
          of each mag cycle, not in uniform truck-damage on every trigger pull.

Arm when (any of):
  - Tube sequence completes (mag full), then bolt-close finishes
  - Tube stops because reserve is empty (with ≥1 chambered), then bolt-close
  - Player fire-cancels remaining shells (soft-cancel), current shell chambers
    if mid-anim, then bolt-close finishes
  - Future: equivalent "reload sequence ended + bolt ready" hooks only

Do NOT arm on:
  - Mere weapon swap / equip without a completed reload sequence
  - Every shot
  - Kill reloads or ammo pickups that do not run the tube/bolt flow
  - Mid-tube intermediate shells (only the first shot AFTER the sequence ends)

Consume:
  - On the first successful shot fired while Argument is armed
  - Follow-ups in that mag are baseline until the player reloads again and re-arms

Provisional power (design target — implement in balance pass):
  - Damage mult ~1.25–1.40 on the Argument shot
    OR a shell-biased bonus (flat shell damage / stagger) if pure mult stacks
    too hard with Hullcracker modules
  - Must be readable in feel (audio/UI optional later): the opening round is
    obviously the expensive one
  - Stock body damage should sit in the §4.2 band so Argument + modules re-earn
    deletion — not base 145 × Argument × five mults

Stacking rules (design intent):
  - Argument is structural. Subsonic / Overpressure / Heavy Grain may multiply
    it, but tuning must prevent nova openers (see §8 Subsonic, Overpressure).
  - Prefer: lower stock damage + clear Argument spike over hot stock + weak spike.
  - One in the Chamber grants an extra shell after full top-off; that bonus
    shell does not automatically re-arm Argument unless it is the first shot
    after a new reload sequence (normal rule). The chamber bonus is capacity
    discipline, not a second free Argument.

--------------------------------------------------------------------------------
3.2 Player-facing combat loop (reload mini-game)
--------------------------------------------------------------------------------

This is the core gameplay loop. Upgrades should change decisions inside it.

  1. ARM
     Tube load (full or partial) → bolt-close → Chambered Argument ready.

  2. SPEND
     Deliver the Argument shot on a priority target (shell, elite, marked heavy).

  3. FOLLOW-UP
     Remaining shells: second targets, Death Mark stacks, peels, chip, setup.

  4. CHOOSE
     - Cancel tube early (enough shells + Argument) vs greed full top-off
     - Reposition / Scouter plant vs hold angle
     - HE panic (if equipped) vs stay on the bolt
     - Spend last shell and accept downtime vs keep one and kite

Litmus test for any module:
  If it does not change a decision in Arm → Spend → Follow-up → Choose,
  it is Standard glue at best. If it deletes a step (no tube, no bolt pace,
  no Argument), it does not ship.

Cancel-to-fire mastery:
  Soft-cancel + bolt-close still ARMS Argument. Canceling for one shell is
  correct under pressure; greeding five shells is correct when the lane is safe.
  Bolt-close must not feel like pure punishment — the Argument shot is the reward.

================================================================================
4. BASE STATS
================================================================================

4.1 Shipped values (v1.2 implementation — WeaponRegistration.ApplyBallisticSniperStats)

  Stat                    Value                         Notes
  ----------------------  ----------------------------  --------------------------
  Damage                  145                           High single-hit; known hot
  Element                 Normal / 0
  Fire interval           1.25 s (~48 RPM)              Deliberate bolt / slow semi
  Automatic               0 (semi)
  Bullets per shot        1
  Magazine                5
  Reserve (ammoCapacity)  20                            ~4 full reloads
  Reload duration         3.1 s                         Tube; ~0.62 s per shell
  refillAmmoOnReload      false                         Tube owns fill
  Bullet speed            220                           Clear travel time
  Bullet gravity          9.5                           Noticeable drop at range
  maxBounces              0
  Falloff start / end     140 / 200
  Max damage range        250
  Max falloff mult        0.65                          Soft only at long range
  Hit force               high (~2.5× base, floor 28)   Anti-materiel stagger
  Hip spread              ~3.8 circle                   Intentionally terrible
  ADS                     enabled, FOV 28, ~0.55 s      Standing ADS tightened in hook
  Recoil                  high vertical, low horizontal Recoverable single kick
  Charge data             disabled                      Not a charge weapon by default
  Chambered Argument      NOT YET IN CODE               Design-sacred; implement next

4.2 Intended balance band (design target — NOT yet mandatory in code)

  Current numbers are known-provisional / likely overtuned (see CHANGELOG 1.1.0).
  Treat the band below as the tuning north star after playtests.

  Stat                    Target band                   Intent
  ----------------------  ----------------------------  --------------------------
  Damage                  110 – 130                     Baseline work shots; fodder
                                                        still dies; elites need
                                                        Argument + shell tech
  Chambered Argument      ~1.25–1.40× (or shell bias)   First post-reload spike
  Fire interval           1.15 – 1.35 s                 Stay deliberate
  Magazine                4 – 5                         Precious
  Reserve                 16 – 24                       Scarce vs typical primaries
  Reload                  ~3.0 – 3.3 s tube             Keep identity
  Bullet speed            200 – 260                     Lead still required
  Gravity                 keep noticeable               Skill shot at distance
  Falloff start / end     120–160 / 180–220             Soft only far out
  ADS time                ~0.55 s                       Commitment
  Hip spread              remain near-unusable          No run-and-gun baseline
  Ammo collect mult       ≤ typical primary             Economy pressure
  Bolt-close              ~0.45–0.55 s                  Weight after tube end/cancel

DPS philosophy:
  Unbuilt AMR should win the OPENER (Argument on a marked elite / shell) and
  lose pure horde clear and marathon DPS to Cycler-class weapons.
  Volume and AoE are earned through geometry modules (echo, twin, pierce,
  ricochet) and HE — never through full-auto or mag-dump reload conversion.
  Stock gun stays honest; grid may go filthy within tube + bolt constraints.

4.3 Handling pillars (feel)

  - High single-shot vertical recoil; low horizontal (learnable)
  - Very stable once planted and aimed
  - Severe movement accuracy penalty while scoped
  - Stop sprint to fire/aim; heavy fight commitment
  - Cannot aim while reloading (base constraints)
  - Argument shot should read as the mag's climax; shots 2–N are deliberate
    follow-ups, not identical truck-taps

================================================================================
5. DESIGN PILLARS (NON-NEGOTIABLE)
================================================================================

  1. Every shot is meaningful. The first shot after reload (Chambered Argument)
     is the most meaningful — reload arms the strike.

  2. Tube reload + bolt-action pace are identity. Playing around reloads
     (partial load, cancel-to-fire, full top-off discipline, downtime tools)
     is the core loop, not pure downtime.

  3. Mobility and close-range performance stay intentionally weak unless a
     module explicitly trades into handling — and that trade must NOT erase
     tube, bolt pace, or Argument arming.

  4. Support mechanisms create WINDOWS: information, soft CC, marks, ally sync,
     shell/shield/immunity answers, panic demo. They must not replace decisions
     with stackable raw watts alone.

  5. Upgrades change pacing and decision-making inside Arm → Spend → Follow-up
     → Choose (§3.2), not only multipliers.

  6. Kinetic impact is the soul. Elements and gadgets are seasoning, not the
     default identity.

  7. The hex grid is for mix-and-match. Themes are gravity wells, not railroads.

  8. Exotic modules are large, equal in footprint to each other, and rebuild
     fantasy when equipped — without granting full-auto or full-mag reload
     unless a future Exotic is explicitly designed as a named identity break
     with heavy cost (none planned).

  9. Prefer fun, legible synergies over dry stat sticks — in line with
     Mycopunk’s experimental loadout culture — but keep the stock gun honest.

================================================================================
6. UPGRADE PHILOSOPHY
================================================================================

6.1 Fluid themes — not skill trees

Mycopunk’s upgrade grid does not lock players into paths. Neither does this gun.

  - Themes describe intended gravity, fantasy language, and strong synergies.
  - Every upgrade is independently equippable (no prerequisite chains).
  - Hybrid grids are first-class and expected.
  - Grid space, rarity footprint, and identity locks are the real constraints.
  - Theme tags on modules may be multi-label (e.g. Spotter = Death Sentence + glue).

Use themes for:
  - Design checklist (does the pool support several distinct feels?)
  - Player discovery (“I want to crack shells” / “I want to mark for the team”)
  - Balance review (does a pure lean stomp? does a hybrid still work?)

Do NOT use themes for:
  - Mutual exclusion in UI
  - Forced loadout rails
  - Punishing curiosity

6.2 Soft tension pairs (interesting, not banned)

  Death Mark fuse timing ↔ Perforator line clears
    Cook windows vs multi-target pierce. Strong if the player spaces shots.

  High Explosive hold-R gadget ↔ pure ADS plant
    Demo tool vs scope discipline. Different fight cadence, both valid.

  Reposition move speed ↔ Anchor stationary perfection
    Roam peek vs turret plant. Classic sniper dual fantasy.

  Partial tube cancel ↔ One in the Chamber full top-off
    Emergency Argument now vs discipline reward later.

  Overpressure charge deliberation ↔ raw bolt tempo
    Every shot becomes a mini-commit; still tube-fed, still Argument-armed.

Retired tensions (modules CUT — do not design around them):
  Overpressure ↔ Auto Trigger
  Clipped ↔ tube-reload fantasy

6.3 Hard identity locks (pool rules)

  - NO full-magazine reload conversion (Clipped CUT).
  - NO full-auto fire-mode conversion (Auto Trigger CUT).
  - Volume / coverage stays projectile-bolt flavored: extra pellets, echo delay,
    pierce, ricochet, delayed marks — not RoF rebuilds into DMR/SMG.
  - Chambered Argument remains base behavior; upgrades may amplify or play with
    reload discipline but must not remove arming on reload end.
  - Support answers: “What do I do with the 2–3 seconds I’m not allowed to be
    the hero?” (marks cooking, scouting, C4, reposition, partial tube).

6.4 Universal truths (pool rules)

  - Target ~26 active gameplay upgrades (CUT removed Clipped + Auto Trigger from
    the design-active 28). Room for 1–2 fillers that reinforce §3.2, not break it.
  - Support at least three distinct build lenses (below) plus hybrids.
  - Exotic shapes are larger than typical rares/epics and all Exotics share the
    same cell count / footprint family.
  - Oddity grid-grow (Boundary Incursion) stays 1-cell spatial, mission-stackable,
    matching vanilla.
  - Shared contraband grid thieves (Edge Fault, Multiversal Thievery) are optional
    parity later — not required for identity.
  - Standard modules may CanStack where pure stats; identity keystones generally
    do not stack endlessly.
  - Heavy Grain and other raw damage glue must not outscale structural Argument
    + shell keystones as the primary power fantasy.
  - Descriptions stay short, mechanical, and slightly SAXON-wry — wiki tone.

6.5 Rarity & footprint guidance

  Rarity       Typical role                         Footprint family
  -----------  -----------------------------------  --------------------
  Standard     Glue stats, small handling           Small (~3 cells)
  Rare         Meaningful identity pieces           Medium / Line
  Epic         Build-defining (not always exclusive) Large / Wide
  Exotic       Fantasy rebuilders                   Exotic (shared large)
  Oddity       Grid manipulation / weird            Tiny (1 cell grow)

================================================================================
7. BUILD LENSES (FLUID THEMES)
================================================================================

Three gravity wells. Lean hard, dip lightly, or braid them. Examples are
illustrative only — many other combinations are valid.
All three lenses KEEP tube reload, bolt pace, and Chambered Argument.

--------------------------------------------------------------------------------
7.1 Death Sentence — mark, condemn, coordinate
--------------------------------------------------------------------------------

Fantasy:
  You don’t just shoot enemies — you condemn them. Fuses, brands, and ally
  follow-ups turn one Argument bolt into a team event.

What you lean into:
  Delayed payoff, information, debuffs, synchronized burst with teammates.
  Downtime while tubing is when fuses cook and allies finish.

Keystones (strong pull):
  Death Mark, Transfer Relay, Synchronize, Spotter

Supports:
  Scouter, Mark of Exhaustion, Wet Rounds, Disrupt Channel, Myco Splash

Natural hybrids:
  + Hullcracker: Argument cracks shell; mark/fuse finishes the core
  + Suppressive: Echo/Twin apply brands across a lane; Spotter reveals clumps

Example lean (not a railroad):
  Death Mark + Spotter + Synchronize + Scouter
  → “I arm Argument, brand the heavy, the squad deletes, fuses clean stragglers.”

Success feel:
  “I branded three heavies and the team deleted them without looking up.”

--------------------------------------------------------------------------------
7.2 Hullcracker — breach, plant, anti-elite
--------------------------------------------------------------------------------

Fantasy:
  Industrial breaching tool. One perfect Argument slug opens the bug. SAXON
  paperwork calls it “Argument-class kinetic intervention.”

What you lean into:
  Shell damage, full-HP / unbroken-target seasoning, planted ADS, charge shots,
  overflow into meat. The Argument shot is the breach charge.

Keystones (strong pull):
  Overpressure, Hullbreaker, Overkill, Subsonic, Longwatch, Perforator

Supports:
  Heavy Grain, Anchor, One in the Chamber, Disrupt Channel

Natural hybrids:
  + Death Sentence: Spotter/Exhaustion set up the breach; Death Mark cooks meat
  + Suppressive: Perforator + Ricochet for line breaches through packed armor

Example lean:
  Overpressure + Hullbreaker + Overkill + Anchor + Longwatch
  → “Argument landed, shell popped, core died on the same paperwork.”

Success feel:
  Priority targets lose their armor story on the opening committed shot.

--------------------------------------------------------------------------------
7.3 Suppressive Precision — lane ownership without breaking the bolt
--------------------------------------------------------------------------------

Fantasy:
  The bolt gun grows teeth. Still heavy, still long-range, still tube-fed —
  now it holds an angle against a wave or reshapes a lane with echo, bounce,
  pierce, twin geometry, and demo tools. NOT a mag-dump SMG. NOT full-auto.

What you lean into:
  Multi-projectile and path tricks, ricochet/echo coverage, reload economy,
  post-kill stability, C4. Cadence stays bolt-deliberate; coverage comes from
  where the bullet goes, not how fast the trigger cycles.

Keystones (strong pull):
  High Explosive, Twin Link, Ricochet Protocol, Powered Echo, Perforator

Supports:
  Deadbolt, Reserve Load, Reposition, Myco Splash, Wet Rounds, One in the Chamber

Natural hybrids:
  + Death Sentence: echo/twin marking; fuse windows during tube
  + Hullcracker: pierce lines through shell walls; HE for post-breach cleanup

Example lean:
  Twin Link + Powered Echo + Ricochet Protocol + Deadbolt + Reserve Load
  → “Nothing crossed the open ground — and I still loaded shells one by one.”

Success feel:
  Angle ownership and multi-target pressure without hipfire SMG fantasy and
  without deleting tube or Argument.

--------------------------------------------------------------------------------
7.4 Cross-theme glue
--------------------------------------------------------------------------------

These modules exist to complete grids and enable hybrids. They should rarely
define a fantasy alone:

  Heavy Grain, Reserve Load, Reposition, Anchor, One in the Chamber,
  Wet Rounds, Myco Splash, Boundary Incursion, Scouter (also Death Sentence-leaning),
  Deadbolt (also Suppressive-leaning)

================================================================================
8. FULL UPGRADE CATALOG
================================================================================

IDs: 87422–87449 (gear 87421).
Design-active gameplay upgrades: 26 (Clipped + Auto Trigger CUT from active pool).
Implementation note: v1.2.x code may still register CUT ids until the removal pass.
Theme tags: DS = Death Sentence, HC = Hullcracker, SP = Suppressive, GL = Glue.
Multi-tags mean the module serves more than one lens.

Stat ranges below mirror shipped Property ranges where noted. Flagged
[HOT] where values look overtuned relative to the target band in §4.2.
Flagged [RETUNE] where Chambered Argument adoption changes intent.

--------------------------------------------------------------------------------
8.1 Standard
--------------------------------------------------------------------------------

Heavy Grain                          id 87422    tags: GL, HC
  Dense projectiles. Large damage increase.
  Damage mult ~1.4–1.6 [HOT if stacked with other raw damage]
  Stackable. Pure kinetic glue.
  Must not outscale Argument + shell keystones as the main fantasy.

Reserve Load                         id 87423    tags: GL, SP
  Expanded reserve capacity.
  Reserve mult ~1.5–1.75
  Stackable. Eases economy pressure on geometry / multi-hit leans.
  Does not speed reload or break tube.

Scouter                              id 87431    tags: DS, GL
  While aiming, pulse-highlight threats every few seconds.
  Interval ~2.5–3 s, radius ~45–60
  Information tool; pairs with Spotter and Longwatch plants.
  Support window: know what to spend Argument on.

Reposition                           id 87444    tags: GL, SP
  Faster movement. Strongly reduced move speed while aiming.
  Move bonus ~0.28–0.38, ADS move mult ~0.5–0.6
  Peek-loop enabler; tensions with Anchor.

Wet Rounds                           id 87443    tags: GL, DS
  Applies heavy Wet (Water) buildup on hit.
  Wet ~10–14
  Team reaction / setup seasoning; keeps kinetic body damage.

--------------------------------------------------------------------------------
8.2 Rare
--------------------------------------------------------------------------------

Hullbreaker                          id 87424    tags: HC
  Massively increased damage to enemy shells and armor plating.
  Shell damage mult ~1.9–2.3 [HOT solo; define as signature HC piece]
  Mission-stackable. Core anti-materiel identity.
  Argument + Hullbreaker is the intended breach open.

Subsonic                             id 87425    tags: HC
  Bonus damage against full-health / unbroken targets.
  Full-HP mult ~2.1–2.6 [HOT] [RETUNE]
  Design intent after Argument ships:
    - Seasoning for fresh elites / untouched plates — NOT the primary
      “first shot” pillar (that is Chambered Argument / reload state).
    - Lower mult once Argument exists, or bias toward unbroken shells so
      it stays Hullcracker and does not double-count opener fantasy.
  Weaker in extended multi-mag fights by design.

Mark of Exhaustion                   id 87429    tags: DS
  Periodically, the next shot slows the target and cripples tracking.
  CD ~3.5–4.5 s, slow duration ~3.5–5 s, strength ~12–18
  Soft CC brand without stealing Death Mark’s delayed blast.
  Support window: create time to tube or plant Argument.

Longwatch                            id 87430    tags: HC, GL
  After 3s continuous ADS: more range, less drop, faster bullets.
  Range +80–120, gravity mult ~0.25–0.4, speed mult ~1.45–1.7
  Rewards plant discipline; Line footprint.
  Filthy with Argument + Anchor plants.

Spotter                              id 87433    tags: DS
  Hits mark the target and nearby enemies for allies.
  Radius ~8–12, duration ~6–8 s
  Team lens centerpiece without requiring Death Mark.

Deadbolt                             id 87434    tags: SP, GL
  After a kill, perfect accuracy for several seconds (hip and ADS).
  Duration ~3–3.5 s
  Chain-delete window after Argument opens a kill; still should not make
  hipfire a lifestyle alone.

Anchor                               id 87439    tags: HC, GL
  While completely stationary, recoil is fully negated.
  Speed threshold ~0.2
  Turret-plant fantasy; tensions with Reposition.

One in the Chamber                   id 87440    tags: HC, GL
  After a full top-off reload, fire one extra round from empty before reloading.
  Rewards complete tube discipline.
  Plays with Argument: full top-off arms Argument as usual; the bonus shell is
  capacity, not a second automatic Argument proc mid-mag.
  Cute with Overpressure mag pressure.

Myco Splash                          id 87442    tags: SP, DS, GL
  Applies heavy Rot on hit and splashes Rot to nearby targets.
  Rot ~10–14, splash radius ~4–5.5
  Fungal seasoning; light AoE without replacing HE.

Overkill                             id 87449    tags: HC
  When damage destroys a shell, leftover damage carries to the layer underneath.
  Signature breach finisher with Hullbreaker / Argument openers.

--------------------------------------------------------------------------------
8.3 Epic
--------------------------------------------------------------------------------

Ricochet Protocol                    id 87426    tags: SP
  Extra bounces. After bouncing, rounds home aggressively.
  Bounces +2–3, post-bounce homing ~8–12
  Lane geometry weapon; still projectile-skill flavored.

Overpressure                         id 87427    tags: HC
  Shots charge before firing. Huge damage, slower move while charging, smaller mag.
  Charge ~0.55–0.7 s, damage mult ~1.85–2.2, move mult ~0.55–0.65, mag −2
  Priority 1 identity keystone for committed breach.
  Still tube-fed; still arms Chambered Argument after reload.
  [RETUNE] Argument × Overpressure × Subsonic × Hullbreaker can nova —
  tune so the OPENER is filthy but not delete-everything-through-mult-soup.
  Prefer charge deliberation + shell bias over unlimited mult stacking.

Twin Link                            id 87428    tags: SP
  Fires an extra projectile. Hitting two different targets fences them with shock.
  +1 pellet, fence damage ~90–130, radius ~3.5–5
  Split-shot control; echoes Plate Launcher “fencing” fantasy in kinetic form.
  Coverage without breaking bolt interval.

Perforator                           id 87432    tags: HC, SP
  Pierce up to 5 targets. Damage falls off per pierce.
  Pierce 5, falloff ~0.22–0.28 per
  Line-breach and packed-wave tool. Argument through a column should feel great.

Death Mark                           id 87435    tags: DS
  Hits apply a delayed explosive mark. Stacks cook harder. Headshots shorten fuse.
  Fuse ~1.6–2.2 s, radius ~5–7, blast scale ~1.4–1.8
  Headshot fuse mult ~0.55, stack scale ~0.4
  Priority 1 Death Sentence keystone. Exotic-sized footprint; rarity Epic today.
  Plays around reload: shoot, start tube, fuse works while you load.
  Open question: promote to Exotic rarity later for clearer keystone parity (§12).

Disrupt Channel                      id 87441    tags: DS, HC
  Hitting a shield blasts it with massive ignore-immunity damage.
  (Design intent historically: disable shield ~10s — align fantasy copy with
  actual combat hook during balance pass.)
  Anti-projector / anti-shield sniper fantasy.
  Support: answer bullshit so Argument can land.

Powered Echo                         id 87445    tags: SP
  A second projectile echoes along the same path shortly after, applying Shock.
  Delay ~0.18–0.28 s, damage scale ~0.75–0.95, shock ~8–12
  Double-tap without full-auto. Great hybrid glue; keeps bolt cadence.

Synchronize                          id 87448    tags: DS
  If an ally damaged the same target within 3 seconds, deal massive bonus damage.
  Window 3 s, mult ~2.6–3.2 [HOT]
  Co-op sniper fantasy; weaker solo unless drones/DoTs count — verify hooks.
  Argument + Synchronize = team finisher fantasy.

--------------------------------------------------------------------------------
8.4 Exotic
--------------------------------------------------------------------------------

High Explosive                       id 87437    tags: SP
  Hold reload to throw C4. Hold reload again to detonate a massive blast.
  C4 damage ~240–320, radius ~9–12, throw force ~20, arm ~0.4 s
  Only stock “gadget” fantasy. Large exotic footprint.
  Demo lean, bunker break, panic button during tube downtime —
  not a replacement primary loop.
  UX: must not constantly fight tube reload input (§12).

Transfer Relay                       id 87446    tags: DS, HC
  When damage is negated (immunity), a large ignore-immunity pulse still lands.
  Transfer scale ~0.85–1.15
  Anti-immunity sniper argument. Keeps kinetic fantasy while answering
  projector / immunity bullshit — peak SAXON field ordnance.

Both Exotics share the Exotic() hex footprint (equal cell count).

--------------------------------------------------------------------------------
8.5 Oddity
--------------------------------------------------------------------------------

Boundary Incursion                   id 87447    tags: GL
  Adds a row or column to the upgrade grid.
  Vanilla-style GridGrow: IsSpatial | CanStackInMission, priority -100, 1 cell.
  Enables greedy hybrid grids; not a combat fantasy piece.

--------------------------------------------------------------------------------
8.6 Catalog checklist (26 design-active)
--------------------------------------------------------------------------------

  [S] Heavy Grain, Reserve Load, Scouter, Reposition, Wet Rounds
  [R] Hullbreaker, Subsonic, Mark of Exhaustion, Longwatch, Spotter,
      Deadbolt, Anchor, One in the Chamber, Myco Splash, Overkill
  [E] Ricochet Protocol, Overpressure, Twin Link, Perforator, Death Mark,
      Disrupt Channel, Powered Echo, Synchronize
  [X] High Explosive, Transfer Relay
  [O] Boundary Incursion

Legacy note:
  Early builds mentioned “Match-Grade Rounds” as a starter to unlock the hex UI.
  Full pool registration now supplies HasUpgrades. Match-Grade is not in the
  live pool; do not reintroduce unless it gains a real mechanical niche (tiny
  ADS precision glue).

--------------------------------------------------------------------------------
8.7 CUT / retired (do not reintroduce lightly)
--------------------------------------------------------------------------------

Clipped                              id 87438    was: R, SP/GL
  WAS: Full magazine reload instead of single-round; large reload speed mult.
  WHY CUT: Deleted tube decision-making — the gun’s strongest unique loop.
  Converted AMR into mag-fed comfort and made cancel-to-fire / partial load
  mastery irrelevant. Soft “break the bolt” trade was too easy and too complete.
  IMPLEMENTATION: Remove registration + behaviour.clipped path in follow-up pass.
  REINTRODUCE ONLY AS: conscious Exotic rebuild with heavy readable cost
  (e.g. lose Chambered Argument, lose shell identity, large footprint) — not planned.

Auto Trigger                         id 87436    was: E, SP
  WAS: Full-auto precision fire; higher mag; lower damage; worse ADS bloom.
  WHY CUT: Converted bolt pace into volume primary. Even with damage/bloom tax,
  the comfort path (with Reserve / Deadbolt / geometry) undermined deliberate
  Argument cadence and taught players to ignore reload play.
  Kept tube in code, but full-auto still erased the sacred “one meaningful
  trigger pull” feel.
  IMPLEMENTATION: Remove registration + behaviour.autoTrigger mutations.
  REINTRODUCE ONLY AS: named Exotic identity break with severe costs — not planned.

Design law after CUT:
  Coverage and pressure come from bullet path and support windows, not fire mode
  or mag-reload conversion.

================================================================================
9. EXAMPLE BUILDS (ILLUSTRATIVE ONLY)
================================================================================

Grid space is the real limit. These are teaching sketches, not meta decrees.
All examples assume Chambered Argument base behavior.

  A. Pure-ish Death Sentence
     Death Mark + Spotter + Synchronize + Scouter + Mark of Exhaustion
     Optional: Wet Rounds, Transfer Relay
     → Arm Argument, brand, let fuses and allies work during tube.

  B. Pure-ish Hullcracker
     Overpressure + Hullbreaker + Overkill + Anchor + Longwatch + Heavy Grain
     Optional: Subsonic (retuned), One in the Chamber
     → Plant, charge, Argument breaches shell, overflow eats core.

  C. Pure-ish Suppressive Precision (tube-true)
     Twin Link + Powered Echo + Ricochet Protocol + Deadbolt + Reserve Load
     Optional: Perforator or High Explosive
     → Lane geometry and echo coverage; still shell-by-shell reload.

  D. Hybrid — Spotter-Cracker
     Hullbreaker + Overkill + Spotter + Death Mark + Heavy Grain
     Mark the hard target, Argument-crack the shell, fuse the meat.

  E. Hybrid — Marked Geometry
     Powered Echo + Twin Link + Death Mark + Spotter + Reserve Load
     Multi-path brands without autofire; tube between waves.

  F. Hybrid — Breaching Demo
     Overpressure + High Explosive + Hullbreaker + Anchor
     Plant the Argument, plant the C4, leave no architecture.

  G. Hybrid — Lane Geometry
     Perforator + Ricochet Protocol + Powered Echo + Longwatch
     Skill-shot playground; still projectile-forward and tube-fed.

  H. Hybrid — Cancel Discipline
     One in the Chamber + Scouter + Hullbreaker + Reposition + Deadbolt
     Teach partial cancel for Argument now vs full top-off for bonus shell later.

Many other combinations are valid. None should require Clipped or Auto Trigger.

================================================================================
10. VISUAL, AUDIO & THEMATIC DESIGN
================================================================================

10.1 Appearance

  Base:
    Long brutalist SAXON rifle — thick barrel, heavy muzzle brake, oversized
    variable-zoom optic, exposed industrial fasteners. Fungal scoring and
    corrosion as wear and field abuse, not “the gun is a mushroom.”

  Death Sentence lean:
    Optic gains a faint brand/reticle tell when a mark is live. Marked enemies
    show a slow-burning countdown glyph (readable in chaos).

  Hullcracker lean:
    Heavier receiver presence, pressure seals, heat/stress on barrel during
    Overpressure charge.

  Suppressive lean:
    Heat haze after sustained geometry fire; ricochet/echo tracers readable.
    No drum-mag / full-auto receiver fantasy (Clipped/Auto Trigger art retired).

  Chambered Argument (optional future tell):
    Subtle chamber/ready cue after bolt-close (optic pip, brass gleam, audio)
    so players feel the armed first shot without a cluttered UI.

  Current implementation note:
    Runtime clone still uses Cartridge SMG mesh. Art swap is a later pass
    (AssetBundle / ModelImportHooks). Design assumes the brutalist rifle above.

10.2 Sound

  Base fire: deep authoritative crack, long echo, solid rearward punch.
  Argument shot: same family, slightly heavier report or distinct chamber ring.
  Bolt cycle: heavy metallic clunk (even on semi).
  Tube reload: distinct single-shell clicks; cancel-to-fire cuts the sequence.
  Bolt-close: definitive clack before Argument is live.
  Death Mark detonation: kinetic/explosive, not magical chime.
  Overpressure: pressure whine → heavier report.
  High Explosive: arm chirp / detonate crump, distinct from gunfire.

10.3 Flavor & SAXON voice

  Gear description (shipped-adjacent):
    Heavy kinetic bolt-action rifle. High single-shot damage, low capacity,
    and a deliberate single-round reload. The first round after reload hits
    hardest. Built for long-range elimination.

  Marketing blurb:
    “When the shell is thicker than policy, authorize Argument-class kinetic
    intervention.”
    — SAXON Field Ordnance, Form 87-AMR

  Optional stinger:
    “Load the Argument. Aim far away. Finish the paperwork later.”
    (Nod to The Last Argument’s brevity without cloning its charge fantasy.)

================================================================================
11. STRENGTHS, WEAKNESSES & PLAYER FANTASY CHECKLIST
================================================================================

Strengths
  - Highest intentional single-shot authority among kinetic primaries in this niche
  - Reload-armed first strike (Chambered Argument) creates rhythmic power spikes
  - Shell / elite disruption when leaning HC modules
  - Team value via marks, highlights, sync windows
  - Rewards map knowledge, positioning, target priority, and reload timing
  - Stagger / hit force sells anti-materiel weight
  - Cancel-to-fire and partial tube are real skill expression

Weaknesses
  - Poor close-range and hip performance
  - Low baseline RoF and horde DPS (by design; no full-auto escape hatch)
  - Reload and re-acquire vulnerability
  - Ammo scarcity
  - ADS movement tax
  - Projectile skill requirement (lead + drop)
  - Follow-up shots are weaker than Argument — marathon fights need support/geometry

Player fantasy checklist (success criteria)

  [ ] Unupgraded gun feels powerful on the opener but loses pure chases and swarms
  [ ] First shot after reload clearly matters more than shots 2–N (Argument readable)
  [ ] Cancel-to-fire arms Argument and feels like mastery, not a bug
  [ ] Full top-off vs partial load is a real fight decision
  [ ] Death Sentence lean makes the player think in fuses and ally follow-ups
  [ ] Hullcracker lean makes shell breaks on Argument shots feel like the point
  [ ] Suppressive lean never becomes hipfire SMG or full-auto DMR; still tube + bolt
  [ ] No equip path removes tube reload or grants full-auto fire mode
  [ ] Hybrids feel smart, not “wrong”
  [ ] No lean fully erases projectile skill without a clear cost
  [ ] Support buys time/info/answers, not only damage mults
  [ ] Exotic moments are fight highlights, not every trigger pull
  [ ] A planted Longwatch + Anchor Argument shot feels filthy in a good way
  [ ] Multiplayer: marks/highlights are readable to allies
  [ ] Heavy Grain glue never replaces Argument + shell identity as the fantasy

================================================================================
12. OPEN DESIGN & BALANCE QUESTIONS
================================================================================

  1. Chambered Argument final numbers
     Lock mult band (~1.25–1.40) vs shell-biased bonus. Implement arm/consume
     on bolt-close end. Optional audio/UI tell. Interaction with Overpressure
     charge (Argument applies to charged shot; avoid mult soup).

  2. Subsonic retune after Argument
     Lower full-HP mult and/or bias to unbroken shells so opener fantasy is not
     double-counted (reload state + full-HP state).

  3. Stock damage drop
     Move base damage into 110–130 before or with Argument implementation.
     Recommendation: lower stock as Argument ships so upgrades re-earn the spike.

  4. Overpressure × Argument × Hullbreaker ceiling
     Cap or diminish stacked opener mults; keep breach fantasy filthy but fair.

  5. Exotic keystone parity
     Today only High Explosive and Transfer Relay are Rarity.Exotic.
     Death Mark already uses the Exotic footprint at Epic rarity.
     Promote Death Mark (and/or a Hullcracker signature) to Exotic for
     one clear exotic-grade pull per lens? Or keep dual-Exotic and let
     Epics carry HC/DS?

  6. Disrupt Channel copy vs code
     Align description with the live hook (ignore-immunity blast vs timed
     shield disable). Pick one fantasy and implement consistently.

  7. Synchronize solo viability
     Does ally-damage window include status ticks, deployables, or only
     other players? Document the truth after hook audit.

  8. High Explosive UX
     Hold-reload gadget must not constantly fight tube reload input.
     Teach in description; consider arm HUD tell.

  9. CUT cleanup (implementation)
     Unregister Clipped (87438) and Auto Trigger (87436); strip behaviour
     fields/mutations; migrate saves/grids that referenced them gracefully.

 10. Third Exotic / filler candidates (if pool grows)
     Prefer modules that deepen §3.2 (reload play, Argument, downtime support)
     e.g. faster bolt-close after full top-off, fuse tick-up while tubing,
     shell-only Argument bonus — NOT full-auto or mag conversion.

 11. Shared contraband parity
     Add Edge Fault / Multiversal Thievery for vanilla grid-thief parity?
     Low priority; Boundary Incursion may be enough.

 12. Multiplayer readability
     Death Mark glyphs, Spotter marks, Scouter pulses, optional Argument tell —
     validate colorblind and chaos-dense fights.

 13. Art / audio pass
     Replace SMG placeholder mesh; bolt animation; per-shell reload foley;
     Argument chamber cue.

================================================================================
13. IMPLEMENTATION MAP (FOR DESIGNERS / MODDERS)
================================================================================

  Plugin.cs                     BepInEx entry, registration timing, sandbox flag
  WeaponRegistration.cs         Clone CartridgeSMG, GearInfo, ApplyBallisticSniperStats
  AntiMaterialRifleBehaviour.cs Data host + runtime (marks, echo, C4, scouter, Argument)
  AntiMaterialRifleReloadHook   Tube reload, per-shell duration, cancel-to-fire, bolt-close
  AntiMaterialRifleCombatHooks  Damage/fire/move/recoil/spread integration
  SpawnGearHooks.cs             Equip remap + identity stamp (NGO-safe)
  AmrUpgradeRegistrar.cs        Register ids (active pool; remove CUT ids)
  Upgrades/*.cs                 Per-module Property Apply/Remove
  UpgradeRegistration.cs        CreateUpgrade helper
  HexPatternUtil.cs             Hex footprints

Persistence:
  Gear ID 87421 is the save key. Register into AllGear before PlayerData.AddGear;
  re-bind GearData after load.

Multiplayer:
  All clients need the same mod and matching gear/upgrade ids.

Pending implementation (design-approved, not necessarily coded):
  - Chambered Argument arm on reload sequence end / bolt-close; consume on fire
  - Unregister Clipped + Auto Trigger; delete mutation paths
  - Base damage band drop; Subsonic / Overpressure retune
  - Optional Argument feedback (audio/UI)

================================================================================
14. DESIGN HISTORY (SHORT)
================================================================================

  v1.0  Stock ballistic sniper + tube reload + persistence + placeholder upgrade
  v1.1  Full ~25 module pool, combat hooks; numbers flagged overtuned
  v1.2  Projectile bullet swap (leave rail), reload cancel, +Boundary/Sync/Overkill
        (28 total); Longwatch meaningful on real projectiles
  Doc   Ground-up rewrite: arsenal niche, fluid themes, full catalog, target bands
  Doc   2026-08-11 pillar review:
        - Chambered Argument adopted as sacred base first-shot / reload state
        - Clipped + Auto Trigger CUT (erased tube identity and bolt pace)
        - Suppressive lens = lane geometry, not fire-mode rebuild
        - Support = windows during downtime, not watt stacking alone
        - Player loop documented: Arm → Spend → Follow-up → Choose
        - Implementation cleanup and Argument coding tracked in §12–13

================================================================================
15. ONE-PAGE SUMMARY
================================================================================

  Anti-Material Rifle is Mycopunk’s missing deliberate kinetic bolt primary.
  Scarce mag, tube reload, projectile drop, ADS commitment, shell-cracking weight.

  Reload arms Chambered Argument — the first shot after a reload sequence is the
  strike. Follow-ups are deliberate. Cancel-to-fire, partial tubes, and full
  top-offs are the skill game. Support tools buy time, info, and answers while
  you load; they do not turn the gun into an SMG.

  Upgrades are a freeform hex pool with three gravity wells —
  Death Sentence, Hullcracker, Suppressive Precision — built to mix.
  All three keep tube + bolt + Argument. Exotics rebuild fantasy within those
  locks. Clipped and Auto Trigger are retired: coverage comes from geometry
  and teamwork, not mag-dump or full-auto.

  Tune until every unupgraded Argument shot feels expensive, every shell break
  feels intentional, every cancel feels like mastery, and every hybrid still
  reads as the same brutal SAXON argument delivered at range.
