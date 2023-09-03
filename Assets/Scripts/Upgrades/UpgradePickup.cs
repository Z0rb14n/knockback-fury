using System;
using Player;
using UnityEngine;

namespace Upgrades
{
    [DisallowMultipleComponent, RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public class UpgradePickup : MonoBehaviour
    {
        public UpgradeType upgrade;
        /// <summary>
        /// Integer data - currently only used for Cloak and Dagger; Target Analysis.
        /// </summary>
        public int upgradeData;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerUpgradeManager playerUpgradeManager = other.GetComponent<PlayerUpgradeManager>();
            if (playerUpgradeManager != null)
            {
                playerUpgradeManager.PickupUpgrade(upgrade, upgradeData);
                if (UpgradeCanvas.Instance) UpgradeCanvas.Instance.Show(upgrade);
                Destroy(gameObject);
            }
        }
    }
}