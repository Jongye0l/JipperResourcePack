using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JALib.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace JipperResourcePack.OverlayContents;

public class Overlay {
    public static Overlay Instance;
    public GameObject GameObject;
    public Canvas Canvas;
    public TextMeshProUGUI ProgressText;
    public TextMeshProUGUI AccuracyText;
    public TextMeshProUGUI XAccuracyText;
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI MapTimeText;
    public TextMeshProUGUI CheckpointText;
    public TextMeshProUGUI AttemptText;
    public TextMeshProUGUI BestText;
    public RectTransform ComboTransform;
    public TextMeshProUGUI ComboTitle;
    public TextMeshProUGUI ComboText;
    public RectTransform ComboTextTransform;
    public TextMeshProUGUI BPMText;
    public TextMeshProUGUI JudgementText;
    public TextMeshProUGUI TimingScaleText;
    public ProgressBar ProgressBar;
    public Color PurePerfectColor = new(1, 0.8549019607843137f, 0);
    public float Progress;
    public int[] Hit = scrMistakesManager.hitMarginsCount;
    public readonly Shader Shader = (Shader) typeof(ShaderUtilities).Property("ShaderRef_MobileSDF").GetValue(null);
    protected int LastTime = -1;
    protected int LastMapTime = -1;
    protected int StartTile;
    protected int NoCheckStartTile;
    protected int LastCheckpoint = -1;
    protected int[] Checkpoints;
    protected int CurCheck;
    protected float LastTileBpm = -1;
    protected float LastCurBpm = -1;
    private readonly Stopwatch _stopwatch;
    protected bool SongPlaying;
    protected float StartProgress;
    protected float CurBest = -1;
    protected bool AutoOnceEnabled;
    protected bool IsDeath;
    protected string MusicTimeCache;
    protected string MapTimeCache;
    private PlayCount.Hash _lastHash;
    private float _lastSavedStartProgress = -1;
    private float _lastMultiplier = 1f;

    public Overlay() {
        Instance = this;
        GameObject = new GameObject("JipperResourcePack Overlay");
        Canvas = GameObject.AddComponent<Canvas>();
        Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = GameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        _stopwatch = new Stopwatch();
        GameObject.SetActive(false);
        InitializeStatus();
        InitializeBPM();
        InitializeJudgement();
        InitializeCombo();
        InitializeProgressBar();
        InitializeTimingScale();
        InitializeAttempt();
        UpdateSize();
        Object.DontDestroyOnLoad(GameObject);
        if(ADOBase.controller is { paused: false } && ADOBase.conductor is { isGameWorld: true }) Show(0);
    }

    protected virtual void InitializeStatus() {
        GameObject gameObject = new("Main");
        RectTransform transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(Canvas.transform);
        gameObject.SetActive(false);
        Status.ProgressObject = gameObject;
        transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(0, 1);
        transform.anchoredPosition = new Vector2(16, -16);
        transform.sizeDelta = new Vector2(456, 100);
        SetupMainText("Progress", ref ProgressText);
        SetupMainText("Accuracy", ref AccuracyText);
        SetupMainText("XAccuracy", ref XAccuracyText);
        SetupMainText("MusicTime", ref TimeText);
        SetupMainText("MapTime", ref MapTimeText);
        SetupMainText("Checkpoint", ref CheckpointText);
        SetupMainText("Best", ref BestText);
    }

    protected void SetupMainText(string name, ref TextMeshProUGUI text) {
        GameObject gameObject2 = new(name);
        RectTransform transform = gameObject2.AddComponent<RectTransform>();
        transform.SetParent(Status.ProgressObject.transform);
        transform.anchorMin = transform.anchorMax = new Vector2(0, 1);
        transform.sizeDelta = new Vector2(456, 30);
        text = gameObject2.AddComponent<TextMeshProUGUI>();
        text.font = BundleLoader.FontAsset;
        text.fontSize = 25;
        SetupShadow(text);
    }

