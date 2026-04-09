using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Match3.UI
{
    /// <summary>
    /// Global PopupManager singleton - Lives in Hall, persists between scenes.
    /// Manages all popup prefab references and display lifecycle.
    /// Buttons/triggers call ShowPopupByType(PopupType) - they don't reference prefabs.
    /// 
    /// PopupManager has its own Canvas (sort order: 1) for displaying popups/modals.
    /// HUDCanvas in each scene (sort order: 0) is independent for base UI elements.
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        public static PopupManager Instance { get; private set; }

        // Popup prefab references - assign in Inspector (in Hall scene)
        [SerializeField] private PopupBase pausePopupPrefab;

        private Stack<PopupBase> activePopups = new Stack<PopupBase>();
        private Canvas popupCanvas; // PopupManager's own Canvas (for popups/modals)

        // Dictionary to map PopupType to prefab
        private Dictionary<PopupType, PopupBase> popupPrefabs = new Dictionary<PopupType, PopupBase>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Persist from Hall to Gameplay
                Debug.Log("[PopupManager] ✅ PopupManager initialized and will persist across scenes");
            }
            else if (Instance != this)
            {
                // Different instance detected - destroy the duplicate
                Debug.LogWarning("[PopupManager] ⚠️ Duplicate PopupManager detected (not from Hall). Destroying this instance.");
                Destroy(gameObject);
                return;
            }
            // If Instance == this, we're reactivating the existing instance (OK - no action needed)
        }

        private void Start()
        {
            // Initialize popup prefab mapping
            InitializePopupPrefabs();

            // Get PopupManager's own Canvas (for popups/modals)
            popupCanvas = GetComponent<Canvas>();
            if (popupCanvas == null)
            {
                Debug.LogError("[PopupManager] ❌ PopupManager must have a Canvas component!");
            }
        }

        private void OnEnable()
        {
            // When loading a new scene, ensure popupCanvas reference is still valid
            if (Instance == this && gameObject.activeInHierarchy && popupCanvas == null)
            {
                popupCanvas = GetComponent<Canvas>();
            }
        }

        /// <summary>
        /// Initialize the popup prefab mapping.
        /// First checks Inspector assignment, then loads from Resources if needed.
        /// </summary>
        private void InitializePopupPrefabs()
        {
            popupPrefabs.Clear();
            
            // Try Inspector assignment first
            if (pausePopupPrefab != null)
            {
                popupPrefabs[PopupType.Pause] = pausePopupPrefab;
                Debug.Log("[PopupManager] ✅ PausePopup prefab loaded from Inspector");
            }
            else
            {
                // Fallback: Load from Resources
                PausePopup loadedPrefab = Resources.Load<PausePopup>("Prefabs/UI/PausePopup");
                if (loadedPrefab != null)
                {
                    pausePopupPrefab = loadedPrefab;
                    popupPrefabs[PopupType.Pause] = pausePopupPrefab;
                    Debug.Log("[PopupManager] ✅ PausePopup prefab loaded from Resources/Prefabs/UI/PausePopup");
                }
                else
                {
                    Debug.LogError("[PopupManager] ❌ PausePopup prefab not found! Not assigned in Inspector and not in Resources/Prefabs/UI/PausePopup");
                }
            }
            
            // Add more popup types here as they are created (Shop, Settings, etc.)
        }

        /// <summary>
        /// Show a popup by type. The prefab must be assigned in the Inspector.
        /// This is the main entry point for buttons/triggers.
        /// </summary>
        public PopupBase ShowPopupByType(PopupType popupType)
        {
            if (!popupPrefabs.ContainsKey(popupType))
            {
                Debug.LogError($"[PopupManager] No prefab registered for popup type: {popupType}");
                return null;
            }

            PopupBase prefab = popupPrefabs[popupType];
            if (prefab == null)
            {
                Debug.LogError($"[PopupManager] Prefab for popup type {popupType} is null!");
                return null;
            }

            return ShowPopup(prefab);
        }

        /// <summary>
        /// Show a popup of type T. Instantiates the prefab under the popup canvas.
        /// </summary>
        public T ShowPopup<T>(T prefabOrInstance) where T : PopupBase
        {
            if (prefabOrInstance == null) return null;

            // Instantiate under popup canvas (layer 2)
            if (popupCanvas == null)
            {
                Debug.LogError("[PopupManager] Popup canvas not found!");
                return null;
            }

            PopupBase popup = Instantiate(prefabOrInstance, popupCanvas.transform);
            popup.gameObject.SetActive(true);

            activePopups.Push(popup);
            popup.Initialize(this);
            popup.OnShow();

            // Pause game if there are active popups
            UpdateGamePauseState();

            return popup as T;
        }

        /// <summary>
        /// Close the topmost popup.
        /// </summary>
        public void ClosePopup(PopupBase popup)
        {
            if (activePopups.Count == 0) return;
            if (activePopups.Peek() == popup)
            {
                activePopups.Pop();
            }
            else
            {
                // Remove from stack even if not on top
                var temp = new Stack<PopupBase>();
                while (activePopups.Count > 0)
                {
                    var p = activePopups.Pop();
                    if (p != popup) temp.Push(p);
                }
                while (temp.Count > 0) activePopups.Push(temp.Pop());
            }

            popup.OnHide();
            popup.gameObject.SetActive(false);
            Destroy(popup.gameObject);

            UpdateGamePauseState();
        }

        /// <summary>Close all popups.</summary>
        public void CloseAllPopups()
        {
            while (activePopups.Count > 0)
            {
                var popup = activePopups.Pop();
                popup.OnHide();
                popup.gameObject.SetActive(false);
                Destroy(popup.gameObject);
            }
            UpdateGamePauseState();
        }

        /// <summary>Check if any popup is active.</summary>
        public bool HasActivePopups => activePopups.Count > 0;

        /// <summary>Pause/resume the game based on popup state.</summary>
        private void UpdateGamePauseState()
        {
            if (HasActivePopups)
                Time.timeScale = 0f;
            else
                Time.timeScale = 1f;
        }
    }
}
