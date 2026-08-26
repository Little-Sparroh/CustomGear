using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Draw charge bar with a fixed sweet-spot band (AlwaysPracticeMakesPerfect QTE DNA).
/// Band is not random — it sits at behaviour sweetSpotMin/Max (default 80–90%).
/// </summary>
internal static class HeavenPiercerDrawHud
{
    private static GameObject sharedBarRoot;
    private static RectTransform trackRt;
    private static RectTransform sweetSpotRt;
    private static Image sweetSpotImage;
    private static RectTransform cursorRt;
    private static Image cursorImage;
    private static TextMeshProUGUI promptLabel;
    private static bool barVisible;

    private static float feedbackUntil;
    private static FeedbackKind feedbackKind;
    private static float lastProgress01;
    private static float lastSweetMin01 = HpBalance.SweetSpotMin;
    private static float lastSweetMax01 = HpBalance.SweetSpotMax;
    private static bool lastInWindow;

    private enum FeedbackKind
    {
        None,
        Success,
        Miss
    }

    // Bar layout (reference 1920x1080) — slightly higher than APMP reload bar
    private const float BarWidth = 280f;
    private const float BarHeight = 14f;
    private const float CursorWidth = 3f;
    private const float AnchorY = 0.34f;

    private static readonly Color TrackColor = new(0.05f, 0.07f, 0.10f, 0.72f);
    private static readonly Color SweetSpotIdle = new(0.98f, 0.82f, 0.35f, 0.45f);
    private static readonly Color SweetSpotActive = new(0.98f, 0.82f, 0.35f, 0.92f);
    private static readonly Color SweetSpotSuccess = new(0.20f, 0.90f, 0.45f, 0.95f);
    private static readonly Color SweetSpotMiss = new(0.95f, 0.30f, 0.35f, 0.90f);
    private static readonly Color CursorIdle = new(0.95f, 0.96f, 0.98f, 0.95f);
    private static readonly Color CursorActive = new(1f, 0.92f, 0.35f, 1f);
    private static readonly Color PromptColor = new(1f, 0.92f, 0.35f, 1f);

    internal static void Cleanup()
    {
        HideBar();
        if (sharedBarRoot != null)
        {
            Object.Destroy(sharedBarRoot);
            sharedBarRoot = null;
            trackRt = null;
            sweetSpotRt = null;
            sweetSpotImage = null;
            cursorRt = null;
            cursorImage = null;
            promptLabel = null;
        }
    }

    /// <summary>Brief green flash after a sweet-spot loose.</summary>
    internal static void FlashSweetSuccess()
    {
        feedbackKind = FeedbackKind.Success;
        feedbackUntil = Time.unscaledTime + 0.35f;
        UpdateBarVisual(lastProgress01, lastSweetMin01, lastSweetMax01, inWindow: true);
    }

    /// <summary>
    /// Drive the bar from the active local Heaven Piercer, or expire feedback / hide.
    /// </summary>
    internal static void Tick(Gun gun, HeavenPiercerBehaviour behaviour)
    {
        TickFeedbackExpiry();

        if (gun == null || behaviour == null || !gun.IsOwner || !gun.Active)
        {
            if (feedbackKind == FeedbackKind.None && barVisible)
                HideBar();
            else if (feedbackKind != FeedbackKind.None)
                UpdateBarVisual(lastProgress01, lastSweetMin01, lastSweetMax01, lastInWindow);
            return;
        }

        if (feedbackKind != FeedbackKind.None)
        {
            UpdateBarVisual(lastProgress01, lastSweetMin01, lastSweetMax01, lastInWindow);
            return;
        }

        ref ChargeData charge = ref gun.GunData.chargeData;
        if (!charge.Enabled)
        {
            if (barVisible)
                HideBar();
            return;
        }

        bool drawing = charge.isCurrentlyCharging ||
                       (charge.time > 0.01f && charge.isChargingUp);

        // Also show briefly while charge is still visible after release edge
        // (stopCharge window) so the player sees where they loosed.
        bool justReleased = !charge.isCurrentlyCharging &&
                            charge.stopChargeValue > 0f &&
                            Time.time - charge.stopChargeTime < 0.12f;

        if (!drawing && !justReleased)
        {
            if (barVisible)
                HideBar();
            return;
        }

        float progress01 = Mathf.Clamp01(charge.NormalizedChargeTime);
        if (justReleased && charge.duration > 0f)
            progress01 = Mathf.Clamp01(charge.stopChargeValue / charge.duration);

        float sweetMin = behaviour.WeaponData.sweetSpotMin;
        float sweetMax = behaviour.WeaponData.sweetSpotMax;
        bool inWindow = progress01 >= sweetMin && progress01 <= sweetMax;

        lastProgress01 = progress01;
        lastSweetMin01 = sweetMin;
        lastSweetMax01 = sweetMax;
        lastInWindow = inWindow;

        UpdateBarVisual(progress01, sweetMin, sweetMax, inWindow);
    }

