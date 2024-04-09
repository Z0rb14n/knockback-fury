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

        public ComputerTunnelUIComputer mainComputer;
        private ComputerTunnel _computerTunnel;
        private ComputerRoom _room;
        private int _selectedIndex = -1;
        private ComputerTunnelUIComputer[] uiNeighbors;
        public void Open(ComputerTunnel trigger)
        {
            gameObject.SetActive(true);
            _computerTunnel = trigger;
            _computerTunnel.enabled = false;
            _room = _computerTunnel.computerRoom;
            UIUtil.OpenUI();
            CreateNeighbors();
            _selectedIndex = -1;
            mainComputer.DisplayingVirus = true;
        }

        private void CreateNeighbors()
        {
            int len = _room.neighbors.Length;
            ObjectUtil.EnsureLength(neighbors, len, neighborPrefab);
            uiNeighbors = new ComputerTunnelUIComputer[len];
            for (int i = 0; i < len; i++)
            {
                ComputerTunnelUIComputer comp = neighbors.GetChild(i).GetComponent<ComputerTunnelUIComputer>();
                comp.SetRadiusAndAngle(neighborRadius, i * Mathf.PI*2/len);
                comp.DisplayingVirus = false;
                uiNeighbors[i] = comp;
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (_selectedIndex != -1) Teleport();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                if (_selectedIndex == -1)
                    ChangeIndex(uiNeighbors.Length-1);
                else if (_selectedIndex == uiNeighbors.Length - 1)
                    ChangeIndex(0);
                else
                    ChangeIndex(_selectedIndex+1);
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                switch (_selectedIndex)
                {
                    case -1:
                        ChangeIndex(0);
                        break;
                    case 0:
                        ChangeIndex(uiNeighbors.Length-1);
                        break;
                    default:
                        ChangeIndex(_selectedIndex-1);
                        break;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape)) Close();
        }

        private void ChangeIndex(int newIndex)
        {
            int prevIndex = _selectedIndex;
            _selectedIndex = newIndex;
            if (prevIndex == -1) mainComputer.DisplayingVirus = false;
            else uiNeighbors[prevIndex].DisplayingVirus = false;

            uiNeighbors[_selectedIndex].DisplayingVirus = true;
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
            _room.neighbors[_selectedIndex].SpawnPlayer();
        }
    }
}