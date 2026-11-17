using JipperResourcePack.KeyViewerContents;
using JipperResourcePack.OtherModApi.TUFReplay.DTO;

namespace JipperResourcePack.OtherModApi.TUFReplay;

public static partial class TUFReplayAPI {
    private sealed class ProviderV2 : Provider {
        public override bool CheckUsableAPIVersion(int version) => version >= 2;

        public void OnReplayBegin(bool render) {
            Recorded = null;
            ReplayActive = true;
            _replayTicks = 0;
            SetInputSuspended(true);
        }

        public void OnReplayState(RunState state, long ticks) {
            Recorded = state;
            KeyViewer.Instance.SetReplayCounts(state);
        }

        public void OnReplayEvent(KeyStyleChange e, long _) => KeyViewer.Instance.OnKeyStyleChange(e);
        public void OnReplayEvent(KeyChange e, long _) => KeyViewer.Instance.OnKeyChange(e);
        public void OnReplayEvent(KeyNameChange e, long _) => KeyViewer.Instance.OnKeyNameChange(e);
        public void OnReplayEvent(KeyCountReset e, long _) => KeyViewer.Instance.OnKeyCountReset();
    }
}
