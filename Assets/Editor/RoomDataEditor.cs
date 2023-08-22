using FloorGen;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(RoomData)), CanEditMultipleObjects]
    public class RoomDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (serializedObject.targetObjects.Length != 1) return;
            RoomData roomData = (RoomData)target;
            roomData.ToPreview = EditorGUILayout.IntField("Layout To Preview", roomData.ToPreview);
        }

        private void OnSceneGUI()
        {
            RoomData roomData = (RoomData)target;
            if (roomData.layouts == null || roomData.ToPreview >= roomData.layouts.Length || roomData.ToPreview < 0) return;
            Vector3 initialPos = roomData.transform.position;
            /*
            Renderer renderer = roomData.GetComponentInChildren<Renderer>();
            if (!renderer) return;
            GUIStyle newStyle = GUI.skin.GetStyle("Label");
            newStyle.alignment = TextAnchor.UpperCenter;
            Handles.Label(renderer.bounds.center, $"Layout {roomData.ToPreview}", newStyle);
            Handles.color = Color.red;
            Handles.DrawWireCube(initialPos, renderer.bounds.size);
            */
            Layout layout = roomData.layouts[roomData.ToPreview];
            EditorGUI.BeginChangeCheck();
            Vector3[] newPositions = new Vector3[layout.sockets.Length];
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < layout.sockets.Length; i++)
            {
                SocketShape socket = layout.sockets[i];
                Handles.color = Color.yellow;
                Handles.DrawWireCube(initialPos+(Vector3)socket.position, (Vector3) socket.size + Vector3.forward);
                newPositions[i] = Handles.PositionHandle(initialPos + (Vector3)socket.position, roomData.transform.rotation);
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(roomData, $"Change Layout {roomData.ToPreview} Socket position");
                for (int i = 0; i < layout.sockets.Length; i++)
                {
                    roomData.layouts[roomData.ToPreview].sockets[i].position = newPositions[i];
                }
            }
        }
    }
}