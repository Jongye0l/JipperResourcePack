using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using HarmonyLib;
using JALib.Core.Patch;
using SkyHook;
using UnityEngine;
using EventType = SkyHook.EventType;

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
    private static HashSet<KeyCode> _unityKeyLimitKeys;
    private static HashSet<ushort> _asyncKeyLimitKeys;

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
                    KeyLabel label = VersionSafe.UnityKeyToSkyHookKey(keyCode);
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
    
    [JAPatch(typeof(RDInputType_Keyboard), nameof(RDInputType_Keyboard.MainIgnoreActive), PatchType.Transpiler, false)]
    private static IEnumerable<CodeInstruction> KeyLimitPatch(IEnumerable<CodeInstruction> instructions) {
        List<CodeInstruction> codes = new(instructions);
        for(int i = 0; i < codes.Count; i++) {
            CodeInstruction code = codes[i];
            if(code.operand is FieldInfo { Name: nameof(Persistence.keyLimiterKeys) }) {
                code.operand = typeof(KeyViewer).GetField(nameof(_unityKeyLimitKeys), BindingFlags.NonPublic | BindingFlags.Static);
                codes.RemoveAt(i + 1);
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
                codes[i + 3].labels.AddRange(codes[i].labels);
                codes.RemoveRange(i, 3);
            } else if(code.operand is MethodInfo { Name: "get_" + nameof(RDInput.useKeyLimiter) }) {
                LocalBuilder local = (LocalBuilder) codes[i - 1].operand;
                codes[i].opcode = OpCodes.Ldloc;
                codes[i++].operand = local;
                codes.Insert(i++, new CodeInstruction(OpCodes.Call, typeof(KeyViewer).GetMethod(nameof(IsLimitedKey), BindingFlags.NonPublic | BindingFlags.Static)));
                int j = i;
                while(true) {
                    CodeInstruction cur = codes[++j];
                    if(cur.opcode == OpCodes.Brfalse || cur.opcode == OpCodes.Brfalse_S) break;
                }
                codes.RemoveRange(i, j - i);
            }
        }
        return codes;
    }

    private static bool IsLimitedKey(AsyncKeyCode keyCode) {
        try {
            if(!RDInput.useKeyLimiter) return true;

            if(Settings.AutoSetupKeyLimit) {
                if(keyCode.label != KeyLabel.Unknown) {
                    foreach(KeyCode unityKeyLimitKey in _unityKeyLimitKeys) {
                        KeyLabel label = VersionSafe.UnityKeyToSkyHookKey(unityKeyLimitKey);
                        if(label == keyCode.label) return true;
                    }
                }
                return _asyncKeyLimitKeys.Contains(keyCode.key);
            }
        } catch (Exception e) {
            Main.Instance.LogReportException(e);
        }

        HashSet<ushort> asyncKeysCache = Persistence.keyLimiterKeys.asyncKeysCache;
        return asyncKeysCache.Count <= 0 || asyncKeysCache.Contains(keyCode.key);
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
            } catch (ThreadAbortException) {
                // Handle thread abort if necessary
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
            KeyCode capturedKey = VersionSafe.SkyHookKeyToUnityKey(keyEvent.Label);
            _capturedKeyCode = capturedKey switch {
                KeyCode.None => keyEvent.Key + 0x1000,
                KeyCode.Less => (int) KeyCode.Comma,
                KeyCode.Greater => (int) KeyCode.Equals,
                KeyCode.Pipe => (int) KeyCode.Backslash,
                _ => (int) capturedKey
            };
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
