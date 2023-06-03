using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [Tooltip("Maximum View Distance")]
    public float maxViewDist = 150;
    private Camera _mainCam;

    private void Awake() {
        _mainCam = Camera.main;
    }

    private void FixedUpdate() {
        ApplyMouseDisplacement();
    }

    /// <summary>
    /// Displace the camera in the direction of the mouse
    /// </summary>
    private void ApplyMouseDisplacement() {
        float maxScreenDist = new Vector2(Screen.width, Screen.height).magnitude; // not constant - screen size can change (resizing window)

        Vector2 lookVec = LookVector();
        Vector3 direction = lookVec.normalized;
        float distance = lookVec.magnitude;

        Transform transform1 = transform;
        transform1.localPosition = direction * Mathf.Lerp(0, maxViewDist, distance/maxScreenDist) + new Vector3(0,0,transform.position.z);
    }

    /// <summary>
    /// Returns vector of mouse from origin(player) to mouse position
    /// </summary>
    private Vector2 LookVector() {
        Vector2 worldMousePos = _mainCam.ScreenToWorldPoint(Input.mousePosition);
        return transform.InverseTransformPoint(worldMousePos);
    }
}
