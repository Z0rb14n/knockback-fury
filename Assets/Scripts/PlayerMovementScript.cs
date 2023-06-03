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
    [Tooltip("Wall Jump Impulse")]
    public Vector2 wallJumpForce = new(10,5);
    [Min(0), Tooltip("Test Mouse1 Knockback Impulse")]
    public float testKnockbackForce = 10;
    [Min(0), Tooltip("Dash movement per physics update")]
    public float dashSpeed = 1;
    [Min(0), Tooltip("Time in Air Dash")]
    public float dashTime = 1;
    private ContactFilter2D _groundFilter;
    private ContactFilter2D _leftWallFilter;
    private ContactFilter2D _rightWallFilter;
    private Rigidbody2D _body;
    private Camera _cam;
    private bool _jumpRequest;
    private bool _knockbackRequest;
    private Vector2 _knockbackDirection;
    private float _speed;
    private float _currentVelocity;
    private bool _dashing;
    private Vector2 _dashDirection;
    private int _physicsCheckMask;
    
    private bool Grounded => _body.IsTouching(_groundFilter);
    private bool IsOnLeftWall => _body.IsTouching(_leftWallFilter);
    private bool IsOnRightWall => _body.IsTouching(_rightWallFilter);

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _cam = Camera.main;
        InitializeContactFilters();
    }

    private void InitializeContactFilters()
    {
        _physicsCheckMask = LayerMask.GetMask("Default");
        _groundFilter = new ContactFilter2D
        {
            layerMask = _physicsCheckMask,
            useLayerMask = true,
            useNormalAngle = true,
            minNormalAngle = 30,
            maxNormalAngle = 150
        };
        _leftWallFilter = new ContactFilter2D
        {
            layerMask = _physicsCheckMask,
            useLayerMask = true,
            useNormalAngle = true,
            minNormalAngle = -60,
            maxNormalAngle = 60
        };
        _rightWallFilter = new ContactFilter2D
        {
            layerMask = _physicsCheckMask,
            useLayerMask = true,
            useNormalAngle = true,
            minNormalAngle = 120,
            maxNormalAngle = 240
        };
    }

    private void Update()
    {
        float xInput = 0;
        
        if (Input.GetKey(KeyCode.A)) xInput -= 1;
        if (Input.GetKey(KeyCode.D)) xInput += 1;

        // Only change velocity when there's input
        if (xInput != 0) 
        {
            _speed = Mathf.SmoothDamp(_speed, xInput * maxSpeed, ref _currentVelocity, speedSmoothness);
            _body.velocity = new Vector2(_speed, _body.velocity.y);
        }

        if (Input.GetKeyDown(KeyCode.Space)) _jumpRequest = true;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!_dashing && !Grounded && xInput != 0)
            {
                _dashing = true;
                _dashDirection = new Vector2(xInput, 0);
                StartCoroutine(DashCoroutine());
            }
        }
        
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
            if (Grounded)
                _body.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            else if (IsOnLeftWall)
                _body.AddForce(new Vector2(wallJumpForce.x, wallJumpForce.y), ForceMode2D.Impulse);
            else if (IsOnRightWall)
                _body.AddForce(new Vector2(-wallJumpForce.x,wallJumpForce.y),ForceMode2D.Impulse);
            _jumpRequest = false;
        }

        if (_knockbackRequest)
        {
            _body.AddForce(_knockbackDirection * testKnockbackForce, ForceMode2D.Impulse);
            _knockbackRequest = false;
        }
    }

    private IEnumerator DashCoroutine()
    {
        for (float timePassed = 0; timePassed < dashTime; timePassed += Time.fixedDeltaTime)
        {
            if (_body.IsTouchingLayers(_physicsCheckMask)) break;
            Vector2 pos = _body.position;
            _body.MovePosition(pos + (_dashDirection * dashSpeed));
            yield return new WaitForFixedUpdate();
        }

        _dashing = false;
    }
}
