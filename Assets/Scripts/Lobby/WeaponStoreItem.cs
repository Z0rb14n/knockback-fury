using FileSave;
using Player;
using UnityEngine;
using Util;
using Weapons;

namespace Lobby
{
    [RequireComponent(typeof(SpriteRenderer),typeof(Collider2D))]
    public class WeaponStoreItem : TriggerTextScript
    {
        public WeaponData data;
        [SerializeField] private GameObject pickupPrefab;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = Color.gray;

        private void Awake()
        {
            if (CrossRunInfo.Instance)
            {
                CrossRunInfo.Instance.OnCheeseCountChange += OnCheeseCountChangeHandler;
            }
            GetComponent<SpriteRenderer>().sprite = data.sprite;
            UpdateDisplay();
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
                    pickup.UpdateSprite(data);
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
            GetComponent<SpriteRenderer>().sprite = data.sprite;
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