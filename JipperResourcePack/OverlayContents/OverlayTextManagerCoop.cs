using System;
using UnityEngine;

namespace JipperResourcePack.OverlayContents;

public class OverlayTextManagerCoop : IOverlayTextManager {
    public readonly PlayerData[] PlayerDatas;
    public float MaxProgress;
    public float CurBest = -1;
    public int CurCheck;
    public int LastCheckpoint = -1;
    
    public OverlayTextManagerCoop(Overlay overlay) {
        PlayerDatas = new PlayerData[scrPlayerManager.playerCount];
        overlay.ProgressText.color = Color.white;
        overlay.AccuracyText.color = Color.white;
        overlay.XAccuracyText.color = Color.white;
    }

    public void SetBest(float best) => CurBest = best;
    
    public void CacheProgress(scrPlanet planet) {
        if((object) planet == null) {
            float count = ADOBase.lm.listFloors.Count;
            for(int i = 0; i < PlayerDatas.Length; i++) 
                SetProgress(ref PlayerDatas[i], (scrPlayerManager.instance.allPlayers[i].planetarySystem.chosenPlanet.currfloor.seqID + 1) / count);
        } else {
            SetProgress(ref PlayerDatas[planet.player.playerID], (planet.currfloor.seqID + 1) / (float) ADOBase.lm.listFloors.Count);
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
                for(int i = 0; i < PlayerDatas.Length; i++)
                    SetAccuracy(ref PlayerDatas[i], overlay.NoCheckStartTile, index);
            else SetAccuracy(ref PlayerDatas[index], overlay.NoCheckStartTile, index);
            
            string[] strings = new string[PlayerDatas.Length + 1];
            strings[0] = "Accuracy";
            for(int i = 0; i < PlayerDatas.Length; i++) 
                strings[i + 1] = PlayerDatas[i].AccuracyString;
            overlay.AccuracyText.text = string.Concat(strings);
        }
        if(Status.Settings.ShowXAccuracy) {
            if(index == -1)
                for(int i = 0; i < PlayerDatas.Length; i++)
                    SetXAccuracy(ref PlayerDatas[i], index);
            else SetXAccuracy(ref PlayerDatas[index], index);
            
            string[] strings = new string[PlayerDatas.Length + 1];
            strings[0] = "XAccuracy";
            for(int i = 0; i < PlayerDatas.Length; i++) 
                strings[i + 1] = PlayerDatas[i].XAccuracyString;
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
        string[] strings = new string[PlayerDatas.Length + 1];
        strings[0] = "Progress";
        for(int i = 0; i < PlayerDatas.Length; i++) 
            strings[i + 1] = PlayerDatas[i].ProgressString;
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
    
    public struct PlayerData {
        public float Progress = 0;
        public string ProgressString;
        public string AccuracyString;
        public string XAccuracyString;
        public PlayerData() {
        }
    }
}