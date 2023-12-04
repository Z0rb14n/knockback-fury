using System;
using UnityEngine;

namespace FloorGen
{
    [RequireComponent(typeof(Collider2D))]
    public class RoomTrigger : MonoBehaviour
    {
        public event Action OnPlayerEnter;
        public event Action OnPlayerExit;
        private RoomData _roomData;
        private int _layer;
        private void Awake()
        {
            _roomData = GetComponentInParent<RoomData>();
            if (!_roomData)
            {
                Debug.LogWarning("Can't find room data: deleting object");
                Destroy(this);
            }

            _layer = LayerMask.NameToLayer("Player");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == _layer) OnPlayerEnter?.Invoke();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.layer == _layer) OnPlayerExit?.Invoke();
        }
    }
}