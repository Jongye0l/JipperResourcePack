using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using Object = UnityEngine.Object;

namespace JipperResourcePack;

public static class BundleLoader {
    public const string AdofaiFontName = "$<ADOFAI>";
    private static AssetBundle _bundle;
    public static TMP_FontAsset FontAsset;
    public static TMP_FontAsset DefaultFontAsset;
    public static GameObject ProgressObject;
    public static Sprite Auto;
    public static Sprite KeyBackground;
    public static Sprite KeyOutline;
    public static Sprite GhostRain;
    public static Texture2D SideImage;
    private static bool _requiredFontUnload;

    public static void LoadBundle() {
        RemoveLegacyBundle();
        LoadDefaultFont();
        _bundle = AssetBundle.LoadFromFile(Path.Combine(Main.Instance.Path, "jipperresourcepackbundle"));
        foreach(Object asset in _bundle.LoadAllAssets()) {
            switch(asset.name) {
                case "ProgressBar":
                    ProgressObject = (GameObject) asset;
                    break;
                case "Auto":
                    if(asset is Sprite s) Auto = s;
                    break;
                case "KeyBackground":
                    if(asset is Sprite s1) KeyBackground = s1;
                    break;
                case "KeyOutline":
                    if(asset is Sprite s2) KeyOutline = s2;
                    break;
                case "SideImage":
                    if(asset is Texture2D t) SideImage = t;
                    break;
                case "GhostRain":
                    if(asset is Sprite s3) GhostRain = s3;
                    break;
            }
        }
        if(!string.IsNullOrEmpty(Main.Settings.FontName)) {
            if(Main.Settings.FontName == AdofaiFontName) LoadAdofaiFont();
            else LoadCustomFont(Main.Settings.FontName);
        }
    }

    private static void RemoveLegacyBundle() {
        try {
            foreach(string name in new[] { "Linux", "Mac" }) {
                string directory = Path.Combine(Main.Instance.Path, name);
                if(!Directory.Exists(directory)) continue;
                Directory.Delete(directory, true);
                Main.Instance.Log("Removed legacy bundle directory: " + name);
            }
            string bundle = Path.Combine(Main.Instance.Path, "jipperresourcepackbundle2022");
            if(!File.Exists(bundle)) return;
            File.Delete(bundle);
            Main.Instance.Log("Removed legacy bundle file: jipperresourcepackbundle2022");
        } catch (Exception e) {
            Main.Instance.Warning("Failed to remove legacy bundle\n" + e);
        }
    }

    private static void LoadDefaultFont() {
        string fontPath = Path.Combine(Main.Instance.Path, "Font/MAPLESTORY_OTF_BOLD.OTF");
        TMP_FontAsset fontAsset = VersionSafe.CreateFontAssetFromFile(fontPath);
        if(!fontAsset) {
            Main.Instance.Error("Failed to create font asset for default font: " + fontPath);
            return;
        }
        try {
            fontAsset.fallbackFontAssetTable = [RDConstants.data.latinFontTMPro, RDConstants.data.koreanFontTMPro, RDConstants.data.japaneseFontTMPro, RDConstants.data.chineseFontTMPro];
        } catch (Exception e) {
            Main.Instance.Warning("Failed to add fallback font asset for default font\n" + e);
        }
        FontAsset = DefaultFontAsset = fontAsset;
    }

    public static void UnloadBundle() {
        UnloadCustomFont(null, false);
        Object.Destroy(DefaultFontAsset);
        DefaultFontAsset = null;
        _bundle.Unload(true);
    }

    public static void LoadCustomFont(string fontName, string fontPath = null) {
        fontPath ??= GetPathForFontName(fontName);
        TMP_FontAsset fontAsset = VersionSafe.CreateFontAssetFromFile(fontPath ?? fontName);
        if(!fontAsset) {
            Main.Instance.Warning("Failed to create font asset for system font: " + fontName);
            return;
        }
        try {
            fontAsset.fallbackFontAssetTable = [RDConstants.data.latinFontTMPro, RDConstants.data.koreanFontTMPro, RDConstants.data.japaneseFontTMPro, RDConstants.data.chineseFontTMPro];
        } catch (Exception e) {
            Main.Instance.Warning("Failed to add fallback font asset for system font: " + fontName + "\n" + e);
        }
        if(VersionSafe.FontScaleSupported) {
            try {
                MatchDefaultFontScale(fontAsset);
            } catch (Exception e) {
                Main.Instance.Warning("Failed to adjust font scale for system font: " + fontName + "\n" + e);
            }
        }

        UnloadCustomFont(fontAsset, true);
    }

    public static void LoadAdofaiFont() {
        TMP_FontAsset source = RDString.fontData.fontTMP;
        if(!VersionSafe.FontScaleSupported) {
            UnloadCustomFont(source, false);
            return;
        }
        TMP_FontAsset fontAsset = null;
        try {
            fontAsset = CopyFontAsset(source);
            if(fontAsset) MatchDefaultFontScale(fontAsset);
        } catch (Exception e) {
            Main.Instance.Warning("Failed to adjust font scale for ADOFAI font\n" + e);
            if(fontAsset) Object.Destroy(fontAsset);
            fontAsset = null;
        }
        if(fontAsset) UnloadCustomFont(fontAsset, true);
        else UnloadCustomFont(source, false);
    }

    private static TMP_FontAsset CopyFontAsset(TMP_FontAsset source) {
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(source.sourceFontFile, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
        if(fontAsset) fontAsset.fallbackFontAssetTable = source.fallbackFontAssetTable;
        return fontAsset;
    }

    private static void MatchDefaultFontScale(TMP_FontAsset fontAsset) {
        if(!DefaultFontAsset || DefaultFontAsset == fontAsset) return;
        float defaultCapHeight = GetCapHeight(DefaultFontAsset);
        float capHeight = GetCapHeight(fontAsset);
        Main.Instance.Log("Matching font scale for " + fontAsset.name + ": default cap height = " + defaultCapHeight + ", new cap height = " + capHeight);
        if(defaultCapHeight <= 0 || capHeight <= 0) return;
        FaceInfo faceInfo = fontAsset.faceInfo;
        faceInfo.scale = Mathf.Clamp(defaultCapHeight / capHeight, 0.5f, 2f);
        fontAsset.faceInfo = faceInfo;
    }

    private static float GetCapHeight(TMP_FontAsset fontAsset) {
        FaceInfo faceInfo = fontAsset.faceInfo;
        if(faceInfo.pointSize <= 0) return 0;
        float capHeight = faceInfo.capLine;
        if(capHeight <= 0 && fontAsset.HasCharacter('H', false, true) && fontAsset.characterLookupTable.TryGetValue('H', out TMP_Character character))
            capHeight = character.glyph.metrics.horizontalBearingY;
        return capHeight / faceInfo.pointSize;
    }

    private static string GetPathForFontName(string fontName) {
        string[] names = Font.GetOSInstalledFontNames();
        string[] paths = Font.GetPathsToOSFonts();
        int index = Array.IndexOf(names, fontName);
        return index >= 0 && index < paths.Length ? paths[index] : null;
    }

    public static void UnloadCustomFont(TMP_FontAsset fontAsset, bool requireUnload) {
        if(_requiredFontUnload) Object.Destroy(FontAsset);
        FontAsset = fontAsset;
        _requiredFontUnload = requireUnload;
    }
}
