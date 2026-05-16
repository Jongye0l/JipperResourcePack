using System.IO;
using JALib.Tools;
using TMPro;
using UnityEngine;

namespace JipperResourcePack;

public class BundleLoader {
    public static AssetBundle Bundle;
    public static TMP_FontAsset FontAsset;
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
        
        Bundle = AssetBundle.LoadFromFile(path);
        foreach(Object asset in Bundle.LoadAllAssets()) {
            switch(asset.name) {
                case "MAPLESTORY_OTF_BOLD SDF":
                    FontAsset = (TMP_FontAsset) asset;
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
    }
    
    public static void UnloadBundle() {
        Bundle.Unload(true);
    }
}
