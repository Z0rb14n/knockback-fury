using Player;
using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public class WeaponPickup : MonoBehaviour
    {
        public WeaponData weaponData;
        public float delay;

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            UpdateSprite();
        }

        public void UpdateSprite() => UpdateSprite(weaponData);

        public void UpdateSprite(WeaponData data)
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (data != null) weaponData = data;
            if (weaponData != null) _spriteRenderer.sprite = weaponData.sprite;
        }

        private void Update()
        {
            delay -= Time.deltaTime;
        }

        private void OnValidate()
        {
            UpdateSprite();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerWeaponControl playerWeaponControl = other.GetComponent<PlayerWeaponControl>();
            if (playerWeaponControl != null)
            {
                playerWeaponControl.PickupWeapon(this);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            PlayerWeaponControl playerWeaponControl = other.GetComponent<PlayerWeaponControl>();
            if (playerWeaponControl != null)
            {
                playerWeaponControl.weaponsOn.Remove(this);
            }
        }
    }
}