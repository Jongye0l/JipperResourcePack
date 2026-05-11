using JALib.Core;
using JALib.Core.Setting;
using JipperResourcePack.KeyViewerContents;
using JipperResourcePack.OverlayContents;
using UnityEngine;

namespace JipperResourcePack.Jongyeol;

public class JMain {
    public static bool ModeEnabled;
    private static readonly RDCheatCode CheatCode = new("jongyeol");
    private static JASetting _setting;

    public static bool CheckEnable(JASetting setting) {
        _setting = setting;
        return ModeEnabled = setting["Jongyeol"]?.ToObject<bool>() == true;
    }

    public static void Update(float deltaTime) {
        if(ModeEnabled) JOverlay.Instance.UpdateFPS(deltaTime);
        if(!ADOBase.isLevelSelect || !CheatCode.CheckCheatCode()) return;
        _setting["Jongyeol"] = ModeEnabled = !ModeEnabled;
        Main.Instance.SaveSetting();
        Overlay.Instance.Destroy();
        Main.Instance.FeatureReset(ModeEnabled);
    }

    public static Feature[] GetFeatures() {
        ResourceChanger resourceChanger = new();
        ResourceChanger.ResourcePackName = "<size=80>Jipper Resource Pack</size><size=50> - Jongyeol Ver</size>";
        ResourceChanger.PlanetColor = new Color(0.62109375f, 0.7265625f, 1);
        ResourceChanger.TitleColor = new Color(0.5546875f, 0.86328125f, 0.96484375f);
        ResourceChanger.TileColor = new Color(0.88235295f, 0.9882353f, 1f);
        return [new JStatus(), new Jbpm(), new JCombo(), new Judgement(), new TimingScale(), new Attempt(), resourceChanger, new KeyViewer()];
    }
}