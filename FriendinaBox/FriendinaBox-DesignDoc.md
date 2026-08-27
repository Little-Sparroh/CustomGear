# Friend in a Box — Design Document

> Status: **Implemented core + 18 upgrades** · design expansion to ~30 · remaining cards **Planned**.  
> Ship name: **Friend in a Box**.  
> Project: `.content.FriendinaBox` · GUID `sparroh.friendinabox` · Gear id **92100**.  
> Structure aligned with Honey Jar / Splash Canister / DMLR / Heat Cycler design docs.

---

## 1. High Concept / Fantasy

**Friend in a Box** is the throwable ally.

Where **Incendiary** is a fire boom economy, **Acid** is puddles and pull toys, **Shock** is mobility lightning, **Honey Jar** is territory bees, and **Splash Canister** is water-as-primer, Friend in a Box is a **deployable second body**: you throw a chassis that arms on the field, works without you holding the trigger, and — while equipped — lets solo Ouroboros treat you as if you brought a friend.

**One-liner:** *Throw a friend. It mines, turrets, mortars, or drones — draws aggro so you don’t, and while it’s in your kit, multiplayer-only upgrades can show up solo.*

**Element / payload:** `EffectType.Normal` (shared family baseline). Identity is **not** a status element — it is **presence + AI form + Taunt sustain + coop unlock**. Mode shots may still blend toward primary weapon stats via Calibrated Link.


---

## 2. Role in the Arsenal

| Gear | Fantasy | Friend in a Box relationship |
|------|---------|------------------------------|
| **Incendiary Grenade** | Instant fire boom + self-fire economy | Friend **suppresses** the instant boom and arms a field entity. Same throw feel; different afterlife. |
| **Acid Grenade** | Puddles, vacuum pull, overhealth toys | Friend can **leave** an acid puddle on expire (Lingering Gift) but is not a puddle weapon. Sustain is **Taunt** (enemy attention on the chassis), not Polymer dumps or blue overhealth. |

| **Shock Grenade** | Shock boom + mobility / Live Wire | Complementary. Friend is field control + ally fantasy, not self-nade mobility (except Parting Boost / planned Corridor Rush). |
| **Honey Jar** | Bees + clouds/hives/cloak/nebula | Complementary minion slot. Honey = status swarms. Friend = **one smart chassis** with discrete modes. Do not clone bee clouds. |
| **Splash Canister** | Water catalyst + lakes/waves | Complementary. Splash = zones/reactions. Friend = **units**. |
| **Swarm Launcher** | Hover pellets + Breeding Season ally fire | **Hive Kin** hard-hooks Breeding Season so deployables count as FriendFire allies. Friend is not a second Swarm gun. |
| **Real co-op players** | Second brain, real Coop drops, Breeding Season bodies | Friend approximates **Coop upgrade access** while equipped and optional Rally/Hive Kin presence. Never a full second player brain. |
| **Bruiser Shield Projector** | Deployed hard-light defense | Different layer (shield tool vs offensive/utility ally chassis). |

**Niche tags:** Deployable Ally · Taunt · Proximity Mine · Turret / Mortar / Drone · Solo Coop Unlock · Field Economy

**Slot:** Throwable / grenade gear  
**Range:** Mid throw arc → field entity (mine radius / turret~50m+ / mortar lob / drone follow)  
**Role:** Persistent field presence, **Taunt** tanking, mode-flexible DPS/control, solo Ouroboros coop enabler, Swarm Breeding Season bridge


**Not trying to be:** generic bigger Incendiary, pure CC stick, full autofire primary replacement, or a fake multiplayer client.

---

## 3. Design Pillars

1. **Ally presence first**  
   The fantasy is *something on the field that works with you*. Upgrades answer how the Friend lives, dies, aims, multiplies, or pays you back — not only bigger boom numbers.

2. **Baseline is a clean mine**  
   No upgrades = proximity mine with a readable duration. Mode kits, sustain, and economy are **opt-in via the grid**.

3. **Gravity wells, not locked paths**  
   Upgrade “paths” are thematic attractors (**Minefield / Fireteam / Quartermaster**). Players mix freely. Hybrids (Buddy + Sentry, Buddy + Lobber, multi-kit boards) are intended.

4. **Coop unlock is the signature passive**  
   While Friend is **equipped**, solo runs can roll `UpgradeFlags.Coop` Ouroboros upgrades. This is load-bearing identity — document it, balance around it, never bury it.

5. **Readable mode exotics**  
   Three mode-defining kits (Sentry / Lobber / Buddy) plus three utility exotics already shipped (Scuttle / Calibrated Link / Hive Kin). **Six Exotics is intentional shipped state** (see §7) — not a mistake to “fix” in the doc.

6. **Taunt is the sustain home**  
   Friend survivability identity is **Taunt** — enemies prefer the deployable chassis (Decoy Protocol and related aggro tools). Not pure HoT (Honey), not regenerating chip shell (Splash), not timed % DR (Caustic), not flat shield dumps (Voltaic), not flat HP pulses (Thermite). Blue overhealth cards (Sympathetic Link, Reactive Shell, Aftercare) may remain as **secondary utility**, not the sustain pillar.


7. **Grounded in real APIs**  
   Detonate prefix, RailBullet / MortarBullet, `HealWithOverhealth`, Swarm FriendFire internals, `PlayerData.FilterUpgrades` coop gate — prefer decompile hooks over invented parallel systems.

8. **Sandbox honesty**  
   Local-owner deploy authority as implemented. Multiplayer full NGO custom prefab is a later polish pass, not required for fantasy completeness.

---

## 4. Baseline (No Upgrades)

**Feel target:** throw → land → sit as a mine → enemy walks in → boom. Duration is visible pressure. Equip alone already matters (coop unlock). Shared family boom numbers; identity is deployable + Taunt upgrades.

**Shared family baseline (locked):** damage **100** · effect amount **10** · max charges **3** · recharge **45** · explosion radius (`hitForce`) **6**.

