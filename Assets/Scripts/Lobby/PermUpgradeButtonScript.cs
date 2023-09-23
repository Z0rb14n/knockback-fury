using System.Linq;
using FileSave;
using PermUpgrade;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Weapons;

namespace Lobby
{
    /// <summary>
    /// Script attached to the buttons to purchase permanent upgrades.
    /// </summary>
    public class PermUpgradeButtonScript : MonoBehaviour
    {
        [SerializeField, Tooltip("Canvas to send mouse clicks to")] private PermUpgradeCanvas canvas;
        [SerializeField, Tooltip("Upgrade Type")] private PermUpgradeData upgradeData;
        [SerializeField, Tooltip("Weapon data of weapon unlock (if applicable)")] private WeaponData weaponData;
        [SerializeField, Tooltip("Text displaying title")] private TextMeshProUGUI title;
        [SerializeField, Tooltip("Text displaying description")] private TextMeshProUGUI description;
        [SerializeField, Tooltip("Text displaying cost/unlocked")] private TextMeshProUGUI cost;
        [SerializeField, Tooltip("Button to manage interact-ability")] private Button button;
        [SerializeField, Tooltip("Prerequisite upgrades.")] private PermUpgradeData[] requires;
        [SerializeField, Tooltip("Prerequisite weapons.")] private WeaponData[] requiredWeapons;

        private static bool HasUpgrade(PermUpgradeType type, string weaponName = null)
        {
            // ReSharper disable once Unity.NoNullPropagation
            SaveData data = CrossRunInfo.Instance?.data;
            if (data == null) return false;
            if (type != PermUpgradeType.WeaponUnlock) return data.unlockedPermUpgradeTypes != null && data.unlockedPermUpgradeTypes.Contains(type);
            return data.unlockedWeaponSet?.Contains(weaponName) ?? weaponName == null;
        }

        private void Awake()
        {
            if (CrossRunInfo.Instance)
                CrossRunInfo.Instance.OnUpgradesChanged += OnUpgradeUpdate;
            OnUpgradeUpdate();
        }

        // ReSharper disable once IdentifierTypo
        private bool HasRequiredPrereqs()
        {
            if (requires != null)
            {
                if (!requires.All(data => data == null || HasUpgrade(data.upgradeType))) return false;
            }

            if (requiredWeapons != null)
            {
                // ReSharper disable once Unity.NoNullPropagation
                SaveData data = CrossRunInfo.Instance?.data;
                if (data == null) return requiredWeapons == null || requiredWeapons.Length == 0;
                if (!requiredWeapons.All(weapon => !weapon || (data.unlockedWeaponSet != null && data.unlockedWeaponSet.Contains(weapon.weaponName))))
                    return false;
            }

            return true;
        }

        private void OnValidate() => OnUpgradeUpdate();

        private void OnUpgradeUpdate()
        {
            if (upgradeData)
            {
                // ReSharper disable once Unity.NoNullPropagation
                bool hasThis = HasUpgrade(upgradeData.upgradeType, weaponData?.weaponName);
                bool hasRequired = HasRequiredPrereqs();

                bool isWeapon = upgradeData.upgradeType == PermUpgradeType.WeaponUnlock;
                if (isWeapon) Debug.Assert(weaponData, "WeaponData must be provided for weapon unlock");
                if (title)
                {
                    title.text = upgradeData.displayName;
                    if (isWeapon) title.text = title.text.Replace("{{name}}", weaponData!.weaponName);
                }
                if (description)
                {
                    description.text = upgradeData.infoText;
                    if (isWeapon) description.text = description.text.Replace("{{name}}", weaponData!.weaponName);
                }
                if (cost) cost.text = hasThis ? "Unlocked" : upgradeData.cheeseCost + " cheese";

                button.interactable = !hasThis && hasRequired;
            }
        }

        /// <summary>
        /// Receiver of button mouse clicks - forward to canvas.
        /// </summary>
        public void HandleClick()
        {
            // ReSharper disable once Unity.NoNullPropagation
            canvas.OnClick(upgradeData.upgradeType, weaponData?.weaponName, upgradeData.cheeseCost);
        }
    }
}