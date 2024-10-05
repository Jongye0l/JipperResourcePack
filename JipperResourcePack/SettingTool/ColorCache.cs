using JALib.Tools;
using Newtonsoft.Json;
using UnityEngine;

namespace JipperResourcePack.SettingTool;

public class ColorCache(Color color) {
    public float r = color.r;
    [JsonIgnore] public string rString;
    public float g = color.g;
    [JsonIgnore] public string gString;
    public float b = color.b;
    [JsonIgnore] public string bString;
    public float a = color.a;
    [JsonIgnore] public string aString;

    public bool SettingGUI(SettingGUI settingGUI, Color defaultColor) {
        bool changed = false;
        settingGUI.AddSettingSliderFloat(ref r, defaultColor.r, ref rString, "R", 0, 1, () => changed = true);
        settingGUI.AddSettingSliderFloat(ref g, defaultColor.g, ref gString, "G", 0, 1, () => changed = true);
        settingGUI.AddSettingSliderFloat(ref b, defaultColor.b, ref bString, "B", 0, 1, () => changed = true);
        settingGUI.AddSettingSliderFloat(ref a, defaultColor.a, ref aString, "A", 0, 1, () => changed = true);
        return changed;
    }

    public Color GetColor() {
        return new Color(r, g, b, a);
    }

    public static implicit operator Color(ColorCache cache) {
        return cache.GetColor();
    }
}