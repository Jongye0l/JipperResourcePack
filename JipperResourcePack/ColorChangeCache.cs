using JALib.Tools;
using UnityEngine;

namespace JipperResourcePack;

public class ColorChangeCache(Color color) {
    public float r = color.r;
    public string rString;
    public float g = color.g;
    public string gString;
    public float b = color.b;
    public string bString;
    public float a = color.a;
    public string aString;

    public void SettingGUI(SettingGUI settingGUI, Color defaultColor, ref Color color) {
        settingGUI.AddSettingSliderFloat(ref r, defaultColor.r, ref rString, "R", 0, 1);
        settingGUI.AddSettingSliderFloat(ref g, defaultColor.g, ref gString, "G", 0, 1);
        settingGUI.AddSettingSliderFloat(ref b, defaultColor.b, ref bString, "B", 0, 1);
        settingGUI.AddSettingSliderFloat(ref a, defaultColor.a, ref aString, "A", 0, 1);
        color = new Color(r, g, b, a);
    }
}