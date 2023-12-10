using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FloorGen
{
    public class Grid : IEnumerable<GridRoom>
    {
        private readonly Dictionary<Vector2Int, GridRoom> _rooms = new();

        private Vector2Int _finalRoomPos;

        public Vector2Int FinalRoomPos
        {
            get => _finalRoomPos;
            set
            {
                if (_rooms.TryGetValue(_finalRoomPos, out GridRoom room)) room.IsFinalRoom = false;
                _finalRoomPos = value;
                if (_rooms.TryGetValue(_finalRoomPos, out room)) room.IsFinalRoom = true;
            }
        }

        public Vector2Int GenerationStart;

        public GridRoom this[Vector2Int key]
        {
            get => _rooms[key];
            set => _rooms[key] = value;
        }

        public void EnsureRoomsPresent(params Vector2Int[] keys)
        {
            foreach (Vector2Int key in keys) _rooms.TryAdd(key, new GridRoom { Pos = key });
        }

        public void AddGridEdge(Vector2Int first, Vector2Int second, RoomType type)
        {
            if (!_rooms.TryGetValue(first, out GridRoom room1))
            {
                Debug.LogWarning($"No room at {first} found");
                return;
            }
            if (!_rooms.TryGetValue(second, out GridRoom room2))
            {
                Debug.LogWarning($"No room at {second} found");
                return;
            }
            room1.Edges.Add(new GridEdge
            {
                Room1 = first,
                Room2 = second,
                Type = type
            });
            room2.Edges.Add(new GridEdge
            {
                Room1 = second,
                Room2 = first,
                Type = type.GetOpposing()
            });
        }

        public void SetWeaponRoom(Vector2Int key, bool isWeaponRoom = true)
        {
            if (!_rooms.TryGetValue(key, out GridRoom val))
            {
                Debug.LogWarning($"No room at {key} found");
                return;
            }

            val.IsWeaponRoom = isWeaponRoom;
        }

        public void SetSmithingRoom(Vector2Int key, bool isSmithingRoom = true)
        {
            if (!_rooms.TryGetValue(key, out GridRoom val))
            {
                Debug.LogWarning($"No room at {key} found");
                return;
            }

            val.IsSmithingRoom = isSmithingRoom;
        }

        public void SetEndingHasBoss(bool hasBoss = true)
        {
            if (!_rooms.TryGetValue(_finalRoomPos, out GridRoom val))
            {
                Debug.LogWarning($"No room at {_finalRoomPos} found");
                return;
            }

            val.IsBossRoom = hasBoss;
        }

        public void SetData(Vector2Int key, RoomData val) => _rooms[key].Data = val;

        public IEnumerable<Vector2Int> Rooms => _rooms.Keys;

        public bool HasRoomPos(Vector2Int pos) => _rooms.ContainsKey(pos);

        /// <inheritdoc />
        public IEnumerator<GridRoom> GetEnumerator() => _rooms.Values.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_rooms).GetEnumerator();
    }
    public class GridRoom
    {
        public Vector2Int Pos;
        public RoomData Data;
        public bool IsWeaponRoom;
        public bool IsSmithingRoom;
        public bool IsFinalRoom;
        public bool IsBossRoom;
        public readonly List<GridEdge> Edges = new();

        public RoomType AggregateType => Edges.Select(edge => edge.Type).Aggregate((acc, next) => acc | next);
    }

    public class GridEdge
    {
        public Vector2Int Room1;
        public Vector2Int Room2;
        public RoomType Type;
    }
}