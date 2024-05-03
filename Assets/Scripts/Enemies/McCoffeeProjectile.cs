using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class McCoffeeProjectile : ContactDamage
    {
        [SerializeField, Min(0)] private float speed = 15;
        private LayerMask enemyLayer;
        private LayerMask enemyBombLayer;
        private Rigidbody2D _rigidbody2D;

        public override void Awake()
        {
            base.Awake();
            enemyLayer = LayerMask.NameToLayer("Enemy");
            enemyBombLayer = LayerMask.NameToLayer("EnemyBomb");
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void Initialize(float angle)
        {
            Vector2 dir = new(Mathf.Cos(angle), Mathf.Sin(angle));
            _rigidbody2D.velocity = dir * speed;
        }
        
        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            base.OnCollisionEnter2D(collision);
            if (collision.collider.gameObject.layer != enemyLayer && collision.collider.gameObject.layer != enemyBombLayer)
            {
                Destroy(gameObject);
            }
        }
    }
}