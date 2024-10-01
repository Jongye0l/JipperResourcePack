using JALib.Bootstrap;
using UnityModManagerNet;

namespace Bootstrap {
    public class Bootstrap {
        public static void Setup(UnityModManager.ModEntry modEntry) => JABootstrap.Load(modEntry);
    }
}