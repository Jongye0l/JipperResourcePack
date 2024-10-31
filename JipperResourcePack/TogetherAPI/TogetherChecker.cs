using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JALib.Core.Patch;
using JALib.Tools;
using UnityModManagerNet;

namespace JipperResourcePack.TogetherAPI;

public class TogetherChecker {
    public static bool TogetherInit;
    private static UnityModManager.ModEntry _modEntry;
    private static Assembly _togetherAssembly;
    private static JAPatcher togetherInitPatcher;

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
        if(_modEntry.OnToggle == null) {
            togetherInitPatcher = new JAPatcher(Main.Instance);
            togetherInitPatcher.AddPatch(AssemblyLoadPatch);
            togetherInitPatcher.Patch();
            return;
        }
        SetTogetherAssembly(_modEntry.OnToggle.Method.DeclaringType.Assembly);
    }

    [JAPatch(typeof(Assembly), "Load", PatchType.Postfix, true, ArgumentTypesType = [typeof(byte[])])]
    private static void AssemblyLoadPatch(Assembly __result) {
        Main.Instance.Log(__result.GetName().Name + " is loaded.");
        if(__result.GetName().Name != "Together") return;
        SetTogetherAssembly(__result);
        togetherInitPatcher.Dispose();
        togetherInitPatcher = null;
    }

    private static void SetTogetherAssembly(Assembly assembly) {
        _togetherAssembly = assembly;
        AppDomain.CurrentDomain.AssemblyResolve += TogetherResolver;
        TogetherInit = true;
        Main.Instance.AddTogether();
    }

    private static void CheckTogetherBootstrap() {
        _modEntry = typeof(TogetherBootstrap.Main).Fields().First(f => f.FieldType == typeof(UnityModManager.ModEntry)).GetValue<UnityModManager.ModEntry>();
    }

    private static Assembly TogetherResolver(object sender, ResolveEventArgs args) => args.Name == "Together" ? _togetherAssembly : null;
}