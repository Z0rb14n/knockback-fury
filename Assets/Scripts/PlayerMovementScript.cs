using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    [Min(0), Tooltip("Affects the speed of the player")]
    public float speed = 69;
    [Min(0), Tooltip("Jump Impulse")]
    public float jumpForce = 10;
    [Min(0), Tooltip("Test Mouse1 Knockback Impulse")]
    public float testKnockbackForce = 10;
    private Rigidbody2D _body;
    private Camera _cam;
    private Vector2 _force;
    private bool _jumpRequest;
    private bool _knockbackRequest;
    private Vector2 _knockbackDirection;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _cam = Camera.main;
    }

    private void Update()
    {
        _force = Vector2.zero;
        
        if (Input.GetKey(KeyCode.A)) _force += Vector2.left;
        if (Input.GetKey(KeyCode.D)) _force += Vector2.right;

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
        _body.AddForce(_force * (Time.fixedDeltaTime * speed), ForceMode2D.Force);

        if (_jumpRequest)
        {
            _body.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            _jumpRequest = false;
        }

        if (_knockbackRequest)
        {
            _body.AddForce(_knockbackDirection * testKnockbackForce, ForceMode2D.Impulse);
            _knockbackRequest = false;
        }
    }
}