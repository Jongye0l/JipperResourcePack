using JALib.Bootstrap;
using UnityModManagerNet;

namespace JipperResourcePack {
    public class Bootstrap {
        public static void Setup(UnityModManager.ModEntry modEntry) => JABootstrap.Load(modEntry);
    }
}