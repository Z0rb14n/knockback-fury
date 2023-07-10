﻿using UnityEngine;
using UnityEngine.UI;

namespace Enemies
{
    public class BossHealthBar : MonoBehaviour
    {
        public EntityHealth health;
        [SerializeField] private Image healthBar;
        private RectTransform _healthBarRect;
        

        private void Awake()
        {
            _healthBarRect = healthBar.GetComponent<RectTransform>();
            SetValues();
        }

        private void SetValues()
        {
            float ratio = (float)health.health / health.maxHealth;
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