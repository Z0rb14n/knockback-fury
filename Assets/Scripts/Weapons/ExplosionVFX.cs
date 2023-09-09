using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ExplosionVFX : MonoBehaviour
    {
        [Min(0), Tooltip("Delay before destruction on creation")]
        public float duration;
        private float _elapsed;

        private void Awake()
        {
            Destroy(gameObject, duration);
        }

        public void SetSize(float radius)
        {
            float size = radius * 2;
            transform.localScale = new Vector3(size, size, size);
        }
    }
}