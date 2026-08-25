using System;
using System.Text;
using JALib.Tools;
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
    
    public void UpdateDeath(JOverlay overlay, scrPlanet planet) {
        if((object) planet == null) 
            for(int i = 0; i < JPlayerArray.Length; i++) 
                JPlayerArray[i].SetDeath(overlay, scrPlayerManager.instance.players[i].tapsOnThisFloor, scrMistakesManager.marginTrackers[i].hitMarginsCount);
        else JPlayerArray[planet.player.playerID].SetDeath(overlay, planet.currfloor.seqID, scrMistakesManager.marginTrackers[planet.player.playerID].hitMarginsCount);
        
        string[] strings = ConcatBuffer;
        strings[0] = "Death";
        for(int i = 0; i < JPlayerArray.Length; i++) strings[i + 1] = JPlayerArray[i].DeathString;
        overlay.DeathText.text = string.Concat(strings);
    }

    public void UpdateState(JOverlay overlay, scrPlanet planet) {
        if((object) planet == null) {
            for(int i = 0; i < JPlayerArray.Length; i++) 
                JPlayerArray[i].SetState(overlay, i, scrMistakesManager.marginTrackers[i].hitMarginsCount);
        } else JPlayerArray[planet.player.playerID].SetState(overlay, planet.player.playerID, scrMistakesManager.marginTrackers[planet.player.playerID].hitMarginsCount);

        StringBuilder sb = VersionSafe.GetSharedBuilder();
        sb.Append("State");
        for(int i = 0; i < JPlayerArray.Length; i++) sb.Append(JPlayerArray[i].StateString);
        if(overlay.StartTile != 0) sb.Append(" | (중간에서 시작)");
        overlay.StateText.text = sb.ToString();
    }
    
    public void CheckPurePerfect(JOverlay overlay, scrPlanet planet) {
        bool isXPerfectSupport = VersionControl.releaseNumber >= 148;
        int max = isXPerfectSupport ? 12 : 10;
        if((object) planet == null) {
            for(int index = 0; index < JPlayerArray.Length; index++) {
                int[] hit = scrMistakesManager.marginTrackers[index].hitMarginsCount;
                for(int i = 0; i < max; i++) {
                    if(!isXPerfectSupport && i is 3 or 7) i++;
                    if(isXPerfectSupport) {
                        switch(i) {
                            case >= 3 and <= 5: i = 6;
                                break;
                            case 9: i++;
                                break;
                        }
                    }
                    if(hit[i] == 0) continue;
                    overlay.PurePerfect = false;
                    return;
                }
            }
        } else {
            int[] hit = scrMistakesManager.marginTrackers[planet.player.playerID].hitMarginsCount;
            for(int i = 0; i < max; i++) {
                if(!isXPerfectSupport && i is 3 or 7) i++;
                if(isXPerfectSupport) {
                    switch(i) {
                        case >= 3 and <= 5: i = 6;
                            break;
                        case 9: i++;
                            break;
                    }
                }
                if(hit[i] == 0) continue;
                overlay.PurePerfect = false;
                return;
            }
        }
    }

    public int GetTooJudgement(JOverlay _) {
        int count = 0;
        int tooLateIndex = VersionControl.releaseNumber < 148 ? 6 : 8;
        for(int i = 0; i < JPlayerArray.Length; i++) {
            int[] hit = scrMistakesManager.marginTrackers[i].hitMarginsCount;
            count += hit[0];
            count += hit[tooLateIndex];
        }
        return count;
    }

    public struct JPlayerData {
        public int Death;
        public string DeathString;
        public string StateString;

        public void SetDeath(JOverlay overlay, int currentTile, int[] hit) {
            Death = VersionControl.releaseNumber < 148 ? hit[8] + hit[9] : hit[10] + hit[11];
            float max = (currentTile - overlay.StartTile) * 0.05f;
            Color color = overlay.GetColor(1 - Math.Min(Death, max) / max);
            DeathString = " | <color=" + ColorUtility.ToHtmlStringRGB(color) + ">" + Death + "</color>";
        }

        public void SetState(JOverlay overlay, int index, int[] hit) {
            StringBuilder sb = VersionSafe.GetSharedBuilder();
            sb.Append(" | ");
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
                    else if(hit[1] != 0 || hit[VersionControl.releaseNumber < 148 ? 5 : 7] != 0) sb.Append("노미스");
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