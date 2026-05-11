using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using JipperResourcePack.OverlayContents;
using TMPro;
using UnityEngine;

namespace JipperResourcePack.Jongyeol;

public class JOverlay : Overlay {
    public static new JOverlay Instance;
    public TextMeshProUGUI FPSText;
    public TextMeshProUGUI AuthorText;
    public TextMeshProUGUI StateText;
    public TextMeshProUGUI DeathText;
    public TextMeshProUGUI StartText;
    public TextMeshProUGUI TimingText;

    private List<float> _timings;
    private bool _purePerfect;
    private int _deathCount;
    private int _lastDeath = -1;
    private int _pseudoFloor = -1;
    private float _lastCurKps = -1;
    private static LevelData LevelData => scnGame.instance ? scnGame.instance.levelData : null;
    private float _fpsTime;
    private bool _perToCom;

    public JOverlay() {
        Instance = this;
    }

    protected override void InitializeStatus() {
        base.InitializeStatus();
        SetupMainText("FPS", ref FPSText);
        SetupMainText("Author", ref AuthorText);
        SetupMainText("State", ref StateText);
        SetupMainText("Checkpoint", ref CheckpointText);
        SetupMainText("Death", ref DeathText);
        SetupMainText("Start", ref StartText);
        SetupMainText("Timing", ref TimingText);
    }

    public override void SetupLocationMain() {
        if(!FPSText) return;
        int y = -15;
        bool checkAuto = !JStatus.Settings.RemoveNotRequireInAuto || !RDC.auto;
        SetupLocationMainText(FPSText, JStatus.Settings.ShowFPS, ref y);
        SetupLocationMainText(AuthorText, !string.IsNullOrEmpty(LevelData?.author) && JStatus.Settings.ShowAuthor, ref y);
        SetupLocationMainText(ProgressText, JStatus.Settings.ShowProgress, ref y);
        SetupLocationMainText(AccuracyText, checkAuto && JStatus.Settings.ShowAccuracy, ref y);
        SetupLocationMainText(XAccuracyText, checkAuto && JStatus.Settings.ShowXAccuracy, ref y);
        SetupLocationMainText(TimeText, JStatus.Settings.ShowMusicTime, ref y);
        SetupLocationMainText(MapTimeText, JStatus.Settings.ShowMapTime, ref y);
        checkpoints ??= scrLevelMaker.instance.listFloors.FindAll(floor => floor.GetComponent<ffxCheckpoint>()).Select(floor => floor.seqID).ToArray();
        SetupLocationMainText(CheckpointText, checkAuto && JStatus.Settings.ShowCheckpoint && checkpoints.Length > 0, ref y);
        SetupLocationMainText(BestText, checkAuto && JStatus.Settings.ShowBest, ref y);
        SetupLocationMainText(StateText, JStatus.Settings.ShowState, ref y);
        SetupLocationMainText(DeathText, scrController.instance.noFail && JStatus.Settings.ShowDeath, ref y);
        SetupLocationMainText(StartText, startTile != 0 && JStatus.Settings.ShowStart, ref y);
        SetupLocationMainText(TimingText, checkAuto && JStatus.Settings.ShowTiming, ref y);
        UpdateProgress();
        UpdateAccuracy();
        UpdateTime();
        UpdateAuthor();
        UpdateState();
        UpdateDeath();
        UpdateStart();
        if(_timings != null) return;
        _timings = [];
        UpdateTiming(0);
        _timings.Clear();
    }

    public override void UpdateProgress() {
        if(!GameObject.activeSelf) return;
        if(_purePerfect) CheckPurePerfect();
        base.UpdateProgress();
        UpdateState();
        UpdateDeath();
    }

    public override void UpdateProgressText() {
        int cur = scrController.instance.currentSeqID;
        int last = ADOBase.lm.listFloors.Count - 1;
        ProgressText.text = $"<color=white>Progress |</color> {cur} / {last}{(cur == last ? "" : $" [-{last - cur}]")} ({Math.Round(Progress * 100, 5)}%)";
        ProgressText.color = JStatus.Settings.ProgressColor.GetColor(Progress);
    }

