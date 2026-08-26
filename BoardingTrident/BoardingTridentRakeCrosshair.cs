using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Custom Boarding Trident reticle: 5 equally spaced dots along the combat axis.
///
/// Dots are laid out on local Y. Root Z matches barrel:
///   hip Z=90° → local Y maps to screen horizontal (deck rake)
///   RMB Z=0°  → local Y stays screen vertical (mast stake)
///
/// (Dots on local X would invert that mapping — do not switch without flipping Z.)
/// </summary>
public sealed class BoardingTridentRakeCrosshair : MonoBehaviour
{
    public const int DotCount = 5;

    /// <summary>Half-span of the outermost dots from center (UI units).</summary>
    public float HalfSpan = 48f;

    /// <summary>Dot size in UI units.</summary>
    public float DotSize = 6f;

    /// <summary>Dot color (readable on most biomes).</summary>
    public Color DotColor = new Color(1f, 1f, 1f, 0.92f);

    private RectTransform _root;
    private RectTransform[] _dots;
    private Image[] _dotImages;
    private static Sprite _whiteSprite;

    public static BoardingTridentRakeCrosshair Create(Transform parent)
    {
        if (parent == null)
            return null;

        var go = new GameObject("BoardingTridentRakeCrosshair", typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        var rake = go.AddComponent<BoardingTridentRakeCrosshair>();
        rake.Build();
        return rake;
    }

    private void Build()
    {
        _root = (RectTransform)transform;
        _dots = new RectTransform[DotCount];
        _dotImages = new Image[DotCount];

        Sprite sprite = GetWhiteSprite();

        for (int i = 0; i < DotCount; i++)
        {
            var dotGo = new GameObject($"Dot{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            dotGo.layer = gameObject.layer;
            var drt = dotGo.GetComponent<RectTransform>();
            drt.SetParent(_root, false);
            drt.anchorMin = drt.anchorMax = new Vector2(0.5f, 0.5f);
            drt.pivot = new Vector2(0.5f, 0.5f);
            drt.sizeDelta = new Vector2(DotSize, DotSize);
            drt.localScale = Vector3.one;
            drt.localRotation = Quaternion.identity;

            var img = dotGo.GetComponent<Image>();
            img.sprite = sprite;
            img.color = DotColor;
            img.raycastTarget = false;

            // Even spacing along local Y so barrel Z maps 1:1 to on-screen axis.
            // t in [-1..+1]
            float t = DotCount <= 1 ? 0f : i / (float)(DotCount - 1) * 2f - 1f;
            drt.anchoredPosition = new Vector2(0f, t * HalfSpan);

            _dots[i] = drt;
            _dotImages[i] = img;
        }
    }

    /// <summary>
    /// Same Z as barrel: hip 90° → horizontal dots, RMB 0° → vertical dots.
    /// </summary>
    public void SetBarrelZ(float degrees)
    {
        if (_root != null)
            _root.localEulerAngles = new Vector3(0f, 0f, degrees);
    }

    public void SetVisible(bool visible)
    {
        if (gameObject != null && gameObject.activeSelf != visible)
            gameObject.SetActive(visible);
    }

    public void ApplyLayout(float halfSpan, float dotSize, Color color)
    {
        HalfSpan = halfSpan;
        DotSize = dotSize;
        DotColor = color;

        if (_dots == null)
            return;

        for (int i = 0; i < _dots.Length; i++)
        {
            if (_dots[i] == null)
                continue;
            float t = DotCount <= 1 ? 0f : i / (float)(DotCount - 1) * 2f - 1f;
            _dots[i].sizeDelta = new Vector2(DotSize, DotSize);
            _dots[i].anchoredPosition = new Vector2(0f, t * HalfSpan);
            if (_dotImages[i] != null)
                _dotImages[i].color = DotColor;
        }
    }


    private static Sprite GetWhiteSprite()
    {
        if (_whiteSprite != null)
            return _whiteSprite;

        var tex = Texture2D.whiteTexture;
        _whiteSprite = Sprite.Create(
            tex,
            new Rect(0f, 0f, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f);
        _whiteSprite.name = "BT_WhiteDot";
        return _whiteSprite;
    }

    private void OnDestroy()
    {
        _dots = null;
        _dotImages = null;
        _root = null;
    }
}
