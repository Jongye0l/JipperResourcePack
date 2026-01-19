using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JipperResourcePack.Async;
using UnityEngine;
using UnityEngine.UI;

namespace JipperResourcePack.Keyviewer;

public class Key : MonoBehaviour {
    public AsyncText text;
    public Image background;
    public Image outline;
    public AsyncText value;
    public GameObject rainParent;
    public int color;
    public List<RawRain> RainList = [];
    public ConcurrentQueue<RawRain> RawRainQueue = new();
    public Rain[] rainPool = new Rain[16];
    public int rainPoolCount;
    
    private void Update() {
        while(RawRainQueue.TryDequeue(out RawRain rawRain)) {
            Rain rainComponent = GetOrNewRain();
            rainComponent.rawRain = rawRain;
            rainComponent.image.color = color switch {
                1 => KeyViewer.Settings.RainColor,
                3 => KeyViewer.Settings.RainColor3,
                _ => KeyViewer.Settings.RainColor2
            };
            rainComponent.transform.SetSiblingIndex(color - 1);
        }
    }

    private void OnDestroy() {
        RawRainQueue = null;
        foreach(RawRain rawRain in RainList) GC.SuppressFinalize(rawRain);
        RainList = null;
        rainPool = null;
    }

    public void AddPool(Rain rain) {
        rain.gameObject.SetActive(false);
        if(rainPoolCount == rainPool.Length) {
            Rain[] newRainPool = new Rain[rainPool.Length * 2];
            rainPool.CopyTo(newRainPool, 0);
        }
        rainPool[rainPoolCount++] = rain;
    }

    private Rain GetOrNewRain() {
        if(rainPoolCount <= 0) return CreateRain();
        Rain rain = rainPool[--rainPoolCount];
        rain.gameObject.SetActive(true);
        return rain;
    }
    
    private Rain CreateRain() {
        GameObject rainPrefab = new("Rain");
        RectTransform transform = rainPrefab.AddComponent<RectTransform>();
        transform.SetParent(rainParent.transform);
        transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(0.5f, 1);
        transform.anchoredPosition = transform.sizeDelta = Vector2.zero;
        transform.localScale = Vector3.one;
        Rain rain = rainPrefab.AddComponent<Rain>();
        rain.transform = transform;
        rain.key = this;
        return rain;
    }
}