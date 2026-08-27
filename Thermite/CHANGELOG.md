# Changelog

## 0.6.0

- Phase 6 remaining epics/rares (**Give and Take** and **Hot Boxing** cut):
  - Cauterize Jacket (Rare) — ignited → next Welding pulse amplified
  - Maniac Maneuver (Epic) — WildfireBurn flag / self-damage Wildfire path
  - Ember Relay (Epic) — detonate while ignited refunds charge (no death)
  - Violent Reaction (Epic) — corroded hit → next boom radius (`corrosionRadius`)
  - Impact Cascade (Epic) — melee ignited → grenade charge (`punchCharge`)
  - Afterburn Fuse (Epic) — longer fuse + boom Fire/radius
- 28 upgrades registered

## 0.5.0


- Phase 5 Scorched Earth (Exotic 92630) + Funeral Mote (Rare 92619):
  - Primary detonate plants lingering slag field (enemy Fire ticks; no ally HoT)
  - Cluster children do not multi-field; IC nova can plant at feet
  - Mobile Hearth merge: scorched counts as heat-home for move-gated recharge (single pipeline)
  - Funeral Mote: allies who **move into** a field get one small instant heal per field
  - Large boom-scale radius (same family as enlarged Hearth), max 2 concurrent fields
- ThermiteScorchedSystem + runner; white-hot slag disc visual

## 0.4.0


- Phase 4 Mobile Hearth (Exotic 92628):
  - Detonation plants a heat ember at impact (primary only; cluster children do not spam)
  - Recharge bonus only while **moving** inside your ember (stationary ≈ 0)
  - IC nova re-plants ember at your feet when both exotics are owned
  - Deep Charge / Warm Front scale duration / radius / move-recharge
  - Lightweight cylinder placeholder visual
- ThermiteHearthSystem + runner; cluster Kill path plants when primary splits

## 0.3.0


- Phase 2 heal + self-fire rares:
  - Welding Heat (instant boom heal via vanilla explosionHealing path)
  - Restoration Protocol (pure `Player.Heal` on throw — never overhealth)
  - Napalm, Heat Sink, Volatile Explosives
  - Ember Stride (detonate movespeed), Warm Front (heat radius / move-recharge prep)
- Phase 3 exotics + path rare:
  - Internal Combustion (combust stacks → nova + instant self heal)
  - Cluster Bomb (child bomblets)
  - Slag Splitter (+children while Cluster equipped)
- ThermiteHooks: throw heal, detonate Ember Stride, ApplyUpgrades → Sync + OnUpgradesEnabled rebind
- 19 upgrades registered (92601–92618, 92627, 92629)

## 0.2.0

- Phase 1 standard spine (9 stackable upgrades, ids 92601–92609):
  - Wide Bore, White Phosphor, Heated Charge, Hard Charge, Fire Gel
  - Deep Charge, Two's Company (+1 charge / mild +CD on card only), Throw Weight, Quick Tongs
- No cooldown-tax culture on radius/damage/utility cards (Two's Company is the explicit mild +CD exception)
- Debug `GrantAllUpgrades` tops inventory to 1 unlocked instance per registered upgrade

## 0.1.0

- Initial Thermite scaffold (Phase 0)
- Separate gear from vanilla Incendiary Grenade (`thermite`, id 92600)
- Runtime clone of vanilla **IncendiaryGrenade** (Fire element + IncendiaryGrenadeBullet path)
- Bland baseline boom: ~10 damage, Fire effect amount **10** (full-sat dump ballpark)
- Vanilla Incendiary gimmicks zeroed on Thermite instances (no free heal/IC/cluster/hearth/Gambler)
- Caustic Flask-grade equip remap/stamp + PlayerData.OnAwake registration timing + grenadeID persistence
- Design doc: `Thermite-DesignDoc.md`
