using System.Collections.Generic;
using UnityEngine;
using Match3.Data;

namespace Match3.Core
{
    public class BoardController : MonoBehaviour
    {
        private const int GRID_WIDTH = 6;
        private const int GRID_HEIGHT = 6;
        
        private PieceData[,] grid;

        public int Width => GRID_WIDTH;
        public int Height => GRID_HEIGHT;
        public PieceData[,] Grid => grid;

        public void Initialize(int levelNumber = 1)
        {
            grid = new PieceData[GRID_WIDTH, GRID_HEIGHT];
            LoadLevel(levelNumber);
        }

        /// <summary>
        /// Loads a specific level from JSON.
        /// Falls back to random board if level not found.
        /// </summary>
        private void LoadLevel(int levelNumber)
        {
            LevelData levelData = LevelLoader.LoadLevel(levelNumber);
            
            if (levelData != null)
            {
                Debug.Log($"[BoardController] Loading level {levelNumber}");
                PopulateGridFromLevelData(levelData);
            }
            else
            {
                Debug.Log($"[BoardController] Level {levelNumber} not found, generating random board instead");
                FillInitialBoard();
            }
        }

        /// <summary>
        /// Populates grid from loaded LevelData.
        /// </summary>
        private void PopulateGridFromLevelData(LevelData levelData)
        {
            foreach (var pieceEntry in levelData.pieces)
            {
                ColorType color = pieceEntry.GetColorType();
                grid[pieceEntry.x, pieceEntry.y] = new PieceData(pieceEntry.x, pieceEntry.y, color);
            }
        }

        private void FillInitialBoard()
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    ColorType color = GetRandomColorType();
                    grid[x, y] = new PieceData(x, y, color);
                }
            }
        }

        private ColorType GetRandomColorType()
        {
            int random = Random.Range(0, 4);
            return (ColorType)(random + 1);
        }

        public PieceData GetPiece(int x, int y)
        {
            if (IsValidPosition(x, y))
                return grid[x, y];
            return null;
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < GRID_WIDTH && y >= 0 && y < GRID_HEIGHT;
        }

        public void SetPiece(int x, int y, ColorType color)
        {
            if (IsValidPosition(x, y))
            {
                grid[x, y].colorType = color;
            }
        }

        public void SwapPieces(int x1, int y1, int x2, int y2)
        {
            if (!IsValidPosition(x1, y1) || !IsValidPosition(x2, y2))
                return;

            PieceData temp = grid[x1, y1];
            grid[x1, y1] = grid[x2, y2];
            grid[x2, y2] = temp;

            grid[x1, y1].x = x1;
            grid[x1, y1].y = y1;
            grid[x2, y2].x = x2;
            grid[x2, y2].y = y2;
        }

        public List<PieceData> FindMatches()
        {
            HashSet<PieceData> matchedPieces = new HashSet<PieceData>();

            // Horizontal matches
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                for (int x = 0; x < GRID_WIDTH; x++)
                {
                    if (grid[x, y].colorType == ColorType.Empty)
                        continue;

                    CheckHorizontalMatch(x, y, matchedPieces);
                }
            }

            // Vertical matches
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    if (grid[x, y].colorType == ColorType.Empty)
                        continue;

                    CheckVerticalMatch(x, y, matchedPieces);
                }
            }

            List<PieceData> matches = new List<PieceData>(matchedPieces);

            // Special piece creation is now handled by GameManager.ProcessMatchesLoop
            // This keeps the game logic centralized there

            return matches;
        }

        private void CheckHorizontalMatch(int startX, int startY, HashSet<PieceData> matches)
        {
            ColorType color = grid[startX, startY].colorType;
            if (color == ColorType.Empty)
                return;

            int matchCount = 1;
            int x = startX + 1;

            while (x < GRID_WIDTH && grid[x, startY].colorType == color)
            {
                matchCount++;
                x++;
            }

            if (matchCount >= 3)
            {
                for (int i = startX; i < startX + matchCount; i++)
                {
                    matches.Add(grid[i, startY]);
                }
            }
        }

        private void CheckVerticalMatch(int startX, int startY, HashSet<PieceData> matches)
        {
            ColorType color = grid[startX, startY].colorType;
            if (color == ColorType.Empty)
                return;

            int matchCount = 1;
            int y = startY + 1;

            while (y < GRID_HEIGHT && grid[startX, y].colorType == color)
            {
                matchCount++;
                y++;
            }

            if (matchCount >= 3)
            {
                for (int i = startY; i < startY + matchCount; i++)
                {
                    matches.Add(grid[startX, i]);
                }
            }
        }

        public void MarkPiecesForRemoval(List<PieceData> pieces)
        {
            // Apply special effects if any matched pieces are special
            List<PieceData> piecesToRemove = SpecialPieceEffects.ApplySpecialEffects(pieces, grid, GRID_WIDTH, GRID_HEIGHT);

            foreach (var piece in piecesToRemove)
            {
                piece.MarkForRemoval();
            }
        }

        public void RemoveMarkedPieces()
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    if (grid[x, y].isMarkedForRemoval)
                    {
                        grid[x, y] = new PieceData(x, y, ColorType.Empty);
                    }
                }
            }
        }

        public List<(int x, int fromY, int toY)> ApplyGravity()
        {
            var movements = new List<(int x, int fromY, int toY)>();

            for (int x = 0; x < GRID_WIDTH; x++)
            {
                int writeY = 0;
                for (int readY = 0; readY < GRID_HEIGHT; readY++)
                {
                    if (grid[x, readY].colorType != ColorType.Empty)
                    {
                        if (readY != writeY)
                        {
                            movements.Add((x, readY, writeY));
                            grid[x, writeY] = grid[x, readY];
                            grid[x, writeY].y = writeY;
                            grid[x, readY] = new PieceData(x, readY, ColorType.Empty);
                        }
                        writeY++;
                    }
                }
            }

            return movements;
        }

        public List<PieceData> FillEmptySpaces()
        {
            var newPieces = new List<PieceData>();

            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    if (grid[x, y].colorType == ColorType.Empty)
                    {
                        grid[x, y].colorType = GetRandomColorType();
                        newPieces.Add(grid[x, y]);
                    }
                }
            }

            return newPieces;
        }

        public PieceData[,] GetGridCopy()
        {
            PieceData[,] copy = new PieceData[GRID_WIDTH, GRID_HEIGHT];
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    copy[x, y] = grid[x, y].Clone();
                }
            }
            return copy;
        }
    }
}
