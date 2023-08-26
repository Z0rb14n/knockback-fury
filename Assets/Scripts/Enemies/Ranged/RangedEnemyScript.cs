using Player;
using UnityEngine;

namespace Enemies.Ranged
{
    public class RangedEnemyScript : MonoBehaviour
    {
        public GameObject bullet;
        public Transform bulletPos;

        private float timer;
        private PlayerMovementScript _playerMovement;
        
        private void Awake()
        {
            _playerMovement = FindObjectOfType<PlayerMovementScript>();
        }

        // Update is called once per frame
        void Update()
        {
            float distance = Vector2.Distance(transform.position, _playerMovement.transform.position);

            if (distance < 4)
            {
                timer += Time.deltaTime;

                if (timer > 2)
                {
                    timer = 0;
                    shoot();
                }
            }
        }

        // instantiates a bullet at the position of the ranged enemy
        void shoot()
        {
            Instantiate(bullet, bulletPos.position, Quaternion.identity);
        }
    }
}
