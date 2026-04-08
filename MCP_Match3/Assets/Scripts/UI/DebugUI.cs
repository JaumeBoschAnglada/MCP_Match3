using UnityEngine;

namespace Match3.UI
{
    public class DebugUI : MonoBehaviour
    {
        private bool isSlowMo = false;
        private float slowMoSpeed = 0.1f;
        private float normalSpeed = 1f;
        private Match3.Gameplay.GameManager gameManager;

        private void Start()
        {
            gameManager = FindObjectOfType<Match3.Gameplay.GameManager>();
            Debug.Log("[DebugUI] Started. Ready for OnGUI controls.");
        }

        private void OnGUI()
        {
            // Create debug controls in the bottom-left corner using GUILayout
            GUILayout.BeginArea(new Rect(10, Screen.height - 80, 250, 70));
            
            GUILayout.Label("═══════════════════════════", GUI.skin.label);
            
            // Time scale button
            string buttonLabel = isSlowMo ? "⏱ Speed: 0.1x" : "⏱ Speed: 1.0x";
            if (GUILayout.Button(buttonLabel, GUILayout.Height(40)))
            {
                ToggleTimeScale();
            }
            
            GUILayout.Label("═══════════════════════════", GUI.skin.label);
            
            GUILayout.EndArea();

            // Draw grid state on right side
            DrawGridState();
        }

        private void ToggleTimeScale()
        {
            isSlowMo = !isSlowMo;
            Time.timeScale = isSlowMo ? slowMoSpeed : normalSpeed;
            Debug.Log($"[DebugUI] Time.timeScale: {Time.timeScale}x");
        }

        private void DrawGridState()
        {
            if (gameManager == null) return;

            // Calculate area for grid display (top-right corner)
            float width = 300;
            float height = 400;
            Rect gridArea = new Rect(Screen.width - width - 10, 10, width, height);
            
            GUILayout.BeginArea(gridArea);
            GUILayout.Box("Grid State (Green pieces)", GUILayout.Width(width - 10));

            // Display grid as text
            GUILayout.Label(GetGridString(), GUI.skin.box);
            
            GUILayout.EndArea();
        }

        private string GetGridString()
        {
            if (gameManager == null) return "Loading...";

            string grid = "Posiciones (x,y):\n";
            grid += "─────────────────\n";
            
            int count = 0;
            for (int x = 0; x < 6; x++)
            {
                string row = "";
                for (int y = 0; y < 6; y++)
                {
                    var piece = gameManager.GetPieceAt(x, y);
                    if (piece != null && piece.gameObject.activeSelf)
                    {
                        string typeIcon = piece.Data.type switch
                        {
                            Match3.Data.PieceType.Red => "🔴",
                            Match3.Data.PieceType.Blue => "🔵",
                            Match3.Data.PieceType.Green => "🟢",
                            Match3.Data.PieceType.Yellow => "🟡",
                            _ => "⚪"
                        };
                        row += typeIcon + " ";
                        count++;
                    }
                    else
                    {
                        row += "⚫ ";
                    }
                }
                grid += row + "\n";
            }
            
            grid += "─────────────────\n";
            grid += $"Active: {count}/36\n";
            grid += $"Speed: {Time.timeScale}x\n";

            return grid;
        }

        private void OnDestroy()
        {
            // Ensure time scale is reset when debug UI is destroyed
            Time.timeScale = 1f;
        }
    }
}
