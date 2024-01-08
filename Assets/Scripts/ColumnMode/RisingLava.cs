using Player;
using UnityEngine;

namespace ColumnMode
{
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D))]
    public class RisingLava : MonoBehaviour
    {
        [SerializeField, Min(0)] private float speed;
        [SerializeField, Min(0)]
        private float diffBeforeJump;
        public float max;
        
        private Rigidbody2D _rigidbody;

        private PlayerMovementScript _player;
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.velocity = new Vector2(0, speed);
            _player = PlayerMovementScript.Instance;
        }

        private void FixedUpdate()
        {
            if (_rigidbody.position.y >= max)
            {
                enabled = false;
                return;
            }

            if (_rigidbody.position.y + diffBeforeJump < _player.Pos.y)
                _rigidbody.position = new Vector2(_rigidbody.position.x, _player.Pos.y - diffBeforeJump);
        }
    }
}