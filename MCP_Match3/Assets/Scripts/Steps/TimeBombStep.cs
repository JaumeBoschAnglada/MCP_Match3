using UnityEngine;
using Match3.Core;
using Match3.Data;

namespace Match3.Steps
{
    /// <summary>
    /// TimeBomb step: Decrements all TimeBomb counters on the board.
    /// If any reach zero, triggers game fail. Otherwise advances to next step.
    /// Stub for now — no TimeBomb items exist yet (Phase 8).
    /// </summary>
    public class TimeBombStep : BaseStep
    {
        public TimeBombStep(MatchManager matchManager) : base(matchManager) { }

        public override void Step_Play()
        {
            // TODO Phase 8: Iterate all boards, decrement TimeBomb items
            // For now, skip directly to next step
            MatchMgr.SetStep(StepType.IceCream);
        }
    }
}
