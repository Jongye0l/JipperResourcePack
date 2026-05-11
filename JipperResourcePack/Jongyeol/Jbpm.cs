using JALib.Core;
using Newtonsoft.Json.Linq;

namespace JipperResourcePack.Jongyeol;

public class Jbpm : OverlayContents.BPM {
    public static new JbpmSettings Settings;

    public Jbpm() : base(typeof(JbpmSettings)) {
        Settings = (JbpmSettings) Setting;
    }

    protected override void OnGUI() {
        Main.SettingGUI.AddSettingToggle(ref Settings.CheckPseudo, Main.Instance.Localization["bpm.checkPesudo"]);
        base.OnGUI();
    }

    public class JbpmSettings(JAMod mod, JObject jsonObject = null) : BPMSettings(mod, jsonObject) {
        public bool CheckPseudo = true;
    }
}