# Rapture's Chosen

Dual-mode **shock lance** primary for **Mycopunk**.

Charge **M1** for a piercing shock coil. Hold **RMB** to charge and release **Auger** (joust/drill). Shock on both modes. ADS is off.

**Sandbox mod** — all clients need the same plugin in multiplayer. Vanilla Shocklance is left untouched.

## Phase status

| Phase | Status |
|-------|--------|
| 0 Registration | Done — clones Shocklance, unique gear id/API name |
| 1 Base mechanics | Done — coil charge, RMB Auger, Shock element, ADS off |
| Upgrades (Scatter / Rail / Dynamo) | Not yet |

## Controls (baseline)

| Input | Role |
|-------|------|
| Hold M1 | Charge coil |
| Release M1 | Fire piercing shock coil (full charge required) |
| Hold RMB | Charge Auger |
| Release RMB | Launch Auger drill |
| R | Reload |
| ADS | Disabled |

## Balance

All base numbers live in `RcBalance.cs` (same style as Anti-Material Rifle’s `AmrBalance`).

Draft anchors (playtest):

- Damage 40, Shock 5.5
- Mag 8 / reserve 32, reload ~1.9s
- Charge ~0.35s (full required; no Half Cocked curve)
- Falloff 16 / 22 / max 26
- Modest single-stack Auger on RMB

## Architecture

| File | Role |
|------|------|
| `Plugin.cs` | BepInEx entry, registration timing, persistence |
| `RcBalance.cs` | Base GunData + ShocklanceData / Auger constants |
| `WeaponRegistration.cs` | Clone Shocklance, GearInfo, ApplyRapturesChosenStats |
| `RapturesChosenBehaviour.cs` | Dual-mode host + baseline identity |
| `RapturesChosenCombatHooks.cs` | FireInterval, RMB Auger remap, Auger element |
| `SpawnGearHooks.cs` | NGO equip remap + stamp |
| `UpgradeRegistration.cs` | CreateUpgrade helpers (unused until upgrades) |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/RapturesChosen.dll`

## Install

```
BepInEx/plugins/RapturesChosen.dll
```

## In-game checklist

1. Log: registered gear `raptures_chosen` id 87700  
2. Gear select lists **Rapture's Chosen** (vanilla Shocklance still present)  
3. Equip → hold M1 charges, release fires **spiral shock coil**  
4. Hold RMB charges Auger; release launches drill (R reloads)  
5. No ADS  
6. Quit/relaunch → `Persistence OK`  

## Identity

- GUID: `sparroh.raptureschosen`
- API name: `raptures_chosen`
- Gear id: `87700`
- Base clone: `Shocklance`

## License

MIT — see `LICENSE`
