using Player;
using System.Collections;
using UnityEngine;

namespace Enemies
{
    public class HeavyAttack : MonoBehaviour
    {
        public float attackDistance;
        public float attackWidth;
        public float attackDelay;
        public int attackDamage;
        public float knockbackForce;

        [SerializeField] private CapsuleCollider2D _collider;
        [SerializeField] private LayerMask _playerLayer;
        private Vector2 _attackBoxCenter;
        private Vector2 _attackBoxSize;
        private PatrolMovement _movement;
        private PlayerMovementScript _playerMovement;
        private EntityHealth _playerHealth;
        private Transform _player;
        private float _attackTimer;
        private bool _isAttacking;


        private void Awake()
        {
            _movement = GetComponent<PatrolMovement>();
            _attackTimer = 0;
            _playerMovement = PlayerMovementScript.Instance;
            _playerHealth = PlayerHealth.Instance;
        }

        /// <summary>
        /// Enemy should always stand still when player is in range, only attacks when _attackTimer is below 0
        /// </summary>
        private void Update()
        {
            _attackTimer -= Time.deltaTime;
            if (PlayerInRange()) {
                _movement.DisableMovement();
                if (_attackTimer <= 0)
                {
                    PerformAttack();
                }
            } else if (!_isAttacking)
            {
                _movement.EnableMovement();
            }

        }

        /// <summary>
        ///  Determines whether player is within attack range; range identical to box in OnDrawGizmos()
        /// </summary>
        private bool PlayerInRange()
        {
            _attackBoxCenter = _collider.bounds.center + transform.right * attackDistance * _movement.GetDirection();
            _attackBoxSize = new Vector3(_collider.bounds.size.x * attackWidth, _collider.bounds.size.y * 1.1f, _collider.bounds.size.z);

            RaycastHit2D hit = Physics2D.BoxCast(_attackBoxCenter, _attackBoxSize, 0,
                Vector2.left, 0, _playerLayer);

            if (hit.collider != null)
            {
                _player = hit.collider.transform;
            }

            return hit.collider != null;
        }

        /// <summary>
        /// Visualization of attack range
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_collider.bounds.center + transform.right * attackDistance * transform.localScale.x,
                new Vector3(_collider.bounds.size.x * attackWidth, _collider.bounds.size.y * 1.1f, _collider.bounds.size.z));
        }

        /// <summary>
        /// attacks player after set amount of delay depending on animation, sets attack cooldown timer
        /// </summary>
        private void PerformAttack()
        {
            _attackTimer = attackDelay;
            _isAttacking = true;
            StartCoroutine(DelayBeforeAttack());
        }

        private IEnumerator DelayBeforeAttack()
        {
            yield return new WaitForSeconds(1); // adjust attack animation time here

            if (PlayerInRange())
            {
                _playerHealth.TakeDamage(attackDamage);
                Vector2 knockbackDirection = new((_player.position - transform.position).normalized.x * 0.1f, 0.04f);
                _playerMovement.RequestKnockback(knockbackDirection, knockbackForce);
            }

            _isAttacking = false;
        }

    }
}