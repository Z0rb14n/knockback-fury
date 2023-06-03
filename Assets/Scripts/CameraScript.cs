using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    // CONSTANT
    const float maxViewDist = 150;
    Camera mainCam;

    // Start is called before the first frame update
    void Start() {
        mainCam = Camera.main;
    }

    void FixedUpdate() {
        ApplyMouseDisplacement();
    }

    // displace the camera in the direction of the mouse
    private void ApplyMouseDisplacement() {
        float maxScreenDist = new Vector2(Screen.width, Screen.height).magnitude; // not constant - screen size can change (resizing window)

        Vector2 lookVec = LookVector();
        Vector3 direction = lookVec.normalized;
        float distance = lookVec.magnitude;

        transform.localPosition = direction * Mathf.Lerp(0, maxViewDist, distance/maxScreenDist) + new Vector3(0,0,transform.position.z);
    }

    // vector of mouse from origin(player) to mouse position
    private Vector2 LookVector() {
        Vector2 worldMousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        return transform.InverseTransformPoint(worldMousePos);
    }

}
