using System.Collections.Generic;
using UnityEngine;

namespace Match3.Data
{
    /// <summary>
    /// Data structure representing a single level loaded from JSON.
    /// See AgentConfig/level-structure.md for format specification.
    /// </summary>
    public class LevelData
    {
        public int levelNumber;
        public string name;
        public int width;
        public int height;
        public List<PieceDataEntry> pieces;

        /// <summary>
        /// Represents a single piece entry in level JSON.
        /// </summary>
        [System.Serializable]
        public class PieceDataEntry
        {
            public int x;
            public int y;
            public string type;

            /// <summary>
            /// Converts string type to ColorType enum.
            /// </summary>
            public ColorType GetColorType()
            {
                return type switch
                {
                    "Red" => ColorType.Red,
                    "Blue" => ColorType.Blue,
                    "Green" => ColorType.Green,
                    "Yellow" => ColorType.Yellow,
                    "Empty" => ColorType.Empty,
                    _ => ColorType.Empty
                };
            }

            /// <summary>
            /// Legacy method for backward compatibility
            /// </summary>
            public PieceType GetPieceType()
            {
                return (PieceType)GetColorType();
            }
        }

        /// <summary>
        /// Validates level data structure.
        /// Returns true if valid, false otherwise.
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            // Check piece count
            if (pieces.Count != width * height)
            {
                errorMessage = $"Invalid piece count: expected {width * height}, got {pieces.Count}";
                return false;
            }

            // Check for duplicate coordinates
            HashSet<(int, int)> seen = new HashSet<(int, int)>();
            foreach (var piece in pieces)
            {
                if (piece.x < 0 || piece.x >= width || piece.y < 0 || piece.y >= height)
                {
                    errorMessage = $"Piece at ({piece.x}, {piece.y}) out of bounds for {width}x{height} board";
                    return false;
                }

                if (!seen.Add((piece.x, piece.y)))
                {
                    errorMessage = $"Duplicate piece at ({piece.x}, {piece.y})";
                    return false;
                }

                // Validate type
                if (piece.type != "Red" && piece.type != "Blue" && 
                    piece.type != "Green" && piece.type != "Yellow" && piece.type != "Empty")
                {
                    errorMessage = $"Invalid piece type '{piece.type}' at ({piece.x}, {piece.y})";
                    return false;
                }
            }

            errorMessage = "";
            return true;
        }
    }

    /// <summary>
    /// Loads level data from JSON files in Resources/Levels/
    /// Usage: LevelData level = LevelLoader.LoadLevel(1);
    /// </summary>
    public static class LevelLoader
    {
        private const string LEVELS_FOLDER = "Levels";

        /// <summary>
        /// Loads a level by number.
        /// </summary>
        /// <param name="levelNumber">Level identifier (1, 2, 3, etc)</param>
        /// <returns>Loaded LevelData or null if not found</returns>
        public static LevelData LoadLevel(int levelNumber)
        {
            string path = $"{LEVELS_FOLDER}/level_{levelNumber}";
            TextAsset jsonFile = Resources.Load<TextAsset>(path);

            if (jsonFile == null)
            {
                Debug.LogError($"[LevelLoader] Level {levelNumber} not found at Resources/{path}.json");
                return null;
            }

            try
            {
                LevelData levelData = JsonUtility.FromJson<LevelData>(jsonFile.text);

                // Validate loaded data
                if (!levelData.IsValid(out string errorMessage))
                {
                    Debug.LogError($"[LevelLoader] Level {levelNumber} validation failed: {errorMessage}");
                    return null;
                }

                Debug.Log($"[LevelLoader] Loaded level {levelNumber} '{levelData.name}' ({levelData.width}x{levelData.height})");
                return levelData;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LevelLoader] Failed to parse level {levelNumber}: {e.Message}");
                return null;
            }
        }
    }
}
