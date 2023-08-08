using Player;
using UnityEngine;

namespace Weapons
{
    [DisallowMultipleComponent, RequireComponent(typeof(Collider2D))]
    public class WeaponUpgradeTrigger : MonoBehaviour
    {
        public WeaponUpgradeUI weaponUpgradeUI;
        public GameObject notificationOnEnter;
        
        private bool _isPlayerInside;

        private void Update()
        {
            if (_isPlayerInside && Input.GetKeyDown(KeyCode.E))
            {
                weaponUpgradeUI.Open(this);
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
