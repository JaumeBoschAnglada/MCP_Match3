using UnityEngine;
using Match3.Core;
using Match3.Data;

namespace Match3.Steps
{
    /// <summary>
    /// Shuffling step: No valid moves exist. Shuffle the board and return to Wait.
    /// </summary>
    public class ShufflingStep : BaseStep
    {
        public ShufflingStep(MatchManager matchManager) : base(matchManager) { }

        public override void Step_Play()
        {
            Debug.Log("[ShufflingStep] Shuffling board...");
            MatchMgr.SetStep(StepType.Wait);
        }
    }
}
