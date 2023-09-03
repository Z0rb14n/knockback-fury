using System.Collections.Generic;
using UnityEngine;

namespace Upgrades
{
    /// <summary>
    /// Manager to handle all upgrade pickup data.
    /// </summary>
    [DisallowMultipleComponent]
    public class UpgradeManager : MonoBehaviour
    {
        [Tooltip("Please just drag and drop this in from the Upgrades folder.")]
        public UpgradePickupData[] allData;
        
        public static UpgradeManager Instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<UpgradeManager>();
                return _instance;
            }
        }
        private static UpgradeManager _instance;

        /// <summary>
        /// Mapping of upgrade type to pickup data objects.
        /// </summary>
        public Dictionary<UpgradeType, UpgradePickupData> UpgradeMapping { get; } = new();

        /// <summary>
        /// Set of all implemented upgrade types.
        /// </summary>
        public HashSet<UpgradeType> ImplementedUpgrades { get; } = new();

        private void Awake()
        {
            foreach (UpgradePickupData data in allData)
            {
                if (UpgradeMapping.ContainsKey(data.upgradeType))
                {
                    Debug.LogWarning("Duplicate Data Keys: " + data.upgradeType);
                }

                UpgradeMapping[data.upgradeType] = data;
                if (data.implemented) ImplementedUpgrades.Add(data.upgradeType);
            }
        }
    }
}