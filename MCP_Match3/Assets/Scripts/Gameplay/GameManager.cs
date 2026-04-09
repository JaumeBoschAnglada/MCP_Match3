using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Match3.Data;
using Match3.Core;
using Match3.Animation;
using Match3.Input;
using Match3.Effects;

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

        [Header("Piece Prefabs (one per color)")]
        [SerializeField] private GameObject redPiecePrefab;
        [SerializeField] private GameObject bluePiecePrefab;
        [SerializeField] private GameObject greenPiecePrefab;
        [SerializeField] private GameObject yellowPiecePrefab;

        [Header("Special Piece Prefabs (Special_Horizontal per color)")]
        [SerializeField] private GameObject specialHorizontalRedPrefab;
        [SerializeField] private GameObject specialHorizontalBluePrefab;
        [SerializeField] private GameObject specialHorizontalGreenPrefab;
        [SerializeField] private GameObject specialHorizontalYellowPrefab;

        [Header("Special Piece Prefabs (Special_Vertical per color)")]
        [SerializeField] private GameObject specialVerticalRedPrefab;
        [SerializeField] private GameObject specialVerticalBluePrefab;
        [SerializeField] private GameObject specialVerticalGreenPrefab;
        [SerializeField] private GameObject specialVerticalYellowPrefab;

        [Header("Debug Visualization")]
        [SerializeField] private bool showGridVisuals = true;
        [SerializeField] private Color gridColor = new Color(0, 1, 0, 0.5f);
        
        private Dictionary<(int, int), Piece> piecesOnBoard;
        private Dictionary<(ColorType, SpecialEffect), Queue<Piece>> pool;
        private bool isProcessing;

        public bool IsProcessing => isProcessing;

        public int GetCurrentLevelNumber() => currentLevelNumber;

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

        private void Update()
        {
            // Keep idle pieces tracked so they spring back if moved manually in the editor
            if (isProcessing || piecesOnBoard == null) return;
            foreach (var kvp in piecesOnBoard)
            {
                Piece piece = kvp.Value;
                if (piece == null || !piece.gameObject.activeSelf) continue;
                Vector3 targetPos = new Vector3(piece.Data.x, piece.Data.y, 0);
                pieceAnimator.EnsureTracked(piece, targetPos);
            }
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
            pool = new Dictionary<(ColorType, SpecialEffect), Queue<Piece>>();
            foreach (ColorType color in new[] { ColorType.Red, ColorType.Blue, ColorType.Green, ColorType.Yellow })
                foreach (SpecialEffect effect in new[] { SpecialEffect.None, SpecialEffect.HorizontalRow, SpecialEffect.VerticalRow })
                    pool[(color, effect)] = new Queue<Piece>();
            
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
                    if (data.colorType != ColorType.Empty)
                    {                      
                        Piece piece = GetFromPool(data);
                        piece.Initialize(data);
                        piece.gameObject.SetActive(true);
                        piecesOnBoard[(x, y)] = piece;
                        pieceAnimator.PlaySpawnAnimation(piece);
                    }
                }
            }
        }

        private GameObject GetPrefabForData(PieceData data)
        {
            if (data.specialEffect == SpecialEffect.HorizontalRow)
            {
                var prefab = data.colorType switch
                {
                    ColorType.Red    => specialHorizontalRedPrefab,
                    ColorType.Blue   => specialHorizontalBluePrefab,
                    ColorType.Green  => specialHorizontalGreenPrefab,
                    ColorType.Yellow => specialHorizontalYellowPrefab,
                    _ => null
                };
                if (prefab == null)
                    Debug.LogError($"[GameManager] Special_Horizontal prefab not assigned for color {data.colorType}!");
                return prefab;
            }

            if (data.specialEffect == SpecialEffect.VerticalRow)
            {
                var prefab = data.colorType switch
                {
                    ColorType.Red    => specialVerticalRedPrefab,
                    ColorType.Blue   => specialVerticalBluePrefab,
                    ColorType.Green  => specialVerticalGreenPrefab,
                    ColorType.Yellow => specialVerticalYellowPrefab,
                    _ => null
                };
                if (prefab == null)
                    Debug.LogError($"[GameManager] Special_Vertical prefab not assigned for color {data.colorType}!");
                return prefab;
            }

            return data.colorType switch
            {
                ColorType.Red    => redPiecePrefab,
                ColorType.Blue   => bluePiecePrefab,
                ColorType.Green  => greenPiecePrefab,
                ColorType.Yellow => yellowPiecePrefab,
                _ => null
            };
        }

        private Piece GetFromPool(PieceData data)
        {
            var key = (data.colorType, data.specialEffect);

            if (pool.TryGetValue(key, out var queue) && queue.Count > 0)
            {
                Piece piece = queue.Dequeue();
                piece.gameObject.SetActive(false);
                piece.ResetVisuals();
                Debug.Log($"[GameManager] Got {data.specialEffect} {data.colorType} from pool");
                return piece;
            }

            GameObject prefab = GetPrefabForData(data);
            if (prefab == null)
            {
                Debug.LogError($"[GameManager] CRITICAL: No prefab found for {data.colorType}/{data.specialEffect}");
                return null;
            }

            Debug.Log($"[GameManager] Instantiating prefab for {data.colorType}/{data.specialEffect}: {prefab.name}");
            GameObject go = Instantiate(prefab, boardParent);
            if (go == null)
            {
                Debug.LogError($"[GameManager] Instantiate failed for {prefab.name}");
                return null;
            }
            
            Piece piece2 = go.GetComponent<Piece>();
            if (piece2 == null)
            {
                Debug.LogError($"[GameManager] Instantiated object has no Piece component: {go.name}");
                return null;
            }
            
            go.SetActive(false);
            Debug.Log($"[GameManager] Successfully created {piece2.GetType().Name} at {go.name}");
            return piece2;
        }

        private void ReturnToPool(Piece piece)
        {
            if (piece == null) return;
            
            Debug.Log($"[GameManager] 🔄 ReturnToPool called for {piece.Data.colorType} at ({piece.Data.x},{piece.Data.y}), IsAnimating={piece.IsAnimating}");
            
            // If piece is animating (pop), wait for animation to finish before deactivating
            if (piece.IsAnimating)
            {
                StartCoroutine(ReturnToPoolAfterAnimation(piece));
                return;
            }
            
            // Spawn effect before returning to pool
            ColorType colorType = piece.Data.colorType;
            if (colorType != ColorType.Empty)
            {
                if (EffectManager.Instance != null)
                {
                    Vector3 effectPosition = piece.transform.position;
                    EffectManager.Instance.SpawnEffect(effectPosition, colorType);
                }
            }
            
            piece.gameObject.SetActive(false);
            pieceAnimator.UnregisterPiece(piece);
            var key = (piece.Data.colorType, piece.Data.specialEffect);
            if (pool.ContainsKey(key))
            {
                pool[key].Enqueue(piece);
                Debug.Log($"[GameManager] ✅ Returned {piece.Data.specialEffect} {piece.Data.colorType} to pool");
            }
        }

        private IEnumerator ReturnToPoolAfterAnimation(Piece piece)
        {
            Debug.Log($"[GameManager] Waiting for pop animation to complete on {piece.gameObject.name}");
            
            // Wait until piece is no longer animating or is inactive
            float timeout = 1.0f;
            float elapsed = 0f;
            while (piece != null && piece.IsAnimating && piece.gameObject.activeSelf && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (piece != null)
            {
                Debug.Log($"[GameManager] Pop animation complete, returning {piece.gameObject.name} to pool");
                // Now safe to deactivate
                ColorType colorType = piece.Data.colorType;
                if (colorType != ColorType.Empty)
                {
                    if (EffectManager.Instance != null)
                    {
                        Vector3 effectPosition = piece.transform.position;
                        EffectManager.Instance.SpawnEffect(effectPosition, colorType);
                    }
                }
                
                piece.gameObject.SetActive(false);
                pieceAnimator.UnregisterPiece(piece);
                var key = (piece.Data.colorType, piece.Data.specialEffect);
                if (pool.ContainsKey(key))
                    pool[key].Enqueue(piece);
            }
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
            
            // Animate swap (now handled by Update, so we just wait for duration)
            pieceAnimator.PlaySwapAnimation(piece1, piece2);
            yield return new WaitForSeconds(0.2f); // swapDuration
            
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
                pieceAnimator.PlaySwapAnimation(piece1, piece2);
                yield return new WaitForSeconds(0.2f); // swapDuration
                
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
                bool hasExistingSpecialPiece = matches.Exists(p => p.specialEffect != SpecialEffect.None);
                List<PieceData> piecesToEliminate;

                if (hasExistingSpecialPiece)
                {
                    // Existing special piece activated (row/column sweep)
                    PieceData specialData = matches.Find(p => p.specialEffect != SpecialEffect.None);
                    
                    Debug.Log($"[GameManager] 🔥 Special piece activation at ({specialData.x},{specialData.y}) with type {specialData.specialEffect}");
                    
                    // Get ALL pieces in the row/column that should be eliminated
                    piecesToEliminate = SpecialPieceEffects.ApplySpecialEffects(matches, boardController.Grid, boardController.Width, boardController.Height);
                    
                    // Get elimination order (from center outward)
                    List<PieceData> eliminationOrder = SpecialPieceEffects.GetEliminationOrder(piecesToEliminate, specialData);

                    Debug.Log($"[GameManager] Elimination order: {eliminationOrder.Count} pieces");

                    // Collect visual pieces for animation
                    List<Piece> piecesToAnimate = new List<Piece>();
                    foreach (var data in eliminationOrder)
                    {
                        if (piecesOnBoard.TryGetValue((data.x, data.y), out Piece p))
                        {
                            piecesToAnimate.Add(p);
                            Debug.Log($"[GameManager] Added piece from piecesOnBoard: ({data.x},{data.y})");
                        }
                        else
                        {
                            Debug.LogWarning($"[GameManager] ⚠️ Piece ({data.x},{data.y}) NOT FOUND in piecesOnBoard!");
                        }
                    }

                    Piece specialPiece = null;
                    piecesOnBoard.TryGetValue((specialData.x, specialData.y), out specialPiece);

                    Debug.Log($"[GameManager] Starting elimination animations for {piecesToAnimate.Count} pieces");

                    SpecialPieceAnimator specialAnimator = GetComponent<SpecialPieceAnimator>();
                    if (specialAnimator == null) specialAnimator = gameObject.AddComponent<SpecialPieceAnimator>();
                    
                    // Pass PieceAnimator reference so it can queue animations
                    specialAnimator.SetPieceAnimator(pieceAnimator);
                    
                    // Wait for all elimination animations to complete
                    yield return StartCoroutine(specialAnimator.EliminateGradually(specialPiece, piecesToAnimate));

                    Debug.Log($"[GameManager] ✅ Animation phase complete, marking pieces for removal");
                    boardController.MarkPiecesForRemoval(piecesToEliminate);
                }
                else
                {
                    // Check if any 4+ match creates a new special piece
                    List<PieceData> newSpecials;
                    piecesToEliminate = SpecialPieceCreator.CreateSpecialPiecesFromMatches(
                        new List<PieceData>(matches),
                        boardController.Grid,
                        boardController.Width,
                        boardController.Height,
                        out newSpecials);

                    // Swap visual for each newly created special piece
                    // Pass all matched pieces so we can animate the components converging to center
                    foreach (var specialData in newSpecials)
                        yield return StartCoroutine(VisualSwapToSpecialPiece(specialData, matches));

                    // Animate normal pop for pieces that get eliminated
                    boardController.MarkPiecesForRemoval(piecesToEliminate);
                    foreach (var data in piecesToEliminate)
                        if (piecesOnBoard.TryGetValue((data.x, data.y), out Piece p)) pieceAnimator.PlayPopAnimation(p);

                    yield return new WaitForSeconds(0.4f);
                }

                // Return eliminated pieces to pool
                Debug.Log($"[GameManager] 🗑️ Returning {piecesToEliminate.Count} eliminated pieces to pool");
                foreach (var data in piecesToEliminate)
                {
                    if (piecesOnBoard.TryGetValue((data.x, data.y), out Piece piece))
                    {
                        ReturnToPool(piece);
                        piecesOnBoard.Remove((data.x, data.y));
                        Debug.Log($"[GameManager] Removed ({data.x},{data.y}) from piecesOnBoard");
                    }
                    else
                    {
                        Debug.LogWarning($"[GameManager] ❌ Piece at ({data.x},{data.y}) NOT in piecesOnBoard!");
                    }
                }

                boardController.RemoveMarkedPieces();

                // Synchronize board state after removal
                SynchronizeAllPieces();

                // Gravity + fill
                var gravityMoves = boardController.ApplyGravity();
                StartGravityAnimations(gravityMoves);

                var newPieces = boardController.FillEmptySpaces();
                StartFillAnimations(newPieces);

                yield return new WaitUntil(() => pieceAnimator.IsAllSettled());

                // Final synchronization after all animations complete
                foreach (var (x, fromY, toY) in gravityMoves)
                    if (piecesOnBoard.TryGetValue((x, toY), out Piece gp) && gp != null) gp.UpdatePosition();
                foreach (var data in newPieces)
                    if (piecesOnBoard.TryGetValue((data.x, data.y), out Piece fp) && fp != null) fp.UpdatePosition();

                matches = boardController.FindMatches();
            }

            // Final board synchronization
            SynchronizeAllPieces();
        }

        private IEnumerator VisualSwapToSpecialPiece(PieceData specialData, List<PieceData> matchedPieces)
        {
            Debug.Log($"[GameManager] VisualSwap starting for {specialData.colorType}/{specialData.specialEffect} at ({specialData.x}, {specialData.y})");
            Vector3 worldPos = new Vector3(specialData.x, specialData.y, 0);
            Vector3 centerPos = new Vector3(specialData.x, specialData.y, 0);

            // Spawn creation effect
            if (EffectManager.Instance != null)
                EffectManager.Instance.SpawnCreationEffect(worldPos, specialData.colorType);

            // Find the OTHER 3 pieces that form this special (not the center piece)
            List<Piece> componentPieces = new List<Piece>();
            if (specialData.specialEffect == SpecialEffect.HorizontalRow)
            {
                // Find other pieces in the same row
                foreach (var data in matchedPieces)
                {
                    if (data.y == specialData.y && !(data.x == specialData.x && data.y == specialData.y))
                        if (piecesOnBoard.TryGetValue((data.x, data.y), out Piece p) && p != null)
                            componentPieces.Add(p);
                }
            }
            else if (specialData.specialEffect == SpecialEffect.VerticalRow)
            {
                // Find other pieces in the same column
                foreach (var data in matchedPieces)
                {
                    if (data.x == specialData.x && !(data.x == specialData.x && data.y == specialData.y))
                        if (piecesOnBoard.TryGetValue((data.x, data.y), out Piece p) && p != null)
                            componentPieces.Add(p);
                }
            }

            // Animate component pieces moving toward center using spring physics
            float componentAnimDuration = 0.3f;
            foreach (var componentPiece in componentPieces)
            {
                if (componentPiece != null && componentPiece.gameObject.activeSelf)
                {
                    Debug.Log($"[GameManager] Moving component piece from ({componentPiece.Data.x}, {componentPiece.Data.y}) to center ({specialData.x}, {specialData.y})");
                    Vector3 componentStartPos = componentPiece.transform.localPosition;
                    pieceAnimator.PlayFallAnimation(componentPiece, componentStartPos, centerPos);
                }
            }

            // Return old normal piece to pool (no destroy effect)
            if (piecesOnBoard.TryGetValue((specialData.x, specialData.y), out Piece oldPiece))
            {
                Debug.Log($"[GameManager] Found old piece at ({specialData.x}, {specialData.y}): {oldPiece.GetType().Name}");
                oldPiece.gameObject.SetActive(false);
                pieceAnimator.UnregisterPiece(oldPiece);
                var oldKey = (specialData.colorType, SpecialEffect.None);
                if (pool.ContainsKey(oldKey)) pool[oldKey].Enqueue(oldPiece);
            }
            else
            {
                Debug.LogWarning($"[GameManager] No old piece found at ({specialData.x}, {specialData.y})");
            }

            // Spawn new special piece visual
            Piece newPiece = GetFromPool(specialData);
            if (newPiece != null)
            {
                Debug.Log($"[GameManager] Got new piece: {newPiece.GetType().Name}");
                newPiece.Initialize(specialData);
                newPiece.gameObject.SetActive(true);
                piecesOnBoard[(specialData.x, specialData.y)] = newPiece;
                pieceAnimator.PlaySpawnAnimation(newPiece);
                Debug.Log($"[GameManager] Swap complete: {newPiece.GetType().Name} at ({specialData.x}, {specialData.y})");
            }
            else
            {
                Debug.LogError($"[GameManager] Failed to get new piece for {specialData.colorType}/{specialData.specialEffect}");
            }

            yield return new WaitForSeconds(componentAnimDuration);
        }

        private void SynchronizeAllPieces()
        {
            Debug.Log($"[GameManager] 🔄 SynchronizeAllPieces: Before sync - piecesOnBoard has {piecesOnBoard.Count} entries");
            
            // First, remove pieces that no longer exist in grid or are marked as Empty
            var keysToRemove = new List<(int, int)>();
            foreach (var kvp in piecesOnBoard)
            {
                var (x, y) = kvp.Key;
                Piece piece = kvp.Value;
                
                if (piece == null || !piece.gameObject.activeSelf)
                {
                    Debug.LogWarning($"[GameManager] ⚠️ Removing dead piece at ({x},{y}): piece={piece?.name ?? "null"}, activeSelf={piece?.gameObject.activeSelf}");
                    keysToRemove.Add((x, y));
                    continue;
                }

                // Check if this piece still exists in the grid at the expected position
                PieceData gridData = boardController.GetPiece(x, y);
                if (gridData.colorType == ColorType.Empty)
                {
                    Debug.LogWarning($"[GameManager] ⚠️ Grid mismatch at ({x},{y}): piecesOnBoard has {piece.Data.colorType} but grid is Empty");
                    keysToRemove.Add((x, y));
                    continue;
                }
                
                // Verify the piece data matches the grid
                if (piece.Data.x != x || piece.Data.y != y)
                {
                    Debug.LogWarning($"[GameManager] ⚠️ Position mismatch: piece.Data=({piece.Data.x},{piece.Data.y}) but dictionary key=({x},{y})");
                    keysToRemove.Add((x, y));
                    continue;
                }
            }

            // Remove invalid entries
            foreach (var key in keysToRemove)
            {
                Debug.Log($"[GameManager] Removing invalid entry at {key}");
                piecesOnBoard.Remove(key);
            }

            // Update position for valid pieces
            foreach (var piece in piecesOnBoard.Values)
            {
                if (piece != null && piece.gameObject.activeSelf)
                {
                    piece.UpdatePosition();
                }
            }
            
            Debug.Log($"[GameManager] ✅ SynchronizeAllPieces: After sync - piecesOnBoard has {piecesOnBoard.Count} entries");
        }

        private void StartGravityAnimations(List<(int x, int fromY, int toY)> movements)
        {
            foreach (var (x, fromY, toY) in movements)
            {
                if (piecesOnBoard.TryGetValue((x, fromY), out Piece piece) && piece != null)
                {
                    piecesOnBoard.Remove((x, fromY));
                    piecesOnBoard[(x, toY)] = piece;
                    pieceAnimator.PlayFallAnimation(piece, new Vector3(x, fromY, 0), new Vector3(x, toY, 0));
                    Debug.Log($"[GameManager] 📍 Gravity: ({x},{fromY}) → ({x},{toY}) | {piece.Data.colorType}");
                }
                else
                {
                    Debug.LogWarning($"[GameManager] ❌ No piece found at ({x},{fromY}) for gravity");
                }
            }
        }

        private void StartFillAnimations(List<PieceData> newPieces)
        {
            foreach (var data in newPieces)
            {
                Piece piece = GetFromPool(data);
                piece.Initialize(data);
                
                if (piecesOnBoard.ContainsKey((data.x, data.y)))
                {
                    Debug.LogError($"[GameManager] ❌ DUPLICATE KEY: ({data.x},{data.y}) already exists in piecesOnBoard!");
                }
                
                piecesOnBoard[(data.x, data.y)] = piece;
                piece.gameObject.SetActive(true);
                Vector3 spawnPos = new Vector3(data.x, boardController.Height + 1, 0);
                Vector3 targetPos = new Vector3(data.x, data.y, 0);
                pieceAnimator.PlayFallAnimation(piece, spawnPos, targetPos);
                Debug.Log($"[GameManager] 🆕 Fill at ({data.x},{data.y}): {data.colorType}");
            }
        }

        // Reactions disabled - keeping cleaner animation flow
        private IEnumerator TriggerAdjacentReactions(int gridX, int gridY)
        {
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
