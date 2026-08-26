# Aussie Special – Design Document (v1.1)

## 1. High Concept / Fantasy

A break-action hopping shotgun that lives on ricochets, fires from two independent triggers by default, or dumps the whole mag into a blight hose.

Baseline is dual-trigger shells that wake up after a bounce. Open maps get a seeking ricochet crown. Hold the hose and the magazine becomes fuel.

**One-liner:** Bounce the room. Twin the barrels. Burn the rest.

**Product shape:** New primary weapon (**Aussie Special**). Does **not** replace vanilla Au-Si Jackrabbit / BounceShotgun.


## 2. Role & Fantasy in the Arsenal

- **Slot:** Primary
- **Range:** Close to mid (shotgun falloff; bounce and slug extend effective angles)
- **Role:** Deliberate dual-trigger pellet volume, ricochet geometry, fire application / flamethrower fuel hose
- **Gap filled:** Vanilla Jackrabbit funnels real damage into Double Barrel + Double Time + Ghosting + Slideshot, neuters pre-bounce hits hard (×0.75), and strands bounce fantasy on wall-less maps. Pesticide is a great idea bolted onto AIM with no integration. Aussie Special makes double-barrel the gun, not the tax card.
- **Synergies:** Movement (slide, airborne backboost), fire/immolation self-loops, shock-on-bounce riders, co-op ignite auras (turbo DNA optional later)

**Not trying to be:** long-range DMR, pure infinite hose without economy, full-auto spray primary, or a second heavy weapon.


## 3. Design Pillars

1. **Post-bounce is the mythic hit** — full damage + fire after ricochet. Pre-bounce is softened so empty grid is playable, not a brick.
2. **Open maps must not delete the fantasy** — Seeking Shell (exotic) homes pellets after a bounce; Ghosting remains an optional pierce branch, not the only answer.
3. **Double barrel is the gun** — LMB/RMB, shared mag 2, ADS removed on baseline. Dual-trigger is identity, not an exotic unlock.
4. **Two crowns + baseline hopper** — Ricochet (Seeking) / Break-Action tempo supports / Blightflame (M1 hose). No mandatory exotic to “become” the shotgun.
5. **Damage is distributed** — many Standard/Rare/Epic cards carry meaningful damage, pellets, bounce mult, fire appl, and economy. Slideshot and dual-trigger are identity/tempo, not DPS crutches.
6. **Slug is a first-class transformation** — baseline stays multi-pellet; slug trades volume for single-projectile punch and readable bounces.
7. **Pesticide is a fire mode, not an ADS parasite** — Blightflame owns M1; pellets/slug disabled while equipped; hose **consumes ammo per second**.
8. **Fire / immolation is the elemental spine** — Shock only as bounce riders (e.g. Opening Charge DNA). Acid/Decay are out by default.
9. **~30 upgrades for v1** — exotic shapes larger than others; each exotic same cell count; paths intermingle.
10. **Full rename pass** — new player-facing names (vanilla echoes only in the fate table).


## 4. Core Mechanics & Gunfeel

### 4.1 Base gun

| Trait | Draft / intent |
|---|---|
| Fire mode | **Dual-trigger break-action** pellet shotgun (not full-auto) |
| Damage | Modest per pellet; pays off after bounce |
| Bullets per shot | 6 pellets per trigger pull |
| Fire interval | Per-barrel cadence (~0.3s spirit from last shot of that barrel); shared global interval OK as v1 simplification |
| Magazine / reserve | **Shared mag 2**; reserve loop kept (not heat-style infinite) |
| Reload | Standard reload beat; fills both chambers together |
| Max bounces | 1 baseline |
| Bullet speed | Fast rail-style pellets (ShotgunBounceBullet / RailBullet DNA) |
| Element | Fire after bounce (identity) |
| ADS | **None** on baseline (aim animations off) |
| Model / audio | Borrow BounceShotgun until custom art |

### 4.2 Bounce neuter rules (LOCKED direction)

Vanilla `ShotgunBounceBullet`:
- Pre-bounce: damage × **0.75**, strips fire to `defaultEffect` / amount (unless ApplyFireToCores on a core)
- Post-bounce: full damage + fire; optional falloff reset / per-bounce damage flags

**Aussie Special baseline:**
- Pre-bounce damage multiplier: **~0.90×** (softened; tune 0.88–0.92 in playtest)
- Pre-bounce still largely **non-fire** (fire remains the bounce mythic beat) unless a card says otherwise
- Post-bounce: **1.0×** damage + full fire application
- Optional tiny post-bounce bonus on baseline: none required if 0.90× pre feels fair

**Intent:** empty-grid gun clears packs; bounce still feels like the “real” shell waking up.

### 4.3 Inputs (baseline)

| Input | Role |
|---|---|
| **M1** | Left barrel — one shell from shared mag |
| **RMB** | Right barrel — one shell from shared mag (does **not** ADS) |
| **R tap** | Reload |
| **R hold** | No baseline special (Molotov / Pummeler crowns own hold-R) |
| **Slide** | Movement only until Slideshot-type card equipped |
| **ADS** | Disabled on baseline |

### 4.4 Input map when crowns equipped

