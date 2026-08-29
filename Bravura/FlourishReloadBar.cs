using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Fixed-band Flourish reload QTE bar (pattern from AlwaysPracticeMakesPerfect).
/// Screen-space track + sweet-spot + cursor. Owned per BravuraBehaviour instance.
/// </summary>
internal sealed class FlourishReloadBar
{
    private GameObject root;
    private RectTransform trackRt;
    private RectTransform sweetSpotRt;
    private Image sweetSpotImage;
    private RectTransform cursorRt;
    private Image cursorImage;
    private TextMeshProUGUI promptLabel;
    private bool visible;

    private float feedbackUntil;
    private FeedbackKind feedback = FeedbackKind.None;

    private const float BarWidth = 280f;
    private const float BarHeight = 14f;
    private const float CursorWidth = 3f;
    private const float AnchorY = 0.30f;

    private static readonly Color TrackColor = new(0.05f, 0.07f, 0.10f, 0.72f);
    private static readonly Color SweetSpotIdle = new(0.98f, 0.82f, 0.35f, 0.45f);
    private static readonly Color SweetSpotActive = new(0.98f, 0.82f, 0.35f, 0.92f);
    private static readonly Color SweetSpotSuccess = new(0.20f, 0.90f, 0.45f, 0.95f);
    private static readonly Color SweetSpotMiss = new(0.95f, 0.30f, 0.35f, 0.90f);
    private static readonly Color CursorIdle = new(0.95f, 0.96f, 0.98f, 0.95f);
    private static readonly Color CursorActive = new(1f, 0.92f, 0.35f, 1f);
    private static readonly Color PromptColor = new(1f, 0.92f, 0.35f, 1f);
    private static readonly Color OutlineColor = new(0.45f, 0.35f, 0.15f, 0.55f);

    private enum FeedbackKind
    {
        None,
        Success,
        Miss
    }

    public void Show(float progress01, float sweetMin01, float sweetMax01, bool inWindow)
    {
        Ensure();
        TickFeedbackExpiry();

        progress01 = Mathf.Clamp01(progress01);
        sweetMin01 = Mathf.Clamp01(sweetMin01);
        sweetMax01 = Mathf.Clamp01(Mathf.Max(sweetMax01, sweetMin01 + 0.01f));

        float sweetX = sweetMin01 * BarWidth;
        float sweetW = Mathf.Max(4f, (sweetMax01 - sweetMin01) * BarWidth);
        sweetSpotRt.anchoredPosition = new Vector2(sweetX, 0f);
        sweetSpotRt.sizeDelta = new Vector2(sweetW, 0f);
        cursorRt.anchoredPosition = new Vector2(progress01 * BarWidth, 0f);

        if (feedback == FeedbackKind.Success)
        {
            sweetSpotImage.color = SweetSpotSuccess;
            cursorImage.color = SweetSpotSuccess;
            SetPrompt("NICE", SweetSpotSuccess, true);
        }
        else if (feedback == FeedbackKind.Miss)
        {
            sweetSpotImage.color = SweetSpotMiss;
            cursorImage.color = SweetSpotMiss;
            SetPrompt("MISS", SweetSpotMiss, true);
        }
        else if (inWindow)
        {
            float pulse = 0.85f + 0.15f * Mathf.Sin(Time.unscaledTime * 14f);
            var c = SweetSpotActive;
            c.a *= pulse;
            sweetSpotImage.color = c;
            cursorImage.color = CursorActive;
            SetPrompt("FIRE", PromptColor, true);
        }
        else
        {
            sweetSpotImage.color = SweetSpotIdle;
            cursorImage.color = CursorIdle;
            SetPrompt(null, PromptColor, false);
        }

        if (!root.activeSelf)
            root.SetActive(true);
        visible = true;
    }

    public void FlashSuccess()
    {
        feedback = FeedbackKind.Success;
        feedbackUntil = Time.unscaledTime + 0.35f;
    }

    public void FlashMiss()
    {
        feedback = FeedbackKind.Miss;
        feedbackUntil = Time.unscaledTime + 0.35f;
    }

    public void Hide()
    {
        TickFeedbackExpiry();
        if (feedback != FeedbackKind.None)
            return;

        if (root != null && root.activeSelf)
            root.SetActive(false);
        visible = false;
    }

    public void Destroy()
    {
        feedback = FeedbackKind.None;
        if (root != null)
        {
            Object.Destroy(root);
            root = null;
        }
        visible = false;
    }

    private void TickFeedbackExpiry()
    {
        if (feedback == FeedbackKind.None)
            return;
        if (Time.unscaledTime < feedbackUntil)
            return;
        feedback = FeedbackKind.None;
        if (root != null && root.activeSelf && !visible)
            root.SetActive(false);
    }

    private void SetPrompt(string text, Color color, bool on)
    {
        if (promptLabel == null)
            return;
        if (!on || string.IsNullOrEmpty(text))
        {
            promptLabel.gameObject.SetActive(false);
            return;
        }
        promptLabel.text = text;
        promptLabel.color = color;
        promptLabel.gameObject.SetActive(true);
    }

    private void Ensure()
    {
        if (root != null)
            return;

        root = new GameObject("Bravura_FlourishReloadBar");
        Object.DontDestroyOnLoad(root);

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9000;

        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        var containerGo = new GameObject("Container");
        containerGo.transform.SetParent(root.transform, false);
        var containerRt = containerGo.AddComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0.5f, AnchorY);
        containerRt.anchorMax = new Vector2(0.5f, AnchorY);
        containerRt.pivot = new Vector2(0.5f, 0.5f);
        containerRt.sizeDelta = new Vector2(BarWidth + 24f, BarHeight + 40f);

        var outlineGo = new GameObject("Outline");
        outlineGo.transform.SetParent(containerGo.transform, false);
        var outlineRt = outlineGo.AddComponent<RectTransform>();
        outlineRt.anchorMin = new Vector2(0.5f, 0.5f);
        outlineRt.anchorMax = new Vector2(0.5f, 0.5f);
        outlineRt.pivot = new Vector2(0.5f, 0.5f);
        outlineRt.sizeDelta = new Vector2(BarWidth + 4f, BarHeight + 4f);
        var outlineImage = outlineGo.AddComponent<Image>();
        outlineImage.color = OutlineColor;
        outlineImage.raycastTarget = false;

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
        promptLabel.text = "FIRE";
        promptLabel.alignment = TextAlignmentOptions.Center;
        promptLabel.fontSize = 22f;
        promptLabel.fontStyle = FontStyles.Bold;
        promptLabel.color = PromptColor;
        promptLabel.raycastTarget = false;
        if (TMP_Settings.defaultFontAsset != null)
            promptLabel.font = TMP_Settings.defaultFontAsset;
        promptLabel.gameObject.SetActive(false);

        root.SetActive(false);
        visible = false;
    }
}
