using System.Collections;
using Player;
using UnityEngine;

namespace Enemies.Ranged
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyBulletScript : MonoBehaviour
    {
        public float force;
        public int bulletDamage;
        public int knockbackForce;
        public float delayBeforeDestruction = 10;

        private Rigidbody2D _rb;
        private LayerMask _layerMask;
        private float _timer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _layerMask = LayerMask.NameToLayer("Player");
        }

        public void Initialize()
        {
            if (!_rb) _rb = GetComponent<Rigidbody2D>();
            
            Vector3 direction = PlayerMovementScript.Instance.transform.position - transform.position;
            _rb.velocity = direction.normalized * force;
            float rot = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.localEulerAngles = new Vector3(0, 0, rot);
            StartCoroutine(DestroyCoroutine());
        }

        private IEnumerator DestroyCoroutine()
        {
            yield return new WaitForSeconds(delayBeforeDestruction);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((other.gameObject.layer & _layerMask) != 0)
            {
                EntityHealth playerHealth = other.gameObject.GetComponent<EntityHealth>();
                PlayerMovementScript playerMovement = other.gameObject.GetComponent<PlayerMovementScript>();

                playerHealth.TakeDamage(bulletDamage);
                Vector2 knockbackDirection = new((other.transform.position - transform.position).normalized.x * 0.1f, 0.04f);
                playerMovement.RequestKnockback(knockbackDirection, knockbackForce);

                Destroy(gameObject);
            }
        }
    }
}
