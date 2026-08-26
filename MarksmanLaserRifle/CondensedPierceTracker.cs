using UnityEngine;

/// <summary>
/// Attached to a RailBullet fired under Condensed Munitions.
/// Limits how many targets the bullet may pierce before piercing stops,
/// and tracks ammo spent for laser-charge scaling on hit.
/// </summary>
public sealed class CondensedPierceTracker : MonoBehaviour
{
    /// <summary>Max targets this bullet may damage (including the first).</summary>
    public int maxPierces;

    /// <summary>Targets damaged so far.</summary>
    public int piercedCount;

    /// <summary>Magazine ammo dumped into this shot (for charge scaling).</summary>
    public float ammoSpent;

    /// <summary>True after we've granted the multi-ammo laser charge bonus for this shot.</summary>
    public bool chargeGranted;

    public bool CanPierceMore => piercedCount < maxPierces;

    public void RegisterHit()
    {
        piercedCount++;
    }

    public void ResetState()
    {
        maxPierces = 0;
        piercedCount = 0;
        ammoSpent = 0f;
        chargeGranted = false;
        enabled = false;
    }
}
