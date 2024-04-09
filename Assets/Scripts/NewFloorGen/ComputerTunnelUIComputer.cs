using UnityEngine;
using UnityEngine.UI;

namespace NewFloorGen
{
    public class ComputerTunnelUIComputer : MonoBehaviour
    {
        public GameObject displayedVirus;
        public Image connection;
        public GameObject computer;

        public Color connectionActiveColor = Color.yellow;
        public Color connectionInactiveColor = Color.gray;

        public bool DisplayingVirus
        {
            get => displayedVirus.activeSelf;
            set
            {
                displayedVirus.SetActive(value);
                if (connection)
                {
                    connection.color = value ? connectionActiveColor : connectionInactiveColor;
                }
            }
        }

        public void SetRadiusAndAngle(float rad, float angle)
        {
            if (connection)
            {
                connection.transform.localEulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg);
            }

            if (computer)
            {
                computer.transform.localPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * rad;
            }
        }
    }
}