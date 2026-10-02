using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace JipperResourcePack.OverlayContents;

public class OverlayTextManagerCoop : IOverlayTextManager {
    public readonly PlayerData[] PlayerArray;
    private readonly double[] _timingSums;
    private readonly int[] _timingCounts;
    public readonly float[] LastTimings;
    public float MaxProgress;
    public float CurBest = -1;
    public int CurCheck;
    public int LastCheckpoint = -1;

    public OverlayTextManagerCoop(Overlay overlay) {
        PlayerArray = new PlayerData[scrPlayerManager.playerCount];
        _timingSums = new double[PlayerArray.Length];
        _timingCounts = new int[PlayerArray.Length];
        LastTimings = new float[PlayerArray.Length];
        for(int i = 0; i < PlayerArray.Length; i++) {
            ref PlayerData pData = ref PlayerArray[i];
            pData.ProgressCache.Buffer = new char[48];
            pData.AccuracyCache.Buffer = new char[64];
            pData.PotentialAccuracyCache.Buffer = new char[48];
            pData.XAccuracyCache.Buffer = new char[64];
            pData.PotentialXAccuracyCache.Buffer = new char[48];
            pData.XScoreCache.Buffer = new char[96];
            pData.PotentialXScoreCache.Buffer = new char[48];
            pData.TimingCache.Buffer = new char[64];
            pData.AvgTimingCache.Buffer = new char[48];
        }
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
        char[] buffer = pData.ProgressCache.Buffer;
        int index = WritePlayerStart(buffer, Status.Settings.ProgressColor.GetColor(progress));
        index = Overlay.WritePercent(buffer, index, progress * 100, Status.Settings.ProgressDecimalPlaces);
        pData.ProgressCache.Length = WritePlayerEnd(buffer, index);
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
                char[] buffer = Main.SharedBuffer;
                int textIndex = Overlay.WriteText(buffer, "Accuracy", 0);
                for(int i = 0; i < PlayerArray.Length; i++)
                    textIndex = PlayerArray[i].AccuracyCache.WriteTo(buffer, textIndex);
                overlay.AccuracyText.SetCharArray(buffer, 0, textIndex);
            }
            if(type is PotentialTextType.Potential or PotentialTextType.Both) {
                char[] buffer = Main.SharedBuffer;
                int textIndex = Overlay.WriteText(buffer, "P.Accuracy", 0);
                for(int i = 0; i < PlayerArray.Length; i++)
                    textIndex = PlayerArray[i].PotentialAccuracyCache.WriteTo(buffer, textIndex);
                overlay.PotentialAccuracyText.SetCharArray(buffer, 0, textIndex);
            }
        }
        if(Status.Settings.ShowXAccuracy) {
            if(index == -1)
                for(int i = 0; i < PlayerArray.Length; i++)
                    SetXAccuracy(overlay, ref PlayerArray[i], i);
            else SetXAccuracy(overlay, ref PlayerArray[index], index);

            PotentialTextType type = Status.Settings.XAccuracyTextType;
            if(type != PotentialTextType.Potential) {
                char[] buffer = Main.SharedBuffer;
                int textIndex = Overlay.WriteText(buffer, "XAccuracy", 0);
                for(int i = 0; i < PlayerArray.Length; i++)
                    textIndex = PlayerArray[i].XAccuracyCache.WriteTo(buffer, textIndex);
                overlay.XAccuracyText.SetCharArray(buffer, 0, textIndex);
            }
            if(type is PotentialTextType.Potential or PotentialTextType.Both) {
                char[] buffer = Main.SharedBuffer;
                int textIndex = Overlay.WriteText(buffer, "P.XAccuracy", 0);
                for(int i = 0; i < PlayerArray.Length; i++)
                    textIndex = PlayerArray[i].PotentialXAccuracyCache.WriteTo(buffer, textIndex);
                overlay.PotentialXAccuracyText.SetCharArray(buffer, 0, textIndex);
            }
        }
        if(Status.Settings.ShowXScore && VersionSafe.XScoreSupported) {
            if(index == -1)
                for(int i = 0; i < PlayerArray.Length; i++)
                    SetXScore(overlay, ref PlayerArray[i], i);
            else SetXScore(overlay, ref PlayerArray[index], index);

            PotentialTextType type = Status.Settings.XScorePotentialTextType;
            if(type != PotentialTextType.Potential) {
                char[] buffer = Main.SharedBuffer;
                int textIndex = Overlay.WriteText(buffer, "XScore", 0);
                for(int i = 0; i < PlayerArray.Length; i++)
                    textIndex = PlayerArray[i].XScoreCache.WriteTo(buffer, textIndex);
                overlay.XScoreText.SetCharArray(buffer, 0, textIndex);
            }
            if(type is PotentialTextType.Potential or PotentialTextType.Both) {
                char[] buffer = Main.SharedBuffer;
                int textIndex = Overlay.WriteText(buffer, "P.XScore", 0);
                for(int i = 0; i < PlayerArray.Length; i++)
                    textIndex = PlayerArray[i].PotentialXScoreCache.WriteTo(buffer, textIndex);
                overlay.PotentialXScoreText.SetCharArray(buffer, 0, textIndex);
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
            char[] buffer = pData.XScoreCache.Buffer;
            int index = WritePlayerStart(buffer, Status.Settings.XScoreColor.GetColor(maxXScore == 0 ? 1 : (float) xScore / maxXScore));
            index = Status.WriteXScoreText(buffer, index, xScore, maxXScore);
            if(type == PotentialTextType.BothInOneLine) {
                index = Overlay.WriteText(buffer, " (", index);
                index = Status.WriteXScoreText(buffer, index, potentialXScore, totalXScore);
                buffer[index++] = ')';
            }
            pData.XScoreCache.Length = WritePlayerEnd(buffer, index);
        }
        if(type is PotentialTextType.Potential or PotentialTextType.Both) {
            char[] buffer = pData.PotentialXScoreCache.Buffer;
            int index = WritePlayerStart(buffer, Status.Settings.XScoreColor.GetColor(totalXScore == 0 ? 1 : (float) potentialXScore / totalXScore));
            index = Status.WriteXScoreText(buffer, index, potentialXScore, totalXScore);
            pData.PotentialXScoreCache.Length = WritePlayerEnd(buffer, index);
        }
    }

    private void SetAccuracy(Overlay overlay, ref PlayerData pData, int i) {
        scrMarginTracker tracker = scrMistakesManager.marginTrackers[i];
        int seqID = GetSeqID(i);
        float acc = tracker.percentAcc;
        float xacc = tracker.percentXAcc.SetIfNaN(1);
        float potentialAcc = Status.GetPotentialAccuracy(tracker.hitMarginsCount, acc, seqID);
        float maxAcc = 1 + (seqID - overlay.NoCheckStartTile + 1) * 0.0001f;
        PotentialTextType type = Status.Settings.AccuracyTextType;
        int decimalPlaces = Status.Settings.AccuracyDecimalPlaces;
        // ReSharper disable CompareOfFloatsByEqualityOperator
        if(type != PotentialTextType.Potential) {
            char[] buffer = pData.AccuracyCache.Buffer;
            int index = WritePlayerStart(buffer, Status.Settings.AccuracyColor.GetColor(xacc == 1 ? 1 : acc / maxAcc)); // 20
            index = Overlay.WritePercent(buffer, index, acc * 100, decimalPlaces);
            if(type == PotentialTextType.BothInOneLine) index = WriteInOnePercent(buffer, index, potentialAcc * 100, decimalPlaces);
            pData.AccuracyCache.Length = WritePlayerEnd(buffer, index);
        }
        if(type is PotentialTextType.Potential or PotentialTextType.Both) {
            char[] buffer = pData.PotentialAccuracyCache.Buffer;
            int index = WritePlayerStart(buffer, Status.Settings.AccuracyColor.GetColor(xacc == 1 ? 1 : potentialAcc / (maxAcc + overlay.GetRemainingTiles(seqID) * 0.0001f)));
            index = Overlay.WritePercent(buffer, index, potentialAcc * 100, decimalPlaces);
            pData.PotentialAccuracyCache.Length = WritePlayerEnd(buffer, index);
        }
        // ReSharper restore CompareOfFloatsByEqualityOperator
    }

    private void SetXAccuracy(Overlay overlay, ref PlayerData pData, int i) {
        scrMarginTracker tracker = scrMistakesManager.marginTrackers[i];
        int seqID = GetSeqID(i);
        int remaining = overlay.GetRemainingTiles(seqID);
        float xacc = tracker.percentXAcc;
        if(float.IsNaN(xacc)) xacc = 1;
        float potentialXAcc = VersionSafe.GetPotentialXAccuracy(i, xacc, seqID, remaining);
        PotentialTextType type = Status.Settings.XAccuracyTextType;
        int decimalPlaces = Status.Settings.XAccuracyDecimalPlaces;
        if(type != PotentialTextType.Potential) {
            char[] buffer = pData.XAccuracyCache.Buffer;
            int index = WritePlayerStart(buffer, Status.Settings.XAccuracyColor.GetColor(xacc));
            index = Overlay.WritePercent(buffer, index, xacc * 100, decimalPlaces);
            if(type == PotentialTextType.BothInOneLine) index = WriteInOnePercent(buffer, index, potentialXAcc * 100, decimalPlaces);
            pData.XAccuracyCache.Length = WritePlayerEnd(buffer, index);
        }
        if(type is PotentialTextType.Potential or PotentialTextType.Both) {
            char[] buffer = pData.PotentialXAccuracyCache.Buffer;
            int index = WritePlayerStart(buffer, Status.Settings.XAccuracyColor.GetColor(potentialXAcc));
            index = Overlay.WritePercent(buffer, index, potentialXAcc * 100, decimalPlaces);
            pData.PotentialXAccuracyCache.Length = WritePlayerEnd(buffer, index);
        }
    }

    public void UpdateProgress(Overlay overlay) {
        char[] buffer = Main.SharedBuffer;
        int index = Overlay.WriteText(buffer, "Progress", 0);
        for(int i = 0; i < PlayerArray.Length; i++) 
            index = PlayerArray[i].ProgressCache.WriteTo(buffer, index);
        overlay.ProgressText.SetCharArray(buffer, 0, index);
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
        else if(CurBest > MaxProgress || overlay.AutoOnceEnabled) return;
        UpdateBestText(overlay);
    }
    
    public float GetProgress() => MaxProgress;

    protected void UpdateBestText(Overlay overlay) {
        float best = CurBest > MaxProgress || overlay.AutoOnceEnabled ? CurBest : MaxProgress;
        char[] buffer = Main.SharedBuffer;
        int index = Overlay.WriteLabel(buffer, "Best");
        index = Overlay.WritePercent(buffer, index, best * 100, Status.Settings.BestDecimalPlaces);
        overlay.BestText.SetCharArray(buffer, 0, index);
        overlay.BestText.color = Status.Settings.BestColor.GetColor(best);
    }

    protected static string ColorToString(in Color color) => "<color=#" + Overlay.ColorToHex(color) + ">";

    protected static int WritePlayerStart(char[] buffer, Color color) {
        int index = Overlay.WriteText(buffer, " | <color=#", 0);
        index = Overlay.WriteColorHex(buffer, index, color);
        buffer[index++] = '>';
        return index;
    }

    protected static int WritePlayerEnd(char[] buffer, int index) => Overlay.WriteText(buffer, "</color>", index);

    private static int WriteInOnePercent(char[] buffer, int index, double value, int decimalPlaces) {
        index = Overlay.WriteText(buffer, " (", index);
        index = Overlay.WritePercent(buffer, index, value, decimalPlaces);
        buffer[index++] = ')';
        return index;
    }
    
    public void SetupUnderTextLocation(Overlay overlay) {
        overlay.JudgementText.rectTransform.anchoredPosition = new Vector2(0, 85);
        overlay.TimingScaleText.rectTransform.anchoredPosition = new Vector2(0, 50 + 40 * Main.Settings.Size + 35 * scrPlayerManager.playerCount);
    }

    public void UpdateJudgement(Overlay overlay, int index) {
        if(index == -1) {
            for(int i = 0; i < PlayerArray.Length; i++) 
                PlayerArray[i].SetJudgement(i, scrMistakesManager.marginTrackers[i].hitMarginsCount);
        } else PlayerArray[index].SetJudgement(index, scrMistakesManager.marginTrackers[index].hitMarginsCount);

        char[] buffer = Main.SharedBuffer;
        int textIndex = 0;
        for(int i = 0; i < PlayerArray.Length; i++) {
            if(i != 0) buffer[textIndex++] = '\n';
            textIndex = PlayerArray[i].JudgementCache.WriteTo(buffer, textIndex);
        }
        overlay.JudgementText.SetCharArray(buffer, 0, textIndex);
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
        char[] buffer = Main.SharedBuffer;
        if(type != TimingTextType.AvgTiming) {
            int index = Overlay.WriteText(buffer, "Timing", 0);
            for(int i = 0; i < PlayerArray.Length; i++) index = PlayerArray[i].TimingCache.WriteTo(buffer, index);
            overlay.TimingText.SetCharArray(buffer, 0, index);
        }
        if(type is not (TimingTextType.AvgTiming or TimingTextType.Both)) return;
        int avgIndex = Overlay.WriteText(buffer, "A.Timing", 0);
        for(int i = 0; i < PlayerArray.Length; i++) avgIndex = PlayerArray[i].AvgTimingCache.WriteTo(buffer, avgIndex);
        overlay.AvgTimingText.SetCharArray(buffer, 0, avgIndex);
    }

    private void SetTiming(ref PlayerData pData, int i, TimingTextType type, int decimalPlaces) {
        float timing = LastTimings[i];
        int count = _timingCounts[i];
        float average = count == 0 ? 0 : (float) (_timingSums[i] / count);
        char[] buffer = pData.TimingCache.Buffer;
        int index = WritePlayerStart(buffer, Status.GetTimingColor(timing));
        index = Overlay.WriteRounded(buffer, index, timing, decimalPlaces);
        if(type == TimingTextType.BothInOneLine) {
            index = Overlay.WriteText(buffer, " (", index);
            index = Overlay.WriteRounded(buffer, index, average, decimalPlaces);
            buffer[index++] = ')';
        }
        pData.TimingCache.Length = WritePlayerEnd(buffer, index);
        buffer = pData.AvgTimingCache.Buffer;
        index = WritePlayerStart(buffer, Status.GetTimingColor(average));
        index = Overlay.WriteRounded(buffer, index, average, decimalPlaces);
        pData.AvgTimingCache.Length = WritePlayerEnd(buffer, index);
    }

    public struct PlayerData {
        public CharCache ProgressCache;
        public CharCache AccuracyCache;
        public CharCache PotentialAccuracyCache;
        public CharCache XAccuracyCache;
        public CharCache PotentialXAccuracyCache;
        public CharCache XScoreCache;
        public CharCache PotentialXScoreCache;
        public CharCache TimingCache;
        public CharCache AvgTimingCache;
        public CharCache JudgementCache;
        private string _alivePrefix;
        private string _alivePostfix;
        private string _deadPrefix;
        private string _deadPostfix;

        public void SetJudgement(int i, int[] hits) {
            if(_alivePrefix == null) {
                _alivePrefix = $"{ColorToString(scrPlayerManager.playerColors[i].ToRealColor())}P{i + 1} |</color> ";
                _alivePostfix = $"<color=#0000>P{i + 1} | </color>";
                _deadPrefix = $"<color=grey>P{i + 1} | ";
                _deadPostfix = $"</color><color=#0000>P{i + 1} | </color>";
            }
            StringBuilder sb = VersionSafe.GetSharedBuilder();
            if(scrPlayerManager.instance.allPlayers[i].alive) VersionSafe.AppendHitMarginText(sb, hits, _alivePrefix, _alivePostfix);
            else VersionSafe.AppendHitMarginTextWithoutColor(sb, hits, _deadPrefix, _deadPostfix);
            JudgementCache.Store(sb);
        }
    }
}
