# Changelog

## 0.5.0

- Phase 5 Exotics (6): Mycelial Tap, Mutual Covenant, Hemophage Protocol,
  Spore Lattice, Bond Molt, Graft Aura
- Full frozen catalog 30/30 (Standards + Rares + Epics + Exotics + Oddity)
- HUD: COVENANT / WEAK PULSE / MOLT READY / AURA / HEMOPHAGE

## 0.4.1

- Fix auto-grant: per upgrade type, grant when live Instances.Count < 1
- Stop using HasEverUnlocked / TotalInstancesCollected (lifetime counters blocked re-grant)


## 0.4.0

- Phase 4 Epics (8): Exsanguinate, Photosynth Carapace, Bloodprice Rounds, Jumping Leech,
  Idle Culture, Open Vein, Shared Pulse, Transfusion Invert
- Kill hook, Idle Culture V regen, Bloodprice on Fire commit, Open Vein self-wound convert
- Catalog 24 upgrades (Phases 2–4)


## 0.3.1

- Fix: unequip no longer copies full GunData from prefab in Remove (broke FireBullet)
- Remove restores HelminthBehaviour only; vanilla resets GunData; we re-guard ammo flags


## 0.3.0

- Phase 3 path Rares (8): Arterial Hitch, Anemic Mark, Well-Fed Protocols, Soft Mouth,
  Frenzy Feed, Critical Host, Siphon Cadence, Scar Tissue
- Combat: OnBeforeDamage amps, Feed Overdraw, Critical Host arm/heal, Siphon cadence V
- HUD: CRITICAL / OVERDRAW / MARK / SCAR state pips
- Catalog now 16 upgrades (Phase 2 glue + Phase 3 Rares)


## 0.2.6

- Hardened Stock verified (fire path healthy with recoil/spread scale)
- FireDbg off by default (`HelminthFireDebug.Enabled`); set true to re-enable


## 0.2.5

- Debug: [Helminth][FireDbg] logs on Fire / FireBullet / HardenedStock.Apply (temporary)


## 0.2.4

- Fix: auto-grant checks HasEverUnlocked / TotalInstancesCollected before CollectInstance (no duplicate stacks)
- Restore Hardened Stock identity: scales recoil ranges + spread on live Gun; Remove restores prefab + guards



## 0.2.2

- Fix: Hardened Stock no longer mutates GunData in Apply (was breaking FireBullet)
- Handling mults live on HelminthBehaviour and stamp from prefab baseline after upgrades


## 0.2.1

- Fix: Vitality spent only after a successful Fire (no V drain on failed shots)
- Fix: Pulse Metering / Hardened Stock clamp fireInterval + spread; re-guard GunData after Apply/Remove
- Auto-grant all Phase 2 upgrades (CollectInstance + quiet Unlock) with the weapon


## 0.2.0

- Phase 2: Standards + glue upgrade pool (8 cards)
  - Vital Efficiency, Secondary Mouth, Hardened Stock, Leech Efficiency, Crimson Efficiency
  - Longer Tendrils, Pulse Metering, Boundary Incursion (GridGrow Oddity)
- Upgrade registration via PlayerData.CreateUpgrade (ids 92023–92030)


## 0.1.4

- Innate leech ticks are Normal + DoT only (no Acid / element saturation)


## 0.1.3

- Fix leech: resolve Gun from bullet ParentSource chain (hits were skipped)
- Economy: 3 V/shot, faster Feed (40 V/s) + drip (4 V/s), ~6.5 HP/10 V
- Log once when leech first applies (BepInEx log)


## 0.1.2

- Fix: Feed/drip Host tax no longer applies leech/Acid to the player
- Leech only targets non-players; skip Host while spending HP
- Feed cost tuned to ~8 HP per 10 V (middle ground)


## 0.1.1

- Feel pass: Feed costs more noticeable; quieter passive drip
- Leech: higher DPS, longer duration, Acid+DoT ticks, min damage floor
- Combat hooks subscribe on Update if AfterUpgradesEnabled was skipped
- HUD shows Feed HP/s, LEECH count, Bond, and flash on leech apply


## 0.1.0

- Phase 0+1: Helminth Receiver identity and sacred baseline loop

- Runtime clone of CartridgeSMG as `helminth_receiver` (gear id 92000)
- Vitality buffer economy (fire spends V; empty = soft lock)
- Hold-reload Feed channel (Host HP → V, safety floor 18%)
- Tiny passive drip above safety floor
- Innate light leech tick + Bond stacks on hit
- Minimal Vitality / state HUD via SparrohUILib
- Sandbox MycoMod flag; soft dependency on SparrohUILib
- No upgrade catalog yet (Phase 2+)
