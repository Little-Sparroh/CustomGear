# Bravura — Design Document (v1)

> Status: **Design only** — no implementation yet.
> Working titles in notes: Style Carbine / Exhibition Gun / Form 88.
> **Ship name: Bravura.**
> Template base: weapon content template (CartridgeSMG clone path until custom art).
> Product shape: **separate primary gear** — does not replace any vanilla gun.
>
> Sibling docs (contrast locks — do not absorb):
>   - Rhythm Stitchers — dual-trigger BPM / Sonic tempo (NOT variety grading)
>   - Heat Cycler — heat resource / redline stance (NOT performance rank)
>   - Rapture's Chosen — coil ↔ Auger Dynamo stacks (two-mode only)
>   - DMLR Rework — anatomy setup → execute (NOT stylish multi-verb)
>   - Junk Flinger — Hot Streak accuracy ramp (NOT move diversity)
>   - Caduceus / Mercy Staff — Grace support meter (NOT flair)
>   - Blood Carver / Helminth — HP/blood economy (NOT Style)
>   - Boarding Trident — hip/ADS axis switch skill (posture, not repertoire)
>   - AMR — Chambered Argument climax shot (single bolt punctuation)
>   - Aussie Special — bounce / dual-trigger / hose crowns
>   - Melee kits (Fists / Blade / Impaler / Hooklash) — Style stays a **primary gun** fantasy
>
> Fantasy anchors: Devil May Cry Style meter (variety + aggression + escalating rank feedback)
> + SAXON industrial exhibition / morale-ordnance tone (not anime weeb cosplay).

---

## 0. Locked Decisions (2026-08-15)

| Decision | Lock |
|----------|------|
| Product shape | Parallel new primary — nothing replaced |
| Player-facing name | **Bravura** |
| Working APIName | `bravura` |
| Working GUID | `sparroh.bravura` |
| Slot | **Primary** |
| Paths | **Critic / Repertoire / Encore** |
| Style Rank | **Always on baseline** — not upgrade-gated |
| Core skill | **Variety + aggression** — repeating the same verb is punished |
| Baseline verbs | **Verse / Chorus / Tag / Flourish / Entrance** (five empty-grid verbs) |
| Mag / reload | **Keeps a real mag** — Flourish makes reload a Style verb (not heat-infinite) |
| Power from rank | Mild baseline; **A/S unlock Finale / Encore windows**; not pure DPS meter |
| BPM / metronome | **Out** — no master Tempo (Rhythm Stitchers owns that) |
| Anatomy transfer | **Out** — DMLR owns that |
| Heal / Grace | **Out** — Caduceus owns that |
| Blood / Vitality feed | **Out** — Carver / Helminth own that |
| Dual-wield LMB/RMB fire | **Out as identity** — RMB is Tag (and path claims), not second gun |
| Melee kit | **Out** — primary carbine/gunblade hybrid presentation |
| Soft crowns | Hybrids allowed; no hard exclusion matrix |
| ~30 upgrades v1 | Exotic shapes larger; each exotic same cell count |
| MycoMod (impl) | IsSandbox |
| Doc scope | Full bible |

---

## 1. High Concept / Fantasy

**Bravura** is SAXON's ceremonial **exhibition carbine** — a morale weapon issued to operators who make the fight look easy, and to problem employees who treat Form 88-STY ("Combat Exhibition Waiver") as a personality.

It is not the hottest gun. It is not the bloodiest. It is the gun that **grades how you fight**.

Baseline gives you a small repertoire of distinct combat verbs. Chain them without looping the same move, stay aggressive, and the **Style Rank** climbs — D through S — with escalating audio, brass, and power. Repeat yourself, turtle, or eat a clean hit, and the room (and the rank) boos.

Upgrades fork three peer fantasies:

- **Critic** — rank is the weapon; harsher taxes, faster climbs, live at S
- **Repertoire** — more verbs, longer memory, fancier combos
- **Encore** — cash the rank into climax windows and once-per-fight supers

**One-liner:** *The gun grades your fight. Repeat yourself and the room boos. Mix it up and the rank sings.*

**Product shape:** New primary (**Bravura**). Does not replace any vanilla gun or melee kit.

**SAXON marketing blurb (draft):**
  “SAXON BR-88 Bravura — Exhibition-class carbine for personnel authorized under
   Form 88-STY. Integrated critic firmware grades variety, aggression, and flair.
   Rank is not a toy. (It is a performance review with teeth.) Repeat offenders
   will notice the firmware noticing them.”

Optional stingers:
- “If the rank is still D, you are not fighting. You are filing.”
- “Same move three times is not a combo. It is a habit. Habits are for civilians.”
- “S-rank does not mean immortal. It means the paperwork will be interesting.”
- “Flourish the reload. The magazine is part of the show.”

---

## 2. Role & Fantasy in the Arsenal

| Trait | Value |
|-------|--------|
| **Slot** | Primary |
| **Range** | Close–mid (carbine; Verse is mid-capable, Chorus/Entrance lean closer) |
| **Role** | Expressive multi-verb skirmisher — variety combat, spectatable rank climbs, optional climax cash-outs |
| **Gap filled** | Collection is rich in **managed resources** (Heat, Junk, Blood, Vitality, Grace), **mode switches** (DMLR, Trident, Rapture, Caduceus), and **space ownership** (HLC, Scrapworks, Hive). Nothing owns **performance as the resource** — anti-repetition as skill, escalating flair feedback, “don’t loop the same sentence.” |
| **Synergies** | Movement (Entrance, slide/jump tech), co-op spectating (rank is readable), Tag setups for ally focus (soft — not AMR Death Mark), secondary/heavy as optional Encore partners |

