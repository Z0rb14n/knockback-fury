using Enemies;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(PatrolMovement))]
    public class PatrolMovementEditor : UnityEditor.Editor
    {
        // Custom in-scene UI for when ExampleScript
        // component is selected.
        public void OnSceneGUI()
        {
            PatrolMovement movement = (PatrolMovement) target;
            for (int i = 0; i < movement.patrolPoints.Length; i++)
            {
                EditorGUI.BeginChangeCheck();
                if (!movement.patrolPoints[i]) continue;
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