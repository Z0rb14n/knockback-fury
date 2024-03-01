using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Player;
using UnityEngine;
using Upgrades;
using Weapons;
using FMODUnity;
using FMOD.Studio;
using Util;
using Complex = System.Numerics.Complex;

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
                    new Vector2(0,10), projectileSpeed, delayBeforeDestruction);
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
        /// Don't. Ask. Questions. I tested it, don't worry.
        /// </summary>
        /// <returns>Of an equation Ax^4 + Bx^3 + Cx^2 + Dx + E = 0, find all roots.</returns>
        private static List<Complex> QuarticZeros(float a, float b, float c, float d, float e)
        {
            // https://en.wikipedia.org/wiki/Quartic_function#General_formula_for_roots
            float p = (8 * a * c - 3 * b * b) / (8 * a * a);
            float q = (Mathf.Pow(b, 3) - 4 * a * b * c + 8 * a * a * d) / (8 * Mathf.Pow(a, 3));
            float d0 = c * c - 3 * b * d + 12 * a * e;
            float d1 = 2 * Mathf.Pow(c, 3) - 9 * b * c * d + 27 * b * b * e + 27 * a * d * d - 72 * a * c * e;
            Complex Q = Complex.Pow((d1 + Complex.Sqrt(d1 * d1 - 4 * Mathf.Pow(d0, 3))) / 2, 1 / 3.0);
            Complex S = Complex.Sqrt(-2*p/3+(Q+d0/Q)/(3*a)) / 2;

            float minusB4a = -b / (4 * a);
            List<Complex> retVal = new();
            retVal.Add(minusB4a-S+Complex.Sqrt(-4*S*S-2*p+q/S)/2);
            retVal.Add(minusB4a-S-Complex.Sqrt(-4*S*S-2*p+q/S)/2);
            retVal.Add(minusB4a+S-Complex.Sqrt(-4*S*S-2*p-q/S)/2);
            retVal.Add(minusB4a+S+Complex.Sqrt(-4*S*S-2*p-q/S)/2);
            return retVal;
        }


        /// <summary>
        /// Calculates the velocity that we want given a starting position and ending position.
        /// </summary>
        /// <param name="startingPos">Starting position</param>
        /// <param name="endingPos">Ending position</param>
        /// <param name="maxSpeed">Max projectile speed</param>
        /// <param name="maxTime">Max time</param>
        /// <param name="g">Custom gravity: NaN if use global</param>
        /// <returns>Velocity to be at, clamped to max velocity.</returns>
        /// <seealso cref="CalculateVelocity(UnityEngine.Vector2,UnityEngine.Vector2,UnityEngine.Vector2,float,float,float)"/>
        public static Vector2 CalculateVelocity(Vector2 startingPos, Vector2 endingPos, float maxSpeed, float maxTime, float g = float.NaN)
        {
            Vector2 diff = endingPos - startingPos;
            Debug.DrawLine(startingPos, endingPos, Color.cyan, 2);
            if (float.IsNaN(g)) g = -Physics2D.gravity.y;

            /*
             * Numerical solution for minimizing squared velocity given ending target is stationary.
             *
             * Moving target requires a quartic polynomial solution.
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
            /*
             * Unconstrained case has 2 equations, 3 unknowns;
             * let (x0,y0) be position of enemy, (x1,y1) be position of target
             * let <x', y'> be initial velocity vector, g being gravity, t1 = time to impact
             * Let <px', py'> be player velocity vector
             * x(t) + px'*t = x0 + x'*t (assumes player velocity constant; could use forward differences, but we want it to be *dodge-able*)
             * y(t) + py'*t = y0 + y't - gt^2/2
             * Solve for t1, x', y' where x(t1) + px'*t1 = x1, y(t1) + py'*t1 = y1
             * Constraints: t1 ≤ projectileDuration, sqrt(x'^2+y'^2) ≤ projectileSpeed
             * No guaranteed solutions, instead trying to minimize velocity magnitude, i.e. sqrt(x'^2 + y'^2)
             * since sqrt(x) is monotonic, can instead minimize x'^2 + y'^2 w.r.t. t
             * deriv. w.r.t t = 2x'(dx'/dt) + 2y'(dy'/dt); note t != 0, but we need to check t = projectileDuration (boundary case)
             * Let Δx = x1 - x0, Δy = y1 - y0
             * x' = Δx/t + px' ; dx'/dt = -Δx/t^2
             * y' = Δy/t + py' + gt/2; dy'/dt = -Δy/t^2 + g/2
             * Solve 0 = 2(Δx/t + px')(-Δx/t^2) + 2(Δy/t + py' + gt/2)(-Δy/t^2 + g/2)
             * 0 = -2Δx^2/t^3 - 2Δx * px'/t^2 - 2Δy^2/t^3 - 2py'*Δy/t^2-gt*Δy/t^2+g*Δy/t+g*py'+g^2*t/2
             * ...
             * 2(dx^2+dy^2)+2t*dot(p',Δp)=t^3*g*py'+g^2/2 * t^4 -> 4th degree polynomial
             * g^2/2 t^4 + g*py' * t^3 - 0t^2 - 2dot(p',Δp)t - 2(sqrMag(diff)) = 0
             *
             * Assume player velocity is 0 (disable velocity prediction):
             * 2(dx^2+dy^2) = g^2/2 * t^4 -> t = 4th root of 4(dx^2+dy^2)/g^2
             */
            
            Vector2 diff = endingPos - startingPos;
            Debug.DrawLine(startingPos, endingPos, Color.cyan, 2);
            if (float.IsNaN(g)) g = -Physics2D.gravity.y;
            
            if (endingVel.magnitude < 0.1f) return CalculateVelocity(startingPos, endingPos, maxSpeed, maxTime, g);
            float e = -2 * diff.sqrMagnitude;
            float d = -2 * Vector2.Dot(diff, endingVel);
            const float c = 0;
            float b = g*endingVel.y;
            float a = (g * g) / 2;
            (Vector2, float) vec = QuarticZeros(a, b, c, d, e)
                .Where(zero => Mathf.Abs((float)zero.Imaginary) <= 0.001f)
                .Select(zero => (float)zero.Real)
                .Where(val => val >= 0)
                .Where(val => val <= maxTime)
                .Select(thisT => (new Vector2(diff.x / thisT + endingVel.x,
                    diff.y / thisT + endingVel.y + g * thisT / 2), thisT))
                .Select(pair => (pair.Item1, pair.thisT, pair.Item1.magnitude))
                .OrderBy(pair => pair.magnitude)
                .Select(pair => (pair.Item1, pair.thisT))
                .DefaultIfEmpty((Vector2.zero, -1))
                .FirstOrDefault();
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (vec.Item2 == -1)
            {
                Debug.LogWarning("No solutions to Quartic polynomial.");
                float approximateTime = (diff.magnitude) / maxSpeed;
                endingPos += endingVel * approximateTime;
                Debug.DrawLine(startingPos, endingPos, Color.green, 2);
                return CalculateVelocity(startingPos, endingPos, maxSpeed, maxTime, g);
            }

            Vector2 newVel = vec.Item1;
            float time = vec.Item2;
            Debug.DrawLine(startingPos, startingPos + newVel, Color.red, 2);
            Vector2 collision = new(startingPos.x + newVel.x * time,
                startingPos.y + newVel.y * time - g * time * time / 2);
            DebugUtil.DrawCircle(collision, 1, 8, Color.cyan, 4);
            Debug.Log(newVel.magnitude);
            return Vector2.ClampMagnitude(newVel, maxSpeed);
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