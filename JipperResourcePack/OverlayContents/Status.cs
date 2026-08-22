using System;
using System.Threading.Tasks;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using JALib.Tools;
using JipperResourcePack.SettingTool;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace JipperResourcePack.OverlayContents;

public class Status : Feature {
    public static ProgressSetting Settings;
    public static GameObject ProgressObject;
    public static GameObject ProgressBarObject;
    public static Status Instance;
    private string _progressDecimalPlacesString;
    private string _accuracyDecimalPlacesString;
    private string _xAccuracyDecimalPlacesString;
    private string _bestDecimalPlacesString;
    
    public Status() : this(typeof(ProgressSetting)) {
    }

    protected Status(Type settingType) : base(Main.Instance, nameof(Status), true, typeof(Status), settingType) {
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
        if(Settings.ShowProgress) {
            if(Settings.ProgressColor.SettingGUI(settingGUI, localization["progress.progressColor"]))
                Overlay.Instance.OverlayTextManager.UpdateProgress(Overlay.Instance);
            settingGUI.AddSettingSliderInt(ref Settings.ProgressDecimalPlaces, 2, ref _progressDecimalPlacesString, localization["progress.progressDecimalPlaces"], 0, 4,
                () => Overlay.Instance.OverlayTextManager.UpdateProgress(Overlay.Instance));
        }
        settingGUI.AddSettingToggle(ref Settings.ShowAccuracy, localization["progress.showAccuracy"], Overlay.Instance.SetupLocationMain);
        if(Settings.ShowAccuracy) {
            if(Settings.AccuracyColor.SettingGUI(settingGUI, localization["progress.accuracyColor"]))
                Overlay.Instance.UpdateAccuracy();
            settingGUI.AddSettingSliderInt(ref Settings.AccuracyDecimalPlaces, 2, ref _accuracyDecimalPlacesString, localization["progress.accuracyDecimalPlaces"], 0, 4,
                () => Overlay.Instance.UpdateAccuracy());
        }
        settingGUI.AddSettingToggle(ref Settings.ShowXAccuracy, localization["progress.showXAccuracy"], Overlay.Instance.SetupLocationMain);
        if(Settings.ShowXAccuracy) {
            if(Settings.XAccuracyColor.SettingGUI(settingGUI, localization["progress.xAccuracyColor"]))
                Overlay.Instance.UpdateAccuracy();
            settingGUI.AddSettingSliderInt(ref Settings.XAccuracyDecimalPlaces, 2, ref _xAccuracyDecimalPlacesString, localization["progress.xAccuracyDecimalPlaces"], 0, 4,
                () => Overlay.Instance.UpdateAccuracy());
        }
        if(!XScoreSupported) GUI.enabled = false;
        settingGUI.AddSettingToggle(ref Settings.ShowXScore, localization["progress.showXScore"], Overlay.Instance.SetupLocationMain);
        if(XScoreSupported) {
            if(Settings.ShowXScore) {
                if(Settings.XScoreColor.SettingGUI(settingGUI, localization["progress.xScoreColor"]))
                    Overlay.Instance.UpdateAccuracy();
                settingGUI.AddSettingEnum(ref Settings.XScoreTextType, localization["progress.xScoreTextType"], () => Overlay.Instance.UpdateAccuracy());
            }
        } else {
            GUI.enabled = true;
            GUILayout.Label(localization["progress.xScoreRequireR148"]);
        }
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
        if(Settings.ShowBest) {
            if(Settings.BestColor.SettingGUI(settingGUI, localization["progress.bestColor"]))
                Overlay.Instance.OverlayTextManager.UpdateBest(Overlay.Instance);
            settingGUI.AddSettingSliderInt(ref Settings.BestDecimalPlaces, 2, ref _bestDecimalPlacesString, localization["progress.bestDecimalPlaces"], 0, 4,
                () => Overlay.Instance.OverlayTextManager.UpdateBest(Overlay.Instance));
        }
        settingGUI.AddSettingToggle(ref Settings.ShowProgressBar, localization["progress.showProgressBar"], () => {
            ProgressBarObject?.SetActive(Settings.ShowProgressBar);
        });
        if(!Settings.ShowProgressBar) return;
        if(Settings.ProgressBarColor.SettingGUI(settingGUI, localization["progress.progressBarColor"]) ||
           Settings.ProgressBarBackgroundColor.SettingGUI(settingGUI, localization["progress.progressBarBackgroundColor"]) ||
           Settings.ProgressBarBorderColor.SettingGUI(settingGUI, localization["progress.progressBarBorderColor"]))
            Overlay.Instance.UpdateProgressBar();
    }

    public static bool XScoreSupported => VersionControl.releaseNumber >= 148;

    public static string GetXScoreText(int xScore, int maxXScore) => Settings.XScoreTextType switch {
        XScoreTextType.WithMax => $"{xScore}/{maxXScore}",
        XScoreTextType.MaxMinus => $"{xScore} (MAX-{maxXScore - xScore})",
        _ => xScore.ToString()
    };

