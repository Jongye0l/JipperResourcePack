namespace JipperResourcePack.Keyviewer;

public class RawRain {
    public readonly long StartTime;
    public readonly float XSize;
    public readonly bool IsGhost;
    public float FinalSizeY;
    public bool FinishSize;
    public bool FinishSizeSetup;
    public bool SizeOver;
    public Key Key;
    
    public RawRain(Key key, long startTime, bool isGhost) {
        Key = key;
        StartTime = startTime;
        XSize = key.Color switch {
            1 => 50,
            3 => 30,
            _ => 40
        };
        IsGhost = isGhost;
    }

    public void Finish(long time) {
        FinalSizeY = (time - StartTime) / 300f * KeyViewer.Settings.rainSpeed;
        FinishSize = true;
    }
}