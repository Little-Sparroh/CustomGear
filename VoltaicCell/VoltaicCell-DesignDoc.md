# Voltaic Cell — Design Document

> Status: **Design only** — no implementation yet.  
> Working title in notes: Shock Grenade Rework. **Ship name: Voltaic Cell.**  
> Template base: `.new.ShockGrenadeRework` grenade content project.  
> Product shape: **separate gear** — vanilla Shock Grenade is left unmodified.

---

## 1. High Concept / Fantasy

**Voltaic Cell** is the throwable capacitor.

Where **Incendiary** is a fire boom economy, **Caustic Flask** is corrosion + timed DR plating, **Honey Jar** is bees + nectar HoT, and **Splash Canister** is water primer + thin regenerating chip-shell, Voltaic Cell is **speed lightning with a blue bar**: a bland shock boom at stock that upgrades into live-wire body storms, fat overshield dumps, aerial lightning weather, and illegal pocket spam — without stealing HoT, % armor plating, or Splash’s rebuild-on-idle shell.

**One-liner:** *Throw a cell of lightning. Upgrades turn you into a live wire, a walking overshield capacitor, or a flying storm — speed and blue bar, not HoT or carapace.*

**Element:** `EffectType.Shock`  
Load-bearing identity: electrocute saturation, Live Wire body-boom, overshield (negative-heal / overhealth), and mobility toys. Not Fire, Acid, Bees, or Water.

---

## 2. Role in the Arsenal

| Gear | Fantasy | Voltaic Cell relationship |
|------|---------|---------------------------|
| **Vanilla Shock Grenade** | Legacy mobility kit; Live Wire + Illegal Pocket dominates | **Left in game.** Cell is the intentional parallel kit; no patch requirement on vanilla. |
| **Caustic Flask** | Acid boom + puddles / vacuum / **timed % DR** | Flask = plating DR. Cell = **blue overshield HP**. Different verb. Lightspeed **lives here** (Flask already ships it out). |
| **Splash Canister** | Water primer + **small regenerating** chip shell (~5–10) | Splash = thin shell that rebuilds if you stop eating chip. Cell = **fat capacitor dumps** on boom / Wire pulses. |
| **Honey Jar** | Bees + pure HoT nectar | Soft synergy only. No nectar, no base-HP HoT identity. |
| **Incendiary Grenade** | Fire boom + self-fire economy | Cell is mobility + capacitor, not heal-on-burn. |
| **Friend in a Box** | Deployable ally AI | Complementary. Cell is self/field lightning, not a unit. |
| **Photon Disc** | Tumble disc + attunement | Wholly different projectile fantasy. |
| **Arc Lightning / Shock weapons** | Electrocute payoffs | Soft synergy via Shock Funnel / Higher Voltage analogues; no code coupling. |

**Niche tags:** Shock · Mobility · Overshield (Capacitor) · Live Wire · Storm Field

**Slot:** Throwable / grenade gear  
**Range:** Mid throw arc, impact AOE + optional body-boom / aerial strikes / teleport  
**Role:** Primary-element status nade that *opts into* speed demon, blue-bar diver, or storm carpet

---

## 3. Design Pillars

1. **Bland baseline**  
   Stock throw matches the other primary-element grenades: damage + Shock apply. No free Live Wire, overshield, teleport, Flash Storm strikes, or Pocket economy.

2. **Flat Shield (overshield) is opt-in and *this kit’s* home**  
   Blue overhealth / flat shield dumps live on upgrades only. Never baseline. Never pure HoT. Never Caustic-style % DR plating. Never Splash-style always-on regenerating chip engine as the headline.


3. **Overshield model B (locked)**  
   Overshield grants fire on **primary boom** *and* on **Live Wire explosion pulses** when those systems are active. Capacitor and Wire are meant to marry, not silo.

4. **Speed is the other spine**  
   Live Wire remains a first-class exotic and cultural peak — not a guilty pleasure to nerf out of existence.

5. **Lightspeed Material Transfer lives here**  
   Stolen from Acid / explicitly moved off Caustic Flask. Teleport-to-impact is Shock’s mobility grammar.

6. **Three gravity wells + one economy exotic**  
   Live Wire · Capacitor · Storm, plus **Illegal Pocket Cell** as peer exotic utility (spam enabler across wells).

7. **Pocket + Wire stays a celebrated peak**  
   Still one of the best / most fun boards — but **Capacitor** and **Storm** must be peer-complete builds, not support cast for Wire.

8. **Readable exotics**  
   Body storm · fat overshield reservoir · aerial lightning · illegal pocket economy. Equal large hex footprints.

9. **Names are free to change**  
   Fantasy and mechanical role matter more than vanilla strings. Iconic beats (Live Wire, Pocket, Flash Storm, Lightspeed) may keep or rename for Voltaic Cell branding.

10. **Self-contained v1**  
    No hard deps on Honey Jar, Splash, Caustic, Friend, ArcLightning, or other mods. Cross-kit notes are player-facing only.

11. **~30 upgrades**  
    Honey Jar / Splash / Caustic / DMLR universal truths apply.

---

## 4. Baseline (No Upgrades)

**Feel target:** same honesty as the shared throwable family — a clean elemental boom. Identity is on the grid.