| Property | v0 / target | Notes |
|----------|--------------|--------|
| Throw / arc / bounce | Incendiary-like | Runtime clone path OK; stamp Friend identity |
| On “detonate” (fuse complete) | **Arm deployable**, skip vanilla AOE | `FriendGrenadeHooks` prefix |
| Default mode | **Proximity mine** | `FriendDeployMode.None` |
| Deploy duration | **20s** | `deployDuration` |
| Detect radius | **75% of explosion radius** if unset | `detectRadius` 0 → derive |
| Explosion radius (`hitForce`) | **6** | Shared family; Wider Net scales |
| Impact damage | **100** | Shared family boom (mine detonate / snapshot) |
| Element | `EffectType.Normal` | Not Fire lineage; Calibrated Link may blend mode-shot element |
| Effect amount (`damageEffectAmount`) | **10** | Shared family; meaningful on Normal payload |
| Concurrent deploys | **1** | New arm replaces oldest quietly |
| Max charges | **3** | Shared family baseline |
| Recharge duration | **45** | Shared family baseline |
| Healing / overheal | **None** | Not the sustain home |
| **Taunt** | **None** at stock | Decoy Protocol / taunt upgrades unlock — sustain is upgrade-gated |
| Mode fire | N/A until kit | Turret fireRate default **2**/s when converted; `modeDamageScale` default **0.35** |
| Drone follow | N/A until Buddy | follow **0.85**, move **14**, hover height **2.4** |
| Visual (dev) | Magenta debug box | Green / orange / cyan by mode |
| **Coop unlock** | **Active while equipped** | Even with zero upgrades |

### Baseline arm sequence

```
Throw → bounce/fuse (vanilla grenade rules)
  → GrenadeBullet.Detonate (Friend prefix)
      → suppress vanilla AOE
      → snapshot FriendinaBoxBehaviour.Data
      → spawn FriendDeployable(s) at impact (+ squad spread if multi)
      → Kill projectile
  → Deployable ticks mode behaviour until detonate / expire / scuttle / replace
```

### Why mine-first (not always-turret)

- Always-turret steals Sentry Kit’s exotic beat.
- Mine teaches “field presence + duration” immediately and fills a real arsenal gap (no vanilla player mine).
- Mode kits remain exciting board moments.

---

## 5. System Truths

### 5.1 Deploy modes (flags, combinable)

```
FriendDeployMode (flags):
  None   = 0   // Mine
  Turret = 1
  Mortar = 2
  Drone  = 4
```

**Primary resolution priority:** Drone > Mortar > Turret > Mine.

| Primary | Behaviour |
|---------|-----------|
| **Mine** | Scan detect radius on interval; enemy in range → full detonate |
| **Turret** | Auto-fire RailBullet at enemies; engage range `max(detect, 50)` |
| **Mortar** | Lob MortarBullet / rocket-style; slower RoF (~0.55×); range `max(detect × mortarRangeMult, 50)` |
| **Drone** | Hover near player in formation ring; default **suicide dive** on enemy in ~1.5× detect |

**Hybrids (intended):**

| Flags | Result |
|-------|--------|
| Drone + Turret | Follow player, fire rail, **no** suicide dive |
| Drone + Mortar | Follow player, lob mortar, **no** suicide dive |
| Drone + Turret + Mortar | Follow; fire logic prefers mortar-only lob when mortar set without treating dual as rail — see impl (`hasMortar && !hasTurret` lobbed path); document preferred design: **turret wins hitscan if both**, mortar if mortar-only hybrid |
| Turret + Mortar (no drone) | Primary = Mortar (priority) — stationary lobber |

### 5.2 Duration, detonate, expire

```
remainingDuration counts down every frame.
  → 0: Expire()
      - optional acid puddle (Lingering Gift)
      - optional move speed buff (Parting Boost)
      - no full mine boom unless Failsafe Fuse (planned)
  → proximity / dive / scuttle: Detonate(reason)
      - SpawnExplosionFirstPerson with snapshot damage + radius
  → ForceDespawn: quiet remove (concurrent replace) — no payoffs
```

**Scuttle Charge:** player shoots deployable → detonate with power `Lerp(0.35, 1.0, remaining/max)` so early scuttle still hurts, late scuttle is full value.

### 5.3 Concurrent deploys & formation

- `FriendDeployTracker.MaxConcurrentDeploys` (baseline 1).
- On register, if at cap → **oldest** `ForceDespawn`.
- **Squad Drop:** raises max concurrent; one throw spawns **count** deploys in a horizontal ring (index 0 on impact).
- **Drones:** formation slots by spawn order; ring radius from `droneFollowDistance`, widened with count; yaw-aligned to player.

### 5.4 Combat hooks (owner gear + local player)

| Hook | Effect |
|------|--------|
| Friend damage | Sympathetic Link → blue overhealth (`HealWithOverhealth`) |
| Friend damage | Painted Targets → mark enemy |
| Friend kill | Field Recharge → grenade charge |
| Friend kill | Overtime → extend all active durations |
| Player outgoing damage | Designated Target → remember last hit enemy for Friend focus |
| Player outgoing damage | Mark amplify if target marked |
| Player took damage | Reactive Shell → blue overhealth on ICD |
| Per-frame (optional) | Scuttle raycast while Fire held |

### 5.5 Coop unlock (signature)

Vanilla Ouroboros filters `UpgradeFlags.Coop` when `GameManager.players.Count == 1`.

**Friend rule:** while Friend is equipped on the local player, effective player count for that check is **at least 2** (`CoopUnlockHook` transpiler → `GetEffectivePlayerCount`).

**Scope:**
- Does **not** spawn a fake player pawn.
- Does **not** enable all multiplayer-only *systems* — only the **upgrade filter** gate documented here.
- Unequip → solo filter returns.

**Balance note:** this is strong. Mitigations live in §12 (failure modes) and in not stacking infinite free power on the grenade itself.

