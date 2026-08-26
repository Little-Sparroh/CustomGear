# Rhythm Stitchers — Design Document
# Mycopunk custom primary (mod: sparroh.rhythmstitchers)
# Status: Design bible v1 — pre-implementation
# Locked decisions: 2026-08-06
#
#   1. Ship name ............. Rhythm Stitchers
#   2. Fire mode ............. High-RoF semi-auto per channel (not full-auto hose)
#   3. Input ................. LMB = Left stitcher, RMB = Right stitcher, no ADS
#   4. Magazines ............. Independent L/R mags
#   5. Cross Sweep / Desync .. Exotic keystones (not free baseline power)
#   6. Element ............... New custom Sonic element (systems work required)
#   7. Doc depth ............. Full ~30 upgrade catalog + tuning bands
#   8. Paths ................. Metronome / Cross Sweep / Desync + glue (mix-and-match)
#
# Working folder: .new.RhythmStitchers (weapon template scaffold)

================================================================================
1. HIGH CONCEPT / FANTASY
================================================================================

SAXON dual machine pistols that stitch the battlefield to a shared Tempo.
Two independent triggers. Two magazines. One beat.

Left channel (LMB) and Right channel (RMB) fire as high-rate semi machine
pistols — intentional notes, not a brain-off hose. Land stitches on the
metronome for harmony crumbs. Equip the grid to Sweep both guns into sonic
waves, or Desync the channels so discordant tones crash into enemies and
stun them open.

Upgrades decide the performance:

  Metronome    — ride the Tempo; on-beat mults, measure finishers, quantize
  Cross Sweep  — angular dual-fire paints sonic waves across the room
  Desync       — phase offset and off-key crashes; stun surgeon / CC engine

One-liner:
  Two needles. One beat. Stitch, sweep, or tear the song apart.

Codex / gear select line (in-game style):

  Rhythm Stitchers
  Dual machine pistols. LMB left, RMB right. Independent mags. No aim-down-sights.
  High-rate semi fire locked to a shared Tempo. Sonic-native. Stitch on the beat.

SAXON marketing blurb:

  "Type-RS stereo ordnance does not accept a single trigger discipline.
   Authorize bilateral fire control per Form 9-BPM. Operators who cannot keep
   time will still make noise. Operators who can will make doctrine."
  — SAXON Field Ordnance, Acoustic Systems Desk

Optional stingers:
  "If you only pull one trigger, you brought half a gun."
  "Desync is not a malfunction. It is a feature request from the target."

================================================================================
2. ROLE IN THE ARSENAL
================================================================================

Slot:     Primary
API name: rhythm_stitchers           (provisional)
Gear ID:  93000                      (provisional — confirm free at implement)
Base clone (runtime): CartridgeSMG → rewritten GunData + RhythmStitchersBehaviour
                      until a real dual-pistol prefab ships

Job:
  - Close–mid dual-wield pressure with beat literacy skill expression
  - Independent stereo fire (true two-button primary — unique in arsenal)
  - Sonic element application + optional stun/CC via Desync path
  - Room control via Cross Sweep waves
  - Optional full-auto / quantize / polyrhythm rebuilds on the grid

What it is NOT:
  - Not Cycler / Heat Cycler (full-auto hose / heat magazine)
  - Not Lead Flinger (single-trigger slug tempo revolver fantasy)
  - Not DMLR (mode switch LMB/RMB; anatomy transfer)
  - Not Jackrabbit (bounce shotgun)
  - Not AMR (deliberate kinetic bolt deletion)
  - Not a pure aim-down-sights precision primary
  - Not free stun / free waves on stock kit

Open niche it fills:
  The arsenal’s only true dual-trigger primary — musical tempo, stereo channels,
  sonic waves, and intentional discord. No vanilla akimbo exists.

--------------------------------------------------------------------------------
2.1 Comparison snapshot
--------------------------------------------------------------------------------

  Weapon              Niche                         Rhythm Stitchers differentiator
  ------------------  ----------------------------  --------------------------------
  Cycler / Heat       Volume SMG / heat uptime      Semi dual-channel; beat skill
  Lead Flinger        Single-trigger slug tempo     Two guns, stereo, Sonic, waves
  DMLR / Marksman     Dual-mode LMB/RMB roles       Both buttons fire (not modes)
  Jackrabbit          Bounce fire shells            Hitscan/projectile stitches + Tempo
  Trident S2          Multi-pellet auto rifle       Independent triggers + mags
  Street Sweeper      Wide short-range burst        Sweep is motion-wave, not pellet cone
  Helminth Receiver   Vitality / bond organism      Tempo/Sonic performance fantasy
  Photon Disc / DW    Disc waves on grenade path    Gun-native sweep waves + dual fire

================================================================================
3. CORE MECHANICS (SACRED)
================================================================================

These define the gun. Upgrades may bend them; they should not casually erase
them without a clear cost and a readable fantasy shift.

  1. Two channels: LMB = Left stitcher, RMB = Right stitcher.
  2. No ADS (canAim = false). Aim input is Right fire, never zoom.
  3. High-RoF semi-auto per channel at baseline (mash ceiling ≠ optimal play).
  4. Independent magazines and (conceptually) independent fire clocks.
  5. Shared master Tempo (BPM + phase) always runs while equipped/active.
  6. Light on-beat crumb on baseline hits near phase — teaching tool only.
  7. Cross Sweep power is upgrade-gated (Exotic keystone + supports).
  8. Desync stun/crash power is upgrade-gated (Exotic keystone + supports).
  9. Sonic is the native element identity (custom element — see §4).
 10. Reload never mode-toggles; R is ammo only (both or priority — see §3.4).
 11. Both held = both may fire (true akimbo). No "one mode wins" unless an
     upgrade explicitly steals priority (rare; document if added).

--------------------------------------------------------------------------------
3.1 Dual input model (LOCKED)
--------------------------------------------------------------------------------

Input polling pattern (impl DNA: DMLR rework):

  fireHeld = PlayerInput.Controls.Player.Fire.IsPressed()   // LMB → Left
  aimHeld  = PlayerInput.Controls.Player.Aim.IsPressed()    // RMB → Right

  canAim = false
  HandleAim / ADS FOV skipped
  Aim action still tracked every frame for Right channel

