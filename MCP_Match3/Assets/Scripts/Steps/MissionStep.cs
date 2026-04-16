using UnityEngine;
using Match3.Core;
using Match3.Data;

namespace Match3.Steps
{
    /// <summary>
    /// Mission step: Checks if there are pending matches (re-enter Matching),
    /// or if mission is clear/failed, or returns to Wait for the next turn.
    /// </summary>
    public class MissionStep : BaseStep
    {
        public override bool IsItemStep => false;

        public MissionStep(MatchManager matchManager) : base(matchManager) { }

        public override void Step_Play()
        {
            bool hasPending = false;
            for (int i = 0; i < 81; i++)
            {
                if (MatchMgr.m_ListBoard[i].m_MatchingCheck)
                {
                    hasPending = true;
                    break;
                }
            }

            if (hasPending)
            {
                Debug.Log("[MissionStep] Pending matches found, returning to Matching");
                MatchMgr.SetStep(StepType.Matching);
                return;
            }

            // TODO Phase 6: CheckMissionClear() → SetStep(Clear)
            // TODO Phase 6: CheckMissionFail() → SetStep(Fail)

            MatchMgr.ComboCnt = 0;
            MatchMgr.SetStep(StepType.Wait);
        }
    }
}
