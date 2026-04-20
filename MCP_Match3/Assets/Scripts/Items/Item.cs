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

        // === Burst animation ===
        /// <summary>World position to move toward during destroy animation (set by MatchManager for fusion effect).</summary>
        public Vector3? m_BurstFusionTarget { get; set; }

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
            transform.localScale = Vector3.one;  // reset after pool reuse
            m_BurstFusionTarget = null;

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
        /// Plays a scale-to-zero animation; if m_BurstFusionTarget is set, also moves toward that position.
        /// </summary>
        public virtual void Brust(Action onComplete = null)
        {
            StartCoroutine(Co_DestroyAnim(onComplete));
        }

        /// <summary>
        /// Scale this item from 1 to 0 over a short duration, optionally sliding toward a fusion target.
        /// </summary>
        protected System.Collections.IEnumerator Co_DestroyAnim(Action onComplete)
        {
            Vector3? fusionTarget = m_BurstFusionTarget;
            m_BurstFusionTarget = null;

            float duration = 0.25f;
            float elapsed = 0f;
            Vector3 startPos = transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (fusionTarget.HasValue)
                    transform.position = Vector3.Lerp(startPos, fusionTarget.Value, t);
                else
                    transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
                yield return null;
            }

            if (!fusionTarget.HasValue)
                transform.localScale = Vector3.zero;
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
