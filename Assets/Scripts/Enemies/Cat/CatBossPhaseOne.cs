using System;
using System.Collections;
using Player;
using UnityEngine;

namespace Enemies.Cat
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer), typeof(Rigidbody2D))]
    public class CatBossPhaseOne : MonoBehaviour
    {
        [NonSerialized]
        public CatBossManager CatManager;

        [SerializeField] private Vector2 initialVelocity = new(0, -10);
        [SerializeField, Min(0)] private float delayBeforeAttacking = 1.5f;
        [SerializeField, Min(0)] private float delayBetweenJumps = 2.5f;
        [SerializeField, Min(0)] private Vector2 jumpVector = new(10,10);
        [SerializeField, Min(0)] private Vector2 smallJumpVector = new(10,3);
        [SerializeField] private float playerOffsetForSmallJump = 1;
        [SerializeField] private GameObject floorToDestroy;
        [SerializeField] private float delayBeforeJumpBreak = 3;
        [SerializeField] private Vector2 spotAboveFloor;
        [SerializeField] private float timeToSpotAboveFloor = 0.5f;
        [SerializeField] private float delayAboveFloor = 1;
        [SerializeField] private Vector2 spotBelowFloor;
        [SerializeField] private float floorFallTime = 0.5f;

        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite leftSprite;
        [SerializeField] private Sprite rightSprite;
        [SerializeField] private Sprite backSprite;
        [SerializeField] private Collider2D roomCeiling;
        private PlayerMovementScript _player;
        private Collider2D _collider;
        private Rigidbody2D _rigidbody;
        private LayerMask _groundLayer;
        private SpriteRenderer _spriteRenderer;
        private BossEnemy _originalBoss;
        private Collider2D _floorCollider;
        private bool _attacking;

        private bool Grounded => _collider.IsTouchingLayers(_groundLayer);
        private void Awake()
        {
            _player = PlayerMovementScript.Instance;
            _collider = GetComponent<Collider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _floorCollider = floorToDestroy.GetComponent<Collider2D>();
            _groundLayer = LayerMask.GetMask("Default");
        }

        public void PrepareToSpawn(BossEnemy originalBoss)
        {
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            _originalBoss = originalBoss;
            _rigidbody.position = new Vector2(originalBoss.transform.position.x, _rigidbody.position.y);
            Physics2D.IgnoreCollision(_collider, roomCeiling);
        }

        public void StartSpawn()
        {
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rigidbody.AddForce(initialVelocity * _rigidbody.mass, ForceMode2D.Impulse);
            StartCoroutine(DelayBeforeAttacking());
        }

        private IEnumerator DelayBeforeAttacking()
        {
            yield return new WaitForSeconds(delayBeforeAttacking);
            _attacking = true;
            Physics2D.IgnoreCollision(_collider, roomCeiling, false);
            StartCoroutine(JumpCoroutine());
        }

        private void DestroyOldBossIfPresent()
        {
            if (!_originalBoss) return;
            if (transform.position.y <= _originalBoss.transform.position.y)
            {
                Destroy(_originalBoss.gameObject);
                _originalBoss = null;
            }
        }

        private void LookAtPlayer()
        {
            if (Grounded)
            {
                Vector3 playerPos = _player.transform.position;
                Vector2 pos = _rigidbody.position;
                if (playerPos.x < pos.x) _spriteRenderer.sprite = leftSprite;
                else if (playerPos.x > pos.x) _spriteRenderer.sprite = rightSprite;
            }
        }

        private IEnumerator JumpCoroutine()
        {
            while (_attacking)
            {
                yield return new WaitForSeconds(delayBetweenJumps);
                
                Vector3 playerPos = _player.transform.position;
                Vector2 pos = _rigidbody.position;
                Vector2 force = jumpVector;
                if (playerPos.y < pos.y + playerOffsetForSmallJump) force = smallJumpVector;
                force.x = Mathf.Min(10, Mathf.Abs(playerPos.x - pos.x));
                if (playerPos.x < pos.x) force *= new Vector2(-1,1);
                _rigidbody.AddForce(force * _rigidbody.mass, ForceMode2D.Impulse);
            }
        }

        public void OnDeath()
        {
            StartCoroutine(DeathCoroutine());
        }

        private IEnumerator DeathCoroutine()
        {
            _collider.enabled = false;
            _rigidbody.isKinematic = true;
            _spriteRenderer.sprite = normalSprite;
            yield return new WaitForSeconds(delayBeforeJumpBreak);
            Vector2 prevPos = _rigidbody.position;
            Vector2 trSpotAboveFloor = transform.parent.TransformPoint(spotAboveFloor);
            for (float time = 0; time < timeToSpotAboveFloor; time += Time.deltaTime)
            {
                _rigidbody.position = Vector2.Lerp(prevPos, trSpotAboveFloor, time / timeToSpotAboveFloor);
                yield return null;
            }

            _rigidbody.position = trSpotAboveFloor;
            yield return new WaitForSeconds(delayAboveFloor);
            float floorUpperBound = _floorCollider.bounds.max.y;
            Vector2 trSpotBelowFloor = transform.parent.TransformPoint(spotBelowFloor);
            for (float time = 0; time < floorFallTime; time += Time.deltaTime)
            {
                _rigidbody.position = Vector2.Lerp(trSpotAboveFloor, trSpotBelowFloor, time / floorFallTime);
                if (floorToDestroy && _rigidbody.position.y < floorUpperBound)
                {
                    Destroy(floorToDestroy);
                    floorToDestroy = null;
                }
                yield return null;
            }
            CatManager.EndPhaseOne();
            Destroy(gameObject);
        }

        private void FixedUpdate()
        {
            DestroyOldBossIfPresent();
            if (_attacking)
            {
                LookAtPlayer();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.parent.TransformPoint(spotAboveFloor), 1);
            Gizmos.DrawWireSphere(transform.parent.TransformPoint(spotBelowFloor), 1);
        }
    }
}