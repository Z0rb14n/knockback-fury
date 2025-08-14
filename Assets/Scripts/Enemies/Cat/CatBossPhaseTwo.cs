using System;
using System.Collections;
using DashVFX;
using Enemies.Ranged;
using Player;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Enemies.Cat
{
    [RequireComponent(typeof(EntityHealth), typeof(SpriteRenderer), typeof(Rigidbody2D)),
    RequireComponent(typeof(AbstractDashTrail), typeof(AudioSource))]
    public class CatBossPhaseTwo : MonoBehaviour
    {
        public AttackSetting[] attackSettings;
        private AttackSetting CurrSetting => attackSettings[_phase + 1];

        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite leftSprite;
        [SerializeField] private Sprite rightSprite;
        [SerializeField] private CatInvulnMachine[] invulnDevices;
        [SerializeField] private CatTurret[] turrets;
        [SerializeField] private float timeBetweenTurrets = 0.2f;
        [SerializeField] private float timeAfterTurrets = 10;
        [SerializeField] private GameObject invulnShield;

        [SerializeField] private GameObject batGroup;
        [SerializeField] private Transform batTransform;

        [SerializeField] private float batStartRotation = -5;
        [SerializeField] private float batEndRotation = -30;
        [SerializeField] private int batAttackDamage;
        [SerializeField] private float batKnockback = 50;
        [SerializeField] private Vector2 batJumpVector = new(10, 10);
        [SerializeField, Min(0)] private float batDownLength = 1;
        [SerializeField, Min(0)] private float batDelay = 1.5f;
        [SerializeField] private GameObject catDebris;
        // -1: normal
        // 0: can dash, slightly faster
        // 1: can dash, even faster, activates invuln
        // 2: hides, activates invuln, does thing
        private int _phase = -1;
        private PlayerMovementScript _player;
        private Collider2D _collider;
        private Rigidbody2D _rigidbody;
        private LayerMask _groundLayer;
        private SpriteRenderer _spriteRenderer;
        private Collider2D _floorCollider;
        private AudioSource _audioSource;
        private AbstractDashTrail _dashTrail;
        private int _invulnDevices;
        private BossHealthBar _bossHealthBar;
        private CatEntityHealth _catEntityHealth;
        private IEnumerator _batCoroutine;
        private PlayerHealth _playerHealth;
        private bool _canAttackWithBat = true;
        private bool _turretPhaseOver;
        private bool Grounded => _collider.IsTouchingLayers(_groundLayer);

        private void Awake()
        {
            _player = PlayerMovementScript.Instance;
            _playerHealth = PlayerHealth.Instance;
            _collider = GetComponent<Collider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _dashTrail = GetComponent<AbstractDashTrail>();
            _audioSource = GetComponent<AudioSource>();
            _bossHealthBar = FindAnyObjectByType<BossHealthBar>(FindObjectsInactive.Include);
            _catEntityHealth = GetComponent<CatEntityHealth>();
            _groundLayer = LayerMask.GetMask("Default", "IgnorePlayer");
        }

        public void FixedUpdate()
        {
            if (_phase == 2 && _turretPhaseOver)
            {
                Vector3 playerPos = _player.Pos;
                Vector3 pos = transform.position;
                bool isLeft = playerPos.x < pos.x;
                bool isIn = _collider.bounds.Contains(playerPos);
                transform.localScale = new Vector3(isLeft ? -CurrSetting.size : CurrSetting.size, CurrSetting.size, 1);
                if (Grounded && !isIn)
                {
                    _rigidbody.linearVelocity = batJumpVector * new Vector2(isLeft ? -1 : 1, 1);
                }

                _spriteRenderer.sprite = rightSprite;
            } else if (Grounded) LookAtPlayer();
        }

        public void Activate()
        {
            StartCoroutine(AttackCoroutine());
        }

        private IEnumerator AttackCoroutine()
        {
            while (_phase < 2)
            {
                yield return new WaitForSeconds(CurrSetting.attackDelay);
                if (!Grounded) continue;
                _dashTrail.StopDash();
                bool doDash = CurrSetting.canDash && Random.Range(0.0f, 1.0f) < CurrSetting.dashFreq;
                if (doDash)
                {
                    _spriteRenderer.sprite = normalSprite;
                    Vector2 pos = _rigidbody.position;
                    _rigidbody.linearVelocity = EnemyBombScript.CalculateVelocity(pos, new Vector2(pos.x, _player.Pos.y),
                        new Vector2(0, _player.Velocity.y), CurrSetting.maxVel, CurrSetting.timeBeforeDash);
                    yield return new WaitForSeconds(CurrSetting.timeBeforeDash);
                    _rigidbody.gravityScale = 0;
                    
                    // ReSharper disable once Unity.InefficientPropertyAccess
                    Vector2 playerPos = _player.Pos;
                    pos = _rigidbody.position;
                    _rigidbody.linearVelocity = (playerPos - pos).normalized * CurrSetting.dashVel;
                    LookAtPlayer();
                    _dashTrail.StartDash(false);
                    yield return new WaitForSeconds(0.5f);
                    _rigidbody.gravityScale = 1; 
                }
                else
                {
                    Vector2 playerPos = _player.Pos;
                    Vector2 pos = _rigidbody.position;
                    Vector2 force;
                    if (CurrSetting.predictVel)
                    {
                        force = EnemyBombScript.CalculateVelocity(pos, playerPos, _player.Velocity, CurrSetting.maxVel,
                            CurrSetting.maxTime);
                    }
                    else
                    {
                        force = EnemyBombScript.CalculateVelocity(pos, playerPos, CurrSetting.maxVel, CurrSetting.maxTime);
                    }
                    
                    if (!float.IsNaN(force.x)) _rigidbody.linearVelocity = force;
                }
            }
            _dashTrail.StopDash();
        }

        private void LookAtPlayer()
        {
            Vector3 playerPos = _player.Pos;
            Vector2 pos = _rigidbody.position;
            if (playerPos.x < pos.x) _spriteRenderer.sprite = leftSprite;
            else if (playerPos.x > pos.x) _spriteRenderer.sprite = rightSprite;
        }


        public void OnTriggerReach(int phase)
        {
            _phase = phase;
            UpdateSize();
            if (phase == 1 || phase == 2)
            {
                _catEntityHealth.Invuln = true;
                _bossHealthBar.BarColor = Color.yellow;
                invulnShield.SetActive(true);
            }
            if (phase == 1)
            {
                _invulnDevices = invulnDevices.Length;
                foreach (CatInvulnMachine go in invulnDevices) go.gameObject.SetActive(true);
            }

            if (phase == 2)
            {
                batGroup.SetActive(true);
                foreach (CatTurret turret in turrets) turret.Activate();
                StartCoroutine(TurretPhaseCoroutine());
            }
        }

        private IEnumerator TurretPhaseCoroutine()
        {
            turrets.Shuffle();
            foreach (CatTurret turret in turrets)
            {
                turret.Fire();
                yield return new WaitForSeconds(timeBetweenTurrets);
            }

            yield return new WaitForSeconds(timeAfterTurrets);
            _catEntityHealth.Invuln = false;
            _bossHealthBar.BarColor = Color.red;
            invulnShield.SetActive(false);
            _turretPhaseOver = true;
        }

        public void OnDeath()
        {
            Destroy(gameObject);
            GameObject go = Instantiate(catDebris, transform.position, Quaternion.identity);
            go.transform.localScale = transform.localScale;
        }

        private void UpdateSize()
        {
            transform.localScale = new Vector3(CurrSetting.size, CurrSetting.size, 1);
        }
        public void OnInvulnDeath()
        {
            _invulnDevices--;
            if (_invulnDevices <= 0)
            {
                _bossHealthBar.BarColor = Color.red;
                _catEntityHealth.Invuln = false;
                invulnShield.SetActive(false);
            }
        }

        [Serializable]
        public struct AttackSetting
        {
            public float attackDelay;
            public float maxVel;
            public float maxTime;
            public bool predictVel;
            public bool canDash;
            [Range(0,1)]
            public float dashFreq;
            public float dashVel;
            public float timeBeforeDash;
            public float size;
        }
        
        
        // TODO REFACTOR - DUPLICATE OF GOON
        private IEnumerator BatCoroutine()
        {
            while (_batCoroutine != null)
            {
                yield return new WaitUntil(() => _canAttackWithBat);
                StartCoroutine(AttackAnimation());
            }
        }

        private IEnumerator AttackAnimation()
        {
            _canAttackWithBat = false;
            batTransform.localEulerAngles = new Vector3(0, 0, batEndRotation);
            _playerHealth.TakeDamage(batAttackDamage);
            _audioSource.Play();
            Vector2 knockbackDirection = new((_player.Pos - (Vector2)_collider.bounds.center).normalized.x * 0.1f, 0.04f);
            _player.RequestKnockback(knockbackDirection, batKnockback);
            yield return new WaitForSeconds(batDownLength);
            // ReSharper disable once Unity.InefficientPropertyAccess
            batTransform.localEulerAngles = new Vector3(0,0,batStartRotation);
            yield return new WaitForSeconds(batDelay - batDownLength);
            _canAttackWithBat = true;
        }

        public void StartAttackingPlayerWithBat()
        {
            if (!_turretPhaseOver) return;
            _batCoroutine = BatCoroutine();
            StartCoroutine(_batCoroutine);
        }

        public void StopAttackingPlayerWithBat()
        {
            StopCoroutine(_batCoroutine);
            _batCoroutine = null;
            batTransform.localEulerAngles = Vector3.zero;
        }
    }
}