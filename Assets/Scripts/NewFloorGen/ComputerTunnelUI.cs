using Player;
using UnityEngine;

namespace NewFloorGen
{
    public class ComputerTunnelUI : MonoBehaviour
    {
        public GameObject visibleUI;
        private ComputerTunnel _computerTunnel;
        private int _selectedIndex;
        public void Open(ComputerTunnel trigger)
        {
            _computerTunnel = trigger;
            _computerTunnel.enabled = false;
            Time.timeScale = 0;
            PlayerMovementScript.Instance.enabled = false;
            PlayerWeaponControl.Instance.enabled = false;
            /*
            for (int i = 0; i < buttonsArea.childCount; i++)
            {
                buttonsArea.GetChild(i).gameObject.SetActive(_upgradeTrigger.allowedButtons == null || _upgradeTrigger.allowedButtons.Contains(i));
            }
            */
            visibleUI.SetActive(true);
        }
    }
}