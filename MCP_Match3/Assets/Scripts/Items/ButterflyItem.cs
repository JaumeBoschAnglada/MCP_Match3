using UnityEngine;
using Match3.Data;

namespace Match3.Items
{
    /// <summary>
    /// Butterfly item: flies to a random item of the same color and destroys it.
    /// Created by matching a 2×2 square.
    /// </summary>
    public class ButterflyItem : Item
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
            if (m_Board == null || m_MatchMgr == null) { onComplete?.Invoke(); return; }

            // Find a random board with an item of the same color
            var boards = m_MatchMgr.m_ListBoard;
            var candidates = new System.Collections.Generic.List<int>();

            for (int i = 0; i < 81; i++)
            {
                var b = boards[i];
                if (!b.IsActiveCell || b.m_Item == null) continue;
                if (b == m_Board) continue; // skip self
                var targetItem = b.m_Item as Item;
                if (targetItem != null && targetItem.m_Color == m_Color)
                    candidates.Add(i);
            }

            if (candidates.Count > 0)
            {
                int targetIdx = candidates[Random.Range(0, candidates.Count)];
                boards[targetIdx].m_isMatchBrust = true;
            }

            onComplete?.Invoke();
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
