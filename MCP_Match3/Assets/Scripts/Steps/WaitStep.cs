using UnityEngine;
using Match3.Core;
using Match3.Data;

namespace Match3.Steps
{
    /// <summary>
    /// Wait step: Idle state where the player can interact.
    /// Runs hint timer and shuffling checks.
    /// </summary>
    public class WaitStep : BaseStep
    {
        private float m_HintTimer = 0f;
        private const float HINT_DELAY = 5f;

        public WaitStep(MatchManager matchManager) : base(matchManager) { }

        public override void Step_Play()
        {
            m_HintTimer = 0f;
            // TODO Phase 11: ShufflingCheck()
            // TODO Phase 11: FocusShow()
        }

        public override void Step_Process()
        {
            m_HintTimer += Time.deltaTime;

            // TODO Phase 11: Hint offer after inactivity
            // if (m_HintTimer >= HINT_DELAY) { MatchMgr.HintOffer(); m_HintTimer = 0f; }
        }
    }
}
