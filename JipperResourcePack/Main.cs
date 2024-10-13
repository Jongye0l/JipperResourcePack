using System.Linq;
using System.Reflection;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using JALib.Tools;
using JipperResourcePack.Jongyeol;
using JipperResourcePack.Keyviewer;
using UnityEngine;
using UnityModManagerNet;

namespace JipperResourcePack;

public class Main : JAMod {
    public static Main Instance;
    public static JAPatcher Patcher;
    public static SettingGUI SettingGUI;
    public static ResourcePackSetting Settings;
    private static bool CreditsShown;
    private string sizeString;
    
    public Main(UnityModManager.ModEntry modEntry) : base(modEntry, true, typeof(ResourcePackSetting), discord: "https://discord.gg/qTbnPhY7YA", gid: 1313107549) {
        Instance = this;
        Patcher = new JAPatcher(this);
        Patcher.AddPatch(OnGameStart);
        Patcher.AddPatch(OnGameStop);
		FeatureReset(Jongyeol.Main.CheckEnable(Setting));
        Settings = (ResourcePackSetting) Setting;
        SettingGUI = new SettingGUI(this);
    }

    private void AddFeature() {
        AddFeature(new Status(), new BPM(), new Combo(), new Judgement(), new TimingScale(), new Attempt(), new ResourceChanger(), new KeyViewer());
    }

    public void FeatureReset(bool jongyeolMode) {
        bool enabled = Features.Count != 0;
        foreach(Feature feature in Features) feature.Invoke("Disable");
        this.GetValue<JASetting>("ModSetting").PutFieldData();
        Features.Clear();
        this.GetValue<JASetting>("ModSetting").RemoveFieldData();
        if(jongyeolMode) AddFeature(Jongyeol.Main.GetFeatures());
        else AddFeature();
        if(!enabled) return;
        if(jongyeolMode) new JOverlay();
        else new Overlay();
        foreach(Feature feature in Features.Where(feature => feature.Enabled)) feature.Invoke("Enable");
    }
    
    protected override void OnEnable() {
        BundleLoader.LoadBundle();
        PlayCount.Load();
        if(Jongyeol.Main.CheckEnable(Setting)) new JOverlay();
        else new Overlay();
        Patcher.Patch();
    }
    
    protected override void OnDisable() {
        PlayCount.Dispose();
        SaveSetting();
        Patcher.Unpatch();
        Overlay.Instance.Destroy();
        BundleLoader.UnloadBundle();
    }

    protected override void OnUpdate(float deltaTime) {
        Overlay.Instance.UpdateTime();
        Overlay.Instance.UpdateComboSize();
        Jongyeol.Main.Update(deltaTime);
    }

    protected override void OnGUI() {
        SettingGUI.AddSettingSliderFloat(ref Settings.Size, 1, ref sizeString, Localization["size"], 0, 2, Overlay.Instance.UpdateSize);
    }

    protected override void OnHideGUI() {
        sizeString = null;
    }

    protected override void OnGUIBehind() {
        if(!CreditsShown) {
            if(GUILayout.Button(Localization["credit.button"])) CreditsShown = true;
            GUILayout.FlexibleSpace();
            return;
        }
        if(GUILayout.Button(Localization["credit.buttonClose"])) CreditsShown = false;
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        if(GUILayout.Button(BundleLoader.SideImage, GUI.skin.label)) Application.OpenURL("https://github.com/Jongye0l/JipperResourcePack");
        GUILayout.BeginVertical();
        GUILayout.Space(5f);
        URLLabel($"Jipper Resource Pack", "https://github.com/Jongye0l/JipperResourcePack");
        URLLabel(Localization["credit.developer"] + ": Jongyeol", "https://www.youtube.com/@Jongyeol");
        URLLabel(Localization["credit.design"] + ": Jipper", "https://www.youtube.com/@jipper1214");
        URLLabel(Localization["credit.translator"] + ": changhyeon", "https://www.youtube.com/@changhyeon7492");
        URLLabel(Localization["credit.translator"] + ": sjk / Phrygia", "https://www.youtube.com/@sjk04_");
        URLLabel(Localization["credit.tester"] + ": dofaer", "https://www.youtube.com/@%EB%8F%84%ED%8E%98");
        URLLabel(Localization["credit.tester"] + ": NTure", "https://www.youtube.com/@NTure_1253");
        GUILayout.Space(25f);
        GUILayout.Label(Localization["credit.reference"]);
        URLLabel("Overlayer(By. c3nb)", "https://github.com/c3nb/Overlayer/tree/2cdf95b13add797f9c274d5766786c24c54adb9f");
        URLLabel("ShowBPM(By. Flower)", "https://github.com/FLOWERs-Modding/ADOFAI_ShowBPM");
        URLLabel("ProgressDisplayer2(By. Flower)", "https://github.com/FLOWERs-Modding/ADOFAI_ProgressDisplayer2");
        URLLabel("AdofaiTweaks(By. PizzaLovers007)", "https://github.com/PizzaLovers007/AdofaiTweaks");
        URLLabel("MovingManN(By. Kittut)", "https://github.com/Jongye0l/JIpper-Overlayer/blob/main/Scripts/MovingManN.js");
        URLLabel("MoreTimeTags(By. Jongyeol)", "https://github.com/Jongye0l/MoreTimeTags");
        URLLabel("BetterCalibration(By. Jongyeol)", "https://github.com/Jongye0l/BetterCalibration");
        if(Jongyeol.Main.ModeEnabled) {
            URLLabel("State(By. Jongyeol)", "https://github.com/Jongye0l/State");
            URLLabel("AdvancedCombo(By. Jongyeol)", "https://github.com/Jongye0l/AdvancedCombo");
        }
        URLLabel("AdofaiModInstaller(By. tjwogud)", "https://github.com/tjwogud/AdofaiModInstaller");
        GUILayout.Space(25f);
        GUILayout.Label(Localization["credit.font"]);
        URLLabel("Maplestory OTF Bold", "https://fontmeme.com/ktype/maplestory-font/");
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private static void URLLabel(string label, string url) {
        if(GUILayout.Button(label, GUI.skin.label)) Application.OpenURL(url);
    }

    [JAPatch(typeof(scnGame), "Play", PatchType.Postfix, false)]
    [JAPatch(typeof(scrPressToStart), "ShowText", PatchType.Postfix, false)]
    private static void OnGameStart() {
        Overlay.Instance.Show();
    }

    [JAPatch(typeof(scrUIController), "WipeToBlack", PatchType.Postfix, false)]
    [JAPatch(typeof(scnEditor), "ResetScene", PatchType.Postfix, false)]
    [JAPatch(typeof(scrController), "StartLoadingScene", PatchType.Postfix, false)]
    private static void OnGameStop() {
        Overlay.Instance.Hide();
    }
}
