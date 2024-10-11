using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using JALib.Tools;
using JipperResourcePack.Async;
using JipperResourcePack.Keyviewer.OtherModApi;
using JipperResourcePack.SettingTool;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using Application = UnityEngine.Application;
using Object = UnityEngine.Object;

namespace JipperResourcePack.Keyviewer;

public class KeyViewer : Feature {

    public static KeyViewerSettings Settings;
    public static readonly Color Background = new(0.5607843f, 0.2352941f, 1, 0.1960784f);
    public static readonly Color BackgroundClicked = Color.white;
    public static readonly Color Outline = new(0.5529412f, 0.2431373f, 1);
    public static readonly Color OutlineClicked = Color.white;
    public static readonly Color Text = Color.white;
    public static readonly Color TextClicked = Color.black;
    public static readonly Color RainColor = new(0.5137255f, 0.1254902f, 0.858823538f);
    public static readonly Color RainColor2 = Color.white;
    public static readonly Color RainColor3 = Color.magenta;
    public static readonly byte[] BackSequence12 = [9, 8, 10, 11];
    public static readonly byte[] BackSequence16 = [12, 13, 9, 8, 10, 11, 14, 15];
    public static readonly byte[] BackSequence20 = [12, 13, 9, 8, 10, 11, 14, 15, 17, 16, 18, 19];
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
    private bool KeyChangeExpanded;
    private bool TextChangeExpanded;
    private bool[] ColorExpanded;
    private KeyviewerStyle currentKeyViewerStyle;

    public int SelectedKey = -1;
    public int WinAPICool;
    public bool TextChanged;

