using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Center-screen Bravura crosshair:
///   ^     south chevron, tip at true center
///   S     Style rank letter
///  VCVFS  last verbs (oldest → newest, left → right)
/// </summary>
internal sealed class BravuraCrosshairHud
{
    private GameObject root;
    private RectTransform chevronLeft;
    private RectTransform chevronRight;
    private Image chevronLeftImg;
    private Image chevronRightImg;
    private TextMeshProUGUI rankLabel;
    private TextMeshProUGUI verbsLabel;
    private bool visible;

    // Layout (reference 1920×1080)
    private const float ChevronArmLength = 18f;
    private const float ChevronThickness = 2.5f;
    private const float ChevronHalfAngleDeg = 32f; // each arm from vertical
    private const float RankOffsetY = -22f;
    private const float VerbsOffsetY = -42f;

    private static readonly Color DefaultWhite = new(0.95f, 0.96f, 0.98f, 0.95f);
    private static readonly Color Shadow = new(0f, 0f, 0f, 0.65f);

    private readonly StringBuilder verbsBuilder = new StringBuilder(16);

    public void Show(BravuraBehaviour.StyleRank rank, char rankLetter, IEnumerable<BravuraBehaviour.VerbId> recent)
    {
        Ensure();

        Color c = RankColor(rank);
        if (chevronLeftImg != null) chevronLeftImg.color = c;
        if (chevronRightImg != null) chevronRightImg.color = c;

        if (rankLabel != null)
        {
            rankLabel.text = rankLetter.ToString();
            rankLabel.color = c;
        }

        if (verbsLabel != null)
        {
            verbsBuilder.Clear();
            bool first = true;
            foreach (var v in recent)
            {
                if (!first) verbsBuilder.Append(' ');
                first = false;
                verbsBuilder.Append(VerbChar(v));
            }
            verbsLabel.text = verbsBuilder.Length > 0 ? verbsBuilder.ToString() : "—";
            verbsLabel.color = new Color(0.88f, 0.88f, 0.9f, 0.9f);
        }

        if (!root.activeSelf)
            root.SetActive(true);
        visible = true;
    }

    public void Hide()
    {
        if (root != null && root.activeSelf)
            root.SetActive(false);
        visible = false;
    }

    public void Destroy()
    {
        if (root != null)
        {
            Object.Destroy(root);
            root = null;
        }
        visible = false;
    }

    private static char VerbChar(BravuraBehaviour.VerbId v)
    {
        return v switch
        {
            BravuraBehaviour.VerbId.Verse => 'V',
            BravuraBehaviour.VerbId.Chorus => 'C',
            BravuraBehaviour.VerbId.Steel => 'S',
            BravuraBehaviour.VerbId.Flourish => 'F',
            BravuraBehaviour.VerbId.Entrance => 'E',
            _ => '?'
        };
    }

    private static Color RankColor(BravuraBehaviour.StyleRank rank)
    {
        return rank switch
        {
            BravuraBehaviour.StyleRank.C => new Color(0.7f, 0.85f, 1f, 0.95f),
            BravuraBehaviour.StyleRank.B => new Color(0.55f, 0.9f, 0.55f, 0.95f),
            BravuraBehaviour.StyleRank.A => new Color(1f, 0.85f, 0.3f, 0.95f),
            BravuraBehaviour.StyleRank.S => new Color(1f, 0.55f, 0.2f, 0.95f),
            _ => DefaultWhite
        };
    }

    private void Ensure()
    {
        if (root != null)
            return;

        root = new GameObject("Bravura_CrosshairHud");
        Object.DontDestroyOnLoad(root);

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 8500; // under Flourish bar (9000)

        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        // Center pivot container
        var centerGo = new GameObject("Center");
        centerGo.transform.SetParent(root.transform, false);
        var centerRt = centerGo.AddComponent<RectTransform>();
        centerRt.anchorMin = centerRt.anchorMax = new Vector2(0.5f, 0.5f);
        centerRt.pivot = new Vector2(0.5f, 0.5f);
        centerRt.anchoredPosition = Vector2.zero;
        centerRt.sizeDelta = new Vector2(200f, 120f);

        // South chevron: two thin bars meeting at center, opening downward.
        // Left arm: rotated +angle from vertical-down; right arm: -angle.
        // Tip sits at (0,0). Arms extend down-left and down-right.
        chevronLeft = CreateArm(centerGo.transform, "ChevronL", +ChevronHalfAngleDeg);
        chevronRight = CreateArm(centerGo.transform, "ChevronR", -ChevronHalfAngleDeg);
        chevronLeftImg = chevronLeft.GetComponent<Image>();
        chevronRightImg = chevronRight.GetComponent<Image>();

        rankLabel = CreateLabel(centerGo.transform, "Rank", RankOffsetY, 22f, FontStyles.Bold);
        verbsLabel = CreateLabel(centerGo.transform, "Verbs", VerbsOffsetY, 16f, FontStyles.Normal);

        root.SetActive(false);
        visible = false;
    }

    /// <summary>
    /// Arm pivots at the tip (top of the rect). Unrotated, the rect hangs downward (south).
    /// Small Z rotation opens the V toward the bottom of the screen.
    /// </summary>
    private static RectTransform CreateArm(Transform parent, string name, float yawFromDownDeg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        // Pivot at tip (top-center) → body extends south when rotation is 0.
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(ChevronThickness, ChevronArmLength);
        rt.anchoredPosition = Vector2.zero;
        // Do NOT add 180° — that flipped the chevron open-north.
        rt.localRotation = Quaternion.Euler(0f, 0f, yawFromDownDeg);

        var img = go.AddComponent<Image>();
        img.color = DefaultWhite;
        img.raycastTarget = false;
        return rt;
    }


    private static TextMeshProUGUI CreateLabel(Transform parent, string name, float y, float fontSize, FontStyles style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(280f, 28f);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = DefaultWhite;
        tmp.raycastTarget = false;
        tmp.text = "";
        // Soft outline for readability on bright scenes
        tmp.outlineWidth = 0.15f;
        tmp.outlineColor = Shadow;
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
        return tmp;
    }
}
