using System.Collections.Generic;
using UnityEngine;
using Util;

namespace Weapons
{
    [DisallowMultipleComponent, RequireComponent(typeof(Collider2D))]
    public class WeaponUpgradeTrigger : TriggerTextScript
    {
        public WeaponUpgradeUI weaponUpgradeUI;
        public HashSet<int> allowedButtons;

        private void Awake()
        {
            if (!weaponUpgradeUI) weaponUpgradeUI = FindAnyObjectByType<WeaponUpgradeUI>();
        }

        protected override void OnPlayerInteraction()
        {
            weaponUpgradeUI.Open(this);
        }
    }
}
