using Player;
using UnityEngine;

namespace Enemies.Cat
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(EntityHealth))]
    public class HarmlessCat : MonoBehaviour
    {
        private PlayerMovementScript _player;
        private Collider2D _collider;
        private Rigidbody2D _rigidbody;
        private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite leftSprite;
        [SerializeField] private Sprite rightSprite;
        [SerializeField] private GameObject catDebris;
        [SerializeField] private HarmlessCat[] friends;
        [SerializeField] private Vector2 jumpVector = new(2, 2);
        private LayerMask _groundMask;
        private ContactFilter2D _groundFilter;
        private bool Grounded => _collider.IsTouching(_groundFilter);

        private bool _friendDead;

        private void Awake()
        {
            _player = PlayerMovementScript.Instance;
            _collider = GetComponent<Collider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _groundMask = LayerMask.GetMask("Default","Platform", "IgnorePlayer");
            _groundFilter = new ContactFilter2D
            {
                layerMask = _groundMask,
                useLayerMask = true,
                useNormalAngle = true,
                minNormalAngle = 30,
                maxNormalAngle = 150
            };
            GetComponent<EntityHealth>().OnDeath += OnDeath;
        }

        private void FixedUpdate()
        {
            Vector2 playerPos = _player.Pos;
            Vector2 pos = _rigidbody.position;
            if (playerPos.x < pos.x)
                _spriteRenderer.sprite = _friendDead ? rightSprite : leftSprite;
            else
                _spriteRenderer.sprite = _friendDead ? leftSprite : rightSprite;

            bool isLeft = playerPos.x < pos.x;
            if (Grounded && _friendDead)
            {
                _rigidbody.velocity = jumpVector* new Vector2(isLeft? 1 : -1, 1);
            }
        }

        private void OnDeath(EntityHealth ignored)
        {
            GameObject go = Instantiate(catDebris, transform.position, Quaternion.identity);
            go.transform.localScale = transform.lossyScale;
            foreach (HarmlessCat friend in friends)
            {
                if (friend) friend._friendDead = true;
            }
        }
    }
}