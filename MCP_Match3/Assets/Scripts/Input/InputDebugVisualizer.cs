using UnityEngine;
using UnityEngine.InputSystem;

namespace Match3.Input
{
    /// <summary>
    /// Debug helper to visualize input detection in Editor
    /// Add to scene to see click/drag visualization
    /// </summary>
    public class InputDebugVisualizer : MonoBehaviour
    {
        private Vector2 lastClickPos;
        private Vector2 lastDragPos;
        private bool wasClicked = false;
        private float clickTimeout = 0.5f;
        private float lastClickTime = 0;

        private void Update()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                lastClickPos = Mouse.current.position.ReadValue();
                wasClicked = true;
                lastClickTime = Time.time;
                Debug.Log($"[InputDebugVisualizer] Click at screen pos: {lastClickPos}");
            }

            if (Mouse.current != null && Mouse.current.leftButton.isPressed && wasClicked)
            {
                lastDragPos = Mouse.current.position.ReadValue();
            }

            // Also detect touch
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                var touchPhase = Touchscreen.current.primaryTouch.phase.ReadValue();
                if (touchPhase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    lastClickPos = Touchscreen.current.primaryTouch.position.ReadValue();
                    wasClicked = true;
                    lastClickTime = Time.time;
                    Debug.Log($"[InputDebugVisualizer] Touch at screen pos: {lastClickPos}");
                }
                else if (touchPhase == UnityEngine.InputSystem.TouchPhase.Moved)
                {
                    lastDragPos = Touchscreen.current.primaryTouch.position.ReadValue();
                }
            }

            // Clear click indicator after timeout
            if (Time.time - lastClickTime > clickTimeout)
            {
                wasClicked = false;
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Box("=== INPUT DEBUG ===");
            
            GUILayout.Label($"Platform: {Application.platform}");
            
            if (Mouse.current != null)
            {
                GUILayout.Label($"Mouse Position: {Mouse.current.position.ReadValue()}");
                GUILayout.Label($"Left Mouse: {(Mouse.current.leftButton.isPressed ? "PRESSED" : "RELEASED")}");
            }

            if (Touchscreen.current != null)
            {
                GUILayout.Label($"Touch Count: {Touchscreen.current.touches.Count}");
                if (Touchscreen.current.primaryTouch.press.isPressed)
                {
                    GUILayout.Label($"Touch Position: {Touchscreen.current.primaryTouch.position.ReadValue()}");
                    GUILayout.Label($"Touch Phase: {Touchscreen.current.primaryTouch.phase.ReadValue()}");
                }
            }

            if (wasClicked)
            {
                Vector2 delta = lastDragPos - lastClickPos;
                GUILayout.Label($"Drag Delta: {delta.magnitude:F1}px");
                GUILayout.Label($"Drag Dir: {(Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? "HORIZONTAL" : "VERTICAL")}");
            }
            
            GUILayout.EndArea();
        }
    }
}