**Shared family baseline (locked):** damage **100** · effect amount **10** · max charges **3** · recharge **45** · explosion radius (`hitForce`) **6**.

| Property | v0 target | Notes |
|----------|-----------|--------|
| Throw / arc / bounce | Incendiary / Shock-like | Clone throw feel; Shock element |
| Impact damage | **100** | Shared family boom; delivery + shock |
| Element | `EffectType.Shock` | |
| Shock effect amount (`damageEffectAmount`) | **10** | Full-sat class dump from empty (shared family) |
| Explosion radius (`hitForce`) | **6** | Wide Arc scales |
| **Live Wire** | **None** | Exotic unlock |
| **Overshield / Flat Shield** | **None** | Capacitor upgrades unlock — event-based blue bar dumps |
| **Teleport** | **None** | Lightspeed unlock |
| **Aerial lightning** | **None** | Flash Storm unlock |
| Healing / HoT / % DR | **None** | Permanently out of identity (except blue OS via upgrades) |
| Max charges | **3** | Shared family baseline; Twin Cell stacks on top |
| Recharge duration | **45** | Shared family baseline |
| Self-hit | Shock can apply at reduced amount | Insulated Gloves reduces further |
| Self damage | Low vs enemy impact | Launch / Thunder toys use self-hit intentionally |


### Baseline detonation sequence

```
Throw → bounce/fuse (vanilla grenade rules)
  → Primary boom (DamageData: standard damage, EffectType.Shock, high effect amount, AOE)
  → Done
```

### Why bland stock

- Primary-element grenades are **bland at baseline**; fantasy is upgrade-gated.  
- Free Live Wire steals the exotic.  
- Free overshield steals Faraday / Static Sheath.  
- Vanilla’s problem was not “too simple stock” — it was **one-dimensional upgrade gravity** (Pocket + Wire).

---

## 5. System Truths

### 5.1 Shock element (from game)

- Shock status: saturation + full-sat electrocute DoT (enemy/player tick constants per vanilla).  
- Saturation add: `amount * 0.1` per application → roughly **10 effect amount ≈ full electrocute** from empty, before decay.  
- Vanilla Water interactions remain world rules (Splash wets → Shock pays out harder). Cell does not reimplement Water; soft synergy only.  
- Default full-sat lifetime in line with standard elements (~3s class unless a linger system re-applies).

### 5.2 Language for upgrade text

| Term | Meaning |
|------|---------|
| **Electrified / electrocuted** | Target has Shock status with saturation > 0 (or full-sat where specified) |
| **Fully electrocuted** | `IsFullySaturated` on Shock |
| **Applying shock** | Damage/effect that adds Shock saturation (boom, Wire pulse, Flash Storm strike, residual field) |
| **Live Wire active** | Local player is in Live Wire body-boom state |
| **Your overshield / capacitor** | Blue overhealth granted by **this player’s** Voltaic Cell systems only |
| **Wire pulse** | Each explosion tick while Live Wire is active (interval from Live Wire rolls) |

### 5.3 Overshield (upgrade-only) — model B locked

```
VoltaicOvershield (blue overhealth):
  grant sources (when respective upgrades equipped):
    - Primary boom detonate → players in radius (self always eligible; allies if card says so)
    - Each Live Wire pulse explosion → self (and allies in pulse radius if card says so)
  amount       = from upgrade rolls (rares small; Faraday large)
  stack policy = additive up to hard CAP from this gear (v0: ~80–120 total blue from Cell sources)
  decay        = timed bleed after short hold, or delayed decay (readable blue bar; pick one at impl and stick)
  type         = overhealth HP via negative-heal / overhealth API family
                 (Acid Polymer anchor: IDamageSource.HealTarget(source, player, -amount, pos))
  on damage    = overshield absorbs before base health (vanilla overhealth rules)
  display      = blue bar / stack read mandatory while > 0
  NOT included = base-HP HoT, % DR plating, Splash regenerating chip engine,
                 permanent resist, baseline grant
```

**Differentiation (player-facing):**

| Kit | Sustain verb |
|-----|----------------|
| Honey Jar | Pure HoT to base health |
| Splash Canister | Thin shell that **rebuilds after you stop taking chip** |
| Caustic Flask | Timed **% damage resistance** (no blue bar) |
| **Voltaic Cell** | **Flat Shield** — fat capacitor dump when you boom / Wire — spend it by diving |


**Model B marriage:**  
Static Sheath / Leyden Gate / Faraday Reservoir define *how much* and *who*. Ground Path and Faraday explicitly scale **Wire pulse** grants so Live Wire builds are also capacitor builds when invested.

### 5.4 Live Wire (upgrade-gated exotic)

Port vanilla Live Wire fantasy:

```
On activation (throw / detonate path per vanilla Live Wire rules — prefer: you become the grenade):
  duration          = D from rolls (~4–7s vanilla ballpark)
  explosion interval = I from rolls (~1.3–1.6s vanilla ballpark)
  movespeed bonus   = S from rolls
  each pulse        = shock explosion at player position (damage/radius from gun + modifiers)
  overshield        = if Capacitor cards equipped, each pulse may grant OS (model B)
```

