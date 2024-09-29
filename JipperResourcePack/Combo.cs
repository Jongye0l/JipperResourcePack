using System;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace JipperResourcePack;

public class Combo : Feature {
    public static int combo;
    public static ComboSettings Settings;
    public static GameObject ComboObject;
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
        Main.SettingGUI.AddSettingToggle(ref Settings.EnableAutoCombo, Main.Instance.Localization["combo.enableAutoCombo"], Overlay.Instance.SetupLocationMain);
    }

    public class ComboSettings : JASetting {
        public bool EnableAutoCombo = true;

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