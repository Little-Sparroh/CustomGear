# Helminth Receiver — Design Document
# Mycopunk custom primary (mod: sparroh.livingweapon / sparroh.helminthreceiver)
# Status: Design bible v1.1 — pillar cohesion pass (Blood Math)
# Locked decisions: 2026-05-08; cohesion review 2026-08-11
#
#   1. Economy ........ Vitality buffer fed by Host HP (not raw HP-per-shot baseline)
#   2. Baseline DoT ... Light innate leech tick on hit (paths amplify)
#   3. Fire mode ...... Mid-RoF organic pulse rifle
#   4. Ship name ...... Helminth Receiver
#   5. Doc depth ...... Full ~30 upgrade catalog + tuning bands
#   6. Blood Math ..... %HP effects ride Bond/leech/Host state (not flat stat sticks)
#
# Working folder: .content.primary.HelminthReceiver
# Implementation: sacred loop + full ~30 pool registered; tune to this bible

================================================================================
1. HIGH CONCEPT / FANTASY
================================================================================

SAXON Type-L bio-ordnance. A primary that bonds to the operator and drinks
vitality instead of magazines. Baseline fire spends a Vitality buffer; the
buffer is refilled by feeding it your health. On hit, the weapon leaves a light
leech tick — the organism tasting the target.

Upgrades decide the relationship:

  Helpful Leech  — enemies pay; drain, spread, sustain
  Symbiote       — mutualism; well-fed Host bonuses over time
  Parasite       — the gun wins; Host blood for monstrous spikes

One-liner:
  Feed it. Direct it. Survive the relationship.

Codex / gear select line (in-game style):

  Helminth Receiver
  Mid-rate organic pulse rifle. Fires on Vitality, not ammo. Light leech on hit.
  Hold reload to feed Host health into the weapon. Bond carefully.

SAXON marketing blurb:

  "Type-L receivers do not accept conventional propellant. Authorize biological
  expenditure per Form 12-HEL. Operator fatality is a documented edge case,
  not a defect."
  — SAXON Field Ordnance, Symbiotic Systems Desk

Optional stinger:
  "It likes you. That is not necessarily good news."

================================================================================
2. ROLE IN THE ARSENAL
================================================================================

Slot:     Primary
API name: helminth_receiver          (provisional)
Gear ID:  92000                      (provisional — confirm free at implement)
Base clone (runtime): CartridgeSMG → rewritten GunData + LivingWeaponBehaviour
                      until a real prefab ships

Job:
  - Mid-range sustained pressure with a unique HP/Vitality economy
  - DoT / bond setup and payoff
  - Optional team sustain (Symbiote) or glass-cannon bloodfire (Parasite)
  - Skill expression: when to feed, when to push low Host HP, when to molt Bond

What it is NOT:
  - Not Carver (melee blood stacks, saw loop)
  - Not Swarm Launcher (ammo pellets with organic flavor)
  - Not Jackrabbit Immolator-only (self-fire as a side exotic)
  - Not AMR (deliberate kinetic bolt deletion)
  - Not DMLR (anatomy transfer / dual-mode laser)
  - Not a pure healbot primary
  - Not infinite free fire — starving the organism is a real failure state

Open niche it fills:
  The arsenal's organic risk engine — health-as-magazine, relationship fantasy,
  three peer paths around drain / mutualism / parasitism.

--------------------------------------------------------------------------------
2.1 Comparison snapshot
--------------------------------------------------------------------------------

  Weapon              Niche                         Helminth differentiator
  ------------------  ----------------------------  ------------------------------
  Carver              Melee blood stacks, self-dmg  Ranged Vitality economy + Bond
  Swarm Launcher      Hover pellets, ally heal      HP feed core; not ammo swarm
  Jackrabbit          Bounce fire, self-ignite      Full identity is the bond
  Globbler            Acid globs, taken-dmg meter   Proactive feed, not reactive
  DMLR                Anatomy mark/transfer         Soft tissue DoT/bond, not parts
  AMR                 Kinetic bolt, scarce ammo     Mid-RoF pulse, Vitality scarce
  Cycler/Accelerator  Chaotic volume primaries      Economy discipline, not spray

================================================================================
3. CORE MECHANICS (SACRED)
================================================================================

These define the gun. Upgrades may bend them; they should not casually erase
them without a clear cost and a readable fantasy shift.

  1. Vitality is the magazine. Conventional ammo is disabled or irrelevant.
  2. Host health feeds Vitality (passive drip + hold-reload Feed).
  3. Safety floor: cannot Feed (and baseline cannot auto-drain Host) below a
     minimum HP% unless a Parasite module explicitly lowers/removes it.
  4. Light innate leech tick on hit — short, modest; paths amplify.
  5. Mid-RoF organic pulse — neither bolt sniper nor SMG hose.
  6. Bond is the shared vocabulary upgrades speak (stacks/links on targets/self).
  7. Empty Vitality soft-locks fire (stutter click + organism pulse), never
     forces Host death by shooting.
  8. Self-harm spikes are opt-in depth (Parasite), not mandatory grief on stock.

--------------------------------------------------------------------------------
3.1 Vitality Buffer (economy model — LOCKED)
--------------------------------------------------------------------------------

Why a buffer (not raw HP per bullet baseline):
  - Avoids death-protection / net sync edge cases on every trigger pull
  - Lets Leech/Symbiote refund Vitality without always full heal-botting
  - Still reads as health-as-ammo because feeding the buffer hurts
  - Parasite exotics may collapse buffer → direct bloodfire (see Hemophage)

Definitions:

  Vitality (V)     Weapon resource spent to fire. Shown on custom HUD bar.
  Host             The local player.
  Feed             Convert Host HP → Vitality (hold reload).
  Safety Floor     Minimum Host HP% that baseline Feed will not cross.
  Bond             Per-target (and sometimes Host) stack state built by play.
  Leech Tick       Baseline short DoT applied on hit.
  Molt             Spend/convert Bond for a burst payoff (upgrade-gated).
  Graft            Attach a lasting effect (DoT nest, ally buff, self wound).

Baseline flow:

  1. Gun has Vitality pool (max V).
  2. Each shot spends V (not ammo). If V < shot cost → cannot fire (soft lock).
  3. Passive drip: while equipped and Host above Safety Floor, slowly convert
     a tiny amount of Host HP → V (very small — Feed is the intentional top-off).
  4. Feed (hold reload): faster HP → V conversion; interruptible; telegraphed.
  5. On hit: apply light Leech Tick; small chance or fractional Bond build.
  6. Leech Tick damage is mostly "taste" at baseline; refunds are upgrade-gated
     except a tiny optional Vitality crumb if needed for tutorial readability.

Empty Vitality feel:
  - Trigger pull → dry organism click, barrel twitch, HUD bar flash
  - No shot, no HP loss from the click itself
  - Player must Feed or wait for drip / leech refunds

Safety Floor (baseline target): 18% Host HP
  - Feed refuses to pull below floor (haptic + HUD deny)
  - Passive drip also respects floor
  - Parasite cards may lower floor or allow Overdraw

--------------------------------------------------------------------------------
3.2 Baseline gunfeel & stats
--------------------------------------------------------------------------------