**Not trying to be:**
- Rhythm Stitchers (BPM dual-pistol metronome)
- Heat Cycler (infinite heat hose)
- Rapture Dynamo (two-mode stack dialogue only)
- DMLR (anatomy setup/execute)
- Caduceus (support Grace)
- Blood Carver / Helminth (HP economy)
- A pure DPS sticker meter that rewards holding M1

### 2.1 Comparison snapshot

```
Weapon / kit              Niche                         Bravura differentiator
------------------------  ----------------------------  ------------------------------------------
Rhythm Stitchers          Dual-trigger BPM / Sonic      No master Tempo; variety > on-beat %
Heat Cycler               Heat resource / redline       Mag + multi-verb; rank ≠ heat bar
Rapture's Chosen          Coil ↔ Auger Dynamo           ≥5 baseline verbs; repeat tax
DMLR                      Anatomy build → spend         No part transfer; Tag is spotlight
Junk Flinger              Cylinder + Junk + Hot Streak  Streak is variety/aggression, not hits only
Caduceus                  Mend/OC/Judgment + Grace      No heal; rank is flair not triage
AMR                       Bolt + Chambered Argument     Climax is rank-gated Finale, not tube
Boarding Trident          Hip/ADS axis switch           Verb repertoire, not combat axis
Aussie / Twin-Hopper      Dual barrel / bounce          Not bounce shotgun; Tag is one verb
Melee kits                Assassin / hoplite / tether   Primary gun fantasy + Style HUD
```

### 2.2 Why a primary (not a melee kit)

- Style wants **spectatable mid-range gunfight** readability in co-op.
- Melee roster already splits assassin / hoplite / tether / brawler.
- Flourish-as-reload and Verse mid-range need gun mag grammar.
- Gunblade *presentation* (Chorus sweep, Entrance flash) is flavor on a primary, not a MeleeRework kit.

---

## 3. Design Pillars

1. **Style Rank is baseline identity** — always on, always readable, zero upgrades.
2. **Variety is the skill** — repeating the same verb caps or drains rank; mixing pays.
3. **Aggression is required** — downtime decays rank; passive play is a D-rank lifestyle.
4. **Five empty-grid verbs minimum** — repertoire must exist before the upgrade grid.
5. **Rank is feedback first, power second** — D→S must feel good even when damage mults are mild.
6. **A/S unlock climax tools** — Finale / Encore are earned windows, not every trigger.
7. **On-verb and state payoffs > flat % stickers.**
8. **Three peer paths (Critic / Repertoire / Encore); hybrids intended; no anti-synergy matrix.**
9. **Mag stays real** — Flourish makes reload a stylish verb; do not delete ammo anxiety with heat-infinite.
10. **RMB is Tag (and path claims)** — not ADS-required; not second full gun channel.
11. **~30 upgrades for v1**; exotic shapes larger; each exotic same cell count.
12. **SAXON exhibition tone** — brass critic, Form 88, industrial showpiece — not pure anime Style announcer (a little irony is fine).
13. **Failure states stay fun** — dropped S from a hit, repeat-tax boos, whiffed Finale, Tag into empty air.
14. **Co-op readable** — allies should see your rank climb without a wiki.

---

## 4. Core Mechanics & Gunfeel (Baseline)

### 4.1 Base gun

| Trait | Draft / intent |
|-------|----------------|
| Form | Mid-size exhibition carbine with folding bayonet/gunblade rail (Chorus melee-adjacent sweep is still a gun hit volume / short arc — not a full melee kit) |
| Fire feel | Crisp, performative; each verb has distinct audio/VFX |
| Damage | Modest per Verse shot; Chorus hits harder; power budget lives in chaining + rank windows |
| Range | Close–mid; Verse falloff honest; not a sniper |
| Mag / reserve | Medium mag (draft **18–24**); reserves normal primary spirit |
| Reload | Standard reload beat — **Flourish** can stylish-cancel (see §4.5) |
| Element | **Normal** baseline; path cards may add Shock/Fire spice (not identity spine) |
| ADS | Optional light ADS on Verse only if needed; **not required**. Prefer hip-forward showmanship |
| Model/audio | Chrome/hazard exhibition finish, brass critic chime on rank-up, boo buzzer on repeat tax |

Draft firefeel band (VALIDATE IN PLAYTEST):

| Param | Draft |
|-------|--------|
| Mag | 20 |
| Reserve | ~120–160 |
| Verse fire interval | ~0.11–0.14 s (fast semi / soft auto tap stream — **not** Cycler hose) |
| Chorus charge | ~0.35–0.5 s hold → release |
| Tag cooldown | ~0.8–1.2 s |
| Flourish window | during reload, ~0.2–0.35 s perfect band |
| Style decay delay | ~1.6–2.2 s out of combat actions |
| Repeat window | last **3–4** verbs remembered for tax |

### 4.2 Inputs (baseline)

| Input | Verb | Role |
|-------|------|------|
| **Tap / short M1** | **Verse** | Light shots — safe filler, low Style value alone |
| **Hold M1 → release** | **Chorus** | Heavier shot or short forward arc sweep — medium Style |
| **RMB / Aim press** | **Tag** | Launch a short-lived **Spotlight** projectile/marker (bounce optional once) |
| **R tap** | Reload | Normal reload |
| **R during reload (timed)** | **Flourish** | Stylish reload cancel / timed reload — Style crumb + brief handling buff |
| **Slide / jump / stow-flash fire** | **Entrance** | Movement-integrated attack (slide-shot, jump-shot, or equip flash) |
| **Heavy** | Normal heavy | No baseline heavy link |

**Both M1 held rules:** Hold past Chorus threshold → Chorus on release. Tap fire → Verse. Do not full-auto Chorus.

**ADS:** If aim is held without Tag claim from cards, mild ADS on Verse is OK. **Tag owns RMB press** as the baseline special (short press Tag; optional hold-ADS if implement needs split — prefer **press = Tag**, long-hold ADS only if playtest demands).

