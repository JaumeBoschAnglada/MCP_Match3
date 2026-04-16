using UnityEngine;
using Match3.Core;
using Match3.Data;
using Match3.Managers;
using Match3.UI;

namespace Match3.Steps
{
    public class MissionStep : BaseStep
    {
        private MissionManager missionMgr;

        public override bool IsItemStep => false;

        public MissionStep(MatchManager matchManager) : base(matchManager)
        {
            missionMgr = MissionManager.Instance;
        }

        public override void Step_Play()
        {
            // Lazy-init: MissionManager may not exist yet (e.g. Phase 6 partial setup)
            if (!missionMgr)
                missionMgr = MissionManager.Instance;

            // Without MissionManager just pass through to Wait — no missions to check
            if (!missionMgr)
            {
                MatchMgr.SetStep(StepType.Wait);
                return;
            }

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
                MatchMgr.SetStep(StepType.Matching);
                return;
            }

            if (missionMgr.CheckMissionClear())
            {
                MatchMgr.SetStep(StepType.Clear);
                return;
            }

            // Descontar el movimiento primero, luego comprobar derrota
            missionMgr.MoveLimitApply();
            TopUIController.Instance?.Refresh();

            if (missionMgr.CheckMissionFail())
            {
                MatchMgr.SetStep(StepType.Fail);
                return;
            }

            MatchMgr.SetStep(StepType.Wait);
        }
    }
}
