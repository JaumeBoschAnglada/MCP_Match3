using UnityEngine;
using Match3.Core;
using Match3.Data;

namespace Match3.Steps
{
    /// <summary>
    /// Clear step: Player completed all missions. Triggers bonus time, then game clear.
    /// </summary>
    public class ClearStep : BaseStep
    {
        public override bool IsItemStep => false;

        public ClearStep(MatchManager matchManager) : base(matchManager) { }

        public override void Step_Play()
        {
            Debug.Log("[ClearStep] Game Clear!");
            MatchMgr.SetMatchState(MatchState.GameClear);
        }
    }
}
