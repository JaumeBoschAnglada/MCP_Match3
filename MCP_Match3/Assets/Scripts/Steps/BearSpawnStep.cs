using Match3.Core;
using Match3.Data;

namespace Match3.Steps
{
    /// <summary>Stub: Bear spawn. Advances immediately. (Phase 8)</summary>
    public class BearSpawnStep : BaseStep
    {
        public BearSpawnStep(MatchManager matchManager) : base(matchManager) { }
        public override void Step_Play() { MatchMgr.SetStep(StepType.Mission); }
    }
}
