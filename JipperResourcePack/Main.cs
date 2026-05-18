using System;
using System.Linq;
using System.Reflection;
using JALib.Core;
using JALib.Core.Patch;
using JALib.Core.Setting;
using JALib.Tools;
using JipperResourcePack.Jongyeol;
using JipperResourcePack.KeyViewerContents;
using JipperResourcePack.OverlayContents;
using MonsterLove.StateMachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JipperResourcePack;

// ReSharper disable once ClassNeverInstantiated.Global
public class Main() : JAMod(typeof(ResourcePackSetting)) {
    // ReSharper disable once UnassignedField.Global
    public static Main Instance;
    public static SettingGUI SettingGUI;
    public static ResourcePackSetting Settings;
    private static bool _creditsShown;
    private string _sizeString;

    protected override void OnSetup() {
        Patcher.AddPatch(OnGameStart1);
        Patcher.AddPatch(OnGameStart2);
        Patcher.AddPatch(OnChangeState);
        Patcher.AddPatch(OnGameStop);
        Patcher.AddPatch(UpdatePlayersCount);
        VersionSafe.Setup();
        FeatureReset(JMain.CheckEnable(Setting));
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
        if(jongyeolMode) AddFeature(JMain.GetFeatures());
        else AddFeature();
        if(!enabled) return;
        _ = jongyeolMode ? new JOverlay() : new Overlay();
        foreach(Feature feature in Features.Where(feature => feature.Enabled)) feature.Invoke("Enable");
    }
    
    protected override void OnEnable() {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        BundleLoader.LoadBundle();
        PlayCount.Load();
        _ = JMain.CheckEnable(Setting) ? new JOverlay() : new Overlay();
    }

    protected override void OnDisable() {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        try {
            PlayCount.Dispose();
            Overlay.Instance.Destroy();
        } catch (Exception e) {
            LogException(e);
        }
        SaveSetting();
        BundleLoader.UnloadBundle();
    }

    private static void OnSceneUnloaded(Scene _) => Overlay.Instance.Hide();
    
    protected override void OnUpdate(float deltaTime) {
        Overlay.Instance.UpdateTime();
        Overlay.Instance.UpdateComboSize();
        JMain.Update(deltaTime);
    }

    protected override void OnGUI() {
        SettingGUI.AddSettingSliderFloat(ref Settings.Size, 1, ref _sizeString, Localization["size"], 0, 2, Overlay.Instance.UpdateSize);
    }

    protected override void OnHideGUI() {
        _sizeString = null;
    }

    protected override void OnGUIBehind() {
        if(!_creditsShown) {
            if(GUILayout.Button(Localization["credit.button"])) _creditsShown = true;
            return;
        }
        if(GUILayout.Button(Localization["credit.buttonClose"])) _creditsShown = false;
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        if(GUILayout.Button(BundleLoader.SideImage, GUI.skin.label)) Application.OpenURL("https://github.com/Jongye0l/JipperResourcePack");
        GUILayout.BeginVertical();
        GUILayout.Space(5f);
        URLLabel("Jipper Resource Pack", "https://github.com/Jongye0l/JipperResourcePack");
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
        if(JMain.ModeEnabled) {
            URLLabel("State(By. Jongyeol)", "https://github.com/Jongye0l/State");
            URLLabel("AdvancedCombo(By. Jongyeol)", "https://github.com/Jongye0l/AdvancedCombo");
        }
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
    private static void OnGameStart1(int seqID) { // scnEditor, scnGame (except practice)
        if(GCS.practiceMode) return;
        Overlay.Instance.Show(seqID);
    }

    [JAPatch(typeof(scrPressToStart), "ShowText", PatchType.Postfix, false)]
    private static void OnGameStart2() { // internal, practice
        if(!GCS.practiceMode && scnGame.instance) return;
        Overlay.Instance.Show(scrController.instance.currentSeqID);
    }

    [JAPatch(typeof(StateBehaviour), "ChangeState", PatchType.Postfix, true, ArgumentTypesType = [typeof(Enum)])]
    public static void OnChangeState(Enum newState) {
        switch((States) newState) {
            case States.Fail2:
                Overlay.Instance.Death();
                break;
            case States.Won:
                Overlay.Instance.Clear();
                break;
        }
    }

    [JAPatch(typeof(scrUIController), "WipeToBlack", PatchType.Postfix, false)]
    [JAPatch(typeof(scnEditor), "ResetScene", PatchType.Postfix, false)]
    [JAPatch(typeof(scrController), "StartLoadingScene", PatchType.Postfix, false)]
    private static void OnGameStop() {
        Overlay.Instance.Hide();
    }

    [JAPatch(typeof(scrMistakesManager), nameof(scrMistakesManager.SetPlayerCount), PatchType.Postfix, false, MinVersion = 141)]
    private static void UpdatePlayersCount() => Overlay.Instance.OnChangePlayers();
}
