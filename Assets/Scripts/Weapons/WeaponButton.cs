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
        public TextMeshProUGUI text;
        public TextMeshProUGUI gunSprite;
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
            gunSprite.text = data.displayText;
            gunSprite.rectTransform.localScale = new Vector3(data.shouldFlipDisplay ? -1 : 1, 1, 1);
            gunSprite.rectTransform.localPosition = data.displayPosOffset;
            text.text = data.weaponName;
        }

        public void OnClick()
        {
            _weaponUpgradeUI.SelectWeapon(index);
        }
    }
}