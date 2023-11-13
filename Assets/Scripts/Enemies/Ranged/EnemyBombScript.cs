using System.Collections;
using Player;
using UnityEngine;
using Upgrades;
using Weapons;
using FMODUnity;
using FMOD.Studio;

namespace Enemies.Ranged
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class EnemyBombScript : EnemyBulletScript
    {
        public GameObject explosionVFX;
        public float radius = 1;
        public int playerDamage = 100;
        public bool playerVelocityPrediction = true;

        private bool _hitByPlayer;
        private int _projectileLayer;
        private IEnumerator _detonationCoroutine;

        [SerializeField] private EventReference _bombSound;
        [SerializeField] private EventReference _fuseSound;
        private EventInstance _fuseSFX;

        public override void Initialize(float damageMult)
        {
            damageMultiplier = damageMult;
            if (!rigidbody2D) rigidbody2D = GetComponent<Rigidbody2D>();

            PlayerMovementScript playerMovementScript = PlayerMovementScript.Instance;
            if (playerVelocityPrediction)
            {
                rigidbody2D.velocity = CalculateVelocity(transform.position, playerMovementScript.transform.position,
                    playerMovementScript.Velocity, projectileSpeed, delayBeforeDestruction);
            }
            else
            {
                rigidbody2D.velocity = CalculateVelocity(transform.position, playerMovementScript.transform.position,
                    projectileSpeed, delayBeforeDestruction);
            }

            _detonationCoroutine = DelayedExplosion();
            StartCoroutine(_detonationCoroutine);
            _projectileLayer = LayerMask.NameToLayer("Projectile");
            _fuseSFX = RuntimeManager.CreateInstance(_fuseSound);
            _fuseSFX.start();
            _fuseSFX.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject, rigidbody2D));
        }


        /// <summary>
        /// Calculates the velocity that we want given a starting position, ending position and target's velocity
        /// </summary>
        /// <param name="startingPos">Starting position</param>
        /// <param name="endingPos">Ending position</param>
        /// <param name="maxSpeed">Max projectile speed</param>
        /// <param name="maxTime">Max time</param>
        /// <param name="g">Custom gravity: NaN if use global</param>
        /// <returns>Velocity to be at, clamped to max velocity.</returns>
        public static Vector2 CalculateVelocity(Vector2 startingPos, Vector2 endingPos, float maxSpeed, float maxTime, float g = float.NaN)
        {
            Vector2 diff = endingPos - startingPos;
            Debug.DrawLine(startingPos, endingPos, Color.cyan, 2);
            if (float.IsNaN(g)) g = -Physics2D.gravity.y;

            /*
             * Numerical solution for minimizing squared velocity given ending target is stationary.
             *
             * Moving target requires a quartic polynomial solution, and that doesn't work as floating points don't
             * have complex number support.
             */
            float t = Mathf.Clamp(Mathf.Sqrt(Mathf.Sqrt(4 / (g * g) * diff.sqrMagnitude)), 0.001f, maxTime);
            Vector2 numericalSolution = new(diff.x / t, diff.y / t + g * t / 2);
            Debug.DrawLine(startingPos, startingPos + numericalSolution, Color.red, 2);
            //Debug.Log(numericalSolution.magnitude);
            return Vector2.ClampMagnitude(numericalSolution, maxSpeed);
        }

        /// <summary>
        /// Calculates the velocity that we want given a starting position, ending position and target's velocity
        /// </summary>
        /// <param name="startingPos">Starting position</param>
        /// <param name="endingPos">Ending position</param>
        /// <param name="endingVel">Target's ending velocity</param>
        /// <param name="maxSpeed">Max projectile speed</param>
        /// <param name="maxTime">Max time</param>
        /// <param name="g">Custom gravity: NaN if use global</param>
        /// <returns>Velocity to be at, clamped to max velocity.</returns>
        public static Vector2 CalculateVelocity(Vector2 startingPos, Vector2 endingPos, Vector2 endingVel,
            float maxSpeed, float maxTime, float g = float.NaN)
        {
            if (float.IsNaN(g)) g = -Physics2D.gravity.y;
            Vector2 diff = endingPos - startingPos;
            // approximate new position by just increasing by velocity slightly
            // actual numerical solution is too complicated
            float approximateTime = (diff.magnitude) / maxSpeed;
            endingPos += endingVel * approximateTime;
            Debug.DrawLine(startingPos, endingPos, Color.green, 2);
            return CalculateVelocity(startingPos, endingPos, maxSpeed, maxTime, g);
        }

        private void FixedUpdate()
        {
            Vector2 vel = rigidbody2D.velocity;
            if (vel.magnitude >= 0.001f)
                transform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg);
        }

        public void OnHitByPlayer()
        {
            if (!_hitByPlayer && PlayerUpgradeManager.Instance[UpgradeType.TossBack] > 0)
            {
                rigidbody2D.velocity *= -1;
                gameObject.layer = _projectileLayer;
                StopCoroutine(_detonationCoroutine);
                _detonationCoroutine = DelayedExplosion();
                StartCoroutine(_detonationCoroutine);
            }

            _hitByPlayer = true;

            if (PlayerUpgradeManager.Instance[UpgradeType.TossBack] <= 0) Detonate(true);
        }

        public static void DetonateHitPlayer(Vector3 pos, GameObject vfx, int damage, float radius,
            float knockbackForce)
        {
            GameObject explosionObject = Instantiate(vfx, pos, Quaternion.identity);
            explosionObject.GetComponent<ExplosionVFX>().SetSize(radius);

            Collider2D playerCollider = Physics2D.OverlapCircle(pos, radius, LayerMask.GetMask("Player"));
            if (playerCollider)
            {
                PlayerMovementScript playerMovement = PlayerMovementScript.Instance;
                EntityHealth playerHealth = PlayerHealth.Instance;
                playerHealth.TakeDamage(damage);
                Vector2 knockbackDirection = new((playerMovement.transform.position - pos).normalized.x * 0.1f, 0.04f);
                playerMovement.RequestKnockback(knockbackDirection, knockbackForce);
            }
        }

        private void Detonate(bool playerCaused)
        {
            Vector3 pos = transform.position;
            RuntimeManager.PlayOneShot(_bombSound, pos);
            DetonateHitPlayer(pos, explosionVFX, bulletDamage, radius, knockbackForce);
            _fuseSFX.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            Destroy(gameObject);

            if (!playerCaused) return;
            bool prev = Physics2D.queriesHitTriggers;
            Physics2D.queriesHitTriggers = false;
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

            Physics2D.queriesHitTriggers = prev;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.GetComponent<PlayerMovementScript>() ||
                (_hitByPlayer && other.collider.GetComponent<EntityHealth>()))
            {
                Detonate(_hitByPlayer);
            }
        }

        private IEnumerator DelayedExplosion()
        {
            yield return new WaitForSeconds(delayBeforeDestruction);
            Detonate(_hitByPlayer);
        }
    }
}