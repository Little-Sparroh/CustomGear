using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Shared Severance helpers: part typing, Mark/Expose state, damage transfer.
/// State lives on <see cref="MarksmanLaserRifleBehaviour"/>; this type is stateless logic.
/// </summary>
internal static class SeveranceSystem
{
    public enum PartKind : byte
    {
        None = 0,
        Limb = 1,
        Shell = 2,
        Core = 3,
        Other = 4
    }

    /// <summary>
    /// Playtest aid: BepInEx log + floating text + health outline on transfer dest.
    /// Flip to false (or remove) once Severance feel is validated.
    /// </summary>
    public static bool DebugTransfer = true;

    private static readonly Color DebugTransferTextColor = new Color(0.45f, 0.95f, 1f, 1f);

    /// <summary>DamageFlags.Custom marks transfer hits so they cannot chain-transfer.</summary>
    public const DamageFlags TransferFlag = DamageFlags.Custom;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTransferHit(in DamageData data)
    {
        return (data.damageFlags & TransferFlag) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTransferHit(in DamageCallbackData data)
    {
        return IsTransferHit(in data.damageData);
    }

    public static PartKind GetPartKind(ITarget target)
    {
        if (target == null)
            return PartKind.None;

        // Concrete types first (fast path).
        if (target is EnemyLimbPart)
            return PartKind.Limb;
        if (target is EnemyShell)
            return PartKind.Shell;
        if (target is EnemyCore)
            return PartKind.Core;

        if (target is EnemyPart part)
        {
            EnemyComponentType t = part.ComponentType;
            if ((t & EnemyComponentType.Limb) != 0)
                return PartKind.Limb;
            if ((t & EnemyComponentType.Shell) != 0)
                return PartKind.Shell;
            if ((t & EnemyComponentType.Core) != 0)
                return PartKind.Core;
            return PartKind.Other;
        }

        return PartKind.None;
    }

    public static EnemyPart AsEnemyPart(ITarget target)
    {
        return target as EnemyPart;
    }

    public static EnemyBrain GetBrain(ITarget target)
    {
        if (target is EnemyPart part)
            return part.Brain;
        return null;
    }

    public static EnemyCore GetCore(ITarget target)
    {
        return GetCore(GetBrain(target));
    }

    public static EnemyCore GetCore(EnemyBrain brain)
    {
        if (brain == null)
            return null;
        try
        {
            // Publicizer exposes Core; cast in case the property type is EnemyPart.
            object c = brain.Core;
            return c as EnemyCore;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Whether a core will accept normal (non-explosive / non-IgnoreImmunity) damage.
    /// Mirrors current <see cref="EnemyCore.Damage"/> shell-child gating:
    /// cores ignore most hits until <c>ChildComponents.Count < initialChildCount</c>
    /// (i.e. at least one child has been removed), unless canTakeDamageWhileAllChildrenLive.
    /// (Experimental builds tracked a separate currentChildCount field; release uses the list count.)
    /// </summary>
    public static bool CanCoreAcceptNormalDamage(EnemyCore core)
    {
        if (core == null || !core.IsAlive)
            return false;

        try
        {
            // Publicizer exposes these private fields on EnemyCore.
            if (core.canTakeDamageWhileAllChildrenLive)
                return true;

            List<IEnemyComponent> children = core.ChildComponents;
            int current = children != null ? children.Count : 0;
            return current < core.initialChildCount;
        }
        catch
        {
            // Fallback: if the core still has a living shell child, treat as gated.
            return !HasLivingShellChild(core);
        }
    }


    public static bool HasLivingShellChild(EnemyPart part)
    {
        if (part?.ChildComponents == null)
            return false;

        List<IEnemyComponent> children = part.ChildComponents;
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] is EnemyShell shell && shell.IsAlive)
                return true;
        }

        return false;
    }

    /// <summary>
    /// True for leg/arm segments. Transfer must never bounce limb → limb.
    /// </summary>
    public static bool IsLimbPart(ITarget target)
    {
        return GetPartKind(target) == PartKind.Limb;
    }