**Livelier:** requires Live Wire; extra speed + can’t stop sprinting while Wire active.  
**Illegal Pocket Cell:** does not replace Wire; it feeds Wire (and everything else) via recharge.

### 5.5 Lightspeed Material Transfer (upgrade-gated epic)

Port vanilla Acid Lightspeed fantasy onto this gear:

```
On grenade land / detonate (define one authoritative moment at impl — prefer impact/detonate):
  teleport thrower to impact position (safe ground snap / vanilla Lightspeed rules)
  optional tiny i-frame or momentum preserve — match vanilla feel first
```

No multi-hop exotic in v1 unless a stretch card appears. Lightspeed is the **epic mobility spike**; Live Wire is the **duration mobility mode**.

### 5.6 Flash Storm (upgrade-gated exotic)

Port vanilla Flash Storm:

```
While grenade projectile is in flight:
  periodic downward lightning strikes along path (interval from rolls)
  each strike = shock damage + shock apply in small radius
```

Storm Relay rare improves interval / strike power. Does not require Live Wire.

### 5.7 Illegal Pocket Cell (economy exotic)

Port vanilla Illegal Pocket Grenade:

```
While equipped:
  explosion radius heavily reduced
  explosion damage heavily reduced
  recharge massively accelerated (vanilla Pocket ballpark: CD ~1.8–2.2, size −70%+, damage −75%+)
```

**Design read:** Pocket is the **spam exotic**, not a Wire-only card.  
- Pocket + Wire = celebrated peak (many weak body booms + OS ticks if Capacitor invested).  
- Pocket + Faraday = rapid capacitor top-ups (small booms, frequent OS).  
- Pocket + Flash Storm = carpet of tiny cells with strike weather.  

Do **not** silently hard-require Wire for Pocket to feel good.

### 5.8 Launch / knock toys

| Card | Fantasy |
|------|---------|
| **Launch Charge** | Self-hit launches you **up** |
| **Thunder Pressure** | Explosion launches you **away** from epicenter |
| **Cloud Skip** | Small double jump (movement toy; not throw-gated forever — vanilla “gain double jump”) |

These stay Wire-well gravity but work without Live Wire equipped (except where text says otherwise).

### 5.9 Cooldown philosophy

| Allowed | Avoid as default pattern |
|---------|---------------------------|
| Quick Cell pure CDR | Hidden recharge bricks on radius/damage cards |
| Twin Cell: +1 charge with **explicit mild +CD** on that card only | Stacking CD taxes until cell feels unusable |
| Shock Funnel / Static Charge / Marathon-adjacent economy | Pocket as the *only* viable recharge story |
| Illegal Pocket Cell as loud explicit trade (size/damage for CD) | Quiet global CD penalties on Faraday / Wire |

**Pocket is allowed to be a huge CD swing** because it is an exotic with loud downside. Standards/rares should not recreate Pocket’s tax culture by accident.

### 5.10 Explicit moves onto / off this kit

| Card / fantasy | Fate |
|----------------|------|
| Lightspeed Material Transfer (vanilla Acid) | **On Voltaic Cell** |
| Polymer Coating overshield (vanilla Acid) | **Spiritual home is Cell** (new names: Static Sheath / Faraday) — not Caustic |
| Connected Systems | Still **future Cryo** (Caustic doc) — not Cell |
| Vanilla Shock Live Wire / Pocket / Flash Storm / mobility suite | **Retained fantasies** (names free) |
| Honey nectar / Caustic timed DR / Splash regen shell | **Out of identity** |

---

## 6. Gravity Wells (Thematic Attractors)

Not exclusive trees. Taking one exotic or epic pulls related cards into value; every upgrade remains equippable with every other (except explicit requirement flags).

### Well A — **Live Wire** (mobility / become the boom)

*You are the cell. Explode, sprint, blink, launch.*

Core pieces: Live Wire, Livelier, Lightspeed Material Transfer, Launch Charge, Thunder Pressure, Cloud Skip, Marathon, System Overload, Static Charge, Fastball, Emergency Evac, Illegal Pocket Cell (enabler).

### Well B — **Capacitor** (overshield)

*Store charge as blue bar. Dive on a full cell.*

Core pieces: Faraday Reservoir, Leyden Gate, Static Sheath, Dielectric Cap, Ground Path, Insulated Gloves, Quick Cell, Twin Cell, (Pocket as rapid top-up enabler).

### Well C — **Storm** (field / arc pressure)

*Lightning as weather and control.*

Core pieces: Flash Storm, Storm Relay, Electromagnet, Stun, Excited Plasma, High Voltage, Wide Arc, Hard Cell, Phase Housing, Shock Funnel.

### Economy (peer exotic, not a fourth well narrative)

**Illegal Pocket Cell** — spam fantasy retained from vanilla Shock. Sits beside wells as “illegal recharge economics,” feeding Wire, Capacitor, and Storm equally.

### Mix examples (expected, not edge cases)

- Live Wire + Livelier + Lightspeed + Marathon → pure speed demon  
- Live Wire + Ground Path + Faraday + Pocket → celebrated peak: body storm that **also** tops blue bar every pulse  
- Faraday + Leyden Gate + Dielectric Cap + Quick Cell → capacitor diver without Wire  
- Flash Storm + Electromagnet + Stun + High Voltage → storm control carpet  
- Pocket + Flash Storm + Shock Funnel → tiny cells, constant strikes, gun electrocutes refund throws  
- Lightspeed + Launch Charge + Thunder Pressure + Cloud Skip → movement tech playground  
- System Overload + Faraday team radius → co-op speed + blue bar peel  

