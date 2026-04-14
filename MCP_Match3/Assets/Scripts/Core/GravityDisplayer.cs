using UnityEngine;
using Match3.Data;

namespace Match3.Core
{
    public class GravityDisplayer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_Arrow;

        private Board m_Board;

        public void Init(Board board)
        {
            m_Board = board;
            Hide();
        }

        public void Show()
        {
            if (m_Arrow == null || m_Board == null) return;

            m_Arrow.gameObject.SetActive(true);

            float angle = 0f;
            switch (m_Board.CurrentDropDir)
            {
                case DROP_DIR.U: angle = 0f; break;
                case DROP_DIR.D: angle = 180f; break;
                case DROP_DIR.L: angle = 90f; break;
                case DROP_DIR.R: angle = -90f; break;
            }
            m_Arrow.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        public void Hide()
        {
            if (m_Arrow != null)
                m_Arrow.gameObject.SetActive(false);
        }
    }
}