| Crown | M1 | RMB | ADS | R |
|---|---|---|---|---|
| None | Left barrel shot | Right barrel shot | **No** | Reload; hold-R if Molotov/Pummeler |
| **Seeking Shell** | Left (seek after bounce) | Right (seek after bounce) | **No** | Unchanged |
| **Blightflame** | **Flamethrower only** (ammo/sec) | — (no pellet fire) | **No** | Reload; hold-R charge toys if present |

**Crown conflict priority (LOCKED):**
1. Blightflame — owns primary fire mode (hose + ammo drain)
2. Seeking Shell — passive on pellets (no input steal)
3. Hold-R charge toys (Molotov / Pummeler) — share R; if both, Pummeler > Molotov (or mutually exclusive shapes — prefer one hold-R exotic in frozen 30)

### 4.5 Dual-trigger mag model (LOCKED: baseline shared mag 2)

- Magazine size forced / treated as **2** on the weapon baseline
- **LMB or RMB** each attempt to fire **one shell** from the shared mag
- Independent trigger cadence: each barrel respects fire interval from last shot **of that barrel** (draft); shared global interval is acceptable v1 simplification if dual timers are painful
- Reload fills both chambers together (standard reload)
- Damage / reload speed / spread: baseline is **identity and tempo**, not a huge flat damage keystone (see §5 damage distribution)
- ADS disabled; aim animations off
- Full-auto conversion is **out of v1** (optional backlog card only)

### 4.6 Blightflame hose (LOCKED: M1 only + ammo per second)

- While equipped: `CanFire` pellet path off; M1 holds flamethrower (vanilla pesticide tick DNA: ~0.1s damage interval, Fire DoT+AOE flags)
- Does **not** use AIM to enable flame
- Pellets and slug disabled for the duration
- **Ammo drain (LOCKED):** while hose is actively spraying, consume magazine ammo at a sustained **ammo/sec** rate (not free hose, not per damage tick as “shots”)
  - Draft dial: **~2–4 ammo/sec** (playtest)
  - Implement as accumulated `ammoDebt` on owner while flame active; spend whole shells when debt ≥ 1
  - Empty mag → hose stops (dry); reload refills; hold M1 again to resume
  - Mag 2 baseline means pure hose is a **short burst** unless Mag Extender / Stand the Heat / Core Snacks / other economy is online — intentional path pressure
- Bake PesticideRework DNA as **path cards**, not a required external mod:
  - range multiplier card / turbo fantasy
  - missing-health damage scaling card
- Flame damage scales modestly with gun damage upgrades so glue cards still matter
- Sync `flamethrowerRange` (vanilla already RPCs this)

### 4.7 Seeking Shell (LOCKED: exotic keystone only)

- After a successful bounce (bounces ≥ 1), retarget pellet direction toward **nearest valid enemy** within seek range (draft 12–18m)
- Prefer living combat targets; skip friendlies / invalid
- Does not seek before first bounce (preserves “ricochet then hunt”)
- Ghosting pierce: if pierce replaces bounce geometry, seeking triggers on pierce pass-through optional — **prefer seek only on real bounce** so Ghosting and Seeking stay distinct
- VFX: bounce trail already shifts post-bounce; add subtle home arc if cheap

### 4.8 Solid Slug transformation

When slug card equipped (and Blightflame not owning M1):
- `bulletsPerShot` → **1**
- Spread → near zero / tight cone
- Per-projectile damage retuned so one slug ≈ meaningful fraction of former full pellet volley (not 6× one pellet)
- Bounce identity kept (neuter rules still apply per hit)
- Excellent pair with Seeking Shell (one smart hammer) and dual-trigger baseline (two deliberate slugs per reload)

### 4.9 Base combat loop (no upgrades)

```
M1 or RMB → one shell from shared mag 2 → bounce → shell wakes (damage + fire)
   ↘ no ADS — hip dual-trigger only
   ↘ reload on empty (both chambers)
   ↘ without walls: still deal ~0.90× pre-bounce — weak but not useless
   ↘ with Seeking Shell: bounce once then home
   ↘ with Blightflame: hold M1 hose, burn ammo/sec, forget pellets
```


## 5. Damage Distribution Rules (LOCKED)

### 5.1 Vanilla failure mode
Effective DPS required: Double Barrel + Double Time + Ghosting + Slideshot. Auto cadence and bounce path felt like traps. Double Barrel was the tax card to make the gun real.

### 5.2 Aussie Special rules
1. **No single Rare is a huge conditional damage gate.** Slideshot = modest next-shot amp and/or handling — movement skill expression, not the damage upgrade.
2. **Baseline dual-trigger is not a damage keystone.** Power budget is control, reload tempo, and spread — not “2× gun for existing.”
3. **Ghosting is not mandatory open-map DPS.** Seeking Shell owns open-map bounce; Ghosting is pierce clear/ST branch.
4. **Damage verbs are spread across the grid:**
   - Flat / pellet damage staples (Standard/Rare)
   - Bounce damage and per-bounce scaling (Ricochet path)
   - Slug concentration (trade pellets for punch)
   - Fire application and ignite payoffs (Blight / Immolator path)
   - Mag economy and honest RoF (Slam Hopper, Hot Potato, Mag Extender, etc.)
   - Core-kill ammo, immolate refill (retuned loops) — especially important under hose ammo/sec
5. **Double Clutch** is a burst/tempo card, not a required multiplier stack to make dual-trigger exist.
6. Hybrids should stack **verbs** (bounce + fire + slug), not four mandatory rares.

