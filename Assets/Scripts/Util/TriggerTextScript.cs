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
        protected bool isPlayerInside;
        
        private void Update()
        {
            if (isPlayerInside && Input.GetKeyDown(KeyCode.E) && CanInteract)
            {
                eventOnInteraction.Invoke();
                OnPlayerInteraction();
            }
        }

        private bool _canInteract = true;
        protected virtual bool CanInteract
        {
            get => _canInteract;
            set
            {
                _canInteract = value;
                UpdatePlayerGrapple();
            }
        }

        protected void UpdatePlayerGrapple()
        {
            if (isPlayerInside) PlayerMovementScript.Instance.CanGrapple = !CanInteract;
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
                isPlayerInside = true;
                if (notification) notification.gameObject.SetActive(true);
                PlayerMovementScript.Instance.CanGrapple = !CanInteract;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMovementScript>())
            {
                isPlayerInside = false;
                if (notification) notification.gameObject.SetActive(false);
                PlayerMovementScript.Instance.CanGrapple = true;
            }
        }
    }
}