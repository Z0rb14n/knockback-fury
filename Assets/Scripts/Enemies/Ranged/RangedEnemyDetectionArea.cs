using Player;
using UnityEngine;

namespace Enemies.Ranged
{
    /// <summary>
    /// Behaviour for the RangedEnemyDetectionArea.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RangedEnemyDetectionArea : MonoBehaviour
    {
        private RangedEnemyScript _rangedEnemyScript;

        private void Awake()
        {
            // maybe we just assign this?
            _rangedEnemyScript = GetComponentInParent<RangedEnemyScript>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // FIX: only trigger if triggerCheck is enabled
            if (!other.GetComponent<PlayerMovementScript>()) return;
            _rangedEnemyScript.OnDetectionAreaEnter();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.GetComponent<PlayerMovementScript>()) return;
            _rangedEnemyScript.OnDetectionAreaExit();
        }
    }
}
