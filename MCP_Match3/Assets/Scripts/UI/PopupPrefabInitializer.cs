using UnityEngine;
using Match3.UI;

namespace Match3.Setup
{
    /// <summary>
    /// Runtime initialization script that assigns popup prefabs to PopupManager.
    /// This script does NOT create GameObjects - it only assigns already-created prefab references.
    /// Places this on a simple GameObject or add to an existing manager.
    /// </summary>
    public class PopupPrefabInitializer : MonoBehaviour
    {
        private static bool isInitialized = false;

        private void Awake()
        {
            // Only run once
            if (isInitialized) return;
            isInitialized = true;

            InitializePopupPrefabs();
        }

        public static void InitializePopupPrefabs()
        {
            PopupManager popupManager = PopupManager.Instance;
            if (popupManager == null)
            {
                Debug.LogWarning("[PopupPrefabInitializer] PopupManager singleton not found yet");
                return;
            }

            // Load PausePopup prefab
            PausePopup pausePopupPrefab = Resources.Load<PausePopup>("Prefabs/UI/PausePopup");
            
            if (pausePopupPrefab == null)
            {
                Debug.LogError("[PopupPrefabInitializer] ❌ PausePopup prefab not found at Resources/Prefabs/UI/PausePopup");
                return;
            }

            // Assign using reflection (PopupManager stores it as private field)
            System.Reflection.FieldInfo field = typeof(PopupManager).GetField("pausePopupPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(popupManager, pausePopupPrefab);
                Debug.Log("[PopupPrefabInitializer] ✅ PausePopup prefab assigned to PopupManager");
            }
            else
            {
                Debug.LogError("[PopupPrefabInitializer] ❌ pausePopupPrefab field not found in PopupManager");
            }
        }
    }
}