    public virtual void SetupLocationMain() {
        int y = -15;
        SetupLocationMainText(ProgressText, Status.Settings.ShowProgress, ref y);
        SetupLocationMainText(AccuracyText, Status.Settings.ShowAccuracy, ref y);
        SetupLocationMainText(XAccuracyText, Status.Settings.ShowXAccuracy, ref y);
        SetupLocationMainText(TimeText, Status.Settings.ShowMusicTime, ref y);
        SetupLocationMainText(MapTimeText, Status.Settings.ShowMapTime, ref y);
        SetupLocationMainText(CheckpointText, 
            Status.Settings.ShowCheckpoint &&
            (Checkpoints ??= scrLevelMaker.instance.listFloors.Where(floor => floor.GetComponent<ffxCheckpoint>())
                 .Select(floor => floor.seqID).ToArray()).Length > 0, ref y);
        SetupLocationMainText(BestText, Status.Settings.ShowBest, ref y);
        UpdateProgress();
        UpdateAccuracy();
        UpdateTime();
    }

    protected static void SetupLocationMainText(TextMeshProUGUI text, bool enabled, ref int y) {
        text.enabled = enabled;
        if(!enabled) return;
        text.rectTransform.anchoredPosition = new Vector2(228, y);
        y -= 35;
    }

    public void SetupLocationJudgement() {
        JudgementText.rectTransform.anchoredPosition = new Vector2(0, Judgement.Settings.LocationUp ? 85 : 5);
    }

    protected void InitializeBPM() {
        GameObject gameObject = new("BPM");
        RectTransform transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(Canvas.transform);
        transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(1, 1);
        transform.anchoredPosition = new Vector2(-16, -16);
        transform.sizeDelta = new Vector2(456, 90);
        BPMText = gameObject.AddComponent<TextMeshProUGUI>();
        BPMText.font = BundleLoader.FontAsset;
        BPMText.alignment = TextAlignmentOptions.TopRight;
        BPMText.lineSpacing = 30;
        BPMText.fontSize = 25;
        SetupShadow(BPMText);
        gameObject.SetActive(false);
        BPM.BpmObject = gameObject;
    }

    private void InitializeJudgement() {
        GameObject gameObject = new("Judgement");
        RectTransform transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(Canvas.transform);
        transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(0.5f, 0);
        transform.sizeDelta = new Vector2(1000, 30);
        JudgementText = gameObject.AddComponent<TextMeshProUGUI>();
        SetupLocationJudgement();
        JudgementText.font = BundleLoader.FontAsset;
        JudgementText.fontSize = 25;
        JudgementText.alignment = TextAlignmentOptions.Bottom;
        JudgementText.color = new Color(0.8509804f, 0.345098f, 1);
        SetupShadow(JudgementText);
        gameObject.SetActive(false);
        Judgement.JudgementObject = gameObject;
    }

    protected void InitializeCombo() {
        GameObject gameObject = new("Combo");
        RectTransform transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(Canvas.transform);
        transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(0.5f, 1);
        transform.sizeDelta = new Vector2(300, 200);
        Combo.ComboTransform = transform;
        GameObject gameObject2 = new("ComboTitle");
        transform = gameObject2.AddComponent<RectTransform>();
        transform.SetParent(Combo.ComboTransform);
        transform.anchorMin = transform.anchorMax = new Vector2(0.5f, 0.45f);
        transform.pivot = new Vector2(0.5f, 0);
        ComboTitle = gameObject2.AddComponent<TextMeshProUGUI>();
        ComboTitle.font = BundleLoader.FontAsset;
        ComboTitle.fontSize = 40;
        ComboTitle.text = "Perfect";
        ComboTitle.alignment = TextAlignmentOptions.Center;
        ContentSizeFitter fitter = gameObject2.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        SetupDarkShadow(ComboTitle);
        ComboTransform = transform;
        gameObject2 = new GameObject("ComboValue");
        transform = gameObject2.AddComponent<RectTransform>();
        transform.SetParent(Combo.ComboTransform);
        transform.anchorMin = transform.anchorMax = new Vector2(0.5f, 0.45f);
        transform.anchoredPosition = Vector2.zero;
        ComboTextTransform = transform;
        ComboText = gameObject2.AddComponent<TextMeshProUGUI>();
        ComboText.font = BundleLoader.FontAsset;
        ComboText.fontSize = 108;
        ComboText.alignment = TextAlignmentOptions.Top;
        fitter = gameObject2.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        SetupDarkShadow(ComboText);
        gameObject.SetActive(false);
        Combo.ComboObject = gameObject;
    }

