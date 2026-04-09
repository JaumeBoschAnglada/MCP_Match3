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
        /// Scan matched pieces and convert 4+ horizontal/vertical matches to special pieces.
        /// Returns the updated matched pieces list.
        /// </summary>
        public static List<PieceData> CreateSpecialPiecesFromMatches(List<PieceData> matchedPieces, PieceData[,] grid, int gridWidth, int gridHeight, out List<PieceData> newlyCreatedSpecials)
        {
            HashSet<PieceData> centerPiecesConverted = new HashSet<PieceData>();

            // Check for horizontal rows (4+ in a row)
            ProcessHorizontalMatches(matchedPieces, grid, gridWidth, gridHeight, centerPiecesConverted);

            // Check for vertical rows (4+ in a column)
            ProcessVerticalMatches(matchedPieces, grid, gridWidth, gridHeight, centerPiecesConverted);

            newlyCreatedSpecials = new List<PieceData>(centerPiecesConverted);

            // Remove center pieces from elimination list - they stay as special pieces
            foreach (var centerPiece in centerPiecesConverted)
            {
                matchedPieces.Remove(centerPiece);
            }

            return matchedPieces;
        }

        private static void ProcessHorizontalMatches(List<PieceData> matchedPieces, PieceData[,] grid, int gridWidth, int gridHeight, HashSet<PieceData> centerPiecesConverted)
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

                    // Find the center piece to become HorizontalRow
                    int centerIndex = (piecesInRow.Count - 1) / 2;
                    PieceData centerPiece = piecesInRow[centerIndex];

                    // Convert center piece to HorizontalRow - keep color, add effect
                    grid[centerPiece.x, centerPiece.y].specialEffect = SpecialEffect.HorizontalRow;
                    centerPiece.specialEffect = SpecialEffect.HorizontalRow;
                    
                    centerPiecesConverted.Add(centerPiece);

                    Debug.Log($"[SpecialPieceCreator] Created HorizontalRow at ({centerPiece.x}, {centerPiece.y}) with color {centerPiece.colorType} from {piecesInRow.Count} horizontal matches");
                }
            }
        }

        private static void ProcessVerticalMatches(List<PieceData> matchedPieces, PieceData[,] grid, int gridWidth, int gridHeight, HashSet<PieceData> centerPiecesConverted)
        {
            // Group by column and check for 4+ consecutive matches
            Dictionary<int, List<PieceData>> columnGroups = new Dictionary<int, List<PieceData>>();

            foreach (var piece in matchedPieces)
            {
                if (centerPiecesConverted.Contains(piece))
                    continue; // Skip if already converted to special

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

                    // Find the center piece to become VerticalRow
                    int centerIndex = (piecesInColumn.Count - 1) / 2;
                    PieceData centerPiece = piecesInColumn[centerIndex];

                    // Convert center piece to VerticalRow - keep color, add effect
                    grid[centerPiece.x, centerPiece.y].specialEffect = SpecialEffect.VerticalRow;
                    centerPiece.specialEffect = SpecialEffect.VerticalRow;

                    centerPiecesConverted.Add(centerPiece);

                    Debug.Log($"[SpecialPieceCreator] Created VerticalRow at ({centerPiece.x}, {centerPiece.y}) with color {centerPiece.colorType} from {piecesInColumn.Count} vertical matches");
                }
            }
        }
    }
}