    /// <summary>
    /// Transfer destination: drill one breach path through armor.
    /// Outer layer → pick weakest shell and sticky-focus it → when it dies, go inward
    /// on that path (not sideways to other outer shells) → eventually core.
    /// Never returns limbs.
    /// </summary>
    public static EnemyPart FindTransferDestination(EnemyPart from)
    {
        return FindTransferDestination(from, null);
    }

    public static EnemyPart FindTransferDestination(EnemyPart from, MarksmanLaserRifleBehaviour behaviour)
    {
        if (from == null)
            return null;

        EnemyBrain brain = from.Brain;
        EnemyCore core = brain != null ? GetCore(brain) : null;
        if (brain == null && core == null)
            return null;

        int brainId = brain != null ? brain.GetInstanceID() : 0;

        // Collect living shells under this brain/core.
        var shells = new List<EnemyShell>(16);
        if (core != null)
            CollectLivingShells(core, shells);
        else if (brain != null)
            CollectLivingShellsFromBrain(brain, shells);

        // No shells left — core if it will take the hit.
        if (shells.Count == 0)
        {
            ClearDrillFocus(behaviour, brainId);
            if (core != null && core.IsAlive && !ReferenceEquals(core, from) &&
                CanCoreAcceptNormalDamage(core))
                return core;
            return null;
        }

        // Sticky focus still alive → keep drilling the same piece.
        if (behaviour != null &&
            behaviour.drillFocusBrainId == brainId &&
            behaviour.drillFocusPartId != 0)
        {
            EnemyPart sticky = FindShellById(shells, behaviour.drillFocusPartId);
            if (sticky != null && sticky.IsAlive)
            {
                behaviour.drillFocusLastPos = sticky.transform.position;
                return sticky;
            }

            // Focus died — drill inward near the hole (not sideways to other outers).
            EnemyPart inward = PickInwardAfterFocusDeath(
                shells, behaviour.drillFocusLastPos, behaviour.drillFocusDepth);
            if (inward != null)
            {
                SetDrillFocus(behaviour, brainId, inward);
                return inward;
            }

            // Path clear enough for core?
            if (core != null && core.IsAlive && CanCoreAcceptNormalDamage(core) &&
                !ReferenceEquals(core, from))
            {
                ClearDrillFocus(behaviour, brainId);
                return core;
            }

            // Start a new breach on remaining armor.
            ClearDrillFocus(behaviour, brainId);
        }

        // Core already open and damageable — dump there (shells may still exist elsewhere).
        // Only if we have no shells at all above... actually nested bosses can have shells
        // while core takes damage. Prefer finishing the drill path first if shells remain
        // on our focus path; if no sticky, still peel one path before core when gated.
        if (core != null &&
            core.IsAlive &&
            !ReferenceEquals(core, from) &&
            CanCoreAcceptNormalDamage(core) &&
            !HasLivingShellChild(core))
        {
            // Direct shell children gone — core is the right dump even if weird attachments remain.
            ClearDrillFocus(behaviour, brainId);
            return core;
        }

        // New focus: outermost living layer, weakest shell (one piece only).
        EnemyPart pick = PickWeakestOutermost(shells);
        if (pick != null)
        {
            SetDrillFocus(behaviour, brainId, pick);
            return pick;
        }

        if (core != null && core.IsAlive && CanCoreAcceptNormalDamage(core) &&
            !ReferenceEquals(core, from))
            return core;

        return null;
    }

    /// <summary>
    /// Shell depth from core: 0 = direct child of core (innermost), higher = more outer.
    /// </summary>
    public static int GetShellDepthFromCore(EnemyPart shell)
    {
        if (shell == null)
            return -1;

        int depth = 0;
        IEnemyComponent p = shell.Parent;
        int guard = 0;
        while (p != null && guard++ < 16)
        {
            if (p is EnemyCore)
                return depth;
            if (p is EnemyShell)
                depth++;
            p = p.Parent;
        }

        // Detached / unknown parentage — treat as outer-ish.
        return depth;
    }

