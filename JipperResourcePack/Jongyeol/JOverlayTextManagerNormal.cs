using System;
using JipperResourcePack.OverlayContents;
using UnityEngine;

namespace JipperResourcePack.Jongyeol;

public class JOverlayTextManagerNormal : OverlayTextManagerNormal, IJOverlayTextManager {
    private int _death = -1;

    public override void UpdateAccuracy(Overlay overlay, int _) {
        float xacc = VersionSafe.GetPercentXAcc();
        if(float.IsNaN(xacc)) xacc = 1;
        if(Status.Settings.ShowAccuracy) {
            float acc = VersionSafe.GetPercentAcc();
            float maxAcc = 1 + (scrController.instance.currentSeqID - overlay.NoCheckStartTile + 1) * 0.0001f;
            overlay.AccuracyText.text = $"<color=white>Accuracy |</color> {Math.Round(acc * 100, 4)}%";
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            overlay.AccuracyText.color = JStatus.Settings.AccuracyColor.GetColor(xacc == 1 ? 1 : acc / maxAcc);
        }
        if(Status.Settings.ShowXAccuracy) {
            overlay.XAccuracyText.text = $"<color=white>X-Accuracy |</color> {Math.Round(xacc * 100, 4)}%";
            overlay.XAccuracyText.color = JStatus.Settings.XAccuracyColor.GetColor(xacc);
        }
    }

    public override void UpdateProgress(Overlay overlay) {
        int cur = scrController.instance.currentSeqID;
        int last = ADOBase.lm.listFloors.Count - 1;
        overlay.ProgressText.text = $"<color=white>Progress |</color> {cur} / {last}{(cur == last ? "" : $" [-{last - cur}]")} ({Math.Round(Progress * 100, 5)}%)";
        overlay.ProgressText.color = JStatus.Settings.ProgressColor.GetColor(Progress);
    }

    protected override void UpdateBestText(Overlay overlay) {
        float best = CurBest > Progress || overlay.AutoOnceEnabled ? CurBest : Progress;
        overlay.BestText.text = $"<color=white>Best |</color> {Math.Round(best * 100, 5)}%";
        overlay.BestText.color = JStatus.Settings.BestColor.GetColor(best);
    }
    
    public void UpdateDeath(JOverlay overlay, scrPlanet _) {
        int deathCount;
        if(_death != (deathCount = overlay.Hit[8] + overlay.Hit[9])) {
            overlay.DeathText.text = $"<color=white>Death |</color> {deathCount}";
            _death = deathCount;
        }
        float max = (scrController.instance.currentSeqID - overlay.StartTile) * 0.05f;
        overlay.DeathText.color = overlay.GetColor(1 - Math.Min(deathCount, max) / max);
    }

    public void UpdateState(JOverlay overlay, scrPlanet _) {
        string s;
        overlay.StateText.color = Color.white;
        if(scrController.instance.currentSeqID == overlay.StartTile) s = "대기";
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
            else if(hits[1] != 0 || hits[5] != 0) s = "노미스";
            else s = "완벽주의";
        }
        if(scrController.instance.currentSeqID != ADOBase.lm.listFloors.Count) s += " 중";
        if(overlay.StartTile != 0) s += "(중간에서 시작)";
        overlay.StateText.text = $"<color=white>State |</color> {s}";
    }
    
    public void CheckPurePerfect(JOverlay overlay, scrPlanet _) {
        int[] hit = overlay.Hit;
        for(int i = 0; i < 10; i++) {
            if(i is 3 or 7) i++;
            if(hit[i] != 0) {
                overlay.PurePerfect = false;
                return;
            }
        }
    }
    
    public int GetTooJudgement(JOverlay overlay) {
        return overlay.Hit[0] + overlay.Hit[6];
    }
}