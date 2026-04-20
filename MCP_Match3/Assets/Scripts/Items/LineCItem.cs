using UnityEngine;
using Match3.Data;

namespace Match3.Items
{
    /// <summary>
    /// LineCItem (Diagonal Cross): destroys both diagonals (X shape) when burst.
    /// Created by matching an L or T shape.
    /// </summary>
    public class LineCItem : Item
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
            
            // Main diagonal: top-left to bottom-right (\)
            var diag1 = m_Board;
            // Move to top-left corner of diagonal
            while (diag1.Top != null && diag1.Top.IsActiveCell && diag1.Left != null && diag1.Left.IsActiveCell)
                diag1 = diag1.Top.Left;
            
            var cur = diag1;
            while (cur != null && cur.IsActiveCell)
            {
                cur.m_isMatchBrust = true;
                // Move down-right
                if (cur.Right != null && cur.Right.IsActiveCell && cur.Bottom != null && cur.Bottom.IsActiveCell)
                    cur = cur.Bottom.Right;
                else
                    break;
            }

            // Anti-diagonal: top-right to bottom-left (/)
            var diag2 = m_Board;
            // Move to top-right corner of diagonal
            while (diag2.Top != null && diag2.Top.IsActiveCell && diag2.Right != null && diag2.Right.IsActiveCell)
                diag2 = diag2.Top.Right;
            
            cur = diag2;
            while (cur != null && cur.IsActiveCell)
            {
                cur.m_isMatchBrust = true;
                // Move down-left
                if (cur.Left != null && cur.Left.IsActiveCell && cur.Bottom != null && cur.Bottom.IsActiveCell)
                    cur = cur.Bottom.Left;
                else
                    break;
            }

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
