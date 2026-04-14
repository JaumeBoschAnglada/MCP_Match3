using System.Collections.Generic;
using UnityEngine;
using Match3.Data;
using Match3.Core;

namespace Match3.Items
{
    /// <summary>
    /// Item factory singleton - manages item creation, pooling, and spawn rules.
    /// </summary>
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager Instance { get; private set; }

        // === Item prefabs ===
        [Header("Item Prefabs")]
        [SerializeField] private GameObject m_NormalItemPrefab;

        // TODO: Add special item prefabs in later phases
        // [SerializeField] private GameObject m_LineXPrefab;
        // [SerializeField] private GameObject m_LineYPrefab;
        // [SerializeField] private GameObject m_BombPrefab;
        // [SerializeField] private GameObject m_RainbowPrefab;

        // === Object pool reference ===
        private ObjectPool m_ObjectPool;

        // === Special item spawn intervals (for future implementation) ===
        [Header("Special Item Spawn Rules")]
        [SerializeField] private int m_LineItemInterval = 10;
        [SerializeField] private int m_BombItemInterval = 15;
        [SerializeField] private int m_RainbowItemInterval = 20;

        private int m_LineItemCounter = 0;
        private int m_BombItemCounter = 0;
        private int m_RainbowItemCounter = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Initialize ObjectPool reference early
            m_ObjectPool = ObjectPool.Instance;
        }

        private void Start()
        {
            // Pre-initialize item pool if ObjectPool is ready
            if (m_ObjectPool != null)
                Init_ObjectPool_Item();
        }

        /// <summary>
        /// Pre-instantiate items into the object pool.
        /// </summary>
        private void Init_ObjectPool_Item()
        {
            if (m_ObjectPool == null || m_NormalItemPrefab == null) return;

            // Pre-create normal items (6 colors × 10 instances = 60 items)
            m_ObjectPool.CreatePool(m_NormalItemPrefab, 60);

            Debug.Log("[ItemManager] Object pool initialized with items.");
        }

        /// <summary>
        /// Create an item of the specified type and color.
        /// Returns null if the item type is not yet implemented.
        /// </summary>
        public Item CreateItem(ItemType itemType, ColorType color)
        {
            // Lazy initialization: ensure ObjectPool is ready
            if (m_ObjectPool == null)
            {
                m_ObjectPool = ObjectPool.Instance;
                if (m_ObjectPool == null)
                {
                    Debug.LogError("[ItemManager] ObjectPool.Instance is null! Cannot create items.");
                    return null;
                }
                Init_ObjectPool_Item();
            }

            GameObject prefab = GetPrefabForItemType(itemType);
            if (prefab == null)
            {
                Debug.LogWarning($"[ItemManager] No prefab found for ItemType {itemType}");
                return null;
            }

            GameObject itemObj = m_ObjectPool.GetObject(prefab, null);
            if (itemObj == null)
            {
                Debug.LogError($"[ItemManager] Failed to get object from pool for {itemType}");
                return null;
            }

            Item item = itemObj.GetComponent<Item>();
            if (item == null)
            {
                Debug.LogError($"[ItemManager] Prefab for {itemType} does not have an Item component!");
                return null;
            }

            item.Init(itemType, color);
            return item;
        }

        /// <summary>
        /// Get the prefab for a given item type.
        /// </summary>
        private GameObject GetPrefabForItemType(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Normal:
                    return m_NormalItemPrefab;

                // TODO: Add special items in future phases
                // case ItemType.Line_X:
                //     return m_LineXPrefab;
                // case ItemType.Line_Y:
                //     return m_LineYPrefab;
                // case ItemType.Line_C:
                // case ItemType.Bomb:
                //     return m_BombPrefab;
                // case ItemType.Rainbow:
                //     return m_RainbowPrefab;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Check if a special Line item can be created (based on spawn interval).
        /// </summary>
        public bool IsCreateLineItem()
        {
            m_LineItemCounter++;
            if (m_LineItemCounter >= m_LineItemInterval)
            {
                m_LineItemCounter = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if a special Bomb item can be created (based on spawn interval).
        /// </summary>
        public bool IsCreateBombItem()
        {
            m_BombItemCounter++;
            if (m_BombItemCounter >= m_BombItemInterval)
            {
                m_BombItemCounter = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if a special Rainbow item can be created (based on spawn interval).
        /// </summary>
        public bool IsCreateRainbowItem()
        {
            m_RainbowItemCounter++;
            if (m_RainbowItemCounter >= m_RainbowItemInterval)
            {
                m_RainbowItemCounter = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reset spawn counters (call when starting a new level).
        /// </summary>
        public void ResetSpawnCounters()
        {
            m_LineItemCounter = 0;
            m_BombItemCounter = 0;
            m_RainbowItemCounter = 0;
        }
    }
}
