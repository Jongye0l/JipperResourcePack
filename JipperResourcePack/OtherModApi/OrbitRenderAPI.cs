using System;
using System.Collections.Generic;
using System.Reflection;
using JipperResourcePack.KeyViewerContents;
using JipperResourcePack.OverlayContents;
using UnityEngine;

namespace JipperResourcePack.OtherModApi;

// Optional bridge to OrbitRender's public overlay API.
//
// No compile-time reference to OrbitRender is required.
public static class OrbitRenderAPI {
    public const string ProviderId = "JipperResourcePack";
    public const int RequiredApiVersion = 2;

    private static Type _apiType;
    private static Func<string, object, bool> _register;
    private static Action<string> _unregister;
    private static Func<bool> _isRendering;
    private static bool _registered;
    private static Provider _provider;
    private static long _replayTicks;
    private static volatile bool _replayActive;
    private static readonly HashSet<RectTransform> RainTransforms = new();

    public static bool IsReplayActive => _replayActive;

    public static bool IsRendering => _isRendering != null && _isRendering();

    public static bool TryGetReplayTicks(out long ticks) {
        ticks = _replayTicks;
        return _replayActive;
    }

    public static void Setup() => TryRegister();

    public static void TryRegister() {
        if(_registered) return;
        try {
            if(!Resolve()) return;
            _provider ??= new Provider();
            if(_register(ProviderId, _provider)) {
                _registered = true;
                Main.Instance.Log("OrbitRenderAPI is loaded.");
            }
        } catch(Exception e) {
            Main.Instance.Log("OrbitRenderAPI is not loaded.");
            Main.Instance.LogException(e);
        }
    }

    public static void Shutdown() {
        if(!_registered) return;
        try {
            _unregister?.Invoke(ProviderId);
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
        foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            Type type = assembly.GetType("OrbitRender.OverlayApi", false);
            FieldInfo versionField = type?.GetField("ApiVersion", BindingFlags.Public | BindingFlags.Static);
            if(versionField?.GetValue(null) is not int version || version < RequiredApiVersion) continue;

            MethodInfo register = type.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
            MethodInfo unregister = type.GetMethod("Unregister", BindingFlags.Public | BindingFlags.Static);
            MethodInfo isRendering = type.GetProperty("IsRendering", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
            if((object) register == null || (object) unregister == null || (object) isRendering == null) continue;

            _register = (Func<string, object, bool>) Delegate.CreateDelegate(typeof(Func<string, object, bool>), register);
            _unregister = (Action<string>) Delegate.CreateDelegate(typeof(Action<string>), unregister);
            _isRendering = (Func<bool>) Delegate.CreateDelegate(typeof(Func<bool>), isRendering);
            _apiType = type;
            return true;
        }
        return false;
    }

    // OrbitRender looks these methods up by name on the registered object.
    private sealed class Provider {
        public IEnumerable<Canvas> GetOverlayCanvases() {
            Overlay overlay = Overlay.Instance;
            if(overlay.Canvas) yield return overlay.Canvas;
            // The key viewer is part of the replay presentation only; a normal
            // render with Mod Overlay enabled shows the status HUD alone.
            if(!_replayActive) yield break;
            KeyViewer keyViewer = KeyViewer.Instance;
            if(!keyViewer.KeyViewerObject) yield break;
            Canvas canvas = keyViewer.KeyViewerObject.GetComponent<Canvas>();
            yield return canvas;
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
            if(!manager) return;
            foreach(Rain rain in manager.RainList) {
                RectTransform transform = rain.Transform;
                Vector3 position = transform.localPosition;
                if(Mathf.Approximately(position.z, 0f)) continue;
                RainTransforms.Add(transform);
                position.z = 0f;
                transform.localPosition = position;
            }
        }

        public void OnAfterOverlayCapture(bool replay) => RestoreRainDepths();

        public void OnReplayKey(KeyCode key, int? nativeKey, bool down, long ticks) {
            KeyViewer.Instance.ApplyReplayKey(key, nativeKey, down, ticks);
        }
    }

    // Replay input is fed directly through the API, so the live SkyHook
    // listener must not also dispatch the same virtual events.
    private static void SetInputSuspended(bool suspended) {
        KeyViewer.Instance.SetReplayInput(suspended);
    }

    private static void RestoreRainDepths() {
        foreach(RectTransform transform in RainTransforms) {
            Vector3 position = transform.localPosition;
            position.z = 0f;
            transform.localPosition = position;
        }
        RainTransforms.Clear();
    }
}
