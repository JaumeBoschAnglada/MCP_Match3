using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3.UI
{
    /// <summary>
    /// Popup that shows all available levels as numbered buttons in a scrollable grid.
    /// Scans Resources/Levels/ for JSON files numbered 1..N.
    /// Open via: PopupManager.Instance.Show&lt;LevelListPopup&gt;();
    /// </summary>
    public class LevelListPopup : PopupBase
    {
        [Header("Level List")]
        [SerializeField] private Transform  buttonContainer;   // Content inside ScrollView
        [SerializeField] private GameObject levelButtonPrefab;  // Prefab: Button + TMP child

        protected override void OnShow()
        {
            BuildList();
        }

        private void BuildList()
        {
            // Clear previous buttons
            for (int i = buttonContainer.childCount - 1; i >= 0; i--)
                Destroy(buttonContainer.GetChild(i).gameObject);

            // Create one button per level found in Resources/Levels/
            for (int i = 1; i <= 9999; i++)
            {
                var asset = Resources.Load<TextAsset>("Levels/" + i);
                if (asset == null) break;

                int levelIndex = i;
                var go  = Instantiate(levelButtonPrefab, buttonContainer);
                var btn = go.GetComponent<Button>();
                var lbl = go.GetComponentInChildren<TextMeshProUGUI>();

                if (lbl != null) lbl.text = levelIndex.ToString();
                if (btn != null) btn.onClick.AddListener(() => OnLevelClicked(levelIndex));
            }
        }

        private void OnLevelClicked(int index)
        {
            Close();
            Core.MatchManager.Instance?.LoadLevel(index);
        }
    }
}
