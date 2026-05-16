using System;

namespace JipperResourcePack.OverlayContents;

public class OverlayTextManagerNormal : IOverlayTextManager {
    public float Progress;
    public int CurCheck;
    public int LastCheckpoint = -1;
    public float CurBest = -1;

    public void SetBest(float best) => CurBest = best;
    
    public void CacheProgress(scrPlanet planet) {
        Progress = scrController.instance.percentComplete;
    }
    
    public virtual void UpdateAccuracy(Overlay overlay, int index) {
        float xacc = VersionSafe.GetPercentXAcc();
        if(float.IsNaN(xacc)) xacc = 1;
        if(Status.Settings.ShowAccuracy) {
            float acc = VersionSafe.GetPercentAcc();
            float maxAcc = 1 + (scrController.instance.currentSeqID - overlay.NoCheckStartTile + 1) * 0.0001f;
            overlay.AccuracyText.text = $"<color=white>Accuracy |</color> {Math.Round(acc * 100, 2)}%";
            overlay.AccuracyText.color = Status.Settings.AccuracyColor.GetColor(xacc == 1 ? 1 : acc / maxAcc);
        }
        if(Status.Settings.ShowXAccuracy) {
            overlay.XAccuracyText.text = $"<color=white>XAccuracy |</color> {Math.Round(xacc * 100, 2)}%";
            overlay.XAccuracyText.color = Status.Settings.XAccuracyColor.GetColor(xacc);
        }
    }

    public virtual void UpdateProgress(Overlay overlay) {
        overlay.ProgressText.text = $"<color=white>Progress |</color> {Math.Round(Progress * 100, 2)}%";
        overlay.ProgressText.color = Status.Settings.ProgressColor.GetColor(Progress);
    }
    
    public void UpdateProgressBar(Overlay overlay) {
        ProgressBar progressBar = overlay.ProgressBar;
        progressBar.LineTransform.SizeDeltaX(Progress * 638);
        progressBar.BackgroundImage.color = Status.Settings.ProgressBarBackgroundColor.GetColor(Progress);
        progressBar.LineImage.color = Status.Settings.ProgressBarColor.GetColor(Progress);
        progressBar.BorderImage.color = Status.Settings.ProgressBarBorderColor.GetColor(Progress);
    }
    
    public void UpdateCheckpoint(Overlay overlay) {
        bool updated = false;
        while(overlay.Checkpoints.Length > CurCheck && scrController.instance.currentSeqID >= overlay.Checkpoints[CurCheck]) {
            CurCheck++;
            updated = true;
        }
        if(LastCheckpoint == scrController.checkpointsUsed && !updated) return;
        overlay.CheckpointText.text = $"<color=white>CheckPoint |</color> {scrController.checkpointsUsed} ({CurCheck}/{overlay.Checkpoints.Length})";
        LastCheckpoint = scrController.checkpointsUsed;
    }
    
    public void UpdateBest(Overlay overlay) {
        if(RDC.auto && !overlay.AutoOnceEnabled) overlay.AutoOnceEnabled = true;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if(CurBest == -1) CurBest = PlayCount.GetData(overlay.LastHash)?.GetBest(overlay.StartProgress, overlay.LastMultiplier) ?? 0;
        else if(CurBest > Progress || overlay.AutoOnceEnabled) return;
        UpdateBestText(overlay);
    }
    
    public float GetProgress() => Progress;

    public virtual void UpdateBestText(Overlay overlay) {
        float best = CurBest > Progress || overlay.AutoOnceEnabled ? CurBest : Progress;
        overlay.BestText.text = $"<color=white>Best |</color> {Math.Round(best * 100, 2)}%";
        overlay.BestText.color = Status.Settings.BestColor.GetColor(best);
    }
}