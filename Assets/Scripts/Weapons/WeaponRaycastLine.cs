using System.Collections;
using UnityEngine;

namespace Weapons
{
    [DisallowMultipleComponent, RequireComponent(typeof(LineRenderer))]
    public class WeaponRaycastLine : MonoBehaviour
    {
        [Tooltip("Color of the line")]
        public Color color = Color.yellow;

        private float _time;
        private bool isFading;
        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
            isFading = false;
        }

        public void Initialize(Vector2 start, Vector2 end, float time)
        {
            _lineRenderer.SetPositions(new Vector3[]{start,end});
            _time = time;
            isFading = true;
        }

        private void Update()
        {
            if(isFading)
            {
                float startTime = _time;
                Color finalColor = new Color(color.r, color.g, color.b,0);
                float ratio = _time / startTime;
                Color thisColor = Color.LerpUnclamped(color, finalColor, 1 - ratio);
                _lineRenderer.startColor = thisColor;
                _lineRenderer.endColor = thisColor;
                _time -= Time.deltaTime;
                if(_time <= 0)
                {
                    isFading = false;
                    ResetAndReturnToPool(); // you should write your own pooling logic
                }
            }
        }

        private void ResetAndReturnToPool()
        {
            // your logic here
        }
    }
}