using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Match3.Gameplay;

namespace Match3.Animation
{
    // Used for tween-based animations (swap, pop, spawn)
    public class PieceAnimation
    {
        public enum AnimationType { Swap, Pop, Spawn }

        public AnimationType type;
        public Piece piece;
        public float elapsed;
        public float duration;
        public Vector3 startPos;
        public Vector3 endPos;
        public Piece otherPiece;
        public Vector3 otherStartPos;
        public Vector3 otherEndPos;
        public Vector3 originalScale;

        public bool IsComplete => elapsed >= duration;
    }

    public class PieceAnimator : MonoBehaviour
    {
        [Header("Tween Durations")]
        [SerializeField] private float swapDuration = 0.2f;
        [SerializeField] private float popDuration = 0.4f;
        [SerializeField] private float spawnDuration = 0.25f;

        [Header("Spring Physics (Fall/Movement)")]
        [SerializeField] private float springStiffness = 100f;  // Higher = faster, stiffer
        [SerializeField] private float springDamping = 14f;     // Higher = less bounce (critical at ~20)

        // Spring state per piece
        private class SpringState
        {
            public Vector3 velocity;
            public Vector3 targetPos;
            public bool settled;
        }

        private readonly Dictionary<Piece, SpringState> springStates = new Dictionary<Piece, SpringState>();
        private readonly List<PieceAnimation> activeAnimations = new List<PieceAnimation>();

        // ==================== UNITY LOOP ====================

        private void Update()
        {
            UpdateSpringPhysics();
            UpdateTweenAnimations();
        }

        private void UpdateSpringPhysics()
        {
            foreach (var kvp in springStates)
            {
                SpringState state = kvp.Value;
                if (state.settled) continue;

                Piece piece = kvp.Key;
                if (piece == null || !piece.gameObject.activeSelf) continue;

                Vector3 currentPos = piece.transform.localPosition;
                Vector3 displacement = state.targetPos - currentPos;

                // Spring-damper: acceleration = stiffness * displacement - damping * velocity
                Vector3 acceleration = displacement * springStiffness - state.velocity * springDamping;
                state.velocity += acceleration * Time.deltaTime;
                piece.transform.localPosition = currentPos + state.velocity * Time.deltaTime;

                // Settle when position and velocity are negligible
                if (displacement.sqrMagnitude < 0.0001f && state.velocity.sqrMagnitude < 0.0001f)
                {
                    piece.transform.localPosition = state.targetPos;
                    piece.UpdatePosition();
                    state.velocity = Vector3.zero;
                    state.settled = true;
                    piece.SetAnimating(false);
                }
            }
        }

        private void UpdateTweenAnimations()
        {
            for (int i = activeAnimations.Count - 1; i >= 0; i--)
            {
                PieceAnimation anim = activeAnimations[i];
                anim.elapsed += Time.deltaTime;
                if (anim.elapsed > anim.duration) anim.elapsed = anim.duration;

                float t = anim.elapsed / anim.duration;
                switch (anim.type)
                {
                    case PieceAnimation.AnimationType.Swap:  UpdateSwapAnimation(anim, t);  break;
                    case PieceAnimation.AnimationType.Pop:   UpdatePopAnimation(anim, t);   break;
                    case PieceAnimation.AnimationType.Spawn: UpdateSpawnAnimation(anim, t); break;
                }

                if (anim.IsComplete) activeAnimations.RemoveAt(i);
            }
        }

        // ==================== SPRING API ====================

        /// <summary>
        /// Start moving a piece toward toPos using spring physics.
        /// Resets velocity — use this for explicit falls (gravity, fill).
        /// </summary>
        public void PlayFallAnimation(Piece piece, Vector3 fromPos, Vector3 toPos)
        {
            piece.transform.localPosition = fromPos;
            piece.SetAnimating(true);

            if (!springStates.TryGetValue(piece, out var state))
            {
                state = new SpringState();
                springStates[piece] = state;
            }
            state.velocity = Vector3.zero;
            state.targetPos = toPos;
            state.settled = false;
        }

        /// <summary>
        /// Register a piece for spring tracking without resetting velocity.
        /// Only triggers movement if the target differs from the current one.
        /// Use this from Update() for idle correction.
        /// </summary>
        public void EnsureTracked(Piece piece, Vector3 targetPos)
        {
            if (!springStates.TryGetValue(piece, out var state))
            {
                // New piece: register as settled at current position; spring doesn't move it yet
                state = new SpringState { targetPos = targetPos, settled = true };
                springStates[piece] = state;
                return;
            }

            // If target has changed significantly, unsettling the spring
            if (Vector3.Distance(state.targetPos, targetPos) > 0.05f)
            {
                state.targetPos = targetPos;
                state.settled = false;
                piece.SetAnimating(true);
            }
        }

