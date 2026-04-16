using UnityEngine;
using UnityEngine.UI;

namespace Match3.UI
{
    /// <summary>
    /// Attach to the in-game pause button. Opens PausePopup via PopupManager.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class PauseButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            PopupManager.Instance?.Show<PausePopup>();
        }
    }
}
