using UnityEngine;
using TMPro;

namespace Match3.UI
{
    public class ScoreDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;

        public void UpdateScore(int score)
        {
            if (scoreText) scoreText.text = "Score: " + score;
        }
    }
}
