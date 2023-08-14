using Player;
using UnityEngine;

namespace FileSave
{
    /// <summary>
    /// MonoBehaviour to be attached to a Cheese pickup object.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Collider2D))]
    public class CheesePickup : MonoBehaviour
    {
        [Tooltip("Amount of Cheese given on pickup")]
        public int amount = 1;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.GetComponent<PlayerMovementScript>()) return;
            if (CrossRunInfo.Instance)
                CrossRunInfo.Instance.data.cheese += amount;
            Destroy(gameObject);
        }
    }
}