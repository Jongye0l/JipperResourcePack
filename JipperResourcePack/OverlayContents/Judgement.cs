using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using JALib.Tools;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace JipperResourcePack.OverlayContents;

public class Judgement : Feature {
    public static GameObject JudgementObject;
    public static JudgementSettings Settings;
    public static Judgement Instance;

    public Judgement() : base(Main.Instance, nameof(Judgement), true, typeof(Judgement), typeof(JudgementSettings)) {
        Instance = this;
    }

    protected override void OnEnable() {
        JudgementObject?.SetActive(true);
    }
    
    protected override void OnDisable() {
        JudgementObject?.SetActive(false);
    }

    protected override void OnGUI() {
        SettingGUI settingGUI = Main.SettingGUI;
        JALocalization localization = Main.Instance.Localization;
        settingGUI.AddSettingToggle(ref Settings.LocationUp, localization["judgement.locationUp"], Overlay.Instance.SetupLocationJudgement);
    }

    // ReSharper disable once UnusedMember.Local
    [JAPatch(typeof(scrMistakesManager), "AddHit", PatchType.Postfix, true, MaxVersion = 140)]
    [JAPatch(nameof(scrMarginTracker), nameof(scrMarginTracker.AddHit), PatchType.Postfix, true, MinVersion = 141)]
    [JAPatch(typeof(scrMistakesManager), "Reset", PatchType.Postfix, false, MaxVersion = 140)]
    [JAPatch(nameof(scrMarginTracker), nameof(scrMarginTracker.Reset), PatchType.Postfix, false, MinVersion = 141)]
    private static void OnHit() => Overlay.Instance.UpdateJudgement();

    public class JudgementSettings : JASetting {
        public bool LocationUp;

        public JudgementSettings(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
            Settings = this;
        }
    }
}