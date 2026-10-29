using System.Collections.Frozen;
using System.Collections.Generic;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents.KeyMapper;

public class KeyCodeMapperMac : KeyCodeMapper {
	private readonly KeyValuePair<KeyCode, ushort[]> _pair = new(KeyCode.Backslash, new ushort[] { 0x2A, 0x0A });

	private readonly FrozenDictionary<ushort, KeyCode> _keyCodeMap = new Dictionary<ushort, KeyCode> {
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

	public override void AddKeyCode(HashSet<ushort> keyCodes, KeyCode keyCode) {
		if(_pair.Key == keyCode) {
			foreach(ushort v in _pair.Value) keyCodes.Add(v);
		} else {
			base.AddKeyCode(keyCodes, keyCode);
		}
	}

	public override bool TryGetCustomKeyCode(ushort keyCode, out KeyCode key) {
		return _keyCodeMap.TryGetValue(keyCode, out key);
	}
}
