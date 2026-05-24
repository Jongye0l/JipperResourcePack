using JALib.Core;
using JALib.Core.Patch;
using JipperResourcePack.OverlayContents;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

namespace JipperResourcePack.Jongyeol;

public class JCombo : Combo {
    public new static JComboSettings Settings;
    protected override void AddPatch() {
        Patcher.AddPatch(OnHit2);
    }

    public JCombo() : base(typeof(JComboSettings)) {
        Settings = (JComboSettings) Setting;
    }

    protected override void OnGUI() {
        base.OnGUI();
        Main.SettingGUI.AddSettingToggle(ref Settings.YellowCombo, Main.Instance.Localization["combo.yellowCombo"]);
    }

    [JAPatch(typeof(scrMistakesManager), "AddHit", PatchType.Postfix, true, MaxVersion = 140)]
    [JAPatch(nameof(scrMarginTracker), nameof(scrMarginTracker.AddHit), PatchType.Postfix, true, MinVersion = 141)]
    private static void OnHit2(HitMargin hit) {
        if(!Settings.YellowCombo) {
            OnHit(hit);
            return;
        }
        switch(hit) {
            case HitMargin.Perfect:
            case HitMargin.EarlyPerfect:
            case HitMargin.LatePerfect:
            case HitMargin.Auto when Settings.EnableAutoCombo:
                Overlay.Instance.UpdateCombo(++ComboCount, true);
                break;
            case HitMargin.Auto when !Settings.EnableAutoCombo:
                break;
            default:
                Overlay.Instance.UpdateCombo(ComboCount = 0, false);
                break;
        }
        if(hit is not HitMargin.Perfect and not HitMargin.Auto) JOverlay.Instance.PerfectToCombo();
    }


    [JAPatch(typeof(scrController), "Awake_Rewind", PatchType.Postfix, false)]
    public static void OnHUDTextAwake2(Text ___txtLevelName) => OnHUDTextAwake(___txtLevelName);

    public class JComboSettings(JAMod mod, JObject jsonObject = null) : ComboSettings(mod, jsonObject) {
        public bool YellowCombo = true;
    }
}