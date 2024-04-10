using UnityEngine;
using Upgrades;
using Util;

namespace NewFloorGen
{
    public class ComputerCDDrive : TriggerTextScript
    {
        [SerializeField] private GameObject uiToCreate;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite hasUpgradeSprite;
        [SerializeField] private Sprite emptySprite;

        public UpgradePickupData[] data;

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
            GameObject go = Instantiate(uiToCreate);
            go.GetComponent<FileUpgradeUI>().Open(this);
        }

        public void Consume()
        {
            _isEmpty = true;
            spriteRenderer.sprite = emptySprite;
        }
    }
}