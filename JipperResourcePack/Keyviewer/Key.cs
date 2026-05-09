using System.Collections.Concurrent;
using System.Collections.Generic;
using JipperResourcePack.Async;
using UnityEngine;

namespace JipperResourcePack.Keyviewer;

public class Key : MonoBehaviour {
    public readonly List<RawRain> RainList = [];
    public readonly List<RawRain> GhostRainList = [];
    public AsyncText text;
    public AsyncImage background;
    public AsyncImage outline;
    public AsyncText value;
    public int color;
    public int siblingIndex;
    public RainPool RainPool;
    public ConcurrentQueue<RawRain> RawRainQueue = new();
    
    private void Update() {
        while(RawRainQueue.TryDequeue(out RawRain rawRain)) {
            Rain rainComponent = RainPool.GetOrNewRain(rawRain.IsGhost);
            rainComponent.image.color = color switch {
                1 => KeyViewer.Settings.RainColor,
                3 => KeyViewer.Settings.RainColor3,
                _ => KeyViewer.Settings.RainColor2
            };
            rainComponent.RawRain = rawRain;
            rainComponent.transform.SetSiblingIndex(rawRain.IsGhost ? siblingIndex + 1 : siblingIndex);
        }
    }

    private void OnDestroy() {
        RawRainQueue = null;
    }
}