    public class ProgressSetting: JASetting {
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        public bool ShowProgress = true;
        public ColorPerDictionary ProgressColor;
        public int ProgressDecimalPlaces = 2;
        public bool ShowAccuracy;
        public ColorPerDictionary AccuracyColor;
        public int AccuracyDecimalPlaces = 2;
        public bool ShowXAccuracy = true;
        public ColorPerDictionary XAccuracyColor;
        public int XAccuracyDecimalPlaces = 2;
        public bool ShowXScore;
        public ColorPerDictionary XScoreColor;
        public XScoreTextType XScoreTextType = XScoreTextType.WithMax;
        public bool ShowMusicTime = true;
        public ColorPerDictionary MusicTimeColor;
        public bool ShowMapTime;
        public ColorPerDictionary MapTimeColor;
        public bool ShowMapTimeIfNotMusic = true;
        public TimeTextType TimeTextType = TimeTextType.Korean;
        public bool ShowCheckpoint;
        public bool ShowBest;
        public ColorPerDictionary BestColor;
        public int BestDecimalPlaces = 2;
        public bool ShowProgressBar = true;
        public ColorPerDictionary ProgressBarColor;
        public ColorPerDictionary ProgressBarBackgroundColor;
        public ColorPerDictionary ProgressBarBorderColor;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        
        public ProgressSetting(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
            ColorPerDictionary.Setup(ref ProgressColor, [
                (0f, Color.white),
                (1f, new Color(0.87450980392156863f, 0.70980392156862745f, 1))
            ]);
            
            ColorPerDictionary.Setup(ref AccuracyColor, [
                (0.98f, Color.magenta),
                (1f, Color.white)
            ], new Color(1, 0.8549019607843137f, 0));
            
            ColorPerDictionary.Setup(ref XAccuracyColor, [
                (0.98f, Color.white),
                (1f, Color.white)
            ], new Color(1, 0.8549019607843137f, 0));
            
            ColorPerDictionary.Setup(ref XScoreColor, [
                (0.98f, Color.white),
                (1f, Color.white)
            ], new Color(1, 0.8549019607843137f, 0));

            ColorPerDictionary.Setup(ref MusicTimeColor, [(1f, Color.white)]);
            ColorPerDictionary.Setup(ref MapTimeColor, [(1f, Color.white)]);
            
            ColorPerDictionary.Setup(ref BestColor, [
                (0f, Color.white),
                (1f, new Color(0.87450980392156863f, 0.70980392156862745f, 1))
            ]);
            
            ColorPerDictionary.Setup(ref ProgressBarColor, [(1f, new Color(0.9215686f, 0.8039216f, 0.9764706f))]);
            ColorPerDictionary.Setup(ref ProgressBarBackgroundColor, [(1f, Color.white)]);
            ColorPerDictionary.Setup(ref ProgressBarBorderColor, [(1f, Color.black)]);
        }
    }

    // ReSharper disable UnusedMember.Local
    [JAPatch(typeof(scrMistakesManager), "CalculatePercentAcc", PatchType.Postfix, false, MaxVersion = 140)]
    private static void OnAccuracyChange() {
        VersionSafe.RunAfter(() => Overlay.Instance.UpdateAccuracy());
    }
    
    [JAPatch(nameof(scrMarginTracker), nameof(scrMarginTracker.CalculatePercentAcc), PatchType.Postfix, false, MinVersion = 141)]
    private static void OnAccuracyChange(object __instance) {
        int index = 0;
        if(scrController.coopMode) {
            scrMarginTracker marginTracker = __instance.AsUnsafe<scrMarginTracker>();
            for(int i = 0; i < scrPlayerManager.playerCount; i++) {
                if(scrMistakesManager.marginTrackers[i] != marginTracker) continue;
                index = i;
                break;
            }
        }
        VersionSafe.RunAfter(() => Overlay.Instance.UpdateAccuracy(index));
    }

    [JAPatch(typeof(scrPlanet), "MoveToNextFloor", PatchType.Postfix, false)]
    private static void OnProgressChange(scrPlanet __instance) {
        Overlay.Instance.UpdateProgress(__instance);
    }
    
    [JAPatch(typeof(scrShowIfDebug), "Awake", PatchType.Postfix, false, TryingCatch = false)]
    private static void OnShowIfDebugAwake(scrShowIfDebug __instance) {
        VersionSafe.RunAfter(() => {
            try {
                if(__instance) {
                    RectTransform transform = __instance.GetComponent<RectTransform>();
                    transform.anchoredPosition = new Vector2(300, transform.anchoredPosition.y);
                }
            } catch (Exception e) {
                Main.Instance.LogReportException(e);
            }
        });
    }
    // ReSharper restore UnusedMember.Local
}