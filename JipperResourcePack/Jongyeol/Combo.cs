using JALib.Core;
using JALib.Core.Patch;
using Newtonsoft.Json.Linq;

namespace JipperResourcePack.Jongyeol;

public class Combo : JipperResourcePack.Combo {
    public new static JComboSettings Settings;
    protected override void AddPatch() {
        Patcher.AddPatch(OnHit2);
    }

    public Combo() : base(nameof(Combo), typeof(JComboSettings)) {
        Settings = (JComboSettings) Setting;
    }

    protected override void OnGUI() {
        base.OnGUI();
        JipperResourcePack.Main.SettingGUI.AddSettingToggle(ref Settings.YellowCombo, JipperResourcePack.Main.Instance.Localization["combo.yellowCombo"]);
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
                Overlay.Instance.UpdateCombo(++combo, true);
                break;
            case HitMargin.Auto when !Settings.EnableAutoCombo:
                break;
            default:
                Overlay.Instance.UpdateCombo(combo = 0, false);
                break;
        }
        if(hit is not HitMargin.Perfect and not HitMargin.Auto) JOverlay.Instance.PerfectToCombo();
    }

    public class JComboSettings : ComboSettings {
        public bool YellowCombo = true;

        public JComboSettings(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
        }
    }
}