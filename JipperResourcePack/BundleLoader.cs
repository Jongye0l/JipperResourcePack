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
                goto case Platform.Windows;
        }
        
        Bundle = AssetBundle.LoadFromFile(path);
        foreach(Object asset in Bundle.LoadAllAssets()) {
            switch(asset.name) {
                case "MAPLESTORY_OTF_BOLD SDF":
                    FontAsset = (TMP_FontAsset) asset;
                    FontAsset.fallbackFontAssetTable.Add(RDConstants.data.chineseFontTMPro);
                    
                    if(FontAsset.material) {
                        Material fontMaterial = FontAsset.material;
                        Shader fallbackShader = Shader.Find("TextMeshPro/Mobile/Distance Field");
                        if(!fallbackShader) {
                            fallbackShader = Shader.Find("TextMeshPro/Distance Field");
                            if(!fallbackShader) {
                                fallbackShader = Shader.Find("UI/Default");
                            }
                        }
                        if(fallbackShader) {
                            fontMaterial.shader = fallbackShader;
                            Main.Instance.Log($"Shader changed to: {fallbackShader.name}");
                        }
                    }
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