    public override void UpdateAccuracy() {
        if(!GameObject.activeSelf) return;
        float xacc = scrController.instance.mistakesManager?.percentXAcc ?? 1;
        xacc.SetIfNaN(1);
        if(OverlayContents.Status.Settings.ShowAccuracy) {
            float acc = scrController.instance.mistakesManager?.percentAcc ?? 1;
            float maxAcc = 1 + (scrController.instance.currentSeqID - noCheckStartTile + 1) * 0.0001f;
            AccuracyText.text = $"<color=white>Accuracy |</color> {Math.Round(acc * 100, 4)}%";
            AccuracyText.color = JStatus.Settings.AccuracyColor.GetColor(xacc == 1 ? 1 : acc / maxAcc);
        }
        if(OverlayContents.Status.Settings.ShowXAccuracy) {
            XAccuracyText.text = $"<color=white>X-Accuracy |</color> {Math.Round(xacc * 100, 4)}%";
            XAccuracyText.color = GetColor(xacc);
        }
    }

    public override void UpdateTime() {
        if(!GameObject.activeSelf || !OverlayContents.Status.Instance.Enabled || death) return;
        bool requireMusicToMap = false;
        if(JStatus.Settings.ShowMusicTime) {
            AudioSource song = scrConductor.instance.song;
            if(!song?.clip && JStatus.Settings.ShowMapTimeIfNotMusic) requireMusicToMap = true;
            else {
                float time = song.time;
                float totalTime = song.clip?.length ?? 0;
                if(time > 0) songPlaying = true;
                else if(time == 0 && songPlaying) time = totalTime;
                TimeSpan now = TimeSpan.FromSeconds(time);
                TimeSpan length = TimeSpan.FromSeconds(totalTime);
                TimeText.text = $@"<color=white>{(JStatus.Settings.TimeTextType == TimeTextType.Korean ? "음악 시간" : "Music Time")} |</color> {now:m\:ss\.f}~{length:m\:ss\.f}";
                TimeText.color = JStatus.Settings.MusicTimeColor.GetColor(time / totalTime);
            }
        }
        if(JStatus.Settings.ShowMapTime || requireMusicToMap) {
            float time = (float) (scrConductor.instance.addoffset + scrConductor.instance.songposition_minusi);
            float totalTime = (float) scrLevelMaker.instance.listFloors.Last().entryTime;
            if(time < 0) time = 0;
            else if(time > totalTime) time = totalTime;
            if(!JStatus.Settings.ShowMapTime && !requireMusicToMap) return;
            TimeSpan now = TimeSpan.FromSeconds(time);
            TimeSpan length = TimeSpan.FromSeconds(totalTime);
            string text = $@"<color=white>{(JStatus.Settings.TimeTextType == TimeTextType.Korean ? "맵 시간" : "Map Time")} |</color> {now:m\:ss\.f}~{length:m\:ss\.f}";
            if(JStatus.Settings.ShowMapTime) {
                MapTimeText.text = text;
                MapTimeText.color = JStatus.Settings.MapTimeColor.GetColor(time / totalTime);
            }
            if(requireMusicToMap) {
                TimeText.text = text;
                TimeText.color = JStatus.Settings.MusicTimeColor.GetColor(time / totalTime);
            }
        }
    }

    public override Color UpdateComboColor(int combo) {
        if(_purePerfect) return PurePerfectColor;
        float value = (float) combo / (scrController.instance.currentSeqID - startTile + hit[0] + hit[6] + 1) * 2;
        if(value > 1) value = 1;
        return GetColor(value, 0.2f, false);
    }

    public override void UpdateBestText() {
        float best = curBest > Progress || autoOnceEnabled ? curBest : Progress;
        BestText.text = $"<color=white>Best |</color> {Math.Round(best * 100, 5)}%";
        BestText.color = JStatus.Settings.BestColor.GetColor(best);
    }

    private Color GetColor(float value, float middle = 0.5f, bool ppColor = true) {
        return value < middle         ? new Color(1 - value / middle * 0.0117647058823529f,value / middle, value / middle * 0.3019607843137255f) :
               value < 1f || !ppColor ? new Color(0.9882352941176471f - (value - middle) / (1 - middle) * 0.6156862745098039f, 1, 0.3019607843137255f + (value - middle) / (1 - middle) * 0.01f) :
                                        PurePerfectColor;
    }

