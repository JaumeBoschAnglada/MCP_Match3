using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Match3.Gameplay;

namespace Match3.Animation
{
    public class PieceAnimator : MonoBehaviour
    {
        [SerializeField] private float swapDuration = 0.2f;
        [SerializeField] private float fallDuration = 0.3f;
        [SerializeField] private float popDuration = 0.4f;

        public float GetFallDuration() => fallDuration;

        public IEnumerator PlaySwapAnimation(Piece piece1, Piece piece2)
        {
            piece1.SetAnimating(true);
            piece2.SetAnimating(true);

            Vector3 pos1 = piece1.transform.localPosition;
            Vector3 pos2 = piece2.transform.localPosition;
            float elapsed = 0;

            while (elapsed < swapDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / swapDuration;
                
                // Smooth ease-in-out cubic
                float easeT = t < 0.5f
                    ? 4f * t * t * t
                    : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
                
                piece1.transform.localPosition = Vector3.Lerp(pos1, pos2, easeT);
                piece2.transform.localPosition = Vector3.Lerp(pos2, pos1, easeT);
                
                yield return null;
            }

            piece1.transform.localPosition = pos2;
            piece2.transform.localPosition = pos1;
            piece1.UpdatePosition();
            piece2.UpdatePosition();

            piece1.SetAnimating(false);
            piece2.SetAnimating(false);
        }

        public IEnumerator PlayFallAnimation(Piece piece, Vector3 fromPos, Vector3 toPos)
        {
            piece.SetAnimating(true);
            piece.transform.localPosition = fromPos;
            float elapsed = 0;

            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fallDuration;
                
                // Ease-out cubic for natural, smooth fall
                float easeT = 1f - Mathf.Pow(1f - t, 3f);
                piece.transform.localPosition = Vector3.Lerp(fromPos, toPos, easeT);
                
                yield return null;
            }

            piece.transform.localPosition = toPos;
            piece.UpdatePosition();
            piece.SetAnimating(false);
        }

        public IEnumerator PlayPopAnimation(Piece piece)
        {
            Vector3 originalScale = piece.transform.localScale;
            float elapsed = 0;

            while (elapsed < popDuration * 0.4f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (popDuration * 0.4f);
                piece.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.2f, t);
                yield return null;
            }

            elapsed = 0;
            Renderer rend = piece.GetComponent<Renderer>();
            MaterialPropertyBlock block = new MaterialPropertyBlock();

            while (elapsed < popDuration * 0.6f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (popDuration * 0.6f);
                piece.transform.localScale = Vector3.Lerp(originalScale * 1.2f, Vector3.zero, t);

                if (rend != null)
                {
                    rend.GetPropertyBlock(block);
                    Color c = rend.material.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    block.SetColor("_Color", c);
                    rend.SetPropertyBlock(block);
                }

                yield return null;
            }

