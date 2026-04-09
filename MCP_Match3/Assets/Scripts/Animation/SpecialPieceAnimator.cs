using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Match3.Data;
using Match3.Gameplay;

namespace Match3.Animation
{
    /// <summary>
    /// Handles the gradual elimination animation for special pieces.
    /// When HorizontalRow or VerticalRow is triggered, eliminates pieces gradually from center outward.
    /// </summary>
    public class SpecialPieceAnimator : MonoBehaviour
    {
        private const float ELIMINATION_DELAY = 0.05f;
        private PieceAnimator pieceAnimator;

        private void Awake()
        {
            pieceAnimator = GetComponent<PieceAnimator>();
        }

        /// <summary>
        /// Start gradual elimination for special pieces with pop animation.
        /// Pass the special piece and the pieces to eliminate in order.
        /// </summary>
        public void EliminateGradually(Piece specialPiece, List<Piece> piecesToEliminate)
        {
            StartCoroutine(EliminateGraduallyCoroutine(specialPiece, piecesToEliminate));
        }

        private IEnumerator EliminateGraduallyCoroutine(Piece specialPiece, List<Piece> piecesToEliminate)
        {
            if (pieceAnimator == null)
                pieceAnimator = GetComponent<PieceAnimator>();

            // Animate pop for each piece with delay (center outward)
            foreach (var piece in piecesToEliminate)
            {
                if (piece == null) continue;

                // Play pop animation
                if (pieceAnimator != null)
                    pieceAnimator.PlayPopAnimation(piece);

                yield return new WaitForSeconds(ELIMINATION_DELAY);
            }

            // Wait for last pop animation to complete
            yield return new WaitForSeconds(0.4f);

            Debug.Log("[SpecialPieceAnimator] ✅ Special piece elimination complete");
        }
    }
}
