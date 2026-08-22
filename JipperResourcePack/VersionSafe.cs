using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using JALib.Core.Patch;
using JALib.Tools;
using UnityEngine;

namespace JipperResourcePack;

public static class VersionSafe {
    public static void Setup() {
        Main.Instance.Log("Version Safe Setup");
        JAPatcher patcher = new(Main.Instance);

        if(VersionControl.releaseNumber < 141) {
            patcher.AddPatch(ColorLogoR136, new JAPatchAttribute(ColorLogoSafe, PatchType.Transpiler, false));
            patcher.AddPatch(CalculatePercentAccR136, new JAPatchAttribute(CalculatePercentAcc, PatchType.Transpiler, false));
            patcher.AddPatch(GetHitMarginsCountR136, new JAPatchAttribute(GetHitMarginsCount, PatchType.Transpiler, false));
            patcher.AddPatch(GetPlanetSpeedR136, new JAPatchAttribute(GetPlanetSpeed, PatchType.Transpiler, false));
            patcher.AddPatch(LoadSceneR136, new JAPatchAttribute(LoadScene, PatchType.Transpiler, false));
            patcher.AddPatch(GetPercentAccR136, new JAPatchAttribute(GetPercentAcc, PatchType.Transpiler, false));
            patcher.AddPatch(GetPercentXAccR136, new JAPatchAttribute(GetPercentXAcc, PatchType.Transpiler, false));
            patcher.AddPatch(IsCoopModeR136, new JAPatchAttribute(IsCoopMode, PatchType.Replace, false));
            patcher.AddPatch(GetPlayerCountR136, new JAPatchAttribute(GetPlayerCount, PatchType.Replace, false));
        } else {
            patcher.AddPatch(ColorLogoR141, new JAPatchAttribute(ColorLogoSafe, PatchType.Replace, false));
            patcher.AddPatch(CalculatePercentAccR141, new JAPatchAttribute(CalculatePercentAcc, PatchType.Replace, false));
            patcher.AddPatch(GetHitMarginsCountR141, new JAPatchAttribute(GetHitMarginsCount, PatchType.Replace, false));
            patcher.AddPatch(GetPlanetSpeedR141, new JAPatchAttribute(GetPlanetSpeed, PatchType.Replace, false));
            patcher.AddPatch(LoadSceneR141, new JAPatchAttribute(LoadScene, PatchType.Replace, false));
            patcher.AddPatch(GetPercentAccR141, new JAPatchAttribute(GetPercentAcc, PatchType.Replace, false));
            patcher.AddPatch(GetPercentXAccR141, new JAPatchAttribute(GetPercentXAcc, PatchType.Replace, false));
            patcher.AddPatch(IsCoopModeR141, new JAPatchAttribute(IsCoopMode, PatchType.Replace, false));
            patcher.AddPatch(GetPlayerCountR141, new JAPatchAttribute(GetPlayerCount, PatchType.Replace, false));
        }

        if(VersionControl.releaseNumber < 146) {
            patcher.AddPatch(RunAfterR145, new JAPatchAttribute(RunAfter, PatchType.Replace, false));
        } else {
            patcher.AddPatch(RunAfterR146, new JAPatchAttribute(RunAfter, PatchType.Replace, false));
        }

        if(VersionControl.releaseNumber < 148) {
            patcher.AddPatch(IsEnableCompetitiveModeR147, new JAPatchAttribute(IsEnableCompetitiveMode, PatchType.Replace, false));
            patcher.AddPatch(WriteHitMarginTextR147, new JAPatchAttribute(WriteHitMarginText, PatchType.Replace, false));
            patcher.AddPatch(WriteHitMarginTextWithoutColorR147, new JAPatchAttribute(WriteHitMarginTextWithoutColor, PatchType.Replace, false));
        } else {
            patcher.AddPatch(IsEnableCompetitiveModeR148, new JAPatchAttribute(IsEnableCompetitiveMode, PatchType.Replace, false));
            patcher.AddPatch(WriteHitMarginTextR148, new JAPatchAttribute(WriteHitMarginText, PatchType.Replace, false));
            patcher.AddPatch(WriteHitMarginTextWithoutColorR148, new JAPatchAttribute(WriteHitMarginTextWithoutColor, PatchType.Replace, false));
        }
        patcher.Patch();
    }

