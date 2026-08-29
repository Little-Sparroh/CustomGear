# Needle Carbine

SAXON N-series **Needle Carbine** for Mycopunk — a mid-range medical-industrial primary.

## Phase 0 + 1 (this release)

| Feature | Status |
|---|---|
| Gear registration (`needle_carbine`, id `87530`) | Yes — clones `ScoutLaserRifle` |
| Balance sheet (`NcBalance`) | Yes |
| Laser / ADS disabled (RMB free) | Yes |
| Save persistence | Yes |
| Needle stacks + grace (7 / 3s) | Yes |
| Supercombine detonation + poison dump | Yes |
| True Poison EffectType (11) + DoT | Yes |
| Baseline Extract (aim press → heal sip) | Yes |
| Upgrade pool | Not yet |

## Fantasy

M1 needles stick toxin darts, build Needler-style supercombine, and saturate **Poison**.  
RMB **Extract** sips sustain from a poisoned / needled target.  
R stays reload.

## Install

```
BepInEx/plugins/NeedleCarbine.dll
```

Requires BepInEx Pack Mycopunk. Marked **IsSandbox** (gameplay rules).

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/NeedleCarbine.dll`

## In-game checklist

1. Log shows gear registration + poison inject  
2. Gear select lists **Needle Carbine** (auto-unlocked)  
3. Equip primary → full-auto fire (Scout model)  
4. Laser mode never engages; aim does not ADS  
5. ~7 focused hits → supercombine bang + poison dump  
6. Aim-press Extract on poisoned target heals a sip  
7. Quit / relaunch keeps unlock + loadout  

## Authors

- Sparroh

## License

MIT — see `LICENSE`
