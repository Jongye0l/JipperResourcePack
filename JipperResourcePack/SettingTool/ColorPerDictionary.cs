using System.Collections.Generic;
using JALib.Tools;
using Newtonsoft.Json;
using UnityEngine;

namespace JipperResourcePack.SettingTool;

public class ColorPerDictionary {
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    public ColorCache PerfectColor;
    public ProgressList List = [];
    // ReSharper restore FieldCanBeMadeReadOnly.Global
    [JsonIgnore] private bool _expanded;
    [JsonIgnore] private ProgressColorCache _expandedCache;

    public ColorPerDictionary(IEnumerable<(float, Color)> collection) {
        foreach((float, Color) item in collection) Add(item);
    }

    public ColorPerDictionary(ColorCache perfectColor) {
        PerfectColor = perfectColor;
    }

    public ColorPerDictionary(Color color) : this(new ColorCache(color)) {
    }

    public ColorPerDictionary(IEnumerable<(float, Color)> collection, Color color) {
        foreach((float, Color) item in collection) Add(item);
        PerfectColor = new ColorCache(color);
    }

    public ColorPerDictionary(IEnumerable<(float, Color)> collection, ColorCache color) {
        foreach((float, Color) item in collection) Add(item);
        PerfectColor = color;
    }

    public Color GetColor(float key) {
        if(key < 0) key = 0;
        if(key > 1) key = 1;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if(PerfectColor != null && key == 1) return PerfectColor;
        if(List.Count == 0) return PerfectColor ?? Color.white;
        int index = List.BinarySearch(key);
        if(index == 0) return List[0];
        if(index == List.Count) return List[^1];
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if(List[index].Progress == key) return List[index];
        float start = List[index - 1].Progress;
        float end = List[index].Progress;
        float progress = (key - start) / (end - start);
        return Color.Lerp(List[index - 1], List[index], progress);
    }

    public bool SettingGUI(SettingGUI settingGUI, string text) {
        GUILayout.BeginHorizontal();
        _expanded = GUILayout.Toggle(_expanded, _expanded ? "◢" : "▶", new GUIStyle {
            fixedWidth = 10f,
            normal = new GUIStyleState { textColor = Color.white },
            fontSize = 15,
            margin = new RectOffset(4, 2, 6, 6)
        });
        if(GUILayout.Button(text, GUI.skin.label)) _expanded = !_expanded;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(!_expanded) return false;
        bool changed = false;
        GUILayout.BeginHorizontal();
        GUILayout.Space(18f);
        GUILayout.BeginVertical();
        if(GUILayout.Button(Main.Instance.Localization["Color.Add"])) {
            List.Add(JARandom.Instance.NextFloat(), new Color(JARandom.Instance.NextFloat(), JARandom.Instance.NextFloat(), JARandom.Instance.NextFloat()));
            Main.Instance.SaveSetting();
        }
        foreach(ProgressColorCache cache in List) {
            GUILayout.BeginHorizontal();
            bool expanded = _expandedCache == cache;
            expanded = GUILayout.Toggle(expanded, expanded ? "◢" : "▶", new GUIStyle {
                fixedWidth = 10f,
                normal = new GUIStyleState { textColor = Color.white },
                fontSize = 15,
                margin = new RectOffset(4, 2, 6, 6)
            });
            if(GUILayout.Button(cache.Progress * 100 + "%", GUI.skin.label)) expanded = !expanded;
            if(_expandedCache == cache != expanded) _expandedCache = expanded ? cache : null;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if(!expanded) continue;
            GUILayout.BeginHorizontal();
            GUILayout.Space(18f);
            GUILayout.BeginVertical();
            settingGUI.AddSettingSliderFloat(ref cache.Progress, 0, ref cache.ProgressString, "Percent", 0, 1, () => changed = true);
            bool br = false;
            if(changed) {
                Reload(cache);
                Main.Instance.SaveSetting();
                br = true;
            }
            if(cache.SettingGUI(settingGUI, cache)) {
                changed = true;
                Main.Instance.SaveSetting();
            }
            if(GUILayout.Button(Main.Instance.Localization["Color.Delete"])) {
                List.Remove(cache);
                Main.Instance.SaveSetting();
                br = true;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(12f);
            if(br) break;
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.Space(12f);
        return changed;
    }

    public void Add((float, Color) item) {
        List.Add(item.Item1, item.Item2);
    }

    public void Reload(ProgressColorCache item) {
        List.Remove(item);
        List.Add(item);
    }

    public void Add(ProgressColorCache item) {
        if(item != null) List.Add(item);
    }

    public class ProgressList : List<ProgressColorCache> {
        public ProgressList() {
        }

        public ProgressList(IEnumerable<ProgressColorCache> collection) : base(collection) {
        }

        public ProgressList(int capacity) : base(capacity) {
        }

        public void Add(float progress, Color color) {
            Add(new ProgressColorCache(progress, color));
        }

        public new void Add(ProgressColorCache item) {
            Insert(BinarySearch(item.Progress), item);
        }

        public int BinarySearch(float value) {
            if(Count == 0) return 0;
            int start = 0;
            int end = Count - 1;
            while(start <= end) {
                int i = (start + end) / 2;
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if(this[i].Progress == value) return i;
                if(this[i].Progress < value) start = i + 1;
                else end = i - 1;
            }
            return start;
        }
    }
}