### 5.3 Budget sketch (playtest dials, not sacred)
- Baseline pre-bounce 0.90×
- Baseline hopper package: identity + modest handling — damage lives on distributed cards
- Slideshot / Slide Brace: ~+10–20% next shot after slide (down from “build-defining”)
- Seeking Shell: consistency tax already paid by exotic slot; little flat damage
- Blightflame: hose DPS tuned as full primary mode; **ammo/sec** + mag 2 sets uptime — economy cards are real power
- Draft hose drain: ~2–4 ammo/sec (tune vs boss ST and pack clear)


## 6. Upgrade Paths (gravity wells — hybrids intended)

Organizing question for every build: **How do you spend two shells — and what happens when the mag is empty?**

### Path A — RICOCHET / SEEKING
**“Every surface is a free angle.”**

- **Spine:** bounces, bounce damage, falloff reset, geometry, slug-as-hammer
- **Crown:** **Seeking Shell** — post-bounce home to nearest enemy
- **Supports:** Power Skip, Fresh Angle, Extra Hop, Contact Shock, Ghost Trail, Solid Slug, Skip Tax, bounce count
- **Hybrid hooks:** slug + seek (two deliberate homing hammers per reload); bounce damage feeds hopper alphas; fire-on-bounce feeds Blight path even without hose

### Path B — BREAK-ACTION / TEMPO
**“Two barrels. Two triggers. No wasted shells.”**

- **Spine:** dual-trigger mastery, mag-2 tempo, deliberate shots, linked burst, reload discipline
- **Crown:** **None (exotic removed).** Dual-trigger is baseline. Path peak = **Side-by-Side** (signature Epic) + Double Clutch / Quick Break
- **Supports:** Double Clutch, Slam Hopper, Heavy Bore, Choke, Quick Break, Side-by-Side
- **Hybrid hooks:** everything uses dual trigger now — B cards are the universal tempo layer; strongest with A (seeking slugs) or pure reload-alpha hopper
- **Note:** Full-auto conversion is backlog only; Slam Hopper donates RoF/spread on the trigger model, not auto spray

### Path C — BLIGHTFLAME / FUEL
**“The magazine is a fuel tank.”**

- **Spine:** M1 flamethrower, **ammo/sec drain**, fire appl, immolation economy, missing-HP / range
- **Crown:** **Blightflame** — M1 hose only; pellets off; consumes ammo while spraying
- **Supports:** Immolator, Melting Point, Wildfire Bore, Long Wick, Cull the Weak, Stand the Heat, Hot Potato/Reload, flame range, missing-HP scaling
- **Economy gravity:** Mag Extender, Stand the Heat, Core Snacks (and similar) are **path-relevant** under ammo/sec + mag 2 — not optional glue flavor
- **Hybrid hooks:** Immolator + movement fire cards; bounce path mostly idle while hose active (acceptable — mode swap fantasy)

### Path × verb matrix

```
                 RICOCHET              BREAK-ACTION           BLIGHTFLAME
Bounce           core + Seeking        still on every shell   suspended (hose)
Slug             smart hammer          two deliberate slugs   off
Fire appl        wake-up shells        alpha ignite shots     hose + immolate
Tempo            per-shell geometry    dual trigger / reload  ammo/sec fuel burn
Open map         Seeking Shell         aim + slug / seek      hose doesn't care
Mag 2 pressure   make each shell count cadence + Quick Break  extend / refill fuel
```


## 7. Crowns & Sacred Cows

### Seeking Shell (Exotic) — Ricochet crown
- After bounce, home toward nearest valid enemy in range
- Exotic keystone only (no baseline seek, no cheap Rare seek in v1)
- Distinct from Ghosting (pierce without home)

### Blightflame (Exotic) — Pesticide rewrite
- M1 = flamethrower only
- Pellets/slug/ADS disabled
- **Consumes ammo per second while spraying**
- Native range + missing-HP scaling live on support cards (PesticideRework DNA)
- Must feel like a full primary mode; uptime gated by mag economy

### Immolator (Exotic) — keep / retune
- Applying fire also applies fire to you; self-ignite → immolation window + mag identity
- Support cards: Stand the Heat (ammo on immolate), Fire In My Heart (move while ignited)
- Optional turbo DNA later: ally fire aura (SparrohsTurbocharges idea) — backlog

### Shellstorm (Exotic) — Pummeler rewrite
- Hold R charge shells; release rapid burst of charged count
- Owns hold-R vs Molotov — prefer Shellstorm in frozen 30; Molotov backlog or Epic non-hold if both desired

### Powder Kick (Exotic) — All Powdered Up rewrite
- Airborne shots push you back; force scales as mag empties
- Movement sacred cow; keep

### Ghost Trail (Epic or Exotic) — Ghosting rewrite
- Pierce instead of bounce
- **Not** the open-map tax card; Seeking owns that
- If exotic slot tight: ship as Epic

### Side-by-Side (Epic) — Break-Action signature
- Firing one barrel slightly shortens the other barrel’s remaining fire interval
- Path B poster card (not a mode crown; dual-trigger already baseline)

### Boundary Incursion (Oddity)
- Grid grow — universal keep

### Removed: Twin-Hopper exotic
- Dual-trigger + mag 2 + no ADS is **baseline** (v1.1). No exotic unlocks the hopper identity.


