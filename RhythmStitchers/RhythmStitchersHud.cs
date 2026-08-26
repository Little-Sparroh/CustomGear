using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Custom crosshair + pendulum metronome for Rhythm Stitchers.
/// Center dot aim point. Bottom arc: one needle sweeps L ↔ R.
/// Sweet zones at the LEFT and RIGHT ends — fire that channel when the
/// needle is in that end's window (alternate-trigger skill expression).
/// </summary>
internal static class RhythmStitchersHud
{
    private static readonly FieldInfo HudField = AccessTools.Field(typeof(Gun), "hud");
    private static readonly FieldInfo CrosshairLinesField = AccessTools.Field(typeof(GunHUD), "crosshairLines");
    private static readonly FieldInfo CrosshairField = AccessTools.Field(typeof(GunHUD), "crosshair");

    private static Sprite _whiteSprite;
    private static GameObject _root;
    private static RectTransform _rootRt;
    private static Canvas _canvas;
    private static bool _usingOverlayFallback;

    private static Image _dot;
    private static RectTransform _arcContainer;
    private static readonly Image[] _arcSegments = new Image[ArcSegmentCount];
    private static Image _leftSweet;
    private static Image _rightSweet;
    private static RectTransform _cursorRt;
    private static Image _cursor;
    private static Image _leftLabel;
    private static Image _rightLabel;

    private static Gun _boundGun;
    private static readonly List<Graphic> SuppressedGraphics = new();
    private static readonly List<bool> SuppressedWasEnabled = new();

    private static float _leftFlashUntil;
    private static float _rightFlashUntil;
    private static bool _leftFlashSuccess;
    private static bool _rightFlashSuccess;

    private static bool _visible;

    // Layout (reference 1920x1080)
    private const float ArcRadius = 48f;
    private const float ArcThickness = 4f;
    private const float DotSize = 6f;
    private const float CursorSize = 10f;
    private const float CursorThickness = 3.5f;
    private const float SweetMarkerSize = 10f;
    private const float FlashDuration = 0.28f;
    private const int SortingOrder = 8500;
    private const int ArcSegmentCount = 24;

    // Bottom semicircle: angle 180° (left) → 0° (right), Unity UI (0=right, 90=up).
    private const float ArcAngleLeft = 180f;
    private const float ArcAngleRight = 0f;

    private static readonly Color TrackIdle = new(0.50f, 0.58f, 0.68f, 0.40f);
    private static readonly Color LeftAccent = new(0.45f, 0.85f, 0.95f, 0.90f);
    private static readonly Color RightAccent = new(0.95f, 0.55f, 0.85f, 0.90f);
    private static readonly Color SweetIdle = new(0.98f, 0.88f, 0.40f, 0.35f);
    private static readonly Color SweetActive = new(0.98f, 0.88f, 0.40f, 0.95f);
    private static readonly Color CursorIdle = new(0.95f, 0.96f, 0.98f, 0.95f);
    private static readonly Color CursorActive = new(1f, 0.92f, 0.35f, 1f);
    private static readonly Color DotColor = new(0.95f, 0.97f, 1f, 0.92f);
    private static readonly Color SuccessColor = new(0.25f, 0.95f, 0.50f, 0.95f);
    private static readonly Color MissColor = new(0.90f, 0.35f, 0.40f, 0.75f);

    public static void Show(Gun gun, RhythmStitchersBehaviour behaviour)
    {
        if (gun == null || behaviour == null)
            return;

        EnsureBuilt();
        if (_root == null)
            return;

        _boundGun = gun;
        SuppressVanillaCrosshair(gun, suppress: true);
        ReparentToBestAnchor();
        _root.SetActive(true);
        _visible = true;
        Tick(behaviour);
    }

    public static void Hide()
    {
        if (_boundGun != null)
            SuppressVanillaCrosshair(_boundGun, suppress: false);

        _boundGun = null;
        _leftFlashUntil = 0f;
        _rightFlashUntil = 0f;

        if (_root != null && _root.activeSelf)
            _root.SetActive(false);
        _visible = false;
    }

