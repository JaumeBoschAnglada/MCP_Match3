using UnityEngine;

namespace Match3.Core
{
    public class DebugManager : MonoBehaviour
    {
        private bool m_IsSlowMo = false;
        private bool m_ShowBoardInfo = false;
        private bool m_PanelVisible = false;
        private bool m_ShowTestLevels = false;
        private string[] m_TestLevels = null;
        private string m_SelectedLevel = "level_fase8";
        private GUIStyle m_BtnStyle;
        private GUIStyle m_LabelStyle;
        private GUIStyle m_CellStyle;

        // Offset Y para no solaparse con el TopUI (120px de alto)
        private const float k_TopUIHeight = 130f;
        private const float k_ToggleSize  = 36f;

        private void OnGUI()
        {
            // Init styles once (needs to happen inside OnGUI for GUI.skin access)
            if (m_BtnStyle == null)
            {
                m_BtnStyle = new GUIStyle(GUI.skin.button) { fontSize = 24 };
                m_LabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
                m_CellStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 22,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                m_CellStyle.normal.background = Texture2D.grayTexture;
            }

            // ── Botón toggle arriba-izquierda ────────────────────────
            var toggleStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, fontStyle = FontStyle.Bold };
            GUI.color = m_PanelVisible ? Color.yellow : Color.white;
            if (GUI.Button(new Rect(10, 10, k_ToggleSize, k_ToggleSize), "🛠", toggleStyle))
                m_PanelVisible = !m_PanelVisible;
            GUI.color = Color.white;

            if (!m_PanelVisible) return;

            // ── Panel de herramientas (bajo el TopUI) ────────────────
            float y = k_TopUIHeight;

            // Cargar lista de niveles de test si no está inicializada
            if (m_TestLevels == null)
            {
                // Solo archivos que empiecen por "level_fase" y terminen en .json
                var levels = Resources.LoadAll<TextAsset>("Levels");
                System.Collections.Generic.List<string> found = new System.Collections.Generic.List<string>();
                foreach (var l in levels)
                {
                    if (l.name.StartsWith("level_fase"))
                        found.Add(l.name);
                }
                found.Sort();
                m_TestLevels = found.ToArray();
                // Por defecto selecciona el de la fase 8 si existe
                if (System.Array.IndexOf(m_TestLevels, "level_fase8") >= 0)
                    m_SelectedLevel = "level_fase8";
                else if (m_TestLevels.Length > 0)
                    m_SelectedLevel = m_TestLevels[m_TestLevels.Length-1];
            }

            GUILayout.BeginArea(new Rect(10, y, 320, 180));
            GUI.color = m_IsSlowMo ? Color.yellow : Color.green;
            if (GUILayout.Button(m_IsSlowMo ? "TimeScale: 0.1x" : "TimeScale: 1.0x", m_BtnStyle, GUILayout.Height(40)))
            {
                m_IsSlowMo = !m_IsSlowMo;
                Time.timeScale = m_IsSlowMo ? 0.1f : 1f;
            }
            GUI.color = m_ShowBoardInfo ? Color.cyan : Color.white;
            if (GUILayout.Button(m_ShowBoardInfo ? "Board Info: ON" : "Board Info: OFF", m_BtnStyle, GUILayout.Height(40)))
            {
                m_ShowBoardInfo = !m_ShowBoardInfo;
            }
            GUI.color = m_ShowTestLevels ? Color.yellow : Color.white;
            if (GUILayout.Button("Niveles de Test", m_BtnStyle, GUILayout.Height(40)))
            {
                m_ShowTestLevels = !m_ShowTestLevels;
            }
            GUI.color = Color.white;
            GUILayout.EndArea();

            // Panel de selección de niveles de test
            if (m_ShowTestLevels && m_TestLevels != null && m_TestLevels.Length > 0)
            {
                GUILayout.BeginArea(new Rect(340, y, 220, 40 + 48 * m_TestLevels.Length));
                GUILayout.Label("Selecciona nivel de test:", m_LabelStyle);
                for (int i = 0; i < m_TestLevels.Length; ++i)
                {
                    string lvl = m_TestLevels[i];
                    bool isSelected = lvl == m_SelectedLevel;
                    GUI.color = isSelected ? Color.green : Color.white;
                    if (GUILayout.Button(lvl, m_BtnStyle, GUILayout.Height(40)))
                    {
                        m_SelectedLevel = lvl;
                        m_ShowTestLevels = false;
                        // Llama a cargar el nivel
                        var matchMgr = MatchManager.Instance;
                        if (matchMgr != null)
                        {
                            matchMgr.LoadLevel(lvl);
                        }
                    }
                }
                GUI.color = Color.white;
                GUILayout.EndArea();
            }

            // State info
            var mgr = MatchManager.Instance;
            if (mgr == null) return;

            m_LabelStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(10, y + 125, 700, 50),
                $"Step: {mgr.m_StepType}  State: {mgr.m_MatchState}  Combo: {mgr.ComboCnt}",
                m_LabelStyle);

            // Board overlay
            if (!m_ShowBoardInfo || mgr.m_ListBoard == null) return;

            var cam = Camera.main;
            if (cam == null) return;

            for (int i = 0; i < 81; i++)
            {
                Board board = mgr.m_ListBoard[i];
                if (!board.IsActiveCell) continue;

                Vector3 screenPos = cam.WorldToScreenPoint(board.transform.position);
                if (screenPos.z < 0) continue;
                float guiY = Screen.height - screenPos.y;

                var item = board.m_Item as Match3.Items.Item;
                if (item != null)
                {
                    // Short 3-letter label
                    string label = ShortColor(item.m_Color);
                    m_CellStyle.normal.textColor = GetLabelColor(item.m_Color);
                    GUI.Label(new Rect(screenPos.x - 30, guiY + 12, 60, 26), label, m_CellStyle);
                }
                else
                {
                    m_CellStyle.normal.textColor = Color.gray;
                    GUI.Label(new Rect(screenPos.x - 30, guiY + 12, 60, 26), "---", m_CellStyle);
                }
            }
        }

        private string ShortColor(Match3.Data.ColorType c)
        {
            switch (c)
            {
                case Match3.Data.ColorType.RED:    return "RED";
                case Match3.Data.ColorType.YELLOW: return "YEL";
                case Match3.Data.ColorType.GREEN:  return "GRN";
                case Match3.Data.ColorType.BLUE:   return "BLU";
                case Match3.Data.ColorType.PURPLE: return "PUR";
                case Match3.Data.ColorType.ORANGE: return "ORA";
                default: return "???";
            }
        }

        private Color GetLabelColor(Match3.Data.ColorType c)
        {
            switch (c)
            {
                case Match3.Data.ColorType.RED:    return new Color(1f, 0.3f, 0.3f);
                case Match3.Data.ColorType.YELLOW: return new Color(1f, 1f, 0.3f);
                case Match3.Data.ColorType.GREEN:  return new Color(0.3f, 1f, 0.3f);
                case Match3.Data.ColorType.BLUE:   return new Color(0.4f, 0.6f, 1f);
                case Match3.Data.ColorType.PURPLE: return new Color(0.85f, 0.4f, 1f);
                case Match3.Data.ColorType.ORANGE: return new Color(1f, 0.65f, 0.25f);
                default: return Color.white;
            }
        }
    }
}
