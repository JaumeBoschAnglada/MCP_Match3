using UnityEngine;
using Match3.Data;

namespace Match3.Panels
{
    /// <summary>
    /// Lolly cage placeholder for the panel system.
    /// Behaves like a layered cage until its dedicated mechanics are implemented.
    /// </summary>
    public class LollyCagePanel : Panel
    {
        public override int VisualSortingOrder => OverlaySortingOrder;
        public override bool BlocksItemSwitch => true;
        public override bool BlocksGravity => m_Board != null && m_Board.m_Item != null;

        [SerializeField] private SpriteRenderer m_Sprite;
        [SerializeField] private Sprite m_Layer1Sprite;
        [SerializeField] private Sprite m_Layer2Sprite;

        private void Awake()
        {
            m_PanelType = PanelType.Lolly_Cage;
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
                if (panel != null && panel != this && IsCagePanel(panel.m_PanelType))
                {
                    hasAnotherCage = true;
                    break;
                }
            }

            m_Board.IsPanelCage = hasAnotherCage;
        }

        private void RefreshSprite()
        {
            if (!m_Sprite)
                return;

            m_Sprite.sprite = Defence >= 2 ? m_Layer2Sprite : m_Layer1Sprite;
        }

        private static bool IsCagePanel(PanelType panelType)
        {
            return panelType == PanelType.Ice_Cage
                || panelType == PanelType.Lolly_Cage
                || panelType == PanelType.Bottle_Cage;
        }
    }
}