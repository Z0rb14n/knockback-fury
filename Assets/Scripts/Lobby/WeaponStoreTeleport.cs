using Player;
using UnityEngine;
using Util;

namespace Lobby
{
    public class WeaponStoreTeleport : TriggerTextScript
    {
        [SerializeField] private Vector3 positionToTeleport;
        [SerializeField] private bool isTeleportEnabled = true;
        [SerializeField] private GameObject objectToDisable;

        private void Awake()
        {
            if (objectToDisable)
            {
                objectToDisable.SetActive(!isTeleportEnabled);
                notification.enabled = isTeleportEnabled;
            }
        }

        protected override void OnPlayerInteraction()
        {
            if (isTeleportEnabled) PlayerMovementScript.Instance.transform.position = positionToTeleport;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(positionToTeleport,0.5f);
        }

        private void OnValidate()
        {
            if (objectToDisable) objectToDisable.SetActive(!isTeleportEnabled);
            notification.enabled = isTeleportEnabled;
        }
    }
}