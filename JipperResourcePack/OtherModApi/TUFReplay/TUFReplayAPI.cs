using System;
using System.Collections.Generic;
using System.Reflection;
using JipperResourcePack.KeyViewerContents;
using JipperResourcePack.OtherModApi.TUFReplay.DTO;
using JipperResourcePack.OverlayContents;
using UnityEngine;

namespace JipperResourcePack.OtherModApi.TUFReplay;

// Optional bridge to TUFReplay's ReplayApi. TUFReplay hands its replays to JRP
// the same way whether it plays a run in the game or renders it to video.
//
// No compile-time reference to TUFReplay is required.
public static partial class TUFReplayAPI {
    public const string ProviderId = "JipperResourcePack";

    public static bool ReplayActive;
    private static bool _registered;
    private static int _apiVersion;

    private static Func<string, object, bool> _register;
    private static Action<string> _unregister;
    private static Func<object, bool> _addEvent;
    private static Func<bool> _isRecording;

    private static Provider _provider;
    private static long _replayTicks;
    private static readonly HashSet<RectTransform> RainTransforms = new();

    // What the replayed run showed as it started. Null when TUFReplay saved nothing for it.
    internal static RunState Recorded;

    public static bool TryGetReplayTicks(out long ticks) {
        ticks = _replayTicks;
        return ReplayActive;
    }

    public static void TryRegister() {
        if(_registered) return;
        try {
            if(!Resolve()) return;
            CheckProvider();
            if(_register(ProviderId, _provider)) {
                _registered = true;
                Main.Instance.Log("TUFReplay ReplayApi is loaded.");
            }
        } catch(Exception e) {
            Main.Instance.Log("TUFReplay ReplayApi is not loaded.");
            Main.Instance.LogException(e);
        }
    }

    private static void CheckProvider() {
        if(_provider != null && _provider.CheckUsableAPIVersion(_apiVersion)) return;
        _provider = _apiVersion >= 2 ? new ProviderV2() : new ProviderV1();
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
            Recorded = null;
            _replayTicks = 0;
        }
    }

    // Adds a change made during a run, such as new key viewer settings, at this moment of the run.
    // TUFReplay hands it back at the same moment of the replay, through the Provider's
    // OnReplayEvent overload for its type. Each kind of change is its own class:
    //
    //   internal sealed class KeyViewerStyleChanged { public KeyviewerStyle Style; }
    //   public void OnReplayEvent(KeyViewerStyleChanged e, long ticks) { ... }   // on Provider
    //   if(TUFReplayAPI.IsRecording()) TUFReplayAPI.AddEvent(new KeyViewerStyleChanged { Style = ... });
    //
    // Outside a recorded run the event is dropped. So is an event of a type Provider has no
    // OnReplayEvent for, and TUFReplay writes that to its log.
    public static void AddEvent(object e) => _addEvent?.Invoke(e);

    // True while TUFReplay records a run, which is exactly when AddEvent keeps an event, so
    // check it before building one. Always false without TUFReplay's ReplayApi 2.
    public static bool IsRecording() => _isRecording != null && _isRecording();

    private static bool Resolve() {
        if(_register != null) return true;
        foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            Type type = assembly.GetType("TUFReplay.ReplayApi", false);
            FieldInfo versionField = type?.GetField("ApiVersion", BindingFlags.Public | BindingFlags.Static);
            if(versionField?.GetValue(null) is not int version) continue;

            _apiVersion = version;

            if(version < 1) {
                Main.Instance.Warning($"TUFReplay ReplayApi version {version} is too old, need 1 or higher. Skipping.");
                continue;
            }

            MethodInfo register = type.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
            MethodInfo unregister = type.GetMethod("Unregister", BindingFlags.Public | BindingFlags.Static);
            if((object) register == null || (object) unregister == null) {
                Main.Instance.Warning($"TUFReplay ReplayApi version {version} is missing Register or Unregister method. Skipping.");
                continue;
            }

            _register = (Func<string, object, bool>) Delegate.CreateDelegate(typeof(Func<string, object, bool>), register);
            _unregister = (Action<string>) Delegate.CreateDelegate(typeof(Action<string>), unregister);

            if(version < 2) return true;

            MethodInfo isRecording = type.GetProperty("IsRecording", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
            if(isRecording?.ReturnType == typeof(bool)) _isRecording = (Func<bool>) Delegate.CreateDelegate(typeof(Func<bool>), isRecording);
            else Main.Instance.Warning($"TUFReplay ReplayApi version {version} is missing IsRecording, so no events are added.");

            MethodInfo getEventStore = type.GetMethod("GetEventStore", BindingFlags.Public | BindingFlags.Static);
            if((object) getEventStore == null) {
                Main.Instance.Warning($"TUFReplay ReplayApi version {version} is missing GetEventStore method.");
                return true;
            }

            object eventStore = getEventStore.Invoke(null, new object[] { ProviderId });
            MethodInfo addEvent = eventStore?.GetType().GetMethod("AddEvent", BindingFlags.Public | BindingFlags.Instance);
            if((object) addEvent == null) {
                Main.Instance.Warning($"TUFReplay ReplayApi version {version} is missing AddEvent method on its event store.");
                return true;
            }

            _addEvent = (Func<object, bool>) Delegate.CreateDelegate(typeof(Func<object, bool>), eventStore, addEvent);
            return true;
        }
        return false;
    }

    // TUFReplay looks these methods up by name on the registered object.
    public abstract class Provider {
        public abstract bool CheckUsableAPIVersion(int version);

        // TUFReplay calls this when it starts recording a run's input, at the countdown or
        // checkpoint, and saves it as the run's start value. Changes made later in the run
        // are added with AddEvent.
        public RunState CaptureRunState() => RunState.Capture();

        // In the game and in videos alike: the replay's keys come only through
        // OnReplayKey, so the live SkyHook listener stops and counts start from a copy
        // of the player's until OnReplayState hands over the run's own.
        // public void OnReplayBegin(bool render);

        // Right after OnReplayBegin, before the level starts, with the run's start value
        // (null when it has none), and again after a timeline seek, which starts the run
        // over. The overlay reads the attempts and best when the level starts; the key
        // viewer counts on from the value's key counts.
        // public void OnReplayState(RunState state, long ticks);

        public void OnReplayEnd() {
            RestoreRainDepths();
            SetInputSuspended(false);
            ReplayActive = false;
            Recorded = null;
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
