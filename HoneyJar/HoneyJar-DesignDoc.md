# Honey Jar (Beenade) — Design Document

> Status: **Design only** — no implementation yet.  
> Working title in notes: Beenade. **Ship name: Honey Jar.**  
> Template base: `.new.HoneyJar` grenade content project.

---

## 1. High Concept / Fantasy

**Honey Jar** is the throwable hive.

Where **Swarm Launcher** is a *carried* swarm you release on command, and **The Accelerator** is *personal* bee status and self-swarm power fantasy, Honey Jar is **territory bees**: you seed the battlefield with living damage that keeps working after the throw, while **nectar** (temporary regeneration) keeps you in the fight long enough for natural regen — and long enough to lean into self-damage loops.

**One-liner:** *Throw a jar of bees. They work the field while nectar keeps you fighting.*

**Element:** `EffectType.Bees` (not Normal, Fire, or Acid).  
This is load-bearing identity: bee saturation, Accelerator synergies (Wasp Gun / Requeening / Combustive Rounds), and Queen’s Arsenal all key off **Bees** as a real damage effect.

---

## 2. Role in the Arsenal

| Gear | Fantasy | Honey Jar relationship |
|------|---------|------------------------|
| **Swarm Launcher** | Hover pellets, dive on release, FriendFire allies | Thematic cousin (minions). Honey Jar does **not** hard-depend on Swarm; no Breeding Season / Hive Kin hooks in v1. |
| **The Accelerator** | Shock bursts + optional bee kit (Wasp Gun, Sweet Heavens, Requeening) | Soft synergy only: self-bees and ally bees from the jar feed existing Accelerator upgrades. No code coupling. |
| **Friend in a Box** | One smart deployable ally (mine/turret/mortar/drone) | Complementary slot. Friend = discrete unit AI. Honey Jar = **bee status + clouds/hives/cloak/nebula**. Do not clone Friend modes. |
| **Incendiary Grenade** | Instant fire boom + self-fire economy | Honey Jar trades burst for **linger, stacks, and temp regen**. |
| **Acid Grenade** | Puddles, pull, overhealth toys | Honey Jar linger is **mobile/seeking bees**, not ground acid. |

**Niche tags:** Minions · Temporary Regeneration · Bee Element · Field Control

---

## 3. Design Pillars

1. **Minions first**  
   Upgrades should create *things that keep working* (cloud, hive, cloak drones, nebula orbiters), not only bigger boom numbers.

2. **Temporary regen, not permanent tank**  
   Nectar is a short HoT window. It bridges into natural regen and enables aggressive / self-damage play, then falls off. **Pure HoT only** — no blue overhealth, no permanent regen rate buffs.

3. **Bee element identity**  
   Apply and stack `EffectType.Bees`. Saturation and DoT are the spine. Unique vs Fire/Acid grenades and vs generic elemental boosts (bee damage is its own type).

4. **Gravity wells, not locked paths**  
   Upgrade “paths” are **thematic attractors** (Apiary / Colony / Nebula). Players mix and match freely. Nothing is path-exclusive except where a rare explicitly *requires* an exotic flag (e.g. Attack Drones → Hive Master). Builds emerge from gravity, not from forced trees.

5. **Readable exotics**  
   Three mode-defining exotics: stationary hive · personal cloak · ally mobility nebula. Same large hex footprint each.

6. **Risk / reward on self-bees**  
   The jar *can* sting you. Swarmkeeper and self-application are spicy enablers (Stolen Knights, Accelerator Requeening), not free power.

7. **Self-contained v1**  
   No hard or soft dependencies on FriendinaBox, Swarm FriendFire hooks, or other mods. Wiki synergies are **player-facing notes only**.

---

## 4. Baseline (No Upgrades)

**Feel target:** status-first throwable with a readable minion aftertaste. Shared family boom numbers; identity is bees + cloud + nectar upgrades.

**Shared family baseline (locked):** damage **100** · effect amount **10** · max charges **3** · recharge **45** · explosion radius (`hitForce`) **6**.