    public static void ColorLogoSafe(this scrLogoText text, Color color, bool isFire) => throw new NotSupportedException("This functionality is not implemented");
    public static void CalculatePercentAcc() => throw new NotSupportedException("This functionality is not implemented");
    public static int[] GetHitMarginsCount() => throw new NotSupportedException("This functionality is not implemented");
    public static double GetPlanetSpeed(scrController controller) => throw new NotSupportedException("This functionality is not implemented");
    public static void LoadScene(string name) => throw new NotSupportedException("This functionality is not implemented");
    public static float GetPercentAcc() => throw new NotSupportedException("This functionality is not implemented");
    public static float GetPercentXAcc() => throw new NotSupportedException("This functionality is not implemented");
    public static bool IsCoopMode() => throw new NotSupportedException("This functionality is not implemented");
    public static int GetPlayerCount() => throw new NotSupportedException("This functionality is not implemented");
    public static void RunAfter(Action action) => throw new NotSupportedException("This functionality is not implemented");
    public static bool IsEnableCompetitiveMode() => throw new NotSupportedException("This functionality is not implemented");
    public static string WriteHitMarginText(int[] hits, string prefix, string postfix) => throw new NotSupportedException("This functionality is not implemented");
    public static string WriteHitMarginTextWithoutColor(int[] hits, string prefix, string postfix) => throw new NotSupportedException("This functionality is not implemented");

    #region R136

    private static IEnumerable<CodeInstruction> ColorLogoR136(IEnumerable<CodeInstruction> instructions) => [
        new(OpCodes.Ldarg_0),
        new(OpCodes.Ldarg_1),
        new(OpCodes.Ldarg_2),
        new(OpCodes.Call, typeof(scrLogoText).GetMethod("ColorLogo", BindingFlags.Public | BindingFlags.Instance)),
        new(OpCodes.Ret)
    ];

