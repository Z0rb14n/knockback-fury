using Player;
using UnityEngine;
using Util;

namespace Weapons
{
    [DisallowMultipleComponent]
    public class WeaponUpgradeUI : MonoBehaviour
    {
        public GameObject weaponButtonPrefab;
        public RectTransform weaponsArea;
        public RectTransform buttonsArea;

        private WeaponButton[] _weaponButtons;
        private WeaponUpgradeTrigger _upgradeTrigger;
        private int _selectedWeaponIndex;

        private void EnsureLength(int len)
        {
            ObjectUtil.EnsureLength(weaponsArea, len, weaponButtonPrefab);
            _weaponButtons = new WeaponButton[len];
            for (int i = 0; i < len; i++)
            {
                _weaponButtons[i] = weaponsArea.GetChild(i).GetComponent<WeaponButton>();
            }
        }

        public void Open(WeaponUpgradeTrigger trigger)
        {
            _upgradeTrigger = trigger;
            _upgradeTrigger.enabled = false;
            UIUtil.OpenUI();
            int numWeapons = PlayerWeaponControl.Instance.NumWeapons;
            EnsureLength(numWeapons);
            for (int i = 0; i < numWeapons; i++)
            {
                _weaponButtons[i].SetDisplayedWeapon(i);
            }
            SelectWeapon(0);
            for (int i = 0; i < buttonsArea.childCount; i++)
            {
                buttonsArea.GetChild(i).gameObject.SetActive(_upgradeTrigger.allowedButtons == null || _upgradeTrigger.allowedButtons.Contains(i));
            }
        }

        public void SelectWeapon(int index)
        {
            _selectedWeaponIndex = index;
            for (int i = 0; i < weaponsArea.childCount; i++)
            {
                _weaponButtons[i].ButtonInteractable = (index != i);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Close();
        }

        public void Close()
        {
            if (_upgradeTrigger) _upgradeTrigger.enabled = true;
            UIUtil.CloseUI();
            Destroy(gameObject);
        }

        public void UpgradeAmmo()
        {
            PlayerWeaponControl.Instance.Inventory[_selectedWeaponIndex].UpgradeAmmoCapacity();
            Destroy(_upgradeTrigger.gameObject);
            _upgradeTrigger = null;
            Close();
        }

        public void UpgradeDamage()
        {
            PlayerWeaponControl.Instance.Inventory[_selectedWeaponIndex].UpgradeDamage();
            Destroy(_upgradeTrigger.gameObject);
            _upgradeTrigger = null;
            Close();
        }
        
        public void UpgradeAccuracy()
        {
            PlayerWeaponControl.Instance.Inventory[_selectedWeaponIndex].UpgradeAccuracy();
            Destroy(_upgradeTrigger.gameObject);
            _upgradeTrigger = null;
            Close();
        }

        public void UpgradeRecoil()
        {
            PlayerWeaponControl.Instance.Inventory[_selectedWeaponIndex].UpgradeRecoil();
            Destroy(_upgradeTrigger.gameObject);
            _upgradeTrigger = null;
            Close();
        }

        public void UpgradeKnockback()
        {
            PlayerWeaponControl.Instance.Inventory[_selectedWeaponIndex].UpgradeKnockback();
            Destroy(_upgradeTrigger.gameObject);
            _upgradeTrigger = null;
            Close();
        }

        public void UpgradeRange()
        {
            PlayerWeaponControl.Instance.Inventory[_selectedWeaponIndex].UpgradeRange();
            Destroy(_upgradeTrigger.gameObject);
            _upgradeTrigger = null;
            Close();
        }
    }
}
