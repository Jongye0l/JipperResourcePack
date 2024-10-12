using JALib.Core;
using UnityEngine;

namespace JipperResourcePack;

public class Attempt : Feature {
    public static Attempt Instance;
    public static GameObject AttemptObject;

    public Attempt() : base(Main.Instance, nameof(Attempt), true, typeof(Attempt)) {
        Instance = this;
    }

    protected override void OnEnable() {
        AttemptObject.SetActive(true);
        if(scrLevelMaker.instance) Overlay.Instance.UpdateAttempts();
    }

    protected override void OnDisable() {
        AttemptObject.SetActive(false);
    }
}