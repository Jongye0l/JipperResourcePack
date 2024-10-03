using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using JALib.Tools;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace JipperResourcePack;

public class Status : Feature {
    public static ProgressSetting Settings;
    public static GameObject ProgressObject;
    public static GameObject ProgressBarObject;
    
    public Status() : this(nameof(Status), typeof(ProgressSetting)) {
    }

    protected Status(string name, Type settingType) : base(Main.Instance, name, true, typeof(Status), settingType) {
        Settings = (ProgressSetting) Setting;
    }
    
    protected override void OnEnable() {
        ProgressObject?.SetActive(true);
        if(Settings.ShowProgressBar) ProgressBarObject?.SetActive(true);
    }
    
    protected override void OnDisable() {
        ProgressObject?.SetActive(false);
        if(Settings.ShowProgressBar) ProgressBarObject?.SetActive(false);
    }

    protected override void OnGUI() {
        SettingGUI settingGUI = Main.SettingGUI;
        JALocalization localization = Main.Instance.Localization;
        settingGUI.AddSettingToggle(ref Settings.ShowProgress, localization["progress.showProgress"], Overlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowAccuracy, localization["progress.showAccuracy"], Overlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowXAccuracy, localization["progress.showXAccuracy"], Overlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowMusicTime, localization["progress.showMusicTime"], Overlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowMapTime, localization["progress.showMapTime"], Overlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowMapTimeIfNotMusic, localization["progress.showMapTimeIfNotMusic"]);
        settingGUI.AddSettingToggle(ref Settings.ShowProgressBar, localization["progress.showProgressBar"], () => {
            ProgressBarObject?.SetActive(Settings.ShowProgressBar);
        });
    }

    public class ProgressSetting : JASetting {
        public bool ShowProgress = true;
        public bool ShowAccuracy = false;
        public bool ShowXAccuracy = true;
        public bool ShowMusicTime = true;
        public bool ShowMapTime = false;
        public bool ShowMapTimeIfNotMusic = true;
        public bool ShowProgressBar = true;
        
        public ProgressSetting(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
        }
    }

    [JAPatch(typeof(scrMistakesManager), "CalculatePercentAcc", PatchType.Postfix, false)]
    private static void OnAccuracyChange() {
        Overlay.Instance.UpdateAccuracy();
    }

    [JAPatch(typeof(scrPlanet), "MoveToNextFloor", PatchType.Postfix, false)]
    private static void OnProgressChange() {
        Overlay.Instance.UpdateProgress();
    }
    
    [JAPatch(typeof(scrShowIfDebug), "Awake", PatchType.Postfix, false, TryingCatch = false)]
    private static async void OnShowIfDebugAwake(scrShowIfDebug __instance) {
        try {
            await Task.Yield();
            if(__instance) {
                RectTransform transform = __instance.GetComponent<RectTransform>();
                transform.anchoredPosition = new Vector2(300, transform.anchoredPosition.y);
            }
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    [JAPatch(typeof(scrController), "Awake_Rewind", PatchType.Postfix, false)]
    private static void OnHUDTextAwake(Text ___txtLevelName) {
        if(!___txtLevelName) return;
        RectTransform transform = ___txtLevelName.GetComponent<RectTransform>();
        transform.anchoredPosition = new Vector3(0, -28);
        transform.localScale = new Vector3(0.5f, 0.5f);
        transform.sizeDelta = new Vector2(transform.sizeDelta.x * 2.5f, transform.sizeDelta.y);
        ___txtLevelName.text = ___txtLevelName.text.Replace('\n', ' ');
    }
}