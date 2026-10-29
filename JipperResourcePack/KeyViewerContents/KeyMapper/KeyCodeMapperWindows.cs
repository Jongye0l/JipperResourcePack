using System.Collections.Frozen;
using System.Collections.Generic;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents.KeyMapper;

public class KeyCodeMapperWindows : KeyCodeMapper {
	private readonly FrozenDictionary<KeyCode, ushort[]> _keyCodeMap1 = new Dictionary<KeyCode, ushort[]> {
		{ KeyCode.Menu, [ 0x5D ] },
		{ KeyCode.LeftControl, [ 0xA2, 0xA3 ]},
		{ KeyCode.LeftAlt, [ 0xA4, 0xA5 ]},
		{ KeyCode.Semicolon, [ 0xBA ] },
		{ KeyCode.BackQuote, [ 0xC0, 0xDF ]},
		{ KeyCode.Backslash, [ 0xDC, 0xE2 ]},
		{ KeyCode.RightWindows, [ 92 ]}
	}.ToFrozenDictionary();

	private readonly FrozenDictionary<ushort, KeyCode> _keyCodeMap2 = new Dictionary<ushort, KeyCode> {
		{ 0x5D, KeyCode.Menu },
		{ 0xA3, KeyCode.LeftControl },
		{ 0xA5, KeyCode.LeftAlt },
		{ 0xBA, KeyCode.Semicolon },
		{ 0xC0, KeyCode.BackQuote },
		{ 0xDF, KeyCode.BackQuote },
		{ 0xE2, KeyCode.Backslash },
		{ 91, KeyCode.LeftWindows },
		{ 92, KeyCode.RightWindows },
		{ 220, KeyCode.Backslash },
		{ 221, KeyCode.RightBracket },
		{ 219, KeyCode.LeftBracket },
		{ 191, KeyCode.Slash },
		{ 187, KeyCode.Equals },
		{ 188, KeyCode.Comma }
	}.ToFrozenDictionary();

	public override void AddKeyCode(HashSet<ushort> keyCodes, KeyCode keyCode) {
		if(_keyCodeMap1.TryGetValue(keyCode, out ushort[] codes)) {
			foreach(ushort code in codes) keyCodes.Add(code);
		} else {
			base.AddKeyCode(keyCodes, keyCode);
		}
	}

	public override bool TryGetCustomKeyCode(ushort keyCode, out KeyCode key) {
		return _keyCodeMap2.TryGetValue(keyCode, out key);
	}
}