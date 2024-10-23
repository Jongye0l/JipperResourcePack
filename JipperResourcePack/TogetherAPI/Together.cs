using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Tools;

namespace JipperResourcePack.TogetherAPI;

public class Together : Feature {
    public static bool TogetherFound;
    public static Together Instance;
    public OverlayCanvasPrefabScript OverlayCanvasPrefabScript;
    public List<OverlayPlayerPrefabScript> OverlayPlayerPrefabScripts;

    public Together() : base(Main.Instance, nameof(Together)) {
        Instance = this;
        TogetherMainLoad();
    }

    private async void TogetherMainLoad() {
        while(true) {
            try {
                CheckTogether();
                break;
            } catch (Exception e) {
            }
            await Task.Yield();
        }
        foreach(FieldInfo field in typeof(OverlayCanvasPrefabScript).Fields()) {
            if(field.FieldType != typeof(List<OverlayPlayerPrefabScript>)) continue;
            OverlayPlayerPrefabScripts = (List<OverlayPlayerPrefabScript>) field.GetValue();
            break;
        }



        Patcher.AddPatch(typeof(TogetherPatches));
    }

    public static void Initialize() {
        try {
            CheckTogetherBootstrap();
            Main.Instance.Log("TogetherAPI is loaded.");
        } catch (FileNotFoundException) {
            Main.Instance.Log("TogetherAPI is not loaded.");
        } catch (TypeLoadException) {
            Main.Instance.Log("TogetherAPI is not loaded.");
        } catch (Exception e) {
            Main.Instance.Log("TogetherAPI is not loaded.");
            Main.Instance.LogException(e);
            return;
        }
        Main.Instance.AddTogether();
    }

    private static void CheckTogetherBootstrap() {
        TogetherFound = typeof(TogetherBootstrap.Main).Assembly != null;
    }

    private static void CheckTogether() {
        _ = typeof(Together).Assembly;
    }


    protected override void OnEnable() {

    }

    protected override void OnDisable() {

    }

    public class TogetherPatches {
        [JAPatch(typeof(OverlayCanvasPrefabScript), "Awake", PatchType.Postfix, true)]
        private static void SetupOverlay(OverlayCanvasPrefabScript __instance) {
            Instance.OverlayCanvasPrefabScript = __instance;
        }
    }
}