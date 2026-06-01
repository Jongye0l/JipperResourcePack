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
}