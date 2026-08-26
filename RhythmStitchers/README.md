# Rhythm Stitchers

Mycopunk custom primary — dual machine pistols with independent L/R triggers and magazines, locked to a shared Tempo.

**Phase 0/1 (v0.1.0):** registration + baseline combat. No upgrades yet.

## Fantasy

Two needles. One beat. LMB fires the left stitcher, RMB the right. High-rate semi per channel, independent mags, no ADS. Land stitches near the master Tempo for a light on-beat damage crumb.

## Identity

| Field | Value |
|---|---|
| GUID | `sparroh.rhythmstitchers` |
| API name | `rhythm_stitchers` |
| Gear ID | `93000` |
| Base clone | `AcceleratorGun` (rail visuals) |
| MycoMod | `IsSandbox` |

## Baseline stats (tune in `RhythmStitchersBalance.cs`)

- Damage 17 / stitch, semi, ~630 RPM mash ceiling per channel
- Mag 14|14, shared reserve 168, reload 1.15s
- Falloff 21→42, soft mid identity
- BPM 120, on-beat window ±55ms, crumb +8%
- No Cross Sweep / Desync / Sonic element yet

## Architecture

| File | Role |
|---|---|
| `Plugin.cs` | BepInEx entry, registration timing |
| `WeaponRegistration.cs` | Clone AcceleratorGun, GearInfo, ApplyStats + neuter |
| `RhythmStitchersBalance.cs` | AMR-style balance sheet |
| `RhythmStitchersBehaviour.cs` | L/R mags, fire clocks, Tempo |
| `RhythmStitchersCombatHooks.cs` | Dual fire, reload, HUD, on-beat crumb |
| `SpawnGearHooks.cs` | Equip remap + stamp |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/RhythmStitchers.dll`

## In-game smoke checklist

1. Log shows registered `rhythm_stitchers` / id 93000, base AcceleratorGun  
2. Gear select lists **Rhythm Stitchers** (auto-unlocked)  
3. Equip → no ADS on RMB; LMB and RMB both shoot  
4. HUD shows `14|14` style; one side can empty while the other fires  
5. Reload tops both from shared reserve  
6. No Accelerator burst ramp / bees / warp  
7. Quit/relaunch with equipped → Persistence OK  

## License

MIT — see `LICENSE`
