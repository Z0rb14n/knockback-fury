using System.Collections;
using UnityEngine;

public class EntityHealth : MonoBehaviour
{
    public int maxHealth;
    public int health;
    public float iFrameLength;

    protected float _iFrameTimer;
    protected SpriteRenderer _sprite;


    protected virtual void Awake()
    {
        health = maxHealth;
        _sprite = GetComponent<SpriteRenderer>();
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
            DoTakeDamage(dmg);
            
            if (health <= 0)
            {
                Die();
            }
        }
        StartCoroutine(DamageFlash());
    }

    private IEnumerator DamageFlash()
    {
        _sprite.color = new Color(1, 0, 0, 0.5f);
        yield return new WaitForSeconds(0.1f);
        _sprite.color = Color.white;
    }

    protected virtual void DoTakeDamage(int dmg)
    {
        health -= dmg;
        _iFrameTimer = iFrameLength;
    }

    protected virtual void Die()
    {
        Debug.Log("Death");
        // TODO: general entity death
    }
}
