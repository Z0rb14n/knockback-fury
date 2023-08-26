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
                    Debug.LogError("[SocketBehaviour::SpawnEnemy called with invalid type: " + type);
                    go = null;
                    return false;
            }
        }

        private bool SpawnJumper(out GameObject go)
        {
            foreach (EnemySpawnPoint point in _spawnPoints)
            {
                if ((point.types & EnemySpawnType.Jumper) == 0) continue;
                if (point.SpawnJumper(out go)) return true;
            }

            go = null;
            return false;
        }

        private bool SpawnHeavy(out GameObject go)
        {
            foreach (EnemySpawnPoint point in _spawnPoints)
            {
                if ((point.types & EnemySpawnType.Heavy) == 0) continue;
                if (point.SpawnHeavy(out go)) return true;
            }

            go = null;
            return false;
        }

        private bool SpawnRanged(out GameObject go)
        {
            foreach (EnemySpawnPoint point in _spawnPoints)
            {
                if ((point.types & EnemySpawnType.Ranged) == 0) continue;
                if (point.SpawnRanged(out go)) return true;
            }

            go = null;
            return false;
        }

        private bool SpawnChaser(out GameObject go)
        {
            foreach (EnemySpawnPoint point in _spawnPoints)
            {
                if ((point.types & EnemySpawnType.Chaser) == 0) continue;
                if (point.SpawnChaser(out go)) return true;
            }

            go = null;
            return false;
        }
    }
}