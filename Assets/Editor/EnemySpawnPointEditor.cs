using FloorGen;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(EnemySpawnPoint)), CanEditMultipleObjects]
    public class EnemySpawnPointEditor : UnityEditor.Editor
    {
        // ReSharper disable once InconsistentNaming
        private static readonly float alpha = 0.2f;
        public void OnSceneGUI()
        {
            EnemySpawnPoint spawnPoint = (EnemySpawnPoint) target;
            Vector3 pos = spawnPoint.transform.position;

            if ((spawnPoint.types & EnemySpawnType.Heavy) != 0)
            {
                Rect rect = new((Vector2)pos - Vector2.one/2f, Vector2.one);
                Color orange = new Color(1, 0.5f, 39/255f, alpha);
                Handles.DrawSolidRectangleWithOutline(rect, orange, Color.black);
            }
            if ((spawnPoint.types & EnemySpawnType.Ranged) != 0)
            {
                Vector3 vertexOne = pos + new Vector3(-0.5f, -0.5f);
                Vector3 vertexTwo = pos + new Vector3(0.5f, -0.5f);
                Vector3 vertexThree = pos + new Vector3(0, 0.5f);
                
                Color darkRed = new Color(136/255f, 0, 21/255f, alpha);
                Handles.color = darkRed;
                Handles.DrawAAConvexPolygon(vertexOne,vertexTwo,vertexThree);
            }
            if ((spawnPoint.types & EnemySpawnType.Chaser) != 0)
            {
                Handles.color = WithAlpha(Color.red);
                Handles.DrawSolidDisc(pos, Vector3.forward, 0.75f);
            }
            if ((spawnPoint.types & EnemySpawnType.Jumper) != 0)
            {
                Handles.color = WithAlpha(Color.green);
                Handles.DrawSolidDisc(pos, Vector3.forward, 0.75f);
            }
        }

        private static Color WithAlpha(Color color)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}