### 4.3 The five baseline verbs (sacred)

#### Verse (M1 tap stream)
- Fast light projectiles or hitscan-feel taps
- Low per-hit damage; builds mag pressure
- Style: **low** per shot; still counts as a verb for variety when mixed
- Repeat tax: Verse×4+ without other verbs → hard diminishing Style gain

#### Chorus (M1 hold → release)
- Single heavier payload: fat slug **or** short cone/arc sweep (pick one primary presentation at impl; **recommend: short gunblade energy arc** so it reads different from Verse)
- Costs more ammo (draft 2–3) or one heavy chamber equivalent
- Style: **medium**; higher if it finishes a Tagged target or follows Entrance

#### Tag (RMB)
- Fires a **Spotlight** marker projectile (readable gold/brass reticle on target)
- Duration draft 2.5–4 s; refreshable; max targets tagged draft 1–2 baseline
- On Tag hit: tiny damage; applies **Spotlight** state (personal — not team Death Mark fuse)
- Style: **medium** on apply; **high** when a *different* verb consumes/finishes the Spotlight
- Consume rules (baseline): Chorus or Entrance hit on Spotlight target = **Stylish Finish** (Style spike + small damage mult); Verse can nibble but weaker finish

**Tag is NOT:**
- AMR Death Mark (no delayed team bomb default)
- DMLR Mark/Expose anatomy
- Caduceus Condemned damage-taken stack spine
- Friend Painted Targets deployable mark

#### Flourish (timed reload)
- Start reload normally (R)
- During a timing band, press fire/reload confirm (impl: tap Fire or re-tap R in window) → **Flourish**
- Effects: slightly faster remaining reload **or** ammo crumb + **Style gain** + brief move/accuracy buff (draft 0.6–1.0 s)
- Failed window: normal reload, no punish beyond missed Style
- Style: **medium–high** — this is the “show-off reload” verb
- Mag-empty forced reload can still Flourish

#### Entrance (movement attack)
- Triggers when firing during/after: slide, jump/airborne, or weapon-equip flash (hold-V not used — primary equip)
- Draft: next Verse or auto-modded shot becomes Entrance variant (trail VFX, slight damage/Style bump)
- ICD draft 0.8–1.2 s so it is not every frame
- Style: **medium–high** — rewards mobility literacy (Aussie Slide Brace lesson: movement tech, not DPS tax card)

### 4.4 Style Rank system (sacred — always on)

#### Rank ladder

| Rank | Name (player-facing) | Draft power | Feedback |
|------|----------------------|-------------|----------|
| D | Dull | 1.00× | Quiet, flat trails |
| C | Competent | ~1.03× | Soft brass tick on enter |
| B | Bold | ~1.06× | Trail brightens; critic VO crumb |
| A | Acclaimed | ~1.10× + **Finale ready** | Halo pip; hymn sting |
| S | Sublime | ~1.14× + **Encore eligible** | Full brass; screen edge heat (tasteful) |
| SS | (optional path) | Critic/Encore only | Mythic — not required baseline |

Numbers are **mild**. Style is not a second Doobie. Damage budget stays in verbs + path cards; rank is the skill fantasy and climax gate.

#### Style points (internal 0–100 per rank band — or continuous meter with rank thresholds)

**Gain (draft weights — tune in playtest):**

| Action | Style gain | Notes |
|--------|------------|-------|
| New verb not in recent memory | High | Core variety pay |
| Stylish Finish (non-Tag verb on Spotlight) | Very high | Tag → other verb |
| Entrance hit | High | Mobility |
| Flourish success | High | Reload skill |
| Chorus hit | Medium | |
| Verse hit | Low | Filler |
| Multi-target Chorus / Entrance | Bonus crumb | Clear flair |
| Kill during A/S | Bonus crumb | Maintain aggression |

**Recent memory:** queue of last **N = 3–4** verb IDs. Using a verb already in the queue grants **reduced** Style (repeat tax). Using a fresh verb grants full + clears oldest.

**Decay / punish:**

| Condition | Effect |
|-----------|--------|
| No Style-eligible combat action for DecayDelay (~1.8 s) | Rank meter drains; can drop ranks |
| Same verb spammed beyond soft cap | Near-zero gain + optional “Boo” audio at harsh tiers |
| Owner takes significant hit (threshold) | Meter chunk loss; at S/A harsher; D minimal |
| Whiff Chorus / Tag into empty air | Tiny loss or zero gain (not brutal) |

**Do NOT:**
- Full reset to D on one hit at low ranks (feels bad)
- Require perfect play for C-rank (C should be default competent play)
- Grant Style for damage-over-time ticks alone (aggression must be active verbs)

#### Finale (baseline A-rank)

When rank first reaches **A** (or while at A/S):
- **Finale charge = 1** (max 1 baseline)
- Next **Chorus** can be consumed as **Finale Chorus**: big readable showstopper (damage mult draft ×1.5–1.8, wider arc, brass fanfare)
- Consuming Finale does **not** auto-drop rank (Encore path may cash harder)

#### Encore state (baseline S-rank — mild)

While at **S**:
- Slight handling sugar (reload, move) + mild damage
- Path Encore exotics rewrite what S means (cash-out super, team spotlight, etc.)
- Baseline S is “you’re cooking,” not “mission delete”

### 4.5 Base combat loop (zero upgrades)

```
Entrance (slide in) → Verse poke → Tag spotlight
  → Chorus Stylish Finish on Tag → rank climbs
  → mix Entrance / Verse / Tag / Chorus without looping
  → mag low → Flourish reload → back in
  → hit A → Finale Chorus as punctuation
  → drop aggression or repeat Verse forever → rank decays / boos
```

Skill without upgrades: verb diversity, Tag setups, movement Entrances, Flourish timing, when to spend Finale, not turtle at range with Verse only.

