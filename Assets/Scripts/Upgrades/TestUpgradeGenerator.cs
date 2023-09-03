using System;
using System.Linq;
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
            Debug.Assert(UpgradeManager.Instance.UpgradeMapping.Count == Enum.GetValues(typeof(UpgradeType)).Length);
            (UpgradeType, UpgradePickupData)[] upgrades = UpgradeManager.Instance.UpgradeMapping
                .Select(pair => (pair.Key, pair.Value)).ToArray();
            for (int i = 0; i < upgrades.Length; i++)
            {
                Vector3 pos = Vector3.Lerp(left, right, (float)i / (upgrades.Length-1));
                GameObject go = Instantiate(upgradePrefab, pos, Quaternion.identity, transform);
                UpgradePickup pickup = go.GetComponent<UpgradePickup>();
                upgrades[i].Item2.Set(pickup, false);
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