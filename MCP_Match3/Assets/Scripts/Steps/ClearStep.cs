using UnityEngine;
using Match3.Core;
using Match3.Data;
using Match3.Managers;
using Match3.UI;

namespace Match3.Steps
{
    public class ClearStep : BaseStep
    {
        public override bool IsItemStep => false;

        public ClearStep(MatchManager matchManager) : base(matchManager) { }

        public override void Step_Play()
        {
            Debug.Log("[ClearStep] Mission completed!");

            var missionMgr = MissionManager.Instance;
            int finalScore = missionMgr ? missionMgr.CurrentScore : 0;
            int stars      = CalculateStars(finalScore, missionMgr);

            if (PopupManager.Instance)
            {
                PopupManager.Instance.Show<VictoryPopup>(
                    p => p.SetContent(finalScore, stars),
                    onClosed: () => LoadNextLevel());
            }
            else
            {
                LoadNextLevel();
            }
        }

        private int CalculateStars(int score, MissionManager mgr)
        {
            if (mgr == null) return 1;
            if (score >= mgr.ScoreStar3) return 3;
            if (score >= mgr.ScoreStar2) return 2;
            if (score >= mgr.ScoreStar1) return 1;
            return 0;
        }

        private void LoadNextLevel()
        {
            Debug.Log("[ClearStep] Loading next level...");
        }
    }
}
