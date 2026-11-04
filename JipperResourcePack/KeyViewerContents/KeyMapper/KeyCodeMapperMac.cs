using System.Collections.Frozen;
using System.Collections.Generic;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents.KeyMapper;

public class KeyCodeMapperMac : KeyCodeMapper {
	private readonly KeyValuePair<KeyCode, ushort[]> _pair = new(KeyCode.Backslash, new ushort[] { 0x31, 0x64 });
	private readonly FrozenDictionary<ushort, KeyCode> _keyCodeMap = new Dictionary<ushort, KeyCode> {
		{ 0xE1, KeyCode.LeftShift },
		{ 0xE5, KeyCode.RightShift },
		{ 0xE3, KeyCode.LeftMeta },
		{ 0xE7, KeyCode.RightMeta },
		{ 0x33, KeyCode.Semicolon },
		{ 0x34, KeyCode.Quote },
		{ 0x38, KeyCode.Slash },
		{ 0x2E, KeyCode.Equals },
		{ 0x31, KeyCode.Backslash },
		{ 0x64, KeyCode.Backslash },
		{ 0x30, KeyCode.RightBracket },
		{ 0x2F, KeyCode.LeftBracket },
		{ 0x36, KeyCode.Comma }
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