    public static void Tick(RhythmStitchersBehaviour behaviour)
    {
        if (behaviour == null || !_visible)
            return;

        EnsureBuilt();
        if (_root == null)
            return;

        if (_usingOverlayFallback)
            ReparentToBestAnchor();

        if (_boundGun != null && SuppressedGraphics.Count == 0)
            SuppressVanillaCrosshair(_boundGun, suppress: true);

        float pendulum = behaviour.GetPendulum01(); // 0 = left end, 1 = right end
        bool leftSweet = behaviour.IsChannelOnBeat(RhythmStitchersBehaviour.Channel.Left);
        bool rightSweet = behaviour.IsChannelOnBeat(RhythmStitchersBehaviour.Channel.Right);

        PlaceOnArc(_cursorRt, pendulum);
        if (_cursorRt != null)
        {
            float angle = Mathf.Lerp(ArcAngleLeft, ArcAngleRight, pendulum);
            _cursorRt.localEulerAngles = new Vector3(0f, 0f, angle - 90f);
        }

        // Sweet markers sit at the ends
        PlaceOnArc(_leftSweet != null ? _leftSweet.rectTransform : null, 0f);
        PlaceOnArc(_rightSweet != null ? _rightSweet.rectTransform : null, 1f);

        float now = Time.unscaledTime;
        bool leftFlashing = now < _leftFlashUntil;
        bool rightFlashing = now < _rightFlashUntil;

        // Arc track tint: mild L/R gradient via segment colors
        for (int i = 0; i < ArcSegmentCount; i++)
        {
            var seg = _arcSegments[i];
            if (seg == null)
                continue;
            float t = ArcSegmentCount <= 1 ? 0.5f : i / (float)(ArcSegmentCount - 1);
            seg.color = Color.Lerp(LeftAccent, RightAccent, t);
            var c = seg.color;
            c.a = TrackIdle.a;
            seg.color = c;
        }

        // Sweet end markers
        if (_leftSweet != null)
        {
            if (leftFlashing)
                _leftSweet.color = _leftFlashSuccess ? SuccessColor : MissColor;
            else if (leftSweet)
            {
                float pulse = 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * 14f);
                var c = SweetActive;
                c.a *= pulse;
                _leftSweet.color = c;
            }
            else
                _leftSweet.color = SweetIdle;
        }

        if (_rightSweet != null)
        {
            if (rightFlashing)
                _rightSweet.color = _rightFlashSuccess ? SuccessColor : MissColor;
            else if (rightSweet)
            {
                float pulse = 0.75f + 0.25f * Mathf.Sin(Time.unscaledTime * 14f);
                var c = SweetActive;
                c.a *= pulse;
                _rightSweet.color = c;
            }
            else
                _rightSweet.color = SweetIdle;
        }

        // Side pip labels (small L/R end caps)
        if (_leftLabel != null)
        {
            _leftLabel.color = leftFlashing
                ? (_leftFlashSuccess ? SuccessColor : MissColor)
                : (leftSweet ? LeftAccent : Color.Lerp(TrackIdle, LeftAccent, 0.5f));
        }
        if (_rightLabel != null)
        {
            _rightLabel.color = rightFlashing
                ? (_rightFlashSuccess ? SuccessColor : MissColor)
                : (rightSweet ? RightAccent : Color.Lerp(TrackIdle, RightAccent, 0.5f));
        }

        // Cursor
        if (_cursor != null)
        {
            if (leftFlashing || rightFlashing)
            {
                bool ok = (leftFlashing && _leftFlashSuccess) || (rightFlashing && _rightFlashSuccess);
                bool miss = (leftFlashing && !_leftFlashSuccess) || (rightFlashing && !_rightFlashSuccess);
                _cursor.color = ok ? SuccessColor : (miss ? MissColor : CursorIdle);
            }
            else if (leftSweet || rightSweet)
                _cursor.color = CursorActive;
            else
                _cursor.color = CursorIdle;
        }

