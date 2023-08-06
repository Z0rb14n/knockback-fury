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
            FloorGenerator floorGen = (FloorGenerator)target;
            floorGen.ToPreview = EditorGUILayout.IntField("Layout To Preview", floorGen.ToPreview);
        }

        private void OnSceneGUI()
        {
            FloorGenerator floorGen = (FloorGenerator)target;
            if (floorGen.ToPreview >= floorGen.layouts.Length || floorGen.ToPreview < 0) return;
            Vector3 initialPos = floorGen.transform.position;
            Vector3 sizeOffset = new Vector3(0, floorGen.gridSize.y/2,0) + Vector3.down;
            GUIStyle newStyle = GUI.skin.GetStyle("Label");
            newStyle.alignment = TextAnchor.UpperCenter;
            Handles.Label(initialPos + sizeOffset, $"Layout {floorGen.ToPreview}", newStyle);
            Handles.color = Color.red;
            // grid generation has overlap on 1 cell boundaries
            // inside space is -2 tiles
            Handles.DrawWireCube(initialPos + new Vector3(-0.5f,0.5f,0), (Vector3)floorGen.gridSize + Vector3.forward - new Vector3(1,1,0));
            Layout layout = floorGen.layouts[floorGen.ToPreview];
            EditorGUI.BeginChangeCheck();
            Vector3[] newPositions = new Vector3[layout.sockets.Length];
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < layout.sockets.Length; i++)
            {
                SocketShape socket = layout.sockets[i];
                Handles.color = Color.yellow;
                Handles.DrawWireCube(initialPos+(Vector3)socket.position, (Vector3) socket.size + Vector3.forward);
                newPositions[i] = Handles.PositionHandle(initialPos + (Vector3)socket.position, floorGen.transform.rotation);
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(floorGen, $"Change Layout {floorGen.ToPreview} Socket position");
                for (int i = 0; i < layout.sockets.Length; i++)
                {
                    floorGen.layouts[floorGen.ToPreview].sockets[i].position = newPositions[i];
                }
            }
        }
    }
}