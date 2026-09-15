using System;
using System.IO;
using TMPro;
using UnityEngine;
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
            if(Main.Settings.FontName == AdofaiFontName) UnloadCustomFont(RDString.fontData.fontTMP);
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
        UnloadCustomFont(null);
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

        UnloadCustomFont(fontAsset);
        _requiredFontUnload = true;
    }

    private static string GetPathForFontName(string fontName) {
        string[] names = Font.GetOSInstalledFontNames();
        string[] paths = Font.GetPathsToOSFonts();
        int index = Array.IndexOf(names, fontName);
        return index >= 0 && index < paths.Length ? paths[index] : null;
    }

    public static void UnloadCustomFont(TMP_FontAsset fontAsset) {
        if(_requiredFontUnload) Object.Destroy(FontAsset);
        FontAsset = fontAsset;
    }
}
