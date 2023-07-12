using UnityEngine;
using Enemies;
using UnityEditor;

namespace Editor
{
    [CustomEditor(typeof(JumperMovement))]
    public class JumperMovementEditor : UnityEditor.Editor
    {
        // Custom in-scene UI for when ExampleScript
        // component is selected.
        public void OnSceneGUI()
        {
            JumperMovement movement = (JumperMovement) target;
            for (int i = 0; i < movement.patrolPoints.Length; i++)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 oldPos = movement.patrolPoints[i].position;
                Vector3 newTargetPosition = Handles.PositionHandle(oldPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(movement.patrolPoints[i], $"Change Patrol Point {i+1} Position");
                    movement.patrolPoints[i].position = newTargetPosition;
                }   
            }
        }
    }
}