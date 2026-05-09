using UnityEngine;

namespace JipperResourcePack.Keyviewer;

public class RawRain {
    public readonly long StartTime;
    public float XSize;
    public Vector2 FinalSize;
    public Vector2? SizeDelta;
    public Vector2? AnchoredPosition;
    public bool Removed;
    public bool IsGhost;
    public bool FinishSize;
    
    public bool UpdateLocation(long time, bool updateSize, float speed, float height) {
        float y = (time - StartTime) / 300f * speed;
        updateSize &= !FinishSize;
        if(updateSize) FinalSize = new Vector2(XSize, (time - StartTime) / 300f * speed);
        else FinishSize = true;
        if(y > height) {
            float sizeY = FinalSize.y - y + height;
            if(sizeY < 0) return false;
            SizeDelta = new Vector2(FinalSize.x, sizeY);
            AnchoredPosition = new Vector2(0, height);
        } else {
            if(updateSize) SizeDelta = FinalSize;
            AnchoredPosition = new Vector2(0, y);
        }
        return true;
    }
    
    public RawRain(long startTime, int color, bool isGhost) {
        StartTime = startTime;
        XSize = color switch {
            1 => 50,
            3 => 30,
            _ => 40
        };
        IsGhost = isGhost;
    }
}