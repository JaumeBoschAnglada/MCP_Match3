using UnityEngine;

namespace Match3.UI
{
    /// <summary>
    /// Button that opens the pause popup.
    /// No direct prefab references - just calls PopupManager.ShowPopupByType(PopupType.Pause).
    /// The prefab is managed by PopupManager.
    /// </summary>
    public class PauseButton : MonoBehaviour
    {
        private PopupManager popupManager;

        private void Start()
        {
            popupManager = PopupManager.Instance;
            if (popupManager == null)
            {
                Debug.LogError("[PauseButton] PopupManager not found in scene!");
            }
        }

        public void OnPausePressed()
        {
            if (popupManager == null)
            {
                Debug.LogError("[PauseButton] PopupManager not initialized");
                return;
            }
            popupManager.ShowPopupByType(PopupType.Pause);
        }
    }
}
