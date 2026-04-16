using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3.UI
{
    /// <summary>
    /// Shown when the player runs out of moves or triggers a game-over condition.
    /// Open via: PopupManager.Instance.Show&lt;DefeatPopup&gt;(p => p.SetContent("No moves left!"));
    /// </summary>
    public class DefeatPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI reasonText;
        [SerializeField] private Button retryBtn;

        private System.Action m_OnRetry;

        protected override void Awake()
        {
            base.Awake();
            if (retryBtn) retryBtn.onClick.AddListener(OnRetryClicked);
        }

        /// <param name="reason">Text explaining why the player lost.</param>
        /// <param name="onRetry">Callback when the player taps Retry.</param>
        public void SetContent(string reason, System.Action onRetry = null)
        {
            m_OnRetry = onRetry;
            if (titleText)  titleText.text  = "DERROTA";
            if (reasonText) reasonText.text = reason;
        }

        private void OnRetryClicked()
        {
            m_OnRetry?.Invoke();
            m_OnRetry = null;
            Close();
        }
    }
}
