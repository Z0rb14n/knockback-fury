using Player;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Util
{
    /// <summary>
    /// Simple script to allow text to be displayed when a player enters a trigger.
    ///
    /// Also handles player input.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TriggerTextScript : MonoBehaviour
    {
        [SerializeField, Tooltip("Text notification to be displayed")] protected TextMeshPro notification;

        [SerializeField, Tooltip("Event triggered on interaction")] protected UnityEvent eventOnInteraction;
        private bool _isPlayerInside;
        
        private void Update()
        {
            if (_isPlayerInside && Input.GetKeyDown(KeyCode.E))
            {
                eventOnInteraction.Invoke();
                OnPlayerInteraction();
            }
        }

        /// <summary>
        /// Called when the player interacts with this object while the player is inside.
        /// </summary>
        protected virtual void OnPlayerInteraction()
        {
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMovementScript>())
            {
                _isPlayerInside = true;
                notification.gameObject.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMovementScript>())
            {
                _isPlayerInside = false;
                notification.gameObject.SetActive(false);
            }
        }
    }
}