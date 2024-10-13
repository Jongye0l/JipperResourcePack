using System;
using System.Threading.Tasks;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using JALib.Tools;
using JipperResourcePack.SettingTool;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace JipperResourcePack;

public class Status : Feature {
    public static ProgressSetting Settings;
    public static GameObject ProgressObject;
    public static GameObject ProgressBarObject;
    public static Status Instance;
    
    public Status() : this(nameof(Status), typeof(ProgressSetting)) {
    }

    protected Status(string name, Type settingType) : base(Main.Instance, name, true, typeof(Status), settingType) {
        Settings = (ProgressSetting) Setting;
        Instance = this;
    }
    
    protected override void OnEnable() {
        ProgressObject?.SetActive(true);
        if(scrLevelMaker.instance) Overlay.Instance?.SetupLocationMain();
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
        if(Settings.ShowProgress && Settings.ProgressColor.SettingGUI(settingGUI, localization["progress.progressColor"]))
            Overlay.Instance.UpdateProgressText();
        settingGUI.AddSettingToggle(ref Settings.ShowAccuracy, localization["progress.showAccuracy"], Overlay.Instance.SetupLocationMain);
        if(Settings.ShowAccuracy && Settings.AccuracyColor.SettingGUI(settingGUI, localization["progress.accuracyColor"]))
            Overlay.Instance.UpdateAccuracy();
        settingGUI.AddSettingToggle(ref Settings.ShowXAccuracy, localization["progress.showXAccuracy"], Overlay.Instance.SetupLocationMain);
        if(Settings.ShowXAccuracy && Settings.XAccuracyColor.SettingGUI(settingGUI, localization["progress.xAccuracyColor"]))
            Overlay.Instance.UpdateAccuracy();
        settingGUI.AddSettingToggle(ref Settings.ShowMusicTime, localization["progress.showMusicTime"], Overlay.Instance.SetupLocationMain);
        if(Settings.ShowMusicTime && Settings.MusicTimeColor.SettingGUI(settingGUI, localization["progress.musicTimeColor"]))
            Overlay.Instance.UpdateTime();
        settingGUI.AddSettingToggle(ref Settings.ShowMapTime, localization["progress.showMapTime"], Overlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowMapTimeIfNotMusic, localization["progress.showMapTimeIfNotMusic"], Overlay.Instance.UpdateTime);
        if(Settings.ShowMapTime && Settings.MapTimeColor.SettingGUI(settingGUI, localization["progress.mapTimeColor"]))
            Overlay.Instance.UpdateTime();
        settingGUI.AddSettingEnum(ref Settings.TimeTextType, localization["progress.timeTextType"], Overlay.Instance.UpdateTime);
        settingGUI.AddSettingToggle(ref Settings.ShowCheckpoint, localization["progress.showCheckpoint"], Overlay.Instance.SetupLocationMain);
        settingGUI.AddSettingToggle(ref Settings.ShowBest, localization["progress.showBest"], Overlay.Instance.SetupLocationMain);
        if(Settings.ShowBest && Settings.BestColor.SettingGUI(settingGUI, localization["progress.bestColor"]))
            Overlay.Instance.UpdateBestText();
        settingGUI.AddSettingToggle(ref Settings.ShowProgressBar, localization["progress.showProgressBar"], () => {
            ProgressBarObject?.SetActive(Settings.ShowProgressBar);
        });
        if(!Settings.ShowProgressBar) return;
        if(Settings.ProgressBarColor.SettingGUI(settingGUI, localization["progress.progressBarColor"]) ||
           Settings.ProgressBarBackgroundColor.SettingGUI(settingGUI, localization["progress.progressBarBackgroundColor"]) ||
           Settings.ProgressBarBorderColor.SettingGUI(settingGUI, localization["progress.progressBarBorderColor"]))
            Overlay.Instance.UpdateProgressBar();
    }

    public class ProgressSetting : JASetting {
        public bool ShowProgress = true;
        public ColorPerDictionary ProgressColor = new([
            (0f, Color.white),
            (1f, new Color(0.87450980392156863f, 0.70980392156862745f, 1))
        ]);
        public bool ShowAccuracy = false;
        public ColorPerDictionary AccuracyColor = new([
            (0.98f, Color.magenta),
            (1f, Color.white)
        ], new Color(1, 0.8549019607843137f, 0));
        public bool ShowXAccuracy = true;
        public ColorPerDictionary XAccuracyColor = new([
            (0.98f, Color.magenta),
            (1f, Color.white)
        ], new Color(1, 0.8549019607843137f, 0)) ;
        public bool ShowMusicTime = true;
        public ColorPerDictionary MusicTimeColor = new([ (1f, Color.white) ]);
        public bool ShowMapTime = false;
        public ColorPerDictionary MapTimeColor = new([(1f, Color.white)]);
        public bool ShowMapTimeIfNotMusic = true;
        public TimeTextType TimeTextType = TimeTextType.Korean;
        public bool ShowCheckpoint = false;
        public bool ShowBest = false;
        public ColorPerDictionary BestColor = new([
            (0f, Color.white),
            (1f, new Color(0.87450980392156863f, 0.70980392156862745f, 1))
        ]);
        public bool ShowProgressBar = true;
        public ColorPerDictionary ProgressBarColor = new([(1f, new Color(0.9215686f, 0.8039216f, 0.9764706f))]);
        public ColorPerDictionary ProgressBarBackgroundColor = new([(1f, Color.white)]);
        public ColorPerDictionary ProgressBarBorderColor = new([(1f, Color.black)]);
        
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
        float size = Main.Settings.Size;
        transform.anchoredPosition = new Vector2(0, -20 - 7 * size);
        transform.localScale = new Vector3(0.5f * size, 0.5f * size);
        transform.sizeDelta = new Vector2(Math.Abs(transform.sizeDelta.x * 2.5f), transform.sizeDelta.y);
        ___txtLevelName.text = ___txtLevelName.text.Replace('\n', ' ');
    }
}