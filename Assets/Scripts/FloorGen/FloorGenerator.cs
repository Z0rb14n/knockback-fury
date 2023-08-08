using System;
using System.Collections.Generic;
using System.Linq;
using FileSave;
using Player;
using UnityEngine;
using Upgrades;
using Util;
using Weapons;
using Grid = System.Collections.Generic.Dictionary<UnityEngine.Vector2Int, FloorGen.RoomType>;
using Random = System.Random;

namespace FloorGen
{
    [DisallowMultipleComponent]
    public class FloorGenerator : MonoBehaviour
    {
        [Min(0), Tooltip("Floor index for Enemy generation")]
        public int floorNumber;
        [Header("Enemy generation")]
        public FloorEnemyPack[] floorEnemyPacks = {
            new() { normalPackSize = 8, endingPackSize = 14, endingHasBoss = false },
            new() { normalPackSize = 10, endingPackSize = 17, endingHasBoss = false },
            new() { normalPackSize = 13, endingPackSize = 0, endingHasBoss = true },
        };
        
        [Min(0), Tooltip("Ranged Pack Cost")]
        public float rangedPackCost = 1.5f;
        [Min(0), Tooltip("Heavy Pack Cost")]
        public float heavyPackCost = 3f;
        [Min(0), Tooltip("Chaser Pack Cost")]
        public float chaserPackCost = 1f;
        [Min(0), Tooltip("Jumper Pack Cost")]
        public float jumperPackCost = 2f;
        [Tooltip("Player Upgrade prefab")]
        public GameObject playerUpgradePrefab;
        [Tooltip("Cheese prefab")]
        public GameObject cheesePrefab;
        
        [Header("Socket Generation")]
        public Layout[] layouts;
        public SocketObject[] socketObjects;
        [Header("Weapon Generation")]
        public WeaponData[] weaponsList;
        public GameObject weaponPickupPrefab;
        public GameObject weaponUpgradePrefab;
        [Header("Room/Cell Generation")]
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

        public int ToPreview { get; set; } = -1;

        private readonly Dictionary<RoomType, GameObject[]> _pairsDict = new();
        private readonly Dictionary<Vector2, List<GameObject>> _socketPrefabSizes = new();
        private static readonly float[] UnweightedWeights = { 1, 1, 1, 1 };