### 5.6 Hive Kin ↔ Swarm Breeding Season

When `countsAsSwarmAlly` and local player fires Swarm with FriendFire radius > 0:

- Active Friend deployables in radius count as Breeding Season allies.
- Extra custom swarm pellets spawn from deploy positions (ground forms +1.5y, drones +0.35y).
- Stack display includes Friend allies + real player Swarm allies.

Requires player to actually run **Swarm + Breeding Season**; Hive Kin alone does nothing.

### 5.7 Calibrated Link (weapon stat blend)

When enabled, turret/mortar (and hybrid drone guns) blend toward the player’s **first non-throwable IWeapon**:

- Damage, effect type/amount, fire rate (partial), bullets-per-shot (rail volleys).
- Portion `weaponStatPortion` (shipped roll ~0.35–0.55).
- `modeDamageScale` still applies as a global mode tax (default 0.35) so Friend does not equal a second primary.

### 5.8 Designated Target

On player damaging an enemy (non-Friend source), store target for `sharedTargetDuration`. Friend aim prefers that target if still valid and in range.

### 5.9 Identity & registration (implementation truth)

| Key | Value |
|-----|--------|
| Display | Friend in a Box |
| APIName | `friend_in_a_box` |
| Gear ID | **92100** |
| Upgrade IDs | **92101–92118** shipped; **92119–92130** reserved planned |
| Base clone | IncendiaryGrenade (throw feel); stamp `damageEffect = Normal`, family stats |
| Catalog | Disabled clone in `Global.AllGear` (NetworkObject stripped) |
| Live equip | NGO spawns **base** Incendiary prefab; hooks stamp Friend identity + behaviour + ApplyUpgrades |
| Mod flag | `ModFlags.IsSandbox` |

---

## 6. Gravity Wells (Thematic Attractors)

Not exclusive trees. Taking a mode exotic or a sustain epic pulls related cards into value; every upgrade remains equippable with every other unless a future rare explicitly requires a flag.

### Well A — **Minefield** (control / proximity / expire)

*Seed the ground. Detonate on contact. Pay off when the timer wins or the pack steps wrong.*

Core pieces: Wider Net, Long Watch, Quick Deploy, Lingering Gift, Parting Boost, Squad Drop, Failsafe Fuse, Tripwire Lattice, Decoy Protocol, Hot Swap Socket.

### Well B — **Fireteam** (modes / guns / focus)

*Change what the chassis is. Make it shoot like you. Point it where you point.*

Core pieces: Sentry Kit, Lobber Kit, Buddy Protocol, Calibrated Link, Designated Target, Scuttle Charge, Hardpoint, Pack Mentality, Shoulder Guard, Chain of Command.

### Well C — **Quartermaster** (Taunt sustain / economy / coop fantasy)

*The Friend holds aggro and pays you back — taunt, charge, marks, beacon recharge, rally. Overhealth is optional glue, not the headline.*

Core pieces: **Decoy Protocol** (Taunt spine), Field Recharge, Overtime, Painted Targets, Hive Kin, Resupply Beacon, Rally Point, Corridor Rush; secondary: Sympathetic Link, Reactive Shell, Aftercare.


**Mix examples (expected, not edge cases):**

- Sentry + Calibrated Link + Designated Target → aim-linked rail emplacement  
- Buddy + Lobber + Squad Drop → mortar drone wing  
- Minefield Wider Net + Lingering Gift + Failsafe Fuse → zone denial even on timer loss  
- Hive Kin + Swarm Breeding Season + Squad Drop → multi-ally pellet storm  
- Decoy Protocol + Squad Drop + Long Watch → multi-bait taunt field  
- Reactive Shell + Sympathetic Link + Aftercare → optional blue-overhealth glue (secondary; watch failure modes)  

- Scuttle + Overtime + Field Recharge → manual detonate economy  
- Rally Point + coop unlock → true multiplayer support fantasy when friends are present  

---

## 7. Content Budget & Universal Truths

| Rule | Value |
|------|--------|
| Total upgrades | **~30** (18 Implemented + 12 Planned) |
| Exotics | **6 shipped** — intentional exception to Honey/Splash “exactly 3” |
| Exotic hex footprint | Prefer **large equal** shapes for the three **mode** kits (Sentry / Lobber / Buddy). Utility exotics (Scuttle / Calibrated / Hive Kin) may share that footprint or use a shared “utility exotic” size — author at hex pass. |
| Epics | Shipped: 0 pure Epic rarity today (mode kits registered Exotic). Planned adds Epics (Decoy, Corridor, Resupply, Rally, …). |
| Rares / Standards | As registered + planned spine fillers |
| Path locks | **None** required in v1 planned set |
| Stackables | Stat spine + several rares; mode kits unique |
| Oddity / Contraband grid steal | Out of scope unless later parity pass |
| Shared vanilla staples (Grenade Belt, In For A Penny, Boundary Incursion) | Optional parity later — **not** required in custom ~30 |
| Cross-mod hard hooks | **Hive Kin → Swarm only**; all else soft |

### Rarity honesty note

Shipped registration uses **Standard / Rare / Exotic** only (no Epic tier on the live 18). Planned cards introduce **Epic** for mid-high fantasy pieces so the board reads closer to vanilla grenade curves. Do **not** demote the six live Exotics in this doc.

### ID budget

| Range | Use |
|-------|-----|
| 92100 | Gear |
| 92101–92118 | Implemented upgrades |
| 92119–92130 | Planned upgrades |
| 92131+ | Stretch / post-30 |

Avoid collisions: template `920xx`, Honey Jar `922xx`, Splash `923xx`.

---

## 8. Full Upgrade Table

Rarity key: **S** Standard · **R** Rare · **E** Epic · **X** Exotic  
Stack: **✓** CanStack · **—** unique  
Status: **I** Implemented · **P** Planned  
Well: primary gravity well (still mixable)

