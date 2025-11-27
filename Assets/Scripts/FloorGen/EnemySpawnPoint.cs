using System;
using System.Collections.Generic;
using Enemies;
using UnityEngine;
using Util;

namespace FloorGen
{
    /// <summary>
    /// Logic to spawn an enemy at this position on floor generation.
    /// </summary>
    public class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField, Tooltip("Jumper enemy prefab")]
        private GameObject jumperPrefab;

        [SerializeField, Tooltip("Heavy enemy prefab")]
        private GameObject heavyPrefab;

        [SerializeField, Tooltip("Ranged enemy prefab")]
        private GameObject rangedPrefab;

        [SerializeField, Tooltip("Chaser enemy prefab")]
        private GameObject chaserPrefab;

        [SerializeField, Tooltip("Patrol Points")]
        private Transform[] patrolPoints;

        [Tooltip("Types of enemies that can spawn here.")]
        public EnemySpawnType types;
        [SerializeField, Range(0,10), Tooltip("Number of enemies spawned on awake")]
        private int enemiesSpawnedOnAwake;

        private readonly HashSet<EntityHealth> _enemies = new();

        private void Awake()
        {
            for (int i = 0; i < enemiesSpawnedOnAwake; i++)
                SpawnRandomEnemy();
        }

        private void Start()
        {
            if (_enemies.Count == 0) Destroy(gameObject);
        }

        /// <summary>
        /// Called on enemy death - deletes this object.
        /// </summary>
        /// <param name="health"></param>
        private void OnEntityDeath(EntityHealth health)
        {
            health.OnDeath -= OnEntityDeath;
            _enemies.Remove(health);
            if (_enemies.Count == 0) Destroy(gameObject);
        }

        /// <summary>
        /// Spawns a random enemy.
        /// </summary>
        /// <returns>Spawned GameObject</returns>
        public GameObject SpawnRandomEnemy()
        {
            EnemySpawnType[] allowedTypes = types.GetParts();
            return SpawnEnemy(allowedTypes.GetRandom());
        }

        /// <summary>
        /// Spawn an enemy of a given type.
        /// </summary>
        /// <param name="type">Type of enemy to spawn - can't be a flag.</param>
        /// <returns>Spawned GameObject</returns>
        public GameObject SpawnEnemy(EnemySpawnType type)
        {
            switch (type)
            {
                case EnemySpawnType.Jumper:
                    return SpawnJumper();
                case EnemySpawnType.Heavy:
                    return SpawnHeavy();
                case EnemySpawnType.Ranged:
                    return SpawnRanged();
                case EnemySpawnType.Chaser:
                    return SpawnChaser();
                default:
                    Debug.LogError("[EnemySpawnPoint::SpawnEnemy] called with invalid type: " + type);
                    return null;
            }
        }

        /// <summary>
        /// Spawns an enemy prefab at a location and provides the patrol points.
        /// </summary>
        /// <param name="prefab">Enemy prefab</param>
        /// <param name="hasPatrolPoints">Whether the enemy has patrol points</param>
        /// <returns>Returned object</returns>
        private GameObject SpawnEntityPrefab(GameObject prefab, bool hasPatrolPoints) =>
            SpawnEntityPrefab(prefab, Vector3.zero, hasPatrolPoints);

        /// <summary>
        /// Spawns an enemy prefab at a location + offset and provides the patrol points.
        /// </summary>
        /// <param name="prefab">Enemy prefab</param>
        /// <param name="offset">Location offset</param>
        /// <param name="hasPatrolPoints">Whether the enemy has patrol points</param>
        /// <returns>Returned object</returns>
        private GameObject SpawnEntityPrefab(GameObject prefab, Vector3 offset, bool hasPatrolPoints)
        {
            GameObject go = Instantiate(prefab, transform);
            // TODO FIX FROG UNCENTERED
            go.transform.position = transform.position + offset;
            if (hasPatrolPoints)
                go.GetComponent<PatrolMovement>().patrolPoints = patrolPoints ?? Array.Empty<Transform>();
            EntityHealth health = go.GetComponent<EntityHealth>();
            _enemies.Add(health);
            health.OnDeath += OnEntityDeath;
            return go;
        }

        /// <summary>
        /// Spawn a jumper enemy.
        /// </summary>
        /// <returns>Created GameObject if present</returns>
        public GameObject SpawnJumper()
        {
            return CanSpawnType(EnemySpawnType.Jumper)
                ? SpawnEntityPrefab(jumperPrefab, new Vector3(0, 0.75f), true)
                : null;
        }

        /// <summary>
        /// Spawn a heavy enemy.
        /// </summary>
        /// <returns>Created GameObject if present</returns>
        public GameObject SpawnHeavy()
        {
            return CanSpawnType(EnemySpawnType.Heavy) ? SpawnEntityPrefab(heavyPrefab, true) : null;
        }

        /// <summary>
        /// Spawn a ranged enemy.
        /// </summary>
        /// <returns>Created GameObject if present</returns>
        public GameObject SpawnRanged()
        {
            return CanSpawnType(EnemySpawnType.Ranged) ? SpawnEntityPrefab(rangedPrefab, false) : null;
        }

        /// <summary>
        /// Spawn a chaser enemy.
        /// </summary>
        /// <returns>Created GameObject if present</returns>
        public GameObject SpawnChaser()
        {
            return CanSpawnType(EnemySpawnType.Chaser) ? SpawnEntityPrefab(chaserPrefab, false) : null;
        }

        /// <summary>
        /// Similar to <see cref="Enum.HasFlag"/> but avoids boxing - determines if we can spawn this type.
        /// </summary>
        /// <param name="type">EnemySpawnType type</param>
        /// <returns>Whether we have this enemy spawn type</returns>
        public bool CanSpawnType(EnemySpawnType type)
        {
            return (types & type) != 0;
        }
    }
}