## 8. Full Upgrade List (~30 ship + backlog)

Rarity: Standard / Rare / Epic / Exotic / Oddity  
Tags: R Ricochet · H Hopper/Tempo · B Blight · G Glue · F Fire · S Slug · M Move  
Cell rule: Exotics larger; all Exotics same cell count.  
Names are player-facing (full rename).

------------------------------------------------------------------------------
PATH A — RICOCHET / SEEKING                              [~9]
------------------------------------------------------------------------------

A1. Seeking Shell — Exotic (Keystone)
    After a bounce, pellets home toward the nearest valid enemy.
    Open-map bounce fantasy without Ghosting.

A2. Power Skip — Epic (Power Ricochet rewrite)
    +1 bounce. Damage increases with each bounce after the first.

A3. Fresh Angle — Epic (Friction Compensators rewrite)
    Damage falloff resets when a bullet bounces.

A4. Extra Hop — Standard (Reinforced Coating rewrite)
    Bullets ricochet an extra time.

A5. Contact Shock — Epic (Opening Charge rewrite)
    Bullets apply Shock before bouncing; Fire still applies after bounce
    (or on seek impact — implementer pick one clear rule).

A6. Ghost Trail — Epic (Ghosting rewrite)
    Bullets pierce targets instead of bouncing.
    Pierce path; does not home unless Seeking also equipped and a bounce
    still occurs on world geometry (prefer: no seek on pure pierce hits).

A7. Solid Slug — Rare (NEW)
    Fires a single slug instead of pellets. Higher per-hit damage, minimal
    spread, bounce identity retained. Retune damage so slug ≈ focused volley.

A8. Skip Tax — Rare
    +Damage on post-bounce hits only. Rewards geometry without gating pre-bounce
    into uselessness (pre-bounce already softened on baseline).

A9. Hareline — Standard
    Slightly +bullet speed and +bounce seek range if Seeking Shell equipped;
    else +bullet speed only. Tiny glue.

------------------------------------------------------------------------------
PATH B — BREAK-ACTION / TEMPO                            [~7]
------------------------------------------------------------------------------

B1. ~~Twin-Hopper~~ — REMOVED (v1.1)
    Dual-trigger, shared mag 2, no ADS is now **weapon baseline**.

B2. Double Clutch — Rare (Double Time rewrite)
    Fire a 2-shot burst per trigger pull (per barrel).
    Tempo card; damage budget stays honest.

B3. Slam Hopper — Rare (Slam Fire rewrite)
    Faster fire rate, wider spread. Mag stays baseline 2 unless Mag Extender
    (or similar) is equipped — Slam donates RoF/spread only when mag rules conflict.

B4. Heavy Bore — Rare (Heavy rewrite)
    Slower fire, more damage, more fire application.
    Distributed damage staple — works on hopper pellets and feeds flame scaling.

B5. Choke — Rare (Focus Spread rewrite)
    Tighter spread, slightly less damage. Marksman pellet/slug helper.

B6. Quick Break — Rare (Simplest Solution rewrite)
    +Reload speed, −magazine size. Natural mag-2 pair; reload fantasy core.

B7. Side-by-Side — Epic (Path B signature)
    Firing one barrel slightly shortens the other barrel’s remaining fire
    interval (light dual-wield rhythm). Always relevant on baseline dual-trigger.

------------------------------------------------------------------------------
PATH C — BLIGHTFLAME / FUEL                              [~8]
------------------------------------------------------------------------------

C1. Blightflame — Exotic (Keystone) (Pesticide rewrite)
    Primary fire becomes a flamethrower. Pellets and slug disabled.
    Does not use AIM. **Consumes ammo per second while spraying.**
    Full primary hose fantasy; uptime gated by mag economy.

C2. Immolator — Exotic (keep identity)
    Applying fire to a target also applies fire to you. Self-ignite grants
    Immolation for several seconds (mag/identity hooks via supports).

C3. Long Wick — Epic (PesticideRework range DNA)
    +Flamethrower range (and slight target magnetism). Meaningful with Blightflame;
    no effect without flame mode (path tax OK).

C4. Cull the Weak — Epic (PesticideRework missing-HP DNA)
    Flamethrower damage increases as target missing health rises.
    Hose execute card.

C5. Stand the Heat — Epic
    Extra ammo added to the magazine when you gain Immolation.
    High value under hose ammo/sec.

C6. Reactive Jacket — Epic (Reactive Coating rewrite)
    Bullets (and flame ticks if easy) always apply fire to enemy cores.

C7. Wildfire Bore — Rare (Wild Fire rewrite)
    +Fire application, much wider spread (pellets). Hose: +flame effect amount.

C8. Melting Point — Standard
    +Fire application (universal fire staple).

------------------------------------------------------------------------------
GLUE / GUNFEEL / MOVEMENT / ECONOMY                      [~10]
------------------------------------------------------------------------------

G1. Powder Kick — Exotic (All Powdered Up rewrite)
    While airborne, firing pushes you backward. Force increases as magazine
    empties.

G2. Shellstorm — Exotic (Pummeler rewrite)
    Hold RELOAD to charge shells; release to fire charged count in a rapid burst.
    Primary hold-R exotic for v1.

G3. Mag Extender — Standard
    +Magazine size. **Path-relevant for Blightflame fuel uptime** (and comfort on hopper).