| Property | v0 target | Notes |
|----------|-----------|--------|
| Throw / arc / bounce | Incendiary-like | Clone throw feel; swap element + behaviour |
| Impact damage | **100** | Shared family boom; delivery + bee apply |
| Element | `EffectType.Bees` | |
| Bee effect amount (`damageEffectAmount`) | **10** | Full-sat class dump from empty (shared family); cloud/hive re-apply holds stacks |
| Explosion radius (`hitForce`) | **6** | Tunable via Wide Comb |
| **Aftershock cloud** | **Weak linger bee cloud, ~1.5–2.5s** | Sells minion niche immediately; Kick the Nest upgrades this |
| Cloud tick | Small damage + small bee application in radius | Weaker than boom; presence > power |
| Healing | **None** | Nectar (**Heal over Time**) is upgrade fantasy only |
| Max charges | **3** | Shared family baseline; Double Brood stacks on top |
| Recharge duration | **45** | Shared family baseline |
| Self-hit | Bees **can** apply to thrower at reduced amount | Enables cloak / Stolen Knights / Requeening without forcing suicide |
| Self damage | Low impact damage to self vs enemies | Soft Landing reduces self bee amount further |


### Baseline detonation sequence

```
Throw → bounce/fuse (vanilla grenade rules)
  → Primary boom (DamageData: modest damage, EffectType.Bees, high effect amount, AOE)
  → Spawn weak BeeCloud at impact (short duration, small tick rate)
  → Cloud expires quietly (no second nuke unless upgraded)
```

### Why boom + weak cloud (not boom-only / always-hive)

- Boom-only reads as “another elemental nade.”
- Always-hive steals Hive Master’s exotic beat.
- Weak cloud teaches the niche in the first throw and gives Apiary upgrades something to grow.

---

## 5. Bee & Nectar Rules (System Truths)

### 5.1 Bees element (from game)

- `BeeStatusEffect`: full saturation duration **5s** (longer than default **3s**).
- Once fully saturated, DoT ticks (enemy ≈ 10, player ≈ 0.165 per tick — vanilla constants).
- Saturation add: `amount * 0.1` per application → roughly **10 effect amount ≈ full swarm** from empty, before decay.
- Bees last longer / build differently than Fire/Acid; design around **stacking and re-applying**, not only one-shot ignite.

### 5.2 “Bees applied” triggers

For upgrade text, **applying bees** means: dealing damage/effect that adds bee saturation to a target (boom, cloud tick, hive sting, cloak retaliate, sticky residual, etc.).

**Fully swarmed** means: target has bee status at full saturation (`IsFullySaturated`).

### 5.3 Temporary regeneration (Nectar) — pure HoT

```
NectarRegen buff (player or ally):
  regenPerSecond = R          // from upgrade rolls
  duration       = D          // e.g. 2.5–4.0s typical
  refresh        = re-apply extends remaining time, capped at Dmax
  stack policy   = single instance per target (refresh only; no infinite multi-HoT)
  type           = healing over time to base health only
  NOT included   = blue overhealth, damage resist, permanent regen delay changes
```

**Design intent:** help the player stay up *before* natural regen kicks in; pair with self-damage effects; never replace defensive gear.

| Source upgrade | Who | When |
|----------------|-----|------|
| Sugar Coated | Self | When **you** apply bees (to anyone, or only to enemies — prefer **any apply you cause**) |
| Symbiotic Nectar | Ally | When bees are applied to that ally |
| Nectar Nebula | Allies in cloud | Periodic weak HoT while inside (+ antigrav/momentum) |
| Swarmkeeper | — | **No free HoT**; optional tiny self HoT only when cloak bees damage an **enemy** (stretch — default off) |

---

## 6. Gravity Wells (Thematic Attractors)

Not exclusive trees. Think **magnets** on the upgrade board: taking one exotic or epic pulls related rares/standards into value, but every upgrade remains equippable with every other (except explicit requirement flags).

### Well A — **Apiary** (field minions)

*Seed the map. External DPS. Things that sit and work.*

Core fantasy pieces: Kick the Nest, Hive Master, Attack Drones, Pheromone Burst, Recon Drones, cloud/hive duration & radius standards.

### Well B — **Colony** (status economy + nectar)

*Stacks, self/ally bees, recharge while swarmed, temp regen, element amp.*

Core fantasy pieces: Swarmkeeper, Neurotoxin, Honey Bomb, Sugar Coated, Stolen Knights, Symbiotic Nectar, Queen’s Arsenal.

### Well C — **Nebula** (space control + mobility support)

*Zone denial, ally flight/momentum, orbiting bees, soft support control.*

Core fantasy pieces: Nectar Nebula, density/duration epics, in-cloud pressure rares.

