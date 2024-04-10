using System;
using System.Linq;
using UnityEngine;
using Util;
using Random = System.Random;

namespace NewFloorGen
{
    public class ComputerFloorGenerator : MonoBehaviour
    {
        [Header("Floors")]
        [Min(0), Tooltip("Floor index for Enemy generation")]
        public int floorNumber;
        [Tooltip("Data to generate this floor")]
        public FloorData[] floorData;
        [Tooltip("Where to generate next place")]
        public Vector2Int generationStartDeltaNewFloor = new(0, 6);
        public int seed;
        [Min(2), Tooltip("Max number of rooms generated")]
        public int minRooms = 12;
        [Min(2), Tooltip("Max number of rooms generated")]
        public int maxRooms = 15;
        [Min(2), Tooltip("Minimum number of rows generated on center row")]
        public int centerRowMin = 8;
        
        [Tooltip("Size of the grid cells (world size)")]
        public Vector2 gridSize = Vector2.one;
        
        [Tooltip("Where the world generates from and where the player starts")]
        public Vector2Int generationStart = Vector2Int.zero;

        private Random _random;

        private void Awake()
        {
            Generate();
        }

        private void OnValidate()
        {
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

        private void GenerateRNG()
        {
            if (seed == 0)
            {
                int guidHash = Guid.NewGuid().GetHashCode();
                Debug.Log($"Random seed: {guidHash}");
                _random = new Random(guidHash);
            }
            else
            {
                Debug.Log($"Reusing old seed: {seed}");
                _random = new Random(seed);
            } 
        }

        private void Generate()
        {
            ComputerFloor floor = new();
            GenerateRNG();
            int roomCount =_random.Next(minRooms,maxRooms+1);
            int middleLength = GenerateMiddleRow(floor, roomCount);
            Vector2Int lastRoom = new Vector2Int(middleLength, 0) + generationStart;
            floor.EnsureRoomsPresent(lastRoom);
            floor.AddConnection(lastRoom, lastRoom - new Vector2Int(1, 0));
            floor[lastRoom].IsBossRoom = true;
            roomCount -= middleLength;

            Vector2Int randomPos = lastRoom + new Vector2Int(1, 0);

            while (roomCount > 0)
            {
                Vector2Int randomRoom = floor.Rooms.ToList().GetRandom(_random);
                while (randomRoom == lastRoom) randomRoom = floor.Rooms.ToList().GetRandom(_random);
                
                floor.EnsureRoomsPresent(randomPos);
                floor.AddConnection(randomRoom, randomPos);
                randomPos += new Vector2Int(1, 0);
                roomCount--;
            }
            GenerateFromGrid(floor);
        }

        private int GenerateMiddleRow(ComputerFloor floor, int maxRoomCount)
        {
            int middle = (maxRoomCount + centerRowMin) / 2;
            int roomsToGenerate =_random.Next(centerRowMin, middle + 1)-1;
            int retVal = roomsToGenerate;
            Vector2Int currVector2Int = generationStart;
            while (roomsToGenerate > 0)
            {
                floor.EnsureRoomsPresent(currVector2Int);
                if (currVector2Int != generationStart)
                {
                    floor.AddConnection(currVector2Int - new Vector2Int(1, 0), currVector2Int);
                }
                currVector2Int += new Vector2Int(1, 0);
                roomsToGenerate--;
            }
            return retVal;
        }

        public void Transition()
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            generationStart += generationStartDeltaNewFloor;
            floorNumber++;
            seed = _random.Next();
            Instantiate(gameObject);
            Destroy(gameObject);
        }
        
        private void GenerateFromGrid(ComputerFloor grid)
        {
            FloorData currFloor = floorData[floorNumber];
            GameObject[] objects = currFloor.normalRoom;
            foreach (GridRoom room in grid)
            {
                GameObject randomCellPrefab = room.IsBossRoom ? currFloor.bossRoom : objects.GetRandom(_random);
                GameObject cellObject = Instantiate(randomCellPrefab, room.Pos * gridSize, Quaternion.identity, transform);
                ComputerRoom roomData = cellObject.GetComponent<ComputerRoom>();
                room.CellObject = cellObject;
                grid.SetData(room.Pos, roomData);
            }

            foreach (GridRoom room in grid)
            {
                ComputerRoom data = room.Data;
                data.neighbors = room.Neighbors.Values.Select(neighbor => neighbor.Data).ToArray();
            }
            
            grid[generationStart].Data.SpawnPlayer();
        }
    }

    [Serializable]
    public struct FloorData
    {
        public GameObject[] normalRoom;
        public GameObject bossRoom;
    }
}