using Player;
using UnityEngine;
using Upgrades;

namespace Weapons
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    public class WeaponProjectile : MonoBehaviour
    {
        [Tooltip("Whether the projectile is a rotating one")]
        public bool rotating;
        [Tooltip("Explosion range")]
        public float explosionRange;
        [Tooltip("(For Grenades) Detonates on destruction")]
        public bool detonateOnDestroy;
        [Tooltip("Detonation VFX Prefab")]
        public GameObject detonationVFX;
        private float _remainingDistance;
        private int _damage;
        private Vector3 _prevPosition;
        private Rigidbody2D _body;
        private bool _hitPlayer;
        private WeaponData _weaponData;
        private Collider2D[] _colliderTest = new Collider2D[20];
        
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
        /// <param name="hitPlayer">Whether to hit the player or not</param>
        public void Initialize(WeaponData data, Vector2 direction, bool hitPlayer = false)
        {
            _weaponData = data;
            _damage = data.projectileDamage;
            _remainingDistance = data.actualRange;

            _body.velocity = direction * data.projectileSpeed;
            _hitPlayer = hitPlayer;
        }

        public void ModifyDamage(float multiplier)
        {
            _damage = Mathf.RoundToInt(_damage * multiplier);
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
                RefundAmmoLogic(false);
                Detonation();
                Destroy(gameObject);
            }
        }

        private void RefundAmmoLogic(bool hitEntity)
        {
            if (hitEntity) return;
            if (_weaponData.numProjectiles != 1 || PlayerUpgradeManager.Instance[UpgradeType.EfficientScurry] <= 0) return;
            if (Random.Range(0f, 1f) > 0.5f)
            {
                _weaponData.IncrementClip();
            }
        }


        private void OnCollisionEnter2D(Collision2D other)
        {
            EntityHealth health = other.collider.GetComponent<EntityHealth>();
            if (!_hitPlayer && health is PlayerHealth) return;
            bool hitEntity = Weapon.HitEntity(other.collider, _damage);
            RefundAmmoLogic(hitEntity);
            Detonation();
            Destroy(gameObject);
        }

        private void Detonation()
        {
            if (!detonateOnDestroy) return;
            int size = Physics2D.OverlapCircleNonAlloc(_body.position, explosionRange, _colliderTest);
            for (int i = 0; i < size; i++)
            {
                Weapon.HitEntity(_colliderTest[i], _damage);
            }

            GameObject go = Instantiate(detonationVFX, transform.parent);
            go.transform.position = transform.position;
            go.GetComponent<ExplosionVFX>().SetSize(explosionRange);
        }
    }
}