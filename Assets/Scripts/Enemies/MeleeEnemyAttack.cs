using UnityEngine;

namespace Enemies
{
    public class MeleeEnemyAttack : MonoBehaviour
    {
        public int damage;
        public int knockbackForce;

        private LayerMask _playerLayer;
        private Rigidbody2D _playerBody;
        private EntityHealth _playerHealth;
        private bool _knockbackRequest;


        public void Awake()
        {
            _playerLayer = LayerMask.NameToLayer("Player");
        }



        private void FixedUpdate()
        {
            if (!_knockbackRequest) return;
            Vector2 knockbackDirection = new Vector2((_playerBody.transform.position - transform.position).normalized.x * 0.1f, 0.04f);
            _playerBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            _knockbackRequest = false;
        }

        /// <summary>
        /// Hits player: requests knockback, deals damage
        /// </summary>
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.collider.gameObject.layer != _playerLayer) return;
            _playerBody = collision.collider.gameObject.GetComponent<Rigidbody2D>();
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (ReferenceEquals(_playerHealth, null))
                _playerHealth = collision.collider.gameObject.GetComponent<EntityHealth>();
            _playerHealth.TakeDamage(damage);
            _knockbackRequest = true;
        }
    }
}
