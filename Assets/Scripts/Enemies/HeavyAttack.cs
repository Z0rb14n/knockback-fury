using Player;
using System.Collections;
using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(Animator), typeof(PatrolMovement), typeof(AudioSource))]
    public class HeavyAttack : MonoBehaviour
    {
        public float attackDistance;
        public float attackWidth;
        public float attackDelay;
        public int attackDamage;
        public float knockbackForce;

        [SerializeField] private CapsuleCollider2D _collider;
        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private float _delayBeforeAttack;
        private Vector2 _attackBoxCenter;
        private Vector2 _attackBoxSize;
        private PatrolMovement _movement;
        private PlayerMovementScript _playerMovement;
        private EntityHealth _playerHealth;
        private Transform _player;
        private float _attackTimer;
        private bool _isAttacking;
        private Animator _animator;
        private AudioSource _audioSource;
        private float _attackAnimationTime;

        private static readonly int _animationAtkHash = Animator.StringToHash("Attacking");


        private void Awake()
        {
            _movement = GetComponent<PatrolMovement>();
            _attackTimer = 0;
            _playerMovement = PlayerMovementScript.Instance;
            _playerHealth = PlayerHealth.Instance;
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
            GetAttackAnimationTime();
        }

        /// <summary>
        /// Enemy should always stand still when player is in range, only attacks when _attackTimer is below 0
        /// </summary>
        private void Update()
        {
            _attackTimer -= Time.deltaTime;
            if (!_isAttacking)
            {
                if (PlayerInRange())
                {
                    _movement.DisableMovement();
                    if (_attackTimer <= 0)
                    {
                        _movement.StartAttack();
                        PerformAttack();
                    }
                }
                else if (!_isAttacking)
                {
                    _movement.EnableMovement();
                }
            }
        }


        /// <summary>
        ///  Determines whether player is within attack range; range identical to box in OnDrawGizmos()
        /// </summary>
        private bool PlayerInRange()
        {
            _attackBoxCenter = _collider.bounds.center + transform.right * attackDistance * _movement.Direction;
            _attackBoxSize = new Vector3(_collider.bounds.size.x * attackWidth, _collider.bounds.size.y * 1.1f,
                _collider.bounds.size.z);

            RaycastHit2D hit = Physics2D.BoxCast(_attackBoxCenter, _attackBoxSize, 0,
                Vector2.left, 0, _playerLayer);

            if (hit.collider != null)
            {
                _player = hit.collider.transform;
            }

            return hit.collider != null;
        }

        /// <summary>
        /// Visualization of attack range
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                _collider.bounds.center + transform.right * attackDistance * (_movement?.Direction ?? 1),
                new Vector3(_collider.bounds.size.x * attackWidth, _collider.bounds.size.y * 1.1f,
                    _collider.bounds.size.z));
        }

        /// <summary>
        /// Attacks player after set amount of delay and if player is still in range, sets attack cooldown timer
        /// </summary>
        private void PerformAttack()
        {
            _attackTimer = attackDelay;
            _isAttacking = true;
            _animator.SetBool(_animationAtkHash, true);
            StartCoroutine(DelayBeforeAttack());
        }

        /// <summary>
        /// Helper for PerformAttack(); attacks player after set delay, damages player if in range, then waits for animation to finish
        /// </summary>
        private IEnumerator DelayBeforeAttack()
        {
            if (_delayBeforeAttack < 0 || _delayBeforeAttack > _attackAnimationTime)
            {
                Debug.LogError("HeavyAttack: Delay before attack must be 0 <= delay <= animation length");
            }

            yield return new WaitForSeconds(_delayBeforeAttack);

            if (PlayerInRange())
            {
                _playerHealth.TakeDamage(attackDamage);
                _audioSource.Play();
                Vector2 knockbackDirection =
                    new((_player.position - _collider.bounds.center).normalized.x * 0.1f, 0.04f);
                _playerMovement.RequestKnockback(knockbackDirection, knockbackForce);
            }

            yield return new WaitForSeconds(_attackAnimationTime - _delayBeforeAttack);

            _isAttacking = false;
            _movement.EndAttack();
            _animator.SetBool(_animationAtkHash, false);
        }

        /// <summary>
        /// Gets length of attack animation
        /// </summary>
        private void GetAttackAnimationTime()
        {
            AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == "Attack")
                {
                    _attackAnimationTime = clip.length;
                }
            }
        }

        /// <summary>
        /// On disable, hide animations
        /// </summary>
        private void OnDisable()
        {
            _isAttacking = false;
            _animator.SetBool(_animationAtkHash, false);
        }
    }
}