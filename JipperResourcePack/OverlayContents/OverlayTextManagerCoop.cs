using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace JipperResourcePack.OverlayContents;

public class OverlayTextManagerCoop : IOverlayTextManager {
    public readonly PlayerData[] PlayerArray;
    protected readonly string[] ConcatBuffer;
    private readonly StringBuilder _builder;
    private readonly double[] _timingSums;
    private readonly int[] _timingCounts;
    public readonly float[] LastTimings;
    public float MaxProgress;
    public float CurBest = -1;
    public int CurCheck;
    public int LastCheckpoint = -1;

    public OverlayTextManagerCoop(Overlay overlay) {
        PlayerArray = new PlayerData[scrPlayerManager.playerCount];
        ConcatBuffer = new string[PlayerArray.Length + 1];
        _builder = new StringBuilder(128 * PlayerArray.Length);
        _timingSums = new double[PlayerArray.Length];
        _timingCounts = new int[PlayerArray.Length];
        LastTimings = new float[PlayerArray.Length];
        overlay.ProgressText.color = Color.white;
        overlay.AccuracyText.color = Color.white;
        overlay.XAccuracyText.color = Color.white;
        overlay.XScoreText.color = Color.white;
        overlay.TimingText.color = Color.white;
        overlay.AvgTimingText.color = Color.white;
    }

    public void SetBest(float best) => CurBest = best;
    
    public void CacheProgress(scrPlanet planet) {
        if((object) planet == null) {
            float count = ADOBase.lm.listFloors.Count;
            for(int i = 0; i < PlayerArray.Length; i++) 
                SetProgress(ref PlayerArray[i], (scrPlayerManager.instance.allPlayers[i].planetarySystem.chosenPlanet.currfloor.seqID + 1) / count);
        } else {
            SetProgress(ref PlayerArray[planet.player.playerID], (planet.currfloor.seqID + 1) / (float) ADOBase.lm.listFloors.Count);
        }
    }

    private void SetProgress(ref PlayerData pData, float progress) {
        pData.ProgressString = " | " + ColorToString(Status.Settings.ProgressColor.GetColor(progress)) + Math.Round(progress * 100, Status.Settings.ProgressDecimalPlaces) + "%</color>";
        if(MaxProgress < progress) MaxProgress = progress;
    }
    
