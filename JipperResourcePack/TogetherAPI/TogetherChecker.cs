using System;
using System.IO;
using System.Threading.Tasks;

namespace JipperResourcePack.TogetherAPI;

public class TogetherChecker {
    public static bool TogetherFound;

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
        AddTogetherWhenReady();
    }

    private static void CheckTogetherBootstrap() {
        TogetherFound = typeof(TogetherBootstrap.Main).Assembly != null;
    }

    public static void AddTogetherWhenReady() {
        try {
            CheckTogether();
            Main.Instance.AddTogether();
        } catch (Exception) {
            Task.Yield().GetAwaiter().OnCompleted(AddTogetherWhenReady);
            Main.Instance.Log("Together Not Found: Retrying...");
        }
    }

    private static void CheckTogether() {
        _ = typeof(Together).Assembly;
    }

}