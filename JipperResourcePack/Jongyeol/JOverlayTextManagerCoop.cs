using System;
using JipperResourcePack.OverlayContents;

namespace JipperResourcePack.Jongyeol;

public class JOverlayTextManagerCoop : OverlayTextManagerCoop, IJOverlayTextManager {
    
    public JOverlayTextManagerCoop(Overlay overlay) : base(overlay) {
    }

    protected override void SetProgress(ref PlayerData pData, float progress) {
        pData.Progress = progress;
        pData.ProgressString = $" | {ColorToString(JStatus.Settings.ProgressColor.GetColor(progress))}{Math.Round(progress * 100, 5)}%</color>";
    }

    protected override void SetAccuracy(ref PlayerData pData, int noCheckStartTile, int i) {
        float acc = scrMistakesManager.marginTrackers[i].percentAcc;
        float maxAcc = 1 + (scrPlayerManager.instance.allPlayers[i].planetarySystem.chosenPlanet.currfloor.seqID - noCheckStartTile + 1) * 0.0001f;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        pData.AccuracyString = $" | {ColorToString(Status.Settings.AccuracyColor.GetColor(scrMistakesManager.marginTrackers[i].percentXAcc.SetIfNaN(1) == 1 ? 1 : acc / maxAcc))}{Math.Round(acc * 100, 5)}%</color>";
    }
    
    protected override void SetXAccuracy(ref PlayerData pData, int i) {
        float xacc = scrMistakesManager.marginTrackers[i].percentXAcc;
        if(float.IsNaN(xacc)) xacc = 1;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        pData.XAccuracyString = $" | {ColorToString(Status.Settings.XAccuracyColor.GetColor(xacc))}{Math.Round(xacc * 100, 5)}%</color>";
    }
    
    public override void UpdateBestText(Overlay overlay) {
        float best = CurBest > MaxProgress || overlay.AutoOnceEnabled ? CurBest : MaxProgress;
        overlay.BestText.text = $"<color=white>Best |</color> {Math.Round(best * 100, 5)}%";
        overlay.BestText.color = JStatus.Settings.BestColor.GetColor(best);
    }
}