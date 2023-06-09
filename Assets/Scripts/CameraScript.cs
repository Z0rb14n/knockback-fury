using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [Tooltip("Maximum View Distance")]
    public float maxViewDist = 150;
    private Camera _mainCam;
    private Vector2 _screenDims; // Cache screen dimensions
    private Vector3 _displacement; // Reuse displacement vector to prevent frequent object creation

    private void Awake() 
    {
        _mainCam = Camera.main;
        // Cache the screen dimensions magnitude
        _screenDims = new Vector2(Screen.width, Screen.height);
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
        _displacement.x = direction.x * Mathf.Lerp(0, maxViewDist, distance / _screenDims.magnitude);
        _displacement.y = direction.y * Mathf.Lerp(0, maxViewDist, distance / _screenDims.magnitude);
        _displacement.z = transform.position.z;

        // how ironic that an optimization actually resulted in an inefficient property access huh
        // ReSharper disable once Unity.InefficientPropertyAccess
        transform.localPosition = _displacement;
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