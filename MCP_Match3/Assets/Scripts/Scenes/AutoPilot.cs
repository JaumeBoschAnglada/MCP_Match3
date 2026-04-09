using UnityEngine;
using UnityEngine.SceneManagement;

namespace Match3.Scenes
{
    /// <summary>
    /// Autopilot: automatically navigates through the game for testing.
    /// Attach to a GameObject in the Hall scene.
    /// First action: auto-enter the Gameplay level after a short delay.
    /// </summary>
    public class AutoPilot : MonoBehaviour
    {
        [SerializeField] private float autoPlayDelay = 1.5f;
        [SerializeField] private bool autoPlayEnabled = true;

        private float timer;

        private void Start()
        {
            timer = autoPlayDelay;
        }

        private void Update()
        {
            if (!autoPlayEnabled) return;

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                autoPlayEnabled = false;
                Debug.Log("[AutoPilot] Auto-entering Gameplay scene...");
                SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
            }
        }
    }
}