        // Center dot
        if (_dot != null)
        {
            if (leftFlashing || rightFlashing)
            {
                bool ok = (leftFlashing && _leftFlashSuccess) || (rightFlashing && _rightFlashSuccess);
                bool miss = (leftFlashing && !_leftFlashSuccess) || (rightFlashing && !_rightFlashSuccess);
                _dot.color = ok ? SuccessColor : (miss ? MissColor : DotColor);
            }
            else if (leftSweet || rightSweet)
            {
                float pulse = 0.85f + 0.15f * Mathf.Sin(Time.unscaledTime * 14f);
                var c = DotColor;
                c.a = DotColor.a * pulse;
                _dot.color = c;
            }
            else
                _dot.color = DotColor;
        }
    }

    public static void NotifyShot(RhythmStitchersBehaviour.Channel channel, bool onBeat)
    {
        float until = Time.unscaledTime + FlashDuration;
        if (channel == RhythmStitchersBehaviour.Channel.Right)
        {
            _rightFlashUntil = until;
            _rightFlashSuccess = onBeat;
        }
        else
        {
            _leftFlashUntil = until;
            _leftFlashSuccess = onBeat;
        }
    }

    public static void Cleanup()
    {
        Hide();
        SuppressedGraphics.Clear();
        SuppressedWasEnabled.Clear();

        if (_root != null)
        {
            Object.Destroy(_root);
            _root = null;
        }

        _rootRt = null;
        _canvas = null;
        _dot = null;
        _arcContainer = null;
        for (int i = 0; i < _arcSegments.Length; i++)
            _arcSegments[i] = null;
        _leftSweet = _rightSweet = null;
        _cursorRt = null;
        _cursor = null;
        _leftLabel = _rightLabel = null;
        _usingOverlayFallback = false;
    }

    // ── Build ──────────────────────────────────────────────────────────────

    private static void EnsureBuilt()
    {
        if (_root != null)
            return;

        _root = new GameObject("RhythmStitchers_MetronomeHud");
        Object.DontDestroyOnLoad(_root);

        _canvas = _root.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = SortingOrder;

        var scaler = _root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        _root.AddComponent<GraphicRaycaster>();

        _rootRt = _root.GetComponent<RectTransform>();
        if (_rootRt == null)
            _rootRt = _root.AddComponent<RectTransform>();

        float box = ArcRadius * 2f + 24f;
        var container = CreateRect("Container", _root.transform);
        container.anchorMin = container.anchorMax = new Vector2(0.5f, 0.5f);
        container.pivot = new Vector2(0.5f, 0.5f);
        container.sizeDelta = new Vector2(box, box);
        container.anchoredPosition = Vector2.zero;

        // Arc segments along bottom semicircle (left → right)
        _arcContainer = CreateRect("Arc", container);
        _arcContainer.anchorMin = _arcContainer.anchorMax = new Vector2(0.5f, 0.5f);
        _arcContainer.pivot = new Vector2(0.5f, 0.5f);
        _arcContainer.sizeDelta = new Vector2(box, box);
        _arcContainer.anchoredPosition = Vector2.zero;

        for (int i = 0; i < ArcSegmentCount; i++)
        {
            float t = ArcSegmentCount <= 1 ? 0.5f : i / (float)(ArcSegmentCount - 1);
            var seg = CreateImage($"Seg{i}", _arcContainer, TrackIdle);
            seg.rectTransform.sizeDelta = new Vector2(ArcThickness + 1f, ArcThickness + 1f);
            PlaceOnArc(seg.rectTransform, t);
            _arcSegments[i] = seg;
        }

        // End sweet markers (larger ticks at L/R tips)
        _leftSweet = CreateImage("LeftSweet", container, SweetIdle);
        _leftSweet.rectTransform.sizeDelta = new Vector2(SweetMarkerSize, SweetMarkerSize);
        PlaceOnArc(_leftSweet.rectTransform, 0f);

        _rightSweet = CreateImage("RightSweet", container, SweetIdle);
        _rightSweet.rectTransform.sizeDelta = new Vector2(SweetMarkerSize, SweetMarkerSize);
        PlaceOnArc(_rightSweet.rectTransform, 1f);

        // Small end caps (channel identity)
        _leftLabel = CreateImage("LeftCap", container, LeftAccent);
        _leftLabel.rectTransform.sizeDelta = new Vector2(5f, 14f);
        PlaceOnArc(_leftLabel.rectTransform, 0f);
        _leftLabel.rectTransform.localEulerAngles = new Vector3(0f, 0f, 90f);

        _rightLabel = CreateImage("RightCap", container, RightAccent);
        _rightLabel.rectTransform.sizeDelta = new Vector2(5f, 14f);
        PlaceOnArc(_rightLabel.rectTransform, 1f);
        _rightLabel.rectTransform.localEulerAngles = new Vector3(0f, 0f, 90f);

        // Pendulum cursor (one needle)
        _cursor = CreateImage("Cursor", container, CursorIdle);
        _cursorRt = _cursor.rectTransform;
        _cursorRt.sizeDelta = new Vector2(CursorThickness, CursorSize);
        _cursorRt.pivot = new Vector2(0.5f, 0.5f);
        PlaceOnArc(_cursorRt, 0.5f);

        // Center aim dot
        _dot = CreateImage("Dot", container, DotColor);
        var dotRt = _dot.rectTransform;
        dotRt.anchorMin = dotRt.anchorMax = new Vector2(0.5f, 0.5f);
        dotRt.pivot = new Vector2(0.5f, 0.5f);
        dotRt.sizeDelta = new Vector2(DotSize, DotSize);
        dotRt.anchoredPosition = Vector2.zero;

        _root.SetActive(false);
        _visible = false;
        _usingOverlayFallback = true;
    }

    /// <summary>
    /// Place a rect on the bottom semicircle.
    /// pendulum01: 0 = left tip (180°), 1 = right tip (0°).
    /// </summary>
    private static void PlaceOnArc(RectTransform rt, float pendulum01)
    {
        if (rt == null)
            return;

        pendulum01 = Mathf.Clamp01(pendulum01);
        float angleDeg = Mathf.Lerp(ArcAngleLeft, ArcAngleRight, pendulum01);
        float rad = angleDeg * Mathf.Deg2Rad;
        float x = Mathf.Cos(rad) * ArcRadius;
        float y = Mathf.Sin(rad) * ArcRadius;
        rt.anchoredPosition = new Vector2(x, y);
    }

    private static void ReparentToBestAnchor()
    {
        if (_root == null)
            return;

        Transform reticle = null;
        try
        {
            if (Player.LocalPlayer != null &&
                Player.LocalPlayer.PlayerLook != null &&
                Player.LocalPlayer.PlayerLook.Reticle != null)
            {
                reticle = Player.LocalPlayer.PlayerLook.Reticle;
            }
        }
        catch
        {
            reticle = null;
        }

        if (reticle != null)
        {
            if (_root.transform.parent != reticle)
            {
                if (_canvas != null)
                {
                    var scaler = _root.GetComponent<CanvasScaler>();
                    if (scaler != null)
                        Object.Destroy(scaler);
                    var raycaster = _root.GetComponent<GraphicRaycaster>();
                    if (raycaster != null)
                        Object.Destroy(raycaster);
                    Object.Destroy(_canvas);
                    _canvas = null;
                }

                _root.transform.SetParent(reticle, false);
                if (_rootRt == null)
                    _rootRt = _root.GetComponent<RectTransform>();
                if (_rootRt != null)
                {
                    float box = ArcRadius * 2f + 24f;
                    _rootRt.anchorMin = _rootRt.anchorMax = new Vector2(0.5f, 0.5f);
                    _rootRt.pivot = new Vector2(0.5f, 0.5f);
                    _rootRt.anchoredPosition = Vector2.zero;
                    _rootRt.sizeDelta = new Vector2(box, box);
                    _rootRt.localScale = Vector3.one;
                    _rootRt.localRotation = Quaternion.identity;
                }

                _usingOverlayFallback = false;
            }
        }
        else if (!_usingOverlayFallback || _root.transform.parent != null)
        {
            if (_root.transform.parent != null)
                _root.transform.SetParent(null, false);

            if (_canvas == null)
            {
                _canvas = _root.GetComponent<Canvas>();
                if (_canvas == null)
                    _canvas = _root.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = SortingOrder;

                if (_root.GetComponent<CanvasScaler>() == null)
                {
                    var scaler = _root.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920f, 1080f);
                    scaler.matchWidthOrHeight = 0.5f;
                }

                if (_root.GetComponent<GraphicRaycaster>() == null)
                    _root.AddComponent<GraphicRaycaster>();
            }

            Object.DontDestroyOnLoad(_root);
            _usingOverlayFallback = true;
        }
    }

    private static void SuppressVanillaCrosshair(Gun gun, bool suppress)
    {
        if (!suppress)
        {
            for (int i = 0; i < SuppressedGraphics.Count; i++)
            {
                var g = SuppressedGraphics[i];
                if (g != null)
                    g.enabled = i < SuppressedWasEnabled.Count && SuppressedWasEnabled[i];
            }
            SuppressedGraphics.Clear();
            SuppressedWasEnabled.Clear();
            return;
        }

        if (SuppressedGraphics.Count > 0)
            return;

        try
        {
            var hud = HudField?.GetValue(gun) as GunHUD;
            if (hud == null)
                return;

            var lines = CrosshairLinesField?.GetValue(hud) as Graphic[];
            if (lines != null)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] == null)
                        continue;
                    SuppressedGraphics.Add(lines[i]);
                    SuppressedWasEnabled.Add(lines[i].enabled);
                    lines[i].enabled = false;
                }
            }

            var crosshair = CrosshairField?.GetValue(hud) as Transform;
            if (crosshair != null)
            {
                var graphics = crosshair.GetComponentsInChildren<Graphic>(true);
                for (int i = 0; i < graphics.Length; i++)
                {
                    var g = graphics[i];
                    if (g == null || SuppressedGraphics.Contains(g))
                        continue;
                    SuppressedGraphics.Add(g);
                    SuppressedWasEnabled.Add(g.enabled);
                    g.enabled = false;
                }
            }
        }
        catch
        {
            // HUD may not be ready
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Sprite WhiteSprite()
    {
        if (_whiteSprite != null)
            return _whiteSprite;
        var tex = Texture2D.whiteTexture;
        _whiteSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        return _whiteSprite;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        var rt = CreateRect(name, parent);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = WhiteSprite();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }
}
