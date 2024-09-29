using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using JALib.Core;
using JALib.Core.Setting;
using JALib.Tools;
using JipperResourcePack.Async;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace JipperResourcePack.Keyviewer;

public class KeyViewer : Feature {

    public static Color Background;
    public static Color Outline;
    public static Color RainColor;
    public GameObject KeyViewerObject;
    public Canvas Canvas;
    public Key[] Keys;
    public Thread KeyinputListener;
    public Key Kps;
    public int lastKps;
    public Key Total;
    public ConcurrentQueue<long> PressTimes;
    public Stopwatch Stopwatch;
    private bool Save;
    private bool KeyShare;

    public int SelectedKey = -1;
    public bool TextChanged;

    public KeyViewer() : base(Main.Instance, nameof(KeyViewer), true, typeof(KeyViewer), typeof(KeyViewerSettings)) {
        Background = new Color(0.5607843f, 0.2352941f, 1, 0.1960784f);
        Outline = new Color(0.5529412f, 0.2431373f, 1);
        RainColor = new Color(0.5137255f, 0.1254902f, 0.858823538f);
    }

    protected override void OnEnable() {
        KeyViewerObject = new GameObject("JipperResourcePack KeyViewer");
        Canvas = KeyViewerObject.AddComponent<Canvas>();
        Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = Canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        Canvas.gameObject.AddComponent<GraphicRaycaster>();
        Keys = new Key[24];
        KeyViewerSettings settings = KeyViewerSettings.Settings;
        switch(settings.KeyViewerStyle) {
            case KeyviewerStyle.Key12:
                Initialize0KeyViewer();
                break;
            case KeyviewerStyle.Key16:
                Initialize1KeyViewer();
                break;
        }
        switch(settings.FootKeyViewerStyle) {
            case FootKeyviewerStyle.Key2:
                Initialize0FootKeyViewer();
                break;
            case FootKeyviewerStyle.Key4:
                Initialize1FootKeyViewer();
                break;
            case FootKeyviewerStyle.Key6:
                Initialize2FootKeyViewer();
                break;
            case FootKeyviewerStyle.Key8:
                Initialize3FootKeyViewer();
                break;
        }
        Object.DontDestroyOnLoad(KeyViewerObject);
        PressTimes = new ConcurrentQueue<long>();
        Stopwatch = Stopwatch.StartNew();
        KeyinputListener = new Thread(ListenKey);
        KeyinputListener.Start();
        Application.wantsToQuit += Application_wantsToQuit;
    }

    private bool Application_wantsToQuit() {
        KeyinputListener.Abort();
        KeyinputListener.Interrupt();
        return true;
    }

    protected override void OnDisable() {
        if(!KeyViewerObject) return;
        Object.Destroy(KeyViewerObject);
        GC.SuppressFinalize(KeyViewerObject);
        KeyViewerObject = null;
        GC.SuppressFinalize(Canvas);
        Canvas = null;
        GC.SuppressFinalize(Keys);
        Keys = null;
        KeyinputListener.Abort();
        KeyinputListener.Interrupt();
        KeyinputListener = null;
        GC.SuppressFinalize(PressTimes);
        PressTimes = null;
        Application.wantsToQuit -= Application_wantsToQuit;
    }