Baseline fire rules:

  - Left fires on Fire press edges (semi) if L mag > 0 and L fire interval ready
  - Right fires on Aim press edges (semi) if R mag > 0 and R fire interval ready
  - Each channel has its own fire-interval timer
  - Holding a button does NOT full-auto at baseline
  - Re-click required per shot (high RoF cap when mashed)
  - Optional tiny input buffer (~30–50ms) so rhythmic play feels fair on high Hz

Both held:
  - Both channels attempt to fire on their own semi cadence
  - Unison (same beat window): Harmony opportunity
  - Alternate clicking: gallop / polyrhythm feel

What baseline does NOT do:
  - No mode swap (DMLR Hot Swap is a different gun)
  - No laser/alt weapon on RMB
  - No force-fire when empty (dry click per empty channel)

--------------------------------------------------------------------------------
3.2 Fire mode — high-RoF semi (LOCKED)
--------------------------------------------------------------------------------

Why semi:
  Beat literacy needs intentional shots. Full-auto at SMG rates turns on-beat
  into noise. Dual triggers only read as stereo performance if each pull is a
  note. Independent mags and Desync phase want punctuation, not a continuous
  tick hose.

Why high RoF:
  Still machine pistols — not slow revolvers. Players can mash near-auto rates;
  skilled players click with Tempo for crumbs and path payoffs.

  Stat                         Target band                 Intent
  ---------------------------  --------------------------  --------------------------
  Damage / stitch (per gun)    14 – 20                     Low–mid; volume + riders
  Element                      Sonic (native buildup)      Custom element
  Fire interval / channel      0.08 – 0.11 s               ~550–750 RPM mash ceiling
  Automatic (baseline)         0 (semi)                    Full-auto is upgrade-owned
  Bullets per shot / channel   1                           Upgrades may split
  L magazine size              12 – 16                     Independent
  R magazine size              12 – 16                     Independent (match L base)
  L + R reserve (each)         ~72 – 96                    Or shared reserve pool*
  Reload duration (per side)   1.0 – 1.3 s                 See §3.4
  Projectile speed             120 – 160                   Readable mid stitches
  Falloff start / end          18–24 / 36–48               Close–mid identity
  Max falloff mult             ~0.5 – 0.6                  Soft past mid
  Hit force                    modest                      Not AMR stagger
  Spread hip (per channel)     workable SMG-pistol         No ADS crutch
  Recoil                       light alternating kick      Stereo readable
  Master Tempo (BPM)           110 – 130 default           Config + upgrades
  On-beat window               ±40–70 ms                   Teaching crumb
  On-beat damage crumb         +6 – 10%                    Baseline only; paths amp

*Reserve ammo: prefer per-channel reserves for fantasy; shared reserve is
 acceptable if UI/network simpler — decide at implement. Magazines stay independent.

DPS philosophy:
  Unbuilt Stitchers should win at close–mid rhythmic dual-pressure and lose pure
  openers to AMR and pure hose clear to Cycler-class. Waves, stun locks, measure
  finishers, and auto-fire are earned on the grid.

Mash vs music:
  Max mash RoF is a ceiling, not the intended DPS peak without upgrades.
  On-beat play should roughly match or slightly beat brainless mash after
  accounting for crumb + reload efficiency; Metronome path widens that gap.

--------------------------------------------------------------------------------
3.3 Shared Tempo (LOCKED sacred)
--------------------------------------------------------------------------------

While the gear is active/equipped (owner):

  - Master clock runs at BPM (phase 0..1 each beat)
  - Audio metronome (subtle; mix under combat; config volume)
  - Optional HUD pip / bar notch (see §12)
  - Each shot records channel, time, phase error vs master

On-beat (baseline):
  - If |shotPhase - beatCenter| <= onBeatWindow → crumb bonus damage
  - Soft audio confirmation (higher stitch tick)
  - Does NOT baseline stun, wave, or big mult

Measure (bar):
  - Default 4 beats = 1 measure (upgrade-adjustable)
  - Baseline: no big measure payoff (upgrade-owned)
  - Paths hang finishers, waves, and crashes on measure boundaries

BPM changes:
  - Upgrades may raise/lower BPM (faster beat = tighter windows or more measures)
  - Polyrhythm exotic may run L clock ≠ R clock vs master

--------------------------------------------------------------------------------
3.4 Independent magazines & reload (LOCKED mags; reload detail)
--------------------------------------------------------------------------------

Magazines:
  - LeftMag / RightMag separate integers
  - Left shot spends LeftMag only; Right spends RightMag only
  - One side empty → that channel dry-clicks; other channel still fires
  - "Playing on one lung" is a valid failure/recovery state

Reload input (baseline recommendation — LOCKED for doc):

  Tap R:
    - Begin reload on any empty or partial channel
    - Default: reload BOTH channels in a staggered dual-reload
      (e.g. L racks then R, or parallel with single animation gated time)
    - Total time ≈ 1.0–1.3s effective to full both from empty
    - Cannot fire a channel while that channel is reloading
    - Other channel may still fire if not reloading (if staggered allows)
      → prefer: during dual reload both locked briefly for readability
        UNLESS an upgrade grants one-hand reload

  Hold R:
    - Baseline: same as tap (no Residue-style hold spend)
    - Upgrades may steal hold-R (Encore dump, Drop the Beat, etc.)

Reload upgrades may add:
  - Priority reload empty side only (faster)
  - Reload-on-beat speed bonus
  - One channel free-fire while other reloads
  - Phantom Duet on reload complete

Dry channel feel:
  - Per-side dry click + HUD flash on that side’s ammo pip
  - Not a full weapon lock

--------------------------------------------------------------------------------
3.5 Baseline combat loop (no upgrades)
--------------------------------------------------------------------------------

  Hear Tempo → click L and/or R on beats → stitches apply light Sonic buildup
     ↘ mash when panicked (still works, weaker efficiency)
     ↘ one mag empty → keep stitching with the live hand → R when safe
     ↘ both held near same beat → tiny Harmony crumb (optional baseline micro-bonus)
     ↘ no waves, no stun crashes, no measure nukes

Skill without upgrades:
  dual-hand rhythm, ammo asymmetry management, positioning at mid range,
  not wasting both mags dry in the open.

