using UnityEngine;
using UnityEngine.InputSystem;
using Match3.Gameplay;

namespace Match3.Input
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private float swipeThreshold = 30f;

        private Piece draggedPiece;
        private Vector2 startScreenPos;
        private bool isDragging;

        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Mouse.current == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
                OnPointerDown(Mouse.current.position.ReadValue());
            else if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
                OnPointerUp(Mouse.current.position.ReadValue());
        }

        private void OnPointerDown(Vector2 screenPos)
        {
            if (gameManager == null || gameManager.IsProcessing) return;

            Ray ray = mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Piece piece = hit.collider.GetComponent<Piece>();
                if (piece != null)
                {
                    draggedPiece = piece;
                    startScreenPos = screenPos;
                    isDragging = true;
                }
            }
        }

        private void OnPointerUp(Vector2 screenPos)
        {
            isDragging = false;
            if (draggedPiece == null) return;

            Vector2 delta = screenPos - startScreenPos;
            if (delta.magnitude >= swipeThreshold)
            {
                Vector2Int dir = GetSwipeDirection(delta);
                int targetX = draggedPiece.Data.x + dir.x;
                int targetY = draggedPiece.Data.y + dir.y;

                Piece targetPiece = gameManager.GetPieceAt(targetX, targetY);
                if (targetPiece != null)
                    gameManager.SwapPieces(draggedPiece, targetPiece);
            }

            draggedPiece = null;
        }

        private Vector2Int GetSwipeDirection(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                return delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                return delta.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
    }
}
