using System;
using JALib.Core;
using JALib.Core.Patch;
using UnityEngine;

namespace JipperResourcePack;

public class TimingScale : Feature {
    public static GameObject TimingScaleObject;

    public TimingScale() : base(Main.Instance, nameof(TimingScale), true, typeof(TimingScale)) {
    }
    
    protected override void OnEnable() {
        TimingScaleObject?.SetActive(true);
    }
    
    protected override void OnDisable() {
        TimingScaleObject?.SetActive(false);
    }
    
    [JAPatch(typeof(scrPlanet), "MoveToNextFloor", PatchType.Postfix, true)]
    private static void OnMoveToNextFloor() {
        Main.Instance.Log("MoveToNextFloor");
        Overlay.Instance.UpdateTimingScale();
    }
}