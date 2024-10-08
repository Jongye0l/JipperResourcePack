using JALib.Core;
using JALib.Core.Patch;
using JALib.Tools;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

namespace JipperResourcePack.Jongyeol;

public class Status : JipperResourcePack.Status {
    public new static JProgressSetting Settings;

    public Status() : base(nameof(Status), typeof(JProgressSetting)) {
        Settings = (JProgressSetting) Setting;
        Patcher.AddPatch(typeof(Status));
    }

    protected override void OnGUI() {
        base.OnGUI();
        SettingGUI settingGUI = JipperResourcePack.Main.SettingGUI;
        JALocalization localization = JipperResourcePack.Main.Instance.Localization;
        settingGUI.AddSettingToggle(ref Settings.ShowFPS, localization["progress.showFPS"], JOverlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowAuthor, localization["progress.showAuthor"], JOverlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowState, localization["progress.showState"], JOverlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.HideDebugText, localization["progress.hideDebugText"], JOverlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowCheckpoint, localization["progress.showCheckpoint"], JOverlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowDeath, localization["progress.showDeath"], JOverlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowStart, localization["progress.showStart"], JOverlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowTiming, localization["progress.showTiming"], JOverlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.RemoveNotRequireInAuto, localization["progress.removeNotRequireInAuto"], JOverlay.Instance.SetupLocationMain);
    }

    [JAPatch(typeof(scrShowIfDebug), "Update", PatchType.Prefix, false)]
    private static bool HideDebugText(Text ___txt) => !Settings.HideDebugText || (___txt.enabled = false);

    [JAPatch(typeof(RDC), "set_auto", PatchType.Postfix, false)]
    private static void OnAutoChange() {
        if(!ADOBase.isScnGame) return;
        JipperResourcePack.Main.Instance.Log("Auto: " + RDC.auto);
        JOverlay.Instance.SetupLocationMain();
        JOverlay.Instance.UpdateState();
    }

    [JAPatch(typeof(scrMisc), "GetHitMargin", PatchType.Postfix, false)]
    private static void OnHitMarginChange(float hitangle, float refangle, bool isCW, float bpmTimesSpeed, float conductorPitch) {
        float angle = (hitangle - refangle) * (isCW ? 1 : -1) * 57.29578f;
        float timing = angle / 180 / bpmTimesSpeed / conductorPitch * 60000;
        JOverlay.Instance.UpdateTiming(timing);
    }

    public class JProgressSetting : ProgressSetting {
        public bool ShowFPS = true;
        public bool ShowAuthor = true;
        public bool ShowState = true;
        public bool HideDebugText = true;
        public bool ShowCheckpoint = true;
        public bool ShowDeath = true;
        public bool ShowStart = true;
        public bool ShowTiming = true;
        public bool RemoveNotRequireInAuto = true;

        public JProgressSetting(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
            if(jsonObject == null) ShowAccuracy = true;
        }
    }
}