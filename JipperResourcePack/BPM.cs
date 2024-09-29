using System;
using JALib.Core;
using JALib.Core.Patch;
using UnityEngine;

namespace JipperResourcePack;

public class BPM : Feature {
    public static GameObject BPMObject;

    public BPM() : this(nameof(BPM), null) {
    }

    public BPM(string name, Type settingType) : base(Main.Instance, name, true, typeof(BPM), settingType) {
    }

    protected override void OnEnable() {
        BPMObject?.SetActive(true);
    }
    
    protected override void OnDisable() {
        BPMObject?.SetActive(false);
    }

    [JAPatch(typeof(scrController), "Hit", PatchType.Postfix, true)]
    private static void OnHit() {
        Overlay.Instance.UpdateBPM();
    }
}