Fire mode: mid-RoF organic pulse rifle (semi-auto feel with light auto assist
or true auto at measured interval — pick one in implement; design assumes
controllable sustained pulse, not mag-dump hose).

  Stat                    Target band                   Intent
  ----------------------  ----------------------------  --------------------------
  Damage / pulse          22 – 30                       Medium; DoT/bond carry weight
  Element                 Normal + leech rider          Rider is DoT, not full element
  Fire interval           0.14 – 0.18 s (~330–430 RPM) Mid pulse
  Automatic               1 (auto) or soft-auto         Controllable stream
  Bullets per shot        1                             Pulse spit (upgrades may split)
  Magazine (display)      = Vitality shots              Hide vanilla ammo or mirror V
  Reserve ammo            N/A / unused                  hasLimitedAmmo false path
  "Reload"                Feed channel                  Hold R = Feed; tap may cancel
  Pulse projectile speed  90 – 130                      Readable mid-range spit
  Gravity                 low – moderate                Organic arc optional light
  Falloff start / end     28–36 / 48–60                 Mid-range identity
  Max falloff mult        ~0.55 – 0.65                  Soft past mid
  Hit force               modest                        Not AMR stagger
  Spread hip / ADS        workable / tight              ADS optional; not AMR plant
  Recoil                  soft organic kick             Learnable vertical bias
  Vitality max            100 (abstract units)          Map to ~N shots
  V cost / shot           4 – 6                         ~18–25 shots per full V
  Feed: HP per 10 V       4 – 7 HP                      Feeding hurts, not suicides
  Passive drip            ~2–4 V/s while below max      Tiny; respects floor
  Safety floor            18% HP                        Parasite can lower
  Leech tick duration     1.8 – 2.5 s                   Short taste
  Leech tick DPS          ~12–18% of pulse hit          Modest; paths scale
  Bond/hit (baseline)     +1 Bond on target (cap low)   Cap ~3–5 without upgrades

Display rules:
  - Prefer custom Vitality bar over lying vanilla mag numbers when possible
  - If mag UI must stay: magazineSize tracks remaining whole shots from V;
    reload animation = Feed channel

DPS philosophy:
  Unbuilt Helminth should win sustained mid-range chip + light DoT and lose
  pure openers to AMR and pure horde hose to Cycler-class weapons. Volume,
  plague clear, tankiness, and blood spikes are earned on the grid.

--------------------------------------------------------------------------------
3.3 Baseline leech tick (LOCKED innate)
--------------------------------------------------------------------------------

On damaging hit (owner, non-transferred junk as applicable):

  - Apply / refresh Leech Tick on the hit part or brain (implementer choice:
    prefer brain-level tick for readability unless part-DoT is free)
  - Tick uses DamageFlags.DamageOverTime
  - Does NOT baseline refund HP or Vitality in meaningful amounts
    (optional: 0–1 V crumb per tick for juice — tune to near-zero if strong)
  - Bond: +1 on hit, short decay, low cap without upgrades

What baseline does NOT include:
  - No multi-target jump
  - No Host heal on leech
  - No Molt detonation
  - No direct HP-per-shot
  - No ally aura
  - No safety floor removal

================================================================================
4. SHARED VOCABULARY
================================================================================

4.1 Vitality (V)
  Weapon resource. Spent to fire. Gained via Feed, drip, and path refunds.

4.2 Feed
  Hold reload: channel Host HP into V.
  - Cancel on release / swap / stun as appropriate
  - Visual: tendrils retract into receiver; Host HUD pulse
  - Audio: wet draw, not mag insert

4.3 Bond
  Stacks on enemies (primary) and sometimes Host (Symbiote/Parasite).
  - Built by hits, leech ticks, and path cards
  - Decays out of combat or after timeout
  - Spent by Molt cards; amplified by Graft cards
  - Multi-path readable: one Bond language, many spenders

4.4 Leech
  Enemy-side drain fantasy. DoT, spread, execute, refunds into V/Host.

4.5 Symbiosis
  Host-side over-time boons while well-fed / bonded conditions hold.

4.6 Parasitism
  Host-side spend and low-HP breakpoints for power.

4.7 Molt
  Burst convert Bond → damage / plague / heal / V.

4.8 Graft
  Lasting attach: spore nest on enemy, carapace on Host, wound on Host, etc.

4.9 Overdraw (upgrade-gated)
  Spending past Safety Floor or spending Host HP directly.

4.10 Well-Fed / Starving (Symbiote states)
  Well-Fed: V above threshold (e.g. >50%)
  Starving: V below threshold (e.g. <25%) or recently empty
  Symbiote keystones hang buffs on Well-Fed and punishments on Starving

4.11 Blood Math (cohesion vocabulary — LOCKED intent)
  % health damage and related "blood weight" effects. Not a fourth path and not
  flat +%max HP on hit. Blood Math is a dialect of Bond / leech / Host state:

    Current HP%   — leech/tick riders on healthy / tanky bodies (elite pressure)
    Missing HP%   — finishers, Molt riders, execute bands (payoff after setup)
    Max HP%       — rare ICD- or Bond-gated spikes only (never unrestricted DPS)
    Host missing% — Parasite: your emptiness sharpens drain (Open Vein family)

  Rules of thumb:
    - %HP always rides Bond, leech, Molt, or Host-spend state — never baseline
    - Prefer PPS / ICD caps so trash does not evaporate and bosses stay readable
    - Differentiate player-low (Parasite power) from enemy-low (Leech execute)
    - Relationship fantasy still beats generic % damage stacking (craft pillar 1)

4.12 Buff / debuff honesty (risk ↔ reward)
  Every major self-buff should answer: "what did I risk or maintain?"
  Every major enemy debuff should answer: "what did the organism take from them?"
  Cards that fail both are glue — or cut candidates.

  Connected patterns (keep):
    Well-Fed / Starving  ↔  V economy (Covenant, Carapace, Graft, Culture)
    Scar Tissue          ↔  Host HP spend (Feed / Parasite tax)
    Open Vein            ↔  self DoT → enemy leech (blood bridge)
    Bond                 ↔  Hitch / Mark / Molt / Tap-at-cap (one stack language)

  Disconnection risks (fix):
    Symbiote DR/damage that never cares if anything is bleeding
    Generic damage-amp marks with no anemia / Blood Math identity
    Critical Host armed only by random chip, never by Helminth-sourced spend
    Ally fantasy sold as "shared blood" when only abstract V moves

================================================================================
5. DESIGN PILLARS (NON-NEGOTIABLE)
================================================================================

Core fantasy pillars (evaluation brief — must reinforce each other):

  A. Health/Vitality is the magazine (ammo costs health via buffer + Feed).
  B. Bullets bleed enemies (innate leech; paths amplify drain / spread / execute).
  C. Buffs for the Host, debuffs for enemies (relationship state, not free power).
  D. Blood Math: a portion of the upgrade path centers % health damage — but only
     as Bond/leech/Host-state riders (§4.11), never as flat unrestricted % sticks.

