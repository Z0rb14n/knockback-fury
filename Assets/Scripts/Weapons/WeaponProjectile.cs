using Player;
using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class WeaponProjectile : MonoBehaviour
    {
        private float _remainingDistance;
        private int _damage;
        private Vector3 _prevPosition;
        private Rigidbody2D _body;
        private bool _hitPlayer;
        
        private void Awake()
        {
            _prevPosition = transform.position;
            _body = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Initializes required parameters
        /// </summary>
        /// <param name="damage">Damage</param>
        /// <param name="range">Distance before projectile disappears</param>
        /// <param name="speed">Projectile Speed, units/sec</param>
        /// <param name="direction">Normalized Direction</param>
        public void Initialize(int damage, float range, float speed, Vector2 direction, bool hitPlayer = false)
        {
            _damage = damage;
            _remainingDistance = range;

            _body.velocity = direction * speed;
            _hitPlayer = hitPlayer;
        }

        private void FixedUpdate()
        {
            Vector3 currPos = transform.position;
            _remainingDistance -= (currPos - _prevPosition).magnitude;
            _prevPosition = currPos;
            if (_remainingDistance <= 0)
            {
                Destroy(gameObject);
            }
        }


        private void OnCollisionEnter2D(Collision2D other)
        {
            EntityHealth health = other.collider.GetComponent<EntityHealth>();
            if (!_hitPlayer && health is PlayerHealth) return;
            Weapon.HitEntityHealth(health,_damage);
            Destroy(gameObject);
        }
    }
}