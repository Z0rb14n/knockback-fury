using Enemies.Cat;
using UnityEngine;

namespace NewFloorGen
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FileDebris : CatDebris
    {
        private SpriteRenderer _spriteRenderer;

        protected override void Awake()
        {
            base.Awake();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }
    }
}