using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Match3.Data;
using Match3.Core;
using Match3.Animation;
using Match3.Input;

namespace Match3.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private BoardController boardController;
        [SerializeField] private PieceAnimator pieceAnimator;
        [SerializeField] private Transform boardParent;

        [Header("Piece Prefabs (one per type)")]
        [SerializeField] private GameObject redPiecePrefab;
        [SerializeField] private GameObject bluePiecePrefab;
        [SerializeField] private GameObject greenPiecePrefab;
        [SerializeField] private GameObject yellowPiecePrefab;
        
        private Dictionary<(int, int), Piece> piecesOnBoard;
        private Dictionary<PieceType, Queue<Piece>> pool;
        private bool isProcessing;

        public bool IsProcessing => isProcessing;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (boardController == null)
            {
                Debug.LogError("BoardController not assigned!");
                return;
            }

            boardController.Initialize();
            piecesOnBoard = new Dictionary<(int, int), Piece>();
            pool = new Dictionary<PieceType, Queue<Piece>>
            {
                { PieceType.Red, new Queue<Piece>() },
                { PieceType.Blue, new Queue<Piece>() },
                { PieceType.Green, new Queue<Piece>() },
                { PieceType.Yellow, new Queue<Piece>() }
            };
            
            SpawnBoard();
        }

        private void SpawnBoard()
        {
            for (int x = 0; x < boardController.Width; x++)
            {
                for (int y = 0; y < boardController.Height; y++)
                {
                    PieceData data = boardController.GetPiece(x, y);
                    if (data.type != PieceType.Empty)
                    {
                        Piece piece = GetFromPool(data.type);
                        piece.Initialize(data);
                        piecesOnBoard[(x, y)] = piece;
                    }
                }
            }
        }

        private GameObject GetPrefabForType(PieceType type)
        {
            return type switch
            {
                PieceType.Red => redPiecePrefab,
                PieceType.Blue => bluePiecePrefab,
                PieceType.Green => greenPiecePrefab,
                PieceType.Yellow => yellowPiecePrefab,
                _ => null
            };
        }

        private Piece GetFromPool(PieceType type)
        {
            if (pool.TryGetValue(type, out var queue) && queue.Count > 0)
            {
                Piece piece = queue.Dequeue();
                piece.gameObject.SetActive(true);
                piece.ResetVisuals();
                return piece;
            }

            GameObject prefab = GetPrefabForType(type);
            GameObject go = Instantiate(prefab, boardParent);
            return go.GetComponent<Piece>();
        }

        private void ReturnToPool(Piece piece)
        {
            if (piece == null) return;
            piece.gameObject.SetActive(false);
            PieceType type = piece.Data.type;
            if (pool.ContainsKey(type))
                pool[type].Enqueue(piece);
        }

        public Piece GetPieceAt(int x, int y)
        {
            piecesOnBoard.TryGetValue((x, y), out Piece piece);
            return piece;
        }

        public void SwapPieces(Piece piece1, Piece piece2)
        {
            if (isProcessing)
                return;

            StartCoroutine(SwapPiecesCoroutine(piece1, piece2));
        }

        private IEnumerator SwapPiecesCoroutine(Piece piece1, Piece piece2)
        {
            isProcessing = true;
            
            int x1 = piece1.Data.x;
            int y1 = piece1.Data.y;
            int x2 = piece2.Data.x;
            int y2 = piece2.Data.y;

            // Swap data
            boardController.SwapPieces(x1, y1, x2, y2);
            
            // Animate swap
            yield return StartCoroutine(pieceAnimator.PlaySwapAnimation(piece1, piece2));
            
            // Update visual dictionary
            piecesOnBoard[(x2, y2)] = piece1;
            piecesOnBoard[(x1, y1)] = piece2;

            // Check matches
            List<PieceData> matches = boardController.FindMatches();
            
            if (matches.Count > 0)
            {
                yield return StartCoroutine(ProcessMatchesLoop());
            }
            else
            {
                // Revert swap
                boardController.SwapPieces(x1, y1, x2, y2);
                yield return StartCoroutine(pieceAnimator.PlaySwapAnimation(piece1, piece2));
                
                piecesOnBoard[(x1, y1)] = piece1;
                piecesOnBoard[(x2, y2)] = piece2;
            }

            isProcessing = false;
        }

        private IEnumerator ProcessMatchesLoop()
        {
            List<PieceData> matches = boardController.FindMatches();

            while (matches.Count > 0)
            {
                boardController.MarkPiecesForRemoval(matches);

                // Animate pops (all in parallel)
                var popCoroutines = new List<Coroutine>();
                foreach (var data in matches)
                {
                    if (piecesOnBoard.TryGetValue((data.x, data.y), out Piece piece))
                    {
                        popCoroutines.Add(StartCoroutine(pieceAnimator.PlayPopAnimation(piece)));
                    }
                }
                foreach (var c in popCoroutines)
                    yield return c;

                // Return popped pieces to pool
                foreach (var data in matches)
                {
                    if (piecesOnBoard.TryGetValue((data.x, data.y), out Piece piece))
                    {
                        ReturnToPool(piece);
                        piecesOnBoard.Remove((data.x, data.y));
                    }
                }

                boardController.RemoveMarkedPieces();

                // Gravity
                var gravityMoves = boardController.ApplyGravity();
                yield return StartCoroutine(AnimateGravity(gravityMoves));

                // Fill empty spaces with new pieces
                var newPieces = boardController.FillEmptySpaces();
                yield return StartCoroutine(AnimateFill(newPieces));

                // Check for cascade matches
                matches = boardController.FindMatches();
            }
        }

        private IEnumerator AnimateGravity(List<(int x, int fromY, int toY)> movements)
        {
            var coroutines = new List<Coroutine>();

            foreach (var (x, fromY, toY) in movements)
            {
                if (piecesOnBoard.TryGetValue((x, fromY), out Piece piece))
                {
                    piecesOnBoard.Remove((x, fromY));
                    piecesOnBoard[(x, toY)] = piece;

                    Vector3 from = new Vector3(x, fromY, 0);
                    Vector3 to = new Vector3(x, toY, 0);
                    coroutines.Add(StartCoroutine(pieceAnimator.PlayFallAnimation(piece, from, to)));
                }
            }

            foreach (var c in coroutines)
                yield return c;
        }

        private IEnumerator AnimateFill(List<PieceData> newPieces)
        {
            var coroutines = new List<Coroutine>();

            foreach (var data in newPieces)
            {
                Piece piece = GetFromPool(data.type);
                piece.Initialize(data);
                piecesOnBoard[(data.x, data.y)] = piece;

                Vector3 spawnPos = new Vector3(data.x, boardController.Height + 1, 0);
                Vector3 targetPos = new Vector3(data.x, data.y, 0);
                coroutines.Add(StartCoroutine(pieceAnimator.PlayFallAnimation(piece, spawnPos, targetPos)));
            }

            foreach (var c in coroutines)
                yield return c;
        }
    }
}
