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

        // Show position and type as label on the piece during gameplay
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
                // Create label style
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };

                // Prepare text lines
                string posLine = $"({data.x},{data.y})";
                string typeLine = $"{data.colorType}";
                if (data.specialEffect != SpecialEffect.None)
                    typeLine += $"\n{data.specialEffect}";

                // Draw centered on piece (accounting for screen Y flip)
                Rect rect = new Rect(screenPos.x - 50, Screen.height - screenPos.y - 35, 100, 30);
                
                // Draw semi-transparent background
                Color prevColor = GUI.color;
                GUI.color = new Color(0, 0, 0, 0.6f);
                GUI.Box(rect, "");
                GUI.color = prevColor;

                // Draw labels
                GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 35, 100, 15), posLine, labelStyle);
                GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 20, 100, 15), typeLine, labelStyle);
            }
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (data == null || !gameObject.activeSelf)
                return;

            // Draw position label in scene view
            Vector3 pos = transform.position;
            UnityEditor.Handles.Label(pos, $"({data.x},{data.y})");
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pos, 0.12f);
        }
        #endif
    }
}
