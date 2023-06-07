using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHealth : MonoBehaviour
{
    public int maxHealth;
    public int health;
    public float iFrameLength;

    protected float _iFrameTimer;

    protected virtual void Awake()
    {
        health = maxHealth;
    }

    private void Update()
    {
        if (_iFrameTimer > 0f)
        {
            _iFrameTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Takes Damage: decreases health, sets iFrame timer, requests death if required
    /// </summary>
    public virtual void TakeDamage(int dmg)
    {
        if (_iFrameTimer <= 0)
        {
            health -= dmg;
            _iFrameTimer = iFrameLength;

            if (health <= 0)
            {
                // TODO: die
                Debug.Log("Death");
            }
        }
    }
}
