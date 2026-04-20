using UnityEngine;
using Match3.Data;

namespace Match3.Panels
{
    /// <summary>
    /// Bottle cage placeholder for the panel system.
    /// In the full game it is opened by keys; for now it behaves as a cage
    /// and exposes layered defence for later phases.
    /// </summary>
    public class BottleCagePanel : Panel
    {
        public override int VisualSortingOrder => OverlaySortingOrder;
        public override bool BlocksItemSwitch => true;
        public override bool BlocksGravity => m_Board != null && m_Board.m_Item != null;

        [SerializeField] private SpriteRenderer m_Sprite;
        [SerializeField] private Sprite m_Layer1Sprite;
        [SerializeField] private Sprite m_Layer2Sprite;
        [SerializeField] private Sprite m_Layer3Sprite;

        private void Awake()
        {
            m_PanelType = PanelType.Bottle_Cage;
            Defence = 2;
        }

        public void SetLayers(int layers)
        {
            Defence = Mathf.Clamp(layers, 1, 3);
            RefreshSprite();
        }

        public void BottleBrust()
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

            if (Defence >= 3 && m_Layer3Sprite != null)
                m_Sprite.sprite = m_Layer3Sprite;
            else if (Defence >= 2 && m_Layer2Sprite != null)
                m_Sprite.sprite = m_Layer2Sprite;
            else if (m_Layer1Sprite != null)
                m_Sprite.sprite = m_Layer1Sprite;
        }

        private static bool IsCagePanel(PanelType panelType)
        {
            return panelType == PanelType.Ice_Cage
                || panelType == PanelType.Lolly_Cage
                || panelType == PanelType.Bottle_Cage;
        }
    }
}