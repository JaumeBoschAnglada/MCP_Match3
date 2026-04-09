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
        
        // Inspector debug info
        [SerializeField]
        private string gridDebugInfo = "--";

        protected virtual void Awake()
        {
            if (pieceRenderer == null)
                pieceRenderer = GetComponent<Renderer>();
            originalScale = transform.localScale;
            isAnimating = false;
        }

        public void SetAnimating(bool animating) => isAnimating = animating;

        public virtual void ResetVisuals()
        {
            transform.localScale = originalScale;
            if (pieceRenderer != null)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                pieceRenderer.SetPropertyBlock(block);
            }
        }

        public virtual void Initialize(PieceData pieceData)
        {
            data = pieceData;
            UpdatePosition();
            UpdateDebugInfo();
        }

        public virtual void UpdatePosition()
        {
            transform.localPosition = new Vector3(data.x, data.y, 0);
            UpdateDebugInfo();
        }

        private void UpdateDebugInfo()
        {
            if (data != null)
            {
                gridDebugInfo = $"Grid ({data.x}, {data.y}) | {data.colorType} {data.specialEffect}";
            }
        }

        // Show position as label on the piece during gameplay
        private void OnGUI()
        {
            if (data == null || !gameObject.activeSelf)
                return;

            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            // Convert world position to screen position
            Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position);
            if (screenPos.z > 0 && screenPos.z < 1000) // Only if in front of camera
            {
                // Create label style with better readability
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };

                // Draw centered on piece (accounting for screen Y flip)
                Rect rect = new Rect(screenPos.x - 30, Screen.height - screenPos.y - 15, 60, 20);
                
                // Draw semi-transparent background
                Color prevColor = GUI.color;
                GUI.color = new Color(0, 0, 0, 0.5f);
                GUI.Box(rect, "");
                GUI.color = prevColor;

                // Draw position label
                GUI.Label(rect, $"({data.x},{data.y})", labelStyle);
            }
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (data == null || !gameObject.activeSelf)
                return;

            // Draw position label in scene view
            Vector3 pos = transform.position;
            UnityEditor.Handles.Label(pos + Vector3.up * 0.5f, $"({data.x},{data.y})");
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pos, 0.12f);
        }
        #endif
    }
}
