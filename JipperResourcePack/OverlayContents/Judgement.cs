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
        settingGUI.AddSettingToggle(ref Settings.LocationUp, localization["judgement.locationUp"], () => Overlay.Instance.OverlayTextManager.SetupUnderTextLocation(Overlay.Instance));
    }

    // ReSharper disable UnusedMember.Local
    [JAPatch(typeof(scrMistakesManager), "AddHit", PatchType.Postfix, true, MaxVersion = 140)]
    [JAPatch(typeof(scrMistakesManager), "Reset", PatchType.Postfix, false, MaxVersion = 140)]
    private static void OnHit() => Overlay.Instance.UpdateJudgement();

    [JAPatch(nameof(scrMarginTracker), nameof(scrMarginTracker.AddHit), PatchType.Postfix, true, MinVersion = 141)]
    [JAPatch(nameof(scrMarginTracker), nameof(scrMarginTracker.Reset), PatchType.Postfix, false, MinVersion = 141)]
    private static void OnHit(object __instance) {
        int index = 0;
        if(scrController.coopMode) {
            scrMarginTracker marginTracker = __instance.AsUnsafe<scrMarginTracker>();
            for(int i = 0; i < scrPlayerManager.playerCount; i++) {
                if(scrMistakesManager.marginTrackers[i] != marginTracker) continue;
                index = i;
                break;
            }
        }
        Overlay.Instance.UpdateJudgement(index);
    }
    // ReSharper restore UnusedMember.Local

    public class JudgementSettings : JASetting {
        public bool LocationUp;

        public JudgementSettings(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
            Settings = this;
        }
    }
}