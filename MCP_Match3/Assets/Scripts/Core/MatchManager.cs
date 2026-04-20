using System.Collections.Generic;
using UnityEngine;
using Match3.Data;
using Match3.Steps;
using Match3.UI;
using Newtonsoft.Json;

namespace Match3.Core
{
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }

        // === Board ===
        [SerializeField] private GameObject m_BoardPrefab;
        [SerializeField] private Transform m_Field;

        public Board[] m_ListBoard { get; private set; } = new Board[81];

        // === Stage data ===
        public Stage m_CSD { get; private set; }
        public string LoadedLevelFileName { get; private set; }
        public List<ColorType> m_AppearColor { get; private set; } = new List<ColorType>();

        // === State ===
        public MatchState m_MatchState { get; private set; } = MatchState.PrepareGame;
        public StepType m_StepType { get; private set; } = StepType.Wait;
        public TouchState m_TouchState { get; set; } = TouchState.Switching;

        // === Gravity lists ===
        public List<Board> m_ListDropStart { get; private set; } = new List<Board>();
        public List<Board> m_ListDropHead { get; private set; } = new List<Board>();

        // === Combo ===
        public int ComboCnt { get; set; }

        // === Special spawn positioning ===
        /// <summary>Boards involved in the last player-initiated swap; used to decide where a special item spawns.</summary>
        public Board m_LastSwapBoardA { get; private set; }
        public Board m_LastSwapBoardB { get; private set; }

        // === Step system (Phase 4) ===
        private Dictionary<StepType, BaseStep> m_DicStep;
        private BaseStep m_CurrentStep;

        // === Managers (assigned via SerializeField) ===
        [SerializeField] private ObjectPool m_ObjectPool;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            BoardCreate();

            // Debug overlay
            if (FindFirstObjectByType<DebugManager>() == null)
            {
                var go = new GameObject("[Debug]");
                go.AddComponent<DebugManager>();
                DontDestroyOnLoad(go);
            }

            // Carga inicial: nivel de test de la fase actual (fase 8)
            LoadLevel("level_fase8");
        }

        /// <summary>
        /// Load and start a level by index. Clears current state first.
        /// </summary>
        public void LoadLevel(int levelIndex)
        {
            LoadedLevelFileName = levelIndex + ".json";
            TextAsset levelAsset = Resources.Load<TextAsset>("Levels/" + levelIndex);
            if (levelAsset != null)
            {
                // Reset board and items before restarting
                ResetGame();
                Stage stage = JsonConvert.DeserializeObject<Stage>(levelAsset.text);
                StartGame(stage);
                Debug.Log($"[MatchManager] Loaded level {levelIndex}");
            }
            else
            {
                Debug.LogWarning($"[MatchManager] Level {levelIndex} not found in Resources/Levels/");
            }
        }

        /// <summary>
        /// Load and start a level by name (e.g. 'level_fase8'). Clears current state first.
        /// </summary>
        public void LoadLevel(string levelName)
        {
            if (string.IsNullOrEmpty(levelName))
            {
                Debug.LogWarning("[MatchManager] LoadLevel called with null or empty name.");
                return;
            }

            LoadedLevelFileName = levelName + ".json";
            TextAsset levelAsset = Resources.Load<TextAsset>("Levels/" + levelName);
            if (levelAsset != null)
            {
                // Reset board and items before restarting
                ResetGame();
                Stage stage = JsonConvert.DeserializeObject<Stage>(levelAsset.text);
                StartGame(stage);
                Debug.Log($"[MatchManager] Loaded level '{levelName}'");
            }
            else
            {
                Debug.LogWarning($"[MatchManager] Level '{levelName}' not found in Resources/Levels/");
            }
        }

        /// <summary>
        /// Clears all items and panels from the board without destroying Board GameObjects.
        /// </summary>
        private void ResetGame()
        {
            SetMatchState(MatchState.PrepareGame);
            for (int i = 0; i < 81; i++)
            {
                var board = m_ListBoard[i];
                if (board == null) continue;

                // Return item to pool
                if (board.m_Item != null)
                {
                    var item = board.m_Item as Items.Item;
                    if (item != null) ObjectPool.Instance?.Restore(item.gameObject);
                    board.m_Item = null;
                }

                // Return panels to pool
                for (int p = board.m_ListPanel.Count - 1; p >= 0; p--)
                {
                    var panel = board.m_ListPanel[p];
                    if (panel != null) Managers.PanelManager.Instance?.RestorePanel(panel.gameObject);
                }
                board.m_ListPanel.Clear();
            }
        }

        /// <summary>
        /// Full game initialization from stage data.
        /// </summary>
        public void StartGame(Stage stage)
        {
            m_CSD = stage;

            if (!stage.isUseGravity)
                stage.CreateGravity();

            VariableInit();
            StepInit();
            StageSetting(stage);
            BoardSetting(stage);
            PanelSetting(stage);
            BoardPositionSetting();
            ItemSetting();

            Managers.MissionManager.Instance?.MissionSetting(stage);

            SetMatchState(MatchState.Playing);
            SetStep(StepType.Wait);

            TopUIController.Instance?.SetLevelFileName(LoadedLevelFileName);
        }

        private void Update()
        {
            if (m_MatchState != MatchState.Playing) return;
            m_CurrentStep?.Step_Process();
        }

        private void VariableInit()
        {
            ComboCnt = 0;
            m_AppearColor.Clear();
            m_ListDropStart.Clear();
            m_ListDropHead.Clear();
        }

        /// <summary>
        /// Create all step instances and register them in the dictionary.
        /// </summary>
        private void StepInit()
        {
            m_DicStep = new Dictionary<StepType, BaseStep>
            {
                { StepType.Wait,          new WaitStep(this) },
                { StepType.Switching,     new MatchingStep(this) },
                { StepType.Matching,      new MatchingStep(this) },
                { StepType.TimeBomb,      new TimeBombStep(this) },
                { StepType.IceCream,      new IceCreamStep(this) },
                { StepType.ConveyerBelt,  new ConveyerBeltStep(this) },
                { StepType.Chameleon,     new ChameleonStep(this) },
                { StepType.MagicColor,    new MagicColorStep(this) },
                { StepType.BearJump,      new BearJumpStep(this) },
                { StepType.BearSpawn,     new BearSpawnStep(this) },
                { StepType.Mission,       new MissionStep(this) },
                { StepType.Clear,         new ClearStep(this) },
                { StepType.Fail,          new FailStep(this) },
                { StepType.Shuffling,     new ShufflingStep(this) },
            };

            foreach (var step in m_DicStep.Values)
                step.Step_Init();
        }

        /// <summary>
        /// Load active colors from stage data.
        /// </summary>
        private void StageSetting(Stage stage)
        {
            if (stage.appearColor != null)
            {
                for (int i = 0; i < stage.appearColor.Length && i < 6; i++)
                {
                    if (stage.appearColor[i])
                        m_AppearColor.Add((ColorType)(i + (int)ColorType.RED));
                }
            }

            if (m_AppearColor.Count == 0)
            {
                m_AppearColor.Add(ColorType.RED);
                m_AppearColor.Add(ColorType.YELLOW);
                m_AppearColor.Add(ColorType.GREEN);
            }
        }

        /// <summary>
        /// Instantiate 81 Board cells under Field. Called once.
        /// </summary>
        private void BoardCreate()
        {
            if (m_BoardPrefab == null)
            {
                Debug.LogError("[MatchManager] Board prefab not assigned!");
                return;
            }

            for (int i = 0; i < 81; i++)
            {
                GameObject obj;
                if (m_ObjectPool != null)
                    obj = m_ObjectPool.GetObject(m_BoardPrefab, m_Field);
                else
                    obj = Instantiate(m_BoardPrefab, m_Field);

                Board board = obj.GetComponent<Board>();
                m_ListBoard[i] = board;
            }

            // Wire up neighbor references
            for (int i = 0; i < 81; i++)
            {
                int x = i % 9;
                int y = i / 9;
                Board b = m_ListBoard[i];

                b.Top = y > 0 ? m_ListBoard[x + (y - 1) * 9] : null;
                b.Bottom = y < 8 ? m_ListBoard[x + (y + 1) * 9] : null;
                b.Left = x > 0 ? m_ListBoard[(x - 1) + y * 9] : null;
                b.Right = x < 8 ? m_ListBoard[(x + 1) + y * 9] : null;

                b.TopLeft = (x > 0 && y > 0) ? m_ListBoard[(x - 1) + (y - 1) * 9] : null;
                b.TopRight = (x < 8 && y > 0) ? m_ListBoard[(x + 1) + (y - 1) * 9] : null;
                b.BottomLeft = (x > 0 && y < 8) ? m_ListBoard[(x - 1) + (y + 1) * 9] : null;
                b.BottomRight = (x < 8 && y < 8) ? m_ListBoard[(x + 1) + (y + 1) * 9] : null;
            }
        }

        /// <summary>
        /// Configure each board cell from stage data (coordinates, gravity, active state).
        /// </summary>
        private void BoardSetting(Stage stage)
        {
            for (int i = 0; i < 81; i++)
            {
                int x = i % 9;
                int y = i / 9;
                Board board = m_ListBoard[i];
                board.Init(x, y, stage);

                // Determine if the cell is active from panel data
                bool isActive = true;
                if (stage.panels != null && i < stage.panels.Length && stage.panels[i]?.listinfo != null)
                {
                    bool hasFullPanel = false;
                    foreach (var pd in stage.panels[i].listinfo)
                    {
                        if (pd.paneltype == PanelType.Default_Empty || pd.paneltype == PanelType.Fixed_Block)
                        {
                            isActive = false;
                            break;
                        }
                        if (pd.paneltype == PanelType.Default_Full)
                            hasFullPanel = true;
                    }
                    // If panels exist but none is Default_Full or explicit Empty/Fixed, treat as active
                    // (Ice_Cage, Bread, etc. sit ON TOP of a Default_Full cell)
                }

                board.IsActiveCell = isActive;
                board.SetDisplayer(isActive);
                board.gameObject.SetActive(true);

                // Track drop starts
                if (board.isListDrop)
                    m_ListDropStart.Add(board);
            }

            GetBoardDropStartSetting();
            GetGravitySetting();
        }

        /// <summary>
        /// Calculate m_ListDropStart: cells that are gravity branch points.
        /// </summary>
        private void GetBoardDropStartSetting()
        {
            for (int i = 0; i < 81; i++)
            {
                Board b = m_ListBoard[i];
                if (!b.IsActiveCell) continue;
                if (m_ListDropStart.Contains(b)) continue;

                if (b.PossibleDrop_Dirs != null && b.PossibleDrop_Dirs.Length > 1)
                {
                    m_ListDropStart.Add(b);
                    continue;
                }

                Board dropFrom = b.DropBoard;
                if (dropFrom == null || !dropFrom.IsActiveCell || dropFrom.BlocksGravityFlow)
                {
                    if (!m_ListDropStart.Contains(b))
                        m_ListDropStart.Add(b);
                }
                else if (dropFrom.PossibleDrop_Dirs != null && dropFrom.PossibleDrop_Dirs.Length > 1)
                {
                    if (!m_ListDropStart.Contains(b))
                        m_ListDropStart.Add(b);
                }
            }
        }

        private void RefreshDropStartSetting()
        {
            m_ListDropStart.Clear();

            for (int i = 0; i < 81; i++)
            {
                Board board = m_ListBoard[i];
                if (board == null || !board.IsActiveCell) continue;

                if (board.isListDrop)
                    m_ListDropStart.Add(board);
            }

            GetBoardDropStartSetting();
        }

        /// <summary>
        /// Calculate m_ListDropHead: cells that are top of a drop column (spawn new pieces).
        /// </summary>
        private void GetGravitySetting()
        {
            for (int i = 0; i < 81; i++)
            {
                Board b = m_ListBoard[i];
                if (!b.IsActiveCell) continue;

                Board dropFrom = b.DropBoard;
                if (dropFrom == null || !dropFrom.IsActiveCell)
                {
                    m_ListDropHead.Add(b);
                }
            }
        }

        /// <summary>
        /// Create panels on all board cells from stage data.
        /// Each cell's panels[] array defines stacked panel types (bottom to top).
        /// Called after BoardSetting so IsActiveCell is already set.
        /// </summary>
        private void PanelSetting(Stage stage)
        {
            var pm = Managers.PanelManager.Instance;
            if (pm == null)
            {
                Debug.LogWarning("[MatchManager] PanelManager not found — skipping PanelSetting.");
                return;
            }

            if (stage.panels == null)
            {
                Debug.LogWarning("[MatchManager] Stage has no panel data.");
                return;
            }

            int panelCount = 0;
            for (int i = 0; i < 81; i++)
            {
                var board = m_ListBoard[i];
                if (i >= stage.panels.Length) break;

                var pannels = stage.panels[i];
                if (pannels?.listinfo == null) continue;

                foreach (var info in pannels.listinfo)
                {
                    // Skip DefaultFull — the board visual already represents it
                    if (info.paneltype == PanelType.Default_Full) continue;
                    // Skip DefaultEmpty — handled by IsActiveCell already
                    if (info.paneltype == PanelType.Default_Empty) continue;

                    var panel = pm.CreatePanel(info.paneltype, board);
                    if (panel == null) continue;
                    panelCount++;

                    // Apply extra data from JSON
                    if (info.value > 0)
                    {
                        if (panel is Panels.IceCagePanel iceCage)
                            iceCage.SetLayers(info.value);
                        else if (panel is Panels.LollyCagePanel lollyCage)
                            lollyCage.SetLayers(info.value);
                        else if (panel is Panels.BottleCagePanel bottleCage)
                            bottleCage.SetLayers(info.value);
                    }

                    // Sync Board flags from panels
                    if (info.paneltype == PanelType.Fixed_Block)  board.IsPanelFixed = true;
                    if (info.paneltype == PanelType.Ice_Cage
                        || info.paneltype == PanelType.Lolly_Cage
                        || info.paneltype == PanelType.Bottle_Cage)
                        board.IsPanelCage = true;
                }
            }

            Debug.Log($"[MatchManager] PanelSetting complete. {panelCount} special panels created.");
        }

        /// <summary>
        /// Generate initial items for all active cells.
        /// Uses predefined colors from stage if available, otherwise random.
        /// </summary>
        private void ItemSetting()
        {
            for (int i = 0; i < 81; i++)
            {
                Board board = m_ListBoard[i];
                if (!board.IsActiveCell) continue;

                ItemType itemType = ItemType.Normal;
                ColorType itemColor = ColorType.None;

                if (m_CSD != null)
                {
                    if (m_CSD.items != null && i < m_CSD.items.Length)
                        itemType = m_CSD.items[i];
                    if (m_CSD.colors != null && i < m_CSD.colors.Length)
                        itemColor = m_CSD.colors[i];
                }

                if (itemType == ItemType.None)
                    continue; // empty cell marker

                if (itemType != ItemType.Normal || itemColor != ColorType.None)
                {
                    // Spawn special or color-fixed item directly
                    board.GenItem(itemType, itemColor);
                    continue;
                }

                // No predefined type or color — generate random normal
                board.TopSpawnItem();
            }

            Debug.Log("[MatchManager] Initial items generated.");
        }

        /// <summary>
        /// Center and scale the board field to fit the camera view.
        /// </summary>
        private void BoardPositionSetting()
        {
            // Find bounds of active cells
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            int activeCount = 0;

            for (int i = 0; i < 81; i++)
            {
                if (!m_ListBoard[i].IsActiveCell) continue;
                activeCount++;
                Vector3 pos = m_ListBoard[i].transform.localPosition;
                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
            }

            if (activeCount == 0) return;

            // Center the field
            float centerX = (minX + maxX) * 0.5f;
            float centerY = (minY + maxY) * 0.5f;
            m_Field.localPosition = new Vector3(-centerX, -centerY, 0f);

            // Adjust camera to fit the board
            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.orthographic)
            {
                // Calculate board dimensions
                float boardWidth = maxX - minX + 1f;  // +1 for cell size
                float boardHeight = maxY - minY + 1f;

                // Get screen aspect ratio
                float screenAspect = (float)Screen.width / Screen.height;

                // Calculate required orthographic size
                // OrthographicSize is half-height of the view
                float requiredHeightSize = boardHeight * 0.5f;
                float requiredWidthSize = boardWidth / screenAspect * 0.5f;

                // Use the larger value to ensure everything fits, with padding
                float padding = 1.5f; // Extra space around the board
                mainCamera.orthographicSize = Mathf.Max(requiredHeightSize, requiredWidthSize) + padding;

                Debug.Log($"[MatchManager] Camera adjusted: Board size ({boardWidth:F1} x {boardHeight:F1}), Ortho size: {mainCamera.orthographicSize:F1}");
            }
        }

        public void SetMatchState(MatchState state)
        {
            m_MatchState = state;
        }

        public void SetStep(StepType step)
        {
            m_StepType = step;

            if (m_DicStep != null && m_DicStep.TryGetValue(step, out var newStep))
            {
                m_CurrentStep = newStep;
                m_CurrentStep.Step_Play();
            }
        }

        /// <summary>
        /// Get a random color from the active colors for this level.
        /// </summary>
        public ColorType GetRandomColor()
        {
            if (m_AppearColor.Count == 0) return ColorType.RED;
            return m_AppearColor[Random.Range(0, m_AppearColor.Count)];
        }

        /// <summary>
        /// Handle item switching (swap) initiated by player input.
        /// Checks if swap creates a match, then confirms or reverts.
        /// </summary>
        public void Switching(Match3.Items.Item itemA, Match3.Items.Item itemB)
        {
            if (itemA == null || itemB == null)
            {
                Debug.LogWarning("[MatchManager] Switching called with null items.");
                return;
            }

            if (itemA.m_Board == null || itemB.m_Board == null)
            {
                Debug.LogWarning("[MatchManager] Items have no board reference.");
                return;
            }

            if (itemA.m_Board.BlocksItemSwitch || itemB.m_Board.BlocksItemSwitch)
            {
                itemA.m_Board.NotifyItemSwitchAttempt();
                if (itemB.m_Board != itemA.m_Board)
                    itemB.m_Board.NotifyItemSwitchAttempt();

                Debug.Log($"[MatchManager] Switching blocked by panel. A={itemA.m_Board.name} blocked={itemA.m_Board.BlocksItemSwitch}, B={itemB.m_Board.name} blocked={itemB.m_Board.BlocksItemSwitch}");
                return;
            }

            StartCoroutine(Coroutine_Switching(itemA, itemB));
        }

        /// <summary>
        /// Coroutine: Swap two items, check for matches, confirm or revert swap.
        /// Phase 3: Full implementation with match detection and burst.
        /// </summary>
        private System.Collections.IEnumerator Coroutine_Switching(Match3.Items.Item itemA, Match3.Items.Item itemB)
        {
            SetStep(StepType.Switching);

            Board boardA = itemA.m_Board;
            Board boardB = itemB.m_Board;

            Debug.Log($"[MatchManager] Switching: {itemA.name} at {boardA.name} <-> {itemB.name} at {boardB.name}");

            // Store original positions
            Vector3 posA = itemA.transform.position;
            Vector3 posB = itemB.transform.position;

            Debug.Log($"  -> Item A world pos: ({posA.x:F1},{posA.y:F1}), Item B world pos: ({posB.x:F1},{posB.y:F1})");

            // Swap board references
            boardA.m_Item = itemB;
            boardB.m_Item = itemA;
            itemA.m_Board = boardB;
            itemB.m_Board = boardA;

            // Animate swap
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                itemA.transform.position = Vector3.Lerp(posA, posB, t);
                itemB.transform.position = Vector3.Lerp(posB, posA, t);

                yield return null;
            }

            // Ensure final positions match exactly (world space only, no parent change)
            itemA.transform.position = posB; // itemA now at boardB's position
            itemB.transform.position = posA; // itemB now at boardA's position

            Debug.Log($"  -> After swap animation: Item A at ({itemA.transform.position.x:F1},{itemA.transform.position.y:F1})");
            Debug.Log($"  -> After swap animation: Item B at ({itemB.transform.position.x:F1},{itemB.transform.position.y:F1})");

            // Phase 3: Check for matches after swap
            itemA.CheckCombine();
            itemB.CheckCombine();

            // Record swap boards so CheckMatchCondition places specials at the right position
            m_LastSwapBoardA = boardA;
            m_LastSwapBoardB = boardB;

            // Detect matches on both swapped items
            bool hasMatch = CheckMatchCondition();

            // Special + special: swapping two special pieces together is always valid
            // (the actual combine effects will be resolved in Fase 5 CheckCombine)
            if (!hasMatch && IsSpecialItem(itemA.m_ItemType) && IsSpecialItem(itemB.m_ItemType))
            {
                itemA.m_Board.m_isMatchBrust = true;
                itemB.m_Board.m_isMatchBrust = true;
                hasMatch = true;
            }

            Debug.Log($"[MatchManager] Swap completed. HasMatch: {hasMatch}");

            if (!hasMatch)
            {
                // No match - revert swap after a brief delay (no move cost)
                yield return new UnityEngine.WaitForSeconds(0.3f);

                // Swap back
                boardA.m_Item = itemA;
                boardB.m_Item = itemB;
                itemA.m_Board = boardA;
                itemB.m_Board = boardB;

                // Animate revert
                elapsed = 0f;
                Vector3 currentPosA = itemA.transform.position;
                Vector3 currentPosB = itemB.transform.position;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;

                    itemA.transform.position = Vector3.Lerp(currentPosA, posA, t);
                    itemB.transform.position = Vector3.Lerp(currentPosB, posB, t);

                    yield return null;
                }

                itemA.transform.position = posA;
                itemB.transform.position = posB;

                // Swap fallido: volver directamente a Wait sin pasar por MissionStep
                Match3.Items.Item.SwitchingTouch = false;
                SetStep(StepType.Wait);
                yield break;
            }
            else
            {
                // Valid match - trigger match detection and burst sequence
                SetStep(StepType.Matching);

                // Burst → Drop → Cascade loop
                yield return StartCoroutine(Co_MatchBurst());
                m_LastSwapBoardA = null; // cascades after the first burst have no swap board
                m_LastSwapBoardB = null;
                yield return StartCoroutine(Co_Drop());

                while (CheckMatchCondition())
                {
                    ComboCnt++;
                    yield return StartCoroutine(Co_MatchBurst());
                    yield return StartCoroutine(Co_Drop());
                }
            }

            // Advance through the post-match step chain:
            // TimeBomb → IceCream → ConveyerBelt → Chameleon → MagicColor → BearJump → BearSpawn → Mission → Wait
            Match3.Items.Item.SwitchingTouch = false;
            SetStep(StepType.TimeBomb);
        }

        // ========== PHASE 3: MATCH DETECTION ==========

        /// <summary>
        /// Returns true if the item type is a special (activatable) item — not a Normal piece.
        /// Special items activate via color match or special+special swap.
        /// </summary>
        private static bool IsSpecialItem(ItemType t)
        {
            return t == ItemType.Butterfly || t == ItemType.Line_X || t == ItemType.Line_Y ||
                   t == ItemType.Line_C    || t == ItemType.Bomb    || t == ItemType.Rainbow;
        }

        /// <summary>
        /// Returns true if any board cell is already flagged for burst
        /// (e.g. from a special item effect that fired in the previous wave).
        /// </summary>
        private bool HasPendingBursts()
        {
            for (int i = 0; i < 81; i++)
                if (m_ListBoard[i].m_isMatchBrust && m_ListBoard[i].m_Item != null)
                    return true;
            return false;
        }

        /// <summary>
        /// From a match group, pick the best board for spawning the special item.
        /// Prefers whichever board the player last swapped to; falls back to the board nearest the group center.
        /// </summary>
        private Board FindSpecialSpawnBoard(System.Collections.Generic.List<Board> matchGroup)
        {
            if (m_LastSwapBoardA != null && matchGroup.Contains(m_LastSwapBoardA))
                return m_LastSwapBoardA;
            if (m_LastSwapBoardB != null && matchGroup.Contains(m_LastSwapBoardB))
                return m_LastSwapBoardB;

            // Cascade — find the board closest to the geometric center of the group
            Vector3 avg = Vector3.zero;
            foreach (var b in matchGroup) avg += b.transform.position;
            avg /= matchGroup.Count;

            Board closest = matchGroup[0];
            float bestDist = float.MaxValue;
            foreach (var b in matchGroup)
            {
                float d = (b.transform.position - avg).sqrMagnitude;
                if (d < bestDist) { bestDist = d; closest = b; }
            }
            return closest;
        }

        /// <summary>
        /// Set m_FusionTargetPos on every board in the group so their items slide toward spawnBoard on destroy.
        /// </summary>
        private void SetFusionTargets(System.Collections.Generic.IEnumerable<Board> group, Board spawnBoard)
        {
            Vector3 targetPos = spawnBoard.transform.position;
            foreach (var b in group)
                b.m_FusionTargetPos = targetPos;
        }

        /// <summary>
        /// Check all boards for matches and mark them for bursting.
        /// Returns true if any matches were found.
        /// </summary>
        public bool CheckMatchCondition()
        {
            bool foundMatch = false;
            int matchCount = 0;

            // Clear all match flags before fresh detection
            for (int i = 0; i < 81; i++)
            {
                m_ListBoard[i].m_isMatchBrust = false;
                m_ListBoard[i].m_NextItemType = ItemType.None;
            }

            // === Priority 6: 5+ in a line → Rainbow ===
            for (int i = 0; i < 81; i++)
            {
                Board board = m_ListBoard[i];
                if (!board.IsNowItemMatch) continue;
                if (board.m_isMatchBrust) continue;

                var horizontal = board.FindMatchesHorizontal();
                var vertical = board.FindMatchesVertical();

                if (horizontal.Count >= 5)
                {
                    foreach (var b in horizontal) b.m_isMatchBrust = true;
                    var spawnH = FindSpecialSpawnBoard(horizontal);
                    spawnH.m_NextItemType = ItemType.Rainbow;
                    SetFusionTargets(horizontal, spawnH);
                    foundMatch = true;
                }
                else if (vertical.Count >= 5)
                {
                    foreach (var b in vertical) b.m_isMatchBrust = true;
                    var spawnV = FindSpecialSpawnBoard(vertical);
                    spawnV.m_NextItemType = ItemType.Rainbow;
                    SetFusionTargets(vertical, spawnV);
                    foundMatch = true;
                }
            }

            // === Priority 5: L-shape (corner) → Bomb ===
            // === Priority 4: T-shape or + → Line_C ===
            // Both require matches in both H and V directions (≥2 each, total ≥4)
            for (int i = 0; i < 81; i++)
            {
                Board board = m_ListBoard[i];
                if (!board.IsNowItemMatch) continue;
                if (board.m_isMatchBrust) continue;

                var horizontal = board.FindMatchesHorizontal();
                var vertical = board.FindMatchesVertical();

                // Need at least 2 in each direction with total >= 4
                // horizontal/vertical include the center cell, so Count>=2 means 1 neighbor + center
                int hCount = horizontal.Count; // includes center
                int vCount = vertical.Count;   // includes center
                if (hCount < 2 || vCount < 2) continue;
                if ((hCount + vCount - 1) < 4) continue; // -1 because center is counted twice

                // Determine if L-shape or T-shape using bit pattern
                // Check which sides have neighbors
                int bits = 0;
                var item = (board.m_Item as Match3.Items.Item);
                if (item == null) continue;
                ColorType c = item.m_Color;

                // Check horizontal neighbors (excluding center)
                bool hasLeft = board.Left != null && board.Left.IsNowItemMatch &&
                    (board.Left.m_Item as Match3.Items.Item)?.m_Color == c;
                bool hasRight = board.Right != null && board.Right.IsNowItemMatch &&
                    (board.Right.m_Item as Match3.Items.Item)?.m_Color == c;
                bool hasTop = board.Top != null && board.Top.IsNowItemMatch &&
                    (board.Top.m_Item as Match3.Items.Item)?.m_Color == c;
                bool hasBottom = board.Bottom != null && board.Bottom.IsNowItemMatch &&
                    (board.Bottom.m_Item as Match3.Items.Item)?.m_Color == c;

                if (hasLeft)  bits |= 1;
                if (hasRight) bits |= 2;
                if (hasBottom) bits |= 4;
                if (hasTop)   bits |= 8;

                // L-shape: neighbors on exactly one side of each axis
                // bits 5 = left+bottom, 6 = right+bottom, 9 = left+top, 10 = right+top
                bool isLShape = (bits == 5 || bits == 6 || bits == 9 || bits == 10);

                // Combine both lists (remove duplicates)
                var combined = new System.Collections.Generic.HashSet<Board>(horizontal);
                foreach (var b in vertical) combined.Add(b);
                foreach (var b in combined) b.m_isMatchBrust = true;

                var combinedList = new System.Collections.Generic.List<Board>(combined);
                var spawnCross = FindSpecialSpawnBoard(combinedList);
                if (isLShape)
                    spawnCross.m_NextItemType = ItemType.Bomb;    // L → Bomb
                else
                    spawnCross.m_NextItemType = ItemType.Line_C;  // T or + → Line_C (cross)
                SetFusionTargets(combinedList, spawnCross);

                foundMatch = true;
            }

            // === Priority 3: 4 in vertical → Line_X (destroys row, perpendicular) ===
            // === Priority 2: 4 in horizontal → Line_Y (destroys column, perpendicular) ===
            for (int i = 0; i < 81; i++)
            {
                Board board = m_ListBoard[i];
                if (!board.IsNowItemMatch) continue;
                if (board.m_isMatchBrust) continue;

                var horizontal = board.FindMatchesHorizontal();
                var vertical = board.FindMatchesVertical();

                if (vertical.Count == 4)
                {
                    foreach (var b in vertical) b.m_isMatchBrust = true;
                    var spawnLineX = FindSpecialSpawnBoard(vertical);
                    spawnLineX.m_NextItemType = ItemType.Line_X; // 4 vertical → destroys row (perpendicular)
                    SetFusionTargets(vertical, spawnLineX);
                    foundMatch = true;
                }
                else if (horizontal.Count == 4)
                {
                    foreach (var b in horizontal) b.m_isMatchBrust = true;
                    var spawnLineY = FindSpecialSpawnBoard(horizontal);
                    spawnLineY.m_NextItemType = ItemType.Line_Y; // 4 horizontal → destroys column (perpendicular)
                    SetFusionTargets(horizontal, spawnLineY);
                    foundMatch = true;
                }
            }

            // === Priority 1: 2×2 square → Butterfly ===
            bool squarePivotAssigned = false;
            for (int i = 0; i < 81; i++)
            {
                Board board = m_ListBoard[i];
                if (!board.IsNowItemMatch) continue;
                if (board.m_isMatchBrust) continue;

                var square = board.FindMatchesSquare();
                if (square.Count == 4)
                {
                    foreach (var b in square) b.m_isMatchBrust = true;
                    if (!squarePivotAssigned)
                    {
                        var spawnButterfly = FindSpecialSpawnBoard(square);
                        spawnButterfly.m_NextItemType = ItemType.Butterfly;
                        SetFusionTargets(square, spawnButterfly);
                        squarePivotAssigned = true;
                    }
                    foundMatch = true;
                }
            }

            // === Priority 0: 3 in a line → Normal (no special) ===
            for (int i = 0; i < 81; i++)
            {
                Board board = m_ListBoard[i];
                if (!board.IsNowItemMatch) continue;
                if (board.m_isMatchBrust) continue;

                var horizontal = board.FindMatchesHorizontal();
                var vertical = board.FindMatchesVertical();

                if (horizontal.Count >= 3)
                {
                    foreach (var b in horizontal)
                    {
                        b.m_isMatchBrust = true;
                        matchCount++;
                    }
                    foundMatch = true;
                }

                if (vertical.Count >= 3)
                {
                    foreach (var b in vertical)
                    {
                        b.m_isMatchBrust = true;
                        matchCount++;
                    }
                    foundMatch = true;
                }
            }

            if (foundMatch)
            {
                Debug.Log($"[MatchManager] CheckMatchCondition found {matchCount} cells to burst");
            }

            return foundMatch;
        }

        // ========== PHASE 3: BURST & GRAVITY ==========

        /// <summary>
        /// Execute burst on all marked cells.
        /// Coroutine version for sequential animation.
        /// </summary>
        private System.Collections.IEnumerator Co_MatchBurst()
        {
            Debug.Log("[MatchManager] Co_MatchBurst: Collecting burst coroutines");

            // Keep bursting in waves until no more flags remain.
            // Each wave may generate new flags via special item effects (Line, Bomb, etc.)
            // All waves are resolved before returning, so Co_Drop only runs once per turn.
            int waveLimit = 20; // safety cap against infinite loops
            while (HasPendingBursts() && waveLimit-- > 0)
            {
                // Snapshot boards flagged for this wave
                var boardsToBurst = new List<Board>();
                for (int i = 0; i < 81; i++)
                {
                    Board board = m_ListBoard[i];
                    if (board.m_isMatchBrust && board.m_Item != null)
                    {
                        Debug.Log($"  -> Queueing burst for {board.name} with item {board.m_Item.name}");
                        boardsToBurst.Add(board);
                    }
                    // Clear ALL flags now — specials will re-set flags for their targets
                    board.m_isMatchBrust = false;
                }

                if (boardsToBurst.Count == 0) break;

                Debug.Log($"[MatchManager] Co_MatchBurst: Starting {boardsToBurst.Count} burst coroutines");

                foreach (var board in boardsToBurst)
                    StartCoroutine(board.Co_Brust());

                // Wait for this wave's animations to finish before starting the next wave
                yield return new UnityEngine.WaitForSeconds(0.35f);
            }

            Debug.Log("[MatchManager] Co_MatchBurst: Complete");
        }

        /// <summary>
        /// Apply gravity and refill empty cells.
        /// Coroutine version for animated drops.
        /// </summary>
        private System.Collections.IEnumerator Co_Drop()
        {
            Debug.Log("[MatchManager] Co_Drop: Starting gravity and refill");

            int passCount = 0;
            int totalWaitCount = 0;
            const int maxPasses = 20;
            bool anyGravityChange = false;

            while (passCount < maxPasses)
            {
                RefreshDropStartSetting();

                anyGravityChange = false;

                // Process gravity independently for each active segment.
                foreach (Board dropStart in m_ListDropStart)
                {
                    if (dropStart == null || !dropStart.IsActiveCell) continue;

                    Debug.Log($"[MatchManager] Processing gravity segment from {dropStart.name} (pass {passCount + 1})");
                    if (dropStart.GravityDropItemRow())
                        anyGravityChange = true;
                }

                if (!anyGravityChange)
                    break;
                passCount++;
            }

            if (anyGravityChange || passCount > 0)
            {
                bool stillDropping = true;
                int waitCount = 0;
                const int maxWait = 40;

                while (stillDropping && waitCount < maxWait)
                {
                    stillDropping = false;
                    for (int i = 0; i < 81; i++)
                    {
                        if (m_ListBoard[i].m_DropAnim)
                        {
                            stillDropping = true;
                            break;
                        }
                    }

                    if (stillDropping)
                    {
                        yield return null;
                        waitCount++;
                    }
                }

                totalWaitCount += waitCount;
            }

            Debug.Log($"[MatchManager] Co_Drop: Completed after {passCount} passes. Waited {totalWaitCount} cycles.");
        }

        /// <summary>
        /// Synchronous match burst (for immediate destruction).
        /// </summary>
        public void MatchBrust()
        {
            for (int i = 0; i < 81; i++)
            {
                Board board = m_ListBoard[i];
                if (board.m_isMatchBrust && board.m_Item != null)
                {
                    board.Brust();
                }
            }

            for (int i = 0; i < 81; i++)
            {
                m_ListBoard[i].m_isMatchBrust = false;
            }
        }

        /// <summary>
        /// Synchronous gravity drop (for immediate refill).
        /// </summary>
        public void Drop()
        {
            int passCount = 0;
            const int maxPasses = 20;

            while (passCount < maxPasses)
            {
                RefreshDropStartSetting();
                bool anyGravityChange = false;

                foreach (Board dropStart in m_ListDropStart)
                {
                    if (dropStart == null || !dropStart.IsActiveCell) continue;

                    if (dropStart.GravityDropItemRow())
                        anyGravityChange = true;
                }

                if (!anyGravityChange)
                    break;

                passCount++;
            }
        }
    }
}
