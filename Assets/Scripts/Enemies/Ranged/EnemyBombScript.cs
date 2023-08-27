using System.Collections;
using Player;
using UnityEngine;
using Weapons;

namespace Enemies.Ranged
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class EnemyBombScript : EnemyBulletScript
    {
        public GameObject explosionVFX;
        public float verticalDirOffset = 2;
        public float radius = 1;
        public int playerDamage = 100;
        public bool playerVelocityPrediction = true;

        public override void Initialize()
        {
            if (!rigidbody2D) rigidbody2D = GetComponent<Rigidbody2D>();

            PlayerMovementScript playerMovementScript = PlayerMovementScript.Instance;
            rigidbody2D.velocity = CalculateVelocity(transform.position, playerMovementScript.transform.position, playerMovementScript.Velocity);
            StartCoroutine(DelayedExplosion());
        }

        /// <summary>
        /// Calculates the velocity that we want given a starting position, ending position and target's velocity
        /// </summary>
        /// <param name="startingPos">Starting position</param>
        /// <param name="endingPos">Ending position</param>
        /// <param name="endingVel">Target's ending velocity</param>
        /// <returns>Velocity to be at, clamped to max velocity.</returns>
        /// <remarks>
        /// cba to do linear algebra here - my notes from my initial 30 minutes of checking: 
        /// <p> - unconstrained case has 2 equations with 3 unknowns:</p>
        /// <p> - x(t) = x0 + x'*t </p>
        /// <p> - y(t) = y0 + y't - gt^2/2 </p>
        /// <p> - Solve for t1, x', y' given x(t1) = x1, y(t1) = y1 </p>
        /// <p> - constraints: t1 ≤ projectileDuration, sqrt(x'^2+y'^2) ≤ projectileSpeed </p>
        /// <p> - No guaranteed solutions, not sure how we want to approach? </p>
        /// I'm just using a vertical offset since I CANNOT BE BOTHERED.
        /// </remarks>
        private Vector2 CalculateVelocity(Vector2 startingPos, Vector2 endingPos, Vector2 endingVel)
        {
            Vector2 diff = endingPos - startingPos;

            if (playerVelocityPrediction)
            {
                float approximateTime = (diff.magnitude) / projectileSpeed;
                endingPos += endingVel * approximateTime;
                diff = endingPos - startingPos;
            }
            Vector2 direction = diff + new Vector2(0,verticalDirOffset);
            return direction.normalized * projectileSpeed;
        }

        private void FixedUpdate()
        {
            Vector2 vel = rigidbody2D.velocity;
            transform.localEulerAngles = new Vector3(0, 0,  Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg);
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