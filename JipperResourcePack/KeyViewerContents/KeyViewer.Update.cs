using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using SkyHook;
using UnityEngine;
using EventType = SkyHook.EventType;
using ThreadPriority = System.Threading.ThreadPriority;

namespace JipperResourcePack.KeyViewerContents;

public partial class KeyViewer {
    private const long NoEventOffset = long.MinValue >> 1;
    private static readonly bool StopwatchTicksAreDateTimeTicks = System.Diagnostics.Stopwatch.Frequency == TimeSpan.TicksPerSecond;

    public static long CurrentTicks {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => StopwatchTicksAreDateTimeTicks ? Stopwatch.ElapsedTicks : Stopwatch.Elapsed.Ticks;
    }

    private readonly bool[] _keyState = new bool[GhostOutIndex];
    private ConcurrentQueue<long> _pressTimes;
    private ConcurrentQueue<KeyEvent> _eventQueue;
    private SemaphoreSlim _eventSignal;
    private volatile bool _listening;
    private KeyBinding _keyBinding;
    private long _eventOffsetTicks = NoEventOffset;
    private volatile int _capturedKeyCode;
    private int _lastKpsCount;
    private int _lastTotalCount;

    private void RebuildKeyBinding() {
        if(Keys == null) return;
        Dictionary<KeyLabel, List<int>> labels = new();
        Dictionary<ushort, List<int>> natives = new();
        AddKeyCodes(GetKeyCode(), 0);
        AddKeyCodes(GetFootKeyCode(), HandOutIndex);
        AddKeyCodes(GetGhostKeyCode(), FootOutIndex);
        _keyBinding = new KeyBinding(ToArrayMap(labels), ToArrayMap(natives));
        return;

        void AddKeyCodes(KeyCode[] keyCodes, int offset) {
            for(int i = 0; i < keyCodes.Length; i++) {
                KeyCode keyCode = keyCodes[i];
                if(keyCode == KeyCode.None) continue;
                int index = i + offset;
                if((int) keyCode < 0x1000) {
                    KeyLabel label = SkyHookKeyMapper.UnityKeyToSkyHookKey(keyCode);
                    if(label == KeyLabel.Unknown) continue;
                    if(!labels.TryGetValue(label, out List<int> list)) labels[label] = list = [];
                    list.Add(index);
                } else {
                    ushort nativeKeyCode = (ushort) ((int) keyCode - 0x1000);
                    if(!natives.TryGetValue(nativeKeyCode, out List<int> list)) natives[nativeKeyCode] = list = [];
                    list.Add(index);
                }
            }
        }
    }

    private static Dictionary<T, int[]> ToArrayMap<T>(Dictionary<T, List<int>> map) {
        Dictionary<T, int[]> result = new(map.Count);
        foreach(KeyValuePair<T, List<int>> pair in map) result[pair.Key] = pair.Value.ToArray();
        return result;
    }

    private void StartEventListener() {
        _eventQueue = new ConcurrentQueue<KeyEvent>();
        _eventSignal = new SemaphoreSlim(0);
        _listening = true;
        new Thread(ProcessKeyEvents) {
            Name = "JipperResourcePack KeyViewer Input Thread",
            IsBackground = true
        }.Start();
        SkyHookManager.KeyUpdated.AddListener(OnKeyEvent);
    }

    private void StopEventListener() {
        SkyHookManager.KeyUpdated.RemoveListener(OnKeyEvent);
        _listening = false;
        _eventSignal?.Release();
        _eventQueue = null;
        _eventSignal = null;
    }

    private void OnKeyEvent(SkyHookEvent hookEvent) {
        ConcurrentQueue<KeyEvent> queue = _eventQueue;
        SemaphoreSlim signal = _eventSignal;
        if(queue == null || signal == null) return;
        queue.Enqueue(new KeyEvent(hookEvent.Label, hookEvent.Key, hookEvent.Type == EventType.KeyPressed, ToStopwatchTicks(hookEvent.GetTimeInTicks())));
        signal.Release();
    }

    private void ProcessKeyEvents() {
        ConcurrentQueue<KeyEvent> queue = _eventQueue;
        SemaphoreSlim signal = _eventSignal;
        while(_listening) {
            try {
                signal.Wait();
                while(_listening && queue.TryDequeue(out KeyEvent keyEvent)) ProcessKeyEvent(keyEvent);
            } catch (Exception e) {
                if(!_listening) return;
                Main.Instance.LogException(e);
            }
        }
    }

    private void ProcessKeyEvent(KeyEvent keyEvent) {
        KeyBinding binding = _keyBinding;
        if(binding == null || !Enabled) return;
        bool pressed = keyEvent.Pressed;
        if(pressed && _selectedKey != -1 && _changeState != 1) {
            KeyCode capturedKey = SkyHookKeyMapper.SkyHookKeyToUnityKey(keyEvent.Label);
            _capturedKeyCode = capturedKey == KeyCode.None ? keyEvent.Key + 0x1000 : (int) capturedKey;
        }
        bool counted = false;
        if(binding.LabelMap.TryGetValue(keyEvent.Label, out int[] indexes)) counted = Work(indexes, pressed, keyEvent.Ticks);
        if(binding.NativeMap.TryGetValue(keyEvent.Key, out indexes)) counted |= Work(indexes, pressed, keyEvent.Ticks);
        if(counted && Enabled) KeyCountData.Instance.Save();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private long ToStopwatchTicks(long eventTicks) {
        long now = CurrentTicks;
        long currentTicks = eventTicks - _eventOffsetTicks;
        if(currentTicks < now) return currentTicks;
        _eventOffsetTicks = eventTicks - now;
        return now;
    }

    private bool Work(int[] indexes, bool pressed, long currentTicks) {
        KeyViewerSetting settings = Settings;
        KeyCountData countData = KeyCountData.Instance;
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

    private readonly struct KeyEvent(KeyLabel label, ushort key, bool pressed, long ticks) {
        public readonly KeyLabel Label = label;
        public readonly ushort Key = key;
        public readonly bool Pressed = pressed;
        public readonly long Ticks = ticks;
    }

    private sealed class KeyBinding(Dictionary<KeyLabel, int[]> labelMap, Dictionary<ushort, int[]> nativeMap) {
        public readonly Dictionary<KeyLabel, int[]> LabelMap = labelMap;
        public readonly Dictionary<ushort, int[]> NativeMap = nativeMap;
    }

    public class KeyViewerUpdater : MonoBehaviour {
        private void Update() {
            KeyViewer instance = Instance;
            ConcurrentQueue<long> pressTimes = instance._pressTimes;
            if(pressTimes == null) return;
            long currentTicks = CurrentTicks;
            while(pressTimes.TryPeek(out long result) && currentTicks - result > TimeSpan.TicksPerSecond) pressTimes.TryDequeue(out _);
            if(instance.Kps != null) {
                int kpsCount = pressTimes.Count;
                if(kpsCount != instance._lastKpsCount) {
                    instance._lastKpsCount = kpsCount;
                    instance.Kps.Value.TMP.text = kpsCount.ToString();
                }
            }
            if(instance.Total == null) return;
            int totalCount = KeyCountData.Instance.TotalCount;
            if(totalCount == instance._lastTotalCount) return;
            instance._lastTotalCount = totalCount;
            instance.Total.Value.TMP.text = totalCount.ToString();
        }
    }
}
