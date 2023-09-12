using System;
using Player;
using UnityEngine;

namespace Upgrades
{
    [DisallowMultipleComponent]
    public class UpgradeDashDamageCanvas : MonoBehaviour
    {
        public RectTransform adrenaline;
        public RectTransform stabilizedAim;
        public RectTransform dashCharges;
        
        public void Update()
        {
            adrenaline.gameObject.SetActive(PlayerUpgradeManager.Instance[UpgradeType.Adrenaline] > 0);
            for (int i = 0; i < adrenaline.childCount; i++)
            {
                adrenaline.GetChild(i).gameObject.SetActive(i < PlayerWeaponControl.Instance.AdrenalineStacks);
            }
            stabilizedAim.gameObject.SetActive(PlayerUpgradeManager.Instance[UpgradeType.StabilizedAim] > 0);
            for (int i = 0; i < stabilizedAim.childCount; i++)
            {
                stabilizedAim.GetChild(i).gameObject.SetActive(i < PlayerWeaponControl.Instance.StabilizedAimStacks);
            }

            int charges = PlayerMovementScript.Instance.EffectiveDashes;
            for (int i = 0; i < dashCharges.childCount; i++)
            {
                dashCharges.GetChild(i).gameObject.SetActive(i < charges);
            }
        }
    }
}