**Mix examples (expected, not edge cases):**

- Hive Master + Sugar Coated + Queen’s Arsenal → apiary DPS + personal nectar + bee amp  
- Swarmkeeper + Stolen Knights + Kick the Nest → self-swarm recharge engine + stronger field cloud  
- Nectar Nebula + Symbiotic Nectar + Pheromone Burst → support zone that also amps bee pressure  
- Honey Bomb + Neurotoxin + Thick Nectar → sticky stack pump  

---

## 7. Content Budget & Universal Truths

Aligned with FriendinaBox / vanilla grenade expectations:

| Rule | Value |
|------|--------|
| Total upgrades | **~30** (named kit + standards + fillers) |
| Exotics | **3** — Hive Master, Swarmkeeper, Nectar Nebula |
| Exotic hex footprint | **Equal and large** across all three |
| Epics | **~7–8** |
| Rares | **~9–10** |
| Standards | **~8–10** (stackable spine) |
| Path locks | **Avoid**; only **Attack Drones** requires Hive Master |
| Oddity / Contraband grid steal | Out of scope unless later parity pass |
| v1 cross-mod | **None** |

---

## 8. Full Upgrade Table

IDs are design placeholders (implementation range suggestion: **92200+** gear, **92201+** upgrades — avoid template `920xx` and Friend `921xx`).

Rarity key: **S** Standard · **R** Rare · **E** Epic · **X** Exotic  

Stack: **✓** CanStack · **—** unique  

Well: primary gravity well (still mixable)

### 8.1 Standards (~9) — stat spine

| # | Name | Well | Stack | Intent | Rough numbers (v0) |
|---|------|------|-------|--------|---------------------|
| 1 | **Wide Comb** | Apiary | ✓ | Explosion + cloud radius | +15–30% radius |
| 2 | **Thick Nectar** | Colony | ✓ | Bee effect amount on boom (and cloud slightly) | +20–40% effect amount |
| 3 | **Quick Jar** | Colony | ✓ | Faster recharge | −12–20% recharge |
| 4 | **Deep Cell** | Apiary | ✓ | Cloud / hive / nebula duration | +20–35% linger durations |
| 5 | **Soft Landing** | Colony | ✓ | Less self bee application from your jar | −25–40% self effect amount |
| 6 | **Wax Seal** | Colony | ✓ | Impact damage | +12–22% boom damage |
| 7 | **Double Brood** | Apiary | ✓ | + max charges; longer recharge | +1 charge; +15–25% CD |
| 8 | **Pollen Trail** | Nebula | ✓ | Short bee application along throw arc / near flight path | Light trail ticks |
| 9 | **Comb Pressure** | Apiary | ✓ | Cloud tick rate / tick damage | +15–25% cloud DPS |

### 8.2 Rares (~10)

| # | Name | Well | Stack | Intent | Notes |
|---|------|------|-------|--------|-------|
| 10 | **Sugar Coated** | Colony | ✓ | When you apply bees, gain **temporary HoT** that wears off after a delay | Primary healing identity |
| 11 | **Stolen Knights** | Colony | ✓ | Recharge faster **while bees are applied to you** | Pairs Swarmkeeper / self splash |
| 12 | **Queen’s Arsenal** | Colony | ✓ | Outgoing damage matching this grenade’s element (**Bees**) increased | Napalm analogue |
| 13 | **Symbiotic Nectar** | Colony | ✓ | Applying bees to **allies** grants them short **movespeed + temp HoT** | Support colony |
| 14 | **Attack Drones** | Apiary | ✓ | While **Hive Master** equipped: drones seek farther and more frequently | **Requires Hive Master** |
| 15 | **Brood Link** | Apiary | ✓ | Hive / cloud / cloak kills refund small grenade charge | Minion economy |
| 16 | **Alarm Pheromones** | Apiary | — | First enemy to enter your cloud/hive radius each deploy gets a heavy bee dump | Burst apply |
| 17 | **Royal Attendance** | Nebula | ✓ | Allies inside your cloud/nebula take slightly less bee self-pressure from *your* effects; enemies take slightly more tick damage | Soft support bias |
| 18 | **Waxwing** | Nebula | ✓ | You gain a brief movespeed burst when your jar detonates | Mobility glue |
| 19 | **Cell Divider** | Apiary | ✓ | Weak cloud splits into 2 smaller clouds on expire (or on kill) | Minion multiplication |

