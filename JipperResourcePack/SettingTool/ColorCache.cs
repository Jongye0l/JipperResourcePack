using System;
using System.Text;
using JALib.Tools;
using Newtonsoft.Json;
using UnityEngine;
using Unsafe = System.Runtime.CompilerServices.Unsafe;

namespace JipperResourcePack.SettingTool;

public class ColorCache(Color color) {
    // ReSharper disable InconsistentNaming
    public float r = color.r;
    public float g = color.g;
    public float b = color.b;
    public float a = color.a;
    // ReSharper restore InconsistentNaming
    [JsonIgnore] private Color _originalColor = color;
    [JsonIgnore] private string _rString;
    [JsonIgnore] private string _gString;
    [JsonIgnore] private string _bString;
    [JsonIgnore] private string _aString;
    [JsonIgnore] private string _oldHexString;
    [JsonIgnore] private string _hexString;

    public bool SettingGUI(SettingGUI settingGUI, bool reset = true) {
        bool changed = false;
        _oldHexString ??= _hexString = GetHexString();

        settingGUI.AddSettingString(ref _hexString, _oldHexString, "Hex", () => {
            changed = CheckHexString();
        });
        if(changed) _rString = _gString = _bString = _aString = null;

        settingGUI.AddSettingSliderFloat(ref r, r, ref _rString, "R", 0, 1, () => changed = true);
        settingGUI.AddSettingSliderFloat(ref g, g, ref _gString, "G", 0, 1, () => changed = true);
        settingGUI.AddSettingSliderFloat(ref b, b, ref _bString, "B", 0, 1, () => changed = true);
        settingGUI.AddSettingSliderFloat(ref a, a, ref _aString, "A", 0, 1, () => changed = true);

        if(reset && GUILayout.Button(Main.Instance.Localization["Color.Reset"])) {
            Reset();
            changed = true;
        }
        if(changed) _oldHexString = _hexString = GetHexString();
        return changed;
    }

    public void Reset() => SetPreset(_originalColor);

    private bool CheckHexString() {
        if(string.IsNullOrEmpty(_hexString)) {
            _hexString = _oldHexString;
            return false;
        }
        _hexString = _hexString.Trim().TrimStart('#');
        if(_hexString.Length is not (6 or 8)) return false;
        try {
            switch(_hexString.Length) {
                case 3 or 4:
                    r = Convert.ToInt32(_hexString[..1], 16) / 15f;
                    g = Convert.ToInt32(_hexString[1..2], 16) / 15f;
                    b = Convert.ToInt32(_hexString[2..3], 16) / 15f;
                    a = _hexString.Length == 4 ? Convert.ToInt32(_hexString[3..4], 16) / 15f : 1;
                    break;
                case 6 or 8:
                    r = Convert.ToInt32(_hexString[..2], 16) / 255f;
                    g = Convert.ToInt32(_hexString[2..4], 16) / 255f;
                    b = Convert.ToInt32(_hexString[4..6], 16) / 255f;
                    a = _hexString.Length == 8 ? Convert.ToInt32(_hexString[6..8], 16) / 255f : 1;
                    break;
                default:
                    return false;
            }
            _oldHexString = GetHexString();
            return true;
        } catch {
            return false;
        }
    }

    private string GetHexString() {
        StringBuilder sb = VersionSafe.GetSharedBuilder();
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

    public void SetColor(ref Color color) {
        color.r = r;
        color.g = g;
        color.b = b;
        color.a = a;
    }

    public void ApplyHue(Color baseColor) {
        Color newColor = ApplyHuePreview(baseColor);
        r = newColor.r;
        g = newColor.g;
        b = newColor.b;
    }

    public Color ApplyHuePreview(Color baseColor) {
        Color.RGBToHSV(this, out _, out float s, out float v);
        Color.RGBToHSV(baseColor, out float h, out _, out _);
        Color newColor = Color.HSVToRGB(h, s, v);
        newColor.a = a;
        return newColor;
    }

    public void SetPreset(Color color) {
        r = color.r;
        g = color.g;
        b = color.b;
        a = color.a;
        _rString = _gString = _bString = _aString = null;
        _oldHexString = _hexString = GetHexString();
    }

    public static implicit operator Color(ColorCache cache) {
        Unsafe.SkipInit(out Color color);
        cache.SetColor(ref color);
        return color;
    }

    public static void Setup(ref ColorCache cache, Color color) {
        if(cache == null) cache = new ColorCache(color);
        else cache._originalColor = color;
    }
}