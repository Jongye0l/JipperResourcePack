using System;
#if !R145After
using System.Collections.Frozen;
using System.Collections.Generic;
#endif
using System.Runtime.CompilerServices;
using System.Text;
using SkyHook;
using TMPro;
#if R146After
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using Object = UnityEngine.Object;

namespace JipperResourcePack;

public static class VersionSafe {
    private static readonly StringBuilder SharedBuilder = new(256);

    static VersionSafe() {
#if !R145After
        SetupKey();
#endif
    }

    public static StringBuilder GetSharedBuilder() {
        SharedBuilder.Length = 0;
        return SharedBuilder;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ColorLogoSafe(this scrLogoText text, Color color, bool isFire) => text.ColorLogo(color, isFire);

    private static int DigitCount(int value) {
        return value < 10 ? 1 :
            value < 100 ? 2 :
            value < 1000 ? 3 :
            value < 10000 ? 4 :
            value < 100000 ? 5 :
            value < 1000000 ? 6 :
            value < 10000000 ? 7 :
            value < 100000000 ? 8 :
            value < 1000000000 ? 9 : 10;
    }

    private static void AppendBalance(StringBuilder sb, int count) {
        if(count <= 0) return;
        sb.Append("<color=#0000>").Append('0', count).Append("</color>");
    }

#if R141After
    public static bool FontScaleSupported => true;

    public static void CalculatePercentAcc() {
        foreach(scrMarginTracker tracker in scrMistakesManager.marginTrackers) tracker.CalculatePercentAcc();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int[] GetHitMarginsCount() => scrMistakesManager.marginTrackers[0].hitMarginsCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GetPlanetSpeed(scrController controller) => controller.playerOne.planetarySystem.speed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LoadScene(string name) => ADOBase.loader.LoadScene(name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetPercentAcc() => scrPlayerManager.instance.mistakesManager.percentAcc;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetPercentXAcc() => scrPlayerManager.instance.mistakesManager.percentXAcc;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCoopMode() => scrPlayerManager.playerCount > 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TMP_FontAsset CreateFontAssetFromFile(string fontPath) =>
        TMP_FontAsset.CreateFontAsset(fontPath, 0, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KeyCode SkyHookKeyToUnityKey(KeyLabel key) => SkyHookKeyMapper.SkyHookKeyToUnityKey(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KeyLabel UnityKeyToSkyHookKey(KeyCode key) => SkyHookKeyMapper.UnityKeyToSkyHookKey(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T FindAnyObjectByType<T>() where T : Object => Object.FindAnyObjectByType<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color GetOrange() => Color.orange;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color GetSkyBlue() => Color.skyBlue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color GetPurple() => Color.purple;

#else
    public static bool FontScaleSupported => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CalculatePercentAcc() => scrController.instance.mistakesManager.CalculatePercentAcc();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int[] GetHitMarginsCount() => scrMistakesManager.hitMarginsCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GetPlanetSpeed(scrController controller) => controller.speed;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LoadScene(string name) => ADOBase.LoadScene(name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetPercentAcc() => scrController.instance.mistakesManager.percentAcc;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetPercentXAcc() => scrController.instance.mistakesManager.percentXAcc;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCoopMode() => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TMP_FontAsset CreateFontAssetFromFile(string fontPath) =>
        TMP_FontAsset.CreateFontAsset(new Font(fontPath), 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KeyCode SkyHookKeyToUnityKey(KeyLabel key) => AsyncKeyMapper.AsyncKeyToUnityKey(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KeyLabel UnityKeyToSkyHookKey(KeyCode key) => AsyncKeyMapper.UnityKeyToAsyncKey(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS0618 // Type or member is obsolete
    public static T FindAnyObjectByType<T>() where T : Object => Object.FindObjectOfType<T>();
#pragma warning restore CS0618 // Type or member is obsolete

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color GetOrange() => new Color(1f, 0.6470588f, 0.0f, 1f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color GetSkyBlue() => new Color(0.5294118f, 0.8078432f, 0.9215687f, 1f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color GetPurple() => new Color(0.627451f, 0.1254902f, 0.9411765f, 1f);

#endif

#if R145After
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort KeyLabelToNativeKeyCode(KeyLabel label) => SkyHookKeyMapper.KeyLabelToNativeKeyCode(label);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KeyLabel NativeKeyCodeToKeyLabel(ushort nativeKeyCode) => SkyHookKeyMapper.NativeKeyCodeToKeyLabel(nativeKeyCode);
#else
    private static FrozenDictionary<KeyLabel, ushort> _keyLabelToNativeKeyCodeMap;
    private static FrozenDictionary<ushort, KeyLabel> _nativeKeyCodeToKeyLabelMap;

    private static void SetupKey() {
        if(_keyLabelToNativeKeyCodeMap != null && _nativeKeyCodeToKeyLabelMap != null) return;
        Dictionary<KeyLabel, ushort> keyLabelToNativeKeyCodeMap = new();
        Dictionary<ushort, KeyLabel> nativeKeyCodeToKeyLabelMap = new();
        switch(ADOBase.platform) {
            case Platform.Windows:
                Span<ushort> keyCodes1 = [
                    0, 114/*MouseLeft*/, 1, 115/*MouseRight*/, 2, 116/*MouseMiddle*/,
                    3, 117/*MouseX1*/, 4, 118/*MouseX2*/, 8, 38/*Backspace*/,
                    9, 39/*Tab*/, 13, 65/*Enter*/, 19, 86/*PauseBreak*/,
                    20, 53/*CapsLock*/, 27, 0/*Escape*/, 32, 81/*Space*/,
                    33, 89/*PageUp*/, 34, 92/*PageDown*/, 35, 91/*End*/,
                    36, 88/*Home*/, 37, 94/*ArrowLeft*/, 38, 93/*ArrowUp*/,
                    39, 96/*ArrowRight*/, 40, 95/*ArrowDown*/, 44, 84/*PrintScreen*/,
                    45, 87/*Insert*/, 46, 90/*Delete*/, 48, 35/*Alpha0*/,
                    49, 26/*Alpha1*/, 50, 27/*Alpha2*/, 51, 28/*Alpha3*/,
                    52, 29/*Alpha4*/, 53, 30/*Alpha5*/, 54, 31/*Alpha6*/,
                    55, 32/*Alpha7*/, 56, 33/*Alpha8*/, 57, 34/*Alpha9*/,
                    65, 54/*A*/, 66, 71/*B*/, 67, 69/*C*/,
                    68, 56/*D*/, 69, 42/*E*/, 70, 57/*F*/,
                    71, 58/*G*/, 72, 59/*H*/, 73, 47/*I*/,
                    74, 60/*J*/, 75, 61/*K*/, 76, 62/*L*/,
                    77, 73/*M*/, 78, 72/*N*/, 79, 48/*O*/,
                    80, 49/*P*/, 81, 40/*Q*/, 82, 43/*R*/,
                    83, 55/*S*/, 84, 44/*T*/, 85, 46/*U*/,
                    86, 70/*V*/, 87, 41/*W*/, 88, 68/*X*/,
                    89, 45/*Y*/, 90, 67/*Z*/, 91, 79/*Super*/,
                    96, 110/*Keypad0*/, 97, 101/*Keypad1*/, 98, 102/*Keypad2*/,
                    99, 103/*Keypad3*/, 100, 104/*Keypad4*/, 101, 105/*Keypad5*/,
                    102, 106/*Keypad6*/, 103, 107/*Keypad7*/, 104, 108/*Keypad8*/,
                    105, 109/*Keypad9*/, 106, 99/*KeypadAsterisk*/, 107, 112/*KeypadPlus*/,
                    109, 100/*KeypadMinus*/, 110, 111/*KeypadDot*/, 111, 98/*KeypadSlash*/,
                    112, 1/*F1*/, 113, 2/*F2*/, 114, 3/*F3*/,
                    115, 4/*F4*/, 116, 5/*F5*/, 117, 6/*F6*/,
                    118, 7/*F7*/, 119, 8/*F8*/, 120, 9/*F9*/,
                    121, 10/*F10*/, 122, 11/*F11*/, 123, 12/*F12*/,
                    124, 13/*F13*/, 125, 14/*F14*/, 126, 15/*F15*/,
                    127, 16/*F16*/, 128, 17/*F17*/, 129, 18/*F18*/,
                    130, 19/*F19*/, 131, 20/*F20*/, 132, 21/*F21*/,
                    133, 22/*F22*/, 134, 23/*F23*/, 135, 24/*F24*/,
                    144, 97/*NumLock*/, 145, 85/*ScrollLock*/, 160, 66/*LShift*/,
                    161, 77/*RShift*/, 162, 78/*LControl*/, 163, 83/*RControl*/,
                    164, 80/*LAlt*/, 165, 82/*RAlt*/, 186, 63/*Semicolon*/,
                    187, 37/*Equal*/, 188, 74/*Comma*/, 189, 36/*Minus*/,
                    190, 75/*Dot*/, 191, 76/*Slash*/, 192, 25/*Grave*/,
                    219, 50/*LeftBrace*/, 220, 52/*BackSlash*/, 221, 51/*RightBrace*/,
                    222, 64/*Apostrophe*/
                ];
                AddKeyPairs(keyCodes1, keyLabelToNativeKeyCodeMap, nativeKeyCodeToKeyLabelMap);
                break;
            case Platform.Linux:
                Span<ushort> keyCodes2 = [
                    0xff08, 38/*Backspace*/, 0xff09, 39/*Tab*/, 0xff0d, 65/*Enter*/,
                    0xff13, 86/*PauseBreak*/, 0xff14, 85/*ScrollLock*/, 0xff1b, 0/*Escape*/,
                    0xffff, 90/*Delete*/, 0xff50, 88/*Home*/, 0xff51, 94/*ArrowLeft*/,
                    0xff52, 93/*ArrowUp*/, 0xff53, 96/*ArrowRight*/, 0xff54, 95/*ArrowDown*/,
                    0xff55, 89/*PageUp*/, 0xff56, 92/*PageDown*/, 0xff57, 91/*End*/,
                    0xff61, 84/*PrintScreen*/, 0xff63, 87/*Insert*/, 0xff7f, 97/*NumLock*/,
                    0xff8d, 113/*KeypadEnter*/, 0xffb0, 88/*Home*/, 0xffb4, 94/*ArrowLeft*/,
                    0xffb8, 93/*ArrowUp*/, 0xffb6, 96/*ArrowRight*/, 0xffb2, 95/*ArrowDown*/,
                    0xffb5, 89/*PageUp*/, 0xffb7, 92/*PageDown*/, 0xffb1, 91/*End*/,
                    0xffb9, 87/*Insert*/, 0xff9f, 90/*Delete*/, 0xffaa, 99/*KeypadAsterisk*/,
                    0xffab, 112/*KeypadPlus*/, 0xffad, 100/*KeypadMinus*/, 0xffae, 111/*KeypadDot*/,
                    0xffaf, 98/*KeypadSlash*/, 0xffb0, 110/*Keypad0*/, 0xffb1, 101/*Keypad1*/,
                    0xffb2, 102/*Keypad2*/, 0xffb3, 103/*Keypad3*/, 0xffb4, 104/*Keypad4*/,
                    0xffb5, 105/*Keypad5*/, 0xffb6, 106/*Keypad6*/, 0xffb7, 107/*Keypad7*/,
                    0xffb8, 108/*Keypad8*/, 0xffb9, 109/*Keypad9*/, 0xffbe, 1/*F1*/,
                    0xffbf, 2/*F2*/, 0xffc0, 3/*F3*/, 0xffc1, 4/*F4*/,
                    0xffc2, 5/*F5*/, 0xffc3, 6/*F6*/, 0xffc4, 7/*F7*/,
                    0xffc5, 8/*F8*/, 0xffc6, 9/*F9*/, 0xffc7, 10/*F10*/,
                    0xffc8, 11/*F11*/, 0xffc9, 12/*F12*/, 0xffca, 13/*F13*/,
                    0xffcb, 14/*F14*/, 0xffcc, 15/*F15*/, 0xffcd, 16/*F16*/,
                    0xffce, 17/*F17*/, 0xffcf, 18/*F18*/, 0xffd0, 19/*F19*/,
                    0xffd1, 20/*F20*/, 0xffd2, 21/*F21*/, 0xffd3, 22/*F22*/,
                    0xffd4, 23/*F23*/, 0xffd5, 24/*F24*/, 0xffe1, 66/*LShift*/,
                    0xffe2, 77/*RShift*/, 0xffe3, 78/*LControl*/, 0xffe4, 83/*RControl*/,
                    0xffe5, 53/*CapsLock*/, 0xffe9, 80/*LAlt*/, 0xffea, 82/*RAlt*/,
                    0xffeb, 79/*Super*/, 0xffec, 79/*Super*/, 0x0020, 81/*Space*/,
                    0x002c, 74/*Comma*/, 0x002b, 37/*Equal*/, 0x002d, 36/*Minus*/,
                    0x002e, 75/*Dot*/, 0x002f, 76/*Slash*/, 0x0030, 35/*Alpha0*/,
                    0x0031, 26/*Alpha1*/, 0x0032, 27/*Alpha2*/, 0x0033, 28/*Alpha3*/,
                    0x0034, 29/*Alpha4*/, 0x0035, 30/*Alpha5*/, 0x0036, 31/*Alpha6*/,
                    0x0037, 32/*Alpha7*/, 0x0038, 33/*Alpha8*/, 0x0039, 34/*Alpha9*/,
                    0x003d, 37/*Equal*/, 0x003b, 63/*Semicolon*/, 0x0027, 64/*Apostrophe*/,
                    0x0061, 54/*A*/, 0x0062, 71/*B*/, 0x0063, 69/*C*/,
                    0x0064, 56/*D*/, 0x0065, 42/*E*/, 0x0066, 57/*F*/,
                    0x0067, 58/*G*/, 0x0068, 59/*H*/, 0x0069, 47/*I*/,
                    0x006a, 60/*J*/, 0x006b, 61/*K*/, 0x006c, 62/*L*/,
                    0x006d, 73/*M*/, 0x006e, 72/*N*/, 0x006f, 48/*O*/,
                    0x0070, 49/*P*/, 0x0071, 40/*Q*/, 0x0072, 43/*R*/,
                    0x0073, 55/*S*/, 0x0074, 44/*T*/, 0x0075, 46/*U*/,
                    0x0076, 70/*V*/, 0x0077, 41/*W*/, 0x0078, 68/*X*/,
                    0x0079, 45/*Y*/, 0x007a, 67/*Z*/, 0x005b, 50/*LeftBrace*/,
                    0x005d, 51/*RightBrace*/, 0x005c, 52/*BackSlash*/, 0x0060, 25/*Grave*/
                ];
                AddKeyPairs(keyCodes2, keyLabelToNativeKeyCodeMap, nativeKeyCodeToKeyLabelMap);
                break;
            case Platform.Mac:
                Span<ushort> keyCodes3 = [
                    0x64, 52/*BackSlash*/, 0x9E, 65/*Enter*/, 0xE7, 79/*Super*/,
                    0x04, 54/*A*/, 0x05, 71/*B*/, 0x06, 69/*C*/,
                    0x07, 56/*D*/, 0x08, 42/*E*/, 0x09, 57/*F*/,
                    0x0A, 58/*G*/, 0x0B, 59/*H*/, 0x0C, 47/*I*/,
                    0x0D, 60/*J*/, 0x0E, 61/*K*/, 0x0F, 62/*L*/,
                    0x10, 73/*M*/, 0x11, 72/*N*/, 0x12, 48/*O*/,
                    0x13, 49/*P*/, 0x14, 40/*Q*/, 0x15, 43/*R*/,
                    0x16, 55/*S*/, 0x17, 44/*T*/, 0x18, 46/*U*/,
                    0x19, 70/*V*/, 0x1A, 41/*W*/, 0x1B, 68/*X*/,
                    0x1C, 45/*Y*/, 0x1D, 67/*Z*/, 0x1E, 26/*Alpha1*/,
                    0x1F, 27/*Alpha2*/, 0x20, 28/*Alpha3*/, 0x21, 29/*Alpha4*/,
                    0x22, 30/*Alpha5*/, 0x23, 31/*Alpha6*/, 0x24, 32/*Alpha7*/,
                    0x25, 33/*Alpha8*/, 0x26, 34/*Alpha9*/, 0x27, 35/*Alpha0*/,
                    0x28, 65/*Enter*/, 0x29, 0/*Escape*/, 0x2A, 38/*Backspace*/,
                    0x2B, 39/*Tab*/, 0x2C, 81/*Space*/, 0x2D, 36/*Minus*/,
                    0x2E, 37/*Equal*/, 0x2F, 50/*LeftBrace*/, 0x30, 51/*RightBrace*/,
                    0x31, 52/*BackSlash*/, 0x33, 63/*Semicolon*/, 0x34, 64/*Apostrophe*/,
                    0x35, 25/*Grave*/, 0x36, 74/*Comma*/, 0x37, 75/*Dot*/,
                    0x38, 76/*Slash*/, 0x39, 53/*CapsLock*/, 0x3A, 1/*F1*/,
                    0x3B, 2/*F2*/, 0x3C, 3/*F3*/, 0x3D, 4/*F4*/,
                    0x3E, 5/*F5*/, 0x3F, 6/*F6*/, 0x40, 7/*F7*/,
                    0x41, 8/*F8*/, 0x42, 9/*F9*/, 0x43, 10/*F10*/,
                    0x44, 11/*F11*/, 0x45, 12/*F12*/, 0x46, 84/*PrintScreen*/,
                    0x47, 85/*ScrollLock*/, 0x48, 86/*PauseBreak*/, 0x49, 87/*Insert*/,
                    0x4A, 88/*Home*/, 0x4B, 89/*PageUp*/, 0x4C, 90/*Delete*/,
                    0x4D, 91/*End*/, 0x4E, 92/*PageDown*/, 0x4F, 96/*ArrowRight*/,
                    0x50, 94/*ArrowLeft*/, 0x51, 95/*ArrowDown*/, 0x52, 93/*ArrowUp*/,
                    0x53, 97/*NumLock*/, 0x54, 98/*KeypadSlash*/, 0x55, 99/*KeypadAsterisk*/,
                    0x56, 100/*KeypadMinus*/, 0x57, 112/*KeypadPlus*/, 0x58, 113/*KeypadEnter*/,
                    0x59, 101/*Keypad1*/, 0x5A, 102/*Keypad2*/, 0x5B, 103/*Keypad3*/,
                    0x5C, 104/*Keypad4*/, 0x5D, 105/*Keypad5*/, 0x5E, 106/*Keypad6*/,
                    0x5F, 107/*Keypad7*/, 0x60, 108/*Keypad8*/, 0x61, 109/*Keypad9*/,
                    0x62, 110/*Keypad0*/, 0x63, 111/*KeypadDot*/, 0x68, 13/*F13*/,
                    0x69, 14/*F14*/, 0x6A, 15/*F15*/, 0x6B, 16/*F16*/,
                    0x6C, 17/*F17*/, 0x6D, 18/*F18*/, 0x6E, 19/*F19*/,
                    0x6F, 20/*F20*/, 0x70, 21/*F21*/, 0x71, 22/*F22*/,
                    0x72, 23/*F23*/, 0x73, 24/*F24*/, 0xE0, 78/*LControl*/,
                    0xE2, 80/*LAlt*/, 0xE3, 79/*Super*/, 0xE4, 83/*RControl*/,
                    0xE6, 82/*RAlt*/
                ];
                AddKeyPairs(keyCodes3, keyLabelToNativeKeyCodeMap, nativeKeyCodeToKeyLabelMap);
                break;
            default:
                return;
        }
        _keyLabelToNativeKeyCodeMap = keyLabelToNativeKeyCodeMap.ToFrozenDictionary();
        _nativeKeyCodeToKeyLabelMap = nativeKeyCodeToKeyLabelMap.ToFrozenDictionary();
    }

    private static void AddKeyPairs(ReadOnlySpan<ushort> pairs, Dictionary<KeyLabel, ushort> labelToNative, Dictionary<ushort, KeyLabel> nativeToLabel) {
        for(int i = 0; i < pairs.Length; i++) {
            ushort nativeKeyCode = pairs[i++];
            KeyLabel label = (KeyLabel) pairs[i];
            labelToNative[label] = nativeKeyCode;
            nativeToLabel[nativeKeyCode] = label;
        }
    }

    // ReSharper disable once CanSimplifyDictionaryTryGetValueWithGetValueOrDefault
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort KeyLabelToNativeKeyCode(KeyLabel label) => _keyLabelToNativeKeyCodeMap.TryGetValue(label, out ushort nativeKeyCode) ? nativeKeyCode : ushort.MaxValue;

    // ReSharper disable once CanSimplifyDictionaryTryGetValueWithGetValueOrDefault
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static KeyLabel NativeKeyCodeToKeyLabel(ushort nativeKeyCode) => _nativeKeyCodeToKeyLabelMap.TryGetValue(nativeKeyCode, out KeyLabel label) ? label : KeyLabel.Unknown;
#endif

#if R146After
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RunAfter(Action action) => PlayerLoopHelper.AddContinuation(PlayerLoopTiming.LastUpdate, action);
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RunAfter(Action action) => Task.Yield().GetAwaiter().OnCompleted(action);
#endif

#if R149After
    public static bool XScoreSupported => true;

    public static void AppendHitMarginText(StringBuilder sb, int[] hits, string prefix, string postfix) {
        bool competitive = Persistence.enableCompetitiveMode;
        int diff = DigitCount(hits[11]) + DigitCount(hits[0]) + DigitCount(hits[1]) + DigitCount(hits[2])
                   - DigitCount(hits[6]) - DigitCount(hits[7]) - DigitCount(hits[8]) - DigitCount(hits[10]);
        if(competitive) diff += DigitCount(hits[3]) - DigitCount(hits[5]);
        AppendBalance(sb, -diff);
        sb.Append(prefix).Append(hits[11])
            .Append(" <color=red>").Append(hits[0])
            .Append(" <color=#FF6F4E>").Append(hits[1])
            .Append(" <color=#A0FF4E>").Append(hits[2])
            .Append(" <color=#60FF4E>");
        if(competitive)
            sb.Append(hits[3]).Append(" <color=#FFF>").Append(hits[4] + hits[12]).Append("</color> ").Append(hits[5]);
        else sb.Append(hits[3] + hits[4] + hits[5] + hits[12]);
        sb.Append("</color> ").Append(hits[6])
            .Append("</color> ").Append(hits[7])
            .Append("</color> ").Append(hits[8])
            .Append("</color> ").Append(hits[10])
            .Append(postfix);
        AppendBalance(sb, diff);
    }

    public static void AppendHitMarginTextWithoutColor(StringBuilder sb, int[] hits, string prefix, string postfix) {
        bool competitive = Persistence.enableCompetitiveMode;
        int diff = DigitCount(hits[11]) + DigitCount(hits[0]) + DigitCount(hits[1]) + DigitCount(hits[2])
                   - DigitCount(hits[6]) - DigitCount(hits[7]) - DigitCount(hits[8]) - DigitCount(hits[10]);
        if(competitive) diff += DigitCount(hits[3]) - DigitCount(hits[5]);
        AppendBalance(sb, -diff);
        sb.Append(prefix).Append(hits[11])
            .Append(' ').Append(hits[0])
            .Append(' ').Append(hits[1])
            .Append(' ').Append(hits[2])
            .Append(' ');
        if(competitive)
            sb.Append(hits[3]).Append(' ').Append(hits[4] + hits[12]).Append(' ').Append(hits[5]);
        else sb.Append(hits[3] + hits[4] + hits[5] + hits[12]);
        sb.Append(' ').Append(hits[6])
            .Append(' ').Append(hits[7])
            .Append(' ').Append(hits[8])
            .Append(' ').Append(hits[10])
          .Append(postfix);
        AppendBalance(sb, diff);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetJudgedTiles(int[] hits, int seqID) => seqID - hits[12 /* Auto */] - hits[14 /* MidSpin */];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetPotentialXAccuracy(int index, float xacc, int seqId, int remaining) => scrMistakesManager.marginTrackers[index].maxPossibleXAcc.SetIfNaN(1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int, int) GetAccuracyCounts(int[] hits) {
        int perfect = hits[3 /* PerfectMinus */] + hits[4 /* XPerfect */] + hits[5 /* PerfectPlus */] + hits[12 /* Auto */] + hits[14 /* MidSpin */];
        int accurate = perfect + hits[2 /* EarlyPerfect */] + hits[6 /* LatePerfect */];
        return (perfect, accurate);
    }
#else
    public static bool XScoreSupported => false;

    public static void AppendHitMarginText(StringBuilder sb, int[] hits, string prefix, string postfix) {
        int diff = DigitCount(hits[9]) + DigitCount(hits[0]) + DigitCount(hits[1]) + DigitCount(hits[2])
                   - DigitCount(hits[4]) - DigitCount(hits[5]) - DigitCount(hits[6]) - DigitCount(hits[8]);
        AppendBalance(sb, -diff);
        sb.Append(prefix).Append(hits[9])
            .Append(" <color=red>").Append(hits[0])
            .Append(" <color=#FF6F4E>").Append(hits[1])
            .Append(" <color=#A0FF4E>").Append(hits[2])
            .Append(" <color=#60FF4E>").Append(hits[3] + hits[10])
            .Append("</color> ").Append(hits[4])
            .Append("</color> ").Append(hits[5])
            .Append("</color> ").Append(hits[6])
            .Append("</color> ").Append(hits[8])
            .Append(postfix);
        AppendBalance(sb, diff);
    }

    public static void AppendHitMarginTextWithoutColor(StringBuilder sb, int[] hits, string prefix, string postfix) {
        int diff = DigitCount(hits[9]) + DigitCount(hits[0]) + DigitCount(hits[1]) + DigitCount(hits[2])
                   - DigitCount(hits[4]) - DigitCount(hits[5]) - DigitCount(hits[6]) - DigitCount(hits[8]);
        AppendBalance(sb, -diff);
        sb.Append(prefix).Append(hits[9])
            .Append(' ').Append(hits[0])
            .Append(' ').Append(hits[1])
            .Append(' ').Append(hits[2])
            .Append(' ').Append(hits[3] + hits[10])
            .Append(' ').Append(hits[4])
            .Append(' ').Append(hits[5])
            .Append(' ').Append(hits[6])
            .Append(' ').Append(hits[8])
            .Append(postfix);
        AppendBalance(sb, diff);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetJudgedTiles(int[] hits, int seqID) => seqID;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetPotentialXAccuracy(int index, float xacc, int seqId, int remaining) {
        int total = seqId + remaining;
        return total == 0 ? 1 : (xacc * seqId + remaining) / total;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int, int) GetAccuracyCounts(int[] hits) {
        int perfect = hits[3 /* Perfect */] + hits[10 /* Auto */];
        int accurate = perfect + hits[2 /* EarlyPerfect */] + hits[4 /* LatePerfect */];
        return (perfect, accurate);
    }
#endif
}
