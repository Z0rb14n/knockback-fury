using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class WeaponProjectile : MonoBehaviour
    {
        private int _damage;
        private Rigidbody2D _body;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Initializes required parameters
        /// </summary>
        /// <param name="damage">Damage</param>
        /// <param name="range">Distance before projectile disappears</param>
        /// <param name="speed">Projectile Speed, units/sec</param>
        /// <param name="direction">Normalized Direction</param>
        public void Initialize(int damage, float range, float speed, Vector2 direction)
        {
            _damage = damage;
            _body.velocity = direction * speed;
            Destroy(gameObject, range / speed); // projectile will be destroyed after travelling its range
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.tag == "Player") // using tag comparison instead of GetComponent
            {
                Debug.Log($"Hit Player for {_damage}");
            }
            Destroy(gameObject);
            // TODO HIT ENEMY
        }
    }
}