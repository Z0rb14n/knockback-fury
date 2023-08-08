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
        [Tooltip("Types of enemies that can spawn here.")]
        public EnemySpawnType types;

        /// <summary>
        /// Spawn an enemy of a given type.
        /// </summary>
        /// <param name="type">Type of enemy to spawn - can't be a flag.</param>
        /// <returns>True if an enemy was created.</returns>
        public bool SpawnEnemy(EnemySpawnType type)
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
                    return false;
            }
        }

        /// <summary>
        /// Spawn a jumper enemy.
        /// </summary>
        /// <returns>True if an enemy was created.</returns>
        public bool SpawnJumper()
        {
            if ((types & EnemySpawnType.Jumper) == 0) return false;
            Instantiate(jumperPrefab, transform);
            return true;
        }

        /// <summary>
        /// Spawn a heavy enemy.
        /// </summary>
        /// <returns>True if an enemy was created.</returns>
        public bool SpawnHeavy()
        {
            if ((types & EnemySpawnType.Heavy) == 0) return false;
            Instantiate(heavyPrefab, transform);
            return true;
        }

        /// <summary>
        /// Spawn a ranged enemy.
        /// </summary>
        /// <returns>True if an enemy was created.</returns>
        public bool SpawnRanged()
        {
            if ((types & EnemySpawnType.Heavy) == 0) return false;
            Instantiate(rangedPrefab, transform);
            return true;
        }

        /// <summary>
        /// Spawn a chaser enemy.
        /// </summary>
        /// <returns>True if an enemy was created.</returns>
        public bool SpawnChaser()
        {
            if ((types & EnemySpawnType.Chaser) == 0) return false;
            Instantiate(chaserPrefab, transform);
            return true;
        }
    }
}