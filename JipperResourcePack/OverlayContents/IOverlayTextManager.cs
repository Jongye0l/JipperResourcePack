namespace JipperResourcePack.OverlayContents;

public interface IOverlayTextManager {
    void SetBest(float best);
    void CacheProgress(scrPlanet planet);
    void UpdateAccuracy(Overlay overlay, int index);
    void UpdateProgress(Overlay overlay);
    void UpdateProgressBar(Overlay overlay);
    void UpdateCheckpoint(Overlay overlay);
    void UpdateBest(Overlay overlay);
    float GetProgress();
    void SetupUnderTextLocation(Overlay overlay);
    void UpdateJudgement(Overlay overlay, int index);
}