G4. Shell Packer — Epic
    +1 pellet per shot (no effect on Solid Slug / Blightflame hose).

G5. Core Snacks — Epic (Delicious Cores rewrite)
    Killing a core returns ammo to the magazine. Strong Blightfuel synergy.

G6. Hot Potato — Rare
    Fire faster while you are ignited.

G7. Hot Reload — Rare
    Reload faster while ignited.

G8. Fire In My Heart — Epic
    Move faster while ignited and holding this weapon.

G9. Slide Brace — Rare (Slideshot rewrite)
    After sliding, next shot gains modest damage and/or tighter spread.
    **Not** a build-defining damage gate.

G10. Stable Grip — Standard
    −Recoil.

G11. Steadier Aim — Standard
    −Spread while ADS (no effect under baseline ADS-off / Blightflame).

G12. Steady Aim — Standard
    +Range while ADS (same ADS-off caveat).

G13. Pellet Press — Standard (NEW — distributed damage)
    Modest +damage to pellet hits (pre- and post-bounce). Honest staple so
    players are not forced into a single path for damage.

G14. Kindling Rounds — Rare (NEW — distributed damage)
    Modest +damage and small +fire appl. Generalist rare.

G15. Boundary Incursion — Oddity
    +Upgrade grid size.

------------------------------------------------------------------------------
FROZEN v1 SHIP POOL (exactly 30)
------------------------------------------------------------------------------

EXOTIC (5) — Twin-Hopper removed; dual-trigger is baseline
  1  Seeking Shell           (A)
  2  Blightflame             (C)
  3  Immolator               (C)
  4  Powder Kick             (G)
  5  Shellstorm              (G)

Recommended frozen 30:

  EXOTIC (5)
    1  Seeking Shell
    2  Blightflame
    3  Immolator
    4  Powder Kick
    5  Shellstorm

  EPIC (8)
    6  Power Skip
    7  Fresh Angle
    8  Contact Shock
    9  Ghost Trail
    10 Side-by-Side
    11 Long Wick
    12 Cull the Weak
    13 Shell Packer

  RARE (11)
    14 Solid Slug
    15 Skip Tax
    16 Double Clutch
    17 Slam Hopper
    18 Heavy Bore
    19 Choke
    20 Quick Break
    21 Wildfire Bore
    22 Hot Potato
    23 Slide Brace
    24 Kindling Rounds

  STANDARD (5)
    25 Extra Hop
    26 Melting Point
    27 Mag Extender
    28 Pellet Press
    29 Stable Grip

  ODDITY (1)
    30 Boundary Incursion

BACKLOG (designed, expand later)
  Hareline, Stand the Heat, Reactive Jacket, Core Snacks,
  Hot Reload, Fire In My Heart, Steadier Aim, Steady Aim,
  Molotov rewrite (hold-R ignite grenade — conflicts Shellstorm; add when R-router exists),
  Cull/Long Wick turbo merges, ally Immolator aura (turbo DNA),
  Full-auto conversion card (optional; not required for fantasy),
  Asymmetric barrel (one flame / one slug) — post-v1 only,
  Multiversal Thievery / Edge Fault (contraband parity only if desired)

------------------------------------------------------------------------------
CUT / DEMOTE FROM VANILLA IDENTITY
------------------------------------------------------------------------------

| Vanilla | Fate |
|---|---|
| Pre-bounce ×0.75 | **Soften ~×0.90** |
| Pesticide hold AIM | **Blightflame** M1 hose only + **ammo/sec** |
| Double Barrel mag2 + huge package | **Baseline identity** (dual trigger, mag 2, no ADS); moderate damage |
| Twin-Hopper as exotic (doc v1) | **Removed** — folded into baseline (v1.1) |
| Ghosting as open-map tax | **Ghost Trail** pierce branch; Seeking owns open map |
| Slideshot big next-shot | **Slide Brace** modest |
| Double Time mandatory burst DPS | **Double Clutch** tempo |
| Damage only on exotic rare stack | **Pellet Press / Kindling / Heavy / Skip Tax / Slug** spread |
| (none) | **Seeking Shell** new |
| (none) | **Solid Slug** new |
| Full-auto Jackrabbit cadence | **Not baseline**; optional backlog only |
| Molotov | Backlog (hold-R conflict with Shellstorm) |
| Above-damage / aerial mult if present in data | Optional glue later |


## 9. Example Builds

### Pure Ricochet (seeking hopper)
Pellet Press → Extra Hop → Power Skip → Fresh Angle → Skip Tax → Kindling Rounds → **Seeking Shell**  
*Play:* Dual-trigger spray into geometry or feet; seek cleans packs on open maps. No mode exotic required.

### Seeking Slug hammer
Solid Slug → Extra Hop → Power Skip → Skip Tax → Contact Shock → **Seeking Shell** → Choke  
*Play:* One heavy shell per trigger, bounce, home, delete priority targets. Two slugs per reload.

### Pure Break-Action
Double Clutch → Quick Break → Heavy Bore → Side-by-Side → Slide Brace → Kindling Rounds → Pellet Press  
*Play:* LMB/RMB rhythm, short mag discipline, movement between breaks. Damage from Heavy/Kindling/Pellet Press.

### Slug Hopper
Solid Slug + Seeking Shell + Heavy Bore + Side-by-Side + Quick Break  
*Play:* Two deliberate seeking slugs per reload; tempo cards keep the dance alive.

