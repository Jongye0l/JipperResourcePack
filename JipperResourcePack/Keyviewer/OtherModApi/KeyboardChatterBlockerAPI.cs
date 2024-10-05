using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using KeyboardChatterBlocker;
using UnityEngine;

namespace JipperResourcePack.Keyviewer.OtherModApi;

public class KeyboardChatterBlockerAPI {
    public static bool IsExist;
    public static object Setting;

    public static void Setup() {
        if(IsExist) return;
        try {
            SetSetting();
            IsExist = true;
            Main.Instance.Log("KeyboardChatterBlockerAPI is loaded.");
        } catch (FileNotFoundException) {
            Main.Instance.Log("KeyboardChatterBlockerAPI is not loaded.");
        } catch (TypeLoadException) {
            Main.Instance.Log("KeyboardChatterBlockerAPI is not loaded.");
        } catch (Exception e) {
            Main.Instance.Log("KeyboardChatterBlockerAPI is not loaded.");
            Main.Instance.LogException(e);
        }
    }

    public static void SetSetting() {
        Setting = KeyboardChatterBlocker.Main.setting;
        if(Setting == null) throw new InstanceNotFoundException("KeyboardChatterBlocker setting not found.");
    }

    public static void UpdateKeyLimit(List<KeyCode> keys, List<ushort> asyncKeys) {
        try {
            KeyLimiterProfile profile = KeyboardChatterBlocker.Main.selectedKeyLimiterProfile;
            if(profile.name != "JipperResourcePack") {
                Setting setting = (Setting) Setting;
                profile = setting.keyLimiterProfiles.FirstOrDefault(t => t.name == "JipperResourcePack");
                if(profile == null) {
                    profile = new KeyLimiterProfile("JipperResourcePack");
                    setting.keyLimiterProfiles.Add(profile);
                    KeyboardChatterBlocker.Main.selectedKeyLimiterProfile = profile;
                }
            }
            profile.allowedKeys = keys;
            profile.allowedAsyncKeys = asyncKeys;
        } catch (Exception e) {
            Main.Instance.Error("Failed to update KeyboardChatterBlocker key limit.");
            Main.Instance.LogException(e);
        }
    }
}