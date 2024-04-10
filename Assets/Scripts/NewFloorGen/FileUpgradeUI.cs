using System;
using Player;
using UnityEngine;
using Upgrades;
using Util;

namespace NewFloorGen
{
    public class FileUpgradeUI : MonoBehaviour
    {
        [SerializeField] private RectTransform fileLocations;
        [SerializeField] private GameObject fileIconPrefab;

        private ComputerCDDrive _cdDrive;
        
        public void Open(ComputerCDDrive drive)
        {
            _cdDrive = drive;
            ObjectUtil.EnsureLength(fileLocations, drive.data.Length, fileIconPrefab);
            for (int i = 0; i < drive.data.Length; i++)
            {
                fileLocations.GetChild(i).GetComponent<FileUpgradeButton>().SetDisplay(drive.data[i], this);
            }
            UIUtil.OpenUI();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Close(null);  
        }

        public void Close(UpgradePickupData selected)
        {
            UIUtil.CloseUI();
            Destroy(gameObject);
            if (selected)
            {
                PlayerUpgradeManager.Instance.PickupUpgrade(selected.upgradeType, selected.upgradeData);
                _cdDrive.Consume();
            }
        }
    }
}