---

## 7. Content Budget & Universal Truths

Aligned with Honey Jar / Splash Canister / Caustic Flask / FriendinaBox / vanilla grenade expectations:

| Rule | Value |
|------|--------|
| Total upgrades | **~30** (named kit + standards + fillers) |
| Exotics | **4** — Live Wire, Faraday Reservoir, Flash Storm, Illegal Pocket Cell |
| Exotic hex footprint | **Equal and large** across all four |
| Epics | **~7** |
| Rares | **~10** |
| Standards | **~9** (stackable spine) |
| Path locks | **Livelier** requires **Live Wire** only |
| Oddity / Contraband grid steal | Out of scope unless later parity pass |
| Shared vanilla staples (Grenade Belt, Penny line, Boundary Incursion) | Optional parity later — **not** required in custom ~30 |
| v1 cross-mod | **None** |
| Vanilla Shock Grenade | **Unmodified** |

---

## 8. Full Upgrade Table

IDs are design placeholders (implementation range: **gear 92500**, **upgrades 92501–92530** — avoid template `920xx`, Friend `921xx`, Honey `922xx`, Splash `923xx`, Caustic `924xx`).

Rarity key: **S** Standard · **R** Rare · **E** Epic · **X** Exotic  

Stack: **✓** CanStack · **—** unique  

Well: primary gravity well (still mixable)

### 8.1 Standards (~9) — stat spine

| # | Name | Well | Stack | Intent | Rough numbers (v0) |
|---|------|------|-------|--------|---------------------|
| 1 | **Wide Arc** | Storm | ✓ | Explosion (+ Wire pulse / strike) radius | +15–30% radius · **no CD tax** |
| 2 | **High Voltage** | Storm | ✓ | Shock effect amount on boom (pulses/strikes slightly) | +20–40% effect amount |
| 3 | **Quick Cell** | Capacitor | ✓ | Faster recharge | −12–20% recharge · pure CDR |
| 4 | **Hard Cell** | Storm | ✓ | Impact / pulse damage | +12–22% boom damage |
| 5 | **Insulated Gloves** | Capacitor | ✓ | Less self Shock application from your cell | −25–40% self effect amount |
| 6 | **Fastball** | Wire | ✓ | Throw force up; less gravity feel | Mobility of the *throw* |
| 7 | **Twin Cell** | Capacitor | ✓ | +1 max charge | +1 charge; **explicit mild +15–25% CD** on this card only |
| 8 | **Marathon** | Wire | ✓ | Move faster while grenade is recharging | Vanilla fantasy kept |
| 9 | **Static Charge** | Wire | ✓ | Recharge faster while sliding | Vanilla fantasy kept |

### 8.2 Rares (~10)

| # | Name | Well | Stack | Intent | Notes |
|---|------|------|-------|--------|-------|
| 10 | **Static Sheath** | Capacitor | ✓ | Boom grants **small overshield** to players in radius | Polymer-scale *entry* card; model B boom source |
| 11 | **Ground Path** | Capacitor | ✓ | **Live Wire pulses** also grant small overshield (self; optional tiny ally radius) | **Model B Wire source** without needing Faraday |
| 12 | **Dielectric Cap** | Capacitor | ✓ | +overshield grant amount and/or +hard cap / slower decay from your Cell OS | Stacks within hard cap |
| 13 | **System Overload** | Wire | ✓ | Players hit by boom (and Wire pulses if easy) gain short movespeed | Vanilla fantasy kept |
| 14 | **Shock Funnel** | Storm | ✓ | Gain grenade charge when you electrocute a target with another weapon | Vanilla fantasy kept |
| 15 | **Stun** | Storm | ✓ | Boom / pulses stun enemies briefly | Vanilla fantasy kept |
| 16 | **Phase Housing** | Storm | ✓ | Projectiles / explosions phase through shields | Vanilla fantasy kept |
| 17 | **Excited Plasma** | Storm | — | If explosion hits an **ignited** target, create a second damaging explosion | Vanilla fantasy kept; unique proc |
| 18 | **Storm Relay** | Storm | ✓ | Improves Flash Storm interval and/or strike damage/radius | Stronger with Flash Storm; mild value alone optional |
| 19 | **Emergency Evac** | Wire | ✓ | At critical health, throwing partially recharges movement ability | Vanilla fantasy kept |

### 8.3 Epics (~7)

| # | Name | Well | Stack | Intent | Notes |
|---|------|------|-------|--------|-------|
| 20 | **Lightspeed Material Transfer** | Wire | — | Teleport to your grenade when it lands | **Moved from Acid** |
| 21 | **Livelier** | Wire | — | While Live Wire active: even faster, can’t stop sprinting | **Requires Live Wire** |
| 22 | **Cloud Skip** | Wire | — | Gain a small double jump | Vanilla fantasy kept |
| 23 | **Launch Charge** | Wire | — | Grenades launch you straight up when hitting you | Vanilla fantasy kept |
| 24 | **Thunder Pressure** | Wire | — | Grenades launch you away from the explosion | Vanilla fantasy kept |
| 25 | **Electromagnet** | Storm | — | Grenades magnetize toward enemies | Vanilla fantasy kept |
| 26 | **Leyden Gate** | Capacitor | — | Boom grants a **solid mid-tier overshield** to players in radius; defines non-exotic Capacitor spike | Team-capable; stacks with Sheath/Faraday within hard cap |

