using System.Linq;
using UnityEngine;

namespace FloorGen
{
    [DisallowMultipleComponent]
    public class SocketBehaviour : MonoBehaviour
    {
        public Vector2 size;
        private EnemySpawnPoint[] _spawnPoints;

        private void Awake()
        {
            _spawnPoints = GetComponentsInChildren<EnemySpawnPoint>();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(Vector3.zero, size);
        }

        public EnemySpawnType AllowedSpawnTypes => _spawnPoints.Aggregate<EnemySpawnPoint, EnemySpawnType>(0, (current, point) => current | point.types);
        
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
                    Debug.LogError("[SocketBehaviour::SpawnEnemy called with invalid type: " + type);
                    return null;
            }
        }

        private GameObject SpawnJumper()
        {
            return _spawnPoints.Where(point => point.CanSpawnType(EnemySpawnType.Jumper))
                .Select(point => point.SpawnJumper()).FirstOrDefault(go => go);
        }

        private GameObject SpawnHeavy()
        {
            return _spawnPoints.Where(point => point.CanSpawnType(EnemySpawnType.Heavy))
                .Select(point => point.SpawnHeavy()).FirstOrDefault(go => go);
        }

        private GameObject SpawnRanged()
        {
            return _spawnPoints.Where(point => point.CanSpawnType(EnemySpawnType.Ranged))
                .Select(point => point.SpawnRanged()).FirstOrDefault(go => go);
        }

        private GameObject SpawnChaser()
        {
            return _spawnPoints.Where(point => point.CanSpawnType(EnemySpawnType.Chaser))
                .Select(point => point.SpawnChaser()).FirstOrDefault(go => go);
        }
    }
}