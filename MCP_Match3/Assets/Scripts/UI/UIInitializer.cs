using UnityEngine;

namespace Match3.UI
{
    /// <summary>
    /// Initializes PopupManager singleton if it doesn't exist.
    /// All UI (Canvas, buttons, etc.) should be created in the Editor.
    /// </summary>
    public class UIInitializer : MonoBehaviour
    {
        private void Awake()
        {
            if (PopupManager.Instance == null)
            {
                GameObject pmGO = new GameObject("PopupManager");
                pmGO.AddComponent<PopupManager>();
                Debug.Log("[UIInitializer] PopupManager created");
            }
        }
    }
}
