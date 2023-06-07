using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyAttack : MonoBehaviour
{

    public int damage;
    public float damageInterval;
    public int knockbackForce;
    public LayerMask playerLayer;
    public GameObject playerObject;

    private Rigidbody2D _playerBody;
    private EntityHealth _playerHealth;
    private float _damageTimer;
    private bool _knockbackRequest;


    public void Awake()
    {
        _playerHealth = playerObject.GetComponent<EntityHealth>();
        _playerBody = playerObject.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (_damageTimer > 0f)
        {
            _damageTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (_knockbackRequest)
        {
            Vector2 knockbackDirection = (_playerBody.transform.position - transform.position).normalized;
            _playerBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            _knockbackRequest = false;
        }
    }

    /// <summary>
    /// Hits player: requests knockback, deals damage, and resets damage timer
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (_damageTimer <= 0f)
            {
                _playerHealth.takeDamage(damage);
                _damageTimer = damageInterval;
            }

            _knockbackRequest = true;
        }
    }
}