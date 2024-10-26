using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JALib.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace JipperResourcePack;

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
    public GameObject ComboObject;
    public TextMeshProUGUI ComboTitle;
    public TextMeshProUGUI ComboText;
    public TextMeshProUGUI BPMText;
    public TextMeshProUGUI JudgementText;
    public TextMeshProUGUI TimingScaleText;
    public ProgressBar ProgressBar;
    public Color PurePerfectColor = new(1, 0.8549019607843137f, 0);
    public float Progress;
    public int[] hit = scrMistakesManager.hitMarginsCount;
    public Shader Shader = (Shader) typeof(ShaderUtilities).Property("ShaderRef_MobileSDF").GetValue(null);
    protected int lastTime = -1;
    protected int lastMapTime = -1;
    protected int startTile;
    protected int noCheckStartTile;
    protected int lastCheckpoint = -1;
    protected int[] checkpoints;
    protected int curCheck;
    protected float lastTileBPM = -1;
    protected float lastCurBPM = -1;
    private Stopwatch Stopwatch;
    protected bool songPlaying;
    protected float startProgress;
    protected float curBest = -1;
    protected bool autoOnceEnabled;
    protected bool death;

    public Overlay() {
        Instance = this;
        GameObject = new GameObject("JipperResourcePack Overlay");
        Canvas = GameObject.AddComponent<Canvas>();
        Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = Canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        Canvas.gameObject.AddComponent<GraphicRaycaster>();
        Stopwatch = new Stopwatch();
        GameObject.SetActive(false);
        InitializeStatus();
        InitializeBPM();
        InitializeJudgement();
        InitializeCombo();
        InitializeProgressBar();
        InitializeTimingScale();
        InitializeAttempt();
        UpdateSize();
        Object.DontDestroyOnLoad(Canvas.gameObject);
        if(ADOBase.controller && ADOBase.conductor && ADOBase.conductor.isGameWorld) Show();
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
        checkpoints ??= scrLevelMaker.instance.listFloors.FindAll(floor => floor.GetComponent<ffxCheckpoint>()).Select(floor => floor.seqID).ToArray();
        SetupLocationMainText(CheckpointText, Status.Settings.ShowCheckpoint && checkpoints.Length > 0, ref y);
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
        BPM.BPMObject = gameObject;
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
        GameObject gameObject2 = new("ComboTitle");
        transform = gameObject2.AddComponent<RectTransform>();
        transform.SetParent(gameObject.transform);
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
        ComboObject = gameObject2;
        gameObject2 = new GameObject("ComboValue");
        transform = gameObject2.AddComponent<RectTransform>();
        transform.SetParent(gameObject.transform);
        transform.anchorMin = transform.anchorMax = new Vector2(0.5f, 0.45f);
        transform.anchoredPosition = Vector2.zero;
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
        ProgressBar = gameObject.AddComponent<ProgressBar>();
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
        transform.anchoredPosition = new Vector2(300, 5);
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
        Combo.ComboObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -43 - 14 * size);
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
        xacc.SetIfNaN(1);
        if(Status.Settings.ShowAccuracy) {
            float acc = scrController.instance.mistakesManager?.percentAcc ?? 1;
            float maxAcc = 1 + (scrController.instance.currentSeqID - noCheckStartTile + 1) * 0.0001f;
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
        if(checkpoints.Length == 0) return;
        bool updated = false;
        while(checkpoints.Length > curCheck && scrController.instance.currentSeqID >= checkpoints[curCheck]) {
            curCheck++;
            updated = true;
        }
        if(lastCheckpoint == scrController.checkpointsUsed && !updated) return;
        CheckpointText.text = $"<color=white>CheckPoint |</color> {scrController.checkpointsUsed} ({curCheck}/{checkpoints.Length})";
        lastCheckpoint = scrController.checkpointsUsed;
    }

    public void UpdateAttempts() {
        AttemptText.text = $"Attempt {PlayCount.GetData()?.GetAttempts(startProgress) ?? 0}";
    }

    public void UpdateBest() {
        if(RDC.auto && !autoOnceEnabled) autoOnceEnabled = true;
        if(curBest == -1) curBest = PlayCount.GetData()?.GetBest(startProgress) ?? 0;
        else if(curBest > Progress || autoOnceEnabled) return;
        UpdateBestText();
    }

    public virtual void UpdateBestText() {
        float best = curBest > Progress || autoOnceEnabled ? curBest : Progress;
        BestText.text = $"<color=white>Best |</color> {Math.Round(best * 100, 2)}%";
        BestText.color = Status.Settings.BestColor.GetColor(best);
    }

    public void UpdateJudgement() {
        if(!GameObject.activeSelf) return;
        int[] hits = hit;
        JudgementText.text = $"{hits[9]} <color=red>{hits[0]} <color=#FF6F4E>{hits[1]} <color=#A0FF4E>{hits[2]} <color=#60FF4E>{hits[3] + hits[10]}</color> {hits[4]}</color> {hits[5]}</color> {hits[6]}</color> {hits[8]}";
    }
    
    public virtual void UpdateTime() {
        if(!GameObject.activeSelf || !Status.Instance.Enabled || death) return;
        bool requireMusicToMap = false;
        if(Status.Settings.ShowMusicTime) {
            AudioSource song = scrConductor.instance.song;
            if(!song?.clip && Status.Settings.ShowMapTimeIfNotMusic) requireMusicToMap = true;
            else {
                float time = song.time;
                float totalTime = song.clip?.length ?? 0;
                if(lastTime == (int) time) return;
                if(time > 0) songPlaying = true;
                else if(time == 0 && songPlaying) time = totalTime;
                TimeSpan now = TimeSpan.FromSeconds(time);
                TimeSpan length = TimeSpan.FromSeconds(totalTime);
                TimeText.text = $@"<color=white>{(Status.Settings.TimeTextType == TimeTextType.Korean ? "음악 시간" : "Music Time")} |</color> {now:m\:ss}~{length:m\:ss}";
                lastTime = (int) time;
                TimeText.color = Status.Settings.MusicTimeColor.GetColor(time / totalTime);
            }
        }
        if(Status.Settings.ShowMapTime || requireMusicToMap) {
            float time;
            float totalTime;
            time = (float) (scrConductor.instance.addoffset + scrConductor.instance.songposition_minusi);
            totalTime = (float) scrLevelMaker.instance.listFloors.Last().entryTime;
            if(time < 0) time = 0;
            else if(time > totalTime) time = totalTime;
            if((!Status.Settings.ShowMapTime || lastMapTime == (int) time) &&
               (!requireMusicToMap || lastTime == (int) time)) return;
            TimeSpan now = TimeSpan.FromSeconds(time);
            TimeSpan length = TimeSpan.FromSeconds(totalTime);
            string text = $@"<color=white>{(Status.Settings.TimeTextType == TimeTextType.Korean ? "맵 시간" : "Map Time")} |</color> {now:m\:ss}~{length:m\:ss}";
            if(Status.Settings.ShowMapTime) {
                MapTimeText.text = text;
                lastMapTime = (int) time;
                MapTimeText.color = Status.Settings.MapTimeColor.GetColor(time / totalTime);
            }
            if(requireMusicToMap) {
                TimeText.text = text;
                lastTime = (int) time;
                TimeText.color = Status.Settings.MusicTimeColor.GetColor(time / totalTime);
            }
        }
    }
    
    public void UpdateCombo(int combo, bool bump) {
        if(!GameObject.activeSelf) return;
        ComboText.text = combo.ToString();
        ComboText.color = UpdateComboColor(combo);
        if(bump) {
            Stopwatch.Restart();
            UpdateComboSize();
        } else {
            Stopwatch.Stop();
            ComboText.fontSize = 78;
            ComboObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 43.505f);
        }
    }

    public virtual Color UpdateComboColor(int combo) {
        if(combo > Combo.Settings.ComboColorMax) combo = Combo.Settings.ComboColorMax;
        return Combo.Settings.ComboColor.GetColor((float) combo / Combo.Settings.ComboColorMax);
    }

    public void UpdateComboSize() {
        if(!Stopwatch.IsRunning || !GameObject.activeSelf) return;
        double t = Stopwatch.Elapsed.TotalMilliseconds / 500;
        if(t > 1) {
            t = 1;
            Stopwatch.Stop();
        }
        ComboText.fontSize = 30 * OutExpoChange(t) + 78;
        UpdateComboLocation();
    }

    private async void UpdateComboLocation() {
        await Task.Yield();
        ComboObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, ComboText.GetComponent<RectTransform>().sizeDelta.y / 2);
    }
    
    private static float OutExpoChange(double t) => (float) (t == 1 ? 0 : Math.Pow(2, -10 * t));

    public virtual void UpdateBPM() {
        if(!GameObject.activeSelf) return;
        scrFloor floor = scrController.instance.currFloor ?? scrController.instance.firstFloor;
        scrConductor conductor = scrConductor.instance;
        float bpm = (float) (conductor.bpm * conductor.song.pitch * scrController.instance.speed);
        float cbpm = floor.nextfloor ? (float) (60.0 / (floor.nextfloor.entryTime - floor.entryTime) * conductor.song.pitch) : bpm;
        float kps = cbpm / 60;
        if(lastTileBPM == bpm && lastCurBPM == cbpm) return;
        BPMText.text = $"<color=white>TBPM | <color=#{ColorToHex(BPM.Settings.BpmColor.GetColor(bpm / BPM.Settings.BpmColorMax))}>{Math.Round(bpm, 2)}</color>\n" +
                       $"CBPM |</color> {Math.Round(cbpm, 2)}\n" +
                       $"<color=white>KPS |</color> {Math.Round(kps, 2)}";
        if(lastCurBPM != cbpm) BPMText.color = BPM.Settings.BpmColor.GetColor(cbpm / BPM.Settings.BpmColorMax);
        lastTileBPM = bpm;
        lastCurBPM = cbpm;
    }

    protected static string ColorToHex(Color color) => $"{Mathf.RoundToInt(color.r * 255):X2}{Mathf.RoundToInt(color.g * 255):X2}{Mathf.RoundToInt(color.b * 255):X2}{(color.a == 1 ? "" : Mathf.RoundToInt(color.a * 255).ToString("X2"))}";

    public void UpdateTimingScale() {
        if(!GameObject.activeSelf) return;
        TimingScaleText.text = $"Timing Scale - {Math.Round(scrController.instance.currFloor.marginScale * 100, 2)}%";
    }
    
    public virtual void Show() {
        bool active = GameObject.activeSelf;
        if(active && ADOBase.isScnGame) return;
        autoOnceEnabled = RDC.auto || ADOBase.controller.noFail;
        if(!autoOnceEnabled && active) PlayCount.SetBest(startProgress, Progress);
        MainThread.Run(new JAction(Main.Instance, () => {
            if(ADOBase.isScnGame && scrController.checkpointsUsed == 0) {
                checkpoints = null;
                noCheckStartTile = startTile = 0;
                startProgress = 1f / ADOBase.lm.listFloors.Count;
                curBest = lastCheckpoint = -1;
            } else {
                if(!active) {
                    checkpoints = null;
                    noCheckStartTile = scrController.instance.currentSeqID;
                }
                if(!active || scrController.checkpointsUsed != 0) {
                    startTile = scrController.instance.currentSeqID;
                    startProgress = scrController.instance.percentComplete;
                    curBest = lastCheckpoint = -1;
                }
            }
            if(Status.Instance.Enabled && !autoOnceEnabled) PlayCount.AddAttempts(startProgress);
            GameObject.SetActive(true);
            curCheck = 0;
            scrMistakesManager manager = scrController.instance.mistakesManager;
            manager.percentAcc = 1;
            manager.percentXAcc = 1;
            songPlaying = false;
            death = false;
            if(Status.Instance.Enabled) SetupLocationMain();
            if(Judgement.Instance.Enabled) UpdateJudgement();
            if(Combo.Instance.Enabled) UpdateCombo(0, false);
            if(BPM.Instance.Enabled) UpdateBPM();
            if(TimingScale.Instance.Enabled) UpdateTimingScale();
            if(Attempt.Instance.Enabled) UpdateAttempts();
            Combo.combo = 0;
        }));
    }

    public void Death() {
        death = true;
        if(!autoOnceEnabled) PlayCount.SetBest(startProgress, Progress);
    }
    
    public virtual void Hide() {
        if(!GameObject.activeSelf) return;
        GameObject.SetActive(false);
        if(!autoOnceEnabled && startProgress != -1) PlayCount.SetBest(startProgress, Progress);
        if(startProgress == Progress && !autoOnceEnabled) PlayCount.RemoveAttempts(startProgress);
        startProgress = startTile = noCheckStartTile = -1;
    }

    public void Destroy() {
        Object.Destroy(GameObject);
        GC.SuppressFinalize(this);
    }
}
