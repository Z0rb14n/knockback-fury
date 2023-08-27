using System;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemies.Ranged
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyBulletScript : MonoBehaviour
    {
        [FormerlySerializedAs("force")] public float projectileSpeed;
        public int bulletDamage;
        public int knockbackForce;
        public float delayBeforeDestruction = 10;
        public float verticalKnockback = 0.04f;
        public float knockbackStrength = 0.1f;

        protected new Rigidbody2D rigidbody2D;
        protected LayerMask playerLayerMask;
        private float _timer;

        protected virtual void Awake()
        {
            rigidbody2D = GetComponent<Rigidbody2D>();
            playerLayerMask = LayerMask.GetMask("Player");
        }

        public virtual void Initialize()
        {
            if (!rigidbody2D) rigidbody2D = GetComponent<Rigidbody2D>();
            
            Vector3 direction = PlayerMovementScript.Instance.transform.position - transform.position;
            rigidbody2D.velocity = direction.normalized * projectileSpeed;
            float rot = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.localEulerAngles = new Vector3(0, 0, rot);
            Destroy(gameObject, delayBeforeDestruction);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((other.gameObject.layer & playerLayerMask) != 0)
            {
                EntityHealth playerHealth = other.gameObject.GetComponent<EntityHealth>();
                PlayerMovementScript playerMovement = other.gameObject.GetComponent<PlayerMovementScript>();

                playerHealth.TakeDamage(bulletDamage);
                Vector2 knockbackDirection = new((other.transform.position - transform.position).normalized.x * knockbackStrength, verticalKnockback);
                playerMovement.RequestKnockback(knockbackDirection, knockbackForce);

                Destroy(gameObject);
            }
        }
    }
}
