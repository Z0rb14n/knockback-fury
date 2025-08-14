using System.Collections;
using Player;
using UnityEngine;

namespace Enemies.Cat
{
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D), typeof(AudioSource))]
    public class CatGoon : MonoBehaviour
    {
        [SerializeField, Min(0)] private int attackDamage = 2;
        [SerializeField, Min(0)] private float knockbackForce = 150;
        [SerializeField] private Transform batTransform;
        [SerializeField, Min(0)] private float attackDelay = 1.5f;
        [SerializeField, Min(0)] private float batDownLength = 1f;
        [SerializeField, Min(0)] private Vector2 jumpVector = new(10,10);
        [SerializeField, Range(-180,180)] private float batEndRotation = 30;
        private bool Grounded => _collider.IsTouching(_groundFilter);
        private ContactFilter2D _groundFilter;
        private LayerMask _groundLayer;
        private PlayerHealth _playerHealth;
        private PlayerMovementScript _playerMovement;
        private Rigidbody2D _rigidbody;
        private AudioSource _audioSource;
        private Collider2D _collider;
        private IEnumerator _attackCoroutine;
        private bool _canAttack = true;

        private void Awake()
        {
            _playerHealth = PlayerHealth.Instance;
            _playerMovement = PlayerMovementScript.Instance;
            _collider = GetComponent<Collider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _audioSource = GetComponent<AudioSource>();
            _groundLayer = LayerMask.GetMask("Default");
            _groundFilter = new ContactFilter2D
            {
                layerMask = _groundLayer,
                useLayerMask = true,
                useNormalAngle = true,
                minNormalAngle = 30,
                maxNormalAngle = 150
            };
        }

        public void StartAttackingPlayer()
        {
            _attackCoroutine = AttackCoroutine();
            StartCoroutine(_attackCoroutine);
        }

        public void StopAttackingPlayer()
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
            batTransform.localEulerAngles = Vector3.zero;
        }

        private void FixedUpdate()
        {
            Vector3 playerPos = _playerMovement.transform.position;
            Vector3 pos = transform.position;
            bool isLeft = playerPos.x < pos.x;
            bool isIn = _collider.bounds.Contains(playerPos);
            transform.localScale = new Vector3(isLeft ? -1 : 1, 1, 1);
            if (Grounded && !isIn)
            {
                _rigidbody.AddForce(jumpVector * new Vector2(isLeft? -1 : 1, 1) * _rigidbody.mass, ForceMode2D.Impulse);
            }
        }

        private IEnumerator AttackCoroutine()
        {
            while (_attackCoroutine != null)
            {
                yield return new WaitUntil(() => _canAttack);
                StartCoroutine(AttackAnimation());
            }
        }

        private IEnumerator AttackAnimation()
        {
            _canAttack = false;
            batTransform.localEulerAngles = new Vector3(0, 0, batEndRotation);
            _playerHealth.TakeDamage(attackDamage);
            _audioSource.Play();
            Vector2 knockbackDirection = new((_playerMovement.transform.position - _collider.bounds.center).normalized.x * 0.1f, 0.04f);
            _playerMovement.RequestKnockback(knockbackDirection, knockbackForce);
            yield return new WaitForSeconds(batDownLength);
            // ReSharper disable once Unity.InefficientPropertyAccess
            batTransform.localEulerAngles = Vector3.zero;
            yield return new WaitForSeconds(attackDelay - batDownLength);
            _canAttack = true;
        }
    }
}