--------------------------------------------------------------------------------
3.6 What baseline does NOT include
--------------------------------------------------------------------------------

  - No Cross Sweep waves
  - No Desync stun
  - No full-auto
  - No measure finisher
  - No Phantom Duet / echo guns
  - No polyrhythm (L BPM ≠ R BPM)
  - No strong Harmony mult (micro only if needed for juice)
  - No ally aura / team Tempo share (upgrade or backlog)

================================================================================
4. SONIC ELEMENT (CUSTOM — LOCKED IDENTITY)
================================================================================

Sonic is a new damage/status element. It is not "Shock with different VFX."
Implementation is a real systems project; design specifies desired behavior.

--------------------------------------------------------------------------------
4.1 Fantasy
--------------------------------------------------------------------------------

Pressure, vibration, standing waves in fungal tissue and armor cavities.
Saturate = structural resonance failure: stagger, brief stun, vulnerability
to further acoustic damage (waves, harmony crashes).

--------------------------------------------------------------------------------
4.2 Mechanical sketch (starting targets)
--------------------------------------------------------------------------------

  Buildup on stitch hit:     modest per bullet (dual-fire applies twice as fast
                             when both channels land)
  Saturate threshold:        comparable to Shock band (tune in playtest)
  On saturate:
    - Short stun/stagger (baseline element saturate — weaker than Desync keystone
      crashes; Desync path amps duration/quality)
    - Brief Sonic Vulnerability: +damage taken from Sonic sources and from
      Rhythm Stitchers waves/harmony (10–15%, short)
    - Refresh rules: standard element saturate cooldowns / DR patterns
  DoT while saturated (optional): low vibrating chip, not burn-melt
  Visual: cyan/violet shockwave rings, chromatic shimmer, ear-ring UI tick
  Audio: band-pass thump, glass-resonance crack on saturate

--------------------------------------------------------------------------------
4.3 Cross-talk (backlog unless free)
--------------------------------------------------------------------------------

  Sonic × Shock  — conductive resonance (arc on saturate)
  Sonic × Fire   — overpressure pop (small radial on ignite+sonic)
  Sonic × Acid   — brittle shell (bonus shell damage while both active)

Primary ship identity does not require multi-element interlacing (unlike Heat
Cycler). Sonic alone must feel complete.

--------------------------------------------------------------------------------
4.4 Implementation staging
--------------------------------------------------------------------------------

  Stage A: Mod-local "Sonic" status via custom behaviour + damage flags / VFX
           that plays like an element on this gun only
  Stage B: True global element hook (enum, UI colors, enemy generic reactions,
           other weapons can apply Sonic if desired)
  Design doc assumes Stage B fantasy; ship may launch Stage A with same rules
  scoped to this gear.

  Risk: element work is larger than a normal weapon mod. Schedule separately
  from dual-input + Tempo spine if needed.

================================================================================
5. SHARED VOCABULARY
================================================================================

5.1 Channel (L / R)
  Which stitcher fired. Stereo pairs, independent mags, independent intervals.

5.2 Tempo / Beat / Phase
  Master BPM clock. Phase error grades on-beat vs off-beat.

5.3 Measure / Bar
  Every N beats (default 4). Bigger payoffs hang here when upgraded.

5.4 Harmony
  Both channels contributing within a short window (and/or both on-beat).
  Upgrade hooks: damage, Sonic appl, wave spawn, ammo crumb.

5.5 Sweep
  Angular aim/view motion while firing one or both channels.
  Cross Sweep keystone converts sweep into sonic waves.

5.6 Desync / Discord
  Phase offset between L and R fire events, or vs master Tempo.
  Desync keystone converts discord into stun crashes and vulnerability.

5.7 Resonance
  Stacks on enemies from rhythmic Sonic hits (path spenders amp stun/waves).

5.8 Encore
  End-of-mag, reload, or measure-end echo performances (Phantom Limb DNA).

5.9 Quantize
  Upgrade-owned assist that snaps fire timing toward beat (anti-skill floor
  relief for Metronome; must not delete semi identity entirely unless exotic).

5.10 Polyrhythm
  L and R operating on different subdivisions or BPMs (exotic transformer).

================================================================================
6. DESIGN PILLARS (NON-NEGOTIABLE)
================================================================================

  1. Dual triggers are real — both buttons shoot; neither is fake ADS.
  2. Semi-auto intentionality at baseline; hose is opt-in on the grid.
  3. Independent mags make stereo ammo a resource minigame.
  4. Tempo is always present; power verbs (Sweep / Desync / Measure) are earned.
  5. Sonic is the elemental identity — stun/wave fantasy hangs off it.
  6. Three peer paths + glue; mix-and-match; no exclusion matrix.
  7. On-hit / on-beat / stereo decisions > flat % damage stacking.
  8. Exotic modules are large, equal footprint, fantasy rebuilders.
  9. ~30 upgrades v1; fun legible hybrids over dry rails.
 10. Failure states stay fun (one lung dry, off-beat whiffs, bad sweep timing).
 11. Co-op readable: stuns/waves telegraphed; personal Tempo can stay local.
 12. Prefer diminishing flags on stun duration and wave spam so hybrids spice
     the room without permanent CC lock forever.

================================================================================
7. UPGRADE PHILOSOPHY
================================================================================

7.1 Fluid themes — not skill trees

  - Themes = gravity wells for fantasy and balance review
  - No prerequisite chains, no UI exclusion matrix
  - Hybrid grids are first-class
  - Grid space + rarity footprint + soft tensions are the real constraints
  - Multi-tags allowed

7.2 Soft tension pairs (interesting, not banned)

  Perfect Pitch quantize  ↔  raw Desync off-beat crashes
    Assist timing vs reward for playing dirty/late.

  Wall of Sound dual-hold  ↔  one-lung precision Desync
    Both guns blazing vs surgical single-channel opens.

  High BPM click track     ↔  slow heavy stitches (Double Action DNA)
    More beats vs fatter notes.

  Sweep clear waves        ↔  ST measure execute
    Room paint vs boss bar punctuation.

  Full-auto Autoloop       ↔  pure semi beat purity
    Hose comfort vs maximum crumb efficiency.

