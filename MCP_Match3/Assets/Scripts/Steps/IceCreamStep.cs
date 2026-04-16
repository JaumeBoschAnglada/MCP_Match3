using Match3.Core;
using Match3.Data;

namespace Match3.Steps
{
    /// <summary>Stub: IceCream expansion. Advances immediately. (Phase 8)</summary>
    public class IceCreamStep : BaseStep
    {
        public IceCreamStep(MatchManager matchManager) : base(matchManager) { }
        public override void Step_Play() { MatchMgr.SetStep(StepType.ConveyerBelt); }
    }
}