### Pure Blightflame
**Blightflame** → Long Wick → Cull the Weak → Melting Point → Immolator → Mag Extender → Hot Potato  
*Play:* Walk up, hose, burn ammo/sec, immolate loop, execute low-HP packs. Mag Extender / refill is the fuel story.

### Immolator skater
Immolator → Fire In My Heart (backlog) → Hot Potato → Powder Kick → Kindling Rounds → Melting Point  
*Play:* Set yourself on fire, move fast, airborne kick shots; works with pellets or hose.

### Hybrid poster (recommended trailer build)
Seeking Shell + Solid Slug + Fresh Angle + Side-by-Side + Kindling Rounds  
*Note:* Show dual-trigger seeking slugs on baseline hopper. Second trailer: Blightflame + Immolator + Cull the Weak + Mag Extender (fuel anxiety → payoff).


## 10. Strengths, Weaknesses & Risks

### Strengths
- Bounce fantasy works on open maps (Seeking Shell)
- Dual-trigger is the gun immediately — no exotic tax to feel like Aussie Special
- Break-Action path rewards mastery and reload without a dead “become DB” card
- Blightflame is an integrated primary mode with real fuel tension (ammo/sec)
- Slug fills the missing “single projectile shotgun” niche
- Fire/immolation loop retained and supported

### Weaknesses / fun failure states
- No bounce + no Seeking + long range = soft damage (by design)
- Mag 2 empty mid-fight without reload cards
- Blightflame dry-hose mid-pack without economy cards
- Blightflame loses all bounce/slug toys while equipped
- Immolator greed without heal/DR support
- Shellstorm whiff (charged dump into empty air)
- Players who wanted full-auto Jackrabbit may bounce off baseline (by design; backlog auto card later)

### Design risks
- Seeking target selection jank (priority: nearest in cone vs true nearest)
- Slug damage retune too high → sniper shotgun; too low → trap card
- Softened pre-bounce + Pellet Press + Kindling + Heavy stacking — watch total
- Hold-R: only one of Shellstorm/Molotov in v1
- Flame damage must scale with some gun damage cards or hose feels disconnected again
- Ammo/sec too high → hose unusable on mag 2; too low → infinite-feeling hose returns
- Path B crownless — Side-by-Side + Quick Break must read as a real well in UI/grid gravity


## 11. Success Criteria / Player Fantasy Checklist

- [ ] Empty-grid Aussie Special clears packs without feeling broken-on-purpose
- [ ] Pre-bounce ~0.90× is noticeable but not “why did I equip this”
- [ ] Post-bounce hits still feel like the shells “turn on”
- [ ] Seeking Shell makes bounce matter on open / outdoor maps
- [ ] Ghost Trail is optional pierce, not required for damage
- [ ] Baseline dual-trigger feels like the gun’s identity in the first magazine
- [ ] Break-Action cards (Side-by-Side, Double Clutch, Quick Break) feel like mastery, not mandatory taxes
- [ ] Blightflame M1 hose is obvious; no ADS-steal confusion
- [ ] Blightflame visibly spends ammo while spraying; dry mag stops the hose
- [ ] Mag Extender / immolate refill matter on pure hose builds
- [ ] Solid Slug is a satisfying pellet → hammer transform
- [ ] Best damage is a **grid of cards**, not Slide+DB+Ghost+DoubleTime only
- [ ] Slide Brace is fun movement tech, not a DPS tax
- [ ] Immolator loop still pops off with supports
- [ ] Vanilla Au-Si Jackrabbit still exists untouched
- [ ] Co-op: flame and ignite help allies; seeking doesn’t grief friendlies


## 12. Universal Truths (Mycopunk alignment)

- Exotic shapes should always be larger than others; each exotic should use the same number of cells.
- v1 targets **~30** upgrades (frozen list above); backlog is real design, not trash.
- Three paths create different build options but **may intermingle** on the grid.
- Full rename for rework identity.
- Prefer bounce / hopper tempo / blight fuel / slug verbs over generic +% only.
- Primary element identity: **Fire** (+ Shock as bounce rider only).


## 13. Vanilla Jackrabbit → Aussie Special Fate Table

