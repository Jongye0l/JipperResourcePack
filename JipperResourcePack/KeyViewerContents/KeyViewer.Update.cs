using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents;

public partial class KeyViewer {
    private readonly bool[] _keyState = new bool[GhostOutIndex];
    private Queue<long> _pressTimes;
    private int _kpsCount;
    private int _lastKpsCount;
    private int _lastTotalCount;
    private long _lastUpdateMillis;
    
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
        KeyViewerSetting settings = Settings;
        KeyCountData countData = KeyCountData.Instance;
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
            key.Value.Text = (++countData.Count[i]).ToString();
            countData.TotalCount++;
            _pressTimes.Enqueue(currentMillis);
            if(settings.useRain) {
                RawRain rawRain;
                lock(key) {
                    if(key.LastRain?.FinishSize == false) rawRain = null;
                    else rawRain = key.LastRain = RawRain.GetOrNewRawRain(key, currentMillis, false);
                }
                if(rawRain != null) RainManager.RawRainQueue.Enqueue(rawRain);
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
            _pressTimes.Enqueue(currentMillis);
            countData.Count[index]++;
            countData.TotalCount++;
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
                    RawRain rawRain;
                    lock(key) {
                        if(key.LastGhostRain?.FinishSize == false) rawRain = null;
                        else rawRain = key.LastGhostRain = RawRain.GetOrNewRawRain(key, currentMillis, true);
                    }
                    if(rawRain != null) RainManager.RawRainQueue.Enqueue(rawRain);
                }
            }
        }
        while(_pressTimes.TryPeek(out long result)) {
            if(currentMillis - result > 1000)
                _pressTimes.Dequeue();
            else break;
        }
        _kpsCount = _pressTimes.Count;
        if(skipSave || !_save || !Enabled) return;
        KeyCountData.Instance.Save();
        _save = false;
    }

    private void UpdateKey(int i, bool enabled) {
        Key key = Keys[i];
        KeyViewerSetting settings = Settings;
        key.Background.Color = enabled ? settings.BackgroundClicked : settings.Background;
        key.Outline.Color = enabled ? settings.OutlineClicked : settings.Outline;
        key.Text.Color = enabled ? settings.TextClicked : settings.Text;
        key.Value?.Color = key.Text.Color;
    }

    public class KeyViewerUpdater : MonoBehaviour {
        private void Update() {
            int kpsCount = Instance._kpsCount;
            if(kpsCount != Instance._lastKpsCount) {
                Instance._lastKpsCount = kpsCount;
                Instance.Kps.Value.TMP.text = kpsCount.ToString();
            }
            int totalCount = KeyCountData.Instance.TotalCount;
            if(totalCount != Instance._lastTotalCount) {
                Instance._lastTotalCount = totalCount;
                Instance.Total.Value.TMP.text = totalCount.ToString();
            }
            long currentMillis = Stopwatch.ElapsedMilliseconds;
            int delay = (int) (Instance._lastUpdateMillis - currentMillis);
            if(delay < 5) return;
            Main.Instance.Log("Key Listen Work in MainThread (delay:" + delay + "ms)");
            Instance._lastUpdateMillis = currentMillis;
            Instance.Work(currentMillis, true);
        }
    }
}