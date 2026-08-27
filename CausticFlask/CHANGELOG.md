# Changelog

## 0.6.0

- Phase 6 remaining kit (~30 board complete):
  - Deteriorate (Epic) — dual Acid+Rot on metal
  - Overclock (Epic) — OC charge refund, no CD tax
  - Exothermic Reaction (Epic) — Shock sat → Fire
  - Heavy Support (Exotic) — cargo drop, boom damage cost, 12s ICD
  - Heavy Payload (Rare) — requires Heavy Support
  - Odd Cocktail (Epic) — random non-Acid second boom
  - Greased Joints (Rare) — movement ability charge on boom
- FlaskPhase6Hooks for dual-status + heavy ICD

## 0.5.0


- Phase 5 Carapace (timed armor DR):
  - Polymer Plating (Rare), Saxonite Carapace (Exotic), Plate Polish, Puddle Harden, Defensive Spurt
  - ArmorPlatingBuff: OnBeforeTakeDamage multiply, hard cap 45%, time-only
  - Solvent Cure + Corrosion Pulse rules with any armor source
  - FlaskArmorDetonateHook applies team plating on boom

## 0.4.0


- Phase 4 Vacuum Lab:
  - Vacuum Tube (Epic) — fuse-active pull via vanilla AcidGrenadeBullet path
  - Event Horizon (Exotic) — longer arm + collapse payoff preferring corroded
  - Clump Tax (Rare) — bonus detonate damage; full if corroded (requires vacuum)
- FlaskVacuumDetonateHook on AcidGrenadeBullet.Detonate for collapse/clump packets
- **Fix Vacuum Tube:** pull requires fuse > 0 (`GunData.reloadDuration`); Tube now sets fuse ~1.1–1.6s; OnFired re-syncs pull Data + forces fuse on bullet


## 0.3.0


- Phase 3 Solvent Field upgrades:
  - Gas Puddle (Epic), Catalytic Reservoir (Exotic), Catalytic Seal, Gas Valves, Universal Solvent, Solvent Siphon
- Sync Flask Data → live AcidGrenade.Data after ApplyUpgrades so vanilla puddle/valves bullet paths work
- FlaskPlayerHooks for Solvent Siphon kill refunds + valves rebind

## 0.2.1


- Ported AntiMaterialRifle debug upgrade grant: `Debug.GrantAllUpgrades` (default true) tops inventory to 1 unlocked instance per Flask upgrade (idempotent; does not auto-equip hex)
- Ported AMR save/equip persistence: `EnsureGearData`, OnAwake rebind, `PersistEquippedCatalogId` writes `grenadeID=92400` after stamp so Flask stays equipped across restarts

## 0.2.0


- Phase 2: standard spine upgrades (9, all stackable)
  - Wide Mouth, Strong Solvent, Quick Cap, Hard Flask, Base Lining
  - Deep Vat, Twin Flask (+1 charge / mild +CD on card only), Viscous Mix, Throw Weight
- Upgrade ids 92401–92409 registered on `caustic_flask`
- No cooldown-tax culture on radius/damage/utility cards (Twin Flask is the explicit mild +CD exception)

## 0.1.0

- Initial Caustic Flask scaffold (Phase 0+1)
- Separate gear from vanilla Acid Grenade (`caustic_flask`, id 92400)
- Runtime clone of vanilla **AcidGrenade** (Acid element + AcidGrenadeBullet path)
- Bland baseline boom: ~10 damage, Acid effect amount **6** (partial sat, not full)
- Vanilla Acid gimmicks zeroed on Flask instances (no free puddle/pull/armor/OC/heavy)
- FriendinaBox-grade equip remap/stamp + PlayerData.OnAwake registration timing
- Design doc: `CausticFlask-DesignDoc.md`
