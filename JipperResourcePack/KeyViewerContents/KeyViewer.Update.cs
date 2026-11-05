using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using HarmonyLib;
using JALib.Core.Patch;
using JipperResourcePack.KeyViewerContents.KeyMapper;
using SkyHook;
using UnityEngine;
using EventType = SkyHook.EventType;

namespace JipperResourcePack.KeyViewerContents;

public partial class KeyViewer {
    private const long NoEventOffset = long.MinValue >> 1;
    private static readonly bool StopwatchTicksAreDateTimeTicks = System.Diagnostics.Stopwatch.Frequency == TimeSpan.TicksPerSecond;

    public static ulong CurrentTicks {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => JipperResourcePack.OtherModApi.OrbitRenderAPI.TryGetReplayTicks(out long replayTicks)
            ? (ulong) Math.Max(0, replayTicks)
            : (ulong) (StopwatchTicksAreDateTimeTicks ? Stopwatch.ElapsedTicks : Stopwatch.Elapsed.Ticks);
    }

    private readonly bool[] _keyState = new bool[GhostOutIndex];
    private ConcurrentQueue<ulong> _pressTimes;
    private ConcurrentQueue<KeyEvent> _eventQueue;
    private SemaphoreSlim _eventSignal;
    private volatile bool _listening;
    private Thread _eventThread;
    private KeyCountData _replayCounts;
    private FrozenDictionary<int, int[]> _keyBinding;
    private long _eventOffsetTicks = NoEventOffset;
    private volatile int _capturedKeyCode;
    private int _lastKpsCount;
    private int _lastTotalCount;
    private static HashSet<KeyCode> _unityKeyLimitKeys = [];
    private static HashSet<ushort> _asyncKeyLimitKeys = [];

    private void RebuildKeyBinding() {
        Dictionary<int, List<int>> indexMap = new();
        AddKeyCodes(indexMap, GetKeyCode(), 0);
        AddKeyCodes(indexMap, GetFootKeyCode(), HandOutIndex);
        AddKeyCodes(indexMap, GetGhostKeyCode(), FootOutIndex);

        Dictionary<int, int[]> keyBinding = new(indexMap.Count);
        foreach(KeyValuePair<int, List<int>> pair in indexMap) keyBinding[pair.Key] = pair.Value.ToArray();
        _keyBinding = keyBinding.ToFrozenDictionary();
        return;

        void AddKeyCodes(Dictionary<int, List<int>> keyIndexMap, KeyCode[] keyCodes, int offset) {
            for(int i = 0; i < keyCodes.Length; i++) {
                KeyCode keyCode = keyCodes[i];
                if(keyCode == KeyCode.None) continue;
                int index = i + offset;
                if(!keyIndexMap.TryGetValue((int) keyCode, out List<int> list)) keyIndexMap[(int) keyCode] = list = [];
                list.Add(index);
            }
        }
    }
    
