using System;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JipperResourcePack.Jongyeol;

public class JOverlay : Overlay {
    public new static JOverlay Instance;
    public TextMeshProUGUI FPSText;
    public TextMeshProUGUI AuthorText;
    public TextMeshProUGUI StateText;
    public TextMeshProUGUI CheckpointText;
    public TextMeshProUGUI DeathText;
    public TextMeshProUGUI StartText;
    public TextMeshProUGUI TimingText;

    private int lastCheckpoint;
    private List<float> timings;
    private bool purePerfect;
    private int death;
    private int lastDeath = -1;
    private int pseudoFloor = -1;
    private float lastCurKPS = -1;
    private LevelData levelData => scnGame.instance?.levelData;
    private float fpsTime;
    private bool perToCom;

    public JOverlay() {
        Instance = this;
    }

    protected override void InitializeMain() {
        base.InitializeMain();
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
        bool checkAuto = !Status.Settings.RemoveNotRequireInAuto || !RDC.auto;
        SetupLocationMainText(FPSText, Status.Settings.ShowFPS, ref y);
        SetupLocationMainText(AuthorText, !string.IsNullOrEmpty(levelData?.author) && Status.Settings.ShowAuthor, ref y);
        SetupLocationMainText(ProgressText, Status.Settings.ShowProgress, ref y);
        SetupLocationMainText(AccuracyText, checkAuto && Status.Settings.ShowAccuracy, ref y);
        SetupLocationMainText(XAccuracyText, checkAuto && Status.Settings.ShowXAccuracy, ref y);
        SetupLocationMainText(TimeText, Status.Settings.ShowMusicTime, ref y);
        SetupLocationMainText(MapTimeText, Status.Settings.ShowMapTime, ref y);
        SetupLocationMainText(StateText, Status.Settings.ShowState, ref y);
        SetupLocationMainText(CheckpointText, scrController.checkpointsUsed != 0 && Status.Settings.ShowCheckpoint, ref y);
        SetupLocationMainText(DeathText, scrController.instance.noFail && Status.Settings.ShowDeath, ref y);
        SetupLocationMainText(StartText, startTile != 0 && Status.Settings.ShowStart, ref y);
        SetupLocationMainText(TimingText, checkAuto && Status.Settings.ShowTiming, ref y);
    }

    public override void UpdateProgress() {
        if(!GameObject.activeSelf) return;
        base.UpdateProgress();
        UpdateState();
        UpdateCheckpoint();
        UpdateDeath();
    }

    public override void UpdateProgressText() {
        int cur = scrController.instance.currentSeqID;
        int last = ADOBase.lm.listFloors.Count - 1;
        ProgressText.text = $"<color=white>Progress |</color> {cur} / {last}{(cur == last ? "" : $" [-{last - cur}]")} ({Math.Round(Progress * 100, 5)}%)";
        ProgressText.color = Status.Settings.ProgressColor.GetColor(Progress);
    }

    public override void UpdateAccuracy() {
        if(!GameObject.activeSelf) return;
        float xacc = scrController.instance.mistakesManager?.percentXAcc ?? 1;
        xacc.SetIfNaN(1);
        if(JipperResourcePack.Status.Settings.ShowAccuracy) {
            float acc = scrController.instance.mistakesManager?.percentAcc ?? 1;
            float maxAcc = 1 + (scrController.instance.currentSeqID - startTile + 1) * 0.0001f;
            AccuracyText.text = $"<color=white>Accuracy |</color> {Math.Round(acc * 100, 4)}%";
            AccuracyText.color = Status.Settings.AccuracyColor.GetColor(xacc == 1 ? 1 : acc / maxAcc);
        }
        if(JipperResourcePack.Status.Settings.ShowXAccuracy) {
            XAccuracyText.text = $"<color=white>X-Accuracy |</color> {Math.Round(xacc * 100, 4)}%";
            XAccuracyText.color = GetColor(xacc);
        }
    }