Supporting craft pillars:

  1. Relationship fantasy > flat % damage stacking.
  2. Three peer paths (Leech / Symbiote / Parasite) + glue; mix-and-match.
  3. Self-harm is opt-in depth, not stock grief.
  4. On-hit / Bond / Feed decisions matter more than pure stat sticks.
  5. Exotic modules are large, equal footprint, fantasy rebuilders.
  6. ~30 upgrades; fun legible "broken" hybrids over dry rails.
  7. Failure states stay fun (starve stutter, overfeed frenzy, bad molt timing).
  8. Co-op readable: ally grafts/heals telegraphed; Parasite self-harm is local.
  9. Prefer diminishing or non-stacking flags on refunds so multi-path hybrids
     are spicy, not unkillable infinite engines.
 10. Feed remains a real verb on all three pure leans in mixed content; if a lean
     never Feeds outside edge cases, retune refunds/culture down — do not delete Feed.
 11. Symbiote must not fully optionalize leech: at least one SY keystone or support
     should care about active leeches / Bond (organism is feeding, not just a DR buff).
 12. Damage mults use tagged channels with soft caps (blood-tax / bond-setup /
     covenant-state) — avoid unrestricted BeforeDamage factorial stacking.

================================================================================
6. UPGRADE PHILOSOPHY
================================================================================

6.1 Fluid themes — not skill trees

  - Themes = gravity wells for fantasy and balance review
  - No prerequisite chains, no UI exclusion matrix
  - Hybrid grids are first-class
  - Grid space + rarity footprint + soft tensions are the real constraints
  - Multi-tags allowed (e.g. a heal-on-leech-kill card = Leech + Symbiote glue)

6.2 Soft tension pairs (interesting, not banned)

  Mycelial Tap sustain  ↔  Hemophage bloodfire
    Refund comfort vs expensive spikes. Both can fit; rhythm fights itself.

  Mutual Covenant Well-Fed  ↔  Starve-the-Host / low-HP Parasite breakpoints
    Stay topped off vs live in the red.

  Jumping Leech clear  ↔  single-target Bond Molt ST
    Spread vs stack-and-detonate.

  Soft Mouth (cheap Feed)  ↔  Frenzy Feed / Overdraw power
    Gentle top-offs vs violent channels.

  Ally Graft aura  ↔  pure selfish Parasite lifesteal
    Team organism vs solitary predator.

  Blood Math on leech/Bond (elite ST)  ↔  jump-leech pack clear
    % current / missing HP pressure on marked bodies vs room-wide ticks.
    Both valid; ST lean needs Blood Math or Molt so tanks do not stall.

6.3 Universal truths (pool rules)

  - Target ~30 upgrades total for v1 ship pool
  - Support three distinct build lenses + hybrids
  - Exotic shapes larger than typical; all Exotics same cell count
  - Oddity grid-grow (Boundary Incursion) = 1-cell spatial, mission-stackable
  - Shared contraband (Edge Fault, Multiversal Thievery) optional parity later
  - Standards may CanStack where pure stats; identity keystones generally don't
  - Descriptions short, mechanical, slightly SAXON-wry
  - Blood Math is a cross-cutting dialect (§4.11), not a fourth gravity well
  - No unrestricted "+% max HP on hit" Standards — %HP is upgrade-gated and state-gated
  - Prefer retuning existing cards over exploding the frozen 30 when adding Blood Math

6.4 Rarity & footprint guidance

  Rarity       Typical role                         Footprint family
  -----------  -----------------------------------  --------------------
  Standard     Glue stats, small economy            Small (~3 cells)
  Rare         Meaningful identity pieces           Medium / Line
  Epic         Build-defining                       Large / Wide
  Exotic       Fantasy rebuilders                   Exotic (shared large)
  Oddity       Grid manipulation                    Tiny (1 cell grow)

================================================================================
7. BUILD LENSES (FLUID THEMES)
================================================================================

--------------------------------------------------------------------------------
7.1 Helpful Leech — enemies pay
--------------------------------------------------------------------------------

Fantasy:
  Tendrils in the swarm. You point; they bleed the bill. Sustain through drain,
  clear through jump-leech, elites through Bond stack → Molt.

What you lean into:
  DoT amp, multi-target spread, leech execute, V refunds, light Host heal on
  full drains, mark/anemia debuffs, Blood Math on Bond/leech (elite ST).

Keystones (strong pull):
  Mycelial Tap, Spore Lattice, Exsanguinate, Bond Molt (Leech-leaning)

Supports:
  Arterial Hitch, Jumping Leech, Anemic Mark, Siphon Cadence, Leech Efficiency,
  Secondary Mouth

Natural hybrids:
  + Symbiote: leech heals become team pulses; Well-Fed easier to maintain
  + Parasite: convert leech refunds into overdraw fuel; plague self-seed

Example lean:
  Mycelial Tap + Spore Lattice + Jumping Leech + Exsanguinate + Siphon Cadence
  → "I never Feed mid-fight; the room pays."

Success feel:
  Packs collapse under shared ticks; Vitality bar rebounds on kills without
  panic-feeding. Elites and bosses still melt under Bond + Blood Math / Molt,
  not only under trash density.

--------------------------------------------------------------------------------
7.2 Symbiote — mutualism
--------------------------------------------------------------------------------

Fantasy:
  Keep the organism happy and it improves you. Well-Fed carapace, ally grafts,
  calm culture growth. Starving feels like betraying a partner.

What you lean into:
  Threshold buffs (V high), passive V culture, DR/move/utility, ally heal/buff,
  anti-starve tools, longer comfortable uptime. At least one card should still
  care that the organism is feeding (active leeches / Bond) so SY does not
  optionalize pillar B.

Keystones (strong pull):
  Mutual Covenant, Graft Aura, Photosynth Carapace, Idle Culture

Supports:
  Well-Fed Protocols, Shared Pulse, Soft Mouth, Hive Calm, Symbiotic Rhythm,
  Secondary Mouth (multi)

Natural hybrids:
  + Leech: sustain engine that also protects the squad
  + Parasite: risky — Covenant wants high V while Parasite wants red Host;
    tense hybrid for experts

Example lean:
  Mutual Covenant + Photosynth Carapace + Soft Mouth + Shared Pulse + Idle Culture
  → "I'm the squad's wetware battery."

Success feel:
  Long fights without panic Feed; allies notice grafts; Starving is a mistake
  you feel immediately. Comfort must not erase Feed entirely in mixed content
  (Idle Culture + Soft Mouth watch) — pillar A stays universal.

--------------------------------------------------------------------------------
7.3 Parasite — the gun wins
--------------------------------------------------------------------------------

Fantasy:
  User fatality is a feature. Spend Host, break floors, frenzy, low-HP monsters.
  Highest ceiling; highest wipe rate. Carver's cousin at range.

What you lean into:
  Direct HP spend, floor lower, low-HP breakpoints, self-DoT → enemy plague,
  kill lifesteal (not passive regen), Overdraw Feed, damage-taken riders.
  Host missing-HP% sharpens drain (Blood Math on the operator side).

Keystones (strong pull):
  Hemophage Protocol, Bloodprice Rounds, Open Vein, Transfusion Invert

Supports:
  Starve the Host, Frenzy Feed, Critical Host, Scar Tissue, Crimson Efficiency

