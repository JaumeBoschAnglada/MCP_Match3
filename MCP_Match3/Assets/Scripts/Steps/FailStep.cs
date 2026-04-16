using UnityEngine;
using Match3.Core;
using Match3.Data;

namespace Match3.Steps
{
    /// <summary>
    /// Fail step: Player ran out of moves or a TimeBomb expired.
    /// </summary>
    public class FailStep : BaseStep
    {
        public override bool IsItemStep => false;

        public FailStep(MatchManager matchManager) : base(matchManager) { }

        public override void Step_Play()
        {
            Debug.Log("[FailStep] Game Fail!");
            MatchMgr.SetMatchState(MatchState.GameFail);
        }
    }
}
