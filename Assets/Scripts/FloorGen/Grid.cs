using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloorGen
{
    public class Grid : IEnumerable<KeyValuePair<Vector2Int, RoomType>>
    {
        private readonly Dictionary<Vector2Int, RoomType> _data = new();
        private readonly Dictionary<Vector2Int, RoomData> _dataMappings = new();
        public Vector2Int FinalRoom;
        public Vector2Int GenerationStart;
        public bool EndingHasBoss;
        public readonly List<Vector2Int> WeaponRooms = new();
        // ReSharper disable once IdentifierTypo
        public readonly List<Vector2Int> SmithingRooms = new();

        public RoomType this[Vector2Int key]
        {
            get => _data[key];
            set => _data[key] = value;
        }

        public RoomData GetData(Vector2Int key) => _dataMappings[key];

        public void SetData(Vector2Int key, RoomData val) => _dataMappings[key] = val;

        public IEnumerable<KeyValuePair<Vector2Int, RoomData>> GetAllData() => _dataMappings;

        public IEnumerable<Vector2Int> Rooms => _data.Keys;

        public bool HasRoomPos(Vector2Int pos) => _data.ContainsKey(pos);

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<Vector2Int, RoomType>> GetEnumerator() => _data.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_data).GetEnumerator();
    }
}