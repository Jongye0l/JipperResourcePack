using System;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using JALib.Tools;
using JipperResourcePack.SettingTool;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace JipperResourcePack.OverlayContents;

public class Combo : Feature {
    public static int ComboCount;
    public static ComboSettings Settings;
    public static GameObject ComboObject;
    public static RectTransform ComboTransform;
    private string _comboColorMaxString;
    public static Combo Instance;

    public Combo() : base(Main.Instance, nameof(Combo), true, typeof(Combo), typeof(ComboSettings)) {
        Instance = this;
    }

    protected override void OnEnable() {
        ComboObject?.SetActive(true);
    }

    protected override void OnDisable() {
        ComboObject?.SetActive(false);
    }

    protected override void OnGUI() {
        SettingGUI settingGUI = Main.SettingGUI;
        settingGUI.AddSettingToggle(ref Settings.EnableAutoCombo, Main.Instance.Localization["combo.enableAutoCombo"]);
        settingGUI.AddSettingEnum(ref Settings.ComboJudgementTier, Main.Instance.Localization["combo.comboJudgementTier"]);
        settingGUI.AddSettingInt(ref Settings.ComboColorMax, 1000, ref _comboColorMaxString, Main.Instance.Localization["combo.comboColorMax"], 0);
        if(Settings.ComboColor.SettingGUI(settingGUI, Main.Instance.Localization["combo.comboColor"])) Overlay.Instance.UpdateComboColor(ComboCount);
    }

    public class ComboSettings : JASetting {
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        public bool EnableAutoCombo = true;
        public ComboTier ComboJudgementTier = ComboTier.Green;
        public int ComboColorMax = 1000;
        public ColorPerDictionary ComboColor;
        // ReSharper restore FieldCanBeMadeReadOnly.Global

        public ComboSettings(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
            Settings = this;
            ColorPerDictionary.Setup(ref ComboColor, [
                (0f, new Color(0.8745098039215686f, 0.7098039215686275f, 1)),
                (1f, new Color(0.7176470588235294f, 0.3490196078431373f, 1))
            ]);

            if(jsonObject == null) return;
            
            if(jsonObject.TryGetValue("YellowCombo", out JToken totalCount)) {
                jsonObject.Remove("YellowCombo");
                if(totalCount.Value<bool>()) ComboJudgementTier = ComboTier.Yellow;
            }
        }
    }
    
    [JAPatch(typeof(scrMistakesManager), "AddHit", PatchType.Postfix, true, MaxVersion = 140)]
    [JAPatch(nameof(scrMarginTracker), nameof(scrMarginTracker.AddHit), PatchType.Postfix, true, MinVersion = 141)]
    public static void OnHit(HitMargin hit) {
        switch(hit) {
            case HitMargin.XPerfect:
                Overlay.Instance.UpdateCombo(++ComboCount, true);
                break;
            case HitMargin.PerfectMinus or HitMargin.PerfectPlus when Settings.ComboJudgementTier >= ComboTier.Green:
                Overlay.Instance.UpdateCombo(++ComboCount, true);
                Overlay.Instance.ChangeComboText(ComboTier.Green);
                break;
            case HitMargin.EarlyPerfect or HitMargin.LatePerfect when Settings.ComboJudgementTier == ComboTier.Yellow:
                Overlay.Instance.UpdateCombo(++ComboCount, true);
                Overlay.Instance.ChangeComboText(ComboTier.Yellow);
                break;
            case HitMargin.Auto when Settings.EnableAutoCombo:
                Overlay.Instance.UpdateCombo(++ComboCount, true);
                break;
            case HitMargin.Auto:
                break;
            default:
                Overlay.Instance.UpdateCombo(ComboCount = 0, false);
                Overlay.Instance.ChangeComboText(ComboTier.Yellow);
                break;
        }
    }

    [JAPatch(typeof(scrController), "Awake_Rewind", PatchType.Postfix, false)]
    public static void OnHUDTextAwake(Text ___txtLevelName) {
        if(!___txtLevelName) return;
        RectTransform transform = ___txtLevelName.GetComponent<RectTransform>();
        float size = Main.Settings.Size;
        transform.anchoredPosition = new Vector2(0, -20 - 7 * size);
        transform.localScale = new Vector3(0.5f * size, 0.5f * size);
        transform.sizeDelta = new Vector2(Math.Abs(transform.sizeDelta.x * 2.5f), transform.sizeDelta.y);
        ___txtLevelName.text = ___txtLevelName.text.Replace('\n', ' ');
    }
}