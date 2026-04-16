using UnityEditor;
using UnityEngine;
using Match3.Items;

namespace Match3.Editor
{
    /// <summary>
    /// Draws debug gizmos for Items to visualize their target position (Board position).
    /// Shows misalignment between Item world position and its assigned Board position.
    /// </summary>
    [CustomEditor(typeof(Item), true)]
    public class ItemDebugDrawer : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            Item item = (Item)target;
            if (item == null || item.m_Board == null) return;

            Vector3 itemPos = item.transform.position;
            Vector3 boardPos = item.m_Board.transform.position;
            float distance = Vector3.Distance(itemPos, boardPos);

            // More strict threshold for visual clarity
            const float ALIGNMENT_THRESHOLD = 0.001f;

            // Draw line from item to its target board position
            Handles.color = distance > ALIGNMENT_THRESHOLD ? Color.yellow : Color.green;
            Handles.DrawDottedLine(itemPos, boardPos, 3f);

            // Draw sphere at target board position
            Handles.color = Color.green;
            Handles.SphereHandleCap(0, boardPos, Quaternion.identity, 0.2f, EventType.Repaint);

            // Always show distance and positions for debugging
            Handles.color = distance > ALIGNMENT_THRESHOLD ? Color.red : Color.cyan;
            Vector3 labelPos = (itemPos + boardPos) / 2f;
            string label = distance > ALIGNMENT_THRESHOLD 
                ? $"⚠ OFFSET: {distance:F4}\nItem: ({itemPos.x:F3}, {itemPos.y:F3}, {itemPos.z:F3})\nBoard: ({boardPos.x:F3}, {boardPos.y:F3}, {boardPos.z:F3})"
                : $"✓ Aligned ({distance:F4})";
            Handles.Label(labelPos, label);

            // Draw board reference in label
            Handles.color = Color.white;
            Handles.Label(itemPos + Vector3.up * 0.5f, $"→ {item.m_Board.name}");
        }
    }

    /// <summary>
    /// Global gizmo drawer for all Items in the scene.
    /// Draws small icons and debug info even when not selected.
    /// </summary>
    [InitializeOnLoad]
    public static class ItemGizmoDrawer
    {
        static ItemGizmoDrawer()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!Application.isPlaying) return;

            // Find all items in scene
            Item[] items = Object.FindObjectsByType<Item>(FindObjectsSortMode.None);

            const float ALIGNMENT_THRESHOLD = 0.001f;

            foreach (var item in items)
            {
                if (item == null || item.m_Board == null) continue;

                Vector3 itemPos = item.transform.position;
                Vector3 boardPos = item.m_Board.transform.position;
                float distance = Vector3.Distance(itemPos, boardPos);

                // Draw small indicator at item position
                if (distance > ALIGNMENT_THRESHOLD)
                {
                    // Misaligned - draw red dot with distance label
                    Handles.color = new Color(1f, 0f, 0f, 0.8f);
                    Handles.SphereHandleCap(0, itemPos, Quaternion.identity, 0.15f, EventType.Repaint);

                    // Show distance in red
                    Handles.color = Color.red;
                    Handles.Label(itemPos + Vector3.right * 0.3f, $"{distance:F4}");
                }
                else
                {
                    // Aligned - draw small green dot
                    Handles.color = new Color(0f, 1f, 0f, 0.3f);
                    Handles.SphereHandleCap(0, itemPos, Quaternion.identity, 0.1f, EventType.Repaint);
                }
            }
        }
    }
}
