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


        private void Awake()
        {
            _movement = GetComponent<PatrolMovement>();
            _attackTimer = 0;
        }

        private void Update()
        {
            _attackTimer -= Time.deltaTime;
            if (PlayerInRange() && _attackTimer <= 0)
            {
                _attackTimer = attackDelay;
                PerformAttack();
            }
        }

        /// <summary>
        ///  Determines whether player is within attack range
        /// </summary>
        private bool PlayerInRange()
        {
            _attackBoxCenter = _collider.bounds.center + transform.right * attackDistance * _movement.GetDirection();
            _attackBoxSize = new Vector3(_collider.bounds.size.x * attackWidth, _collider.bounds.size.y * 1.1f, _collider.bounds.size.z);

            RaycastHit2D hit = Physics2D.BoxCast(_attackBoxCenter, _attackBoxSize, 0,
                Vector2.left, 0, _playerLayer);

            if (hit.collider != null)
            {
                _playerMovement = hit.collider.gameObject.GetComponent<PlayerMovementScript>();
                _playerHealth = hit.collider.gameObject.GetComponent<EntityHealth>();
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
        /// attacks player after set amount of delay depending on animation, goes into cooldown afterwards
        /// </summary>
        private void PerformAttack()
        {
            StartCoroutine(DelayBeforeAttack());
        }

        private IEnumerator DelayBeforeAttack()
        {
            _movement.DisableMovement();
            yield return new WaitForSeconds(1); // adjust attack animation time here

            if (PlayerInRange())
            {
                _playerHealth.TakeDamage(attackDamage);
                Vector2 knockbackDirection = new((_player.position - transform.position).normalized.x * 0.1f, 0.04f);
                _playerMovement.RequestKnockback(knockbackDirection, knockbackForce);
            }

            _movement.EnableMovement();
        }

    }
}