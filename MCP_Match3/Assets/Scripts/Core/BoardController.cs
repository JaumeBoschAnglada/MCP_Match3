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

        public void Initialize()
        {
            grid = new PieceData[GRID_WIDTH, GRID_HEIGHT];
            FillInitialBoard();
        }

        private void FillInitialBoard()
        {
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    PieceType type = GetRandomPieceType();
                    grid[x, y] = new PieceData(x, y, type);
                }
            }
        }

        private PieceType GetRandomPieceType()
        {
            int random = Random.Range(0, 4);
            return (PieceType)(random + 1);
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

        public void SetPiece(int x, int y, PieceType type)
        {
            if (IsValidPosition(x, y))
            {
                grid[x, y].type = type;
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
                    if (grid[x, y].type == PieceType.Empty)
                        continue;

                    CheckHorizontalMatch(x, y, matchedPieces);
                }
            }

            // Vertical matches
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    if (grid[x, y].type == PieceType.Empty)
                        continue;

                    CheckVerticalMatch(x, y, matchedPieces);
                }
            }

            return new List<PieceData>(matchedPieces);
        }

        private void CheckHorizontalMatch(int startX, int startY, HashSet<PieceData> matches)
        {
            PieceType type = grid[startX, startY].type;
            int matchCount = 1;
            int x = startX + 1;

            while (x < GRID_WIDTH && grid[x, startY].type == type)
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
            PieceType type = grid[startX, startY].type;
            int matchCount = 1;
            int y = startY + 1;

            while (y < GRID_HEIGHT && grid[startX, y].type == type)
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
            foreach (var piece in pieces)
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
                        grid[x, y] = new PieceData(x, y, PieceType.Empty);
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
                    if (grid[x, readY].type != PieceType.Empty)
                    {
                        if (readY != writeY)
                        {
                            movements.Add((x, readY, writeY));
                            grid[x, writeY] = grid[x, readY];
                            grid[x, writeY].y = writeY;
                            grid[x, readY] = new PieceData(x, readY, PieceType.Empty);
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
                    if (grid[x, y].type == PieceType.Empty)
                    {
                        grid[x, y].type = GetRandomPieceType();
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