| Vanilla name | Aussie Special name | Path | Notes |
|---|---|---|---|
| (baseline neuter 0.75×) | Softened ~0.90× pre-bounce | — | Fire still mostly post-bounce |
| (auto fire + ADS) | Dual-trigger, mag 2, no ADS | baseline | Double-barrel identity default |
| (no seek) | Seeking Shell | A | New exotic |
| (no slug) | Solid Slug | A | New rare |
| Power Ricochet | Power Skip | A | |
| Friction Compensators | Fresh Angle | A | |
| Reinforced Coating | Extra Hop | A | |
| Opening Charge | Contact Shock | A | |
| Ghosting | Ghost Trail | A | Pierce branch |
| Double Barrel | **Baseline** (was Twin-Hopper exotic in v1) | B identity | LMB/RMB, shared mag 2, no ADS — not an upgrade |
| Double Time | Double Clutch | B | |
| Slam Fire | Slam Hopper | B | RoF/spread; mag stays 2 without Extender |
| Heavy | Heavy Bore | B/G | Distributed damage |
| Focus Spread | Choke | B/G | |
| Simplest Solution | Quick Break | B | Frozen in v1.1 |
| Pesticide | Blightflame | C | M1 hose + ammo/sec |
| Immolator | Immolator | C | Keep name OK or flavor rename later |
| (PesticideRework range) | Long Wick | C | Native |
| (PesticideRework HP scale) | Cull the Weak | C | Native |
| Stand the Heat | Stand the Heat | backlog | Fuel synergy |
| Reactive Coating | Reactive Jacket | backlog | |
| Wild Fire | Wildfire Bore | C | |
| Melting Point | Melting Point | C/G | |
| All Powdered Up | Powder Kick | G | |
| Pummeler | Shellstorm | G | Hold-R |
| Molotov | — | backlog | Hold-R conflict |
| Delicious Cores | Core Snacks | backlog | Fuel synergy |
| Shell Packer | Shell Packer | G | |
| Mag Extender | Mag Extender | G/C economy | Critical for hose uptime |
| Hot Potato | Hot Potato | G | |
| Hot Reload | Hot Reload | backlog | |
| Fire In My Heart | Fire In My Heart | backlog | |
| Slideshot | Slide Brace | G | Modest amp |
| Stable Grip | Stable Grip | G | |
| Steadier Aim / Steady Aim | same | backlog | ADS-off caveat |
| Boundary Incursion | Boundary Incursion | G | Keep name |
| (none) | Pellet Press | G | Distributed damage staple |
| (none) | Kindling Rounds | G | Distributed damage rare |
| (none) | Skip Tax | A | Post-bounce damage |
| (none) | Side-by-Side | B | Hopper rhythm signature epic |
| (none) | Hareline | backlog | |
| Edge Fault / Multiversal | — | — | Optional contraband parity |


## 14. Implementation Notes (for later coding passes)

### Product / registration
- New primary via weapon template flow: clone **BounceShotgun** (not CartridgeSMG)
- Unique gear id + APIName e.g. `aussie_special`
- `PlayerData.CreateUpgrade` pool; SpawnGear remap + stamp identity + ApplyUpgrades
- `[MycoMod(..., ModFlags.IsSandbox)]`
- Does not remove vanilla BounceShotgun from AllGear

### Host behaviour
- `AussieSpecialBehaviour` (or subclass when prefab exists) holding:
  - preBounceDamageMult (default ~0.90)
  - seek enabled / seek range
  - dual-trigger always on baseline (per-barrel timers; mag 2; canAim false)
  - blightflame enabled / flame stats / **ammoPerSecond drain**
  - slug enabled
  - distributed damage mults, slide brace, etc.
- Prefer mutating live `ShotgunBounceBullet` behavior via Harmony on `DamageTarget` / `Bounce` rather than shipping a full custom bullet prefab in v1
- `LockedTargetLocalPos` already exists on `ShotgunBounceBullet` — investigate before inventing parallel lock-on

### Hooks
| Area | Approach |
|---|---|
| Pre-bounce neuter | Postfix/prefix `ShotgunBounceBullet.DamageTarget` — use behaviour mult instead of const 0.75 |
| Seeking | After `Bounce`, if seek flag: set `data.direction` toward nearest enemy; optionally set LockedTargetLocalPos |
| Dual-trigger fire | Baseline: M1 left barrel; RMB right barrel without ADS — patch CanAim / IsAiming / fire path; force mag 2 on spawn/apply |
| Blightflame | On apply: set flamethrowerRange > 0; **rebind flame from OnAim to fire-held**; CanFire pellets false while flame mode; disable EnableAimAnimations; **drain ammo/sec while spraying** |
| Slug | On apply: bulletsPerShot = 1; retune damage; tighten spread |
| Hold-R Shellstorm | Existing bulletChargeSpeed DNA on BounceShotgun |
| Powder Kick | Existing maxBackboostForce DNA |
| Immolator | Existing immolationSelfFire / stacks DNA |
| PesticideRework | Port Long Wick / Cull the Weak as first-class properties; no hard dep on sparroh.pesticiderework |

### Vanilla BounceShotgun flags (reference)
```
IncreaseDamageOnBounce = 1
ApplyFireToCores = 2
PhaseThroughTargets = 4
Heavy = 8
ResetFalloffOnBounce = 0x10
ChargeLockOn = 0x20
```
Map rewrites onto these where possible; use UserDefined bits / behaviour flags for Seeking, Blightflame-M1, slug, ammo/sec hose.

### Dual-trigger detail (baseline)
- Track `lastFireTimeLeft` / `lastFireTimeRight` or single interval if simplified
- RMB must not enter ADS: `canAim = false` always on this weapon (unless a future card restores ADS — none in v1)
- Consume ammo once per successful barrel fire
- Reload unchanged
- No exotic flag to enable hopper — always on

### Blightflame detail
- Mirror vanilla flame tick (0.1s, DoT|AOE, Fire effect)
- Start/Stop flamethrower VFX/sound on fire pressed/released, not aim
- `SyncShotgunData_Rpc(flamethrowerRange)` still relevant
- Scale `flamethrowerDamage` with gun damage ratio like vanilla OnUpgradesEnabled
- **Ammo/sec:** while local owner holding fire and flame active and mag > 0, accumulate drain; spend integer ammo; stop flame path when mag hits 0 (mirror dry-fire UX)

