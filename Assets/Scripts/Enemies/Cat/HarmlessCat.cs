using Player;
using UnityEngine;

namespace Enemies.Cat
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(EntityHealth))]
    public class HarmlessCat : MonoBehaviour
    {
        private PlayerMovementScript _player;
        private Rigidbody2D _rigidbody;
        private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite leftSprite;
        [SerializeField] private Sprite rightSprite;
        [SerializeField] private GameObject catDebris;
        private void Awake()
        {
            _player = PlayerMovementScript.Instance;
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void FixedUpdate()
        {
            LookAtPlayer();
        }

        private void LookAtPlayer()
        {
            Vector3 playerPos = _player.transform.position;
            Vector2 pos = _rigidbody.position;
            if (playerPos.x < pos.x) _spriteRenderer.sprite = leftSprite;
            else if (playerPos.x > pos.x) _spriteRenderer.sprite = rightSprite;
        }

        private void OnDestroy()
        {
            GameObject go = Instantiate(catDebris, transform.position, Quaternion.identity);
            go.transform.localScale = transform.localScale;
        }
    }
}