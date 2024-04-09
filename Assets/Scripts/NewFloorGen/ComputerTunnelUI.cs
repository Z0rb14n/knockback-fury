using System.Linq;
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
        
        private void TryMoveAngle(float targetAngle)
        {
            if (_selectedIndex == -1)
            {
                int index = uiNeighbors.Select((comp, index) =>
                    (Mathf.Abs(Mathf.DeltaAngle(comp.Angle,targetAngle)), index)).Min().index;
                ChangeIndex(index);
            }
            else
            {
                float initialDiff = Mathf.Abs(Mathf.DeltaAngle(uiNeighbors[_selectedIndex].Angle,targetAngle));
                int newIndex = _selectedIndex == 0 ? uiNeighbors.Length-1 : _selectedIndex - 1;
                int otherIndex = _selectedIndex == uiNeighbors.Length - 1 ? 0 : _selectedIndex + 1;
                float newDiff = Mathf.Abs(Mathf.DeltaAngle(uiNeighbors[newIndex].Angle,targetAngle));
                float otherDiff = Mathf.Abs(Mathf.DeltaAngle(uiNeighbors[otherIndex].Angle,targetAngle));
                if (newDiff <= initialDiff) ChangeIndex(newIndex);
                else if (otherDiff <= initialDiff) ChangeIndex(otherIndex);
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (_selectedIndex != -1) Teleport();
            }

            if (Input.GetKeyDown(KeyCode.A)) TryMoveAngle(180);
            if (Input.GetKeyDown(KeyCode.D)) TryMoveAngle(0);
            if (Input.GetKeyDown(KeyCode.W)) TryMoveAngle(90);
            if (Input.GetKeyDown(KeyCode.S)) TryMoveAngle(-90);

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