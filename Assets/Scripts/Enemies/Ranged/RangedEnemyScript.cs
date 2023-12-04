using System.Collections;
using Player;
using UnityEngine;
using FMODUnity;

namespace Enemies.Ranged
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public class RangedEnemyScript : EnemyBehaviour
    {
        [Tooltip("Prefab of bullet object")]
        public GameObject bulletPrefab;
        [Tooltip("Transform to create bullet position at")]
        public Transform bulletPos;
        [Min(0), Tooltip("Time (seconds) between firing")]
        public float fireDelay = 2;
        [HideInInspector]
        public float damageMultiplier = 1;

        private bool _isPlayerInside;
        private IEnumerator _shootCoroutine;
        [SerializeField] private EventReference _source;
        private PlayerMovementScript _playerMovement;
        private SpriteRenderer _sprite;
        private Animator _animator;
        private static readonly int AnimatorThrowHash = Animator.StringToHash("Throw");
        
        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _playerMovement = PlayerMovementScript.Instance;
        }

        private void FixedUpdate()
        {
            _sprite.flipX = _playerMovement.transform.position.x >= transform.position.x;
        }

        private IEnumerator ShootCoroutine()
        {
            while (_isPlayerInside)
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
            if (!other.GetComponent<PlayerMovementScript>()) return;
            _isPlayerInside = true;
            _shootCoroutine = ShootCoroutine();
            StartCoroutine(_shootCoroutine);
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
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
            if (!_source.Guid.IsNull) RuntimeManager.PlayOneShot(_source,transform.position);
        }
    }
}
