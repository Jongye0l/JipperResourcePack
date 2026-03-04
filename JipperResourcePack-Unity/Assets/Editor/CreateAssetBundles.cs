using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateAssetBundle {
    [MenuItem("Assets/Build Bundle")]
    static void BuildAllAssetBundles() {
        string assetBundleDirectory = "Assets/AssetBundles";
        if(!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, 
            BuildAssetBundleOptions.None, 
            BuildTarget.StandaloneWindows);
        string linuxBundleDirectory = Path.Combine(assetBundleDirectory, "Linux");
        if (!Directory.Exists(linuxBundleDirectory))
        {
            Directory.CreateDirectory(linuxBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(linuxBundleDirectory,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneLinux64);
        string macBundleDirectory = Path.Combine(assetBundleDirectory, "Mac");
        if (!Directory.Exists(macBundleDirectory))
        {
            Directory.CreateDirectory(macBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(macBundleDirectory,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneOSX);
    }
}