using System;
using System.Threading;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents;

public partial class KeyViewer {
    private readonly bool[] KeyState = new bool[GhostOutIndex];
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
            if(key == null || current == KeyState[i]) continue;
            KeyState[i] = current;
            UpdateKey(i, current);
            if(!current) {
                key.LastRain?.Finish(currentMillis);
                continue;
            }
            if(i == 9 && settings.KeyViewerStyle == KeyviewerStyle.Key10) i = 10;
            key.Value.Text = (++settings.Count[i]).ToString();
            Total.Value.Text = (++settings.TotalCount).ToString();
            PressTimes.Enqueue(currentMillis);
            if(settings.useRain) {
                RawRain rawRain = key.LastRain = new RawRain(key, currentMillis, false);
                RainManager.RawRainQueue.Enqueue(rawRain);
            }
            _save = true;
        }
        keyCodes = GetFootKeyCode();
        for(int i = 0; i < keyCodes.Length; i++) {
            bool current = CheckKey(keyCodes[i]);
            int index = i + HandOutIndex;
            Key key = Keys[index];
            if(key == null || current == KeyState[index]) continue;
            KeyState[index] = current;
            UpdateKey(index, current);
            if(!current) continue;
            PressTimes.Enqueue(currentMillis);
            settings.Count[index]++;
            Total.Value.Text = (++settings.TotalCount).ToString();
            _save = true;
        }
        if(settings.useRain && settings.useGhostRain) {
            keyCodes = GetGhostKeyCode();
            for(int i = 0; i < keyCodes.Length; i++) {
                bool current = CheckKey(keyCodes[i]);
                Key key = Keys[i];
                if(key == null) continue;
                int index = i + FootOutIndex;
                if(current == KeyState[index]) continue;
                KeyState[index] = current;
                if(!current) {
                    key.LastGhostRain?.Finish(currentMillis);
                } else {
                    RawRain rawRain = key.LastGhostRain = new RawRain(key, currentMillis, true);
                    RainManager.RawRainQueue.Enqueue(rawRain);
                }
            }
        }
        while(PressTimes.TryPeek(out long result)) {
            if(currentMillis - result > 1000)
                PressTimes.Dequeue();
            else break;
        }
        if(LastKps != PressTimes.Count) {
            LastKps = PressTimes.Count;
            Kps.Value.Text = LastKps.ToString();
        }
        if(++_saveRepeat < 100 || !_save || !Enabled || skipSave) return;
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
            long currentMillis = Stopwatch.ElapsedMilliseconds;
            if(currentMillis == Instance._lastUpdateMillis) return;
            if(currentMillis - Instance._lastUpdateMillis > 1) Main.Instance.Log("Listen Delay: " + (currentMillis - Instance._lastUpdateMillis)); // TODO: Remove this
            Instance._lastUpdateMillis = currentMillis;
            Instance.Work(currentMillis, true);
        }
    }
}