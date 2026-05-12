using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using JALib.Tools;
using KeyboardChatterBlocker;
using UnityEngine;
using UnityModManagerNet;

namespace JipperResourcePack.KeyViewerContents.OtherModApi;

public static class KeyboardChatterBlockerAPI {
    public static bool IsExist;
    private static object _setting;

    public static void Setup() {
        if(IsExist) return;
        if(UnityModManager.modEntries.FirstOrDefault(mod => mod.Info.Id == "KeyboardChatterBlocker") is not { Enabled: true }) {
            Main.Instance.Log("KeyboardChatterBlockerAPI is not loaded.");
            return;
        }
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
        _setting = KeyboardChatterBlocker.Main.setting;
        if(_setting is not Setting) throw new InstanceNotFoundException("KeyboardChatterBlocker setting not found.");
    }

    public static void UpdateKeyLimit(List<KeyCode> keys, List<ushort> asyncKeys) {
        try {
            KeyLimiterProfile profile = KeyboardChatterBlocker.Main.selectedKeyLimiterProfile;
            if(profile.name != "JipperResourcePack") {
                Setting setting = _setting.AsUnsafe<Setting>();
                foreach(KeyLimiterProfile limiterProfile in setting.keyLimiterProfiles) {
                    if(limiterProfile.name != "JipperResourcePack") continue;
                    profile = limiterProfile;
                    break;
                }
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