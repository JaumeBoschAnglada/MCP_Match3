using UnityEngine;
using Match3.Data;

namespace Match3.Gameplay
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private Renderer pieceRenderer;
        
        private PieceData data;
        private Vector3 originalScale;
        private bool isAnimating;

        public PieceData Data => data;
        public bool IsAnimating => isAnimating;

        private void Awake()
        {
            if (pieceRenderer == null)
                pieceRenderer = GetComponent<Renderer>();
            originalScale = transform.localScale;
            isAnimating = false;
        }

        public void SetAnimating(bool animating) => isAnimating = animating;

        public void ResetVisuals()
        {
            transform.localScale = originalScale;
            if (pieceRenderer != null)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                pieceRenderer.SetPropertyBlock(block);
            }
        }

        public void Initialize(PieceData pieceData)
        {
            data = pieceData;
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            transform.localPosition = new Vector3(data.x, data.y, 0);
        }

        // Show position as label on the piece
        private void OnGUI()
        {
            if (data == null || !gameObject.activeSelf)
                return;

            // Convert world position to screen position to draw label
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            if (screenPos.z > 0) // Only if in front of camera
            {
                // Create LARGE, bold label style
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = 32;
                labelStyle.fontStyle = FontStyle.Bold;
                labelStyle.alignment = TextAnchor.MiddleCenter;
                
                // Draw position label (x,y) centered on piece - much bigger rect
                GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 20, 100, 40), 
                    $"({data.x},{data.y})", labelStyle);
            }
        }
    }
}
