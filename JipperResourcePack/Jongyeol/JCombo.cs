using JALib.Core;
using JALib.Core.Patch;
using JipperResourcePack.OverlayContents;
using Newtonsoft.Json.Linq;

namespace JipperResourcePack.Jongyeol;

public class JCombo : Combo {
    public new static JComboSettings Settings;
    protected override void AddPatch() {
        Patcher.AddPatch(OnHit2);
    }

    public JCombo() : base(nameof(JCombo), typeof(JComboSettings)) {
        Settings = (JComboSettings) Setting;
    }

    protected override void OnGUI() {
        base.OnGUI();
        Main.SettingGUI.AddSettingToggle(ref Settings.YellowCombo, Main.Instance.Localization["combo.yellowCombo"]);
    }

    [JAPatch(typeof(scrMistakesManager), "AddHit", PatchType.Postfix, true)]
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

    public class JComboSettings(JAMod mod, JObject jsonObject = null) : ComboSettings(mod, jsonObject) {
        public bool YellowCombo = true;
    }
}