using System.Collections.Generic;
using UnityEngine;
using Match3.Data;

namespace Match3.Core
{
    /// <summary>
    /// Detects matches of 4+ pieces and creates special pieces (HorizontalRow, VerticalRow).
    /// Called after match detection to upgrade regular matches into power-ups.
    /// </summary>
    public class SpecialPieceCreator
    {
        /// <summary>
        /// Scan matched pieces and determine positions where special pieces should be created.
        /// Returns the list of matched pieces (ALL of them, including those that will become specials).
        /// Returns positions where specials should be created (centerPiecesPositions).
        /// Does NOT modify the original PieceData - all pieces get eliminated together.
        /// </summary>
        public static List<PieceData> CreateSpecialPiecesFromMatches(List<PieceData> matchedPieces, PieceData[,] grid, int gridWidth, int gridHeight, (int x1, int y1, int x2, int y2) swapPositions, out List<(int x, int y, ColorType color)> specialPiecePositions)
        {
            specialPiecePositions = new List<(int x, int y, ColorType color)>();

            // Check for horizontal rows (4+ in a row)
            ProcessHorizontalMatches(matchedPieces, grid, gridWidth, gridHeight, swapPositions, specialPiecePositions);

            // Check for vertical rows (4+ in a column)
            ProcessVerticalMatches(matchedPieces, grid, gridWidth, gridHeight, swapPositions, specialPiecePositions);

            // IMPORTANT: Return ALL matched pieces for elimination (including those that will become specials)
            // Do NOT remove any pieces from the list - they all get eliminated
            return matchedPieces;
        }

        private static void ProcessHorizontalMatches(List<PieceData> matchedPieces, PieceData[,] grid, int gridWidth, int gridHeight, (int x1, int y1, int x2, int y2) swapPositions, List<(int x, int y, ColorType color)> specialPiecePositions)
        {
            // Group by row and check for 4+ consecutive matches
            Dictionary<int, List<PieceData>> rowGroups = new Dictionary<int, List<PieceData>>();
            
            foreach (var piece in matchedPieces)
            {
                if (!rowGroups.ContainsKey(piece.y))
                    rowGroups[piece.y] = new List<PieceData>();
                rowGroups[piece.y].Add(piece);
            }

            foreach (var rowEntry in rowGroups)
            {
                int y = rowEntry.Key;
                List<PieceData> piecesInRow = rowEntry.Value;

                if (piecesInRow.Count >= 4)
                {
                    // Sort by x position
                    piecesInRow.Sort((a, b) => a.x.CompareTo(b.x));

                    // Prioritize swap positions if they exist in this row
                    PieceData centerPiece = null;
                    
                    if (swapPositions.y1 == y)
                    {
                        // One of the swap positions is in this row - check if it's in the match
                        var swapPiece1 = piecesInRow.Find(p => p.x == swapPositions.x1);
                        if (swapPiece1 != null)
                            centerPiece = swapPiece1;
                    }
                    
                    if (centerPiece == null && swapPositions.y2 == y)
                    {
                        var swapPiece2 = piecesInRow.Find(p => p.x == swapPositions.x2);
                        if (swapPiece2 != null)
                            centerPiece = swapPiece2;
                    }
                    
                    // If no swap position found, use mathematical center
                    if (centerPiece == null)
                    {
                        int centerIndex = (piecesInRow.Count - 1) / 2;
                        centerPiece = piecesInRow[centerIndex];
                    }

                    // Record the position and color for special piece creation (do NOT modify PieceData)
                    specialPiecePositions.Add((centerPiece.x, centerPiece.y, centerPiece.colorType));

                    Debug.Log($"[SpecialPieceCreator] Detected HorizontalRow at ({centerPiece.x}, {centerPiece.y}) with color {centerPiece.colorType} - will be created after elimination");
                }
            }
        }

        private static void ProcessVerticalMatches(List<PieceData> matchedPieces, PieceData[,] grid, int gridWidth, int gridHeight, (int x1, int y1, int x2, int y2) swapPositions, List<(int x, int y, ColorType color)> specialPiecePositions)
        {
            // Group by column and check for 4+ consecutive matches
            Dictionary<int, List<PieceData>> columnGroups = new Dictionary<int, List<PieceData>>();

            foreach (var piece in matchedPieces)
            {
                if (!columnGroups.ContainsKey(piece.x))
                    columnGroups[piece.x] = new List<PieceData>();
                columnGroups[piece.x].Add(piece);
            }

            foreach (var columnEntry in columnGroups)
            {
                int x = columnEntry.Key;
                List<PieceData> piecesInColumn = columnEntry.Value;

                if (piecesInColumn.Count >= 4)
                {
                    // Sort by y position
                    piecesInColumn.Sort((a, b) => a.y.CompareTo(b.y));

                    // Prioritize swap positions if they exist in this column
                    PieceData centerPiece = null;
                    
                    if (swapPositions.x1 == x)
                    {
                        // One of the swap positions is in this column - check if it's in the match
                        var swapPiece1 = piecesInColumn.Find(p => p.y == swapPositions.y1);
                        if (swapPiece1 != null)
                            centerPiece = swapPiece1;
                    }
                    
                    if (centerPiece == null && swapPositions.x2 == x)
                    {
                        var swapPiece2 = piecesInColumn.Find(p => p.y == swapPositions.y2);
                        if (swapPiece2 != null)
                            centerPiece = swapPiece2;
                    }
                    
                    // If no swap position found, use mathematical center
                    if (centerPiece == null)
                    {
                        int centerIndex = (piecesInColumn.Count - 1) / 2;
                        centerPiece = piecesInColumn[centerIndex];
                    }

                    // Record the position and color for special piece creation (do NOT modify PieceData)
                    specialPiecePositions.Add((centerPiece.x, centerPiece.y, centerPiece.colorType));

                    Debug.Log($"[SpecialPieceCreator] Detected VerticalRow at ({centerPiece.x}, {centerPiece.y}) with color {centerPiece.colorType} - will be created after elimination");
                }
            }
        }
    }
}