    protected override void OnGUI() {
        SettingGUI settingGUI = Main.SettingGUI;
        JALocalization localization = Main.Instance.Localization;
        KeyViewerSettings settings = KeyViewerSettings.Settings;
        settingGUI.AddSettingToggle(ref KeyShare, localization["keyViewer.keyShare"]);
        settingGUI.AddSettingToggle(ref settings.DownLocation, localization["keyViewer.downLocation"], ResetKeyViewer);
        settingGUI.AddSettingEnum(ref settings.KeyViewerStyle, localization["keyViewer.style"], ChangeKeyViewer);
        settingGUI.AddSettingEnum(ref settings.FootKeyViewerStyle, localization["keyViewer.style"], ResetFootKeyViewer);
        KeyCode[] keyCodes = settings.KeyViewerStyle == KeyviewerStyle.Key12 ? settings.key12 : settings.key16;
        KeyCode[] footKeyCodes = settings.FootKeyViewerStyle switch {
            FootKeyviewerStyle.Key2 => settings.footkey2,
            FootKeyviewerStyle.Key4 => settings.footkey4,
            FootKeyviewerStyle.Key6 => settings.footkey6,
            FootKeyviewerStyle.Key8 => settings.footkey8,
            _ => null
        };
        string[] keyTexts = settings.KeyViewerStyle == KeyviewerStyle.Key12 ? settings.key12Text : settings.key16Text;
        GUILayout.Label(localization["keyViewer.keyChange"]);
        GUILayout.BeginHorizontal();
        for(int i = 0; i < 8; i++) CreateButton(i, false);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        switch(settings.KeyViewerStyle) {
            case KeyviewerStyle.Key12:
                CreateButton(9, false);
                CreateButton(8, false);
                CreateButton(10, false);
                CreateButton(11, false);
                break;
            case KeyviewerStyle.Key16:
                CreateButton(12, false);
                CreateButton(13, false);
                CreateButton(9, false);
                CreateButton(8, false);
                CreateButton(10, false);
                CreateButton(11, false);
                CreateButton(14, false);
                CreateButton(15, false);
                break;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(footKeyCodes != null) {
            GUILayout.BeginHorizontal();
            for(int i = 0; i < footKeyCodes.Length; i++) CreateButton(i++ + 16, false);
            for(int i = 1; i < footKeyCodes.Length; i++) CreateButton(i++ + 16, false);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        if(SelectedKey != -1 && !TextChanged) GUILayout.Label($"<b>{localization["keyViewer.inputKey"]}</b>");
        GUILayout.Label(localization["keyViewer.textChange"]);
        GUILayout.BeginHorizontal();
        for(int i = 0; i < 8; i++) CreateButton(i, true);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        switch(settings.KeyViewerStyle) {
            case KeyviewerStyle.Key12:
                CreateButton(9, true);
                CreateButton(8, true);
                CreateButton(10, true);
                CreateButton(11, true);
                break;
            case KeyviewerStyle.Key16:
                CreateButton(12, true);
                CreateButton(13, true);
                CreateButton(9, true);
                CreateButton(8, true);
                CreateButton(10, true);
                CreateButton(11, true);
                CreateButton(14, true);
                CreateButton(15, true);
                break;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(SelectedKey == -1) return;
        if(TextChanged) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(localization["keyViewer.inputText"]);
            string textArea = GUILayout.TextArea(keyTexts[SelectedKey] ?? KeyToString(keyCodes[SelectedKey]));
            if(keyTexts[SelectedKey] != textArea) {
                Keys[SelectedKey].text.tmp.text = textArea;
                if(textArea == KeyToString(keyCodes[SelectedKey])) textArea = null;
                keyTexts[SelectedKey] = textArea;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if(GUILayout.Button(localization["keyViewer.textReset"])) {
                keyTexts[SelectedKey] = null;
                SelectedKey = -1;
                Main.Instance.SaveSetting();
                return;
            }
            if(GUILayout.Button(localization["keyViewer.textSave"])) {
                SelectedKey = -1;
                Main.Instance.SaveSetting();
                return;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        } else {
            if(!Input.anyKeyDown) return;
            foreach(KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) {
                if(!Input.GetKeyDown(keyCode)) continue;
                if(SelectedKey < 16) keyCodes[SelectedKey] = keyCode;
                else footKeyCodes[SelectedKey - 16] = keyCode;
                Keys[SelectedKey].text.tmp.text = (SelectedKey < 16 ? keyTexts[SelectedKey] : null) ?? KeyToString(keyCode);
                SelectedKey = -1;
                Main.Instance.SaveSetting();
                break;
            }
        }
        return;

        void CreateButton(int i, bool textChanged) {
            if(!GUILayout.Button(Bold(i < 16 ? textChanged ? keyTexts[i] ?? KeyToString(keyCodes[i]) : keyCodes[i].ToString() : footKeyCodes[i - 16].ToString(),
                   i == SelectedKey && textChanged == TextChanged))) return;
            SelectedKey = i;
            TextChanged = textChanged;
        }
    }

    private void ChangeKeyViewer() {
        if(KeyShare) {
            KeyCode[] keyCode1;
            KeyCode[] keyCode2;
            string[] keyText1;
            string[] keyText2;
            switch(KeyViewerSettings.Settings.KeyViewerStyle) {
                case KeyviewerStyle.Key12:
                    keyCode1 = KeyViewerSettings.Settings.key12;
                    keyCode2 = KeyViewerSettings.Settings.key16;
                    keyText1 = KeyViewerSettings.Settings.key12Text;
                    keyText2 = KeyViewerSettings.Settings.key16Text;
                    break;
                case KeyviewerStyle.Key16:
                    keyCode1 = KeyViewerSettings.Settings.key16;
                    keyCode2 = KeyViewerSettings.Settings.key12;
                    keyText1 = KeyViewerSettings.Settings.key16Text;
                    keyText2 = KeyViewerSettings.Settings.key12Text;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            for(int i = 0; i < 12; i++) {
                keyCode1[i] = keyCode2[i];
                keyText1[i] = keyText2[i];
            }
            Mod.SaveSetting();
        }
        ResetKeyViewer();
    }

    private void ResetKeyViewer() {
        SelectedKey = -1;
        for(int i = 0; i < 16; i++) {
            Key key = Keys[i];
            if(key) Object.Destroy(key.gameObject);
        }
        Object.Destroy(Total.gameObject);
        Object.Destroy(Kps.gameObject);
        switch(KeyViewerSettings.Settings.KeyViewerStyle) {
            case KeyviewerStyle.Key12:
                Initialize0KeyViewer();
                break;
            case KeyviewerStyle.Key16:
                Initialize1KeyViewer();
                break;
        }
    }

    private void ResetFootKeyViewer() {
        for(int i = 16; i < 24; i++) {
            Key key = Keys[i];
            if(key) Object.Destroy(key.gameObject);
        }
        switch(KeyViewerSettings.Settings.FootKeyViewerStyle) {
            case FootKeyviewerStyle.Key2:
                Initialize0FootKeyViewer();
                break;
            case FootKeyviewerStyle.Key4:
                Initialize1FootKeyViewer();
                break;
            case FootKeyviewerStyle.Key6:
                Initialize2FootKeyViewer();
                break;
            case FootKeyviewerStyle.Key8:
                Initialize3FootKeyViewer();
                break;
        }
    }

    private static string Bold(string text, bool bold) {
        return bold ? $"<b>{text}</b>" : text;
    }

    protected override void OnHideGUI() {
        if(SelectedKey == -1) return;
        Main.Instance.SaveSetting();
        SelectedKey = -1;
    }

    private void ListenKey() {
        try {
            KeyViewerSettings settings = KeyViewerSettings.Settings;
            bool[] keyState = new bool[24];
            int repeat = 0;
            while(KeyinputListener.IsAlive && Enabled) {
                long elapsedMilliseconds = Stopwatch.ElapsedMilliseconds;
                KeyCode[] keyCodes = settings.KeyViewerStyle == 0 ? settings.key12 : settings.key16;
                for(int i = 0; i < keyCodes.Length; i++) {
                    bool current = Input.GetKey(keyCodes[i]);
                    Key key = Keys[i];
                    if(!key) continue;
                    for(int j = 0; j < key.rainList.Count; j++) {
                        RawRain rain = key.rainList[j];
                        if(rain.UpdateLocation(elapsedMilliseconds, current && keyState[i] && j == key.rainList.Count - 1)) continue;
                        key.rainList.Remove(rain);
                        rain.removed = true;
                        j--;
                    }
                    if(current == keyState[i]) continue;
                    keyState[i] = current;
                    UpdateKey(i, current);
                    if(!current) continue;
                    key.value.text = (++settings.Count[i]).ToString();
                    Total.value.text = (++settings.TotalCount).ToString();
                    PressTimes.Enqueue(elapsedMilliseconds);
                    RawRain rawRain = new(key.rain.transform, elapsedMilliseconds, key.isGreenColor);
                    key.rawRainQueue.Enqueue(rawRain);
                    key.rainList.Add(rawRain);
                    Save = true;
                }
                keyCodes = settings.FootKeyViewerStyle switch {
                    FootKeyviewerStyle.Key2 => settings.footkey2,
                    FootKeyviewerStyle.Key4 => settings.footkey4,
                    FootKeyviewerStyle.Key6 => settings.footkey6,
                    FootKeyviewerStyle.Key8 => settings.footkey8,
                    _ => []
                };
                for(int i = 0; i < keyCodes.Length; i++) {
                    bool current = Input.GetKey(keyCodes[i]);
                    Key key = Keys[i + 16];
                    if(!key || current == keyState[i + 16]) continue;
                    keyState[i + 16] = current;
                    UpdateKey(i + 16, current);
                    if(!current) continue;
                    PressTimes.Enqueue(elapsedMilliseconds);
                    settings.Count[i + 16]++;
                    Total.value.text = (++settings.TotalCount).ToString();
                    Save = true;
                }
                while(PressTimes.TryPeek(out long result)) {
                    if(elapsedMilliseconds - result > 1000)
                        PressTimes.TryDequeue(out long _);
                    else break;
                }
                if(lastKps == PressTimes.Count) continue;
                lastKps = PressTimes.Count;
                Kps.value.text = lastKps.ToString();
                if(++repeat < 100 || !Save || !Enabled) continue;
                Main.Instance.SaveSetting();
                Save = false;
                repeat = 0;
            }
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    private void UpdateKey(int i, bool enabled) {
        Key key = Keys[i];
        key.background.color = enabled ? Color.white : Background;
        key.outline.color = enabled ? Color.white : Outline;
        key.text.color = enabled ? Color.black : Color.white;
        if(key.value) key.value.color = key.text.color;
    }

    private void Initialize0KeyViewer() {
        int remove = KeyViewerSettings.Settings.DownLocation ? 200 : 0;
        for(int i = 0; i < 8; i++) Keys[i] = CreateKey(i, 54 * i, 279 - remove, 50, 0);
        Keys[8] = CreateKey(8, 81 + 54, 225 - remove, 77, 2);
        Keys[9] = CreateKey(9, 81, 225 - remove, 50, 2);
        Keys[10] = CreateKey(10, 54 * 4, 225 - remove, 77, 2);
        Keys[11] = CreateKey(11, 54 * 4 + 81, 225 - remove, 50, 2);
        Kps = CreateKey(-1, 0, 225 - remove, 77, -1);
        Total = CreateKey(-2, 81 + 54 * 5, 225 - remove, 77, -1);
    }

    private void Initialize1KeyViewer() {
        int remove = KeyViewerSettings.Settings.DownLocation ? 200 : 0;
        for(int i = 0; i < 8; i++) Keys[i] = CreateKey(i, 54 * i, 320 - remove, 50, 0);
        Keys[8] = CreateKey(8, 54 * 3, 266 - remove, 50, 1);
        Keys[8].rain = Keys[3].rain;
        Keys[9] = CreateKey(9, 54 * 2, 266 - remove, 50, 1);
        Keys[9].rain = Keys[2].rain;
        Keys[10] = CreateKey(10, 54 * 4, 266 - remove, 50, 1);
        Keys[10].rain = Keys[4].rain;
        Keys[11] = CreateKey(11, 54 * 5, 266 - remove, 50, 1);
        Keys[11].rain = Keys[5].rain;
        Keys[12] = CreateKey(12, 0, 266 - remove, 50, 1);
        Keys[12].rain = Keys[0].rain;
        Keys[13] = CreateKey(13, 54, 266 - remove, 50, 1);
        Keys[13].rain = Keys[1].rain;
        Keys[14] = CreateKey(14, 54 * 6, 266 - remove, 50, 1);
        Keys[14].rain = Keys[6].rain;
        Keys[15] = CreateKey(15, 54 * 7, 266 - remove, 50, 1);
        Keys[15].rain = Keys[7].rain;
        Kps = CreateKey(-1, 0, 220 - remove, 212, -1, true);
        Total = CreateKey(-2, 216, 220 - remove, 212, -1, true);
    }

    private void Initialize0FootKeyViewer() {
        Keys[16] = CreateKey(16, 432, 15, 30, -1, true, false);
        Keys[17] = CreateKey(17, 466, 15, 30, -1, true, false);
    }

    private void Initialize1FootKeyViewer() {
        Keys[16] = CreateKey(16, 432, 15, 30, -1, true, false);
        Keys[17] = CreateKey(17, 500, 15, 30, -1, true, false);
        Keys[18] = CreateKey(18, 466, 15, 30, -1, true, false);
        Keys[19] = CreateKey(19, 534, 15, 30, -1, true, false);
    }

    private void Initialize2FootKeyViewer() {
        Keys[16] = CreateKey(16, 432, 15, 30, -1, true, false);
        Keys[17] = CreateKey(17, 534, 15, 30, -1, true, false);
        Keys[18] = CreateKey(18, 466, 15, 30, -1, true, false);
        Keys[19] = CreateKey(19, 568, 15, 30, -1, true, false);
        Keys[20] = CreateKey(20, 500, 15, 30, -1, true, false);
        Keys[21] = CreateKey(21, 602, 15, 30, -1, true, false);
    }

    private void Initialize3FootKeyViewer() {
        Keys[16] = CreateKey(16, 432, 15, 30, -1, true, false);
        Keys[17] = CreateKey(17, 568, 15, 30, -1, true, false);
        Keys[18] = CreateKey(18, 466, 15, 30, -1, true, false);
        Keys[19] = CreateKey(19, 602, 15, 30, -1, true, false);
        Keys[20] = CreateKey(20, 500, 15, 30, -1, true, false);
        Keys[21] = CreateKey(21, 636, 15, 30, -1, true, false);
        Keys[22] = CreateKey(22, 534, 15, 30, -1, true, false);
        Keys[23] = CreateKey(23, 670, 15, 30, -1, true, false);
    }

    private Key CreateKey(int i, float x, float y, float sizeX, int raining, bool slim = false, bool count = true) {
        GameObject obj = new("Key " + i);
        RectTransform transform = obj.AddComponent<RectTransform>();
        transform.SetParent(KeyViewerObject.transform);
        transform.sizeDelta = new Vector2(sizeX, slim ? 30 : 50);
        transform.anchorMin = transform.anchorMax = Vector2.zero;
        transform.pivot = new Vector2(0, 0.5f);
        transform.anchoredPosition = new Vector2(x, y);
        transform.localScale = Vector3.one;
        Key key = obj.AddComponent<Key>();
        GameObject gameObject = new("Background");
        transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(obj.transform);
        transform.anchorMin = transform.anchorMax = transform.pivot = Vector2.zero;
        transform.anchoredPosition = Vector2.zero;
        transform.sizeDelta = new Vector2(sizeX, slim ? 30 : 50);
        transform.localScale = Vector3.one;
        Image image = gameObject.AddComponent<Image>();
        image.color = Background;
        image.sprite = BundleLoader.KeyBackground;
        key.background = gameObject.AddComponent<AsyncImage>();
        gameObject = new GameObject("Outline");
        transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(obj.transform);
        transform.anchorMin = transform.anchorMax = transform.pivot = Vector2.zero;
        transform.anchoredPosition = Vector2.zero;
        transform.sizeDelta = new Vector2(sizeX, slim ? 30 : 50);
        transform.localScale = Vector3.one;
        image = gameObject.AddComponent<Image>();
        image.color = Outline;
        image.sprite = BundleLoader.KeyOutline;
        key.outline = gameObject.AddComponent<AsyncImage>();
        gameObject = new GameObject("KeyText");
        transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(obj.transform);
        if(slim) {
            transform.sizeDelta = new Vector2(sizeX / 2, 30);
            transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(0, 0.5f);
            transform.anchoredPosition = new Vector2(count ? 10 : 7.5f, 0);
        } else {
            transform.sizeDelta = new Vector2(sizeX - 4, 32);
            transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(0.5f, 1);
            transform.anchoredPosition = new Vector2(0, 2);
        }
        transform.localScale = Vector3.one;
        TextMeshProUGUI text = gameObject.AddComponent<TextMeshProUGUI>();
        text.font = BundleLoader.FontAsset;
        text.enableAutoSizing = true;
        text.fontSizeMin = 0;
        text.fontSizeMax = 20;
        text.alignment = slim ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
        key.text = gameObject.AddComponent<AsyncText>();
        if(count) {
            gameObject = new GameObject("CountText");
            transform = gameObject.AddComponent<RectTransform>();
            transform.SetParent(obj.transform);
            if(slim) {
                transform.sizeDelta = new Vector2(sizeX / 2, 30);
                transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(1, 0.5f);
                transform.anchoredPosition = new Vector2(-10, 0);
            } else {
                transform.sizeDelta = new Vector2(sizeX - 4, 16);
                transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(0.5f, 0);
                transform.anchoredPosition = new Vector2(0, 2);
            }
            transform.localScale = Vector3.one;
            text = gameObject.AddComponent<TextMeshProUGUI>();
            text.font = BundleLoader.FontAsset;
            text.enableAutoSizing = true;
            text.fontSizeMin = 0;
            text.fontSizeMax = 20;
            text.alignment = slim ? TextAlignmentOptions.Right : TextAlignmentOptions.Top;
            key.value = gameObject.AddComponent<AsyncText>();
        }
        UpdateKeyText(key, i);
        key.isGreenColor = raining != 0;
        if(raining != 0 && raining != 2) return key;
        key.rain = new GameObject("RainLine");
        transform = key.rain.AddComponent<RectTransform>();
        transform.SetParent(obj.transform);
        transform.sizeDelta = new Vector2(sizeX, 275);
        transform.anchorMin = transform.anchorMax = transform.pivot = Vector2.zero;
        transform.anchoredPosition = new Vector2(0, raining == 0 ? -223 : -169);
        transform.localScale = Vector3.one;
        return key;
    }

    public void UpdateKeyText(Key key, int i) {
        switch(i) {
            case -1:
                key.text.tmp.text = "KPS";
                key.value.tmp.text = "0";
                return;
            case -2:
                key.text.tmp.text = "Total";
                key.value.tmp.text = KeyViewerSettings.Settings.TotalCount.ToString();
                return;
            default:
                KeyCode[] keyCodes;
                KeyViewerSettings Settings = KeyViewerSettings.Settings;
                if(i < 16) {
                    keyCodes = Settings.KeyViewerStyle == 0 ? Settings.key12 : Settings.key16;
                    string[] keyText = Settings.KeyViewerStyle == 0 ? Settings.key12Text : Settings.key16Text;
                    key.text.tmp.text = keyText[i] ?? KeyToString(keyCodes[i]);
                    key.value.tmp.text = Settings.Count[i].ToString();
                } else {
                    keyCodes = Settings.FootKeyViewerStyle switch {
                        FootKeyviewerStyle.Key2 => Settings.footkey2,
                        FootKeyviewerStyle.Key6 => Settings.footkey6,
                        FootKeyviewerStyle.Key8 => Settings.footkey8,
                        _ => Settings.footkey4
                    };
                    key.text.tmp.text = KeyToString(keyCodes[i - 16]);
                }
                break;
        }
    }

    public static string KeyToString(KeyCode keyCode) {
        string keyString = keyCode.ToString();
        if(keyString.StartsWith("Alpha")) keyString = keyString[5..];
        if(keyString.StartsWith("Keypad")) keyString = keyString[6..];
        if(keyString.StartsWith("Left")) keyString = 'L' + keyString[4..];
        if(keyString.StartsWith("Right")) keyString = 'R' + keyString[5..];
        if(keyString.EndsWith("Shift")) keyString = keyString[..^5] + "⇧";
        if(keyString.EndsWith("Control")) keyString = keyString[..^7] + "Ctrl";
        if(keyString.StartsWith("Mouse")) keyString = "M" + keyString[5..];
        return keyString switch {
            "Plus" => "+",
            "Minus" => "-",
            "Multiply" => "*",
            "Divide" => "/",
            "Enter" => "↵",
            "Equals" => "=",
            "Period" => ".",
            "Return" => "↵",
            "None" => " ",
            "Tab" => "⇥",
            "Backslash" => "\\",
            "Backspace" => "Back",
            "Slash" => "/",
            "LBracket" => "[",
            "RBracket" => "]",
            "Semicolon" => ";",
            "Comma" => ",",
            "Quote" => "'",
            "UpArrow" => "↑",
            "DownArrow" => "↓",
            "LeftArrow" => "←",
            "RightArrow" => "→",
            "Space" => "␣",
            "BackQuote" => "`",
            "PageDown" => "Pg↓",
            "PageUp" => "Pg↑",
            "CapsLock" => "⇪",
            "Insert" => "Ins",
            _ => keyString
        };
    }

    public class KeyViewerSettings : JASetting {
        public static KeyViewerSettings Settings;
        public KeyviewerStyle KeyViewerStyle = KeyviewerStyle.Key16;
        public FootKeyviewerStyle FootKeyViewerStyle = FootKeyviewerStyle.Key4;
        public KeyCode[] key12 = [
            KeyCode.Tab, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.E, KeyCode.P, KeyCode.Equals, KeyCode.Backspace, KeyCode.Backslash,
            KeyCode.Space, KeyCode.C, KeyCode.Comma, KeyCode.Period
        ];
        public string[] key12Text = new string[12];
        public KeyCode[] key16 = [
            KeyCode.Tab, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.E, KeyCode.P, KeyCode.Equals, KeyCode.Backspace, KeyCode.Backslash,
            KeyCode.Space, KeyCode.C, KeyCode.Comma, KeyCode.Period, KeyCode.CapsLock, KeyCode.LeftShift, KeyCode.Return, KeyCode.H
        ];
        public string[] key16Text = new string[16];
        public KeyCode[] footkey2 = [KeyCode.F2, KeyCode.F7];
        public KeyCode[] footkey4 = [KeyCode.F2, KeyCode.F7, KeyCode.F3, KeyCode.F6];
        public KeyCode[] footkey6 = [KeyCode.F1, KeyCode.F8, KeyCode.F2, KeyCode.F7, KeyCode.F3, KeyCode.F6];
        public KeyCode[] footkey8 = [KeyCode.F1, KeyCode.F8, KeyCode.F2, KeyCode.F7, KeyCode.F3, KeyCode.F6, KeyCode.F4, KeyCode.F5];
        public int[] Count = new int[24];
        public int TotalCount;
        public bool DownLocation;

        public KeyViewerSettings(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
            Settings = this;
        }
    }
}