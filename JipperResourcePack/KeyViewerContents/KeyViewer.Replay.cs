using System;
using System.Collections.Frozen;
using JALib.Tools;
using JipperResourcePack.KeyViewerContents.KeyMapper;
using JipperResourcePack.OtherModApi.TUFReplay;
using JipperResourcePack.OtherModApi.TUFReplay.DTO;
using UnityEngine;

namespace JipperResourcePack.KeyViewerContents;

public partial class KeyViewer {
    // TUFReplay feeds recorded input directly while it replays a run, in the game or in a video.
    public void SetReplayInput(bool replay) {
        if(replay) {
            if(_listening) StopEventListener();
            // ReplayApi 2 hands over the run's start value after OnReplayBegin, and a run can
            // have none. Until it comes the replay shows the player's keys and counts on from
            // a copy of the player's counts.
            KeyCountData counts = new();
            KeyCountData player = KeyCountData.Instance;
            if(player != null) {
                Array.Copy(player.Count, counts.Count, counts.Count.Length);
                counts.TotalCount = player.TotalCount;
            }
            _replayCounts = counts;
            ApplyReplayState(TUFReplayAPI.Recorded);
            return;
        }
        if(_replayCounts == null) return;
        _replayKey = null;
        _replayFootKey = null;
        _replayGhostKey = null;
        _replayKeyText = null;
        _replayCounts = null;
        _currentKeyViewerStyle = Settings.KeyViewerStyle;
        _currentFootKeyViewerStyle = Settings.FootKeyViewerStyle;
        _eventOffsetTicks = NoEventOffset;
        if(!KeyViewerObject) return;
        ResetRain();
        ResetKeyViewer();
        ResetFootKeyViewer();
        if(Enabled && !_listening) StartEventListener();
    }

    // Shows the run's keys, styles and counts from its start value. A null value keeps the
    // player's keys, as a run TUFReplay saved no value for showed them.
    public void ApplyReplayState(RunState recorded) {
        if(_replayCounts == null) return;
        if(recorded != null) {
            SetReplayKey(recorded);
            SetReplayCounts(recorded);
            _currentKeyViewerStyle = (KeyviewerStyle) recorded.KeyStyle;
            _currentFootKeyViewerStyle = (FootKeyviewerStyle) recorded.FootKeyStyle;
            RebuildKeyBinding();
        }
        // A disabled key viewer has no keys to rebuild; OnEnable applies the value again.
        if(!KeyViewerObject) return;
        ResetRain();
        ResetKeyViewer();
        ResetFootKeyViewer();
    }

    private void SetReplayKey(RunState recorded) {
        int size = GetKeyCount((KeyviewerStyle) recorded.KeyStyle);
        int footSize = GetKeyCount((KeyviewerStyle) recorded.FootKeyStyle);
        _replayKey = new KeyCode[size];
        _replayFootKey = new KeyCode[footSize];
        _replayGhostKey = new KeyCode[size];
        Array.Copy(recorded.KeyCode, 0, _replayKey, 0, size);
        Array.Copy(recorded.KeyCode, size, _replayFootKey, 0, footSize);
        Array.Copy(recorded.KeyCode, size + footSize, _replayGhostKey, 0, size);
        _replayKeyText = recorded.KeyNames;
    }

    // The replay counts from the run's own key counts when TUFReplay saved them, else from the player's.
    // The counts are replaced, not added to, so the same value handed over twice counts once.
    public void SetReplayCounts(RunState recorded) {
        KeyCountData counts = _replayCounts;
        if(counts == null) return;
        if(recorded?.KeyCounts == null) return;
        int length = Math.Min(recorded.KeyCounts.Length, counts.Count.Length);
        Array.Copy(recorded.KeyCounts, counts.Count, length);
        counts.TotalCount = recorded.TotalKeyCount;
    }

    // Called on Unity's main thread by TUFReplay's ReplayApi.
    public void ApplyReplayKey(KeyCode key, int? nativeKey, bool pressed, long ticks) {
        FrozenDictionary<int, int[]> binding = _keyBinding;
        if(_replayCounts == null || binding == null || !Enabled) return;
        // Match the live-input path, including native keys on Korean keyboards.
        if(nativeKey.HasValue) {
            ushort native = (ushort) nativeKey.Value;
            if(KeyCodeMapper.TryInit() && KeyCodeMapper.Instance.TryGetCustomKeyCode(native, out KeyCode mapped)) key = mapped;
            if(key == KeyCode.None) key = (KeyCode) (native + 0x1000);
        }
        if(binding.TryGetValue((int) key, out int[] indexes)) Work(indexes, pressed, (ulong) Math.Max(0, ticks));
    }

    public void OnKeyStyleChange(KeyStyleChange e) {
        if(e.IsFootKey) {
            _currentFootKeyViewerStyle = e.FootKeyViewerStyle;
            _replayFootKey = e.KeyCode.AsUnsafe<KeyCode[]>();
        } else {
            _currentKeyViewerStyle = e.KeyViewerStyle;
            int size = GetKeyCount(_currentKeyViewerStyle);
            _replayKey = new KeyCode[size];
            _replayGhostKey = new KeyCode[size];
            Array.Copy(e.KeyCode, 0, _replayKey, 0, size);
            Array.Copy(e.KeyCode, size, _replayGhostKey, 0, size);
        }
        RebuildKeyBinding();
    }

    public void OnKeyChange(KeyChange e) {
        switch(e.Index) {
            case < HandOutIndex:
                _replayKey[e.Index] = (KeyCode) e.KeyCode;
                break;
            case < FootOutIndex:
                _replayFootKey[e.Index - HandOutIndex] = (KeyCode) e.KeyCode;
                break;
            default:
                _replayGhostKey[e.Index - FootOutIndex] = (KeyCode) e.KeyCode;
                break;
        }
        RebuildKeyBinding();
        if(e.Index < HandOutIndex && _replayKeyText[e.Index] != null) return;
        Keys[e.Index].Text.SetTextForce(KeyToString((KeyCode) e.KeyCode));
    }

    public void OnKeyNameChange(KeyNameChange e) {
        _replayKeyText[e.Index] = e.Text;
        Keys[e.Index].Text.SetTextForce(e.Text ?? KeyToString(_replayKey[e.Index]));
    }

    public void OnKeyCountReset() {
        Total?.Value.TMP.text = "0";
        foreach(Key key in Keys) key?.Value.SetTextForce("0");
        Array.Clear(_replayCounts.Count, 0, FootOutIndex);
        _replayCounts.TotalCount = 0;
    }
}