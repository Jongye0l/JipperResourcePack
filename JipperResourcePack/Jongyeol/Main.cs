using JALib.Core;
using JALib.Core.Setting;
using JipperResourcePack.Keyviewer;
using UnityEngine;

namespace JipperResourcePack.Jongyeol;

public class Main {
    public static bool ModeEnabled;
    private static readonly RDCheatCode CheatCode = new("jongyeol");
    private static JASetting Setting;

    public static bool CheckEnable(JASetting setting) {
        Setting = setting;
        return ModeEnabled = setting["Jongyeol"]?.ToObject<bool>() == true;
    }

    public static void Update(float deltaTime) {
        if(ModeEnabled) JOverlay.Instance.UpdateFPS(deltaTime);
        if(!CheatCode.CheckCheatCode()) return;
        Setting["Jongyeol"] = ModeEnabled = !ModeEnabled;
        JipperResourcePack.Main.Instance.SaveSetting();
        Overlay.Instance.Destroy();
        JipperResourcePack.Main.Instance.FeatureReset(ModeEnabled);
        if(ModeEnabled) new JOverlay();
        else new Overlay();
    }

    public static Feature[] GetFeatures() {
        ResourceChanger resourceChanger = new();
        ResourceChanger.ResourcePackName = "<size=80>Jipper Resource Pack</size><size=50> - Jongyeol Ver</size>";
        ResourceChanger.PlanetColor = new Color(0.62109375f, 0.7265625f, 1);
        ResourceChanger.TitleColor = new Color(0.5546875f, 0.86328125f, 0.96484375f);
        ResourceChanger.TileColor = new Color(0.88235295f, 0.9882353f, 1f);
        KeyViewer keyViewer = new();
        KeyViewer.Background = new Color(0.4392157f, 0.8f, 0.9372549f, 0.1960784f);
        KeyViewer.Outline = new Color(0.243137255f, 0.4862745f, 1);
        KeyViewer.RainColor = new Color(0.1254902f, 0.7176471f, 0.85882354f);
        return [new Status(), new BPM(), new Combo(), new Judgement(), new TimingScale(), resourceChanger, keyViewer];
    }
}