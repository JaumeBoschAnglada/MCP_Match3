using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Match3.Scenes
{
    public class HallManager : MonoBehaviour
    {
        [SerializeField] private Button playButton;

        private void Start()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }
            else
            {
                Debug.LogError("HallManager: PlayButton not assigned in Inspector!");
            }

            //Entramos directo.
            Debug.Log("------------- HallManager: ENTRAMOS DIRECTO A LEVEL!");
            OnPlayButtonClicked();
        }

        private void OnPlayButtonClicked()
        {
            SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
        }
    }
}