### 8.3 Epics (~8)

| # | Name | Well | Stack | Intent | Notes |
|---|------|------|-------|--------|-------|
| 20 | **Kick the Nest** | Apiary | — | Creates / greatly strengthens a **lingering cloud of bees** | Baseline cloud → serious field denial |
| 21 | **Neurotoxin** | Colony | — | While bees is on a target, continuing to apply bees deals **bonus damage every 10 stacks applied** | Stack milestone nuke |
| 22 | **Honey Bomb** | Colony | — | On impact, the grenade **sticks** to its target | Stick → fuse/boom on body |
| 23 | **Pheromone Burst** | Apiary | — | Initial explosion releases pheromones: **draw / amplify** nearby bee attacks (cloud, hive, cloak, DoT) | Amp window in radius |
| 24 | **Recon Drones** | Apiary | — | Active bees periodically **reveal enemy health / parts** in range (training-dummy style highlight) | Info minion |
| 25 | **Mitosis Jar** | Apiary | ✓ | Cloud / hive periodically spawns a weak seeking bee bolt | Extra minion pressure |
| 26 | **Comb Density** | Nebula | ✓ | Nebula / cloud lasts longer and is larger; slightly stronger in-zone bee ticks | Nebula gravity |
| 27 | **Hiveguard** | Colony | — | While you are swarmed, take reduced damage from your **own** bee ticks (not enemy damage) | Makes Swarmkeeper livable |

### 8.4 Exotics (3)

| # | Name | Well | Stack | Intent |
|---|------|------|-------|--------|
| 28 | **Hive Master** | Apiary | — | Detonation leaves a **stationary hive** that seeks nearby targets and stings (bee apply + damage) for a duration |
| 29 | **Swarmkeeper** | Colony | — | You wear a **cloak of bees**: they attack your attackers, and **occasionally sting you** |
| 30 | **Nectar Nebula** | Nebula | — | Explodes into a **cosmic honey nebula**: bees orbit inside; allies gain **flight/momentum (antigrav)** and light nectar HoT |

*Count = 30 on the nose. Stretch candidates live in §12 if a few standards merge in balance.*

---

## 9. Exotic Deep-Dives

### 9.1 Hive Master (Apiary)

**Fantasy:** You plant a hive. It works the room.

**Behaviour sketch:**

- On detonate (after boom + cloud rules): spawn **Hive** at impact (or cloud center).
- Duration: medium (e.g. 8–14s v0), improved by Deep Cell.
- Pulse interval: seeks nearest valid enemy in radius; applies bees + small hit.
- Does **not** replace the boom; it is the exotic afterlife of the throw.
- Visual: stationary comb / jar remnant (placeholder OK).

**Attack Drones (Rare):**  
If Hive Master is equipped, increase seek range and reduce pulse interval (stackable mild). If Hive Master unequipped, property no-ops (or hidden in UI if API allows — otherwise grey text “Requires Hive Master”).

**Mix notes:**  
Works under Nectar Nebula (hive at center). Works with Swarmkeeper (independent). Pheromone Burst should amp hive pulses in the window.

### 9.2 Swarmkeeper (Colony)

**Fantasy:** You *are* the nest. Power at a sting tax.

**Behaviour sketch:**

- While equipped and upgrades enabled: maintain **BeeCloak** on the local player.
- On player damaged by an enemy: cloak bees retaliate (bee apply + small damage to attacker), ICD to prevent chaos.
- **Self sting:** periodic or chance-based small bee application (and tiny damage) to the wearer so Stolen Knights / Requeening / Hiveguard have a loop.
- Soft Landing reduces accidental boom self-apply; Hiveguard reduces *cloak tax* pain; neither should delete the fantasy.

**Risk budget:**  
Cloak should never out-DPS a careful player’s survivability without investment. Self sting is telegraphed (stack display / audio).

**Mix notes:**  
Stolen Knights is the recharge engine. Sugar Coated can proc off cloak applies. Queen’s Arsenal amps cloak bee damage. Neurotoxin loves repeated applies from cloak + jar.

### 9.3 Nectar Nebula (Nebula)

**Fantasy:** A honey cosmos — bees in orbit, allies float and push.

**Behaviour sketch:**

