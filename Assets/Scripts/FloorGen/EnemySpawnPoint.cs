using System;
using Enemies;
using UnityEngine;

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

        [SerializeField, Tooltip("Spawn Points")]
        private Transform[] patrolPoints;
        [Tooltip("Types of enemies that can spawn here.")]
        public EnemySpawnType types;

        /// <summary>
        /// Spawn an enemy of a given type.
        /// </summary>
        /// <param name="type">Type of enemy to spawn - can't be a flag.</param>
        /// <param name="go">Spawned GameObject</param>
        /// <returns>True if an enemy was created.</returns>
        public bool SpawnEnemy(EnemySpawnType type, out GameObject go)
        {
            switch (type)
            {
                case EnemySpawnType.Jumper:
                    return SpawnJumper(out go);
                case EnemySpawnType.Heavy:
                    return SpawnHeavy(out go);
                case EnemySpawnType.Ranged:
                    return SpawnRanged(out go);
                case EnemySpawnType.Chaser:
                    return SpawnChaser(out go);
                default:
                    Debug.LogError("[EnemySpawnPoint::SpawnEnemy] called with invalid type: " + type);
                    go = null;
                    return false;
            }
        }

        /// <summary>
        /// Spawn a jumper enemy.
        /// </summary>
        /// <param name="go">Spawned GameObject</param>
        /// <returns>True if an enemy was created.</returns>
        public bool SpawnJumper(out GameObject go)
        {
            go = null;
            if ((types & EnemySpawnType.Jumper) == 0) return false;
            go = Instantiate(jumperPrefab, transform);
            // TODO FIX FROG UNCENTERED
            go.transform.position = transform.position + new Vector3(0,0.75f,0);
            go.GetComponent<PatrolMovement>().patrolPoints = patrolPoints ?? Array.Empty<Transform>();
            return true;
        }

        /// <summary>
        /// Spawn a heavy enemy.
        /// </summary>
        /// <param name="go">Spawned GameObject</param>
        /// <returns>True if an enemy was created.</returns>
        public bool SpawnHeavy(out GameObject go)
        {
            go = null;
            if ((types & EnemySpawnType.Heavy) == 0) return false;
            go = Instantiate(heavyPrefab, transform);
            go.transform.position = transform.position;
            go.GetComponent<PatrolMovement>().patrolPoints = patrolPoints ?? Array.Empty<Transform>();
            return true;
        }

        /// <summary>
        /// Spawn a ranged enemy.
        /// </summary>
        /// <param name="go">Spawned GameObject</param>
        /// <returns>True if an enemy was created.</returns>
        public bool SpawnRanged(out GameObject go)
        {
            go = null;
            if ((types & EnemySpawnType.Ranged) == 0) return false;
            go = Instantiate(rangedPrefab, transform);
            go.transform.position = transform.position;
            return true;
        }

        /// <summary>
        /// Spawn a chaser enemy.
        /// </summary>
        /// <param name="go">Spawned GameObject</param>
        /// <returns>True if an enemy was created.</returns>
        public bool SpawnChaser(out GameObject go)
        {
            go = null;
            if ((types & EnemySpawnType.Chaser) == 0) return false;
            go = Instantiate(chaserPrefab, transform);
            go.transform.position = transform.position;
            return true;
        }
    }
}