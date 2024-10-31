using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using DG.Tweening;
using HarmonyLib;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Tools;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace JipperResourcePack.TogetherAPI;

public class Together : Feature {
    public static bool TogetherFound;
    public static Together Instance;
    public List<OverlayPlayerPrefabScript> OverlayPlayerPrefabScripts;
    public List<OverlayTeamPrefabScript> OverlayTeamPrefabScripts;
    public OverlayCanvasPrefabScript CanvasPrefab;
    public Dictionary<string, Type> packets;
    public Type packetType;
    public Type userInfoType;
    public FieldInfo usernameField;
    public FieldInfo displayNameField;
    public MethodInfo xAccuracyMethod;
    public MethodInfo deathMethod;
    public MethodInfo addUserDataMethod;
    public Type userPlayDataType;
    public FieldInfo userData_userName;
    public FieldInfo userData_displayName;
    public FieldInfo userData_isReady;
    public FieldInfo userData_xAcc;
    public FieldInfo userData_fails;

    public Dictionary<OverlayPlayerPrefabScript, PlayData> PlayData = new();

    public Together() : base(Main.Instance, nameof(Together)) {
        Instance = this;
        TogetherMainLoad();
    }

    private void TogetherMainLoad() {
        try {
            CheckTogether();
        } catch (Exception) {
            Task.Yield().GetAwaiter().OnCompleted(TogetherMainLoad);
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
            foreach(MethodInfo method in typeof(OverlayCanvasPrefabScript).Methods()) {
                if(method.DeclaringType != typeof(OverlayCanvasPrefabScript) || method.ReturnType != typeof(void) ||
                   method.GetParameters().Length != 1 || method.GetParameters()[0].ParameterType.Assembly != typeof(OverlayCanvasPrefabScript).Assembly) continue;
                userPlayDataType = method.GetParameters()[0].ParameterType;
                break;
            }
            if(userPlayDataType == null) throw new TogetherApiException("Failed to find userPlayData type.");
            foreach(MethodInfo method in typeof(OverlayPlayerPrefabScript).Methods()) {
                if(method.DeclaringType != typeof(OverlayPlayerPrefabScript) || method.ReturnType != typeof(void) ||
                   method.GetParameters().Length != 1) continue;
                Type parameterType = method.GetParameters()[0].ParameterType;
                if(parameterType == typeof(float)) xAccuracyMethod = method;
                if(parameterType == typeof(int)) deathMethod = method;
                if(parameterType == userPlayDataType) addUserDataMethod = method;
                break;
            }
            if(xAccuracyMethod == null) throw new TogetherApiException("Failed to find xAccuracy method.");
            if(deathMethod == null) throw new TogetherApiException("Failed to find death method.");
            if(addUserDataMethod == null) throw new TogetherApiException("Failed to find addUserData method.");
            object playDataTest = userPlayDataType.New("", null, false, -1, -1);
            foreach(FieldInfo field in userPlayDataType.Fields()) {
                if(field.FieldType == typeof(string)) {
                    if(field.GetValue<string>(playDataTest) == "") userData_userName = field;
                    else userData_displayName = field;
                }
                if(field.FieldType == typeof(bool)) userData_isReady = field;
                if(field.FieldType == typeof(float)) userData_xAcc = field;
                if(field.FieldType == typeof(int)) userData_fails = field;
            }
            Patcher.AddPatch(typeof(TogetherPatches));
            Patcher.AddPatch(TogetherPatches.SetXAccuracy, new JAPatchAttribute(xAccuracyMethod, PatchType.Postfix, true));
            Patcher.AddPatch(TogetherPatches.AddUserDataPatch, new JAPatchAttribute(addUserDataMethod, PatchType.Transpiler, true));
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

    public GameObject RankingObject;
    public GameObject RankingElement;

    protected override void OnEnable() {
        RankingObject = new GameObject();
        RectTransform transform = RankingObject.AddComponent<RectTransform>();
        transform.SetParent(Overlay.Instance.Canvas.transform);
        transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(1, 0.5f);
        transform.sizeDelta = new Vector2(300, 500);
        GameObject background = new();
        RectTransform backgroundTransform = background.AddComponent<RectTransform>();
        backgroundTransform.SetParent(RankingObject.transform);
        backgroundTransform.sizeDelta = new Vector2(300, 500);
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0, 0, 0, 0.6588235f);
        RankingElement = new GameObject();
        RankingObject.SetActive(false);
    }

    protected override void OnDisable() {
        Object.Destroy(RankingObject);
    }

    public void UpdateUserData() {
        PlayData[] sortedData = PlayData.Values.OrderBy(a => a.XAcc).ToArray();
        for(int i = 0; i < sortedData.Length; i++) {
            PlayData data = sortedData[i];
            data.ranking.transform.SetSiblingIndex(i);
            if(data.moving || data.moveDestination == i) continue;
            data.moveDestination = i;
            data.moving = true;
            data.ranking.transform.DOAnchorPosY(60 * (sortedData.Length - i - 1), 1f).SetEase(Ease.InQuad).OnComplete(() => data.moving = false);
        }
    }

    public class TogetherPatches {
        [JAPatch(typeof(OverlayCanvasPrefabScript), "Awake", PatchType.Postfix, true)]
        public static void SetupOverlay(OverlayCanvasPrefabScript __instance) {
            RectTransform transform = __instance.playerListObject.GetComponent<RectTransform>();
            transform.anchoredPosition = new Vector2(-10000, -10000);
            Instance.RankingObject.SetActive(true);
            foreach(PlayData data in Instance.PlayData.Values) Object.Destroy(data.ranking.gameObject);
            Instance.PlayData.Clear();
        }

        [JAPatch(typeof(OverlayCanvasPrefabScript), "OnDestroy", PatchType.Postfix, true)]
        public static void CleanupOverlay() => Instance.RankingObject.SetActive(false);

        public static void SetXAccuracy(OverlayPlayerPrefabScript __instance) => Instance.PlayData[__instance].UpdateText();

        public static IEnumerable<CodeInstruction> AddUserDataPatch(IEnumerable<CodeInstruction> instructions) =>
            instructions.Concat([
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, ((Delegate) AddUserData).Method)
            ]);

        public static void AddUserData(OverlayPlayerPrefabScript instance, object o) {
            PlayData playData = new(o);
            Instance.PlayData.Add(instance, playData);
            GameObject obj = new(playData.Username);
            Ranking ranking = playData.ranking = obj.AddComponent<Ranking>();
            ranking.transform.SetParent(Instance.RankingObject.transform);
            ranking.usernameText.text = playData.DisplayName;
            if(instance.avatar) ranking.profileImage.sprite = instance.avatar.sprite;
            instance.avatar = ranking.profileImage;
            Instance.UpdateUserData();
        }
    }
}