### 4.6 What baseline does NOT include

- No BPM / metronome quantize
- No heat bar / infinite ammo
- No blood / vitality feed
- No heal / Grace
- No anatomy transfer / Expose
- No Death Mark team fuse
- No dual-pistol independent L/R mags
- No SS rank (path-owned)
- No once-per-fight Super cash-out (Encore exotic)
- No extra verbs beyond the five (Repertoire path)
- No hard lockout on repeat (tax, not stun)

Those are path-, exotic-, or never-owned.

---

## 5. Shared Framework Vocabulary

Upgrades speak these verbs. Baseline owns Style Rank + five combat verbs only.

### 5.1 Verb
- Discrete combat action ID: Verse, Chorus, Tag, Flourish, Entrance, + path-added IDs
- Style memory and path cards key off verb IDs

### 5.2 Style Rank / Style Meter
- Always-on performance grade
- Paths: faster climb, harsher tax, longer memory, cash-out rules

### 5.3 Spotlight (Tag state)
- Short personal mark from Tag
- Stylish Finish when consumed by other verbs
- Repertoire may add Tag variants (ricochet Tag, aerial Tag)

### 5.4 Finale
- A-rank climax Chorus (baseline)
- Critic may improve; Encore may replace with bigger spends

### 5.5 Encore (path + state)
- S-rank living state (baseline mild)
- Encore path: spend rank for burst power / team show / super

### 5.6 Memory
- Recent verb queue length; Repertoire extends; Critic may shorten for harder grading

### 5.7 Repertoire (path)
- Additional verbs and combo rules

### 5.8 Critic (path)
- Rank economy mastery — gain curves, decay, hit punish, S-living

### 5.9 Boo / Brava (feedback)
- Negative/positive audio-visual critic responses (not just damage numbers)

---

## 6. Upgrade Paths (gravity wells — hybrids intended)

### Path A — CRITIC
**“The firmware is watching. Perform.”**

- Spine: Style gain mult, harsher repeat tax, slower decay when varied, hit-punish mitigation or inversion, S-rank passive power, rank floor crumbs, critic feedback intensity
- Crown fantasies: live at S; rank *is* damage/DR; stylish defense (rank saves a hit)
- Clear vs ST: flexible — rank amp helps both; not a clear hose identity
- Hybrid hooks: Critic keeps Encore cash-outs fed; Critic makes Repertoire verbs grade harder/faster

### Path B — REPERTOIRE
**“More moves. Longer phrases. No chorus on loop.”**

- Spine: new verbs, extended memory, Tag variants, Chorus transforms, Entrance upgrades, multi-Spotlight, combo recipes (specific sequences for bonus Style/damage)
- Crown fantasies: expanded kit that still teaches variety; “gunfu” without becoming a second melee slot
- Hybrid hooks: more verbs make Critic climbs easier; Encore sequences spend fancy finishers

### Path C — ENCORE
**“Save it for the high note.”**

- Spine: Finale upgrades, S-rank cash-out, rank spend → burst, team spotlight, once-per-fight Super, post-cash recovery rules
- Crown fantasies: intentional dump of rank for fight-winning punctuation; risk of falling to C/D after
- Hybrid hooks: Critic builds rank faster to dump; Repertoire dumps with fancier verbs

### Path × verb matrix

```
                  CRITIC                  REPERTOIRE               ENCORE
Style climb       core fantasy            easier via more verbs    fuel for cash-out
Repeat tax        harsher / readable      longer memory softens    tax before dump optional
Verse/Chorus      amp by rank             new variants             Finale/Super rewrite Chorus
Tag               finish mult by rank     Tag variants / multi     cash-out on tagged stage
Flourish          rank-scaled sugar       new flourish types       reload Super arming
Entrance          stylish DR / iframes    new movement verbs       aerial encore spend
S-rank            live there              show off kit             spend it
```

---

## 7. Crowns & Sacred Cows

### 7.1 Exotics (6 — equal large shapes)

**A-EX1. Harsh Critic — Exotic (Critic crown)**  
- Style gain from variety ↑; repeat tax harsher (earlier boo).  
- Rank damage mult band raised (still not Doobie).  
- At S: small DR crumb or “withstand one stagger” ICD (stylish toughness — not Caduceus heal).  
- The “rank is the build” keystone.

**A-EX2. Standing Ovation — Exotic (Critic crown)**  
- Reaching S triggers a short **Ovation window** (draft 4–6 s): ally-readable aura, mild team damage crumb **or** personal handling god-mode (pick one primary; **prefer personal** to avoid Caduceus overlap).  
- Taking a hit during Ovation ends it early (risk).  
- ICD between Ovations so it is not permanent S-lock.

**B-EX1. Expanded Atlas — Exotic (Repertoire crown)**  
- Adds **two** new baseline-class verbs (see §7.3) and +1–2 memory slots.  
- Mode-defining “I have a full kit” exotic.  
- Must ship with clear input map in tooltip.

**B-EX2. Double Feature — Exotic (Repertoire crown)**  
- Spotlight max targets +1–2; Tag can ricochet once to a second target.  
- Stylish Finish can chain (reduced) to second Spotlight.  
- Clear multi-tag showmanship without AMR fuse bombs.

**C-EX1. Curtain Call — Exotic (Encore crown)**  
- While at S (or on demand at S): **Cash Out** — spend rank down to C (or D) for a massive Chorus-class nuke / stage clear cone (readable super).  
- Once armed per ICD (draft 45–75 s) **or** once per full S reach (playtest).  
- Fight highlight button; failure = spending too early.

**C-EX2. Bravo Protocol — Exotic (Encore crown)**  
- Finale charges can bank to 2; Finale Chorus refunds a crumb of Style if it kills.  
- Optional: Finale can be Entrance or Tag-finish triggered (not only Chorus) — expands cash-out grammar.  
- Sustained encore fantasy without full Curtain Call dump.

