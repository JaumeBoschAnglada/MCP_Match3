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

        public override void ItemSwitch()
        {
            // Caged items cannot be swapped — consume the attempt as a hit
            Brust();
        }

        protected override void OnDamage()
        {
            RefreshSprite();
        }

        private void RefreshSprite()
        {
            if (!m_Sprite) return;
            m_Sprite.sprite = Defence >= 2 ? m_Layer2Sprite : m_Layer1Sprite;
        }
    }
}
