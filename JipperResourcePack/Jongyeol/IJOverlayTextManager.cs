using JipperResourcePack.OverlayContents;

namespace JipperResourcePack.Jongyeol;

public interface IJOverlayTextManager : IOverlayTextManager {
    void UpdateDeath(JOverlay overlay, scrPlanet planet);
    void UpdateState(JOverlay overlay, scrPlanet planet);
    void CheckPurePerfect(JOverlay overlay, scrPlanet planet);
    int GetTooJudgement(JOverlay overlay);
}