using UnityEngine;
using UnityEngine.SceneManagement;

namespace Match3.Scenes
{
    /// <summary>
    /// Simple scene manager for the Hall (main menu) scene.
    /// Provides methods to load the Gameplay scene with a specific level.
    /// </summary>
    public class HallManager : MonoBehaviour
    {
        public static HallManager Instance { get; private set; }

        [SerializeField] private string gameplaySceneName = "Gameplay";

        private int selectedLevel = 1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        /// <summary>
        /// Load the gameplay scene with the specified level number.
        /// The level number is stored statically so MatchManager can read it on scene load.
        /// </summary>
        public void LoadLevel(int levelNumber)
        {
            selectedLevel = levelNumber;
            CurrentLevel = levelNumber;
            SceneManager.LoadScene(gameplaySceneName);
        }

        /// <summary>
        /// Static accessor for the level number to load. Set before scene transition,
        /// read by MatchManager on Gameplay scene load.
        /// </summary>
        public static int CurrentLevel { get; private set; } = 1;

        /// <summary>
        /// Convenience method for UI buttons: load level 1.
        /// </summary>
        public void OnPlayPressed()
        {
            LoadLevel(1);
        }
    }
}
