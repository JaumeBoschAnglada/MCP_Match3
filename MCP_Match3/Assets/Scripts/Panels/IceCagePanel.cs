using UnityEngine;
using Match3.Data;

namespace Match3.Panels
{
    /// <summary>
    /// Ice cage enclosing an item. Takes 1 or 2 hits to destroy (Defence = 1 or 2).
    /// Destroyed by a match on this cell or an adjacent burst.
    /// While active: ItemSwitch() is blocked (item cannot be swapped out).
    /// </summary>
    public class IceCagePanel : Panel
    {
        public override int VisualSortingOrder => OverlaySortingOrder;
        public override bool BlocksItemSwitch => true;
        public override bool BlocksGravity => m_Board != null && m_Board.m_Item != null;

        [SerializeField] private SpriteRenderer m_Sprite;
        [SerializeField] private Sprite m_Layer1Sprite;
        [SerializeField] private Sprite m_Layer2Sprite;

        private void Awake()
        {
            m_PanelType = PanelType.Ice_Cage;
            Defence = 1;
        }

        public void SetLayers(int layers)
        {
            Defence = Mathf.Clamp(layers, 1, 2);
            RefreshSprite();
        }

        public override void ItemMatch()
        {
            Brust();
        }

        public override void ItemSwitch()
        {
            // Caged items block swapping until the cage is destroyed by a match/burst.
        }

        protected override void OnDamage()
        {
            RefreshSprite();
        }

        protected override void OnDestroyed()
        {
            if (m_Board == null)
                return;

            bool hasAnotherCage = false;
            for (int i = 0; i < m_Board.m_ListPanel.Count; i++)
            {
                var panel = m_Board.m_ListPanel[i];
                if (panel != null && panel != this && panel.m_PanelType == PanelType.Ice_Cage)
                {
                    hasAnotherCage = true;
                    break;
                }
            }

            m_Board.IsPanelCage = hasAnotherCage;
        }

        private void RefreshSprite()
        {
            if (!m_Sprite) return;
            m_Sprite.sprite = Defence >= 2 ? m_Layer2Sprite : m_Layer1Sprite;
        }
    }
}
