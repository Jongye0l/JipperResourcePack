﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JipperResourcePack.Async;
using UnityEngine;

namespace JipperResourcePack.Keyviewer;

public class Key : MonoBehaviour {
    public AsyncText text;
    public AsyncImage background;
    public AsyncImage outline;
    public AsyncText value;
    public GameObject rain;
    public bool isGreenColor;
    public List<RawRain> rainList = new();
    public ConcurrentQueue<RawRain> rawRainQueue = new();

    private void Update() {
        while(rawRainQueue.TryDequeue(out RawRain rawRain)) {
            Rain rainComponent = CreateRain(rawRain.transform);
            rainComponent.rawRain = rawRain;
            rainComponent.image.color = isGreenColor ? Color.white : KeyViewer.RainColor;
            rainComponent.transform.SetSiblingIndex(isGreenColor ? 1 : 0);
        }
    }

    private void OnDestroy() {
        rawRainQueue.Clear();
        foreach(RawRain rawRain in rainList) GC.SuppressFinalize(rawRain);
        rainList.Clear();
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    private static Rain CreateRain(Transform parent) {
        GameObject rainPrefab = new("Rain");
        RectTransform transform = rainPrefab.AddComponent<RectTransform>();
        transform.SetParent(parent);
        transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(0.5f, 1);
        transform.anchoredPosition = transform.sizeDelta = Vector2.zero;
        transform.localScale = Vector3.one;
        return rainPrefab.AddComponent<Rain>();
    }
}