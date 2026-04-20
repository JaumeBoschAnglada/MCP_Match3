using System.Collections.Generic;
using UnityEngine;
using Match3.Core;
using Match3.Managers;
using TMPro;

namespace Match3.UI
{
    public class TopUIController : MonoBehaviour
    {
        public static TopUIController Instance { get; private set; }

        [SerializeField] private MovesDisplay movesDisplay;
        [SerializeField] private ScoreDisplay scoreDisplay;
        [SerializeField] private TextMeshProUGUI levelFileText;

        [Header("Missions (dynamic)")]
        [SerializeField] private Transform missionsContainer;
        [SerializeField] private GameObject missionDisplayPrefab;

        private readonly List<MissionDisplay> m_ActiveDisplays = new List<MissionDisplay>();
        private MissionManager missionMgr;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            missionMgr = MissionManager.Instance;
            if (missionMgr != null && missionMgr.Missions != null)
                Init();
        }

        public void Init()
        {
            if (missionMgr == null) missionMgr = MissionManager.Instance;
            if (missionMgr == null) return;

            BuildDisplays();
            Refresh();
            SetLevelFileName(MatchManager.Instance?.LoadedLevelFileName);
        }

        public void SetLevelFileName(string levelFileName)
        {
            if (levelFileText == null) return;
            if (string.IsNullOrEmpty(levelFileName))
                levelFileText.text = "Level: unknown";
            else
                levelFileText.text = "Level: " + levelFileName;
        }

        /// <summary>
        /// Destroys existing MissionDisplay instances and creates exactly one per mission.
        /// </summary>
        private void BuildDisplays()
        {
            // Destroy previous displays
            foreach (var d in m_ActiveDisplays)
                if (d != null) Destroy(d.gameObject);
            m_ActiveDisplays.Clear();

            if (missionMgr.Missions == null || missionDisplayPrefab == null) return;

            foreach (var mission in missionMgr.Missions)
            {
                var go = Instantiate(missionDisplayPrefab, missionsContainer);
                var display = go.GetComponent<MissionDisplay>();
                if (display != null)
                {
                    display.SetMission(mission);
                    m_ActiveDisplays.Add(display);
                }
            }
        }

        public void Refresh()
        {
            if (!missionMgr) missionMgr = MissionManager.Instance;
            if (!missionMgr) return;

            movesDisplay?.UpdateMoves(missionMgr.MovesRemaining);
            scoreDisplay?.UpdateScore(missionMgr.CurrentScore);
            SetLevelFileName(MatchManager.Instance?.LoadedLevelFileName);

            for (int i = 0; i < m_ActiveDisplays.Count; i++)
            {
                var mission = missionMgr.Missions[i];
                int curr = missionMgr.MissionProgress.ContainsKey(mission.kind)
                    ? missionMgr.MissionProgress[mission.kind] : 0;
                m_ActiveDisplays[i].UpdateCount(curr);
            }
        }
    }
}
