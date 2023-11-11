using Player;
using UnityEngine;

namespace Enemies
{
    public class ContactDamage : MonoBehaviour
    {
        public int damage;
        public int knockbackForce;
        public bool damageOnTrigger;

        private LayerMask _playerLayer;


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
            PlayerHealth.Instance.TakeDamage(damage);
            Vector2 knockbackDirection = new((collision.transform.position - transform.position).normalized.x * 0.1f, 0.04f);
            PlayerMovementScript.Instance.RequestKnockback(knockbackDirection, knockbackForce);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!damageOnTrigger) return;
            if (other.gameObject.layer != _playerLayer) return;
            
            PlayerHealth.Instance.TakeDamage(damage);
            Vector2 knockbackDirection = new((other.transform.position - transform.position).normalized.x * 0.1f, 0.04f);
            PlayerMovementScript.Instance.RequestKnockback(knockbackDirection, knockbackForce);
        }
    }
}