7.3 Universal truths (pool rules)

  - Target ~30 upgrades total for v1 ship pool
  - Support three distinct build lenses + hybrids
  - Exotic shapes larger than typical; all Exotics same cell count
  - Oddity grid-grow (Boundary Incursion) = 1-cell spatial, mission-stackable
  - Shared contraband (Edge Fault, Multiversal Thievery) optional parity later
  - Standards may CanStack where pure stats; identity keystones generally don’t
  - Descriptions short, mechanical, slightly SAXON-wry

7.4 Rarity & footprint guidance

  Rarity       Typical role                         Footprint family
  -----------  -----------------------------------  --------------------
  Standard     Glue stats, small economy            Small (~3 cells)
  Rare         Meaningful identity pieces           Medium / Line
  Epic         Build-defining                       Large / Wide
  Exotic       Fantasy rebuilders                   Exotic (shared large)
  Oddity       Grid manipulation                    Tiny (1 cell grow)

================================================================================
8. BUILD LENSES (FLUID THEMES)
================================================================================

--------------------------------------------------------------------------------
8.1 Metronome — ride the Tempo
--------------------------------------------------------------------------------

Fantasy:
  You are the click track. On-beat stitches hit harder, measures pay out,
  BPM becomes a weapon stat. Optional quantize for operators who want the
  gun to meet them halfway.

What you lean into:
  On-beat mults, measure finishers, BPM scale, streak counters, RoF-on-beat,
  reload-on-beat, light Harmony engine without needing Desync.

Keystones (strong pull):
  Perfect Pitch, Drop the Beat, Click-Track Capacitor

Supports:
  Downbeat, Measure Cut, Tempo Sync, Backbeat Reload, Counting Couplet,
  High BPM, Groove Armor

Natural hybrids:
  + Cross Sweep: waves that fire on measure or on-beat sweeps
  + Desync: stun windows opened on off-beat, executed on downbeat

Example lean:
  Perfect Pitch + Drop the Beat + Downbeat + Measure Cut + Tempo Sync
  → "I never miss the 1."

Success feel:
  The room thumps with you; damage spikes visibly on the beat; empty-brain
  mash clearly loses to timed dual clicks.

--------------------------------------------------------------------------------
8.2 Cross Sweep — sonic waves
--------------------------------------------------------------------------------

Fantasy:
  Paint the air. Sweep the pair (or a loaded channel) across a pack and
  launch concussive Sonic waves. Clear and zone control; Disc/Concussive
  Wave DNA with dual-wield authorship.

What you lean into:
  Angular motion detection, wave damage/count/width, dual-hold wave continuous
  (Wall of Sound), ground bass trails, pack clear.

Keystones (strong pull):
  Cross Sweep, Wall of Sound, Bass Drop

Supports:
  Wide Arc, Afterimage Wave, Sweep Fuel, Stereo Fan, Crest Rider,
  Horizontal Mandate

Natural hybrids:
  + Metronome: quantized wave pulses on beat
  + Desync: waves apply Discord / stun riders

Example lean:
  Cross Sweep + Wall of Sound + Wide Arc + Sweep Fuel + Crest Rider
  → "I don't aim heads; I aim brush strokes."

Success feel:
  Turning while firing both is the clear button; standing still single-target
  feels incomplete without other path cards.

--------------------------------------------------------------------------------
8.3 Desync — discord & stun
--------------------------------------------------------------------------------

Fantasy:
  The wrong notes hurt more. Phase offset between channels (or vs Tempo)
  builds Discord; crashes stun and crack Sonic vulnerability. Elite/boss
  CC and ST setup.

What you lean into:
  Off-beat rewards, L/R interval skew, stun on crash, Resonance stacks,
  anti-regroove (keep them locked), execute after stun.

Keystones (strong pull):
  Desync, Dissonant Crash, Polyrhythm

Supports:
  Late Hit, Phase Slip, Stun Flourish, Broken Metronome, One-Lung Cadence,
  Aftershock

Natural hybrids:
  + Metronome: open with discord stun → downbeat execute
  + Cross Sweep: stunned packs eat full wave follow-ups

Example lean:
  Desync + Dissonant Crash + Phase Slip + Stun Flourish + Late Hit
  → "If it can hear, it can stop."

Success feel:
  Dual-fire out of phase is intentional; stuns are fight-defining; pure
  on-beat Metronome without Desync cards feels like a different song.

--------------------------------------------------------------------------------
8.4 Cross-theme glue
--------------------------------------------------------------------------------

  Matched Spools, Needle Oil, Stereo Sights, Spare Clips, Boundary Incursion,
  handling/range staples, light Sonic appl glue

================================================================================
9. FULL UPGRADE CATALOG (~30 v1)
================================================================================

IDs: provisional 93001–93030 (gear 93000). Adjust at implement.
Theme tags: ME = Metronome, SW = Cross Sweep, DE = Desync, GL = Glue.
Numbers are STARTING TARGETS — validate in playtest. [HOT] = watch stacking.

Rarity guide: Standard / Rare / Epic / Exotic / Oddity
Cell rule: Exotic shapes larger than others; all Exotics same cell count.

--------------------------------------------------------------------------------
9.1 EXOTIC (6) — equal large footprint
--------------------------------------------------------------------------------

E1. Perfect Pitch                             id 93001    tags: ME
    Exotic keystone — Metronome
    On-beat window widened. On-beat damage crumb greatly increased.
    Light quantize assist: shots fired slightly early/late within a forgiveness
    band snap toward beat for crumb eligibility (does not full auto-fire).
    Starting targets:
      On-beat window      ×1.4 – 1.6
      On-beat damage      +22 – 30% (replaces/overrides tiny baseline crumb)
      Forgiveness snap     ±25–40 ms toward beat
    HUD beat pip recommended when equipped.

E2. Cross Sweep                               id 93002    tags: SW
    Exotic keystone — Cross Sweep (name locked to path fantasy)
    While firing at least one channel, horizontal aim angular rate above a
    threshold emits a Sonic wave along the sweep arc (cone/line hybrid).
    Dual-channel firing reduces heat/cooldown of wave spawn or increases
    wave damage.
    Starting targets:
      Min angular rate     tune so intentional sweeps proc, idle micro never
      Wave damage          ~55–75% of a single stitch × scaling
      Wave cooldown        0.35 – 0.55 s between waves
      Dual-fire bonus      +25–40% wave damage or −30% cooldown
      Element              Sonic buildup on wave hit
    Standing still and tapping should NOT farm waves.

