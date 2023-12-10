using System;
using System.Collections.Generic;
using System.Linq;
using Enemies;
using FileSave;
using UnityEngine;
using UnityEngine.Tilemaps;
using Upgrades;
using Util;
using Weapons;

using Random = System.Random;

namespace FloorGen
{
    [DisallowMultipleComponent]
    public class RoomData : MonoBehaviour
    {
        [HideInInspector]
        public UpgradePickup pickup;
        [HideInInspector]
        public CheesePickup cheesePickup;
        [HideInInspector]
        public WeaponPickup weaponPickup;
        [HideInInspector]
        public RoomTransitionInteractable roomTransitionInteractable;

        [Tooltip("Spawn offsets of weapon drops")]
        public Vector2 weaponSpawnOffset = Vector2.up;
        [Tooltip("Spawn offset of cheese drops")]
        public Vector2 cheeseSpawnOffset = Vector2.up + Vector2.left;
        [Tooltip("Spawn offset of player powerups")]
        public Vector2 powerupSpawnOffset = Vector2.up;
        [Tooltip("Spawn offset of weapon upgrades")]
        public Vector2 weaponUpgradeSpawnOffset = Vector2.up;
        [Tooltip("Spawn offset of room changer")]
        public Vector2 roomChangeSpawnOffset;
        [Tooltip("Player spawn offset")]
        public Vector2 playerSpawnOffset = Vector2.zero;

        [Tooltip("Room Size")]
        public Vector2 roomSize;
        [Tooltip("Room center offset - only changes gizmo!")]
        public Vector2 roomCenterOffset;

        public SocketShape[] sockets;

        public bool ignoreSocketEnemySpawns = true;

        public TileBase socketTile;

        private readonly HashSet<EntityHealth> _enemies = new();
        private readonly Dictionary<EntityHealth, EnemyBehaviour> _enemyBehaviours = new();

        public List<EntityHealth> Enemies => new(_enemies);

        public bool PlayerVisited { get; private set; }
        public bool PlayerPresent { get; private set; }

        private PipeBehaviour[] _pipes;

        public PipeBehaviour FirstUnoccupiedPipe => Pipes.FirstOrDefault(behaviour => !behaviour.OtherPipe);

        public PipeBehaviour[] Pipes => _pipes ??= GetComponentsInChildren<PipeBehaviour>();

        public event Action OnPlayerVisit;

        private EnemySpawnPoint[] _spawnPoints;
        private Dictionary<EnemySpawnType, List<EnemySpawnPoint>> _spawnTypeMapping;
        private RoomTrigger _roomTrigger;
        private bool _mobsShouldDisappear;

        private void Awake()
        {
            _roomTrigger = GetComponentInChildren<RoomTrigger>();
            InitializePointsAndMappings();
            _mobsShouldDisappear = GetComponentInParent<FloorGenerator>()?.mobsShouldDisappear ?? false;
            _pipes ??= GetComponentsInChildren<PipeBehaviour>();
        }

        private void Start()
        {
            if (!_roomTrigger) return;
            RoomTriggerOnOnPlayerExit();
            _roomTrigger.OnPlayerEnter += RoomTriggerOnOnPlayerEnter;
            _roomTrigger.OnPlayerExit += RoomTriggerOnOnPlayerExit;
        }

        private void RoomTriggerOnOnPlayerExit()
        {
            foreach (EnemyBehaviour behaviours in _enemyBehaviours.Values)
            {
                if (_mobsShouldDisappear) behaviours.gameObject.SetActive(false);
                else behaviours.enabled = false;
            }

            PlayerPresent = false;
        }

        private void RoomTriggerOnOnPlayerEnter()
        {
            foreach (EnemyBehaviour behaviours in _enemyBehaviours.Values)
            {
                if (_mobsShouldDisappear) behaviours.gameObject.SetActive(true);
                else behaviours.enabled = true;
            }

            if (!PlayerVisited)
            {
                PlayerVisited = true;
                OnPlayerVisit?.Invoke();
            }

            PlayerPresent = true;
        }

        private void InitializePointsAndMappings()
        {
            _spawnPoints = GetComponentsInChildren<EnemySpawnPoint>();
            _spawnTypeMapping = new Dictionary<EnemySpawnType, List<EnemySpawnPoint>>();
            foreach (EnemySpawnType t in Enum.GetValues(typeof(EnemySpawnType)).Cast<EnemySpawnType>())
            {
                foreach (EnemySpawnPoint point in _spawnPoints)
                {
                    if ((point.types & t) == 0) continue;
                    if (_spawnTypeMapping.ContainsKey(t)) _spawnTypeMapping[t].Add(point);
                    else _spawnTypeMapping[t] = new List<EnemySpawnPoint>(new []{point});
                }
            }
        }

        /// <summary>
        /// Of a socket given its start top left coordinate, find width and height
        /// </summary>
        /// <param name="tilemap">Tilemap to check grid cells of</param>
        /// <param name="bounds">Max bounds of tilemap</param>
        /// <param name="start">Starting position</param>
        /// <returns>Size, assuming rectangular sockets</returns>
        private Vector2Int SocketSize(Tilemap tilemap, BoundsInt bounds, Vector2Int start)
        {
            int width = 0;
            int height = 0;
            for (int x = start.x; x <= bounds.xMax; x++)
            {
                if (tilemap.GetTile(new Vector3Int(x,start.y,0)) == socketTile) width++;
                else break;
            }
            
            for (int y = start.y; y <= bounds.yMax; y++)
            {
                if (tilemap.GetTile(new Vector3Int(start.x,y,0)) == socketTile) height++;
                else break;
            }

            return new Vector2Int(width, height);
        }

