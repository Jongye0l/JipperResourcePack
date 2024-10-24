using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using HarmonyLib;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Tools;

namespace JipperResourcePack.TogetherAPI;

public class Together : Feature {
    public static bool TogetherFound;
    public static Together Instance;
    public List<OverlayPlayerPrefabScript> OverlayPlayerPrefabScripts;
    public List<OverlayTeamPrefabScript> OverlayTeamPrefabScripts;
    public Dictionary<string, Type> packets;
    public Type packetType;
    public Type userInfoType;
    public FieldInfo usernameField;
    public FieldInfo displayNameField;

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
        try {
            foreach(FieldInfo field in typeof(OverlayCanvasPrefabScript).Fields()) {
                if(field.FieldType == typeof(List<OverlayPlayerPrefabScript>)) OverlayPlayerPrefabScripts = field.GetValue<List<OverlayPlayerPrefabScript>>();
                if(field.FieldType == typeof(List<OverlayTeamPrefabScript>)) OverlayTeamPrefabScripts = field.GetValue<List<OverlayTeamPrefabScript>>();
            }
            if(OverlayPlayerPrefabScripts == null || OverlayTeamPrefabScripts == null) throw new TogetherApiException("Failed to find OverlayPrefabScript.");
            foreach(Type type in typeof(global::Together.Main).Assembly.GetTypes()) {
                FieldInfo field = type.Fields().FirstOrDefault(field => field.FieldType == typeof(Dictionary<string, Type>));
                if(field == null) continue;
                packetType = type;
                packets = (Dictionary<string, Type>) field.GetValue(null);
                if(packets.TryGetValue("UserInfo", out userInfoType)) break;
            }
            if(userInfoType == null) throw new TogetherApiException("Failed to find UserInfo packet.");
            foreach(MethodInfo method in userInfoType.Methods()) {
                if(method.DeclaringType != userInfoType || method.GetBaseDefinition().DeclaringType != packetType) continue;
                IEnumerator<CodeInstruction> enumerator = PatchProcessor.GetCurrentInstructions(method).GetEnumerator();
                bool first = true;
                while(enumerator.MoveNext()) {
                    CodeInstruction code = enumerator.Current;
                    if(code.opcode != OpCodes.Stsfld) continue;
                    FieldInfo field = (FieldInfo) code.operand;
                    if(field.FieldType != typeof(string)) continue;
                    if(first) {
                        usernameField = field;
                        first = false;
                    } else {
                        displayNameField = field;
                        break;
                    }
                }
                if(displayNameField != null) break;
            }
            if(displayNameField == null) throw new TogetherApiException("Failed to find UserInfo fields.");
            Patcher.AddPatch(typeof(TogetherPatches));
        } catch (Exception e) {
            Main.Instance.LogException(e);
            Main.Instance.Log("Together API is currently disabled.");
            TogetherFound = false;
            Main.Instance.RemoveTogether();
        }
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
    }
}