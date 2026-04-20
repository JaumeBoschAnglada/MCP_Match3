using UnityEngine;
using Match3.Data;

namespace Match3.Items
{
    /// <summary>
    /// LineY item: destroys the entire column it is in when burst.
    /// Created by matching 4 in a horizontal line.
    /// </summary>
    public class LineYItem : Item
    {
        [Header("Color Sprites")]
        [SerializeField] private Sprite m_RedSprite;
        [SerializeField] private Sprite m_YellowSprite;
        [SerializeField] private Sprite m_GreenSprite;
        [SerializeField] private Sprite m_BlueSprite;
        [SerializeField] private Sprite m_PurpleSprite;
        [SerializeField] private Sprite m_OrangeSprite;

        public override bool Match => false;

        public override void SetColor(ColorType color)
        {
            m_Color = color;
            gameObject.name = $"Item_{m_ItemType}_{color}";
            if (m_Sprite == null) return;
            Sprite s = GetSprite(color);
            if (s != null) m_Sprite.sprite = s;
        }

        public override void Brust(System.Action onComplete = null)
        {
            if (m_Board == null) { onComplete?.Invoke(); return; }
            // Walk to top, mark entire column
            var top = m_Board;
            while (top.Top != null && top.Top.IsActiveCell) top = top.Top;
            var cur = top;
            while (cur != null && cur.IsActiveCell) { cur.m_isMatchBrust = true; cur = cur.Bottom; }
            StartCoroutine(Co_DestroyAnim(onComplete));
        }

        private Sprite GetSprite(ColorType c)
        {
            switch (c)
            {
                case ColorType.RED:    return m_RedSprite;
                case ColorType.YELLOW: return m_YellowSprite;
                case ColorType.GREEN:  return m_GreenSprite;
                case ColorType.BLUE:   return m_BlueSprite;
                case ColorType.PURPLE: return m_PurpleSprite;
                case ColorType.ORANGE: return m_OrangeSprite;
                default: return null;
            }
        }
    }
}
