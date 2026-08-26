using JALib.Core;
using JALib.Core.Setting;
using JipperResourcePack.KeyViewerContents;
using JipperResourcePack.OverlayContents;
using JipperResourcePack.SettingTool;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace JipperResourcePack;

public class AllColor : Feature {
	public readonly AllColorSetting Settings;

	public AllColor() : base(Main.Instance, nameof(AllColor), false, settingType: typeof(AllColorSetting)) {
		Settings = (AllColorSetting) Setting;
	}

	protected override void OnGUI() {
		Settings.BaseColor.SettingGUI(Main.SettingGUI, false);
		JALocalization localization = Main.Instance.Localization;

		GUILayout.BeginHorizontal();
		if(GUILayout.Button(localization["allColor.red"])) Settings.BaseColor.SetPreset(Color.red);
		if(GUILayout.Button(localization["allColor.orange"])) Settings.BaseColor.SetPreset(Color.orange);
		if(GUILayout.Button(localization["allColor.yellow"])) Settings.BaseColor.SetPreset(Color.yellow);
		if(GUILayout.Button(localization["allColor.green"])) Settings.BaseColor.SetPreset(Color.green);
		if(GUILayout.Button(localization["allColor.skyBlue"])) Settings.BaseColor.SetPreset(Color.skyBlue);
		if(GUILayout.Button(localization["allColor.blue"])) Settings.BaseColor.SetPreset(Color.blue);
		if(GUILayout.Button(localization["allColor.purple"])) Settings.BaseColor.SetPreset(Color.purple);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		DrawBaseColorPreview(Settings.BaseColor);

		if(GUILayout.Button(localization["allColor.apply"])) ApplyBaseColor();
		if(GUILayout.Button(localization["allColor.resetAll"])) ResetAllColors();
	}

	private void ApplyBaseColor() {
		Color baseColor = Settings.BaseColor;
		Status.Settings.ProgressColor.ApplyBaseColor(baseColor);
		Status.Settings.AccuracyColor.ApplyBaseColor(baseColor);
		Status.Settings.XAccuracyColor.ApplyBaseColor(baseColor);
		Status.Settings.MusicTimeColor.ApplyBaseColor(baseColor);
		Status.Settings.MapTimeColor.ApplyBaseColor(baseColor);
		Status.Settings.BestColor.ApplyBaseColor(baseColor);
		Status.Settings.ProgressBarColor.ApplyBaseColor(baseColor);
		Status.Settings.ProgressBarBackgroundColor.ApplyBaseColor(baseColor);
		Status.Settings.ProgressBarBorderColor.ApplyBaseColor(baseColor);

		Bpm.Settings.BpmColor.ApplyBaseColor(baseColor);
		Combo.Settings.ComboColor.ApplyBaseColor(baseColor);

		KeyViewer.Settings.Background.ApplyHue(baseColor);
		KeyViewer.Settings.BackgroundClicked.ApplyHue(baseColor);
		KeyViewer.Settings.Outline.ApplyHue(baseColor);
		KeyViewer.Settings.OutlineClicked.ApplyHue(baseColor);
		KeyViewer.Settings.Text.ApplyHue(baseColor);
		KeyViewer.Settings.TextClicked.ApplyHue(baseColor);
		KeyViewer.Settings.RainColor.ApplyHue(baseColor);
		KeyViewer.Settings.RainColor2.ApplyHue(baseColor);
		KeyViewer.Settings.RainColor3.ApplyHue(baseColor);

		Overlay.Instance.OverlayTextManager.UpdateProgress(Overlay.Instance);
		Overlay.Instance.UpdateAccuracy();
		Overlay.Instance.UpdateTime();
		Overlay.Instance.OverlayTextManager.UpdateBest(Overlay.Instance);
		Overlay.Instance.UpdateProgressBar();
		Overlay.Instance.UpdateBpm();
		Overlay.Instance.UpdateComboColor(Combo.ComboCount);

		if(KeyViewer.Instance.Enabled) KeyViewer.Instance.RefreshColors();

		Main.Instance.SaveSetting();
	}

