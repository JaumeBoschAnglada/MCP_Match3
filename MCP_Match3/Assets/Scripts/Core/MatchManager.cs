using System.Collections.Generic;
using UnityEngine;
using Match3.Data;
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

            // Try to load a test level
            TextAsset levelAsset = Resources.Load<TextAsset>("Levels/1");
            if (levelAsset != null)
            {
                Stage stage = JsonConvert.DeserializeObject<Stage>(levelAsset.text);
                StartGame(stage);
            }
            else
            {
                Debug.Log("[MatchManager] No level found. Create a level JSON in Resources/Levels/1.json");
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
            StageSetting(stage);
            BoardSetting(stage);
            BoardPositionSetting();
            SetMatchState(MatchState.Playing);
        }

        private void VariableInit()
        {
            ComboCnt = 0;
            m_AppearColor.Clear();
            m_ListDropStart.Clear();
            m_ListDropHead.Clear();
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
                if (stage.panels != null && i < stage.panels.Length && stage.panels[i].listinfo != null)
                {
                    for (int p = 0; p < stage.panels[i].listinfo.Count; p++)
                    {
                        if (stage.panels[i].listinfo[p].paneltype == PanelType.Default_Empty)
                        {
                            isActive = false;
                            break;
                        }
                    }
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
                if (dropFrom == null || !dropFrom.IsActiveCell || dropFrom.IsPanelFixed)
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
        }

        public void SetMatchState(MatchState state)
        {
            m_MatchState = state;
        }

        public void SetStep(StepType step)
        {
            m_StepType = step;
        }

        /// <summary>
        /// Get a random color from the active colors for this level.
        /// </summary>
        public ColorType GetRandomColor()
        {
            if (m_AppearColor.Count == 0) return ColorType.RED;
            return m_AppearColor[Random.Range(0, m_AppearColor.Count)];
        }
    }
}
