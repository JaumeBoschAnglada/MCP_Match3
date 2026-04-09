using UnityEngine;

namespace Match3.UI
{
    public class DebugUI : MonoBehaviour
    {
        private bool isSlowMo = false;
        private float slowMoSpeed = 0.1f;
        private float normalSpeed = 1f;
        private Match3.Gameplay.GameManager gameManager;
        
        // FPS calculation
        private float deltaTime;
        private float fps;

        private void Start()
        {
            gameManager = FindObjectOfType<Match3.Gameplay.GameManager>();
            Debug.Log("[DebugUI] Started. Ready for OnGUI controls.");
        }

        private void Update()
        {
            // Calculate FPS
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            fps = 1f / deltaTime;
        }

        private void OnGUI()
        {
            // Create large FPS display in top-right corner
            GUIStyle fpsStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 32,
                alignment = TextAnchor.MiddleCenter
            };
            fpsStyle.normal.textColor = Color.white;
            
            GUI.Label(new Rect(Screen.width - 220, 10, 210, 70), $"FPS: {fps:F1}", fpsStyle);
            
            // Create large button style
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 24
            };
            
            // Create debug controls in the bottom-left corner using GUILayout
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
                        
            // Time scale button
            string buttonLabel = isSlowMo ? "⏱ Speed: 0.1x" : "⏱ Speed: 1.0x";
            if (GUILayout.Button(buttonLabel, buttonStyle, GUILayout.Height(80)))
            {
                ToggleTimeScale();
            }
                        
            GUILayout.EndArea();
        }

        private void ToggleTimeScale()
        {
            isSlowMo = !isSlowMo;
            Time.timeScale = isSlowMo ? slowMoSpeed : normalSpeed;
            Debug.Log($"[DebugUI] Time.timeScale: {Time.timeScale}x");
        }

        private void OnDestroy()
        {
            // Ensure time scale is reset when debug UI is destroyed
            Time.timeScale = 1f;
        }
    }
}
