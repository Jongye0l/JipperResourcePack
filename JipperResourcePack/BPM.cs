using System;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using JALib.Tools;
using JipperResourcePack.SettingTool;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace JipperResourcePack;

public class BPM : Feature {
    public static GameObject BPMObject;
    public static BPMSettings Settings;
    private string BpmColorMaxString;

    public BPM() : this(nameof(BPM), typeof(BPMSettings)) {
    }

    public BPM(string name, Type settingType) : base(Main.Instance, name, true, typeof(BPM), settingType) {
    }

    protected override void OnEnable() {
        BPMObject?.SetActive(true);
    }
    
    protected override void OnDisable() {
        BPMObject?.SetActive(false);
    }

    protected override void OnGUI() {
        SettingGUI settingGUI = Main.SettingGUI;
        settingGUI.AddSettingFloat(ref Settings.BpmColorMax, 8000, ref BpmColorMaxString, Main.Instance.Localization["bpm.bpmColorMax"], 0);
        if(Settings.BpmColor.SettingGUI(settingGUI, Main.Instance.Localization["bpm.bpmColor"])) Overlay.Instance.UpdateBPM();
    }

    [JAPatch(typeof(scrController), "Hit", PatchType.Postfix, true)]
    private static void OnHit() {
        Overlay.Instance.UpdateBPM();
    }

    public class BPMSettings : JASetting {
        public float BpmColorMax = 8000;
        public ColorPerDictionary BpmColor = new([
            (0f, Color.white),
            (1f, Color.magenta)
        ]);

        public BPMSettings(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
            Settings = this;
        }
    }
}