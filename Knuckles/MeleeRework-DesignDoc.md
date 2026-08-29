# MeleeRework — Design Document (v1)

> Status: **Design only** — implementation follows this doc.
> Package / Thunderstore name: **MeleeRework**
> GUID (planned): `sparroh.meleerework`
> MycoMod flags: **IsSandbox** (gameplay rules + loadout architecture)
>
> Scope lock: **P0 (slot + Fists) + P3 (polish / fate / hooks proof)**.
> Not shipping Carver or Wrench as melee kits in this mod.
> Those (and any future melee) consume the **extension hooks** documented here.
>
> Sibling docs (separate projects, not absorbed):
>   - Blood Carver — continuous saw primary (future melee convert candidate)
>   - Saxonite Wrench — gravity slam primary (future melee convert candidate)
>   - DMLR Rework — path/grid discipline reference
>
> Vanilla anchors:
>   - `GearType.Melee` already exists
>   - `MeleeGear : Throwable` at `player.Gear[4]`
>   - Input: `PlayerInput.Controls.Player.Melee` (V by default)
>   - Size via `GunData.bulletMagnetismTarget`; damage via `GunData` + `ModifyMeleeData`
>   - Handspring DNA: `UpgradeVariables.meleeJumpForce` / `meleeJumpExplodeSize`

---

## 1. High Concept / Fantasy

Melee is no longer a joke side button. It is a **loadout slot**.

You always bring fists. Tap **V** for a quick jab without leaving your gun. Hold **V** to put the fists up — full control, a real guard, and a grid that turns knuckles into economy, tempo, or utility.

Future tools (saws, wrenches, whatever SAXON bolts on later) plug into the same slot. This mod only ships the platform and the default kit.

**One-liner:** *Tap V to jab. Hold V to put 'em up. The slot is the product; Fists are the proof.*

---

## 2. Role in the Arsenal

| | |
|--|--|
| **Slot** | Melee (`GearType.Melee`) — first-class loadout pick |
| **v1 kit** | **Fists** only (reworked universal punch) |
| **Range** | Extreme close (reach-limited; no soft falloff tax inside reach) |
| **Role** | Always-available finish / interrupt / proc engine; optional full-equip brawler mode |
| **Gap filled** | Vanilla punch is underpowered and has no home for upgrades; melee cards are scattered across characters/guns with no shared fantasy |
| **Not trying to be** | A primary weapon, Carver, Wrench, or a per-gun Fisticuffs injection pack |

**Product shape:** Systems + content mod. Not a cloned primary weapon template.

---

## 3. Scope Lock

### In (v1 ship)

1. **Melee equip slot** — loadout always has a melee kit equipped (default Fists)
2. **Input contract** — tap V quick-melee; hold V equip-to-hands
3. **Fists baseline** — respectable damage/size/feel with zero upgrades
4. **Fists full-equip RMB** — **Guard**: damage resistance while holding block
5. **Fists upgrade grid** — ~30 cards, 3 paths, upgrades live on Fists
6. **Extension hooks** — register future `GearType.Melee` kits without rewriting input/slot
7. **P3 polish** — gear UI, swap feel, sandbox MP, vanilla melee-card fate, CQC modifier sanity, balance

### Out (explicit non-goals)

- Shipping Carver or Saxonite Wrench as melee options
- Moving vanilla Carver off Primary
- Mass new melee cards on Cycler / Jackrabbit / etc.
- Full character skill-tree melee redesign
- Replacing Handspring / Beehive / From Down Town fantasies
- Making Fists out-DPS dedicated melee primaries in their own kits

### Locked decisions

| Decision | Lock |
|----------|------|
| Package name | **MeleeRework** |
| Hold-V threshold | **~0.25s** (playtest 0.2–0.3) |
| Full-equip Fists RMB | **Guard / damage resist** (baseline) |
| Quick-V RMB | N/A — gun still out; V is attack only |
| v1 kits | **Fists only** |
| Future kits | Hooks only; content in other mods/projects |
| Cross-gun melee injection | **Out of v1** |
| Character melee exotics | **Keep** on characters |
| Vanilla weapon melee cards | **Leave vanilla** (optional backlog redirect) |

---

## 4. Design Pillars

