using System;
using UnityEngine;

/// <summary>
/// Custom gameplay host for Aussie Special — baseline hopper + chamber state.
/// Live instances are still BounceShotgun (NGO clone); this MB holds mod state.
///
/// Ammo model: independent left/right chambers + shared reserve (gun.StoredAmmo).
/// Vanilla RemainingAmmo is kept as ammoLeft + ammoRight for reload/empty systems.
/// </summary>
public sealed class AussieSpecialBehaviour : MonoBehaviour
{
    [Serializable]
    public struct Data
    {
        public float preBounceDamageMult;
        public float barrelFireInterval;
        public int chamberSizeLeft;
        public int chamberSizeRight;
    }

    [SerializeField]
    private Data data = CreateDefaultData();

    private Data prefabSnapshot = CreateDefaultData();
    private string description = SparrohPlugin.GearDescription;

    public float LastFireTimeLeft { get; set; } = -999f;
    public float LastFireTimeRight { get; set; } = -999f;

    public enum Barrel
    {
        Left = 0,
        Right = 1
    }

    public Barrel PendingBarrel { get; set; } = Barrel.Left;
    public bool IsFiringRightBarrel { get; set; }

    /// <summary>Live shells in left chamber.</summary>
    public int AmmoLeft { get; private set; }

    /// <summary>Live shells in right chamber.</summary>
    public int AmmoRight { get; private set; }

    /// <summary>True once chambers have been initialized from gun ammo at least once.</summary>
    public bool ChambersInitialized { get; private set; }

    private bool aimBound;
    private Gun boundGun;
    private Action<UnityEngine.InputSystem.InputAction.CallbackContext> onAimPerformed;
    private Action<UnityEngine.InputSystem.InputAction.CallbackContext> onAimCanceled;

    public ref Data WeaponData => ref data;
    public Data GetPrefabSnapshot() => prefabSnapshot;
    public string Description => description;

    public int ChamberCapacityTotal =>
        Mathf.Max(0, data.chamberSizeLeft) + Mathf.Max(0, data.chamberSizeRight);

    public int ChamberAmmoTotal => AmmoLeft + AmmoRight;

    public static Data CreateDefaultData()
    {
        return new Data
        {
            preBounceDamageMult = AussieSpecialBalance.PreBounceDamageMult,
            barrelFireInterval = AussieSpecialBalance.BarrelFireInterval,
            chamberSizeLeft = AussieSpecialBalance.ChamberSizeLeft,
            chamberSizeRight = AussieSpecialBalance.ChamberSizeRight
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

    public void CopyFrom(AussieSpecialBehaviour template)
    {
        if (template == null)
            return;
        description = template.description;
        prefabSnapshot = template.prefabSnapshot;
        data = prefabSnapshot;
        ResetRuntime();
    }

    public void CopySnapshotFrom(AussieSpecialBehaviour template) => CopyFrom(template);

    public void ResetRuntime()
    {
        LastFireTimeLeft = -999f;
        LastFireTimeRight = -999f;
        PendingBarrel = Barrel.Left;
        IsFiringRightBarrel = false;
        AmmoLeft = 0;
        AmmoRight = 0;
        ChambersInitialized = false;
    }

    public void OnUpgradesApplied(Gun gun)
    {
        boundGun = gun;
        WeaponRegistration.ApplyAussieSpecialStats(gun);
        EnsureChambersInitialized(gun);
        // Aim→right-barrel bind happens in Gun.Enable postfix only.
    }

    public void OnUpgradesCleared(Gun gun)
    {
        BindAimAsRightBarrel(gun, bind: false);
        data = prefabSnapshot;
        ResetRuntime();
        boundGun = null;
    }

    public void EnsureChambersInitialized(Gun gun)
    {
        if (gun == null)
            return;

        if (!ChambersInitialized)
        {
            // Fresh equip / stamp: full chambers (1|1).
            FillChambersFull();
            ChambersInitialized = true;
        }

        SyncRemainingAmmo(gun);
        PushPrimaryHud(gun);
    }


    /// <summary>Fill both chambers to capacity (equip / full restore).</summary>
    public void FillChambersFull()
    {
        AmmoLeft = Mathf.Max(0, data.chamberSizeLeft);
        AmmoRight = Mathf.Max(0, data.chamberSizeRight);
    }

    /// <summary>
    /// Snapshot taken before vanilla OnAmmoLoaded mutates Remaining/Stored.
    /// </summary>
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
            snap.remaining = ChamberAmmoTotal;
            snap.stored = 0f;
        }
        return snap;
    }