Numbers for **I** cards match `FriendUpgradeProperties` ranges. **P** numbers are v0 targets.

### 8.1 Implemented — Standards (6)

| ID | Name | Well | Stack | Intent | Numbers (shipped) |
|----|------|------|-------|--------|-------------------|
| 92101 | **Wider Net** | Minefield | ✓ | Mine detect + explosion radius | +20–35% both (`radiusBonus`); also scales `hitForce` |
| 92102 | **Long Watch** | Minefield | ✓ | Longer duration, slower throw CD | Duration +35–50%; recharge +20–30% |
| 92103 | **Quick Deploy** | Minefield | ✓ | Faster CD, shorter duration | Recharge −20–30%; duration −15–25% (floors at 0.25×) |
| 92110 | **Sympathetic Link** | Quartermaster | ✓ | Friend damage → blue overhealth | 8–15% of damage dealt |
| 92111 | **Field Recharge** | Quartermaster | ✓ | Friend kill → grenade charge | +0.25–0.40 charge units |
| 92112 | **Overtime** | Quartermaster | ✓ | Friend kill → extend deploys | +1.5–2.5s to all active |

### 8.2 Implemented — Rares (6)

| ID | Name | Well | Stack | Intent | Numbers (shipped) |
|----|------|------|-------|--------|-------------------|
| 92104 | **Lingering Gift** | Minefield | — | Expire → acid puddle | Puddle duration 4–6s |
| 92105 | **Parting Boost** | Minefield | — | Expire → move speed | +20–30% speed for 4–6s |
| 92109 | **Squad Drop** | Minefield | ✓ | +deploys per throw (multi-spawn ring) | +1 concurrent per stack |
| 92113 | **Painted Targets** | Quartermaster | ✓ | Friend hits mark; marked take bonus dmg | +15–25% dmg taken for 4–6s |
| 92115 | **Reactive Shell** | Quartermaster | ✓ | On player damaged → blue overhealth ICD | +8–14 overheal; CD 2.5–4s (prefers shorter CD when stacking) |
| 92117 | **Designated Target** | Fireteam | — | Friend focuses last player-hit enemy | Focus memory 4–6s |

### 8.3 Implemented — Exotics (6)

| ID | Name | Well | Stack | Intent |
|----|------|------|-------|--------|
| 92106 | **Sentry Kit** | Fireteam | — | Deploy as stationary **auto-turret** (RailBullet). Debug: green. |
| 92107 | **Lobber Kit** | Fireteam | — | Deploy as **mortar** (lobbed AoE). Debug: orange. |
| 92108 | **Buddy Protocol** | Fireteam | — | Deploy as **drone** near player. Suicide dive default; combines with Sentry/Lobber. Debug: cyan. |
| 92114 | **Scuttle Charge** | Fireteam | — | **Shoot** deployable to detonate early; power scales with remaining duration. |
| 92116 | **Calibrated Link** | Fireteam | ✓ | Mode guns blend toward primary weapon stats (dmg/effect/RoF/BPS). Portion **35–55%**. |
| 92118 | **Hive Kin** | Quartermaster | — | Deployables count as **Swarm Breeding Season** allies; extra pellets from their positions. |

### 8.4 Planned — to ~30 (12)

| ID | Name | Rarity | Well | Stack | Intent | Rough numbers (v0) |
|----|------|--------|------|-------|--------|---------------------|
| 92119 | **Decoy Protocol** | E | Quartermaster / Minefield | — | **Primary Taunt identity:** deploy becomes **enemy bait** (real aggro API if available). Hits on Friend drain duration. | **v1 must-ship sustain spine.** Aggro radius ≈ detect×1.25–1.5; duration loss per hit tunable (e.g. 0.4–0.8s) |

| 92120 | **Corridor Rush** | E | Quartermaster | — | **Hold grenade button** + **consume 1 charge** → speed corridor in aim direction. Always requires a charge (even with 0 active deploys). | Speed +25–40% while in line; line duration 2.5–4s; width ~2–3m |
| 92121 | **Hardpoint** | R | Fireteam | ✓ | Stationary turret/mortar: +fire rate and +engage range. Drones get a **smaller** bonus. | Stationary +15–25% RoF, +10–20% range; drone ~40% of that |
| 92122 | **Tripwire Lattice** | R | Minefield | — | Mine detect becomes a **forward cone/arc** (throw facing / player facing at arm) instead of pure sphere — longer reach, narrower. | Cone length +30–50% vs sphere radius; half-angle ~35–50° |
| 92123 | **Aftercare** | R | Quartermaster | ✓ | On Friend **kill**: small blue overhealth pulse to you; mild ally pulse if other players in radius. | Self 4–8 OH; allies 2–4 OH; radius ~8m |
| 92124 | **Resupply Beacon** | E | Quartermaster | — | While ≥1 Friend active: grenade recharges faster (Hearth & Home / Gas Valves cousin — **field unit**, not puddle). | −15–25% remaining recharge rate while active |
| 92125 | **Pack Mentality** | R | Fireteam | ✓ | Per **additional** active Friend beyond the first: mild damage and/or RoF to all deploys. | +6–10% mode damage per extra ally (cap +3 extras) |
| 92126 | **Failsafe Fuse** | S | Minefield | ✓ | On **expire** (no detonate): small consolation boom at a fraction of full mine power. | 25–40% damage/radius of full detonate |
| 92127 | **Shoulder Guard** | R | Fireteam | — | **Drone** (any hybrid) intercepts a fraction of damage you take (ICD); drains deploy duration. | Absorb 15–25% of hit; ICD 1.5–2.5s; duration drain 0.5–1.0s |
| 92128 | **Rally Point** | E | Quartermaster | — | Players near an active deploy gain mild **movespeed** (and optional mild reload/recharge assist). Co-op fantasy card. | +10–15% move in radius ~6–8m; refresh while inside |
| 92129 | **Chain of Command** | S | Fireteam | ✓ | Slightly longer Designated Target memory and Painted mark duration (glue). | +15–25% to those durations |
| 92130 | **Hot Swap Socket** | S | Minefield | ✓ | On **scuttle or expire**: brief personal outgoing damage buff. (Alternate to a second concurrent-stat card — avoids Squad Drop overlap.) | +10–18% damage for 2–3.5s |

