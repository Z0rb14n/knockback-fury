using UnityEngine;

namespace Player
{
    [DisallowMultipleComponent]
    public class CameraScript : MonoBehaviour
    {
        private static CameraScript _instance;

        public static CameraScript Instance
        {
            get
            {
                if (!_instance) _instance = FindAnyObjectByType<CameraScript>(FindObjectsInactive.Include);
                return _instance;
            }
        }
        
        [Tooltip("Maximum View Distance")]
        public float maxViewDist = 150;
        [Tooltip("Camera Offset")]
        public Vector3 camOffset = new(-1, 0);
        public bool columnMode;
        public float columnMinX;
        public float columnMaxX;
        private Camera _mainCam;
        /// <summary>
        /// Cached screen dimensions
        /// </summary>
        private Vector2 _screenDims;
        private Vector3 _displacement; // Reuse displacement vector to prevent frequent object creation

        public float CameraShakeStrength { get; set; }

        public void SetColumnMode(bool value) => columnMode = value;

        private void Awake() 
        {
            _mainCam = Camera.main;
            _screenDims = new Vector2(Screen.width, Screen.height);
        }

        private void Update() 
        {
            ApplyMouseDisplacement();
            if (columnMode)
            {
                Vector3 pos = transform.position;
                pos.x = Mathf.Clamp(pos.x, columnMinX, columnMaxX);
                transform.position = pos;
            }
            transform.localPosition += (Vector3) Random.insideUnitCircle * CameraShakeStrength;
        }

        /// <summary>
        /// Displace the camera in the direction of the mouse
        /// </summary>
        private void ApplyMouseDisplacement() 
        {
            Vector2 lookVec = LookVector();
            Vector3 direction = lookVec.normalized;
            float distance = lookVec.magnitude;
        
            // Reuse displacement vector and update its components
            _displacement.x = direction.x * Mathf.Lerp(0, maxViewDist, distance / _screenDims.magnitude);
            _displacement.y = direction.y * Mathf.Lerp(0, maxViewDist, distance / _screenDims.magnitude);
            _displacement.z = transform.localPosition.z;

            // how ironic that an optimization actually resulted in an inefficient property access huh
            // ReSharper disable once Unity.InefficientPropertyAccess
            transform.localPosition = _displacement + camOffset;
        }

        /// <summary>
        /// Returns vector of mouse from origin(player) to mouse position
        /// </summary>
        private Vector2 LookVector() 
        {
            Vector2 worldMousePos = _mainCam.ScreenToWorldPoint(Input.mousePosition);
            return transform.InverseTransformPoint(worldMousePos);
        }
    }
}