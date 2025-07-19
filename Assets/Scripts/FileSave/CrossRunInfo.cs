using System;
using System.Linq;
using PermUpgrade;
using Player;
using UnityEngine;

namespace FileSave
{
    /// <summary>
    /// MonoBehaviour to save/store data on application start/exit.
    /// </summary>
    [DisallowMultipleComponent]
    public class CrossRunInfo : MonoBehaviour
    {
        public SaveData data;

        #region Singleton

        public static CrossRunInfo Instance
        {
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<CrossRunInfo>();
                return _instance;
            }
        }

        private static CrossRunInfo _instance;

        #endregion

        public static bool HasUpgrade(PermUpgradeType type)
        {
            return Instance && Instance.data?.unlockedPermUpgradeTypes != null
                            && Instance.data.unlockedPermUpgradeTypes.Contains(type);
        }

        public event Action<int> OnCheeseCountChange;

        public event Action OnUpgradesChanged;

        public void ReadFromSave()
        {
            data = SaveIO.Read();
            PermUpgradeType[] allTypes = Enum.GetValues(typeof(PermUpgradeType)).Cast<PermUpgradeType>().ToArray();
            data.unlockedPermUpgradeTypes =
                data.unlockedPermanentUpgrades.Select(i => allTypes[i]).ToHashSet();
            data.unlockedWeaponSet = data.unlockedWeapons.ToHashSet();
            OnCheeseCountChange?.Invoke(data.cheese);
            OnUpgradesChanged?.Invoke();
        }

        public void WriteToSave()
        {
            data.unlockedPermanentUpgrades = data.unlockedPermUpgradeTypes != null
                ? data.unlockedPermUpgradeTypes.Cast<int>().ToArray()
                : Array.Empty<int>();

            data.unlockedWeapons = data.unlockedWeaponSet != null
                ? data.unlockedWeaponSet.ToArray()
                : Array.Empty<string>();
            SaveIO.Save(data);
        }

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Debug.LogWarning("Found two copies of CrossRunInfo: deleting this.");
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Debug.Log("[CrossRunInfo::Awake] Reading save data from " + SaveIO.saveLocation);
            ReadFromSave();
        }

        private void OnApplicationQuit()
        {
            Debug.Log("[CrossRunInfo::OnApplicationQuit] Writing Save Data to " + SaveIO.saveLocation);
            WriteToSave();
        }

        public void AddCheese(int cheese)
        {
            data.cheese += cheese;
            OnCheeseCountChange?.Invoke(cheese);
        }

        public void BuyUpgrade(PermUpgradeType type, string weaponData, int cost)
        {
            Debug.Assert(data.cheese >= cost);
            if (type != PermUpgradeType.WeaponUnlock) data.unlockedPermUpgradeTypes.Add(type);
            if (weaponData != null) data.unlockedWeaponSet.Add(weaponData);
            data.cheese -= cost;
            OnCheeseCountChange?.Invoke(-1);
            OnUpgradesChanged?.Invoke();
            if (type == PermUpgradeType.ExtraHolster)
            {
                PlayerWeaponControl.Instance.SetNewInventorySize(PlayerWeaponControl.Instance.Inventory.Length + 1);
            }
        }

        public void ClearUpgrades()
        {
            data.unlockedPermUpgradeTypes?.Clear();
            OnUpgradesChanged?.Invoke();
        }

        public void ClearUnlockedWeapons()
        {
            data.unlockedWeaponSet?.Clear();
            OnUpgradesChanged?.Invoke();
        }
    }
}