using UnityEngine;
using Match3.Data;
using Match3.UI;
using System.Collections.Generic;

namespace Match3.Managers
{
    /// <summary>
    /// MissionManager: Manages all mission data and progress tracking.
    /// Singleton that works with MissionStep to track objectives.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }

        [SerializeField] private Stage m_CurrentStage;
        
        private int m_MovesRemaining;
        private List<MissionData> m_Missions;
        private int m_CurrentScore;
        private Dictionary<MissionKind, int> m_MissionProgress; // kind → current count

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Initialize missions from stage data.
        /// Called by MatchManager at game start.
        /// </summary>
        public void MissionSetting(Stage stage)
        {
            m_CurrentStage = stage;
            m_MovesRemaining = stage.limit_Move;
            m_CurrentScore = 0;
            m_Missions = new List<MissionData>();
            m_MissionProgress = new Dictionary<MissionKind, int>();

            if (stage.missionInfo != null)
            {
                m_Missions.AddRange(stage.missionInfo);
                foreach (var mission in m_Missions)
                {
                    m_MissionProgress[mission.kind] = 0;
                }
            }

            Debug.Log($"[MissionManager] Missions initialized: {m_Missions.Count} objectives, {m_MovesRemaining} moves");

            // Inicializar TopUI ahora que los datos están listos
            TopUIController.Instance?.Init();
        }

        public void MissionApply(ItemType itemType, ColorType color)
        {
            if (m_Missions == null || m_Missions.Count == 0) return;

            foreach (var mission in m_Missions)
            {
                if (mission.type == MissionType.OrderN)
                {
                    // OrderN: collect N items of a specific color.
                    // MissionKind (Red=1..Orange=6) values match ColorType (RED=1..ORANGE=6).
                    if (itemType == ItemType.Normal && (int)mission.kind == (int)color)
                    {
                        if (!m_MissionProgress.ContainsKey(mission.kind))
                            m_MissionProgress[mission.kind] = 0;

                        m_MissionProgress[mission.kind]++;
                        Debug.Log($"[MissionManager] {mission.kind}: {m_MissionProgress[mission.kind]}/{mission.count}");

                        var topUI = TopUIController.Instance;
                        topUI?.Refresh();
                    }
                }
            }
        }

        /// <summary>
        /// Check if all missions are completed.
        /// Returns false if no missions are configured (prevents false victory on empty level).
        /// </summary>
        public bool CheckMissionClear()
        {
            if (m_Missions == null || m_Missions.Count == 0)
                return false;

            foreach (var mission in m_Missions)
            {
                int progress = m_MissionProgress.ContainsKey(mission.kind) ? m_MissionProgress[mission.kind] : 0;
                if (progress < mission.count)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check if mission has failed (no moves left).
        /// </summary>
        public bool CheckMissionFail()
        {
            return m_MovesRemaining <= 0;
        }

        /// <summary>
        /// Deduct one move from the limit.
        /// </summary>
        public void MoveLimitApply()
        {
            m_MovesRemaining--;
            Debug.Log($"[MissionManager] Move used. Remaining: {m_MovesRemaining}");
        }

        /// <summary>
        /// Add score to total.
        /// </summary>
        public void AddScore(long points)
        {
            m_CurrentScore += (int)points;
        }

        // ========== Getters for UI ==========

        public int MovesRemaining => m_MovesRemaining;
        public int CurrentScore   => m_CurrentScore;
        public List<MissionData>            Missions         => m_Missions;
        public Dictionary<MissionKind, int> MissionProgress  => m_MissionProgress;

        // Star thresholds (read from stage data)
        public int ScoreStar1 => m_CurrentStage != null ? (int)m_CurrentStage.scoreStar1 : 0;
        public int ScoreStar2 => m_CurrentStage != null ? (int)m_CurrentStage.scoreStar2 : 0;
        public int ScoreStar3 => m_CurrentStage != null ? (int)m_CurrentStage.scoreStar3 : 0;
    }
}
