using System;
using System.Text;
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
    [JsonIgnore] public string oldHexString;
    [JsonIgnore] public string hexString;

    public bool SettingGUI(SettingGUI settingGUI, Color defaultColor) {
        bool changed = false;
        oldHexString ??= hexString = GetHexString();
        settingGUI.AddSettingString(ref hexString, oldHexString, "Hex", () => {
            changed = CheckHexString();
        });
        if(changed) {
            defaultColor = GetColor();
            rString = gString = bString = aString = null;
        }
        settingGUI.AddSettingSliderFloat(ref r, defaultColor.r, ref rString, "R", 0, 1, () => changed = true);
        settingGUI.AddSettingSliderFloat(ref g, defaultColor.g, ref gString, "G", 0, 1, () => changed = true);
        settingGUI.AddSettingSliderFloat(ref b, defaultColor.b, ref bString, "B", 0, 1, () => changed = true);
        settingGUI.AddSettingSliderFloat(ref a, defaultColor.a, ref aString, "A", 0, 1, () => changed = true);
        if(changed) oldHexString = hexString = GetHexString();
        return changed;
    }

    private bool CheckHexString() {
        if(string.IsNullOrEmpty(hexString)) {
            hexString = oldHexString;
            return false;
        }
        hexString = hexString.Trim().TrimStart('#');
        if(hexString.Length is not (6 or 8)) return false;
        try {
            float r, g, b, a;
            switch(hexString.Length) {
                case 3 or 4:
                    r = Convert.ToInt32(hexString[..1], 16) / 15f;
                    g = Convert.ToInt32(hexString[1..2], 16) / 15f;
                    b = Convert.ToInt32(hexString[2..3], 16) / 15f;
                    a = hexString.Length == 4 ? Convert.ToInt32(hexString[3..4], 16) / 15f : 1;
                    break;
                case 6 or 8:
                    r = Convert.ToInt32(hexString[..2], 16) / 255f;
                    g = Convert.ToInt32(hexString[2..4], 16) / 255f;
                    b = Convert.ToInt32(hexString[4..6], 16) / 255f;
                    a = hexString.Length == 8 ? Convert.ToInt32(hexString[6..8], 16) / 255f : 1;
                    break;
                default:
                    return false;
            }
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
            oldHexString = GetHexString();
            return true;
        } catch {
            return false;
        }
    }

    private string GetHexString() {
        StringBuilder sb = new();
        sb.Append(Normalize(r).ToString("X2"));
        sb.Append(Normalize(g).ToString("X2"));
        sb.Append(Normalize(b).ToString("X2"));
        if(a < 1) sb.Append(Normalize(a).ToString("X2"));
        return sb.ToString();
    }
    
    private static int Normalize(float value) {
        return value switch {
            <= 0 => 0,
            >= 1 => 255,
            _ => (int) MathF.Round(value * 255)
        };
    }

    public Color GetColor() {
        return new Color(r, g, b, a);
    }

    public static implicit operator Color(ColorCache cache) {
        return cache.GetColor();
    }
}