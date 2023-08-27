using System.Collections;
using Player;
using UnityEngine;
using Weapons;

namespace Enemies.Ranged
{
    public class EnemyBombScript : EnemyBulletScript
    {
        public GameObject explosionVFX;
        public float verticalDirOffset = 2;
        public float radius = 1;
        public int playerDamage = 100;

        protected override void Awake()
        {
            base.Awake();   
        }

        public override void Initialize()
        {
            if (!rigidbody2D) rigidbody2D = GetComponent<Rigidbody2D>();
            
            Vector3 direction = PlayerMovementScript.Instance.transform.position - transform.position + new Vector3(0,verticalDirOffset,0);
            rigidbody2D.velocity = direction.normalized * projectileSpeed;
            float rot = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.localEulerAngles = new Vector3(0, 0, rot);
            StartCoroutine(DelayedExplosion());
        }

        public void Detonate(bool playerCaused)
        {
            Vector3 pos = transform.position;
            GameObject explosionObject = Instantiate(explosionVFX, pos, Quaternion.identity);
            explosionObject.GetComponent<ExplosionVFX>().SetSize(radius);

            Collider2D playerCollider = Physics2D.OverlapCircle(pos, radius, playerLayerMask);
            Debug.Log(playerCollider);
            if (playerCollider)
            {
                PlayerMovementScript playerMovement = PlayerMovementScript.Instance;
                EntityHealth playerHealth = PlayerHealth.Instance;
                playerHealth.TakeDamage(bulletDamage);
                Vector2 knockbackDirection = new((playerMovement.transform.position - pos).normalized.x * 0.1f, 0.04f);
                playerMovement.RequestKnockback(knockbackDirection, knockbackForce);
            }
            Destroy(gameObject);

            if (!playerCaused) return;
            // ReSharper disable once Unity.PreferNonAllocApi
            Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, radius);
            foreach (Collider2D col in colliders)
            {
                EntityHealth health = col.GetComponent<EntityHealth>();
                if (health && health != PlayerHealth.Instance)
                {
                    health.TakeDamage(playerDamage);
                }
            }
        }
        
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.GetComponent<PlayerMovementScript>())
            {
                Detonate(false);
            }
        }

        private IEnumerator DelayedExplosion()
        {
            yield return new WaitForSeconds(delayBeforeDestruction);
            Detonate(false);
        }
    }
}