﻿using UnityEngine;

namespace JipperResourcePack.Keyviewer;

public class RawRain {
    public Transform transform;
    public long startTime;
    public byte color;
    public Vector2 FinalSize;
    public Vector2? sizeDelta;
    public Vector2? anchoredPosition;
    public bool removed;
    
    public bool UpdateLocation(long time, bool updateSize) {
        float y = (time - startTime) / 300f * 100;
        if(updateSize) FinalSize = new Vector2(color switch {
            1 => 50,
            3 => 30,
            _ => 40
        }, (time - startTime) / 300f * 100);
        if(y > 200) {
            float sizeY = FinalSize.y - y + 200;
            if(sizeY < 0) return false;
            sizeDelta = new Vector2(FinalSize.x, sizeY);
            anchoredPosition = new Vector2(0, 200);
        } else {
            if(updateSize) sizeDelta = FinalSize;
            anchoredPosition = new Vector2(0, y);
        }
        return true;
    }
    
    public RawRain(Transform transform, long startTime, byte color) {
        this.transform = transform;
        this.startTime = startTime;
        this.color = color;
    }
}