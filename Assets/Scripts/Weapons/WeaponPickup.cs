using Player;
using TMPro;
using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(Collider2D))]
    public class WeaponPickup : MonoBehaviour
    {
        public WeaponData weaponData;
        public float delay;

        [SerializeField] private TextMeshPro gunSprite;

        private void Awake()
        {
            UpdateSprite();
        }

        public void UpdateSprite() => UpdateSprite(weaponData);

        public void UpdateSprite(WeaponData data)
        {
            if (data != null) weaponData = data;
            if (weaponData)
            {
                gunSprite.text = data.displayText;
                gunSprite.rectTransform.localScale = new Vector3(data.shouldFlipDisplay ? -1 : 1, 1, 1);
                gunSprite.rectTransform.localPosition = data.displayPosOffset;
            }
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