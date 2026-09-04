using System;
using UnityEngine;

namespace SkyHook;

public enum KeyLabel : ushort;

#if R141After
public static class SkyHookKeyMapper {
	public static KeyCode SkyHookKeyToUnityKey(KeyLabel key) => throw new Exception("stub");
	public static KeyLabel UnityKeyToSkyHookKey(KeyCode key) => throw new Exception("stub");
}
#else
public static class AsyncKeyMapper {
	public static KeyCode AsyncKeyToUnityKey(KeyLabel key) => throw new Exception("stub");
	public static KeyLabel UnityKeyToAsyncKey(KeyCode key) => throw new Exception("stub");
}
#endif