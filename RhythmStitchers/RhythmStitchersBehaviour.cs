using System;
using UnityEngine;

/// <summary>
/// Custom gameplay host for Rhythm Stitchers — dual channels, independent mags, Tempo.
/// Live instances are still AcceleratorGun (NGO clone); this MB holds mod state.
///
/// Ammo model: independent left/right mags + shared reserve (gun.StoredAmmo).
/// Vanilla RemainingAmmo is kept as ammoLeft + ammoRight for reload/empty systems.
/// </summary>
public sealed class RhythmStitchersBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        public float channelFireInterval;
        public int magSizeLeft;
        public int magSizeRight;
        public float bpm;
        public float onBeatWindow;
        public float onBeatDamageMult;
        public int measureBeats;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = SparrohPlugin.GearDescription;

    public float LastFireTimeLeft { get; set; } = -999f;
    public float LastFireTimeRight { get; set; } = -999f;

    public enum Channel
    {
        Left = 0,
        Right = 1
    }

    public Channel PendingChannel { get; set; } = Channel.Left;
    public bool IsFiringRightChannel { get; set; }

    /// <summary>True when the shot currently being fired landed on-beat (set before Fire).</summary>
    public bool PendingShotOnBeat { get; set; }

    /// <summary>Live shells in left mag.</summary>
    public int AmmoLeft { get; private set; }

    /// <summary>Live shells in right mag.</summary>
    public int AmmoRight { get; private set; }

    /// <summary>True once mags have been initialized from gun ammo at least once.</summary>
    public bool MagsInitialized { get; private set; }

    // --- Tempo ---
    public float Phase { get; private set; }
    public int BeatIndex { get; private set; }
    private float _tempoAccum;

    /// <summary>
    /// Continuous time in beats since equip (for pendulum). Does not wrap per beat.
    /// </summary>
    private float _beatTime;


    private bool aimBound;
    private Gun boundGun;
    private Action<UnityEngine.InputSystem.InputAction.CallbackContext> onAimPerformed;
    private Action<UnityEngine.InputSystem.InputAction.CallbackContext> onAimCanceled;

    public ref Data WeaponData => ref data;
    public Data GetPrefabSnapshot() => prefabSnapshot;
    public string Description => description;

    public int MagCapacityTotal =>
        Mathf.Max(0, data.magSizeLeft) + Mathf.Max(0, data.magSizeRight);

    public int MagAmmoTotal => AmmoLeft + AmmoRight;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            channelFireInterval = RhythmStitchersBalance.ChannelFireInterval,
            magSizeLeft = RhythmStitchersBalance.MagSizeLeft,
            magSizeRight = RhythmStitchersBalance.MagSizeRight,
            bpm = RhythmStitchersBalance.Bpm,
            onBeatWindow = RhythmStitchersBalance.OnBeatWindow,
            onBeatDamageMult = RhythmStitchersBalance.OnBeatDamageMult,
            measureBeats = RhythmStitchersBalance.MeasureBeats
        };
    }

    public void InitializeAsPrefab(string desc)
    {
        description = desc ?? SparrohPlugin.GearDescription;
        data = CreateDefaultData();
        prefabSnapshot = data;
        ResetRuntime();
    }

    public void RestoreFromPrefab()
    {
        data = prefabSnapshot;
    }

    public void CapturePrefabSnapshot() => prefabSnapshot = data;

    public void CopyFrom(RhythmStitchersBehaviour template)
    {
        if (template == null)
            return;
        description = template.description;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        ResetRuntime();
    }

    public void CopySnapshotFrom(RhythmStitchersBehaviour template) => CopyFrom(template);

    public void ResetRuntime()
    {
        LastFireTimeLeft = -999f;
        LastFireTimeRight = -999f;
        PendingChannel = Channel.Left;
        IsFiringRightChannel = false;
        PendingShotOnBeat = false;
        AmmoLeft = 0;
        AmmoRight = 0;
        MagsInitialized = false;
        Phase = 0f;
        BeatIndex = 0;
        _tempoAccum = 0f;
        _beatTime = 0f;
    }


    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        WeaponRegistration.ApplyRhythmStitchersStats(gun);
        EnsureMagsInitialized(gun);
    }

    public void OnUpgradesCleared(Gun gun)
    {
        BindAimAsRightChannel(gun, bind: false);
        data = prefabSnapshot;
        ResetRuntime();
        boundGun = null;
    }

    private void Update()
    {
        if (boundGun == null || !boundGun.IsOwner || !boundGun.Active)
            return;

        TickTempo(Time.unscaledDeltaTime);
        RhythmStitchersHud.Tick(this);
    }

    public void TickTempo(float dt)
    {
        float bpm = data.bpm > 1f ? data.bpm : RhythmStitchersBalance.Bpm;
        float beatDuration = 60f / bpm;
        if (beatDuration <= 0.001f)
            return;

        float step = Mathf.Max(0f, dt);
        _tempoAccum += step;
        _beatTime += step / beatDuration;

        while (_tempoAccum >= beatDuration)
        {
            _tempoAccum -= beatDuration;
            BeatIndex++;
            int measure = data.measureBeats > 0 ? data.measureBeats : 4;
            if (BeatIndex >= measure)
                BeatIndex = 0;
        }

        Phase = Mathf.Clamp01(_tempoAccum / beatDuration);
    }

    /// <summary>
    /// Pendulum position 0..1 along the arc.
    /// 0 = left tip (LMB sweet), 1 = right tip (RMB sweet).
    /// Triangle wave over <see cref="RhythmStitchersBalance.PendulumBeatsPerCycle"/> beats
    /// so the needle sweeps L→R→L continuously.
    /// </summary>
    public float GetPendulum01()
    {
        float cycleBeats = RhythmStitchersBalance.PendulumBeatsPerCycle;
        if (cycleBeats < 0.5f)
            cycleBeats = 2f;

        // 0..1 over one full L→R→L cycle
        float cycle = _beatTime / cycleBeats;
        cycle -= Mathf.Floor(cycle);
        // Triangle: 0→1 over first half (L→R), 1→0 over second half (R→L)
        if (cycle < 0.5f)
            return cycle * 2f;
        return 2f - cycle * 2f;
    }

    /// <summary>
    /// Seconds of travel from current pendulum position to the nearest end (0 or 1),
    /// measured along the pendulum path.
    /// </summary>
    public float GetPendulumEndErrorSeconds(Channel channel)
    {
        float bpm = data.bpm > 1f ? data.bpm : RhythmStitchersBalance.Bpm;
        float beatDuration = 60f / bpm;
        float cycleBeats = RhythmStitchersBalance.PendulumBeatsPerCycle;
        if (cycleBeats < 0.5f)
            cycleBeats = 2f;

        // One-way L→R (or R→L) duration in seconds
        float halfCycleSeconds = cycleBeats * 0.5f * beatDuration;
        if (halfCycleSeconds <= 0.001f)
            return 999f;

        float p = GetPendulum01();
        float target = channel == Channel.Right ? 1f : 0f;
        float dist = Mathf.Abs(p - target); // 0..1 along arc
        return dist * halfCycleSeconds;
    }

    /// <summary>
    /// True when the pendulum is in the sweet zone for this channel's end.
    /// Left sweet → LMB crumb; Right sweet → RMB crumb.
    /// </summary>
    public bool IsChannelOnBeat(Channel channel)
    {
        float window = data.onBeatWindow > 0f ? data.onBeatWindow : RhythmStitchersBalance.OnBeatWindow;
        return GetPendulumEndErrorSeconds(channel) <= window;
    }

    /// <summary>
    /// Legacy: true if either end is currently in its sweet window.
    /// Prefer <see cref="IsChannelOnBeat"/> for fire grading.
    /// </summary>
    public bool IsOnBeatNow()
    {
        return IsChannelOnBeat(Channel.Left) || IsChannelOnBeat(Channel.Right);
    }


    public void EnsureMagsInitialized(Gun gun)
    {
        if (gun == null)
            return;

        if (!MagsInitialized)
        {
            FillMagsFull();
            MagsInitialized = true;
        }

        SyncRemainingAmmo(gun);
        PushPrimaryHud(gun);
    }

    public void FillMagsFull()
    {
        AmmoLeft = Mathf.Max(0, data.magSizeLeft);
        AmmoRight = Mathf.Max(0, data.magSizeRight);
    }

    public struct ReloadSnapshot
    {
        public int ammoLeft;
        public int ammoRight;
        public float remaining;
        public float stored;
        public bool valid;
    }

    public ReloadSnapshot CaptureReloadSnapshot(Gun gun)
    {
        var snap = new ReloadSnapshot
        {
            ammoLeft = AmmoLeft,
            ammoRight = AmmoRight,
            valid = true
        };
        try
        {
            snap.remaining = gun.RemainingAmmo;
            snap.stored = gun.StoredAmmo;
        }
        catch
        {
            snap.remaining = MagAmmoTotal;
            snap.stored = 0f;
        }
        return snap;
    }

    /// <summary>
    /// After vanilla mag refill: undo its Remaining/Stored changes and top up
    /// each channel independently (never move shells between L/R).
    /// </summary>
    public void ApplyChannelAwareReload(Gun gun, ReloadSnapshot snap)
    {
        if (gun == null || !snap.valid)
            return;

        int capL = Mathf.Max(0, data.magSizeLeft);
        int capR = Mathf.Max(0, data.magSizeRight);

        try
        {
            gun.StoredAmmo = snap.stored;
            gun.RemainingAmmo = snap.ammoLeft + snap.ammoRight;
        }
        catch
        {
            // ignore
        }

        AmmoLeft = Mathf.Clamp(snap.ammoLeft, 0, capL);
        AmmoRight = Mathf.Clamp(snap.ammoRight, 0, capR);

        int needL = capL - AmmoLeft;
        int needR = capR - AmmoRight;
        int need = needL + needR;
        if (need <= 0)
        {
            MagsInitialized = true;
            SyncRemainingAmmo(gun);
            PushPrimaryHud(gun);
            return;
        }

        int reserve;
        try
        {
            reserve = Mathf.Max(0, (int)gun.StoredAmmo);
        }
        catch
        {
            reserve = 0;
        }

        int take = Mathf.Min(need, reserve);
        int fillL = Mathf.Min(needL, take);
        take -= fillL;
        int fillR = Mathf.Min(needR, take);

        AmmoLeft += fillL;
        AmmoRight += fillR;
        int spent = fillL + fillR;

        try
        {
            if (spent > 0)
                gun.StoredAmmo = Mathf.Max(0f, gun.StoredAmmo - spent);
        }
        catch
        {
            // ignore
        }

        MagsInitialized = true;
        SyncRemainingAmmo(gun);
        PushPrimaryHud(gun);
    }

    public bool HasChannelAmmo(Channel channel)
    {
        return channel == Channel.Right ? AmmoRight > 0 : AmmoLeft > 0;
    }

    public void SpendChannel(Gun gun, Channel channel)
    {
        if (channel == Channel.Right)
            AmmoRight = Mathf.Max(0, AmmoRight - 1);
        else
            AmmoLeft = Mathf.Max(0, AmmoLeft - 1);

        SyncRemainingAmmo(gun);
        PushPrimaryHud(gun);
    }

    public void SyncRemainingAmmo(Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;

        int sum = MagAmmoTotal;
        try
        {
            if (gun.RemainingAmmoCount != sum)
                gun.RemainingAmmo = sum;
        }
        catch
        {
            // ignore
        }
    }

    public int FormatPrimaryHud(char[] buffer)
    {
        if (buffer == null || buffer.Length < 3)
            return 0;

        int i = 0;
        i = WriteNumber(buffer, i, AmmoLeft);
        if (i < buffer.Length)
            buffer[i++] = '|';
        i = WriteNumber(buffer, i, AmmoRight);
        return i;
    }

    private static int WriteNumber(char[] buffer, int index, int value)
    {
        value = Mathf.Max(0, value);
        if (value < 10)
        {
            if (index < buffer.Length)
                buffer[index++] = (char)('0' + value);
            return index;
        }

        string s = value.ToString();
        for (int c = 0; c < s.Length && index < buffer.Length; c++)
            buffer[index++] = s[c];
        return index;
    }

    public void PushPrimaryHud(Gun gun)
    {
        if (gun == null || !gun.IsOwner)
            return;

        try
        {
            char[] buf = Global.charBuffer;
            int len = FormatPrimaryHud(buf);
            if (len > 0)
                RhythmStitchersCombatHooks.InvokePrimaryHud(gun, len, buf);
        }
        catch
        {
            // HUD may not be ready
        }
    }

    public void BindAimAsRightChannel(Gun gun, bool bind)
    {
        if (gun == null)
            return;

        if (bind && aimBound)
            return;
        if (!bind && !aimBound)
            return;

        try
        {
            if (onAimPerformed == null)
                onAimPerformed = OnAimPerformed;
            if (onAimCanceled == null)
                onAimCanceled = OnAimCanceled;

            if (bind)
            {
                boundGun = gun;
                EnsureMagsInitialized(gun);
                PlayerInput.Controls.Player.Aim.performed += onAimPerformed;
                PlayerInput.Controls.Player.Aim.canceled += onAimCanceled;
                aimBound = true;
                PushPrimaryHud(gun);
            }
            else
            {
                PlayerInput.Controls.Player.Aim.performed -= onAimPerformed;
                PlayerInput.Controls.Player.Aim.canceled -= onAimCanceled;
                aimBound = false;
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[RhythmStitchers] BindAimAsRightChannel({bind}): {ex.Message}");
        }
    }

    private void OnAimPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        TryFireRightChannel();
    }

    private void OnAimCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
    }

    public void TryFireRightChannel()
    {
        Gun gun = boundGun != null ? boundGun : GetComponent<Gun>();
        if (gun == null || !gun.IsOwner || !gun.Active)
            return;

        EnsureMagsInitialized(gun);

        if (!CanFireChannel(gun, Channel.Right))
        {
            if (!HasChannelAmmo(Channel.Right) && gun.Player != null)
            {
                try { gun.Player.FlashAmmoCounter(gun); }
                catch { /* ignore */ }
            }
            return;
        }

        PendingChannel = Channel.Right;
        IsFiringRightChannel = true;
        PendingShotOnBeat = IsChannelOnBeat(Channel.Right);

        float savedLastFire = gun.LastFireTime;
        try
        {
            if (gun.RemainingAmmo < 1f)
                gun.RemainingAmmo = 1f;

            RhythmStitchersCombatHooks.InvokeGunFire(gun);
            LastFireTimeRight = Time.time;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[RhythmStitchers] Right channel fire failed: {ex.Message}");
        }
        finally
        {
            IsFiringRightChannel = false;
            PendingChannel = Channel.Left;
            // PendingShotOnBeat cleared in Fire postfix after ModifyBulletData.
            try
            {
                gun.LastFireTime = savedLastFire;
            }
            catch
            {
                // ignore
            }
        }
    }

    public bool CanFireChannel(Gun gun, Channel channel)
    {
        if (gun == null)
            return false;

        try
        {
            if (gun.Player != null && gun.Player.IsFireLocked)
                return false;
        }
        catch
        {
            // ignore
        }

        try
        {
            if (gun.Reloading)
                return false;
        }
        catch
        {
            // ignore
        }

        if (!HasChannelAmmo(channel))
            return false;

        float last = channel == Channel.Right ? LastFireTimeRight : LastFireTimeLeft;
        float interval = data.channelFireInterval > 0.01f
            ? data.channelFireInterval
            : Mathf.Max(0.05f, gun.FireInterval);

        if (Time.time - last < interval)
            return false;

        return true;
    }

    public void NotifyChannelFired(Channel channel)
    {
        float t = Time.time;
        if (channel == Channel.Right)
            LastFireTimeRight = t;
        else
            LastFireTimeLeft = t;
    }

    /// <summary>Call after a shot is committed with the on-beat grade used for that shot.</summary>
    public void NotifyShotFeedback(Channel channel, bool onBeat)
    {
        RhythmStitchersHud.NotifyShot(channel, onBeat);
    }

    private void OnDestroy()
    {
        if (boundGun != null)
            BindAimAsRightChannel(boundGun, bind: false);
        else
            BindAimAsRightChannel(GetComponent<Gun>(), bind: false);
        RhythmStitchersHud.Hide();
    }

    private void OnDisable()
    {
        if (aimBound)
            BindAimAsRightChannel(boundGun != null ? boundGun : GetComponent<Gun>(), bind: false);
    }


    public static bool TryGet(IGear gear, out RhythmStitchersBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<RhythmStitchersBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == SparrohPlugin.GearApiName ||
                       gear.Info.ID == SparrohPlugin.GearId);

        RhythmStitchersBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<RhythmStitchersBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;

        behaviour = gear.gameObject.AddComponent<RhythmStitchersBehaviour>();
        behaviour.InitializeAsPrefab(desc);
        if (prefabBehaviour != null)
            behaviour.CopyFrom(prefabBehaviour);
        return true;
    }

    public static bool IsOurGear(IGear gear)
    {
        if (gear?.Info == null)
            return false;
        return gear.Info.APIName == SparrohPlugin.GearApiName ||
               gear.Info.ID == SparrohPlugin.GearId;
    }

    public static bool IsOurGear(IUpgradable gear)
    {
        if (gear?.Info == null)
            return false;
        return gear.Info.APIName == SparrohPlugin.GearApiName ||
               gear.Info.ID == SparrohPlugin.GearId;
    }

    public static bool IsOurDamageSource(IDamageSource source)
    {
        if (source == null)
            return false;

        if (source is Gun gun)
            return IsOurGear(gun);

        if (source.ParentSource is Gun parentGun)
            return IsOurGear(parentGun);

        return false;
    }
}
