using Player;
using UnityEngine;

namespace FloorGen
{
    [RequireComponent(typeof(Collider2D))]
    public class HeartPickup : MonoBehaviour
    {
        [SerializeField] private bool canOverHeal;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
            if (canOverHeal || PlayerHealth.Instance.health < PlayerHealth.Instance.maxHealth)
                PlayerHealth.Instance.health += 1;
            Destroy(gameObject);
        }
    }
}