using JipperResourcePack.Async;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents;

public class Key {
    public GameObject GameObject;
    public AsyncText Text;
    public AsyncImage Background;
    public AsyncImage Outline;
    public AsyncText Value;
    public int Color;
    public int SiblingIndex;
    public RainPool RainPool;
    public RawRain LastRain;
    public RawRain LastGhostRain;

    public Key(GameObject gameObject) {
        GameObject = gameObject;
    }
}