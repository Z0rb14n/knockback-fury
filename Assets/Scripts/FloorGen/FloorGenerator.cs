using System;
using System.Collections.Generic;
using System.Linq;
using Player;
using UnityEngine;
using Util;
using Weapons;
using Grid = System.Collections.Generic.Dictionary<UnityEngine.Vector2Int, FloorGen.RoomType>;
using Random = System.Random;

namespace FloorGen
{
    [DisallowMultipleComponent]
    public class FloorGenerator : MonoBehaviour
    {
        [Header("Socket Generation")]
        public Pair[] pairs;
        public Layout[] layouts;
        public SocketObject[] socketObjects;
        [Header("Weapon Generation")]
        public WeaponData[] weaponsList;
        public GameObject weaponPickupPrefab;
        [Header("Room/Cell Generation")]
        public int seed;
        [Min(0), Tooltip("Number of rows for generation")]
        public int maxRows = 3;
        [Min(0), Tooltip("Max number of rooms generated")]
        public int minRooms = 12;
        [Min(0), Tooltip("Max number of rooms generated")]
        public int maxRooms = 15;
        [Min(0), Tooltip("Minimum number of rows generated on center row")]
        public int centerRowMin = 8;
        // ReSharper disable once StringLiteralTypo
        [Tooltip("Branchiness of final branch sweep")]
        // ReSharper disable two IdentifierTypo
        public int branchiness = 1;
        public int branchinessDecrease = 4;
        public Transform worldParent;
        
        public Vector2 gridSize = Vector2.one;

        public int ToPreview { get; set; } = -1;

        private readonly Dictionary<RoomType, GameObject[]> _pairsDict = new();
        private static readonly float[] UnweightedWeights = { 1, 1, 1, 1 };

        private void Awake()
        {
            _pairsDict.Clear();
            foreach (Pair pair in pairs)
            {
                // bro unity wtf
                if (pair.type == (RoomType)(-1))
                {
                    _pairsDict.Add(RoomType.BottomOpen | RoomType.LeftOpen | RoomType.RightOpen | RoomType.TopOpen, pair.roomPrefab);
                }
                _pairsDict.Add(pair.type, pair.roomPrefab);
            }
            Generate();
        }

        private void OnValidate()
        {
            _pairsDict.Clear();
            foreach (Pair pair in pairs) _pairsDict.Add(pair.type, pair.roomPrefab);
            if (minRooms < centerRowMin)
            {
                Debug.LogError("MinRooms < centerRowMin; clamping.");
                minRooms = centerRowMin;
            }

            if (maxRooms < minRooms)
            {
                Debug.LogError("MaxRooms < minRooms; clamping.");
                maxRooms = minRooms;
            }
        }

        private Random GenerateRNG()
        {
            if (seed == 0)
            {
                int guidHash = Guid.NewGuid().GetHashCode();
                Debug.Log($"Random seed: {guidHash}");
                return new Random(guidHash);
            }
            Debug.Log($"Reusing old seed: {seed}");
            return new Random(seed);
        }

        private void Generate()
        {
            Grid grid = new();
            Random random = GenerateRNG();
            
            int roomCount = random.Next(minRooms,maxRooms+1);

            List<Vector2Int> toBranch = new() { Vector2Int.zero };
            int middleLength = GenerateMiddleRow(random, grid, toBranch, roomCount);
            roomCount -= middleLength;

            List<Vector2Int> withinIterationToBranch = new(toBranch);

            while (roomCount > 0)
            {
                if (withinIterationToBranch.Count == 0)
                {
                    // Error reproducible on seed -2112351525
                    Debug.LogWarning("Ran out of rooms to branch from - re-adding initial list.");
                    withinIterationToBranch.AddRange(toBranch);
                }
                // Swap with last element and remove last instead of removing by index.
                int index = random.Next(0, withinIterationToBranch.Count);
                Vector2Int start = withinIterationToBranch[index];
                withinIterationToBranch.SwapRemove(index);
                int currBranchiness = branchiness;
                do
                {
                    RoomType dir = RandomDirUnweighted(random, AllowedBranchMovements(start, middleLength - 1));
                    Vector2Int movedStart = dir.Move(start);
                    if (!grid.ContainsKey(movedStart)) roomCount--;
                    ExpandSide(grid, start, dir);
                    start = movedStart;
                    currBranchiness -= branchinessDecrease;
                } while (roomCount > 0 && CalculateRandomLog(random, currBranchiness));
            }
            GenerateFromGrid(random, grid, middleLength);
        }

        private int GenerateMiddleRow(Random random, Grid grid, List<Vector2Int> toBranch, int maxRoomCount)
        {
            int middle = (maxRoomCount + centerRowMin) / 2;
            int roomsToGenerate = random.Next(centerRowMin, middle + 1)-1;
            int retVal = roomsToGenerate;
            Vector2Int currVector2Int = Vector2Int.zero;
            while (roomsToGenerate > 0)
            {
                ExpandSide(grid, currVector2Int, RoomType.RightOpen);
                currVector2Int = RoomType.RightOpen.Move(currVector2Int);
                roomsToGenerate--;
                if (roomsToGenerate != 0)
                    toBranch.Add(currVector2Int);
            }
            return retVal;
        }

