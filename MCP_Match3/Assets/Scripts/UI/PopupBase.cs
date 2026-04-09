using UnityEngine;
using UnityEngine.UI;

namespace Match3.UI
{
    /// <summary>
    /// Base class for all popups. Provides common lifecycle, title, and close button handling.
    /// All popups inherit from this and should have a consistent structure:
    /// - Canvas (PopupBase component)
    ///   - Panel (background/content)
    ///     - Title (Text or TextMeshProUGUI)
    ///     - CloseButton (Button)
    ///     - [Custom content for each popup type]
    /// </summary>
    public abstract class PopupBase : MonoBehaviour
    {
        [SerializeField] protected Text titleText;          // Title text component
        [SerializeField] protected Button closeButton;      // Close button

        protected PopupManager popupManager;
        protected Canvas popupCanvas;

        /// <summary>Initialize the popup with a reference to the PopupManager.</summary>
        public virtual void Initialize(PopupManager manager)
        {
            popupManager = manager;
            popupCanvas = GetComponent<Canvas>();

            // Auto-find components if not assigned in inspector
            if (titleText == null)
            {
                titleText = GetComponentInChildren<Text>();
            }

            if (closeButton == null)
            {
                // Look for button named "CloseButton" first
                Transform closeButtonTransform = transform.Find("CloseButton");
                if (closeButtonTransform == null)
                {
                    // Fallback: find first button in children
                    closeButton = GetComponentInChildren<Button>();
                }
                else
                {
                    closeButton = closeButtonTransform.GetComponent<Button>();
                }
            }

            // Wire up close button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
        }

        /// <summary>Set the title of the popup.</summary>
        public virtual void SetTitle(string title)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }
        }

        /// <summary>Called when the popup is shown.</summary>
        public virtual void OnShow() { }

        /// <summary>Called when the popup is hidden.</summary>
        public virtual void OnHide() { }

        /// <summary>Close this popup.</summary>
        public virtual void Close()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
            }

            if (popupManager != null)
            {
                popupManager.ClosePopup(this);
            }
        }
    }
}

