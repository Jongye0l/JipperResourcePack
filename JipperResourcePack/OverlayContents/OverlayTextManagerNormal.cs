using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace JipperResourcePack.OverlayContents;

public class OverlayTextManagerNormal : IOverlayTextManager {
    private double _timingSum;
    private int _timingCount;
    public float LastTiming;
    public float Progress;
    public int CurCheck;
    public int LastCheckpoint = -1;
    public float CurBest = -1;

    public void SetBest(float best) => CurBest = best;
    
    public void CacheProgress(scrPlanet planet) {
        Progress = scrController.instance.percentComplete;
    }
    
    public virtual void UpdateAccuracy(Overlay overlay, int _) {
        float xacc = VersionSafe.GetPercentXAcc();
        if(float.IsNaN(xacc)) xacc = 1;
        if(Status.Settings.ShowAccuracy) {
            float acc = VersionSafe.GetPercentAcc();
            float maxAcc = 1 + (scrController.instance.currentSeqID - overlay.NoCheckStartTile) * 0.0001f;
            overlay.AccuracyText.text = "<color=white>Accuracy |</color> " + Math.Round(acc * 100, Status.Settings.AccuracyDecimalPlaces) + "%";
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            overlay.AccuracyText.color = Status.Settings.AccuracyColor.GetColor(xacc == 1 ? 1 : acc / maxAcc);
        }
        if(Status.Settings.ShowXAccuracy) {
            overlay.XAccuracyText.text = "<color=white>XAccuracy |</color> " + Math.Round(xacc * 100, Status.Settings.XAccuracyDecimalPlaces) + "%";
            overlay.XAccuracyText.color = Status.Settings.XAccuracyColor.GetColor(xacc);
        }
        if(Status.Settings.ShowXScore && Status.XScoreSupported) UpdateXScore(overlay);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void UpdateXScore(Overlay overlay) {
        int xScore = scrMistakesManager.marginTrackers[0].xScore;
        int maxXScore = (scrController.instance.currentSeqID - overlay.Hit[(int) HitMargin.Midspin]) * HitMargin.XPerfect.ToXScore();
        overlay.XScoreText.text = "<color=white>XScore |</color> " + Status.GetXScoreText(xScore, maxXScore);
        overlay.XScoreText.color = Status.Settings.XScoreColor.GetColor(maxXScore == 0 ? 1 : (float) xScore / maxXScore);
    }

    public virtual void UpdateProgress(Overlay overlay) {
        overlay.ProgressText.text = "<color=white>Progress |</color> " + Math.Round(Progress * 100, Status.Settings.ProgressDecimalPlaces) + "%";
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
        overlay.CheckpointText.text = "<color=white>CheckPoint |</color> " + scrController.checkpointsUsed + " (" + CurCheck + "/" + overlay.Checkpoints.Length + ")";
        LastCheckpoint = scrController.checkpointsUsed;
    }
    
    public void UpdateBest(Overlay overlay) {
        if(RDC.auto && !overlay.AutoOnceEnabled) overlay.AutoOnceEnabled = true;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if(CurBest == -1) CurBest = PlayCount.GetData(overlay.LastHash)?.GetBest(overlay.StartProgress, overlay.LastMultiplier) ?? 0;
        else if(CurBest > Progress || overlay.AutoOnceEnabled) return;
        
        float best = CurBest > Progress || overlay.AutoOnceEnabled ? CurBest : Progress;
        overlay.BestText.text = "<color=white>Best |</color> " + Math.Round(best * 100, Status.Settings.BestDecimalPlaces) + "%";
        overlay.BestText.color = Status.Settings.BestColor.GetColor(best);
    }
    
    public float GetProgress() => Progress;

    public void SetupUnderTextLocation(Overlay overlay) {
        overlay.JudgementText.rectTransform.anchoredPosition = new Vector2(0, Judgement.Settings.LocationUp ? 85 : 5);
        overlay.TimingScaleText.rectTransform.anchoredPosition = new Vector2(0, 90 + 40 * Main.Settings.Size);
    }

    public void UpdateJudgement(Overlay overlay, int _) {
        int[] hits = overlay.Hit;
        overlay.JudgementText.text = VersionSafe.WriteHitMarginText(hits, null, null);
    }

    public void UpdateTiming(Overlay overlay, float timing, int _) {
        _timingSum += timing;
        _timingCount++;
        LastTiming = timing;
        RefreshTiming(overlay);
    }

    public void RefreshTiming(Overlay overlay) {
        if(!Status.Settings.ShowTiming) return;
        int decimalPlaces = Status.Settings.TimingDecimalPlaces;
        float average = _timingCount == 0 ? 0 : (float) (_timingSum / _timingCount);
        switch(Status.Settings.TimingTextType) {
            case TimingTextType.Timing:
                SetTiming(overlay, decimalPlaces);
                break;
            case TimingTextType.AvgTiming:
                SetAvgTiming(overlay, average, decimalPlaces);
                break;
            case TimingTextType.Both:
                SetTiming(overlay, decimalPlaces);
                SetAvgTiming(overlay, average, decimalPlaces);
                break;
            default:
                overlay.TimingText.text = "<color=white>Timing |</color> " + Math.Round(LastTiming, decimalPlaces) + " (" + Math.Round(average, decimalPlaces) + ")";
                overlay.TimingText.color = Status.GetTimingColor(LastTiming);
                break;
        }
    }

    private void SetTiming(Overlay overlay, int decimalPlaces) {
        overlay.TimingText.text = "<color=white>Timing |</color> " + Math.Round(LastTiming, decimalPlaces);
        overlay.TimingText.color = Status.GetTimingColor(LastTiming);
    }

    private static void SetAvgTiming(Overlay overlay, float average, int decimalPlaces) {
        overlay.AvgTimingText.text = "<color=white>A.Timing |</color> " + Math.Round(average, decimalPlaces);
        overlay.AvgTimingText.color = Status.GetTimingColor(average);
    }
}