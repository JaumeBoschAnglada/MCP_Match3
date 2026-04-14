using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Match3.UI
{
    /// <summary>
    /// Pause popup - appears when user presses pause button during gameplay.
    /// Inherits from PopupBase which provides: Title, Close button (auto-wired).
    /// This popup adds: Resume button, Restart button.
    /// </summary>
    public class PausePopup : PopupBase
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;

        public override void Initialize(PopupManager manager)
        {
            base.Initialize(manager);

            if (resumeButton == null)
            {
                Transform t = transform.Find("ResumeButton");
                if (t == null) t = transform.Find("Panel/ResumeButton");
                if (t != null) resumeButton = t.GetComponent<Button>();
            }

            if (restartButton == null)
            {
                Transform t = transform.Find("RestartButton");
                if (t == null) t = transform.Find("Panel/RestartButton");
                if (t != null) restartButton = t.GetComponent<Button>();
            }
        }

        public override void OnShow()
        {
            base.OnShow();
            SetTitle("PAUSED");

            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumePressed);
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartPressed);
        }

        public override void OnHide()
        {
            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(OnResumePressed);
            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartPressed);

            base.OnHide();
        }

        private void OnResumePressed()
        {
            Close();
        }

        private void OnRestartPressed()
        {
            if (popupManager != null)
                popupManager.CloseAllPopups();

            // Reload the current gameplay scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
