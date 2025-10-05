using Player;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using Upgrades;
using static UnityEditor.PlayerSettings;

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
        
        [SerializeField] private GameObject hookBottom;
        [SerializeField] private Color ropeColor;
        [SerializeField] private float shakeAmount;
        [SerializeField] private float shakeSpeed;
        private float shakeX;
        private float shakeY;
        private Vector2 _originalPos;

        //This is the percentage of the time in which the rope will turn red before breaking
        [SerializeField] private float redPercent;

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
                    //transform.position = hookedPos;
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

        public void setHookRot(Vector2 dir)
        {
            if (dir != Vector2.zero)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // Subtract 90 to align top
            }

        }

        private void OnHookedEntityDeath(EntityHealth health)
        {
            if (PlayerUpgradeManager.Instance[UpgradeType.BountyHunter] > 0)
            {
                PlayerMovementScript.Instance.GrappleHookCooldown = 0;
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!_isFixed)
            {
                

                Vector2 pos = hookBottom.transform.position;// Instead using the grapple contact point! to get contact do: other.GetContact(0).point;
                Vector2 playerPos = _player.transform.position;
                _isFixed = true;
                EntityHealth otherEntity = other.collider.GetComponent<EntityHealth>();
                if (otherEntity && otherEntity.grappleHookTarget)
                {
                    pos = otherEntity.transform.position;
                    _hookedEntity = otherEntity;
                    _hookedEntity.OnDeath += OnHookedEntityDeath;
                    PlayerMovementScript.Instance.OnEnemyHook(_hookedEntity);
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

                _line.startColor = ropeColor;
                _line.endColor = ropeColor;

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
            float timer = maxDuration;
            float redTime = maxDuration * redPercent;
            _originalPos = transform.position;
            float shakeTimer = 0;
            while (timer > 0)
            {
                if(timer < redTime)
                {
                    float t = ((timer / redTime)/2) + 0.5f;

                    _line.startColor = new Color(ropeColor.r, ropeColor.g * t, ropeColor.b * t, 1);
                    _line.endColor = new Color(ropeColor.r, ropeColor.g * t, ropeColor.b * t, 1);

                    shakeTimer += Time.deltaTime;
                    if (shakeTimer > shakeSpeed)
                    {
                        shakeTimer = 0;
                        shakeX = Random.Range(-shakeAmount, shakeAmount); //* (1 - t);
                        shakeY = Random.Range(-shakeAmount, shakeAmount); //* (1 - t);
                        transform.position = new Vector3(_originalPos.x + shakeX, _originalPos.y + shakeY, transform.position.z);
                        _line.SetPosition(1, hookBottom.transform.position);
                    }
                }




                timer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            _player.OnGrappleExpire();
            Destroy(gameObject);
        }
    }
}