    protected void InitializeProgressBar() {
        GameObject gameObject = Object.Instantiate(BundleLoader.ProgressObject);
        RectTransform transform = gameObject.GetComponent<RectTransform>();
        transform.SetParent(Canvas.transform);
        transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(0.5f, 1);
        transform.anchoredPosition = new Vector2(0, -10);
        transform.sizeDelta = new Vector2(642, 18);
        ProgressBar = new ProgressBar(transform);
        gameObject.SetActive(false);
        Status.ProgressBarObject = gameObject;
    }

    protected void InitializeTimingScale() {
        GameObject gameObject = new("TimingScale");
        RectTransform transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(Canvas.transform);
        transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(0.5f, 0);
        transform.sizeDelta = new Vector2(300, 30);
        TimingScaleText = gameObject.AddComponent<TextMeshProUGUI>();
        TimingScaleText.font = BundleLoader.FontAsset;
        TimingScaleText.fontSize = 20;
        TimingScaleText.alignment = TextAlignmentOptions.Bottom;
        SetupShadow(TimingScaleText);
        gameObject.SetActive(false);
        TimingScale.TimingScaleObject = gameObject;
    }

    protected void InitializeAttempt() {
        GameObject gameObject = new("Attempt");
        RectTransform transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(Canvas.transform);
        transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(0.5f, 0);
        transform.anchoredPosition = new Vector2(310, 35);
        transform.sizeDelta = new Vector2(300, 30);
        AttemptText = gameObject.AddComponent<TextMeshProUGUI>();
        AttemptText.font = BundleLoader.FontAsset;
        AttemptText.fontSize = 25;
        AttemptText.alignment = TextAlignmentOptions.BottomLeft;
        SetupShadow(AttemptText);
        gameObject.SetActive(false);
        Attempt.AttemptObject = gameObject;
    }

    public void UpdateSize() {
        Transform transform = GameObject.transform;
        int count = transform.childCount;
        float size = Main.Settings.Size;
        Vector3 scale = new(size, size, 1);
        for(int i = 0; i < count; i++) transform.GetChild(i).localScale = scale;
        TimingScaleText.rectTransform.anchoredPosition = new Vector2(0, 90 + 40 * size);
        RectTransform txtLevelName = ADOBase.controller?.txtLevelName?.GetComponent<RectTransform>();
        if(txtLevelName) {
            txtLevelName.anchoredPosition = new Vector2(0, -20 - 7 * size);
            txtLevelName.localScale = new Vector3(0.5f * size, 0.5f * size);
        }
        Combo.ComboTransform.anchoredPosition = new Vector2(0, -43 - 14 * size);
    }

