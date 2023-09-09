using GameEnd;
using UnityEngine;
using UnityEngine.UI;

namespace Enemies
{
    public class BossHealthBar : MonoBehaviour
    {
        public EntityHealth health;
        [SerializeField] private Image healthBar;
        private RectTransform _healthBarRect;

        private bool _hasDied;
        

        private void Awake()
        {
            _healthBarRect = healthBar.GetComponent<RectTransform>();
            if (!health)
            {
                BossEnemy boss = FindObjectOfType<BossEnemy>(true);
                Debug.Assert(boss);
                health = boss.GetComponent<EntityHealth>();
                Debug.Assert(health);
            }
            SetValues();
        }

        private void SetValues()
        {
            float ratio = (float)health.health / health.maxHealth;
            // wtf is with this anchoredPosition + sizeDelta BS
            _healthBarRect.anchoredPosition = new Vector2(-1000f * (1 - ratio) / 2, 0);
            _healthBarRect.sizeDelta = new Vector2(-10 - 1000f * (1 - ratio), -10);
            if (health.health <= 0 && !_hasDied)
            {
                // TODO MOVE TO BOSS ENTITY HEALTH
                // BUT I'M LAZY
                _hasDied = true;
                if (GameEndCanvas.Instance)
                {
                    GameEndCanvas.Instance.DisplayAfterDelay(1, true);
                }
            }
        }

        private void Update()
        {
            SetValues();
        }
    }
}