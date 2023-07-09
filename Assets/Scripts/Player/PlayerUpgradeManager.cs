﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Upgrades;

namespace Player
{
    [DisallowMultipleComponent]
    public class PlayerUpgradeManager : MonoBehaviour
    {
        public static PlayerUpgradeManager Instance
        {
            get
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                if (_instance == null) _instance = FindObjectOfType<PlayerUpgradeManager>();
                Debug.Assert(_instance != null);
                return _instance;
            }
        }
        private static PlayerUpgradeManager _instance;
        public PlayerUpgradeCount[] upgrades;
        [Range(0,1)]public float oneYearOfReloadPercent = 0.6f;
        [Min(0)] public float oneYearOfReloadTiming = 0.75f;

        private readonly Dictionary<UpgradeType, int> _upgradesDict = new();
        private readonly Dictionary<UpgradeType, int> _upgradesData = new();

        public int this[UpgradeType arg] => _upgradesDict.TryGetValue(arg, out int val) ? val : 0;

        public int GetData(UpgradeType arg) => _upgradesData[arg];
        
        public void PickupUpgrade(UpgradeType upgrade, int upgradeData)
        {
            if (_upgradesDict.TryGetValue(upgrade, out int value))
            {
                for (int i = 0; i < upgrades.Length; i++)
                {
                    if (upgrades[i].type != upgrade) continue;
                    upgrades[i].count = value + 1;
                    break;
                }
                _upgradesDict[upgrade] = value + 1;
            }
            else
            {
                _upgradesDict[upgrade] = 1;
                _upgradesData[upgrade] = upgradeData;
            }
        }

        private void Awake()
        {
            _instance = this;
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
            if (upgrades == null) return;
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
        public UpgradeType type;
        [Min(0), Tooltip("Number of this upgrade presently equipped")] public int count;
        [Tooltip("Unused: indicate optional comment")]
        public string comment;
        public int integerData;
    }
}