### 8.4 Exotics (4)

| # | Name | Well | Stack | Intent |
|---|------|------|-------|--------|
| 27 | **Live Wire** | Wire | — | You become the grenade: multi-explode over a duration, gain movespeed. Pulses feed model B OS if Capacitor cards equipped |
| 28 | **Faraday Reservoir** | Capacitor | — | Boom (and Wire pulses) grant **large overshield** to players in radius — defines the Capacitor well |
| 29 | **Flash Storm** | Storm | — | Grenades fire lightning strikes downward as they fly |
| 30 | **Illegal Pocket Cell** | Economy | — | Tiny weak boom, absurd recharge — multi-well spam enabler |

*Count = 30 on the nose. Stretch: merge Storm Relay into Flash Storm stats if board feels tight; or split Dielectric Cap if Capacitor needs another lever.*

---

## 9. Exotic Deep-Dives

### 9.1 Live Wire (Wire)

**Fantasy:** You *are* the cell — a sprinting detonation schedule.

**Behaviour sketch:**

- On trigger (match vanilla Live Wire activation feel): enter Live Wire state for duration D.  
- While active: every interval I, explode at player with Shock damage/apply; gain movespeed S.  
- Livelier amplifies speed and locks sprint.  
- **Model B:** if Static Sheath / Ground Path / Leyden / Faraday contribute Wire-pulse OS flags/amounts, each pulse grants overshield (self-first; ally radius from Faraday/Leyden team rules).  
- Visual: crackling body VFX, pulse boom cues (placeholder OK).

**Mix notes:**  
Pocket + Wire = many pulses, many OS ticks if Capacitor invested — **celebrated peak**.  
Lightspeed + Wire = blink in, become the storm.  
Launch / Thunder + Wire = movement tech during pulses.

**Risk budget:**  
Pulse damage must not delete the user; Insulated Gloves matters. OS from pulses must respect hard cap so Pocket+Wire+Faraday is strong, not immortal.

### 9.2 Faraday Reservoir (Capacitor)

**Fantasy:** The cell is a portable substation — impact fills the blue bar.

**Behaviour sketch:**

- On detonate: apply **large overshield** to alive players in radius (including thrower).  
- On Live Wire pulse (model B): apply **large (or slightly reduced vs boom)** overshield at pulse origin radius.  
- Amount and radius higher than Leyden Gate / Static Sheath; Dielectric Cap still stacks within **hard OS cap**.  
- Decay readable; no base-HP heal.  
- Visual: blue flash / capacitor ring on grant.

**Mix notes:**  
Sheath + Leyden + Faraday = one OS system with stacked amounts, not three independent gods — enforce single cap at apply time.  
Pocket + Faraday = frequent small booms still dump meaningful OS if exotic amounts are tuned for “per proc” with cap.  
Wire + Faraday without Pocket = fewer, fatter pulse grants during Wire window.

**Risk budget:**  
Hard cap from this gear (~80–120 v0). Decay mandatory. No DR hybrid on this gear. Co-op team coat is allowed but cap is per-target from your grants.

### 9.3 Flash Storm (Storm)

**Fantasy:** The throw draws a lightning curtain on the way in.

**Behaviour sketch:**

- While projectile airborne: strikes downward on interval (vanilla Flash Storm).  
- Each strike: Shock damage + apply, small radius; Wide Arc / High Voltage / Storm Relay / Hard Cell scale.  
- Independent of Live Wire.  
- Visual: sky-to-ground bolts along flight path.

**Mix notes:**  
Electromagnet pulls the cell through packs while storms hit. Stun on boom finishes control. Pocket shortens time between throws so storm weather is frequent. Excited Plasma is fire-cross spice, not required.

### 9.4 Illegal Pocket Cell (Economy)

**Fantasy:** This one shouldn’t be legal — and that’s the point.

**Behaviour sketch:**

- While equipped: massive recharge acceleration; heavy damage and radius penalties (vanilla Pocket ballpark).  
- Does **not** disable other exotics.  
- Multi-charge / Twin Cell: still respects max charges; Pocket makes refills fast, not infinite parallel gods without charge count.  
- Description must show the trade loudly.

**Mix notes:**  
Peak poster: Pocket + Live Wire (+ optional Faraday/Ground Path).  
Peer posters: Pocket + Faraday; Pocket + Flash Storm.  
Storm/Capacitor without Pocket must still function via Quick Cell / Funnel / Static Charge / Twin Cell.

### 9.5 Exotic coexistence

| Pair | Rule |
|------|------|
| Live Wire + Faraday | **Encouraged.** Model B peak. |
| Live Wire + Pocket | **Encouraged.** Celebrated peak. |
| Faraday + Pocket | **Encouraged.** Capacitor spam peer build. |
| Flash Storm + any | Allowed. Air strikes + mode exotic. |
| All four | Allowed if grid fits; power via OS cap, Pocket damage tax, Wire duration. |
| Footprints | All four exotics **same cell count**, larger than typical rares/epics. |
| Sheath + Leyden + Faraday | One OS pipeline; stacked amounts; one hard cap. |
| Livelier without Live Wire | No-op / requires flag. |

