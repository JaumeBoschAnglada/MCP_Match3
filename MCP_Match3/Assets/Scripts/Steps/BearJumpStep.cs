using Match3.Core;
using Match3.Data;

namespace Match3.Steps
{
    /// <summary>Stub: Bear jump. Advances immediately. (Phase 8)</summary>
    public class BearJumpStep : BaseStep
    {
        public BearJumpStep(MatchManager matchManager) : base(matchManager) { }
        public override void Step_Play() { MatchMgr.SetStep(StepType.BearSpawn); }
    }
}
