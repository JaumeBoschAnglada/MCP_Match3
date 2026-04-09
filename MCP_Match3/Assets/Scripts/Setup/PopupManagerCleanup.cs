using UnityEngine;
using Match3.UI;

namespace Match3.Setup
{
    /// <summary>
    /// Cleanup script to find and remove duplicate PopupManager instances.
    /// Only keeps one PopupManager singleton with Canvas.
    /// Place on any GameObject and call CleanupDuplicatePopupManagers()
    /// </summary>
    public class PopupManagerCleanup : MonoBehaviour
    {
        public static void CleanupDuplicatePopupManagers()
        {
            var allPopupManagers = FindObjectsOfType<PopupManager>();
            
            if (allPopupManagers.Length <= 1)
            {
                Debug.Log("[PopupManagerCleanup] No duplicates found or only one PopupManager exists");
                return;
            }

            Canvas canonicalCanvas = null;
            PopupManager toKeep = null;

            // Find the one with Canvas (the correct one)
            foreach (var pm in allPopupManagers)
            {
                var canvas = pm.GetComponent<Canvas>();
                if (canvas != null)
                {
                    toKeep = pm;
                    canonicalCanvas = canvas;
                    break;
                }
            }

            // Delete all others
            int deleted = 0;
            foreach (var pm in allPopupManagers)
            {
                if (pm != toKeep)
                {
                    Debug.Log($"[PopupManagerCleanup] Destroying duplicate PopupManager: {pm.gameObject.name}");
                    DestroyImmediate(pm.gameObject);
                    deleted++;
                }
            }

            Debug.Log($"[PopupManagerCleanup] ✅ Cleaned up {deleted} duplicate PopupManager instance(s). Kept the one with Canvas at {toKeep.gameObject.name}");
        }
    }
}
