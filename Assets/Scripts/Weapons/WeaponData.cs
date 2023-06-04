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
        public float knockbackStrength = 12;
        public float recoilAnimationDuration = 0.2f;
        public FireMode fireMode = FireMode.SemiAuto;

        public Sprite sprite;

        private int _clip;

        public bool IsClipEmpty => _clip <= 0;

        private void Awake()
        {
            Reload();
        }

        public void DecrementClip()
        {
            _clip -= 1;
        }

        public void Reload()
        {
            _clip = clipSize;
            Debug.Log("Reloaded...");
        }

        /// <summary>
        /// Return the Damage Per Second (assuming all shots hit)
        /// </summary>
        public float DPS => numProjectiles * projectileDamage * roundsPerSecond;

    }
}