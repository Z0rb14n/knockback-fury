using FileSave;
using PermUpgrade;
using Player;
using UnityEngine;
using Util;

namespace Lobby
{
    /// <summary>
    /// In the lobby, a teleport object for the weapon store.
    /// </summary>
    /// <remarks>
    /// If other things can use this, rename the file.
    /// </remarks>
    public class WeaponStoreTeleport : TriggerTextScript
    {
        [SerializeField] private Vector3 positionToTeleport;
        [SerializeField] private bool isTeleportEnabled = true;
        [SerializeField] private bool teleportDependsOnUpgrade;
        [SerializeField] private GameObject objectToDisable;

        private void Awake()
        {
            if (teleportDependsOnUpgrade)
            {
                CrossRunInfo.Instance.OnUpgradesChanged += OnUpgradesChanged;
                isTeleportEnabled = CrossRunInfo.HasUpgrade(PermUpgradeType.PrepperRat);
            }
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

        private void OnUpgradesChanged()
        {
            if (teleportDependsOnUpgrade)
            {
                isTeleportEnabled = CrossRunInfo.HasUpgrade(PermUpgradeType.PrepperRat);
            }
            if (objectToDisable)
            {
                objectToDisable.SetActive(!isTeleportEnabled);
                notification.enabled = isTeleportEnabled;
            }
        }

        private void OnDestroy()
        {
            if (CrossRunInfo.Instance) CrossRunInfo.Instance.OnUpgradesChanged -= OnUpgradesChanged;
        }

        private void OnValidate()
        {
            if (teleportDependsOnUpgrade)
            {
                isTeleportEnabled = CrossRunInfo.HasUpgrade(PermUpgradeType.PrepperRat);
            }
            if (objectToDisable) objectToDisable.SetActive(!isTeleportEnabled);
            notification.enabled = isTeleportEnabled;
        }
    }
}