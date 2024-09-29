using JALib.Core;
using JALib.Core.Setting;
using Newtonsoft.Json.Linq;

namespace JipperResourcePack.Jongyeol;

public class BPM : JipperResourcePack.BPM {
    public static BPMSettings Settings;

    public BPM() : base(nameof(BPM), typeof(BPMSettings)) {
        Settings = (BPMSettings) Setting;
    }

    protected override void OnGUI() {
        JipperResourcePack.Main.SettingGUI.AddSettingToggle(ref Settings.CheckPseudo, JipperResourcePack.Main.Instance.Localization["bpm.checkPesudo"]);
    }

    public class BPMSettings : JASetting {
        public bool CheckPseudo = true;

        public BPMSettings(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
        }
    }
    
}