Natural hybrids:
  + Leech: bloodfire that refunds through enemy drain (volatile sustain)
  + Symbiote: difficult on purpose; Scar Tissue + Covenant can make "hurt to
    grow" builds

Example lean:
  Hemophage Protocol + Bloodprice Rounds + Critical Host + Open Vein + Scar Tissue
  → "If I'm not dying, I'm underperforming."

Success feel:
  Huge pulse damage and plague detonations bought with visible Host HP; clutch
  kill refunds feel filthy.

--------------------------------------------------------------------------------
7.4 Cross-theme glue
--------------------------------------------------------------------------------

  Vital Efficiency, Longer Tendrils, Soft Mouth, Hardened Stock, Pulse Metering,
  Secondary Mouth, Boundary Incursion, Handling/range staples

================================================================================
8. FULL UPGRADE CATALOG (~30 v1)
================================================================================

IDs: provisional 92001–92030 (gear 92000). Adjust at implement.
Theme tags: HL = Helpful Leech, SY = Symbiote, PA = Parasite, GL = Glue.
Numbers are STARTING TARGETS — validate in playtest. [HOT] = watch stacking.

Rarity guide: Standard / Rare / Epic / Exotic / Oddity
Cell rule: Exotic shapes larger than others; all Exotics same cell count.

--------------------------------------------------------------------------------
8.1 EXOTIC (6) — equal large footprint
--------------------------------------------------------------------------------

E1. Mycelial Tap                              id 92001    tags: HL
    Exotic keystone — Helpful Leech
    Leech ticks refund Vitality. At max Bond on a target, ticks also heal Host
    for a small amount (anti-heal-bot: heal only while that target is Bond-capped
    and tick is yours).
    Blood Math rider (cohesion): V refund may scale slightly with target
    current HP% (more from healthy elites, less from scraps) so pack-clear
    refunds do not fully delete Feed while tanks still pay the magazine.
    Starting targets:
      V refund / tick     1.5 – 2.5 V (base; optional current-HP% scale)
      Host heal / tick    1 – 2 HP at max Bond only
      Does not stack endlessly with identical refund cards — prefer highest
      or diminishing flag on V refunds from ticks (single V-refund channel).

E2. Mutual Covenant                           id 92002    tags: SY
    Exotic keystone — Symbiote
    While Vitality > 50% (Well-Fed): gain scaling damage resistance and a modest
    damage bonus. If you become Starving (V < 25%) or hit empty V, lose the buff
    and suffer a short Weak Pulse (reduced damage and/or move) until you return
    to Well-Fed.
    Cohesion rider (pillar B/C): while Well-Fed, DR and/or damage gain a small
    bonus per active leech you own (hard cap, e.g. +0–3 leeches). Keeps Symbiote
    from fully optionalizing bleed — the partner is happier when it is feeding.
    Starting targets:
      Well-Fed DR         12 – 18%
      Well-Fed damage     +8 – 12%
      Per-active-leech    small DR or damage crumb; cap ~3
      Weak Pulse duration 2.5 – 4 s on starve enter
    Readable HUD state mandatory.

E3. Hemophage Protocol                        id 92003    tags: PA
    Exotic keystone — Parasite
    Shots spend a portion of cost as Host HP directly (Overdraw) in addition to
    or instead of part of V cost — huge damage multiplier. Kills refund a portion
    of recent spend as Vitality and HP. Missing is expensive (HP already gone).
    Starting targets:
      Bonus damage        +45 – 65% while protocol active
      HP per shot         2 – 4 HP (on top of reduced V cost OR partial replace)
      Kill refund         35 – 50% of HP spent in last 1.5 s returned
      Safety floor        ignored for this spend (still can't shoot at 0 HP /
                          death-protection rules apply)
    Soft-locks if Host would die from the shot — clamp or deny fire at lethal.

E4. Spore Lattice                             id 92004    tags: HL, SY
    Exotic transformer — clear / team edge
    Leech ticks can jump once to a nearby enemy (reduced tick). If Cross-path
    with ally heal cards, jumped ticks that hit nothing may become a tiny ally
    mend pulse (optional rider — keep weak).
    Starting targets:
      Jump range          6 – 9 m
      Jump damage scale   55 – 70% of primary tick
      Max jumps           1 (upgrade backlog could raise)

E5. Bond Molt                                 id 92005    tags: HL, PA
    Exotic transformer — ST payoff / Blood Math spend
    Activate (tap reload when not holding Feed, or threshold auto — prefer
    intentional tap alternate): consume Bond on the aimed/last target to detonate
    a Molt burst scaled by Bond stacks. Parasite lean: if Host below 40% HP,
    Molt radius/damage increased.
    Blood Math: burst = (flat x Bond) + small (% current or % max HP x Bond),
    with a PPS/ICD-style sanity cap so Molt remains punctuation, not a delete
    button on every trash pack. Prefer % current on full tanks; % missing as
    optional execute spice only if flat+Bond already paid setup cost.
    Starting targets:
      Damage / Bond       ~18 – 28 flat
      %HP / Bond          tiny (tune); hard cap per Molt
      Max Bond considered 8
      Low-HP bonus        +25 – 40% damage/radius below 40% Host
      Cooldown            3 – 5 s or Bond rebuild gate
    Must not fully replace shooting; it's a punctuation mark.

E6. Graft Aura                                id 92006    tags: SY
    Exotic transformer — co-op
    While Well-Fed, nearby allies periodically gain a short Graft (minor DR or
    tiny regen). You pay a continuous small Vitality upkeep while allies are in
    range (organism sharing).
    Starting targets:
      Radius              8 – 12 m
      Ally DR             6 – 10%
      Pulse interval      2 – 3 s
      V upkeep            1 – 2 V/s while >=1 ally in range
    Starving disables aura immediately.

--------------------------------------------------------------------------------
8.2 EPIC (8)
--------------------------------------------------------------------------------

P1. Exsanguinate                              id 92007    tags: HL
    Leech ticks deal more damage. Targets below 30% HP take amplified leech and
    grant bonus V on leech kill.
    Blood Math split: keep coefficient tick amp for general pressure; add a
    missing-HP% execute rider on leech ticks or leech kill (small, capped) so
    finishers read as drain, not only "more DPS numbers." Do not double-dip
    full missing-% on both every tick and Molt without diminishing.
    Tick damage         +40 – 60%
    Execute leech mult  +30 – 50% below 30% HP
    Execute % missing   small rider below 30% (PPS/kill cap)
    V on leech kill     +8 – 14 V

P2. Photosynth Carapace                       id 92008    tags: SY
    While Well-Fed, gain a regenerating carapace layer (small absorb or DR stack
    that rebuilds out of damage). Breaking carapace spends a little V.
    Cohesion rider: rebuild delay shortens slightly while you own >=1 active
    leech (organism feeding → shell regrows). Optional; keep modest so SY+HL
    hybrid is rewarded without making Carapace a leech tax stick.
    Absorb / DR band    modest (not full shield primary)
    Rebuild delay       3 – 5 s out of damage (- while leeching, floor ~2 s)
    Break tax           4 – 8 V

