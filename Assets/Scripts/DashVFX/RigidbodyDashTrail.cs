using UnityEngine;

namespace DashVFX
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class RigidbodyDashTrail : MultiSpriteDashTrail
    {
        private Rigidbody2D _rigidbody;
        
        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        protected override Vector2 GetObjectVelocity()
        {
            return _rigidbody.linearVelocity;
        }
    }
}