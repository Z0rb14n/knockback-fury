using System.Collections;
using Player;
using Polarith.AI.Move;
using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(AIMFollow))]
    public class ChaserBehaviour : MonoBehaviour
    {
        private GameObject player;
        private AIMFollow _aimFollow;
        public float explodeDistance;
        public float explodeDelayTime;

        private void Awake()
        {
            player = PlayerMovementScript.Instance.gameObject;
            _aimFollow = GetComponent<AIMFollow>();
            _aimFollow.Enabled = false;
            _aimFollow.Target = player;
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
            //do something after explosion delay
            Destroy(gameObject);
        }
    }
}
