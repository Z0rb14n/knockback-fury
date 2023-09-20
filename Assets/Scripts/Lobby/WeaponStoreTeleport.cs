using System;
using Player;
using UnityEngine;
using Util;

namespace Lobby
{
    public class WeaponStoreTeleport : TriggerTextScript
    {
        [SerializeField] private Vector3 positionToTeleport;
        protected override void OnPlayerInteraction()
        {
            PlayerMovementScript.Instance.transform.position = positionToTeleport;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(positionToTeleport,0.5f);
        }
    }
}