    public override void UpdateTime() {
        if(!GameObject.activeSelf) return;
        scrConductor conductor = scrConductor.instance;
        if(JipperResourcePack.Status.Settings.ShowMusicTime) {
            AudioSource song = conductor.song;
            float time = song.clip? song.time : GetMaptime();
            float totalTime = (float) (song.clip?.length ?? ADOBase.lm.listFloors.Last().entryTime);
            if(time < 0) time = 0;
            else if(time > totalTime) time = totalTime;
            if(song.clip && time > 0) songPlaying = true;
            else if(time == 0 && songPlaying) {
                if(Math.Abs(lastTime - (int) (time * 10f)) <= 1) time = totalTime;
                else return;
            }
            TimeText.color = Status.Settings.MusicTimeColor.GetColor(time / totalTime);
            if(lastTime == (int) (time * 10f)) return;
            TimeSpan now = TimeSpan.FromSeconds(time);
            TimeSpan length = TimeSpan.FromSeconds(totalTime);
            TimeText.text = $@"<color=white>Time |</color> {now:m\:ss\.f}~{length:m\:ss\.f}";
            lastTime = (int) (time * 10f);
        }
        if(JipperResourcePack.Status.Settings.ShowMapTime) {
            float time = GetMaptime();
            float totalTime = (float) ADOBase.lm.listFloors.Last().entryTime;
            if(time < 0) time = 0;
            else if(time > totalTime) time = totalTime;
            MapTimeText.color = Status.Settings.MapTimeColor.GetColor(time / totalTime);
            if(lastMapTime == (int) (time * 10f)) return;
            TimeSpan now = TimeSpan.FromSeconds(time);
            TimeSpan length = TimeSpan.FromSeconds(totalTime);
            MapTimeText.text = $@"<color=white>MapTime |</color> {now:m\:ss\.f}~{length:m\:ss\.f}";
            lastMapTime = (int) (time * 10f);
        }
        return;

        float GetMaptime() => (float) (conductor.songposition_minusi - conductor.addoffset);
    }

    protected override Color UpdateComboColor(int combo) {
        if(purePerfect) CheckPurePerfect();
        if(purePerfect) return PurePerfectColor;
        float value = (float) combo / (scrController.instance.currentSeqID - startTile + hit[0] + hit[6] + 1) * 2;
        if(value > 1) value = 1;
        return GetColor(value, 0.2f, false);
    }

    private Color GetColor(float value, float middle = 0.5f, bool ppColor = true) {
        return value < middle         ? new Color(1 - value / middle * 0.0117647058823529f,value / middle, value / middle * 0.3019607843137255f) :
               value < 1f || !ppColor ? new Color(0.9882352941176471f - (value - middle) / (1 - middle) * 0.6156862745098039f, 1, 0.3019607843137255f + (value - middle) / (1 - middle) * 0.01f) :
                                        PurePerfectColor;
    }

    public void UpdateFPS(float deltaTime) {
        if(!Status.Settings.ShowFPS || !GameObject.activeSelf || (fpsTime += deltaTime) < 0.01f) return;
        FPSText.text = $"FPS | {1 / deltaTime:F4}";
        fpsTime %= 0.01f;
    }

    public void UpdateAuthor() {
        if(!Status.Settings.ShowAuthor || !GameObject.activeSelf) return;
        AuthorText.text = $"Author | {levelData?.author ?? ""}";
    }

