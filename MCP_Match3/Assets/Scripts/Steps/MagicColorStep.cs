using Match3.Core;
using Match3.Data;

namespace Match3.Steps
{
    /// <summary>Stub: Magic color. Advances immediately. (Phase 8)</summary>
    public class MagicColorStep : BaseStep
    {
        public MagicColorStep(MatchManager matchManager) : base(matchManager) { }
        public override void Step_Play() { MatchMgr.SetStep(StepType.BearJump); }
    }
}
