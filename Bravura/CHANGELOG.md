# Changelog

## 0.1.0

- Phase 0: Register Bravura as parallel primary (clone Lead Flinger / FastReloadShotgun)
- Phase 0: BravuraBalance gunfeel sheet + ApplyBravuraStats; strip vanilla kill→reload
- Phase 0: Persistence + SpawnGear remap/stamp (GearId 91300, api `bravura`)
- Phase 1: Style Rank always-on (D–S), memory queue, decay, hit punish
- Phase 1: **Verse/Chorus** via vanilla ChargeData (`fireOnRelease` + `canFireWhileCharging`); short release = Verse, hold ≥0.4s = Chorus

- Phase 1: **Steel** (RMB) — sword melee swing; 1.25× pistol dmg first unique hit, 1× on repeat chain
- Phase 1: **Flourish** fixed-band reload QTE bar (anim-normalized; Fire in window)
- Phase 1: Entrance (slide/air/equip), Finale at A-rank Chorus
- Phase 1: `hitForce = 0` (explosion radius field; no baseline blast)
- Phase 1: Center crosshair HUD — south chevron + rank letter + last 5 verbs (no bottom-left OnGUI)

- Phase 1: Hit punish via Player.OnAfterTakeDamage (not nonexistent TakeDamage)
- Spotlight / Tag mark deferred to upgrades

- No upgrade pool yet (empty-grid baseline)
