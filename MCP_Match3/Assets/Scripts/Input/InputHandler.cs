using UnityEngine;
using UnityEngine.InputSystem;
using Match3.Gameplay;

namespace Match3.Input
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private float swipeThreshold = 15f;  // Reduced for mobile (was 30)

        private Piece draggedPiece;
        private Vector2 startScreenPos;
        private bool isDragging;

        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            
            Debug.Log($"[InputHandler] Initialized. Using Input System. Platform: {Application.platform}");
            Debug.Log($"[InputHandler] Swipe Threshold: {swipeThreshold}px");
            Debug.Log($"[InputHandler] Main Camera: {mainCamera?.name ?? "NULL"}");
            Debug.Log($"[InputHandler] GameManager: {gameManager?.name ?? "NULL"}");
        }

        private void Update()
        {
            // Both touch and mouse use the same Input System API now
            HandleInputSystem();
        }

        private void HandleInputSystem()
        {
            // Check for touch input on mobile - IMPORTANT: Check phase first before press.isPressed
            // press.isPressed becomes FALSE on release, so we need to read the phase directly
            if (Touchscreen.current != null)
            {
                var touchPhase = Touchscreen.current.primaryTouch.phase.ReadValue();
                
                // Process touch if:
                // - Currently pressed (Began, Moved)
                // - OR just released (Ended, Canceled)
                if (touchPhase == UnityEngine.InputSystem.TouchPhase.Began
                    || touchPhase == UnityEngine.InputSystem.TouchPhase.Moved
                    || touchPhase == UnityEngine.InputSystem.TouchPhase.Ended
                    || touchPhase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    HandleTouchInput();
                    return;
                }
            }

            // Check for mouse input (desktop/editor)
            if (Mouse.current != null)
            {
                HandleMouseInput();
            }
        }

        private void HandleTouchInput()
        {
            var touchInput = Touchscreen.current.primaryTouch;
            var touchPhase = touchInput.phase.ReadValue();
            var touchPos = touchInput.position.ReadValue();

            if (touchPhase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                Debug.Log($"[InputHandler] 👆 TOUCH BEGAN at screen pos ({touchPos.x:F0}, {touchPos.y:F0})");
                OnPointerDown(touchPos);
            }
            else if (touchPhase == UnityEngine.InputSystem.TouchPhase.Moved && isDragging)
            {
                Vector2 delta = touchPos - startScreenPos;
                Debug.Log($"[InputHandler] 👆 TOUCH MOVING - delta: {delta.magnitude:F1}px, dir: ({delta.x:F1}, {delta.y:F1})");
            }
            else if (touchPhase == UnityEngine.InputSystem.TouchPhase.Ended && isDragging)
            {
                Debug.Log($"[InputHandler] 👆 TOUCH ENDED at ({touchPos.x:F0}, {touchPos.y:F0})");
                OnPointerUp(touchPos);
            }
            else if (touchPhase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                Debug.Log($"[InputHandler] 👆 TOUCH CANCELED");
                isDragging = false;
                draggedPiece = null;
            }
        }

        private void HandleMouseInput()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Debug.Log($"[InputHandler] 🖱️ Mouse button down at {mousePos}");
                OnPointerDown(mousePos);
            }
            else if (Mouse.current.leftButton.isPressed && isDragging)
            {
                // Optional: drag visualization
                Vector2 currentPos = Mouse.current.position.ReadValue();
                Vector2 delta = currentPos - startScreenPos;
                if (delta.magnitude > 5) // Only log significant movement
                {
                    Debug.Log($"[InputHandler] 🖱️ Dragging (mouse)... delta={delta.magnitude:F1}");
                }
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Debug.Log($"[InputHandler] 🖱️ Mouse button up at {mousePos}");
                OnPointerUp(mousePos);
            }
        }

        private void OnPointerDown(Vector2 screenPos)
        {
            Debug.Log($"[InputHandler] >>> OnPointerDown called with screenPos=({screenPos.x:F0}, {screenPos.y:F0})");

            if (gameManager == null)
            {
                Debug.LogError("[InputHandler] ❌ CRITICAL: GameManager is NULL!");
                return;
            }

            if (gameManager.IsProcessing)
            {
                Debug.Log("[InputHandler] ⏳ GameManager is processing, ignoring input");
                return;
            }

            if (mainCamera == null)
            {
                Debug.LogError("[InputHandler] ❌ CRITICAL: Main Camera is NULL!");
                return;
            }

            // Use raycast to detect piece
            Ray ray = mainCamera.ScreenPointToRay(screenPos);
            Debug.Log($"[InputHandler] 📍 Raycast from camera: origin={ray.origin}, direction={ray.direction}");

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Debug.Log($"[InputHandler] ✅ Raycast HIT at distance {hit.distance:F2}: {hit.collider.name}");
                
                Piece piece = hit.collider.GetComponent<Piece>();
                if (piece != null)
                {
                    draggedPiece = piece;
                    startScreenPos = screenPos;
                    isDragging = true;
                    Debug.Log($"[InputHandler] 🎮 ✅ PIECE SELECTED: ({piece.Data.x},{piece.Data.y}) {piece.Data.colorType}");
                }
                else
                {
                    Debug.LogWarning($"[InputHandler] ⚠️ Hit {hit.collider.name} but NO Piece component!");
                }
            }
            else
            {
                Debug.LogWarning($"[InputHandler] ❌ Raycast MISS - no collider hit at {screenPos}");
            }
        }

        private void OnPointerUp(Vector2 screenPos)
        {
            Debug.Log($"[InputHandler] >>> OnPointerUp called. isDragging={isDragging}, draggedPiece={draggedPiece?.name ?? "NULL"}");

            isDragging = false;
            if (draggedPiece == null)
            {
                Debug.Log("[InputHandler] No piece was selected, ignoring release");
                return;
            }

            Vector2 delta = screenPos - startScreenPos;
            Debug.Log($"[InputHandler] 📏 Swipe info: delta=({delta.x:F1}, {delta.y:F1}), magnitude={delta.magnitude:F1}, threshold={swipeThreshold}");
            
            if (delta.magnitude >= swipeThreshold)
            {
                Vector2Int dir = GetSwipeDirection(delta);
                int targetX = draggedPiece.Data.x + dir.x;
                int targetY = draggedPiece.Data.y + dir.y;

                Debug.Log($"[InputHandler] 🔄 Swipe VALID! Direction: {dir}, Source: ({draggedPiece.Data.x},{draggedPiece.Data.y}), Target: ({targetX},{targetY})");

                Piece targetPiece = gameManager.GetPieceAt(targetX, targetY);
                if (targetPiece != null)
                {
                    Debug.Log($"[InputHandler] ✅ Target piece found, initiating SWAP");
                    gameManager.SwapPieces(draggedPiece, targetPiece);
                }
                else
                {
                    Debug.LogWarning($"[InputHandler] ❌ No target piece at ({targetX},{targetY})");
                }
            }
            else
            {
                Debug.Log($"[InputHandler] ⚠️ Swipe too SHORT: {delta.magnitude:F1}px < {swipeThreshold}px threshold");
            }

            draggedPiece = null;
        }

        private Vector2Int GetSwipeDirection(Vector2 delta)
        {
            float absDeltaX = Mathf.Abs(delta.x);
            float absDeltaY = Mathf.Abs(delta.y);
            
            if (absDeltaX > absDeltaY)
                return delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                return delta.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
    }
}