### Network
- All clients need matching mod
- Flame start/stop already RPC’d on vanilla; reuse patterns
- Ammo is already replicated via gun ammo state — prefer draining through normal ammo APIs
- Seeking is owner-sim on rail bullets (owner Fire path) — keep owner-authoritative

### HUD / UX
- Dual-trigger: consider dual-chamber ammo pips if cheap; else mag 2 is enough
- Blightflame: hide misleading ADS prompt; show flame range cue if possible; ammo tick should be readable on the mag counter
- Seeking: optional mild reticle tick when bounce home acquires (stretch)


## 15. Open Tuning Questions (playtest, not design blockers)

1. Pre-bounce mult 0.88 vs 0.90 vs 0.92
2. Seeking range 12–18m and whether line-of-sight required
3. Dual-trigger: per-barrel interval vs shared global interval
4. Baseline hopper damage/handling package exact numbers
5. Solid Slug damage vs 6-pellet expected value (start ~70–85% of full open-pellet post-bounce volley at optimal range?)
6. Blightflame hose DPS vs pellet ST on bosses
7. **Blightflame ammo/sec** (start 2–4/s) vs mag economy card density
8. Cull the Weak max missing-HP mult (PesticideRework default +125% at 0 HP — may be high for native exotic path)
9. Whether Ghost Trail stays Epic or needs exotic cell
10. Slide Brace percent (start +15% damage next shot)
11. Stacking ceiling: Pellet Press + Kindling + Heavy Bore + Skip Tax
12. Whether Path B needs a future exotic soft-crown (elevated Side-by-Side) after playtest


## 16. Locked Decisions Log

| Decision | Lock |
|---|---|
| Product name | **Aussie Special** |
| Product shape | New primary; vanilla Jackrabbit untouched |
| Baseline fire | **Dual-trigger** LMB/RMB; **shared mag 2**; **no ADS** |
| Twin-Hopper exotic | **Removed** (v1.1) — identity is baseline |
| Full-auto | **Not baseline**; backlog only |
| Blightflame fire | **M1 hose only**; pellets/slug off; **ammo per second while spraying** |
| Seeking | **Exotic keystone only** |
| Baseline neuter | **Soften pre-bounce** (~0.90×) |
| Scope | Aggressive rewrite, full rename, ~30 frozen |
| Slug | **In pool** (Solid Slug) |
| Damage economy | **Distributed** across many cards; Slide/DB not crutches |
| Hold-R v1 | **Shellstorm** (Pummeler); Molotov backlog |
| Crown priority | Blightflame > Seeking (passive) |
| Gravity wells | **Ricochet / Break-Action / Blightfuel** |
| Path B crown | **None**; Side-by-Side is signature Epic |
| External mods | PesticideRework DNA **baked in**; no hard dependency |
| Elements | Fire spine; Shock bounce rider; no Acid/Decay default |

### Design changelog

#### v1.1
- **Double-barrel is baseline:** dual-trigger LMB/RMB, shared mag 2, no ADS
- **Twin-Hopper exotic removed**; Path B → Break-Action / Tempo (supports only)
- Gravity wells retitled: **Ricochet / Seeking**, **Break-Action / Tempo**, **Blightflame / Fuel**
- **Blightflame consumes ammo per second** while hose active; mag economy is path pressure
- Frozen 30: 5 exotics; **Quick Break** in; Twin-Hopper out
- Builds, fate table, impl notes, success criteria updated for hopper-default + fuel hose
- Auto-fire Jackrabbit cadence deferred to optional backlog

#### v1
- Aussie Special identity and three paths
- Softened pre-bounce; Seeking Shell; Twin-Hopper dual trigger exotic; Blightflame M1
- Solid Slug; distributed damage rules; Slide Brace demotion
- Frozen 30 + backlog + fate table
- Impl notes against BounceShotgun / ShotgunBounceBullet


## 17. Related mods / DNA to reuse (not required at runtime)

| Source | DNA |
|---|---|
| PesticideRework | Flame range mult; missing-HP flame scaling → Long Wick / Cull the Weak |
| SparrohsTurbocharges (Jackrabbit) | Pesticide turbo, Pummeler charge speed, Immolator ally aura (backlog), Powder Kick enable |
| BounceIndicatorPlus | Player-facing bounce readability (optional companion; don’t depend) |
| Weapon template / DMLR / Heat Cycler / Blood Carver | Registration, design-doc structure, crown patterns |
| Vanilla BounceShotgun | Flame VFX/RPC, immolator, pummeler charge, molotov lob, backboost, slide stacks |


## 18. Next Steps After This Doc

1. Review frozen 30 vs backlog cuts (5 exotics, Quick Break in, economy cards still mostly backlog)
2. Implement behaviour host + pre-bounce mult hook on ShotgunBounceBullet
3. Implement **baseline dual-trigger** (canAim false, RMB fire, mag 2) — not behind an upgrade
4. Implement Seeking Shell bounce retarget
5. Implement Blightflame M1 hose rebind (detach from AIM) + **ammo/sec drain**
6. Implement Solid Slug + distributed damage staples
7. Register crowns + frozen pool; icons/rename strings
8. Balance pass: empty grid hopper, pure tempo, hose fuel, slug seek
9. Optional: retire or gate PesticideRework when Aussie Special ships equivalent cards
10. Playtest whether Path B needs a future exotic soft-crown

---

*End Aussie Special Design Doc v1.1*
