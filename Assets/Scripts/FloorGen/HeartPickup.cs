using Player;
using UnityEngine;

namespace FloorGen
{
    [RequireComponent(typeof(Collider2D))]
    public class HeartPickup : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
            PlayerHealth.Instance.health += 1;
            Destroy(gameObject);
        }
    }
}