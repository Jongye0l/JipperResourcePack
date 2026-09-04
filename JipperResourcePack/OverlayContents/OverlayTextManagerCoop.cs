using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace JipperResourcePack.OverlayContents;

public class OverlayTextManagerCoop : IOverlayTextManager {
    public readonly PlayerData[] PlayerArray;
    protected readonly string[] ConcatBuffer;
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
        _timingSums = new double[PlayerArray.Length];
        _timingCounts = new int[PlayerArray.Length];
        LastTimings = new float[PlayerArray.Length];
        overlay.ProgressText.color = Color.white;
        overlay.AccuracyText.color = Color.white;
        overlay.PotentialAccuracyText.color = Color.white;
        overlay.XAccuracyText.color = Color.white;
        overlay.PotentialXAccuracyText.color = Color.white;
        overlay.XScoreText.color = Color.white;
        overlay.PotentialXScoreText.color = Color.white;
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

    protected virtual void SetProgress(ref PlayerData pData, float progress) {
        pData.ProgressString = " | " + ColorToString(Status.Settings.ProgressColor.GetColor(progress)) + Math.Round(progress * 100, Status.Settings.ProgressDecimalPlaces) + "%</color>";
        if(MaxProgress < progress) MaxProgress = progress;
    }
    
    public void UpdateAccuracy(Overlay overlay, int index) {
        if(Status.Settings.ShowAccuracy) {
            if(index == -1)
                for(int i = 0; i < PlayerArray.Length; i++)
                    SetAccuracy(overlay, ref PlayerArray[i], i);
            else SetAccuracy(overlay, ref PlayerArray[index], index);

            PotentialTextType type = Status.Settings.AccuracyTextType;
            if(type != PotentialTextType.Potential) {
                string[] strings = ConcatBuffer;
                strings[0] = "Accuracy";
                for(int i = 0; i < PlayerArray.Length; i++)
                    strings[i + 1] = PlayerArray[i].AccuracyString;
                overlay.AccuracyText.text = string.Concat(strings);
            }
            if(type is PotentialTextType.Potential or PotentialTextType.Both) {
                string[] strings = ConcatBuffer;
                strings[0] = "P.Accuracy";
                for(int i = 0; i < PlayerArray.Length; i++)
                    strings[i + 1] = PlayerArray[i].PotentialAccuracyString;
                overlay.PotentialAccuracyText.text = string.Concat(strings);
            }
        }
        if(Status.Settings.ShowXAccuracy) {
            if(index == -1)
                for(int i = 0; i < PlayerArray.Length; i++)
                    SetXAccuracy(overlay, ref PlayerArray[i], i);
            else SetXAccuracy(overlay, ref PlayerArray[index], index);

            PotentialTextType type = Status.Settings.XAccuracyTextType;
            if(type != PotentialTextType.Potential) {
                string[] strings = ConcatBuffer;
                strings[0] = "XAccuracy";
                for(int i = 0; i < PlayerArray.Length; i++)
                    strings[i + 1] = PlayerArray[i].XAccuracyString;
                overlay.XAccuracyText.text = string.Concat(strings);
            }
            if(type is PotentialTextType.Potential or PotentialTextType.Both) {
                string[] strings = ConcatBuffer;
                strings[0] = "P.XAccuracy";
                for(int i = 0; i < PlayerArray.Length; i++)
                    strings[i + 1] = PlayerArray[i].PotentialXAccuracyString;
                overlay.PotentialXAccuracyText.text = string.Concat(strings);
            }
        }
        if(Status.Settings.ShowXScore && Status.XScoreSupported) {
            if(index == -1)
                for(int i = 0; i < PlayerArray.Length; i++)
                    SetXScore(overlay, ref PlayerArray[i], i);
            else SetXScore(overlay, ref PlayerArray[index], index);

            PotentialTextType type = Status.Settings.XScorePotentialTextType;
            if(type != PotentialTextType.Potential) {
                string[] strings = ConcatBuffer;
                strings[0] = "XScore";
                for(int i = 0; i < PlayerArray.Length; i++)
                    strings[i + 1] = PlayerArray[i].XScoreString;
                overlay.XScoreText.text = string.Concat(strings);
            }
            if(type is PotentialTextType.Potential or PotentialTextType.Both) {
                string[] strings = ConcatBuffer;
                strings[0] = "P.XScore";
                for(int i = 0; i < PlayerArray.Length; i++)
                    strings[i + 1] = PlayerArray[i].PotentialXScoreString;
                overlay.PotentialXScoreText.text = string.Concat(strings);
            }
        }
    }

