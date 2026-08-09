using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using Object = UnityEngine.Object;

namespace JipperResourcePack;

public static class BundleLoader {
    private static AssetBundle _bundle;
    public static TMP_FontAsset FontAsset;
    public static TMP_FontAsset DefaultFontAsset;
    public static GameObject ProgressObject;
    public static Sprite Auto;
    public static Sprite KeyBackground;
    public static Sprite KeyOutline;
    public static Sprite GhostRain;
    public static Texture2D SideImage;

    public static void LoadBundle() {
        string path;
        switch(ADOBase.platform) {
            case Platform.Windows:
                path = Path.Combine(Main.Instance.Path, "jipperresourcepackbundle");
                break;
            case Platform.Linux:
                path = Path.Combine(Main.Instance.Path, "Linux/jipperresourcepackbundle");
                break;
            case Platform.Mac:
                path = Path.Combine(Main.Instance.Path, "Mac/jipperresourcepackbundle");
                break;
            default:
                Main.Instance.Warning("Unsupported platform, defaulting to Windows path");
                goto case Platform.Windows;
        }

        Main.Instance.Log("Unity Version: " + Application.unityVersion);
        if(Application.unityVersion.StartsWith("2022")) path += "2022";

        _bundle = AssetBundle.LoadFromFile(path);
        foreach(Object asset in _bundle.LoadAllAssets()) {
            switch(asset.name) {
                case "MAPLESTORY_OTF_BOLD SDF":
                    FontAsset = DefaultFontAsset = (TMP_FontAsset) asset;
                    FontAsset.fallbackFontAssetTable.Add(RDConstants.data.chineseFontTMPro);
                    break;
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
        if(!string.IsNullOrEmpty(Main.Settings.FontName)) LoadCustomFont(Main.Settings.FontName);
    }

    public static void UnloadBundle() {
        UnloadCustomFont();
        _bundle.Unload(true);
    }

    public static void LoadCustomFont(string fontName, string fontPath = null) {
        fontPath ??= GetPathForFontName(fontName);
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(fontPath ?? fontName, 0, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
        if(!fontAsset) {
            Main.Instance.Warning("Failed to create font asset for system font: " + fontName);
            return;
        }
        try {
            fontAsset.fallbackFontAssetTable = [RDConstants.data.latinFontTMPro, RDConstants.data.koreanFontTMPro, RDConstants.data.japaneseFontTMPro, RDConstants.data.chineseFontTMPro];
        } catch (Exception e) {
            Main.Instance.Warning("Failed to add fallback font asset for system font: " + fontName + "\n" + e);
        }

        UnloadCustomFont();
        FontAsset = fontAsset;
    }

    private static string GetPathForFontName(string fontName) {
        string[] names = Font.GetOSInstalledFontNames();
        string[] paths = Font.GetPathsToOSFonts();
        int index = Array.IndexOf(names, fontName);
        return index >= 0 && index < paths.Length ? paths[index] : null;
    }

    public static void UnloadCustomFont() {
        if(FontAsset == DefaultFontAsset) return;
        Object.Destroy(FontAsset);
        FontAsset = DefaultFontAsset;
    }
}