*Count = 30 on the nose (18 I + 12 P).*

### 8.5 Stretch (post-30 / not required for fantasy complete)

- Vanilla staple parity (Grenade Belt, In For A Penny line, Boundary Incursion)  
- Munitions Mirror (Friend shots apply light copy of primary element amount)  
- Real mesh / VFX AssetBundle  
- Full NGO custom NetworkObject prefab  
- Enemy damage VFX on Decoy hits  
- Audio / Wwise  

---

## 9. Mode & Exotic Deep-Dives

### 9.1 Default Mine (no kit)

**Fantasy:** You left a present. Don’t step on it — they will.

- Arms at rest; scans on `ScanInterval` (0.1s).  
- First valid enemy in detect radius → full explosion.  
- Expire without boom → quiet despawn (+ Lingering Gift / Parting Boost / Failsafe Fuse if owned).  
- Debug: magenta.

### 9.2 Sentry Kit (Exotic) — Fireteam

**Fantasy:** Pop-up hardpoint. Hold the lane.

- Stationary; fires RailBullet via cached Cycler prefab.  
- Engage floor **50m** so it is not a glorified mine with a peashooter.  
- Damage taxed by `modeDamageScale` (0.35 baseline) unless Calibrated Link pulls primary stats in.  
- Does not replace throw boom identity — the *deploy* is the gun.

**Mix:** Calibrated Link, Designated Target, Hardpoint, Pack Mentality, Scuttle, Squad Drop multi-turrets.

### 9.3 Lobber Kit (Exotic) — Fireteam

**Fantasy:** Indirect fire. Soften packs behind cover.

- Stationary mortar; lobbed projectile with gravity; AOE on impact.  
- Slower cadence than sentry (~0.55× fire rate factor).  
- Fallback explosion-at-target if bullet spawn fails.

**Mix:** Wider Net (radius), Painted Targets, Squad Drop mortar battery, Buddy+Lobber flying battery.

### 9.4 Buddy Protocol (Exotic) — Fireteam

**Fantasy:** Shoulder friend. Dive or gun, your call.

- Hovers above player (`HoverHeight` 2.4) with formation offsets when multi-drone.  
- **Default:** suicide dive when enemy in ~1.5× detect → detonate.  
- **With Sentry and/or Lobber:** inherits guns, **cancels** suicide dive.  
- Multi-drone ring so Squad Drop doesn’t stack meshes.

**Mix:** Shoulder Guard (planned), Hive Kin (mobile ally bodies), Calibrated Link.

### 9.5 Scuttle Charge (Exotic) — Fireteam

**Fantasy:** Manual self-destruct. You choose when the box opens.

- While upgrade live: soft hit proxy on deploy; Fire raycast finds nearest deploy.  
- Power scales with **remaining** duration fraction (more time left → closer to full power; early scuttle ≥35%).  
- Note: shipped curve uses remaining/max so **late** life is weaker — if feel is wrong, consider inverting to “more time invested = more power” in a balance pass; **document current behaviour as source of truth** until changed.

**Design intent clarification (preferred fantasy):**  
*Power should reward remaining payload — full at fresh deploy, weaker as it ages — OR reward commitment (stronger near expire). Shipped code: `power = Lerp(0.35, 1, remaining/max)` → fresher = stronger. Keep that unless playtests hate it.*

### 9.6 Calibrated Link (Exotic) — Fireteam

**Fantasy:** Your Friend learned your gun.

- Blends primary weapon damage/effect/RoF/BPS into mode fire.  
- Stackable portion.  
- Still gated by modeDamageScale and engage rules so it cannot fully replace shooting.

### 9.7 Hive Kin (Exotic) — Quartermaster

**Fantasy:** Your box is part of the swarm family.

- All deploy forms count for Breeding Season FriendFire.  
- Soft requirement: Swarm Launcher + Breeding Season equipped to matter.  
- Stack UI shows combined ally count.

### 9.8 Exotic coexistence

| Pair | Rule |
|------|------|
| Sentry + Lobber | Allowed; primary priority makes Mortar win if both stationary |
| Buddy + either gun kit | Allowed hybrid; recommended fantasy |
| All three mode kits | Allowed if grid fits; power via duration/RoF tax, not hard ban |
| Scuttle + any | Allowed |
| Calibrated + any gun mode | Allowed; no-ops meaningfully on pure mine until a gun mode exists |
| Hive Kin + any | Allowed |
| Six exotics on one board | Grid space is the limiter |

---

## 10. Named Kit — Detailed Specs

### 10.1 Implemented (non-trivial)

#### Wider Net (Standard, stackable)
- Multiplies `explosionRadiusMultiplier` and `detectRadiusMultiplier`.  
- Also multiplies weapon `hitForce` (explosion radius source).  
- Remove path restores prefab snapshot + prefab hitForce.

#### Long Watch / Quick Deploy (Standard, stackable)
- Opposing duration ↔ cooldown dials.  
- Both may be stacked; net feel is the product of multipliers.  
- Quick Deploy floors duration and recharge multipliers at 0.25.

#### Lingering Gift (Rare)
- On expire only (not detonate / not force despawn): `SpawnAcidPuddle_Rpc` sized from explosion radius.  
- Duration from upgrade roll; max with existing data field.

#### Parting Boost (Rare)
- On expire: local player move speed buff via `FriendExpireSpeedBuff`.  
- Local player only.

#### Squad Drop (Rare, stackable)
- `maxConcurrentDeploys += extra` (usually +1).  
- One throw loops spawn with ring offsets.  
- Tracker cap raised before multi-register.

