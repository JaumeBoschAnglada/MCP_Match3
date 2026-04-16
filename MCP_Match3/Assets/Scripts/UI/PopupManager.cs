using System;
using System.Collections.Generic;
using UnityEngine;

namespace Match3.UI
{
    /// <summary>
    /// Singleton that owns and stacks all in-game popups.
    ///
    /// Usage:
    ///   PopupManager.Instance.Show<VictoryPopup>(p => p.SetContent(...), onClosed: () => ...);
    ///   PopupManager.Instance.CloseTop();
    ///   PopupManager.Instance.CloseAll();
    ///
    /// All PopupBase instances that are children of this GameObject are
    /// auto-registered on Awake. Popups start hidden (SetActive false).
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        public static PopupManager Instance { get; private set; }

        private readonly Stack<PopupBase> m_Stack = new Stack<PopupBase>();
        private readonly Dictionary<Type, PopupBase> m_Registry = new Dictionary<Type, PopupBase>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            foreach (var popup in GetComponentsInChildren<PopupBase>(includeInactive: true))
            {
                Register(popup);
                popup.gameObject.SetActive(false);
            }
        }

        public void Register(PopupBase popup)
        {
            var t = popup.GetType();
            if (!m_Registry.ContainsKey(t))
                m_Registry[t] = popup;
        }

        public T Show<T>(Action<T> setup = null, Action onClosed = null) where T : PopupBase
        {
            if (!m_Registry.TryGetValue(typeof(T), out var popup))
            {
                Debug.LogWarning($"[PopupManager] Popup of type {typeof(T).Name} not registered.");
                return default;
            }

            var typed = popup as T;
            setup?.Invoke(typed);
            m_Stack.Push(popup);

            // Inject a combined callback: pop from stack + user callback
            popup.Show(() =>
            {
                PopFromStack(popup);
                onClosed?.Invoke();
            });

            return typed;
        }

        public void CloseTop()
        {
            if (m_Stack.Count > 0)
                m_Stack.Peek().Close();
        }

        public void CloseAll()
        {
            while (m_Stack.Count > 0)
                m_Stack.Pop().Close();
        }

        public bool IsAnyOpen => m_Stack.Count > 0;

        public bool IsOpen<T>() where T : PopupBase
            => m_Registry.TryGetValue(typeof(T), out var p) && p.gameObject.activeSelf;

        private void PopFromStack(PopupBase popup)
        {
            if (m_Stack.Count == 0) return;
            if (m_Stack.Peek() == popup) { m_Stack.Pop(); return; }

            var temp = new List<PopupBase>(m_Stack);
            temp.Reverse();
            m_Stack.Clear();
            foreach (var p in temp)
                if (p != popup) m_Stack.Push(p);
        }
    }
}
