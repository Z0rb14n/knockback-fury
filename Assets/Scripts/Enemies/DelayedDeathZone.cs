using System.Collections;
using Player;
using UnityEngine;

namespace Enemies
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public class DelayedDeathZone : MonoBehaviour
    {
        [SerializeField] private Color warningColor = new Color(1,1f,0,0.3f);
        [SerializeField] private Color activeColor = new Color(1,0,0,0.5f);
        private SpriteRenderer _sprite;
        private int _damage;
        private bool _active;

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
        }

        public void StartZone(float delay, float duration, int damage)
        {
            _damage = damage;
            StartCoroutine(ZoneCoroutine(delay, duration));
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            TriggerLogic(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TriggerLogic(other);
        }

        private void TriggerLogic(Collider2D other)
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health == null) return;
            if (_active) health.TakeDamage(_damage);
        }

        private IEnumerator ZoneCoroutine(float delay, float duration)
        {
            _sprite.enabled = true;
            _sprite.color = warningColor;
            yield return new WaitForSeconds(delay);
            _sprite.color = activeColor;
            _active = true;
            yield return new WaitForSeconds(duration);
            _active = false;
            _sprite.enabled = false;
        }
    }
}