	private void ResetAllColors() {
		Status.Settings.ProgressColor.Reset();
		Status.Settings.AccuracyColor.Reset();
		Status.Settings.XAccuracyColor.Reset();
		Status.Settings.MusicTimeColor.Reset();
		Status.Settings.MapTimeColor.Reset();
		Status.Settings.BestColor.Reset();
		Status.Settings.ProgressBarColor.Reset();
		Status.Settings.ProgressBarBackgroundColor.Reset();
		Status.Settings.ProgressBarBorderColor.Reset();

		Bpm.Settings.BpmColor.Reset();
		Combo.Settings.ComboColor.Reset();

		KeyViewer.Settings.Background.Reset();
		KeyViewer.Settings.BackgroundClicked.Reset();
		KeyViewer.Settings.Outline.Reset();
		KeyViewer.Settings.OutlineClicked.Reset();
		KeyViewer.Settings.Text.Reset();
		KeyViewer.Settings.TextClicked.Reset();
		KeyViewer.Settings.RainColor.Reset();
		KeyViewer.Settings.RainColor2.Reset();
		KeyViewer.Settings.RainColor3.Reset();

		Overlay.Instance.OverlayTextManager.UpdateProgress(Overlay.Instance);
		Overlay.Instance.UpdateAccuracy();
		Overlay.Instance.UpdateTime();
		Overlay.Instance.OverlayTextManager.UpdateBest(Overlay.Instance);
		Overlay.Instance.UpdateProgressBar();
		Overlay.Instance.UpdateBpm();
		Overlay.Instance.UpdateComboColor(Combo.ComboCount);

		if(KeyViewer.Instance.Enabled) KeyViewer.Instance.RefreshColors();

		Main.Instance.SaveSetting();
	}