#### Sympathetic Link (Standard, stackable)
- On Friend-sourced damage: `dealt * lifestealFraction` as blue overhealth.  
- Does **not** heal missing base HP first.

#### Field Recharge (Standard, stackable)
- On Friend-sourced kill: `CooldownData.AddCharge(chargeOnKill)`.

#### Overtime (Standard, stackable)
- On Friend-sourced kill: `FriendDeployTracker.ExtendAllDurations`.

#### Painted Targets (Rare, stackable)
- On Friend damage: apply mark component with bonus + duration.  
- Player (and generally outgoing) damage amplified vs marked in `OnBeforeDamage`.

#### Reactive Shell (Rare, stackable)
- On local player took damage: grant overheal amount on cooldown.  
- Independent of whether Friend dealt damage.

#### Designated Target (Rare)
- Player non-Friend damage sets shared target.  
- Friend `TryFindEnemy` prefers it when in range and valid.

#### Mode kits (Exotics)
- OR flags into `deployMode` on Apply.  
- Remove restores prefab snapshot (clears flags unless reapplied by remaining upgrades — standard snapshot caveat: full restore then re-Apply all is PlayerData’s job on rebuild).

### 10.2 Planned

#### Decoy Protocol (Epic) — **prefer real aggro API**
**Fantasy:** They shoot the box, not you.

**Behaviour sketch:**
1. While deploy active, enemies in aggro radius should prefer the deployable as a target.
2. **Implementation order:**  
   - Search decompile for enemy brain / threat / target selection (`ITarget`, brain target fields, taunt-style APIs, damage-threat tables).  
   - If a **real aggro/retarget API** exists, use it (set target, add threat, taunt flag, etc.).  
   - If not available safely: fallback heuristic — deploy deals tiny periodic “tag” damage or uses any vanilla “force target” pattern found on decoy-like gear; last resort = duration-tax-only “paper tank” without true aggro (weaker card — avoid if possible).
3. When deploy takes hit (if damageable) **or** on a proxy “aggro pulse ICD”, drain `remainingDuration`.
4. Optional: slightly larger debug silhouette / threat VFX when Decoy owned.
5. Bosses / CC-immune: reduced aggro strength or duration tax only.

**Risk budget:** Must not permanently park bosses on an immortal box — duration drain and finite lifetime are the valves.

#### Corridor Rush (Epic)
**Fantasy:** Pop a charge, paint a runway, go.

**Input:** Hold grenade button (throwable alt/hold path — match game grenade hold semantics during impl).  
**Cost:** **Always consumes 1 full charge** (locked). Works even with **zero** active deploys. Fails with empty feedback if no charge.  
**Effect:** Spawn a temporary speed volume/line along aim forward from player:
- Duration D, width W, length L (v0: D 2.5–4s, W 2–3m, L 12–18m).  
- Allies (optional v1: local only; v1.1: teammates) gain movespeed while overlapping.  
- Does not deal damage baseline.  
**Not:** a second Live Wire full self-nade rewrite.

#### Hardpoint (Rare, stackable)
- If primary mode is Turret or Mortar (stationary): apply RoF and range mults.  
- If Drone hybrid: apply reduced mults.  
- Pure mine: no-op or tiny detect bonus (prefer **no-op** for clarity).

#### Tripwire Lattice (Rare)
- Mine-only detect shape becomes cone oriented at arm time (store yaw on spawn).  
- Turret/mortar/drone ignore or keep sphere (prefer **mine-only**).

#### Aftercare (Rare, stackable)
- Friend kill callback: blue overhealth pulse self + optional allies in radius.  
- Uses same overhealth API family as Sympathetic Link.

#### Resupply Beacon (Epic)
- While `FriendDeployTracker.HasActive` and local owns Friend: multiply recharge speed / reduce recharge timer.  
- Clear optional stack display “Beacon”.  
- Multiple deploys do not stack beacon infinitely (once active = on).

#### Pack Mentality (Rare, stackable)
- Each tick or on fire: `bonus = min(extraAllies, cap) * perAlly`.  
- Applies to mode damage scale or outgoing Friend damage.

#### Failsafe Fuse (Standard, stackable)
- On Expire path before destroy: small explosion fraction.  
- Does **not** fire on ForceDespawn replace.  
- Does **not** double with full detonate.

#### Shoulder Guard (Rare)
- Requires drone flag on at least one active deploy.  
- On player damage: reduce damage by absorb fraction, ICD, drain duration on the nearest drone.  
- If no drone alive, no-op.

#### Rally Point (Epic)
- Players in radius of any active deploy gain movespeed buff (refresh while inside).  
- Optional mild equipment recharge — keep mild so it doesn’t obsolete character kits.  
- Readable ring VFX preferred.

#### Chain of Command (Standard, stackable)
- Multiplies `sharedTargetDuration` and `markDuration` on Apply.

#### Hot Swap Socket (Standard, stackable)
- On scuttle detonate **or** expire: grant short personal damage buff.  
- ICD per proc so Squad Drop multi-expire doesn’t machine-gun the buff.  
- Alternate pick vs another concurrent-deploy stat card (avoids Squad Drop redundancy).

---

## 11. Soft Synergy Notes (Player-Facing)

| Partner | Why it feels good |
|---------|-------------------|
| **Swarm Launcher + Breeding Season** | Hive Kin hard synergy — pellets from every Friend body |
| **Any Coop-tagged Ouroboros upgrades** | Signature passive while Friend equipped |
| **High RoF / multi-hit primaries** | Calibrated Link + Designated Target |
| **Acid kits / puddle love** | Lingering Gift expire puddles |
| **Mobility characters** | Parting Boost, Corridor Rush, Rally Point |
| **Honey Jar** | Two minion philosophies; no code link |
| **Splash Canister** | Zones + units; no code link |
| **Incendiary economy cards on other nades** | N/A directly — Friend is separate gear |
| **Shield / overhealth stacks** | Sympathetic + Reactive + Aftercare layer with character defenses |

