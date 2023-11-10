using System;
using UnityEngine;

namespace Enemies.Cat
{
    [RequireComponent(typeof(EntityHealth), typeof(Collider2D), typeof(Rigidbody2D))]
    public class CatBoss : MonoBehaviour
    {
        [NonSerialized]
        public CatBossManager CatManager;

        [SerializeField] private Vector2 initialVelocity = new Vector2(0, -10);
        private Collider2D _collider;
        private Rigidbody2D _rigidbody;
        private BossEnemy _originalBoss;
        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public void PrepareToSpawn(BossEnemy originalBoss)
        {
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            _originalBoss = originalBoss;
            _rigidbody.position = new Vector2(originalBoss.transform.position.x, _rigidbody.position.y);
        }

        public void StartSpawn()
        {
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rigidbody.AddForce(initialVelocity * _rigidbody.mass, ForceMode2D.Impulse);
        }

        private void DestroyOldBossIfPresent()
        {
            if (!_originalBoss) return;
            if (transform.position.y <= _originalBoss.transform.position.y)
            {
                Destroy(_originalBoss.gameObject);
                _originalBoss = null;
            }
        }

        private void FixedUpdate()
        {
            DestroyOldBossIfPresent();
        }
    }
}