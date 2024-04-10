using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewFloorGen
{
    public class ComputerFloor : IEnumerable<GridRoom>
    {
        private readonly Dictionary<Vector2Int, GridRoom> _rooms = new();

        public GridRoom this[Vector2Int key]
        {
            get => _rooms[key];
            set => _rooms[key] = value;
        }

        public void EnsureRoomsPresent(params Vector2Int[] keys)
        {
            foreach (Vector2Int key in keys) _rooms.TryAdd(key, new GridRoom { Pos = key });
        }

        public void AddConnection(Vector2Int first, Vector2Int second)
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

            if (!room1.Neighbors.TryAdd(second, room2))
            {
                Debug.LogWarning($"Attempted to add duplicate edge from {first} to {second}.");
                return;
            }

            room2.Neighbors.Add(first, room1);
        }

        public void SetData(Vector2Int key, ComputerRoom val) => _rooms[key].Data = val;

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
        public ComputerRoom Data;
        public GameObject CellObject;
        public bool IsBossRoom;
        public readonly Dictionary<Vector2Int, GridRoom> Neighbors = new();
    }
}