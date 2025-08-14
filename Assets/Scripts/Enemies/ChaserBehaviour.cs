using System.Collections;
using Enemies.Ranged;
using Player;
using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(EntityHealth), typeof(Rigidbody2D))]
    public class ChaserBehaviour : EnemyBehaviour
    {
        [Tooltip("VFX to create on explosion")]
        public GameObject explosionVFX;
        [Min(0), Tooltip("Explosion radius")]
        public float explosionRadius = 1;
        [Min(0), Tooltip("Damage of explosion")]
        public int explosionDamage = 1;
        [Min(0), Tooltip("Explosion knockback force")]
        public float knockbackForce = 10;
        [Min(0), Tooltip("Distance before explosion timer starts")]
        public float explodeDistance;
        [Min(0), Tooltip("Delay before explosion occurs")]
        public float explodeDelayTime;
        [Min(0), Tooltip("Speed, m/s")]
        public float speed = 1;

        private EntityHealth _entityHealth;
        private Rigidbody2D _rigidbody2D;
        private PlayerMovementScript _player;
        private bool _isExploding;
        
        private void Awake()
        {
            _player = PlayerMovementScript.Instance;
            _entityHealth = GetComponent<EntityHealth>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            Vector2 pos = _rigidbody2D.position;
            Vector2 playerPos = _player.Pos;
            Vector2 targetDir = playerPos - pos;
            if (!_isExploding && Vector2.SqrMagnitude(targetDir) < explodeDistance * explodeDistance)
            {
                _isExploding = true;
                StartCoroutine(Explode());
            }
            _rigidbody2D.MovePosition(pos + Vector2.ClampMagnitude(targetDir, speed * Time.fixedDeltaTime));
            _rigidbody2D.SetRotation(Mathf.Acos(Vector2.Dot(Vector2.up, targetDir.normalized))
                                     * Mathf.Rad2Deg
                                     * -Mathf.Sign(targetDir.x));
        }

        private IEnumerator Explode() 
        {
            //do something before explosion delay
            yield return new WaitForSeconds(explodeDelayTime);
            _entityHealth.KillNonPlayer();
            EnemyBombScript.DetonateHitPlayer(transform.position, explosionVFX, explosionDamage, explosionRadius, knockbackForce, null);
            Destroy(gameObject);
        }
    }
}
