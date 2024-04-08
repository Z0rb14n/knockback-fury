using UnityEngine;

namespace Weapons
{
    [RequireComponent(typeof(SpriteRenderer), typeof(AudioSource))]
    public class ExplosionVFX : MonoBehaviour
    {
        [Min(0), Tooltip("Delay before destruction on creation")]
        public float duration;
        private float _elapsed;
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            Destroy(gameObject, duration);
        }

        public void SetAudio(AudioClip clip)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
        }

        public void SetSize(float radius)
        {
            float size = radius * 2;
            transform.localScale = new Vector3(size, size, size);
        }
    }
}