E3. Desync                                    id 93003    tags: DE
    Exotic keystone — Desync (name locked to path fantasy)
    Tracks phase offset between recent L and R shots (and/or each vs master).
    When offset exceeds a Discord threshold, the next hit(s) trigger a
    Discordant Crash: bonus Sonic damage + short stun.
    Starting targets:
      Discord threshold    ~90–160 ms channel skew or opposite half-phase
      Crash damage         +35–50% on the crashing shot
      Stun                 0.45 – 0.75 s (DR/resist respectful; [HOT] w/ element)
      Builder              alternate-channel fire builds Discord faster than unison
    Unison on-beat Harmony play still works but is not this card’s peak.

E4. Phantom Duet                              id 93004    tags: ME, DE, GL
    Exotic transformer — Encore / Phantom Limb DNA
    On reload complete, phantom Left and Right stitchers materialize and
    replay a compressed echo of shots fired since the previous reload
    (or last N shots cap). Echoes deal reduced damage, apply reduced Sonic,
    and can proc on-beat/Desync at reduced rate.
    Starting targets:
      Echo damage          40–55% 
      Max echoed shots     8–12 per channel (cap)
      Echo fire rate       fast sequential burst over ~0.6–1.0 s
    Big reload fantasy; pairs with independent mag juggling.

E5. Polyrhythm                                id 93005    tags: DE, ME
    Exotic transformer — dual clock
    Left and Right run different subdivisions of master Tempo
    (e.g. L on quarter notes feel, R on triplets — implemented as separate
    interval mults and beat-window sets).
    Desync Discord builds passively from the inherent skew.
    Metronome crumbs can proc on each channel’s own grid.
    Starting targets:
      R interval mult      ×0.66 or ×1.5 vs L (rolled or fixed — pick at impl)
      Passive Discord      +small per second while both channels used
    Readable HUD: two phase pips or L/R color ticks.

E6. Wall of Sound                             id 93006    tags: SW, ME
    Exotic transformer — dual-hold identity
    While both channels are being actively fired (both buttons held and both
    mags live), emit a continuous low-grade Sonic pressure wave forward
    (weaker than Cross Sweep spikes but constant).
    Move speed slightly reduced while active; damage resistance small optional.
    Starting targets:
      Tick rate            every 0.25–0.35 s
      Tick damage          low (clear assist, not delete)
      Requires             both held + both mags > 0
    Cross Sweep spikes can still overlay for brush-stroke bursts.

--------------------------------------------------------------------------------
9.2 EPIC (8)
--------------------------------------------------------------------------------

P1. Drop the Beat                             id 93007    tags: ME
    On measure boundary (or when you empty either mag — pick primary: measure),
    next stitch or small nova deals bonus damage and Sonic.
    Alternate acceptable rule: emptying a mag triggers a local bass nova;
    measure triggers a personal damage buff for 1 beat.
    RECOMMENDED LOCK: measure-end empowers next Harmony window (both channels
    within 0.2s) for a big double-stitch chord.
    Chord damage         +40–60% on those 1–2 shots
    Sonic appl           +flat

P2. Bass Drop                                 id 93008    tags: SW
    Cross Sweep waves that hit 3+ targets gain a second ground bass pulse
    (Disc World wave-trail DNA, short range).
    Bonus vs packed rooms; weak in pure ST.
    Ground pulse damage  ~40–60% of parent wave
    Radius               3–5 m

P3. Dissonant Crash                           id 93009    tags: DE
    Discord crashes gain radius (small AoE stun/damage around primary target).
    Requires Desync keystone for full effect; without it, minor off-beat AoE
    crumb only (or hard-require — prefer soft so card isn’t dead).
    AoE radius           2.5–4 m
    AoE scale            50–70% of crash shot

P4. Click-Track Capacitor                     id 93010    tags: ME, GL
    Every successful on-beat hit grants a Cap stack (cap 6–10).
    At max stacks, next reload or next measure auto-spends for a forward
    Sonic beam/pulse (Dump Charge / Capacitor Dump DNA).
    Or: spend on demand with hold-R when at cap.
    RECOMMENDED: hold-R when Cap full → directed pulse ∝ stacks; clears stacks.
    Damage / stack       meaningful mid clear/ST hybrid

P5. Afterimage Wave                           id 93011    tags: SW
    Waves leave a brief resonant afterimage line that deals tick damage
    and applies light Sonic for 0.6–1.0 s.
    Encourages painting lanes.

P6. Stun Flourish                             id 93012    tags: DE, ME
    Hitting a stunned target with an on-beat stitch refunds 1 ammo to the
    channel that hit and deals bonus damage.
    Ties Desync opens to Metronome executes.
    Bonus damage         +20–30%
    Ammo refund          1 per flourish, ICD 0.2 s

P7. Autoloop                                  id 93013    tags: GL, ME
    Channels become full-auto while held at a controlled rate
    (interval ≈ baseline semi mash ceiling or slightly slower).
    On-beat crumb still applies to auto shots that land in window
    (Metronome still matters; pure hose without Pitch is mid).
    Fire interval auto   0.09–0.12 s
    Spread               +small while auto
    This is the "I want machine pistols that run" card.

P8. Encore Magazine                           id 93014    tags: ME, DE, GL
    The last 2 bullets in each mag gain bonus damage and guaranteed stronger
    Sonic application. If both channels’ last-2 windows overlap, mini-Harmony
    chord bonus.
    Last-n damage        +25–40%
    Pairs with Phantom Duet and independent mag desync empties.

--------------------------------------------------------------------------------
9.3 RARE (10)
--------------------------------------------------------------------------------

R1. Downbeat                                  id 93015    tags: ME
    First beat of each measure (the "1") grants a larger damage crumb
    than other on-beats.
    Downbeat mult        +15–25% additional on the 1

R2. Measure Cut                               id 93016    tags: ME, SW
    Measure length becomes 3 beats (waltz cut) OR 2 beats (half-time rush) —
    rolled on apply or fixed config. More frequent measure payoffs; tighter
    stamina on Drop the Beat style cards.
    Document choice at impl: prefer player-facing single mode (3-beat) for clarity.

