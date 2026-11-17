namespace JipperResourcePack.OtherModApi.TUFReplay.DTO;

public sealed class KeyNameChange {
	public int Index;
	public string Text;

	public KeyNameChange() {
	}

	public KeyNameChange(int index, string text) {
		Index = index;
		Text = text;
	}
}