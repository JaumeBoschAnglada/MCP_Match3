using System;
using UnityEngine;
using UnityEngine.UI;

namespace Match3.UI
{
    /// <summary>
    /// Base class for all popups.
    /// Expected hierarchy on the prefab:
    ///   Root (CanvasGroup + this component)
    ///     ├── Backdrop   (Image – semi-transparent overlay)
    ///     └── Panel      (RectTransform – the visible card)
    ///          ├── TitleBar
    ///          │    ├── TitleText  (TMP – optional)
    ///          │    └── CloseButton (Button – optional)
    ///          └── Body  (content injected by subclasses via [SerializeField])
    /// </summary>
    public abstract class PopupBase : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected Button closeButton;

        private Action m_OnClosed;

        // ── Lifecycle ────────────────────────────────────────────────

        protected virtual void Awake()
        {
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
            if (closeButton)  closeButton.onClick.AddListener(Close);
        }

        /// <summary>Show this popup and register an optional callback for when it closes.</summary>
        public virtual void Show(Action onClosed = null)
        {
            m_OnClosed = onClosed;
            gameObject.SetActive(true);
            if (canvasGroup)
            {
                canvasGroup.alpha          = 1f;
                canvasGroup.interactable   = true;
                canvasGroup.blocksRaycasts = true;
            }
            OnShow();
        }

        /// <summary>Close this popup and fire the onClosed callback.</summary>
        public virtual void Close()
        {
            OnClose();
            gameObject.SetActive(false);
            m_OnClosed?.Invoke();
            m_OnClosed = null;
        }

        // ── Overridable hooks ─────────────────────────────────────────

        /// <summary>Called right after the popup becomes visible. Override for entry animations, etc.</summary>
        protected virtual void OnShow() { }

        /// <summary>Called right before the popup is hidden. Override for exit animations, etc.</summary>
        protected virtual void OnClose() { }
    }
}