- Detonation replaces or upgrades the linger into a **Nebula zone** (larger, longer than baseline cloud).
- **Enemies:** orbiting bee ticks (apply + damage), pressure to leave or die swarmed.
- **Allies (including thrower):**  
  - Antigrav / low gravity (reuse `AntigravityField` or `IGravityModifier` pattern)  
  - Momentum / move assist (short speed add while inside)  
  - **Light pure HoT** (weaker than Sugar Coated peak, but areal)
- Comb Density scales size/duration/ticks.
- Symbiotic Nectar can still proc if nebula applies bees to allies (ally bee apply → movespeed + HoT refresh).

**Mix notes:**  
Not a second Friend drone. No turret AI. Zone control + support mobility is the exotic beat.

### 9.4 Exotic coexistence

| Pair | Rule |
|------|------|
| Hive Master + Nectar Nebula | Allowed. Hive sits in nebula; strong but expensive board space. |
| Swarmkeeper + either | Allowed. Cloak is player-bound. |
| All three | Allowed if grid fits; power budget via duration/ICD, not hard ban. |
| Footprints | All three exotics **same cell count**, larger than typical rares/epics. |

---

## 10. Named Kit — Detailed Specs

### Kick the Nest (Epic)

- Greatly increases baseline cloud duration and tick strength.
- If Nectar Nebula is also equipped, feeds into nebula density rather than double-spawning conflicting zones (implementation: nebula consumes/upgrades cloud layer).
- Stack: unique.

### Neurotoxin (Epic)

- Track **bee application stacks caused by you** on each target while they have bee status (or global apply counter per target).
- Every **10 stacks applied**, deal bonus damage instance (bee-typed or true-ish burst — prefer `EffectType.Bees` with 0 extra sat to avoid infinite loop, or Normal with flag).
- Resets or continues per saturation life — prefer **continues while status present**, counter persists across re-saturations for the fight window (cap per target).
- Stack: unique.

### Honey Bomb (Epic)

- Projectile **sticks** to first valid enemy (or surface if no target — designer choice: **prefer enemy stick, else normal bounce/fuse**).
- On stick: shortened fuse then boom, or boom on stick after brief arm (v0: **arm 0.4–0.8s then detonate**).
- Sticky residual: optional small bee drip on stuck target before boom (stretch).
- Stack: unique.

### Pheromone Burst (Epic)

- On primary boom: apply **Pheromone** window in radius for T seconds.
- While active: bee DoT ticks, hive pulses, cloak retaliations, and cloud ticks in radius deal increased damage and/or apply extra bee amount.
- Optional light pull toward epicenter (careful with gameplay feel; pull can be mild or VFX-only in v0).
- Stack: unique.

### Recon Drones (Epic)

- While you have active bees on enemies in range (or active cloud/hive), periodically highlight enemy parts / show health emphasis similar to hub training dummies.
- Info only — no damage. ICD per enemy.
- Stack: unique.

### Sugar Coated (Rare)

- On successful bee application from your Honey Jar systems → refresh **NectarRegen** on self.
- Pure HoT; duration short; stackable upgrades improve R and/or D.
- Does not grant overhealth.

### Stolen Knights (Rare)

- While local player has bee saturation > 0 (or full swarm only — prefer **any bees on you**): multiply recharge rate / reduce remaining CD speed.
- Clear stack display while active.

### Queen’s Arsenal (Rare)

- `GenericGrenadeData.outgoingDamageMultiplier`-style, but filtered to damage with `EffectType.Bees` (mirror Incendiary Napalm pattern).
- Affects jar boom, cloud, hive, cloak, nebula ticks you own.

### Symbiotic Nectar (Rare)

- When bees applied to an ally (friendly player): grant them movespeed buff + NectarRegen HoT.
- Your boom self-hit should not count as “ally” unless coop friendly fire edge cases — specify **other players only** for movespeed; optional self excluded.

### Attack Drones (Rare)

- Gate: Hive Master equipped.
- +seek range, −pulse interval; mild stack.

---

## 11. Synergy Notes (Player-Facing, Soft Only)

No mod dependencies. These are loadout tips for the README / codex blurb.

| Partner | Why it feels good |
|---------|-------------------|
| Accelerator **Wasp Gun** | Shared bee status language |
| Accelerator **Requeening** | Self-bees from jar/Swarmkeeper enable “while swarmed” buffs |
| Accelerator **Honey Sweet** | Ally bee applies can double as heal bullets fantasy |
| Accelerator **Sweet Heavens** | Parallel heal-then-sting fantasy; jar is the throwable half |
| Accelerator **Combustive Rounds** | Non-elemental grenades convert to bees; Honey Jar **already is bees** — clean magazine convert |
| Swarm Launcher | Thematic minion duo; Munition Siphon still feeds any grenade charge |
| Friend in a Box | Two different minion philosophies in one loadout |
| Scrapper **Beehive!** | Pure flavor crossover |

