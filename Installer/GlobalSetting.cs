using System.Collections.Generic;
using System.Threading.Tasks;

namespace JipperResourcePack.Installer;

public class GlobalSetting {
    public static GlobalSetting Instance = new();
    public string InstallPath;
    public Task<ModData[]> AdditionMods;
    public List<ModData> SelectedMods;
}