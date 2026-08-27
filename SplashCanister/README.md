# Splash Canister

Water-element path throwable for **Mycopunk**. Clones **Photon Disc** motion (tumble + surface wave), strips Disc kit culture, and paints a **lingering wet wall** along the path so other elements pay out.

> Phase 0–1: Disc clone + path-wall baseline. Upgrades come later.

Vanilla **Photon Disc** is left unmodified — this is a separate gear entry.

## Baseline (no upgrades)

| Stat | Value |
|------|--------|
| Delivery | Photon Disc tumble → long surface wave |
| Element | `EffectType.Water` |
| Step damage | **5** (tiny path slap) |
| Wall first hit | **100** damage + **10** Water (once per target per throw) |
| Wall wet retick | **10** Water (full-sat class), no damage |

| Wave length | **15** |
| Wave speed | **14** |
| Wall linger | **~3 s** ribbon along path |
| Wall size | height **1.75**, thickness **1.8**, segment **3.2** |

| Max charges | **3** |
| Recharge | **45 s** |
| Attunement / ammo-toss / bounce-chain | **None** (stripped) |

```
Throw → disc flight → surface wave ride
  → each wave step: spawn wall segment
  → first contact with any segment: full 100 dmg + wet once
  → further segments / ticks: wet only (no extra damage)
```


## Architecture

| Piece | Role |
|-------|------|
| `SplashCanisterBalance` | Path + wall + gun baseline numbers |
| `GrenadeRegistration` | Clone PhotonDisc → AllGear + GearData; strip Disc gimmicks |
| `SpawnGearHooks` | NGO remap to PhotonDisc prefab + stamp identity |
| `SplashCanisterBehaviour` | Custom data host |
| `SplashCanisterDetonateHook` | Postfix `PhotonDiscBullet.Detonate` → wall segment |
| `WaterWallSegment` | Local linger wet ribbon |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/SplashCanister.dll`

## Install

```
BepInEx/plugins/SplashCanister.dll
```

## In-game checklist

1. Log shows registered `splash_canister` id `94200` with Water baseline  
2. Gear select lists **Splash Canister** (auto-unlocked)  
3. Equip throwable → throw uses **Disc motion** (not grenade arc)  
4. On surface: long path ride, **blue wall ribbon** left behind  
5. Enemies crossing the wall pick up **Water**  
6. No attunement UI / ammo-toss off guns / Disc bounce-chain  
7. Vanilla Photon Disc still present and unmodified  
8. Save/reload keeps unlock + equipped throwable  

## Design

See `SplashCanister-DesignDoc.md` for full kit fantasy (exotics / reactions later).  
Baseline identity is now **path-wall primer**, not boom+slick.

## Authors

- Sparroh

## License

MIT — see `LICENSE`
