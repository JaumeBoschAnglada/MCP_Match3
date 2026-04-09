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

    public void SetPieceAnimator(PieceAnimator animator)
    {
        pieceAnimator = animator;
    }

    /// <summary>
    /// Start gradual elimination for special pieces with pop animation.
    /// Pass the special piece and the pieces to eliminate in order.
    /// Returns IEnumerator so GameManager can wait for completion.
    /// </summary>
    public IEnumerator EliminateGradually(Piece specialPiece, List<Piece> piecesToEliminate)
        {
            yield return StartCoroutine(EliminateGraduallyCoroutine(specialPiece, piecesToEliminate));
        }

        private IEnumerator EliminateGraduallyCoroutine(Piece specialPiece, List<Piece> piecesToEliminate)
        {
            if (pieceAnimator == null)
            {
                Debug.LogError("[SpecialPieceAnimator] ❌ ERROR: pieceAnimator is null! Cannot queue animations!");
                yield break;
            }

            // Ensure all pieces are ACTIVE before starting animations
            foreach (var piece in piecesToEliminate)
            {
                if (piece != null && !piece.gameObject.activeSelf)
                {
                    Debug.LogWarning($"[SpecialPieceAnimator] ⚠️ Piece at ({piece.Data.x},{piece.Data.y}) was inactive! Activating...");
                    piece.gameObject.SetActive(true);
                }
            }

            // Animate pop for each piece with delay (center outward)
            foreach (var piece in piecesToEliminate)
            {
                if (piece == null) continue;

                Debug.Log($"[SpecialPieceAnimator] Playing pop animation for {piece.Data.colorType} at ({piece.Data.x},{piece.Data.y})");

                // CRITICAL: Ensure piece is active before animation
                if (!piece.gameObject.activeSelf)
                {
                    piece.gameObject.SetActive(true);
                }

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
