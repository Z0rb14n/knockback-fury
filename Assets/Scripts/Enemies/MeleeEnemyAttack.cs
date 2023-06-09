using Player;
using UnityEngine;

namespace Enemies
{
    public class MeleeEnemyAttack : MonoBehaviour
    {
        public int damage;
        public int knockbackForce;

        private LayerMask _playerLayer;
        private PlayerMovementScript _playerMovement;
        private EntityHealth _playerHealth;


        public void Awake()
        {
            _playerLayer = LayerMask.NameToLayer("Player");
        }

        /// <summary>
        /// Hits player: requests knockback, deals damage
        /// </summary>
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.collider.gameObject.layer != _playerLayer) return;
            // ReSharper disable twice ConvertIfStatementToNullCoalescingAssignment
            if (ReferenceEquals(_playerHealth, null))
                _playerHealth = collision.collider.gameObject.GetComponent<EntityHealth>();
            if (ReferenceEquals(_playerMovement, null))
                _playerMovement = collision.collider.gameObject.GetComponent<PlayerMovementScript>();
            _playerHealth.TakeDamage(damage);
            Vector2 knockbackDirection = new((collision.transform.position - transform.position).normalized.x * 0.1f, 0.04f);
            _playerMovement.RequestKnockback(knockbackDirection, knockbackForce);
        }
    }
}
