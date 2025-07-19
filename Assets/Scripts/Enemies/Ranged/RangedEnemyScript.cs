using System.Collections;
using Player;
using UnityEngine;
using FMODUnity;
using UnityEngine.Serialization;

namespace Enemies.Ranged
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public class RangedEnemyScript : EnemyBehaviour
    {
        [Tooltip("Prefab of bullet object")] public GameObject bulletPrefab;

        [Tooltip("Transform to create bullet position at")]
        public Transform bulletPos;

        [Min(0), Tooltip("Time (seconds) between firing")]
        public float fireDelay = 2;

        [Tooltip("Whether we check distance to player")]
        public bool useProximityCheck = true;

        [Tooltip("True if triggers are used")]
        public bool useTriggerCheck = true;
        [Tooltip("If trigger check is false, hard-coded distance")]
        public float proximityDistance = 10;
        [HideInInspector] public float damageMultiplier = 1;

        private bool _isPlayerInside;
        private IEnumerator _shootCoroutine;

        [FormerlySerializedAs("_source")] [SerializeField]
        private EventReference source;

        private PlayerMovementScript _playerMovement;
        private SpriteRenderer _sprite;
        private Animator _animator;
        private static readonly int AnimatorThrowHash = Animator.StringToHash("Throw");

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _playerMovement = PlayerMovementScript.Instance;
            if (!useProximityCheck)
            {
                _shootCoroutine = ShootCoroutine();
                StartCoroutine(_shootCoroutine);
            }
        }

        private void FixedUpdate()
        {
            Vector2 pos = transform.position;
            _sprite.flipX = _playerMovement.Pos.x >= pos.x;
            if (useProximityCheck && !useTriggerCheck)
            {
                if (_isPlayerInside && Vector2.Distance(_playerMovement.Pos, pos) > proximityDistance)
                {
                    _isPlayerInside = false;
                    StopCoroutine(_shootCoroutine);
                    _shootCoroutine = null;
                }
                else if (!_isPlayerInside && Vector2.Distance(_playerMovement.Pos, pos) <= proximityDistance)
                {
                    _isPlayerInside = true;
                    _shootCoroutine = ShootCoroutine();
                    StartCoroutine(_shootCoroutine);
                }
            }
        }

        private IEnumerator ShootCoroutine()
        {
            while (!useProximityCheck || _isPlayerInside)
            {
                yield return new WaitForSeconds(fireDelay);
                if (_animator) _animator.SetTrigger(AnimatorThrowHash);
                else
                {
                    GameObject go = Instantiate(bulletPrefab, bulletPos.position, Quaternion.identity);
                    go.GetComponent<EnemyBulletScript>().Initialize(damageMultiplier);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (useProximityCheck) return;
            if (!useTriggerCheck) return;
            if (!other.GetComponent<PlayerMovementScript>()) return;
            _isPlayerInside = true;
            _shootCoroutine = ShootCoroutine();
            StartCoroutine(_shootCoroutine);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (useProximityCheck) return;
            if (!useTriggerCheck) return;
            if (!other.GetComponent<PlayerMovementScript>()) return;
            _isPlayerInside = false;
            StopCoroutine(_shootCoroutine);
            _shootCoroutine = null;
        }

        /// <summary>
        /// Called when the animator reaches the 'throw' part
        /// </summary>
        public void AnimatorEventReached()
        {
            GameObject go = Instantiate(bulletPrefab, bulletPos.position, Quaternion.identity);
            go.GetComponent<EnemyBulletScript>().Initialize(damageMultiplier);
            if (!source.Guid.IsNull) RuntimeManager.PlayOneShot(source, transform.position);
        }
    }
}