    public KeyViewer() : base(Main.Instance, nameof(KeyViewer), settingType: typeof(KeyViewerSettings)) {
        if(ADOBase.platform != Platform.Windows) return;
        Patcher.AddPatch(Load);
        currentKeyViewerStyle = Settings.KeyViewerStyle;
        AdofaiTweaksAPI.Setup();
        KeyboardChatterBlockerAPI.Setup();
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
        Keys = new Key[28];
        KeyViewerSettings settings = Settings;
        switch(settings.KeyViewerStyle) {
            case KeyviewerStyle.Key12:
                Initialize0KeyViewer();
                break;
            case KeyviewerStyle.Key16:
                Initialize1KeyViewer();
                break;
            case KeyviewerStyle.Key20:
                Initialize2KeyViewer();
                break;
        }
        switch(settings.FootKeyViewerStyle) {
            case FootKeyviewerStyle.Key2:
                InitializeFootKeyViewer(2);
                break;
            case FootKeyviewerStyle.Key4:
                InitializeFootKeyViewer(4);
                break;
            case FootKeyviewerStyle.Key6:
                InitializeFootKeyViewer(6);
                break;
            case FootKeyviewerStyle.Key8:
                InitializeFootKeyViewer(8);
                break;
        }
        Object.DontDestroyOnLoad(KeyViewerObject);
        PressTimes = new ConcurrentQueue<long>();
        Stopwatch = Stopwatch.StartNew();
        KeyinputListener = new Thread(ListenKey);
        KeyinputListener.Start();
        Application.wantsToQuit += Application_wantsToQuit;
        UpdateKeyLimit();
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
        KeyViewerSettings settings = Settings;
        settingGUI.AddSettingToggle(ref KeyShare, localization["keyViewer.keyShare"]);
        settingGUI.AddSettingToggle(ref settings.DownLocation, localization["keyViewer.downLocation"], ResetKeyViewer);
        if(ADOBase.platform == Platform.Windows && (AdofaiTweaksAPI.IsExist || KeyboardChatterBlockerAPI.IsExist))
            settingGUI.AddSettingToggle(ref settings.AutoSetupKeyLimit, localization["keyViewer.autoSetupKeyLimit"], UpdateKeyLimit);
        settingGUI.AddSettingEnum(ref settings.KeyViewerStyle, localization["keyViewer.style"], ChangeKeyViewer);
        settingGUI.AddSettingEnum(ref settings.FootKeyViewerStyle, localization["keyViewer.style"], ResetFootKeyViewer);
        KeyCode[] keyCodes = GetKeyCode();
        KeyCode[] footKeyCodes = GetFootKeyCode();
        string[] keyTexts = GetKeyText();
        GUILayout.Space(12f);
        GUIStyle toggleStyle = new() {
            fixedWidth = 10f,
            normal = new GUIStyleState { textColor = Color.white },
            fontSize = 15,
            margin = new RectOffset(4, 2, 6, 6)
        };
        GUILayout.BeginHorizontal();
        KeyChangeExpanded = GUILayout.Toggle(KeyChangeExpanded, KeyChangeExpanded ? "◢" : "▶", toggleStyle);
        if(GUILayout.Button(localization["keyViewer.keyChange"], GUI.skin.label)) KeyChangeExpanded = !KeyChangeExpanded;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(KeyChangeExpanded) {
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
                for(int i = 0; i < footKeyCodes.Length; i++) CreateButton(i++ + 20, false);
                for(int i = 1; i < footKeyCodes.Length; i++) CreateButton(i++ + 20, false);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            if(SelectedKey != -1 && !TextChanged) GUILayout.Label($"<b>{localization["keyViewer.inputKey"]}</b>");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.Space(12f);
        }
        GUILayout.BeginHorizontal();
        TextChangeExpanded = GUILayout.Toggle(TextChangeExpanded, TextChangeExpanded ? "◢" : "▶", toggleStyle);
        if(GUILayout.Button(localization["keyViewer.textChange"], GUI.skin.label)) TextChangeExpanded = !TextChangeExpanded;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(TextChangeExpanded) {
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
            if(SelectedKey != -1 && TextChanged) {
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
                }
                if(GUILayout.Button(localization["keyViewer.textSave"])) {
                    SelectedKey = -1;
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
        bool a = GUILayout.Toggle(ColorExpanded != null, ColorExpanded != null ? "◢" : "▶", toggleStyle);
        if(ColorExpanded != null != a) ColorExpanded = a ? new bool[8] : null;
        if(GUILayout.Button(localization["keyViewer.color"], GUI.skin.label)) ColorExpanded = ColorExpanded == null ? new bool[8] : null;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if(ColorExpanded != null) {
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
                ColorExpanded[i] = GUILayout.Toggle(ColorExpanded[i], ColorExpanded[i] ? "◢" : "▶", toggleStyle);
                if(GUILayout.Button(localization["keyViewer.color." + char.ToLower(names[i][0]) + names[i][1..]], GUI.skin.label)) ColorExpanded[i] = !ColorExpanded[i];
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if(!ColorExpanded[i]) continue;
                GUILayout.BeginHorizontal();
                GUILayout.Space(18f);
                GUILayout.BeginVertical();
                if(settings.GetValue<ColorCache>(names[i]).SettingGUI(settingGUI, typeof(KeyViewer).GetValue<Color>(names[i]))) {
                    for(int i2 = 0; i2 < keyCodes.Length; i2++) UpdateKey(i2, CheckKey(keyCodes[i2]));
                    if(footKeyCodes != null) for(int i2 = 0; i2 < footKeyCodes.Length; i2++) UpdateKey(i2 + 20, CheckKey(footKeyCodes[i2]));
                    Kps.background.color = Total.background.color = settings.Background;
                    Kps.outline.color = Total.outline.color = settings.Outline;
                    Kps.text.color = Kps.value.color = Total.text.color = Total.value.color = settings.Text;
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
        if(SelectedKey == -1 || TextChanged || !Application.isFocused) return;
        if(Input.anyKeyDown) {
            foreach(KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) {
                if(!Input.GetKeyDown(keyCode)) continue;
                SetupKey(keyCode);
                break;
            }
        } else {
            for(int i = 0; i < 256; i++) {
                if((GetAsyncKeyState(i) & 0x8000) == 0) continue;
                if(WinAPICool++ < 6) break;
                KeyCode keyCode = (KeyCode) i + 0x1000;
                SetupKey(keyCode);
                break;
            }
        }
        return;

        void CreateButton(int i, bool textChanged) {
            if(!GUILayout.Button(Bold(i < 20 ? textChanged ? keyTexts[i] ?? KeyToString(keyCodes[i]) : ToString(keyCodes[i]) : ToString(footKeyCodes[i - 20]),
                   i == SelectedKey && textChanged == TextChanged))) return;
            SelectedKey = i;
            TextChanged = textChanged;
        }

        void SetupKey(KeyCode keyCode) {
            if(SelectedKey < 20) keyCodes[SelectedKey] = keyCode;
            else footKeyCodes[SelectedKey - 20] = keyCode;
            Keys[SelectedKey].text.tmp.text = (SelectedKey < 20 ? keyTexts[SelectedKey] : null) ?? KeyToString(keyCode);
            SelectedKey = -1;
            WinAPICool = 0;
            UpdateKeyLimit();
            Main.Instance.SaveSetting();
        }
    }

    private static KeyCode[] GetKeyCode() {
        return Settings.KeyViewerStyle switch {
            KeyviewerStyle.Key12 => Settings.key12,
            KeyviewerStyle.Key16 => Settings.key16,
            KeyviewerStyle.Key20 => Settings.key20,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static KeyCode[] GetFootKeyCode() {
        return Settings.FootKeyViewerStyle switch {
            FootKeyviewerStyle.Key2 => Settings.footkey2,
            FootKeyviewerStyle.Key4 => Settings.footkey4,
            FootKeyviewerStyle.Key6 => Settings.footkey6,
            FootKeyviewerStyle.Key8 => Settings.footkey8,
            _ => []
        };
    }

    private static string[] GetKeyText() {
        return Settings.KeyViewerStyle switch {
            KeyviewerStyle.Key12 => Settings.key12Text,
            KeyviewerStyle.Key16 => Settings.key16Text,
            KeyviewerStyle.Key20 => Settings.key20Text,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static byte[] GetBackSequence() {
        return Settings.KeyViewerStyle switch {
            KeyviewerStyle.Key12 => BackSequence12,
            KeyviewerStyle.Key16 => BackSequence16,
            KeyviewerStyle.Key20 => BackSequence20,
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


    private void ChangeKeyViewer() {
        KeyViewerSettings settings = Settings;
        if(KeyShare) {
            KeyCode[] keyCode1 = GetKeyCode();
            KeyCode[] keyCode2;
            string[] keyText1 = GetKeyText();
            string[] keyText2;
            switch(currentKeyViewerStyle) {
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
        currentKeyViewerStyle = settings.KeyViewerStyle;
        ResetKeyViewer();
    }

    private void ResetKeyViewer() {
        SelectedKey = -1;
        for(int i = 0; i < 20; i++) {
            Key key = Keys[i];
            if(key) Object.Destroy(key.gameObject);
        }
        Object.Destroy(Total.gameObject);
        Object.Destroy(Kps.gameObject);
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
        }
    }

    private void ResetFootKeyViewer() {
        for(int i = 20; i < 28; i++) {
            Key key = Keys[i];
            if(key) Object.Destroy(key.gameObject);
        }
        switch(Settings.FootKeyViewerStyle) {
            case FootKeyviewerStyle.Key2:
                InitializeFootKeyViewer(2);
                break;
            case FootKeyviewerStyle.Key4:
                InitializeFootKeyViewer(4);
                break;
            case FootKeyviewerStyle.Key6:
                InitializeFootKeyViewer(6);
                break;
            case FootKeyviewerStyle.Key8:
                InitializeFootKeyViewer(8);
                break;
        }
    }

    private static string Bold(string text, bool bold) {
        return bold ? $"<b>{text}</b>" : text;
    }

    protected override void OnHideGUI() {
        WinAPICool = 0;
        if(SelectedKey == -1) return;
        Main.Instance.SaveSetting();
        SelectedKey = -1;
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private static bool CheckKey(KeyCode keyCode) {
        return (int) keyCode < 0x1000 ? Input.GetKey(keyCode) : GetAsyncKeyState((int) keyCode - 0x1000) != 0;
    }

    private void ListenKey() {
        try {
            KeyViewerSettings settings = Settings;
            bool[] keyState = new bool[28];
            int repeat = 0;
            while(KeyinputListener is { IsAlive: true } && Enabled) {
                long elapsedMilliseconds = Stopwatch.ElapsedMilliseconds;
                if(Application.isFocused) {
                    KeyCode[] keyCodes = GetKeyCode();
                    for(int i = 0; i < keyCodes.Length; i++) {
                        bool current = CheckKey(keyCodes[i]);
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
                        RawRain rawRain = new(key.rain.transform, elapsedMilliseconds, key.color);
                        key.rawRainQueue.Enqueue(rawRain);
                        key.rainList.Add(rawRain);
                        Save = true;
                    }
                    keyCodes = GetFootKeyCode();
                    for(int i = 0; i < keyCodes.Length; i++) {
                        bool current = CheckKey(keyCodes[i]);
                        int index = i + 20;
                        Key key = Keys[index];
                        if(!key || current == keyState[index]) continue;
                        keyState[index] = current;
                        UpdateKey(index, current);
                        if(!current) continue;
                        PressTimes.Enqueue(elapsedMilliseconds);
                        settings.Count[index]++;
                        Total.value.text = (++settings.TotalCount).ToString();
                        Save = true;
                    }
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
        } catch (ThreadAbortException) {
        } catch (Exception e) {
            if(KeyinputListener is not { IsAlive: true }) return;
            Main.Instance.LogException(e);
        }
    }

    private void UpdateKey(int i, bool enabled) {
        Key key = Keys[i];
        KeyViewerSettings settings = Settings;
        key.background.color = enabled ? settings.BackgroundClicked : settings.Background;
        key.outline.color = enabled ? settings.OutlineClicked : settings.Outline;
        key.text.color = enabled ? settings.TextClicked : settings.Text;
        if(key.value) key.value.color = key.text.color;
    }

    private void Initialize0KeyViewer() {
        int remove = Settings.DownLocation ? 200 : 0;
        for(int i = 0; i < 8; i++) Keys[i] = CreateKey(i, 54 * i, 279 - remove, 50, 0);
        Keys[8] = CreateKey(8, 81 + 54, 225 - remove, 77, 2);
        Keys[9] = CreateKey(9, 81, 225 - remove, 50, 2);
        Keys[10] = CreateKey(10, 54 * 4, 225 - remove, 77, 2);
        Keys[11] = CreateKey(11, 54 * 4 + 81, 225 - remove, 50, 2);
        Kps = CreateKey(-1, 0, 225 - remove, 77, -1);
        Total = CreateKey(-2, 81 + 54 * 5, 225 - remove, 77, -1);
    }

    private void Initialize1KeyViewer() {
        int remove = Settings.DownLocation ? 200 : 0;
        for(int i = 0; i < 8; i++) Keys[i] = CreateKey(i, 54 * i, 320 - remove, 50, 0);
        for(int i = 0; i < 8; i++) {
            int j = BackSequence16[i];
            Keys[j] = CreateKey(j, 54 * i, 266 - remove, 50, 1);
            Keys[j].rain = Keys[i].rain;
        }
        Kps = CreateKey(-1, 0, 220 - remove, 212, -1, true);
        Total = CreateKey(-2, 216, 220 - remove, 212, -1, true);
    }

    private void Initialize2KeyViewer() {
        int remove = Settings.DownLocation ? 200 : 0;
        for(int i = 0; i < 8; i++) Keys[i] = CreateKey(i, 54 * i, 333 - remove, 50, 0);
        for(int i = 0; i < 8; i++) {
            int j = BackSequence20[i];
            Keys[j] = CreateKey(j, 54 * i, 279 - remove, 50, 1);
            Keys[j].rain = Keys[i].rain;
        }
        Keys[16] = CreateKey(16, 81 + 54, 225 - remove, 77, 3);
        Keys[17] = CreateKey(17, 81, 225 - remove, 50, 3);
        Keys[18] = CreateKey(18, 54 * 4, 225 - remove, 77, 3);
        Keys[19] = CreateKey(19, 54 * 4 + 81, 225 - remove, 50, 3);
        Kps = CreateKey(-1, 0, 225 - remove, 77, -1);
        Total = CreateKey(-2, 81 + 54 * 5, 225 - remove, 77, -1);
    }

    private void InitializeFootKeyViewer(int size) {
        size += 20;
        int x = 432;
        for(int i = 20; i < 22; i++) for(int j = i; j < size; j++) {
            Keys[j] = CreateKey(j++, x, 15, 30, -1, true, false);
            x += 34;
        }
    }

    private Key CreateKey(int i, float x, float y, float sizeX, int raining, bool slim = false, bool count = true) {
        GameObject obj = new("Key " + i);
        KeyViewerSettings settings = Settings;
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
        image.color = settings.Background;
        image.sprite = BundleLoader.KeyBackground;
        image.type = Image.Type.Sliced;
        key.background = gameObject.AddComponent<AsyncImage>();
        gameObject = new GameObject("Outline");
        transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(obj.transform);
        transform.anchorMin = transform.anchorMax = transform.pivot = Vector2.zero;
        transform.anchoredPosition = Vector2.zero;
        transform.sizeDelta = new Vector2(sizeX, slim ? 30 : 50);
        transform.localScale = Vector3.one;
        image = gameObject.AddComponent<Image>();
        image.color = settings.Outline;
        image.sprite = BundleLoader.KeyOutline;
        image.type = Image.Type.Sliced;
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
        text.color = settings.Text;
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
        key.color = (byte) (raining < 2 ? raining + 1 : raining);
        if(raining != 0 && raining != 2 && raining != 3) return key;
        key.rain = new GameObject("RainLine");
        transform = key.rain.AddComponent<RectTransform>();
        transform.SetParent(obj.transform);
        transform.sizeDelta = new Vector2(sizeX, 275);
        transform.anchorMin = transform.anchorMax = transform.pivot = Vector2.zero;
        transform.anchoredPosition = new Vector2(0, raining switch {
            0 => -223,
            3 => -277,
            _ => -169
        });
        transform.localScale = Vector3.one;
        return key;
    }

    private static void UpdateKeyText(Key key, int i) {
        switch(i) {
            case -1:
                key.text.tmp.text = "KPS";
                key.value.tmp.text = "0";
                return;
            case -2:
                key.text.tmp.text = "Total";
                key.value.tmp.text = Settings.TotalCount.ToString();
                return;
            default:
                KeyCode[] keyCodes;
                KeyViewerSettings settings = Settings;
                if(i < 20) {
                    keyCodes = GetKeyCode();
                    string[] keyText = GetKeyText();
                    key.text.tmp.text = keyText[i] ?? KeyToString(keyCodes[i]);
                    key.value.tmp.text = settings.Count[i].ToString();
                } else {
                    keyCodes = GetFootKeyCode();
                    key.text.tmp.text = KeyToString(keyCodes[i - 20]);
                }
                break;
        }
    }

    #region KeyCode To Showing String

    public static string KeyToString(KeyCode keyCode) {
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
        KeyViewerSettings settings = Settings;
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
        List<KeyCode> keyList = keys.ToList();
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

    public class KeyViewerSettings : JASetting {
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
        public KeyCode[] key20 = [
            KeyCode.Tab, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.E, KeyCode.P, KeyCode.Equals, KeyCode.Backspace, KeyCode.Backslash,
            KeyCode.Space, KeyCode.C, KeyCode.Comma, KeyCode.Period, KeyCode.CapsLock, KeyCode.LeftShift, KeyCode.Return, KeyCode.H,
            KeyCode.CapsLock, KeyCode.D, KeyCode.RightShift, KeyCode.Semicolon
        ];
        public string[] key20Text = new string[20];
        public KeyCode[] footkey2 = [KeyCode.F2, KeyCode.F7];
        public KeyCode[] footkey4 = [KeyCode.F2, KeyCode.F7, KeyCode.F3, KeyCode.F6];
        public KeyCode[] footkey6 = [KeyCode.F1, KeyCode.F8, KeyCode.F2, KeyCode.F7, KeyCode.F3, KeyCode.F6];
        public KeyCode[] footkey8 = [KeyCode.F1, KeyCode.F8, KeyCode.F2, KeyCode.F7, KeyCode.F3, KeyCode.F6, KeyCode.F4, KeyCode.F5];
        public int[] Count = new int[36];
        public int TotalCount;
        public bool DownLocation;
        public bool AutoSetupKeyLimit = true;
        public ColorCache Background = new(KeyViewer.Background);
        public ColorCache BackgroundClicked = new(KeyViewer.BackgroundClicked);
        public ColorCache Outline = new(KeyViewer.Outline);
        public ColorCache OutlineClicked = new(KeyViewer.OutlineClicked);
        public ColorCache Text = new(KeyViewer.Text);
        public ColorCache TextClicked = new(KeyViewer.TextClicked);
        public ColorCache RainColor = new(KeyViewer.RainColor);
        public ColorCache RainColor2 = new(KeyViewer.RainColor2);
        public ColorCache RainColor3 = new(KeyViewer.RainColor3);

        public KeyViewerSettings(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
            Settings = this;
            if(Count.Length != 24) return;
            int[] cur = Count;
            Count = new int[36];
            for(int i = 0; i < 16; i++) Count[i] = cur[i];
            for(int i = 16; i < 24; i++) Count[i + 4] = cur[i];
        }
    }
}