    public void UpdateState() {
        if(!Status.Settings.ShowState || !GameObject.activeSelf) return;
        string s;
        StateText.color = Color.white;
        if(scrController.instance.currentSeqID == startTile) s = "대기";
        else if(scrController.instance.currFloor && scrController.instance.currFloor.nextfloor && scrController.instance.currFloor.nextfloor.auto) {
            s = "자동 플레이 타일";
            StateText.color = new Color(1, 0.5f, 0);
        } else if(RDC.auto) {
            s = "자동 플레이";
            StateText.color = new Color(0.1058823529411765f, 1, 0);
        } else if(purePerfect) {
            s = "완벽한 플레이";
            StateText.color = PurePerfectColor;
        } else {
            int[] hits = hit;
            if(death != 0) s = "완주";
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
                purePerfect = false;
                return;
            }
        }
    }

    public void UpdateCheckpoint() {
        if(!Status.Settings.ShowCheckpoint || lastCheckpoint == scrController.checkpointsUsed || !GameObject.activeSelf) return;
        CheckpointText.text = $"Checkpoint | {scrController.checkpointsUsed}";
        SetupLocationMain();
    }

    public void UpdateDeath() {
        if(!Status.Settings.ShowDeath || !GameObject.activeSelf) return;
        if(lastDeath != (death = hit[8] + hit[9])) DeathText.text = $"<color=white>Death |</color> {death}";
        float max = (scrController.instance.currentSeqID - startTile) * 0.05f;
        DeathText.color = GetColor(1 - Math.Min(death, max) / max);
    }

    public void UpdateStart() {
        if(!Status.Settings.ShowStart || !GameObject.activeSelf || startTile != scrController.instance.currentSeqID) return;
        StartText.text = $"Start | {startTile} ({Math.Round(Progress*100, 5)}%)";
    }

    public void UpdateTiming(float timing) {
        if(!Status.Settings.ShowTiming || !GameObject.activeSelf) return;
        timings.Add(timing);
        TimingText.text = $"<color=white>Timing |</color> {Math.Round(timing, 5)} ({Math.Round(timings.Average(), 5)})";
        TimingText.color = GetColor(1 - Math.Min(Math.Abs(timing), 150) / 150);
    }

    public override void UpdateBPM() {
        if(!GameObject.activeSelf) return;
        scrFloor floor = scrController.instance.currFloor ?? scrController.instance.firstFloor;
        if(floor.seqID <= pseudoFloor) return;
        scrConductor conductor = scrConductor.instance;
        float bpm = (float) (conductor.bpm * conductor.song.pitch * scrController.instance.speed);
        bool checkPseudo = BPM.Settings.CheckPseudo;
        float cbpm = 0;
        int count = 0;
        bool isPesudo = checkPseudo && CheckPseudo(floor, bpm, out cbpm, out count);
        if(!isPesudo) cbpm = floor.nextfloor ? (float) (60.0 / (floor.nextfloor.entryTime - floor.entryTime) * conductor.song.pitch) : bpm;
        float kps = cbpm / 60;
        if(isPesudo) kps *= count;
        if(lastTileBPM == bpm && lastCurBPM == cbpm && lastCurKPS == kps) return;
        BPMText.text = $"<color=white>TBPM | <color=#{ColorToString(GetColorBPM(bpm))}>{Math.Round(bpm, 2)}</color>\n" +
                       $"CBPM |</color> {Math.Round(cbpm, 2)}\n" +
                       $"<color=white>KPS |</color> {(isPesudo ? $"<color=#{ColorToString(GetColorBPM(cbpm * count))}>" : "")}{Math.Round(kps, 2)}{(isPesudo ? "</color>" : "")}";
        if(lastCurBPM != cbpm) BPMText.color = GetColorBPM(cbpm);
        lastTileBPM = bpm;
        lastCurBPM = cbpm;
        lastCurKPS = kps;
    }

    public void PerfectToCombo() {
        if(perToCom) return;
        ComboTitle.text = "Combo";
        perToCom = true;
    }

    private Color GetColorBPM(float value) {
        return value < 400f   ? new Color(0.37254901960784315f, 1, 0.3058823529411765f) :
               value < 1600f  ? new Color(0.37254901960784315f + (value - 400f) / 1200f * 0.6156862745098039f, 1, 0.3058823529411765f - (value - 400f) / 1200f * 0.3058823529411765f) :
               value < 6000f  ? new Color(0.9882352941176471f + (value - 1600f) / 4400f * 0.0117647058823529f, 1 - (value - 1600f) / 4400f * 0.11372549019607843f, 0.3019607843137255f) :
               value < 12000f ? new Color(1, 0.4352941176470588f - (value - 6000f) / 8000f * 0.4352941176470588f, 0.3019607843137255f - (value - 6000f) / 8000f * 0.3019607843137255f) :
                                new Color(1, 0, 0);
    }

    private static string ColorToString(Color color) {
        int r = (int) (color.r * 255);
        int g = (int) (color.g * 255);
        int b = (int) (color.b * 255);
        return $"{r:X2}{g:X2}{b:X2}";
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
                    pseudoFloor = floor.seqID - 1;
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
        pseudoFloor = floor.seqID;
        return true;
    }

    private static bool Check90(double angle) {
        return Math.Abs(angle - 1.57079642638564) < 0.00000000000001;
    }

    public override void Show() {
        perToCom = false;
        purePerfect = true;
        pseudoFloor = -1;
        ComboTitle.text = "Perfect";
        base.Show();
        UpdateAuthor();
        UpdateState();
        lastCheckpoint = -1;
        UpdateCheckpoint();
        UpdateDeath();
        UpdateStart();
        timings = new List<float>();
        UpdateTiming(0);
        timings.Clear();
    }

    public override void Hide() {
        base.Hide();
        timings = null;
    }
}