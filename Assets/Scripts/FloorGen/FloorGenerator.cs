using System;
using UnityEngine;

using Cell = System.ValueTuple<int, int>;
using Grid = System.Collections.Generic.Dictionary<System.ValueTuple<int, int>, FloorGen.RoomType>;
using Random = System.Random;

namespace FloorGen
{
    public class FloorGenerator : MonoBehaviour
    {
        public Pair[] pairs;
        public int seed = 0;
        public int pathLength = 4;
        public int pathHeight = 1;

        private void Awake()
        {
            Generate();
        }

        public void Generate()
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

            int remainingLength = pathLength;
            int remainingHeight = pathHeight;
            Cell currCell = (0, 0);
            while (remainingLength > 0)
            {
                int upwardBias = remainingHeight - (remainingLength >> 1);
                bool shouldGoUp = CalculateRandom(random, upwardBias);
                bool shouldGoDown = !shouldGoUp && CalculateRandom(random, -upwardBias);
                ExpandSide(grid, currCell, RoomType.RightOpen);
                currCell = RoomType.RightOpen.Move(currCell);
                if (shouldGoUp)
                {
                    ExpandSide(grid, currCell, RoomType.TopOpen);
                    currCell = RoomType.TopOpen.Move(currCell);
                } else if (shouldGoDown)
                {
                    ExpandSide(grid, currCell, RoomType.BottomOpen);
                    currCell = RoomType.BottomOpen.Move(currCell);
                }

                remainingLength--;
            }

        }

        private static void ExpandSide(Grid grid, Cell cell, RoomType dir)
        {
            // can't be mixed
            Debug.Assert(dir is RoomType.BottomOpen or RoomType.LeftOpen or RoomType.RightOpen or RoomType.TopOpen);
            cell = dir.Move(cell);
            if (grid.ContainsKey(cell))
                grid[cell] |= dir.GetOpposing();
            else
                grid[cell] = dir.GetOpposing();

        }

        private static bool CalculateRandom(Random random, int bias)
        {
            float exp = Mathf.Exp(bias);
            float prob = exp / (1 + exp);
            return random.Next(0, 100) > prob * 100;
        }
    }

    [Serializable]
    public struct Pair
    {
        public RoomType type;
        public GameObject[] roomPrefab;
    }
}