using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JipperResourcePack.OverlayContents;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents.OtherModApi;

// Optional bridge to OrbitRender's public overlay API.
//
// No compile-time reference to OrbitRender is required.
public static class OrbitRenderAPI {
    public const string ProviderId = "JipperResourcePack";
    public const int RequiredApiVersion = 1;

    private static Type _apiType;
    private static MethodInfo _register;
    private static MethodInfo _unregister;
    private static PropertyInfo _isRendering;
    private static readonly object Gate = new();
    private static bool _registered;
    private static float _nextResolve;
    private static Provider _provider;
    private static long _replayTicks;
    private static volatile bool _replayActive;
    private static Harmony _lifecyclePatches;
    private static readonly Dictionary<RectTransform, float> RainDepths = new();

    public static bool IsReplayActive => _replayActive;

    public static bool IsRendering => Resolve() && _isRendering?.GetValue(null) is true;

    public static bool TryGetReplayTicks(out long ticks) {
        ticks = _replayTicks;
        return _replayActive;
    }

    public static void Setup() {
        if(_lifecyclePatches != null) return;
        Harmony harmony = new("JipperResourcePack.OrbitRenderAPI");
        try {
            harmony.Patch(AccessTools.DeclaredMethod(typeof(Main), "OnEnable"), postfix: new HarmonyMethod(typeof(OrbitRenderAPI), nameof(TryRegister)));
            harmony.Patch(AccessTools.DeclaredMethod(typeof(Main), "OnUpdate"), postfix: new HarmonyMethod(typeof(OrbitRenderAPI), nameof(TryRegister)));
            harmony.Patch(AccessTools.DeclaredMethod(typeof(Main), "OnDisable"), prefix: new HarmonyMethod(typeof(OrbitRenderAPI), nameof(Shutdown)));
            _lifecyclePatches = harmony;
        } catch(Exception e) {
            harmony.UnpatchAll(harmony.Id);
            Main.Instance.LogException(e);
        }
    }

    private static void TryRegister() {
        if(_registered) return;
        if(Time.unscaledTime < _nextResolve) return;
        _nextResolve = Time.unscaledTime + 1f;
        if(!Resolve()) return;
        if(_isRendering?.GetValue(null) is true) return;
        try {
            _provider ??= new Provider();
            if(_register.Invoke(null, new object[] { ProviderId, _provider }) is true) {
                _registered = true;
                Main.Instance.Log("OrbitRenderAPI is loaded.");
            }
        } catch(Exception e) {
            Main.Instance.Log("OrbitRenderAPI is not loaded.");
            Main.Instance.LogException(e);
        }
    }

    private static void Shutdown() {
        if(!_registered) return;
        try {
            _unregister?.Invoke(null, new object[] { ProviderId });
        } catch(Exception e) {
            Main.Instance.LogException(e);
        } finally {
            RestoreRainDepths();
            SetInputSuspended(false);
            _registered = false;
            _replayActive = false;
            _replayTicks = 0;
        }
    }

    private static bool Resolve() {
        if(_apiType != null) return true;
        lock(Gate) {
            if(_apiType != null) return true;
            foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                Type type = assembly.GetType("OrbitRender.OverlayApi", false);
                if(type == null) continue;
                FieldInfo versionField = type.GetField("ApiVersion", BindingFlags.Public | BindingFlags.Static);
                if(versionField == null || versionField.GetValue(null) is not int version || version < RequiredApiVersion) continue;
                MethodInfo register = type.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
                if(register == null) continue;
                _apiType = type;
                _register = register;
                _unregister = type.GetMethod("Unregister", BindingFlags.Public | BindingFlags.Static);
                _isRendering = type.GetProperty("IsRendering", BindingFlags.Public | BindingFlags.Static);
                return true;
            }
            return false;
        }
    }

    // OrbitRender looks these methods up by name on the registered object.
    private sealed class Provider {
        public IEnumerable<Canvas> GetOverlayCanvases() {
            Overlay overlay = Overlay.Instance;
            if(overlay != null && overlay.Canvas != null) yield return overlay.Canvas;
            // The key viewer is part of the replay presentation only; a normal
            // render with Mod Overlay enabled shows the status HUD alone.
            if(!_replayActive) yield break;
            KeyViewer keyViewer = KeyViewer.Instance;
            if(keyViewer == null || !keyViewer.KeyViewerObject) yield break;
            Canvas canvas = keyViewer.KeyViewerObject.GetComponent<Canvas>();
            if(canvas != null) yield return canvas;
        }

        public void OnRenderSessionBegin(bool replay) {
            _replayActive = replay;
            _replayTicks = 0;
            if(replay) SetInputSuspended(true);
        }

        public void OnRenderSessionEnd() {
            RestoreRainDepths();
            SetInputSuspended(false);
            _replayActive = false;
            _replayTicks = 0;
        }

        public void OnReplayClock(double videoSeconds, long ticks) => _replayTicks = ticks;

        public void OnBeforeOverlayCapture(bool replay) {
            if(!replay) return;
            RainManager manager = KeyViewer.RainManager;
            if(manager == null) return;
            foreach(Rain rain in manager.RainList) {
                if(rain?.Transform == null) continue;
                RectTransform transform = rain.Transform;
                Vector3 position = transform.localPosition;
                if(Mathf.Approximately(position.z, 0f)) continue;
                if(!RainDepths.ContainsKey(transform)) RainDepths.Add(transform, position.z);
                position.z = 0f;
                transform.localPosition = position;
            }
        }

        public void OnAfterOverlayCapture(bool replay) => RestoreRainDepths();

        public void OnReplayInput(string key, bool down, long ticks) {
            if(!Enum.TryParse(key, true, out KeyCode code) || code == KeyCode.None) return;
            KeyViewer.Instance?.ApplyReplayKey(code, down, ticks);
        }
    }

    // Replay input is fed directly through the API, so the live SkyHook
    // listener must not also dispatch the same virtual events.
    private static void SetInputSuspended(bool suspended) {
        KeyViewer.Instance?.SetReplayInput(suspended);
    }

    private static void RestoreRainDepths() {
        foreach(KeyValuePair<RectTransform, float> entry in RainDepths) {
            if(entry.Key == null) continue;
            Vector3 position = entry.Key.localPosition;
            position.z = entry.Value;
            entry.Key.localPosition = position;
        }
        RainDepths.Clear();
    }

}
