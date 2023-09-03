using System.Collections;
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
        
        
        public delegate void TargetAnalysisUpdateHandler();

        public event TargetAnalysisUpdateHandler OnTargetAnalysisUpdate;

        [SerializeField] private int sneakyJumperCooldown = 3;
        [SerializeField] private int sneakyJumperInvulnTime = 1;

        public int TargetAnalysisDamage { get; private set; }

        public bool IsTargetAnalysisShieldActive { get; private set; }

        private int _currSneakyJumpCooldown;

        protected override void Awake()
        {
            base.Awake();
            _instance = this;
            _upgradeManager = GetComponent<PlayerUpgradeManager>();
        }

        /// <summary>
        /// Takes Damage: decreases health, sets iFrame timer, restricts player movement
        /// </summary>
        protected override void DoTakeDamage(int dmg)
        {
            if (!IsTargetAnalysisShieldActive)
            {
                health -= dmg;
                _iFrameTimer = iFrameLength;
            }
            else
            {
                IsTargetAnalysisShieldActive = false;
                OnTargetAnalysisUpdate?.Invoke();
            }
            StartCoroutine(DisableCollision());
            
        }

        protected override void Die()
        {
            Debug.Log("Player death");
            // TODO: player death
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
                _iFrameTimer = sneakyJumperInvulnTime;
                _currSneakyJumpCooldown = sneakyJumperCooldown;
            }
        }
    }
}
