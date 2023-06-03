using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // CONSTANT
    [SerializeField] 
    private Transform spritePivot;
    [SerializeField]
    private Transform sprite;
    private Camera mainCam;
    private const float knockbackStrength = 12;
    private const float recoilAnimDuration = 0.2f;
    private Vector2 spriteStartPosition;
    private Vector2 recoilAnimDisplacement;

    // VARYING
    private float recoilAnimTimer = 0;


    // Start is called before the first frame update
    void Start() {
        mainCam = Camera.main;
        spriteStartPosition = sprite.localPosition;
        recoilAnimDisplacement = new Vector2(-0.02f,0);
    }

    void FixedUpdate() {
        pointAtCursor();

        float dt = Time.deltaTime;

        if (recoilAnimTimer > 0) {
            FireAnimation(dt);
        }
    }

    // fire weapon
    public void Fire() {
        StartFireAnimation();
        // instantiate & shoot bullets etc
    }

    // returns current weapons recoil knockback strength
    public float GetKnockbackStrength() {
        return knockbackStrength;
    }

    // return current lookdirection vector
    public Vector2 GetLookDirection() {
        return spritePivot.right;
    }

    // Initialise values and start firing animation
    private void StartFireAnimation() {
        recoilAnimTimer = recoilAnimDuration;
    }

    // update firing animation and values
    private void FireAnimation(float dt) {
        float weight = recoilAnimTimer/recoilAnimDuration;
        if (weight <= 0.5) {
            weight = 1 - weight;
        }
        weight = (weight - 0.5f)*2;

        sprite.localPosition = Vector2.Lerp(recoilAnimDisplacement, spriteStartPosition, weight);

        recoilAnimTimer -= dt;
        if (recoilAnimTimer <= 0) {
            sprite.localPosition = spriteStartPosition;
        }
    }

    // return world mouse coordinates
    private Vector2 GetMousePos() {
        return mainCam.ScreenToWorldPoint(Input.mousePosition);
    }

    // make the sprite pivot point towards to cursor
    private void pointAtCursor() {
        Vector2 pivotPoint = spritePivot.position;
        Vector2 mousePos = GetMousePos();
        spritePivot.right = mousePos - pivotPoint;
   }
}
