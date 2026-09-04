using System;
using JALib.Tools;
using JipperResourcePack.OverlayContents;
using UnityEngine;

namespace JipperResourcePack.Jongyeol;

public class JOverlayTextManagerNormal : OverlayTextManagerNormal, IJOverlayTextManager {
    private int _death = -1;

    public override void UpdateProgress(Overlay overlay) {
        int cur = scrController.instance.currentSeqID;
        int last = ADOBase.lm.listFloors.Count - 1;
        overlay.ProgressText.text = $"<color=white>Progress |</color> {cur} / {last}{(cur == last ? "" : $" [-{last - cur}]")} ({Math.Round(Progress * 100, 5)}%)";
        overlay.ProgressText.color = JStatus.Settings.ProgressColor.GetColor(Progress);
    }

    public void UpdateDeath(JOverlay overlay, scrPlanet _) {
        int deathCount;
        if(_death != (deathCount = VersionControl.releaseNumber < 149 ? overlay.Hit[8] + overlay.Hit[9] : overlay.Hit[10] + overlay.Hit[11])) {
            overlay.DeathText.text = "<color=white>Death |</color> " + deathCount;
            _death = deathCount;
        }
        float max = (scrController.instance.currentSeqID - overlay.StartTile) * 0.05f;
        overlay.DeathText.color = overlay.GetColor(1 - Math.Min(deathCount, max) / max);
    }

    public void UpdateState(JOverlay overlay, scrPlanet _) {
        string s;
        overlay.StateText.color = Color.white;
        if(scrController.instance.state is States.Start or States.Countdown) s = "대기";
        else if(scrController.instance.currFloor && scrController.instance.currFloor.nextfloor && scrController.instance.currFloor.nextfloor.auto) {
            s = "자동 플레이 타일";
            overlay.StateText.color = new Color(1, 0.5f, 0);
        } else if(RDC.auto) {
            s = "자동 플레이";
            overlay.StateText.color = new Color(0.1058823529411765f, 1, 0);
        } else if(overlay.PurePerfect) {
            s = "완벽한 플레이";
            overlay.StateText.color = overlay.PurePerfectColor;
        } else {
            int[] hits = overlay.Hit;
            if(_death > 0) s = "완주";
            else if(hits[0] != 0) s = "클리어";
            else if(hits[1] != 0 || hits[VersionControl.releaseNumber < 149 ? 5 : 7] != 0) s = "노미스";
            else s = "완벽주의";
        }
        if(scrController.instance.currentSeqID != ADOBase.lm.listFloors.Count) s += " 중";
        if(overlay.StartTile != 0) s += "(중간에서 시작)";
        overlay.StateText.text = "<color=white>State |</color> " + s;
    }
    
    public void CheckPurePerfect(JOverlay overlay, scrPlanet _) {
        int[] hit = overlay.Hit;
        bool isXPerfectSupport = VersionControl.releaseNumber >= 149;
        int max = isXPerfectSupport ? 12 : 10;
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
            if(hit[i] != 0) {
                overlay.PurePerfect = false;
                return;
            }
        }
    }
    
    public int GetTooJudgement(JOverlay overlay) {
        return VersionControl.releaseNumber < 149 ? overlay.Hit[0] + overlay.Hit[6] : overlay.Hit[0] + overlay.Hit[8];
    }
}