P3. Bloodprice Rounds                         id 92009    tags: PA
    Each shot can pay extra Host HP for bonus damage (toggle or always-on lean).
    Not full Hemophage — smaller tax, stacks tension with Hemophage [HOT if both].
    Prefer: Bloodprice adds optional Overpay hold-fire modifier OR flat small HP
    rider with +damage; if both Hemophage + Bloodprice, diminish combined tax.
    HP rider            1 – 2 HP/shot
    Damage              +20 – 30%

P4. Jumping Leech                             id 92010    tags: HL
    On leech kill (tick or pulse kill while tick active), spawn a short-lived
    spore that applies a fresh weak leech to the nearest unmarked enemy.
    Jump search radius  10 – 14 m
    Weak tick scale     40 – 55%

P5. Idle Culture                              id 92011    tags: SY
    While not firing and Well-Fed, Vitality regenerates without Host HP (culture
    growth). Firing pauses culture briefly.
    Culture V/s         3 – 6 V/s
    Pause after fire    0.6 – 1.0 s
    Does not work Starving

P6. Open Vein                                 id 92012    tags: PA, HL
    Apply a self Wound while firing (Host DoT). Pulse hits spread a portion of
    that Wound as bonus leech on enemies (Internal Combustion / Immolator DNA).
    Blood Math: convert ratio scales up with Host missing HP% (your emptiness
    sharpens the drain). Primary bridge between self-harm and enemy bleed.
    Self DoT            low, telegraphed
    Convert ratio       50 – 80% base of recent self-tick → enemy leech on hit
    Missing-HP scale    up toward top of band as Host HP falls (cap the curve)
    Stop firing clears self Wound after short tail

P7. Shared Pulse                              id 92013    tags: SY, HL
    When you gain Host heal from any Helminth source (Tap max-Bond heal, kill
    refunds, etc.), pulse a percentage to nearby allies.
    Share ratio         40 – 60% of heal amount
    Radius              10 – 14 m

P8. Transfusion Invert                        id 92014    tags: PA
    Kill refunds that would heal you can be inverted: hold a control (or auto
    when full HP) to bank refunds as a Blood Charge that empowers next Molt or
    next 3 pulses instead of HP.
    Bank cap            1 strong charge or 3 pulse buffs
    Pulse buff          +25 – 40% damage
    Gives Parasite something to do at full HP

--------------------------------------------------------------------------------
8.3 RARE (10)
--------------------------------------------------------------------------------

R1. Arterial Hitch                            id 92015    tags: HL
    Hitting a target already under Leech Tick builds bonus Bond and briefly
    slows that target's movement slightly (anemia hitch).
    Cohesion: also slightly extends leech duration on hitch (or resists cleanse
    if the game exposes it) so the debuff is more than Bond++.
    Bonus Bond / hitch  +1
    Slow                mild, 1 – 1.5 s refresh
    Tick duration       +0.2 – 0.4 s on hitch refresh (small)

R2. Anemic Mark                               id 92016    tags: HL, GL
    At 3+ Bond, target is anemic: modestly increased damage from all sources
    (team setup) plus a Blood Math leech rider — leech ticks deal a tiny
    % current HP (hard PPS cap) and/or the target receives reduced healing if
    the API allows. Duration refreshes with Bond.
    Do not ship as generic +damage-taken only; the anemia fantasy is the point.
    Damage taken amp    +8 – 12%
    Bond threshold      3
    Leech % current     tiny per tick; strict PPS cap (anti-trash delete)
    Heal cut (optional) mild if supported; else skip

R3. Well-Fed Protocols                        id 92017    tags: SY
    Well-Fed threshold improves (buffs activate at 40% V instead of 50%) and
    Starving threshold lowers (starve at <15% instead of 25%). Comfort card.
    Enables easier Covenant uptime.

R4. Soft Mouth                                id 92018    tags: SY, GL
    Feed converts HP → V more efficiently (less HP per V). Feed channel slightly
    slower (gentle draw).
    HP cost             -25 – 35%
    Channel speed       -10 – 15% (longer channel, cheaper total)

R5. Frenzy Feed                               id 92019    tags: PA
    Feed is faster and can Overdraw below Safety Floor down to a lower hard
    floor (e.g. 5%). While Overdrawing, gain damage and take increased damage.
    Feed speed          +40 – 60%
    Overdraw damage     +15 – 25%
    Damage taken        +10 – 18% during Overdraw Feed
    Hard floor          5% HP

R6. Critical Host                             id 92020    tags: PA
    Melodramatic Hero DNA: after reaching critical Host HP (e.g. <25%), your
    next few pulses gain huge damage, tighter spread, and on-hit Host heal.
    Can't retrigger until you've been above 60% HP again.
    Cohesion: prefer arming when critical HP was reached via Helminth-sourced
    spend (Feed / Hemophage / Bloodprice / Open Vein) OR generic low HP — but
    give a clearer / stronger arm if the gun drew the blood, so the relationship
    owns the moment more than random chip.
    Empowered pulses    3 – 5
    Damage              +50 – 80%
    Heal / hit          3 – 6 HP

R7. Siphon Cadence                            id 92021    tags: HL, GL
    Every Nth pulse that hits a leeched target refunds bonus V (cadence drum).
    N                   3 – 4
    Bonus V             5 – 8

R8. Scar Tissue                               id 92022    tags: PA, SY
    After spending Host HP via Feed or Parasite taxes, gain brief DR. Rewards
    getting hurt by your own gun.
    DR                  10 – 16% for 1.5 – 2.5 s after HP spend
    Refreshable

R9. Longer Tendrils                           id 92023    tags: GL, HL
    +Range / softer falloff. Leech tick duration slightly increased.
    Falloff start/end   +20 – 30%
    Tick duration       +0.4 – 0.7 s

R10. Pulse Metering                           id 92024    tags: GL, PA
    Fire interval improved (faster pulse) but V cost per shot increased slightly.
    RoF                 +12 – 18%
    V cost              +10 – 15%
    Parasite likes spending; Leech must refund harder.

--------------------------------------------------------------------------------
8.4 STANDARD (5)
--------------------------------------------------------------------------------

S1. Vital Efficiency                          id 92025    tags: GL
    Max Vitality increased.
    Max V               +15 – 25
    Stackable lightly (CanStack) if desired

S2. Secondary Mouth                           id 92026    tags: GL, SY
    Passive drip improved (more V/s from Host while above floor). Still tiny vs Feed.
    Drip                +40 – 70% of baseline drip

S3. Hardened Stock                            id 92027    tags: GL
    Recoil down, ADS/hip spread improved slightly. Pure handling glue.

S4. Leech Efficiency                          id 92028    tags: HL, GL
    Baseline leech tick damage modestly increased.
    Tick                +15 – 25%

S5. Crimson Efficiency                        id 92029    tags: PA, GL
    Parasite HP taxes reduced slightly (Hemophage/Bloodprice/Open Vein self costs).
    Tax mult            x0.85 – 0.9
    Does nothing if no HP-spend cards equipped (dead card risk — acceptable glue
    for Parasite leans; alt: also slightly improves kill HP refunds)

--------------------------------------------------------------------------------
8.5 ODDITY (1)
--------------------------------------------------------------------------------

