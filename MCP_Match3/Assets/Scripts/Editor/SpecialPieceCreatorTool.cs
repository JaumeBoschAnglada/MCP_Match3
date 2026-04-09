using UnityEditor;
using UnityEngine;
using Match3.Gameplay;
using System.IO;

namespace Match3.Editor
{
    /// <summary>
    /// Editor utility to generate and manage piece prefabs.
    /// </summary>
    public class SpecialPieceCreatorTool
    {
        private const string PIECES_PREFAB_DIR = "Assets/Prefabs/Pieces";
        private const string RED_MAT_GUID = "4d4d5c935f715f14aaec3447bea3d3df";
        private const string BLUE_MAT_GUID = "3d9b5f8c1e2a4b7f9c1d5e3a8f2b4c6d";
        private const string GREEN_MAT_GUID = "2c8a4e7d9f1b3a5c8e0d2f4a7c9b1e3d";
        private const string YELLOW_MAT_GUID = "1b7a3d6c8e0a2f5d9c1e4a7b0f2d5e8c";

        [MenuItem("Match3/Generate All Piece Prefabs")]
        public static void GenerateAllPiecePrefabs()
        {
            // Ensure directory exists
            if (!Directory.Exists(PIECES_PREFAB_DIR))
            {
                Directory.CreateDirectory(PIECES_PREFAB_DIR);
            }

            // Generate normal color pieces
            CreateColorPiecePrefab("Red", "#FF0000FF", "Piece_Red");
            CreateColorPiecePrefab("Blue", "#0000FFFF", "Piece_Blue");
            CreateColorPiecePrefab("Green", "#00FF00FF", "Piece_Green");
            CreateColorPiecePrefab("Yellow", "#FFFF00FF", "Piece_Yellow");

            // Generate special effect pieces
            CreateSpecialPiecePrefab(PIECES_PREFAB_DIR, "HorizontalRowPiece", typeof(HorizontalRowPiece), "Horizontal Row Piece", new Color(1f, 0.5f, 0f));
            CreateSpecialPiecePrefab(PIECES_PREFAB_DIR, "VerticalRowPiece", typeof(VerticalRowPiece), "Vertical Row Piece", new Color(1f, 0.5f, 0f));

            Debug.Log("[SpecialPieceCreatorTool] ✅ All piece prefabs generated successfully!");
            AssetDatabase.Refresh();
        }

        private static void CreateColorPiecePrefab(string colorName, string colorHex, string prefabName)
        {
            string prefabPath = $"{PIECES_PREFAB_DIR}/{prefabName}.prefab";

            // Check if prefab already exists
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                Debug.Log($"[SpecialPieceCreatorTool] Prefab already exists: {prefabPath}. Updating...");
                
                // Ensure it has the Piece component
                Piece pieceComponent = existingPrefab.GetComponent<Piece>();
                if (pieceComponent == null)
                {
                    existingPrefab.AddComponent<Piece>();
                    Debug.Log($"[SpecialPieceCreatorTool] Added Piece component to {prefabName}");
                }
                return;
            }

            // Create a new GameObject
            GameObject go = new GameObject(prefabName);

            // Add Renderer
            var meshFilter = go.AddComponent<MeshFilter>();
            var meshRenderer = go.AddComponent<MeshRenderer>();
            var boxCollider = go.AddComponent<BoxCollider>();
            
            // Create mesh
            meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            
            // Apply material with color
            Material mat = new Material(Shader.Find("Standard"));
            if (ColorUtility.TryParseHtmlString(colorHex, out Color color))
            {
                mat.color = color;
            }
            meshRenderer.material = mat;

            // Add collider
            boxCollider.size = Vector3.one;

            // Add the Piece script
            go.AddComponent<Piece>();

            // Scale
            go.transform.localScale = Vector3.one * 0.9f;

            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Debug.Log($"[SpecialPieceCreatorTool] Created prefab: {prefabPath}");

            // Cleanup temporary object
            Object.DestroyImmediate(go);
        }

        private static void CreateSpecialPiecePrefab(string prefabDir, string prefabName, System.Type scriptType, string displayName, Color highlightColor)
        {
            string prefabPath = $"{prefabDir}/{prefabName}.prefab";

            // Check if prefab already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.Log($"[SpecialPieceCreatorTool] Prefab already exists: {prefabPath}. Skipping.");
                return;
            }

            // Create a new GameObject
            GameObject go = new GameObject(prefabName);

            // Add Renderer
            var meshFilter = go.AddComponent<MeshFilter>();
            var meshRenderer = go.AddComponent<MeshRenderer>();
            var boxCollider = go.AddComponent<BoxCollider>();
            
            // Create mesh
            meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            
            // Apply a highlighted material for special pieces
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = highlightColor;
            meshRenderer.material = mat;

            // Add collider
            boxCollider.size = Vector3.one;

            // Add the special piece script
            go.AddComponent(scriptType);

            // Scale
            go.transform.localScale = Vector3.one * 0.85f;  // Slightly smaller to distinguish

            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Debug.Log($"[SpecialPieceCreatorTool] Created prefab: {prefabPath}");

            // Cleanup temporary object
            Object.DestroyImmediate(go);
        }
    }
}
