using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ExplosionVFX : MonoBehaviour
    {
        public float duration;
        private SpriteRenderer _renderer;
        private float _elapsed;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        public void SetSize(float radius)
        {
            float size = radius * 2;
            transform.localScale = new Vector3(size, size, size);
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed > duration) Destroy(gameObject);
            Color color = _renderer.color;
            color.a = 1-_elapsed / duration;
            _renderer.color = color;
        }
    }
}