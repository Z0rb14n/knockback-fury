using System.Collections.Generic;
using FileSave;
using UnityEngine;
using Upgrades;

namespace FloorGen
{
    [DisallowMultipleComponent]
    public class RoomEnemyManager : MonoBehaviour
    {
        public UpgradePickup pickup;
        public CheesePickup cheesePickup;

        private readonly HashSet<EntityHealth> _enemies = new();

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
        }
    }
}