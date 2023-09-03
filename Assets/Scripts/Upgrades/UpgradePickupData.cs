using UnityEngine;

namespace Upgrades
{
    /// <summary>
    /// Scriptable object containing title and info text.
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrade Data")]
    public class UpgradePickupData : ScriptableObject
    {
        [Tooltip("Internal upgrade type")]
        public UpgradeType upgradeType;
        [Tooltip("Upgrade data - currently only used for Cloak and Dagger/Target Analysis as of now")]
        public int upgradeData;
        [Tooltip("Title/display name of this upgrade")]
        public string displayName;
        [TextArea, Tooltip("Informational text about this upgrade")]
        public string infoText;
        [Tooltip("Whether this is implemented (i.e. generated in game)")]
        public bool implemented;

        /// <summary>
        /// Sets the UpgradePickup using the data in this scriptable object.
        /// </summary>
        /// <param name="pickup">Pickup Object</param>
        /// <param name="checkUpgradeType">Whether a warning is produced on mismatching upgrade types.</param>
        public void Set(UpgradePickup pickup, bool checkUpgradeType = true)
        {
            if (checkUpgradeType)
            {
                if (pickup.upgrade != upgradeType)
                {
                    Debug.LogWarning("Mismatching upgrade types: " + pickup.upgrade + " vs " + upgradeType);
                }
            }

            pickup.upgrade = upgradeType;
            pickup.upgradeData = upgradeData;
        }
    }
}