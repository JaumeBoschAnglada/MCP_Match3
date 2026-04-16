using UnityEngine;
using Match3.Managers;

namespace Match3.UI
{
    public class TopUIController : MonoBehaviour
    {
        public static TopUIController Instance { get; private set; }

        [SerializeField] private MovesDisplay movesDisplay;
        [SerializeField] private ScoreDisplay scoreDisplay;
        [SerializeField] private MissionDisplay[] missionDisplays;
        private MissionManager missionMgr;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable() { missionMgr = MissionManager.Instance; if (missionMgr) Init(); }
        private void Init()
        {
            int i = 0;
            if (missionMgr.Missions != null)
                foreach (var m in missionMgr.Missions)
                    if (i < missionDisplays.Length) missionDisplays[i++].SetMission(m);
            while (i < missionDisplays.Length) missionDisplays[i++].SetEmpty();
        }
        public void Refresh()
        {
            if (!missionMgr) missionMgr = MissionManager.Instance;
            if (!missionMgr) return;
            movesDisplay?.UpdateMoves(missionMgr.MovesRemaining);
            scoreDisplay?.UpdateScore(missionMgr.CurrentScore);
            int i = 0;
            foreach (var m in missionMgr.Missions)
            {
                if (i >= missionDisplays.Length) break;
                int curr = missionMgr.MissionProgress.ContainsKey(m.kind) ? missionMgr.MissionProgress[m.kind] : 0;
                missionDisplays[i++].UpdateCount(curr);
            }
        }
    }
}
