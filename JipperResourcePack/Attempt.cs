using JALib.Core;
using JALib.Core.Setting;
using JALib.Tools;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace JipperResourcePack;

public class Attempt : Feature {
    public static AttemptSetting Settings;
    public static Attempt Instance;
    public static GameObject AttemptObject;

    public Attempt() : base(Main.Instance, nameof(Attempt), true, typeof(Attempt), typeof(AttemptSetting)) {
        Instance = this;
        Settings = (AttemptSetting) Setting;
    }

    protected override void OnEnable() {
        AttemptObject.SetActive(true);
        if(scrLevelMaker.instance) Overlay.Instance.UpdateAttempts();
    }

    protected override void OnGUI() {
        SettingGUI settingGUI = Main.SettingGUI;
        JALocalization localization = Main.Instance.Localization;
        settingGUI.AddSettingToggle(ref Settings.ShowAttempt, localization["attempt.showAttempt"]);
        settingGUI.AddSettingToggle(ref Settings.ShowFullAttempt, localization["attempt.showFullAttempt"]);
    }

    protected override void OnDisable() {
        AttemptObject.SetActive(false);
    }

    public class AttemptSetting : JASetting {
        public bool ShowAttempt = true;
        public bool ShowFullAttempt;
        
        public AttemptSetting(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
        }
    }
}