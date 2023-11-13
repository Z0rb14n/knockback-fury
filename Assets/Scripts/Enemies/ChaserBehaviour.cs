using System.Collections;
using Enemies.Ranged;
using Player;
using Polarith.AI.Move;
using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(AIMFollow), typeof(EntityHealth), typeof(AIMSteeringFilter))]
    public class ChaserBehaviour : MonoBehaviour
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

        private static AIMSteeringPerceiver GlobalSteeringPerceiver
        {
            get
            {
                if (!_globalSteeringPerceiver) _globalSteeringPerceiver = FindObjectOfType<AIMSteeringPerceiver>();
                return _globalSteeringPerceiver;
            }
        }
        private static AIMSteeringPerceiver _globalSteeringPerceiver;

        private GameObject _player;
        private EntityHealth _entityHealth;
        private AIMSteeringFilter _aimSteeringFilter;
        private AIMFollow _aimFollow;
        
        private void Awake()
        {
            _player = PlayerMovementScript.Instance.gameObject;
            _aimFollow = GetComponent<AIMFollow>();
            _aimFollow.Enabled = false;
            _aimFollow.Target = _player;
            _entityHealth = GetComponent<EntityHealth>();
            _aimSteeringFilter = GetComponent<AIMSteeringFilter>();
            if (!_aimSteeringFilter.SteeringPerceiver)
            {
                _aimSteeringFilter.SteeringPerceiver = GlobalSteeringPerceiver;
            }
        }

        private void FixedUpdate()
        {
            if (Vector2.Distance(_player.transform.position, transform.position) < explodeDistance)
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
            _entityHealth.KillNonPlayer();
            EnemyBombScript.DetonateHitPlayer(transform.position, explosionVFX, explosionDamage, explosionRadius, knockbackForce);
            Destroy(gameObject);
        }
    }
}
