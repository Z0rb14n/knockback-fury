using System.Collections;
using Player;
using UnityEngine;

public class EntityHealth : MonoBehaviour
{
    public int maxHealth;
    public int health;
    public float iFrameLength;

    public bool doesChangeColor = true;
    public bool canDropCheese = true;
    public GameObject cheeseItemPickup;

    public delegate void DeathDelegate(EntityHealth source);

    public event DeathDelegate OnDeath;
    
    protected float _iFrameTimer;
    protected SpriteRenderer _sprite;
    protected bool isDead;
    protected Color normalColor;


    protected virtual void Awake()
    {
        health = maxHealth;
        _sprite = GetComponent<SpriteRenderer>();
        normalColor = _sprite.color;
    }

    protected virtual void Update()
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
        if (doesChangeColor) StartCoroutine(DamageFlash());
    }

    private IEnumerator DamageFlash()
    {
        _sprite.color = new Color(1, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.1f);
        // ReSharper disable once Unity.InefficientPropertyAccess
        _sprite.color = normalColor;
    }

    protected virtual void DoTakeDamage(int dmg)
    {
        int actualDamage = Mathf.Min(health, dmg);
        health -= dmg;
        _iFrameTimer = iFrameLength;
        PlayerHealth.Instance.OnDamageDealtToOther(actualDamage);
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
