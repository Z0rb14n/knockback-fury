using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    [Min(0), Tooltip("Affects the speed of the player")]
    public float speed = 69;
    private Rigidbody2D _body;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Vector2 force = Vector2.zero;
        
        if (Input.GetKey(KeyCode.A)) force += Vector2.left;
        if (Input.GetKey(KeyCode.D)) force += Vector2.right;
        
        _body.AddForce(force * (Time.deltaTime * speed), ForceMode2D.Force);
        
        if (Input.GetKeyDown(KeyCode.Space)) _body.AddForce(new Vector2(0,10), ForceMode2D.Impulse);
    }
}
