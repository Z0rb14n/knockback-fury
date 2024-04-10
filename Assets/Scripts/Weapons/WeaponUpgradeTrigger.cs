using System.Collections.Generic;
using UnityEngine;
using Util;

namespace Weapons
{
    [DisallowMultipleComponent, RequireComponent(typeof(Collider2D))]
    public class WeaponUpgradeTrigger : TriggerTextScript
    {
        public GameObject weaponUpgradeUIPrefab;
        public HashSet<int> allowedButtons;

        protected override void OnPlayerInteraction()
        {
            GameObject go = Instantiate(weaponUpgradeUIPrefab);
            go.GetComponent<WeaponUpgradeUI>().Open(this);
        }
    }
}
