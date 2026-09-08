using System;
using System.Globalization;
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

#else

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
    public static T FindAnyObjectByType<T>() where T : Object => Object.FindObjectOfType<T>();

#endif

#if R146After
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RunAfter(Action action) => PlayerLoopHelper.AddContinuation(PlayerLoopTiming.LastUpdate, action);
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RunAfter(Action action) => Task.Yield().GetAwaiter().OnCompleted(action);
#endif

#if R149After
    public static string WriteHitMarginText(int[] hits, string prefix, string postfix) {
        bool competitive = Persistence.enableCompetitiveMode;
        int diff = DigitCount(hits[11]) + DigitCount(hits[0]) + DigitCount(hits[1]) + DigitCount(hits[2])
                   - DigitCount(hits[6]) - DigitCount(hits[7]) - DigitCount(hits[8]) - DigitCount(hits[10]);
        if(competitive) diff += DigitCount(hits[3]) - DigitCount(hits[5]);
        StringBuilder sb = GetSharedBuilder();
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
        return sb.ToString();
    }

    public static string WriteHitMarginTextWithoutColor(int[] hits, string prefix, string postfix) {
        bool competitive = Persistence.enableCompetitiveMode;
        int diff = DigitCount(hits[11]) + DigitCount(hits[0]) + DigitCount(hits[1]) + DigitCount(hits[2])
                   - DigitCount(hits[6]) - DigitCount(hits[7]) - DigitCount(hits[8]) - DigitCount(hits[10]);
        if(competitive) diff += DigitCount(hits[3]) - DigitCount(hits[5]);
        StringBuilder sb = GetSharedBuilder();
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
        return sb.ToString();
    }
#else
    public static string WriteHitMarginText(int[] hits, string prefix, string postfix) {
        int diff = DigitCount(hits[9]) + DigitCount(hits[0]) + DigitCount(hits[1]) + DigitCount(hits[2])
                   - DigitCount(hits[4]) - DigitCount(hits[5]) - DigitCount(hits[6]) - DigitCount(hits[8]);
        StringBuilder sb = GetSharedBuilder();
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
        return sb.ToString();
    }

    public static string WriteHitMarginTextWithoutColor(int[] hits, string prefix, string postfix) {
        int diff = DigitCount(hits[9]) + DigitCount(hits[0]) + DigitCount(hits[1]) + DigitCount(hits[2])
                   - DigitCount(hits[4]) - DigitCount(hits[5]) - DigitCount(hits[6]) - DigitCount(hits[8]);
        StringBuilder sb = GetSharedBuilder();
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
        return sb.ToString();
    }
#endif
}