    private static void TickFeedbackExpiry()
    {
        if (feedbackKind == FeedbackKind.None)
            return;

        if (Time.unscaledTime < feedbackUntil)
            return;

        feedbackKind = FeedbackKind.None;
        HideBar();
    }

    private static void EnsureBar()
    {
        if (sharedBarRoot != null)
            return;

        sharedBarRoot = new GameObject("HeavenPiercer_DrawBar");
        Object.DontDestroyOnLoad(sharedBarRoot);

        var canvas = sharedBarRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9000;

        var scaler = sharedBarRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        sharedBarRoot.AddComponent<GraphicRaycaster>();

        var containerGo = new GameObject("Container");
        containerGo.transform.SetParent(sharedBarRoot.transform, false);
        var containerRt = containerGo.AddComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0.5f, AnchorY);
        containerRt.anchorMax = new Vector2(0.5f, AnchorY);
        containerRt.pivot = new Vector2(0.5f, 0.5f);
        containerRt.sizeDelta = new Vector2(BarWidth + 24f, BarHeight + 40f);

        var trackGo = new GameObject("Track");
        trackGo.transform.SetParent(containerGo.transform, false);
        trackRt = trackGo.AddComponent<RectTransform>();
        trackRt.anchorMin = new Vector2(0.5f, 0.5f);
        trackRt.anchorMax = new Vector2(0.5f, 0.5f);
        trackRt.pivot = new Vector2(0.5f, 0.5f);
        trackRt.sizeDelta = new Vector2(BarWidth, BarHeight);
        var trackImage = trackGo.AddComponent<Image>();
        trackImage.color = TrackColor;
        trackImage.raycastTarget = false;

        var outlineGo = new GameObject("Outline");
        outlineGo.transform.SetParent(containerGo.transform, false);
        var outlineRt = outlineGo.AddComponent<RectTransform>();
        outlineRt.anchorMin = new Vector2(0.5f, 0.5f);
        outlineRt.anchorMax = new Vector2(0.5f, 0.5f);
        outlineRt.pivot = new Vector2(0.5f, 0.5f);
        outlineRt.sizeDelta = new Vector2(BarWidth + 4f, BarHeight + 4f);
        outlineRt.SetSiblingIndex(0);
        var outlineImage = outlineGo.AddComponent<Image>();
        outlineImage.color = new Color(0.25f, 0.40f, 0.50f, 0.55f);
        outlineImage.raycastTarget = false;

        var sweetGo = new GameObject("SweetSpot");
        sweetGo.transform.SetParent(trackGo.transform, false);
        sweetSpotRt = sweetGo.AddComponent<RectTransform>();
        sweetSpotRt.anchorMin = new Vector2(0f, 0f);
        sweetSpotRt.anchorMax = new Vector2(0f, 1f);
        sweetSpotRt.pivot = new Vector2(0f, 0.5f);
        sweetSpotRt.anchoredPosition = Vector2.zero;
        sweetSpotRt.sizeDelta = new Vector2(20f, 0f);
        sweetSpotImage = sweetGo.AddComponent<Image>();
        sweetSpotImage.color = SweetSpotIdle;
        sweetSpotImage.raycastTarget = false;

