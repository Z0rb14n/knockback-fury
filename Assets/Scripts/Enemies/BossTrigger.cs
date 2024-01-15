using Player;
using UnityEngine;
using UnityEngine.Events;

namespace Enemies
{
    [RequireComponent(typeof(Collider2D))]
    public class BossTrigger : MonoBehaviour
    {
        [SerializeField] private BossEnemy boss;
        [SerializeField] private GameObject wallBehindPlayer;
        [SerializeField] private BossHealthBar healthBar;
        [SerializeField] private UnityEvent eventOnTrigger;

        private void Awake()
        {
            if (healthBar) return;
            healthBar = FindObjectOfType<BossHealthBar>(true);
            Debug.Assert(healthBar, "Boss Trigger needs boss health bar.");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMovementScript>() == null) return;
            
            Destroy(gameObject);
            
            if (boss) boss.StartBoss();
            wallBehindPlayer.SetActive(true);
            healthBar.gameObject.SetActive(true);
            eventOnTrigger?.Invoke();
        }
    }
}