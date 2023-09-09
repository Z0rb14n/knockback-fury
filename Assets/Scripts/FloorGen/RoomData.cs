using System;
using System.Collections.Generic;
using System.Linq;
using FileSave;
using UnityEngine;
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

        [Tooltip("Spawn offsets of weapon drops")]
        public Vector2 weaponSpawnOffset = Vector2.up;
        [Tooltip("Spawn offset of cheese drops")]
        public Vector2 cheeseSpawnOffset = Vector2.up + Vector2.left;
        [Tooltip("Spawn offset of player powerups")]
        public Vector2 powerupSpawnOffset = Vector2.up;
        [Tooltip("Spawn offset of weapon upgrades")]
        public Vector2 weaponUpgradeSpawnOffset = Vector2.up;
        [Tooltip("Player spawn offset")]
        public Vector2 playerSpawnOffset = Vector2.zero;

        public ExitHider[] hiders;
        
        public Layout[] layouts;

        public bool ignoreSocketEnemySpawns = true;
        
        
        public int ToPreview { get; set; } = -1;

        private readonly HashSet<EntityHealth> _enemies = new();

        public List<EntityHealth> Enemies => new(_enemies);

        private EnemySpawnPoint[] _spawnPoints;
        private Dictionary<EnemySpawnType, List<EnemySpawnPoint>> _spawnTypeMapping;

        private void Awake()
        {
            InitializePointsAndMappings();
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

        public void EnsureType(RoomType type)
        {
            foreach (ExitHider hider in hiders)
            {
                if ((type & hider.hidingType) == 0) hider.toEnable.SetActive(true);
            }
        }

        public List<(SocketBehaviour, EnemySpawnType)> GenerateSockets(Random random, Dictionary<Vector2, List<GameObject>> socketPrefabSizes)
        {
            List<(SocketBehaviour, EnemySpawnType)> socketBehaviours = new();
            if (layouts == null || layouts.Length == 0)
            {
                Debug.LogWarning("[RoomData::GenerateSockets] No layouts for this object. Skipping.");
                return socketBehaviours;
            }
            Layout layout = layouts.GetRandom(random);
            foreach (SocketShape shape in layout.sockets)
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
            if (_enemies.Count != 0) return;
            if (pickup) pickup.gameObject.SetActive(true);
            if (cheesePickup) cheesePickup.gameObject.SetActive(true);
            if (weaponPickup) weaponPickup.gameObject.SetActive(true);
        }

        [Serializable]
        public struct ExitHider
        {
            public RoomType hidingType;
            public GameObject toEnable;
        }
    }
}