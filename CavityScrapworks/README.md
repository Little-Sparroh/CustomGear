# Cavity Scrapworks

SAXON CS-8 **Cavity Scrapworks** — a parallel primary weapon for **Mycopunk**. Magnetic plateworks that stick, recall, and catch. Vanilla FR.15833 Plate Launcher is left unmodified.

**Phase 0 / 1 (this build):** registration + honest empty-grid baseline. No upgrade pool yet.

## What ships

| Piece | Detail |
|--------|--------|
| Gear | `cavity_scrapworks` / id `91400` |
| Clone base | Vanilla **PlateLauncher** (plate bullet, recall, catch DNA) |
| Balance | `CsBalance.cs` (AMR-style single sheet) |
| Flags | `[MycoMod(IsSandbox)]` |
| Upgrades | None (Phase 2+) |

### Baseline loop

```
M1 → fire plate → stick in target/terrain
    → recall (vanilla plate trigger) → plate returns
    → catch → cavity ready
```

Mag **1**, Normal element, ~150 RPM spirit, high reserve. No fence / blades / Vector / High Ground until upgrades.

## Architecture

Runtime-cloned `NetworkBehaviour` gear cannot change concrete C# type without a Unity prefab. This mod:

1. **Clones** `PlateLauncher` from `Global.AllGear` as a catalog entry
2. Assigns a **new `GearInfo`** with an empty upgrade pool
3. Applies **`CsBalance`** onto catalog `GunData`
4. Attaches **`CavityScrapworksBehaviour`** for future upgrade data
5. On equip: NGO spawns **vanilla PlateLauncher**, then stamps catalog identity

`PlateBullet.OnInitialized` casts `source` to `PlateLauncher` — the live gun **must** remain a real `PlateLauncher` instance (spawn remap guarantees this).

### Registration flow

```
Plugin.Awake
  ├─ Harmony: Global.LoadInstance → TryRegisterGear
  ├─ Harmony: PlayerData.OnAwake prefix/postfix (persistence)
  ├─ Harmony: SpawnGear_Server + GearSelectionWindow
  └─ TryRegisterGear

TryRegisterGear
  ├─ Find PlateLauncher
  ├─ Instantiate disabled catalog clone (strip NetworkObject)
  ├─ GearInfo + TextBlocks + empty upgrades
  ├─ CavityScrapworksBehaviour
  ├─ ApplyPlateStats (CsBalance)
  └─ Inject AllGear + EnsureGearData
```

## Project layout

| File | Purpose |
|------|---------|
| `Plugin.cs` | BepInEx entry, timing, persistence |
| `CsBalance.cs` | Base GunData / aim dials |
| `CavityScrapworksBehaviour.cs` | Custom data host (thin in Phase 1) |
| `WeaponRegistration.cs` | Clone / GearInfo / AllGear / ApplyPlateStats |
| `SpawnGearHooks.cs` | Equip remap + GearSelectionWindow safety |
| `UpgradeRegistration.cs` | CreateUpgrade helpers (unused until Phase 2) |
| `CavityScrapworks-DesignDoc.md` | Full design bible |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/CavityScrapworks.dll`

Install to:

```
<Mycopunk>/BepInEx/plugins/CavityScrapworks.dll
```

or your r2modman profile plugins folder.

## In-game checklist

1. BepInEx log: registered `cavity_scrapworks` id `91400`
2. Gear select lists **Cavity Scrapworks** (auto-unlocked)
3. Equip primary → Plate Launcher visuals / paddles
4. Fire → stick → recall → catch
5. Mag behaves as 1 cavity plate
6. Vanilla Plate Launcher still present and unchanged
7. No vanilla plate upgrade cards on Scrapworks
8. Quit / relaunch → unlock + equip id kept (`Persistence OK`)

## Tuning

Edit **`CsBalance.cs` only** for empty-grid feel. Do not scatter magic numbers in registration.

## Roadmap (later phases)

- ~30 upgrades (Salvo / Lattice / Interdictor + universal Clearing Plasma)
- Vector bounce-block removal on Scrapworks only
- Fence, High Ground, Improvised (no grenade tax), Painter, Nice Catch rewrites

See `CavityScrapworks-DesignDoc.md`.

## Authors

- Sparroh

## License

MIT — see `LICENSE`
