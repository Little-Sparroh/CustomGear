using System;
using System.Reflection;
using Sparroh.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Caduceus reticle HUD: polarity strip [last | current | next] as colored rects under the crosshair.
/// Also suppresses useless Shocklance charge-line chrome while our gun is active.
/// </summary>
internal static class CaduceusHUD
{
    private const float StripWidth = 84f;
    private const float StripHeight = 10f;
    private const float Gap = 3f;
    private const float SideScale = 0.72f;   // last/next vs current height
    private const float SideAlpha = 0.42f;
    private const float CurrentAlpha = 0.95f;
    private const float Pad = 2f;

    private static HudHandle _handle;
    private static Gun _boundGun;
    private static float _rebuildCooldown;

    private static Image _lastImg;
    private static Image _currentImg;
    private static Image _nextImg;
    private static CaduceusBehaviour.Polarity _shown = (CaduceusBehaviour.Polarity)(-1);

    // Shocklance charge chrome we hide while Caduceus is equipped.
    private static readonly string[] ShocklanceChromeFields =
    {
        "chargeLineL",
        "chargeLineR",
        "pierceDamageLine",
        "augerCharge",
        "centerDot",
        "detachPopup"
    };

    public static void Tick(Gun gun, CaduceusBehaviour b)
    {
        if (gun == null || b == null)
            return;

        if (!gun.IsOwner || !gun.Active)
        {
            SetVisible(false);
            return;
        }

        EnsureHandle(gun);
        if (!HudHandle.IsValid(_handle) || _currentImg == null)
            return;

        _handle.SetActive(true);
        SuppressShocklanceChrome(gun);

        CaduceusBehaviour.Polarity cur = b.CurrentPolarity;
        if (cur == _shown)
            return;
        _shown = cur;

        CaduceusBehaviour.Polarity last = Prev(cur);
        CaduceusBehaviour.Polarity next = Next(cur);

        ApplySwatch(_lastImg, last, SideAlpha, side: true);
        ApplySwatch(_currentImg, cur, CurrentAlpha, side: false);
        ApplySwatch(_nextImg, next, SideAlpha, side: true);
    }

    public static void Hide()
    {
        SetVisible(false);
        _shown = (CaduceusBehaviour.Polarity)(-1);
    }

    private static void SetVisible(bool want)
    {
        if (HudHandle.IsValid(_handle))
            _handle.SetActive(want);
    }

    private static CaduceusBehaviour.Polarity Prev(CaduceusBehaviour.Polarity p) =>
        (CaduceusBehaviour.Polarity)(((int)p + 2) % 3);

    private static CaduceusBehaviour.Polarity Next(CaduceusBehaviour.Polarity p) =>
        (CaduceusBehaviour.Polarity)(((int)p + 1) % 3);

    private static void ApplySwatch(Image img, CaduceusBehaviour.Polarity p, float alpha, bool side)
    {
        if (img == null)
            return;
        Color c = CaduceusBehaviour.PolarityColor(p);
        c.a = alpha;
        img.color = c;

        // Current is full height; sides slightly shorter (visual hierarchy without text).
        if (img.rectTransform != null && side)
        {
            // height driven by layout anchors already; keep color-only for sides
        }
    }