    protected void SetupShadow(TextMeshProUGUI text) {
        Material material = new(text.fontSharedMaterial);
        if(Shader) material.shader = Shader;
        material.EnableKeyword(ShaderUtilities.Keyword_Outline);
        material.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
        material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.01f);
        material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0, 0, 0, 0.5f));
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.8f);
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.8f);
        material.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.3f);
        material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.2f);
        text.fontSharedMaterial = material;
    }

    protected void SetupDarkShadow(TextMeshProUGUI text) {
        Material material = new(text.fontSharedMaterial);
        if(Shader) material.shader = Shader;
        material.EnableKeyword(ShaderUtilities.Keyword_Outline);
        material.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
        material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.01f);
        material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0, 0, 0, 0.7f));
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 1f);
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -1f);
        material.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0f);
        material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0f);
        text.fontSharedMaterial = material;
    }

    public virtual void UpdateAccuracy() {
        if(!GameObject.activeSelf) return;
        float xacc = scrController.instance.mistakesManager?.percentXAcc ?? 1;
        if(float.IsNaN(xacc)) xacc = 1;
        if(Status.Settings.ShowAccuracy) {
            float acc = scrController.instance.mistakesManager?.percentAcc ?? 1;
            float maxAcc = 1 + (scrController.instance.currentSeqID - NoCheckStartTile + 1) * 0.0001f;
            AccuracyText.text = $"<color=white>Accuracy |</color> {Math.Round(acc * 100, 2)}%";
            AccuracyText.color = Status.Settings.AccuracyColor.GetColor(xacc == 1 ? 1 : acc / maxAcc);
        }
        if(Status.Settings.ShowXAccuracy) {
            XAccuracyText.text = $"<color=white>XAccuracy |</color> {Math.Round(xacc * 100, 2)}%";
            XAccuracyText.color = Status.Settings.XAccuracyColor.GetColor(xacc);
        }
    }
    
    public virtual void UpdateProgress() {
        if(!GameObject.activeSelf) return;
        Progress = scrController.instance.percentComplete;
        if(Status.Settings.ShowProgress) UpdateProgressText();
        if(Status.Settings.ShowCheckpoint) UpdateCheckPointText();
        if(Status.Settings.ShowProgressBar) UpdateProgressBar();
        if(Status.Settings.ShowBest) UpdateBest();
    }

    public void UpdateProgressBar() {
        try {
            if(!ProgressBar.LineTransform) return;
            ProgressBar.LineTransform.SizeDeltaX(Progress * 638);
            ProgressBar.BackgroundImage.color = Status.Settings.ProgressBarBackgroundColor.GetColor(Progress);
            ProgressBar.LineImage.color = Status.Settings.ProgressBarColor.GetColor(Progress);
            ProgressBar.BorderImage.color = Status.Settings.ProgressBarBorderColor.GetColor(Progress);
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    public virtual void UpdateProgressText() {
        ProgressText.text = $"<color=white>Progress |</color> {Math.Round(Progress * 100, 2)}%";
        ProgressText.color = Status.Settings.ProgressColor.GetColor(Progress);
    }

    public void UpdateCheckPointText() {
        if(Checkpoints.Length == 0) return;
        bool updated = false;
        while(Checkpoints.Length > CurCheck && scrController.instance.currentSeqID >= Checkpoints[CurCheck]) {
            CurCheck++;
            updated = true;
        }
        if(LastCheckpoint == scrController.checkpointsUsed && !updated) return;
        CheckpointText.text = $"<color=white>CheckPoint |</color> {scrController.checkpointsUsed} ({CurCheck}/{Checkpoints.Length})";
        LastCheckpoint = scrController.checkpointsUsed;
    }

    public void UpdateAttempts() {
        string[] values = new string[2];
        int count = 0;
        if(Attempt.Settings.ShowAttempt) values[count++] = $"Attempt {PlayCount.GetData(_lastHash)?.GetAttempts(StartProgress) ?? 0}";
        if(Attempt.Settings.ShowFullAttempt) values[count++] = $"Full Attempt {PlayCount.GetData(_lastHash)?.GetAttempts() ?? 0}";
        AttemptText.text = count switch {
            0 => "",
            1 => values[0],
            _ => $"{values[0]}\n{values[1]}"
        };
    }

    public void UpdateBest() {
        if(RDC.auto && !AutoOnceEnabled) AutoOnceEnabled = true;
        if(CurBest == -1) CurBest = PlayCount.GetData(_lastHash)?.GetBest(StartProgress, _lastMultiplier) ?? 0;
        else if(CurBest > Progress || AutoOnceEnabled) return;
        UpdateBestText();
    }

    public virtual void UpdateBestText() {
        float best = CurBest > Progress || AutoOnceEnabled ? CurBest : Progress;
        BestText.text = $"<color=white>Best |</color> {Math.Round(best * 100, 2)}%";
        BestText.color = Status.Settings.BestColor.GetColor(best);
    }

    public void UpdateJudgement() {
        if(!GameObject.activeSelf) return;
        int[] hits = Hit;
        JudgementText.text = $"{hits[9]} <color=red>{hits[0]} <color=#FF6F4E>{hits[1]} <color=#A0FF4E>{hits[2]} <color=#60FF4E>{hits[3] + hits[10]}</color> {hits[4]}</color> {hits[5]}</color> {hits[6]}</color> {hits[8]}";
    }
    
    public virtual void UpdateTime() {
        if(!GameObject.activeSelf || !Status.Instance.Enabled || IsDeath) return;
        bool requireMusicToMap = false;
        if(Status.Settings.ShowMusicTime) {
            AudioSource song = scrConductor.instance.song;
            if(!song?.clip && Status.Settings.ShowMapTimeIfNotMusic) requireMusicToMap = true;
            else {
                float time = song!.time;
                float totalTime = song.clip?.length ?? 0;
                if(LastTime == (int) time) return;
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
                TimeText.text = $"<color=white>{(Status.Settings.TimeTextType == TimeTextType.Korean ? "음악 시간" : "Music Time")} |</color> {timeStr}~{MusicTimeCache}";
                LastTime = (int) time;
                TimeText.color = Status.Settings.MusicTimeColor.GetColor(time / totalTime);
            }
        }
        if(Status.Settings.ShowMapTime || requireMusicToMap) {
            float time = (float) (scrConductor.instance.addoffset + scrConductor.instance.songposition_minusi);
            float totalTime = (float) scrLevelMaker.instance.listFloors.Last().entryTime;
            if(time < 0) time = 0;
            else if(time > totalTime) time = totalTime;
            if((!Status.Settings.ShowMapTime || LastMapTime == (int) time) &&
               (!requireMusicToMap || LastTime == (int) time)) return;
            bool hourNeed = totalTime >= 3600;
            MapTimeCache ??= GetTimeString(totalTime, hourNeed);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            string timeStr = time == totalTime ? MapTimeCache : GetTimeString(time, hourNeed);
            string text = $"<color=white>{(Status.Settings.TimeTextType == TimeTextType.Korean ? "맵 시간" : "Map Time")} |</color> {timeStr}~{MapTimeCache}";
            if(Status.Settings.ShowMapTime) {
                MapTimeText.text = text;
                LastMapTime = (int) time;
                MapTimeText.color = Status.Settings.MapTimeColor.GetColor(time / totalTime);
            }
            if(requireMusicToMap) {
                TimeText.text = text;
                LastTime = (int) time;
                TimeText.color = Status.Settings.MusicTimeColor.GetColor(time / totalTime);
            }
        }
    }
    
    private static string GetTimeString(float time, bool hour) {
        int timeInt = (int) time;
        return hour ? $"{timeInt / 3600}:{timeInt % 3600 / 60:00}:{timeInt % 60:00}" : $"{timeInt / 60}:{timeInt % 60:00}";
    }
    
    public void UpdateCombo(int combo, bool bump) {
        if(!GameObject.activeSelf) return;
        ComboText.text = combo.ToString();
        ComboText.color = UpdateComboColor(combo);
        if(bump) {
            _stopwatch.Restart();
            UpdateComboSize();
        } else {
            _stopwatch.Stop();
            ComboText.fontSize = 78;
            ComboTransform.anchoredPosition = new Vector2(0, 43.505f);
        }
    }

    public virtual Color UpdateComboColor(int combo) {
        if(combo > Combo.Settings.ComboColorMax) combo = Combo.Settings.ComboColorMax;
        return Combo.Settings.ComboColor.GetColor((float) combo / Combo.Settings.ComboColorMax);
    }

    public void UpdateComboSize() {
        if(!_stopwatch.IsRunning || !GameObject.activeSelf) return;
        double t = _stopwatch.Elapsed.TotalMilliseconds / 500;
        if(t > 1) {
            t = 1;
            _stopwatch.Stop();
        }
        ComboText.fontSize = 30 * OutExpoChange(t) + 78;
        Task.Yield().OnCompleted(UpdateComboLocation);
    }

    private void UpdateComboLocation() {
        try {
            ComboTransform.anchoredPosition = new Vector2(0, ComboTextTransform.sizeDelta.y / 2);
        } catch (Exception e) {
            Main.Instance.LogReportException("Failed to update combo location", e);
        }
    }
    
    private static float OutExpoChange(double t) => (float) (t == 1 ? 0 : Math.Pow(2, -10 * t));

    public virtual void UpdateBPM() {
        if(!GameObject.activeSelf) return;
        scrFloor floor = scrController.instance.currFloor ?? scrController.instance.firstFloor;
        scrConductor conductor = scrConductor.instance;
        float bpm = (float) (conductor.bpm * conductor.song.pitch * scrController.instance.speed);
        float cbpm = floor.nextfloor ? (float) (60.0 / (floor.nextfloor.entryTime - floor.entryTime) * conductor.song.pitch) : bpm;
        float kps = cbpm / 60;
        if(LastTileBpm == bpm && LastCurBpm == cbpm) return;
        BPMText.text = $"<color=white>TBPM | <color=#{ColorToHex(BPM.Settings.BpmColor.GetColor(bpm / BPM.Settings.BpmColorMax))}>{Math.Round(bpm, 2)}</color>\n" +
                       $"CBPM |</color> {Math.Round(cbpm, 2)}\n" +
                       $"<color=white>KPS |</color> {Math.Round(kps, 2)}";
        if(LastCurBpm != cbpm) BPMText.color = BPM.Settings.BpmColor.GetColor(cbpm / BPM.Settings.BpmColorMax);
        LastTileBpm = bpm;
        LastCurBpm = cbpm;
    }

    protected static string ColorToHex(Color color) => $"{Mathf.RoundToInt(color.r * 255):X2}{Mathf.RoundToInt(color.g * 255):X2}{Mathf.RoundToInt(color.b * 255):X2}{(color.a == 1 ? "" : Mathf.RoundToInt(color.a * 255).ToString("X2"))}";

    public void UpdateTimingScale() {
        if(!GameObject.activeSelf) return;
        TimingScaleText.text = $"Timing Scale - {Math.Round(scrController.instance.currFloor.marginScale * 100, 2)}%";
    }
    
    public virtual void Show(int floor) {
        if(_lastSavedStartProgress != -1) {
            if(!AutoOnceEnabled) PlayCount.SetBest(_lastHash, _lastSavedStartProgress, Progress, _lastMultiplier);
            _lastSavedStartProgress = -1;
            MusicTimeCache = MapTimeCache = null;
        }
        
        PlayCount.Hash hash = PlayCount.GetMapHash();
        if(_lastHash != hash) {
            _lastHash = hash;
            Checkpoints = null;
        }
        
        if(scnEditor.instance) {
            if(scrController.checkpointsUsed == 0) NoCheckStartTile = floor;
        } else if(!GCS.practiceMode) {
            NoCheckStartTile = 0;
        } else {
            NoCheckStartTile = floor;
        }
        
        AutoOnceEnabled = RDC.auto || ADOBase.controller.noFail;
        StartTile = floor;
        _lastSavedStartProgress = StartProgress = (float) floor / ADOBase.lm.listFloors.Count;
        CurBest = LastCheckpoint = -1;
        _lastMultiplier = (float) (ADOBase.conductor.song.pitch * ADOBase.controller.speed);
        if(Status.Instance.Enabled && !AutoOnceEnabled) PlayCount.AddAttempts(_lastHash, StartProgress);
        
        GameObject.SetActive(true);
        CurCheck = 0;
        scrMistakesManager manager = scrController.instance.mistakesManager;
        manager.percentAcc = 1;
        manager.percentXAcc = 1;
        SongPlaying = false;
        IsDeath = false;
        
        if(Status.Instance.Enabled) SetupLocationMain();
        if(Judgement.Instance.Enabled) UpdateJudgement();
        if(Combo.Instance.Enabled) UpdateCombo(0, false);
        if(BPM.Instance.Enabled) UpdateBPM();
        if(TimingScale.Instance.Enabled) UpdateTimingScale();
        if(Attempt.Instance.Enabled) UpdateAttempts();
        Combo.ComboCount = 0;
    }

    public void Death() {
        IsDeath = true;
        if(AutoOnceEnabled || _lastSavedStartProgress == -1) return;
        PlayCount.SetBest(_lastHash, _lastSavedStartProgress, Progress, _lastMultiplier);
        _lastSavedStartProgress = -1;
        CurBest = Progress;
    }

    public void Clear() {
        if(AutoOnceEnabled || _lastSavedStartProgress == -1) return;
        PlayCount.SetBest(_lastHash, _lastSavedStartProgress, 1, _lastMultiplier);
        _lastSavedStartProgress = -1;
        CurBest = 1;
    }
    
    public virtual void Hide() {
        if(!GameObject.activeSelf) return;
        GameObject.SetActive(false);
        try {
            if(!AutoOnceEnabled && _lastSavedStartProgress != -1) {
                PlayCount.SetBest(_lastHash, _lastSavedStartProgress, Progress, _lastMultiplier);
                _lastSavedStartProgress = -1;
            }
            if(StartProgress == Progress && !AutoOnceEnabled) PlayCount.RemoveAttempts(_lastHash, StartProgress);
        } catch (Exception e) {
            Main.Instance.LogException("Failed to set play data on hide", e);
        }
        StartProgress = StartTile = NoCheckStartTile = -1;
    }

    public void Destroy() {
        Object.Destroy(GameObject);
        GC.SuppressFinalize(this);
    }
}