**Explicit non-goals v1:** Breeding Season ally spoofing, Friend deployable integration, shared trackers across mods.

---

## 12. Strengths, Weaknesses & Failure Modes

### Strengths

- Map presence after the throw  
- Stacking DoT and multi-source bee pressure  
- Temporary HoT enables aggressive play and self-damage synergies  
- Only dedicated **Bees** grenade identity  
- High multiplayer support ceiling (nebula + symbiotic) without being a pure healer  
- Mix-and-match wells create many boards  

### Weaknesses

- Lower raw burst than Incendiary  
- Minions need time; weak vs pack that instantly leaves the zone  
- Self-bee risk without Soft Landing / Hiveguard  
- Bee damage may not ride generic “elemental damage” amp (unique type)  
- Info/recon power is non-DPS board space  

### Failure modes to avoid in tuning

| Failure | Mitigation |
|---------|------------|
| Permanent sustain god | Pure HoT, short D, refresh cap, no overhealth |
| Swarmkeeper suicide | ICD self sting, Hiveguard, Soft Landing, low self damage |
| Hive Master = best Friend turret | No gun copy; bee ticks only; duration caps; no player weapon stat leach |
| Nebula = free infinite flight | Gravity multiplier floor, zone exit clears buff quickly |
| Neurotoxin infinite loop | Bonus hit applies 0 bee amount or non-recursive flag |
| 30 upgrades but 3 builds only | Keep standards universally good; avoid exclusive path locks |

---

## 13. Implementation Appendix (For Later — Not This Pass)

Design-only milestone: **this document**. When coding starts, prefer:

| Piece | Approach |
|-------|----------|
| Registration | Existing HoneyJar `GrenadeRegistration` clone path; set `GunData.damageEffect = Bees`, tune damage/effect amount |
| Name / IDs | Display **Honey Jar**; `APIName` e.g. `honey_jar`; gear id **92200**; upgrades **92201–92230** |
| Data host | `HoneyJarBehaviour` (rename from example) with `Data` struct for all flags/scalars |
| Detonate | Harmony on `GrenadeBullet.Detonate` (FriendinaBox-style prefix/postfix) |
| Field entities | `BeeCloud`, `BeeHive`, `BeeCloak`, `NectarNebulaZone` + lightweight tracker |
| Stick | Honey Bomb custom OnHit stick or bullet flag |
| Antigrav | Pool/spawn `AntigravityField` or custom `IGravityModifier` |
| Reveal | Recon Drones spike against dummy/part highlight APIs during impl |
| Nectar HoT | Player heal-over-time helper (stack display); no overhealth APIs |
| Upgrades | `PlayerData.CreateUpgrade` + `UpgradeProperty` Apply/Remove restoring prefab snapshot |
| Mod flags | `[MycoMod(..., ModFlags.IsSandbox)]` — changes gameplay rules |
| Cross-mod | None in v1 |

### Suggested `HoneyJarBehaviour.Data` fields (sketch)

```
// Baseline / scales
float explosionRadiusMultiplier;
float beeEffectAmountMultiplier;
float boomDamageMultiplier;
float selfBeeMultiplier;
float cloudDuration;
float cloudRadiusMultiplier;
float cloudTickInterval;
float cloudTickDamage;
float cloudTickBeeAmount;

// Kick the Nest / density
float cloudDurationBonus;
float cloudDamageMultiplier;

// Hive Master
bool hiveMaster;
float hiveDuration;
float hiveSeekRadius;
float hivePulseInterval;
float hivePulseDamage;
float hivePulseBeeAmount;
float attackDroneSeekBonus;      // Attack Drones
float attackDroneIntervalMult;

// Swarmkeeper
bool swarmkeeper;
float cloakRetaliateIcd;
float cloakRetaliateDamage;
float cloakRetaliateBee;
float cloakSelfStingInterval;
float cloakSelfStingBee;
float cloakSelfStingDamage;

// Nectar Nebula
bool nectarNebula;
float nebulaDuration;
float nebulaRadius;
float nebulaGravityMult;
float nebulaMoveSpeedAdd;
float nebulaAllyHoT;
float nebulaEnemyTickDamage;
float nebulaEnemyTickBee;

// Honey Bomb
bool sticky;

// Neurotoxin
bool neurotoxin;
int neurotoxinStacksPerProc;     // 10
float neurotoxinBonusDamage;

// Pheromone
bool pheromoneBurst;
float pheromoneDuration;
float pheromoneAmpMult;
float pheromoneRadius;

// Recon
bool reconDrones;
float reconInterval;
float reconRange;

// Nectar (Sugar Coated / Symbiotic)
float sugarCoatedHoT;
float sugarCoatedDuration;
float symbioticHoT;
float symbioticDuration;
float symbioticMoveSpeed;
float symbioticMoveDuration;

// Queen’s Arsenal / Stolen Knights / Brood Link / etc.
float beesOutgoingDamageMult;
float rechargeWhileSelfBeesMult;
float chargeOnMinionKill;
// ... additional rare/epic scalars
```

