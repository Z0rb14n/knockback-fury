using System;
using UnityEngine;
using Util;

namespace NewFloorGen
{
    public class ComputerTunnel : TriggerTextScript
    {
        public ComputerRoom computerRoom;
        public ComputerTunnelUI tunnelUI;
        [SerializeField] private SpriteRenderer computerSprite;

        public Sprite enabledSprite;
        public Sprite disabledSprite;

        protected override bool CanInteract
        {
            get => enabled;
            set => enabled = value;
        }

        private void Awake()
        {
            if (!tunnelUI) tunnelUI = FindObjectOfType<ComputerTunnelUI>(true);
            if (!computerRoom) computerRoom = GetComponentInParent<ComputerRoom>();
        }

        private void OnEnable()
        {
            computerSprite.sprite = enabledSprite;
        }

        private void OnDisable()
        {
            computerSprite.sprite = disabledSprite;
        }

        protected override void OnPlayerInteraction()
        {
            tunnelUI.Open(this);
        }
    }
}