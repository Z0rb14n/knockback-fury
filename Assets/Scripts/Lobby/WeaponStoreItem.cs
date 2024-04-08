using FileSave;
using Player;
using TMPro;
using UnityEngine;
using Util;
using Weapons;

namespace Lobby
{
    /// <summary>
    /// In the weapon store in the lobby, a 'pedestal' of a weapon to purchase.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class WeaponStoreItem : TriggerTextScript
    {
        public WeaponData data;
        [SerializeField] private TextMeshPro gunSprite;
        [SerializeField] private GameObject pickupPrefab;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = Color.gray;

        private void Awake()
        {
            if (CrossRunInfo.Instance)
            {
                CrossRunInfo.Instance.OnCheeseCountChange += OnCheeseCountChangeHandler;
            }
            UpdateDisplayedGun();
            UpdateDisplay();
        }

        private void UpdateDisplayedGun()
        {
            gunSprite.text = data.displayText;
            gunSprite.rectTransform.localScale = new Vector3(data.shouldFlipDisplay ? -1 : 1, 1, 1);
            gunSprite.rectTransform.localPosition = data.displayPosOffset;
        }

        private void UpdateDisplay()
        {
            notification.text = data.weaponName + " - " + data.cheeseCost + " cheese";
            notification.color = inactiveColor;
            if (CrossRunInfo.Instance && CrossRunInfo.Instance.data.cheese >= data.cheeseCost)
            {
                notification.text = "[E] " + notification.text;
                notification.color = activeColor;
            }
        }

        protected override void OnPlayerInteraction()
        {
            if (CrossRunInfo.Instance)
            {
                if (CrossRunInfo.Instance.data.cheese >= data.cheeseCost)
                {
                    GameObject go = Instantiate(pickupPrefab, transform.position - new Vector3(0,0.5f,0), Quaternion.identity);
                    WeaponPickup pickup = go.GetComponent<WeaponPickup>();
                    pickup.UpdateSprite(Instantiate(data));
                    pickup.delay = -1;
                    PlayerWeaponControl.Instance.PickupWeapon(pickup);
                    Destroy(gameObject);
                    CrossRunInfo.Instance.AddCheese(-data.cheeseCost);
                }
            }
        }

        private void OnValidate()
        {
            if (!data) return;
            UpdateDisplayedGun();
            UpdateDisplay();
        }

        private void OnCheeseCountChangeHandler(int _)
        {
            UpdateDisplay();
        }

        private void OnDestroy()
        {
            if (CrossRunInfo.Instance)
            {
                CrossRunInfo.Instance.OnCheeseCountChange -= OnCheeseCountChangeHandler;
            }
        }
    }
}