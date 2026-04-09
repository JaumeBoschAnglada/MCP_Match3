using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Match3.Debugging
{
    /// <summary>
    /// Touch Input Diagnostic Tool
    /// Add this to an empty GameObject with a Canvas to test touch input on mobile devices
    /// Displays real-time touch information on screen
    /// </summary>
    public class TouchInputDiagnostic : MonoBehaviour
    {
        [SerializeField] private Text diagnosticText;
        [SerializeField] private Camera mainCamera;
        
        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
                
            if (diagnosticText == null)
                diagnosticText = GetComponentInChildren<Text>();
                
            Debug.Log("[TouchInputDiagnostic] Started. Using Input System. Platform: " + Application.platform);
        }

        private void Update()
        {
            if (diagnosticText == null) return;

            string info = $"=== TOUCH DIAGNOSTIC ===\n";
            info += $"Platform: {Application.platform}\n";
            info += $"Input System Active\n\n";

            // Check mouse input
            if (Mouse.current != null)
            {
                info += $"MOUSE STATUS:\n";
                info += $"Position: {Mouse.current.position.ReadValue()}\n";
                info += $"Button: {(Mouse.current.leftButton.isPressed ? "PRESSED" : "RELEASED")}\n\n";
            }

            // Check touch input
            if (Touchscreen.current != null)
            {
                info += $"TOUCH STATUS:\n";
                info += $"Touch Count: {Touchscreen.current.touches.Count}\n";
                
                if (Touchscreen.current.primaryTouch.press.isPressed)
                {
                    var touchPhase = Touchscreen.current.primaryTouch.phase.ReadValue();
                    var touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
                    var touchDelta = Touchscreen.current.primaryTouch.delta.ReadValue();
                    
                    info += $"Primary Touch Phase: {touchPhase}\n";
                    info += $"Touch Position: {touchPos}\n";
                    info += $"Touch Delta: {touchDelta}\n\n";

                    // Raycast test
                    Ray ray = mainCamera.ScreenPointToRay(touchPos);
                    if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                    {
                        info += $"✅ Raycast HIT!\n";
                        info += $"Hit Object: {hit.collider.name}\n";
                        info += $"Hit Distance: {hit.distance:F2}\n";
                        if (hit.collider.TryGetComponent<Match3.Gameplay.Piece>(out var piece))
                        {
                            info += $"Piece At: ({piece.Data.x}, {piece.Data.y})\n";
                            info += $"Piece Color: {piece.Data.colorType}\n";
                        }
                    }
                    else
                    {
                        info += $"❌ Raycast MISS\n";
                    }
                }
                else
                {
                    info += "No touch detected\n";
                }
            }
            else
            {
                info += $"❌ Touchscreen not available\n";
            }

            diagnosticText.text = info;
        }
    }
}
