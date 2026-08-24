using JALib.Core;
using JALib.Core.Patch;
using JALib.Tools;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

namespace JipperResourcePack.Jongyeol;

public class JStatus : OverlayContents.Status {
    public static new JProgressSetting Settings;
    private static bool _auto;

    public JStatus() : base(typeof(JProgressSetting)) {
        Settings = (JProgressSetting) Setting;
        Patcher.AddPatch(typeof(JStatus));
    }

    protected override void OnGUI() {
        base.OnGUI();
        SettingGUI settingGUI = Main.SettingGUI;
        JALocalization localization = Main.Instance.Localization;
        settingGUI.AddSettingToggle(ref Settings.ShowFPS, localization["progress.showFPS"], JOverlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowAuthor, localization["progress.showAuthor"], JOverlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowState, localization["progress.showState"], JOverlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.HideDebugText, localization["progress.hideDebugText"], JOverlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowDeath, localization["progress.showDeath"], JOverlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowStart, localization["progress.showStart"], JOverlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.RemoveNotRequireInAuto, localization["progress.removeNotRequireInAuto"], JOverlay.Instance.SetupLocationMain);
    }

    // ReSharper disable UnusedMember.Local
    [JAPatch(typeof(scrShowIfDebug), "Update", PatchType.Prefix, false)]
    private static bool HideDebugText(Text ___txt) => !Settings.HideDebugText || (___txt.enabled = false);

    [JAPatch(typeof(RDC), "set_auto", PatchType.Postfix, false)]
    private static void OnAutoChange() {
        if(ADOBase.isScnGame || _auto == RDC.auto) return;
        JOverlay.Instance.SetupLocationMain();
        JOverlay.Instance.UpdateState();
        _auto = RDC.auto;
    }

    [JAPatch(nameof(scrPlayer), nameof(scrPlayer.Die), PatchType.Postfix, false, MinVersion = 141)]
    private static void OnDie(PlanetarySystem ___planetarySystem) {
        JOverlay.Instance.UpdateProgress(___planetarySystem.chosenPlanet);
    }
    // ReSharper restore UnusedMember.Local

    public class JProgressSetting : ProgressSetting {
        public bool ShowFPS = true;
        public bool ShowAuthor = true;
        public bool ShowState = true;
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public bool HideDebugText = true;
        public bool ShowDeath = true;
        public bool ShowStart = true;
        public bool RemoveNotRequireInAuto = true;

        public JProgressSetting(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
            if(jsonObject == null) ShowAccuracy = true;
        }
    }
}