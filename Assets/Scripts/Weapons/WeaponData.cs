using System;
using UnityEngine;

namespace Weapons
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "WeaponData")]
    public class WeaponData : ScriptableObject
    {
        public string weaponName;
        [Min(0)]
        public int numProjectiles = 1;
        [Min(0)]
        public float range = 10;
        [Min(0)]
        public float projectileSpeed = 1;
        [Min(0)]
        public int projectileDamage = 1;
        [Min(0)]
        public float roundsPerSecond = 1;
        [Min(0)]
        public int clipSize = 1;
        [Min(0), Tooltip("Reload time (seconds)")]
        public float reloadTime = 1.5f;
        [Min(0), Tooltip("Spread of the weapon in degrees")]
        public float spread = 15;
        [Tooltip("Whether the gun is Hitscan")]
        public bool isHitscan;
        [Tooltip("Whether the gun has Melee")]
        public bool hasMelee;
        public float knockbackStrength = 12;
        public float recoilAnimationDuration = 0.2f;
        public FireMode fireMode = FireMode.SemiAuto;
        public BurstInfo burstInfo;
        public MeleeInfo meleeInfo;

        public Sprite sprite;

        public int Clip { get; private set; }

        public bool IsClipEmpty => Clip <= 0;

        public void DecrementClip()
        {
            Clip -= 1;
        }

        public void Reload()
        {
            Clip = clipSize;
        }

        /// <summary>
        /// Return the Damage Per Second (assuming all shots hit)
        /// </summary>
        public float DPS => numProjectiles * projectileDamage * roundsPerSecond;
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