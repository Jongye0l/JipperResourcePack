using System;
using JALib.Tools;
using JipperResourcePack.OverlayContents;
using UnityEngine;

namespace JipperResourcePack.Jongyeol;

public class JOverlayTextManagerCoop : OverlayTextManagerCoop, IJOverlayTextManager {
    public readonly JPlayerData[] JPlayerArray;

    public JOverlayTextManagerCoop(JOverlay overlay) : base(overlay) {
        JPlayerArray = new JPlayerData[scrPlayerManager.playerCount];
        for(int i = 0; i < JPlayerArray.Length; i++) {
            JPlayerArray[i].DeathCache.Buffer = new char[48];
            JPlayerArray[i].StateCache.Buffer = new char[48];
        }
        overlay.DeathText.color = Color.white;
        overlay.StateText.color = Color.white;
    }

    protected override void SetProgress(ref PlayerData pData, float progress) {
        char[] buffer = pData.ProgressCache.Buffer;
        int index = WritePlayerStart(buffer, JStatus.Settings.ProgressColor.GetColor(progress));
        index = Overlay.WritePercent(buffer, index, progress * 100, 5);
        pData.ProgressCache.Length = WritePlayerEnd(buffer, index);
        if(MaxProgress < progress) MaxProgress = progress;
    }

    public void UpdateDeath(JOverlay overlay, scrPlanet planet) {
        if((object) planet == null)
            for(int i = 0; i < JPlayerArray.Length; i++)
                JPlayerArray[i].SetDeath(overlay, scrPlayerManager.instance.players[i].tapsOnThisFloor, scrMistakesManager.marginTrackers[i].hitMarginsCount);
        else JPlayerArray[planet.player.playerID].SetDeath(overlay, planet.currfloor.seqID, scrMistakesManager.marginTrackers[planet.player.playerID].hitMarginsCount);

        char[] buffer = Main.SharedBuffer;
        int index = Overlay.WriteText(buffer, "Death", 0);
        for(int i = 0; i < JPlayerArray.Length; i++) index = JPlayerArray[i].DeathCache.WriteTo(buffer, index);
        overlay.DeathText.SetCharArray(buffer, 0, index);
    }

    public void UpdateState(JOverlay overlay, scrPlanet planet) {
        if((object) planet == null) {
            for(int i = 0; i < JPlayerArray.Length; i++) 
                JPlayerArray[i].SetState(overlay, i, scrMistakesManager.marginTrackers[i].hitMarginsCount);
        } else JPlayerArray[planet.player.playerID].SetState(overlay, planet.player.playerID, scrMistakesManager.marginTrackers[planet.player.playerID].hitMarginsCount);

        char[] buffer = Main.SharedBuffer;
        int index = Overlay.WriteText(buffer, "State", 0);
        for(int i = 0; i < JPlayerArray.Length; i++) index = JPlayerArray[i].StateCache.WriteTo(buffer, index);
        if(overlay.StartTile != 0) index = Overlay.WriteText(buffer, " | (중간에서 시작)", index);
        overlay.StateText.SetCharArray(buffer, 0, index);
    }
    
    public void CheckPurePerfect(JOverlay overlay, scrPlanet planet) {
        bool isXPerfectSupport = VersionControl.releaseNumber >= 149;
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
        int tooLateIndex = VersionControl.releaseNumber < 149 ? 6 : 8;
        for(int i = 0; i < JPlayerArray.Length; i++) {
            int[] hit = scrMistakesManager.marginTrackers[i].hitMarginsCount;
            count += hit[0];
            count += hit[tooLateIndex];
        }
        return count;
    }

    public struct JPlayerData {
        public int Death;
        public CharCache DeathCache;
        public CharCache StateCache;

        public void SetDeath(JOverlay overlay, int currentTile, int[] hit) {
            Death = VersionControl.releaseNumber < 149 ? hit[8] + hit[9] : hit[10] + hit[11];
            float max = (currentTile - overlay.StartTile) * 0.05f;
            char[] buffer = DeathCache.Buffer;
            int index = WritePlayerStart(buffer, overlay.GetColor(1 - Math.Min(Death, max) / max));
            index = Overlay.WriteNumber(buffer, index, Death);
            DeathCache.Length = WritePlayerEnd(buffer, index);
        }

        public void SetState(JOverlay overlay, int index, int[] hit) {
            char[] buffer = StateCache.Buffer;
            int textIndex = Overlay.WriteText(buffer, " | ", 0);
            bool color = false;
            if(scrController.instance.state is States.Start or States.Countdown) textIndex = Overlay.WriteText(buffer, "대기", textIndex);
            else if(!RDC.auto && scrPlayerManager.instance.players[index].auto) {
                textIndex = Overlay.WriteText(buffer, "<color=red>리스폰 대기", textIndex);
                color = true;
            } else {
                scrFloor curFloor = scrPlayerManager.instance.players[index].planetarySystem.chosenPlanet.currfloor;
                if(curFloor && curFloor.nextfloor && curFloor.nextfloor.auto) {
                    textIndex = Overlay.WriteText(buffer, "<color=#ff7f00>자동 플레이 타일", textIndex);
                    color = true;
                } else if(RDC.auto) {
                    textIndex = Overlay.WriteText(buffer, "<color=#1bff00>자동 플레이", textIndex);
                    color = true;
                } else if(IsPurePerfect(hit)) {
                    textIndex = Overlay.WriteText(buffer, "<color=#ffda00>완벽한 플레이", textIndex);
                    color = true;
                } else {
                    if(Death > 0) textIndex = Overlay.WriteText(buffer, "완주", textIndex);
                    else if(hit[0] != 0) textIndex = Overlay.WriteText(buffer, "클리어", textIndex);
                    else if(hit[1] != 0 || hit[VersionControl.releaseNumber < 149 ? 5 : 7] != 0) textIndex = Overlay.WriteText(buffer, "노미스", textIndex);
                    else textIndex = Overlay.WriteText(buffer, "완벽주의", textIndex);
                }
            }
            if(scrController.instance.currentSeqID != ADOBase.lm.listFloors.Count) textIndex = Overlay.WriteText(buffer, " 중", textIndex);
            if(color) textIndex = Overlay.WriteText(buffer, "</color>", textIndex);
            StateCache.Length = textIndex;
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