using Player;
using UnityEngine;

namespace FloorGen
{
    [DisallowMultipleComponent, RequireComponent(typeof(Collider2D))]
    public class RoomTransitionInteractable : MonoBehaviour
    {
        public FloorGenerator floorGenerator;
        public GameObject notificationOnEnter;
        
        private bool _isPlayerInside;
        
        private void Update()
        {
            if (_isPlayerInside && Input.GetKeyDown(KeyCode.E))
            {
                PlayerMovementScript.Instance.ClearPlatformsOn();
                floorGenerator.Transition();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMovementScript>())
            {
                _isPlayerInside = true;
                notificationOnEnter?.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMovementScript>())
            {
                _isPlayerInside = false;
                notificationOnEnter?.SetActive(false);
            }
        }
    }
}