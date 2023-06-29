using System;
using System.Collections.Generic;
using UnityEngine;

using Grid = System.Collections.Generic.Dictionary<UnityEngine.Vector2Int, FloorGen.RoomType>;
using Random = System.Random;

namespace FloorGen
{
    [DisallowMultipleComponent]
    public class FloorGenerator : MonoBehaviour
    {
        public Pair[] pairs;
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

            while (roomCount > 0)
            {
                int index = random.Next(0, toBranch.Count);
                Vector2Int start = toBranch[index];
                toBranch.RemoveAt(index); // yes, this is O(n). But n = like 10, so too bad!
                // ReSharper disable once IdentifierTypo
                int currBranchiness = branchiness;
                do
                {
                    RoomType dir = RandomDirUnweighted(random, AllowedBranchMovements(start, middleLength - 1));
                    if (!grid.ContainsKey(dir.Move(start))) roomCount--;
                    ExpandSide(grid, start, dir);
                    start = dir.Move(start);
                    currBranchiness -= branchinessDecrease;
                } while (roomCount > 0 && CalculateRandomLog(random, currBranchiness));
            }
            GenerateFromGrid(random,grid);
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

        private void GenerateFromGrid(Random random, Grid grid)
        {
            foreach ((Vector2Int pos, RoomType type) in grid)
            {
                GameObject[] objects = _pairsDict[type];
                int index = random.Next(0, objects.Length);
                if (ReferenceEquals(worldParent, null))
                    Instantiate(objects[index], pos * gridSize, Quaternion.identity);
                else Instantiate(objects[index], pos * gridSize, Quaternion.identity, worldParent);
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

    [Serializable]
    public struct Pair
    {
        public RoomType type;
        public GameObject[] roomPrefab;
    }
}