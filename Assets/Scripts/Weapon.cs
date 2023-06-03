using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    // CONSTANT
    [SerializeField] 
    private Transform spritePivot;
    private Camera mainCam;
    private const float knockbackStrength = 10;


    // Start is called before the first frame update
    void Start() {
        mainCam = Camera.main;
    }

    void FixedUpdate() {
        pointAtCursor();
    }

    // fire weapon
    public void fire() {
        // instantiate & shoot bullets etc
    }

    public float getKnockbackStrength() {
        return knockbackStrength;
    }

    public Vector2 getLookDirection() {
        return spritePivot.right;
    }

    Vector2 getMousePos() {
        return mainCam.ScreenToWorldPoint(Input.mousePosition);;
    }

   void pointAtCursor() {
        Vector2 pivotPoint = spritePivot.position;
        Vector2 mousePos = getMousePos();
        spritePivot.right = mousePos - pivotPoint;
   }
}