        /// <summary>Remove a piece from spring tracking (call when returning to pool).</summary>
        public void UnregisterPiece(Piece piece)
        {
            springStates.Remove(piece);
        }

        /// <summary>True when all tracked springs have settled.</summary>
        public bool IsAllSettled()
        {
            foreach (var state in springStates.Values)
                if (!state.settled) return false;
            return true;
        }

        /// <summary>True when this specific piece's spring has settled (or piece is untracked).</summary>
        public bool IsSettled(Piece piece)
        {
            return !springStates.ContainsKey(piece) || springStates[piece].settled;
        }

        // ==================== TWEEN API ====================

        public void PlaySwapAnimation(Piece piece1, Piece piece2)
        {
            piece1.SetAnimating(true);
            piece2.SetAnimating(true);

            activeAnimations.Add(new PieceAnimation
            {
                type = PieceAnimation.AnimationType.Swap,
                piece = piece1,
                otherPiece = piece2,
                startPos = piece1.transform.localPosition,
                endPos = piece2.transform.localPosition,
                otherStartPos = piece2.transform.localPosition,
                otherEndPos = piece1.transform.localPosition,
                duration = swapDuration,
                elapsed = 0f
            });
        }

        public void PlayPopAnimation(Piece piece)
        {
            piece.SetAnimating(true);
            activeAnimations.Add(new PieceAnimation
            {
                type = PieceAnimation.AnimationType.Pop,
                piece = piece,
                originalScale = piece.transform.localScale,
                duration = popDuration,
                elapsed = 0f
            });
        }

        public void PlaySpawnAnimation(Piece piece)
        {
            piece.SetAnimating(true);
            piece.transform.localScale = Vector3.zero;
            activeAnimations.Add(new PieceAnimation
            {
                type = PieceAnimation.AnimationType.Spawn,
                piece = piece,
                originalScale = piece.transform.localScale,
                duration = spawnDuration,
                elapsed = 0f
            });
        }

        // ==================== TWEEN IMPLEMENTATIONS ====================

        private void UpdateSwapAnimation(PieceAnimation anim, float t)
        {
            float easeT = t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

            anim.piece.transform.localPosition = Vector3.Lerp(anim.startPos, anim.endPos, easeT);
            anim.otherPiece.transform.localPosition = Vector3.Lerp(anim.otherStartPos, anim.otherEndPos, easeT);

            if (anim.IsComplete)
            {
                anim.piece.transform.localPosition = anim.endPos;
                anim.otherPiece.transform.localPosition = anim.otherEndPos;
                anim.piece.UpdatePosition();
                anim.otherPiece.UpdatePosition();
                anim.piece.SetAnimating(false);
                anim.otherPiece.SetAnimating(false);
            }
        }

        private void UpdatePopAnimation(PieceAnimation anim, float t)
        {
            if (t < 0.4f)
            {
                float growT = t / 0.4f;
                anim.piece.transform.localScale = Vector3.Lerp(anim.originalScale, anim.originalScale * 1.2f, growT);
            }
            else
            {
                float shrinkT = (t - 0.4f) / 0.6f;
                anim.piece.transform.localScale = Vector3.Lerp(anim.originalScale * 1.2f, Vector3.zero, shrinkT);

                Renderer rend = anim.piece.GetComponent<Renderer>();
                if (rend != null)
                {
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    rend.GetPropertyBlock(block);
                    Color c = rend.material.color;
                    c.a = Mathf.Lerp(1f, 0f, shrinkT);
                    block.SetColor("_Color", c);
                    rend.SetPropertyBlock(block);
                }
            }

            if (anim.IsComplete) anim.piece.SetAnimating(false);
        }

        private void UpdateSpawnAnimation(PieceAnimation anim, float t)
        {
            float easeT = 1f + 2.7f * Mathf.Pow(t - 1f, 3f) + 1.7f * Mathf.Pow(t - 1f, 2f);
            anim.piece.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, anim.originalScale, easeT);

            if (anim.IsComplete)
            {
                anim.piece.transform.localScale = anim.originalScale;
                anim.piece.SetAnimating(false);
            }
        }

        // ==================== LEGACY STUBS ====================

        public IEnumerator PlayReactionAnimation(Piece piece, Vector3 direction) { yield return null; }
        public IEnumerator PlayLandParticle(Vector3 position) { yield return null; }
        public IEnumerator PlayDynamicMovement(Piece piece, Vector3 startPos, Vector3 targetPos, float duration) { yield return null; }
        public void ApplySuddenForce(Vector3 force) { }
        public IEnumerator PlayFallAnimationDynamic(Piece piece, Vector3 fromPos, Vector3 toPos) { yield return null; }
        public IEnumerator PlaySwapAnimationDynamic(Piece piece1, Piece piece2) { yield return null; }
    }
}

