using System.Collections;
using Player;
using Polarith.AI.Move;
using UnityEngine;
using Weapons;

namespace Enemies
{
    [RequireComponent(typeof(AIMFollow))]
    public class ChaserBehaviour : MonoBehaviour
    {
        public GameObject explosionVFX;
        [Min(0)]
        public float explosionRadius = 1;
        [Min(0)]
        public int explosionDamage = 1;
        [Min(0)]
        public float knockbackForce = 10;
        [Min(0)]
        public float explodeDistance;
        [Min(0)]
        public float explodeDelayTime;

        private GameObject player;
        private AIMFollow _aimFollow;
        private LayerMask _playerLayerMask;
        
        private void Awake()
        {
            player = PlayerMovementScript.Instance.gameObject;
            _aimFollow = GetComponent<AIMFollow>();
            _aimFollow.Enabled = false;
            _aimFollow.Target = player;
            _playerLayerMask = LayerMask.GetMask("Player");
        }

        private void FixedUpdate()
        {
            if (Vector2.Distance(player.transform.position, transform.position) < explodeDistance)
            {
                StartCoroutine(Explode());   
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.GetComponent<PlayerMovementScript>()) return;
            _aimFollow.Enabled = true;
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.GetComponent<PlayerMovementScript>()) return;
            _aimFollow.Enabled = false;
        }

        private IEnumerator Explode() 
        {
            //do something before explosion delay
            yield return new WaitForSeconds(explodeDelayTime);
            Vector3 pos = transform.position;
            GameObject explosionObject = Instantiate(explosionVFX, pos, Quaternion.identity);
            explosionObject.GetComponent<ExplosionVFX>().SetSize(explosionRadius);

            Collider2D playerCollider = Physics2D.OverlapCircle(pos, explosionRadius, _playerLayerMask);
            if (playerCollider)
            {
                PlayerMovementScript playerMovement = PlayerMovementScript.Instance;
                EntityHealth playerHealth = PlayerHealth.Instance;
                playerHealth.TakeDamage(explosionDamage);
                Vector2 knockbackDirection = new((playerMovement.transform.position - pos).normalized.x * 0.1f, 0.04f);
                playerMovement.RequestKnockback(knockbackDirection, knockbackForce);
            }
            Destroy(gameObject);
        }
    }
}