    public void UpdateAccuracy(Overlay overlay, int index) {
        if(Status.Settings.ShowAccuracy) {
            if(index == -1)
                for(int i = 0; i < PlayerArray.Length; i++)
                    SetAccuracy(ref PlayerArray[i], overlay.NoCheckStartTile, index);
            else SetAccuracy(ref PlayerArray[index], overlay.NoCheckStartTile, index);
            
            string[] strings = ConcatBuffer;
            strings[0] = "Accuracy";
            for(int i = 0; i < PlayerArray.Length; i++) 
                strings[i + 1] = PlayerArray[i].AccuracyString;
            overlay.AccuracyText.text = string.Concat(strings);
        }
        if(Status.Settings.ShowXAccuracy) {
            if(index == -1)
                for(int i = 0; i < PlayerArray.Length; i++)
                    SetXAccuracy(ref PlayerArray[i], index);
            else SetXAccuracy(ref PlayerArray[index], index);
            
            string[] strings = ConcatBuffer;
            strings[0] = "XAccuracy";
            for(int i = 0; i < PlayerArray.Length; i++) 
                strings[i + 1] = PlayerArray[i].XAccuracyString;
            overlay.XAccuracyText.text = string.Concat(strings);
        }
        if(Status.Settings.ShowXScore && Status.XScoreSupported) {
            if(index == -1)
                for(int i = 0; i < PlayerArray.Length; i++)
                    SetXScore(ref PlayerArray[i], i);
            else SetXScore(ref PlayerArray[index], index);
            
            string[] strings = ConcatBuffer;
            strings[0] = "XScore";
            for(int i = 0; i < PlayerArray.Length; i++) 
                strings[i + 1] = PlayerArray[i].XScoreString;
            overlay.XScoreText.text = string.Concat(strings);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SetXScore(ref PlayerData pData, int i) {
        int xScore = scrMistakesManager.marginTrackers[i].xScore;
        int maxXScore = (scrPlayerManager.instance.allPlayers[i].planetarySystem.chosenPlanet.currfloor.seqID - scrMistakesManager.marginTrackers[i].GetHits(HitMargin.Midspin)) * HitMargin.XPerfect.ToXScore();
        pData.XScoreString = " | " + ColorToString(Status.Settings.XScoreColor.GetColor(maxXScore == 0 ? 1 : (float) xScore / maxXScore)) + Status.GetXScoreText(xScore, maxXScore) + "</color>";
    }

    private void SetAccuracy(ref PlayerData pData, int noCheckStartTile, int i) {
        float acc = scrMistakesManager.marginTrackers[i].percentAcc;
        float maxAcc = 1 + (scrPlayerManager.instance.allPlayers[i].planetarySystem.chosenPlanet.currfloor.seqID - noCheckStartTile + 1) * 0.0001f;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        pData.AccuracyString = " | " + ColorToString(Status.Settings.AccuracyColor.GetColor(scrMistakesManager.marginTrackers[i].percentXAcc.SetIfNaN(1) == 1 ? 1 : acc / maxAcc)) + Math.Round(acc * 100, Status.Settings.AccuracyDecimalPlaces) + "%</color>";
    }
    
    private void SetXAccuracy(ref PlayerData pData, int i) {
        float xacc = scrMistakesManager.marginTrackers[i].percentXAcc;
        if(float.IsNaN(xacc)) xacc = 1;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        pData.XAccuracyString = " | " + ColorToString(Status.Settings.XAccuracyColor.GetColor(xacc)) + Math.Round(xacc * 100, Status.Settings.XAccuracyDecimalPlaces) + "%</color>";
    }

    public void UpdateProgress(Overlay overlay) {
        string[] strings = ConcatBuffer;
        strings[0] = "Progress";
        for(int i = 0; i < PlayerArray.Length; i++) 
            strings[i + 1] = PlayerArray[i].ProgressString;
        overlay.ProgressText.text = string.Concat(strings);
    }
    
    public void UpdateProgressBar(Overlay overlay) {
        ProgressBar progressBar = overlay.ProgressBar;
        progressBar.LineTransform.SizeDeltaX(MaxProgress * 638);
        progressBar.BackgroundImage.color = Status.Settings.ProgressBarBackgroundColor.GetColor(MaxProgress);
        progressBar.LineImage.color = Status.Settings.ProgressBarColor.GetColor(MaxProgress);
        progressBar.BorderImage.color = Status.Settings.ProgressBarBorderColor.GetColor(MaxProgress);
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
        else if(CurBest > MaxProgress || overlay.AutoOnceEnabled) return;
        UpdateBestText(overlay);
    }
    
    public float GetProgress() => MaxProgress;

    protected void UpdateBestText(Overlay overlay) {
        float best = CurBest > MaxProgress || overlay.AutoOnceEnabled ? CurBest : MaxProgress;
        overlay.BestText.text = "<color=white>Best |</color> " + Math.Round(best * 100, Status.Settings.BestDecimalPlaces) + "%";
        overlay.BestText.color = Status.Settings.BestColor.GetColor(best);
    }

    protected static string ColorToString(in Color color) => "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">";
    
    public void SetupUnderTextLocation(Overlay overlay) {
        overlay.JudgementText.rectTransform.anchoredPosition = new Vector2(0, 85);
        overlay.TimingScaleText.rectTransform.anchoredPosition = new Vector2(0, 50 + 40 * Main.Settings.Size + 35 * scrPlayerManager.playerCount);
    }

    public void UpdateJudgement(Overlay overlay, int index) {
        if(index == -1) {
            for(int i = 0; i < PlayerArray.Length; i++) 
                PlayerArray[i].SetJudgement(i, scrMistakesManager.marginTrackers[i].hitMarginsCount);
        } else PlayerArray[index].SetJudgement(index, scrMistakesManager.marginTrackers[index].hitMarginsCount);

        StringBuilder sb = _builder;
        sb.Length = 0;
        for(int i = 0; i < PlayerArray.Length; i++) sb.Append(PlayerArray[i].JudgementText).Append('\n');
        sb.Length -= 1;
        overlay.JudgementText.text = sb.ToString();
    }
    
    public void UpdateTiming(Overlay overlay, float timing, int player) {
        if(player < 0 || player >= PlayerArray.Length) player = 0;
        _timingSums[player] += timing;
        _timingCounts[player]++;
        LastTimings[player] = timing;
        RefreshTiming(overlay);
    }

    public void RefreshTiming(Overlay overlay) {
        if(!Status.Settings.ShowTiming) return;
        TimingTextType type = Status.Settings.TimingTextType;
        int decimalPlaces = Status.Settings.TimingDecimalPlaces;
        for(int i = 0; i < PlayerArray.Length; i++) SetTiming(ref PlayerArray[i], i, type, decimalPlaces);
        if(type != TimingTextType.AvgTiming) {
            string[] strings = ConcatBuffer;
            strings[0] = "Timing";
            for(int i = 0; i < PlayerArray.Length; i++) strings[i + 1] = PlayerArray[i].TimingString;
            overlay.TimingText.text = string.Concat(strings);
        }
        if(type is not (TimingTextType.AvgTiming or TimingTextType.Both)) return;
        string[] avgStrings = ConcatBuffer;
        avgStrings[0] = "A.Timing";
        for(int i = 0; i < PlayerArray.Length; i++) avgStrings[i + 1] = PlayerArray[i].AvgTimingString;
        overlay.AvgTimingText.text = string.Concat(avgStrings);
    }

    private void SetTiming(ref PlayerData pData, int i, TimingTextType type, int decimalPlaces) {
        float timing = LastTimings[i];
        int count = _timingCounts[i];
        float average = count == 0 ? 0 : (float) (_timingSums[i] / count);
        string timingPrefix = " | " + ColorToString(Status.GetTimingColor(timing)) + Math.Round(timing, decimalPlaces);
        pData.TimingString = type == TimingTextType.BothInOneLine ?
                                 timingPrefix + " (" + Math.Round(average, decimalPlaces) + ")</color>" :
                                 timingPrefix + "</color>";
        pData.AvgTimingString = " | " + ColorToString(Status.GetTimingColor(average)) + Math.Round(average, decimalPlaces) + "</color>";
    }

    public struct PlayerData {
        public string ProgressString;
        public string AccuracyString;
        public string XAccuracyString;
        public string XScoreString;
        public string TimingString;
        public string AvgTimingString;
        public string JudgementText;

        public void SetJudgement(int i, int[] hits) {
            JudgementText = scrPlayerManager.instance.allPlayers[i].alive ? 
                                VersionSafe.WriteHitMarginText(hits, $"{ColorToString(scrPlayerManager.playerColors[i].ToRealColor())}P{i + 1} |</color> ", $"<color=#0000>P{i + 1} | </color>") : 
                                VersionSafe.WriteHitMarginTextWithoutColor(hits, $"<color=grey>P{i + 1} | ", $"</color><color=#0000>P{i + 1} | </color>");
        }
    }
}