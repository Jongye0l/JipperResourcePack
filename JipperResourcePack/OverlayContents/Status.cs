using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using JALib.Tools;
using JipperResourcePack.SettingTool;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace JipperResourcePack.OverlayContents;

public class Status : Feature {
    public static StatusSetting Settings;
    public static GameObject ProgressObject;
    public static GameObject ProgressBarObject;
    public static Status Instance;
    private string _progressDecimalPlacesString;
    private string _accuracyDecimalPlacesString;
    private string _xAccuracyDecimalPlacesString;
    private string _bestDecimalPlacesString;
    private string _timingDecimalPlacesString;

    public Status() : this(typeof(StatusSetting)) {
    }

    protected Status(Type settingType) : base(Main.Instance, nameof(Status), true, typeof(Status), settingType) {
        Settings = (StatusSetting) Setting;
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
                Overlay.Instance.OverlayTextManager?.UpdateProgress(Overlay.Instance);
            settingGUI.AddSettingSliderInt(ref Settings.ProgressDecimalPlaces, 2, ref _progressDecimalPlacesString, localization["progress.progressDecimalPlaces"], 0, 4,
                () => Overlay.Instance.OverlayTextManager?.UpdateProgress(Overlay.Instance));
        }
        settingGUI.AddSettingToggle(ref Settings.ShowAccuracy, localization["progress.showAccuracy"], Overlay.Instance.SetupLocationMain);
        if(Settings.ShowAccuracy) {
            if(Settings.AccuracyColor.SettingGUI(settingGUI, localization["progress.accuracyColor"]))
                Overlay.Instance.UpdateAccuracy();
            settingGUI.AddSettingSliderInt(ref Settings.AccuracyDecimalPlaces, 2, ref _accuracyDecimalPlacesString, localization["progress.accuracyDecimalPlaces"], 0, 4,
                () => Overlay.Instance.UpdateAccuracy());
            settingGUI.AddSettingEnum(ref Settings.AccuracyTextType, localization["progress.accuracyTextType"], () => {
                Overlay.Instance.SetupLocationMain();
                Overlay.Instance.UpdateAccuracy();
            });
        }
        settingGUI.AddSettingToggle(ref Settings.ShowXAccuracy, localization["progress.showXAccuracy"], Overlay.Instance.SetupLocationMain);
        if(Settings.ShowXAccuracy) {
            if(Settings.XAccuracyColor.SettingGUI(settingGUI, localization["progress.xAccuracyColor"]))
                Overlay.Instance.UpdateAccuracy();
            settingGUI.AddSettingSliderInt(ref Settings.XAccuracyDecimalPlaces, 2, ref _xAccuracyDecimalPlacesString, localization["progress.xAccuracyDecimalPlaces"], 0, 4,
                () => Overlay.Instance.UpdateAccuracy());
            settingGUI.AddSettingEnum(ref Settings.XAccuracyTextType, localization["progress.xAccuracyTextType"], () => {
                Overlay.Instance.SetupLocationMain();
                Overlay.Instance.UpdateAccuracy();
            });
        }
        if(!VersionSafe.XScoreSupported) GUI.enabled = false;
        settingGUI.AddSettingToggle(ref Settings.ShowXScore, localization["progress.showXScore"], Overlay.Instance.SetupLocationMain);
        if(VersionSafe.XScoreSupported) {
            if(Settings.ShowXScore) {
                if(Settings.XScoreColor.SettingGUI(settingGUI, localization["progress.xScoreColor"]))
                    Overlay.Instance.UpdateAccuracy();
                settingGUI.AddSettingEnum(ref Settings.XScoreTextType, localization["progress.xScoreTextType"], () => Overlay.Instance.UpdateAccuracy());
                settingGUI.AddSettingEnum(ref Settings.XScorePotentialTextType, localization["progress.xScorePotentialTextType"], () => {
                    Overlay.Instance.SetupLocationMain();
                    Overlay.Instance.UpdateAccuracy();
                });
            }
        } else {
            GUI.enabled = true;
            GUILayout.Label(localization["progress.xScoreRequireR149"]);
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
                Overlay.Instance.OverlayTextManager?.UpdateBest(Overlay.Instance);
            settingGUI.AddSettingSliderInt(ref Settings.BestDecimalPlaces, 2, ref _bestDecimalPlacesString, localization["progress.bestDecimalPlaces"], 0, 4,
                () => Overlay.Instance.OverlayTextManager?.UpdateBest(Overlay.Instance));
        }
        settingGUI.AddSettingToggle(ref Settings.ShowTiming, localization["progress.showTiming"], Overlay.Instance.SetupLocationMain);
        if(Settings.ShowTiming) {
            if(Settings.TimingColor.SettingGUI(settingGUI, localization["progress.timingColor"]))
                Overlay.Instance.RefreshTiming();
            settingGUI.AddSettingSliderInt(ref Settings.TimingDecimalPlaces, 5, ref _timingDecimalPlacesString, localization["progress.timingDecimalPlaces"], 0, 5,
                () => Overlay.Instance.RefreshTiming());
            settingGUI.AddSettingEnum(ref Settings.TimingTextType, localization["progress.timingTextType"], () => {
                Overlay.Instance.SetupLocationMain();
                Overlay.Instance.RefreshTiming();
            });
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

    public static string GetXScoreText(int xScore, int maxXScore) => Settings.XScoreTextType switch {
        XScoreTextType.WithMax => xScore + "/" + maxXScore,
        XScoreTextType.MaxMinus => xScore + " (MAX-" + (maxXScore - xScore) + ")",
        _ => xScore.ToString()
    };

    public static float GetPotentialAccuracy(int[] hits, float acc, int seqId) {
        List<scrFloor> floors = ADOBase.lm.listFloors;
        int remaining = Math.Max(floors.Count - 1 - seqId, 0);
        (int perfect, int accurate) = VersionSafe.GetAccuracyCounts(hits);
        float rate = acc - perfect * 0.0001f;
        int count = accurate == 0 || rate <= 0 || float.IsNaN(rate) ? seqId : (int) Math.Round(accurate / rate);
        if(count < accurate) count = accurate;
        int total = count + remaining;
        return total == 0 ? 1 : (perfect + remaining) * 0.0001f + (float) (accurate + remaining) / total;
    }

    public static Color GetTimingColor(float timing) => Settings.TimingColor.GetColor(1 - Math.Min(Math.Abs(timing), 150) / 150);

    public class StatusSetting: JASetting {
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        public bool ShowProgress = true;
        public ColorPerDictionary ProgressColor;
        public int ProgressDecimalPlaces = 2;
        public bool ShowAccuracy;
        public ColorPerDictionary AccuracyColor;
        public int AccuracyDecimalPlaces = 2;
        public PotentialTextType AccuracyTextType = PotentialTextType.Current;
        public bool ShowXAccuracy = true;
        public ColorPerDictionary XAccuracyColor;
        public int XAccuracyDecimalPlaces = 2;
        public PotentialTextType XAccuracyTextType = PotentialTextType.Current;
        public bool ShowXScore;
        public ColorPerDictionary XScoreColor;
        public XScoreTextType XScoreTextType = XScoreTextType.MaxMinus;
        public PotentialTextType XScorePotentialTextType = PotentialTextType.Current;
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
        public bool ShowTiming;
        public ColorPerDictionary TimingColor;
        public int TimingDecimalPlaces = 5;
        public TimingTextType TimingTextType = TimingTextType.Timing;
        public bool ShowProgressBar = true;
        public ColorPerDictionary ProgressBarColor;
        public ColorPerDictionary ProgressBarBackgroundColor;
        public ColorPerDictionary ProgressBarBorderColor;
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        
        public StatusSetting(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
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
            
            ColorPerDictionary.Setup(ref TimingColor, [
                (0f, Color.red),
                (0.5f, new Color(0.9882352941176471f, 1, 0.3019607843137255f)),
                (1f, new Color(0.3725490196078431f, 1, 0.3119607843137255f))
            ], new Color(1, 0.8549019607843137f, 0));

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
    
    [JAPatch(typeof(scrMisc), "GetHitMargin", PatchType.Postfix, false, MaxVersion = 140)]
    // ReSharper disable once InconsistentNaming
    private static void OnHitMarginChange(float hitangle, float refangle, bool isCW, float bpmTimesSpeed, float conductorPitch) {
        if(!Settings.ShowTiming || RDC.auto || scrController.instance.currFloor.nextfloor && scrController.instance.currFloor.nextfloor.auto) return;
        float angle = (hitangle - refangle) * (isCW ? 1 : -1) * 57.29578f;
        float timing = angle / 180 / bpmTimesSpeed / conductorPitch * 60000;
        Overlay.Instance.UpdateTiming(timing);
    }

    private static scrPlanet _planet;
    
    [JAPatch(typeof(scrPlanet), nameof(scrPlanet.SwitchChosen), PatchType.Prefix, true, MinVersion = 141, TryingCatch = false)]
    private static void SwitchChosenPrefix(scrPlanet __instance) => _planet = __instance;

    [JAPatch(typeof(scrPlanet), nameof(scrPlanet.SwitchChosen), PatchType.Finalizer, false, MinVersion = 141, TryingCatch = false)]
    private static void SwitchChosenFinalizer() => _planet = null;

    [JAPatch(typeof(scrMisc), "GetHitMargin", PatchType.Postfix, false, MinVersion = 141, MaxVersion = 148)]
    public static void OnHitMarginChangeR141(float hitangle, float refangle, bool isCW, float bpmTimesSpeed, float conductorPitch, HitMargin __result) {
        if(!IsTimingAvailable(_planet)) return;

        float angle = (hitangle - refangle) * (isCW ? 1 : -1) * 57.29578f;
        float timing = angle / 180 / bpmTimesSpeed / conductorPitch * 60000;
        Overlay.Instance.UpdateTiming(timing, _planet.player.playerID);
    }

    [JAPatch(typeof(scrMisc), nameof(scrMisc.GetHitMarginInDeg), PatchType.Postfix, false, MinVersion = 149)]
    private static void GetHitMarginInDegProxyR149(float hitAngle, float refAngle, bool clockwise, float floorBpm, float conductorPitch, HitMargin __result) {
        if(!IsTimingAvailable(_planet)) return;

        float angle = (hitAngle - refAngle) * (clockwise ? 1 : -1) * 57.29578f;
        float timing = angle / 180 / floorBpm / conductorPitch * 60000;
        Overlay.Instance.UpdateTiming(timing, _planet.player.playerID);
    }

    [JAPatch(typeof(scrMisc), nameof(scrMisc.GetHitMarginInSec), PatchType.Postfix, false, MinVersion = 149)]
    private static void GetHitMarginInSecProxyR149(double timeDiff, HitMargin __result) {
        if(!IsTimingAvailable(_planet)) return;

        float timing = (float) timeDiff * 1000;
        Overlay.Instance.UpdateTiming(timing, _planet.player.playerID);
    }

    private static bool IsTimingAvailable(scrPlanet planet) =>
        Settings.ShowTiming && !RDC.auto && !planet.player.auto && (!planet.currfloor.nextfloor || !planet.currfloor.nextfloor.auto);

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