---

## 10. Named Kit — Detailed Specs

### Live Wire (Exotic)

- Enables body-boom state; duration, interval, speed from rolls.  
- Unique.  
- Pulses are first-class damage events (Shock) and first-class **model B OS hooks**.

### Faraday Reservoir (Exotic)

- See §9.2. Large OS on boom + Wire pulses.  
- Unique. Team-capable.

### Flash Storm (Exotic)

- See §9.3. Airborne strike loop.  
- Unique.

### Illegal Pocket Cell (Exotic)

- See §9.4. CD / size / damage trade.  
- Unique. (Stacking: vanilla Pocket was exotic unique; keep unique unless parity demands stacks later.)

### Lightspeed Material Transfer (Epic)

- Teleport owner to land/detonate point.  
- Unique. No OS grant by default (can pair with boom OS naturally after blink).

### Livelier (Epic)

- Gate: Live Wire equipped / active rules.  
- Extra speed; force sprint while Wire active.  
- Unique.

### Cloud Skip (Epic)

- Grants small double jump force while equipped.  
- Unique.

### Launch Charge (Epic)

- On self-hit by your explosion: upward launch force.  
- Unique.

### Thunder Pressure (Epic)

- On self-hit / explosion interaction: launch away from epicenter.  
- Unique.  
- Compatible with Launch Charge (vertical + lateral tech); tune so they don’t cancel into mush.

### Electromagnet (Epic)

- Projectile seeks enemies (vanilla magnetize).  
- Unique.

### Leyden Gate (Epic)

- On detonate: mid-tier OS to players in radius ( > Sheath, < Faraday ).  
- Unique.  
- Wire pulse participation: **yes** at reduced or full mid-tier (prefer yes — model B consistency).

### Static Sheath (Rare)

- On detonate: small OS to players in radius.  
- Stackable: amount / radius within caps.  
- Wire pulses: small self OS if Ground Path not present? **Prefer:** Sheath is boom-primary; Ground Path is pulse-primary; Faraday/Leyden do both. Keeps rare roles readable.

### Ground Path (Rare)

- While Live Wire active: each pulse grants small self OS (optional tiny ally radius).  
- Stackable amount.  
- Without Live Wire: no-op (acceptable rare gravity).

### Dielectric Cap (Rare)

- Multiplies OS grant amounts and/or raises hard cap and/or slows decay for **your** Cell overshield.  
- Stackable within global hard cap rules.

### System Overload (Rare)

- Players damaged/healed-by-boom in radius gain movespeed buff short D.  
- Prefer include Wire pulse player hits if cheap.

### Shock Funnel (Rare)

- On electrocute apply from **other weapons** (not optional self-cell only): AddCharge scaled by rolls.  
- ICD optional if funnel + Pocket becomes absurd (tune, don’t delete).

### Stun (Rare)

- Boom applies short stun to enemies.  
- Wire pulses may inherit at reduced duration.

### Phase Housing (Rare)

- Explosions / projectile ignore enemy shields (vanilla phase).  

### Excited Plasma (Rare)

- If primary boom hits ignited target: second explosion instance.  
- Unique proc (**—**). Default: does **not** re-trigger boom overshield grants.


### Storm Relay (Rare)

- Reduces Flash Storm interval; +strike damage or radius.  
- Mild benefit if Flash Storm unequipped optional (e.g. tiny bonus strike on boom — stretch). Default: scales Flash Storm only.

### Emergency Evac (Rare)

- When health ≤ critical threshold, on throw: recharge movement ability by fraction.  
- Unique or stack duration/fraction — prefer stackable mild.

### Marathon / Static Charge / Fastball / standards

- Stat spines as table; no hidden OS or Wire enables.

---

## 11. Synergy Notes (Player-Facing, Soft Only)

No mod dependencies. Loadout tips for README / codex blurb.

| Partner | Why it feels good |
|---------|-------------------|
| Splash Canister | Splash wets; Cell electrocutes — vanilla wet+shock amp. Different sustain verbs (chip shell vs capacitor dump). |
| Caustic Flask | DR plating + blue OS is strong in theory — both throw-gated; OK as co-op/loadout skill expression, not a shared API. |
| Honey Jar | No shared heal identity; pure damage/status co-op. |
| Shock weapons / Arc Lightning / DMLR voltaic toys | Shock Funnel + High Voltage payoffs. |
| Movement-heavy employees | Lightspeed, Cloud Skip, Launch, Thunder, Wire. |
| Ignite kits / Incendiary | Excited Plasma cross-spice. |
| Shielded enemies | Phase Housing. |

**Explicit non-goals v1:** patching vanilla Shock; implementing Connected Systems; HoT nectar; timed % DR as Cell identity; Splash regenerating shell clone; cross-mod OS sharing APIs.

---

## 12. Strengths, Weaknesses & Failure Modes

### Strengths

