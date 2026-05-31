using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Tools;
using JipperResourcePack.Async;
using JipperResourcePack.KeyViewerContents.OtherModApi;
using JipperResourcePack.SettingTool;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using Application = UnityEngine.Application;
using Object = UnityEngine.Object;
using ThreadPriority = System.Threading.ThreadPriority;

namespace JipperResourcePack.KeyViewerContents;

public partial class KeyViewer : Feature {
    public const int HandOutIndex = 20;
    public const int FootOutIndex = 36;
    public const int GhostOutIndex = 56;

    public static KeyViewer Instance;
    public static KeyViewerSetting Settings;
    public static readonly Color Background = new(0.5607843f, 0.2352941f, 1, 0.1960784f);
    public static readonly Color BackgroundClicked = Color.white;
    public static readonly Color Outline = new(0.5529412f, 0.2431373f, 1);
    public static readonly Color OutlineClicked = Color.white;
    public static readonly Color Text = Color.white;
    public static readonly Color TextClicked = Color.black;
    public static readonly Color RainColor = new(0.5137255f, 0.1254902f, 0.858823538f);
    public static readonly Color RainColor2 = Color.white;
    public static readonly Color RainColor3 = Color.magenta;
    public static readonly byte[] BackSequence10 = [8, 9];
    public static readonly byte[] BackSequence12 = [9, 8, 10, 11];
    public static readonly byte[] BackSequence16 = [12, 13, 9, 8, 10, 11, 14, 15];
    public static readonly byte[] BackSequence20 = [12, 13, 9, 8, 10, 11, 14, 15, 17, 16, 18, 19];
    public static RainManager RainManager;
    public GameObject KeyViewerObject;
    public GameObject KeyViewerSizeObject;
    public Key[] Keys;
    public Thread KeyInputListener;
    public Key Kps;
    public Key Total;
    public static Stopwatch Stopwatch;
    private bool _save;
    private bool _keyShare;
    private bool _keyChangeExpanded;
    private bool _ghostRainChangeExpanded;
    private bool _textChangeExpanded;
    private bool[] _colorExpanded;
    private KeyviewerStyle _currentKeyViewerStyle;
    private bool[] _keyPressed;
    private bool _confirmResetCount;

    private int _selectedKey = -1;
    private int _winAPICool;
    private int _currentKeyMaxY = 120;
    private int _changeState;
    private string _rainSizeString;
    private string _rainHeightString;
    private string _sizeString;
    private string _yLocationString;

    public KeyViewer() : base(Main.Instance, nameof(KeyViewer), settingType: typeof(KeyViewerSetting)) {
        Instance = this;
        _currentKeyViewerStyle = Settings.KeyViewerStyle;
        if(ADOBase.platform != Platform.Windows) return;
        Patcher.AddPatch(Load);
        AdofaiTweaksAPI.Setup();
        KeyboardChatterBlockerAPI.Setup();
    }

    protected override void OnEnable() {
        KeyCountData.Load();
        KeyViewerObject = new GameObject("JipperResourcePack KeyViewer");
        KeyViewerObject.AddComponent<KeyViewerUpdater>();
        RainManager = KeyViewerObject.AddComponent<RainManager>();
        if(!Settings.useRain) RainManager.enabled = false;
        Canvas canvas = KeyViewerObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2;
        CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        KeyViewerSizeObject = new GameObject("SizeObject");
        RectTransform rectTransform = KeyViewerSizeObject.AddComponent<RectTransform>();
        rectTransform.SetParent(KeyViewerObject.transform);
        rectTransform.localScale = new Vector3(Settings.Size, Settings.Size, 1);
        rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.offsetMin = rectTransform.offsetMax = Vector2.zero;
        Keys = new Key[FootOutIndex];
        InitializeKeyViewer();
        InitializeFootKeyViewer();
        Object.DontDestroyOnLoad(KeyViewerObject);
        PressTimes = new Queue<long>();
        Stopwatch = Stopwatch.StartNew();
        KeyInputListener = new Thread(ListenKey) {
            Name = "JipperResourcePack KeyViewer Listener Thread",
            Priority = ThreadPriority.AboveNormal
        };
        KeyInputListener.Start();
        Application.quitting += ApplicationOnquitting;
        UpdateKeyLimit();
    }
    private void ApplicationOnquitting() {
        KeyInputListener.Abort();
        KeyInputListener.Interrupt();
    }

    protected override void OnDisable() {
        if(!KeyViewerObject) return;
        Object.Destroy(KeyViewerObject);
        KeyViewerObject = null;
        KeyViewerSizeObject = null;
        Keys = null;
        KeyInputListener.Abort();
        KeyInputListener.Interrupt();
        KeyInputListener = null;
        PressTimes = null;
        Application.quitting -= ApplicationOnquitting;
    }

