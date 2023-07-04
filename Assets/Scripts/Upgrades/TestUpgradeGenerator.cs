using System;
using UnityEngine;

namespace Upgrades
{
    public class TestUpgradeGenerator : MonoBehaviour
    {
        public GameObject upgradePrefab;

        public Vector3 left;
        public Vector3 right;

        public void Generate()
        {
            Array array = Enum.GetValues(typeof(UpgradeType));
            int count = array.Length;
            int cloakAndDaggerDamage = 500;
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = Vector3.Lerp(left, right, (float)i / (count-1));
                GameObject go = Instantiate(upgradePrefab, pos, Quaternion.identity, transform);
                UpgradePickup pickup = go.GetComponent<UpgradePickup>();
                pickup.upgrade = (UpgradeType) array.GetValue(i);
                if (pickup.upgrade == UpgradeType.CloakAndDagger) pickup.upgradeData = cloakAndDaggerDamage;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Array array = Enum.GetValues(typeof(UpgradeType));
            int count = array.Length;
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = Vector3.Lerp(left, right, (float)i / (count-1));
                Gizmos.DrawWireSphere(pos, 0.5f);
            }
        }
    }
}