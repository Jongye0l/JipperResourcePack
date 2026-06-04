using System.Threading;
using JALib.Tools;
using JipperResourcePack.Async;
using UnityEngine;
using UnityEngine.UI;

namespace JipperResourcePack.KeyViewerContents;

public class Key(GameObject gameObject) {
    public readonly GameObject GameObject = gameObject;
    public AsyncText Text;
    public Image Background;
    public Image Outline;
    public AsyncText Value;
    public int Color;
    public int SiblingIndex;
    public RainPool RainPool;
    public RawRain LastRain;
    public RawRain LastGhostRain;
    private int _updateRequested;
    private bool _requestEnabled;
    private bool _currentEnabled;

    public void UpdateRequestKey(bool enabled) {
        _requestEnabled = enabled;
        if(Interlocked.Increment(ref _updateRequested) == 1) MainThread.Run(Main.Instance, () => UpdateKey());
    }

    public void UpdateKey(bool force = false) {
        int current;
        do {
            current = Volatile.Read(ref _updateRequested);
            if(current == 0) return;
            bool request = _requestEnabled;
            if(force || request != _currentEnabled) {
                KeyViewerSetting settings = KeyViewer.Settings;
                Background.color = request ? settings.BackgroundClicked : settings.Background;
                Outline.color = request ? settings.OutlineClicked : settings.Outline;
                Value?.TMP.color = Text.TMP.color = request ? settings.TextClicked : settings.Text;
                _currentEnabled = request;
            }
        } while(Interlocked.CompareExchange(ref _updateRequested, 0, current) != current);
    }
}