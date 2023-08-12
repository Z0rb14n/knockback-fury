﻿using System.Collections.Generic;
using FileSave;
using UnityEngine;
using Upgrades;
using Weapons;

namespace FloorGen
{
    [DisallowMultipleComponent]
    public class RoomEnemyManager : MonoBehaviour
    {
        public UpgradePickup pickup;
        public CheesePickup cheesePickup;
        public WeaponPickup weaponPickup;

        private readonly HashSet<EntityHealth> _enemies = new();

        public List<EntityHealth> Enemies => new(_enemies);

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
    }
}