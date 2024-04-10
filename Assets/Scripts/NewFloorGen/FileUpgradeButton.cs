using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Upgrades;

namespace NewFloorGen
{
    public class FileUpgradeButton : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private TextMeshProUGUI desc;
        
        private UpgradePickupData _upgradePickupData;
        private FileUpgradeUI _fileUI;

        public void SetDisplay(UpgradePickupData pickup, FileUpgradeUI ui)
        {
            text.text = pickup.displayName;
            desc.text = pickup.infoText;
            _upgradePickupData = pickup;
            _fileUI = ui;
        }

        public void OnSelect()
        {
            _fileUI.Close(_upgradePickupData);
        }
    }
}