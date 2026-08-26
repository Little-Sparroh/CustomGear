# Heaven Piercer

SAXON industrial **compound bow** primary for **Mycopunk**.

Hold to draw. Release to loose. Charge scales damage, arrow speed, effective range, and **gravity** (longer draw = flatter flight). A baseline **sweet-spot** band near full draw rewards timing with a critical loose.

**Sandbox mod** — all clients need the same plugin in multiplayer.

## Phase status

| Phase | Status |
|-------|--------|
| 0 Registration | Done — clones Shocklance, unique gear id/API name |
| 1 Base mechanics | Done — draw/loose, charge curves, sweet spot, projectile |
| Upgrades / Bleed / Rain / Latch | Not yet |

## Controls (baseline)

| Input | Role |
|-------|------|
| Hold M1 | Draw (charge) |
| Release M1 | Loose arrow at current charge |
| R | Reload |
| RMB | Unbound (reserved for path overrides later) |

## Balance

All base numbers live in `HpBalance.cs` (same style as Anti-Material Rifle’s `AmrBalance`).

Draft anchors (playtest):

- Mag 8 / reserve 40
- Full draw ~0.65s
- Sweet spot 0.82–0.95 ×1.32 crit
- Pluck ~0.35× damage, high gravity lob
- Full draw listed damage, fast flat arrow

## Architecture

| File | Role |
|------|------|
| `Plugin.cs` | BepInEx entry, registration timing, persistence |
| `HpBalance.cs` | Base GunData + charge scaler constants |
| `WeaponRegistration.cs` | Clone Shocklance, GearInfo, projectile swap |
| `HeavenPiercerBehaviour.cs` | Draw/loose runtime + sweet spot |
| `HeavenPiercerCombatHooks.cs` | FireBullet projectile bypass, FireInterval, charge capture |
| `SpawnGearHooks.cs` | NGO equip remap + stamp |
| `UpgradeRegistration.cs` | CreateUpgrade helpers (unused until upgrades) |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/HeavenPiercer.dll`

## Install

```
BepInEx/plugins/HeavenPiercer.dll
```

## In-game checklist

1. Log: registered gear `heaven_piercer` id 87600  
2. Gear select lists **Heaven Piercer**  
3. Equip → hold M1 charges, release fires a **projectile** (not Shocklance spiral)  
4. Early release = slow lob; full draw = faster / flatter / harder  
5. Sweet band hits harder than overhold  
6. Quit/relaunch → `Persistence OK`  

## Identity

- GUID: `sparroh.heavenpiercer`
- API name: `heaven_piercer`
- Gear id: `87600`
- Base clone: `Shocklance`

## License

MIT — see `LICENSE`
