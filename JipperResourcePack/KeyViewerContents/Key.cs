using System.Threading;
using JALib.Tools;
using JipperResourcePack.Async;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents;

public class Key(GameObject gameObject) {
    public readonly GameObject GameObject = gameObject;
    public AsyncText Text;
    public AsyncImage Background;
    public AsyncImage Outline;
    public AsyncText Value;
    public int Color;
    public int SiblingIndex;
    public RainPool RainPool;
    public RawRain LastRain;
    public RawRain LastGhostRain;
    private int _updateRequested;

    public void UpdateKey(bool enabled) {
        KeyViewerSetting settings = KeyViewer.Settings;
        Background.Color = enabled ? settings.BackgroundClicked : settings.Background;
        Outline.Color = enabled ? settings.OutlineClicked : settings.Outline;
        Value?.Color = Text.Color = enabled ? settings.TextClicked : settings.Text;
        if(Interlocked.Increment(ref _updateRequested) == 1) MainThread.Run(Main.Instance, UpdateColors);
    }

    private void UpdateColors() {
        Background.Image.color = Background.Color;
        Outline.Image.color = Outline.Color;
        Text.TMP.color = Text.Color;
        Value?.TMP.color = Value.Color;
    }
}