    [JAPatch(typeof(RDInputType_Keyboard), nameof(RDInputType_Keyboard.MainIgnoreActive), PatchType.Transpiler, false)]
    private static IEnumerable<CodeInstruction> KeyLimitPatch(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = new(instructions);
        for(int i = 0; i < codes.Count; i++) {
            CodeInstruction code = codes[i];
            if(code.operand is FieldInfo { Name: nameof(Persistence.keyLimiterKeys) }) {
                // Keep the game's own key set load and pick between it and ours right after it.
                codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, typeof(KeyViewer).GetMethod(nameof(SelectUnityKeyLimitKeys), BindingFlags.NonPublic | BindingFlags.Static)));
            }
        }
        return codes;
    }
    
    [JAPatch(typeof(RDInputType_AsyncKeyboard), nameof(RDInputType_AsyncKeyboard.Main), PatchType.Transpiler, false)]
    private static IEnumerable<CodeInstruction> AsyncKeyLimitPatch(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = new(instructions);
        for(int i = 0; i < codes.Count; i++) {
            CodeInstruction code = codes[i];
            if(code.operand is FieldInfo { Name: nameof(Persistence.keyLimiterKeys) }) {
                codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, typeof(KeyViewer).GetMethod(nameof(SelectAsyncKeyLimitKeys), BindingFlags.NonPublic | BindingFlags.Static)));
            }
        }
        return codes;
    }

    private static HashSet<KeyCode> SelectUnityKeyLimitKeys(HashSet<KeyCode> manualKeys) => Settings.AutoSetupKeyLimit ? _unityKeyLimitKeys : manualKeys;

    private static HashSet<ushort> SelectAsyncKeyLimitKeys(HashSet<ushort> manualKeys) => Settings.AutoSetupKeyLimit ? _asyncKeyLimitKeys : manualKeys;

    private void StartEventListener() {
        _eventQueue = new ConcurrentQueue<KeyEvent>();
        _eventSignal = new SemaphoreSlim(0);
        _listening = true;
        _eventThread = new Thread(ProcessKeyEvents) {
            Name = "JipperResourcePack KeyViewer Input Thread",
            IsBackground = true
        };
        _eventThread.Start();
        SkyHookManager.KeyUpdated.AddListener(OnKeyEvent);
    }

    private void StopEventListener() {
        SkyHookManager.KeyUpdated.RemoveListener(OnKeyEvent);
        _listening = false;
        _eventSignal?.Release();
        _eventThread?.Join();
        _eventThread = null;
        _eventQueue = null;
        _eventSignal = null;
    }

    // OrbitRender feeds recorded input directly while a replay is rendered.
    internal void SetReplayInput(bool replay) {
        if(replay) {
            if(_replayCounts != null || !Active || KeyCountData.Instance == null) return;
            if(_listening) StopEventListener();
            ResetReplayKeys();
            _replayCounts = new KeyCountData();
            Array.Copy(KeyCountData.Instance.Count, _replayCounts.Count, _replayCounts.Count.Length);
            _replayCounts.TotalCount = KeyCountData.Instance.TotalCount;
            return;
        }
        if(_replayCounts == null) return;
        ResetReplayKeys();
        _replayCounts = null;
        for(int i = 0; i < HandOutIndex; i++) {
            int countIndex = i == 9 && Settings.KeyViewerStyle == KeyviewerStyle.Key10 ? 10 : i;
            if(Keys[i]?.Value != null) Keys[i].Value.Text = KeyCountData.Instance.Count[countIndex].ToString();
        }
        _lastTotalCount = -1;
        _eventOffsetTicks = NoEventOffset;
        if(Enabled && !_listening) StartEventListener();
    }

    private void ResetReplayKeys() {
        foreach(Key key in Keys) key?.SetEnabled(false);
        Array.Clear(_keyState, 0, _keyState.Length);
        ResetRain();
        _pressTimes?.Clear();
        _lastKpsCount = -1;
    }

    // Called on Unity's main thread by OrbitRender.OverlayApi.
    internal void ApplyReplayKey(KeyCode key, int? nativeKey, bool pressed, long ticks) {
        FrozenDictionary<int, int[]> binding = _keyBinding;
        if(_replayCounts == null || binding == null || !Enabled) return;
        // Match the live-input path, including native keys on Korean keyboards.
        if(nativeKey.HasValue) {
            ushort native = (ushort) nativeKey.Value;
            if(KeyCodeMapper.TryInit() && KeyCodeMapper.Instance.TryGetCustomKeyCode(native, out KeyCode mapped)) key = mapped;
            if(key == KeyCode.None) key = (KeyCode) (native + 0x1000);
        }
        if(binding.TryGetValue((int) key, out int[] indexes)) Work(indexes, pressed, (ulong) Math.Max(0, ticks));
    }

    private void OnKeyEvent(SkyHookEvent hookEvent) {
        ConcurrentQueue<KeyEvent> queue = _eventQueue;
        SemaphoreSlim signal = _eventSignal;
        if(queue == null || signal == null) return;
        queue.Enqueue(new KeyEvent(hookEvent.Label, hookEvent.Key, hookEvent.Type, ToStopwatchTicks(hookEvent.GetTimeInTicks())));
        signal.Release();
    }

    private void ProcessKeyEvents() {
        ConcurrentQueue<KeyEvent> queue = _eventQueue;
        SemaphoreSlim signal = _eventSignal;
        while(_listening) {
            try {
                signal.Wait();
                while(_listening && queue.TryDequeue(out KeyEvent keyEvent)) ProcessKeyEvent(keyEvent);
            } catch (ThreadAbortException) {
                // Handle thread abort if necessary
            } catch (Exception e) {
                if(!_listening) return;
                Main.Instance.LogException(e);
            }
        }
    }

    private void ProcessKeyEvent(KeyEvent keyEvent) {
        FrozenDictionary<int, int[]> binding = _keyBinding;
        if(binding == null || !Enabled) return;
        bool pressed = keyEvent.EventType == EventType.KeyPressed;

        if(!KeyCodeMapper.TryInit() || !KeyCodeMapper.Instance.TryGetCustomKeyCode(keyEvent.Key, out KeyCode tempKey))
            tempKey = VersionSafe.SkyHookKeyToUnityKey(keyEvent.Label);

        int capturedKey = tempKey == KeyCode.None ? keyEvent.Key + 0x1000 : (int) tempKey;
        if(pressed && _selectedKey != -1 && _changeState != 1) _capturedKeyCode = capturedKey;
        bool counted = false;

        if(binding.TryGetValue(capturedKey, out int[] indexes)) counted |= Work(indexes, pressed, keyEvent.Ticks);
        if(counted && Enabled) KeyCountData.Instance.Save();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong ToStopwatchTicks(long eventTicks) {
        ulong now = CurrentTicks;
        ulong currentTicks = (ulong) (eventTicks - _eventOffsetTicks);
        if(currentTicks <= now) {
            if(now - currentTicks < 8192) return currentTicks;
            ulong delta = Math.Min(1024, (now - currentTicks) >> 8); // 1/256 second
            _eventOffsetTicks -= (long) delta;
            return currentTicks + delta;
        }
        _eventOffsetTicks = eventTicks - (long) now;
        return now;
    }

    private bool Work(int[] indexes, bool pressed, ulong currentTicks) {
        KeyViewerSetting settings = Settings;
        KeyCountData countData = _replayCounts ?? KeyCountData.Instance;
        bool counted = false;
        foreach(int index in indexes) {
            if(index >= FootOutIndex) {
                if(!settings.useRain || !settings.useGhostRain) continue;
                Key ghostKey = Keys[index - FootOutIndex];
                if(ghostKey == null || _keyState[index] == pressed) continue;
                _keyState[index] = pressed;
                if(!pressed) {
                    ghostKey.LastGhostRain?.Finish(currentTicks);
                    continue;
                }
                RawRain ghostRain = ghostKey.LastGhostRain = RawRain.GetOrNewRawRain(ghostKey, currentTicks, true);
                RainManager.RawRainQueue.Enqueue(ghostRain);
                continue;
            }
            Key key = Keys[index];
            if(key == null || _keyState[index] == pressed) continue;
            _keyState[index] = pressed;
            key.UpdateRequestKey(pressed);
            if(index >= HandOutIndex) {
                if(!pressed) continue;
                countData.Count[index]++;
                countData.TotalCount++;
                _pressTimes?.Enqueue(currentTicks);
                counted = true;
                continue;
            }
            if(!pressed) {
                key.LastRain?.Finish(currentTicks);
                continue;
            }
            int countIndex = index == 9 && settings.KeyViewerStyle == KeyviewerStyle.Key10 ? 10 : index;
            key.Value.Text = (++countData.Count[countIndex]).ToString();
            countData.TotalCount++;
            _pressTimes?.Enqueue(currentTicks);
            counted = true;
            if(!settings.useRain) continue;
            RawRain rawRain = key.LastRain = RawRain.GetOrNewRawRain(key, currentTicks, false);
            RainManager.RawRainQueue.Enqueue(rawRain);
        }
        return counted;
    }

    private readonly struct KeyEvent(KeyLabel label, ushort key, EventType eventType, ulong ticks) {
        public readonly KeyLabel Label = label;
        public readonly ushort Key = key;
        public readonly EventType EventType = eventType;
        public readonly ulong Ticks = ticks;
    }

    public class KeyViewerUpdater : MonoBehaviour {
        private void Update() {
            KeyViewer instance = Instance;
            ConcurrentQueue<ulong> pressTimes = instance._pressTimes;
            if(pressTimes == null) return;
            ulong currentTicks = CurrentTicks;
            while(pressTimes.TryPeek(out ulong result) && currentTicks - result > TimeSpan.TicksPerSecond) pressTimes.TryDequeue(out _);
            if(instance.Kps != null) {
                int kpsCount = pressTimes.Count;
                if(kpsCount != instance._lastKpsCount) {
                    instance._lastKpsCount = kpsCount;
                    instance.Kps.Value.TMP.text = kpsCount.ToString();
                }
            }
            if(instance.Total == null) return;
            int totalCount = (instance._replayCounts ?? KeyCountData.Instance).TotalCount;
            if(totalCount == instance._lastTotalCount) return;
            instance._lastTotalCount = totalCount;
            instance.Total.Value.TMP.text = totalCount.ToString();
        }
    }
}
