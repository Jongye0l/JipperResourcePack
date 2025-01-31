using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using JALib.Tools;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using GameObject = UnityEngine.GameObject;
using Object = UnityEngine.Object;

namespace JipperResourcePack;

public class ResourceChanger : Feature {

    public static bool IsAfterR129;
    public static Color PlanetColor;
    public static Color TitleColor;
    public static Color TileColor;
    public static string ResourcePackName;
    public static Sprite autoSprite;
    public static ResourceChangerSetting Settings;

    public ResourceChanger() : base(Main.Instance, nameof(ResourceChanger), true, typeof(ResourceChanger), typeof(ResourceChangerSetting)) {
        IsAfterR129 = typeof(scrPlanet).Field("planetarySystem") != null;
        Patch();
        ResourcePackName = "Jipper Resource Pack";
        PlanetColor = new Color(0.8125f, 0.70703125f, 0.96875f);
        TitleColor = new Color(0.56640625f, 0.46875f, 0.6328125f);
        TileColor = new Color(0.94921875f, 0.87109375f, 1);
        Settings = (ResourceChangerSetting) Setting;
    }

    private void Patch() {
        Type type = typeof(scrPlanet);
        if(IsAfterR129) type = type.Assembly.GetType("PlanetRenderer");
        MethodInfo method = ((Delegate) OnPlanetAndLogoColorChange).Method;
        foreach(string methodName in (string[]) ["SetRainbow", "LoadPlanetColor", "SetColor"]) {
            if(type.Method(methodName) == null) Main.Instance.Error("Method not found: " + type.FullName + "." + methodName);
            Patcher.AddPatch(method, new JAPatchAttribute(type, methodName, PatchType.Prefix, false) {
                TryingCatch = false
            });
        }
        method = ((Delegate) Prefix).Method;
        foreach(string methodName in (string[]) ["SetPlanetColor", "SetCoreColor", "SetTailColor", "SetRingColor", "SetFaceColor"]) {
            if(type.Method(methodName) == null) Main.Instance.Error("Method not found: " + type.FullName + "." + methodName);
            Patcher.AddPatch(method, new JAPatchAttribute(type, methodName, PatchType.Prefix, false) {
                TryingCatch = false
            });
        }
    }

    protected override void OnEnable() {
        if(ADOBase.isLevelSelect && Settings.ChangeTileColor) ADOBase.LoadScene(ADOBase.sceneName);
        else if(ADOBase.controller) {
            if(Settings.ChangeRabbit) LoadRabbit();
            if(Settings.ChangeBallColor) LoadPlanet();
        }
    }

    protected override async void OnDisable() {
        while(Patcher.patched) await Task.Yield();
        if(ADOBase.isLevelSelect && Settings.ChangeTileColor) ADOBase.LoadScene(ADOBase.sceneName);
        else if(ADOBase.controller) {
            if(Settings.ChangeRabbit) UnloadRabbit();
            if(Settings.ChangeBallColor) UnloadPlanet();
            if(Settings.ChangeTileColor) UnloadTileColor();
        }
    }

    protected override void OnGUI() {
        SettingGUI settingGUI = Main.SettingGUI;
        JALocalization localization = Main.Instance.Localization;
        settingGUI.AddSettingToggle(ref Settings.ChangeRabbit, localization["resourceChanger.changeRabbit"], () => {
            if(Settings.ChangeRabbit) LoadRabbit();
            else UnloadRabbit();
        });
        settingGUI.AddSettingToggle(ref Settings.ChangeBallColor, localization["resourceChanger.changeBallColor"], () => {
            if(Settings.ChangeBallColor) LoadPlanet();
            else UnloadPlanet();
        });
        settingGUI.AddSettingToggle(ref Settings.ChangeTileColor, localization["resourceChanger.changeTileColor"], () => {
            if(ADOBase.isLevelSelect) ADOBase.LoadScene(ADOBase.sceneName);
            else if(!Settings.ChangeTileColor) UnloadTileColor();
        });
    }

    private static void LoadRabbit() {
        if(ADOBase.editor) OnEditorStart();
    }

    private static List<scrPlanet> GetAllPlanets() {
        object obj = ADOBase.controller;
        if(IsAfterR129) obj = obj.GetValue("planetarySystem");
        return obj.GetValue<List<scrPlanet>>("allPlanets");
    }

    private static void LoadPlanet() {
        foreach(scrPlanet planet in GetAllPlanets()) OnPlanetStart(planet);
        if(!ADOBase.isLevelSelect) return;
        scrLogoText logoText = Object.FindObjectOfType<scrLogoText>();
        if(!logoText) return;
        logoText.ColorLogo(PlanetColor, true);
        logoText.ColorLogo(PlanetColor, false);
    }

