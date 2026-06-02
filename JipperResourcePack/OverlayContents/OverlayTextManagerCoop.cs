using System;
using System.Text;
using UnityEngine;

namespace JipperResourcePack.OverlayContents;

public class OverlayTextManagerCoop : IOverlayTextManager {
    public readonly PlayerData[] PlayerArray;
    public float MaxProgress;
    public float CurBest = -1;
    public int CurCheck;
    public int LastCheckpoint = -1;

    public OverlayTextManagerCoop(Overlay overlay) {
        PlayerArray = new PlayerData[scrPlayerManager.playerCount];
        overlay.ProgressText.color = Color.white;
        overlay.AccuracyText.color = Color.white;
        overlay.XAccuracyText.color = Color.white;
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
        pData.Progress = progress;
        pData.ProgressString = $" | {ColorToString(Status.Settings.ProgressColor.GetColor(progress))}{Math.Round(progress * 100, 2)}%</color>";
        if(MaxProgress < progress) MaxProgress = progress;
    }
    
    public void UpdateAccuracy(Overlay overlay, int index) {
        if(Status.Settings.ShowAccuracy) {
            if(index == -1)
                for(int i = 0; i < PlayerArray.Length; i++)
                    SetAccuracy(ref PlayerArray[i], overlay.NoCheckStartTile, index);
            else SetAccuracy(ref PlayerArray[index], overlay.NoCheckStartTile, index);
            
            string[] strings = new string[PlayerArray.Length + 1];
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
            
            string[] strings = new string[PlayerArray.Length + 1];
            strings[0] = "XAccuracy";
            for(int i = 0; i < PlayerArray.Length; i++) 
                strings[i + 1] = PlayerArray[i].XAccuracyString;
            overlay.XAccuracyText.text = string.Concat(strings);
        }
    }

    protected virtual void SetAccuracy(ref PlayerData pData, int noCheckStartTile, int i) {
        float acc = scrMistakesManager.marginTrackers[i].percentAcc;
        float maxAcc = 1 + (scrPlayerManager.instance.allPlayers[i].planetarySystem.chosenPlanet.currfloor.seqID - noCheckStartTile + 1) * 0.0001f;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        pData.AccuracyString = $" | {ColorToString(Status.Settings.AccuracyColor.GetColor(scrMistakesManager.marginTrackers[i].percentXAcc.SetIfNaN(1) == 1 ? 1 : acc / maxAcc))}{Math.Round(acc * 100, 2)}%</color>";
    }
    
    protected virtual void SetXAccuracy(ref PlayerData pData, int i) {
        float xacc = scrMistakesManager.marginTrackers[i].percentXAcc;
        if(float.IsNaN(xacc)) xacc = 1;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        pData.XAccuracyString = $" | {ColorToString(Status.Settings.XAccuracyColor.GetColor(xacc))}{Math.Round(xacc * 100, 2)}%</color>";
    }

    public void UpdateProgress(Overlay overlay) {
        string[] strings = new string[PlayerArray.Length + 1];
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

    protected virtual void UpdateBestText(Overlay overlay) {
        float best = CurBest > MaxProgress || overlay.AutoOnceEnabled ? CurBest : MaxProgress;
        overlay.BestText.text = $"<color=white>Best |</color> {Math.Round(best * 100, 2)}%";
        overlay.BestText.color = Status.Settings.BestColor.GetColor(best);
    }

    protected static string ColorToString(in Color color) => $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
    
    public void SetupUnderTextLocation(Overlay overlay) {
        overlay.JudgementText.rectTransform.anchoredPosition = new Vector2(0, 85);
        overlay.TimingScaleText.rectTransform.anchoredPosition = new Vector2(0, 90 + 40 * Main.Settings.Size + 35 * scrPlayerManager.playerCount);
    }

    public void UpdateJudgement(Overlay overlay, int index) {
        if(index == 1) {
            for(int i = 0; i < PlayerArray.Length; i++) 
                PlayerArray[i].SetJudgement(i, scrMistakesManager.marginTrackers[i].hitMarginsCount);
        } else PlayerArray[index].SetJudgement(index, scrMistakesManager.marginTrackers[index].hitMarginsCount);
        
        StringBuilder sb = new(128 * PlayerArray.Length);
        for(int i = 0; i < PlayerArray.Length; i++) sb.Append(PlayerArray[i].JudgementText).Append('\n');
        sb.Length -= 1;
        overlay.JudgementText.text = sb.ToString();
    }
    
    public struct PlayerData {
        public float Progress;
        public string ProgressString;
        public string AccuracyString;
        public string XAccuracyString;
        public string JudgementText;

        public void SetJudgement(int i, int[] hits) {
            JudgementText = scrPlayerManager.instance.allPlayers[i].alive ? 
                $"{ColorToString(scrPlayerManager.playerColors[i].ToRealColor())}P{i + 1} |</color> {hits[9]} <color=red>{hits[0]} <color=#FF6F4E>{hits[1]} <color=#A0FF4E>{hits[2]} <color=#60FF4E>{hits[3] + hits[10]}</color> {hits[4]}</color> {hits[5]}</color> {hits[6]}</color> {hits[8]}    " :
                $"<color=grey>P{i + 1} | {hits[9]} {hits[0]} {hits[1]} {hits[2]} {hits[3] + hits[10]} {hits[4]} {hits[5]} {hits[6]} {hits[8]}    </color>";
        }
    }
}