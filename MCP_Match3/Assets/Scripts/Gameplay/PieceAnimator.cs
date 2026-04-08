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

        public IEnumerator PlaySwapAnimation(Piece piece1, Piece piece2)
        {
            Vector3 pos1 = piece1.transform.localPosition;
            Vector3 pos2 = piece2.transform.localPosition;
            float elapsed = 0;

            while (elapsed < swapDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / swapDuration;
                
                piece1.transform.localPosition = Vector3.Lerp(pos1, pos2, t);
                piece2.transform.localPosition = Vector3.Lerp(pos2, pos1, t);
                
                yield return null;
            }

            piece1.transform.localPosition = pos2;
            piece2.transform.localPosition = pos1;
        }

        public IEnumerator PlayFallAnimation(Piece piece, Vector3 fromPos, Vector3 toPos)
        {
            piece.transform.localPosition = fromPos;
            float elapsed = 0;

            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fallDuration;
                
                // Easing: ease-out quad
                float easeT = 1f - (1f - t) * (1f - t);
                piece.transform.localPosition = Vector3.Lerp(fromPos, toPos, easeT);
                
                yield return null;
            }

            piece.transform.localPosition = toPos;
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

        public IEnumerator PlayLandParticle(Vector3 position)
        {
            // TODO: Instanciar particle system
            Debug.Log($"Landing particle at {position}");
            yield return null;
        }
    }
}