### 7.2 Sacred cows (do not cut without rewriting identity)

- Style Rank always on at baseline  
- Variety gain + repeat tax  
- Aggression decay  
- Five empty-grid verbs (Verse / Chorus / Tag / Flourish / Entrance)  
- Mag + Flourish reload skill  
- Finale at A; mild S state  
- No BPM identity  
- No heal/Grace identity  
- No anatomy transfer identity  
- No HP-feed identity  
- Three peer paths; hybrids OK  
- ~30 upgrades; 6 equal large exotics  
- Tag ≠ team Death Mark spine  

### 7.3 Expanded Atlas — example added verbs (Repertoire)

When Expanded Atlas is equipped, add **two** of (doc locks both as the exotic package):

| Verb | Input sketch | Role |
|------|--------------|------|
| **Bridge** | Reload cancel into slide (or crouch-shot) | Mobility link verb |
| **Stinger** | Melee-range M1 tap when enemy in face range | Close panic jab (gun butt / bayonet tick) — still primary, not MeleeRework kit |

Alt pair if impl hates crouch: **Echo** (delayed second Verse shot) + **Stinger**.

All added verbs must:
- Have unique verb IDs for Style memory  
- Be readable in HUD glyph strip  
- Not delete the original five  

---

## 8. Full Upgrade List (~30 ship + backlog)

Rarity guide: Standard / Rare / Epic / Exotic / Oddity  
Tags: K Critic · R Repertoire · E Encore · G Glue · S Style · V Verb  
Cell rule: Exotic shapes larger; all Exotics same cell count.  
Player-facing names below. API names at implementation.

------------------------------------------------------------------------------
PATH A — CRITIC
------------------------------------------------------------------------------

A-EX1. Harsh Critic — Exotic (crown)
       Variety gain up; repeat tax harsher; rank power band up; S toughness crumb.

A-EX2. Standing Ovation — Exotic (crown)
       On reaching S: Ovation window; early hit cancels; ICD.

A-EP1. Rubric Rewrite — Epic
       +Style gain from Stylish Finish and Entrance; −Style gain from pure Verse spam further.

A-EP2. Thick Skin Review — Epic
       Hit-punish on Style meter reduced; at B+ small DR while rank meter is rising.

A-EP3. No Encores for Cowards — Epic
       DecayDelay longer if last 3 verbs were all different; shorter if last 3 included duplicates.

A-EP4. S-Floor — Epic
       When you would drop below A from decay alone (not hit punish), linger at A briefly (draft 1.5 s). Hit punish still drops.

A-RA1. Brass Lung — Rare
       +Style gain global modest.

A-RA2. Short Leash — Rare
       Memory queue shorter (harder grading) but +gain when you pass the harsher test.

A-RA3. Critic’s Shield — Rare
       When rank drops from damage, gain brief move speed (escape to reset show).

A-ST1. Warm-Up Act — Standard
       First Style gain after spawn / out-of-combat slightly increased (teach climb).

------------------------------------------------------------------------------
PATH B — REPERTOIRE
------------------------------------------------------------------------------

B-EX1. Expanded Atlas — Exotic (crown)
       +2 verbs (Bridge + Stinger); +memory slots.

B-EX2. Double Feature — Exotic (crown)
       Multi-Spotlight; Tag ricochet; chain Stylish Finish crumb.

B-EP1. Director’s Cut — Epic
       Chorus gains a second mode: tap-release vs hold-release different arc/slug (two Chorus verb IDs or sub-verbs — prefer **sub-verb counts as variety**).

B-EP2. Spotlight Rig — Epic
       +Tag duration; +Stylish Finish damage; Tag applies mild slow on Spotlight (not Condemned).

B-EP3. Marquee Entrance — Epic
       Entrance ICD down; Entrance hits refresh DecayDelay; slight Entrance size.

B-EP4. Call Sheet — Epic
       HUD shows recommended “next fresh verb” pip (QoL teaching); tiny Style crumb if obeyed (optional — cut if too gamey).

B-RA1. Deep Memory — Rare
       +1–2 Style memory slots (easier variety).

B-RA2. Ricochet Tag — Rare
       Tag bounces once (if Double Feature not owned; with it, +bounce rules).

B-RA3. Bayonet Lesson — Rare
       Stinger-range auto if Atlas not owned: close M1 converts to weak Stinger verb (teaches melee-adjacent without exotic). **Or** +Stinger power if Atlas owned.

B-RA4. Second Take — Rare
       Whiffed Chorus refunds 1 ammo and tiny Style consolation (fun failure).

B-ST1. Understudy — Standard
       Minor +Chorus damage.

------------------------------------------------------------------------------
PATH C — ENCORE
------------------------------------------------------------------------------

C-EX1. Curtain Call — Exotic (crown)
       Cash out S → huge stage nuke; rank drops; ICD.

C-EX2. Bravo Protocol — Exotic (crown)
       Bank Finale×2; Finale kill refunds Style; optional multi-verb Finale trigger.

C-EP1. High Note — Epic
       Finale mult up; Finale grants brief ally damage crumb in small radius (team show — keep mild vs Caduceus).

C-EP2. Exit Music — Epic
       After cash-out or Finale, brief invuln frames / DR (0.4–0.7 s) so spending is not suicide.

C-EP3. Reprise — Epic
       Killing during S refunds a slice of Style meter (maintain show).

C-EP4. Intermission — Epic
       Cash-out can stop at B instead of C/D (pay less, keep some rank) — toggle or hold-mod.

C-RA1. Fanfare Fuse — Rare
       Finale arms slightly before full A (at high B) — earlier climax access.

C-RA2. Spotlight Tax — Rare
       Stylish Finish builds bonus Finale charge progress (not full free Finale).

