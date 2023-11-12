using System;
using System.Collections;
using DashVFX;
using Enemies.Ranged;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies.Cat
{
    [RequireComponent(typeof(EntityHealth), typeof(SpriteRenderer), typeof(Rigidbody2D)),
    RequireComponent(typeof(MeshTrail))]
    public class CatBossPhaseTwo : MonoBehaviour
    {
        public AttackSetting[] attackSettings;
        private AttackSetting CurrSetting => attackSettings[_phase + 1];

        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite leftSprite;
        [SerializeField] private Sprite rightSprite;
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
        private MeshTrail _meshTrail;
        private bool Grounded => _collider.IsTouchingLayers(_groundLayer);

        private void Awake()
        {
            _player = PlayerMovementScript.Instance;
            _collider = GetComponent<Collider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _meshTrail = GetComponent<MeshTrail>();
            _groundLayer = LayerMask.GetMask("Default", "IgnorePlayer");
        }

        public void FixedUpdate()
        {
            if (Grounded) LookAtPlayer();
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
                bool doDash = CurrSetting.canDash && Random.Range(0.0f, 1.0f) < CurrSetting.dashFreq;
                if (doDash)
                {
                    _spriteRenderer.sprite = normalSprite;
                    Vector2 pos = _rigidbody.position;
                    _rigidbody.velocity = EnemyBombScript.CalculateVelocity(pos, new Vector2(pos.x, _player.transform.position.y),
                        new Vector2(0, _player.Velocity.y), CurrSetting.maxVel, CurrSetting.timeBeforeDash);
                    yield return new WaitForSeconds(CurrSetting.timeBeforeDash);
                    _rigidbody.gravityScale = 0;
                    
                    // ReSharper disable once Unity.InefficientPropertyAccess
                    Vector2 playerPos = _player.transform.position;
                    pos = _rigidbody.position;
                    _rigidbody.velocity = (playerPos - pos).normalized * CurrSetting.dashVel;
                    LookAtPlayer();
                    _meshTrail.playerSprite = _spriteRenderer.sprite;
                    _meshTrail.StartDash(false);
                    yield return new WaitForSeconds(0.5f);
                    _rigidbody.gravityScale = 1; 
                    _meshTrail.StopDash();
                }
                else
                {
                    Vector2 playerPos = _player.transform.position;
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
                    
                    if (!float.IsNaN(force.x)) _rigidbody.velocity = force;
                }
            }
        }

        private void LookAtPlayer()
        {
            Vector3 playerPos = _player.transform.position;
            Vector2 pos = _rigidbody.position;
            if (playerPos.x < pos.x) _spriteRenderer.sprite = leftSprite;
            else if (playerPos.x > pos.x) _spriteRenderer.sprite = rightSprite;
        }


        public void OnTriggerReach(int phase)
        {
            _phase = phase;
            UpdateSize();
        }

        private void UpdateSize()
        {
            transform.localScale = new Vector3(CurrSetting.size, CurrSetting.size, 1);
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
    }
}