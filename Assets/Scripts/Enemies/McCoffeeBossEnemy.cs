using System;
using UnityEngine;
using UnityEngine.Events;

namespace Enemies
{
    [RequireComponent(typeof(EntityHealth))]
    public class McCoffeeBossEnemy : AbstractBossEnemy
    {
        private EntityHealth _entityHealth;

        public UnityEvent eventOnDeath;

        private void Awake()
        {
            _entityHealth = GetComponent<EntityHealth>();
            _entityHealth.OnDeath += OnDeath;
        }

        private void OnDeath(EntityHealth health)
        {
            eventOnDeath?.Invoke();
        }

        public override void StartBoss()
        {
        }
    }
}