        /// <summary>
        /// Updates and locates all sockets in this tilemap, indicated by a specific tile.
        /// </summary>
        /// <remarks>
        /// Only works on rectangular sockets as it finds the top left corner and iterates over two axes repeatedly.
        ///
        /// Behaviour is undefined for non-rectangular sockets.
        /// </remarks>
        // ReSharper disable Unity.PerformanceAnalysis
        public void LocateSocketFromTilemap()
        {
            Tilemap tilemap = GetComponentInChildren<Tilemap>();
            if (!tilemap)
            {
                Debug.Log("No tilemap.");
                return;
            }

            if (!tilemap.ContainsTile(socketTile))
            {
                Debug.Log("Socket tile not present.");
                sockets = Array.Empty<SocketShape>();
                return;
            }
            tilemap.CompressBounds();
            BoundsInt bounds = tilemap.cellBounds;
            Debug.Log(bounds);
            List<BoundsInt> allFoundBounds = new();
            for (int x = bounds.xMin; x <= bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y <= bounds.yMax; y++)
                {
                    // yes, suboptimal
                    // if you don't like it, build your own KD tree then
                    if (allFoundBounds.Any(bound => bound.Contains(new Vector3Int(x, y)))) continue;
                    if (tilemap.GetTile(new Vector3Int(x, y)) != socketTile) continue;
                    Vector2Int size = SocketSize(tilemap, bounds, new Vector2Int(x, y));
                    allFoundBounds.Add(new BoundsInt(x,y,0,size.x,size.y,1));
                    Debug.Log("found: " + new BoundsInt(x,y,0,size.x,size.y,1));
                }
            }

            sockets = new SocketShape[allFoundBounds.Count];
            Vector3 cellSize = tilemap.cellSize;
            for (int i = 0; i < allFoundBounds.Count; i++)
            {
                BoundsInt bound = allFoundBounds[i];
                Vector2 bottomLeft = tilemap.CellToWorld(bound.position);
                Vector2 size = new(cellSize.x * bound.size.x, cellSize.y * bound.size.y);
                Vector2 center = bottomLeft + size/2;
                sockets[i] = new SocketShape
                {
                    position = center,
                    size = size
                };
            }
        }

        public List<(SocketBehaviour, EnemySpawnType)> GenerateSockets(Random random, Dictionary<Vector2, List<GameObject>> socketPrefabSizes)
        {
            List<(SocketBehaviour, EnemySpawnType)> socketBehaviours = new();
            if (sockets == null)
            {
                Debug.LogWarning("[RoomData::GenerateSockets] No sockets for this object. Skipping.");
                return socketBehaviours;
            }
            foreach (SocketShape shape in sockets)
            {
                if (socketPrefabSizes.TryGetValue(shape.size, out List<GameObject> possibleSocketPrefabs) && possibleSocketPrefabs.Count > 0)
                {
                    GameObject createdSocket = Instantiate(possibleSocketPrefabs.GetRandom(random), transform);
                    createdSocket.transform.localPosition = shape.position;
                    SocketBehaviour behaviour = createdSocket.GetComponent<SocketBehaviour>();
                    EnemySpawnType type = behaviour.AllowedSpawnTypes;
                    if (behaviour && type != 0) socketBehaviours.Add((behaviour, behaviour.AllowedSpawnTypes));
                }
                else
                {
                    Debug.LogWarning($"[RoomData::GenerateSockets] No game object entry for socket of size {shape.size}; skipping");
                }
            }

            return socketBehaviours;
        }

        public GameObject SpawnEnemy(EnemySpawnType type, Random random)
        {
            if (_spawnPoints == null) InitializePointsAndMappings();
            Debug.Assert(type.GetParts().Count == 1, "Room Data Spawn Enemy requires singular type");
            List<EnemySpawnPoint> points = _spawnTypeMapping[type];
            if (points.Count == 0) return null;
            EnemySpawnPoint point = points.GetRandom(random);
            bool didSpawn = point.SpawnEnemy(type, out GameObject go);
            Debug.Assert(didSpawn);
            EntityHealth health = go.GetComponent<EntityHealth>();
            EnemyBehaviour behaviour = go.GetComponent<EnemyBehaviour>();
            _enemyBehaviours.Add(health, behaviour);
            Debug.Assert(health, "Added enemy should have EntityHealth attached");
            AddEnemy(health);
            return go;
        }

        public void AddEnemy(EntityHealth health)
        {
            _enemies.Add(health);
            health.OnDeath += OnEnemyDeath;
        }

        public void OnEnemyDeath(EntityHealth health)
        {
            health.OnDeath -= OnEnemyDeath;
            _enemies.Remove(health);
            _enemyBehaviours.Remove(health);
            if (_enemies.Count != 0) return;
            if (pickup) pickup.gameObject.SetActive(true);
            if (cheesePickup) cheesePickup.gameObject.SetActive(true);
            if (weaponPickup) weaponPickup.gameObject.SetActive(true);
            if (roomTransitionInteractable) roomTransitionInteractable.gameObject.SetActive(true);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position + (Vector3) roomCenterOffset, (Vector3) roomSize + Vector3.forward);
        }
    }
}