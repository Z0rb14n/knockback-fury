using System.Collections.Generic;
using Player;
using UnityEngine;
using Upgrades;
using FMODUnity;
using FMOD.Studio;
using Random = UnityEngine.Random;

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

        [SerializeField] private EventReference _explosionSFX;
        public GameObject detonationVFX;
        private float _remainingDistance;
        private int _damage;
        private int _remainingPierces;
        private Vector3 _prevPosition;
        private Rigidbody2D _body;
        private Collider2D _collider;
        private bool _hitPlayer;
        private WeaponData _weaponData;
        private readonly Collider2D[] _colliderTest = new Collider2D[20];
        private readonly Queue<(EntityHealth, Collider2D, float)> _invulnContacts = new();
        private readonly HashSet<EntityHealth> _entityInvulns = new();
        
        private void Awake()
        {
            _prevPosition = transform.position;
            _collider = GetComponent<Collider2D>();
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
            _damage = data.actualDamage;
            _remainingDistance = data.actualRange;

            _body.velocity = direction * data.projectileSpeed;
            _hitPlayer = hitPlayer;
            _remainingPierces = data.pierceInfo.maxPierces;
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

        private bool UpdateAndCheckInvulnTimer(EntityHealth other)
        {
            float currTime = Time.time;
            while (_invulnContacts.TryPeek(out (EntityHealth, Collider2D, float) pair))
            {
                if (currTime - pair.Item3 >= _weaponData.pierceInfo.invulnTimer)
                {
                    _invulnContacts.Dequeue();
                    _entityInvulns.Remove(pair.Item1);
                    Physics2D.IgnoreCollision(_collider, pair.Item2, false);
                }
                else
                {
                    break;
                }
            }

            return other && _entityInvulns.Contains(other);
        }

        private void CollisionLogic(Collider2D other, bool isTrigger)
        {
            if (isTrigger ^ _collider.isTrigger) return;
            EntityHealth health = other.GetComponent<EntityHealth>();
            if (UpdateAndCheckInvulnTimer(health)) return;
            if (!_hitPlayer && health is PlayerHealth) return;
            bool hitEntity = Weapon.HitEntity(other, _damage);
            Detonation();
            if (Weapon.CheckAndUpdatePiercing(_weaponData, health, ref _remainingPierces))
            {
                RefundAmmoLogic(hitEntity);
                Destroy(gameObject);
            }
            else
            {
                if (health && _weaponData.pierceMode != PierceMode.None)
                {
                    Physics2D.IgnoreCollision(_collider, other, true);
                    _invulnContacts.Enqueue((health, other, Time.time));
                    _entityInvulns.Add(health);
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            CollisionLogic(other.collider, false);
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            CollisionLogic(other.collider, false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            CollisionLogic(other, true);
        }
        
        private void OnTriggerStay2D(Collider2D other)
        {
            CollisionLogic(other, true);
        }

        private void Detonation()
        {
            if (!detonateOnDestroy) return;
            int size = Physics2D.OverlapCircleNonAlloc(_body.position, explosionRange, _colliderTest);
            for (int i = 0; i < size; i++)
            {
                Weapon.HitEntity(_colliderTest[i], _damage, _weaponData.selfDamage);
            }

            GameObject go = Instantiate(detonationVFX, transform.parent);
            RuntimeManager.PlayOneShot(_explosionSFX,transform.position);
            go.transform.position = transform.position;
            go.GetComponent<ExplosionVFX>().SetSize(explosionRange);
        }
    }
}