using TMPro;
using UnityEngine;

namespace Weapons
{
    public class WeaponUI : MonoBehaviour
    {
        public Weapon weapon;
        public TextMeshProUGUI textObject;

        public void Update()
        {
            textObject.text = $"{weapon.weaponData.weaponName}: {weapon.weaponData.Clip}/{weapon.weaponData.clipSize}" + (weapon.ReloadTime <= 0
                ? ""
                : $" Reload Time: {weapon.ReloadTime:F2}");
        }
    }
}