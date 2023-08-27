using System.Collections;
using Player;
using UnityEngine;

namespace Enemies.Ranged
{
    [RequireComponent(typeof(Collider2D))]
    public class RangedEnemyScript : MonoBehaviour
    {
        [Tooltip("Prefab of bullet object")]
        public GameObject bulletPrefab;
        [Tooltip("Transform to create bullet position at")]
        public Transform bulletPos;
        [Min(0), Tooltip("Time (seconds) between firing")]
        public float fireDelay = 2;

        private bool _isPlayerInside;
        private IEnumerator _shootCoroutine;
        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        private IEnumerator ShootCoroutine()
        {
            while (_isPlayerInside)
            {
                yield return new WaitForSeconds(fireDelay);
                GameObject go = Instantiate(bulletPrefab, bulletPos.position, Quaternion.identity);
                go.GetComponent<EnemyBulletScript>().Initialize();
                if (_source && _source.clip) _source.Play();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.GetComponent<PlayerMovementScript>()) return;
            _isPlayerInside = true;
            _shootCoroutine = ShootCoroutine();
            StartCoroutine(_shootCoroutine);
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.GetComponent<PlayerMovementScript>()) return;
            _isPlayerInside = false;
            StopCoroutine(_shootCoroutine);
            _shootCoroutine = null;
        }
    }
}
