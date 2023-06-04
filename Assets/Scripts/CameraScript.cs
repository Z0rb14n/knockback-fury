using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [Tooltip("Maximum View Distance")]
    public float maxViewDist = 150;
    private Camera _mainCam;
    private Vector2 screenDims; // Cache screen dimensions
    private Vector3 displacement; // Reuse displacement vector to prevent frequent object creation

    private void Awake() 
    {
        _mainCam = Camera.main;
        // Cache the screen dimensions magnitude
        screenDims = new Vector2(Screen.width, Screen.height);
    }

    private void FixedUpdate() 
    {
        ApplyMouseDisplacement();
    }

    /// <summary>
    /// Displace the camera in the direction of the mouse
    /// </summary>
    private void ApplyMouseDisplacement() 
    {
        Vector2 lookVec = LookVector();
        Vector3 direction = lookVec.normalized;
        float distance = lookVec.magnitude;
        
        // Reuse displacement vector and update its components
        displacement.x = direction.x * Mathf.Lerp(0, maxViewDist, distance / screenDims.magnitude);
        displacement.y = direction.y * Mathf.Lerp(0, maxViewDist, distance / screenDims.magnitude);
        displacement.z = transform.position.z;

        transform.localPosition = displacement;
    }

    /// <summary>
    /// Returns vector of mouse from origin(player) to mouse position
    /// </summary>
    private Vector2 LookVector() 
    {
        Vector2 worldMousePos = _mainCam.ScreenToWorldPoint(Input.mousePosition);
        return transform.InverseTransformPoint(worldMousePos);
    }
}