O1. Boundary Incursion                        id 92030    tags: GL
    Adds a row or column to the upgrade grid.
    Vanilla-style GridGrow: IsSpatial | CanStackInMission, priority -100, 1 cell.

--------------------------------------------------------------------------------
8.6 v1 frozen pool checklist (30)
--------------------------------------------------------------------------------

  EXOTIC (6)
    1  Mycelial Tap
    2  Mutual Covenant
    3  Hemophage Protocol
    4  Spore Lattice
    5  Bond Molt
    6  Graft Aura

  EPIC (8)
    7  Exsanguinate
    8  Photosynth Carapace
    9  Bloodprice Rounds
    10 Jumping Leech
    11 Idle Culture
    12 Open Vein
    13 Shared Pulse
    14 Transfusion Invert

  RARE (10)
    15 Arterial Hitch
    16 Anemic Mark
    17 Well-Fed Protocols
    18 Soft Mouth
    19 Frenzy Feed
    20 Critical Host
    21 Siphon Cadence
    22 Scar Tissue
    23 Longer Tendrils
    24 Pulse Metering

  STANDARD (5)
    25 Vital Efficiency
    26 Secondary Mouth
    27 Hardened Stock
    28 Leech Efficiency
    29 Crimson Efficiency

  ODDITY (1)
    30 Boundary Incursion

--------------------------------------------------------------------------------
8.7 Backlog (designed vocabulary, not in first 30)
--------------------------------------------------------------------------------

  Hive Calm            — status resist while Well-Fed
  Symbiotic Rhythm     — reload-adjacent utility / swap speed while Well-Fed
  Starve the Host      — max V down, all damage up (permanent lean)
  Nesting Graft        — leech leaves ground spore puddle (AcidPuddle DNA)
  Twin Proboscis       — +1 pellet pulse, split V cost
  Autonomic Feed       — auto-Feed when V < X and Host > floor (QoL / danger)
  Hemolymph Bomb       — Molt leaves lingering plague zone
  Cordyceps Hint       — killed leeched enemies briefly distract neighbors
  Hemolymph Weight     — while Bond >= X, pulses deal bonus % current HP once per
                         Y s (named Blood Math toy; ICD-gated; elite opener)
  Sanguine Ledger      — track recent Host HP spent; next Molt or max-Bond tick
                         consumes ledger as bonus % missing HP damage
  Edge Fault / Thievery — contraband grid parity
  Match-Grade wetware  — tiny ADS precision glue if UI needs a starter card

  If a Blood Math toy must enter the frozen 30, prefer replacing pure glue or
  promoting a retune of Anemic Mark / Exsanguinate / Bond Molt first.

================================================================================
9. EXAMPLE BUILDS (ILLUSTRATIVE ONLY)
================================================================================

Grid space is the real limit. Teaching sketches, not meta decrees.

  A. Pure-ish Helpful Leech
     Mycelial Tap + Spore Lattice + Exsanguinate + Jumping Leech
     + Siphon Cadence + Leech Efficiency + Arterial Hitch
     → Room pays the magazine.

  B. Pure-ish Symbiote
     Mutual Covenant + Graft Aura + Photosynth Carapace + Idle Culture
     + Soft Mouth + Well-Fed Protocols + Shared Pulse
     → Squad wetware; never Starving on purpose.

  C. Pure-ish Parasite
     Hemophage Protocol + Bloodprice Rounds + Open Vein + Critical Host
     + Frenzy Feed + Scar Tissue + Transfusion Invert
     → Red-bar monster; clutch or crater.

  D. Hybrid — Medic Leech
     Mycelial Tap + Shared Pulse + Graft Aura + Jumping Leech + Soft Mouth
     → Drain heals you; you bleed heals outward.

  E. Hybrid — Plague Bloodfire
     Hemophage + Open Vein + Spore Lattice + Exsanguinate + Bond Molt
     → Hurt yourself; paint the room; molt the elite.

  F. Hybrid — Tense Covenant Parasite
     Mutual Covenant + Scar Tissue + Bloodprice + Idle Culture + Soft Mouth
     → Expert: keep V high while skimming Host HP for spikes; Scar Tissue
       forgives the skim. Easy to brick if greedy.

  G. Hybrid — ST Molt Surgeon
     Bond Molt + Arterial Hitch + Anemic Mark + Exsanguinate + Longer Tendrils
     + Mycelial Tap
     → Stack Bond, anemia + Blood Math for team, molt punctuation on elites.

  H. Hybrid — Blood Math Anchor (post-cohesion)
     Anemic Mark + Exsanguinate + Bond Molt + Open Vein + Arterial Hitch
     + Mycelial Tap
     → % current on marked ticks, missing-% execute, Host-missing sharpens Vein,
       Molt spends Bond. Feed still required when the room thins.

Many other combinations are valid.

================================================================================
10. ECONOMY RULES OF THUMB
================================================================================

  - Full V should feel like a short magazine of commitment, not an SMG belt.
  - Feed mid-combat is a vulnerable channel — reward positioning.
  - Leech V refunds should enable skilled players to skip Feed in rich packs,
    not delete the Feed verb entirely in all content.
  - Symbiote Well-Fed must be maintainable in normal play without Parasite taxes.
  - Parasite kill refunds prevent death spirals if you play well; they must not
    create infinite full-HP loops with Tap + Shared Pulse without diminishing.
  - Stack watch list (playtest):
      Mycelial Tap + Siphon Cadence + Exsanguinate kill V
      Hemophage + Bloodprice + Pulse Metering HP drain
      Covenant DR + Carapace + Scar Tissue + Graft Aura (unkillable squad)
      Shared Pulse + Tap heal + Critical Host heal (healbot)
      Anemic Mark % current + Exsanguinate missing-% + Bond Molt %HP (Blood Math pile)
      Idle Culture + Soft Mouth + Secondary Mouth (Feed verb deleted)
  - Prefer flags: non-stacking V-refund channel, heal effectiveness soft cap
    per second, DR diminishing returns.

--------------------------------------------------------------------------------
10.1 Systemic anti-loop & scaling rules (cohesion — implement / enforce)
--------------------------------------------------------------------------------

  V refunds
    - One logical V-refund channel with diminishing returns across Tap, Siphon,
      Exsanguinate kill V, and crumbs — not three independent full faucets.
    - Prefer refunds tied to leech participation / Bond / recent Feed tax, not
      raw kills alone (density should not fully replace skill).

  Healing
    - Soft cap Helminth-sourced heal PPS before Shared Pulse fans out.
    - Tap max-Bond heal stays small and conditional.

  Damage mult channels (BeforeDamage)
    - Tag and soft-cap: blood-tax (Hemophage/Bloodprice/Overdraw), bond-setup
      (Anemic Mark), covenant-state (Well-Fed / Weak Pulse), hero-window
      (Critical Host / Invert). Avoid raw multiply-all stacking.

  Blood Math / %HP
    - Always Bond-, leech-, Molt-, or Host-state-gated; never baseline pulse.
    - ICD or PPS caps on % current; missing-% for execute/Molt; max-% rare+ICD.
    - Differentiate Host-missing (Parasite power) from enemy-missing (Leech finish).

  Self-damage
    - Soft ceiling on Host HP/sec spent by the gun (Hemophage + Bloodprice +
      Open Vein + Feed) so clutch play is not pure binary wipe.
    - HUD should separate Host tax from enemy chip when possible.

  Feed universality
    - All three pure leans should still want Feed sometimes in mixed content.
    - If Leech never Feeds in packs AND bosses, ST tools are too weak or refunds
      too strong — retune, do not accept "Feed is Parasite-only."

  Bond
    - Keep cap/decay tight so Mark + Molt + Tap-heal cannot be permanent uptime
      machines without upkeep hits.

