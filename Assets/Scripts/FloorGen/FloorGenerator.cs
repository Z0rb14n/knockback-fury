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
        public int pathLength = 4;
        public int pathHeight = 1;

        [Min(0), Tooltip("Bias towards end goal. Not guaranteed to terminate if set to 0")]
        public float biasFactor = 10;
        // ReSharper disable once StringLiteralTypo
        [Tooltip("Branchiness of final branch sweep")]
        // ReSharper disable two IdentifierTypo
        public int branchiness = 1;
        public int branchinessDecrease = 4;
        public Transform worldParent;
        
        public Vector2 gridSize = Vector2.one;

        private readonly Dictionary<RoomType, GameObject[]> _pairsDict = new();

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
        }

        private void Generate()
        {
            Grid grid = new();
            Random random;
            if (seed == 0)
            {
                int guidHash = Guid.NewGuid().GetHashCode();
                random = new Random(guidHash);
                Debug.Log(guidHash.ToString());
            }
            else
                random = new Random(seed);

            Vector2Int currVector2Int = Vector2Int.zero;
            Vector2Int target = new(pathLength,pathHeight);
            Queue<Vector2Int> toBranch = new();
            toBranch.Enqueue(currVector2Int);
            int iterationCount = 0;
            while (currVector2Int != target)
            {
                // randomly move towards target; bias towards moving towards (duh)
                // remember: it's bottom, left, right, top
                float[] weights = { 1, 1, 1, 1 };
                int dx = target.x - currVector2Int.x;
                int dy = target.y - currVector2Int.y;
                if (dx > 0) weights[2] += dx * biasFactor;
                else weights[1] -= dx * biasFactor;
                if (dy > 0) weights[3] += dy * biasFactor;
                else weights[0] -= dy * biasFactor;
                RoomType dir = RandomDir(random, weights);
                ExpandSide(grid, currVector2Int, dir);
                currVector2Int = dir.Move(currVector2Int);
                toBranch.Enqueue(currVector2Int);
                iterationCount++;
                Debug.Log(currVector2Int);
                if (iterationCount > 100)
                {
                    // prevent infinite loop from freezing unity (lmao)
                    throw new Exception($"Runtime Iteration Limit Reached: {iterationCount}");
                }
            }

            while (toBranch.Count > 0)
            {
                // ReSharper disable once IdentifierTypo
                int currBranchiness = branchiness;
                currVector2Int = toBranch.Dequeue();
                while (CalculateRandomLog(random, currBranchiness))
                {
                    RoomType dir = RandomDirUnweighted(random);
                    ExpandSide(grid, currVector2Int, dir);
                    currVector2Int = dir.Move(currVector2Int);
                    currBranchiness -= branchinessDecrease;
                }
            }
            GenerateFromGrid(random,grid);
        }

        private void GenerateFromGrid(Random random, Grid grid)
        {
            foreach (KeyValuePair<Vector2Int, RoomType> pair in grid)
            {
                Vector2Int pos = pair.Key;
                RoomType type = pair.Value;
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

        private static RoomType RandomDir(Random random, float[] weights)
        {
            Debug.Assert(weights.Length == 4);
            float sum = weights[0];
            for (int i = 1; i < weights.Length; i++)
            {
                sum += weights[i];
            }

            int rand = random.Next(0, Mathf.RoundToInt(sum * 100));
            if (rand < weights[0] * 100) return RoomType.BottomOpen;
            if (rand < weights[1] * 100) return RoomType.LeftOpen;
            if (rand < weights[2] * 100) return RoomType.RightOpen;
            return RoomType.TopOpen;
        }

        private static RoomType RandomDirUnweighted(Random random)
        {
            int rand = random.Next(0, 4);
            return rand switch
            {
                0 => RoomType.BottomOpen,
                1 => RoomType.LeftOpen,
                2 => RoomType.RightOpen,
                3 => RoomType.TopOpen,
                _ => 0
            };
        }
    }

    [Serializable]
    public struct Pair
    {
        public RoomType type;
        public GameObject[] roomPrefab;
    }
}