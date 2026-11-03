using System.Collections.Frozen;
using System.Collections.Generic;
using SkyHook;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents.KeyMapper;

public class KeyCodeMapperMac : KeyCodeMapper {
	// SkyHook's native key codes on macOS are not the same in every game version: older builds
	// report macOS virtual key codes (kVK_*), newer ones (R150 at least) USB HID usage IDs, where
	// 0x2A is Backspace rather than Backslash. The game's own table tells which: 'A' is kVK 0x00
	// but HID 0x04.
	private readonly FrozenDictionary<KeyCode, ushort[]> _nativeCodes;
	private readonly FrozenDictionary<ushort, KeyCode> _keyCodeMap;

	public KeyCodeMapperMac() {
		if(VersionSafe.KeyLabelToNativeKeyCode(KeyLabel.A) == 0x04) {
			// Only what SkyHook's own mapping gets wrong: Shift has no label, both Commands are
			// labelled Super (and map to LeftMeta), ';' maps to Colon and '\'' to nothing.
			_keyCodeMap = new Dictionary<ushort, KeyCode> {
				{ 0xE1, KeyCode.LeftShift },
				{ 0xE5, KeyCode.RightShift },
				{ 0xE3, KeyCode.LeftMeta },
				{ 0xE7, KeyCode.RightMeta },
				{ 0x33, KeyCode.Semicolon },
				{ 0x34, KeyCode.Quote }
			}.ToFrozenDictionary();
			_nativeCodes = new Dictionary<KeyCode, ushort[]> {
				{ KeyCode.LeftShift, [ 0xE1 ] },
				{ KeyCode.RightShift, [ 0xE5 ] },
				{ KeyCode.LeftMeta, [ 0xE3 ] },
				{ KeyCode.RightMeta, [ 0xE7 ] },
				{ KeyCode.Semicolon, [ 0x33 ] },
				{ KeyCode.Backslash, [ 0x31, 0x64 ] } // ANSI \ and the ISO key next to left Shift
			}.ToFrozenDictionary();
		} else {
			_keyCodeMap = new Dictionary<ushort, KeyCode> {
				{ 0x37, KeyCode.LeftMeta },
				{ 0x36, KeyCode.RightMeta },
				{ 0x2A, KeyCode.Backslash },
				{ 0x0A, KeyCode.Backslash }, // IDK
				{ 0x1E, KeyCode.RightBracket },
				{ 0x21, KeyCode.LeftBracket },
				{ 0x2C, KeyCode.Slash },
				{ 0x18, KeyCode.Equals },
				{ 0x2B, KeyCode.Comma },
				{ 0x29, KeyCode.Semicolon },
				{ 0x32, KeyCode.Tilde }
			}.ToFrozenDictionary();
			_nativeCodes = new Dictionary<KeyCode, ushort[]> {
				{ KeyCode.Backslash, [ 0x2A, 0x0A ] }
			}.ToFrozenDictionary();
		}
	}

	public override void AddKeyCode(HashSet<ushort> keyCodes, KeyCode keyCode) {
		if(_nativeCodes.TryGetValue(keyCode, out ushort[] codes)) {
			foreach(ushort code in codes) keyCodes.Add(code);
		} else {
			base.AddKeyCode(keyCodes, keyCode);
		}
	}

	public override bool TryGetCustomKeyCode(ushort keyCode, out KeyCode key) {
		return _keyCodeMap.TryGetValue(keyCode, out key);
	}
}
