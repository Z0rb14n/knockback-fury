using System;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Weapons
{
    [DisallowMultipleComponent, RequireComponent(typeof(Button))]
    public class WeaponButton : MonoBehaviour
    {
        public Image imageToSet;
        public TextMeshProUGUI text;
        public int index;
        private WeaponUpgradeUI _weaponUpgradeUI;
        private Button _button;

        public bool ButtonInteractable
        {
            get => _button.interactable;
            set
            {
                if (!_button) _button = GetComponent<Button>();
                _button.interactable = value;
            }
        }

        private void Awake()
        {
            _weaponUpgradeUI = GetComponentInParent<WeaponUpgradeUI>();
            _button = GetComponent<Button>();
        }

        public void SetDisplayedWeapon(int newIndex)
        {
            index = newIndex;
            WeaponData data = PlayerWeaponControl.Instance.Inventory[newIndex];
            imageToSet.sprite = data.sprite;
            text.text = data.weaponName;
        }

        public void OnClick()
        {
            _weaponUpgradeUI.SelectWeapon(index);
        }
    }
}