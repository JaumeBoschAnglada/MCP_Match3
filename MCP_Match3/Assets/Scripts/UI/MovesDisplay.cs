using UnityEngine;
using TMPro;

namespace Match3.UI
{
    public class MovesDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI movesText;

        public void UpdateMoves(int remaining)
        {
            if (!movesText) return;
            movesText.text = "Moves: " + remaining;
            movesText.color = remaining <= 5 ? Color.red : (remaining <= 10 ? Color.yellow : Color.white);
        }
    }
}
