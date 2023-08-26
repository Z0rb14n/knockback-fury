using Player;
using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(Collider2D))]
    public class BossTrigger : MonoBehaviour
    {
        [SerializeField] private BossEnemy boss;
        [SerializeField] private GameObject wallBehindPlayer;
        [SerializeField] private BossHealthBar healthBar;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMovementScript>() == null) return;
            
            Destroy(gameObject);

            boss.IsActive = true;
            wallBehindPlayer.SetActive(true);
            healthBar.gameObject.SetActive(true);
        }
    }
}