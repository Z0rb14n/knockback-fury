using System.Linq;
using UnityEngine;

namespace FloorGen
{
    [DisallowMultipleComponent]
    public class SocketBehaviour : MonoBehaviour
    {
        private EnemySpawnPoint[] _spawnPoints;

        private void Awake()
        {
            _spawnPoints = GetComponentsInChildren<EnemySpawnPoint>();
        }

        public EnemySpawnType AllowedSpawnTypes => _spawnPoints.Aggregate<EnemySpawnPoint, EnemySpawnType>(0, (current, point) => current | point.types);
        
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
                    Debug.LogError("[SocketBehaviour::SpawnEnemy called with invalid type: " + type);
                    return false;
            }
        }

        public bool SpawnJumper() => _spawnPoints.Where(point => (point.types & EnemySpawnType.Jumper) != 0).Any(point => point.SpawnJumper());

        public bool SpawnHeavy() => _spawnPoints.Where(point => (point.types & EnemySpawnType.Heavy) != 0).Any(point => point.SpawnHeavy());

        public bool SpawnRanged() => _spawnPoints.Where(point => (point.types & EnemySpawnType.Ranged) != 0).Any(point => point.SpawnRanged());

        public bool SpawnChaser() => _spawnPoints.Where(point => (point.types & EnemySpawnType.Chaser) != 0).Any(point => point.SpawnChaser());
    }
}