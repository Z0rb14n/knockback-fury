using System;
using Player;
using UnityEngine;
using Util;

namespace NewFloorGen
{
    public class ComputerTunnelUI : MonoBehaviour
    {
        public GameObject neighborPrefab;
        public Transform neighbors;
        [Min(0)]
        public float neighborRadius = 300;
        private ComputerTunnel _computerTunnel;
        private ComputerRoom _room;
        private int _selectedIndex = -1;
        public void Open(ComputerTunnel trigger)
        {
            gameObject.SetActive(true);
            _computerTunnel = trigger;
            _computerTunnel.enabled = false;
            _room = _computerTunnel.computerRoom;
            UIUtil.OpenUI();
            /*
            for (int i = 0; i < buttonsArea.childCount; i++)
            {
                buttonsArea.GetChild(i).gameObject.SetActive(_upgradeTrigger.allowedButtons == null || _upgradeTrigger.allowedButtons.Contains(i));
            }
            */
            CreateNeighbors();
        }

        private void CreateNeighbors()
        {
            int len = _room.neighbors.Length;
            ObjectUtil.EnsureLength(neighbors, len, neighborPrefab);
            for (int i = 0; i < len; i++)
            {
                ComputerTunnelUIComputer comp = neighbors.GetChild(i).GetComponent<ComputerTunnelUIComputer>();
                comp.SetRadiusAndAngle(neighborRadius, i * Mathf.PI*2/len);
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (_selectedIndex != -1)
                {
                    
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape)) Close();
        }

        private void Close()
        {
            UIUtil.CloseUI();
            _computerTunnel.enabled = true;
            gameObject.SetActive(false);
        }

        public void Teleport()
        {
            Close();
            // TODO USE CORRECT COMPUTER ROOM
            _computerTunnel.computerRoom.SpawnPlayer();
        }
    }
}