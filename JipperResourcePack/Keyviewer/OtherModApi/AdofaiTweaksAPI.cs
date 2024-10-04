using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Instrumentation;
using AdofaiTweaks.Core;
using AdofaiTweaks.Tweaks.KeyLimiter;
using JALib.Tools;
using UnityEngine;

namespace JipperResourcePack.Keyviewer.OtherModApi;

public class AdofaiTweaksAPI {
    public static bool IsExist;
    public static object keySetting;

    public static void Setup() {
        if(IsExist) return;
        try {
            SetTweakRunner();
            IsExist = true;
            Main.Instance.Log("AdofaiTweaksAPI is loaded.");
        } catch (FileNotFoundException) {
            Main.Instance.Log("AdofaiTweaksAPI is not loaded.");
        } catch (Exception e) {
            Main.Instance.Log("AdofaiTweaksAPI is not loaded.");
            Main.Instance.LogException(e);
        }
    }

    public static void SetTweakRunner() {
        IList tweakRunners = typeof(AdofaiTweaks.AdofaiTweaks).GetValue<IList>("tweakRunners");
        foreach(object tweakRunner in tweakRunners) {
            TweakSettings settings = tweakRunner.Invoke<TweakSettings>("get_Settings");
            if(settings is not KeyLimiterSettings keySettings) continue;
            keySetting = keySettings;
            return;
        }
        if(keySetting == null) throw new InstanceNotFoundException("KeyLimiterSettings not found.");
    }

    public static void UpdateKeyLimit(List<KeyCode> keys, List<ushort> asyncKeys) {
        try {
            if(keySetting == null) SetTweakRunner();
            KeyLimiterSettings keySettings = (KeyLimiterSettings) keySetting;
            keySettings.ActiveKeys = keys;
            keySettings.ActiveAsyncKeys = asyncKeys;
        } catch (Exception e) {
            Main.Instance.Error("Failed to update AdofaiTweaks key limit.");
            Main.Instance.LogException(e);
        }
    }
}