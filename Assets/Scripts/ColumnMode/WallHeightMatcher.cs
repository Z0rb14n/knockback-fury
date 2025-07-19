using System.Collections;
using Player;
using UnityEngine;

namespace ColumnMode
{
    [DisallowMultipleComponent]
    public class WallHeightMatcher : MonoBehaviour
    {
        public float min;
        public float max;
        [SerializeField] private float updateTime = 0.5f;

        private PlayerMovementScript _player;

        private void Awake()
        {
            _player = PlayerMovementScript.Instance;
            StartCoroutine(MovementRoutine());
        }

        private IEnumerator MovementRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(updateTime);
                transform.localPosition = new Vector3(0, Mathf.Clamp(Mathf.Floor(_player.Pos.y), min, max));
            }
        }
    }
}