    private static IEnumerable<CodeInstruction> CalculatePercentAccR136(IEnumerable<CodeInstruction> instructions) => [
        new(OpCodes.Ldsfld, typeof(scrController).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static)),
        new(OpCodes.Ldfld, typeof(scrController).GetField("mistakesManager", BindingFlags.Public | BindingFlags.Instance)),
        new(OpCodes.Call, typeof(scrMistakesManager).GetMethod("CalculatePercentAcc", BindingFlags.Public | BindingFlags.Instance)),
        new(OpCodes.Ret)
    ];

    private static IEnumerable<CodeInstruction> GetHitMarginsCountR136(IEnumerable<CodeInstruction> instructions) => [
        new(OpCodes.Ldsfld, typeof(scrMistakesManager).GetField("hitMarginsCount", BindingFlags.Public | BindingFlags.Static)),
        new(OpCodes.Ret)
    ];

    private static IEnumerable<CodeInstruction> GetPlanetSpeedR136(IEnumerable<CodeInstruction> instructions) => [
        new(OpCodes.Ldarg_0),
        new(OpCodes.Ldfld, typeof(scrController).GetField("speed", BindingFlags.Public | BindingFlags.Instance)),
        new(OpCodes.Ret)
    ];

    private static IEnumerable<CodeInstruction> LoadSceneR136(IEnumerable<CodeInstruction> instructions) => [
        new(OpCodes.Ldarg_0),
        new(OpCodes.Call, typeof(ADOBase).GetMethod("LoadScene", BindingFlags.Public | BindingFlags.Static)),
        new(OpCodes.Ret)
    ];
    
    private static IEnumerable<CodeInstruction> GetPercentAccR136(IEnumerable<CodeInstruction> instructions) => [
        new(OpCodes.Ldsfld, typeof(scrController).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static)),
        new(OpCodes.Ldfld, typeof(scrController).GetField("mistakesManager", BindingFlags.Public | BindingFlags.Instance)),
        new(OpCodes.Ldfld, typeof(scrMistakesManager).GetField("percentAcc", BindingFlags.Public | BindingFlags.Instance)),
        new(OpCodes.Ret)
    ];
    
    private static IEnumerable<CodeInstruction> GetPercentXAccR136(IEnumerable<CodeInstruction> instructions) => [
        new(OpCodes.Ldsfld, typeof(scrController).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static)),
        new(OpCodes.Ldfld, typeof(scrController).GetField("mistakesManager", BindingFlags.Public | BindingFlags.Instance)),
        new(OpCodes.Ldfld, typeof(scrMistakesManager).GetField("percentXAcc", BindingFlags.Public | BindingFlags.Instance)),
        new(OpCodes.Ret)
    ];

    private static bool IsCoopModeR136() => false;
    private static int GetPlayerCountR136() => 1;

    #endregion

    #region R141

    private static void ColorLogoR141(scrLogoText text, Color color, bool isFire) => text.ColorLogo(color, isFire);
    private static void CalculatePercentAccR141() {
        foreach(scrMarginTracker tracker in scrMistakesManager.marginTrackers) tracker.CalculatePercentAcc();
    }
    
    private static int[] GetHitMarginsCountR141() {
        return scrMistakesManager.marginTrackers[0].hitMarginsCount;
    }
    
    private static double GetPlanetSpeedR141(scrController controller) => controller.playerOne.planetarySystem.speed;
    private static void LoadSceneR141(string name) => ADOBase.loader.LoadScene(name);
    private static float GetPercentAccR141() => scrPlayerManager.instance.mistakesManager.percentAcc;
    private static float GetPercentXAccR141() => scrPlayerManager.instance.mistakesManager.percentXAcc;
    private static bool IsCoopModeR141() => scrPlayerManager.playerCount > 1;
    private static int GetPlayerCountR141() => scrPlayerManager.playerCount;

    #endregion

    #region R145

    private static void RunAfterR145(Action action) => Task.Yield().OnCompleted(action);

    #endregion

    #region R146

    private static void RunAfterR146(Action action) => UniTask.Yield(PlayerLoopTiming.LastUpdate).GetAwaiter().OnCompleted(action);

    #endregion

    #region R147

    private static bool IsEnableCompetitiveModeR147() => false;
    private static string WriteHitMarginTextR147(int[] hits, string prefix, string postfix) {
        return $"{prefix}{hits[9]} <color=red>{hits[0]} <color=#FF6F4E>{hits[1]} <color=#A0FF4E>{hits[2]} <color=#60FF4E>{hits[3] + hits[10]}</color> {hits[4]}</color> {hits[5]}</color> {hits[6]}</color> {hits[8]}{postfix}";
    }

    private static string WriteHitMarginTextWithoutColorR147(int[] hits, string prefix, string postfix) {
        return $"{prefix}{hits[9]} {hits[0]} {hits[1]} {hits[2]} {hits[3] + hits[10]} {hits[4]} {hits[5]} {hits[6]} {hits[8]}{postfix}";
    }

    #endregion

    #region R148

    private static bool IsEnableCompetitiveModeR148() => Persistence.enableCompetitiveMode;
    private static string WriteHitMarginTextR148(int[] hits, string prefix, string postfix) {
        return Persistence.enableCompetitiveMode ? 
                   $"{prefix}{hits[9]} <color=red>{hits[0]} <color=#FF6F4E>{hits[1]} <color=#A0FF4E>{hits[2]} <color=#60FF4E>{hits[3]} <color=#FFF>{hits[4] + hits[12]}</color> {hits[5]}</color> {hits[6]}</color> {hits[7]}</color> {hits[8]}</color> {hits[10]}{postfix}" : 
                   $"{prefix}{hits[9]} <color=red>{hits[0]} <color=#FF6F4E>{hits[1]} <color=#A0FF4E>{hits[2]} <color=#60FF4E>{hits[3] + hits[4] + hits[5] + hits[12]}</color> {hits[6]}</color> {hits[7]}</color> {hits[8]}</color> {hits[10]}{postfix}";
    }

    private static string WriteHitMarginTextWithoutColorR148(int[] hits, string prefix, string postfix) {
        return Persistence.enableCompetitiveMode ? 
                   $"{prefix}{hits[9]} {hits[0]} {hits[1]} {hits[2]} {hits[3]} {hits[4] + hits[12]} {hits[5]} {hits[6]} {hits[7]} {hits[8]} {hits[10]}{postfix}" :
                   $"{prefix}{hits[9]} {hits[0]} {hits[1]} {hits[2]} {hits[3] + hits[4] + hits[5] + hits[12]} {hits[6]} {hits[7]} {hits[8]} {hits[10]}{postfix}";
    }

    #endregion

}