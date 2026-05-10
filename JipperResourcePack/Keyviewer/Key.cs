using System.Collections.Concurrent;
using JipperResourcePack.Async;
using UnityEngine;

namespace JipperResourcePack.Keyviewer;

public class Key : MonoBehaviour {
    public AsyncText text;
    public AsyncImage background;
    public AsyncImage outline;
    public AsyncText value;
    public int color;
    public int siblingIndex;
    public RainPool RainPool;
    public ConcurrentQueue<RawRain> RawRainQueue = new();
    public RawRain LastRain;
    public RawRain LastGhostRain;
    
    private void Update() {
        while(RawRainQueue.TryDequeue(out RawRain rawRain)) {
            Rain rainComponent = RainPool.GetOrNewRain(rawRain.IsGhost);
            rainComponent.Image.color = color switch {
                1 => KeyViewer.Settings.RainColor,
                3 => KeyViewer.Settings.RainColor3,
                _ => KeyViewer.Settings.RainColor2
            };
            rainComponent.RawRain = rawRain;
            rainComponent.Transform.SetSiblingIndex(rawRain.IsGhost ? siblingIndex + 1 : siblingIndex);
            KeyViewer.RainManager.RainList.Add(rainComponent);
        }
    }
}