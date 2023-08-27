using System.Collections;
using Player;
using UnityEngine;

public class EntityHealth : MonoBehaviour
{
    public int maxHealth;
    public int health;
    public float iFrameLength;

    public bool canDropCheese = true;
    public GameObject cheeseItemPickup;

    public delegate void DeathDelegate(EntityHealth source);

    public event DeathDelegate OnDeath;
    
    protected float _iFrameTimer;
    protected SpriteRenderer _sprite;
    protected bool isDead;


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
        if (!(_iFrameTimer <= 0)) return;
        DoTakeDamage(dmg);
            
        if (health <= 0) Die();
        StartCoroutine(DamageFlash());
    }

    private IEnumerator DamageFlash()
    {
        Color prevColor = _sprite.color;
        _sprite.color = new Color(1, 0, 0, 0.5f);
        yield return new WaitForSeconds(0.1f);
        _sprite.color = prevColor;
    }

    protected virtual void DoTakeDamage(int dmg)
    {
        health -= dmg;
        _iFrameTimer = iFrameLength;
        PlayerHealth.Instance.OnDamageDealtToOther(dmg);
    }

    protected virtual void Die()
    {
        if (!isDead)
        {
            Debug.Log("Death");
            PlayerMovementScript.Instance.OnEnemyKill();
            Destroy(gameObject);
            OnDeath?.Invoke(this);
            isDead = true;
            if (canDropCheese && cheeseItemPickup && Random.Range(0, 1f) < 0.05f)
            {
                Instantiate(cheeseItemPickup, transform.position, transform.rotation, transform.parent);
            }
        }
    }
}