1. **Slot first, kit second** — architecture must admit a second melee without a rewrite.
2. **Baseline Fists must not suck** — zero-upgrade punch clears trash / finishes / feels good.
3. **Tap vs hold is the skill split** — quick jab never requires full equip; full equip is a commitment.
4. **Upgrades live on the melee gear** — Fists grid owns brawler fantasy; don't pollute primaries.
5. **On-hit / on-kill verbs > flat % stickers** — economy, windows, utility.
6. **Three peer paths on Fists; hybrids intended** — no anti-synergy matrix.
7. **Guard is the full-equip identity beat** — RMB block/DR teaches "hands up" vs "gun out jab."
8. **Don't steal Carver/Wrench niches** — no continuous saw, no gravity slam primary loop on Fists.
9. **~30 upgrades;** exotic shapes larger than others; each exotic the **same** cell count.
10. **Respect vanilla character spice** — Handspring et al. stay character-owned.
11. **Sandbox, self-contained v1** — no hard deps on Carver/Wrench/Turbocharges mods.
12. **Hooks are real** — a smoke-test second kit (dev-only or hidden) proves registration.

---

## 5. Loadout & Slot Architecture

### 5.1 Gear model

```
Player loadout (conceptual):
  [Primary] [Heavy] [Utility/Grenade] [MELEE] [Character…]

Melee slot:
  - GearType.Melee
  - Runtime home: player.Gear[4] (vanilla MeleeGear index — verify on implement)
  - Always occupied; default = Fists
  - Swappable in Gear Selection like other slots (UI work in P3)
```

### 5.2 Fists identity

| Field | Draft |
|-------|--------|
| Display name | **Fists** |
| APIName | `fists` (or keep vanilla melee API name if rebinding in place — prefer stable `fists`) |
| Type string | Melee |
| Base type | Reworked `MeleeGear` (patch-in-place and/or catalog clone with Melee type) |
| Unlock | Always unlocked |
| XP / grid | Yes — full upgrade grid like a real weapon |
| Ammo | None |

### 5.3 Catalog rules

- Only `GearType.Melee` entries appear in the melee slot list.
- v1 list: **Fists**.
- Future kits call into MeleeRework registration hooks (see §12).
- Primaries stay primaries; this mod does **not** reclassify Carver.

### 5.4 Persistence

- Equipped melee kit id saved with loadout (same GearData patterns as other slots).
- Fists upgrades persist by gear id like any CreateUpgrade gear.
- Missing future kit on load → fall back to Fists (never empty slot).

---

## 6. Input Contract

### 6.1 Binding

- Default key: **V** (`Player.Melee`)
- Hold threshold: **0.25s** (config-tunable later)

### 6.2 Behavior matrix

| State | Tap V (< threshold) | Hold V (≥ threshold) | M1 | RMB | R | Weapon swap / primary fire |
|-------|---------------------|----------------------|----|-----|---|----------------------------|
| Gun out (normal) | **Quick melee** (Fists jab); stay on gun after anim | **Equip Fists** to hands | Gun | Gun aim/alt | Gun reload | — |
| Fists equipped (hands up) | **Quick jab** in-place (same family) | Refresh equip / ignore second hold | **Punch** (full profile) | **Guard** (DR) | Unused baseline* | **Stow Fists**, return to previous gun |
| Blocking (RMB held, fists up) | Cancel block → jab **or** ignore tap (prefer: release block first) | — | Break guard → punch | Hold guard | — | Stow + cancel guard |

\* R reserved for future Fists exotics (none required in frozen 30). Overload Contingency-style charged melee is path-owned, not baseline hold-R.

### 6.3 Quick melee (tap V)

