using System;
using UnityEngine;
using Match3.Core;
using Match3.Data;

namespace Match3.Items
{
    /// <summary>
    /// Base class for all match-3 items (normal pieces, special pieces, etc.).
    /// Handles input (drag-to-swap), visual representation, and burst/match behavior.
    /// </summary>
    public abstract class Item : MonoBehaviour
    {
        public const int VisualSortingOrder = 10;

        // === Static swap tracking ===
        public static Item Swap_A;
        public static Item Swap_B;
        public static bool SwitchStart;
        public static bool SwitchingTouch;

        // === Item properties ===
        public ColorType m_Color { get; protected set; } = ColorType.None;
        public ItemType m_ItemType { get; protected set; } = ItemType.None;
        public CombineType m_CombineType { get; protected set; } = CombineType.None;

        // === References ===
        public Board m_Board { get; set; }
        protected MatchManager m_MatchMgr;

        // === Visual ===
        [SerializeField] protected SpriteRenderer m_Sprite;

        // === Abstract/Virtual properties ===
        public virtual bool Match => true;
        public virtual bool Switch => true;
        public virtual bool Drop => true;
        public virtual bool m_Brust => true;
        public virtual bool ArountBrust => false;
        public virtual bool HaveNextItem => false;

        protected virtual void Awake()
        {
            m_MatchMgr = MatchManager.Instance;
            if (m_Sprite == null)
                m_Sprite = GetComponentInChildren<SpriteRenderer>();
        }

        /// <summary>
        /// Initialize item with type and color.
        /// </summary>
        public virtual void Init(ItemType itemType, ColorType color)
        {
            // Lazy-init: items from the pool may have been created before MatchManager existed
            if (m_MatchMgr == null)
                m_MatchMgr = MatchManager.Instance;

            m_ItemType = itemType;
            m_Color = color;
            m_CombineType = CombineType.None;

            gameObject.name = $"Item_{itemType}_{color}";
            ApplyVisualSorting();
            SetColor(color);
        }

        public void ApplyVisualSorting()
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.sortingOrder = VisualSortingOrder;
            }
        }

        /// <summary>
        /// Set the visual color/sprite based on ColorType.
        /// </summary>
        public abstract void SetColor(ColorType color);

        /// <summary>
        /// Set a random color from the level's available colors.
        /// </summary>
        public virtual void SetColorRandom()
        {
            // Lazy-init: items from the pool may have been created before MatchManager existed
            if (m_MatchMgr == null)
                m_MatchMgr = MatchManager.Instance;

            if (m_MatchMgr == null || m_MatchMgr.m_AppearColor == null || m_MatchMgr.m_AppearColor.Count == 0)
            {
                Debug.LogWarning("[Item] No appear colors available for random color.");
                return;
            }

            ColorType randomColor = m_MatchMgr.GetRandomColor();
            SetColor(randomColor);
        }

        // NOTE: Input is now handled by InputManager (supports mouse + touch)
        // These OnMouse* methods are kept for backward compatibility but not used

        /*
        /// <summary>
        /// Mouse down: Start swap tracking (desktop input).
        /// DEPRECATED: Use InputManager instead for cross-platform support.
        /// </summary>
        protected virtual void OnMouseDown()
        {
            if (!CanStartSwap()) return;

            Swap_A = this;
            SwitchStart = true;
        }

        /// <summary>
        /// Mouse enter: If dragging, set Swap_B and trigger swap (desktop input).
        /// DEPRECATED: Use InputManager instead for cross-platform support.
        /// </summary>
        protected virtual void OnMouseEnter()
        {
            if (!SwitchStart) return;
            if (Swap_A == null || Swap_A == this) return;
            if (!CheckNeighbor(this)) return;

            Swap_B = this;
            SwitchStart = false;
            SwitchingTouch = true;

            m_MatchMgr.Switching(Swap_A, Swap_B);
        }

        /// <summary>
        /// Mouse up: Cancel swap tracking.
        /// DEPRECATED: Use InputManager instead for cross-platform support.
        /// </summary>
        protected virtual void OnMouseUp()
        {
            SwitchStart = false;
        }
        */

        /// <summary>
        /// Check if we can start a swap interaction.
        /// </summary>
        protected virtual bool CanStartSwap()
        {
            if (m_MatchMgr == null) return false;
            if (m_MatchMgr.m_MatchState != MatchState.Playing) return false;
            if (m_MatchMgr.m_StepType != StepType.Wait) return false;
            if (SwitchStart) return false;
            if (m_Board == null || !m_Board.IsActiveCell) return false;

            return true;
        }

        /// <summary>
        /// Check if the given item is a neighbor (4 cardinal directions).
        /// </summary>
        protected virtual bool CheckNeighbor(Item other)
        {
            if (Swap_A == null || Swap_A.m_Board == null) return false;
            if (other == null || other.m_Board == null) return false;

            Board boardA = Swap_A.m_Board;
            Board boardB = other.m_Board;

            return boardA.Top == boardB
                || boardA.Bottom == boardB
                || boardA.Left == boardB
                || boardA.Right == boardB;
        }

        /// <summary>
        /// Burst/destroy this item with visual effect.
        /// </summary>
        public virtual void Brust(Action onComplete = null)
        {
            // TODO: Add particle effects, animations, sound
            onComplete?.Invoke();
        }

        /// <summary>
        /// Burst caused by combine/special activation.
        /// </summary>
        public virtual void CombineBrust()
        {
            Brust();
        }

        /// <summary>
        /// Check if this item should combine into a special piece.
        /// </summary>
        public virtual void CheckCombine()
        {
            // Override in special items
        }

        /// <summary>
        /// Apply mission progress (collect goals, etc.).
        /// </summary>
        public virtual void MissionApply()
        {
            Managers.MissionManager.Instance?.MissionApply(m_ItemType, m_Color);
        }

        /// <summary>
        /// Get score for destroying this item.
        /// </summary>
        public virtual int GetDestroyScore(int combo)
        {
            // Base score formula
            int baseScore = 10;
            int comboMultiplier = 1 + combo;
            return baseScore * comboMultiplier;
        }

        /// <summary>
        /// Reset static swap tracking (call between levels/scenes).
        /// </summary>
        public static void ResetSwapTracking()
        {
            Swap_A = null;
            Swap_B = null;
            SwitchStart = false;
            SwitchingTouch = false;
        }
    }
}
