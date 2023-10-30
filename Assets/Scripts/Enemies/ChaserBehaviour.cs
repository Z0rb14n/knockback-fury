using System.Collections;
using Player;
using UnityEngine;

namespace Enemies
{
    public class ChaserBehaviour : MonoBehaviour
    {
        private GameObject player;
        public float explodeDistance;
        public float explodeDelayTime;

        private void Awake()
        {
            player = PlayerMovementScript.Instance.gameObject;
        }

        private void FixedUpdate()
        {
            if (Vector2.Distance(player.transform.position, transform.position) < explodeDistance)
            {
                StartCoroutine(Explode());   
            }
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