R3. Wide Arc                                  id 93017    tags: SW
    Cross Sweep waves gain width/angle. Slightly less peak damage per target.
    Width                +30–50%
    Damage               ×0.9–0.95

R4. Sweep Fuel                                id 93018    tags: SW, GL
    Wave cooldown reduced. Small heat: increased spread for 0.3s after each wave.
    Cooldown             −20–30%

R5. Phase Slip                                id 93019    tags: DE
    After firing L, R’s next shot within 0.35s gains bonus Discord build
    (and vice versa). Teaches alternate-channel gallop.
    Discord bonus        significant toward crash threshold

R6. Late Hit                                  id 93020    tags: DE, ME
    Slightly late shots (just after beat) gain Desync-leaning bonus instead
    of failing crumb entirely — "behind the beat" groove.
    Late window          beatCenter → +80–110 ms
    Bonus                +12–18% damage OR Discord build
    Does not equal Perfect Pitch early snap; different feel.

R7. Backbeat Reload                           id 93021    tags: ME, GL
    Completing a reload near a beat greatly shortens reload or refunds
    1–2 ammo into each mag.
    Encourages rhythmic reload timing (Practice Makes Perfect DNA).

R8. One-Lung Cadence                          id 93022    tags: DE, GL
    While one channel is empty or reloading, the live channel gains fire
    interval improvement and damage.
    RoF                  +12–18%
    Damage               +10–15%
    Rewards independent mag asymmetry.

R9. Stereo Fan                                id 93023    tags: SW, GL
    When both channels fire within 0.1s, add +1 micro-pellet to each shot
    at wide horizontal bias (clear lean).
    Pellet damage        40–50% 
    Spread               horizontal bias

R10. Broken Metronome                         id 93024    tags: DE
    Master Tempo BPM randomly sways ±8–12% over time. Discord builds faster;
    pure Metronome windows jitter (soft tension with Perfect Pitch).
    For players who want chaos Desync identity.

--------------------------------------------------------------------------------
9.4 STANDARD (5)
--------------------------------------------------------------------------------

S1. Matched Spools                            id 93025    tags: GL
    Both magazines +size.
    Mag +2 – 4 per side
    Light CanStack if desired

S2. Needle Oil                                id 93026    tags: GL
    Both channels: slightly faster semi interval (lower interval).
    Interval             −6 – 10%
    Still semi.

S3. Spare Clips                               id 93027    tags: GL
    Reserve ammo increased for both channels.

S4. Sonic Reed                                id 93028    tags: GL, DE
    Baseline Sonic buildup per hit increased.
    Appl                 +15–25%

S5. Stereo Sights                             id 93029    tags: GL, ME
    Spread down, falloff start slightly improved. Pure handling glue.
    No ADS granted.

--------------------------------------------------------------------------------
9.5 ODDITY (1)
--------------------------------------------------------------------------------

O1. Boundary Incursion                        id 93030    tags: GL
    Adds a row or column to the upgrade grid.
    Vanilla-style GridGrow: IsSpatial | CanStackInMission, priority -100, 1 cell.

--------------------------------------------------------------------------------
9.6 v1 frozen pool checklist (30)
--------------------------------------------------------------------------------

  EXOTIC (6)
    1  Perfect Pitch
    2  Cross Sweep
    3  Desync
    4  Phantom Duet
    5  Polyrhythm
    6  Wall of Sound

  EPIC (8)
    7  Drop the Beat
    8  Bass Drop
    9  Dissonant Crash
    10 Click-Track Capacitor
    11 Afterimage Wave
    12 Stun Flourish
    13 Autoloop
    14 Encore Magazine

  RARE (10)
    15 Downbeat
    16 Measure Cut
    17 Wide Arc
    18 Sweep Fuel
    19 Phase Slip
    20 Late Hit
    21 Backbeat Reload
    22 One-Lung Cadence
    23 Stereo Fan
    24 Broken Metronome

  STANDARD (5)
    25 Matched Spools
    26 Needle Oil
    27 Spare Clips
    28 Sonic Reed
    29 Stereo Sights

  ODDITY (1)
    30 Boundary Incursion

--------------------------------------------------------------------------------
9.7 Backlog (designed vocabulary, not in first 30)
--------------------------------------------------------------------------------

  High BPM              — raise master Tempo; tighter windows, faster measures
  Groove Armor          — DR while continuously landing on-beats
  Counting Couplet      — every 2nd on-beat L-R pair refunds ammo
  Crest Rider           — damage bonus at apex of a sweep gesture
  Horizontal Mandate    — vertical aim delta ignored for sweep; only yaw counts
  Aftershock            — stun end pulses small Sonic nova
  Tempo Sync            — ally kills near you briefly widen your on-beat window
  Double Action Stitch  — longer trigger pull, huge single stitch (LF DNA)
  Twin Needles          — +1 pellet both channels, mag economy tax
  Silent Count          — hide metronome audio; visual only; +focus damage
  Opening Chorus        — first 2s of combat: free light quantize
  Fade Out              — swap/stow emits weak wave (tech swap play)
  Edge Fault / Thievery — contraband grid parity
  Sonic × Shock braid   — interlace card if global Sonic exists
  Measure Smash         — boss ST: measure finisher always targets last hurt part

================================================================================
10. EXAMPLE BUILDS (ILLUSTRATIVE ONLY)
================================================================================

Grid space is the real limit. Teaching sketches, not meta decrees.

  A. Pure-ish Metronome
     Perfect Pitch + Drop the Beat + Downbeat + Click-Track Capacitor
     + Backbeat Reload + Needle Oil + Stereo Sights
     → Click the 1; dump Cap on the chorus.

  B. Pure-ish Cross Sweep
     Cross Sweep + Wall of Sound + Bass Drop + Wide Arc
     + Sweep Fuel + Afterimage Wave + Stereo Fan
     → Brush-stroke clear; both triggers down.

  C. Pure-ish Desync
     Desync + Dissonant Crash + Polyrhythm + Phase Slip
     + Late Hit + Stun Flourish + Sonic Reed
     → Gallop off-phase; crash stun; flourish the downed beat.

  D. Hybrid — Stun then Sweep
     Desync + Cross Sweep + Stun Flourish + Bass Drop + Phase Slip
     → Open with discord CC; paint the stunned pack.

  E. Hybrid — Quantized Wall
     Perfect Pitch + Wall of Sound + Autoloop + Downbeat + Matched Spools
     → Auto hose that still pays on the beat; beginner-friendly stereo.

  F. Hybrid — Phantom Encore
     Phantom Duet + Encore Magazine + One-Lung Cadence + Backbeat Reload
     + Desync
     → Empty one side, thrash the other, reload encore double ghost volley.

  G. Hybrid — Boss Conductor
     Perfect Pitch + Desync + Drop the Beat + Stun Flourish + Click-Track Capacitor
     + Stereo Sights
     → Stun windows into measure chords into Cap execute.

