using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using JipperResourcePack.Async;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents;

public partial class KeyViewer {
    private readonly bool[] _keyState = new bool[GhostOutIndex];
    public Queue<long> PressTimes;
    public int KpsCount;
    public int LastKpsCount;
    public int LastTotalCount;
    private long _lastUpdateMillis;
    private int _saveRepeat;
    
    private void ListenKey() {
        try {
            while(KeyInputListener is { IsAlive: true } && Enabled) {
                try {
                    long currentMillis = Stopwatch.ElapsedMilliseconds;
                    while(currentMillis == _lastUpdateMillis) {
                        Thread.Yield();
                        currentMillis = Stopwatch.ElapsedMilliseconds;
                    }
                    _lastUpdateMillis = currentMillis;
                    Work(currentMillis, false);
                } catch (ThreadAbortException) {
                    return;
                } catch (Exception e) {
                    if(KeyInputListener is not { IsAlive: true }) return;
                    Main.Instance.LogException(e);
                }
            }
        } catch (ThreadAbortException) {
        } catch (Exception e) {
            if(KeyInputListener is not { IsAlive: true }) return;
            Main.Instance.LogException(e);
        }
    }

    private void Work(long currentMillis, bool skipSave) {
        KeyViewerSettings settings = Settings;
        KeyCode[] keyCodes = GetKeyCode();
        for(int i = 0; i < keyCodes.Length; i++) {
            bool current = CheckKey(keyCodes[i]);
            Key key = Keys[i];
            if(key == null || current == _keyState[i]) continue;
            _keyState[i] = current;
            UpdateKey(i, current);
            if(!current) {
                key.LastRain?.Finish(currentMillis);
                continue;
            }
            if(i == 9 && settings.KeyViewerStyle == KeyviewerStyle.Key10) i = 10;
            key.Value.Text = (++settings.Count[i]).ToString();
            settings.TotalCount++;
            PressTimes.Enqueue(currentMillis);
            if(settings.useRain) {
                RawRain rawRain = key.LastRain = RawRain.GetOrNewRawRain(key, currentMillis, false);
                RainManager.RawRainQueue.Enqueue(rawRain);
            }
            _save = true;
        }
        keyCodes = GetFootKeyCode();
        for(int i = 0; i < keyCodes.Length; i++) {
            bool current = CheckKey(keyCodes[i]);
            int index = i + HandOutIndex;
            Key key = Keys[index];
            if(key == null || current == _keyState[index]) continue;
            _keyState[index] = current;
            UpdateKey(index, current);
            if(!current) continue;
            PressTimes.Enqueue(currentMillis);
            settings.Count[index]++;
            settings.TotalCount++;
            _save = true;
        }
        if(settings.useRain && settings.useGhostRain) {
            keyCodes = GetGhostKeyCode();
            for(int i = 0; i < keyCodes.Length; i++) {
                bool current = CheckKey(keyCodes[i]);
                Key key = Keys[i];
                if(key == null) continue;
                int index = i + FootOutIndex;
                if(current == _keyState[index]) continue;
                _keyState[index] = current;
                if(!current) {
                    key.LastGhostRain?.Finish(currentMillis);
                } else {
                    RawRain rawRain = key.LastGhostRain = RawRain.GetOrNewRawRain(key, currentMillis, true);
                    RainManager.RawRainQueue.Enqueue(rawRain);
                }
            }
        }
        while(PressTimes.TryPeek(out long result)) {
            if(currentMillis - result > 1000)
                PressTimes.Dequeue();
            else break;
        }
        KpsCount = PressTimes.Count;
        if(skipSave || ++_saveRepeat < 1000 || !_save || !Enabled) return;
        Main.Instance.SaveSetting();
        _save = false;
        _saveRepeat = 0;
    }

    private void UpdateKey(int i, bool enabled) {
        Key key = Keys[i];
        KeyViewerSettings settings = Settings;
        key.Background.Color = enabled ? settings.BackgroundClicked : settings.Background;
        key.Outline.Color = enabled ? settings.OutlineClicked : settings.Outline;
        key.Text.Color = enabled ? settings.TextClicked : settings.Text;
        key.Value?.Color = key.Text.Color;
    }

    public class KeyViewerUpdater : MonoBehaviour {
        private void Update() {
            int kpsCount = Instance.KpsCount;
            if(kpsCount != Instance.LastKpsCount) {
                Instance.LastKpsCount = kpsCount;
                Instance.Kps.Value.TMP.text = kpsCount.ToString();
            }
            int totalCount = Settings.TotalCount;
            if(totalCount != Instance.LastTotalCount) {
                Instance.LastTotalCount = totalCount;
                Instance.Total.Value.TMP.text = totalCount.ToString();
            }
            long currentMillis = Stopwatch.ElapsedMilliseconds;
            if(currentMillis <= Instance._lastUpdateMillis) return;
            if(currentMillis - Instance._lastUpdateMillis > 1) Main.Instance.Log("Listen Delay: " + (currentMillis - Instance._lastUpdateMillis)); // TODO: Remove this
            Instance._lastUpdateMillis = currentMillis;
            Instance.Work(currentMillis, true);
        }
    }
}