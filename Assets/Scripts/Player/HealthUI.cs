using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Upgrades;

namespace Player
{
    public class HealthUI : MonoBehaviour
    {
        public PlayerHealth playerHealth;
        [SerializeField] private TextMeshProUGUI textObject;
        [SerializeField] private Image healthBar;
        [SerializeField] private Gradient colorGradient;
        private RectTransform _healthBarRect;

        private int _prevDisplayedHp;
        private int _prevDisplayedMax;

        private void Awake()
        {
            _healthBarRect = healthBar.GetComponent<RectTransform>();
            SetValues();
            PlayerUpgradeManager.Instance.OnUpgradePickup += OnUpgradePickupHandler;
        }

        private void OnUpgradePickupHandler(UpgradeType type, int data)
        {
            if (type == UpgradeType.TargetAnalysis) SetValues();
        }

        private void SetValues()
        {
            _prevDisplayedHp = playerHealth.health;
            _prevDisplayedMax = playerHealth.maxHealth;
            float ratio = (float)_prevDisplayedHp / _prevDisplayedMax;
            Color color = colorGradient.Evaluate(ratio);
            textObject.color = color;
            healthBar.color = color;
            textObject.text = $"HP: {playerHealth.health}/{playerHealth.maxHealth}";
            // wtf is with this anchoredPosition + sizeDelta BS
            _healthBarRect.anchoredPosition = new Vector2(-190f * (1 - ratio) / 2, 0);
            _healthBarRect.sizeDelta = new Vector2(-10 - 190f * (1 - ratio), -10);
        }

        private void Update()
        {
            if (playerHealth.health != _prevDisplayedHp || playerHealth.maxHealth != _prevDisplayedMax)
                SetValues();
        }

        private void OnValidate()
        {
            _healthBarRect = healthBar.GetComponent<RectTransform>();
            // set colors only
            float ratio = (float)_prevDisplayedHp / _prevDisplayedMax;
            if (float.IsNaN(ratio)) ratio = 1;
            Color color = colorGradient.Evaluate(ratio);
            textObject.color = color;
            healthBar.color = color;
        }
    }
}
