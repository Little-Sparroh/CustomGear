using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Loads upgrade icons from ICONS/ next to the plugin DLL (or project ICONS/ in dev).
/// </summary>
internal static class IconLoader
{
    private static readonly Dictionary<string, Sprite> Cache =
        new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

    private static string _iconsDir;
    private static bool _dirResolved;

    public static Sprite Get(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return null;

        if (Cache.TryGetValue(fileName, out Sprite cached))
            return cached;

        string path = ResolvePath(fileName);
        if (path == null || !File.Exists(path))
        {
            SparrohPlugin.Logger?.LogDebug($"[IconLoader] Missing icon: {fileName}");
            Cache[fileName] = null;
            return null;
        }

        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
            if (!ImageConversion.LoadImage(tex, bytes, markNonReadable: true))
            {
                UnityEngine.Object.Destroy(tex);
                SparrohPlugin.Logger?.LogWarning($"[IconLoader] LoadImage failed: {fileName}");
                Cache[fileName] = null;
                return null;
            }

            tex.name = Path.GetFileNameWithoutExtension(fileName);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var sprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.name = tex.name;
            Cache[fileName] = sprite;
            return sprite;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogWarning($"[IconLoader] Failed '{fileName}': {ex.Message}");
            Cache[fileName] = null;
            return null;
        }
    }

    private static string ResolvePath(string fileName)
    {
        string dir = GetIconsDirectory();
        if (string.IsNullOrEmpty(dir))
            return null;

        // Exact path first
        string exact = Path.Combine(dir, fileName);
        if (File.Exists(exact))
            return exact;

        // Case-insensitive match (Windows usually fine; helps mixed _Icon/_icon)
        try
        {
            foreach (string f in Directory.GetFiles(dir, "*.png"))
            {
                if (string.Equals(Path.GetFileName(f), fileName, StringComparison.OrdinalIgnoreCase))
                    return f;
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    private static string GetIconsDirectory()
    {
        if (_dirResolved)
            return _iconsDir;

        _dirResolved = true;
        try
        {
            string asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(asmDir))
            {
                string besideDll = Path.Combine(asmDir, "ICONS");
                if (Directory.Exists(besideDll))
                {
                    _iconsDir = besideDll;
                    return _iconsDir;
                }

                // Dev: project root next to bin/Debug
                string projectIcons = Path.GetFullPath(Path.Combine(asmDir, "..", "..", "ICONS"));
                if (Directory.Exists(projectIcons))
                {
                    _iconsDir = projectIcons;
                    return _iconsDir;
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger?.LogDebug($"[IconLoader] Dir resolve failed: {ex.Message}");
        }

        _iconsDir = null;
        return null;
    }
}
