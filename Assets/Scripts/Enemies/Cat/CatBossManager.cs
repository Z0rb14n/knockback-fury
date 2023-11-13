using System.Collections;
using GameEnd;
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
        [SerializeField] private CatBossPhaseOne catBossP1;
        [SerializeField] private CatBossPhaseTwo catBossP2;

        [SerializeField, Min(0)] private float delayBeforeWin = 2;
        [SerializeField, Min(0)] private float delayBeforeSpawn = 5;

        private int _triggersRemaining;
        private bool _isPhaseOne = true;
        private Collider2D _originalBossCollider;
        private EntityHealth _catHealth;
        private EntityHealth _catHealthP2;
        private BossHealthBar _bossHealthBar;

        private void Awake()
        {
            catBossP1.CatManager = this;
            _bossHealthBar = FindObjectOfType<BossHealthBar>(true);
            _triggersRemaining = secretTriggers.Length;
            _originalBossCollider = originalBossEnemy.GetComponent<Collider2D>();
            _catHealth = catBossP1.GetComponent<EntityHealth>();
            _catHealthP2 = catBossP2.GetComponent<EntityHealth>();
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
            catBossP1.PrepareToSpawn(originalBossEnemy);
            CameraScript.Instance.CameraShakeStrength = 1;
            originalBossEnemy.StopBeingActive();
            yield return new WaitForSeconds(delayBeforeSpawn);
            CameraScript.Instance.CameraShakeStrength = 0;
            catBossP1.StartSpawn();
            _bossHealthBar.health = _catHealth;
        }

        public void OnCatDeath()
        {
            if (_isPhaseOne)
            {
                _isPhaseOne = false;
                catBossP1.OnDeath();
            }
            else
            {
                catBossP2.OnDeath();
                if (GameEndCanvas.Instance)
                {
                    GameEndCanvas.Instance.DisplayAfterDelay(delayBeforeWin, true);
                }
            }
        }

        public void EndPhaseOne()
        {
            _bossHealthBar.health = _catHealthP2;
        }

        public void CatHealthTriggerReach(int index)
        {
            catBossP2.OnTriggerReach(index);
        }
    }
}