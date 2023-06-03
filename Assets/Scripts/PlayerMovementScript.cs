using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    // CONSTANT
    [Min(0), Tooltip("Affects the speed of the player")]
    public float walkSpeed = 30;
    [Min(0), Tooltip("Jump Impulse")]
    public float jumpForce = 10;
    [Min(0), Tooltip("Test Mouse1 Knockback Impulse")]
    public float testKnockbackForce = 10;
    private Rigidbody2D _body;
    private Camera _cam;

    // VARYING
    [SerializeField] 
    Weapon weaponScript;
    Vector2 inputDirection = Vector2.zero; // current movement direction specified by WASD

    /////////////////////////////
    //        FUNCTIONS        //
    /////////////////////////////

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _cam = Camera.main;
        weaponScript = GetComponentInChildren<Weapon>();
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        inputDirection = new Vector2(horizontalInput, 0).normalized;
        
        if (Input.GetKeyDown(KeyCode.Space)) Jump();

        if (Input.GetMouseButtonDown(0))
        {
            fireWeapon();
        }
    }

    private void FixedUpdate() {
        float dt = Time.deltaTime;

        Move(dt);
    }

    void fireWeapon() {
        weaponScript.fire();
        Vector2 knockbackDirection = -weaponScript.getLookDirection();
        float knockbackStrength = weaponScript.getKnockbackStrength();
        applyKnockback(knockbackDirection, knockbackStrength);
    }

    void applyKnockback(Vector2 direction, float knockbackStrength) {
        _body.AddForce(direction * knockbackStrength, ForceMode2D.Impulse);
    }

    void Move(float dt) {
        _body.AddForce(inputDirection * walkSpeed, ForceMode2D.Force);
    }

    void Jump() {
        _body.AddForce(new Vector2(0,jumpForce), ForceMode2D.Impulse);
    }

}
