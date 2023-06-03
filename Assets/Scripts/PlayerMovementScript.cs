using System.Collections;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    [Min(0), Tooltip("Affects the speed of the player")]
    public float maxSpeed = 69;
    [Min(0), Tooltip("Smoothness of Speed Changes")]
    public float speedSmoothness = 0.5f;
    [Min(0), Tooltip("Jump Impulse")]
    public float jumpForce = 10;
    [Min(0), Tooltip("Test Mouse1 Knockback Impulse")]
    public float testKnockbackForce = 10;
    [Min(0), Tooltip("Post-knockback control delay")]
    public float knockbackControlDelay = 0.5f;
    private Rigidbody2D _body;
    private Camera _cam;
    private bool _jumpRequest;
    private bool _knockbackRequest;
    private Vector2 _knockbackDirection;
    private float _speed;
    private float _currentVelocity;
    private bool _controlEnabled = true;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _cam = Camera.main;
    }

    private void Update()
    {
        float xInput = 0;
        
        if (Input.GetKey(KeyCode.A)) xInput -= 1;
        if (Input.GetKey(KeyCode.D)) xInput += 1;

        if (_controlEnabled)
        {
            _speed = Mathf.SmoothDamp(_speed, xInput * maxSpeed, ref _currentVelocity, speedSmoothness);
            _body.velocity = new Vector2(_speed, _body.velocity.y);
        }

        if (Input.GetKeyDown(KeyCode.Space)) _jumpRequest = true;
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldMousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dirVec = ((Vector2)(transform.position - worldMousePos)).normalized;
            if (dirVec != Vector2.zero)
            {
                _knockbackRequest = true;
                _knockbackDirection = dirVec;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_jumpRequest)
        {
            _body.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            _jumpRequest = false;
        }

        if (_knockbackRequest)
        {
            StartCoroutine(Knockback());
            _knockbackRequest = false;
        }
    }

    private IEnumerator Knockback()
    {
        _controlEnabled = false;
        _body.AddForce(_knockbackDirection * testKnockbackForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knockbackControlDelay);
        _controlEnabled = true;
    }
}
