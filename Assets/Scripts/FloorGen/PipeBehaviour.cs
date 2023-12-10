using UnityEngine;

namespace FloorGen
{
    [DisallowMultipleComponent, RequireComponent(typeof(Collider2D))]
    public class PipeBehaviour : MonoBehaviour
    {
        [SerializeField] private Vector2 spawnOffset;
        [SerializeField] private Transform platformTop;
        [SerializeField] private float horizontalTolerance = 5;
        
        public PipeBehaviour OtherPipe { get; set; }
        
        private int _playerLayer;
        private void Awake()
        {
            _playerLayer = LayerMask.NameToLayer("Player");
            float rotZ = transform.eulerAngles.z;
            if (Mathf.Abs(Mathf.Abs(rotZ) - 90) > 90-horizontalTolerance)
            {
                platformTop.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            if (!OtherPipe) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer != _playerLayer) return;
            if (OtherPipe) other.gameObject.transform.position = OtherPipe.transform.TransformPoint(spawnOffset);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.TransformPoint(spawnOffset), 0.25f);
        }
    }
}