    private static void CollectLivingShells(EnemyPart root, List<EnemyShell> into)
    {
        if (root?.ChildComponents == null)
            return;

        List<IEnemyComponent> children = root.ChildComponents;
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] is not EnemyPart child || !child.IsAlive)
                continue;

            if (child is EnemyShell shell)
                into.Add(shell);

            // Recurse through any part so nested shells under non-shell nodes are found.
            CollectLivingShells(child, into);
        }
    }

    private static void CollectLivingShellsFromBrain(EnemyBrain brain, List<EnemyShell> into)
    {
        EnemyCore core = GetCore(brain);
        if (core != null)
        {
            CollectLivingShells(core, into);
            return;
        }

        // Fallback: nothing structured.
    }

    private static EnemyPart FindShellById(List<EnemyShell> shells, int partId)
    {
        for (int i = 0; i < shells.Count; i++)
        {
            if (GetPartId(shells[i]) == partId)
                return shells[i];
        }

        return null;
    }

    private static EnemyPart PickWeakestOutermost(List<EnemyShell> shells)
    {
        if (shells == null || shells.Count == 0)
            return null;

        int maxDepth = int.MinValue;
        for (int i = 0; i < shells.Count; i++)
        {
            int d = GetShellDepthFromCore(shells[i]);
            if (d > maxDepth)
                maxDepth = d;
        }

        EnemyShell best = null;
        float bestHp = float.MaxValue;
        for (int i = 0; i < shells.Count; i++)
        {
            EnemyShell s = shells[i];
            if (GetShellDepthFromCore(s) != maxDepth)
                continue;

            float hp = s.Health;
            // Tie-break: lower normalized health, then stable by id.
            if (best == null ||
                hp < bestHp - 0.01f ||
                (Mathf.Abs(hp - bestHp) <= 0.01f && GetPartId(s) < GetPartId(best)))
            {
                best = s;
                bestHp = hp;
            }
        }

        return best;
    }

    /// <summary>
    /// After the focused outer shell dies, pick the next piece on that breach:
    /// prefer shells closer to the hole (near lastPos) and more inward (lower depth
    /// than the dead focus), never jumping to a fresh outermost sibling stack.
    /// </summary>
    private static EnemyPart PickInwardAfterFocusDeath(
        List<EnemyShell> shells,
        Vector3 holePos,
        int deadFocusDepth)
    {
        if (shells == null || shells.Count == 0)
            return null;

        EnemyShell best = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < shells.Count; i++)
        {
            EnemyShell s = shells[i];
            int depth = GetShellDepthFromCore(s);

            // Prefer inward (depth < dead outer). Allow same depth only if very close
            // (reparented fragment), but heavily penalize true outermost siblings.
            float depthPenalty;
            if (deadFocusDepth >= 0 && depth > deadFocusDepth)
                depthPenalty = 1000f + depth; // shouldn't happen often
            else if (deadFocusDepth >= 0 && depth == deadFocusDepth)
                depthPenalty = 500f; // other outers — avoid
            else
                depthPenalty = (deadFocusDepth >= 0 ? (deadFocusDepth - depth) : 0) * 0.01f;

            float dist = (s.transform.position - holePos).sqrMagnitude;
            // Weakest among near/inward: blend distance with remaining HP.
            float score = dist + depthPenalty + s.Health * 0.05f;

            if (score < bestScore)
            {
                bestScore = score;
                best = s;
            }
        }

        // If the only options scored as "other outers", still return the nearest weak
        // inward-or-equal rather than null — caller may fall through to core.
        if (best != null && deadFocusDepth >= 0 && GetShellDepthFromCore(best) >= deadFocusDepth)
        {
            // Try again restricting to strictly inward.
            EnemyShell inwardOnly = null;
            float bestIn = float.MaxValue;
            for (int i = 0; i < shells.Count; i++)
            {
                EnemyShell s = shells[i];
                if (GetShellDepthFromCore(s) >= deadFocusDepth)
                    continue;
                float dist = (s.transform.position - holePos).sqrMagnitude + s.Health * 0.05f;
                if (dist < bestIn)
                {
                    bestIn = dist;
                    inwardOnly = s;
                }
            }

            if (inwardOnly != null)
                return inwardOnly;

            // No inward shells — signal caller to try core by returning null.
            return null;
        }

        return best;
    }

    private static void SetDrillFocus(MarksmanLaserRifleBehaviour b, int brainId, EnemyPart shell)
    {
        if (b == null || shell == null)
            return;
        b.drillFocusBrainId = brainId;
        b.drillFocusPartId = GetPartId(shell);
        b.drillFocusLastPos = shell.transform.position;
        b.drillFocusDepth = GetShellDepthFromCore(shell);
    }

    private static void ClearDrillFocus(MarksmanLaserRifleBehaviour b, int brainId)
    {
        if (b == null)
            return;
        if (b.drillFocusBrainId != 0 && b.drillFocusBrainId != brainId)
            return;
        b.drillFocusBrainId = 0;
        b.drillFocusPartId = 0;
        b.drillFocusDepth = 0;
    }

    /// <summary>Depth-first living shell under <paramref name="root"/>, skipping <paramref name="exclude"/>.</summary>
    public static EnemyPart FindLivingShell(EnemyPart root, EnemyPart exclude)
    {
        if (root?.ChildComponents == null)
            return null;

        List<IEnemyComponent> children = root.ChildComponents;
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] is not EnemyPart child || !child.IsAlive)
                continue;
            if (ReferenceEquals(child, exclude))
                continue;

            if (child is EnemyShell)
                return child;

            EnemyPart nested = FindLivingShell(child, exclude);
            if (nested != null)
                return nested;
        }

        return null;
    }



    /// <summary>
    /// Deal transfer damage to the destination part. Sets Custom flag so hooks ignore it.
    /// </summary>
    public static bool DealTransferDamage(
        IDamageSource source,
        EnemyPart destination,
        float damage,
        EffectType effect,
        float effectAmount,
        Vector3 hitPoint)
    {
        return DealTransferDamage(source, destination, damage, effect, effectAmount, hitPoint, null);
    }

    /// <param name="fromPart">Optional source part for debug labels (limb/shell that was hit).</param>
    public static bool DealTransferDamage(
        IDamageSource source,
        EnemyPart destination,
        float damage,
        EffectType effect,
        float effectAmount,
        Vector3 hitPoint,
        EnemyPart fromPart)
    {
        if (source == null || destination == null || !destination.IsAlive)
        {
            if (DebugTransfer)
            {
                SparrohPlugin.Logger?.LogInfo(
                    $"[Severance] Transfer SKIP dest={(destination == null ? "null" : PartLabel(destination))} " +
                    $"alive={destination != null && destination.IsAlive} dmg={damage:F1}");
            }

            return false;
        }

        if (damage <= 0.01f)
            return false;

        var data = new DamageData(
            damage,
            effect > EffectType.Normal ? effect : EffectType.Normal,
            effectAmount,
            TransferFlag | DamageFlags.Precision);

        Vector3 textPos = destination.transform != null
            ? destination.transform.position
            : hitPoint;

        try
        {
            bool ok = IDamageSource.DamageTarget(source, destination, data, hitPoint, null);
            if (DebugTransfer)
                NotifyTransferDebug(fromPart, destination, damage, ok, textPos);
            return ok;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[Severance] Transfer failed: {ex.Message}");
            return false;
        }
    }

    private static void NotifyTransferDebug(
        EnemyPart from,
        EnemyPart dest,
        float damage,
        bool applied,
        Vector3 position)
    {
        string fromLabel = PartLabel(from);
        string destLabel = PartLabel(dest);
        string status = applied ? "OK" : "BLOCKED";

        SparrohPlugin.Logger?.LogInfo(
            $"[Severance] Transfer {status}: {fromLabel} → {destLabel}  dmg={damage:F1}");

        // Floating text at destination (cyan) so shell hits are visible without staring at logs.
        try
        {
            if (Highlighter.TryGetInstance(out Highlighter hl) ||
                ((object)Highlighter.Instance != null && (hl = Highlighter.Instance) != null))
            {
                string text = applied
                    ? $"XFER {damage:F0}→{ShortKind(dest)}"
                    : $"XFER blocked→{ShortKind(dest)}";
                hl.ShowDamageText(dest, position, text, DebugTransferTextColor);

                // Pulse health outline on the destination so you can see which part took it.
                if (applied && dest is ITarget t)
                    hl.ActivateHealthDisplay(t, hl.HighlightMaterial, isInteractable: false, fadeSpeed: 4f);
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Severance] debug VFX: {ex.Message}");
        }
    }

    private static string PartLabel(EnemyPart part)
    {
        if (part == null)
            return "null";
        string name = part.name;
        if (string.IsNullOrEmpty(name))
            name = part.GetType().Name;
        return $"{ShortKind(part)}:{name}";
    }

    private static string ShortKind(EnemyPart part)
    {
        return GetPartKind(part) switch
        {
            PartKind.Limb => "Limb",
            PartKind.Shell => "Shell",
            PartKind.Core => "Core",
            PartKind.Other => "Other",
            _ => "?"
        };
    }


    public static float GetTransferPercent(MarksmanLaserRifleBehaviour behaviour, bool laserMode)
    {
        if (behaviour == null)
            return 0f;

        ref MarksmanLaserRifleBehaviour.Data wd = ref behaviour.WeaponData;
        float pct = laserMode ? wd.transferLaserPercent : wd.transferDmrPercent;

        // Hot Swap flips execute mode: high transfer rides with DMR slugs instead of laser.
        if (wd.hotSwapRoles)
            pct = laserMode ? wd.transferDmrPercent : wd.transferLaserPercent;

        return Mathf.Max(0f, pct);
    }

    // -------------------------------------------------------------------------
    // Mark
    // -------------------------------------------------------------------------

    public static void ApplyMark(MarksmanLaserRifleBehaviour b, EnemyPart part, float duration)
    {
        if (b == null || part == null || duration <= 0f)
            return;

        int id = GetPartId(part);
        if (id == 0)
            return;

        float expiry = Time.time + duration;
        b.EnsureMarkMap()[id] = expiry;
    }

    public static bool IsMarked(MarksmanLaserRifleBehaviour b, EnemyPart part)
    {
        if (b?.markExpiries == null || part == null)
            return false;

        int id = GetPartId(part);
        if (id == 0)
            return false;

        if (!b.markExpiries.TryGetValue(id, out float expiry))
            return false;

        if (Time.time > expiry)
        {
            b.markExpiries.Remove(id);
            return false;
        }

        return true;
    }

    // -------------------------------------------------------------------------
    // Expose (core weakpoint window)
    // -------------------------------------------------------------------------

    public static void ExposeBrain(MarksmanLaserRifleBehaviour b, EnemyBrain brain, float duration)
    {
        if (b == null || brain == null || duration <= 0f)
            return;

        int id = brain.GetInstanceID();
        float expiry = Time.time + duration;
        Dictionary<int, float> map = b.EnsureExposeMap();
        if (map.TryGetValue(id, out float existing) && existing > expiry)
            return;
        map[id] = expiry;
    }

    public static bool IsBrainExposed(MarksmanLaserRifleBehaviour b, EnemyBrain brain)
    {
        if (b?.exposeExpiries == null || brain == null)
            return false;

        int id = brain.GetInstanceID();
        if (!b.exposeExpiries.TryGetValue(id, out float expiry))
            return false;

        if (Time.time > expiry)
        {
            b.exposeExpiries.Remove(id);
            return false;
        }

        return true;
    }

    public static bool IsTargetExposed(MarksmanLaserRifleBehaviour b, ITarget target)
    {
        EnemyBrain brain = GetBrain(target);
        return IsBrainExposed(b, brain);
    }

    // -------------------------------------------------------------------------
    // Open Artery (Arterial Shred)
    // -------------------------------------------------------------------------

    public static void ApplyOpenArtery(MarksmanLaserRifleBehaviour b, EnemyBrain brain, float duration)
    {
        if (b == null || brain == null || duration <= 0f)
            return;
        b.openArteryBrainId = brain.GetInstanceID();
        b.openArteryExpiry = Time.time + duration;
    }

    public static bool TryConsumeOpenArtery(MarksmanLaserRifleBehaviour b, EnemyBrain brain, out float bonusTransfer)
    {
        bonusTransfer = 0f;
        if (b == null || brain == null)
            return false;
        if (b.openArteryBrainId == 0 || Time.time > b.openArteryExpiry)
        {
            b.openArteryBrainId = 0;
            return false;
        }

        if (brain.GetInstanceID() != b.openArteryBrainId)
            return false;

        bonusTransfer = Mathf.Max(0f, b.WeaponData.openArteryTransferBonus);
        b.openArteryBrainId = 0;
        b.openArteryExpiry = 0f;
        return bonusTransfer > 0f;
    }

    // -------------------------------------------------------------------------
    // Overkill tracking (pre-clamp)
    // -------------------------------------------------------------------------

    public static void RecordPotentialOverkill(MarksmanLaserRifleBehaviour b, EnemyPart part, float rawDamage)
    {
        if (b == null || part == null || rawDamage <= 0f)
            return;

        float health = part.Health;
        float overkill = rawDamage - health;
        if (overkill <= 0.01f)
        {
            // Still record part so kill can see it was our hit, overkill 0.
            if (overkill < 0f)
                overkill = 0f;
        }

        b.pendingOverkillPartId = GetPartId(part);
        b.pendingOverkillAmount = Mathf.Max(0f, overkill);
        b.pendingOverkillTime = Time.time;
    }

    public static bool TryTakeOverkill(MarksmanLaserRifleBehaviour b, EnemyPart killed, out float overkill)
    {
        overkill = 0f;
        if (b == null || killed == null)
            return false;
        if (b.pendingOverkillPartId == 0)
            return false;
        if (Time.time - b.pendingOverkillTime > 0.35f)
        {
            ClearOverkill(b);
            return false;
        }

        if (GetPartId(killed) != b.pendingOverkillPartId)
            return false;

        overkill = b.pendingOverkillAmount;
        ClearOverkill(b);
        return true;
    }

    public static void ClearOverkill(MarksmanLaserRifleBehaviour b)
    {
        if (b == null)
            return;
        b.pendingOverkillPartId = 0;
        b.pendingOverkillAmount = 0f;
        b.pendingOverkillTime = 0f;
    }

    // -------------------------------------------------------------------------
    // Ids
    // -------------------------------------------------------------------------

    public static int GetPartId(EnemyPart part)
    {
        if (part == null)
            return 0;
        try
        {
            // NetworkObjectId is stable across clients when spawned; fallback instance id.
            if (part.IsSpawned)
                return (int)part.NetworkObjectId;
        }
        catch
        {
            // ignore
        }

        return part.GetInstanceID();
    }

    public static void PruneExpired(MarksmanLaserRifleBehaviour b)
    {
        if (b == null)
            return;

        float now = Time.time;
        PruneMap(b.markExpiries, now);
        PruneMap(b.exposeExpiries, now);

        if (b.openArteryBrainId != 0 && now > b.openArteryExpiry)
        {
            b.openArteryBrainId = 0;
            b.openArteryExpiry = 0f;
        }
    }

    private static void PruneMap(Dictionary<int, float> map, float now)
    {
        if (map == null || map.Count == 0)
            return;

        List<int> dead = null;
        foreach (var kv in map)
        {
            if (now > kv.Value)
            {
                dead ??= new List<int>(4);
                dead.Add(kv.Key);
            }
        }

        if (dead == null)
            return;
        for (int i = 0; i < dead.Count; i++)
            map.Remove(dead[i]);
    }
}
