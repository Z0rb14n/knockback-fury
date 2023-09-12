using Player;
using UnityEngine;
using Util;

namespace Weapons
{
    [DisallowMultipleComponent]
    public class WeaponUpgradeUI : MonoBehaviour
    {
        public GameObject visibleUI;
        public GameObject weaponButtonPrefab;
        public RectTransform weaponsArea;

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
            Time.timeScale = 0;
            PlayerMovementScript.Instance.enabled = false;
            PlayerWeaponControl.Instance.enabled = false;
            int numWeapons = PlayerWeaponControl.Instance.NumWeapons;
            EnsureLength(numWeapons);
            for (int i = 0; i < numWeapons; i++)
            {
                _weaponButtons[i].SetDisplayedWeapon(i);
            }
            SelectWeapon(0);
            visibleUI.SetActive(true);
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
            if (Input.GetKeyDown(KeyCode.Escape) && visibleUI.activeSelf) Close();
        }

        public void Close()
        {
            Time.timeScale = 1;
            if (_upgradeTrigger) _upgradeTrigger.enabled = true;
            PlayerMovementScript.Instance.enabled = true;
            PlayerWeaponControl.Instance.enabled = true;
            visibleUI.SetActive(false);
        }

        public void UpgradeAmmo()
        {
            PlayerWeaponControl.Instance.GetInventory[_selectedWeaponIndex].UpgradeAmmoCapacity();
            Destroy(_upgradeTrigger.gameObject);
            _upgradeTrigger = null;
            Close();
        }

        public void UpgradeDamage()
        {
            PlayerWeaponControl.Instance.GetInventory[_selectedWeaponIndex].UpgradeDamage();
            Destroy(_upgradeTrigger.gameObject);
            _upgradeTrigger = null;
            Close();
        }
        
        public void UpgradeAccuracy()
        {
            PlayerWeaponControl.Instance.GetInventory[_selectedWeaponIndex].UpgradeAccuracy();
            Destroy(_upgradeTrigger.gameObject);
            _upgradeTrigger = null;
            Close();
        }

        public void UpgradeRecoil()
        {
            PlayerWeaponControl.Instance.GetInventory[_selectedWeaponIndex].UpgradeRecoil();
            Destroy(_upgradeTrigger.gameObject);
            _upgradeTrigger = null;
            Close();
        }

        public void UpgradeKnockback()
        {
            PlayerWeaponControl.Instance.GetInventory[_selectedWeaponIndex].UpgradeKnockback();
            Destroy(_upgradeTrigger.gameObject);
            _upgradeTrigger = null;
            Close();
        }

        public void UpgradeRange()
        {
            PlayerWeaponControl.Instance.GetInventory[_selectedWeaponIndex].UpgradeRange();
            Destroy(_upgradeTrigger.gameObject);
            _upgradeTrigger = null;
            Close();
        }
    }
}
