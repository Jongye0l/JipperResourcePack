using System;
using System.Collections.Generic;
using System.Reflection;
using JipperResourcePack.KeyViewerContents;
using JipperResourcePack.OverlayContents;
using UnityEngine;

namespace JipperResourcePack.OtherModApi;

// Optional bridge to TUFReplay's ReplayApi. TUFReplay hands its replays to JRP
// the same way whether it plays a run in the game or renders it to video.
//
// No compile-time reference to TUFReplay is required.
public static class TUFReplayAPI {
    public const string ProviderId = "JipperResourcePack";
    public const int RequiredApiVersion = 1;

    public static bool ReplayActive;
    private static bool _registered;

    private static Func<string, object, bool> _register;
    private static Action<string> _unregister;

    private static Provider _provider;
    private static long _replayTicks;
    private static readonly HashSet<RectTransform> RainTransforms = new();

    public static bool TryGetReplayTicks(out long ticks) {
        ticks = _replayTicks;
        return ReplayActive;
    }

    public static void TryRegister() {
        if(_registered) return;
        try {
            if(!Resolve()) return;
            _provider ??= new Provider();
            if(_register(ProviderId, _provider)) {
                _registered = true;
                Main.Instance.Log("TUFReplay ReplayApi is loaded.");
            }
        } catch(Exception e) {
            Main.Instance.Log("TUFReplay ReplayApi is not loaded.");
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
            ReplayActive = false;
            _replayTicks = 0;
        }
    }

    private static bool Resolve() {
        if(_register != null) return true;
        foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            Type type = assembly.GetType("TUFReplay.ReplayApi", false);
            FieldInfo versionField = type?.GetField("ApiVersion", BindingFlags.Public | BindingFlags.Static);
            if(versionField?.GetValue(null) is not int version || version < RequiredApiVersion) continue;

            MethodInfo register = type.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
            MethodInfo unregister = type.GetMethod("Unregister", BindingFlags.Public | BindingFlags.Static);
            if((object) register == null || (object) unregister == null) continue;

            _register = (Func<string, object, bool>) Delegate.CreateDelegate(typeof(Func<string, object, bool>), register);
            _unregister = (Action<string>) Delegate.CreateDelegate(typeof(Action<string>), unregister);
            return true;
        }
        return false;
    }

    // TUFReplay looks these methods up by name on the registered object.
    private sealed class Provider {
        // In the game and in videos alike: the replay's keys come only through
        // OnReplayKey, so the live SkyHook listener stops and counts start from a copy.
        public void OnReplayBegin(bool render) {
            ReplayActive = true;
            _replayTicks = 0;
            SetInputSuspended(true);
        }

        public void OnReplayEnd() {
            RestoreRainDepths();
            SetInputSuspended(false);
            ReplayActive = false;
            _replayTicks = 0;
        }

        public void OnReplayClock(long ticks) => _replayTicks = ticks;

        public void OnReplayKey(KeyCode key, int? nativeKey, bool down, long ticks) {
            KeyViewer.Instance.ApplyReplayKey(key, nativeKey, down, ticks);
        }

        // Asked only while a replay is rendered to video.
        public IEnumerable<Canvas> GetReplayCanvases() {
            Overlay overlay = Overlay.Instance;
            if(overlay.Canvas) yield return overlay.Canvas;
            KeyViewer keyViewer = KeyViewer.Instance;
            if(!keyViewer.KeyViewerObject) yield break;
            Canvas canvas = keyViewer.KeyViewerObject.GetComponent<Canvas>();
            yield return canvas;
        }

        public void OnBeforeFrameCapture() {
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

        public void OnAfterFrameCapture() => RestoreRainDepths();
    }

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
