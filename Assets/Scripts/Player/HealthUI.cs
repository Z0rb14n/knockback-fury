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
        [SerializeField] private Image targetAnalysisObject;
        [SerializeField] private Image targetAnalysisGrey;
        [SerializeField] private Color targetAnalysisUnavailableColor = new Color(1, 1, 1, 0.5f);
        private RectTransform _healthBarRect;
        private RectTransform _targetAnalysisBackRect;

        private int _prevDisplayedHp = 0;
        private int _prevDisplayedMax = 0;

        private void Awake()
        {
            _healthBarRect = healthBar.GetComponent<RectTransform>();
            _targetAnalysisBackRect = targetAnalysisGrey.GetComponent<RectTransform>();
            SetValues();
            PlayerUpgradeManager.Instance.OnUpgradePickup += OnUpgradePickupHandler;
            playerHealth.OnTargetAnalysisUpdate += SetValues;
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

            targetAnalysisObject.gameObject.SetActive(PlayerUpgradeManager.Instance[UpgradeType.TargetAnalysis] > 0);
            if (PlayerUpgradeManager.Instance[UpgradeType.TargetAnalysis] > 0)
            {
                bool isShieldActive = playerHealth.IsTargetAnalysisShieldActive;
                ratio = isShieldActive ? 1 : ((float)playerHealth.TargetAnalysisDamage /
                        PlayerUpgradeManager.Instance.GetData(UpgradeType.TargetAnalysis));
                Debug.Log(isShieldActive + "," + playerHealth.TargetAnalysisDamage);
                targetAnalysisObject.color = isShieldActive ? Color.white : targetAnalysisUnavailableColor;
                _targetAnalysisBackRect.anchorMin = new Vector2(ratio, 0);
            }
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