Safety / lethality:
  - Never allow a single pulse to spend lethal HP if death-protection should save
  - Clamp Hemophage shot deny when HP < shot tax + 1
  - Feed deny at Safety Floor (unless Frenzy/Hemophage rules say otherwise)
  - Multiplayer: Host HP spend is owner-authoritative; Bond/leech on enemies
    must respect existing damage authority patterns

================================================================================
11. STRENGTHS, WEAKNESSES & PLAYER FANTASY CHECKLIST
================================================================================

Strengths
  - Unique magazine fantasy (Vitality / Host)
  - High skill expression (Feed timing, Bond molt, low-HP windows)
  - Three peer identities + rich hybrids
  - Co-op value on Symbiote / Shared Pulse / Anemic Mark
  - Memorable audio-visual organism fantasy
  - Failure states are readable (starve stutter, Weak Pulse, overfeed frenzy)
  - Pillars A–C already mutually reinforcing; Blood Math (D) braids via Bond/leech

Weaknesses
  - Punished by chip damage + greedy Feed (double resource anxiety: Host HP + V)
  - Weaker brain-off spray when Starving
  - Weaker synergy with pure ammo-on-damage character cards
  - Requires custom HUD literacy (many states — keep primary bar = V)
  - Parasite lean raises wipe rate in pubs
  - Not top pure single-shot deletion (AMR job) or pure hose (Cycler job)
  - Flat leech without Blood Math / Molt plateaus on tanks and bosses
  - Sustain engines (refund + heal + culture) want to go infinite if uncapped
  - Symbiote comfort path can optionalize bleed if no leech-aware SY cards

Pillar grades (design review 2026-08-11)

  A Health-as-ammo     Strong — Vitality buffer + Feed + floor is correct model
  B Bleed / leech      Strong baseline; clear rich, ST/elite thinner without D
  C Buffs / debuffs    Strong when tied to V/Bond/HP spend; literacy risk
  D Blood Math / %HP   Was weak / not centered — integrate via §4.11 + catalog
                       retunes; do not spray flat %max Standards

Player fantasy checklist (success criteria)

  [ ] Unupgraded gun feels hungry but fair; Feed is a real verb
  [ ] Empty V soft-lock is annoying, not confusing or lethal
  [ ] Light leech tick is visible and clearly "the gun tasting"
  [ ] Leech lean can skip Feed in dense packs without infinite godmode
  [ ] Symbiote lean makes Well-Fed feel like a partner buff you maintain
  [ ] Parasite lean makes red HP feel powerful and scary
  [ ] Hybrids feel smart, not "wrong"
  [ ] Bond Molt is a highlight, not every trigger
  [ ] Graft Aura is noticed by allies in co-op
  [ ] No lean fully erases Vitality economy without Hemophage-level cost
  [ ] Safety floor prevents accidental suicide Feed on stock gun
  [ ] Exotic moments are fight-defining
  [ ] On a single tank, Bond + leech (+ Blood Math) matters more than mag-dump alone
  [ ] On a pack, refunds can replace Feed without full HP lock / godmode
  [ ] On a boss phase with no trash, player still has Molt / execute / %HP plan
  [ ] Symbiote player can explain how the organism (not just "DR buff") helps
  [ ] Parasite biggest hits are visibly paid for in Host HP
  [ ] No build reaches infinite V + infinite HP without deliberate broken stacking
  [ ] %HP effects never feel like generic damage stickers — always blood-weighted

================================================================================
12. VISUAL, AUDIO & THEMATIC DESIGN
================================================================================

12.1 Appearance

  Base:
    Industrial SAXON pulse rifle chassis with exposed wetware: translucent
    vitality bladder along the receiver, capillary lines to the magwell (Feed
    port), faint peristaltic motion when V is high. Fungal-biotech hybrid —
    not "gun is a mushroom," more "gun has a pet organ."

  Leech lean:
    Tendril muzzle petals flare on hit; tick victims show thin spore threads.

  Symbiote lean:
    Bladder glows warm when Well-Fed; carapace microplates along stock;
    ally graft = soft green-gold pulse (colorblind-safe pattern, not color alone).

  Parasite lean:
    Bladder darkens/redlines as Host HP drops; capillary flush on Overdraw;
    Hemophage shots leave darker spit trails.

  Current implementation note:
    Runtime clone will use Cartridge SMG mesh until AssetBundle art.
    Design assumes the wetware rifle above.

12.2 Sound

  Pulse fire: wet-mechanical thump, organic valve hiss — not dry brass.
  Empty V: dry click + unhappy gurgle.
  Feed channel: draw/suck, heartbeat underlay proportional to Host HP stress.
  Leech tick: soft tick-tick on victims (subtle; mix under combat).
  Bond Molt: wet burst / spore cough.
  Well-Fed idle: low contented hum; Starving: arrhythmic click.
  Hemophage shot: heavier wet crack + Host pain stinger (short, not annoying).

12.3 HUD

  Literacy rule: one primary bar (Vitality) + one contextual strip (Bond / state).
  Bury secondary states until the player owns cards that use them. Too many
  simultaneous pips (Overdraw, Critical, Carapace, Invert, Weak Pulse, Molt)
  will make the gun feel like a spreadsheet.

  Required:
    - Vitality bar (primary resource)
    - Safety floor tick mark on Host HP bar if possible, else on V panel
    - Well-Fed / Starving state pip (when SY cards present; always OK if cheap)
    - Bond pips on target or last-target strip (when Bond-using cards present)
  Optional / contextual:
    - Feed channel progress
    - Overdraw warning stripe
    - Molt ready glyph
    - Host-tax vs enemy-damage differentiation (Parasite clarity)
    - Active leech count crumb (Covenant / Carapace cohesion riders)

================================================================================
13. IMPLEMENTATION MAP
================================================================================

Live project: .content.primary.HelminthReceiver (sacred loop + full pool registered).
Tune combat hooks and upgrade properties to this bible; Blood Math retunes are
design-locked intent — implement when touching the listed cards.

  Plugin.cs                     BepInEx entry, sandbox flag, registration timing
  WeaponRegistration.cs         Clone base gun, GearInfo, ApplyHelminthStats
  HelminthBehaviour.cs          Data host: V, Bond, flags, Feed state, leech
  HelminthCombatHooks.cs        Fire gate, OnFiredBullet, damage/heal, Update tick
  HelminthHUD.cs                Vitality / Bond / state (SparrohUILib optional)
  SpawnGearHooks.cs             Equip remap + identity stamp
  HelminthUpgrades.cs           Register ids 92001–92030
  HelminthUpgradeProperties.cs  Apply/Remove + stat ranges (glue)
  HelminthPathRareProperties.cs Path Rares Apply/Remove
  HelminthEpicProperties.cs     Epics Apply/Remove
  HelminthExoticProperties.cs   Exotics Apply/Remove
  HelminthUpgradePatterns.cs    Hex footprints
  UpgradeRegistration.cs        CreateUpgrade helper

