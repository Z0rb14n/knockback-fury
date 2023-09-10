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
            GUI.enabled = !EditorApplication.isPlayingOrWillChangePlaymode;
            if (GUILayout.Button("Create Spawn Point"))
            {
                Object o = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Rooms/Socket/SpawnPointPrefab.prefab", typeof(GameObject));
                Object instantiatedPoint = PrefabUtility.InstantiatePrefab(o, roomData.transform);
                Selection.activeObject = instantiatedPoint;
            }

            GUI.enabled = true;
        }

        private void OnSceneGUI()
        {
            RoomData roomData = (RoomData)target;
            roomData.cheeseSpawnOffset = DrawSocketFor(roomData.cheeseSpawnOffset, "Cheese Spawn Offset");
            roomData.weaponSpawnOffset = DrawSocketFor(roomData.weaponSpawnOffset, "Weapon Spawn Offset");
            roomData.powerupSpawnOffset = DrawSocketFor(roomData.powerupSpawnOffset, "Powerup Spawn Offset");
            roomData.weaponUpgradeSpawnOffset = DrawSocketFor(roomData.weaponUpgradeSpawnOffset, "Upgrade Spawn Offset");
            roomData.playerSpawnOffset = DrawSocketFor(roomData.playerSpawnOffset, "Upgrade Spawn Offset");
            if (roomData.layouts == null || roomData.ToPreview >= roomData.layouts.Length || roomData.ToPreview < 0) return;
            Vector3 initialPos = roomData.transform.position;
            Layout layout = roomData.layouts[roomData.ToPreview];
            EditorGUI.BeginChangeCheck();
            Vector3[] newPositions = new Vector3[layout.sockets.Length];
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < layout.sockets.Length; i++)
            {
                SocketShape socket = layout.sockets[i];
                Handles.color = Color.yellow;
                Handles.DrawWireCube(initialPos+(Vector3)socket.position, (Vector3) socket.size + Vector3.forward);
                newPositions[i] = Handles.SnapValue(Handles.PositionHandle(initialPos + (Vector3)socket.position, roomData.transform.rotation), EditorSnapSettings.move);
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

        private Vector2 DrawSocketFor(Vector2 offset, string thingChanged)
        {
            RoomData roomData = (RoomData)target;
            EditorGUI.BeginChangeCheck();
            Handles.color = Color.yellow;
            Vector3 initialPos = roomData.transform.position;
            Handles.DrawWireCube(initialPos + (Vector3) offset, Vector3.one);
            Vector3 retVal = Handles.SnapValue(Handles.PositionHandle(initialPos + (Vector3) offset, roomData.transform.rotation),
                EditorSnapSettings.move);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(roomData, $"Change Room Data {thingChanged}");
                return retVal;
            }

            return offset;
        }
    }
}