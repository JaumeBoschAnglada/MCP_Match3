using UnityEngine;
using UnityEngine.InputSystem;
using Match3.Core;

namespace Match3.Input
{
    /// <summary>
    /// Manages input for both desktop (mouse) and mobile (touch) using Unity's new Input System.
    /// Automatically works across all platforms (PC, Android, iOS) without platform-specific code.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Input Actions")]
        [SerializeField] private InputActionAsset m_InputActions;

        private MatchManager m_MatchMgr;
        private Camera m_MainCamera;

        // Input actions
        private InputAction m_PointAction;
        private InputAction m_PressAction;

        // Swap tracking
        private Items.Item m_SwapA;
        private Items.Item m_SwapB;
        private bool m_IsDragging;
        private Vector2 m_DragStartPos;
        private GravityDisplayer m_ActiveDisplayer;

        [Header("Settings")]
        [SerializeField] private float m_MinDragDistance = 25f;

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

            // Setup input actions
            if (m_InputActions != null)
            {
                var gameplayMap = m_InputActions.FindActionMap("Gameplay");
                if (gameplayMap != null)
                {
                    m_PointAction = gameplayMap.FindAction("Point");
                    m_PressAction = gameplayMap.FindAction("Press");

                    // Subscribe to Press events
                    m_PressAction.started += OnPressStarted;
                    m_PressAction.canceled += OnPressCanceled;
                }
            }
        }

        private void Start()
        {
            m_MatchMgr = MatchManager.Instance;
            m_MainCamera = Camera.main;
        }

        private void OnEnable()
        {
            m_InputActions?.Enable();
        }

        private void OnDisable()
        {
            m_InputActions?.Disable();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (m_PressAction != null)
            {
                m_PressAction.started -= OnPressStarted;
                m_PressAction.canceled -= OnPressCanceled;
            }
        }

        /// <summary>
        /// Called when press/click/tap starts.
        /// </summary>
        private void OnPressStarted(InputAction.CallbackContext context)
        {
            if (!CanAcceptInput()) return;

            Vector2 screenPos = m_PointAction.ReadValue<Vector2>();
            Items.Item item = GetItemAtScreenPosition(screenPos);

            if (item != null && CanSwap(item))
            {
                m_SwapA = item;
                m_IsDragging = true;
                m_DragStartPos = screenPos;

                // Show gravity arrow on the selected cell (Fase 8)
                if (MatchManager.Instance?.m_CSD?.isUseGravity == true)
                {
                    m_ActiveDisplayer = item.m_Board?.GetComponent<GravityDisplayer>();
                    m_ActiveDisplayer?.Show();
                }
            }
        }

        /// <summary>
        /// Called when press/click/tap is released.
        /// </summary>
        private void OnPressCanceled(InputAction.CallbackContext context)
        {
            CancelSwap();
        }

        /// <summary>
        /// While a drag is in progress, check every frame if the threshold is crossed.
        /// This fires the swap without waiting for the finger to be lifted.
        /// </summary>
        private void Update()
        {
            if (!m_IsDragging || m_SwapA == null) return;

            Vector2 currentPos = m_PointAction.ReadValue<Vector2>();
            Vector2 dragDelta = currentPos - m_DragStartPos;

            if (dragDelta.magnitude > m_MinDragDistance)
            {
                Items.Item target = GetNeighborInDirection(m_SwapA, dragDelta);
                if (target != null)
                {
                    m_SwapB = target;
                    ExecuteSwap();
                }
                // Cancel regardless of whether a valid neighbor was found,
                // so one drag gesture = one swap attempt.
                CancelSwap();
            }
        }

        /// <summary>
        /// Check if we can accept input right now.
        /// </summary>
        private bool CanAcceptInput()
        {
            if (m_MatchMgr == null || m_MainCamera == null) return false;
            if (m_MatchMgr.m_MatchState != Data.MatchState.Playing) return false;
            if (m_MatchMgr.m_StepType != Data.StepType.Wait) return false;
            return true;
        }

        /// <summary>
        /// Get the item at a screen position using raycast.
        /// </summary>
        private Items.Item GetItemAtScreenPosition(Vector2 screenPos)
        {
            if (m_MainCamera == null) return null;

            Ray ray = m_MainCamera.ScreenPointToRay(screenPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                return hit.collider.GetComponent<Items.Item>();
            }

            return null;
        }

        /// <summary>
        /// Get the neighbor in the primary drag direction.
        /// </summary>
        private Items.Item GetNeighborInDirection(Items.Item item, Vector2 dragDelta)
        {
            if (item == null || item.m_Board == null) return null;

            // Determine primary direction (horizontal or vertical)
            bool isHorizontal = Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y);

            Board neighbor = null;

            if (isHorizontal)
            {
                // Drag left or right
                neighbor = dragDelta.x > 0 ? item.m_Board.Right : item.m_Board.Left;
            }
            else
            {
                // Drag up or down
                // NOTE: Screen Y is inverted (up = positive), so we invert the check
                neighbor = dragDelta.y > 0 ? item.m_Board.Top : item.m_Board.Bottom;
            }

            if (neighbor == null || neighbor.m_Item == null) return null;

            return neighbor.m_Item as Items.Item;
        }

        /// <summary>
        /// Check if we can initiate a swap on this item.
        /// </summary>
        private bool CanSwap(Items.Item item)
        {
            if (item == null) return false;
            if (item.m_Board == null || !item.m_Board.IsActiveCell) return false;
            if (!item.Switch) return false;
            if (item.m_Board.BlocksItemSwitch) return false;
            return true;
        }

        /// <summary>
        /// Execute the swap between SwapA and SwapB.
        /// </summary>
        private void ExecuteSwap()
        {
            if (m_SwapA != null && m_SwapB != null && m_MatchMgr != null)
            {
                m_MatchMgr.Switching(m_SwapA, m_SwapB);
            }

            CancelSwap();
        }

        /// <summary>
        /// Cancel the current drag/swap operation.
        /// </summary>
        private void CancelSwap()
        {
            m_ActiveDisplayer?.Hide();
            m_ActiveDisplayer = null;
            m_SwapA = null;
            m_SwapB = null;
            m_IsDragging = false;
        }
    }
}