C-RA3. Showstopper Shells — Rare
       +Finale and cash-out damage %; −Verse damage slightly (commit to peaks).

C-ST1. Curtain Warmers — Standard
       Minor +Style gain while at A or S.

------------------------------------------------------------------------------
GENERIC / GUNFEEL
------------------------------------------------------------------------------

G-RA1. Lead Hook — Rare
       +Verse damage %; Verse repeat tax slightly softer (still not free spam).

G-RA2. Wide Chorus — Rare
       +Chorus arc/size; −Chorus damage slightly.

G-RA3. Quick Change — Rare
       +Reload speed; Flourish window slightly wider.

G-RA4. Touring Rig — Rare
       +Move speed while Style rank ≥ B.

G-ST1. Clean Notes — Standard
       −Recoil / tighter Verse group.

G-ST2. Spare Charts — Standard
       +Ammo reserves.

G-ST3. Stage Lights — Standard
       +Tag projectile speed / lock feel.

G-ST4. Encore Mag — Standard
       +Magazine size modest.

G-OD1. Boundary Incursion — Oddity
       +Upgrade grid size.

------------------------------------------------------------------------------
FROZEN 30 FOR V1 SHIP
------------------------------------------------------------------------------

EXOTIC (6)
  1  Harsh Critic
  2  Standing Ovation
  3  Expanded Atlas
  4  Double Feature
  5  Curtain Call
  6  Bravo Protocol

EPIC (8)
  7  Rubric Rewrite
  8  Thick Skin Review
  9  Director’s Cut
 10  Spotlight Rig
 11  Marquee Entrance
 12  High Note
 13  Exit Music
 14  Reprise

RARE (10)
 15  Brass Lung
 16  Short Leash
 17  Deep Memory
 18  Ricochet Tag
 19  Second Take
 20  Fanfare Fuse
 21  Showstopper Shells
 22  Lead Hook
 23  Wide Chorus
 24  Quick Change

STANDARD (5)
 25  Warm-Up Act
 26  Understudy
 27  Curtain Warmers
 28  Clean Notes
 29  Spare Charts

ODDITY (1)
 30  Boundary Incursion

------------------------------------------------------------------------------
BACKLOG (designed, not in first 30)
------------------------------------------------------------------------------

Critic
- No Encores for Cowards
- S-Floor
- Critic’s Shield
- SS rank ladder
- Ally ovation aura (team damage) as alternate Standing Ovation mode

Repertoire
- Call Sheet HUD coach
- Bayonet Lesson without Atlas
- Echo verb (delayed second shot)
- Aerial Tag only-in-air variant
- Triple Feature (3 spotlights — probably too much)

Encore
- Intermission partial cash-out
- Spotlight Tax
- Team Curtain Call (co-op super — careful)
- Heavy weapon Finale rider (thin; Carver owns feast)

Generic
- Touring Rig, Stage Lights, Encore Mag
- Element tips (Shock/Fire) — keep off identity spine
- True gunblade heavy melee stance (reject if it steals MeleeRework)

Explicitly rejected
- BPM / metronome quantize as identity
- Self-Mend / Grace heal
- Anatomy transfer
- HP feed / blood stacks
- Heat-infinite ammo delete Flourish
- Death Mark team fuse as Tag default
- Dual independent pistol channels (Rhythm owns stereo guns)
- Style as pure damage meter with no repeat tax

---

## 9. Example Builds

**S-Rank Lifespan (Critic)**  
Harsh Critic + Standing Ovation + Rubric Rewrite + Thick Skin Review  
+ Brass Lung + Short Leash + Touring Rig (backlog) / Quick Change  
→ Climb fast, live loud at S, Ovation windows, hate Verse loops.

**Gunfu Kit (Repertoire)**  
Expanded Atlas + Double Feature + Director’s Cut + Spotlight Rig  
+ Marquee Entrance + Deep Memory + Ricochet Tag + Second Take  
→ Full verb kit, multi-tag finishes, fancy Chorus modes.

**Showstopper (Encore)**  
Curtain Call + Bravo Protocol + High Note + Exit Music  
+ Reprise + Fanfare Fuse + Showstopper Shells + Curtain Warmers  
→ Bank Finales, cash S for deletes, survive the dump.

**Hybrid headliner (trailer)**  
Harsh Critic + Expanded Atlas + Curtain Call  
+ Spotlight Rig + Marquee Entrance + Rubric Rewrite + Bravo Protocol  
→ Grade hard, more verbs to pass the grade, cash the high note.

**Tag conductor (co-op readable)**  
Double Feature + Spotlight Rig + High Note + Rubric Rewrite  
+ Ricochet Tag + Lead Hook + Clean Notes  
→ Multi-spotlight finishes; allies see the show; mild team High Note sugar.

---

## 10. Economy & Tuning Rules of Thumb

- **Power budget lives in verb quality, Stylish Finishes, and A/S windows** — not rank mult alone.
- Rank damage band stays mild (D→S roughly +0–14%); path cards and Finales carry spikes.
- Verse must be viable filler but **bad** as a full lifestyle under Critic.
- Tag without finish should feel incomplete (setup toy).
- Flourish must be learnable in one mag cycle; window too tight = frustration.
- Entrance ICD prevents movement spam as free Style faucet.
- Curtain Call ICD / arm rules prevent every-pack super.
- Standing Ovation must not outshine Caduceus team amp — prefer personal or tiny team crumb.
- Memory N=3–4: if variety feels impossible in chaos, raise N; if S is free, lower N or raise tax.
- Mag size: empty-grid must Flourish sometimes; infinite mag deletes a verb.
- Co-op: rank glyph readable at a glance (colorblind-safe shapes, not only color).

### 10.1 Anti-loop rules (implement)

