using UnityEngine;
using Match3.Core;
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
            // Propagate outward up and down from the origin with a delay per step
            m_Board.StartCoroutine(Co_PropagateCol(m_Board));
            StartCoroutine(Co_DestroyAnim(onComplete));
        }

        private static System.Collections.IEnumerator Co_PropagateCol(Board origin)
        {
            var wait = new WaitForSeconds(0.06f);
            Board top    = origin.Top;
            Board bottom = origin.Bottom;
            while (top != null || bottom != null)
            {
                yield return wait;
                if (top != null && top.IsActiveCell)
                { top.m_isMatchBrust = true; top = top.Top; }
                else top = null;
                if (bottom != null && bottom.IsActiveCell)
                { bottom.m_isMatchBrust = true; bottom = bottom.Bottom; }
                else bottom = null;
            }
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
