using System;
using Sparroh.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Compact Helminth status chip under the reticle.
/// Vitality already mirrors the ammo counter — this only shows organism state:
/// fill = V fraction, color = Well-Fed / Hungry / Starving / Feed / empty.
/// </summary>
internal static class HelminthHUD
{
    private const float ChipWidth = 72f;
    private const float ChipHeight = 6f;
    private const float Pad = 1.5f;

    private static HudHandle _handle;
    private static Gun _boundGun;
    private static float _rebuildCooldown;
    private static Image _fill;
    private static Image _track;

    public static void Tick(Gun gun, HelminthBehaviour b)
    {
        if (gun == null || b == null)
            return;

        if (!gun.IsOwner || !gun.Active)
        {
            SetVisible(false);
            return;
        }

        EnsureHandle(gun);
        if (!HudHandle.IsValid(_handle) || _fill == null)
            return;

        _handle.SetActive(true);

        float n = Mathf.Clamp01(b.VitalityNormalized);
        Color c = StateColor(b, n);

        // Soft pulse while Feeding so the channel is readable without text.
        if (b.isFeeding)
        {
            float pulse = 0.72f + 0.28f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 9f));
            c = UIColors.WithAlpha(c, pulse);
        }

        _fill.color = c;
        SetFillWidth(n);

        if (_track != null)
        {
            // Dim track when empty / starving so the chip still reads as "present but dry".
            float trackA = n <= 0.001f || b.IsStarving ? 0.55f : 0.85f;
            _track.color = UIColors.WithAlpha(UIColors.ProgressTrack, trackA);
        }
    }

    public static void Hide()
    {
        SetVisible(false);
    }

    private static void SetVisible(bool want)
    {
        if (HudHandle.IsValid(_handle))
            _handle.SetActive(want);
    }

    private static void SetFillWidth(float normalized)
    {
        if (_fill == null)
            return;

        RectTransform rt = _fill.rectTransform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(Mathf.Clamp01(normalized), 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Feed / economy state only — not ammo counts (those live on the vanilla counter).
    /// </summary>
    private static Color StateColor(HelminthBehaviour b, float n)
    {
        if (b.isFeeding)
            return UIColors.Rose;
        if (n <= 0.001f || !b.CanAffordShot())
            return UIColors.Amber;
        if (b.IsStarving)
            return UIColors.Amber;
        if (b.IsWellFed)
            return UIColors.Shamrock;
        // Hungry mid-band
        return UIColors.Sky;
    }

    private static void EnsureHandle(Gun gun)
    {
        if (HudHandle.IsValid(_handle) && _boundGun == gun && _fill != null)
            return;

        if (Time.unscaledTime < _rebuildCooldown)
            return;

        _rebuildCooldown = Time.unscaledTime + 0.5f;
        _boundGun = gun;

        try
        {
            if (HudHandle.IsValid(_handle))
            {
                _handle.Destroy();
                _handle = null;
            }
        }
        catch
        {
            _handle = null;
        }

        _fill = null;
        _track = null;

        try
        {
            // Tiny chip under reticle — no text lines (ammo counter already shows shots).
            _handle = HudBuilder.Create("HelminthStatusChip")
                .ParentToReticle()
                .Anchor(0.5f, 0.42f)
                .Pivot(new Vector2(0.5f, 0.5f))
                .Size(ChipWidth, ChipHeight)
                .WithBackground(UIColors.WithAlpha(UIColors.PanelBg, 0.35f))
                .Build();

            if (!HudHandle.IsValid(_handle) || _handle.Rect == null)
            {
                _handle = null;
                return;
            }

            // Hide the default empty text line HudBuilder always creates.
            if (_handle.Lines != null)
            {
                for (int i = 0; i < _handle.Lines.Length; i++)
                {
                    if (_handle.Lines[i]?.GameObject != null)
                        _handle.Lines[i].GameObject.SetActive(false);
                }
            }

            // Track + fill inside the chip.
            float inset = UITheme.S(Pad);
            _track = UIFactory.CreateImage("Track", _handle.Rect, UIColors.ProgressTrack, raycast: false);
            UIFactory.ApplyWhiteSprite(_track);
            UIHelpers.SetFillParent(_track.rectTransform, inset);

            _fill = UIFactory.CreateImage("Fill", _track.rectTransform, UIColors.Sky, raycast: false);
            UIFactory.ApplyWhiteSprite(_fill);
            RectTransform fillRt = _fill.rectTransform;
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(1f, 1f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            fillRt.pivot = new Vector2(0f, 0.5f);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Helminth] HUD build failed: {ex.Message}");
            _handle = null;
            _fill = null;
            _track = null;
        }
    }
}
