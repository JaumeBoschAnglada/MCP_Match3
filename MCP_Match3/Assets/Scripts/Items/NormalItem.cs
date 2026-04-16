using UnityEngine;
using Match3.Data;

namespace Match3.Items
{
    /// <summary>
    /// Normal colored piece - the basic match-3 item.
    /// Can be matched with 2+ adjacent items of the same color.
    /// </summary>
    public class NormalItem : Item
    {
        // === Color sprite mapping (assign in prefab or via ItemManager) ===
        [Header("Color Sprites")]
        [SerializeField] private Sprite m_RedSprite;
        [SerializeField] private Sprite m_YellowSprite;
        [SerializeField] private Sprite m_GreenSprite;
        [SerializeField] private Sprite m_BlueSprite;
        [SerializeField] private Sprite m_PurpleSprite;
        [SerializeField] private Sprite m_OrangeSprite;

        /// <summary>
        /// Set the visual sprite based on color.
        /// </summary>
        public override void SetColor(ColorType color)
        {
            m_Color = color;
            gameObject.name = $"Item_{m_ItemType}_{color}";

            if (m_Sprite == null)
            {
                Debug.LogWarning($"[NormalItem] SpriteRenderer not assigned on {gameObject.name}");
                return;
            }

            Sprite targetSprite = GetSpriteForColor(color);
            if (targetSprite != null)
                m_Sprite.sprite = targetSprite;
            else if (color != ColorType.None)
                Debug.LogWarning($"[NormalItem] No sprite assigned for color {color}");
        }

        /// <summary>
        /// Get the sprite for a given color.
        /// </summary>
        private Sprite GetSpriteForColor(ColorType color)
        {
            switch (color)
            {
                case ColorType.RED: return m_RedSprite;
                case ColorType.YELLOW: return m_YellowSprite;
                case ColorType.GREEN: return m_GreenSprite;
                case ColorType.BLUE: return m_BlueSprite;
                case ColorType.PURPLE: return m_PurpleSprite;
                case ColorType.ORANGE: return m_OrangeSprite;
                default: return null;
            }
        }
    }
}
