using UnityEngine;
using Match3.Data;

namespace Match3.Gameplay
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private Renderer pieceRenderer;
        
        private PieceData data;
        private Vector3 originalScale;

        public PieceData Data => data;

        private void Awake()
        {
            if (pieceRenderer == null)
                pieceRenderer = GetComponent<Renderer>();
            originalScale = transform.localScale;
        }

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
    }
}
