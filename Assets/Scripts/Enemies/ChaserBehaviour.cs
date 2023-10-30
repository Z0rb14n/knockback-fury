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

        private void Start()
        {
            player = PlayerMovementScript.Instance.gameObject;
        }

        private void Update()
        {
            Vector3 playerPOS = player.transform.position;
            Vector3 enemyPOS = transform.position;
            float distance = (playerPOS-enemyPOS).magnitude;

            //find distance between player and enemy

            if (distance < explodeDistance) 
            {
                StartCoroutine(Explode());   
            }

        }

        private IEnumerator Explode() 
        {  //do something before explosion delay
            yield return new WaitForSeconds(explodeDelayTime);
            //do something after explosion delay
            Destroy(gameObject);
        }
    }
}
