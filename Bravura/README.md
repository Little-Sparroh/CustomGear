# Bravura

SAXON BR-88 exhibition carbine for **Mycopunk**. A parallel primary that grades how you fight — **pistol + sword**.

**Phase 0/1 (v0.1.0):** registration + empty-grid baseline. No upgrade grid yet.

## Fantasy

Style Rank rises with **variety + aggression** and falls when you repeat yourself or disengage. Five verbs are always available:

| Input | Verb | Role |
|-------|------|------|
| M1 press→release (short) | **Verse** | Light pistol shot (fires on release) |
| M1 hold ≥0.4s → release | **Chorus** | Heavier shot (Finale at A+) — one shot, not Verse+Chorus |

| RMB | **Steel** | Sword melee — 1.25× pistol dmg on first unique hit, 1× on repeats |
| R reload + Fire in window | **Flourish** | Stylish reload QTE (fixed band) |
| Fire while slide / air / equip | **Entrance** | Mobility attack |

Ranks: **D → C → B → A → S**. A arms a **Finale** Chorus. Mild damage mults; skill is the show.

**HUD:** center south-chevron crosshair, rank letter under the tip, last 5 verbs under that.

**Spotlight / Tag mark** is reserved for upgrades — not baseline.


## Clone base

Runtime clone of vanilla **Lead Flinger** (`FastReloadShotgun`) for model / NGO spawn. Vanilla LF is not modified. `hitForce` is explosion radius and is **0** on baseline.

## Identity

| Field | Value |
|-------|--------|
| GUID | `sparroh.bravura` |
| APIName | `bravura` |
| GearId | `91300` |
| MycoMod | IsSandbox |

## Build

```bash
dotnet build --configuration Release
```

Output: `bin/Release/netstandard2.1/Bravura.dll`

## Authors

- Sparroh

## License

MIT — see `LICENSE`
