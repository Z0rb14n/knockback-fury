using System.Collections;
using System.Collections.Generic;
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

        private LayerMask _playerLayerMask;

        public override void Initialize()
        {
            if (!rigidbody2D) rigidbody2D = GetComponent<Rigidbody2D>();

            PlayerMovementScript playerMovementScript = PlayerMovementScript.Instance;
            rigidbody2D.velocity = CalculateVelocity(transform.position, playerMovementScript.transform.position, playerMovementScript.Velocity);
            StartCoroutine(DelayedExplosion());
            _playerLayerMask = LayerMask.GetMask("Player");
        }

        /// <summary>
        /// Don't. Ask. Questions. I tested it, don't worry.
        /// </summary>
        /// <returns>Of an equation Ax^4 + Bx^3 + Cx^2 + Dx + E = 0, find all roots.</returns>
        /// <remarks>
        /// Returns NaN if invalid.
        /// </remarks>
        private static List<float> QuarticZeros(float a, float b, float c, float d, float e)
        {
            // https://en.wikipedia.org/wiki/Quartic_function#General_formula_for_roots
            float p = (8 * a * c - 3 * b * b) / (8 * a * a);
            float q = (Mathf.Pow(b, 3) - 4 * a * b * c + 8 * a * a * d) / (8 * Mathf.Pow(a, 3));
            float d0 = c * c - 3 * b * d + 12 * a * e;
            float d1 = 2 * Mathf.Pow(c, 3) - 9 * b * c * d + 27 * b * b * e + 27 * a * d * d - 72 * a * c * e;
            float Q = (float)System.Math.Cbrt((d1 + Mathf.Sqrt(d1 * d1 - 4 * Mathf.Pow(d0, 3))) / 2);
            float S = Mathf.Sqrt(-2*p/3+(Q+d0/Q)/(3*a)) / 2;

            List<float> retVal = new();
            float minusB4a = -b / (4 * a);
            retVal.Add(minusB4a-S+Mathf.Sqrt(-4*S*S-2*p+q/S)/2);
            retVal.Add(minusB4a-S-Mathf.Sqrt(-4*S*S-2*p+q/S)/2);
            retVal.Add(minusB4a+S-Mathf.Sqrt(-4*S*S-2*p-q/S)/2);
            retVal.Add(minusB4a+S+Mathf.Sqrt(-4*S*S-2*p-q/S)/2);
            return retVal;
        }

        /// <summary>
        /// Calculates the velocity that we want given a starting position, ending position and target's velocity
        /// </summary>
        /// <param name="startingPos">Starting position</param>
        /// <param name="endingPos">Ending position</param>
        /// <param name="endingVel">Target's ending velocity</param>
        /// <returns>Velocity to be at, clamped to max velocity.</returns>
        private Vector2 CalculateVelocity(Vector2 startingPos, Vector2 endingPos, Vector2 endingVel)
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
             * 2(dx^2+dy^2)+2t*dot(p',Δp)=t^3*g*py'+g^2/2 * t^4 -> 4th degree polynomial, kinda annoying to find solutions to
             * g^2/2 t^4 + g*py' * t^3 - 0t^2 - 2dot(p',Δp)t - 2(sqrMag(diff)) = 0
             * Assume player velocity is 0 (disable velocity prediction):
             * 2(dx^2+dy^2) = g^2/2 * t^4 -> t = 4th root of 4(dx^2+dy^2)/g^2
             */
            
            Vector2 diff = endingPos - startingPos;
            Debug.DrawLine(startingPos, endingPos, Color.cyan, 2);
            float g = -Physics2D.gravity.y;
            
            if (endingVel.magnitude != 0 && playerVelocityPrediction)
            {
                // i f--ked up my calculus somewhere as it's 2 am
                // someone else fix it thanks
                /*
                try
                {
                    float e = -2 * diff.sqrMagnitude;
                    float d = -2 * Vector2.Dot(diff, endingVel);
                    const float c = 0;
                    float b = g*endingVel.y;
                    float a = (g * g) / 2;
                    Vector2[] vectors = QuarticZeros(a, b, c, d, e)
                        .Where(val => val <= delayBeforeDestruction)
                        .Append(delayBeforeDestruction)
                        .Select(thisT =>  new Vector2(diff.x / thisT + endingVel.x, diff.y / thisT + endingVel.y + g * thisT / 2))
                        .ToArray();

                    Vector2 best = vectors[0];
                    foreach (Vector2 vec in vectors)
                    {
                        if (vec.magnitude < best.magnitude) best = vec;
                    }
                    
                    Debug.DrawLine(startingPos, startingPos + best, Color.red, 2);
                    Debug.Log(best.magnitude);
                    return Vector2.ClampMagnitude(best, projectileSpeed);
                    
                }
                catch (System.Exception ignored)
                {
                    Debug.Log(ignored);
                    // wayyy too lazy to do 4th degree polynomial solutions here
                    float approximateTime = (diff.magnitude) / projectileSpeed;
                    endingPos += endingVel * approximateTime;
                    diff = endingPos - startingPos;
                
                    Vector2 direction = diff + new Vector2(0,verticalDirOffset);
                    return direction.normalized * projectileSpeed;
                }
                */
                
                float approximateTime = (diff.magnitude) / projectileSpeed;
                endingPos += endingVel * approximateTime;
                diff = endingPos - startingPos;
                Debug.DrawLine(startingPos, endingPos, Color.green, 2);
            }
            
            // however, the calculus for the ending velocity == 0 works
            float t = Mathf.Clamp(
                Mathf.Sqrt(Mathf.Sqrt(4 / (g*g) * diff.sqrMagnitude)),0.001f,delayBeforeDestruction);
            Vector2 numericalSolution = new(diff.x / t, diff.y / t + g * t / 2);
            Debug.DrawLine(startingPos, startingPos + numericalSolution, Color.red, 2);
            //Debug.Log(numericalSolution.magnitude);
            return Vector2.ClampMagnitude(numericalSolution, projectileSpeed);
        }

        private void FixedUpdate()
        {
            Vector2 vel = rigidbody2D.velocity;
            if (vel.magnitude >= 0.001f) transform.localEulerAngles = new Vector3(0, 0,  Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg);
        }

        public void Detonate(bool playerCaused)
        {
            Vector3 pos = transform.position;
            GameObject explosionObject = Instantiate(explosionVFX, pos, Quaternion.identity);
            explosionObject.GetComponent<ExplosionVFX>().SetSize(radius);

            Collider2D playerCollider = Physics2D.OverlapCircle(pos, radius, _playerLayerMask);
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
            bool prev = Physics2D.queriesHitTriggers;
            Physics2D.queriesHitTriggers = false;
            // ReSharper disable once Unity.PreferNonAllocApi
            Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, radius);
            foreach (Collider2D col in colliders)
            {
                if (col.isTrigger) continue;
                EntityHealth health = col.GetComponent<EntityHealth>();
                Debug.DrawLine(col.ClosestPoint(pos), pos, Color.cyan, 1);
                Debug.Log((col.transform.position - pos).magnitude);
                if (health && health != PlayerHealth.Instance)
                {
                    health.TakeDamage(playerDamage);
                }
            }

            Physics2D.queriesHitTriggers = prev;
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