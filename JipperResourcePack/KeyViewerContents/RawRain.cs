namespace JipperResourcePack.KeyViewerContents;

public class RawRain {
    private static RawRain[] _pool = new RawRain[32];
    private static int _poolCount;
    public Key Key;
    public long StartTime;
    public float XSize;
    public bool IsGhost;
    public float FinalSizeY;
    public bool FinishSize;
    public bool FinishSizeSetup;
    public bool SizeOver;

    private RawRain(Key key, long startTime, bool isGhost) => Init(key, startTime, isGhost);

    private void Init(Key key, long startTime, bool isGhost) {
        Key = key;
        StartTime = startTime;
        XSize = key.Color switch {
            1 => 50,
            3 => 30,
            _ => 40
        };
        IsGhost = isGhost;
    }

    private void Reset() {
        FinalSizeY = 0;
        FinishSize = FinishSizeSetup = SizeOver = false;
    }

    public void Finish(long time) {
        FinalSizeY = (time - StartTime) / 300f * KeyViewer.Settings.rainSpeed;
        FinishSize = true;
    }
    
    public static void AddPool(RawRain rain) {
        if(_poolCount == _pool.Length) {
            RawRain[] newRainPool = new RawRain[_pool.Length * 2];
            _pool.CopyTo(newRainPool, 0);
            _pool = newRainPool;
        }
        _pool[_poolCount++] = rain;
    }

    public static RawRain GetOrNewRawRain(Key key, long startTime, bool isGhost) {
        if(_poolCount <= 0) return new RawRain(key, startTime, isGhost);
        RawRain rain = _pool[--_poolCount];
        rain.Init(key, startTime, isGhost);
        rain.Reset();
        return rain;
    }
}