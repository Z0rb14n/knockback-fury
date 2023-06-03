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
    [Min(0), Tooltip("Dash movement per physics update")]
    public float dashSpeed = 1;
    [Min(0), Tooltip("Time in Air Dash")]
    public float dashTime = 1;

    private Weapon _weapon;
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

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _cam = Camera.main;
        _weapon = GetComponentInChildren<Weapon>();
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
        float xInput = Input.GetAxisRaw("Horizontal"); // Using GetAxisRaw() instead of two if statements

        // Only change velocity when there's input
        if (xInput != 0) 
        {
            _speed = Mathf.SmoothDamp(_speed, xInput * maxSpeed, ref _currentVelocity, speedSmoothness);
            _body.velocity = new Vector2(_speed, _body.velocity.y);
        }

        _jumpRequest = Input.GetKeyDown(KeyCode.Space); // Removed if statement
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldMousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
            _knockbackDirection = ((Vector2)(transform.position - worldMousePos)).normalized;
            _knockbackRequest = _knockbackDirection != Vector2.zero; // Removed if statement
        }
    }

    private void FixedUpdate()
    {
        if (_jumpRequest)
        {
            _body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); // Used Vector2.up instead of new Vector2(0, jumpForce)
            _jumpRequest = false;
        }

        if (_knockbackRequest)
        {
            if (!ReferenceEquals(_weapon, null) && !ReferenceEquals(_weapon.weaponData, null))
                _body.AddForce(_knockbackDirection * _weapon.weaponData.knockbackStrength, ForceMode2D.Impulse);
            _knockbackRequest = false;
        }
    }
}
