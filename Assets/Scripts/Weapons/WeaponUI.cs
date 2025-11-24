using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Weapons
{
    /// <summary>
    /// Main weapon info display UI.
    /// </summary>
    public class WeaponUI : MonoBehaviour
    {
        [SerializeField]
        private Image weaponImage;
        [SerializeField] private TextMeshProUGUI maxAmmo;
        [SerializeField] private TextMeshProUGUI currAmmo;
        [SerializeField]
        private RectTransform ammoDisplay;
        [SerializeField]
        private Image reloadDisplay;
        [SerializeField]
        private Color normalReloadColor = Color.white;
        [SerializeField]
        private Color oneYearOfReloadColor = Color.gold;
        private Weapon _weapon;

        private void Awake()
        {
            _weapon = FindAnyObjectByType<Weapon>();
            _weapon.OnWeaponChanged += OnWeaponChanged;
        }

        /// <summary>
        /// Called when the equipped weapon changes — re-updates the sprite.
        /// </summary>
        private void OnWeaponChanged()
        {
            weaponImage.sprite = _weapon.WeaponData.sprite;
        }

        public void Update()
        {
            // todo make this not run every frame
            bool reloading = _weapon.ReloadTime > 0;
            reloadDisplay.gameObject.SetActive(reloading);
            ammoDisplay.gameObject.SetActive(!reloading);
            if (reloading)
            {
                reloadDisplay.fillAmount = 1-_weapon.ReloadTime / _weapon.MaxReloadTime;
                reloadDisplay.color = _weapon.IsOneYearOfReloadPossible ?  oneYearOfReloadColor : normalReloadColor;
            }
            else
            {
                currAmmo.text = _weapon.WeaponData.Clip.ToString();
                maxAmmo.text = _weapon.WeaponData.actualClipSize.ToString();
            }
        }
    }
}