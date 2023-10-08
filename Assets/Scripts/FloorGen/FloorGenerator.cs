using System;
using System.Collections.Generic;
using System.Linq;
using Enemies;
using Enemies.Ranged;
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
        [Header("Floors")]
        [Min(0), Tooltip("Floor index for Enemy generation")]
        public int floorNumber;
        [Tooltip("Place to teleport player in between levels")]
        public Vector3 playerHoldingPosition = new(69420, 666, 0);
        [Tooltip("Where to generate next place")]
        public Vector2Int generationStartOffset = new(0, 6);
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
        [Min(0), Tooltip("Elite enemy health modifier")]
        public float eliteHealthModifier = 2f;
        [Min(0), Tooltip("Elite damage modifier")]
        public float eliteDamageModifier = 2f;
        [Tooltip("Player Upgrade prefab")]
        public GameObject playerUpgradePrefab;
        [Tooltip("Cheese prefab")]
        public GameObject cheesePrefab;
        [Tooltip("Room Changer Prefab")]
        public GameObject roomChangePrefab;
        
        public GameObject[] socketPrefabs;
        [Header("Weapon Generation")]
        public WeaponData[] weaponsList;
        public GameObject weaponPickupPrefab;
        public GameObject weaponUpgradePrefab;
        [Header("Room/Cell Generation")]
        public GameObject bossRoomPrefab;
        public GameObject lootRoomPrefab;
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
        
        [Tooltip("Size of the grid cells (world size)")]
        public Vector2 gridSize = Vector2.one;
        
        [Tooltip("Where the world generates from and where the player starts")]
        public Vector2Int generationStart = Vector2Int.zero;
        [Tooltip("Transform to forcibly set position of")]
        public Transform playerTransform;

        private readonly Dictionary<RoomType, GameObject[]> _pairsDict = new();
        private readonly Dictionary<Vector2, List<GameObject>> _socketPrefabSizes = new();
        private HashSet<UpgradeType> _validUpgradeTypes = new();
        private static readonly float[] UnweightedWeights = { 1, 1, 1, 1 };
        private Random _random;

        private void Awake()
        {
            _validUpgradeTypes = new HashSet<UpgradeType>(UpgradeManager.Instance.ImplementedUpgrades);
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
            Grid grid = new();
            GenerateRNG();
            // Set the player's position based on the starting position
            // (we modify this later, this is just in case it bugs)
            playerTransform.position = generationStart * gridSize;
            
            int roomCount =_random.Next(minRooms,maxRooms+1);

            List<Vector2Int> toBranch = new() { generationStart };
            int middleLength = GenerateMiddleRow(grid, toBranch, roomCount);
            toBranch.Remove(new Vector2Int(middleLength, 0) + generationStart);
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
                Vector2Int start = withinIterationToBranch.RemoveRandom(_random);
                int currBranchiness = branchiness;
                do
                {
                    RoomType dir = RandomDirUnweighted(AllowedBranchMovements(start, middleLength));
                    Vector2Int movedStart = dir.Move(start);
                    if (!grid.ContainsKey(movedStart)) roomCount--;
                    ExpandSide(grid, start, dir);
                    start = movedStart;
                    currBranchiness -= branchinessDecrease;
                } while (roomCount > 0 && CalculateRandomLog(currBranchiness));
            }
            GenerateFromGrid(grid, middleLength);
        }

        private int GenerateMiddleRow(Grid grid, List<Vector2Int> toBranch, int maxRoomCount)
        {
            int middle = (maxRoomCount + centerRowMin) / 2;
            int roomsToGenerate =_random.Next(centerRowMin, middle + 1)-1;
            int retVal = roomsToGenerate;
            Vector2Int currVector2Int = generationStart;
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
            if (socketPrefabs == null) return;
            foreach (GameObject go in socketPrefabs)
            {
                Vector2 size = go.GetComponent<SocketBehaviour>().size;
                if (!_socketPrefabSizes.ContainsKey(size)) _socketPrefabSizes[size] = new List<GameObject>();
                _socketPrefabSizes[size].Add(go);
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

        public void Transition()
        {
            playerTransform.position = playerHoldingPosition;
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            generationStart += generationStartOffset;
            floorNumber++;
            seed = _random.Next();
            Instantiate(gameObject);
            Destroy(gameObject);
        }

        private Vector2Int RandomPosExcept(Grid grid, params Vector2Int[] disallowedRooms)
        {
            return grid.Keys.Except(disallowedRooms).ToList().GetRandom(_random);
        }

        private RoomTransitionInteractable GenerateRoomTransitionInteractable(Vector3 pos, GameObject parent)
        {
            GameObject roomChanger = Instantiate(roomChangePrefab, pos, Quaternion.identity, parent.transform);
            roomChanger.SetActive(false);
            
            RoomTransitionInteractable rti = roomChanger.GetComponent<RoomTransitionInteractable>();
            rti.floorGenerator = this;
            return rti;
        }

        private WeaponPickup GenerateWeaponPickup(Vector3 pos, GameObject parent, bool startsActive)
        {
            GameObject weaponPickupObject = Instantiate(weaponPickupPrefab, pos, Quaternion.identity, parent.transform);
            WeaponPickup weaponPickup = weaponPickupObject.GetComponent<WeaponPickup>();
            HashSet<string> playerCurrInventory =
                PlayerWeaponControl.Instance.Inventory.Where(data => data).Select(data => data.weaponName).ToHashSet();
            List<WeaponData> eligibleWeapons =
                weaponsList.Where(weapon => !playerCurrInventory.Contains(weapon.weaponName)).ToList();
            weaponPickup.weaponData = Instantiate(eligibleWeapons.GetRandom(_random));
            weaponPickup.UpdateSprite();
            weaponPickupObject.SetActive(startsActive);
            return weaponPickup;
        }

        private CheesePickup GenerateCheesePickup(Vector3 position, GameObject parent, int amount)
        {
            GameObject cheesePickupObject = Instantiate(cheesePrefab, position, Quaternion.identity, parent.transform);
            cheesePickupObject.SetActive(false);
            CheesePickup cheesePickup = cheesePickupObject.GetComponent<CheesePickup>();
            cheesePickup.amount = amount;
            return cheesePickup;
        }

        private UpgradePickup GeneratePlayerUpgradePickup(Vector3 position, GameObject parent)
        {
            GameObject playerUpgrade = Instantiate(playerUpgradePrefab, position, Quaternion.identity, parent.transform);
            playerUpgrade.SetActive(false);
            UpgradePickup pickup = playerUpgrade.GetComponent<UpgradePickup>();
            if (_validUpgradeTypes.Count == 0)
            {
                Debug.LogWarning("Empty valid upgrade types - regenerating.");
                _validUpgradeTypes = new HashSet<UpgradeType>(UpgradeManager.Instance.ImplementedUpgrades);
            }
            pickup.upgrade = _validUpgradeTypes.ToList().GetRandom(_random);
            UpgradeManager.Instance.UpgradeMapping[pickup.upgrade].Set(pickup);
            _validUpgradeTypes.Remove(pickup.upgrade);
            return pickup;
        }
        
        private void GenerateEnemies(List<(SocketBehaviour, EnemySpawnType)> sockets, float packSize, RoomData roomData, bool isEndRoom)
        {
            EnemySpawnType[] enumValues = Enum.GetValues(typeof(EnemySpawnType)).Cast<EnemySpawnType>().ToArray();
            while (packSize > 0)
            {
                List<EnemySpawnType> eligibleSpawnTypes = enumValues.Where(type => GetCost(type) <= packSize).ToList();
                if (eligibleSpawnTypes.Count == 0) break;
                bool generatedEnemy = false;
                while (eligibleSpawnTypes.Count > 0)
                {
                    int indexType = eligibleSpawnTypes.GetRandom(_random, out EnemySpawnType type);
                    if (roomData.ignoreSocketEnemySpawns)
                    {
                        GameObject spawnedEnemy = roomData.SpawnEnemy(type,_random);
                        if (!spawnedEnemy)
                        {
                            eligibleSpawnTypes.SwapRemove(indexType);
                            continue;
                        }

                        packSize -= GetCost(type);
                        generatedEnemy = true;
                    }
                    else
                    {
                        List<SocketBehaviour> supportedBehaviours =
                            sockets.Where(pair => (pair.Item2 & type) != 0)
                                .Select(pair => pair.Item1).ToList();
                        if (supportedBehaviours.Count == 0)
                        {
                            eligibleSpawnTypes.SwapRemove(indexType);
                            continue;
                        }
                        SocketBehaviour behaviour = supportedBehaviours.GetRandom(_random);
                        if (behaviour.SpawnEnemy(type, out GameObject spawnedEnemy))
                        {
                            packSize -= GetCost(type);
                            generatedEnemy = true;
                            EntityHealth health = spawnedEnemy.GetComponent<EntityHealth>();
                            Debug.Assert(health, "Added enemy should have EntityHealth attached");
                            roomData.AddEnemy(health);
                        }
                        else
                        {
                            Debug.LogWarning("Tried to spawn enemy of type " + type + " but failed.");
                        }
                    }

                    break;
                }

                if (!generatedEnemy)
                {
                    Debug.LogWarning("Couldn't generate an enemy in an iteration: exiting...");
                    break;
                }
            }

            if (!isEndRoom) return;
            // elite dude
            List<EntityHealth> enemies = roomData.Enemies;
            if (enemies.Count == 0) return;
            EntityHealth randomEnemy = enemies.GetRandom(_random);
            randomEnemy.health = Mathf.RoundToInt(randomEnemy.health * eliteHealthModifier);
            ContactDamage attack = randomEnemy.GetComponent<ContactDamage>();
            if (attack)
            {
                attack.damage = Mathf.RoundToInt(attack.damage * eliteDamageModifier);
            }

            HeavyAttack heavyAttack = randomEnemy.GetComponent<HeavyAttack>();
            if (heavyAttack)
            {
                heavyAttack.attackDamage = Mathf.RoundToInt(heavyAttack.attackDamage * eliteDamageModifier);
            }

            RangedEnemyScript rangedEnemy = randomEnemy.GetComponent<RangedEnemyScript>();
            if (rangedEnemy)
            {
                rangedEnemy.damageMultiplier = eliteDamageModifier;
            }

            if (!attack && !heavyAttack && !rangedEnemy)
            {
                Debug.LogError("Elite enemy doesn't have any damage dealing capabilities.");
            }
        }
        
        private void PopulateSocketsNormal(Vector2Int gridIndex, GameObject cellObject, bool isEndRoom,
            List<(SocketBehaviour, EnemySpawnType)> sockets)
        {
            Vector3 gridPos = gridIndex * gridSize;
            RoomData roomData = cellObject.GetComponent<RoomData>();
            FloorEnemyPack pack = floorEnemyPacks[floorNumber];
            bool generateBoss = isEndRoom && pack.endingHasBoss;
            float packSize = isEndRoom ? pack.endingPackSize : pack.normalPackSize;
            if (!generateBoss)
            {
                roomData.pickup = GeneratePlayerUpgradePickup(gridPos + (Vector3) roomData.powerupSpawnOffset, cellObject);
                roomData.cheesePickup = GenerateCheesePickup(gridPos + (Vector3) roomData.cheeseSpawnOffset, cellObject, isEndRoom ? 10 : 5);
                if (isEndRoom)
                {
                    roomData.weaponPickup = GenerateWeaponPickup( gridPos + (Vector3) roomData.weaponSpawnOffset, cellObject, false);
                    roomData.roomTransitionInteractable = GenerateRoomTransitionInteractable(gridPos + (Vector3)roomData.roomChangeSpawnOffset, cellObject);
                }
                if (gridIndex != generationStart) GenerateEnemies( sockets, packSize, roomData, isEndRoom);
            }
            else
            {
                PopulateSocketsBossRoom( gridIndex, cellObject, sockets);
            }
        }

        private void PopulateSocketsWeaponRoom(Vector2Int gridIndex, GameObject cellObject,
            List<(SocketBehaviour, EnemySpawnType)> sockets)
        {
            RoomData roomData = cellObject.GetComponent<RoomData>();
            Vector3 weaponRoomGridPos = gridIndex * gridSize + roomData.weaponSpawnOffset;
            GenerateWeaponPickup( weaponRoomGridPos, cellObject, true);
        }

        private void PopulateSocketsSmithingRoom(Vector2Int gridIndex, GameObject cellObject,
            List<(SocketBehaviour, EnemySpawnType)> sockets)
        {
            RoomData roomData = cellObject.GetComponent<RoomData>();
            GameObject upgrade = Instantiate(weaponUpgradePrefab, gridIndex * gridSize + roomData.weaponUpgradeSpawnOffset,
                Quaternion.identity, cellObject.transform);
            List<int> buttons = new(new[] { 0, 1, 2, 3, 4 });
            HashSet<int> result = new()
            {
                buttons.RemoveRandom(_random),
                buttons.RemoveRandom(_random)
            };
            upgrade.GetComponent<WeaponUpgradeTrigger>().allowedButtons = result;
        }

        private void PopulateSocketsBossRoom(Vector2Int gridIndex, GameObject cellObject,
            List<(SocketBehaviour, EnemySpawnType)> sockets)
        {
            // TODO BOSS GENERATION
        }
        
        private void GenerateFromGrid(Grid grid, int middleLength)
        {
            Vector2Int finalRoomPos = new Vector2Int(middleLength, 0) + generationStart;
            bool hasWeaponRoom =_random.Next(0, 2) == 1 || PlayerWeaponControl.Instance.HasWeaponSpace;
            Vector2Int weaponRoomPos = hasWeaponRoom ? RandomPosExcept( grid, finalRoomPos) : Vector2Int.zero;
            bool hasSecondSmithingRoom =_random.Next(0, 2) == 1 && PlayerWeaponControl.Instance.HasNoUpgradedWeapons;
            Vector2Int smithOne = RandomPosExcept( grid, finalRoomPos, weaponRoomPos);
            Vector2Int smithTwo = hasSecondSmithingRoom ? RandomPosExcept( grid, finalRoomPos, weaponRoomPos, smithOne) : Vector2Int.zero;
            foreach ((Vector2Int gridIndex, RoomType type) in grid)
            {
                bool isEndRoom = gridIndex == finalRoomPos;
                bool isBossRoom = floorEnemyPacks[floorNumber].endingHasBoss && isEndRoom;
                bool isWeaponRoom = hasWeaponRoom && gridIndex == weaponRoomPos;
                GameObject[] objects = _pairsDict[type];
                GameObject randomCellPrefab = isBossRoom ? bossRoomPrefab : objects.GetRandom(_random);
                randomCellPrefab = isWeaponRoom ? lootRoomPrefab : randomCellPrefab;
                GameObject cellObject = Instantiate(randomCellPrefab, gridIndex * gridSize, Quaternion.identity, worldParent);
                RoomData roomData = cellObject.GetComponent<RoomData>();
                roomData.EnsureType(type);
                List<(SocketBehaviour, EnemySpawnType)> sockets = roomData.GenerateSockets(_random, _socketPrefabSizes);
                if (hasWeaponRoom && gridIndex == weaponRoomPos)
                {
                    PopulateSocketsWeaponRoom( gridIndex, cellObject, sockets);
                }
                else if (gridIndex == smithOne || (hasSecondSmithingRoom && gridIndex == smithTwo))
                {
                    PopulateSocketsSmithingRoom( gridIndex, cellObject, sockets);
                }
                else
                {
                    PopulateSocketsNormal( gridIndex, cellObject, isEndRoom, sockets);
                }
                
                
                // Set the player's position based on the starting position
                if (generationStart == gridIndex)
                {
                    playerTransform.position = generationStart * gridSize + roomData.playerSpawnOffset;
                }
            }
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

        private bool CalculateRandomLog(int bias)
        {
            float prob = 1 / (1 + Mathf.Exp(-bias));
            return _random.Next(0, 100) < prob * 100;
        }

        private RoomType[] AllowedBranchMovements(Vector2Int pos, int middleLen)
        {
            List<RoomType> rooms = Enum.GetValues(typeof(RoomType)).Cast<RoomType>().ToList();
            if (pos.x >= generationStart.x + middleLen - 1) rooms.Remove(RoomType.RightOpen);
            else if (pos.x <= generationStart.x) rooms.Remove(RoomType.LeftOpen);
            
            if (pos.y == generationStart.y)
            {
                if (pos.x > generationStart.x) rooms.Remove(RoomType.LeftOpen);
                rooms.Remove(RoomType.RightOpen);
            }

            int minY = generationStart.y - maxRows/2;
            int maxY = generationStart.y + maxRows/2;
            if (pos.y >= maxY) rooms.Remove(RoomType.TopOpen);
            if (pos.y <= minY) rooms.Remove(RoomType.BottomOpen);
            return rooms.ToArray();
        }

        private RoomType RandomDir(RoomType[] types, float[] weights)
        {
            Debug.Assert(types.Length > 0);
            float sum = weights[0];
            for (int i = 1; i < types.Length; i++) sum += weights[i];

            int rand = _random.Next(0, Mathf.RoundToInt(sum * 100));
            float cumSum = 0;
            for (int i = 0; i < types.Length-1; i++)
            {
                if (rand < cumSum + weights[i] * 100) return types[i];
                cumSum += weights[i] * 100;
            }

            return types[^1];
        }

        private RoomType RandomDirUnweighted(RoomType[] types) => RandomDir(types, UnweightedWeights);
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