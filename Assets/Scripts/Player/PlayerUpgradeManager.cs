using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [DisallowMultipleComponent]
    public class PlayerUpgradeManager : MonoBehaviour
    {
        public PlayerUpgradeCount[] upgrades;

        private readonly Dictionary<PlayerUpgradeType, int> _upgradesDict = new();
        private readonly Dictionary<PlayerUpgradeType, int> _upgradesData = new();

        public int this[PlayerUpgradeType arg] => _upgradesDict.TryGetValue(arg, out int val) ? val : 0;

        public int GetData(PlayerUpgradeType arg) => _upgradesData[arg];

        private void Awake()
        {
            BuildDict();
        }

        private void OnValidate()
        {
            BuildDict();
        }

        private void BuildDict()
        {
            _upgradesDict.Clear();
            _upgradesData.Clear();
            for (int i = 0; i < upgrades.Length; i++)
            {
                _upgradesDict[upgrades[i].type] = upgrades[i].count;
                _upgradesData[upgrades[i].type] = upgrades[i].integerData;
            }
        }
    }

    [Serializable]
    public struct PlayerUpgradeCount
    {
        public PlayerUpgradeType type;
        [Min(0), Tooltip("Number of this upgrade presently equipped")] public int count;
        [Tooltip("Unused: indicate optional comment")]
        public string comment;
        public int integerData;
    }
}