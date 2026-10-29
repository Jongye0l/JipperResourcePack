using System.Collections.Frozen;
using System.Collections.Generic;
using SkyHook;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents.KeyMapper;

public class KeyCodeMapper {
	public static KeyCodeMapper Instance;

	protected readonly FrozenDictionary<KeyLabel, KeyCode[]> KeyLabelToCodeMulti = new Dictionary<KeyLabel, KeyCode[]> {
		{ KeyLabel.Super, [ KeyCode.LeftWindows, KeyCode.RightWindows, KeyCode.LeftMeta, KeyCode.RightMeta ] },
		{ KeyLabel.BackSlash, [ KeyCode.Backslash, KeyCode.Pipe ] },
		{ KeyLabel.RightBrace, [ KeyCode.RightBracket, KeyCode.RightCurlyBracket ] },
		{ KeyLabel.LeftBrace, [ KeyCode.LeftBracket, KeyCode.LeftCurlyBracket ] },
		{ KeyLabel.Slash, [ KeyCode.Slash, KeyCode.Question ] },
		{ KeyLabel.Equal, [ KeyCode.Equals, KeyCode.Greater ] },
		{ KeyLabel.Comma, [ KeyCode.Comma, KeyCode.Less ] },
		{ KeyLabel.Semicolon, [ KeyCode.Colon, KeyCode.Semicolon ] },
		{ KeyLabel.Grave, [ KeyCode.BackQuote, KeyCode.Tilde ] }
	}.ToFrozenDictionary();

	protected readonly FrozenDictionary<KeyLabel, KeyCode> KeyLabelToCodeSolo = new Dictionary<KeyLabel, KeyCode> {
		{ KeyLabel.Apostrophe, KeyCode.Quote },
		{ KeyLabel.F16, KeyCode.F16 },
		{ KeyLabel.F17, KeyCode.F17 },
		{ KeyLabel.F18, KeyCode.F18 },
		{ KeyLabel.F19, KeyCode.F19 },
		{ KeyLabel.F20, KeyCode.F20 },
		{ KeyLabel.F21, KeyCode.F21 },
		{ KeyLabel.F22, KeyCode.F22 },
		{ KeyLabel.F23, KeyCode.F23 },
		{ KeyLabel.F24, KeyCode.F24 }
	}.ToFrozenDictionary();

	protected readonly FrozenDictionary<KeyCode, KeyLabel> KeyCodeToLabel = new Dictionary<KeyCode, KeyLabel> {
		{ KeyCode.Quote, KeyLabel.Apostrophe },
		{ KeyCode.F16, KeyLabel.F16 },
		{ KeyCode.F17, KeyLabel.F17 },
		{ KeyCode.F18, KeyLabel.F18 },
		{ KeyCode.F19, KeyLabel.F19 },
		{ KeyCode.F20, KeyLabel.F20 },
		{ KeyCode.F21, KeyLabel.F21 },
		{ KeyCode.F22, KeyLabel.F22 },
		{ KeyCode.F23, KeyLabel.F23 },
		{ KeyCode.F24, KeyLabel.F24 }
	}.ToFrozenDictionary();

	public static bool TryInit() {
		if(Instance != null) return true;
		if(ADOBase.platform == Platform.None) {
			Main.Instance.Log("Platform is None, retrying in next frame.");
			return false;
		}
		if(ADOBase.platform == Platform.Windows) {
			Instance = new KeyCodeMapperWindows();
		} else if(ADOBase.platform == Platform.Linux) {
			Instance = new KeyCodeMapperLinux();
		} else if(ADOBase.platform == Platform.Mac) {
			Instance = new KeyCodeMapperMac();
		} else {
			Instance = new KeyCodeMapper();
			Main.Instance.Warning($"Unsupported platform {ADOBase.platform}, using default mapper.");
		}
		return true;
	}

	public virtual void AddKeyCode(HashSet<ushort> keyCodes, KeyCode keyCode) {
		KeyLabel label = GetKeyLabel(keyCode);
		keyCodes.Add(VersionSafe.KeyLabelToNativeKeyCode(label));
	}

	public virtual bool TryGetCustomKeyCode(ushort keyCode, out KeyCode key) {
		key = KeyCode.None;
		return false;
	}

	public KeyLabel GetKeyLabel(KeyCode keyCode) {
		return KeyCodeToLabel.TryGetValue(keyCode, out KeyLabel label) ? label : VersionSafe.UnityKeyToSkyHookKey(keyCode);
	}
}