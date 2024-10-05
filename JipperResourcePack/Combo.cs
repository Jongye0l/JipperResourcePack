using System;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using JALib.Tools;
using JipperResourcePack.SettingTool;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace JipperResourcePack;

public class Combo : Feature {
    public static int combo;
    public static ComboSettings Settings;
    public static GameObject ComboObject;
    private string ComboColorMaxString;
    public Combo() : this(nameof(Combo), typeof(ComboSettings)) {
    }

    public Combo(string name, Type settingType) : base(Main.Instance, name, settingType: settingType) {
        AddPatch();
    }

    protected virtual void AddPatch() {
        Patcher.AddPatch(OnHit);
    }
    
    protected override void OnEnable() {
        ComboObject?.SetActive(true);
    }
    
    protected override void OnDisable() {
        ComboObject?.SetActive(false);
    }

    protected override void OnGUI() {
        SettingGUI settingGUI = Main.SettingGUI;
        settingGUI.AddSettingToggle(ref Settings.EnableAutoCombo, Main.Instance.Localization["combo.enableAutoCombo"], Overlay.Instance.SetupLocationMain);
        settingGUI.AddSettingInt(ref Settings.ComboColorMax, 1000, ref ComboColorMaxString, Main.Instance.Localization["combo.comboColorMax"], 0);
        if(Settings.ComboColor.SettingGUI(settingGUI, Main.Instance.Localization["combo.comboColor"])) Overlay.Instance.UpdateComboColor(combo);
    }

    public class ComboSettings : JASetting {
        public bool EnableAutoCombo = true;
        public int ComboColorMax = 1000;
        public ColorPerDictionary ComboColor = new([
            (0f, new Color(0.8745098039215686f, 0.7098039215686275f, 1)),
            (1f, new Color(0.7176470588235294f, 0.3490196078431373f, 1))
        ]);

        public ComboSettings(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
            Settings = this;
        }
    }
    
    [JAPatch(typeof(scrMistakesManager), "AddHit", PatchType.Postfix, true)]
    public static void OnHit(HitMargin hit) {
        if(hit == HitMargin.Perfect || Settings.EnableAutoCombo && hit == HitMargin.Auto) Overlay.Instance.UpdateCombo(++combo, true);
        else if(Settings.EnableAutoCombo || hit != HitMargin.Auto) Overlay.Instance.UpdateCombo(combo = 0, false);
    }
}