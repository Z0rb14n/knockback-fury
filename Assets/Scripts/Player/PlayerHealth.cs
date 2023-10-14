using System;
using System.Collections;
using GameEnd;
using UnityEngine;
using Upgrades;

namespace Player
{
    [DisallowMultipleComponent, RequireComponent(typeof(PlayerMovementScript), typeof(PlayerUpgradeManager))]
    public class PlayerHealth : EntityHealth
    {
        public static PlayerHealth Instance
        {
            get
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                if (_instance == null) _instance = FindObjectOfType<PlayerHealth>();
                return _instance;
            }
        }
        private static PlayerHealth _instance;
        private PlayerUpgradeManager _upgradeManager;

        public event Action OnTargetAnalysisUpdate;

        [SerializeField] private int sneakyJumperCooldown = 3;
        [SerializeField] private int sneakyJumperInvulnTime = 1;

        public int TargetAnalysisDamage { get; private set; }

        public bool IsTargetAnalysisShieldActive { get; private set; }

        private float _currSneakyJumpCooldown;
        private float _currSneakyJumpTime;

        protected override void Awake()
        {
            base.Awake();
            _instance = this;
            _upgradeManager = GetComponent<PlayerUpgradeManager>();
        }

        protected override void Update()
        {
            base.Update();
            _currSneakyJumpCooldown -= Time.deltaTime;
            _currSneakyJumpTime -= Time.deltaTime;
        }

        /// <summary>
        /// Takes Damage: decreases health, sets iFrame timer, restricts player movement
        /// </summary>
        protected override void DoTakeDamage(int dmg)
        {
            if (GameEndCanvas.Instance)
            {
                GameEndCanvas.Instance.endData.hitsTaken++;
            }

            if (_currSneakyJumpTime > 0)
            {
            }
            else if (IsTargetAnalysisShieldActive)
            {
                IsTargetAnalysisShieldActive = false;
                OnTargetAnalysisUpdate?.Invoke();
            }
            else
            {
                if (GameEndCanvas.Instance)
                {
                    GameEndCanvas.Instance.endData.damageTaken += Mathf.Min(health, dmg);
                }
                health -= dmg;
                _iFrameTimer = iFrameLength;
            }
            StartCoroutine(DisableCollision());
            
        }

        protected override void Die()
        {
            Debug.Log("Player death");
            StartCoroutine(OnDeathCoroutine());
        }

        private static IEnumerator OnDeathCoroutine()
        {
            if (GameEndCanvas.Instance)
            {
                PlayerMovementScript.Instance.CanMove = false;
                PlayerWeaponControl.Instance.enabled = false;
                CameraScript.Instance.enabled = false;
                Time.timeScale = 0;
                GameEndCanvas.Instance.DisplayAfterDelay(1, false);
                yield return new WaitForSecondsRealtime(1);
            }
        }

        private IEnumerator DisableCollision()
        {
            int _playerLayerID = LayerMask.NameToLayer("Player");
            int _enemyLayerID = LayerMask.NameToLayer("Enemy");

            Physics2D.IgnoreLayerCollision(_playerLayerID, _enemyLayerID, true);
            for (float i = 0; i < iFrameLength; i += 0.2f)
            {
                _sprite.color = new Color(1, 1, 1, 0.5f);
                yield return new WaitForSeconds(0.1f);
                _sprite.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
            Physics2D.IgnoreLayerCollision(_playerLayerID, _enemyLayerID, false);
        }

        public void OnDamageDealtToOther(int amount)
        {
            if (GameEndCanvas.Instance)
            {
                GameEndCanvas.Instance.endData.damageDealt += amount;
            }
            if (IsTargetAnalysisShieldActive) return;
            if (_upgradeManager[UpgradeType.TargetAnalysis] > 0)
            {
                TargetAnalysisDamage += amount;
                if (TargetAnalysisDamage >= _upgradeManager.GetData(UpgradeType.TargetAnalysis))
                {
                    TargetAnalysisDamage = 0;
                    IsTargetAnalysisShieldActive = true;
                }
                OnTargetAnalysisUpdate?.Invoke();
            }
        }

        public void OnWallLaunch()
        {
            if (_upgradeManager[UpgradeType.SneakyJumper] > 0)
            {
                if (_currSneakyJumpCooldown > 0) return;
                _currSneakyJumpTime = sneakyJumperInvulnTime;
                _currSneakyJumpCooldown = sneakyJumperCooldown;
            }
        }
    }
}
