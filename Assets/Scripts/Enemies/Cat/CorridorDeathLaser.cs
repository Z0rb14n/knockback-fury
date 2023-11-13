using System;
using UnityEngine;

namespace Enemies.Cat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CorridorDeathLaser : MonoBehaviour
    {
        [SerializeField] private GameObject objectToUnhide;
        [SerializeField, Min(0)] private float speed;

        private Rigidbody2D _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public void OnEnable()
        {
            objectToUnhide.SetActive(true);
            if (!_rigidbody) Awake();
            _rigidbody.velocity = new Vector2(speed, 0);
        }
    }
}