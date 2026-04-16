using Match3.Core;
using Match3.Data;

namespace Match3.Steps
{
    /// <summary>Stub: Chameleon color change. Advances immediately. (Phase 8)</summary>
    public class ChameleonStep : BaseStep
    {
        public ChameleonStep(MatchManager matchManager) : base(matchManager) { }
        public override void Step_Play() { MatchMgr.SetStep(StepType.MagicColor); }
    }
}
