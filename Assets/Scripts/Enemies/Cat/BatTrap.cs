using System.Collections;
using FMODUnity;
using UnityEngine;

namespace Enemies.Cat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BatTrap : MonoBehaviour
    {
        [SerializeField] private float speed = 2;
        [SerializeField] private EventReference bonkSound;
        private Rigidbody2D _rigidbody;
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (_rigidbody.rotation < -90f)
            {
                _rigidbody.freezeRotation = true;
                _rigidbody.rotation = -90f;
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            _rigidbody.freezeRotation = true;
            if (!bonkSound.Guid.IsNull) RuntimeManager.PlayOneShot(bonkSound,transform.position);
            StartCoroutine(DisableAfterDelay(other));
        }

        private void OnEnable()
        {
            _rigidbody.angularVelocity = -360 * speed;
        }
        
        private static IEnumerator DisableAfterDelay(Collision2D other)
        {
            // ContactDamage procs on collision stay, not enter
            yield return new WaitForSeconds(0.1f);
            Physics2D.IgnoreCollision(other.collider,other.otherCollider);
        }
    }
}