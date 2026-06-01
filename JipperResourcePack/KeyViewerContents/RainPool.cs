using UnityEngine;

namespace JipperResourcePack.KeyViewerContents;

public class RainPool(RectTransform transform) {
    public readonly RectTransform Transform = transform;
    private Rain[] _pool = new Rain[16];
    private int _poolCount;
    private Rain[] _ghostPool = new Rain[16];
    private int _ghostPoolCount;

    public void AddPool(Rain rain, bool isGhost) {
        rain.GameObject.SetActive(false);
        ref int count = ref isGhost ? ref _ghostPoolCount : ref _poolCount;
        ref Rain[] targetPool = ref isGhost ? ref _ghostPool : ref _pool;
        if(count == targetPool.Length) {
            Rain[] newRainPool = new Rain[targetPool.Length * 2];
            targetPool.CopyTo(newRainPool, 0);
            targetPool = newRainPool;
        }
        targetPool[count++] = rain;
    }

    public Rain GetOrNewRain(bool isGhost) {
        ref int count = ref isGhost ? ref _ghostPoolCount : ref _poolCount;
        if(count <= 0) return new Rain(this, isGhost);
        Rain rain = (isGhost ? _ghostPool : _pool)[--count];
        rain.GameObject.SetActive(true);
        rain.Transform.sizeDelta = Vector2.zero;
        return rain;
    }
}