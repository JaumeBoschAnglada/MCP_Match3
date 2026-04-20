using System.Collections.Generic;
using UnityEngine;
using Match3.Data;
using Match3.Items;
using Match3.Managers;
using Match3.Panels;

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
        public List<Panel> m_ListPanel = new List<Panel>();

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

        public bool HasIceCage
        {
            get
            {
                for (int i = 0; i < m_ListPanel.Count; i++)
                {
                    var panel = m_ListPanel[i];
                    if (panel != null && panel.m_PanelType == PanelType.Ice_Cage)
                        return true;
                }

                return false;
            }
        }

        public bool BlocksItemSwitch
        {
            get
            {
                if (m_Item == null) return false;

                for (int i = 0; i < m_ListPanel.Count; i++)
                {
                    var panel = m_ListPanel[i];
                    if (panel != null && panel.BlocksItemSwitch)
                        return true;
                }

                return false;
            }
        }

        public bool BlocksGravityFlow
        {
            get
            {
                for (int i = 0; i < m_ListPanel.Count; i++)
                {
                    var panel = m_ListPanel[i];
                    if (panel != null && panel.BlocksGravity)
                        return true;
                }

                return false;
            }
        }

        public void NotifyItemSwitchAttempt()
        {
            for (int i = 0; i < m_ListPanel.Count; i++)
            {
                var panel = m_ListPanel[i];
                if (panel != null)
                    panel.ItemSwitch();
            }
        }

        // Visual
        [SerializeField] private SpriteRenderer m_Displayer;
        [SerializeField] private Sprite m_LightDisplayerSprite;
        [SerializeField] private Sprite m_DarkDisplayerSprite;


        private const float SpawnOffsetDistance = 1.15f;

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
        /// NOTE: With localPosition.y = -y, visual "top" (y=0) has higher world.y than "bottom" (y=8).
        /// DROP_DIR.U means gravity pulls UP (toward higher world.y), so pieces come FROM bottom (y+1).
        /// </summary>
        public Board this[DROP_DIR dir]
        {
            get
            {
                switch (dir)
                {
                    case DROP_DIR.U: return Bottom;  // Pieces drop UP from bottom (y+1)
                    case DROP_DIR.D: return Top;     // Pieces drop DOWN from top (y-1)
                    case DROP_DIR.L: return Right;   // Pieces drop LEFT from right (x+1)
                    case DROP_DIR.R: return Left;    // Pieces drop RIGHT from left (x-1)
                    default: return null;
                }
            }
        }

        /// <summary>
        /// The board from which pieces drop into this cell (based on current gravity direction).
        /// For DROP_DIR.U, this is Bottom (y+1) because pieces come FROM below visually.
        /// </summary>
        public Board DropBoard => this[CurrentDropDir];

        /// <summary>
        /// The board in the gravity pull direction (where gravity originates).
        /// For DROP_DIR.U, gravity pulls upward, so pieces originate from Bottom (y+1, visually below).
        /// Use this to find the starting point of a gravity column.
        /// </summary>
        public Board GravitySource
        {
            get
            {
                switch (CurrentDropDir)
                {
                    case DROP_DIR.U: return Bottom;   // Gravity originates from below (y+1)
                    case DROP_DIR.D: return Top;      // Gravity originates from above (y-1)
                    case DROP_DIR.L: return Right;    // Gravity originates from right (x+1)
                    case DROP_DIR.R: return Left;     // Gravity originates from left (x-1)
                    default: return null;
                }
            }
        }

        /// <summary>
        /// The board in the opposite direction of gravity (toward where items move).
        /// For DROP_DIR.U (gravity up), this returns Top (y-1) to traverse toward the destination.
        /// Use this to scan a column from bottom to top when processing gravity.
        /// </summary>
        public Board GravityDestination
        {
            get
            {
                switch (CurrentDropDir)
                {
                    case DROP_DIR.U: return Top;      // Items move upward to Top (y-1)
                    case DROP_DIR.D: return Bottom;   // Items move downward to Bottom (y+1)
                    case DROP_DIR.L: return Left;     // Items move leftward to Left (x-1)
                    case DROP_DIR.R: return Right;    // Items move rightward to Right (x+1)
                    default: return null;
                }
            }
        }

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
        /// Whether this cell can act as the PIVOT of a color match detection
        /// (i.e. initiate FindMatchesHorizontal/Vertical). Normal items only.
        /// </summary>
        public bool IsNowItemMatch
        {
            get
            {
                if (!IsActiveCell || m_Item == null || m_DropAnim || m_ItemBrusting) return false;
                var item = m_Item as Match3.Items.Item;
                return item == null || item.Match; // Match=false for specials: they cannot be pivot
            }
        }

        /// <summary>
        /// Whether this cell can be counted as a neighbor in a color chain.
        /// Special colored items (Butterfly, Bomb, Line...) count as part of a chain
        /// when adjacent to matching normals, even though they cannot start one.
        /// </summary>
        public bool IsNowItemChainable
        {
            get
            {
                if (!IsActiveCell || m_Item == null || m_DropAnim || m_ItemBrusting) return false;
                var item = m_Item as Match3.Items.Item;
                return item == null || item.m_Color != Match3.Data.ColorType.None;
            }
        }

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
                Sprite targetSprite = ((X + Y) & 1) == 0 ? m_LightDisplayerSprite : m_DarkDisplayerSprite;
                if (targetSprite != null && m_Displayer.sprite != targetSprite)
                {
                    m_Displayer.sprite = targetSprite;
                }

                Color c = m_Displayer.color;
                c.r = 1f;
                c.g = 1f;
                c.b = 1f;
                c.a = active ? 1f : 0f;
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

            // Setup item - Items are SIBLINGS of Boards, not children
            m_Item = item;
            item.m_Board = this;

            // Set parent to Field (same as Board's parent), not to Board itself
            item.transform.SetParent(transform.parent, false);

            // Position at Board's world position
            item.transform.position = transform.position;
            item.transform.rotation = Quaternion.identity;
            item.transform.localScale = Vector3.one;
            item.gameObject.SetActive(true);

            Debug.Log($"[Board {name}] Created item {itemType}/{colorType} at world ({transform.position.x:F1}, {transform.position.y:F1}), parent: {item.transform.parent.name}");
        }

        /// <summary>
        /// Spawn a new random item at the top of a drop column.
        /// Used when filling the board from above.
        /// </summary>
        public void TopSpawnItem()
        {
            TopSpawnItem(true);
        }

        public void TopSpawnItem(bool animateIntoCell)
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
            {
                item.SetColorRandom();

                item.transform.position = GetSpawnWorldPosition();

                if (animateIntoCell)
                {
                    m_DropAnim = true;
                    MatchManager.Instance?.StartCoroutine(Co_ItemDropAnimation(item));
                }
            }
        }

        private Vector3 GetSpawnWorldPosition()
        {
            return transform.position + GetSpawnOffsetDirection() * SpawnOffsetDistance;
        }

        private Vector3 GetSpawnOffsetDirection()
        {
            switch (CurrentDropDir)
            {
                case DROP_DIR.U: return Vector3.up;
                case DROP_DIR.D: return Vector3.down;
                case DROP_DIR.L: return Vector3.left;
                case DROP_DIR.R: return Vector3.right;
                default: return Vector3.up;
            }
        }

        // ========== PHASE 3: MATCH DETECTION ==========

        /// <summary>
        /// Find horizontal matches (left + right) recursively.
        /// Returns list of boards that match the current item's color.
        /// </summary>
        public List<Board> FindMatchesHorizontal()
        {
            if (!IsNowItemMatch) return new List<Board>();

            var item = m_Item as Match3.Items.Item;
            if (item == null) return new List<Board>();

            List<Board> matches = new List<Board> { this };
            ColorType targetColor = item.m_Color;

            // Search left
            FindMatches_Dir_Recursive(targetColor, ref matches, SQR_DIR.LEFT);

            // Search right
            FindMatches_Dir_Recursive(targetColor, ref matches, SQR_DIR.RIGHT);

            return matches.Count >= 3 ? matches : new List<Board>();
        }

        /// <summary>
        /// Find vertical matches (up + down) recursively.
        /// Returns list of boards that match the current item's color.
        /// </summary>
        public List<Board> FindMatchesVertical()
        {
            if (!IsNowItemMatch) return new List<Board>();

            var item = m_Item as Match3.Items.Item;
            if (item == null) return new List<Board>();

            List<Board> matches = new List<Board> { this };
            ColorType targetColor = item.m_Color;

            // Search up
            FindMatches_Dir_Recursive(targetColor, ref matches, SQR_DIR.TOP);

            // Search down
            FindMatches_Dir_Recursive(targetColor, ref matches, SQR_DIR.BOTTOM);

            return matches.Count >= 3 ? matches : new List<Board>();
        }

        /// <summary>
        /// Find 2x2 square matches in all 4 corner directions.
        /// Returns list of boards forming a square.
        /// </summary>
        public List<Board> FindMatchesSquare()
        {
            if (!IsNowItemMatch) return new List<Board>();

            var item = m_Item as Match3.Items.Item;
            if (item == null) return new List<Board>();

            ColorType targetColor = item.m_Color;

            // Check all 4 possible 2x2 squares with this cell as one corner
            List<Board>[] squareChecks = new List<Board>[4];

            // Top-Left square: this, right, bottom, bottom-right
            squareChecks[0] = CheckSquarePattern(targetColor, SQR_DIR.RIGHT, SQR_DIR.BOTTOM, SQR_DIR.BOTTOMRIGHT);

            // Top-Right square: this, left, bottom, bottom-left
            squareChecks[1] = CheckSquarePattern(targetColor, SQR_DIR.LEFT, SQR_DIR.BOTTOM, SQR_DIR.BOTTOMLEFT);

            // Bottom-Left square: this, right, top, top-right
            squareChecks[2] = CheckSquarePattern(targetColor, SQR_DIR.RIGHT, SQR_DIR.TOP, SQR_DIR.TOPRIGHT);

            // Bottom-Right square: this, left, top, top-left
            squareChecks[3] = CheckSquarePattern(targetColor, SQR_DIR.LEFT, SQR_DIR.TOP, SQR_DIR.TOPLEFT);

            // Return first valid square found
            foreach (var square in squareChecks)
            {
                if (square != null && square.Count == 4)
                    return square;
            }

            return new List<Board>();
        }

        /// <summary>
        /// Helper: Check if a 2x2 square pattern matches.
        /// </summary>
        private List<Board> CheckSquarePattern(ColorType color, SQR_DIR dir1, SQR_DIR dir2, SQR_DIR dirDiag)
        {
            Board b1 = this[dir1];
            Board b2 = this[dir2];
            Board b3 = this[dirDiag];

            if (b1 == null || b2 == null || b3 == null) return null;
            if (!b1.IsNowItemChainable || !b2.IsNowItemChainable || !b3.IsNowItemChainable) return null;

            var item1 = b1.m_Item as Match3.Items.Item;
            var item2 = b2.m_Item as Match3.Items.Item;
            var item3 = b3.m_Item as Match3.Items.Item;

            if (item1 == null || item2 == null || item3 == null) return null;
            if (item1.m_Color != color || item2.m_Color != color || item3.m_Color != color) return null;

            return new List<Board> { this, b1, b2, b3 };
        }

        /// <summary>
        /// Find matches in all 4 cardinal directions with recursion limit.
        /// Used for special item explosions and advanced match patterns.
        /// </summary>
        public List<Board> FindMatchesAround(int maxRecursion = 1)
        {
            if (!IsNowItemMatch) return new List<Board>();

            var item = m_Item as Match3.Items.Item;
            if (item == null) return new List<Board>();

            List<Board> matches = new List<Board> { this };
            ColorType targetColor = item.m_Color;

            // Search all 4 cardinal directions
            FindMatches_Dir_Recursive(targetColor, ref matches, SQR_DIR.TOP, maxRecursion);
            FindMatches_Dir_Recursive(targetColor, ref matches, SQR_DIR.BOTTOM, maxRecursion);
            FindMatches_Dir_Recursive(targetColor, ref matches, SQR_DIR.LEFT, maxRecursion);
            FindMatches_Dir_Recursive(targetColor, ref matches, SQR_DIR.RIGHT, maxRecursion);

            return matches;
        }

        /// <summary>
        /// Recursive match finder in a specific direction.
        /// </summary>
        private void FindMatches_Dir_Recursive(ColorType targetColor, ref List<Board> matchList, SQR_DIR dir, int depth = 99)
        {
            if (depth <= 0) return;

            Board neighbor = this[dir];
            if (neighbor == null) return;
            if (!neighbor.IsNowItemChainable) return; // chainable: normals + colored specials
            if (matchList.Contains(neighbor)) return;

            var neighborItem = neighbor.m_Item as Match3.Items.Item;
            if (neighborItem == null) return;
            if (neighborItem.m_Color != targetColor) return;

            matchList.Add(neighbor);
            neighbor.FindMatches_Dir_Recursive(targetColor, ref matchList, dir, depth - 1);
        }

        // ========== PHASE 3: BURST & EXPLOSION ==========

        /// <summary>
        /// Burst/explode the item in this cell.
        /// Coroutine version for animated destruction.
        /// </summary>
        public System.Collections.IEnumerator Co_Brust()
        {
            Debug.Log($"[Board {name}] Co_Brust starting, m_Item={m_Item?.name ?? "null"}");

            if (m_Item == null)
            {
                Debug.LogWarning($"[Board {name}] Co_Brust: m_Item is null, aborting");
                yield break;
            }

            m_ItemBrusting = true;
            m_isMatchBrust = false; // Clear flag immediately so we can't be re-collected in the next cascade

            var item = m_Item as Match3.Items.Item;
            ColorType burstColor = ColorType.None;

            if (item != null)
            {
                // Store item data before destroying
                burstColor = item.m_Color;
                ItemType burstType = item.m_ItemType;

                Debug.Log($"[Board {name}] Bursting item {burstType}/{burstColor}");

                // Call item's Brust method (visual effects will be added in Phase 10)
                bool burstComplete = false;
                item.Brust(() => burstComplete = true);

                // Wait for burst animation
                yield return new UnityEngine.WaitUntil(() => burstComplete);

                Debug.Log($"[Board {name}] Burst animation complete, restoring to pool");

                // Return item to pool
                ObjectPool.Instance?.Restore(item.gameObject);
                m_Item = null;

                // MissionApply: notifica la pieza destruida a MissionManager
                item.MissionApply();

                // ScoreApply: suma puntos por pieza destruida
                int score = item.GetDestroyScore(MatchManager.Instance != null ? MatchManager.Instance.ComboCnt : 0);
                Managers.MissionManager.Instance?.AddScore(score);
            }

            // Burst panels on this cell (jaulas, wafer floor, etc.)
            PanelBrust();

            // Normal matches also damage adjacent obstacle panels such as cages and crackers.
            AroundBrust();

            // Phase 5: Generate special item after burst if a special was earned
            if (m_NextItemType != ItemType.None)
            {
                ItemType specialType = m_NextItemType;
                ColorType specialColor = burstColor; // inherit the color of the destroyed item
                m_NextItemType = ItemType.None;

                GenItem(specialType, specialColor);
                Debug.Log($"[Board {name}] Spawned special item {specialType}/{specialColor}");
            }

            Debug.Log($"[Board {name}] Co_Brust complete, m_ItemBrusting=false");
            m_ItemBrusting = false;
        }

        /// <summary>
        /// Synchronous burst (for immediate destruction without animation).
        /// </summary>
        public void Brust()
        {
            if (m_Item == null) return;

            var item = m_Item as Match3.Items.Item;
            if (item != null)
            {
                item.Brust();
                ObjectPool.Instance?.Restore(item.gameObject);
                m_Item = null;
            }

            PanelBrust();
            AroundBrust();

            // Phase 5: Generate special item after burst if a special was earned
            if (m_NextItemType != ItemType.None)
            {
                ItemType specialType = m_NextItemType;
                ColorType specialColor = item != null ? item.m_Color : ColorType.None;
                m_NextItemType = ItemType.None;
                GenItem(specialType, specialColor);
            }
        }

        // ========== PHASE 7: PANEL BURST ==========

        /// <summary>
        /// Notify all panels on this cell that an item was matched/burst here.
        /// Panels with Defence > 0 lose one layer; at 0 they destroy themselves.
        /// </summary>
        public void PanelBrust()
        {
            // Iterate backwards — panels may remove themselves during iteration
            for (int i = m_ListPanel.Count - 1; i >= 0; i--)
            {
                var panel = m_ListPanel[i];
                if (panel != null)
                    panel.ItemMatch();
            }
        }

        /// <summary>
        /// Notify adjacent cells that a burst occurred nearby (for bread, cracker, etc.).
        /// Only hits panels that respond to adjacent bursts (Defence-based destructibles).
        /// </summary>
        public void AroundBrust()
        {
            Board[] neighbors = { Top, Bottom, Left, Right };
            foreach (var nb in neighbors)
            {
                if (nb == null || !nb.IsActiveCell) continue;
                for (int i = nb.m_ListPanel.Count - 1; i >= 0; i--)
                {
                    var panel = nb.m_ListPanel[i];
                    // Only hit obstacle panels (Bread, Cracker, IceCage, WaferFloor)
                    if (panel != null && IsDestructibleByAdjacent(panel))
                        panel.Brust();
                }
            }
        }

        private static bool IsDestructibleByAdjacent(Panels.Panel panel)
        {
            var t = panel.m_PanelType;
            return t == PanelType.Bread_Block
                || t == PanelType.Cracker
                || t == PanelType.Ice_Cage;
        }

        // ========== PHASE 3: GRAVITY & DROP ==========

        /// <summary>
        /// Try to fill this empty cell from a diagonal source relative to the current gravity direction.
        /// A side-drop is only allowed when the candidate cannot continue falling straight in its own lane.
        /// </summary>
        public bool SideDrop()
        {
            if (!IsActiveCell || m_Item != null || m_DropAnim || BlocksGravityFlow)
                return false;

            Board firstCandidate = DropLeft;
            Board secondCandidate = DropRight;

            // Simple alternating priority to avoid persistent left/right bias.
            if (((X + Y) & 1) == 1)
            {
                firstCandidate = DropRight;
                secondCandidate = DropLeft;
            }

            if (TrySideDropFrom(firstCandidate))
                return true;

            return TrySideDropFrom(secondCandidate);
        }

        private bool TrySideDropFrom(Board candidate)
        {
            if (!CanSideDropFrom(candidate))
                return false;

            Debug.Log($"[Board {name}] SideDrop: moving item from {candidate.name}");
            ItemDrop(candidate);
            return true;
        }

        private bool CanSideDropFrom(Board candidate)
        {
            if (candidate == null || !candidate.IsActiveCell || candidate.BlocksGravityFlow)
                return false;

            if (candidate.m_Item == null || candidate.m_DropAnim || candidate.m_ItemBrusting)
                return false;

            var item = candidate.m_Item as Match3.Items.Item;
            if (item != null && !item.Drop)
                return false;

            // If the candidate can continue falling straight in its own lane, do not steal it diagonally.
            Board straightDropBoard = candidate.DropBoard;
            if (straightDropBoard == null || !straightDropBoard.IsActiveCell || straightDropBoard.BlocksGravityFlow)
                return true;

            if (straightDropBoard.m_Item == null && !straightDropBoard.m_DropAnim)
                return false;

            return true;
        }

        /// <summary>
        /// Recursive gravity drop for standard upward gravity (DROP_DIR.U).
        /// Fills empty cells by pulling items from above.
        /// THIS board is the starting point (usually bottom of column).
        /// </summary>
        public bool GravityDropItemRow()
        {
            if (!IsActiveCell) return false;

            Debug.Log($"[Board {name}] GravityDropItemRow starting");

            // Find the first empty cell in THIS column (from bottom toward destination)
            Board emptyBoard = null;
            Board current = this;

            while (current != null && current.IsActiveCell)
            {
                if (current.BlocksGravityFlow)
                {
                    Debug.Log($"  -> Gravity blocked at {current.name}");
                    break;
                }

                if (current.m_Item == null && !current.m_DropAnim)
                {
                    emptyBoard = current;
                    Debug.Log($"  -> Found empty cell: {emptyBoard.name}");
                    break;
                }
                current = current.GravityDestination; // Move toward gravity destination (up for DROP_DIR.U)
            }

            if (emptyBoard == null)
            {
                Debug.Log($"  -> No empty cells found in column");
                return false;
            }

            // Find the first filled cell above the empty one (in direction of gravity destination)
            Board filledBoard = emptyBoard.GravityDestination;
            bool blockedByPanel = false;
            while (filledBoard != null && filledBoard.IsActiveCell)
            {
                if (filledBoard.BlocksGravityFlow)
                {
                    Debug.Log($"  -> Gravity source blocked by {filledBoard.name}");
                    blockedByPanel = true;
                    break;
                }

                if (filledBoard.m_Item != null && !filledBoard.m_DropAnim && !filledBoard.m_ItemBrusting)
                {
                    Debug.Log($"  -> Found filled cell {filledBoard.name} above empty {emptyBoard.name}, initiating drop");

                    // Move item from filled to empty
                    emptyBoard.ItemDrop(filledBoard);

                    // Recursively fill the cell that just became empty (restart from bottom)
                    this.GravityDropItemRow();
                    return true;
                }
                filledBoard = filledBoard.GravityDestination;
            }

            if (blockedByPanel)
            {
                if (emptyBoard.SideDrop())
                {
                    this.GravityDropItemRow();
                    return true;
                }

                Debug.Log($"  -> No spawn beyond blocking panel for empty cell {emptyBoard.name}");
                return false;
            }

            // No filled cell found in this column - spawn new piece from the current lane first.
            // Side-drops from adjacent lanes are only allowed when the current lane cannot spawn.
            Board topCell = emptyBoard;
            while (topCell.GravityDestination != null
                && topCell.GravityDestination.IsActiveCell
                && !topCell.GravityDestination.BlocksGravityFlow)
            {
                topCell = topCell.GravityDestination;
            }

            Debug.Log($"  -> No filled cell found above {emptyBoard.name}, spawning at top cell {topCell.name}");
            topCell.TopSpawnItem(topCell == emptyBoard);

            // After spawning, the new item needs to drop down to fill emptyBoard
            if (topCell != emptyBoard && topCell.m_Item != null)
            {
                Debug.Log($"  -> Dropping newly spawned item from {topCell.name} to {emptyBoard.name}");
                emptyBoard.ItemDrop(topCell);
            }

            // Recursively process remaining empty cells
            this.GravityDropItemRow();
            return true;
        }

        /// <summary>
        /// Animate an item dropping from this board to the destination board.
        /// </summary>
        public void ItemDrop(Board fromBoard)
        {
            if (fromBoard == null || fromBoard.m_Item == null) return;
            if (fromBoard.BlocksGravityFlow || BlocksGravityFlow)
            {
                Debug.Log($"[Board {name}] ItemDrop blocked. Source={fromBoard.name} sourceBlocked={fromBoard.BlocksGravityFlow} targetBlocked={BlocksGravityFlow}");
                return;
            }

            Debug.Log($"[Board {name}] ItemDrop: Moving item from {fromBoard.name} (world {fromBoard.transform.position.x:F1},{fromBoard.transform.position.y:F1}) to {name} (world {transform.position.x:F1},{transform.position.y:F1})");

            // Transfer item reference (logical ownership)
            m_Item = fromBoard.m_Item;
            fromBoard.m_Item = null;

            var item = m_Item as Match3.Items.Item;
            if (item != null)
            {
                m_DropAnim = true;
                item.m_Board = this;
                // NOTE: Item stays at same parent (Field), just changes position
                // No SetParent() call needed - items are siblings of boards

                // Start drop animation
                MatchManager.Instance?.StartCoroutine(Co_ItemDropAnimation(item));
            }
        }

        /// <summary>
        /// Coroutine: Animate item falling to this cell's position.
        /// Items are siblings of Boards, so we only animate world position.
        /// </summary>
        private System.Collections.IEnumerator Co_ItemDropAnimation(Match3.Items.Item item)
        {
            Vector3 startPos = item.transform.position;
            Vector3 targetPos = transform.position; // Target = Board's world position

            Debug.Log($"[Board {name}] Drop Animation: from world ({startPos.x:F1},{startPos.y:F1}) to ({targetPos.x:F1},{targetPos.y:F1})");

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += UnityEngine.Time.deltaTime;
                float t = elapsed / duration;

                // Ease-in gravity
                float easedT = 1f - (1f - t) * (1f - t);

                item.transform.position = Vector3.Lerp(startPos, targetPos, easedT);
                yield return null;
            }

            // Ensure final world position matches Board exactly
            item.transform.position = targetPos;

            Debug.Log($"[Board {name}] Drop Animation Complete: Final world ({item.transform.position.x:F1},{item.transform.position.y:F1}), parent: {item.transform.parent.name}");

            m_DropAnim = false;
        }

        // ========== PHASE 3: SPECIAL ITEM CREATION ==========

        /// <summary>
        /// Determine which special item to create based on match pattern.
        /// Called when a match is detected during CheckMatchCondition.
        /// </summary>
        public ItemType SpecialItemCondition(List<Board> matchList)
        {
            if (matchList == null || matchList.Count < 3) return ItemType.None;

            int matchCount = matchList.Count;

            // 5+ in line → Rainbow
            if (matchCount >= 5)
            {
                return ItemType.Rainbow;
            }

            // Check for L or T shapes (4 cells minimum)
            if (matchCount >= 4)
            {
                // Get horizontal and vertical matches from this board
                var horizontal = FindMatchesHorizontal();
                var vertical = FindMatchesVertical();

                // L or T shape: both directions have matches
                if (horizontal.Count >= 2 && vertical.Count >= 2)
                {
                    // TODO Phase 5: Distinguish between Line_C and Bomb
                    // For now, alternate or use Bomb
                    return ItemType.Bomb;
                }

                // 4 in horizontal line → Line_Y (clears horizontal row)
                if (horizontal.Count == 4 && vertical.Count < 2)
                {
                    return ItemType.Line_Y;
                }

                // 4 in vertical line → Line_X (clears vertical column)
                if (vertical.Count == 4 && horizontal.Count < 2)
                {
                    return ItemType.Line_X;
                }
            }

            // 2x2 square → Bomb
            var square = FindMatchesSquare();
            if (square.Count == 4)
            {
                return ItemType.Bomb;
            }

            // 3 in line → Normal (no special item)
            return ItemType.None;
        }
    }
}