**Explicit non-goals for hard hooks:** Honey Jar hive API, Splash lake infusion, DiscWorld, shared cross-mod trackers beyond Hive Kin.

---

## 12. Strengths, Weaknesses & Failure Modes

### Strengths
- Only dedicated **player mine → multi-mode deployable** identity  
- Solo Ouroboros **Coop upgrade access** while equipped  
- High build diversity (mine control / gun emplacement / drone wing / sustain)  
- Real projectile reuse (Rail / Mortar) keeps gunfeel on-brand  
- Swarm Breeding Season bridge without being a Swarm clone  
- Mix-and-match wells; hybrids first-class  

### Weaknesses
- Baseline mine is passive — weak if enemies never enter radius  
- Mode DPS taxed (`modeDamageScale`) without Calibrated investment  
- Duration mismanagement / replace-despawn can feel punishing  
- Hive Kin blank without Swarm + Breeding Season  
- Local-authority sandbox limits true co-op deploy sync until NGO pass  
- Six exotics compete hard for grid space  

### Failure modes to avoid in tuning

| Failure | Mitigation |
|---------|------------|
| Solo Coop unlock trivializes Ouroboros | Unlock is access, not free power on Friend; don’t also give Friend absurd baseline DPS |
| Multi-Sentry + Calibrated + Squad = second primary | Keep modeDamageScale; Pack Mentality caps; concurrent soft caps |
| Lifesteal + Reactive + Aftercare immortal | Blue OH only; no base HoT; ICD on Reactive; modest Aftercare |
| Decoy immortal boss park | Duration drain; finite life; boss resist |
| Corridor Rush infinite sprint | **Hard charge cost every use** (locked) |
| Shoulder Guard deletes damage | Absorb fraction + ICD + duration tax |
| Resupply Beacon + Field Recharge infinite nades | Beacon mild; kill charge not 1.0 full; CD floors |
| Failsafe + Lingering Gift double dip too strong | Failsafe is fractional boom; puddle separate |
| Scuttle snipes feel bad | Hit proxy radius; generous ray find distance |
| Hive Kin without Swarm confuses players | Description must say Breeding Season explicitly |
| 30 upgrades but 3 builds only | Keep standards universal; no exclusive path locks |
| Hot Swap Socket + multi expire buff spam | Proc ICD |
| Friend replaces needing friends entirely | Rally/Hive Kin still better with real players; coop unlock is the solo concession |

---

## 13. Implementation Appendix

### 13.1 Existing map (source of truth in code)

| Piece | Type / file |
|-------|-------------|
| Plugin / IDs | `FriendinaBoxPlugin` |
| Gear clone + AllGear inject | `GrenadeRegistration` |
| Data host | `FriendinaBoxBehaviour` / `Data` |
| Detonate → arm | `FriendGrenadeHooks` |
| Field entity | `FriendDeployable` |
| Modes | `FriendDeployMode` / `FriendDeployModeUtil` |
| Tracker / formation / allies | `FriendDeployTracker` |
| Combat / scuttle runner | `FriendCombatHooks`, `FriendCombatRunner` |
| Marks / shared target | `FriendMarkedTarget`, `FriendSharedTarget` |
| Expire speed | `FriendExpireSpeedBuff` |
| Bullet prefabs | `FriendBulletCache` |
| Coop filter | `CoopUnlockHook` |
| Swarm bridge | `SwarmFriendFireHooks` |
| Equip stamp | `SpawnGearHooks` |
| Upgrade properties | `FriendUpgradeProperties` |
| Upgrade create helper | `UpgradeRegistration` |
| Debug draw | `FriendDebugVisual` |

### 13.2 Shipped `FriendinaBoxBehaviour.Data` fields

```
deployDuration
detectRadius
detectRadiusMultiplier
explosionRadiusMultiplier
acidPuddleOnExpireDuration
expireMoveSpeedBonus
expireMoveSpeedDuration
deployMode
fireRate
modeDamageScale
mortarRangeMultiplier
droneFollowDistance
droneMoveSpeed
maxConcurrentDeploys
lifestealFraction
chargeOnKill
durationOnKill
markDamageBonus
markDuration
shootToDetonate
overhealOnDamaged
overhealOnDamagedCooldown
copyPlayerWeaponStats
weaponStatPortion
sharePlayerTarget
sharedTargetDuration
countsAsSwarmAlly
```

### 13.3 Planned data fields (sketch)

```
// Decoy Protocol
bool decoyProtocol;
float decoyAggroRadiusMult;
float decoyDurationLossPerHit;
// runtime: damageable proxy / threat handle

// Corridor Rush
bool corridorRush;
float corridorSpeedBonus;
float corridorDuration;
float corridorWidth;
float corridorLength;
// input runner + charge consume

// Hardpoint
float hardpointFireRateMult;
float hardpointRangeMult;
float hardpointDroneEffectiveness; // 0–1

// Tripwire Lattice
bool tripwireLattice;
float tripwireConeHalfAngle;
float tripwireLengthMult;
float armYaw; // runtime on deployable

// Aftercare
float aftercareOverhealSelf;
float aftercareOverhealAlly;
float aftercareRadius;

// Resupply Beacon
bool resupplyBeacon;
float resupplyRechargeMult;

// Pack Mentality
float packMentalityDamagePerExtra;
int packMentalityExtraCap;

// Failsafe Fuse
float failsafePowerFraction; // 0 = off

// Shoulder Guard
bool shoulderGuard;
float shoulderAbsorbFraction;
float shoulderIcd;
float shoulderDurationDrain;

// Rally Point
bool rallyPoint;
float rallyMoveSpeed;
float rallyRadius;

// Chain of Command
float commandDurationMult;

// Hot Swap Socket
float hotSwapDamageBonus;
float hotSwapBuffDuration;
float hotSwapIcd;
```

### 13.4 Ship cut vs stretch