### Ship cut vs stretch

**v1 must-ship (fantasy complete):**

- Baseline boom + weak cloud + Bees element  
- All 3 exotics  
- Kick the Nest, Neurotoxin, Honey Bomb, Pheromone Burst, Recon Drones  
- Sugar Coated, Stolen Knights, Queen’s Arsenal, Symbiotic Nectar, Attack Drones  
- Full standard spine  
- Pure HoT nectar model  

**Stretch / post-v1:**

- Cell Divider split clouds  
- Mitosis seeking bolts polish  
- Pollen Trail VFX  
- Alarm Pheromones juiciness  
- Recon highlight fidelity  
- Audio/Wwise / custom mesh AssetBundle  

---

## 14. Naming & Presentation

| Slot | Value |
|------|--------|
| Display name | **Honey Jar** |
| Internal / API | `honey_jar` |
| Design nickname | Beenade (notes only) |
| Short description | *Bee-element grenade. Bursts into a weak swarm cloud. Upgrades grow hives, cloaks, and nectar nebulae — temporary regen, lasting minions.* |
| Thunderstore name (later) | `HoneyJar` |
| GUID (later) | `sparroh.honeyjar` |

---

## 15. Open Questions (Balance / Feel — Not Blocking Doc)

1. Honey Bomb: stick **enemies only** vs surfaces too?  
2. Neurotoxin: count applies from **all** your bee sources or jar-only? (Recommend **all sources you own**.)  
3. Soft Landing vs Swarmkeeper tax: should Soft Landing reduce cloak self-sting? (Recommend **yes, partially**.)  
4. Baseline cloud: visible enough in loud fights? (VFX priority.)  
5. Exact hex shapes for 30 upgrades — author during implementation pass.  
6. Default max concurrent clouds/hives if Double Brood + long duration? (Cap 2–3 field zones.)  

---

## 16. Design Checklist

- [x] Niche: Minions  
- [x] Healing: Temporary regeneration (**pure HoT** / Heal over Time)  
- [x] Baseline: boom + weak cloud  
- [x] Shared family baseline: dmg **100** · effect **10** · charges **3** · recharge **45** · `hitForce` **6**  

- [x] Name: Honey Jar  
- [x] ~30 upgrades  
- [x] Wells interactive (mix/match), not exclusive paths  
- [x] Self-contained (no cross-mod hooks v1)  
- [x] Three exotics from beenade.txt  
- [x] Epics/rares from beenade.txt preserved and expanded  
- [x] Bee element grounded in `EffectType.Bees` / `BeeStatusEffect`  
- [x] Implementation deferred  

---

## 17. Changelog (Design Doc)

| Date | Change |
|------|--------|
| 2026-08-15 | **Shared throwable baseline lock:** damage **100**, `damageEffectAmount` **10**, max charges **3**, recharge **45**, explosion radius (`hitForce`) **6**. Sustain column confirmed **Heal over Time** (pure HoT nectar). |
| 2026-05-08 | Initial design doc from `beenade.txt`, wiki bee/Swarm/Accelerator research, decompile anchors, FriendinaBox structural lessons. User locks: boom+cloud, Honey Jar name, design-only, self-contained, pure HoT, ~30 upgrades, gravity-well mix paths. |

---

*End of design document. Next step when ready: rename template identifiers and implement baseline boom + cloud only, then layer upgrades by well.*
