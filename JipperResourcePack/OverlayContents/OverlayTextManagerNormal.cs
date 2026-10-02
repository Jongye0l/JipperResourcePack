using System;
using System.Runtime.CompilerServices;
using System.Text;
using JipperResourcePack.SettingTool;
using TMPro;
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
        float xacc = VersionSafe.GetPercentXAcc().SetIfNaN(1);
        int seqID = scrController.instance.currentSeqID;
        int remaining = overlay.GetRemainingTiles(seqID);
        if(Status.Settings.ShowAccuracy) {
            float acc = VersionSafe.GetPercentAcc();
            float potentialAcc = Status.GetPotentialAccuracy(overlay.Hit, acc, seqID);
            float maxAcc = 1 + (seqID - overlay.NoCheckStartTile) * 0.0001f;
            int decimalPlaces = Status.Settings.AccuracyDecimalPlaces;
            // ReSharper disable CompareOfFloatsByEqualityOperator
            SetDualText(Status.Settings.AccuracyTextType, overlay.AccuracyText, overlay.PotentialAccuracyText, "Accuracy",
                acc * 100, potentialAcc * 100, decimalPlaces,
                Status.Settings.AccuracyColor, xacc == 1 ? 1 : acc / maxAcc, xacc == 1 ? 1 : potentialAcc / (maxAcc + remaining * 0.0001f));
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }
        if(Status.Settings.ShowXAccuracy) {
            float potentialXAcc = VersionSafe.GetPotentialXAccuracy(0, xacc, seqID, remaining);
            int decimalPlaces = Status.Settings.XAccuracyDecimalPlaces;
            SetDualText(Status.Settings.XAccuracyTextType, overlay.XAccuracyText, overlay.PotentialXAccuracyText, "XAccuracy",
                xacc * 100, potentialXAcc * 100, decimalPlaces,
                Status.Settings.XAccuracyColor, xacc, potentialXAcc);
        }
        if(Status.Settings.ShowXScore && VersionSafe.XScoreSupported) UpdateXScore(overlay, VersionSafe.GetJudgedTiles(overlay.Hit, seqID), remaining);
    }

    protected static void SetDualText(PotentialTextType type, TextMeshProUGUI text, TextMeshProUGUI potentialText, string label,
                                      double value, double potentialValue, int decimalPlaces, ColorPerDictionary cpd, float fValue, float fPotentialValue) {
        char[] buffer = Main.SharedBuffer;
        if(type != PotentialTextType.Potential) {
            int index = Overlay.WriteLabel(buffer, label);
            index = Overlay.WritePercent(buffer, index, value, decimalPlaces);
            if(type == PotentialTextType.BothInOneLine) {
                index = Overlay.WriteText(buffer, " (", index);
                index = Overlay.WritePercent(buffer, index, potentialValue, decimalPlaces);
                buffer[index++] = ')';
            }
            text.SetCharArray(buffer, 0, index);
            text.color = cpd.GetColor(fValue);
        }
        if(type is not (PotentialTextType.Potential or PotentialTextType.Both)) return;
        int potentialIndex = WritePotentialLabel(buffer, label);
        potentialIndex = Overlay.WritePercent(buffer, potentialIndex, potentialValue, decimalPlaces);
        potentialText.SetCharArray(buffer, 0, potentialIndex);
        potentialText.color = cpd.GetColor(fPotentialValue);
    }

    private static int WritePotentialLabel(char[] buffer, string label) {
        int index = Overlay.WriteText(buffer, "<color=white>P.", 0);
        index = Overlay.WriteText(buffer, label, index);
        return Overlay.WriteText(buffer, " |</color> ", index);
    }

    private static void UpdateXScore(Overlay overlay, int judged, int remaining) {
        int perfectValue = HitMargin.XPerfect.ToXScore();
        int xScore = scrMistakesManager.marginTrackers[0].xScore;
        int maxXScore = judged * perfectValue;
        int potentialXScore = xScore + remaining * perfectValue;
        int totalXScore = maxXScore + remaining * perfectValue;
        PotentialTextType type = Status.Settings.XScorePotentialTextType;
        ColorPerDictionary cpd = Status.Settings.XScoreColor;
        char[] buffer = Main.SharedBuffer;
        if(type != PotentialTextType.Potential) {
            int index = Overlay.WriteLabel(buffer, "XScore");
            index = Status.WriteXScoreText(buffer, index, xScore, maxXScore);
            if(type == PotentialTextType.BothInOneLine) {
                index = Overlay.WriteText(buffer, " (", index);
                index = Status.WriteXScoreText(buffer, index, potentialXScore, totalXScore);
                buffer[index++] = ')';
            }
            overlay.XScoreText.SetCharArray(buffer, 0, index);
            overlay.XScoreText.color = cpd.GetColor(maxXScore == 0 ? 1 : (float) xScore / maxXScore);
        }
        if(type is not (PotentialTextType.Potential or PotentialTextType.Both)) return;
        int potentialIndex = WritePotentialLabel(buffer, "XScore");
        potentialIndex = Status.WriteXScoreText(buffer, potentialIndex, potentialXScore, totalXScore);
        overlay.PotentialXScoreText.SetCharArray(buffer, 0, potentialIndex);
        overlay.PotentialXScoreText.color = cpd.GetColor(totalXScore == 0 ? 1 : (float) potentialXScore / totalXScore);
    }

    public virtual void UpdateProgress(Overlay overlay) {
        char[] buffer = Main.SharedBuffer;
        int index = Overlay.WriteLabel(buffer, "Progress");
        index = Overlay.WritePercent(buffer, index, Progress * 100, Status.Settings.ProgressDecimalPlaces);
        overlay.ProgressText.SetCharArray(buffer, 0, index);
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
        char[] buffer = Main.SharedBuffer;
        int index = Overlay.WriteLabel(buffer, "CheckPoint");
        index = Overlay.WriteNumber(buffer, index, scrController.checkpointsUsed);
        index = Overlay.WriteText(buffer, " (", index);
        index = Overlay.WriteNumber(buffer, index, CurCheck);
        buffer[index++] = '/';
        index = Overlay.WriteNumber(buffer, index, overlay.Checkpoints.Length);
        buffer[index++] = ')';
        overlay.CheckpointText.SetCharArray(buffer, 0, index);
        LastCheckpoint = scrController.checkpointsUsed;
    }
    
    public void UpdateBest(Overlay overlay) {
        if(RDC.auto && !overlay.AutoOnceEnabled) overlay.AutoOnceEnabled = true;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if(CurBest == -1) CurBest = PlayCount.GetData(overlay.LastHash)?.GetBest(overlay.StartProgress, overlay.LastMultiplier) ?? 0;
        else if(CurBest > Progress || overlay.AutoOnceEnabled) return;
        
        float best = CurBest > Progress || overlay.AutoOnceEnabled ? CurBest : Progress;
        char[] buffer = Main.SharedBuffer;
        int index = Overlay.WriteLabel(buffer, "Best");
        index = Overlay.WritePercent(buffer, index, best * 100, Status.Settings.BestDecimalPlaces);
        overlay.BestText.SetCharArray(buffer, 0, index);
        overlay.BestText.color = Status.Settings.BestColor.GetColor(best);
    }
    
    public float GetProgress() => Progress;

    public void SetupUnderTextLocation(Overlay overlay) {
        overlay.JudgementText.rectTransform.anchoredPosition = new Vector2(0, Judgement.Settings.LocationUp ? 85 : 5);
        overlay.TimingScaleText.rectTransform.anchoredPosition = new Vector2(0, 90 + 40 * Main.Settings.Size);
    }

    public void UpdateJudgement(Overlay overlay, int _) {
        StringBuilder sb = VersionSafe.GetSharedBuilder();
        VersionSafe.AppendHitMarginText(sb, overlay.Hit, null, null);
        overlay.JudgementText.SetCharArray(Main.SharedBuffer, 0, Overlay.WriteBuilder(Main.SharedBuffer, sb, 0));
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
                char[] buffer = Main.SharedBuffer;
                int index = Overlay.WriteLabel(buffer, "Timing");
                index = Overlay.WriteRounded(buffer, index, LastTiming, decimalPlaces);
                index = Overlay.WriteText(buffer, " (", index);
                index = Overlay.WriteRounded(buffer, index, average, decimalPlaces);
                buffer[index++] = ')';
                overlay.TimingText.SetCharArray(buffer, 0, index);
                overlay.TimingText.color = Status.GetTimingColor(LastTiming);
                break;
        }
    }

    private void SetTiming(Overlay overlay, int decimalPlaces) {
        char[] buffer = Main.SharedBuffer;
        int index = Overlay.WriteLabel(buffer, "Timing");
        index = Overlay.WriteRounded(buffer, index, LastTiming, decimalPlaces);
        overlay.TimingText.SetCharArray(buffer, 0, index);
        overlay.TimingText.color = Status.GetTimingColor(LastTiming);
    }

    private static void SetAvgTiming(Overlay overlay, float average, int decimalPlaces) {
        char[] buffer = Main.SharedBuffer;
        int index = Overlay.WriteLabel(buffer, "A.Timing");
        index = Overlay.WriteRounded(buffer, index, average, decimalPlaces);
        overlay.AvgTimingText.SetCharArray(buffer, 0, index);
        overlay.AvgTimingText.color = Status.GetTimingColor(average);
    }
}