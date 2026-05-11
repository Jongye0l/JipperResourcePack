using UnityEngine;

namespace JipperResourcePack.KeyViewerContents;

public class RainPool {
    public readonly RectTransform Transform;
    public Rain[] Pool = new Rain[16];
    public int PoolCount;
    public Rain[] GhostPool = new Rain[16];
    public int GhostPoolCount;

    public RainPool(RectTransform transform) {
        Transform = transform;
    }
    
    public void AddPool(Rain rain, bool isGhost) {
        rain.GameObject.SetActive(false);
        ref int count = ref isGhost ? ref GhostPoolCount : ref PoolCount;
        ref Rain[] targetPool = ref isGhost ? ref GhostPool : ref Pool;
        if(count == targetPool.Length) {
            Rain[] newRainPool = new Rain[targetPool.Length * 2];
            targetPool.CopyTo(newRainPool, 0);
            targetPool = newRainPool;
        }
        targetPool[count++] = rain;
    }

    public Rain GetOrNewRain(bool isGhost) {
        ref int count = ref isGhost ? ref GhostPoolCount : ref PoolCount;
        if(count <= 0) return new Rain(this, isGhost);
        Rain rain = (isGhost ? GhostPool : Pool)[--count];
        rain.GameObject.SetActive(true);
        rain.Transform.sizeDelta = Vector2.zero;
        return rain;
    }
}