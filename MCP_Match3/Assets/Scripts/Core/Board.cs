using System.Collections.Generic;
using UnityEngine;
using Match3.Data;

namespace Match3.Core
{
    public class Board : MonoBehaviour
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Index => X + Y * 9;

        // === Navigation (set by MatchManager after all boards exist) ===
        public Board Top { get; set; }
        public Board Bottom { get; set; }
        public Board Left { get; set; }
        public Board Right { get; set; }
        public Board TopLeft { get; set; }
        public Board TopRight { get; set; }
        public Board BottomLeft { get; set; }
        public Board BottomRight { get; set; }

        // === Gravity ===
        public DROP_DIR[] PossibleDrop_Dirs { get; set; }
        public int SeleteDropIndex { get; set; }
        public bool isListDrop { get; set; }
        public DROP_DIR CurrentDropDir => PossibleDrop_Dirs != null && PossibleDrop_Dirs.Length > SeleteDropIndex
            ? PossibleDrop_Dirs[SeleteDropIndex] : DROP_DIR.U;

        // === Cell state ===
        // Item reference (will be typed as Item in Phase 2, using Component for now)
        [HideInInspector] public Component m_Item;
        public List<Component> m_ListPanel = new List<Component>();

        public bool m_DropAnim;
        public bool m_ItemBrusting;
        public bool m_PanelBrusting;
        public bool m_MatchingCheck;
        public bool m_isMatchBrust;
        public ItemType m_NextItemType = ItemType.None;

        // Panel flags
        public bool IsPanelFixed { get; set; }
        public bool IsPanelCage { get; set; }
        public bool m_IsJamBoard { get; set; }
        public bool m_IsWarpOutBoard { get; set; }
        public bool m_IsWarpInBoard { get; set; }
        public Board m_WarpBoard { get; set; }

        // Visual
        [SerializeField] private SpriteRenderer m_Displayer;

        /// <summary>
        /// Indexer by SQR_DIR for neighbor access.
        /// </summary>
        public Board this[SQR_DIR dir]
        {
            get
            {
                switch (dir)
                {
                    case SQR_DIR.TOP: return Top;
                    case SQR_DIR.BOTTOM: return Bottom;
                    case SQR_DIR.LEFT: return Left;
                    case SQR_DIR.RIGHT: return Right;
                    case SQR_DIR.TOPLEFT: return TopLeft;
                    case SQR_DIR.TOPRIGHT: return TopRight;
                    case SQR_DIR.BOTTOMLEFT: return BottomLeft;
                    case SQR_DIR.BOTTOMRIGHT: return BottomRight;
                    default: return null;
                }
            }
        }

        /// <summary>
        /// Indexer by DROP_DIR for gravity-based neighbor access.
        /// Returns the board in the direction pieces come FROM.
        /// </summary>
        public Board this[DROP_DIR dir]
        {
            get
            {
                switch (dir)
                {
                    case DROP_DIR.U: return Top;
                    case DROP_DIR.D: return Bottom;
                    case DROP_DIR.L: return Left;
                    case DROP_DIR.R: return Right;
                    default: return null;
                }
            }
        }

        /// <summary>
        /// The board from which pieces drop into this cell (based on current gravity direction).
        /// </summary>
        public Board DropBoard => this[CurrentDropDir];

        /// <summary>
        /// Diagonal-left relative to the drop direction (for side-drop).
        /// </summary>
        public Board DropLeft
        {
            get
            {
                switch (CurrentDropDir)
                {
                    case DROP_DIR.U: return TopLeft;
                    case DROP_DIR.D: return BottomRight;
                    case DROP_DIR.L: return BottomLeft;
                    case DROP_DIR.R: return TopRight;
                    default: return null;
                }
            }
        }

        /// <summary>
        /// Diagonal-right relative to the drop direction (for side-drop).
        /// </summary>
        public Board DropRight
        {
            get
            {
                switch (CurrentDropDir)
                {
                    case DROP_DIR.U: return TopRight;
                    case DROP_DIR.D: return BottomLeft;
                    case DROP_DIR.L: return TopLeft;
                    case DROP_DIR.R: return BottomRight;
                    default: return null;
                }
            }
        }

        /// <summary>
        /// Whether this cell is active and can hold items (has Default_Full panel).
        /// </summary>
        public bool IsActiveCell { get; set; } = true;