    /// <summary>
    /// After vanilla mag refill: undo its Remaining/Stored changes and top up
    /// each chamber independently (never move shells between barrels).
    /// </summary>
    public void ApplyChamberAwareReload(Gun gun, ReloadSnapshot snap)
    {
        if (gun == null || !snap.valid)
            return;

        int capL = Mathf.Max(0, data.chamberSizeLeft);
        int capR = Mathf.Max(0, data.chamberSizeRight);

        // Restore pre-reload ammo pools — vanilla already shuffled them by total.
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
            ChambersInitialized = true;
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
        // Top up left empties first, then right (order only matters for partial reserve).
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

        ChambersInitialized = true;
        SyncRemainingAmmo(gun);
        PushPrimaryHud(gun);
    }


    public bool HasChamberAmmo(Barrel barrel)
    {
        return barrel == Barrel.Right ? AmmoRight > 0 : AmmoLeft > 0;
    }

    /// <summary>
    /// Spend one shell from the given chamber after a successful Fire.
    /// Re-syncs RemainingAmmo to chamber sum (vanilla already decremented by 1).
    /// </summary>
    public void SpendChamber(Gun gun, Barrel barrel)
    {
        if (barrel == Barrel.Right)
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

        int sum = ChamberAmmoTotal;
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

    /// <summary>Format primary HUD as "L|R" (e.g. 1|1, 0|1).</summary>
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

        // Multi-digit (future mag extender)
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
                AussieSpecialCombatHooks.InvokePrimaryHud(gun, len, buf);
        }
        catch
        {
            // HUD may not be ready
        }
    }


    public void BindAimAsRightBarrel(Gun gun, bool bind)
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
                EnsureChambersInitialized(gun);
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
            SparrohPlugin.Logger?.LogDebug($"[AussieSpecial] BindAimAsRightBarrel({bind}): {ex.Message}");
        }
    }

    private void OnAimPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        TryFireRightBarrel();
    }

    private void OnAimCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
    }

    public void TryFireRightBarrel()
    {
        Gun gun = boundGun != null ? boundGun : GetComponent<Gun>();
        if (gun == null || !gun.IsOwner || !gun.Active)
            return;

        EnsureChambersInitialized(gun);

        if (!CanFireBarrel(gun, Barrel.Right))
        {
            // Dry-fire this chamber while the other may still be loaded.
            if (!HasChamberAmmo(Barrel.Right) && gun.Player != null)
            {
                try { gun.Player.FlashAmmoCounter(gun); }
                catch { /* ignore */ }
            }
            return;
        }


        PendingBarrel = Barrel.Right;
        IsFiringRightBarrel = true;
        float savedLastFire = gun.LastFireTime;
        try
        {
            // Ensure vanilla Fire ammo gate sees at least 1 shell.
            if (gun.RemainingAmmo < 1f)
                gun.RemainingAmmo = 1f;

            AussieSpecialCombatHooks.InvokeGunFire(gun);
            LastFireTimeRight = Time.time;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[AussieSpecial] Right barrel fire failed: {ex.Message}");
        }
        finally
        {
            IsFiringRightBarrel = false;
            PendingBarrel = Barrel.Left;
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

    public bool CanFireBarrel(Gun gun, Barrel barrel)
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

        if (!HasChamberAmmo(barrel))
            return false;

        float last = barrel == Barrel.Right ? LastFireTimeRight : LastFireTimeLeft;
        float interval = data.barrelFireInterval > 0.01f
            ? data.barrelFireInterval
            : Mathf.Max(0.05f, gun.FireInterval);

        if (Time.time - last < interval)
            return false;

        return true;
    }

    public void NotifyBarrelFired(Barrel barrel)
    {
        float t = Time.time;
        if (barrel == Barrel.Right)
            LastFireTimeRight = t;
        else
            LastFireTimeLeft = t;
    }

    private void OnDestroy()
    {
        if (boundGun != null)
            BindAimAsRightBarrel(boundGun, bind: false);
        else
            BindAimAsRightBarrel(GetComponent<Gun>(), bind: false);
    }

    private void OnDisable()
    {
        if (aimBound)
            BindAimAsRightBarrel(boundGun != null ? boundGun : GetComponent<Gun>(), bind: false);
    }

    public static bool TryGet(IGear gear, out AussieSpecialBehaviour behaviour)
    {
        behaviour = null;
        if (gear?.gameObject == null)
            return false;

        behaviour = gear.gameObject.GetComponent<AussieSpecialBehaviour>();
        if (behaviour != null)
            return true;

        bool isOurs = gear.Info != null &&
                      (gear.Info.APIName == SparrohPlugin.GearApiName ||
                       gear.Info.ID == SparrohPlugin.GearId);

        AussieSpecialBehaviour prefabBehaviour = null;
        if (gear.Prefab is Component prefabComp)
            prefabBehaviour = prefabComp.GetComponent<AussieSpecialBehaviour>();

        if (!isOurs && prefabBehaviour == null)
            return false;

        string desc = prefabBehaviour != null
            ? prefabBehaviour.Description
            : SparrohPlugin.GearDescription;

        behaviour = gear.gameObject.AddComponent<AussieSpecialBehaviour>();
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
