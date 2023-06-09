using TMPro;
using UnityEngine;

namespace Player
{
    public class HealthUI : MonoBehaviour
    {
        public PlayerHealth playerHealth;
        public TextMeshProUGUI textObject;

        private void Update()
        {
            textObject.text = $"HP: {playerHealth.health}/{playerHealth.maxHealth}";
        }
    }
}