        /// <summary>
        /// Whether the cell currently has a valid matchable item (not animating, not empty).
        /// </summary>
        public bool IsNowItemMatch =>
            IsActiveCell && m_Item != null && !m_DropAnim && !m_ItemBrusting;

        /// <summary>
        /// Initialize the board cell with coordinates and gravity from stage data.
        /// </summary>
        public void Init(int x, int y, Stage stage)
        {
            X = x;
            Y = y;
            gameObject.name = $"Board_{x}_{y}";

            // Reset flags
            m_DropAnim = false;
            m_ItemBrusting = false;
            m_PanelBrusting = false;
            m_MatchingCheck = false;
            m_isMatchBrust = false;
            m_NextItemType = ItemType.None;
            IsPanelFixed = false;
            IsPanelCage = false;
            m_IsJamBoard = false;
            m_IsWarpOutBoard = false;
            m_IsWarpInBoard = false;
            m_WarpBoard = null;
            m_Item = null;
            m_ListPanel.Clear();
            isListDrop = false;
            SeleteDropIndex = 0;

            // Load gravity directions from stage
            if (stage != null && stage.DropDirs != null && Index < stage.DropDirs.Count)
            {
                var dirs = stage.DropDirs[Index];
                var validDirs = new List<DROP_DIR>();
                for (int i = 0; i < dirs.Length; i++)
                {
                    if (dirs[i] == DROP_DIR.List)
                        isListDrop = true;
                    else
                        validDirs.Add(dirs[i]);
                }
                PossibleDrop_Dirs = validDirs.Count > 0
                    ? validDirs.ToArray()
                    : new[] { DROP_DIR.U };
            }
            else
            {
                PossibleDrop_Dirs = new[] { DROP_DIR.U };
            }

            // Position in local space (X right, Y down)
            transform.localPosition = new Vector3(x, -y, 0f);
        }

        /// <summary>
        /// If this cell has multiple possible drop directions, pick one randomly.
        /// </summary>
        public void ChangeDropDir()
        {
            if (PossibleDrop_Dirs == null || PossibleDrop_Dirs.Length <= 1) return;
            SeleteDropIndex = Random.Range(0, PossibleDrop_Dirs.Length);
        }

        /// <summary>
        /// Set the visual displayer sprite alpha to show/hide the cell.
        /// </summary>
        public void SetDisplayer(bool active)
        {
            if (m_Displayer != null)
            {
                Color c = m_Displayer.color;
                c.a = active ? 0.15f : 0f;
                m_Displayer.color = c;
            }
        }

        /// <summary>
        /// Generate and place an item in this cell.
        /// </summary>
        public void GenItem(ItemType itemType, ColorType colorType)
        {
            if (!IsActiveCell)
            {
                Debug.LogWarning($"[Board] Cannot generate item on inactive cell at ({X}, {Y})");
                return;
            }

            var itemMgr = Match3.Items.ItemManager.Instance;
            if (itemMgr == null)
            {
                Debug.LogError("[Board] ItemManager instance not found!");
                return;
            }

            // Remove existing item if present
            if (m_Item != null)
            {
                var existingItem = m_Item.GetComponent<Match3.Items.Item>();
                if (existingItem != null)
                    ObjectPool.Instance?.Restore(existingItem.gameObject);
                m_Item = null;
            }

            // Create new item
            var item = itemMgr.CreateItem(itemType, colorType);
            if (item == null)
            {
                Debug.LogError($"[Board] Failed to create item {itemType} with color {colorType}");
                return;
            }

            // Setup item
            m_Item = item;
            item.m_Board = this;
            item.transform.SetParent(transform, false);
            item.transform.localPosition = Vector3.zero;
            item.gameObject.SetActive(true);
        }

        /// <summary>
        /// Spawn a new random item at the top of a drop column.
        /// Used when filling the board from above.
        /// </summary>
        public void TopSpawnItem()
        {
            if (!IsActiveCell) return;

            var itemMgr = Match3.Items.ItemManager.Instance;
            if (itemMgr == null) return;

            // For now, always spawn normal items with random color
            // TODO: Add special item logic based on spawn intervals in Phase 3+
            GenItem(ItemType.Normal, ColorType.None);

            // Set random color from level's available colors
            var item = m_Item as Match3.Items.Item;
            if (item != null)
                item.SetColorRandom();
        }
    }
}
