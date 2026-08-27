using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hivemind crosshair: hollow spread box with shot-blocks seated on the outside.
/// Each block = one trigger pull (ammo-per-shot). Blocks hide as mag empties.
/// Box size tracks live <see cref="SpreadData.spreadSize"/>.
/// </summary>
public sealed class HiveLauncherHUD : GunHUD
{
    private const float DefaultBaseSize = 48f;
    private const float PixelsPerSpreadUnit = 10f;
    private const float BoxThickness = 2.5f;
    private const float BlockGapFromBox = 5f;
    private const float DefaultBlockSize = 7f;
    private const float MinBlockSize = 3.5f;
    private const float BlockSpacing = 3f;
    private const float CenterDotSize = 3f;
    private const float LowAmmoFraction = 0.3f;
    private const int SoftMaxBlocksBeforeShrink = 24;

    private static Sprite s_whiteSprite;

    private RectTransform crosshairRt;
    private RectTransform boxRoot;
    private RectTransform blocksRoot;
    private Image[] boxEdges;
    private Image centerDot;
    private readonly List<Image> blocks = new List<Image>(32);

    private Vector2 currentBoxSize = new Vector2(200f, 200f);
    private int configuredBlockCount;
    private int lastFilled = -1;
    private float lastAmmoPerShot = -1f;
    private int lastMagazineSize = -1;
    private Vector2 lastSpread = new Vector2(-1f, -1f);

    private Action<float> onAmmoChanged;
    private Color blockColor = new Color(1f, 1f, 1f, 0.85f);
    private Color blockLowColor;
    private Color boxColor = new Color(1f, 1f, 1f, 0.55f);