    public void UpdateFPS(float deltaTime) {
        if(!JStatus.Settings.ShowFPS || !GameObject.activeSelf || (_fpsTime += deltaTime) < 0.01f) return;
        FPSText.text = $"FPS | {1 / deltaTime:F4}";
        _fpsTime %= 0.01f;
    }

    public void UpdateAuthor() {
        if(!JStatus.Settings.ShowAuthor || !GameObject.activeSelf) return;
        AuthorText.text = $"Author | {LevelData?.author ?? ""}";
    }

    public void UpdateState() {
        if(!JStatus.Settings.ShowState || !GameObject.activeSelf) return;
        string s;
        StateText.color = Color.white;
        if(scrController.instance.currentSeqID == startTile) s = "대기";
        else if(scrController.instance.currFloor && scrController.instance.currFloor.nextfloor && scrController.instance.currFloor.nextfloor.auto) {
            s = "자동 플레이 타일";
            StateText.color = new Color(1, 0.5f, 0);
        } else if(RDC.auto) {
            s = "자동 플레이";
            StateText.color = new Color(0.1058823529411765f, 1, 0);
        } else if(_purePerfect) {
            s = "완벽한 플레이";
            StateText.color = PurePerfectColor;
        } else {
            int[] hits = hit;
            if(_deathCount != 0) s = "완주";
            else if(hits[0] != 0) s = "클리어";
            else if(hits[1] != 0 || hits[5] != 0) s = "노미스";
            else s = "완벽주의";
        }
        if(scrController.instance.currentSeqID != ADOBase.lm.listFloors.Count) s += " 중";
        if(startTile != 0) s += "(중간에서 시작)";
        StateText.text = $"<color=white>State |</color> {s}";
    }

    private void CheckPurePerfect() {
        for(int i = 0; i < 10; i++) {
            if(i is 3 or 7) i++;
            if(hit[i] != 0) {
                _purePerfect = false;
                return;
            }
        }
    }

    public void UpdateDeath() {
        if(!JStatus.Settings.ShowDeath || !GameObject.activeSelf) return;
        if(_lastDeath != (_deathCount = hit[8] + hit[9])) {
            DeathText.text = $"<color=white>Death |</color> {_deathCount}";
            _lastDeath = _deathCount;
        }
        float max = (scrController.instance.currentSeqID - startTile) * 0.05f;
        DeathText.color = GetColor(1 - Math.Min(_deathCount, max) / max);
    }

    public void UpdateStart() {
        if(!JStatus.Settings.ShowStart || !GameObject.activeSelf || startTile != scrController.instance.currentSeqID) return;
        StartText.text = $"Start | {startTile} ({Math.Round(Progress*100, 5)}%)";
    }

    public void UpdateTiming(float timing) {
        if(!JStatus.Settings.ShowTiming || !GameObject.activeSelf) return;
        _timings.Add(timing);
        TimingText.text = $"<color=white>Timing |</color> {Math.Round(timing, 5)} ({Math.Round(_timings.Average(), 5)})";
        TimingText.color = GetColor(1 - Math.Min(Math.Abs(timing), 150) / 150);
    }

