# Changelog

## Unreleased

- **Baseline-only playtest pass**
  - **Enable Upgrades** config (default **off**) — no upgrade registration/grant while iterating empty-grid feel
  - **Single-chamber reload** — `refillAmmoOnReload = false`; each cycle loads +1 from reserve (AMR/tube DNA); mag stays 6; reload duration 0.55s per chamber
  - **Continuous tube reload** — auto-chains until full / dry; **fire press** with ≥1 live round cancels mid-reload (AMR interrupt)
  - **Scrap Pack → press Aim (RMB)** — instant pack on click; hold-R is normal reload only
  - **Stackable Scrap Pack** — each RMB adds a pack tier on live chambers (compound ×1.15 dmg / ×1.08 size per tier, max 3); cost = ceil(live/2) each press; HUD `Junk (P#)` shows max tier



  - **Cylinder counter** — sync Lead Flinger `barrelRotation` / `spinMag` to ammo spent (full wheel rest pose, not “starts on 1”)
  - **Junk status UI** — label is `Junk` / `Junk (P#)` with stack count only in the `xN` slot (fixes `Junk 1 x#`)
- **JunkFlingerBalance.cs** — single balance sheet for base GunData / nested prefab stats. Tune combat, ammo, projectile, range, spread, recoil, charge, fire constraints, and aim here; `WeaponRegistration.ApplyJunkFlingerStats` applies on catalog create and re-register.


## 0.5.0


- Phase 5 Glue / frozen remainder (9 upgrades, ids 93023–93031)
  - **Home Cooking** (Rare) — chance bonus % dmg; smaller self-hit chance mints Junk
  - **Volatile Munitions** (Epic) — core kill elemental explosion (% gun dmg; Fire/Shock/Acid on apply)
  - **Shrapnel Loading** (Rare) — 3 pellets + per-pellet % split EV
  - **High Caliber** (Rare) — +% damage, −RoF
  - **Lead Press** (Standard) — modest +% damage staple
  - **Cylinder Grease** (Standard) — faster reload
  - **Ride the High** (Epic) — airborne kill → brief hover
  - **Delirium** (Epic) — on-kill brief +% damage window
  - **Boundary Incursion** (Oddity) — best-effort +upgrade grid size
- Registered pool now **31** upgrades (Chamber + Junk + Rush + Glue)

## 0.4.1


- **Blood-Rush**: aim via `Player.Aim.WasPressedThisFrame()` (not `IsAiming` edge); rush damage mult baked once into chamber (no double apply)
- **Phantom Limb**: full vanilla ghost-gun clone — dissolve mesh, offset `FireBullet`, phantom SFX/flash; free pellets skip chamber/junk side effects; arms on reload start
- **Juiced Up**: wheel-level buff flag (survives chamber rebuild) so every shot on the next cylinder is supercharged, not only the first
- **Outlaw**: core detect is `kill.target is EnemyCore` (vanilla); refunds ammo + feedback
- **Snap Cylinder**: this-gun kills only while reloading; fills mag and force-speeds reload anim to completion

## 0.4.0


- Phase 4 Rush / Echo (8 upgrades, ids 93015–93022)
  - **Blood-Rush** (Exotic) — RMB/Aim press: spend Junk → load N from reserve → Rush-tagged chambers +% dmg
  - **Phantom Limb** (Exotic) — on reload, echo prior shots as phantom pellets (adjacency copy deferred)
  - **Juiced Up** (Epic) — dump wheel in window → next wheel supercharged
  - **Outlaw** (Epic) — core kill refunds ammo
  - **Snap Cylinder** (Rare) — kill during reload snap-fills mag
  - **Modded Auto** (Epic) — full-auto + slower RoF
  - **Fan Fire** (Rare) — hip-fire +% damage
  - **Fresh Cylinder** (Standard) — brief +% after reload
- Doobie + Blood-Rush allows temp mag overflow

## 0.3.0


- Phase 3 Junk path (7 upgrades, ids 93008–93014)
  - **Big Fat Doobie** (Exotic) — mag 1, huge % damage + size, slower fire, fewer reserves
  - **Residue** (Exotic) — more scrap/casings, higher soft cap, stronger Scrap Pack
  - **Scrap Hopper** (Epic) — more Junk mint + soft cap
  - **Packing Grease** (Rare) — faster/cheaper pack (ceil/3), better packed potency
  - **Refuse Rounds** (Rare) — modest % dmg + Junk-on-hit chance
  - **Lead Poisoning** (Standard) — modest % damage
  - **Bandolier** (Standard) — +ammo capacity
- Scrap Pack cost now uses configurable divisor (baseline 2; Grease → 3)

## 0.2.1


- Fix Dead Man's Hand: preserve chamber variance rolls across the whole cylinder (no longer wipe after shot 1)
- Fix Lucky Last: last chamber boosts `BulletData.force` (hitForce boom VFX) + AOE explosion at **hit** position
- Fix Hot Streak: clear stacks on full miss (any pellet hit keeps the streak)

## 0.2.0

- Phase 2 Chamber path (7 upgrades, ids 93001–93007)

  - **Lucky Bastard** (Exotic) — last chamber high ammo refund
  - **Lucky Last** (Rare) — last chamber AOE on impact (% of shot)
  - **Extra Chambers** (Standard) — +2 magazine / chambers
  - **Heavy Chamber** (Rare) — +% damage, slower RoF, click-to-fire
  - **Hot Streak** (Rare) — hit streak % damage; clears on reload
  - **Dead Man's Hand** (Exotic) — pre-roll chamber variance on reload
  - **Loaded Dice** (Epic) — raises min roll / widens band
- Debug config: Grant All Upgrades (default on while iterating)
- Hex grid unlocks via registered upgrades

## 0.1.0


- Phase 0+1 baseline ship
- New parallel primary **Junk Flinger** (`junk_flinger`, id **93000**) — vanilla Lead Flinger untouched
- Gear id uses dedicated **930xx** block (avoids 910xx DMLR / 920xx Cycler / 921xx FriendinaBox)
- FindGearSafe is API-name-first (no cross-mod ID collisions)
- Catalog clone forces `GearType.Primary` for gear-select category

- Clones `FastReloadShotgun` (Lead Flinger) model / fire setup
- Baseline **cylinder framework** with per-chamber state (6 chambers)
- Baseline **Junk** economy: casings on fire (+1), scrap on kill (+2), soft-cap 30
- Last-chamber baseline carrot (+8% damage)
- Kill→faster reload stripped on clone
- Junk shown on **player status bar** (`Junk N` / `Junk N P#`) — does **not** steal reserve ammo HUD
- Scrap Pack: hold R ≥0.3s → **immediately** pack remaining live chambers (+15% dmg)
- Pack cost = **ceil(remaining rounds / 2)** (full 6 = 3 Junk; partial mags cheaper)
- Empty mag / nothing unpackable → no spend



- `%`-only damage pipeline host ready for later upgrades
- No upgrade pool yet (frozen 30 lands in later phases)
