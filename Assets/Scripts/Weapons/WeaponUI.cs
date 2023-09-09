using FileSave;
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
            textObject.text = $"{weapon.WeaponData.weaponName}: {weapon.WeaponData.Clip}/{weapon.WeaponData.actualClipSize}" + (weapon.ReloadTime <= 0
                ? ""
                : $" Reload Time: {weapon.ReloadTime:F2}") + (weapon.IsOneYearOfReloadPossible ? " OneYearOfReload Possible" : "");
        }
    }
}