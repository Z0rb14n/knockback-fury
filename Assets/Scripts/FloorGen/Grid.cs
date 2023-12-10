using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloorGen
{
    public class Grid : IEnumerable<GridRoom>
    {
        private readonly Dictionary<Vector2Int, GridRoom> _data = new();

        private Vector2Int _finalRoomPos;

        public Vector2Int FinalRoomPos
        {
            get => _finalRoomPos;
            set
            {
                if (_data.TryGetValue(_finalRoomPos, out GridRoom room)) room.IsFinalRoom = false;
                _finalRoomPos = value;
                if (_data.TryGetValue(_finalRoomPos, out room)) room.IsFinalRoom = true;
            }
        }

        public Vector2Int GenerationStart;

        public GridRoom this[Vector2Int key]
        {
            get => _data[key];
            set => _data[key] = value;
        }

        public void AddGridRoom(Vector2Int key, RoomType startingDir)
        {
            GridRoom room = new()
            {
                Pos = key,
                Type = startingDir
            };
            _data.Add(key, room);
        }

        public void AddRoomType(Vector2Int key, RoomType additionalDir)
        {
            if (!_data.TryGetValue(key, out GridRoom val))
            {
                Debug.LogWarning($"No room at {key} found");
                return;
            }

            val.Type |= additionalDir;
        }

        public void SetWeaponRoom(Vector2Int key, bool isWeaponRoom = true)
        {
            if (!_data.TryGetValue(key, out GridRoom val))
            {
                Debug.LogWarning($"No room at {key} found");
                return;
            }

            val.IsWeaponRoom = isWeaponRoom;
        }

        public void SetSmithingRoom(Vector2Int key, bool isSmithingRoom = true)
        {
            if (!_data.TryGetValue(key, out GridRoom val))
            {
                Debug.LogWarning($"No room at {key} found");
                return;
            }

            val.IsSmithingRoom = isSmithingRoom;
        }

        public void SetEndingHasBoss(bool hasBoss = true)
        {
            if (!_data.TryGetValue(_finalRoomPos, out GridRoom val))
            {
                Debug.LogWarning($"No room at {_finalRoomPos} found");
                return;
            }

            val.IsBossRoom = hasBoss;
        }

        public void SetData(Vector2Int key, RoomData val) => _data[key].Data = val;

        public IEnumerable<Vector2Int> Rooms => _data.Keys;

        public bool HasRoomPos(Vector2Int pos) => _data.ContainsKey(pos);

        /// <inheritdoc />
        public IEnumerator<GridRoom> GetEnumerator() => _data.Values.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_data).GetEnumerator();
    }
    public class GridRoom
    {
        public Vector2Int Pos;
        public RoomType Type;
        public RoomData Data;
        public bool IsWeaponRoom;
        public bool IsSmithingRoom;
        public bool IsFinalRoom;
        public bool IsBossRoom;
    }
}