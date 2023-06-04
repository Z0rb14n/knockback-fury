using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyAttack : MonoBehaviour
{
    public EntityHealth playerHealth;
    public int damage;
    public float damageInterval = 1f;
    public LayerMask playerLayer;

    private float damageTimer;

    private void Update()
    {
        if (damageTimer > 0f)
        {
            damageTimer -= Time.deltaTime;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (damageTimer <= 0f)
            {
                playerHealth.takeDamage(damage);
                damageTimer = damageInterval;
            }
        }
    }
}