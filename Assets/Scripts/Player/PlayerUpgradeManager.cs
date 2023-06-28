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

        public int this[PlayerUpgradeType arg] => _upgradesDict[arg];

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
            for (int i = 0; i < upgrades.Length; i++)
            {
                _upgradesDict[upgrades[i].type] = upgrades[i].count;
            }
        }
    }

    [Serializable]
    public struct PlayerUpgradeCount
    {
        public PlayerUpgradeType type;
        [Min(0)]
        public int count;
    }
}