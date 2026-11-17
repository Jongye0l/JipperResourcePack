using UnityEngine;

namespace JipperResourcePack.OtherModApi.TUFReplay.DTO;

public class KeyChange {
	public int Index;
	public int KeyCode;

	public KeyChange() {}

	public KeyChange(int index, KeyCode keyCode) {
		Index = index;
		KeyCode = (int) keyCode;
	}
}