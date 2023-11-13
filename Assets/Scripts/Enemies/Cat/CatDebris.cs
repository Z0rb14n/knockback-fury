using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies.Cat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CatDebris : MonoBehaviour
    {
        [SerializeField] private float timeBeforeDestruction = 2;
        [SerializeField] private float initialVelocity = 2;
        [SerializeField] private float spinSpeed = 2;
        private Rigidbody2D _body;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            Destroy(gameObject, timeBeforeDestruction);
            Vector2 dir = Random.insideUnitCircle;
            if (dir.y < 0) dir.y *= -1;
            _body.velocity = dir * initialVelocity;
            _body.angularVelocity = 360 * spinSpeed;
        }
    }
}