| Loop | Rule |
|------|------|
| Verse only forever | Repeat tax → near-zero Style; Critic makes it painful |
| Tag → Tag → Tag | Memory tax; finish pay is on other verbs |
| Entrance every ICD forever | Cap Entrance Style/sec; still allows movement play |
| Curtain Call every pack | ICD / S-arm gate |
| Ovation permanent | Hit cancels; ICD between triggers |
| Finale every Chorus | Max 1–2 charges; A gate |

---

## 11. Status & Counter Split

| System | Role | Baseline? |
|--------|------|-----------|
| Style Rank / meter | Performance grade | Yes |
| Verb memory queue | Variety tracking | Yes |
| Spotlight | Tag personal mark | Yes |
| Finale charge | A-rank climax | Yes |
| Encore / S-state | Mild S living; path cash-out | Mild yes / path |
| Heat / Blood / Grace | Not used | No |
| BPM Tempo | Not used | No |
| Anatomy Mark/Expose | Not used | No |
| Fire/Shock/Acid | Optional tips backlog | Backlog |
| Poison/Bleed/Bees | Not identity | No |

### 11.1 Spotlight rules (draft)

| Param | Draft |
|-------|--------|
| Duration | 3–4 s |
| Max targets | 1 baseline; 2–3 with Double Feature |
| Consume on Stylish Finish | Yes (full consume) |
| Verse on Spotlight | Partial Style only; no full consume unless card |
| Team damage amp | **None** baseline (not Condemned) |
| VFX | Brass reticle / spotlight cone on enemy |

---

## 12. Strengths, Weaknesses & Co-op

**Strengths**
- Unique arsenal fantasy (performance grading)
- High skill expression and spectatable co-op moments
- Deep hybrid space (grade hard + more verbs + cash-out)
- Teaches movement + reload skill without being a movement character
- Failure states are readable (boo / rank drop) and usually funny

**Weaknesses**
- Higher cognitive load than a hose gun
- Verse-only players will feel “weak Style” (by design)
- Close scramble if Chorus charge whiffed
- Not top brain-off DPS
- HUD dependency (rank must be clear)

**Co-op**
- Rank is a social signal — celebrate S, laugh at boos
- High Note / Ovation team crumbs stay mild
- Tag is personal finish bait, not mandatory team mark UI spam
- Curtain Call VFX must not blind allies

---

## 13. Visual, Audio & Thematic Design

**Appearance**
- SAXON exhibition carbine: polished hazard-white/chrome, brass critic bells at the stock, Form 88-STY wafer seal, folding bayonet rail, fungally scarred “DO NOT BOO THE OPERATOR” stencil
- Rank: color + **shape** pip (D square → S starburst) for colorblind play
- Spotlight: brass volume light on target
- Finale/Curtain: ticket-stub casings / confetti-hazard tape debris (industrial comedy)

**Sound**
- Verse: tight carbine peck
- Chorus: heavier brass-bodied report / arc whoosh
- Tag: spotlight shutter + soft lock chime
- Flourish: snare-roll reload + crowd crumb
- Entrance: slide hiss + sting
- Rank up: rising brass intervals
- Repeat tax: muffled boo buzzer / negative UI
- Curtain Call: full fanfare sting then impact

**Flavor / codex line (in-game style)**
  Bravura  
  Exhibition carbine. Style Rank rises with varied, aggressive play and falls
  when you repeat yourself or disengage.  
  Verse, Chorus, Tag, Flourish, and Entrance are always available.  
  High ranks arm Finale choruses. Encore upgrades cash the show out.

---

## 14. Implementation Notes (for later)

### 14.1 Gear registration
- Weapon template: clone projectile primary (CartridgeSMG OK), APIName `bravura`, high-range gear id, behaviour host, SpawnGear stamp, CreateUpgrade pool.
- Plugin: GUID `sparroh.bravura`, MycoMod **IsSandbox**.
- Persistence: stable gear id before PlayerData.AddGear.

### 14.2 Behaviour host
BravuraBehaviour:
- WeaponData: verb enables, Style curves, memory N, Finale rules, path flags, Spotlight params, cash-out params
- Runtime: rank, meter, memory queue, Spotlight map, Finale charges, Ovation timer, Entrance ICD, Flourish state, cash-out ICD
- Prefab snapshot restore on upgrade Remove

### 14.3 Verb router
Central input → verb ID resolution before fire:

```
if FlourishWindow && confirm: Flourish
else if EntranceConditions && fire: Entrance (modifies shot)
else if RMB Tag ready: Tag
else if M1 held past ChorusThreshold on release: Chorus
else if M1 tap/stream: Verse
```

Each successful verb calls `StyleSystem.RegisterVerb(id, context)`.

### 14.4 Style system
- RegisterVerb: compute gain with memory tax, apply meter, evaluate rank thresholds, fire feedback events
- Tick: decay after DecayDelay
- OnOwnerDamaged: punish if damage > threshold
- StylishFinish detect: damage event on Spotlight target with verb ≠ Tag

### 14.5 Hooks

| Hook | Use |
|------|-----|
| Input / fire gate | Verb router; Chorus hold |
| OnFiredBullet / melee-arc | Verse/Chorus/Entrance damage |
| Reload start/update | Flourish window |
| OnDamageTarget | Stylish Finish; Style kill crumbs |
| OnBeforeTakeDamage | Critic DR; Ovation cancel; hit punish |
| Player move flags | Entrance eligibility (slide/air) |
| HUD | Rank, meter, memory glyphs, Finale pip, Spotlight |

### 14.6 HUD (mandatory design)
- Rank glyph (shape + letter)
- Thin Style meter
- Optional last-verbs strip (3–4 icons) — huge for teaching
- Finale charge pips
- Flourish timing cue on reload bar
- Prefer SparrohUILib if acceptable; else minimal

