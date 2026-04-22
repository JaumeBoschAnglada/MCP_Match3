using UnityEngine;
using Match3.Data;

namespace Match3.Panels
{
    /// <summary>
    /// Base class for all board-cell modifiers (panels).
    /// Panels sit on top of Board cells and modify how items interact with them.
    /// Multiple panels can stack on a single cell (m_ListPanel).
    /// </summary>
    public abstract class Panel : MonoBehaviour
    {
        public const int BackgroundSortingOrder = 0;
        public const int OverlaySortingOrder = 20;


        // === Data ===
        public PanelType m_PanelType  { get; protected set; }
        public int       Defence      { get; protected set; } = 0;  // Hits needed to destroy
        public int       Value        { get; protected set; } = 0;  // Generic value (layer count, etc.)
        public string    addData      { get; protected set; } = ""; // Extra serialized data

        // === Board reference ===
        public Core.Board m_Board { get; set; }

        public virtual int VisualSortingOrder => BackgroundSortingOrder;
        public virtual bool BlocksItemSwitch => false;
        public virtual bool BlocksGravity => false;

        public void ApplyVisualSorting()
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.sortingOrder = VisualSortingOrder;
            }
        }



        // ── Virtual lifecycle ─────────────────────────────────────────

        /// <summary>Called when a new item is placed on this cell.</summary>
        public virtual void ItemExist()  { }

        /// <summary>Called when an item drops through/into this cell.</summary>
        public virtual void ItemDrop()   { }

        /// <summary>Called when an item on this cell is matched.</summary>
        public virtual void ItemMatch()  { }

        /// <summary>Called when an item on this cell is swapped.</summary>
        public virtual void ItemSwitch() { }

        /// <summary>Called by PanelManager immediately after m_Board is assigned. Use to set board flags.</summary>
        public virtual void OnPlaced()   { }

        /// <summary>
        /// Called when an adjacent match hits this panel (or directly when matched).
        /// Reduces Defence; destroys self when it reaches 0.
        /// Returns true if the panel was destroyed this hit.
        /// </summary>
        public virtual bool Brust()
        {
            if (Defence <= 0)
            {
                DestroyPanel();
                return true;
            }
            Defence--;
            OnDamage();
            if (Defence <= 0)
            {
                DestroyPanel();
                return true;
            }
            return false;
        }

        // ── Overridable hooks ─────────────────────────────────────────

        /// <summary>Called each time Defence decreases but the panel survives.</summary>
        protected virtual void OnDamage() { }

        /// <summary>Called just before the panel is removed from the board.</summary>
        protected virtual void OnDestroyed() { }

        // ── Internal destroy ─────────────────────────────────────────

        protected void DestroyPanel()
        {
            OnDestroyed();
            if (m_Board != null)
                m_Board.m_ListPanel.Remove(this);
            Managers.PanelManager.Instance?.RestorePanel(gameObject);
        }
    }
}
