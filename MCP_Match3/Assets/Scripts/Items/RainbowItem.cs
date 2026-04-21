using UnityEngine;
using Match3.Core;
using Match3.Data;

namespace Match3.Items
{
    /// <summary>
    /// RainbowItem: destroys ALL items of the color it is swapped with.
    /// No color of its own. Created by matching 5 in a line.
    /// </summary>
    public class RainbowItem : Item
    {
        [SerializeField] private Sprite m_SpriteRainbow;

        public override bool Match => false;

        /// <summary>Color of the item that was swapped with this Rainbow.</summary>
        public ColorType TargetColor { get; set; } = ColorType.None;

        public override void SetColor(ColorType color)
        {
            m_Color = ColorType.None;
            gameObject.name = $"Item_{m_ItemType}_Rainbow";
            if (m_Sprite != null && m_SpriteRainbow != null)
                m_Sprite.sprite = m_SpriteRainbow;
        }

        public override void Brust(System.Action onComplete = null)
        {
            ColorType target = TargetColor;
            if (target == ColorType.None && m_MatchMgr != null && m_MatchMgr.m_AppearColor.Count > 0)
                target = m_MatchMgr.m_AppearColor[Random.Range(0, m_MatchMgr.m_AppearColor.Count)];
            if (target == ColorType.None) { onComplete?.Invoke(); return; }

            var boards = m_MatchMgr?.m_ListBoard;
            if (boards == null) { onComplete?.Invoke(); return; }

            if (m_Board != null)
            {
                Board origin = m_Board;
                var targets = new System.Collections.Generic.List<Board>();
                for (int i = 0; i < 81; i++)
                {
                    var bd = boards[i];
                    if (!bd.IsActiveCell || bd.m_Item == null) continue;
                    var it = bd.m_Item as Item;
                    if (it != null && it.m_Color == target)
                        targets.Add(bd);
                }
                if (targets.Count > 0)
                {
                    targets.Sort((a, b) =>
                    {
                        int da = System.Math.Abs(a.X - origin.X) + System.Math.Abs(a.Y - origin.Y);
                        int db = System.Math.Abs(b.X - origin.X) + System.Math.Abs(b.Y - origin.Y);
                        return da.CompareTo(db);
                    });
                    origin.StartCoroutine(Co_PropagateColor(origin, targets));
                }
            }

            StartCoroutine(Co_DestroyAnim(onComplete));
        }

        private static System.Collections.IEnumerator Co_PropagateColor(
            Board origin, System.Collections.Generic.List<Board> targets)
        {
            var wait = new WaitForSeconds(0.06f);
            int lastDist = -1;
            foreach (var bd in targets)
            {
                int dist = System.Math.Abs(bd.X - origin.X) + System.Math.Abs(bd.Y - origin.Y);
                if (dist != lastDist && lastDist >= 0)
                    yield return wait;
                bd.m_isMatchBrust = true;
                lastDist = dist;
            }
        }
    }
}
