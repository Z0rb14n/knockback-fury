using FloorGen;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(FloorGenerator)), CanEditMultipleObjects]
    public class FloorGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            
            base.OnInspectorGUI();
            if (serializedObject.targetObjects.Length != 1) return;
            FloorGenerator floorGenerator = (FloorGenerator)target;
            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Test Transition"))
            {
                floorGenerator.Transition();
            }

            GUI.enabled = true;
        }
    }
}