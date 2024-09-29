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
    public static Texture2D SideImage;
    
    public static void LoadBundle() {
        Bundle = AssetBundle.LoadFromFile(Path.Combine(Main.Instance.Path, "jipperresourcepackbundle"));
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
            }
        }
    }
    
    public static void UnloadBundle() {
        Bundle.Unload(true);
    }
}