    private static Sprite WhiteSprite
    {
        get
        {
            if (s_whiteSprite != null)
                return s_whiteSprite;

            Texture2D tex = Texture2D.whiteTexture;
            s_whiteSprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f);
            return s_whiteSprite;
        }
    }

    // AssemblyPublicizer widens GunHUD.Awake to public — override must match.
    public override void Awake()
    {
        BuildHierarchy();
        base.Awake();
        onAmmoChanged = OnAmmoChanged;
        blockLowColor = Global.Instance != null
            ? Global.Instance.RedUIColor
            : new Color(1f, 0.25f, 0.25f, 0.9f);
        if (blockLowColor.a < 0.5f)
            blockLowColor.a = 0.9f;
    }


    private void BuildHierarchy()
    {
        // Prefab Instantiate copies any children built on the template; wipe so one tree wins.
        if (crosshairRt != null)
            return;

        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        RectTransform root = transform as RectTransform;
        if (root == null)
        {
            // Last-resort: replace plain Transform with RectTransform (should not happen if prefab is correct).
            root = gameObject.AddComponent<RectTransform>();
        }

        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        root.pivot = new Vector2(0.5f, 0.5f);
        gameObject.layer = 5;

        crosshairRt = CreateChild("Crosshair", root);

        crosshairRt.anchorMin = crosshairRt.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRt.pivot = new Vector2(0.5f, 0.5f);
        crosshairRt.sizeDelta = currentBoxSize;
        crosshairRt.anchoredPosition = Vector2.zero;

        // GunHUD protected fields (publicized).
        crosshair = crosshairRt;
        crosshairLines = Array.Empty<Graphic>();
        baseSpreadCrosshairSize = new Vector2(DefaultBaseSize, DefaultBaseSize);
        addedSpreadCrosshairSizeMultiplier = new Vector2(PixelsPerSpreadUnit, PixelsPerSpreadUnit);
        maxCrosshairSize = Vector2.zero;

        boxRoot = CreateChild("Box", crosshairRt);
        StretchFull(boxRoot);
        boxEdges = new Image[4];
        boxEdges[0] = CreateEdge("EdgeTop", boxRoot, Edge.Top);
        boxEdges[1] = CreateEdge("EdgeBottom", boxRoot, Edge.Bottom);
        boxEdges[2] = CreateEdge("EdgeLeft", boxRoot, Edge.Left);
        boxEdges[3] = CreateEdge("EdgeRight", boxRoot, Edge.Right);

        centerDot = CreateImage("Center", crosshairRt);
        RectTransform dotRt = centerDot.rectTransform;
        dotRt.anchorMin = dotRt.anchorMax = new Vector2(0.5f, 0.5f);
        dotRt.pivot = new Vector2(0.5f, 0.5f);
        dotRt.sizeDelta = new Vector2(CenterDotSize, CenterDotSize);
        dotRt.anchoredPosition = Vector2.zero;
        centerDot.color = new Color(1f, 1f, 1f, 0.75f);

        blocksRoot = CreateChild("Blocks", crosshairRt);
        StretchFull(blocksRoot);
    }

    private enum Edge { Top, Bottom, Left, Right }

    private Image CreateEdge(string name, RectTransform parent, Edge edge)
    {
        Image img = CreateImage(name, parent);
        RectTransform rt = img.rectTransform;
        switch (edge)
        {
            case Edge.Top:
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.sizeDelta = new Vector2(0f, BoxThickness);
                rt.anchoredPosition = Vector2.zero;
                break;
            case Edge.Bottom:
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.sizeDelta = new Vector2(0f, BoxThickness);
                rt.anchoredPosition = Vector2.zero;
                break;
            case Edge.Left:
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.sizeDelta = new Vector2(BoxThickness, 0f);
                rt.anchoredPosition = Vector2.zero;
                break;
            case Edge.Right:
                rt.anchorMin = new Vector2(1f, 0f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 0.5f);
                rt.sizeDelta = new Vector2(BoxThickness, 0f);
                rt.anchoredPosition = Vector2.zero;
                break;
        }

        img.color = boxColor;
        return img;
    }

    private static RectTransform CreateChild(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        return rt;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    private static Image CreateImage(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = 5;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.sprite = WhiteSprite;
        img.type = Image.Type.Simple;
        img.raycastTarget = false;
        return img;
    }

    public override void Enable(IGear gear)
    {
        // Ensure hierarchy exists before GunHUD.Enable touches crosshair.
        if (crosshairRt == null || crosshair == null)
            BuildHierarchy();

        // base.Enable activates the GO (Awake builds hierarchy) then wires GunHUD defaults.
        base.Enable(gear);
        if (base.Gun != null && onAmmoChanged != null)
        {
            base.Gun.OnRemainingAmmoChanged -= onAmmoChanged;
            base.Gun.OnRemainingAmmoChanged += onAmmoChanged;
        }

        lastFilled = -1;
        RefreshSpreadFromGun();
        RebuildBlocksIfNeeded(force: true);
        RefreshBlockFill(base.Gun != null ? base.Gun.RemainingAmmo : 0f);
    }



    public override void Disable()
    {
        if (base.Gun != null)
            base.Gun.OnRemainingAmmoChanged -= onAmmoChanged;
        base.Disable();
    }

    public override void OnUpgradesEnabled(IGear gear)
    {
        base.OnUpgradesEnabled(gear);
        RefreshSpreadFromGun();
        RebuildBlocksIfNeeded(force: true);
        if (base.Gun != null)
            RefreshBlockFill(base.Gun.RemainingAmmo);
    }

    public override void UpdateSpreadVisuals(Vector2 spreadIncrease)
    {
        // Hivemind uses absolute live spread so the hollow box always matches the plant volume,
        // even when Prefab and live share the same baseline (delta would be zero).
        Vector2 spread = spreadIncrease;
        if (base.Gun != null)
            spread = base.Gun.GunData.spreadData.spreadSize;

        ApplyBoxSizeFromSpread(spread);
    }

    // Do not override Update — publicized GunHUD/HUD Update access can disagree with analyzers.
    // LateUpdate is enough for spread/mag layout; ammo fill is event-driven.
    private void LateUpdate()
    {
        if (!active || base.Gun == null)
            return;

        Vector2 spread = base.Gun.GunData.spreadData.spreadSize;
        if (spread != lastSpread)
            ApplyBoxSizeFromSpread(spread);

        if (base.Gun.GunData.magazineSize != lastMagazineSize ||
            !Mathf.Approximately(GetAmmoPerShot(base.Gun), lastAmmoPerShot))
        {
            RebuildBlocksIfNeeded(force: true);
            RefreshBlockFill(base.Gun.RemainingAmmo);
        }
    }



    private void RefreshSpreadFromGun()
    {
        if (base.Gun == null)
            return;
        ApplyBoxSizeFromSpread(base.Gun.GunData.spreadData.spreadSize);
    }

    private void ApplyBoxSizeFromSpread(Vector2 spread)
    {
        lastSpread = spread;
        Vector2 size = baseSpreadCrosshairSize + spread * addedSpreadCrosshairSizeMultiplier;
        if (maxCrosshairSize.x > 0f)
            size.x = Mathf.Min(size.x, maxCrosshairSize.x);
        if (maxCrosshairSize.y > 0f)
            size.y = Mathf.Min(size.y, maxCrosshairSize.y);

        size.x = Mathf.Max(size.x, 32f);
        size.y = Mathf.Max(size.y, 32f);
        currentBoxSize = size;
        if (crosshairRt != null)
            crosshairRt.sizeDelta = size;

        LayoutBlocks(configuredBlockCount > 0 ? configuredBlockCount : blocks.Count);
    }

    private void OnAmmoChanged(float remaining)
    {
        RebuildBlocksIfNeeded(force: false);
        RefreshBlockFill(remaining);
    }

    internal static float GetAmmoPerShot(Gun gun)
    {
        if (gun == null)
            return 1f;

        ref GunData g = ref gun.GunData;
        int bps = Mathf.Max(1, g.bulletsPerShot);
        int use = Mathf.Max(0, g.useAmmoOnFire);
        if (use <= 0)
            return 1f;

        // Mirrors Gun fire ammo debit:
        // doesEach > 1 → flat that amount; == 1 → bullets fired; else → bullets/bps (=1 per shot).
        float perShot;
        if (g.doesEachBulletInShotRemoveAmmo > 1)
            perShot = g.doesEachBulletInShotRemoveAmmo;
        else if (g.doesEachBulletInShotRemoveAmmo == 1)
            perShot = bps;
        else
            perShot = 1f;

        return Mathf.Max(1f, perShot * use);
    }

    private void RebuildBlocksIfNeeded(bool force)
    {
        if (base.Gun == null)
            return;

        float ammoPerShot = GetAmmoPerShot(base.Gun);
        int mag = Mathf.Max(1, base.Gun.GunData.magazineSize);
        int count = Mathf.Max(1, Mathf.FloorToInt(mag / ammoPerShot));

        if (!force && count == configuredBlockCount &&
            Mathf.Approximately(ammoPerShot, lastAmmoPerShot) &&
            mag == lastMagazineSize)
            return;

        lastAmmoPerShot = ammoPerShot;
        lastMagazineSize = mag;
        configuredBlockCount = count;
        lastFilled = -1;
        EnsureBlockInstances(count);
        LayoutBlocks(count);
    }


    private void EnsureBlockInstances(int count)
    {
        while (blocks.Count < count)
        {
            Image img = CreateImage($"Block_{blocks.Count}", blocksRoot);
            img.color = blockColor;
            blocks.Add(img);
        }

        for (int i = 0; i < blocks.Count; i++)
        {
            bool on = i < count;
            if (blocks[i].gameObject.activeSelf != on)
                blocks[i].gameObject.SetActive(on);
        }
    }

    private void LayoutBlocks(int count)
    {
        if (count <= 0 || blocksRoot == null)
            return;

        float blockSize = DefaultBlockSize;
        if (count > SoftMaxBlocksBeforeShrink)
        {
            float t = Mathf.InverseLerp(SoftMaxBlocksBeforeShrink, SoftMaxBlocksBeforeShrink * 3f, count);
            blockSize = Mathf.Lerp(DefaultBlockSize, MinBlockSize, Mathf.Clamp01(t));
        }

        // Outer path sits just outside the hollow box.
        float halfW = currentBoxSize.x * 0.5f + BlockGapFromBox + blockSize * 0.5f;
        float halfH = currentBoxSize.y * 0.5f + BlockGapFromBox + blockSize * 0.5f;

        // Optional: if perimeter is too short for spacing, shrink further.
        float peri = 2f * (2f * halfW + 2f * halfH);
        float minPeri = count * (blockSize + BlockSpacing);
        if (peri < minPeri && peri > 0.01f)
        {
            float scale = minPeri / peri;
            // Prefer shrinking blocks over exploding the ring into the whole screen.
            blockSize = Mathf.Max(MinBlockSize, blockSize / Mathf.Sqrt(scale));
            halfW = currentBoxSize.x * 0.5f + BlockGapFromBox + blockSize * 0.5f;
            halfH = currentBoxSize.y * 0.5f + BlockGapFromBox + blockSize * 0.5f;
            peri = 2f * (2f * halfW + 2f * halfH);
        }

        for (int i = 0; i < count && i < blocks.Count; i++)
        {
            Vector2 pos = PointOnRectPerimeter(i, count, halfW, halfH);
            RectTransform rt = blocks[i].rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(blockSize, blockSize);
            rt.anchoredPosition = pos;
        }
    }

    /// <summary>
    /// Evenly distributes indices along the rectangle perimeter (top → right → bottom → left).
    /// </summary>
    private static Vector2 PointOnRectPerimeter(int index, int count, float halfW, float halfH)
    {
        float width = halfW * 2f;
        float height = halfH * 2f;
        float peri = 2f * (width + height);
        float d = ((index + 0.5f) / count) * peri;

        // Top edge: left → right at +halfH
        if (d <= width)
            return new Vector2(-halfW + d, halfH);
        d -= width;

        // Right edge: top → bottom at +halfW
        if (d <= height)
            return new Vector2(halfW, halfH - d);
        d -= height;

        // Bottom edge: right → left at -halfH
        if (d <= width)
            return new Vector2(halfW - d, -halfH);
        d -= width;

        // Left edge: bottom → top at -halfW
        return new Vector2(-halfW, -halfH + d);
    }

    private void RefreshBlockFill(float remainingAmmo)
    {
        if (configuredBlockCount <= 0)
            return;

        float ammoPerShot = lastAmmoPerShot > 0f ? lastAmmoPerShot : 1f;
        int filled = Mathf.Clamp(Mathf.FloorToInt(remainingAmmo / ammoPerShot + 1e-4f), 0, configuredBlockCount);
        bool low = configuredBlockCount > 0 &&
                   (filled / (float)configuredBlockCount) <= LowAmmoFraction &&
                   filled > 0;
        Color color = low ? blockLowColor : blockColor;

        if (filled == lastFilled)
        {
            // Still refresh colors when crossing low-ammo threshold.
            for (int i = 0; i < filled && i < blocks.Count; i++)
            {
                if (blocks[i].color != color)
                    blocks[i].color = color;
            }
            return;
        }

        lastFilled = filled;
        for (int i = 0; i < configuredBlockCount && i < blocks.Count; i++)
        {
            bool on = i < filled;
            if (blocks[i].gameObject.activeSelf != on)
                blocks[i].gameObject.SetActive(on);
            if (on)
                blocks[i].color = color;
        }
    }
}
