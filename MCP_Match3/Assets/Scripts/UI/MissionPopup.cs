using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3.UI
{
    /// <summary>
    /// Pre-game popup that shows the level's missions before the player starts.
    /// Open via: PopupManager.Instance.Show&lt;MissionPopup&gt;(p => p.SetContent("Level 1", desc, onStart));
    /// </summary>
    public class MissionPopup : PopupBase
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descText;
        [SerializeField] private Button startBtn;

        private System.Action m_OnStart;

        protected override void Awake()
        {
            base.Awake();
            if (startBtn) startBtn.onClick.AddListener(OnStartClicked);
        }

        /// <param name="title">Level name / number.</param>
        /// <param name="description">Mission description text.</param>
        /// <param name="onStart">Callback when the player taps Play.</param>
        public void SetContent(string title, string description, System.Action onStart = null)
        {
            m_OnStart = onStart;
            if (titleText) titleText.text = title;
            if (descText)  descText.text  = description;
        }

        private void OnStartClicked()
        {
            m_OnStart?.Invoke();
            m_OnStart = null;
            Close();
        }
    }
}