    private static void EnsureHandle(Gun gun)
    {
        if (HudHandle.IsValid(_handle) && _boundGun == gun && _currentImg != null)
            return;

        if (Time.unscaledTime < _rebuildCooldown)
            return;

        _rebuildCooldown = Time.unscaledTime + 0.5f;
        _boundGun = gun;
        _shown = (CaduceusBehaviour.Polarity)(-1);

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

        _lastImg = _currentImg = _nextImg = null;

        try
        {
            // Under reticle — Helminth-style anchor band.
            _handle = HudBuilder.Create("CaduceusPolarityStrip")
                .ParentToReticle()
                .Anchor(0.5f, 0.38f)
                .Pivot(new Vector2(0.5f, 0.5f))
                .Size(StripWidth, StripHeight)
                .WithBackground(UIColors.WithAlpha(UIColors.PanelBg, 0.40f))
                .Build();

            if (!HudHandle.IsValid(_handle) || _handle.Rect == null)
            {
                _handle = null;
                return;
            }

            // Hide default empty text line from HudBuilder.
            if (_handle.Lines != null)
            {
                for (int i = 0; i < _handle.Lines.Length; i++)
                {
                    if (_handle.Lines[i]?.GameObject != null)
                        _handle.Lines[i].GameObject.SetActive(false);
                }
            }

            RectTransform root = _handle.Rect;
            float inset = UITheme.S(Pad);
            float gap = UITheme.S(Gap);
            float innerW = root.sizeDelta.x - inset * 2f;
            float innerH = root.sizeDelta.y - inset * 2f;

            // Width split: side | current | side  →  0.28 | 0.44 | 0.28 of inner (minus gaps)
            float usable = innerW - gap * 2f;
            float sideW = usable * 0.28f;
            float curW = usable * 0.44f;
            float sideH = innerH * SideScale;
            float curH = innerH;

            _lastImg = MakeSwatch("Last", root, sideW, sideH, inset);
            _currentImg = MakeSwatch("Current", root, curW, curH, inset + sideW + gap);
            _nextImg = MakeSwatch("Next", root, sideW, sideH, inset + sideW + gap + curW + gap);

            // Vertical center sides
            CenterVertically(_lastImg.rectTransform, innerH, sideH, inset);
            CenterVertically(_nextImg.rectTransform, innerH, sideH, inset);
            // Current full height
            RectTransform crt = _currentImg.rectTransform;
            crt.anchorMin = new Vector2(0f, 0f);
            crt.anchorMax = new Vector2(0f, 0f);
            crt.pivot = new Vector2(0f, 0f);
            crt.anchoredPosition = new Vector2(inset + sideW + gap, inset);
            crt.sizeDelta = new Vector2(curW, curH);
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[Caduceus] HUD build failed: {ex.Message}");
            _handle = null;
            _lastImg = _currentImg = _nextImg = null;
        }
    }

    private static Image MakeSwatch(string name, RectTransform parent, float w, float h, float x)
    {
        Image img = UIFactory.CreateImage(name, parent, Color.white, raycast: false);
        UIFactory.ApplyWhiteSprite(img);
        RectTransform rt = img.rectTransform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(x, 0f);
        rt.sizeDelta = new Vector2(w, h);
        return img;
    }

    private static void CenterVertically(RectTransform rt, float innerH, float h, float inset)
    {
        if (rt == null)
            return;
        float y = inset + (innerH - h) * 0.5f;
        Vector2 pos = rt.anchoredPosition;
        pos.y = y;
        rt.anchoredPosition = pos;
        Vector2 size = rt.sizeDelta;
        size.y = h;
        rt.sizeDelta = size;
    }

    /// <summary>
    /// Hide Shocklance charge / auger / cable-detach chrome — Caduceus does not use them.
    /// </summary>
    private static void SuppressShocklanceChrome(Gun gun)
    {
        if (gun == null)
            return;
        try
        {
            HUD hud = gun.GetHUD();
            if (hud == null)
                return;

            // Prefer typed ShocklanceHUD fields when present.
            if (hud is ShocklanceHUD)
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                Type t = typeof(ShocklanceHUD);
                for (int i = 0; i < ShocklanceChromeFields.Length; i++)
                {
                    FieldInfo f = t.GetField(ShocklanceChromeFields[i], flags);
                    if (f == null)
                        continue;
                    object val = f.GetValue(hud);
                    if (val is Component c && c != null)
                        c.gameObject.SetActive(false);
                    else if (val is GameObject go && go != null)
                        go.SetActive(false);
                }
            }
        }
        catch
        {
            // ignore — chrome hide is cosmetic
        }
    }
}
