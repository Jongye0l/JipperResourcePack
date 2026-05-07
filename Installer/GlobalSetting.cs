using System.Collections.Generic;
using System.Threading.Tasks;

namespace JipperResourcePack.Installer;

public class GlobalSetting {
    public static GlobalSetting Instance = new();
    public string InstallPath;
    public Task<ModData[]> AdditionMods;
    public bool IsUninstall;
    
    // Install
    public bool InstallUnityModManager;
    public bool InstallDoorstop;
    public bool InstallJalib;
    public bool InstallJipperResourcePack;
    public List<ModData> SelectedMods;
    public List<string> RemoveRequestMods;
}