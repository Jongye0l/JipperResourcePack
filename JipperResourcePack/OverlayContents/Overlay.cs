using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    public TextMeshProUGUI PotentialAccuracyText;
    public TextMeshProUGUI XAccuracyText;
    public TextMeshProUGUI PotentialXAccuracyText;
    public TextMeshProUGUI XScoreText;
    public TextMeshProUGUI PotentialXScoreText;
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI MapTimeText;
    public TextMeshProUGUI CheckpointText;
    public TextMeshProUGUI AttemptText;
    public TextMeshProUGUI BestText;
    public TextMeshProUGUI TimingText;
    public TextMeshProUGUI AvgTimingText;
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
    protected float MapTotalTime = -1;
    private int[] _scorableTiles;
    public PlayCount.Hash LastHash;
    private float _lastSavedStartProgress = -1;
    public float LastMultiplier = 1f;
    private ComboTier _current;

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
        SetupMainText("PotentialAccuracy", ref PotentialAccuracyText);
        SetupMainText("XAccuracy", ref XAccuracyText);
        SetupMainText("PotentialXAccuracy", ref PotentialXAccuracyText);
        SetupMainText("XScore", ref XScoreText);
        SetupMainText("PotentialXScore", ref PotentialXScoreText);
        SetupMainText("MusicTime", ref TimeText);
        SetupMainText("MapTime", ref MapTimeText);
        SetupMainText("Checkpoint", ref CheckpointText);
        SetupMainText("Best", ref BestText);
        SetupMainText("Timing", ref TimingText);
        SetupMainText("AvgTiming", ref AvgTimingText);
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
        if(!GameObject.activeSelf) return;
        int y = -15;
        SetupLocationMainText(ProgressText, Status.Settings.ShowProgress, ref y);
        SetupLocationMainText(AccuracyText, Status.Settings.ShowAccuracy && Status.Settings.AccuracyTextType != PotentialTextType.Potential, ref y);
        SetupLocationMainText(PotentialAccuracyText, Status.Settings.ShowAccuracy && Status.Settings.AccuracyTextType is PotentialTextType.Potential or PotentialTextType.Both, ref y);
        SetupLocationMainText(XAccuracyText, Status.Settings.ShowXAccuracy && Status.Settings.XAccuracyTextType != PotentialTextType.Potential, ref y);
        SetupLocationMainText(PotentialXAccuracyText, Status.Settings.ShowXAccuracy && Status.Settings.XAccuracyTextType is PotentialTextType.Potential or PotentialTextType.Both, ref y);
        SetupLocationMainText(XScoreText, Status.Settings.ShowXScore && Status.XScoreSupported && Status.Settings.XScorePotentialTextType != PotentialTextType.Potential, ref y);
        SetupLocationMainText(PotentialXScoreText, Status.Settings.ShowXScore && Status.XScoreSupported && Status.Settings.XScorePotentialTextType is PotentialTextType.Potential or PotentialTextType.Both, ref y);
        SetupLocationMainText(TimeText, Status.Settings.ShowMusicTime, ref y);
        SetupLocationMainText(MapTimeText, Status.Settings.ShowMapTime, ref y);
        SetupLocationMainText(CheckpointText,
            Status.Settings.ShowCheckpoint &&
            (Checkpoints ??= scrLevelMaker.instance.listFloors.Where(floor => floor.GetComponent<ffxCheckpoint>())
                 .Select(floor => floor.seqID).ToArray()).Length > 0, ref y);
        SetupLocationMainText(BestText, Status.Settings.ShowBest, ref y);
        SetupLocationMainText(TimingText, Status.Settings.ShowTiming && Status.Settings.TimingTextType != TimingTextType.AvgTiming, ref y);
        SetupLocationMainText(AvgTimingText, Status.Settings.ShowTiming && Status.Settings.TimingTextType is TimingTextType.AvgTiming or TimingTextType.Both, ref y);
        UpdateProgress();
        VersionSafe.CalculatePercentAcc(); // UpdateAccuracy();
        UpdateTime();
        RefreshTiming();
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

    public virtual void UpdateFont() {
        ProgressText.font = BundleLoader.FontAsset;
        AccuracyText.font = BundleLoader.FontAsset;
        PotentialAccuracyText.font = BundleLoader.FontAsset;
        XAccuracyText.font = BundleLoader.FontAsset;
        PotentialXAccuracyText.font = BundleLoader.FontAsset;
        XScoreText.font = BundleLoader.FontAsset;
        PotentialXScoreText.font = BundleLoader.FontAsset;
        TimeText.font = BundleLoader.FontAsset;
        MapTimeText.font = BundleLoader.FontAsset;
        CheckpointText.font = BundleLoader.FontAsset;
        BestText.font = BundleLoader.FontAsset;
        TimingText.font = BundleLoader.FontAsset;
        AvgTimingText.font = BundleLoader.FontAsset;
        BpmText.font = BundleLoader.FontAsset;
        JudgementText.font = BundleLoader.FontAsset;
        ComboTitle.font = BundleLoader.FontAsset;
        ComboText.font = BundleLoader.FontAsset;
        TimingScaleText.font = BundleLoader.FontAsset;
        AttemptText.font = BundleLoader.FontAsset;
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
        StringBuilder sb = VersionSafe.GetSharedBuilder();

        if(Attempt.Settings.ShowAttempt) sb.Append("Attempt ").Append(PlayCount.GetData(LastHash)?.GetAttempts(StartProgress, LastMultiplier) ?? 0).Append('\n');
        if(Attempt.Settings.ShowFullAttempt) sb.Append("Full Attempt ").Append(PlayCount.GetData(LastHash)?.GetAttempts() ?? 0).Append('\n');

        sb.Length--;
        AttemptText.text = sb.ToString();
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
                TimeText.text = "<color=white>" + (Status.Settings.TimeTextType == TimeTextType.Korean ? "음악 시간" : "Music Time") + " |</color> " + timeStr + "~" + MusicTimeCache;
                _lastTime = (int) time;
                TimeText.color = Status.Settings.MusicTimeColor.GetColor(time / totalTime);
            }
        }
        if(Status.Settings.ShowMapTime || requireMusicToMap) {
            float time = scrController.instance.state == States.Start ? 0 : (float) (scrConductor.instance.addoffset + scrConductor.instance.songposition_minusi);
            float totalTime = GetMapTotalTime();
            if(time < 0) time = 0;
            else if(time > totalTime) time = totalTime;
            if((!Status.Settings.ShowMapTime || _lastMapTime == (int) time) &&
               (!requireMusicToMap || _lastTime == (int) time)) return;
            bool hourNeed = totalTime >= 3600;
            MapTimeCache ??= GetTimeString(totalTime, hourNeed);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            string timeStr = time == totalTime ? MapTimeCache : GetTimeString(time, hourNeed);
            string text = "<color=white>" + (Status.Settings.TimeTextType == TimeTextType.Korean ? "맵 시간" : "Map Time") + " |</color> " + timeStr + "~" + MapTimeCache;
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
    
    public int GetRemainingTiles(int seqID) {
        List<scrFloor> floors = ADOBase.lm.listFloors;
        int last = floors.Count - 1;
        if(seqID < 0) seqID = 0;
        if(seqID >= last) return 0;
        if(!Status.XScoreSupported) return last - seqID;
        int[] scorableTiles = _scorableTiles ??= BuildScorableTiles(floors);
        return scorableTiles[last] - scorableTiles[seqID];
    }

    private static int[] BuildScorableTiles(List<scrFloor> floors) {
        int[] scorableTiles = new int[floors.Count];
        int count = 0;
        for(int i = 1; i < floors.Count; i++) {
            scrFloor floor = floors[i];
            if(!floor.midSpin && !floor.auto) count++;
            scorableTiles[i] = count;
        }
        return scorableTiles;
    }

    protected float GetMapTotalTime() {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if(MapTotalTime == -1) MapTotalTime = (float) scrLevelMaker.instance.listFloors[^1].entryTime;
        return MapTotalTime;
    }

    private static string GetTimeString(float time, bool hour) {
        int timeInt = (int) time;
        return hour ? (timeInt / 3600) + ":" + (timeInt % 3600 / 60).ToString("00") + ":" + (timeInt % 60).ToString("00") :
                      (timeInt / 60) + ":" + (timeInt % 60).ToString("00");
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
        BpmText.text = "<color=white>TBPM | <color=#" + ColorToHex(Bpm.Settings.BpmColor.GetColor(bpm / Bpm.Settings.BpmColorMax)) + ">" + Math.Round(bpm, Bpm.Settings.DecimalPlaces) +
                       "</color>\nCBPM |</color> " + Math.Round(cbpm, Bpm.Settings.DecimalPlaces) +
                       "\n<color=white>KPS |</color> " + Math.Round(kps, Bpm.Settings.DecimalPlaces);
        if(LastCurBpm != cbpm) BpmText.color = Bpm.Settings.BpmColor.GetColor(cbpm / Bpm.Settings.BpmColorMax);
        // ReSharper restore CompareOfFloatsByEqualityOperator
        LastTileBpm = bpm;
        LastCurBpm = cbpm;
    }

    private const string HexDigits = "0123456789ABCDEF";

    // ReSharper disable once CompareOfFloatsByEqualityOperator
    public static string ColorToHex(Color color) {
        bool withAlpha = color.a != 1;
        char[] chars = new char[withAlpha ? 8 : 6];
        WriteHexByte(chars, 0, Mathf.RoundToInt(color.r * 255));
        WriteHexByte(chars, 2, Mathf.RoundToInt(color.g * 255));
        WriteHexByte(chars, 4, Mathf.RoundToInt(color.b * 255));
        if(withAlpha) WriteHexByte(chars, 6, Mathf.RoundToInt(color.a * 255));
        return new string(chars);
    }

    private static void WriteHexByte(char[] chars, int index, int value) {
        if(value < 0) value = 0;
        else if(value > 255) value = 255;
        chars[index] = HexDigits[value >> 4];
        chars[index + 1] = HexDigits[value & 0xF];
    }

    public void UpdateTiming(float timing, int player = -1) {
        if(!Status.Settings.ShowTiming || !GameObject.activeSelf) return;
        OverlayTextManager.UpdateTiming(this, timing, player);
    }

    public void RefreshTiming() {
        if(!GameObject.activeSelf || OverlayTextManager == null) return;
        OverlayTextManager.RefreshTiming(this);
    }

    public void UpdateTimingScale() {
        if(!GameObject.activeSelf) return;
        TimingScaleText.text = "Timing Scale - " + Math.Round(scrController.instance.currFloor.marginScale * 100, 2) + "%";
    }

    public void ChangeComboText(ComboTier tier) {
        if(_current >= tier) return;
        ComboTitle.text = tier == ComboTier.Green ? "Perfect" : "Combo";
        _current = tier;
    }
    
    public virtual void Show(int floor) {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if(_lastSavedStartProgress != -1) {
            if(!AutoOnceEnabled) PlayCount.SetBest(LastHash, _lastSavedStartProgress, OverlayTextManager.GetProgress(), LastMultiplier);
            _lastSavedStartProgress = -1;
        }

        if(scrController.checkpointsUsed == 0) {
            if(VersionControl.releaseNumber >= 149) {
                _current = ComboTier.White;
                ComboTitle.text = "XPerfect";
            } else {
                _current = ComboTier.Green;
                ComboTitle.text = "Perfect";
            }
        }
        
        PlayCount.Hash hash = PlayCount.GetMapHash();
        if(LastHash != hash) {
            LastHash = hash;
            Checkpoints = null;
            MapTimeCache = null;
            MapTotalTime = -1;
            _scorableTiles = null;
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
    
    public void Hide() {
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