**Fantasy-complete today (shipped):**
- Baseline mine + duration + replace rules  
- Coop unlock while equipped  
- 3 mode kits + hybrids  
- Scuttle, Calibrated Link, Hive Kin  
- Minefield spine + quartermaster combat loop (18 upgrades)  
- Squad Drop multi-spawn + drone formation  

**Design-complete when doc accepted:**  
- Full ~30 table with Planned specs (this document)

**Code-complete for ~30:**  
- Implement 12 Planned properties + runners (Corridor input, Decoy aggro research, Rally/Beacon ticks, etc.)

**Stretch:** mesh, audio, NGO prefab, vanilla staples, Munitions Mirror.

---

## 14. Naming & Presentation

| Slot | Value |
|------|--------|
| Display name | **Friend in a Box** |
| Internal / API | `friend_in_a_box` |
| Short description | *Deployable ally grenade. Lands as a proximity mine. Upgrades convert it into turrets, mortars, or drones, and let it **Taunt** so enemies shoot the box. While equipped, enables multiplayer-only upgrades in Ouroboros.* |

| Thunderstore name | `FriendinaBox` |
| GUID | `sparroh.friendinabox` |
| Gear id | 92100 |
| Upgrade ids | 92101–92130 |

### Flavor (optional codex)

> “Contents: one (1) friend. Warranty void if friend is used as a chair, a football, or a marriage counselor. SAXON is not liable for feelings of attachment.”  
> — Side of crate, Wetworks / Field Ops joint SKU

---

## 15. Open Questions (Balance / Feel — Not Blocking Doc)

1. Scuttle power curve: keep fresher = stronger, or invert after playtests?  
2. Sentry + Lobber both equipped without drone: confirm mortar-primary is desired forever.  
3. Decoy: exact decompile API name once aggro research lands — swap fallback if missing.  
4. Corridor Rush: local-only vs allies in line for v1.  
5. Exact hex shapes for all 30 — author during implementation/UI pass.  
6. Utility exotic footprint vs mode exotic footprint equality.  
7. Max practical concurrent deploys before perf/UX collapse (suggest soft feel cap ~4–5).  
8. Rally Point reload assist: include or movespeed-only?  

### Locked decisions (this pass)

| Topic | Lock |
|-------|------|
| File format | `.txt` fine (same as DMLR/Cycler) |
| Card #30 | **Hot Swap Socket** (not Spare Chassis) |
| Corridor Rush cost | **Always requires/consumes a charge** |
| Decoy Protocol | **Real aggro API if available**; research at impl |

---

## 16. Design Checklist

- [x] Niche: Deployable ally / mine / mode kits / solo coop / **Taunt**  
- [x] Sustain identity: **Taunt** (Decoy Protocol spine; overhealth secondary only)  
- [x] Shared family baseline: dmg **100** · effect **10** (Normal) · charges **3** · recharge **45** · `hitForce` **6**  
- [x] Baseline: arm mine, 20s, detect 75% explode, concurrent 1  

- [x] Signature passive: Coop unlock while equipped  
- [x] ~30 upgrades (18 Implemented + 12 Planned)  
- [x] Wells interactive (Minefield / Fireteam / Quartermaster)  
- [x] Six Exotics documented as intentional shipped state  
- [x] Hive Kin = only hard cross-weapon hook  
- [x] Hybrids first-class  
- [x] Planned set covers outline leftovers (aggro bait, hold-to-speed line)  
- [x] Corridor Rush charge lock  
- [x] Hot Swap Socket as #30 alternate  
- [x] Decoy prefers real aggro API  
- [x] Competitive differentiation vs Honey / Splash / vanilla nades  
- [x] Implementation map for existing code  
- [ ] Hex patterns authored for all 30  
- [ ] Planned 12 implemented in code  
- [ ] Aggro API confirmed in decompile during Decoy impl  

---

## 17. Changelog (Design Doc)

| Date | Change |
|------|--------|
| 2026-08-15 | **Shared throwable baseline lock:** damage **100**, `damageEffectAmount` **10**, max charges **3**, recharge **45**, explosion radius (`hitForce`) **6**. Element locked **Normal** (not Fire lineage). Sustain identity **intentionally** shifted from blue overhealth to **Taunt** (Decoy Protocol elevated to load-bearing); overhealth cards demoted to secondary utility. |
| (prior) | Initial thin outline (high concept, path names, bullet upgrade ideas). |
| 2026-08-06 | **Full design doc v1:** Honey/Splash-class structure; document shipped 18 + systems; expand to 30 with Implemented/Planned; wells Minefield/Fireteam/Quartermaster; keep 6 Exotics; locks: Hot Swap Socket #30, Corridor Rush always costs charge, Decoy real aggro API if available; wiki-informed planned cards (Acid/Shock/Incendiary/Swarm patterns). |


---

## 18. Quick Reference — Shipped Upgrade IDs

```
92101 Wider Net
92102 Long Watch
92103 Quick Deploy
92104 Lingering Gift
92105 Parting Boost
92106 Sentry Kit
92107 Lobber Kit
92108 Buddy Protocol
92109 Squad Drop
92110 Sympathetic Link
92111 Field Recharge
92112 Overtime
92113 Painted Targets
92114 Scuttle Charge
92115 Reactive Shell
92116 Calibrated Link
92117 Designated Target
92118 Hive Kin
```

## 19. Quick Reference — Planned Upgrade IDs

```
92119 Decoy Protocol
92120 Corridor Rush
92121 Hardpoint
92122 Tripwire Lattice
92123 Aftercare
92124 Resupply Beacon
92125 Pack Mentality
92126 Failsafe Fuse
92127 Shoulder Guard
92128 Rally Point
92129 Chain of Command
92130 Hot Swap Socket
```

---

*End of design document. Next engineering step when ready: implement Planned tier starting with Failsafe Fuse / Chain of Command / Hot Swap Socket (data-only), then Hardpoint / Pack Mentality, then runners (Resupply, Rally, Corridor), then Decoy aggro research last.*