Behaviour.Data sketch:

  float vitality, maxVitality, vitalityPerShot;
  float feedHpPerVitality, feedRate, passiveDripRate;
  float safetyFloorFraction;
  float leechDpsFraction, leechDuration;
  int bondPerHit, bondCap;
  bool hemophage, mycelialTap, mutualCovenant, sporeLattice, ...;
  // path floats: refund rates, DR, aura radius, Blood Math riders, etc.

Hooks (expected):
  Gun fire / CanFire / ammo spend suppress
  Gun.Update — drip, culture, aura upkeep, Wound ticks
  OnFiredBullet / OnDamageTarget — leech apply, Bond, cadence
  OnKillTarget — Jumping Leech, refunds, Exsanguinate
  Player damage/heal APIs — Feed tax, Hemophage tax, heals (audit decompile)
  Reload input — Feed channel (Swarm Portable Solar / Globbler siphon pattern)

GunData baseline sketch:
  hasLimitedAmmo = false OR huge mag + useAmmoOnFire = 0
  damage / fireInterval per §3.2
  autoReloadWhenEmpty = false
  refillAmmoOnReload = false

Persistence:
  Gear ID is save key. Register AllGear before PlayerData.AddGear; re-bind.

Multiplayer:
  Same mod + matching ids on all clients. Owner-auth Host HP. Sandbox flag.

MycoMod:
  [MycoMod(null, ModFlags.IsSandbox)] — changes combat economy.

================================================================================
14. OPEN TUNING QUESTIONS (PLAYTEST, NOT DESIGN BLOCKERS)
================================================================================

  1. Exact V max and cost/shot so Feed frequency feels right on 10m / 20m fights.
  2. Whether baseline Bond should display always or only with Bond cards.
  3. Molt input: tap reload vs weapon ability vs automatic at cap.
  4. Hemophage + Bloodprice combined tax curve.
  5. Graft Aura V upkeep vs Idle Culture regen — avoid infinite aura at standstill
     without allies (upkeep only when ally present — already specified).
  6. Leech on parts vs brain — readability vs anatomy synergy with other mods.
  7. Should passive drip exist at all, or only Feed + refunds? (Currently tiny drip.)
  8. Critical Host threshold vs Covenant Starving threshold clarity in HUD.
  9. Promote/demote any Epic↔Exotic after first play duals.
 10. Art pass priority: bladder HUD + muzzle tendrils before full gun mesh?
 11. Blood Math numbers: % current on Anemic Mark ticks vs Molt % split — which
     carries elite DPS fantasy without deleting trash time-to-kill?
 12. Covenant per-leech crumb: DR vs damage — which reads better in co-op?
 13. Critical Host: Helminth-sourced arm only, or hybrid with generic low HP?
 14. Live defaults vs §3.2 bands (V/shot, leech fraction, crumbs) — pull live
     back to hungry baseline or rewrite bands after playtest?

================================================================================
15. DELIBERATE NON-GOALS
================================================================================

  - No forced keystone exclusion matrix
  - No stock direct HP-per-shot (Hemophage earns it)
  - No pure ammo primary loop
  - Support/heal is not the only viable identity
  - Not rewriting enemy prefabs for new organs
  - Not cloning Carver blood stacks 1:1 (Bond is related but ranged/DoT-native)
  - Not shipping Edge Fault / Multiversal Thievery in v1
  - No unrestricted flat %max HP on hit as a Standard/stat stick
  - No fourth peer path UI — Blood Math is vocabulary inside the three wells
  - No baseline %HP on unbuilt gun (taste leech only)

================================================================================
16. REVIEW DECISIONS LOCKED
================================================================================

  [x] Name: Helminth Receiver
  [x] Economy: Vitality buffer fed by Host HP (+ hold-reload Feed)
  [x] Baseline DoT: light innate leech tick on hit
  [x] Fire mode: mid-RoF organic pulse rifle
  [x] Doc depth: full ~30 catalog with number bands
  [x] Three paths: Helpful Leech / Symbiote / Parasite — fluid mix
  [x] Safety floor on stock gun; Parasite may Overdraw
  [x] Empty V = soft lock, not death
  [x] Exotics (6) equal large shapes
  [x] ~30 v1 pool frozen in §8.6; backlog in §8.7
  [x] Blood Math: %HP via Bond/leech/Host state only (§4.11) — not flat sticks
  [x] Pillar cohesion pass 2026-08-11: catalog retunes + systemic anti-loop rules
  [x] Buff/debuff honesty: risk/maintain ↔ organism-taken (§4.12)
  [x] Feed remains universal verb; Symbiote must not fully optionalize leech

================================================================================
17. ONE-PAGE SUMMARY
================================================================================

  Helminth Receiver is Mycopunk's organic risk primary. It fires mid-rate
  vitality pulses, tastes enemies with a light leech tick, and refills by
  feeding on the operator. The hex grid is three gravity wells — Helpful Leech,
  Symbiote, Parasite — built to mix. Exotics rebuild the relationship; Standards
  glue the organism; nothing is a railroad.

  Four fantasy pillars lock together:
    A. Health/Vitality is the magazine.
    B. Bullets bleed (leech).
    C. Host buffs and enemy debuffs are the relationship.
    D. Blood Math (%HP) rides Bond, leech, Molt, and Host state — never flat.

  Tune until feeding feels like a decision, leeching feels like a conversation,
  Blood Math feels like weight in the blood (not a damage sticker), and every
  hybrid still reads as the same hungry SAXON Type-L bondgun.

================================================================================
18. COHESION PASS NOTES (2026-08-11)
================================================================================

  Problem: pillars A–C were strong; D (% health damage) was not centered. Naive
  +%max HP cards would fight pillar "relationship > flat % stacking."

  Resolution:
    - Define Blood Math as shared vocabulary (§4.11), not a fourth path.
    - Retune Anemic Mark, Exsanguinate, Bond Molt, Open Vein, Mycelial Tap,
      Mutual Covenant, Photosynth Carapace, Arterial Hitch, Critical Host.
    - Enforce systemic caps (§10.1): one V-refund channel, heal PPS, mult tags,
      %HP ICD/PPS, Feed universality, Bond decay discipline.
    - Expand fantasy checklist for tank / pack / boss / infinite-loop cases.
    - Backlog named toys (Hemolymph Weight, Sanguine Ledger) if a dedicated
      Blood Math card is still needed after retunes.

  Implementation priority when coding the pass:
    1. Anemic Mark Blood Math rider + Exsanguinate missing-% split
    2. Bond Molt %HP component + Open Vein missing-Host scale
    3. Covenant/Carapace leech-aware crumbs
    4. V-refund / heal PPS / damage-channel soft caps
    5. Critical Host Helminth-sourced arm preference
    6. HUD literacy pass (primary V + contextual strip only)
