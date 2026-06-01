using System;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using JALib.Tools;
using JipperResourcePack.SettingTool;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace JipperResourcePack.OverlayContents;

public class Bpm : Feature {
    public static GameObject BpmObject;
    public static BpmSettings Settings;
    private string _bpmColorMaxString;
    public static Bpm Instance;

    public Bpm() : this(typeof(BpmSettings)) {
    }

    public Bpm(Type settingType) : base(Main.Instance, "BPM", true, typeof(Bpm), settingType) {
        Instance = this;
    }

    protected override void OnEnable() {
        BpmObject?.SetActive(true);
    }
    
    protected override void OnDisable() {
        BpmObject?.SetActive(false);
    }

    protected override void OnGUI() {
        SettingGUI settingGUI = Main.SettingGUI;
        settingGUI.AddSettingFloat(ref Settings.BpmColorMax, 8000, ref _bpmColorMaxString, Main.Instance.Localization["bpm.bpmColorMax"], 0);
        if(Settings.BpmColor.SettingGUI(settingGUI, Main.Instance.Localization["bpm.bpmColor"])) Overlay.Instance.UpdateBpm();
    }

    [JAPatch(typeof(scrController), "Hit", PatchType.Postfix, true, MaxVersion = 140)]
    [JAPatch(nameof(scrPlayer), nameof(scrPlayer.Hit), PatchType.Postfix, true, MinVersion = 141)]
    // ReSharper disable once UnusedMember.Local
    private static void OnHit() {
        Overlay.Instance.UpdateBpm();
    }

    public class BpmSettings : JASetting {
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        public float BpmColorMax = 8000;
        public ColorPerDictionary BpmColor = new([
            (0f, Color.white),
            (1f, Color.magenta)
        ]);
        // ReSharper restore FieldCanBeMadeReadOnly.Global

        public BpmSettings(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
            Settings = this;
        }
    }
}