    protected override void OnGUI() {
        SettingGUI settingGUI = Main.SettingGUI;
        JALocalization localization = Main.Instance.Localization;
        KeyViewerSetting settings = Settings;
        settingGUI.AddSettingToggle(ref _keyShare, localization["keyViewer.keyShare"]);
        GUILayout.BeginHorizontal();
        settingGUI.AddSettingSliderFloat(ref settings.YLocation, 200, ref _yLocationString, localization["keyViewer.yLocation"], 0, _currentKeyMaxY, ResetKeyViewer);
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if(settings.YLocation != 200 && GUILayout.Button(localization["keyViewer.resetYLocation"])) {
            settings.YLocation = 200;
            _yLocationString = null;
            Main.Instance.SaveSetting();
            ResetKeyViewer();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        settingGUI.AddSettingToggle(ref settings.useRain, localization["keyViewer.useRain"], CheckResetRain);
        if(settings.useRain) settingGUI.AddSettingToggle(ref settings.useGhostRain, localization["keyViewer.useGhostRain"]);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        settingGUI.AddSettingSliderFloat(ref settings.rainSpeed, 100, ref _rainSizeString, localization["keyViewer.rainSpeed"], 1, 800);
        settingGUI.AddSettingSliderFloat(ref settings.rainHeight, 200, ref _rainHeightString, localization["keyViewer.rainHeight"], 1, 1000);
        if(ADOBase.platform == Platform.Windows && (AdofaiTweaksAPI.IsExist || KeyboardChatterBlockerAPI.IsExist))
            settingGUI.AddSettingToggle(ref settings.AutoSetupKeyLimit, localization["keyViewer.autoSetupKeyLimit"], UpdateKeyLimit);
        settingGUI.AddSettingEnum(ref settings.KeyViewerStyle, localization["keyViewer.style"], ChangeKeyViewer);
        settingGUI.AddSettingEnum(ref settings.FootKeyViewerStyle, localization["keyViewer.style"], ResetFootKeyViewer);
        settingGUI.AddSettingSliderFloat(ref settings.Size, 1, ref _sizeString, localization["size"], 0, 2, () => {
            KeyViewerSizeObject.transform.localScale = new Vector3(settings.Size, settings.Size, 1);
        });
        KeyCode[] keyCodes = GetKeyCode();
        KeyCode[] footKeyCodes = GetFootKeyCode();
        KeyCode[] ghostKeyCodes = GetGhostKeyCode();
        string[] keyTexts = GetKeyText();
        GUILayout.Space(12f);
        GUIStyle toggleStyle = new() {
            fixedWidth = 10f,
            normal = new GUIStyleState { textColor = Color.white },
            fontSize = 15,
            margin = new RectOffset(4, 2, 6, 6)
        };
        GUILayout.BeginHorizontal();
        _keyChangeExpanded = GUILayout.Toggle(_keyChangeExpanded, _keyChangeExpanded ? "◢" : "▶", toggleStyle);
        if(GUILayout.Button(localization["keyViewer.keyChange"], GUI.skin.label)) _keyChangeExpanded = !_keyChangeExpanded;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(_keyChangeExpanded) {
            GUILayout.BeginHorizontal();
            GUILayout.Space(18f);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            for(int i = 0; i < 8; i++) CreateButton(i, false);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            byte[] backSequence = GetBackSequence();
            for(int i = 0; i < backSequence.Length && i < 8; i++) CreateButton(backSequence[i], false);
            if(backSequence.Length > 8) {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                for(int i = 8; i < backSequence.Length; i++) CreateButton(backSequence[i], false);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if(footKeyCodes != null) {
                GUILayout.BeginHorizontal();
                for(int i = 0; i < footKeyCodes.Length; i++) CreateButton(i++ + HandOutIndex, false);
                for(int i = 1; i < footKeyCodes.Length; i++) CreateButton(i++ + HandOutIndex, false);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            if(_selectedKey != -1 && _changeState == 0) GUILayout.Label($"<b>{localization["keyViewer.inputKey"]}</b>");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(12f);
        }

        if(settings.useRain && settings.useGhostRain) {
            GUILayout.BeginHorizontal();
            _ghostRainChangeExpanded = GUILayout.Toggle(_ghostRainChangeExpanded, _ghostRainChangeExpanded ? "◢" : "▶", toggleStyle);
            if(GUILayout.Button(localization["keyViewer.ghostRainKeyChange"], GUI.skin.label)) _ghostRainChangeExpanded = !_ghostRainChangeExpanded;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if(_ghostRainChangeExpanded) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(18f);
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                for(int i = 0; i < 8; i++) CreateGhostButton(i);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                byte[] backSequence = GetBackSequence();
                for(int i = 0; i < backSequence.Length && i < 8; i++) CreateGhostButton(backSequence[i]);
                if(backSequence.Length > 8) {
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    for(int i = 8; i < backSequence.Length; i++) CreateGhostButton(backSequence[i]);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if(_selectedKey != -1 && _changeState == 2) GUILayout.Label($"<b>{localization["keyViewer.inputKey"]}</b>");
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Space(12f);
            }
        }
        
        GUILayout.BeginHorizontal();
        _textChangeExpanded = GUILayout.Toggle(_textChangeExpanded, _textChangeExpanded ? "◢" : "▶", toggleStyle);
        if(GUILayout.Button(localization["keyViewer.textChange"], GUI.skin.label)) _textChangeExpanded = !_textChangeExpanded;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(_textChangeExpanded) {
            GUILayout.BeginHorizontal();
            GUILayout.Space(18f);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            for(int i = 0; i < 8; i++) CreateButton(i, true);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            byte[] backSequence = GetBackSequence();
            for(int i = 0; i < backSequence.Length && i < 8; i++) CreateButton(backSequence[i], true);
            if(backSequence.Length > 8) {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                for(int i = 8; i < backSequence.Length; i++) CreateButton(backSequence[i], true);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if(_selectedKey != -1 && _changeState == 1) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(localization["keyViewer.inputText"]);
                string textArea = GUILayout.TextArea(keyTexts[_selectedKey] ?? KeyToString(keyCodes[_selectedKey]));
                if(keyTexts[_selectedKey] != textArea) {
                    Keys[_selectedKey].Text.SetTextForce(textArea);
                    if(textArea == KeyToString(keyCodes[_selectedKey])) textArea = null;
                    keyTexts[_selectedKey] = textArea;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if(GUILayout.Button(localization["keyViewer.textReset"])) {
                    keyTexts[_selectedKey] = null;
                    _selectedKey = -1;
                    Main.Instance.SaveSetting();
                }
                if(GUILayout.Button(localization["keyViewer.textSave"])) {
                    _selectedKey = -1;
                    Main.Instance.SaveSetting();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(12f);
        }
        GUILayout.BeginHorizontal();
        bool a = GUILayout.Toggle(_colorExpanded != null, _colorExpanded != null ? "◢" : "▶", toggleStyle);
        if(_colorExpanded != null != a) _colorExpanded = a ? new bool[9] : null;
        if(GUILayout.Button(localization["keyViewer.color"], GUI.skin.label)) _colorExpanded = _colorExpanded == null ? new bool[8] : null;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(_colorExpanded != null) {
            GUILayout.BeginHorizontal();
            GUILayout.Space(18f);
            GUILayout.BeginVertical();
            string[] names = [
                "Background",
                "BackgroundClicked",
                "Outline",
                "OutlineClicked",
                "Text",
                "TextClicked",
                "RainColor",
                "RainColor2",
                "RainColor3"
            ];
            for(int i = 0; i < 9; i++) {
                if(i == 8 && Settings.KeyViewerStyle != KeyviewerStyle.Key20) continue;
                GUILayout.BeginHorizontal();
                _colorExpanded[i] = GUILayout.Toggle(_colorExpanded[i], _colorExpanded[i] ? "◢" : "▶", toggleStyle);
                if(GUILayout.Button(localization["keyViewer.color." + char.ToLower(names[i][0]) + names[i][1..]], GUI.skin.label)) _colorExpanded[i] = !_colorExpanded[i];
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if(!_colorExpanded[i]) continue;
                GUILayout.BeginHorizontal();
                GUILayout.Space(18f);
                GUILayout.BeginVertical();
                if(settings.GetValue<ColorCache>(names[i]).SettingGUI(settingGUI, typeof(KeyViewer).GetValue<Color>(names[i]))) {
                    for(int i2 = 0; i2 < keyCodes.Length; i2++) UpdateKey(i2, CheckKey(keyCodes[i2]));
                    if(footKeyCodes != null) for(int i2 = 0; i2 < footKeyCodes.Length; i2++) UpdateKey(i2 + HandOutIndex, CheckKey(footKeyCodes[i2]));
                    Kps.Background.Image.color = Total.Background.Image.color = settings.Background;
                    Kps.Outline.Image.color = Total.Outline.Image.color = settings.Outline;
                    Kps.Text.TMP.color = Kps.Value.TMP.color = Total.Text.TMP.color = Total.Value.TMP.color = settings.Text;
                    Main.Instance.SaveSetting();
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Space(12f);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(12f);
        }
        if(GUILayout.Button(localization["keyViewer.resetCount"])) _confirmResetCount = true;
        if(_confirmResetCount) {
            GUILayout.Label("<color=red>" + localization["keyViewer.resetCountConfirmText"] + "</color>");
            if(GUILayout.Button(localization["keyViewer.resetCountConfirm"])) {
                _confirmResetCount = false;
                Total.Value.TMP.text = "0";
                foreach(Key key in Keys) key?.Value.SetTextForce("0");
                for(int i = 0; i < KeyCountData.Instance.Count.Length; i++) KeyCountData.Instance.Count[i] = 0;
                KeyCountData.Instance.TotalCount = 0;
                Main.Instance.SaveSetting();
            }
            if(GUILayout.Button(localization["keyViewer.resetCountCancel"])) _confirmResetCount = false;
        }
        if(_selectedKey == -1 || _changeState == 1 || !Application.isFocused) return;
        if(Input.anyKeyDown) {
            foreach(KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) {
                if(!Input.GetKeyDown(keyCode)) continue;
                SetupKey(keyCode);
                break;
            }
        } else {
            if(ADOBase.platform == Platform.Windows) {
                for(int i = 0; i < 256; i++) {
                    if((GetAsyncKeyState(i) & 0x8000) != 0 == _keyPressed[i]) continue;
                    if(_keyPressed[i]) {
                        _keyPressed[i] = false;
                        _winAPICool = 0;
                        continue;
                    }
                    if(_winAPICool++ < 6) break;
                    KeyCode keyCode = (KeyCode) i + 0x1000;
                    SetupKey(keyCode);
                    break;
                }
            }
        }
        return;

        void CreateButton(int i, bool textChanged) {
            if(!GUILayout.Button(Bold(i < HandOutIndex ? textChanged ? keyTexts[i] ?? KeyToString(keyCodes[i]) : ToString(keyCodes[i]) : ToString(footKeyCodes![i - HandOutIndex]),
                   i == _selectedKey && _changeState == (textChanged ? 1 : 0)))) return;
            _selectedKey = i;
            _changeState = textChanged ? 1 : 0;
            if(textChanged) return;
            _winAPICool = 0;
            _keyPressed = new bool[256];
            for(int i2 = 0; i2 < 256; i2++) _keyPressed[i2] = (GetAsyncKeyState(i2) & 0x8000) != 0;
        }

        void CreateGhostButton(int i) {
            if(!GUILayout.Button(Bold(ToString(ghostKeyCodes[i]), i == _selectedKey && _changeState == 2))) return;
            if(ghostKeyCodes[i] != KeyCode.None) {
                ghostKeyCodes[i] = KeyCode.None;
                Main.Instance.SaveSetting();
                return;
            }
            _selectedKey = i;
            _changeState = 2;
            _winAPICool = 0;
            _keyPressed = new bool[256];
            for(int i2 = 0; i2 < 256; i2++) _keyPressed[i2] = (GetAsyncKeyState(i2) & 0x8000) != 0;
        }

        void SetupKey(KeyCode keyCode) {
            if(_changeState == 0) {
                if(_selectedKey < HandOutIndex) keyCodes[_selectedKey] = keyCode;
                else footKeyCodes[_selectedKey - HandOutIndex] = keyCode;
                Keys[_selectedKey].Text.SetTextForce((_selectedKey < HandOutIndex ? keyTexts[_selectedKey] : null) ?? KeyToString(keyCode));
                UpdateKeyLimit();
            } else ghostKeyCodes[_selectedKey] = keyCode;
            
            _selectedKey = -1;
            _winAPICool = 0;
            _keyPressed = null;
            Main.Instance.SaveSetting();
        }
    }

    private static KeyCode[] GetKeyCode() {
        return Settings.KeyViewerStyle switch {
            KeyviewerStyle.Key12 => Settings.key12,
            KeyviewerStyle.Key16 => Settings.key16,
            KeyviewerStyle.Key20 => Settings.key20,
            KeyviewerStyle.Key10 => Settings.key10,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static KeyCode[] GetGhostKeyCode() {
        return Settings.KeyViewerStyle switch {
            KeyviewerStyle.Key12 => Settings.GhostKey12,
            KeyviewerStyle.Key16 => Settings.GhostKey16,
            KeyviewerStyle.Key20 => Settings.GhostKey20,
            KeyviewerStyle.Key10 => Settings.GhostKey10,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static KeyCode[] GetFootKeyCode() {
        return Settings.FootKeyViewerStyle switch {
            FootKeyviewerStyle.Key2 => Settings.footkey2,
            FootKeyviewerStyle.Key4 => Settings.footkey4,
            FootKeyviewerStyle.Key6 => Settings.footkey6,
            FootKeyviewerStyle.Key8 => Settings.footkey8,
            FootKeyviewerStyle.Key16 => Settings.footkey16,
            _ => []
        };
    }

    private static string[] GetKeyText() {
        return Settings.KeyViewerStyle switch {
            KeyviewerStyle.Key12 => Settings.key12Text,
            KeyviewerStyle.Key16 => Settings.key16Text,
            KeyviewerStyle.Key20 => Settings.key20Text,
            KeyviewerStyle.Key10 => Settings.key10Text,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static byte[] GetBackSequence() {
        return Settings.KeyViewerStyle switch {
            KeyviewerStyle.Key12 => BackSequence12,
            KeyviewerStyle.Key16 => BackSequence16,
            KeyviewerStyle.Key20 => BackSequence20,
            KeyviewerStyle.Key10 => BackSequence10,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static string ToString(KeyCode keyCode) {
        if((int) keyCode < 0x1000) return keyCode.ToString();
        #region Custom KeyCode To String

        int code = (int) keyCode - 0x1000;
        switch(ADOBase.platform) {
            case Platform.Windows:
                return code switch {
                    >= 0x7C and <= 0x87 => "F" + (code - 0x6F),
                    >= 0x92 and <= 0x96 or 0xE1 or 0xE3 or 0xE4 or >= 0xE9 and <= 0xF5 => "OEM" + code,
                    _ => code switch {
                        0x15 => "RightAlt",
                        0x16 => "IME ON",
                        0x17 => "Junja",
                        0x18 => "Final",
                        0x19 => "RightControl",
                        0x1A => "IME OFF",
                        0x1C => "Convert",
                        0x1D => "NonConvert",
                        0x1E => "Accept",
                        0x1F => "ModeChange",
                        0xA6 => "BrowserBack",
                        0xA7 => "BrowserForward",
                        0xA8 => "BrowserRefresh",
                        0xA9 => "BrowserStop",
                        0xAA => "BrowserSearch",
                        0xAB => "BrowserFavorites",
                        0xAC => "BrowserHome",
                        0xAD => "VolumeMute",
                        0xAE => "VolumeDown",
                        0xAF => "VolumeUp",
                        0xB0 => "MediaNextTrack",
                        0xB1 => "MediaPreviousTrack",
                        0xB2 => "MediaStop",
                        0xB3 => "MediaPlayPause",
                        0xB4 => "LaunchMail",
                        0xB5 => "SelectMedia",
                        0xB6 => "LaunchApplication1",
                        0xB7 => "LaunchApplication2",
                        0xC1 => @"-\ろ",
                        0xDF => "OME8",
                        0xE2 => @"\\|",
                        0xE5 => "Process",
                        0xE7 => "Packet",
                        0xEB => "変換",
                        0xF6 => "Attn",
                        0xF7 => "CrSel",
                        0xF8 => "ExSel",
                        0xF9 => "EraseEOF",
                        0xFA => "Play",
                        0xFB => "Zoom",
                        0xFC => "NoName",
                        0xFD => "PA1",
                        0xFE => "Clear",
                        _ => "Key" + code
                    }
                };
            case Platform.Linux:
                return code switch {
                    >= 0xB7 and <= 0xC2 => "F" + (code - 0xAA),
                    0x54 or >= 0xC3 and <= 0xC7 or >= 0xF7 and <= 0xFF => "unnamed" + code,
                    _ => code switch {
                        0x00 => "Reserved",
                        0x55 => "Zenkakuhankaku",
                        0x56 => "102ND",
                        0x59 => "RO",
                        0x5A => "Katakana",
                        0x5B => "Hiragana",
                        0x5C => "Henkan",
                        0x5D => "KatakanaHiragana",
                        0x5E => "Muhenkan",
                        0x5F => "Comma",
                        0x60 => "Enter",
                        0x61 => "RightControl",
                        0x62 => "Slash",
                        0x63 => "SysRq",
                        0x64 => "RightAlt",
                        0x65 => "LineFeed",
                        0x70 => "Macro",
                        0x71 => "Mute",
                        0x72 => "VolumeDown",
                        0x73 => "VolumeUp",
                        0x74 => "Power",
                        0x75 => "Equal",
                        0x76 => "PlusMinus",
                        0x79 => "Pause",
                        0x7A => "RightAlt",
                        0x7B => "RightControl",
                        0x7C => "Yen",
                        0x7F => "Compose",
                        0x80 => "Stop",
                        0x81 => "Again",
                        0x82 => "Props",
                        0x83 => "Undo",
                        0x84 => "Front",
                        0x85 => "Copy",
                        0x86 => "Open",
                        0x87 => "Paste",
                        0x88 => "Find",
                        0x89 => "Cut",
                        0x8A => "Help",
                        0x8B => "Menu",
                        0x8C => "Calc",
                        0x8D => "Setup",
                        0x8E => "Sleep",
                        0x8F => "WakeUp",
                        0x90 => "File",
                        0x91 => "SendFile",
                        0x92 => "DeleteFile",
                        0x93 => "Xfer",
                        0x94 => "Prog1",
                        0x95 => "Prog2",
                        0x96 => "WWW",
                        0x97 => "MSDOS",
                        0x99 => "Direction",
                        0x9A => "CycleWindows",
                        0x9B => "Mail",
                        0x9C => "Bookmarks",
                        0x9D => "Computer",
                        0x9E => "Back",
                        0x9F => "Forward",
                        0xA0 => "CloseCD",
                        0xA1 => "EjectCD",
                        0xA2 => "EjectCloseCD",
                        0xA3 => "NextSong",
                        0xA4 => "PlayPause",
                        0xA5 => "PreviousSong",
                        0xA6 => "StopCD",
                        0xA7 => "Record",
                        0xA8 => "Rewind",
                        0xA9 => "Phone",
                        0xAA => "ISO",
                        0xAB => "Config",
                        0xAC => "HomePage",
                        0xAD => "Refresh",
                        0xAE => "Exit",
                        0xAF => "Move",
                        0xB0 => "Edit",
                        0xB3 => "LeftParen",
                        0xB4 => "RightParen",
                        0xB5 => "New",
                        0xB6 => "Redo",
                        0xC8 => "PlayCD",
                        0xC9 => "PauseCD",
                        0xCA => "Prog3",
                        0xCB => "Prog4",
                        0xCC => "Dashboard",
                        0xCD => "Suspend",
                        0xCE => "Close",
                        0xCF => "Play",
                        0xD0 => "FastForward",
                        0xD1 => "BassBoost",
                        0xD2 => "Print",
                        0xD3 => "HP",
                        0xD4 => "Camera",
                        0xD5 => "Sound",
                        0xD6 => "Question",
                        0xD7 => "Email",
                        0xD8 => "Chat",
                        0xD9 => "Search",
                        0xDA => "Connect",
                        0xDB => "Finance",
                        0xDC => "Sport",
                        0xDD => "Shop",
                        0xDE => "AltErase",
                        0xDF => "Cancel",
                        0xE0 => "BrightnessDown",
                        0xE1 => "BrightnessUp",
                        0xE2 => "Media",
                        0xE3 => "SwitchVideoMode",
                        0xE4 => "KbdIllumToggle",
                        0xE5 => "KbdIllumDown",
                        0xE6 => "KbdIllumUp",
                        0xE7 => "Send",
                        0xE8 => "Reply",
                        0xE9 => "ForwardMail",
                        0xEA => "Save",
                        0xEB => "Documents",
                        0xEC => "Battery",
                        0xED => "Bluetooth",
                        0xEE => "WLAN",
                        0xEF => "UWB",
                        0xF0 => "Unknown",
                        0xF1 => "VideoNext",
                        0xF2 => "VideoPrev",
                        0xF3 => "BrightnessCycle",
                        0xF4 => "BrightnessZero",
                        0xF5 => "DisplayOff",
                        0xF6 => "Wimax",
                        _ => "Key" + code
                    }
                };
            case Platform.Mac:
                return code switch {
                    105 => "F13",
                    107 => "F14",
                    113 => "F15",
                    106 => "F16",
                    64 => "F17",
                    79 => "F18",
                    80 => "F19",
                    90 => "F20",
                    63 => "fn",
                    261 => "Option",
                    55 => "Super",
                    _ => "Key" + code
                };
        }
        return "Key" + code;

        #endregion
    }

    private static void CheckResetRain() {
        RainManager.enabled = Settings.useRain;
        if(Settings.useRain) return;
        while(RainManager.RawRainQueue.TryDequeue(out RawRain rawRain)) RawRain.AddPool(rawRain);
        while(RainManager.RainList.Count > 0) {
            int index = RainManager.RainList.Count - 1;
            Rain rain = RainManager.RainList[index];
            RainManager.RainList.RemoveAt(index);
            RawRain.AddPool(rain.RawRain);
            rain.RawRain = null;
            rain.Pool.AddPool(rain, rain.IsGhost);
        }
    }

    private void ChangeKeyViewer() {
        KeyViewerSetting settings = Settings;
        if(_keyShare) {
            KeyCode[] keyCode1 = GetKeyCode();
            KeyCode[] keyCode2;
            string[] keyText1 = GetKeyText();
            string[] keyText2;
            switch(_currentKeyViewerStyle) {
                case KeyviewerStyle.Key12:
                    keyCode2 = settings.key12;
                    keyText2 = settings.key12Text;
                    break;
                case KeyviewerStyle.Key16:
                    keyCode2 = settings.key16;
                    keyText2 = settings.key16Text;
                    break;
                case KeyviewerStyle.Key20:
                    keyCode2 = settings.key20;
                    keyText2 = settings.key20Text;
                    break;
                case KeyviewerStyle.Key10:
                    keyCode2 = settings.key10;
                    keyText2 = settings.key10Text;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            int size = Math.Min(keyCode1.Length, keyCode2.Length);
            for(int i = 0; i < size; i++) {
                keyCode1[i] = keyCode2[i];
                keyText1[i] = keyText2[i];
            }
            Mod.SaveSetting();
        }
        _currentKeyViewerStyle = settings.KeyViewerStyle;
        ResetKeyViewer();
    }

    private void ResetKeyViewer() {
        _selectedKey = -1;
        for(int i = 0; i < HandOutIndex; i++) {
            Key key = Keys[i];
            if(key?.GameObject) Object.Destroy(key.GameObject);
            Keys[i] = null;
        }
        Object.Destroy(Total.GameObject);
        Object.Destroy(Kps.GameObject);
        InitializeKeyViewer();
        UpdateKeyLimit();
    }

    private void InitializeKeyViewer() {
        _currentKeyMaxY = Settings.KeyViewerStyle switch {
            KeyviewerStyle.Key10 or KeyviewerStyle.Key12 => 976,
            KeyviewerStyle.Key20 => 922,
            _ => 940
        };
        switch(Settings.KeyViewerStyle) {
            case KeyviewerStyle.Key12:
                Initialize0KeyViewer();
                break;
            case KeyviewerStyle.Key16:
                Initialize1KeyViewer();
                break;
            case KeyviewerStyle.Key20:
                Initialize2KeyViewer();
                break;
            case KeyviewerStyle.Key10:
                Initialize3KeyViewer();
                break;
        }
    }

    private void ResetFootKeyViewer() {
        for(int i = HandOutIndex; i < FootOutIndex; i++) {
            Key key = Keys[i];
            if(key?.GameObject) Object.Destroy(key.GameObject);
            Keys[i] = null;
        }
        InitializeFootKeyViewer();
        UpdateKeyLimit();
    }

    private void InitializeFootKeyViewer() {
        if(Settings.FootKeyViewerStyle is < FootKeyviewerStyle.Key2 or > FootKeyviewerStyle.Key16) return;
        
        InitializeFootKeyViewer(Settings.FootKeyViewerStyle switch {
            FootKeyviewerStyle.Key2 => 2,
            FootKeyviewerStyle.Key4 => 4,
            FootKeyviewerStyle.Key6 => 6,
            FootKeyviewerStyle.Key8 => 8,
            _ => 16
        });
    }

    private static string Bold(string text, bool bold) {
        return bold ? $"<b>{text}</b>" : text;
    }

    protected override void OnHideGUI() {
        _winAPICool = 0;
        _sizeString = null;
        _keyPressed = null;
        _confirmResetCount = false;
        if(_selectedKey == -1) return;
        Main.Instance.SaveSetting();
        _selectedKey = -1;
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private static bool CheckKey(KeyCode keyCode) {
        return (int) keyCode < 0x1000 ? Input.GetKey(keyCode) : GetAsyncKeyState((int) keyCode - 0x1000) != 0;
    }

    private void Initialize0KeyViewer() {
        float y = Settings.YLocation;
        for(int i = 0; i < 8; i++) Keys[i] = CreateKey(i, 54 * i, 79 + y, 50, 0);
        Keys[8] = CreateKey(8, 81 + 54, 25 + y, 77, 1);
        Keys[9] = CreateKey(9, 81, 25 + y, 50, 1);
        Keys[10] = CreateKey(10, 54 * 4, 25 + y, 77, 1);
        Keys[11] = CreateKey(11, 54 * 4 + 81, 25 + y, 50, 1);
        for(int i = 0; i < 4; i++) {
            int j = BackSequence12[i];
            Keys[j].RainPool = Keys[i + 2].RainPool;
        }
        Kps = CreateKey(-1, 0, 25 + y, 77, -1);
        Total = CreateKey(-2, 81 + 54 * 5, 25 + y, 77, -1);
    }

    private void Initialize1KeyViewer() {
        float y = Settings.YLocation;
        for(int i = 0; i < 8; i++) Keys[i] = CreateKey(i, 54 * i, 115 + y, 50, 0);
        for(int i = 0; i < 8; i++) {
            int j = BackSequence16[i];
            Keys[j] = CreateKey(j, 54 * i, 61 + y, 50, 1);
            Keys[j].RainPool = Keys[i].RainPool;
        }
        Kps = CreateKey(-1, 0, 15 + y, 212, -1, true);
        Total = CreateKey(-2, 216, 15 + y, 212, -1, true);
    }

    private void Initialize2KeyViewer() {
        float y = Settings.YLocation;
        for(int i = 0; i < 8; i++) Keys[i] = CreateKey(i, 54 * i, 133 + y, 50, 0);
        for(int i = 0; i < 8; i++) {
            int j = BackSequence20[i];
            Keys[j] = CreateKey(j, 54 * i, 79 + y, 50, 1);
            Keys[j].RainPool = Keys[i].RainPool;
        }
        Keys[16] = CreateKey(16, 81 + 54, 25 + y, 77, 3);
        Keys[17] = CreateKey(17, 81, 25 + y, 50, 3);
        Keys[18] = CreateKey(18, 54 * 4, 25 + y, 77, 3);
        Keys[19] = CreateKey(19, 54 * 4 + 81, 25 + y, 50, 3);
        Kps = CreateKey(-1, 0, 25 + y, 77, -1);
        Total = CreateKey(-2, 81 + 54 * 5, 25 + y, 77, -1);
    }

    private void Initialize3KeyViewer() {
        float y = Settings.YLocation;
        for(int i = 0; i < 8; i++) Keys[i] = CreateKey(i, 54 * i, 79 + y, 50, 0);
        Keys[8] = CreateKey(8, 81, 25 + y, 131, 1);
        Keys[8].RainPool = Keys[3].RainPool;
        Keys[9] = CreateKey(9, 54 * 4, 25 + y, 131, 1);
        Keys[9].RainPool = Keys[4].RainPool;
        Kps = CreateKey(-1, 0, 25 + y, 77, -1);
        Total = CreateKey(-2, 81 + 54 * 5, 25 + y, 77, -1);
    }

    private void InitializeFootKeyViewer(int size) {
        bool twoLine = size > 10;
        if(twoLine) size /= 2;
        int limit = size + HandOutIndex;
        for(int line = 0; line < (twoLine ? 2 : 1); line++) {
            int x = 432;
            for(int i = 20; i < 22; i++) for(int j = i; j < limit; j++) {
                Keys[j + line * size] = CreateKey(j++ + line * size, x, 15 + line * 30, 30, -1, true, false);
                x += 34;
            }
        }
    }

    private Key CreateKey(int i, float x, float y, float sizeX, int raining, bool slim = false, bool count = true) {
        GameObject gameObject = new("Key " + i);
        KeyViewerSetting settings = Settings;
        RectTransform objTransform = gameObject.AddComponent<RectTransform>();
        objTransform.SetParent(KeyViewerSizeObject.transform);
        objTransform.sizeDelta = new Vector2(sizeX, slim ? 30 : 50);
        objTransform.anchorMin = objTransform.anchorMax = Vector2.zero;
        objTransform.pivot = new Vector2(0, 0.5f);
        objTransform.anchoredPosition = new Vector2(x, y);
        objTransform.localScale = Vector3.one;
        Key key = new(gameObject);
        gameObject = new GameObject("Background");
        RectTransform transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(objTransform);
        transform.anchorMin = transform.anchorMax = transform.pivot = Vector2.zero;
        transform.anchoredPosition = Vector2.zero;
        transform.sizeDelta = new Vector2(sizeX * 2, (slim ? 30 : 50) * 2);
        transform.localScale = new Vector3(0.5f, 0.5f);
        Image image = gameObject.AddComponent<Image>();
        image.color = settings.Background;
        image.sprite = BundleLoader.KeyBackground;
        image.type = Image.Type.Sliced;
        key.Background = new AsyncImage(image);
        gameObject = new GameObject("Outline");
        transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(objTransform);
        transform.anchorMin = transform.anchorMax = transform.pivot = Vector2.zero;
        transform.anchoredPosition = Vector2.zero;
        transform.sizeDelta = new Vector2(sizeX * 2, (slim ? 30 : 50) * 2);
        transform.localScale = new Vector3(0.5f, 0.5f);
        image = gameObject.AddComponent<Image>();
        image.color = settings.Outline;
        image.sprite = BundleLoader.KeyOutline;
        image.type = Image.Type.Sliced;
        key.Outline = new AsyncImage(image);
        gameObject = new GameObject("KeyText");
        transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(objTransform);
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
        text.fontSizeMax = count ? 20 : 13;
        text.alignment = slim && count ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
        text.color = settings.Text;
        key.Text = new AsyncText(text);
        if(count) {
            gameObject = new GameObject("CountText");
            transform = gameObject.AddComponent<RectTransform>();
            transform.SetParent(objTransform);
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
            key.Value = new AsyncText(text);
        }
        UpdateKeyText(key, i);
        key.Color = raining < 2 ? raining + 1 : raining;
        key.SiblingIndex = (key.Color - 1) * 2;
        if(raining != 0 && raining != 2 && raining != 3) return key;
        gameObject = new GameObject("RainLine");
        transform = gameObject.AddComponent<RectTransform>();
        key.RainPool = new RainPool(transform);
        transform.SetParent(objTransform);
        transform.sizeDelta = new Vector2(sizeX, 275);
        transform.anchorMin = transform.anchorMax = transform.pivot = Vector2.zero;
        transform.anchoredPosition = new Vector2(0, raining switch {
            0 => -223,
            3 => -115,
            _ => -169
        });
        transform.localScale = Vector3.one;
        return key;
    }

    private static void UpdateKeyText(Key key, int i) {
        switch(i) {
            case -1:
                key.Text.SetTextForce("KPS");
                key.Value.SetTextForce("0");
                return;
            case -2:
                key.Text.SetTextForce("Total");
                key.Value.SetTextForce(KeyCountData.Instance.TotalCount.ToString());
                return;
            default:
                KeyCode[] keyCodes;
                KeyViewerSetting settings = Settings;
                if(i < HandOutIndex) {
                    keyCodes = GetKeyCode();
                    string[] keyText = GetKeyText();
                    key.Text.SetTextForce(keyText[i] ?? KeyToString(keyCodes[i]));
                    if(i == 9 && settings.KeyViewerStyle == KeyviewerStyle.Key10) i = 10;
                    key.Value.SetTextForce(KeyCountData.Instance.Count[i].ToString());
                } else {
                    keyCodes = GetFootKeyCode();
                    key.Text.SetTextForce(KeyToString(keyCodes[i - HandOutIndex]));
                }
                break;
        }
    }

    #region KeyCode To Showing String

    private static string KeyToString(KeyCode keyCode) {
        string keyString = ToString(keyCode);
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
            "Zenkakuhankaku" => "全角",
            _ => keyString
        };
    }

    #endregion

    private static void UpdateKeyLimit() {
        KeyViewerSetting settings = Settings;
        if(ADOBase.platform != Platform.Windows || !settings.AutoSetupKeyLimit || !AdofaiTweaksAPI.IsExist && !KeyboardChatterBlockerAPI.IsExist) return;
        Dictionary<KeyCode, List<int>> codeDictionary = GetKeyCodes();
        KeyCode[] keyCodes = GetKeyCode();
        KeyCode[] footKeyCodes = GetFootKeyCode();
        HashSet<KeyCode> keys = [..keyCodes.Where(t => (int) t < 0x1000)];
        foreach(KeyCode keyCode in footKeyCodes) if((int) keyCode < 0x1000) keys.Add(keyCode);
        HashSet<ushort> asyncKeys = [];
        foreach(KeyCode code in keyCodes) {
            if((int) code < 0x1000) {
                if(!codeDictionary.TryGetValue(code, out List<int> value)) continue;
                foreach(int i in value) asyncKeys.Add((ushort) i);
            } else asyncKeys.Add((ushort) ((int) code - 0x1000));
        }
        foreach(KeyCode code in footKeyCodes) {
            if((int) code < 0x1000) {
                if(!codeDictionary.TryGetValue(code, out List<int> value)) continue;
                foreach(int i in value) asyncKeys.Add((ushort) i);
            } else asyncKeys.Add((ushort) ((int) code - 0x1000));
        }
        List<KeyCode> keyList = new(keys);
        List<ushort> asyncKeyList = asyncKeys.ToList();
        if(AdofaiTweaksAPI.IsExist) AdofaiTweaksAPI.UpdateKeyLimit(keyList, asyncKeyList);
        if(KeyboardChatterBlockerAPI.IsExist) KeyboardChatterBlockerAPI.UpdateKeyLimit(keyList, asyncKeyList);
    }

    private static Dictionary<KeyCode, List<int>> GetKeyCodes() {
        JArray array = JArray.Parse(File.ReadAllText(Path.Combine(Main.Instance.Path, "KeyCodes.json")));
        Dictionary<KeyCode, List<int>> dictionary = new();
        int i = -1;
        KeyCode lastCode = KeyCode.None;
        foreach(KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) {
            if(i == -1) {
                i++;
                continue;
            }
            if(keyCode > KeyCode.Mouse6) break;
            if(keyCode is KeyCode.WheelUp or KeyCode.WheelDown) continue;
            if(lastCode == keyCode) continue;
            lastCode = keyCode;
            JToken token = array[i++];
            if(token.Type == JTokenType.Array) {
                List<int> list = [];
                list.AddRange(token.Select(t => t.Value<int>()));
                dictionary.Add(keyCode, list);
            } else {
                int value = token.Value<int>();
                if(value == -1) continue;
                dictionary.Add(keyCode, [value]);
            }
        }
        return dictionary;
    }

    [JAPatch(typeof(UnityModManager.ModEntry), "Load", PatchType.Postfix, false, TryingCatch = false)]
    private static void Load() {
        AdofaiTweaksAPI.Setup();
        KeyboardChatterBlockerAPI.Setup();
        UpdateKeyLimit();
    }
}