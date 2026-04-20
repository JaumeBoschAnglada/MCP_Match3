using System.Collections.Generic;
using UnityEngine;
using Match3.Core;
using Match3.Data;

namespace Match3.Managers
{
    /// <summary>
    /// Singleton factory for all Panel types.
    /// Maps PanelType → Prefab and obtains instances via ObjectPool.
    /// </summary>
    public class PanelManager : MonoBehaviour
    {
        public static PanelManager Instance { get; private set; }

        [Header("Panel Prefabs (assign in Inspector)")]
        [SerializeField] private GameObject m_DefaultFullPrefab;
        [SerializeField] private GameObject m_DefaultEmptyPrefab;
        [SerializeField] private GameObject m_CreatorEmptyPrefab;
        [SerializeField] private GameObject m_FixedPrefab;
        [SerializeField] private GameObject m_IceCagePrefab;
        [SerializeField] private GameObject m_BottleCagePrefab;
        [SerializeField] private GameObject m_LollyCagePrefab;
        [SerializeField] private GameObject m_BreadPrefab;
        [SerializeField] private GameObject m_CrackerPrefab;
        [SerializeField] private GameObject m_WaferFloorPrefab;

        private Dictionary<PanelType, GameObject> m_PrefabMap;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildMap();
        }

        private void BuildMap()
        {
            m_PrefabMap = new Dictionary<PanelType, GameObject>
            {
                { PanelType.Default_Full,  m_DefaultFullPrefab  },
                { PanelType.Default_Empty, m_DefaultEmptyPrefab },
                { PanelType.Creator_Empty, m_CreatorEmptyPrefab },
                { PanelType.Fixed_Block,   m_FixedPrefab        },
                { PanelType.Ice_Cage,      m_IceCagePrefab      },
                { PanelType.Bottle_Cage,   m_BottleCagePrefab   },
                { PanelType.Lolly_Cage,    m_LollyCagePrefab    },
                { PanelType.Bread_Block,   m_BreadPrefab        },
                { PanelType.Cracker,       m_CrackerPrefab      },
                { PanelType.Wafer_floor,   m_WaferFloorPrefab   },
            };
        }

        /// <summary>
        /// Create a panel of the given type on the specified board cell.
        /// Uses ObjectPool if available; falls back to Instantiate.
        /// </summary>
        public Panels.Panel CreatePanel(PanelType type, Board board)
        {
            if (!m_PrefabMap.TryGetValue(type, out var prefab) || prefab == null)
            {
                Debug.LogWarning($"[PanelManager] No prefab registered for PanelType.{type}");
                return null;
            }

            GameObject go;
            var pool = ObjectPool.Instance;
            if (pool != null)
                go = pool.GetObject(prefab, board.transform);
            else
            {
                go = Instantiate(prefab, board.transform);
                go.transform.localPosition = Vector3.zero;
            }

            go.SetActive(true);

            var panel = go.GetComponent<Panels.Panel>();
            if (panel != null)
            {
                panel.m_Board = board;
                panel.ApplyVisualSorting();
                board.m_ListPanel.Add(panel);
            }

            return panel;
        }

        /// <summary>Return a panel instance to the pool.</summary>
        public void RestorePanel(GameObject panelGO)
        {
            var pool = ObjectPool.Instance;
            if (pool != null)
                pool.Restore(panelGO);
            else
                Destroy(panelGO);
        }
    }
}