    private static void UnloadRabbit() {
        if(!autoSprite || !ADOBase.editor) return;
        ADOBase.editor.autoImage.sprite = autoSprite;
        ADOBase.editor.Invoke("OttoUpdate");
    }

    private static void UnloadPlanet() {
        if(IsAfterR129) foreach(scrPlanet planet in GetAllPlanets()) planet.Invoke("LoadPlanetColor");
        else UnloadPlanetR129();
        scrLogoText.instance?.UpdateColors();
    }

    private static void UnloadPlanetR129() {
        PlanetarySystem planetarySystem = ADOBase.controller.planetarySystem;
        planetarySystem.planetRed.planetRenderer.LoadPlanetColor(true);
        planetarySystem.planetBlue.planetRenderer.LoadPlanetColor(false);
    }

    private static void UnloadTileColor() {
        foreach(scrFloor floor in Object.FindObjectsByType<scrFloor>(FindObjectsSortMode.None)) {
            if(floor.gameObject.tag != "Beat") return;
            floor.floorRenderer.color = new Color(0.675f, 0.675f, 0.766f, 1f);
        }
    }

    public class ResourceChangerSetting(JAMod mod, JObject jsonObject = null) : JASetting(mod, jsonObject) {
        public bool ChangeRabbit = true;
        public bool ChangeBallColor = true;
        public bool ChangeTileColor = true;
    }

    [JAPatch(typeof(scnEditor), "OttoUpdate", PatchType.Postfix, false)]
    public static void OnEditorStart() {
        if(!Settings.ChangeRabbit) return;
        Image autoImage = scnEditor.instance.autoImage;
        if(autoImage.sprite == BundleLoader.Auto) return;
        autoSprite = autoImage.sprite;
        autoImage.sprite = BundleLoader.Auto;
        // auto ? #9900FF : #320054
        autoImage.color = RDC.auto ? new Color(0.5703125f, 0, 1) : new Color(0.19607843f, 0, 0.32941177f);
    }
    
    [JAPatch(typeof(scrFloor), "Start", PatchType.Postfix, false)]
    public static void OnFloorStart(scrFloor __instance) {
        if(!Settings.ChangeTileColor || __instance.tag != "Beat") return;
        __instance.floorRenderer.color = TileColor;
    }
    
    [JAPatch(typeof(scrPlanet), "Start", PatchType.Postfix, false)]
    public static void OnPlanetStart(scrPlanet __instance) {
        if(!Settings.ChangeBallColor) return;
        object obj = __instance;
        if(IsAfterR129) obj = obj.GetValue("planetRenderer");
        obj.Invoke("DisableAllSpecialPlanets");
        obj.GetValue("sprite").SetValue("sprite", ADOBase.gc.tex_planetWhite);
        obj.Invoke("SetPlanetColor", PlanetColor);
        obj.Invoke("SetTailColor", PlanetColor);
        scrLogoText.instance?.UpdateColors();
    }

    [JAPatch(typeof(scnLevelSelect), "RainbowMode", PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scnLevelSelect), "EnbyMode", PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scrLogoText), "UpdateColors", PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scrLogoText), "LateUpdate", PatchType.Prefix, false, TryingCatch = false)]
    public static bool OnPlanetAndLogoColorChange() => !Settings.ChangeBallColor;

    public static void Prefix(ref Color color) {
        if(Settings.ChangeBallColor) color = PlanetColor;
    }

    [JAPatch(typeof(scrFloor), "SetTileColor", PatchType.Prefix, false, TryingCatch = false)]
    public static bool OnTileColorChange(scrFloor __instance) => !Settings.ChangeTileColor || __instance.tag != "Beat";
    
    [JAPatch(typeof(scrLogoText), "Awake", PatchType.Postfix, false)]
    public static void OnLogoTextAwake(scrLogoText __instance) {
        RectTransform rectTransform = __instance.gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = rectTransform.anchoredPosition with { y = 0.75f };
        if(Settings.ChangeBallColor) {
            __instance.ColorLogo(PlanetColor, true);
            __instance.ColorLogo(PlanetColor, false);
        }
        Transform transform = rectTransform.parent.parent.Find("Hit Space");
        if(transform.Find("JipperResourcepack Logo")) return;
        GameObject gameObject = transform.Find("Education Edition").gameObject;
        gameObject = GameObject.Instantiate(gameObject, gameObject.transform.parent);
        gameObject.SetActive(true);
        gameObject.name = "JipperResourcepack Logo";
        Text text = gameObject.GetComponent<Text>();
        text.text = ResourcePackName;
        text.color = TitleColor;
        text.fontSize = 100;
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-50, 330);
    }
}