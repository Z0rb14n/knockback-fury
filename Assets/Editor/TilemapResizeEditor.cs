using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace Editor
{
    /// <summary>
    /// Script to add an entry in the "GameObjects" menu to compress bounds.
    /// </summary>
    public static class TilemapResizeEditor
    {
        [MenuItem("GameObject/Compress Tilemaps")]
        private static void CompressTilemapBounds()
        {
            Tilemap[] tilemaps = Selection.objects.Select(o => o.GetComponent<Tilemap>())
                .Distinct().Where(map => map).ToArray();
            foreach (Tilemap map in tilemaps)
            {
                map.CompressBounds();
                EditorUtility.SetDirty(map);
            }
        }
    }
}