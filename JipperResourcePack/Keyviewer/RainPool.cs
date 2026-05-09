using UnityEngine;

namespace JipperResourcePack.Keyviewer;

public class RainPool : MonoBehaviour {
    public Rain[] pool = new Rain[16];
    public int poolCount;
    public Rain[] ghostPool = new Rain[16];
    public int ghostPoolCount;
    
    public void AddPool(Rain rain, bool isGhost) {
        rain.gameObject.SetActive(false);
        ref int count = ref isGhost ? ref ghostPoolCount : ref poolCount;
        ref Rain[] targetPool = ref isGhost ? ref ghostPool : ref pool;
        if(count == targetPool.Length) {
            Rain[] newRainPool = new Rain[targetPool.Length * 2];
            targetPool.CopyTo(newRainPool, 0);
            targetPool = newRainPool;
        }
        targetPool[count++] = rain;
    }

    public Rain GetOrNewRain(bool isGhost) {
        ref int count = ref isGhost ? ref ghostPoolCount : ref poolCount;
        if(count <= 0) return CreateRain(isGhost);
        Rain rain = (isGhost ? ghostPool : pool)[--count];
        rain.gameObject.SetActive(true);
        rain.transform.sizeDelta = Vector2.zero;
        return rain;
    }
    
    private Rain CreateRain(bool isGhost) {
        GameObject rainPrefab = new("Rain");
        RectTransform rainTransform = rainPrefab.AddComponent<RectTransform>();
        rainTransform.SetParent(transform);
        rainTransform.anchorMin = rainTransform.anchorMax = rainTransform.pivot = new Vector2(0.5f, 1);
        rainTransform.anchoredPosition = rainTransform.sizeDelta = Vector2.zero;
        rainTransform.localScale = Vector3.one;
        Rain rain = rainPrefab.AddComponent<Rain>();
        rain.transform = rainTransform;
        rain.pool = this;
        rain.Setup(isGhost);
        return rain;
    }
}