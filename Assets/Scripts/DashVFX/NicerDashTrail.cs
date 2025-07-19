using System;
using UnityEngine;

namespace DashVFX
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class NicerDashTrail : AbstractDashTrail
    {
        [SerializeField] private GameObject objectToHide;
        [SerializeField] private SpriteRenderer[] objects;
        [SerializeField] private float velocityScale = 0.1f;

        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody;
        private bool _enabled;
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void FixedUpdate()
        {
            if (!_enabled) return;
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].sprite = _spriteRenderer.sprite;
                objects[i].transform.localPosition = -(i + 1) * velocityScale * _rigidbody.linearVelocity;
            }
        }

        public override void StartDash(bool flipped)
        {
            _enabled = true;
            foreach (SpriteRenderer sr in objects) sr.flipX = flipped;
            objectToHide.SetActive(true);
        }

        public override void StopDash()
        {
            _enabled = false;
            objectToHide.SetActive(false);
        }
    }
}