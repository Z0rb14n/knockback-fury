using UnityEngine;

namespace Enemies.Cat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CorridorDeathLaser : MonoBehaviour
    {
        [SerializeField] private GameObject objectToUnhide;
        [SerializeField, Min(0)] private float speed;
        [SerializeField] private bool isVertical;

        private Rigidbody2D _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public void OnEnable()
        {
            if (objectToUnhide) objectToUnhide.SetActive(true);
            if (!_rigidbody) Awake();
            _rigidbody.linearVelocity = isVertical ? new Vector2(0, speed) : new Vector2(speed, 0);
        }
    }
}