            // Piece will be returned to pool by GameManager
        }

        public IEnumerator PlaySpawnAnimation(Piece piece)
        {
            Vector3 targetScale = piece.transform.localScale;
            piece.transform.localScale = Vector3.zero;
            float elapsed = 0;
            float duration = 0.25f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easeT = 1f + 2.7f * Mathf.Pow(t - 1f, 3f) + 1.7f * Mathf.Pow(t - 1f, 2f);
                piece.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, easeT);
                yield return null;
            }

            piece.transform.localScale = targetScale;
        }

        public IEnumerator PlayReactionAnimation(Piece piece, Vector3 direction)
        {
            // Reactions disabled - not called anymore
            yield return null;
        }

        public IEnumerator PlayLandParticle(Vector3 position)
        {
            // TODO: Instanciar particle system
            Debug.Log($"Landing particle at {position}");
            yield return null;
        }

        // ==================== NEW FORCE-BASED MOVEMENT SYSTEM ====================
        // Pieces are attracted to target positions like black holes
        // External forces (explosions, etc) can affect trajectory but not final destination

        /// <summary>
        /// Move piece to target using force-based attraction (black hole style)
        /// Allows external forces to affect trajectory mid-flight
        /// </summary>
        public IEnumerator PlayDynamicMovement(Piece piece, Vector3 startPos, Vector3 targetPos, float duration)
        {
            piece.SetAnimating(true);
            piece.transform.localPosition = startPos;
            
            Vector3 velocity = Vector3.zero;
            float elapsed = 0f;
            float attractionStrength = 2f; // How strongly target "pulls" the piece
            Vector3 accumulatedExternalForce = Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                Vector3 currentPos = piece.transform.localPosition;
                Vector3 direction = targetPos - currentPos;
                float distance = direction.magnitude;

                // If very close to target, snap and finish
                if (distance < 0.05f)
                {
                    piece.transform.localPosition = targetPos;
                    break;
                }

                // Attraction force (pulls toward target like gravity to a black hole)
                Vector3 attractionForce = direction.normalized * attractionStrength;

                // Total force = attraction + external forces
                Vector3 totalForce = attractionForce + accumulatedExternalForce;

                // Update velocity (with damping for stability)
                float damping = 0.85f;
                velocity = (velocity + totalForce * Time.deltaTime) * damping;

                // Update position
                piece.transform.localPosition = currentPos + velocity * Time.deltaTime;

                // Decay external forces over time (they fade out)
                accumulatedExternalForce *= 0.9f;

                yield return null;
            }

            piece.transform.localPosition = targetPos;
            piece.UpdatePosition();
            piece.SetAnimating(false);
        }

        /// <summary>
        /// Apply external force to piece (e.g., from explosion)
        /// These are non-permanent and decay over time
        /// </summary>
        public void ApplySuddenForce(Vector3 force)
        {
            // This would be called on the coroutine to affect currently animating pieces
            // Implementation: would need to store reference to current force being applied
            // For now, this is a framework placeholder
            Debug.Log($"Force applied: {force}");
        }

        /// <summary>
        /// Updated fall animation using dynamic movement
        /// Instead of Lerp, uses force-based attraction to destination
        /// </summary>
        public IEnumerator PlayFallAnimationDynamic(Piece piece, Vector3 fromPos, Vector3 toPos)
        {
            yield return StartCoroutine(PlayDynamicMovement(piece, fromPos, toPos, fallDuration));
        }

        /// <summary>
        /// Updated swap using dynamic movement
        /// Both pieces are attracted to their target positions simultaneously
        /// </summary>
        public IEnumerator PlaySwapAnimationDynamic(Piece piece1, Piece piece2)
        {
            piece1.SetAnimating(true);
            piece2.SetAnimating(true);

            Vector3 pos1 = piece1.transform.localPosition;
            Vector3 pos2 = piece2.transform.localPosition;

            // Run both movements in parallel
            yield return StartCoroutine(PlayDynamicMovementParallel(piece1, piece2, pos1, pos2, pos2, pos1, swapDuration));

            piece1.SetAnimating(false);
            piece2.SetAnimating(false);
        }

        /// <summary>
        /// Helper to run two pieces' dynamic movements in parallel
        /// </summary>
        private IEnumerator PlayDynamicMovementParallel(Piece piece1, Piece piece2, 
            Vector3 start1, Vector3 start2, Vector3 target1, Vector3 target2, float duration)
        {
            Vector3 vel1 = Vector3.zero;
            Vector3 vel2 = Vector3.zero;
            float elapsed = 0f;
            float attractionStrength = 2f;
            float damping = 0.85f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Piece 1 movement
                Vector3 pos1 = piece1.transform.localPosition;
                Vector3 dir1 = target1 - pos1;
                float dist1 = dir1.magnitude;
                if (dist1 > 0.05f)
                {
                    Vector3 force1 = dir1.normalized * attractionStrength;
                    vel1 = (vel1 + force1 * Time.deltaTime) * damping;
                    piece1.transform.localPosition = pos1 + vel1 * Time.deltaTime;
                }
                else
                {
                    piece1.transform.localPosition = target1;
                }

                // Piece 2 movement
                Vector3 pos2 = piece2.transform.localPosition;
                Vector3 dir2 = target2 - pos2;
                float dist2 = dir2.magnitude;
                if (dist2 > 0.05f)
                {
                    Vector3 force2 = dir2.normalized * attractionStrength;
                    vel2 = (vel2 + force2 * Time.deltaTime) * damping;
                    piece2.transform.localPosition = pos2 + vel2 * Time.deltaTime;
                }
                else
                {
                    piece2.transform.localPosition = target2;
                }

                yield return null;
            }

            piece1.transform.localPosition = target1;
            piece2.transform.localPosition = target2;
            piece1.UpdatePosition();
            piece2.UpdatePosition();
        }
    }
}

