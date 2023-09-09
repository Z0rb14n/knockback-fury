using System;
using UnityEngine;

namespace Weapons
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "WeaponData")]
    public class WeaponData : ScriptableObject, ISerializationCallbackReceiver
    {
        public string weaponName;
        [Min(0)]
        public int numProjectiles = 1;
        [SerializeField, Min(0)]
        private float range = 10;

        [NonSerialized]
        public float actualRange = 10;
        [Min(0)]
        public float projectileSpeed = 1;
        [Min(0)]
        public int projectileDamage = 1;
        [Min(0)]
        public float roundsPerSecond = 1;
        [SerializeField, Min(0)]
        private int clipSize = 1;
        [NonSerialized]
        public int actualClipSize = 1;
        [Min(0), Tooltip("Reload time (seconds)")]
        public float reloadTime = 1.5f;
        [Min(0), Tooltip("Spread of the weapon in degrees")]
        public float spread = 15;
        [Tooltip("Whether the gun is Hitscan")]
        public bool isHitscan;
        [Tooltip("What action is the right click action")]
        public WeaponRightClickAction rightClickAction = WeaponRightClickAction.None;
        [SerializeField]
        private float knockbackStrength = 12;
        [NonSerialized]
        public float actualKnockbackStrength;
        public float recoilAnimationDuration = 0.2f;
        public FireMode fireMode = FireMode.SemiAuto;
        public BurstInfo burstInfo;
        public MeleeInfo meleeInfo;
        public FireMode altFireMode;

        public Sprite sprite;
        public GameObject customProjectile;

        public AudioClip fireEffect;
        [NonSerialized]
        public int numUpgrades;

        public int Clip { get; private set; }

        public bool IsClipEmpty => Clip <= 0;
        
        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            actualKnockbackStrength = knockbackStrength;
            actualRange = range;
            actualClipSize = clipSize;
            numUpgrades = 0;
        }

        public void IncrementClip()
        {
            if (Clip != 0) Clip++;
        }

        public void DecrementClip()
        {
            Clip -= 1;
        }

        public void Reload()
        {
            Clip = actualClipSize;
        }

        /// <summary>
        /// Return the Damage Per Second (assuming all shots hit)
        /// </summary>
        public float DPS => numProjectiles * projectileDamage * roundsPerSecond;

        public void UpgradeAmmoCapacity()
        {
            if (actualClipSize <= 3) actualClipSize++;
            else actualClipSize = Mathf.RoundToInt(actualClipSize * 1.5f);
            numUpgrades++;
        }

        public void UpgradeRecoil()
        {
            // TODO IMPL
            numUpgrades++;
        }

        public void UpgradeKnockback()
        {
            actualKnockbackStrength *= 1.5f;
            numUpgrades++;
        }

        public void UpgradeRange()
        {
            actualRange *= 1.5f;
            numUpgrades++;
        }
    }

    [Serializable]
    public struct BurstInfo
    {
        [Min(0), Tooltip("Number of bullets in the burst.")]
        public int burstAmount;
        [Min(0),Tooltip("RPS within the burst.")]
        public float withinBurstFirerate;
    }
    [Serializable]
    public struct MeleeInfo
    {
        [Tooltip("Base Damage of Melee Weapon")]
        public int baseDamage;
        [Tooltip("Velocity multiplier")]
        public float velMultiplier;
        [Tooltip("Melee Range")]
        public float meleeRange;
    }
}