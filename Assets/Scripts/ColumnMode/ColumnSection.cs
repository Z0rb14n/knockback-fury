using System.Collections.Generic;
using System.Linq;
using FileSave;
using PermUpgrade;
using Player;
using UnityEngine;
using Upgrades;
using Util;
using Weapons;

namespace ColumnMode
{
    [DisallowMultipleComponent]
    public class ColumnSection : MonoBehaviour
    {
        [SerializeField]
        public float height;

        [SerializeField]
        public WeaponData[] weaponsList;
        [SerializeField] private bool hasWeapon;
        [SerializeField] private bool hasWeaponUpgrade;
        [SerializeField] private bool hasPlayerUpgrade;

        public void Initialize()
        {
            if (hasWeapon)
            {
                WeaponPickup pickup = GetComponentInChildren<WeaponPickup>();
                if (pickup)
                {
                    HashSet<string> playerCurrInventory =
                        PlayerWeaponControl.Instance.Inventory.Where(data => data).Select(data => data.weaponName).ToHashSet();
                    List<WeaponData> eligibleWeapons =
                        weaponsList.Where(weapon => !playerCurrInventory.Contains(weapon.weaponName))
                            .Where(weapon =>
                                // ReSharper disable once Unity.NoNullPropagation
                                weapon.unlockedByDefault || (CrossRunInfo.Instance?.data?.unlockedWeaponSet != null &&
                                                             CrossRunInfo.Instance.data.unlockedWeaponSet.Contains(
                                                                 weapon.weaponName))).ToList();
                    pickup.weaponData = Instantiate(eligibleWeapons.GetRandom());
                    pickup.UpdateSprite();
                }
            }

            if (hasPlayerUpgrade)
            {
                UpgradePickup pickup = GetComponentInChildren<UpgradePickup>();
                if (pickup)
                {
                    UpgradeManager upgradeManager = UpgradeManager.Instance;
                    HashSet<UpgradeType> validUpgradeTypes = new(upgradeManager.ImplementedUpgrades);
                    validUpgradeTypes.ExceptWith(PlayerUpgradeManager.Instance.GetUniqueUpgrades);
                    if (!CrossRunInfo.HasUpgrade(PermUpgradeType.SteelClaws))
                    {
                        validUpgradeTypes.RemoveWhere(type =>
                            upgradeManager.UpgradeMapping[type].upgradeCategory == UpgradeCategory.WallRun);
                    }
                    if (!CrossRunInfo.HasUpgrade(PermUpgradeType.GrapplingHook))
                    {
                        validUpgradeTypes.RemoveWhere(type =>
                            upgradeManager.UpgradeMapping[type].upgradeCategory == UpgradeCategory.Grapple);
                    }

                    List<UpgradeType> types = validUpgradeTypes.ToList();
                    if (types.Count == 0)
                    {
                        Debug.LogWarning("Exhausted all upgrades when generating: creating new list");
                        types = UpgradeManager.Instance.ImplementedUpgrades.ToList();
                    }
                    pickup.upgrade = types.ToList().GetRandom();
                    UpgradeManager.Instance.UpgradeMapping[pickup.upgrade].Set(pickup);
                }
            }
        }
    }
}