### 14.7 VFX / audio priority
1. Rank up/down stings  
2. Boo on repeat tax  
3. Spotlight on target  
4. Stylish Finish confirm  
5. Flourish success  
6. Finale / Curtain Call fanfare  
7. Per-verb muzzle personality  

### 14.8 Multiplayer
- Owner-authoritative Style meter  
- Replicate rank byte + Spotlight targets for ally readability  
- Cash-out damage via normal damage authority  
- All clients need matching mod (sandbox)

---

## 15. Deliberate Non-Goals

- Not Rhythm Stitchers BPM dual-wield  
- Not Heat Cycler infinite heat hose  
- Not Caduceus heal/revive  
- Not DMLR anatomy  
- Not Blood/Vitality feed  
- Not AMR Death Mark team brand  
- Not MeleeRework kit registration  
- Not pure damage meter without repeat tax  
- Not SS baseline  
- Not requiring custom prefab for v1 (clone OK)  
- Not anime full-voice Style announcer as hard requirement (brass + text OK)  

---

## 16. Open Tuning Questions (playtest, not design blockers)

1. Mag 18 vs 24; Flourish frequency feel.  
2. Memory N = 3 vs 4.  
3. Rank damage band ceiling (+10% vs +18% at S).  
4. DecayDelay 1.6 vs 2.2 s.  
5. Hit punish threshold (chip vs chunk).  
6. Chorus = arc sweep vs fat slug (presentation).  
7. Tag press vs ADS conflict on controllers.  
8. Entrance: slide-only vs slide+jump+equip.  
9. Curtain Call ICD 45 vs 75 s.  
10. Standing Ovation personal vs tiny team crumb.  
11. Expanded Atlas verb pair finalization.  
12. Whether Call Sheet “next verb coach” ships or stays backlog.  
13. Colorblind rank shapes validation.  

---

## 17. Success Criteria / Player Fantasy Checklist

- [ ] Style Rank visible and meaningful with zero upgrades  
- [ ] Five verbs all usable empty-grid  
- [ ] Verse-only play climbs slowly or stalls under repeat tax  
- [ ] Tag → Chorus Stylish Finish is an obvious “brava” moment  
- [ ] Flourish timing is learnable within a few reloads  
- [ ] Entrance rewards slide/jump without being mandatory tax  
- [ ] A-rank Finale Chorus is a readable climax  
- [ ] S-rank feels like a show, not immortality  
- [ ] Harsh Critic makes living at S a real build  
- [ ] Expanded Atlas clearly adds verbs without HUD chaos  
- [ ] Double Feature multi-tag finishes feel intentional  
- [ ] Curtain Call is a fight highlight with real cost  
- [ ] Bravo Protocol supports multi-Finale shows  
- [ ] No BPM / heal / anatomy / blood identity creep  
- [ ] Co-op allies can tell when someone is cooking  
- [ ] Boos are funny, not rage-quit inducing  
- [ ] ~30 upgrades; 6 equal large exotics  

---

## 18. Review Decisions Locked (2026-08-15)

| Decision | Lock |
|----------|------|
| Name | **Bravura** |
| Slot | Primary |
| Paths | Critic / Repertoire / Encore |
| Style | Always-on; variety + aggression |
| Baseline verbs | Verse / Chorus / Tag / Flourish / Entrance |
| Mag | Real mag; Flourish reload verb |
| Finale | A-rank Chorus climax |
| S-rank | Mild living state; Encore cashes out |
| Tag | Personal Spotlight; not team Death Mark |
| Not BPM | Locked out |
| Not heal/blood/anatomy | Locked out |
| Exotics | Harsh Critic, Standing Ovation, Expanded Atlas, Double Feature, Curtain Call, Bravo Protocol |
| Ship pool | Frozen 30 above |
| Tone | SAXON exhibition / Form 88-STY |
| Gap filled | Only performance-grading primary |

---

## 19. Relationship to Collection Analysis

Bravura is the intentional fill for the arsenal gap identified in the design-collection review:

- Resources you **manage** → Heat, Junk, Blood, Vitality, Grace (owned elsewhere)  
- Modes you **switch** → DMLR, Trident, Rapture, Caduceus (owned elsewhere)  
- Spaces you **own** → HLC, Scrapworks, Hive, Siege Halo (owned elsewhere)  
- **Performance you put on** → **Bravura**

Steal safely: Cycler always-on meter literacy; Rapture mode-dialogue *spirit* without 2-mode cap; Junk streak *presentation*; DMLR setup→spend *pattern* via Tag→Finish; AMR climax *readability* via Finale; Caduceus full-bar *feedback* without heal.

---

## 20. Implementation Checklist (post-design)

- [ ] Rename plugin/csproj/thunderstore → Bravura  
- [ ] BravuraBehaviour + StyleSystem + VerbRouter  
- [ ] Five baseline verbs wired  
- [ ] Rank meter + memory + decay + hit punish  
- [ ] Spotlight + Stylish Finish  
- [ ] Flourish reload window  
- [ ] Entrance move detection  
- [ ] Finale at A  
- [ ] Six exotics  
- [ ] Frozen 30 CreateUpgrade  
- [ ] HUD rank/verbs/Finale/Flourish  
- [ ] Persistence + SpawnGear  
- [ ] Playtest §16 knobs  

---

## 21. Changelog

### v1 (2026-08-15)
- Initial full design bible from collection analysis Style-meter proposal  
- Name locked: Bravura  
- Paths: Critic / Repertoire / Encore  
- Baseline five verbs + always-on Style Rank  
- Frozen 30 + 6 exotics  
- Explicit non-overlap locks vs Rhythm, Cycler, Rapture, DMLR, Caduceus, Carver/Helminth, AMR marks  

---

*End of Bravura Design Doc v1. Next collection step: conflict matrix including Arrest Warrant, Final Judgement, Thermal Solstice, and Bravura.*