        private void GenerateFromGrid(Random random, Grid grid, int middleLength)
        {
            Dictionary<Vector2, List<GameObject>> prefabSizes = new();
            foreach (SocketObject so in socketObjects)
            {
                if (!prefabSizes.ContainsKey(so.size))
                    prefabSizes[so.size] = new();
                prefabSizes[so.size].Add(so.prefab);
            }
            bool hasWeaponRoom = random.Next(0, 2) == 1 || PlayerWeaponControl.Instance.HasWeaponSpace;
            Vector2Int weaponRoomPos = Vector2Int.zero;
            if (hasWeaponRoom)
            {
                List<Vector2Int> positions = grid.Keys.Where(key => key != new Vector2Int(middleLength,0)).ToList();
                weaponRoomPos = positions.GetRandom(random);
            }
            foreach ((Vector2Int gridIndex, RoomType type) in grid)
            {
                GameObject[] objects = _pairsDict[type];
                Vector3 gridPos = gridIndex * gridSize;
                GameObject randomCellPrefab = objects.GetRandom(random);
                GameObject cellObject = ReferenceEquals(worldParent, null) ?
                    Instantiate(randomCellPrefab, gridPos, Quaternion.identity) :
                    Instantiate(randomCellPrefab, gridPos, Quaternion.identity, worldParent);
                // cba to save cellObjects so i'll just do a check here
                if (hasWeaponRoom && weaponRoomPos == gridIndex)
                {
                    GameObject weaponPickupObject = Instantiate(weaponPickupPrefab, gridPos, Quaternion.identity,
                        cellObject.transform);
                    WeaponPickup weaponPickup = weaponPickupObject.GetComponent<WeaponPickup>();
                    weaponPickup.weaponData = Instantiate(weaponsList.GetRandom(random));
                    weaponPickup.UpdateSprite();
                }
                Layout layout = layouts.GetRandom(random);
                foreach (SocketShape shape in layout.sockets)
                {
                    Vector3 pos = (Vector3)shape.position + gridPos;
                    if (prefabSizes.TryGetValue(shape.size, out List<GameObject> possibleSocketPrefabs) && possibleSocketPrefabs.Count > 0)
                    {
                        Instantiate(possibleSocketPrefabs.GetRandom(random), pos, Quaternion.identity, cellObject.transform);
                    }
                    else
                    {
                        Debug.LogWarning($"[FloorGenerator::GenerateFromGrid] No game object entry for socket of size {shape.size}; skipping");
                    }
                }
            }
        }

        private static void ExpandSide(Grid grid, Vector2Int vector2Int, RoomType dir)
        {
            // can't be mixed
            Debug.Assert(dir is RoomType.BottomOpen or RoomType.LeftOpen or RoomType.RightOpen or RoomType.TopOpen);
            if (grid.ContainsKey(vector2Int)) grid[vector2Int] |= dir;
            else grid[vector2Int] = dir;
            vector2Int = dir.Move(vector2Int);
            if (grid.ContainsKey(vector2Int)) grid[vector2Int] |= dir.GetOpposing();
            else grid[vector2Int] = dir.GetOpposing();
        }

        private static bool CalculateRandomLog(Random random, int bias)
        {
            float prob = 1 / (1 + Mathf.Exp(-bias));
            return random.Next(0, 100) < prob * 100;
        }

        private RoomType[] AllowedBranchMovements(Vector2Int pos, int finalRoomX)
        {
            List<RoomType> rooms = new()
            {
                RoomType.BottomOpen,
                RoomType.TopOpen,
                RoomType.LeftOpen,
                RoomType.RightOpen
            };
            if (pos.x >= finalRoomX) rooms.Remove(RoomType.RightOpen);
            if (pos.y == 0)
            {
                if (pos.x > 0) rooms.Remove(RoomType.LeftOpen);
                rooms.Remove(RoomType.RightOpen);
            }

            int minY = -maxRows / 2;
            int maxY = maxRows + minY + 1;
            if (pos.y >= maxY) rooms.Remove(RoomType.TopOpen);
            if (pos.y <= minY) rooms.Remove(RoomType.BottomOpen);
            return rooms.ToArray();
        }

        private static RoomType RandomDir(Random random, RoomType[] types, float[] weights)
        {
            Debug.Assert(types.Length > 0);
            float sum = weights[0];
            for (int i = 1; i < types.Length; i++) sum += weights[i];

            int rand = random.Next(0, Mathf.RoundToInt(sum * 100));
            float cumSum = 0;
            for (int i = 0; i < types.Length-1; i++)
            {
                if (rand < cumSum + weights[i] * 100) return types[i];
                cumSum += weights[i] * 100;
            }

            return types[^1];
        }

        private static RoomType RandomDirUnweighted(Random random, RoomType[] types) => RandomDir(random, types, UnweightedWeights);
    }

    /// <summary>
    /// Grouping of room/cell and open walls
    /// </summary>
    [Serializable]
    public struct Pair
    {
        public RoomType type;
        public GameObject[] roomPrefab;
    }

    /// <summary>
    /// Layout of sockets to be placed within a room/cell
    /// </summary>
    [Serializable]
    public struct Layout
    {
        public SocketShape[] sockets;
    }

    /// <summary>
    /// Socket prefab and its corresponding size
    /// </summary>
    [Serializable]
    public struct SocketObject
    {
        public Vector2 size;
        public GameObject prefab;
    }

    /// <summary>
    /// Individual size/position pairing for socket location
    /// </summary>
    [Serializable]
    public struct SocketShape
    {
        public Vector2 size;
        public Vector2 position;
    }
}