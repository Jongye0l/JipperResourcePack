using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using JALib.Tools;
using SkyHook;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents.KeyMapper;

public class KeyCodeMapperLinux : KeyCodeMapper {
	private readonly FrozenDictionary<KeyCode, ushort[]> _keyCodeMap1;
	private readonly FrozenDictionary<KeyCode, ushort> _keyCodeMap2;
	private readonly FrozenDictionary<ushort, KeyCode> _keyCodeMap3;

	public KeyCodeMapperLinux() {
		if(VersionControl.releaseNumber < 145)
			Main.Instance.Warning("JipperResourcePack can't find the Input System on Linux for versions lower r145. Forcing it to use XInput.");

		switch(VersionSafe.KeyLabelToNativeKeyCode(KeyLabel.Escape)) {
			case 1: // InputDev
				_keyCodeMap1 = new Dictionary<KeyCode, ushort[]> {
					{ KeyCode.AltGr, [ 85, 195 ] }
				}.ToFrozenDictionary();

				_keyCodeMap2 = new Dictionary<KeyCode, ushort> {
					{ KeyCode.BackQuote, 41 },
					{ KeyCode.SysReq, 84 },
					{ KeyCode.Less, 85 },
					{ KeyCode.KeypadEquals, 117 },
					{ KeyCode.WheelDown, 131 },
					{ KeyCode.Help, 138 },
					{ KeyCode.Menu, 139 },
					{ KeyCode.LeftParen, 179 },
					{ KeyCode.RightParen, 180 }
				}.ToFrozenDictionary();

				_keyCodeMap3 = new Dictionary<ushort, KeyCode> {
					{ 13, KeyCode.Equals },
					{ 26, KeyCode.LeftBracket },
					{ 27, KeyCode.RightBracket },
					{ 39, KeyCode.Semicolon },
					{ 41, KeyCode.BackQuote },
					{ 43, KeyCode.Backslash },
					{ 51, KeyCode.Comma },
					{ 53, KeyCode.Slash },
					{ 84, KeyCode.SysReq },
					{ 85, KeyCode.AltGr },
					{ 86, KeyCode.Less },
					{ 117, KeyCode.KeypadEquals },
					{ 131, KeyCode.WheelDown },
					{ 138, KeyCode.Help },
					{ 139, KeyCode.Menu },
					{ 179, KeyCode.LeftParen },
					{ 180, KeyCode.RightParen },
					{ 195, KeyCode.AltGr }
				}.ToFrozenDictionary();
				break;
			case 0xff1b: // XInput
				_keyCodeMap1 = new Dictionary<KeyCode, ushort[]> {
					{ KeyCode.KeypadPeriod, [ 0xFFAE, 0xFF9F ] },
					{ KeyCode.A, [ 0x41, 0x61 ] },
					{ KeyCode.B, [ 0x42, 0x62 ] },
					{ KeyCode.C, [ 0x43, 0x63 ] },
					{ KeyCode.D, [ 0x44, 0x64 ] },
					{ KeyCode.E, [ 0x45, 0x65 ] },
					{ KeyCode.F, [ 0x46, 0x66 ] },
					{ KeyCode.G, [ 0x47, 0x67 ] },
					{ KeyCode.H, [ 0x48, 0x68 ] },
					{ KeyCode.I, [ 0x49, 0x69 ] },
					{ KeyCode.J, [ 0x4a, 0x6a ] },
					{ KeyCode.K, [ 0x4b, 0x6b ] },
					{ KeyCode.L, [ 0x4c, 0x6c ] },
					{ KeyCode.M, [ 0x4d, 0x6d ] },
					{ KeyCode.N, [ 0x4e, 0x6e ] },
					{ KeyCode.O, [ 0x4f, 0x6f ] },
					{ KeyCode.P, [ 0x50, 0x70 ] },
					{ KeyCode.Q, [ 0x51, 0x71 ] },
					{ KeyCode.R, [ 0x52, 0x72 ] },
					{ KeyCode.S, [ 0x53, 0x73 ] },
					{ KeyCode.T, [ 0x54, 0x74 ] },
					{ KeyCode.U, [ 0x55, 0x75 ] },
					{ KeyCode.V, [ 0x56, 0x76 ] },
					{ KeyCode.W, [ 0x57, 0x77 ] },
					{ KeyCode.X, [ 0x58, 0x78 ] },
					{ KeyCode.Y, [ 0x59, 0x79 ] },
					{ KeyCode.Z, [ 0x5a, 0x7a ] },
					{ KeyCode.Delete, [ 0xFFAE, 0xFFFF ] },
					{ KeyCode.Insert, [ 0xFF63, 0xFFB0 ] },
					{ KeyCode.End, [ 0xFF57, 0xFFB1 ] },
					{ KeyCode.DownArrow, [ 0xFF54, 0xFFB2 ] },
					{ KeyCode.PageDown, [ 0xFF56, 0xFFB3 ] },
					{ KeyCode.LeftArrow, [ 0xFF51, 0xFFB4 ] },
					{ KeyCode.RightArrow, [ 0xFF53, 0xFFB6 ] },
					{ KeyCode.Home, [ 0xFF50, 0xFFB7 ] },
					{ KeyCode.UpArrow, [ 0xFF52, 0xFFB8 ] },
					{ KeyCode.PageUp, [ 0xFF55, 0xFFB9 ] },
					{ KeyCode.LeftMeta, [ 0xFFE7, 0xFFEB ] },
					{ KeyCode.RightMeta, [ 0xFFE8, 0xFFEC ] },
					{ KeyCode.RightAlt, [ 0xFE03, 0xFFEA ] }
				}.ToFrozenDictionary();

				_keyCodeMap2 = new Dictionary<KeyCode, ushort> {
					{ KeyCode.Semicolon, 0x3B },
					{ KeyCode.BackQuote, 0x60 },
					{ KeyCode.Keypad7, 0xFF95 },
					{ KeyCode.Keypad8, 0xFF97 },
					{ KeyCode.Keypad9, 0xFF9A },
					{ KeyCode.Keypad4, 0xFF96 },
					{ KeyCode.Keypad5, 0xFF9D },
					{ KeyCode.Keypad6, 0xFF98 },
					{ KeyCode.Keypad1, 0xFF9C },
					{ KeyCode.Keypad2, 0xFF99 },
					{ KeyCode.Keypad3, 0xFF9B },
					{ KeyCode.Keypad0, 0xFF9E },
					{ KeyCode.KeypadPeriod, 0xFF9F },
					{ KeyCode.Less, 0x3C },
					{ KeyCode.KeypadEquals, 0xFFBD },
					{ KeyCode.Menu, 0xFF67 },
					{ KeyCode.WheelDown, 0xFF65 },
					{ KeyCode.Help, 0xFF6A },
					{ KeyCode.LeftParen, 0x28 },
					{ KeyCode.RightParen, 0x29 },
					{ KeyCode.RightMeta, 0xFFEC },
					{ KeyCode.SysReq, 0xFF15 },
					{ KeyCode.AltGr, 0xFF7E }
				}.ToFrozenDictionary();

				_keyCodeMap3 = new Dictionary<ushort, KeyCode> {
					{ 0x3B, KeyCode.Semicolon },
					{ 0x60, KeyCode.BackQuote },
					{ 0xFF95, KeyCode.Keypad7 },
					{ 0xFF97, KeyCode.Keypad8 },
					{ 0xFF9A, KeyCode.Keypad9 },
					{ 0xFF96, KeyCode.Keypad4 },
					{ 0xFF9D, KeyCode.Keypad5 },
					{ 0xFF98, KeyCode.Keypad6 },
					{ 0xFF9C, KeyCode.Keypad1 },
					{ 0xFF99, KeyCode.Keypad2 },
					{ 0xFF9B, KeyCode.Keypad3 },
					{ 0xFF9E, KeyCode.Keypad0 },
					{ 0xFF9F, KeyCode.KeypadPeriod },
					{ 0xFFEA, KeyCode.RightAlt },
					{ 0x3C, KeyCode.Less },
					{ 0xFFBD, KeyCode.KeypadEquals },
					{ 0xFF67, KeyCode.Menu },
					{ 0xFF65, KeyCode.WheelDown },
					{ 0xFF6A, KeyCode.Help },
					{ 0x28, KeyCode.LeftParen },
					{ 0x29, KeyCode.RightParen },
					{ 0x41, KeyCode.A },
					{ 0x42, KeyCode.B },
					{ 0x43, KeyCode.C },
					{ 0x44, KeyCode.D },
					{ 0x45, KeyCode.E },
					{ 0x46, KeyCode.F },
					{ 0x47, KeyCode.G },
					{ 0x48, KeyCode.H },
					{ 0x49, KeyCode.I },
					{ 0x4A, KeyCode.J },
					{ 0x4B, KeyCode.K },
					{ 0x4C, KeyCode.L },
					{ 0x4D, KeyCode.M },
					{ 0x4E, KeyCode.N },
					{ 0x4F, KeyCode.O },
					{ 0x50, KeyCode.P },
					{ 0x51, KeyCode.Q },
					{ 0x52, KeyCode.R },
					{ 0x53, KeyCode.S },
					{ 0x54, KeyCode.T },
					{ 0x55, KeyCode.U },
					{ 0x56, KeyCode.V },
					{ 0x57, KeyCode.W },
					{ 0x58, KeyCode.X },
					{ 0x59, KeyCode.Y },
					{ 0x5A, KeyCode.Z },
					{ 0x5B, KeyCode.LeftBracket },
					{ 0x5C, KeyCode.Backslash },
					{ 0x5D, KeyCode.RightBracket },
					{ 0x2F, KeyCode.Slash },
					{ 0x2B, KeyCode.Greater },
					{ 0x3D, KeyCode.Equals },
					{ 0x2C, KeyCode.Comma },
					{ 0xFF15, KeyCode.SysReq },
					{ 0xFF7E, KeyCode.AltGr },
					{ 0xFFAE, KeyCode.Delete },
					{ 0xFFB0, KeyCode.Insert },
					{ 0xFFB1, KeyCode.End },
					{ 0xFFB2, KeyCode.DownArrow },
					{ 0xFFB3, KeyCode.PageDown },
					{ 0xFFB4, KeyCode.LeftArrow },
					{ 0xFFB5, KeyCode.None },
					{ 0xFFB6, KeyCode.RightArrow },
					{ 0xFFB7, KeyCode.Home },
					{ 0xFFB8, KeyCode.UpArrow },
					{ 0xFFB9, KeyCode.PageUp },
					{ 0xFFEB, KeyCode.LeftMeta },
					{ 0xFFEC, KeyCode.RightMeta },
					{ 0xFFE7, KeyCode.LeftMeta },
					{ 0xFFE8, KeyCode.RightMeta }
				}.ToFrozenDictionary();
				break;
			default:
				throw new NotSupportedException($"Unsupported key code for Escape: {VersionSafe.KeyLabelToNativeKeyCode(KeyLabel.Escape)}");
		}
	}

	public override void AddKeyCode(HashSet<ushort> keyCodes, KeyCode keyCode) {
		if(_keyCodeMap1.TryGetValue(keyCode, out ushort[] codes)) {
			foreach(ushort code in codes) keyCodes.Add(code);
		} else if(_keyCodeMap2.TryGetValue(keyCode, out ushort code)) {
			keyCodes.Add(code);
		} else {
			base.AddKeyCode(keyCodes, keyCode);
		}
	}

	public override bool TryGetCustomKeyCode(ushort keyCode, out KeyCode key) {
		return _keyCodeMap3.TryGetValue(keyCode, out key);
	}
}