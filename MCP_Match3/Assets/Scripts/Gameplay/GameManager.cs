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
    [Header("Level")]
    [SerializeField] private int currentLevelNumber = 1;
        [SerializeField] private BoardController boardController;
        [SerializeField] private PieceAnimator pieceAnimator;
        [SerializeField] private Transform boardParent;

        [Header("Piece Prefabs (one per type)")]
        [SerializeField] private GameObject redPiecePrefab;
        [SerializeField] private GameObject bluePiecePrefab;
        [SerializeField] private GameObject greenPiecePrefab;
        [SerializeField] private GameObject yellowPiecePrefab;

        [Header("Debug Visualization")]
        [SerializeField] private bool showGridVisuals = true;
        [SerializeField] private Color gridColor = new Color(0, 1, 0, 0.5f);
        
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
            // Disabled: SetupDebugUI(); - Using per-piece position labels instead
        }

        private void SetupDebugUI()
        {
            Debug.Log("[GameManager] SetupDebugUI() called!");
            
            // Find canvas and add DebugUI if not present
            Canvas canvas = FindObjectOfType<Canvas>();
            
            if (canvas == null)
            {
                Debug.LogWarning("[GameManager] Canvas NOT FOUND!");
                return;
            }
            
            Debug.Log("[GameManager] Canvas found!");
            
            if (canvas.GetComponent<Match3.UI.DebugUI>() != null)
            {
                Debug.Log("[GameManager] DebugUI already exists!");
                return;
            }
            
            Debug.Log("[GameManager] Adding DebugUI component to Canvas...");
            canvas.gameObject.AddComponent<Match3.UI.DebugUI>();
            Debug.Log("[GameManager] DebugUI component added successfully!");
        }

        private void Initialize()
        {
            if (boardController == null)
            {
                Debug.LogError("BoardController not assigned!");
                return;
            }

            boardController.Initialize(currentLevelNumber);
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

        /// <summary>
        /// Public method to load a specific level and restart the game.
        /// </summary>
        public void LoadLevel(int levelNumber)
        {
            currentLevelNumber = levelNumber;
            ClearBoard();
            Initialize();
        }

        /// <summary>
        /// Clears the current board and returns all pieces to pool.
        /// </summary>
        private void ClearBoard()
        {
            foreach (var piece in piecesOnBoard.Values)
            {
                if (piece != null)
                {
                    ReturnToPool(piece);
                }
            }
            piecesOnBoard.Clear();
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
                        piece.gameObject.SetActive(true); // Make initial pieces visible
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
                piece.gameObject.SetActive(false); // Invisible until positioned correctly
                piece.ResetVisuals();
                return piece;
            }

            GameObject prefab = GetPrefabForType(type);
            GameObject go = Instantiate(prefab, boardParent);
            go.SetActive(false); // Keep invisible until positioned
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

            // Ensure pieces are synchronized
            piece1.UpdatePosition();
            piece2.UpdatePosition();

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

                // Ensure pieces are synchronized
                piece1.UpdatePosition();
                piece2.UpdatePosition();
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

            // Ensure all pieces are synchronized with grid after processing
            SynchronizeAllPieces();
        }

        private void SynchronizeAllPieces()
        {
            foreach (var piece in piecesOnBoard.Values)
            {
                if (piece != null && piece.gameObject.activeSelf)
                {
                    piece.UpdatePosition();
                }
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

            // Ensure all pieces in gravity are synchronized
            foreach (var (x, fromY, toY) in movements)
            {
                if (piecesOnBoard.TryGetValue((x, toY), out Piece piece) && piece != null)
                {
                    piece.UpdatePosition();
                }
            }
        }

        private IEnumerator AnimateFill(List<PieceData> newPieces)
        {
            // Cascade effect: piezas caen una tras otra, no todas simultáneamente
            float cascadeDelay = 0.05f; // Delay entre cada pieza
            float elapsedDelay = 0;

            foreach (var data in newPieces)
            {
                Piece piece = GetFromPool(data.type);
                piece.Initialize(data);
                piecesOnBoard[(data.x, data.y)] = piece;

                Vector3 spawnPos = new Vector3(data.x, boardController.Height + 1, 0);
                Vector3 targetPos = new Vector3(data.x, data.y, 0);
                
                // Start fall animation con delay
                StartCoroutine(PlayFallAndReactWithDelay(piece, spawnPos, targetPos, data.x, data.y, elapsedDelay));
                elapsedDelay += cascadeDelay;
            }

            // Wait for all pieces to land
            yield return new WaitForSeconds(elapsedDelay + pieceAnimator.GetFallDuration());

            // Ensure all new pieces are synchronized
            foreach (var data in newPieces)
            {
                if (piecesOnBoard.TryGetValue((data.x, data.y), out Piece piece) && piece != null)
                {
                    piece.UpdatePosition();
                }
            }
        }

        private IEnumerator PlayFallAndReactWithDelay(Piece piece, Vector3 spawnPos, Vector3 targetPos, int gridX, int gridY, float delay)
        {
            yield return new WaitForSeconds(delay);
            yield return StartCoroutine(PlayFallAndReact(piece, spawnPos, targetPos, gridX, gridY));
        }

        private IEnumerator PlayFallAndReact(Piece piece, Vector3 spawnPos, Vector3 targetPos, int gridX, int gridY)
        {
            // Activate piece before animating (was invisible in pool)
            piece.gameObject.SetActive(true);
            
            // Caer - but don't trigger reactions for filling pieces
            yield return StartCoroutine(pieceAnimator.PlayFallAnimation(piece, spawnPos, targetPos));
            
            // Note: NO reactions for pieces during fill - only during gravity from combos
        }

        // Reactions disabled - keeping cleaner animation flow
        private IEnumerator TriggerAdjacentReactions(int gridX, int gridY)
        {
            // Placeholder - reactions removed for cleaner animations
            yield return null;
        }

        // Debug visualization of grid positions
        private void OnDrawGizmos()
        {
            if (!showGridVisuals || boardController == null)
                return;

            Gizmos.color = gridColor;
            
            // Draw grid sphere markers
            for (int x = 0; x < boardController.Width; x++)
            {
                for (int y = 0; y < boardController.Height; y++)
                {
                    Vector3 gridPos = new Vector3(x, y, 0);
                    Gizmos.DrawWireSphere(gridPos, 0.1f);
                }
            }

            // Draw grid lines
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            for (int x = 0; x <= boardController.Width; x++)
            {
                Gizmos.DrawLine(new Vector3(x - 0.5f, -0.5f, 0), new Vector3(x - 0.5f, boardController.Height - 0.5f, 0));
            }
            for (int y = 0; y <= boardController.Height; y++)
            {
                Gizmos.DrawLine(new Vector3(-0.5f, y - 0.5f, 0), new Vector3(boardController.Width - 0.5f, y - 0.5f, 0));
            }
        }
    }
}
