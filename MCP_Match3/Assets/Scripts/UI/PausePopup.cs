using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Match3.UI
{
    /// <summary>
    /// Pause popup: shown during gameplay when the player hits the pause button.
    /// Pauses time on open, restores it on close.
    /// </summary>
    public class PausePopup : PopupBase
    {
        [SerializeField] private Button resumeBtn;
        [SerializeField] private Button restartBtn;

        protected override void Awake()
        {
            base.Awake();
            if (resumeBtn)  resumeBtn.onClick.AddListener(Close);
            if (restartBtn) restartBtn.onClick.AddListener(Restart);
        }

        protected override void OnShow()
        {
            Time.timeScale = 0f;
        }

        protected override void OnClose()
        {
            Time.timeScale = 1f;
        }

        private void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
