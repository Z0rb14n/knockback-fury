using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyAttack : MonoBehaviour
{

    public int damage;
    public int knockbackForce;
    public LayerMask playerLayer;
    public GameObject playerObject;

    private Rigidbody2D _playerBody;
    private EntityHealth _playerHealth;
    private bool _knockbackRequest;


    public void Awake()
    {
        _playerHealth = playerObject.GetComponent<EntityHealth>();
        _playerBody = playerObject.GetComponent<Rigidbody2D>();
    }



    private void FixedUpdate()
    {
        if (_knockbackRequest)
        {
            Vector2 knockbackDirection = new Vector2((_playerBody.transform.position - transform.position).normalized.x * 0.1f, 0.04f);
            _playerBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            _knockbackRequest = false;
        }
    }

    /// <summary>
    /// Hits player: requests knockback, deals damage
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            _playerHealth.TakeDamage(damage);
            _knockbackRequest = true;
        }
    }
}