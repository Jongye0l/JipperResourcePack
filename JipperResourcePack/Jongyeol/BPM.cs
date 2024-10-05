using JALib.Core;
using JALib.Core.Setting;
using Newtonsoft.Json.Linq;

namespace JipperResourcePack.Jongyeol;

public class BPM : JipperResourcePack.BPM {
    public static JBPMSettings Settings;

    public BPM() : base(nameof(BPM), typeof(JBPMSettings)) {
        Settings = (JBPMSettings) Setting;
    }

    protected override void OnGUI() {
        JipperResourcePack.Main.SettingGUI.AddSettingToggle(ref Settings.CheckPseudo, JipperResourcePack.Main.Instance.Localization["bpm.checkPesudo"]);
        base.OnGUI();
    }

    public class JBPMSettings : BPMSettings {
        public bool CheckPseudo = true;

        public JBPMSettings(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
        }
    }
    
}