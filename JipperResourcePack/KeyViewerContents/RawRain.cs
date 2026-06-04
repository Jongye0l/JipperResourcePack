using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace JipperResourcePack.KeyViewerContents;

public class RawRain {
    private static ConcurrentBag<RawRain> _pool = [];
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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddPool(RawRain rain) => _pool.Add(rain);

    public static RawRain GetOrNewRawRain(Key key, long startTime, bool isGhost) {
        if(!_pool.TryTake(out RawRain rain)) return new RawRain(key, startTime, isGhost);
        rain.Init(key, startTime, isGhost);
        rain.Reset();
        return rain;
    }
}