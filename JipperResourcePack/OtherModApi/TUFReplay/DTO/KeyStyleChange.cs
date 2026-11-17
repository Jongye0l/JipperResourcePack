using JipperResourcePack.KeyViewerContents;

namespace JipperResourcePack.OtherModApi.TUFReplay.DTO;

public sealed class KeyStyleChange {
	public int KeyStyle;
	public int[] KeyCode;
	public string[] KeyNames;

	public KeyStyleChange() {}

	public KeyStyleChange(KeyviewerStyle style, int[] keyCode, string[] keyNames) {
		KeyStyle = (int) style;
		KeyCode = keyCode;
		KeyNames = keyNames;
	}

	public KeyStyleChange(FootKeyviewerStyle style, int[] keyCode) {
		KeyStyle = (int) style | int.MinValue;
		KeyCode = keyCode;
	}

	public bool IsFootKey => (KeyStyle & int.MinValue) != 0;
	public KeyviewerStyle KeyViewerStyle => (KeyviewerStyle) KeyStyle;
	public FootKeyviewerStyle FootKeyViewerStyle => (FootKeyviewerStyle) (KeyStyle & ~int.MinValue);
}