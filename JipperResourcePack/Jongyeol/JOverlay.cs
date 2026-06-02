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
    public new IJOverlayTextManager OverlayTextManager;
    public TextMeshProUGUI FPSText;
    public TextMeshProUGUI AuthorText;
    public TextMeshProUGUI StateText;
    public TextMeshProUGUI DeathText;
    public TextMeshProUGUI StartText;
    public TextMeshProUGUI TimingText;

    private List<float> _timings;
    public bool PurePerfect;
    private int _pseudoFloor = -1;
    private float _lastCurKps = -1;
    private static LevelData LevelData => scnGame.instance ? scnGame.instance.levelData : null;
    private float _fpsTime;
    private bool _perToCom;

    public JOverlay() {
        Instance = this;
    }

    protected override void SetupTextManager() {
        base.OverlayTextManager = OverlayTextManager = VersionSafe.IsCoopMode()
            ? new JOverlayTextManagerCoop(this)
            : new JOverlayTextManagerNormal();
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
        Checkpoints ??= scrLevelMaker.instance.listFloors.FindAll(floor => floor.GetComponent<ffxCheckpoint>()).Select(floor => floor.seqID).ToArray();
        SetupLocationMainText(CheckpointText, checkAuto && JStatus.Settings.ShowCheckpoint && Checkpoints.Length > 0, ref y);
        SetupLocationMainText(BestText, checkAuto && JStatus.Settings.ShowBest, ref y);
        SetupLocationMainText(StateText, JStatus.Settings.ShowState, ref y);
        SetupLocationMainText(DeathText, scrController.instance.noFail && JStatus.Settings.ShowDeath, ref y);
        SetupLocationMainText(StartText, StartTile != 0 && JStatus.Settings.ShowStart, ref y);
        SetupLocationMainText(TimingText, checkAuto && JStatus.Settings.ShowTiming, ref y);
        UpdateProgress();
        VersionSafe.CalculatePercentAcc(); // UpdateAccuracy();
        UpdateTime();
        UpdateAuthor();
        UpdateDeath();
        UpdateState();
        UpdateStart();
        if(_timings != null) return;
        _timings = [];
        UpdateTiming(0);
        _timings.Clear();
    }

    public override void UpdateProgress(scrPlanet planet = null) {
        if(!GameObject.activeSelf) return;
        if(PurePerfect) OverlayTextManager.CheckPurePerfect(this, planet);
        base.UpdateProgress(planet);
        UpdateDeath(planet);
        UpdateState(planet);
    }

    public override void UpdateTime() {
        if(!GameObject.activeSelf || !Status.Instance.Enabled || IsDeath) return;
        bool requireMusicToMap = false;
        if(JStatus.Settings.ShowMusicTime) {
            AudioSource song = scrConductor.instance.song;
            if(!song?.clip && JStatus.Settings.ShowMapTimeIfNotMusic) requireMusicToMap = true;
            else {
                float time = song!.time;
                float totalTime = song.clip?.length ?? 0;
                if(time > 0) SongPlaying = true;
                else if(time == 0 && SongPlaying) time = totalTime;
                bool hourNeed = totalTime >= 3600;
                MusicTimeCache ??= GetTimeString(totalTime, hourNeed);
                string timeStr;
                if(time == 0 && SongPlaying) {
                    time = totalTime;
                    timeStr = MusicTimeCache;
                } else {
                    if(time > 0) SongPlaying = true;
                    timeStr = GetTimeString(time, hourNeed);
                }
                TimeText.text = $"<color=white>{(JStatus.Settings.TimeTextType == TimeTextType.Korean ? "음악 시간" : "Music Time")} |</color> {timeStr}~{MusicTimeCache}";
                TimeText.color = JStatus.Settings.MusicTimeColor.GetColor(time / totalTime);
            }
        }
        if(JStatus.Settings.ShowMapTime || requireMusicToMap) {
            float time = (float) (scrConductor.instance.addoffset + scrConductor.instance.songposition_minusi);
            float totalTime = (float) scrLevelMaker.instance.listFloors.Last().entryTime;
            if(time < 0) time = 0;
            else if(time > totalTime) time = totalTime;
            if(!JStatus.Settings.ShowMapTime && !requireMusicToMap) return;
            bool hourNeed = totalTime >= 3600;
            MapTimeCache ??= GetTimeString(totalTime, hourNeed);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            string timeStr = time == totalTime ? MapTimeCache : GetTimeString(time, hourNeed);
            string text = $"<color=white>{(JStatus.Settings.TimeTextType == TimeTextType.Korean ? "맵 시간" : "Map Time")} |</color> {timeStr}~{MapTimeCache}";
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
    
    private static string GetTimeString(float time, bool hour) {
        int timeInt = (int) time;
        return hour ? $"{timeInt / 3600}:{timeInt % 3600 / 60:00}:{time % 60:00.0}" : $"{timeInt / 60}:{time % 60:00.0}";
    }

    public override Color UpdateComboColor(int combo) {
        if(PurePerfect) return PurePerfectColor;
        float value = (float) combo / (scrController.instance.currentSeqID - StartTile + OverlayTextManager.GetTooJudgement(this) + 1) * 2;
        if(value > 1) value = 1;
        return GetColor(value, 0.2f, false);
    }

    public Color GetColor(float value, float middle = 0.5f, bool ppColor = true) {
        return value < middle         ? new Color(1 - value / middle * 0.0117647058823529f,value / middle, value / middle * 0.3019607843137255f) :
               value < 1f || !ppColor ? new Color(0.9882352941176471f - (value - middle) / (1 - middle) * 0.6156862745098039f, 1, 0.3019607843137255f + (value - middle) / (1 - middle) * 0.01f) :
                                        PurePerfectColor;
    }

    public void UpdateFPS(float deltaTime) {
        if(!JStatus.Settings.ShowFPS || !GameObject.activeSelf || (_fpsTime += deltaTime) < 0.01f) return;
        FPSText.text = $"FPS | {1 / deltaTime:F4}";
        _fpsTime %= 0.01f;
    }

    private void UpdateAuthor() {
        if(!JStatus.Settings.ShowAuthor || !GameObject.activeSelf) return;
        AuthorText.text = $"Author | {LevelData?.author ?? ""}";
    }

    public void UpdateState(scrPlanet planet = null) {
        if(!JStatus.Settings.ShowState || !GameObject.activeSelf) return;
        OverlayTextManager.UpdateState(this, planet);
    }

    private void UpdateDeath(scrPlanet planet = null) {
        if(!JStatus.Settings.ShowDeath || !GameObject.activeSelf) return;
        OverlayTextManager.UpdateDeath(this, planet);
    }


    private void UpdateStart() {
        if(!JStatus.Settings.ShowStart || !GameObject.activeSelf || StartTile != scrController.instance.currentSeqID) return;
        StartText.text = $"Start | {StartTile} ({Math.Round(OverlayTextManager.GetProgress() * 100, 5)}%)";
    }

    public void UpdateTiming(float timing) {
        if(!JStatus.Settings.ShowTiming || !GameObject.activeSelf) return;
        _timings.Add(timing);
        TimingText.text = $"<color=white>Timing |</color> {Math.Round(timing, 5)} ({Math.Round(_timings.Average(), 5)})";
        TimingText.color = GetColor(1 - Math.Min(Math.Abs(timing), 150) / 150);
    }

    public override void UpdateBpm() {
        if(!GameObject.activeSelf) return;
        scrFloor floor = scrController.instance.currFloor ?? scrController.instance.firstFloor;
        if(floor.seqID <= _pseudoFloor) return;
        scrConductor conductor = scrConductor.instance;
        float bpm = (float) (conductor.bpm * conductor.song.pitch * VersionSafe.GetPlanetSpeed(scrController.instance));
        bool checkPseudo = Jbpm.Settings.CheckPseudo;
        float cbpm = 0;
        int count = 0;
        bool isPesudo = checkPseudo && CheckPseudo(floor, bpm, out cbpm, out count);
        if(!isPesudo) cbpm = floor.nextfloor ? (float) (60.0 / (floor.nextfloor.entryTime - floor.entryTime) * conductor.song.pitch) : bpm;
        float kps = cbpm / 60;
        if(isPesudo) kps *= count;
        // ReSharper disable CompareOfFloatsByEqualityOperator
        if(LastTileBpm == bpm && LastCurBpm == cbpm && _lastCurKps == kps) return;
        BpmText.text = $"<color=white>TBPM | <color=#{ColorToHex(Jbpm.Settings.BpmColor.GetColor(bpm / Jbpm.Settings.BpmColorMax))}>{Math.Round(bpm, 2)}</color>\n" +
                       $"CBPM |</color> {Math.Round(cbpm, 2)}\n" +
                       $"<color=white>KPS |</color> {(isPesudo ? $"<color=#{ColorToHex(Jbpm.Settings.BpmColor.GetColor(cbpm * count / Jbpm.Settings.BpmColorMax))}>" : "")}{Math.Round(kps, 2)}{(isPesudo ? "</color>" : "")}";
        if(LastCurBpm != cbpm) BpmText.color = Jbpm.Settings.BpmColor.GetColor(cbpm / Jbpm.Settings.BpmColorMax);
        // ReSharper restore CompareOfFloatsByEqualityOperator
        LastTileBpm = bpm;
        LastCurBpm = cbpm;
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

    public override void Show(int floor) {
        _perToCom = false;
        PurePerfect = true;
        _pseudoFloor = -1;
        if(scrController.checkpointsUsed == 0) ComboTitle.text = "Perfect";
        _timings?.Clear();
        base.Show(floor);
    }

    public override void Hide() {
        base.Hide();
        _timings = null;
    }
}