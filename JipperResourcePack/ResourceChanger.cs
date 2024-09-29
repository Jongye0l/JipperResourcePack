using System.Threading.Tasks;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Tools;
using UnityEngine;
using UnityEngine.UI;
using GameObject = UnityEngine.GameObject;

namespace JipperResourcePack;

public class ResourceChanger : Feature {

    public static Color PlanetColor;
    public static Color TitleColor;
    public static Color TileColor;
    public static string ResourcePackName;
    public static Sprite autoSprite;

    public ResourceChanger() : base(Main.Instance, nameof(ResourceChanger), true, typeof(ResourceChanger)) {
        ResourcePackName = "Jipper Resource Pack";
        PlanetColor = new Color(0.8125f, 0.70703125f, 0.96875f);
        TitleColor = new Color(0.56640625f, 0.46875f, 0.6328125f);
        TileColor = new Color(0.94921875f, 0.87109375f, 1);
    }

    protected override void OnEnable() {
        if(ADOBase.isLevelSelect) ADOBase.LoadScene(ADOBase.sceneName);
        else if(ADOBase.controller) {
            if(ADOBase.editor) OnEditorStart();
            foreach(scrPlanet planet in ADOBase.controller.allPlanets) OnPlanetStart(planet);
        }
    }

    protected override async void OnDisable() {
        while(Patcher.patched) await Task.Yield();
        if(ADOBase.isLevelSelect) ADOBase.LoadScene(ADOBase.sceneName);
        else if(ADOBase.controller) {
            if(autoSprite && ADOBase.editor) {
                ADOBase.editor.autoImage.sprite = autoSprite;
                ADOBase.editor.Invoke("OttoUpdate");
            }
            foreach(scrPlanet planet in ADOBase.controller.allPlanets) planet.SetColor(planet.currentPlanetColor);
            foreach(scrFloor floor in Object.FindObjectsByType<scrFloor>(FindObjectsSortMode.None)) {
                if(floor.gameObject.tag != "Beat") return;
                floor.floorRenderer.color = new Color(0.675f, 0.675f, 0.766f, 1f);
            }
        }
    }

    [JAPatch(typeof(scnEditor), "OttoUpdate", PatchType.Postfix, false)]
    public static void OnEditorStart() {
        Image autoImage = scnEditor.instance.autoImage;
        if(autoImage.sprite == BundleLoader.Auto) return;
        autoSprite = autoImage.sprite;
        autoImage.sprite = BundleLoader.Auto;
        // auto ? #9900FF : #320054
        autoImage.color = RDC.auto ? new Color(0.5703125f, 0, 1) : new Color(0.19607843f, 0, 0.32941177f);
    }
    
    [JAPatch(typeof(scrFloor), "Start", PatchType.Postfix, false)]
    public static void OnFloorStart(scrFloor __instance) {
        if(__instance.gameObject.tag != "Beat") return;
        __instance.floorRenderer.color = TileColor;
    }
    
    [JAPatch(typeof(scrPlanet), "Start", PatchType.Postfix, false)]
    public static void OnPlanetStart(scrPlanet __instance) {
        __instance.DisableAllSpecialPlanets();
        __instance.sprite.sprite = ADOBase.gc.tex_planetWhite;
        __instance.SetPlanetColor(PlanetColor);
        __instance.SetTailColor(PlanetColor);
        scrLogoText.instance?.UpdateColors();
    }

    [JAPatch(typeof(scrPlanet), "SetRainbow", PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scrPlanet), "LoadPlanetColor", PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scrPlanet), "SetColor", PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scnLevelSelect), "RainbowMode", PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scnLevelSelect), "EnbyMode", PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scrLogoText), "UpdateColors", PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scrLogoText), "LateUpdate", PatchType.Prefix, false, TryingCatch = false)]
    public static bool OnPlanetAndLogoColorChange() => false;

    [JAPatch(typeof(scrPlanet), "SetPlanetColor", PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scrPlanet), "SetCoreColor", PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scrPlanet), "SetTailColor", PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scrPlanet), "SetRingColor", PatchType.Prefix, false, TryingCatch = false)]
    [JAPatch(typeof(scrPlanet), "SetFaceColor", PatchType.Prefix, false, TryingCatch = false)]
    public static void Prefix(ref Color color) {
        color = PlanetColor;
    }

    [JAPatch(typeof(scrFloor), "SetTileColor", PatchType.Prefix, false, TryingCatch = false)]
    public static bool OnTileColorChange(scrFloor __instance) => __instance.tag != "Beat";
    
    [JAPatch(typeof(scrLogoText), "Awake", PatchType.Postfix, false)]
    public static void OnLogoTextAwake(scrLogoText __instance) {
        RectTransform rectTransform = __instance.gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = rectTransform.anchoredPosition with { y = 0.75f };
        __instance.ColorLogo(PlanetColor, true);
        __instance.ColorLogo(PlanetColor, false);
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