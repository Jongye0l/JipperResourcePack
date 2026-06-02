using System;
using System.Text;
using JipperResourcePack.OverlayContents;
using UnityEngine;

namespace JipperResourcePack.Jongyeol;

public class JOverlayTextManagerCoop : OverlayTextManagerCoop, IJOverlayTextManager {
    public readonly JPlayerData[] JPlayerArray;
    
    public JOverlayTextManagerCoop(JOverlay overlay) : base(overlay) {
        JPlayerArray = new JPlayerData[scrPlayerManager.playerCount];
        overlay.DeathText.color = Color.white;
        overlay.StateText.color = Color.white;
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

    protected override void UpdateBestText(Overlay overlay) {
        float best = CurBest > MaxProgress || overlay.AutoOnceEnabled ? CurBest : MaxProgress;
        overlay.BestText.text = $"<color=white>Best |</color> {Math.Round(best * 100, 5)}%";
        overlay.BestText.color = JStatus.Settings.BestColor.GetColor(best);
    }
    
    public void UpdateDeath(JOverlay overlay, scrPlanet planet) {
        if((object) planet == null) 
            for(int i = 0; i < JPlayerArray.Length; i++) 
                JPlayerArray[i].SetDeath(overlay, scrPlayerManager.instance.players[i].tapsOnThisFloor, scrMistakesManager.marginTrackers[i].hitMarginsCount);
        else JPlayerArray[planet.player.playerID].SetDeath(overlay, planet.currfloor.seqID, scrMistakesManager.marginTrackers[planet.player.playerID].hitMarginsCount);
        
        string[] strings = new string[JPlayerArray.Length + 1];
        strings[0] = "Death";
        for(int i = 0; i < JPlayerArray.Length; i++) strings[i + 1] = JPlayerArray[i].DeathString;
        overlay.DeathText.text = string.Concat(strings);
    }

    public void UpdateState(JOverlay overlay, scrPlanet planet) {
        if((object) planet == null) {
            for(int i = 0; i < JPlayerArray.Length; i++) 
                JPlayerArray[i].SetState(overlay, i, scrMistakesManager.marginTrackers[i].hitMarginsCount);
        } else JPlayerArray[planet.player.playerID].SetState(overlay, planet.player.playerID, scrMistakesManager.marginTrackers[planet.player.playerID].hitMarginsCount);
        
        StringBuilder sb = new(32 * JPlayerArray.Length);
        sb.Append("State");
        for(int i = 0; i < JPlayerArray.Length; i++) sb.Append(JPlayerArray[i].StateString);
        if(overlay.StartTile != 0) sb.Append(" | (중간에서 시작)");
        overlay.StateText.text = sb.ToString();
    }
    
    public void CheckPurePerfect(JOverlay overlay, scrPlanet planet) {
        if((object) planet == null) {
            for(int index = 0; index < JPlayerArray.Length; index++) {
                int[] hit = scrMistakesManager.marginTrackers[index].hitMarginsCount;
                for(int i = 0; i < 10; i++) {
                    if(i is 3 or 7) i++;
                    if(hit[i] == 0) continue;
                    overlay.PurePerfect = false;
                    return;
                }
            }
        } else {
            int[] hit = scrMistakesManager.marginTrackers[planet.player.playerID].hitMarginsCount;
            for(int i = 0; i < 10; i++) {
                if(i is 3 or 7) i++;
                if(hit[i] == 0) continue;
                overlay.PurePerfect = false;
                return;
            }
        }
    }

    public int GetTooJudgement(JOverlay _) {
        int count = 0;
        for(int i = 0; i < JPlayerArray.Length; i++) {
            int[] hit = scrMistakesManager.marginTrackers[i].hitMarginsCount;
            count += hit[0];
            count += hit[6];
        }
        return count;
    }

    public struct JPlayerData {
        public int Death;
        public string DeathString;
        public string StateString;

        public void SetDeath(JOverlay overlay, int currentTile, int[] hit) {
            Death = hit[8] + hit[9];
            float max = (currentTile - overlay.StartTile) * 0.05f;
            Color color = overlay.GetColor(1 - Math.Min(Death, max) / max);
            DeathString = $" | <color={ColorUtility.ToHtmlStringRGB(color)}>{Death}</color>";
        }

        public void SetState(JOverlay overlay, int index, int[] hit) {
            StringBuilder sb = new(" | ");
            bool color = false;
            if(scrController.instance.state is States.Start or States.Countdown) sb.Append("대기");
            else if(!RDC.auto && scrPlayerManager.instance.players[index].auto) {
                sb.Append("<color=red>리스폰 대기");
                color = true;
            } else {
                scrFloor curFloor = scrPlayerManager.instance.players[index].planetarySystem.chosenPlanet.currfloor;
                if(curFloor && curFloor.nextfloor && curFloor.nextfloor.auto) {
                    sb.Append("<color=#ff7f00>자동 플레이 타일");
                    color = true;
                } else if(RDC.auto) {
                    sb.Append("<color=#1bff00>자동 플레이");
                    color = true;
                } else if(IsPurePerfect(hit)) {
                    sb.Append("<color=#ffda00>완벽한 플레이");
                    color = true;
                } else {
                    if(Death > 0) sb.Append("완주");
                    else if(hit[0] != 0) sb.Append("클리어");
                    else if(hit[1] != 0 || hit[5] != 0) sb.Append("노미스");
                    else sb.Append("완벽주의");
                }
            }
            if(scrController.instance.currentSeqID != ADOBase.lm.listFloors.Count) sb.Append(" 중");
            if(color) sb.Append("</color>");
            StateString = sb.ToString();
        }
        
        private static bool IsPurePerfect(int[] hit) {
            for(int i = 0; i < 10; i++) {
                if(i is 3 or 7) i++;
                if(hit[i] != 0) return false;
            }
            return true;
        }
    }
}