- Does **not** change active weapon slot long-term.
- Plays punch anim / hitbox from equipped melee kit's **quick profile**.
- Fists quick profile = snappy single strike (vanilla MeleeGear DNA, buffed numbers).
- Can interrupt sprint per vanilla rules; should feel panic-safe.
- If hold threshold crossed mid-press → promote to equip (don't also fire a wasted quick hit if possible; prefer: charge-to-equip without double attack).

### 6.4 Full equip (hold V)

- Stow current non-melee gear (primary/heavy/etc. as vanilla swap does).
- Enable melee gear `IGear.Enable` — fists in hands, full input map.
- HUD shows Fists state (guard available, optional stack displays from upgrades).
- Exit: weapon select keys, scroll, shooting primary bind, or dedicated stow — match game's existing swap UX where possible.

### 6.5 Controller

- Same tap/hold on melee face button.
- Guard = left trigger or aim-equivalent while fists equipped (map to whatever Aim is while melee active).
- Document final binds in README after implement.

### 6.6 Edge cases

| Case | Rule |
|------|------|
| Menus / dead / downed | No melee equip |
| Ability anim lock | Queue or deny equip (deny safer v1) |
| Grapple / flight (Scrapper/Glider) | Quick-V allowed if vanilla allows melee; full equip allowed unless it breaks ability (playtest) |
| Already fists up + hold V | No-op or slight ready flourish |
| Swap spam | Short equip cooldown ~0.15–0.25s optional anti-flicker |

---

## 7. Fists — Core Mechanics & Gunfeel

### 7.1 Two profiles, one kit

| | Quick-V | Full equip |
|--|---------|------------|
| Intent | Panic jab / finish / proc without stowing gun | Commit to melee range; punch + guard |
| Damage | Baseline punch (see table) | Same base hit **or** +small full-equip bonus (≤10–15%) so equip feels rewarded — tune in playtest |
| Size / reach | Baseline | Same; upgrades can grow both |
| Chain | Single hit per tap (vanilla cadence) | Can chain punches on M1 with recovery; still discrete, not a saw |
| RMB | N/A | **Guard** |
| Upgrade hooks | OnMeleeHit / Kill from either profile | Same events + OnGuard / WhileGuarding |

### 7.2 Baseline combat stats (starting targets — VALIDATE IN PLAYTEST)

| Trait | Draft target | Intent |
|-------|--------------|--------|
| Damage | Noticeably above vanilla meme tier; ~chunk a basic grunt in 2–3 clean hits or 1 heavy finisher feel | Must not be laughing stock |
| Reach (`rangeData.maxDamageRange`) | Slight buff if vanilla feels stubby (~+10–20%) | Still extreme close |
| Size (`bulletMagnetismTarget`) | Modest multi-target forgiveness | Pack slap without Carver AOE |
| Hitstop | Keep / slightly punchier on enemy hit | Juice |
| Swing rate / cooldown | If Throwable cooldown makes spam awful, ease slightly | Still has cadence |
| Falloff | **None** inside legal reach | Limitation is range, not soft tax |
| Element | None baseline | Path B / cards add elements |
| Ammo | None | Forever loaded |

Exact vanilla numbers to be read from prefab at implement and multiplied — doc stays qualitative + relative.

### 7.3 Baseline RMB — Guard

**Only while Fists are fully equipped.**

| Piece | Draft |
|-------|--------|
| Input | Hold RMB (Aim) |
| Effect | **Damage resistance** while guarding |
| Baseline DR | ~25–35% vs most damage (playtest; not immortal) |
| Move | Slight slow while guarding (~10–20%) so it's a stance, not a sprint shield |
| Duration | As long as held; no baseline stamina meter |
| Break | Optional: heavy hits chip through (backlog); v1 can be flat DR |
| Offensive | No baseline chip damage on block; parry/riposte is upgrade-owned |
| Visual | Hands up / guard pose; clear audio bed |
| Cancel | Release RMB; M1 punch; stow fists; quick-V policy per §6 |

**Intent:** Full equip has a verb gun-out mode lacks. Guard teaches commitment and enables Path C / glue cards (riposte, guard-break explode, guard → economy).

**What Guard is NOT:** Carver deflector, infinite safety, or a replacement for positioning.

### 7.4 Baseline loop (no upgrades)

```
Gun out:  fight normal → tap V to jab finishers / CQC panic → back to shooting
Hot pile: hold V → fists up → M1 punch pack → hold RMB to brace chip →
          stow when gun is better → repeat
```

Skill without upgrades: spacing, when to commit hands-up, when jab-from-gun is enough, not face-tanking with mediocre baseline DR.

### 7.5 What baseline does NOT include

- Free Handspring / melee jump
- Free charged hold-R mega punch
- Blood meter, saw ticks, gravity wells
- Innate element fists
- Innate ammo refund on hit (upgrade-owned)
- Innate lifesteal

---

## 8. Shared Framework (events upgrades speak)

Fists (and future kits) should emit a common vocabulary so properties stay portable.

| Event / state | Fired when | Used for |
|---------------|------------|----------|
| **OnMeleeHit** | Quick or full punch damages enemy | Status, stacks, gun windows*, charge |
| **OnMeleeKill** | Punch kills brain/part per design rules | Chunks, reload, ability drip |
| **OnMeleeCoreHit** | Punch hits enemy core | Anatomy payoffs |
| **OnMeleeSurface** | Punch hits world surface | Jump / utility (mostly upgrade-gated) |
| **OnGuardStart / End** | RMB guard edge | Stance cards |
| **WhileGuarding** | Each damage instance taken under guard | DR already baseline; riposte meters |
| **MeleeEmpowered** | Temporary size/damage window | Path B crowns, ability handoffs |
| **ChargedMelee** | Upgrade-owned charged strike | Path C / Overload DNA |

\* Gun windows from Fists cards buff *your equipped primary* briefly — economy stays on Fists grid, not on the gun's grid. That preserves "upgrades on melee" while still letting knuckles feed guns.

Host (implement): `FistsBehaviour` or `MeleeKitBehaviour` on the melee gear + optional player-side router for quick-V when gun is active.

---

## 9. Fists Upgrade Paths (gravity wells)

### Path A — KNUCKLE ECONOMY
**"Every hit pays the rest of the kit."**

- Spine: melee hit/kill → mag ammo, reserve, grenade charge, ability recharge, heavy drip
- Crown: **Payroll** — kills while fists up (or any melee kill) trigger a strong multi-resource pulse
- Hybrid: Tempo builds more hits → more pay; Utility setups easier executes → more pay

### Path B — BRAWLER TEMPO
**"Hands up means the room gets smaller."**

- Spine: size, damage, swing rate, multi-hit, element fists, empower windows after guard/swap/kill
- Crown: **Haymaker Protocol** — after guard or on stack threshold, next punch is a heavy haymaker (size + damage + hitstop)
- Hybrid: Economy scales off haymaker kills; Utility adds launch to haymaker

### Path C — IMPACT UTILITY
**"The punch is a tool."**

- Spine: surface jump (Handspring-like on Fists grid), knockback, stagger, charged melee, guard upgrades, fuse-start DNA, pull-lite
- Crown: **Pile Driver** — look-down melee blast / launch (Handspring + explode size DNA owned here as Fists exotic, without deleting Wrangler Handspring)
- Hybrid: Economy on utility kills; Tempo makes utility hits actually kill

### Path × verb matrix

```
                 ECONOMY              TEMPO                 UTILITY
Hit             ammo/charge drip     size/dmg/element      stagger/knockback
Kill            multi-resource       empower window        chunk/setup leftover
Guard           DR→refund cards      riposte haymaker      guard duration/move
Surface         —                    —                     jump / blast
Full equip      passive pay aura     stance dmg            tool kit unlocked
```

---

## 10. Crowns & Signature Cards

### Payroll — Exotic (Economy crown)
- On melee kill: refund primary mag ammo + small grenade charge + tiny ability charge
- Stronger if fists fully equipped
- The "why CQC modifier is fun" card

### Haymaker Protocol — Exotic (Tempo crown)
- After releasing Guard, or at N Brawler stacks: next M1 is Haymaker
- Large size + damage; long hitstop; brief self-hitch
- Consumes stacks / has recovery so it isn't hold-RMB forever → delete

### Pile Driver — Exotic (Utility crown)
- While fists equipped (or any melee profile — prefer equipped for clarity): melee surface below (look-down) launches upward
- Optional small explode size stat (Handspring explode DNA)
- Coexists with Wrangler Handspring (character can still own a version; stacking rules: max force wins or additive with soft cap — playtest)

### Iron Guard — Exotic (glue)
- While guarding: higher DR + chance to nullify projectile **or** minor reflect (pick one primary fantasy in impl; recommend **nullify chance**, not full Carver deflector)
- Moving while guarding less punished

### Glass Jaw Tax — Epic (risk)
- +Punch damage; −DR while fists equipped (not while gun out)
- For Tempo freaks who don't want Guard crutch

### Boundary Incursion — Oddity
- Grid grow — keep universal pattern

---

## 11. Full Upgrade List (~30 ship + backlog)

Rarity: Standard / Rare / Epic / Exotic / Oddity  
Tags: E Economy · T Tempo · U Utility · G Guard/glue  
Cell rule: Exotics larger; all Exotics same cell count.  
Names are player-facing (full rename where not vanilla ports).

------------------------------------------------------------------------------
PATH A — KNUCKLE ECONOMY                                 [~9]
------------------------------------------------------------------------------

A1. Payroll — Exotic (Keystone)
    Melee kills refund primary ammo + grenade charge + trickle ability charge.
    Bonus if fists fully equipped.

A2. Magazine Knuckle — Rare
    Melee hits refund a small amount of current-weapon magazine ammo.

A3. Reserve Knuckle — Standard
    Melee kills refund reserve ammo (weaker than Payroll; stacks as drip).

A4. Fuse Tap — Rare
    Melee hits generate grenade charge (Impact Cascade DNA, not fire-gated).

A5. Core Tap — Rare (Core Dump DNA on Fists)
    Melee damage to enemy cores refunds mag ammo (stronger than generic hit drip).

A6. Heavy Handshake — Epic
    Melee kills drip heavy weapon ammo (modest; Carver still owns feast fantasy).

A7. Second Wind Jab — Epic
    Melee kills partially recharge employee movement ability.

A8. Combat Accounting — Standard
    Slightly increased ammo/charge gains from all Fists economy cards.

A9. Gun-Out Bonus — Rare
    Economy effects stronger on **quick-V** hits (reward jab-without-swap).

A10. Hands-Up Bonus — Rare
    Economy effects stronger while **fully equipped** (reward commit).

------------------------------------------------------------------------------
PATH B — BRAWLER TEMPO                                   [~9]
------------------------------------------------------------------------------

B1. Haymaker Protocol — Exotic (Keystone)
    After Guard or at stack threshold: next punch is a Haymaker.

B2. Lead Hands — Standard
    +Melee damage.

B3. Open Hand — Standard
    +Melee size (bulletMagnetismTarget).

B4. Longer Reach — Standard
    +Melee reach (maxDamageRange). Still no falloff.

B5. String Combo — Rare
    Full-equip M1 recovery shortened; mild multi-hit window.

B6. Element Knuckles — Rare
    Punches apply a rolled element (Fire / Shock / Acid on apply). One card.

B7. Blood Rush — Epic
    On melee kill: brief +move speed and +punch damage.

B8. Brawler Stacks — Epic (BrawlerStacks DNA owned on Fists)
    Hits grant stacks → +damage and +size with soft DR curve / power curve.
    Decays out of combat 1-at-a-time (Carver blood lesson).

B9. Swap-In Spike — Rare
    On equip fists (hold V complete): brief empower window.

B10. Finishing Bell — Epic
    Bonus damage to low-HP targets with melee.

------------------------------------------------------------------------------
PATH C — IMPACT UTILITY                                  [~8]
------------------------------------------------------------------------------

C1. Pile Driver — Exotic (Keystone)
    Look-down melee surface launch (+ optional explode size).

C2. Launch Pad — Rare
    +Pile Driver / melee jump force (supports character Handspring too if both present).

C3. Shockwave Knuckle — Epic
    Melee hits emit a tiny kinetic pulse (small radius; not Wrench replacement).

C4. Shove — Rare
    +Melee hit force / knockback.

C5. Skull Check — Rare
    Melee hits vs cores/elites apply brief stagger / interrupt bias.

C6. Deliberate Tap — Rare (Scrapper DNA portable)
    Melee explosive parts: start fuse + increased explosion size (if not already
    covered when Scrapper tree owned — still OK on Fists for other characters).

C7. Overload Fist — Epic (Overload Contingency DNA on Fists)
    Hold R while fists equipped to charge a larger melee attack (size scales).
    Releases on R release or cap. Does **not** steal RMB from Guard.

C8. Aftershock — Standard
    Surface melee hits briefly empower next enemy melee hit.

------------------------------------------------------------------------------
GUARD / GLUE / GUNFEEL                                   [~8]
------------------------------------------------------------------------------

G1. Iron Guard — Exotic
    +Guard DR; projectile nullify chance while guarding.

G2. Guard Training — Standard
    +Baseline Guard DR.

G3. Mobile Guard — Rare
    Reduced move penalty while guarding.

G4. Riposte — Epic
    On releasing Guard after absorbing damage: next punch empowered (feeds Haymaker).

G5. Brace Accounting — Rare
    While guarding, damage taken grants a small economy drip (ammo/charge).

G6. Punch Punch+ — Rare (vanilla port/buff optional)
    Melee kills drop additional health chunks (align with vanilla Punch Punch;
    if vanilla remains, this card may be cut — see fate table).

G7. Quickdraw Jab — Standard
    Faster quick-V startup / shorter gun-return gap.

G8. Soft Hands — Rare
    Taking damage shortly after equipping fists grants brief Guard DR without RMB
    (panic brace).

G9. Boundary Incursion — Oddity
    +Upgrade grid size.

------------------------------------------------------------------------------
FROZEN v1 SHIP POOL (exactly 30)
------------------------------------------------------------------------------

  EXOTIC (5)
    1  Payroll
    2  Haymaker Protocol
    3  Pile Driver
    4  Iron Guard
    5  (flex) Brawler Stacks as Exotic if stack UI needs crown weight
       — RECOMMENDED: keep Brawler Stacks Epic; 5th exotic = **Overload Fist**
       Final exotic five: Payroll, Haymaker Protocol, Pile Driver, Iron Guard, Overload Fist

  EPIC (7)
    6  Heavy Handshake
    7  Second Wind Jab
    8  Blood Rush
    9  Brawler Stacks
    10 Finishing Bell
    11 Shockwave Knuckle
    12 Riposte

  RARE (11)
    13 Magazine Knuckle
    14 Fuse Tap
    15 Core Tap
    16 Gun-Out Bonus
    17 Hands-Up Bonus
    18 String Combo
    19 Element Knuckles
    20 Swap-In Spike
    21 Launch Pad
    22 Shove
    23 Mobile Guard
    24 Brace Accounting
    — trim to 11: drop Hands-Up Bonus OR Gun-Out Bonus to backlog if needed
    RECOMMENDED RARE 11:
      Magazine Knuckle, Fuse Tap, Core Tap, Gun-Out Bonus,
      String Combo, Element Knuckles, Swap-In Spike, Launch Pad,
      Shove, Mobile Guard, Brace Accounting

  STANDARD (6)
    25 Lead Hands
    26 Open Hand
    27 Longer Reach
    28 Reserve Knuckle
    29 Guard Training
    30 Quickdraw Jab

  ODDITY (1) — if 30 already full, Boundary Incursion replaces Reserve Knuckle
    Prefer: cut one Standard to keep Boundary Incursion in frozen 30.

  RECONCILED FROZEN 30:

    EXOTIC (5)
      1  Payroll
      2  Haymaker Protocol
      3  Pile Driver
      4  Iron Guard
      5  Overload Fist

    EPIC (7)
      6  Heavy Handshake
      7  Second Wind Jab
      8  Blood Rush
      9  Brawler Stacks
      10 Finishing Bell
      11 Shockwave Knuckle
      12 Riposte

    RARE (11)
      13 Magazine Knuckle
      14 Fuse Tap
      15 Core Tap
      16 Gun-Out Bonus
      17 String Combo
      18 Element Knuckles
      19 Swap-In Spike
      20 Launch Pad
      21 Shove
      22 Mobile Guard
      23 Brace Accounting

    STANDARD (6)
      24 Lead Hands
      25 Open Hand
      26 Longer Reach
      27 Guard Training
      28 Quickdraw Jab
      29 Combat Accounting

    ODDITY (1)
      30 Boundary Incursion

BACKLOG (designed, expand later)
  Reserve Knuckle, Hands-Up Bonus, Skull Check, Deliberate Tap, Aftershock,
  Soft Hands, Punch Punch+ (if vanilla Punch Punch insufficient),
  Glass Jaw Tax, Skull Check, element-specific knuckles, ally-guard aura,
  parry timing window, guard stamina variant, multiplayer buddy brace

------------------------------------------------------------------------------
CUT / DEMOTE FROM EARLIER MELEE REWORK IDEAS
------------------------------------------------------------------------------

| Idea | Fate |
|------|------|
| Per-primary Fisticuffs cousins | **Cut** from this mod |
| Ship Carver/Wrench as melee | **Cut** — hooks only |
| Baseline free ammo on kill | **Cut** — Payroll/Magazine Knuckle |
| Baseline free Handspring | **Cut** — Pile Driver exotic |
| RMB = shove baseline | **Cut** — RMB is Guard; Shove is card |
| Second Rage-like meter | **Cut** — Brawler Stacks only if taken |

---

## 12. Extension Hooks (future melee kits)

MeleeRework is the **platform**. Other mods/projects register kits.

### 12.1 Goals

- Second `GearType.Melee` gear can appear in melee slot list
- Tap V runs that kit's **quick profile**
- Hold V equips that kit's **full profile**
- Fists remain default fallback
- No requirement to ship Carver/Wrench inside this package

### 12.2 Design contract (implement as stable API / conventions)

```
IMeleeKit (conceptual):
  GearType => Melee
  QuickAttack()          // tap V while another weapon active
  OnFullEquip()          // hold V completed
  OnFullUnequip()
  // Optional overrides:
  SupportsGuard          // Fists true; saw may false
  QuickAttackCancelsEquipCharge
```

Registration (conceptual):

```
MeleeRework.API.RegisterKit(IGear catalogEntry)
MeleeRework.API.SetDefaultKit(fists)
// Slot filter: GearType.Melee only
// Persistence: gear id in loadout melee index
```

### 12.3 Input ownership rules

| Kit equipped | Tap V | Hold V | M1 | RMB | R |
|--------------|-------|--------|----|-----|---|
| Fists | Quick jab | Equip fists | Punch | Guard | Overload Fist if card |
| Future saw | Quick rip | Equip saw | Saw | Kit-defined | Kit-defined |
| Future wrench | Quick tap smash | Equip wrench | Slam/charge | Pull | Kit-defined |

MeleeRework owns **only** tap/hold detection and slot swap. Kit owns attacks.

### 12.4 Convert-later checklist (Carver / Wrench)

When a future project converts a primary → melee:

1. Set `GearType.Melee`; remove from Primary gear lists / drops as needed
2. Implement quick profile (short saw burst / light smash) ≠ full kit dump
3. Call `RegisterKit`
4. Ensure upgrade grid stays on that gear (already true)
5. Dual-wield fantasy: primary slot frees up for a gun — call that out in that kit's doc
6. Network: spawn/equip path same as Fists full equip
7. Do **not** require MeleeRework to know kit-specific blood/torque state

### 12.5 Hook proof (P3)

- Dev-only or hidden duplicate Fists entry **or** null kit smoke test that registers and appears in list
- CI/manual: equip non-default kit id → tap/hold still route
- Document for modders in README section "Adding a melee kit"

---

## 13. Vanilla Melee Inventory & Fate Table

### Character

| Upgrade | Owner | Fate |
|---------|-------|------|
| Handspring | Wrangler | **Keep** — character mobility; stacks softly with Pile Driver |
| The Ole One-Two | Wrangler | **Keep** |
| Beehive! | Scrapper | **Keep** |
| Deliberate Mistake | Scrapper | **Keep** (Fists backlog may mirror for non-Scrappers) |
| Wrecking Ball | Scrapper | **Keep** (grapple swing — not Fists) |
| From Down Town | Glider | **Keep** |

### Universal

| Upgrade | Fate |
|---------|------|
| Core Repurposing | **Keep** — works with Fists kills |
| Scrap Repurposing | **Keep** |
| Punch Punch | **Keep** — Fists Punch Punch+ stays backlog to avoid triple-dip |

### Weapon / grenade / heavy

| Upgrade | Owner | Fate |
|---------|-------|------|
| Fisticuffs | Shocklance | **Leave vanilla** |
| Overload Contingency | Swarm Launcher | **Leave vanilla**; Fists has Overload Fist |
| Hot Boxing | Incendiary | **Leave vanilla** |
| Impact Cascade | Incendiary | **Leave vanilla**; Fists Fuse Tap is generic |
| Core Dump | Laser Cannon | **Leave vanilla**; Fists Core Tap is generic |

### Mission

| Modifier | Fate |
|----------|------|
| Close Quarters Combat | **Support** — baseline Fists + Payroll make it real |

### Systems

| System | Fate |
|--------|------|
| Vanilla MeleeGear punch | **Rework numbers + dual profile + guard** |
| GearType.Melee | **Elevate to real loadout slot** |
| player.Gear[4] | **Keep index unless forced; document** |

---

## 14. Example Builds (Fists)

### Payroll Operator (gun still primary)
Magazine Knuckle → Fuse Tap → Core Tap → Gun-Out Bonus → **Payroll** → Combat Accounting → Quickdraw Jab  
*Play:* Stay on gun; tap V for reloads and grenade drip; hold V only in emergencies.

### Haymaker Brawler
Lead Hands → Open Hand → Brawler Stacks → String Combo → **Haymaker Protocol** → Riposte → Iron Guard → Blood Rush  
*Play:* Hold V in the pile; guard → release → haymaker; stacks up between swings.

### Pile Driver Scout
Longer Reach → Launch Pad → **Pile Driver** → Shove → Shockwave Knuckle → Swap-In Spike → Second Wind Jab  
*Play:* Vertical fight; slam look-down; jab on landing; fists as mobility tool.

### Hybrid CQC
Payroll + Haymaker + Iron Guard + Fuse Tap + Finishing Bell  
*Play:* The poster loadout for Close Quarters Combat modifier.

---

## 15. Strengths, Weaknesses, Risks

### Strengths
- Clear input fantasy (tap vs hold)
- Upgrade home that isn't twelve gun grids
- Guard gives full-equip identity
- Hooks unlock Carver/Wrench later without blocking v1
- CQC modifier becomes honest

### Weaknesses / fun failure states
- Full equip in open sightlines = death (by design)
- Guard DR too strong → turtle meta (tune)
- Economy too strong → guns never reload normally (cap drips)
- Hold-V mis-taps equip when player wanted jab (threshold + cancel rules)

### Design risks
- Gear select UI may not expose Melee slot cleanly — P3 critical path
- Throwable-based MeleeGear vs Gun-based future kits — API must abstract
- Animation: guard pose may need borrowed anims
- MP: equip state + guard DR authority
- Overlap Payroll vs vanilla Core/Scrap Repurposing — diminishing returns preferred

---

## 16. Success Criteria / Player Fantasy Checklist

- [ ] Melee is a visible equippable loadout slot
- [ ] Fists equipped by default; always available
- [ ] Tap V jab feels useful with **zero** upgrades
- [ ] Hold V (~0.25s) puts fists in hands with clear feedback
- [ ] Stow returns to previous gun reliably
- [ ] RMB while fists up = Guard with readable DR
- [ ] Fists grid shows ~30 upgrades; 3 paths feel distinct
- [ ] Payroll build reloads guns via knuckles
- [ ] Haymaker build has a crunchy commit button
- [ ] Pile Driver enables vertical play without Wrangler
- [ ] CQC mission modifier feels supported
- [ ] A second melee kit can register via hooks (proof)
- [ ] Vanilla character melee exotics still work
- [ ] Failure states stay fun (not empty meter, not soft-locked hands)
- [ ] Sandbox MP: clients see punches/equip without hard desync

---

## 17. Implementation Notes (for coding passes)

### 17.1 Project shape

- Retarget away from weapon-clone template over time:
  - Keep CreateUpgrade patterns from upgrade template
  - Drop "new primary CartridgeSMG clone" as the product
- ConfigManager for: Enable, hold threshold, baseline damage mult, guard DR
- `[MycoMod(null, ModFlags.IsSandbox)]`
- GUID `sparroh.meleerework`

### 17.2 Core patches / systems

| Area | Approach |
|------|----------|
| Tap vs hold V | Prefix/postfix on melee input / `MeleeGear` ability path; timer on press |
| Quick attack | Existing `MeleeGear.FireBullet` path when not full-equipped |
| Full equip | `Player` gear swap to melee index; `IGear.Enable` |
| Guard | While fists active + Aim held: subscribe `OnBeforeTakeDamage` DR |
| Baseline buffs | Multiply `GunData` on Fists prefab/instance after upgrades apply |
| Events | Postfix damage/kill from MeleeGear; guard callbacks |
| Upgrades | `PlayerData.CreateUpgrade` on FindGear("fists") |
| UI | GearSelectionWindow melee slot filter `GearType.Melee`; HUD guard hint |
| Hooks | Static API class + register list |

### 17.3 Important vanilla types

- `MeleeGear`, `Throwable`, `GunData`
- `GearType.Melee`
- `BrawlerStacks` (reference for size/damage stacking)
- `Player.ModifyMeleeData`
- `UpgradeVariables.meleeJumpForce` / `meleeJumpExplodeSize`
- `GearSelectionWindow` / `GearSlot`
- `IGear.Enable` / `Disable` / equip flow on `Player` (read full on implement)

### 17.4 Network

- Equip melee = same authority as weapon swap
- Guard DR owner-predicted with server accept if needed
- Mark sandbox; all clients need mod for fair MP

### 17.5 Phased delivery

| Phase | Deliverable |
|-------|-------------|
| P0a | Tap/hold V + full equip/stow Fists |
| P0b | Baseline damage/size/reach buffs + juice |
| P0c | Guard RMB DR |
| P0d | Fists gear identity + grid registration + frozen 30 |
| P3a | Gear select melee slot UX |
| P3b | Hooks API + registration proof |
| P3c | Vanilla fate verification + CQC feel |
| P3d | Balance pass + config knobs + README |

---

## 18. Open Tuning Questions (playtest, not design blockers)

1. Baseline hits-to-kill on common grunt (target 2–3?)
2. Guard DR 25% vs 35% vs 40%
3. Guard move slow 10–20%
4. Full-equip damage bonus 0 vs 10%
5. Hold threshold 0.2 vs 0.25 vs 0.3 (controller)
6. Payroll ammo numbers vs vanilla reload upgrades
7. Brawler stack cap and decay interval
8. Haymaker vs Overload Fist role overlap — keep both if charge (R) ≠ guard-release (RMB)
9. Pile Driver vs Wrangler Handspring stacking rule
10. Whether quick-V can crit/finisher mult separately

---

## 19. Relationship to Sibling Projects

| Project | Relationship |
|---------|----------------|
| **Blood Carver** | Remains primary saw fantasy until a convert pass uses §12 hooks |
| **Saxonite Wrench** | Remains primary slam fantasy until convert pass |
| **DMLR / other reworks** | No hard coupling; Fists economy may buff any held gun |
| **SparrohsTurbocharges** | Optional later turbo on Fists exotics; not required |
| **Weapon template in this folder** | Scaffold only; product is systems+Fists, not new primary |

Future Carver-as-melee and Wrench-as-melee get their own design deltas:

- Slot: Melee not Primary  
- Quick-V profile required  
- Depend on MeleeRework hooks (or merge packages later)

---

## 20. Deliberate Non-Goals (repeat for scanners)

- No second shipped kit in MeleeRework v1  
- No primary-slot competition with guns for Fists  
- No deleting vanilla character melee identity  
- No making Guard a full deflector saw shield  
- No blood/torque meters on Fists  

---

## 21. Design Changelog

### v1 (this doc)

- Product: MeleeRework = melee slot platform + Fists kit
- Scope: P0 + P3; hooks for future kits; no Carver/Wrench ship
- Input: tap V quick melee; hold V 0.25s equip
- Full-equip RMB: **Guard / DR**
- Fists ~30 upgrades across Economy / Tempo / Utility + Guard glue
- Vanilla melee cards: keep characters/universal; leave weapon cards vanilla
- Extension API sketched for future melee registration
- Package name locked: **MeleeRework**

---

## 22. Next Steps After This Doc

1. Implement P0a–P0c (input, buffs, guard) against decompile
2. Register Fists grid frozen 30
3. P3 UI + hooks proof
4. Playtest baseline TTK + Guard DR + Payroll economy
5. When ready: Carver/Wrench convert docs reference §12

---

*End of MeleeRework Design Doc v1*