Many other combinations are valid.

================================================================================
11. ECONOMY RULES OF THUMB
================================================================================

  - Independent mags: dual-hold burns ammo ~2×; reward intentional one-hand
    passages and One-Lung cards.
  - Semi mash ceiling should not fully obsolete on-beat play after Metronome
    investment; without Metronome, mash ≈ on-beat is OK.
  - Wave spawn must require real angular motion — no spin-bot infinite waves
    (cooldown + min angle + per-time angle budget).
  - Stun stacking [HOT]: Desync crash + Sonic saturate + Dissonant Crash AoE.
    Prefer: stun reapply DR, diminishing duration, elite resist curves.
  - Wall of Sound continuous tick must stay weak vs Cross Sweep spikes.
  - Autoloop + Perfect Pitch + Wall of Sound = comfort clear god; watch pack
    TTK vs intentional Cycler competitors.
  - Phantom Duet echo + Encore last-bullets can double-dip — cap echo damage
    and prevent echo from spawning full new Duet (no infinite ghost recursion).
  - Polyrhythm + Broken Metronome may make UI noisy; ensure crashes still readable.

Reload:
  - Dual reload baseline should feel like a short commitment, not Cycler vent.
  - Backbeat Reload is spice, not mandatory for uptime.

Safety / feel:
  - Never hard-lock both channels without audio/HUD reason
  - Dry one side is recoverable mid-fight
  - Metronome audio must have volume config and combat ducking

================================================================================
12. STRENGTHS, WEAKNESSES & PLAYER FANTASY CHECKLIST
================================================================================

Strengths
  - Unique dual-trigger primary fantasy (arsenal gap)
  - High skill expression (timing + two hands + ammo asymmetry)
  - Three peer identities + rich hybrids
  - CC (Desync) and clear (Sweep) both first-class
  - Sonic element gives long-term systemic hooks
  - Failure states readable (one lung, off-beat, no-sweep standing still)

Weaknesses
  - Finger/tempo demand higher than hose guns (Autoloop softens)
  - No ADS long-range identity
  - Independent mag mismanagement punishes
  - Custom element increases impl cost and MP surface
  - Not top pure single-shot deletion (AMR) or pure infinite hose (Heat Cycler)

Player fantasy checklist (success criteria)

  [ ] Unupgraded gun feels like two hungry semis with a heartbeat
  [ ] LMB/RMB clearly map to Left/Right with independent ammo
  [ ] On-beat crumb is audible/visible and optional, not mandatory grief
  [ ] Mash works in panic; timed dual-click feels better with Metronome
  [ ] Cross Sweep only sings when you actually sweep
  [ ] Desync stuns feel like intentional discord, not random CC proc
  [ ] One mag empty still lets you play (One-Lung fantasy)
  [ ] Autoloop run feels like a valid comfort build, not the only build
  [ ] Phantom Duet reload encore is a highlight moment
  [ ] Polyrhythm is readable within one fight
  [ ] Hybrids feel smart, not "wrong"
  [ ] Sonic saturate is distinct from Shock in VFX/feel
  [ ] Co-op: stuns/waves help allies without soft-locking bosses forever
  [ ] Exotic moments are fight-defining

================================================================================
13. VISUAL, AUDIO & THEMATIC DESIGN
================================================================================

13.1 Appearance

  Base:
    Matched pair of industrial SAXON machine pistols with exposed tuning-fork
    receivers, capillary glow that pulses to Tempo, stereo-linked mag wells.
    Fungal-biotech acoustic chambers — not "guns that are guitars," more
    "ordnance that learned meter."

  Metronome lean:
    Stronger barrel LED beat flash; gold/white click accents.

  Cross Sweep lean:
    Muzzle wakes leave ribbon trails; wave fronts visible as pressure crescents.

  Desync lean:
    L/R glow fall out of color phase (L cyan / R magenta split); crash =
    ugly interference flash on target.

  Current implementation note:
    Runtime clone will use Cartridge SMG mesh until AssetBundle dual-pistol art.
    Design assumes true akimbo presentation; interim may be single SMG body
    with stereo VFX/HUD lying convincingly until art.

13.2 Sound

  Master Tempo: soft click or filtered kick; config volume; ducks under big VFX
  L fire vs R fire: slightly different pitch/pan (stereo mandatory)
  On-beat: brighter transient layer
  Off-beat/Desync build: detuned layer creeping in
  Crash stun: dissonant cluster chord + short ringing mute on target
  Wave: whoosh + low brass hit
  Dry channel: one-sided click in pan
  Reload: dual rack sequence, mechanical + tuning fork settle
  Wall of Sound: continuous low octave bed while both held

13.3 HUD

  Required:
    - Dual ammo display (L | R) — non-negotiable with independent mags
    - Tempo phase pip or linear beat bar
  Optional / path:
    - On-beat flash
    - Discord meter (Desync)
    - Cap stacks (Click-Track Capacitor)
    - Sweep ready / wave cooldown
    - Polyrhythm L/R phase marks
    - Measure counter (1-2-3-4)

  Colorblind: phase and Discord must use shape/position, not color alone.

================================================================================
14. IMPLEMENTATION MAP (FOR LATER — NOT THIS DOC PASS)
================================================================================