        private void Awake()
        {
            _socketPrefabSizes.Clear();
            GeneratePrefabSizes();
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

        private void GeneratePrefabSizes()
        {
            foreach (SocketObject so in socketObjects)
            {
                if (!_socketPrefabSizes.ContainsKey(so.size))
                    _socketPrefabSizes[so.size] = new();
                _socketPrefabSizes[so.size].Add(so.prefab);
            }
        }

        private float GetCost(EnemySpawnType type)
        {
            switch (type)
            {
                case EnemySpawnType.Chaser:
                    return chaserPackCost;
                case EnemySpawnType.Heavy:
                    return heavyPackCost;
                case EnemySpawnType.Jumper:
                    return jumperPackCost;
                case EnemySpawnType.Ranged:
                    return rangedPackCost;
                default:
                    return 999;
            }
        }

        private static bool WeaponRoomPos(Random random, Grid grid, out Vector2Int pos, params Vector2Int[] disallowedRooms)
        {
            bool hasWeaponRoom = random.Next(0, 2) == 1 || PlayerWeaponControl.Instance.HasWeaponSpace;
            pos = Vector2Int.zero;
            if (!hasWeaponRoom) return false;
            pos = grid.Keys.Except(disallowedRooms).ToList().GetRandom(random);
            return true;
        }
        
        private static bool SmithingRoomPos(Random random, Grid grid, out Vector2Int posOne, out Vector2Int posTwo, params Vector2Int[] disallowedRooms)
        {
            bool hasSecondSmithingRoom = random.Next(0, 2) == 1 && PlayerWeaponControl.Instance.HasNoUpgradedWeapons;
            HashSet<Vector2Int> disallowed = new(disallowedRooms);
            posOne = grid.Keys.Except(disallowed).ToList().GetRandom(random);
            posTwo = Vector2Int.zero;
            if (!hasSecondSmithingRoom) return false;
            disallowed.Add(posOne);
            posTwo = grid.Keys.Except(disallowed).ToList().GetRandom(random);
            return true;
        }

        // TODO: method length too long
        private void GenerateSockets(Random random, Vector2Int gridIndex, GameObject cellObject, bool hasEnemies, bool isEndRoom)
        {
            Vector3 gridPos = gridIndex * gridSize;
            List<(SocketBehaviour, EnemySpawnType)> socketBehaviours = new();
            Layout layout = layouts.GetRandom(random);
            foreach (SocketShape shape in layout.sockets)
            {
                Vector3 pos = (Vector3)shape.position + gridPos;
                if (_socketPrefabSizes.TryGetValue(shape.size, out List<GameObject> possibleSocketPrefabs) && possibleSocketPrefabs.Count > 0)
                {
                    GameObject createdSocket = Instantiate(possibleSocketPrefabs.GetRandom(random), pos, Quaternion.identity, cellObject.transform);
                    SocketBehaviour behaviour = createdSocket.GetComponent<SocketBehaviour>();
                    EnemySpawnType type = behaviour.AllowedSpawnTypes;
                    if (behaviour && type != 0) socketBehaviours.Add((behaviour, behaviour.AllowedSpawnTypes));
                }
                else
                {
                    Debug.LogWarning($"[FloorGenerator::GenerateFromGrid] No game object entry for socket of size {shape.size}; skipping");
                }
            }

            if (!hasEnemies) return;

            RoomEnemyManager manager = cellObject.AddComponent<RoomEnemyManager>();
            FloorEnemyPack pack = floorEnemyPacks[floorNumber];
            bool generateBoss = isEndRoom && pack.endingHasBoss;
            float packSize = isEndRoom ? pack.endingPackSize : pack.normalPackSize;
            if (!generateBoss)
            {
                // TODO FIX HACKS: artificially increased x/y by 1
                // generate player upgrade
                GameObject playerUpgrade = Instantiate(playerUpgradePrefab, gridPos + Vector3.up, Quaternion.identity, cellObject.transform);
                playerUpgrade.SetActive(false);
                UpgradePickup pickup = playerUpgrade.GetComponent<UpgradePickup>();
                manager.pickup = pickup;
                pickup.upgrade = Enum.GetValues(typeof(UpgradeType)).Cast<UpgradeType>().ToList().GetRandom(random);
                // generate cheese
                GameObject cheesePickupObject = Instantiate(cheesePrefab, gridPos + Vector3.left + Vector3.up, Quaternion.identity, cellObject.transform);
                cheesePickupObject.SetActive(false);
                CheesePickup cheesePickup = cheesePickupObject.GetComponent<CheesePickup>();
                manager.cheesePickup = cheesePickup;
                cheesePickup.amount = 5;
                // generate enemies
                while (packSize > 0)
                {
                    List<EnemySpawnType> eligibleSpawnTypes =
                        Enum.GetValues(typeof(EnemySpawnType)).Cast<EnemySpawnType>().Where(type => GetCost(type) <= packSize).ToList();
                    if (eligibleSpawnTypes.Count == 0) break;
                    while (eligibleSpawnTypes.Count > 0)
                    {
                        int indexType = eligibleSpawnTypes.GetRandom(random, out EnemySpawnType type);
                        List<SocketBehaviour> supportedBehaviours =
                            socketBehaviours.Where(pair => (pair.Item2 & type) != 0)
                                .Select(pair => pair.Item1).ToList();
                        if (supportedBehaviours.Count == 0) eligibleSpawnTypes.SwapRemove(indexType);
                        SocketBehaviour behaviour = supportedBehaviours.GetRandom(random);
                        if (behaviour.SpawnEnemy(type, out GameObject spawnedEnemy))
                        {
                            packSize -= GetCost(type);
                            EntityHealth health = spawnedEnemy.GetComponent<EntityHealth>();
                            if (health)
                            {
                                manager.AddEnemy(health);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Tried to spawn enemy of type " + type + " but failed.");
                        }
                        break;
                    }
                }
            }
            else
            {
                // TODO BOSS GENERATION
            }
        }
        
        // TODO: method length potentially too long
        private void GenerateFromGrid(Random random, Grid grid, int middleLength)
        {
            Vector2Int finalRoomPos = new(middleLength, 0);
            bool hasWeaponRoom = WeaponRoomPos(random, grid, out Vector2Int weaponRoomPos, finalRoomPos);

            Vector2Int smithOne, smithTwo;
            bool hasSecondSmithingRoom = hasWeaponRoom ?
                SmithingRoomPos(random, grid, out smithOne, out smithTwo, finalRoomPos, weaponRoomPos)
                : SmithingRoomPos(random, grid, out smithOne, out smithTwo, finalRoomPos);
            Dictionary<Vector2Int, GameObject> cellObjects = new();
            foreach ((Vector2Int gridIndex, RoomType type) in grid)
            {
                GameObject[] objects = _pairsDict[type];
                Vector3 gridPos = gridIndex * gridSize;
                GameObject randomCellPrefab = objects.GetRandom(random);
                GameObject cellObject = worldParent ?
                    Instantiate(randomCellPrefab, gridPos, Quaternion.identity, worldParent) :
                    Instantiate(randomCellPrefab, gridPos, Quaternion.identity);
                cellObjects.Add(gridIndex, cellObject);
                bool hasEnemies = gridIndex != smithOne &&
                                  (!hasWeaponRoom || gridIndex != weaponRoomPos) &&
                                  (!hasSecondSmithingRoom || gridIndex != smithTwo);
                GenerateSockets(random, gridIndex, cellObject, hasEnemies, gridIndex == finalRoomPos);
            }

            if (hasWeaponRoom)
            {
                // TODO FIX HACK: artificially increased y by 1
                Vector3 weaponRoomGridPos = weaponRoomPos * gridSize + Vector2.up;
                GameObject weaponPickupObject = Instantiate(weaponPickupPrefab, weaponRoomGridPos,
                    Quaternion.identity, cellObjects[weaponRoomPos].transform);
                WeaponPickup weaponPickup = weaponPickupObject.GetComponent<WeaponPickup>();
                weaponPickup.weaponData = Instantiate(weaponsList.GetRandom(random));
                weaponPickup.UpdateSprite();
            }

            // TODO FIX HACK: artificially increased y by 1
            Instantiate(weaponUpgradePrefab, smithOne * gridSize + Vector2.up,
                Quaternion.identity, cellObjects[smithOne].transform);
            if (hasSecondSmithingRoom)
                Instantiate(weaponUpgradePrefab, smithTwo * gridSize + Vector2.up, Quaternion.identity,
                    cellObjects[smithTwo].transform);
        }

        private static void ExpandSide(Grid grid, Vector2Int vector2Int, RoomType dir)
        {
            // can't be mixed
            Debug.Assert(dir.GetParts().Count == 1, "[FloorGenerator::ExpandSide] room type isn't a singular enum");
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
            List<RoomType> rooms = Enum.GetValues(typeof(RoomType)).Cast<RoomType>().ToList();
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

    /// <summary>
    /// Enemy pack size for a floor/room. See design doc.
    /// </summary>
    [Serializable]
    public struct FloorEnemyPack
    {
        public int normalPackSize;
        public int endingPackSize;
        public bool endingHasBoss;
    }
}