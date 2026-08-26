using System;
using System.Collections.Generic;
using System.Reflection;
using JALib.Core.Patch;
using JALib.Tools;
using SkyHook;

namespace JipperResourcePack.KeyViewerContents;

static class AsyncInputHook {
    private static bool _gameEnabled;
    private static bool _available;

    internal static void Setup(JAPatcher patcher) {
        List<MethodInfo> listeners = FindGameListeners();
        if(listeners.Count == 0) {
            Main.Instance.Error("Failed to find the SkyHook listener of AsyncInputManager.");
            return;
        }
        if(listeners.Count > 1) Main.Instance.Warning("Found multiple SkyHook listeners of AsyncInputManager.");

        foreach(MethodInfo listener in listeners)
            patcher.AddPatch(IsActive, new JAPatchAttribute(listener, PatchType.Prefix, false) {
                TryingCatch = false
            });
        patcher.AddPatch(typeof(AsyncInputHook));
        _available = true;
    }

    internal static void Acquire() {
        if(!_available) return;
        SkyHookManager manager = SkyHookManager.Instance;
        _gameEnabled = manager.isHookActive;
        if(_gameEnabled) return;
        try {
            SkyHookManager.StartHook();
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    internal static void Release() {
        if(_gameEnabled) return;
        SkyHookManager manager = SkyHookManager.Instance;
        if(!manager.isHookActive) return;
        try {
            SkyHookManager.StopHook();
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    [JAPatch(typeof(AsyncInputManager), nameof(AsyncInputManager.ToggleHook), PatchType.Prefix, false)]
    private static bool ToggleHook(AsyncInputManager ____instance, bool active) {
        _gameEnabled = active;
        SkyHookManager manager = SkyHookManager.Instance;
        if(!manager.isHookActive) SkyHookManager.StartHook();
        ____instance.enabled = active;
        return false;
    }

    [JAPatch(typeof(AsyncInputManager), "get_isActive", PatchType.Replace, false, TryingCatch = false)]
    private static bool IsActive() => _gameEnabled;

    private static List<MethodInfo> FindGameListeners() {
        List<MethodInfo> listeners = [];
        AddListeners(listeners, typeof(AsyncInputManager));
        foreach(Type nested in typeof(AsyncInputManager).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)) AddListeners(listeners, nested);
        return listeners;
    }

    private static void AddListeners(List<MethodInfo> listeners, Type type) {
        foreach(MethodInfo method in type.Methods()) {
            ParameterInfo[] parameters = method.GetParameters();
            if(method.ReturnType != typeof(void) || parameters.Length != 1 || parameters[0].ParameterType != typeof(SkyHookEvent)) continue;
            listeners.Add(method);
        }
    }
}
