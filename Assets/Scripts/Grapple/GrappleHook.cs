using System.Collections;
using Player;
using UnityEngine;

namespace Grapple
{
    [RequireComponent(typeof(LineRenderer),typeof(SpringJoint2D), typeof(DistanceJoint2D))]
    public class GrappleHook : MonoBehaviour
    {
        [SerializeField, Min(0)]
        private float maxDuration = 6;
        [SerializeField, Min(0)]
        private float maxLength = 10;
        [SerializeField]
        private bool useFixedDistance;
        [SerializeField, Min(0)]
        private float fixedDistance;
        [SerializeField] private bool useDistJoint;
        private SpringJoint2D _joint;
        private DistanceJoint2D _distJoint;
        private Rigidbody2D _body;
        private Rigidbody2D _playerBody;
        private PlayerMovementScript _player;
        private LineRenderer _line;
        private Collider2D _collider;

        private EntityHealth _hookedEntity;

        private bool _isFixed;

        private void Awake()
        {
            _joint = GetComponent<SpringJoint2D>();
            _distJoint = GetComponent<DistanceJoint2D>();
            _line = GetComponent<LineRenderer>();
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _player = PlayerMovementScript.Instance;
            _playerBody = _player.GetComponent<Rigidbody2D>();
            _joint.enabled = false;
            _line.enabled = false;
        }

        private void FixedUpdate()
        {
            if (!_isFixed)
            {
                if (Vector2.Distance(_body.position, _playerBody.position) > maxLength)
                {
                    _player.OnGrappleOOB();
                    Destroy(gameObject);
                }
            }
            else
            {
                _line.SetPosition(0, _player.transform.position);
                if (_hookedEntity)
                {
                    // TODO optimize
                    Vector3 hookedPos = _hookedEntity.transform.position;
                    _line.SetPosition(1, hookedPos);
                    transform.position = hookedPos;
                }
                else
                {
                    if (!ReferenceEquals(_hookedEntity, null))
                    {
                        _player.OnGrappleExpire();
                        Destroy(gameObject);
                    }
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!_isFixed)
            {
                Vector2 pos = other.GetContact(0).point;
                Vector2 playerPos = _player.transform.position;
                _isFixed = true;
                EntityHealth otherEntity = other.collider.GetComponent<EntityHealth>();
                if (otherEntity && otherEntity.grappleHookTarget)
                {
                    pos = otherEntity.transform.position;
                    _hookedEntity = otherEntity;
                }

                float dist = Vector2.Distance(pos, playerPos);
                if (dist > maxLength)
                {
                    _player.OnGrappleOOB();
                    Destroy(gameObject);
                    return;
                }
                
                _line.SetPosition(0, playerPos);
                _line.SetPosition(1, pos);
                _line.enabled = true;
                _body.constraints = RigidbodyConstraints2D.FreezeAll;
                Joint2D usedJoint = useDistJoint ? _distJoint : _joint;
                usedJoint.enabled = true;
                usedJoint.connectedBody = _playerBody;
                if (useDistJoint)
                    _distJoint.distance = useFixedDistance ? fixedDistance : dist;
                else
                    _joint.distance = useFixedDistance ? fixedDistance : dist;
                _collider.enabled = false;
                StartCoroutine(DestroyAfterDelay());
            }
        }

        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(maxDuration);
            _player.OnGrappleExpire();
            Destroy(gameObject);
        }
    }
}