- Clear second pillar (overshield) so the kit is not only Pocket+Wire  
- Pocket+Wire remains a celebrated peak **and** Capacitor/Storm are peer-complete  
- Lightspeed finally sits on the mobility grenade  
- Model B makes Wire and Capacitor friends  
- Distinct vs Honey HoT / Splash chip shell / Caustic DR  
- Speed fantasy preserved and expanded  

### Weaknesses

- Stock throw is intentionally bland  
- Capacitor / Wire / Storm unlock cards are dead stats until found  
- Overshield is proactive (throw first) — panic HoT is Honey’s job  
- Pocket tax means Pocket boards are weak per-boom  
- Mobility tech has a learning curve (Launch + Thunder + Lightspeed)  

### Failure modes to avoid in tuning

| Failure | Mitigation |
|---------|------------|
| Permanent blue-god | Hard OS cap, decay, grant needs throws/pulses |
| Pocket+Wire+Faraday immortal | Cap + Pocket damage tax + pulse OS tuned under cap |
| Capacitor dead without Wire | Leyden + Sheath + Faraday boom path fully playable alone |
| Storm dead without Wire | Flash Storm + control rares complete without Wire |
| Only Pocket+Wire wins | Peer posters explicitly tuned (Faraday dive, Storm carpet) |
| OS steals Splash identity | No idle regen shell; dumps are event-based |
| OS steals Caustic identity | No % DR plating on this gear |
| OS steals Honey identity | No base-HP HoT |
| CD tax culture | Review every property for accidental rechargeDuration writes |
| Livelier without Wire | Requirement flag |
| Excited Plasma / Stun double-dipping absurdity | ICD / unique flags |
| 30 upgrades but 1 forced build | Standards universal; minimal path locks |
| Team OS wins co-op alone | Per-target cap; decay; no DR hybrid |

---

## 13. Implementation Appendix (For Later — Not This Pass)

Design-only milestone: **this document**. When coding starts, prefer:

| Piece | Approach |
|-------|----------|
| Registration | Existing grenade template `GrenadeRegistration` clone path; set `GunData.damageEffect = Shock` |
| Name / IDs | Display **Voltaic Cell**; `APIName` `voltaic_cell`; gear id **92500**; upgrades **92501–92530** |
| Data host | `VoltaicCellBehaviour` (rename from example) with `Data` struct for flags/scalars |
| Detonate | Harmony on `GrenadeBullet.Detonate` (template / FriendinaBox style) |
| Overshield | Negative-heal / overhealth pattern (`HealTarget(..., -amount, pos)`) + hard cap tracker component |
| Live Wire | Port vanilla Voltaic/Shock Live Wire state machine (search decompile `VoltaicGrenade` / Live Wire upgrade flags) |
| Lightspeed | Port Acid Lightspeed teleport-on-land |
| Flash Storm | Port airborne strike loop from vanilla Shock |
| Pocket | Port Illegal Pocket damage/radius/CD modifiers |
| Launch / Thunder / Cloud Skip | Port vanilla movement force hooks |
| Upgrades | `PlayerData.CreateUpgrade` + `UpgradeProperty` Apply/Remove restoring prefab snapshot |
| Mod flags | `[MycoMod(..., ModFlags.IsSandbox)]` |
| Vanilla Shock | **Do not patch** |
| Cross-mod | None in v1 |

### Suggested `VoltaicCellBehaviour.Data` fields (sketch)

```
// Baseline / scales
float explosionRadiusMultiplier;
float shockEffectAmountMultiplier;
float boomDamageMultiplier;
float selfShockMultiplier;

// Live Wire
bool liveWire;
float liveWireDuration;
float liveWireInterval;
float liveWireSpeed;
float livelierSpeed;          // 0 if unequipped
bool livelierForceSprint;

// Overshield (model B)
float overshieldOnBoom;       // Static Sheath / Leyden / Faraday contribute
float overshieldOnWirePulse;  // Ground Path / Leyden / Faraday contribute
float overshieldRadiusMult;
float overshieldDecayMult;
float overshieldHardCap;      // enforce
bool faradayReservoir;
bool leydenGate;

// Lightspeed
bool lightspeedTeleport;

// Flash Storm
bool flashStorm;
float flashStormInterval;
float flashStormDamageMult;
float stormRelayIntervalMult;

// Pocket
bool illegalPocket;
float pocketRechargeMult;
float pocketRadiusMult;
float pocketDamageMult;

// Movement toys
float launchChargeForce;
float thunderPressureForce;
float cloudSkipForce;
float systemOverloadSpeed;
float systemOverloadDuration;
float marathonSpeed;
float staticChargeRechargeMult;
float emergencyEvacMoveRecharge;
float criticalHealthThreshold;

// Storm control
float stunDuration;
bool electromagnet;
bool phaseHousing;
bool excitedPlasma;
float shockFunnelCharge;
float outgoingShockDamageMult;  // High Voltage analogue if separate from effect amount
```

### Ship cut vs stretch

**v1 must-ship (fantasy complete):**

- Baseline bland shock boom  
- All 4 exotics  
- Lightspeed, Livelier, Cloud Skip, Launch Charge, Thunder Pressure, Electromagnet, Leyden Gate  
- Static Sheath, Ground Path, Dielectric Cap, Shock Funnel, System Overload  
- Full standard spine  
- Overshield model B + hard cap + decay  
- Pocket+Wire peak preserved; Capacitor-only and Storm-only boards playable  