	private void DrawBaseColorPreview(Color baseColor) {
		float t20 = Time.unscaledTime % 20f / 20f;
		bool pressed = Time.unscaledTime % 1f >= 0.5f;

		if(Status.Instance.Enabled) {
			float t10 = Time.unscaledTime % 10f / 10f;
			Status.StatusSetting statusSettings = Status.Settings;
			if(statusSettings.ShowProgress)
				AddColorPreviewLine("Progress", statusSettings.ProgressColor.GetColor(t20), statusSettings.ProgressColor.GetPreviewColor(t20, baseColor), $"{t20 * 100:F1}%");
			if(statusSettings.ShowAccuracy)
				AddColorPreviewLine("Accuracy", statusSettings.AccuracyColor.GetColor(t20), statusSettings.AccuracyColor.GetPreviewColor(t20, baseColor), $"{t20 * 100:F1}%");
			if(statusSettings.ShowXAccuracy)
				AddColorPreviewLine("XAccuracy", statusSettings.XAccuracyColor.GetColor(t20), statusSettings.XAccuracyColor.GetPreviewColor(t20, baseColor), $"{t20 * 100:F1}%");
			if(statusSettings.ShowBest)
				AddColorPreviewLine("Best", statusSettings.BestColor.GetColor(t20), statusSettings.BestColor.GetPreviewColor(t20, baseColor), $"{t20 * 100:F1}%");
			if(statusSettings.ShowMusicTime)
				AddColorPreviewLine("MusicTime", statusSettings.MusicTimeColor.GetColor(t10), statusSettings.MusicTimeColor.GetPreviewColor(t10, baseColor), FormatTime(t10 * 10f));
			if(statusSettings.ShowMapTime)
				AddColorPreviewLine("MapTime", statusSettings.MapTimeColor.GetColor(t10), statusSettings.MapTimeColor.GetPreviewColor(t10, baseColor), FormatTime(t10 * 10f));
			if(statusSettings.ShowProgressBar) {
				AddColorPreviewLine("ProgressBar", statusSettings.ProgressBarColor.GetColor(t20), statusSettings.ProgressBarColor.GetPreviewColor(t20, baseColor));
				AddColorPreviewLine("ProgressBarBackground", statusSettings.ProgressBarBackgroundColor.GetColor(t20), statusSettings.ProgressBarBackgroundColor.GetPreviewColor(t20, baseColor));
				AddColorPreviewLine("ProgressBarBorder", statusSettings.ProgressBarBorderColor.GetColor(t20), statusSettings.ProgressBarBorderColor.GetPreviewColor(t20, baseColor));
			}
		}

		if(Bpm.Instance.Enabled) {
			Bpm.BpmSettings bpmSettings = Bpm.Settings;
			float minBpm = bpmSettings.BpmColor.List.Count > 0 ? bpmSettings.BpmColor.List[0].Progress * bpmSettings.BpmColorMax : 0;
			float maxBpm = bpmSettings.BpmColor.List.Count > 0 ? bpmSettings.BpmColor.List[^1].Progress * bpmSettings.BpmColorMax : bpmSettings.BpmColorMax;
			float bpm = Mathf.Lerp(minBpm, maxBpm, t20);
			AddColorPreviewLine("BPM", bpmSettings.BpmColor.GetColor(t20), bpmSettings.BpmColor.GetPreviewColor(t20, baseColor), $"{Mathf.RoundToInt(bpm)}");
		}

		if(Combo.Instance.Enabled) {
			Combo.ComboSettings comboSettings = Combo.Settings;
			float combo = t20 * comboSettings.ComboColorMax;
			AddColorPreviewLine("Combo", comboSettings.ComboColor.GetColor(t20), comboSettings.ComboColor.GetPreviewColor(t20, baseColor), $"{Mathf.RoundToInt(combo)}");
		}

		if(KeyViewer.Instance.Enabled) {
			KeyViewerSetting keySettings = KeyViewer.Settings;
			ColorCache background = pressed ? keySettings.BackgroundClicked : keySettings.Background;
			ColorCache outline = pressed ? keySettings.OutlineClicked : keySettings.Outline;
			ColorCache text = pressed ? keySettings.TextClicked : keySettings.Text;
			AddColorPreviewLine("Background", background, background.ApplyHuePreview(baseColor));
			AddColorPreviewLine("Outline", outline, outline.ApplyHuePreview(baseColor));
			AddColorPreviewLine("Text", text, text.ApplyHuePreview(baseColor));
			AddColorPreviewLine("RainColor", keySettings.RainColor, keySettings.RainColor.ApplyHuePreview(baseColor));
			AddColorPreviewLine("RainColor2", keySettings.RainColor2, keySettings.RainColor2.ApplyHuePreview(baseColor));
			if(keySettings.KeyViewerStyle == KeyviewerStyle.Key20)
				AddColorPreviewLine("RainColor3", keySettings.RainColor3, keySettings.RainColor3.ApplyHuePreview(baseColor));
		}
	}

	private static void AddColorPreviewLine(string label, Color oldColor, Color newColor, string value = null) {
		if(value == null) (value, label) = (label, null);
		GUILayout.Label($"{(label == null ? null : label + ": ")}<color=#{ColorUtility.ToHtmlStringRGB(oldColor)}>{value}</color> -> <color=#{ColorUtility.ToHtmlStringRGB(newColor)}>{value}</color>");
	}

	private static string FormatTime(float seconds) => $"{(int) (seconds / 60):00}:{(int) (seconds % 60):00}";

	public class AllColorSetting : JASetting {
		public ColorCache BaseColor;
		
		public AllColorSetting(JAMod mod, JObject jsonObject = null) : base(mod, jsonObject) {
			ColorCache.Setup(ref BaseColor, Color.purple);
		}
	}
}