    public override void UpdateBPM() {
        if(!GameObject.activeSelf) return;
        scrFloor floor = scrController.instance.currFloor ?? scrController.instance.firstFloor;
        if(floor.seqID <= _pseudoFloor) return;
        scrConductor conductor = scrConductor.instance;
        float bpm = (float) (conductor.bpm * conductor.song.pitch * scrController.instance.speed);
        bool checkPseudo = Jbpm.Settings.CheckPseudo;
        float cbpm = 0;
        int count = 0;
        bool isPesudo = checkPseudo && CheckPseudo(floor, bpm, out cbpm, out count);
        if(!isPesudo) cbpm = floor.nextfloor ? (float) (60.0 / (floor.nextfloor.entryTime - floor.entryTime) * conductor.song.pitch) : bpm;
        float kps = cbpm / 60;
        if(isPesudo) kps *= count;
        if(lastTileBPM == bpm && lastCurBPM == cbpm && _lastCurKps == kps) return;
        BPMText.text = $"<color=white>TBPM | <color=#{ColorToHex(Jbpm.Settings.BpmColor.GetColor(bpm / Jbpm.Settings.BpmColorMax))}>{Math.Round(bpm, 2)}</color>\n" +
                       $"CBPM |</color> {Math.Round(cbpm, 2)}\n" +
                       $"<color=white>KPS |</color> {(isPesudo ? $"<color=#{ColorToHex(Jbpm.Settings.BpmColor.GetColor(cbpm * count / Jbpm.Settings.BpmColorMax))}>" : "")}{Math.Round(kps, 2)}{(isPesudo ? "</color>" : "")}";
        if(lastCurBPM != cbpm) BPMText.color = Jbpm.Settings.BpmColor.GetColor(cbpm / Jbpm.Settings.BpmColorMax);
        lastTileBPM = bpm;
        lastCurBPM = cbpm;
        _lastCurKps = kps;
    }

    public void PerfectToCombo() {
        if(_perToCom) return;
        ComboTitle.text = "Combo";
        _perToCom = true;
    }

    private bool CheckPseudo(scrFloor curFloor, float bpm, out float cbpm, out int count) {
        if(bpm < 200 || !scnGame.instance) {
            cbpm = count = 0;
            return false;
        }
        double allAngle = 0;
        double maxAngle = bpm < 400 ? 0.5236 : 1.0472;
        scrFloor floor = curFloor;
        int midSpin = 0;
        while(floor.nextfloor) {
            if(floor.midSpin) {
                floor = floor.nextfloor;
                midSpin++;
                continue;
            }
            double angle = floor.angleLength;
            allAngle += angle;
            if(allAngle > maxAngle && (bpm < 600 || allAngle - angle > 0.00000000000001 || !Check90(angle))) {
                if(angle < maxAngle && Math.Abs(angle - (floor.prevfloor?.angleLength ?? 0)) < 0.00000000000001) {
                    float speed = floor.speed;
                    do {
                        floor = floor.nextfloor;
                    } while(floor && Math.Abs(floor.angleLength - floor.prevfloor.angleLength) < 0.00000000000001 && Math.Abs(speed - floor.speed) < 0.00000000000001);
                    _pseudoFloor = floor.seqID - 1;
                    cbpm = count = 0;
                    return false;
                }
                break;
            }
            floor = floor.nextfloor;
        }
        if(Check90(curFloor.angleLength)) {
            int count2 = 0;
            scrFloor floor2 = curFloor;
            if(floor2.midSpin) floor2 = floor2.nextfloor;
            while(count2 < 3 && Check90(floor2.angleLength)) {
                count2++;
                floor2 = floor2.nextfloor;
                if(floor2.midSpin) floor2 = floor2.nextfloor;
                if(!floor2) break;
            }
            if(count2 < 3) {
                floor2 = curFloor;
                if(floor2.midSpin) floor2 = floor2.prevfloor;
                while(count2-- < 3 && Check90(floor2.angleLength)) {
                    count2++;
                    floor2 = floor2.prevfloor;
                    if(floor2.midSpin) floor2 = floor2.prevfloor;
                    if(!floor2) break;
                }
            }
            if(count2 >= 3) {
                cbpm = count = 0;
                return false;
            }
        }
        count = floor.seqID - curFloor.seqID + 1 - midSpin;
        if(count <= 1) {
            cbpm = 0;
            return false;
        }
        cbpm = floor.nextfloor ? (float) (60 / (floor.nextfloor.entryTime - curFloor.entryTime)) : (float) (60 / (floor.entryTime - curFloor.entryTime + 60 / bpm));
        cbpm *= scrConductor.instance?.song?.pitch ?? 1;
        _pseudoFloor = floor.seqID;
        return true;
    }

    private static bool Check90(double angle) {
        return Math.Abs(angle - 1.57079642638564) < 0.00000000000001;
    }

    public override void Show() {
        _perToCom = false;
        _purePerfect = true;
        _pseudoFloor = -1;
        ComboTitle.text = "Perfect";
        base.Show();
    }

    public override void Hide() {
        base.Hide();
        _timings = null;
    }
}