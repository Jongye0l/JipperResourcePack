namespace JipperResourcePack.Installer;

public class RequirementStatus {
    public bool IsExistUnityModManager;
    public bool IsExistJALib;
    public bool IsExistJipperResourcePack;
    public bool IsAssemblyInstalled;
    public bool IsOldDoorStop;
    public bool ExistMods;

    public void Reset() {
        IsExistUnityModManager = IsExistJALib = IsExistJipperResourcePack = IsAssemblyInstalled = IsOldDoorStop = ExistMods = false;
    }
}