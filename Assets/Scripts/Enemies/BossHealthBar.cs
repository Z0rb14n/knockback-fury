using UnityEngine;
using UnityEngine.UI;

namespace Enemies
{
    public class BossHealthBar : MonoBehaviour
    {
        public EntityHealth health;
        [SerializeField] private Image healthBar;
        private RectTransform _healthBarRect;

        public Color BarColor
        {
            get => healthBar.color;
            set => healthBar.color = value;
        }

        private bool _hasDied;
        

        private void Awake()
        {
            _healthBarRect = healthBar.GetComponent<RectTransform>();
            if (!health)
            {
                BossEnemy boss = FindAnyObjectByType<BossEnemy>(FindObjectsInactive.Include);
                Debug.Assert(boss);
                health = boss.GetComponent<EntityHealth>();
                Debug.Assert(health);
            }
            SetValues();
        }

        private void SetValues()
        {
            float ratio = health?(float)health.health / health.maxHealth : 0;
            // wtf is with this anchoredPosition + sizeDelta BS
            _healthBarRect.anchoredPosition = new Vector2(-1000f * (1 - ratio) / 2, 0);
            _healthBarRect.sizeDelta = new Vector2(-10 - 1000f * (1 - ratio), -10);
        }

        private void Update()
        {
            SetValues();
        }
    }
}