using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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
            patcher.AddPatch(GetMistakesManagerR136, new JAPatchAttribute(GetMistakesManagerSafe, PatchType.Transpiler, false));
            patcher.AddPatch(CalculatePercentAccR136, new JAPatchAttribute(CalculatePercentAcc, PatchType.Transpiler, false));
            patcher.AddPatch(GetHitMarginsCountR136, new JAPatchAttribute(GetHitMarginsCount, PatchType.Transpiler, false));
            patcher.AddPatch(GetPlanetSpeedR136, new JAPatchAttribute(GetPlanetSpeed, PatchType.Transpiler, false));
            patcher.AddPatch(LoadSceneR136, new JAPatchAttribute(LoadScene, PatchType.Transpiler, false));
            patcher.AddPatch(GetPercentAccR136, new JAPatchAttribute(GetPercentAcc, PatchType.Transpiler, false));
            patcher.AddPatch(GetPercentXAccR136, new JAPatchAttribute(GetPercentXAcc, PatchType.Transpiler, false));
            patcher.AddPatch(IsCoopModeR136, new JAPatchAttribute(IsCoopMode, PatchType.Replace, false));
        } else {
            patcher.AddPatch(ColorLogoR141, new JAPatchAttribute(ColorLogoSafe, PatchType.Replace, false));
            patcher.AddPatch(GetMistakesManagerR141, new JAPatchAttribute(GetMistakesManagerSafe, PatchType.Replace, false));
            patcher.AddPatch(CalculatePercentAccR141, new JAPatchAttribute(CalculatePercentAcc, PatchType.Replace, false));
            patcher.AddPatch(GetHitMarginsCountR141, new JAPatchAttribute(GetHitMarginsCount, PatchType.Replace, false));
            patcher.AddPatch(GetPlanetSpeedR141, new JAPatchAttribute(GetPlanetSpeed, PatchType.Replace, false));
            patcher.AddPatch(LoadSceneR141, new JAPatchAttribute(LoadScene, PatchType.Replace, false));
            patcher.AddPatch(GetPercentAccR141, new JAPatchAttribute(GetPercentAcc, PatchType.Replace, false));
            patcher.AddPatch(GetPercentXAccR141, new JAPatchAttribute(GetPercentXAcc, PatchType.Replace, false));
            patcher.AddPatch(IsCoopModeR141, new JAPatchAttribute(IsCoopMode, PatchType.Replace, false));
        }
        patcher.Patch();
    }

    public static void ColorLogoSafe(this scrLogoText text, Color color, bool isFire) => throw new NotSupportedException("This functionality is not implemented");
    public static scrMistakesManager GetMistakesManagerSafe(this scrController controller) => throw new NotSupportedException("This functionality is not implemented");
    public static void CalculatePercentAcc() => throw new NotSupportedException("This functionality is not implemented");
    public static int[] GetHitMarginsCount() => throw new NotSupportedException("This functionality is not implemented");
    public static double GetPlanetSpeed(scrController controller) => throw new NotSupportedException("This functionality is not implemented");
    public static void LoadScene(string name) => throw new NotSupportedException("This functionality is not implemented");
    public static float GetPercentAcc() => throw new NotSupportedException("This functionality is not implemented");
    public static float GetPercentXAcc() => throw new NotSupportedException("This functionality is not implemented");
    public static bool IsCoopMode() => throw new NotSupportedException("This functionality is not implemented");

    #region R136

    private static IEnumerable<CodeInstruction> ColorLogoR136(IEnumerable<CodeInstruction> instructions) => [
        new(OpCodes.Ldarg_0),
        new(OpCodes.Ldarg_1),
        new(OpCodes.Ldarg_2),
        new(OpCodes.Call, typeof(scrLogoText).GetMethod("ColorLogo", BindingFlags.Public | BindingFlags.Instance)),
        new(OpCodes.Ret)
    ];

    private static IEnumerable<CodeInstruction> GetMistakesManagerR136(IEnumerable<CodeInstruction> instructions) => [
        new(OpCodes.Ldarg_0),
        new(OpCodes.Ldfld, typeof(scrController).GetField("mistakesManager", BindingFlags.Public | BindingFlags.Instance)),
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

    #endregion

    #region R141

    private static void ColorLogoR141(scrLogoText text, Color color, bool isFire) => text.ColorLogo(color, isFire);
    private static scrMistakesManager GetMistakesManagerR141() => scrController.instance.playerManager.mistakesManager;
    private static void CalculatePercentAccR141() {
        foreach(scrMarginTracker tracker in scrMistakesManager.marginTrackers) tracker.CalculatePercentAcc();
    }
    private static int[] GetHitMarginsCountR141() => scrMistakesManager.marginTrackers[0].hitMarginsCount;
    private static double GetPlanetSpeedR141(scrController controller) => controller.playerOne.planetarySystem.speed;
    private static void LoadSceneR141(string name) => ADOBase.loader.LoadScene(name);
    private static float GetPercentAccR141() => scrController.instance.playerManager.mistakesManager.percentAcc;
    private static float GetPercentXAccR141() => scrController.instance.playerManager.mistakesManager.percentXAcc;
    private static bool IsCoopModeR141() => scrController.coopMode;

    #endregion
}