    private static int GetSeqID(int i) => scrPlayerManager.instance.allPlayers[i].planetarySystem.chosenPlanet.currfloor.seqID;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SetXScore(Overlay overlay, ref PlayerData pData, int i) {
        int perfectValue = HitMargin.XPerfect.ToXScore();
        int seqID = GetSeqID(i);
        int remaining = overlay.GetRemainingTiles(seqID);
        int xScore = scrMistakesManager.marginTrackers[i].xScore;
        int maxXScore = (seqID - scrMistakesManager.marginTrackers[i].GetHits(HitMargin.Midspin)) * perfectValue;
        int potentialXScore = xScore + remaining * perfectValue;
        int totalXScore = maxXScore + remaining * perfectValue;
        PotentialTextType type = Status.Settings.XScorePotentialTextType;
        if(type != PotentialTextType.Potential) {
            string value = ColorToString(Status.Settings.XScoreColor.GetColor(maxXScore == 0 ? 1 : (float) xScore / maxXScore)) + Status.GetXScoreText(xScore, maxXScore);
            if(type == PotentialTextType.BothInOneLine) value += " (" + Status.GetXScoreText(potentialXScore, totalXScore) + ")";
            pData.XScoreString = " | " + value + "</color>";
        }
        if(type is PotentialTextType.Potential or PotentialTextType.Both)
            pData.PotentialXScoreString = " | " + ColorToString(Status.Settings.XScoreColor.GetColor(totalXScore == 0 ? 1 : (float) potentialXScore / totalXScore)) +
                                          Status.GetXScoreText(potentialXScore, totalXScore) + "</color>";
    }

    private void SetAccuracy(Overlay overlay, ref PlayerData pData, int i) {
        scrMarginTracker tracker = scrMistakesManager.marginTrackers[i];
        int seqID = GetSeqID(i);
        int remaining = overlay.GetRemainingTiles(seqID);
        float acc = tracker.percentAcc;
        float xacc = tracker.percentXAcc.SetIfNaN(1);
        float potentialAcc = Status.GetPotentialAccuracy(tracker.hitMarginsCount, acc, Status.GetJudgedTiles(tracker.hitMarginsCount, seqID), remaining);
        float maxAcc = 1 + (seqID - overlay.NoCheckStartTile + 1) * 0.0001f;
        PotentialTextType type = Status.Settings.AccuracyTextType;
        int decimalPlaces = Status.Settings.AccuracyDecimalPlaces;
        // ReSharper disable CompareOfFloatsByEqualityOperator
        if(type != PotentialTextType.Potential) {
            string value = ColorToString(Status.Settings.AccuracyColor.GetColor(xacc == 1 ? 1 : acc / maxAcc)) + Math.Round(acc * 100, decimalPlaces) + "%";
            if(type == PotentialTextType.BothInOneLine) value += " (" + Math.Round(potentialAcc * 100, decimalPlaces) + "%)";
            pData.AccuracyString = " | " + value + "</color>";
        }
        if(type is PotentialTextType.Potential or PotentialTextType.Both)
            pData.PotentialAccuracyString = " | " + ColorToString(Status.Settings.AccuracyColor.GetColor(xacc == 1 ? 1 : potentialAcc / (maxAcc + remaining * 0.0001f))) +
                                            Math.Round(potentialAcc * 100, decimalPlaces) + "%</color>";
        // ReSharper restore CompareOfFloatsByEqualityOperator
    }

    private void SetXAccuracy(Overlay overlay, ref PlayerData pData, int i) {
        scrMarginTracker tracker = scrMistakesManager.marginTrackers[i];
        int seqID = GetSeqID(i);
        int remaining = overlay.GetRemainingTiles(seqID);
        float xacc = tracker.percentXAcc;
        if(float.IsNaN(xacc)) xacc = 1;
        float potentialXAcc = Status.GetPotentialXAccuracy(xacc, Status.GetJudgedTiles(tracker.hitMarginsCount, seqID), remaining);
        PotentialTextType type = Status.Settings.XAccuracyTextType;
        int decimalPlaces = Status.Settings.XAccuracyDecimalPlaces;
        if(type != PotentialTextType.Potential) {
            string value = ColorToString(Status.Settings.XAccuracyColor.GetColor(xacc)) + Math.Round(xacc * 100, decimalPlaces) + "%";
            if(type == PotentialTextType.BothInOneLine) value += " (" + Math.Round(potentialXAcc * 100, decimalPlaces) + "%)";
            pData.XAccuracyString = " | " + value + "</color>";
        }
        if(type is PotentialTextType.Potential or PotentialTextType.Both)
            pData.PotentialXAccuracyString = " | " + ColorToString(Status.Settings.XAccuracyColor.GetColor(potentialXAcc)) +
                                             Math.Round(potentialXAcc * 100, decimalPlaces) + "%</color>";
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

        StringBuilder sb = VersionSafe.GetSharedBuilder();
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
        public string PotentialAccuracyString;
        public string XAccuracyString;
        public string PotentialXAccuracyString;
        public string XScoreString;
        public string PotentialXScoreString;
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