**Stretch / post-v1:**

- Storm Relay juiciness without Flash Storm  
- Ally radius fine-tuning on Wire pulse OS  
- Shared staple parity (Belt, Penny line)  
- Custom mesh / Wwise  
- Config toggles for OS cap / Pocket numbers  

---

## 14. Naming & Presentation

| Slot | Value |
|------|--------|
| Display name | **Voltaic Cell** |
| Internal / API | `voltaic_cell` |
| Design nickname | Shock Grenade Rework (notes / folder only) |
| Short description | *Shock-element grenade. Stock throw is a clean lightning boom. Upgrades unlock live-wire body storms, capacitor overshields, aerial flash storms, and illegal pocket economics — speed and blue bar.* |
| Thunderstore name (later) | `VoltaicCell` |
| GUID (later) | `sparroh.voltaiccell` |
| Folder today | `.new.ShockGrenadeRework` (rename optional at ship) |

### Name map (vanilla → Cell)

| Vanilla-ish beat | Cell name | Notes |
|------------------|-----------|--------|
| Shock Grenade (gear) | **Voltaic Cell** | Ship name |
| Live Wire | **Live Wire** | Keep — perfect |
| Illegal Pocket Grenade | **Illegal Pocket Cell** | Brand alignment |
| Flash Storm | **Flash Storm** | Keep |
| Lightspeed Material Transfer | **Lightspeed Material Transfer** | Keep fantasy name (or **Lightspeed Transfer**) |
| Livelier | **Livelier** | Keep |
| Polymer-like OS entry | **Static Sheath** | New |
| OS exotic | **Faraday Reservoir** | New |
| OS epic | **Leyden Gate** | New |
| Wire-pulse OS rare | **Ground Path** | New |
| OS scaling rare | **Dielectric Cap** | New |
| Engorge | **Wide Arc** | Rename OK |
| Higher Voltage | **High Voltage** | Slight rename |
| Cooling / CDR | **Quick Cell** | New spine name |
| Insulated Gloves | **Insulated Gloves** | Keep |
| Others | As table | Free to rename in polish |

### SAXON marketing blurb (draft)

> SAXON Voltaic Cell — Portable lightning storage for employees who treat OSHA like a suggestion.  
> Baseline: shock. Aftermarket: become the grenade, bank a blue capacitor, call weather on the way in, or pocket a cell that legal still pretends not to see.  
> Not a juice box. Not armor spray. If it is blue and temporary, you are holding the right product.  
> “If you’re still standing still after the boom, the cell is not the problem.”

---

## 15. Open Questions (Balance / Feel — Not Blocking Doc)

1. Live Wire activation trigger: exact vanilla parity vs “on detonate only” — prefer **vanilla parity** when decompile is checked.  
2. OS decay: linear bleed vs delayed then dump — prefer **short hold then bleed** for readable diving.  
3. Faraday Wire-pulse amount: full boom amount vs ~50–70% per pulse (Pocket+Wire balance lever).  
4. Team OS default radius: full team always on Faraday/Leyden/Sheath, or Sheath self-biased? (Default: **all team-capable**.)  
5. Thunder Pressure + Launch Charge simultaneous vector resolution.  
6. Exact hex shapes for 30 upgrades — author during implementation.  
7. Shock Funnel + Pocket ICD needs.  
8. Excited Plasma second explosion inheritance of OS grants (default: **boom OS once per throw**, not on plasma echo).  
9. Ship rename of Lightspeed string length in UI.  

---

## 16. Design Checklist

- [x] Separate gear (not in-place vanilla patch)  
- [x] Name: Voltaic Cell  
- [x] Bland baseline (no Wire, OS, teleport, storm, pocket)  
- [x] Overshield = upgrade-only blue overhealth  
- [x] Overshield model B: boom **and** Live Wire pulses  
- [x] Not HoT / not % DR / not Splash regen shell  
- [x] Lightspeed → Cell (from Acid)  
- [x] Exotics: Live Wire, Faraday Reservoir, Flash Storm, Illegal Pocket Cell  
- [x] Pocket+Wire celebrated peak; Capacitor & Storm peer-complete  
- [x] Names free to change; key fantasies retained  
- [x] ~30 upgrades, 4 equal large exotics  
- [x] Gravity wells mix/match  
- [x] Self-contained v1  
- [x] Implementation deferred  

---

## 17. Changelog (Design Doc)

| Date | Change |
|------|--------|
| 2026-06-08 | Initial design doc from vanilla Shock wiki upgrades, Lightspeed (Acid) move, sibling docs (Caustic Flask, Honey Jar, Splash Canister), and user locks: ship name Voltaic Cell, separate gear, overshield model B (boom + Live Wire pulses), exotics A (Live Wire + Faraday Reservoir + Flash Storm + Illegal Pocket), Pocket+Wire remains celebrated peak with peer-complete Capacitor/Storm, names free to change. |

---

*End of design document. Next step when ready: rename template identifiers to Voltaic Cell and implement baseline shock boom only, then layer Live Wire → Capacitor OS → Flash Storm / Pocket → Lightspeed and movement epics.*
