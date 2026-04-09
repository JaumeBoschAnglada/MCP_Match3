using System.Collections.Generic;
using UnityEngine;
using Match3.Data;

namespace Match3.Core
{
    /// <summary>
    /// Handles the special effects and eliminations for HorizontalRow and VerticalRow pieces.
    /// When a special piece is part of a match (combined with 2 other pieces):
    /// - HorizontalRow: eliminates all pieces in its row gradually from center to edges
    /// - VerticalRow: eliminates all pieces in its column gradually from center to edges
    /// </summary>
    public class SpecialPieceEffects
    {
        public const float ELIMINATION_DELAY = 0.05f; // Delay between eliminating each piece

        /// <summary>
        /// Check if any matched pieces are special and apply their effects.
        /// This should be called after a normal match is found and before removing regular pieces.
        /// </summary>
        public static List<PieceData> ApplySpecialEffects(List<PieceData> matchedPieces, PieceData[,] grid, int gridWidth, int gridHeight)
        {
            List<PieceData> totalPiecesToRemove = new List<PieceData>(matchedPieces);

            // Check for special pieces in the match
            foreach (var piece in matchedPieces)
            {
                if (piece.specialEffect == SpecialEffect.HorizontalRow)
                {
                    // Get all pieces in this row and mark them
                    List<PieceData> rowPieces = GetRowPieces(piece.y, grid, gridWidth, gridHeight);
                    foreach (var p in rowPieces)
                    {
                        if (!totalPiecesToRemove.Contains(p))
                            totalPiecesToRemove.Add(p);
                    }
                    Debug.Log($"[SpecialPieceEffects] HorizontalRow triggered at ({piece.x}, {piece.y}) - will eliminate entire row {piece.y}");
                }
                else if (piece.specialEffect == SpecialEffect.VerticalRow)
                {
                    // Get all pieces in this column and mark them
                    List<PieceData> columnPieces = GetColumnPieces(piece.x, grid, gridWidth, gridHeight);
                    foreach (var p in columnPieces)
                    {
                        if (!totalPiecesToRemove.Contains(p))
                            totalPiecesToRemove.Add(p);
                    }
                    Debug.Log($"[SpecialPieceEffects] VerticalRow triggered at ({piece.x}, {piece.y}) - will eliminate entire column {piece.x}");
                }
            }

            return totalPiecesToRemove;
        }

        /// <summary>
        /// Get all non-empty pieces in a specific row.
        /// </summary>
        private static List<PieceData> GetRowPieces(int y, PieceData[,] grid, int gridWidth, int gridHeight)
        {
            List<PieceData> rowPieces = new List<PieceData>();
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y].type != PieceType.Empty)
                {
                    rowPieces.Add(grid[x, y]);
                }
            }
            return rowPieces;
        }

        /// <summary>
        /// Get all non-empty pieces in a specific column.
        /// </summary>
        private static List<PieceData> GetColumnPieces(int x, PieceData[,] grid, int gridWidth, int gridHeight)
        {
            List<PieceData> columnPieces = new List<PieceData>();
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y].type != PieceType.Empty)
                {
                    columnPieces.Add(grid[x, y]);
                }
            }
            return columnPieces;
        }

        /// <summary>
        /// Get pieces in order from center outward (for gradual elimination animation).
        /// </summary>
        public static List<PieceData> GetEliminationOrder(List<PieceData> pieces, PieceData specialPiece)
        {
            List<PieceData> orderedPieces = new List<PieceData>();
            List<PieceData> remaining = new List<PieceData>(pieces);

            // Start from special piece and expand outward
            int centerX = specialPiece.x;
            int centerY = specialPiece.y;
            int distance = 0;
            int maxDistance = int.MaxValue;

            while (remaining.Count > 0)
            {
                List<PieceData> currentDistance = new List<PieceData>();

                for (int i = remaining.Count - 1; i >= 0; i--)
                {
                    PieceData piece = remaining[i];
                    int distToCenter;

                    if (specialPiece.specialEffect == SpecialEffect.HorizontalRow)
                    {
                        // Horizontal: distance by X distance from center
                        distToCenter = Mathf.Abs(piece.x - centerX);
                    }
                    else // VerticalRow
                    {
                        // Vertical: distance by Y distance from center
                        distToCenter = Mathf.Abs(piece.y - centerY);
                    }

                    if (distToCenter == distance)
                    {
                        currentDistance.Add(piece);
                        remaining.RemoveAt(i);
                    }
                }

                orderedPieces.AddRange(currentDistance);
                distance++;
            }

            return orderedPieces;
        }
    }
}
