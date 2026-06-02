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
    public IOverlayTextManager OverlayTextManager;
    public readonly GameObject GameObject;
    public readonly Canvas Canvas;
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
    public TextMeshProUGUI BpmText;
    public TextMeshProUGUI JudgementText;
    public TextMeshProUGUI TimingScaleText;
    public ProgressBar ProgressBar;
    public Color PurePerfectColor = new(1, 0.8549019607843137f, 0);
    public int[] Hit;
    private readonly Shader _shader = (Shader) typeof(ShaderUtilities).Property("ShaderRef_MobileSDF").GetValue(null);
    private int _lastTime = -1;
    private int _lastMapTime = -1;
    public int StartTile;
    public int NoCheckStartTile;
    public int[] Checkpoints;
    protected float LastTileBpm = -1;
    protected float LastCurBpm = -1;
    private readonly Stopwatch _stopwatch;
    protected bool SongPlaying;
    public float StartProgress;
    public bool AutoOnceEnabled;
    protected bool IsDeath;
    protected string MusicTimeCache;
    protected string MapTimeCache;
    public PlayCount.Hash LastHash;
    private float _lastSavedStartProgress = -1;
    public float LastMultiplier = 1f;

    public Overlay() {
        Instance = this;
        OnChangePlayers();
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
        InitializeBpm();
        InitializeTimingScale();
        InitializeJudgement();
        InitializeCombo();
        InitializeProgressBar();
        InitializeAttempt();
        UpdateSize();
        Object.DontDestroyOnLoad(GameObject);
        if(ADOBase.controller is { paused: false } && ADOBase.conductor is { isGameWorld: true }) Show(0);
    }

    public void OnChangePlayers() {
        Hit = VersionSafe.GetHitMarginsCount();
        SetupTextManager();
        if(MainThread.IsMainThread() && TimingScaleText) OverlayTextManager.SetupUnderTextLocation(this);
    }

    protected virtual void SetupTextManager() {
        OverlayTextManager = VersionSafe.IsCoopMode()
            ? new OverlayTextManagerCoop(this)
            : new OverlayTextManagerNormal();
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
        VersionSafe.CalculatePercentAcc(); // UpdateAccuracy();
        UpdateTime();
    }

    protected static void SetupLocationMainText(TextMeshProUGUI text, bool enabled, ref int y) {
        text.enabled = enabled;
        if(!enabled) return;
        text.rectTransform.anchoredPosition = new Vector2(228, y);
        y -= 35;
    }

    private void InitializeBpm() {
        GameObject gameObject = new("BPM");
        RectTransform transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(Canvas.transform);
        transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(1, 1);
        transform.anchoredPosition = new Vector2(-16, -16);
        transform.sizeDelta = new Vector2(456, 90);
        BpmText = gameObject.AddComponent<TextMeshProUGUI>();
        BpmText.font = BundleLoader.FontAsset;
        BpmText.alignment = TextAlignmentOptions.TopRight;
        BpmText.lineSpacing = 30;
        BpmText.fontSize = 25;
        SetupShadow(BpmText);
        gameObject.SetActive(false);
        Bpm.BpmObject = gameObject;
    }

    private void InitializeJudgement() {
        GameObject gameObject = new("Judgement");
        RectTransform transform = gameObject.AddComponent<RectTransform>();
        transform.SetParent(Canvas.transform);
        transform.anchorMin = transform.anchorMax = transform.pivot = new Vector2(0.5f, 0);
        transform.sizeDelta = new Vector2(1000, 30);
        JudgementText = gameObject.AddComponent<TextMeshProUGUI>();
        JudgementText.font = BundleLoader.FontAsset;
        JudgementText.fontSize = 25;
        JudgementText.alignment = TextAlignmentOptions.Bottom;
        JudgementText.color = new Color(0.8509804f, 0.345098f, 1);
        SetupShadow(JudgementText);
        gameObject.SetActive(false);
        Judgement.JudgementObject = gameObject;
    }

    private void InitializeCombo() {
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

    private void InitializeProgressBar() {
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

    private void InitializeTimingScale() {
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

    private void InitializeAttempt() {
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
        RectTransform txtLevelName = ADOBase.controller?.txtLevelName?.GetComponent<RectTransform>();
        if(txtLevelName) {
            txtLevelName.anchoredPosition = new Vector2(0, -20 - 7 * size);
            txtLevelName.localScale = new Vector3(0.5f * size, 0.5f * size);
        }
        Combo.ComboTransform.anchoredPosition = new Vector2(0, -43 - 14 * size);
        OverlayTextManager.SetupUnderTextLocation(this);
    }

    private void SetupShadow(TextMeshProUGUI text) => Shadow(text, 0.5f);

    private void SetupDarkShadow(TextMeshProUGUI text) => Shadow(text, 0.7f);

    private void Shadow(TextMeshProUGUI text, float a) {
        Task.Yield().GetAwaiter().OnCompleted(() => {
            try {
                Material baseMaterial = text.fontSharedMaterial ?? text.fontMaterial;
                Material material = new(baseMaterial);
                if(_shader) material.shader = _shader;
                material.EnableKeyword(ShaderUtilities.Keyword_Outline);
                material.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
                material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.01f);
                material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
                material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0, 0, 0, a));
                material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 1f);
                material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -1f);
                material.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0f);
                material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0f);
                text.fontSharedMaterial = material;
            } catch (Exception e) {
                Main.Instance.LogReportException("Failed to setup shadow", e);
            }
        });
    }

    public void UpdateAccuracy(int index = -1) {
        if(!GameObject.activeSelf) return;
        OverlayTextManager.UpdateAccuracy(this, index);
    }
    
    public virtual void UpdateProgress(scrPlanet planet = null) {
        if(!GameObject.activeSelf) return;
        OverlayTextManager.CacheProgress(planet);
        if(Status.Settings.ShowProgress) OverlayTextManager.UpdateProgress(this);
        if(Status.Settings.ShowCheckpoint) UpdateCheckPointText();
        if(Status.Settings.ShowProgressBar) UpdateProgressBar();
        if(Status.Settings.ShowBest) OverlayTextManager.UpdateBest(this);
    }

    public void UpdateProgressBar() {
        try {
            if(!ProgressBar.LineTransform) return;
            OverlayTextManager.UpdateProgressBar(this);
        } catch (Exception e) {
            Main.Instance.LogException(e);
        }
    }

    private void UpdateCheckPointText() {
        if(Checkpoints.Length == 0) return;
        OverlayTextManager.UpdateCheckpoint(this);
    }

    public void UpdateAttempts() {
        string[] values = new string[2];
        int count = 0;
        if(Attempt.Settings.ShowAttempt) values[count++] = $"Attempt {PlayCount.GetData(LastHash)?.GetAttempts(StartProgress, LastMultiplier) ?? 0}";
        if(Attempt.Settings.ShowFullAttempt) values[count++] = $"Full Attempt {PlayCount.GetData(LastHash)?.GetAttempts() ?? 0}";
        AttemptText.text = count switch {
            0 => "",
            1 => values[0],
            _ => $"{values[0]}\n{values[1]}"
        };
    }

    public void UpdateJudgement(int index = -1) {
        if(!GameObject.activeSelf) return;
        OverlayTextManager.UpdateJudgement(this, index);
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
                if(_lastTime == (int) time) return;
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
                _lastTime = (int) time;
                TimeText.color = Status.Settings.MusicTimeColor.GetColor(time / totalTime);
            }
        }
        if(Status.Settings.ShowMapTime || requireMusicToMap) {
            float time = scrController.instance.state == States.Start ? 0 : (float) (scrConductor.instance.addoffset + scrConductor.instance.songposition_minusi);
            float totalTime = (float) scrLevelMaker.instance.listFloors.Last().entryTime;
            if(time < 0) time = 0;
            else if(time > totalTime) time = totalTime;
            if((!Status.Settings.ShowMapTime || _lastMapTime == (int) time) &&
               (!requireMusicToMap || _lastTime == (int) time)) return;
            bool hourNeed = totalTime >= 3600;
            MapTimeCache ??= GetTimeString(totalTime, hourNeed);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            string timeStr = time == totalTime ? MapTimeCache : GetTimeString(time, hourNeed);
            string text = $"<color=white>{(Status.Settings.TimeTextType == TimeTextType.Korean ? "맵 시간" : "Map Time")} |</color> {timeStr}~{MapTimeCache}";
            if(Status.Settings.ShowMapTime) {
                MapTimeText.text = text;
                _lastMapTime = (int) time;
                MapTimeText.color = Status.Settings.MapTimeColor.GetColor(time / totalTime);
            }
            if(requireMusicToMap) {
                TimeText.text = text;
                _lastTime = (int) time;
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
    
    // ReSharper disable once CompareOfFloatsByEqualityOperator
    private static float OutExpoChange(double t) => (float) (t == 1 ? 0 : Math.Pow(2, -10 * t));

    public virtual void UpdateBpm() {
        if(!GameObject.activeSelf) return;
        scrFloor floor = scrController.instance.currFloor ?? scrController.instance.firstFloor;
        scrConductor conductor = scrConductor.instance;
        float bpm = (float) (conductor.bpm * conductor.song.pitch * VersionSafe.GetPlanetSpeed(scrController.instance));
        float cbpm = floor.nextfloor ? (float) (60.0 / (floor.nextfloor.entryTime - floor.entryTime) * conductor.song.pitch) : bpm;
        float kps = cbpm / 60;
        // ReSharper disable CompareOfFloatsByEqualityOperator
        if(LastTileBpm == bpm && LastCurBpm == cbpm) return;
        BpmText.text = $"<color=white>TBPM | <color=#{ColorToHex(Bpm.Settings.BpmColor.GetColor(bpm / Bpm.Settings.BpmColorMax))}>{Math.Round(bpm, 2)}</color>\n" +
                       $"CBPM |</color> {Math.Round(cbpm, 2)}\n" +
                       $"<color=white>KPS |</color> {Math.Round(kps, 2)}";
        if(LastCurBpm != cbpm) BpmText.color = Bpm.Settings.BpmColor.GetColor(cbpm / Bpm.Settings.BpmColorMax);
        // ReSharper restore CompareOfFloatsByEqualityOperator
        LastTileBpm = bpm;
        LastCurBpm = cbpm;
    }

    // ReSharper disable once CompareOfFloatsByEqualityOperator
    protected static string ColorToHex(Color color) => $"{Mathf.RoundToInt(color.r * 255):X2}{Mathf.RoundToInt(color.g * 255):X2}{Mathf.RoundToInt(color.b * 255):X2}{(color.a == 1 ? "" : Mathf.RoundToInt(color.a * 255).ToString("X2"))}";

    public void UpdateTimingScale() {
        if(!GameObject.activeSelf) return;
        TimingScaleText.text = $"Timing Scale - {Math.Round(scrController.instance.currFloor.marginScale * 100, 2)}%";
    }
    
    public virtual void Show(int floor) {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if(_lastSavedStartProgress != -1) {
            if(!AutoOnceEnabled) PlayCount.SetBest(LastHash, _lastSavedStartProgress, OverlayTextManager.GetProgress(), LastMultiplier);
            _lastSavedStartProgress = -1;
        }
        
        PlayCount.Hash hash = PlayCount.GetMapHash();
        if(LastHash != hash) {
            LastHash = hash;
            Checkpoints = null;
            MapTimeCache = null;
        }
        MusicTimeCache = null;
        
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
        LastMultiplier = (float) (ADOBase.conductor.song.pitch * VersionSafe.GetPlanetSpeed(scrController.instance));
        if(Status.Instance.Enabled && !AutoOnceEnabled) PlayCount.AddAttempts(LastHash, StartProgress, LastMultiplier);
        SetupTextManager();
        
        GameObject.SetActive(true);
        SongPlaying = false;
        IsDeath = false;
        
        if(Status.Instance.Enabled) SetupLocationMain();
        if(Judgement.Instance.Enabled) UpdateJudgement();
        if(Combo.Instance.Enabled) UpdateCombo(0, false);
        if(Bpm.Instance.Enabled) UpdateBpm();
        if(TimingScale.Instance.Enabled) UpdateTimingScale();
        if(Attempt.Instance.Enabled) UpdateAttempts();
        Combo.ComboCount = 0;
    }

    public void Death() {
        IsDeath = true;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if(AutoOnceEnabled || _lastSavedStartProgress == -1) return;
        PlayCount.SetBest(LastHash, _lastSavedStartProgress, OverlayTextManager.GetProgress(), LastMultiplier);
        _lastSavedStartProgress = -1;
        OverlayTextManager.SetBest(OverlayTextManager.GetProgress());
    }

    public void Clear() {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if(AutoOnceEnabled || _lastSavedStartProgress == -1) return;
        PlayCount.SetBest(LastHash, _lastSavedStartProgress, 1, LastMultiplier);
        _lastSavedStartProgress = -1;
        OverlayTextManager.SetBest(1);
    }
    
    public virtual void Hide() {
        if((object) GameObject == null || !GameObject.activeSelf) return;
        GameObject.SetActive(false);
        try {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if(!AutoOnceEnabled && _lastSavedStartProgress != -1) {
                PlayCount.SetBest(LastHash, _lastSavedStartProgress, OverlayTextManager.GetProgress(), LastMultiplier);
                _lastSavedStartProgress = -1;
            }
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if(StartProgress == OverlayTextManager.GetProgress() && !AutoOnceEnabled) PlayCount.RemoveAttempts(LastHash, StartProgress, LastMultiplier);
        } catch (Exception e) {
            Main.Instance.LogException("Failed to set play data on hide", e);
        }
        StartProgress = StartTile = NoCheckStartTile = -1;
        OverlayTextManager = null;
    }

    public void Destroy() {
        Object.Destroy(GameObject);
        GC.SuppressFinalize(this);
    }
}
