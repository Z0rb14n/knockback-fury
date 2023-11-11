using UnityEngine;

namespace Enemies.Cat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SpikeCircle : MonoBehaviour
    {
        [SerializeField] private float speed = 2;
        
        private Rigidbody2D _body;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _body.angularVelocity = 360 * speed;
        }
    }
}