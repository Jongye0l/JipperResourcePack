using System;
using JALib.Tools;
using JipperResourcePack.KeyViewerContents;
using JipperResourcePack.OverlayContents;

namespace JipperResourcePack.OtherModApi.TUFReplay.DTO;

public sealed class RunState {
	public int Attempts;
	public int FullAttempts;
	public float Best;
	public int[] KeyCounts;
	public int TotalKeyCount;
	public int KeyStyle;
	public int FootKeyStyle;
	public int[] KeyCode;
	public string[] KeyNames;

	public static RunState Capture() {
		RunState state = new();
		Overlay overlay = Overlay.Instance;
		if(overlay.LastHash.Data != null && overlay.StartProgress >= 0) {
			PlayCount.PlayData data = PlayCount.GetData(overlay.LastHash);
			state.Attempts = data.GetAttempts(overlay.StartProgress, overlay.LastMultiplier);
			state.FullAttempts = data.GetAttempts();
			float best = data.GetBest(overlay.StartProgress, overlay.LastMultiplier);
			if(!float.IsNaN(best) && !float.IsInfinity(best)) state.Best = best;
		}
		KeyCountData counts = KeyCountData.Instance;
		state.KeyCounts = counts.Count.Clone().AsUnsafe<int[]>();
		state.TotalKeyCount = counts.TotalCount;
		state.KeyStyle = (int) KeyViewer.Settings.KeyViewerStyle;
		state.FootKeyStyle = (int) KeyViewer.Settings.FootKeyViewerStyle;
		int size = KeyViewer.GetKeyCount(KeyViewer.Settings.KeyViewerStyle);
		int footSize = KeyViewer.GetKeyCount(KeyViewer.Settings.FootKeyViewerStyle);
		state.KeyCode = new int[size * 2 + footSize];
		Array.Copy(KeyViewer.GetKeyCode(), 0, state.KeyCode, 0, size);
		Array.Copy(KeyViewer.GetFootKeyCode(), 0, state.KeyCode, size, footSize);
		Array.Copy(KeyViewer.GetGhostKeyCode(), 0, state.KeyCode, size + footSize, size);
		state.KeyNames = new string[size];
		Array.Copy(KeyViewer.GetKeyText(), 0, state.KeyNames, 0, size);
		return state;
	}
}