        var cursorGo = new GameObject("Cursor");
        cursorGo.transform.SetParent(trackGo.transform, false);
        cursorRt = cursorGo.AddComponent<RectTransform>();
        cursorRt.anchorMin = new Vector2(0f, 0.5f);
        cursorRt.anchorMax = new Vector2(0f, 0.5f);
        cursorRt.pivot = new Vector2(0.5f, 0.5f);
        cursorRt.sizeDelta = new Vector2(CursorWidth, BarHeight + 10f);
        cursorRt.anchoredPosition = Vector2.zero;
        cursorImage = cursorGo.AddComponent<Image>();
        cursorImage.color = CursorIdle;
        cursorImage.raycastTarget = false;

        var labelGo = new GameObject("Prompt");
        labelGo.transform.SetParent(containerGo.transform, false);
        var labelRt = labelGo.AddComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0.5f, 0.5f);
        labelRt.anchorMax = new Vector2(0.5f, 0.5f);
        labelRt.pivot = new Vector2(0.5f, 0f);
        labelRt.anchoredPosition = new Vector2(0f, BarHeight * 0.5f + 6f);
        labelRt.sizeDelta = new Vector2(BarWidth, 28f);
        promptLabel = labelGo.AddComponent<TextMeshProUGUI>();
        promptLabel.text = "LOOSE";
        promptLabel.alignment = TextAlignmentOptions.Center;
        promptLabel.fontSize = 22f;
        promptLabel.fontStyle = FontStyles.Bold;
        promptLabel.color = PromptColor;
        promptLabel.raycastTarget = false;
        if (TMP_Settings.defaultFontAsset != null)
            promptLabel.font = TMP_Settings.defaultFontAsset;
        promptLabel.gameObject.SetActive(false);

        sharedBarRoot.SetActive(false);
        barVisible = false;
    }

    private static void UpdateBarVisual(float progress01, float sweetMin01, float sweetMax01, bool inWindow)
    {
        EnsureBar();

        progress01 = Mathf.Clamp01(progress01);
        sweetMin01 = Mathf.Clamp01(sweetMin01);
        sweetMax01 = Mathf.Clamp01(Mathf.Max(sweetMax01, sweetMin01 + 0.01f));

        float sweetX = sweetMin01 * BarWidth;
        float sweetW = Mathf.Max(4f, (sweetMax01 - sweetMin01) * BarWidth);
        sweetSpotRt.anchoredPosition = new Vector2(sweetX, 0f);
        sweetSpotRt.sizeDelta = new Vector2(sweetW, 0f);

        cursorRt.anchoredPosition = new Vector2(progress01 * BarWidth, 0f);

        if (feedbackKind == FeedbackKind.Success)
        {
            sweetSpotImage.color = SweetSpotSuccess;
            cursorImage.color = SweetSpotSuccess;
            if (promptLabel != null)
            {
                promptLabel.text = "NICE";
                promptLabel.color = SweetSpotSuccess;
                promptLabel.gameObject.SetActive(true);
            }
        }
        else if (feedbackKind == FeedbackKind.Miss)
        {
            sweetSpotImage.color = SweetSpotMiss;
            cursorImage.color = SweetSpotMiss;
            if (promptLabel != null)
            {
                promptLabel.text = "MISS";
                promptLabel.color = SweetSpotMiss;
                promptLabel.gameObject.SetActive(true);
            }
        }
        else if (inWindow)
        {
            float pulse = 0.85f + 0.15f * Mathf.Sin(Time.unscaledTime * 14f);
            var c = SweetSpotActive;
            c.a *= pulse;
            sweetSpotImage.color = c;
            cursorImage.color = CursorActive;
            if (promptLabel != null)
            {
                promptLabel.text = "LOOSE";
                promptLabel.color = PromptColor;
                promptLabel.gameObject.SetActive(true);
            }
        }
        else
        {
            sweetSpotImage.color = SweetSpotIdle;
            cursorImage.color = CursorIdle;
            if (promptLabel != null)
                promptLabel.gameObject.SetActive(false);
        }

        if (!sharedBarRoot.activeSelf)
            sharedBarRoot.SetActive(true);
        barVisible = true;
    }

    private static void HideBar()
    {
        feedbackKind = FeedbackKind.None;
        if (sharedBarRoot != null && sharedBarRoot.activeSelf)
            sharedBarRoot.SetActive(false);
        barVisible = false;
    }
}
