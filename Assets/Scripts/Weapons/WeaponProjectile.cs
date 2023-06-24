using Player;
using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class WeaponProjectile : MonoBehaviour
    {
        [Tooltip("Whether the projectile is a rotating one")]
        public bool rotating;
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
        /// <param name="data">Weapon that fired this</param>
        /// <param name="direction">Normalized Direction</param>
        public void Initialize(WeaponData data, Vector2 direction, bool hitPlayer = false)
        {
            _damage = data.projectileDamage;
            _remainingDistance = data.range;

            _body.velocity = direction * data.projectileSpeed;
            _hitPlayer = hitPlayer;
        }

        private void FixedUpdate()
        {
            Vector3 currPos = transform.position;
            _remainingDistance -= (currPos - _prevPosition).magnitude;
            _prevPosition = currPos;
            Vector2 vel = _body.velocity;

            if (rotating)
            {
                float rotation = Mathf.Atan2(vel.y, vel.x);
                transform.localEulerAngles = new Vector3(0, 0, rotation * Mathf.Rad2Deg);
            }

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