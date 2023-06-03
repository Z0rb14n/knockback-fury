using JetBrains.Annotations;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // CONSTANT
    [SerializeField] 
    private Transform spritePivot;
    [SerializeField]
    private SpriteRenderer sprite;

    [CanBeNull] public WeaponData weaponData;
    private Camera _mainCam;
    private Vector2 _spriteStartPosition;
    private Vector2 _recoilAnimDisplacement;

    // VARYING
    private float _recoilAnimTimer = 0;

    public Vector2 LookDirection => spritePivot.right;


    private void Awake() {
        _mainCam = Camera.main;
        _spriteStartPosition = sprite.transform.localPosition;
        _recoilAnimDisplacement = new Vector2(-0.02f,0);
        UpdateFromWeaponData();
    }

    private void FixedUpdate() {
        PointAtCursor();

        float dt = Time.deltaTime;

        if (_recoilAnimTimer > 0) {
            FireAnimation(dt);
        }
    }

    private void UpdateFromWeaponData()
    {
        if (weaponData != null)
            sprite.sprite = weaponData.sprite;
    }

    /// <summary>
    /// Fire weapon
    /// </summary>
    public void Fire() {
        StartFireAnimation();
        // instantiate & shoot bullets etc
    }

    /// <summary>
    /// Initialize values and start firing animation
    /// </summary>
    private void StartFireAnimation() {
        if (weaponData != null)
            _recoilAnimTimer = weaponData.recoilAnimationDuration;
    }

    /// <summary>
    /// Update firing animation and values
    /// </summary>
    /// <param name="dt">Typically <code>Time.deltaTime</code></param>
    private void FireAnimation(float dt) {
        // can be null but cba
        float weight = _recoilAnimTimer/weaponData.recoilAnimationDuration;
        if (weight <= 0.5) {
            weight = 1 - weight;
        }
        weight = (weight - 0.5f)*2;

        sprite.transform.localPosition = Vector2.Lerp(_recoilAnimDisplacement, _spriteStartPosition, weight);

        _recoilAnimTimer -= dt;
        if (_recoilAnimTimer <= 0) {
            sprite.transform.localPosition = _spriteStartPosition;
        }
    }

    /// <summary>
    /// Return world mouse coordinates
    /// </summary>
    private Vector2 GetMousePos() {
        return _mainCam.ScreenToWorldPoint(Input.mousePosition);
    }

    /// <summary>
    /// Make the sprite pivot point towards to cursor
    /// </summary>
    private void PointAtCursor() {
        Vector2 pivotPoint = spritePivot.position;
        Vector2 mousePos = GetMousePos();
        spritePivot.right = mousePos - pivotPoint;
   }
}