Scaffold lives in .new.RhythmStitchers (weapon template).

  Plugin.cs                        BepInEx entry, sandbox flag, registration timing
  WeaponRegistration.cs            Clone base gun, GearInfo, ApplyStitcherStats
  RhythmStitchersBehaviour.cs      Data host: Tempo, L/R mag & timers, flags, Discord
  RhythmStitchersInputHooks.cs     Fire+Aim dual semi routing; suppress ADS
  RhythmStitchersCombatHooks.cs    OnFiredBullet channel tag, on-beat, waves, crashes
  RhythmStitchersReloadHooks.cs    Dual independent reload
  RhythmStitchersHUD.cs            Dual ammo + Tempo (SparrohUILib optional)
  SonicElement.cs / staging        Custom element or mod-local status
  SpawnGearHooks.cs                Equip remap + identity stamp
  RhythmUpgrades.cs                Register ids 93001–93030
  RhythmUpgradeProperties.cs       Apply/Remove + stat ranges
  RhythmUpgradePatterns.cs         Hex footprints
  UpgradeRegistration.cs           CreateUpgrade helper

Behaviour.Data sketch:

  float bpm, phase, onBeatWindow, onBeatDamageMult;
  int leftMag, rightMag, leftMagMax, rightMagMax;
  float leftFireInterval, rightFireInterval;
  float leftCooldown, rightCooldown;
  float discord, discordThreshold;
  float sweepCooldown, lastYaw, yawAccum;
  int measureBeats, beatIndex;
  int capStacks;
  bool perfectPitch, crossSweep, desync, phantomDuet, polyrhythm, wallOfSound;
  bool autoloop;
  // path floats: wave damage, stun duration, etc.

Hooks (expected):

  Gun.Update prefix — Tempo tick, dual input semi fire, wall tick, sweep angle
  PlayerInput Fire + Aim — channel fire (DMLR DNA; both mean shoot)
  Gun aim path — force canAim false / skip HandleAim
  Ammo spend — split to L/R; suppress single-mag vanilla assumptions
  Reload — dual mag refill rules
  OnFiredBullet / OnDamageTarget — channel id, beat grade, Sonic appl, Discord
  OnKillTarget — optional refunds / encore hooks
  Stun application — respect existing CC APIs (audit decompile at impl)

GunData baseline sketch:

  automatic = false (or true only with Autoloop flag driving custom fire)
  canAim = false
  damage / fireInterval per §3.2 (note: dual channel may virtualize intervals)
  magazineSize — may mirror L only in vanilla UI until custom HUD; prefer
    custom dual HUD and hide lying single mag if possible

Persistence:
  Gear ID is save key. Register AllGear before PlayerData.AddGear; re-bind.

Multiplayer:
  Same mod + matching ids on all clients.
  Owner-auth Tempo and fire; stuns/waves via existing damage authority patterns.
  Sandbox flag.

MycoMod:
  [MycoMod(null, ModFlags.IsSandbox)] — changes combat economy + element.

Impl DNA to copy:
  - DMLRRework: dual input polling Fire+Aim, canAim false
  - CyclerRework: custom resource HUD, R verb ownership, path crowns
  - HelminthReceiver doc: full catalog structure, safety floors, frozen 30
  - Lead Flinger wiki: Phantom Limb → Phantom Duet, semi tempo fantasy
  - DiscWorldRework / Concussive Wave: wave feel references
  - Heaven’s Fury Rework: timed AoE pulse reference for measure/crash

================================================================================
15. OPEN TUNING QUESTIONS (PLAYTEST, NOT DESIGN BLOCKERS)
================================================================================

  1. Default BPM exact value and whether mission intensity scales Tempo.
  2. Dual reload: both locked vs allow free channel during staggered reload.
  3. Reserve ammo shared vs per-channel.
  4. Sonic stun duration vs elite CC resist — avoid boss lock forever.
  5. Sweep min angular rate across low/high sens players (normalize by yaw delta).
  6. Autoloop default interval vs semi mash ceiling fairness.
  7. Measure Cut: fixed 3-beat vs rolled modes.
  8. Drop the Beat: measure chord vs mag-empty nova primary trigger.
  9. Click-Track Cap spend: hold-R vs auto on measure.
 10. Stage A vs Stage B Sonic for first public build.
 11. Interim single-mesh presentation vs wait for dual pistol art.
 12. Promote/demote any Epic↔Exotic after first play duals.

================================================================================
16. DELIBERATE NON-GOALS
================================================================================

  - No forced keystone exclusion matrix
  - No free baseline Cross Sweep waves
  - No free baseline Desync stun crashes
  - No ADS zoom identity
  - No replacing vanilla weapons (new gear entry only)
  - No full Heat-Cycler-style multi-element interlacing required at v1
  - Not shipping Edge Fault / Multiversal Thievery in v1
  - Not requiring perfect rhythm to deal baseline damage
  - Not a guitar hero QTEs / button-sequence minigame overlay

================================================================================
17. REVIEW DECISIONS LOCKED
================================================================================

  [x] Name: Rhythm Stitchers
  [x] Fire mode: high-RoF semi-auto per channel; full-auto is upgrade-owned
  [x] Input: LMB Left, RMB Right, no ADS; both held = both fire
  [x] Magazines: independent L/R
  [x] Tempo: always-on master clock + light on-beat crumb baseline
  [x] Cross Sweep: Exotic keystone (not free)
  [x] Desync: Exotic keystone (not free)
  [x] Element: new Sonic (custom; staged impl allowed)
  [x] Paths: Metronome / Cross Sweep / Desync — fluid mix
  [x] Doc depth: full ~30 catalog with number bands
  [x] Exotics (6) equal large shapes
  [x] ~30 v1 pool frozen in §9.6; backlog in §9.7
  [x] Reload: tap R dual/staggered reload; hold-R free for upgrades
  [x] DMLR-style input polling DNA for Fire+Aim

================================================================================
18. ONE-PAGE SUMMARY
================================================================================

  Rhythm Stitchers are Mycopunk’s dual machine pistols — two triggers, two
  mags, one Tempo. Baseline is high-rate semi stereo fire with a heartbeat and
  a light on-beat crumb. The hex grid is three gravity wells — Metronome,
  Cross Sweep, Desync — built to mix. Sonic is the native tongue: pressure,
  resonance, stun, and waves. Exotics rebuild the performance; Standards glue
  the kit; nothing is a railroad.

  Tune until clicking both needles on the beat feels like skill, sweeping the
  pair feels like painting, desyncing them feels like violence, and every
  hybrid still reads as the same SAXON stereo song.
