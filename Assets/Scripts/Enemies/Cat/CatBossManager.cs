using System;
using System.Collections;
using Player;
using UnityEngine;

namespace Enemies.Cat
{
    /// <summary>
    /// Handles secret events and stuff.
    /// </summary>
    public class CatBossManager : MonoBehaviour
    {
        [SerializeField] private EntityHealth[] secretTriggers;
        [SerializeField] private BossEnemy originalBossEnemy;
        [SerializeField] private CatBoss catBoss;

        [SerializeField, Min(0)] private float delayBeforeSpawn = 5;
        [SerializeField] private Collider2D roomCeiling;

        private int _triggersRemaining;
        private Collider2D _originalBossCollider;
        private Collider2D _catCollider;
        private EntityHealth _catHealth;
        private BossHealthBar _bossHealthBar;

        private void Awake()
        {
            catBoss.CatManager = this;
            _bossHealthBar = FindObjectOfType<BossHealthBar>(true);
            _triggersRemaining = secretTriggers.Length;
            _originalBossCollider = originalBossEnemy.GetComponent<Collider2D>();
            _catHealth = catBoss.GetComponent<EntityHealth>();
            _catCollider = catBoss.GetComponent<Collider2D>();
            foreach (EntityHealth health in secretTriggers)
            {
                health.OnDeath += OnDeath;
            }
        }

        private void OnDeath(EntityHealth obj)
        {
            _triggersRemaining--;
            if (_triggersRemaining == 0)
            {
                StartCoroutine(StartSpawnSoon());
            }
        }

        private IEnumerator StartSpawnSoon()
        {
            _originalBossCollider.enabled = false;
            catBoss.PrepareToSpawn(originalBossEnemy);
            Physics2D.IgnoreCollision(_catCollider, roomCeiling);
            CameraScript.Instance.CameraShakeStrength = 1;
            originalBossEnemy.StopBeingActive();
            yield return new WaitForSeconds(delayBeforeSpawn);
            CameraScript.Instance.CameraShakeStrength = 0;
            catBoss.StartSpawn();
            _bossHealthBar.health = _catHealth;
        }
    }
}