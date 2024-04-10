using UnityEngine;
using Util;

namespace NewFloorGen
{
    public class ComputerCDDrive : TriggerTextScript
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite hasUpgradeSprite;
        [SerializeField] private Sprite emptySprite;

        private bool _isEmpty;

        protected override bool CanInteract
        {
            get => !_isEmpty;
            set
            {
                _isEmpty = !value;
                UpdatePlayerGrapple();
            }
        }

        private void Awake()
        {
            spriteRenderer.sprite = hasUpgradeSprite;
            _isEmpty = false;
        }

        protected override void OnPlayerInteraction()
        {
            spriteRenderer.sprite = emptySprite;
        }
    }
}