using System.Collections;
using Player;
using UnityEngine;

namespace Enemies.Ranged
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer), typeof(AudioSource))]
    public class RangedEnemyScript : EnemyBehaviour
    {
        [Tooltip("Prefab of bullet object")] public GameObject bulletPrefab;
        [Tooltip("Transform to create bullet position at")] public Transform bulletPos;
        [Min(0), Tooltip("Time (seconds) between firing)")] public float fireDelay = 2;

        [Tooltip("Whether we check distance to player")] public bool useProximityCheck = true;
        [Tooltip("True if triggers are used")] public bool useTriggerCheck = true;
        [Tooltip("If trigger check is false, hard-coded distance")] public float proximityDistance = 10;
        [HideInInspector] public float damageMultiplier = 1;

        private bool _isPlayerInside;
        private Coroutine _shootCoroutine;

        private PlayerMovementScript _playerMovement;
        private SpriteRenderer _sprite;
        private Animator _animator;
        private AudioSource _audioSource;
        private static readonly int AnimatorThrowHash = Animator.StringToHash("Throw");

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
            _playerMovement = PlayerMovementScript.Instance;

            // FIX: only auto-start shooting if *no proximity or trigger checks* are used
            if (!useProximityCheck && !useTriggerCheck)
                StartShooting();
        }

        private void FixedUpdate()
        {
            Vector2 pos = transform.position;
            _sprite.flipX = _playerMovement.Pos.x >= pos.x;

            // FIX: clarified proximity-only logic
            if (useProximityCheck && !useTriggerCheck)
            {
                float dist = Vector2.Distance(_playerMovement.Pos, pos);

                if (_isPlayerInside && dist > proximityDistance)
                {
                    _isPlayerInside = false;
                    StopShooting();
                }
                else if (!_isPlayerInside && dist <= proximityDistance)
                {
                    _isPlayerInside = true;
                    StartShooting();
                }
            }
        }

        // FIX: centralised coroutine start/stop to prevent duplicates
        private void StartShooting()
        {
            if (_shootCoroutine == null)
                _shootCoroutine = StartCoroutine(ShootCoroutine());
        }

        private void StopShooting()
        {
            if (_shootCoroutine != null)
            {
                StopCoroutine(_shootCoroutine);
                _shootCoroutine = null;
            }
        }

        private IEnumerator ShootCoroutine()
        {
            while (!useProximityCheck || _isPlayerInside)
            {
                yield return new WaitForSeconds(fireDelay);

                if (_animator)
                    _animator.SetTrigger(AnimatorThrowHash);
                else
                {
                    GameObject go = Instantiate(bulletPrefab, bulletPos.position, Quaternion.identity);
                    go.GetComponent<EnemyBulletScript>().Initialize(damageMultiplier);
                }
            }

            // cleanup when exiting loop
            _shootCoroutine = null;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // FIX: only trigger if triggerCheck is enabled
            if (!useTriggerCheck) return;
            if (useProximityCheck) return;
            if (!other.GetComponent<PlayerMovementScript>()) return;

            _isPlayerInside = true;
            StartShooting(); // FIX: guarded against duplicates
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!useTriggerCheck) return;
            if (useProximityCheck) return;
            if (!other.GetComponent<PlayerMovementScript>()) return;

            _isPlayerInside = false;
            StopShooting(); // FIX: use new helper
        }

        /// <summary>
        /// Called when the animator reaches the 'throw' part
        /// </summary>
        public void AnimatorEventReached()
        {
            GameObject go = Instantiate(bulletPrefab, bulletPos.position, Quaternion.identity);
            go.GetComponent<EnemyBulletScript>().Initialize(damageMultiplier);
            _audioSource.Play();
        }
    }
}
