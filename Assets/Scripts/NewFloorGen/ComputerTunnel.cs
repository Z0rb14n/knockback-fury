using UnityEngine;
using Util;

namespace NewFloorGen
{
    public class ComputerTunnel : TriggerTextScript
    {
        public ComputerTunnelUI tunnelUI;

        private void Awake()
        {
            if (!tunnelUI) tunnelUI = FindObjectOfType<ComputerTunnelUI>();
        }

        protected override void OnPlayerInteraction()
        {
            tunnelUI.Open(this);
        }
    }
}