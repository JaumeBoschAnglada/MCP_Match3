using UnityEngine;
using Match3.Core;
using Match3.Data;

namespace Match3.Items
{
    /// <summary>
    /// LineX item: destroys the entire row it is in when burst.
    /// Created by matching 4 in a vertical line.
    /// </summary>
    public class LineXItem : Item
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
            // Propagate outward left and right from the origin with a delay per step
            m_Board.StartCoroutine(Co_PropagateRow(m_Board));
            StartCoroutine(Co_DestroyAnim(onComplete));
        }

        private static System.Collections.IEnumerator Co_PropagateRow(Board origin)
        {
            var wait = new WaitForSeconds(0.06f);
            Board left  = origin.Left;
            Board right = origin.Right;
            while (left != null || right != null)
            {
                yield return wait;
                if (left != null && left.IsActiveCell)
                { left.m_isMatchBrust = true; left = left.Left; }
                else left = null;
                if (right != null && right.IsActiveCell)
                { right.m_isMatchBrust = true; right = right.Right; }
                else right = null;
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
