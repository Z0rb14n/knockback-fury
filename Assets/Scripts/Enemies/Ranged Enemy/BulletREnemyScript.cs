using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class BulletREnemyScript : MonoBehaviour
{
    public float force;
    public int bulletDamage;
    public int knockbackForce;

    private GameObject player;
    private Rigidbody2D rb;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");

        Vector3 direction = player.transform.position - transform.position;
        rb.velocity = new Vector2(direction.x, direction.y).normalized * force;

        float rot = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rot);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 10)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            EntityHealth _playerHealth = other.gameObject.GetComponent<EntityHealth>();
            PlayerMovementScript _playerMovement = other.gameObject.GetComponent<PlayerMovementScript>();

            _playerHealth.TakeDamage(bulletDamage);
            Vector2 knockbackDirection = new((other.transform.position - transform.position).normalized.x * 0.1f, 0.04f);
            _playerMovement.RequestKnockback(knockbackDirection, knockbackForce);

            Destroy(gameObject);
        }
    }
}
