using UnityEngine;

namespace JipperResourcePack.Keyviewer;

public class RawRain {
    public Transform transform;
    public long startTime;
    public byte color;
    public Vector2 FinalSize;
    public Vector2? sizeDelta;
    public Vector2? anchoredPosition;
    public bool removed;
    
    public bool UpdateLocation(long time, bool updateSize, float speed, float height) {
        float y = (time - startTime) / 300f * speed;
        if(updateSize) FinalSize = new Vector2(color switch {
            1 => 50,
            3 => 30,
            _ => 40
        }, (time - startTime) / 300f * speed);
        if(y > height) {
            float sizeY = FinalSize.y - y + height;
            if(sizeY < 0) return false;
            sizeDelta = new Vector2(FinalSize.x, sizeY);
            anchoredPosition = new Vector2(0, height);
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