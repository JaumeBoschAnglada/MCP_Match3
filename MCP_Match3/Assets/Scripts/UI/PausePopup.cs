using UnityEngine;
using UnityEngine.UI;
using Match3.Gameplay;

namespace Match3.UI
{
    /// <summary>
    /// Pause popup - appears when user presses pause button during gameplay.
    /// Inherits from PopupBase which provides: Title, Close button (auto-wired).
    /// This popup adds: Resume button, Restart button.
    /// 
    /// Structure expected:
    /// Canvas (PausePopup prefab with PausePopup component)
    ///   ├─ Panel (background)
    ///   │  ├─ Title (Text - auto-found by PopupBase)
    ///   │  ├─ CloseButton (Button - auto-found)
    ///   │  ├─ ResumeButton (Button)
    ///   │  └─ RestartButton (Button)
    /// </summary>
    public class PausePopup : PopupBase
    {
        [SerializeField] private Button resumeButton;  // Resume game button
        [SerializeField] private Button restartButton; // Restart level button

        public override void Initialize(PopupManager manager)
        {
            base.Initialize(manager);
            
            // Auto-find buttons if not assigned
            if (resumeButton == null)
            {
                Transform resumeButtonTransform = transform.Find("ResumeButton");
                if (resumeButtonTransform == null)
                    resumeButtonTransform = transform.Find("Panel/ResumeButton");
                if (resumeButtonTransform != null)
                    resumeButton = resumeButtonTransform.GetComponent<Button>();
            }

            if (restartButton == null)
            {
                Transform restartButtonTransform = transform.Find("RestartButton");
                if (restartButtonTransform == null)
                    restartButtonTransform = transform.Find("Panel/RestartButton");
                if (restartButtonTransform != null)
                    restartButton = restartButtonTransform.GetComponent<Button>();
            }
        }

        public override void OnShow()
        {
            base.OnShow();
            SetTitle("PAUSED");

            // Wire up button listeners
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumePressed);
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartPressed);
        }

        public override void OnHide()
        {
            // Clean up listeners
            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(OnResumePressed);
            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartPressed);

            base.OnHide();
        }

        private void OnResumePressed()
        {
            Debug.Log("[PausePopup] Resume pressed");
            Close();
        }

        private void OnRestartPressed()
        {
            Debug.Log("[PausePopup] Restart pressed");
            if (popupManager != null)
                popupManager.CloseAllPopups();
            
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.LoadLevel(gameManager.GetCurrentLevelNumber());
            }
        }
    }
}
