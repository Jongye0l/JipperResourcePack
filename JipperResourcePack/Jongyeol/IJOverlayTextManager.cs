using JipperResourcePack.OverlayContents;

namespace JipperResourcePack.Jongyeol;

public interface IJOverlayTextManager : IOverlayTextManager {
    void UpdateDeath(JOverlay overlay, scrPlanet planet);
    void UpdateState(JOverlay overlay, scrPlanet planet);
}