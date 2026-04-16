using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Match3.Data;

namespace Match3.UI
{
    public class MissionDisplay : MonoBehaviour
    {
        [SerializeField] private Image missionIcon;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private CanvasGroup canvasGroup;
        private MissionKind missionKind;
        private int targetCount;

        public void SetMission(MissionData data)
        {
            missionKind = data.kind;
            targetCount = data.count;
            gameObject.SetActive(true);
            if (canvasGroup) canvasGroup.alpha = 1f;
            UpdateCount(0);
        }

        public void SetEmpty()
        {
            gameObject.SetActive(false);
            if (canvasGroup) canvasGroup.alpha = 0f;
        }

        public void UpdateCount(int current)
        {
            if (countText) countText.text = current + "/" + targetCount;
        }

        public MissionKind Kind => missionKind;
    }
}
