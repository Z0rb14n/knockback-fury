using UnityEngine;

namespace PermUpgrade
{
    [CreateAssetMenu(menuName = "Permanent Upgrade Data")]
    public class PermUpgradeData : ScriptableObject
    {
        [Tooltip("Internal upgrade type")]
        public PermUpgradeType upgradeType;
        [Tooltip("Title/display name of this upgrade")]
        public string displayName;
        [TextArea, Tooltip("Informational text about this upgrade")]
        public string infoText;
        [Min(0), Tooltip("Cost in Cheese")]
        public int cheeseCost;
        [Tooltip("Whether this is implemented (i.e. generated in game)")]
        public bool implemented;
        [TextArea, Tooltip("Unused: indicate optional comment")]
        // ReSharper disable once NotAccessedField.Global
        public string comment;
    }
}