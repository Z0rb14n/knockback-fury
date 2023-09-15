using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Upgrades
{
    [DisallowMultipleComponent]
    public class UpgradeDashDamageCanvas : MonoBehaviour
    {
        [SerializeField] private RectTransform adrenaline;
        [SerializeField] private RectTransform stabilizedAim;
        [SerializeField] private RectTransform dashCharges;
        [SerializeField] private Image targetAnalysisObject;
        [SerializeField] private Image targetAnalysisGrey;
        [SerializeField] private Color targetAnalysisUnavailableColor = new(1, 1, 1, 0.5f);
        
        private RectTransform _targetAnalysisBackRect;
        private TextMeshProUGUI _adrenalineText;
        private TextMeshProUGUI _stabilizedAimText;

        private void Awake()
        {
            _adrenalineText = adrenaline.GetComponentInChildren<TextMeshProUGUI>();
            _stabilizedAimText = stabilizedAim.GetComponentInChildren<TextMeshProUGUI>();
            _targetAnalysisBackRect = targetAnalysisGrey.GetComponent<RectTransform>();
        }

        public void Update()
        {
            int adrenalineStacks = Mathf.Max(PlayerWeaponControl.Instance.AdrenalineStacks, PlayerWeaponControl.Instance.maxAdrenalineStacks);
            adrenaline.gameObject.SetActive(adrenalineStacks > 0);
            _adrenalineText.text = adrenalineStacks > 1 ? adrenalineStacks.ToString() : "";
            
            int stabilizedAimStacks = Mathf.Max(PlayerWeaponControl.Instance.StabilizedAimStacks, PlayerWeaponControl.Instance.maxStabilizedAimStacks);
            stabilizedAim.gameObject.SetActive(stabilizedAimStacks > 0);
            _stabilizedAimText.text = stabilizedAimStacks > 1 ? stabilizedAimStacks.ToString() : "";

            int charges = PlayerMovementScript.Instance.EffectiveDashes;
            for (int i = 0; i < dashCharges.childCount; i++)
            {
                dashCharges.GetChild(i).gameObject.SetActive(i < charges);
            }

            bool hasTargetAnalysis = PlayerUpgradeManager.Instance[UpgradeType.TargetAnalysis] > 0;
            
            targetAnalysisObject.gameObject.SetActive(hasTargetAnalysis);
            if (hasTargetAnalysis)
            {
                PlayerHealth playerHealth = PlayerHealth.Instance;
                bool isShieldActive = playerHealth.IsTargetAnalysisShieldActive;
                float ratio = isShieldActive ? 1 : ((float)playerHealth.TargetAnalysisDamage /
                                              PlayerUpgradeManager.Instance.GetData(UpgradeType.TargetAnalysis));
                targetAnalysisObject.color = isShieldActive ? Color.white : targetAnalysisUnavailableColor;
                _targetAnalysisBackRect.anchorMin = new Vector2(ratio, 0);
            }
        }
    }
}