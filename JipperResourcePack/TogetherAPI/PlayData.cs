using JALib.Tools;

namespace JipperResourcePack.TogetherAPI;

public class PlayData {
    private readonly object original;

    public PlayData(object original) {
        this.original = original;
    }

    public string Username => Together.Instance.userData_userName.GetValue<string>(original);
    public string DisplayName => Together.Instance.userData_displayName.GetValue<string>(original);
    public bool IsGameReady => Together.Instance.userData_isReady.GetValue<bool>(original);
    public float XAcc => Together.Instance.userData_xAcc.GetValue<float>(original);
    public int Fails => Together.Instance.userData_fails.GetValue<int>(original);
}