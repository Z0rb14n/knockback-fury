using Player;
using UnityEngine;
using UnityEngine.Events;

namespace Enemies.Cat
{
    [RequireComponent(typeof(Collider2D))]
    public class ObjectUnhideTrigger : MonoBehaviour
    {
        public UnityEvent onPlayerEnter;
        [SerializeField] private GameObject objectToUnhide;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMovementScript>() == null) return;
            
            Destroy(gameObject);
            objectToUnhide.SetActive(true);
            onPlayerEnter.Invoke();
        }
    }
}