using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3.UI
{
    /// <summary>
    /// Shown when the player completes all missions.
    /// Open via: PopupManager.Instance.Show&lt;VictoryPopup&gt;(p => p.SetContent(score, stars));
    /// </summary>
    public class VictoryPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button nextBtn;
        [SerializeField] private Image[] stars = new Image[3];

        private System.Action m_OnNext;

        protected override void Awake()
        {
            base.Awake();
            if (nextBtn) nextBtn.onClick.AddListener(OnNextClicked);
        }

        /// <param name="score">Final score to display.</param>
        /// <param name="starCount">Stars earned (0–3).</param>
        /// <param name="onNext">Callback when the player taps Next.</param>
        public void SetContent(int score, int starCount, System.Action onNext = null)
        {
            m_OnNext = onNext;

            if (scoreText) scoreText.text = $"Score: {score}";
            if (titleText) titleText.text = "¡VICTORIA!";

            for (int i = 0; i < stars.Length; i++)
                if (stars[i]) stars[i].color = i < starCount ? Color.yellow : Color.gray;
        }

        private void OnNextClicked()
        {
            m_OnNext?.Invoke();
            m_OnNext = null;
            Close();
        }
    }
}
