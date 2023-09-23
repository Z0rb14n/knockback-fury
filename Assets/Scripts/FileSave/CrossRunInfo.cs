using System;
using System.Linq;
using PermUpgrade;
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
                if (_instance == null) _instance = FindObjectOfType<CrossRunInfo>();
                return _instance;
            }
        }
        private static CrossRunInfo _instance;
        #endregion

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
            
            data.unlockedWeapons = data.unlockedWeaponSet != null ?
                data.unlockedWeaponSet.ToArray()
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
            OnUpgradesChanged?.Invoke();
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