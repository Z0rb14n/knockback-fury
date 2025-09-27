using UnityEngine;

namespace DashVFX
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class MultiSpriteDashTrail: AbstractDashTrail
    {
        [SerializeField] protected GameObject objectToHide;
        [SerializeField] protected SpriteRenderer[] objects;
        [SerializeField] protected float velocityScale = 0.1f;

        protected SpriteRenderer spriteRenderer;
        protected bool vfxEnabled;
        
        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected void FixedUpdate()
        {
            if (!vfxEnabled) return;
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].sprite = spriteRenderer.sprite;
                objects[i].transform.localPosition = -(i + 1) * velocityScale * GetObjectVelocity();
            }
        }

        protected abstract Vector2 GetObjectVelocity();

        public override void StartDash(bool flipped)
        {
            vfxEnabled = true;
            foreach (SpriteRenderer sr in objects) sr.flipX = flipped;
            objectToHide.SetActive(true);
        }

        public override void StopDash()
        {
            vfxEnabled = false;
            objectToHide.SetActive(false);
        }
    }
}
